//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2025, The OpenBVE Project
//
//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met:
//
//1. Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
//ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

//---------------------------------------------------------------------------------
// NVIDIA Image Scaling SDK  - v1.0.3
// NVScaler fragment shader port for openBVE.
//
// Copyright (c) 2022 NVIDIA CORPORATION & AFFILIATES. All rights reserved.
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and / or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions :
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//---------------------------------------------------------------------------------
// Per-pixel (fragment shader) port of the NVScaler compute shader for GL 3.3,
// which has no compute stages and no texture gather. The compute shader's
// 16x16 block / shared-memory tiling cannot be replicated here, so the 6x6
// luma support is fetched per output pixel - that part is inherent to the
// algorithm in a fragment shader. Everything AROUND that support load is
// optimized:
//
// 1. Coefficient banks live in one RG32F 6x64 texture; one texelFetch returns
//    (coef_scale[t], coef_usm[t]) together, so EvalPoly6 needs 6 coefficient
//    fetches instead of 12 (worst case 60 -> 36 fetches per pixel).
//    Dynamic indexing of const arrays is deliberately avoided: on GL 3.3-class
//    compilers it promotes the array to scratch memory, adding kilobytes of
//    per-pixel traffic. texelFetch against a 3 KB cache-resident texture is a
//    native fast path on every driver (the official SDK uses the same design).
// 2. Support fetches use texture() at exact texel centers,
//    (texel + 0.5) * kSrcNorm: LINEAR filtering at a texel center returns that
//    texel with weight 1.0 (the official tile loader offsets by
//    kShift = 0.5 - (kSupportSize - 1) / 2 for the same reason), so the values
//    are identical to texelFetch while the sampler hardware performs the edge
//    clamping - no per-tap clamp ALU.
// 3. dstX/dstY are the INTEGER output pixel positions (official
//    "srcX = (0.5 + dstX) * kScaleX - 0.5" takes integer dstX), hence the
//    -0.5 correction of gl_FragCoord.
// 4. The edge-map division is guarded (the official shader early-returns when
//    the gradient sum is zero; the guarded form yields the identical all-zero
//    weights without a branch), and the 45/135 degree phase shifts are done
//    with selects instead of data-dependent loop copies.
//---------------------------------------------------------------------------------

#version 330 core
uniform sampler2D uSrc;   // Low-resolution scene texture (LINEAR, CLAMP_TO_EDGE)
uniform sampler2D uCoef;  // NIS filter banks, RG32F 6x64: texel (tap, phase)
                          // .rg = (coef_scale[phase][tap], coef_usm[phase][tap])
uniform vec4 uNisCfg0;    // kDetectRatio, kDetectThres, kMinContrastRatio, kRatioNorm
uniform vec4 uNisCfg1;    // kContrastBoost, kEps, kSharpStartY, kSharpScaleY
uniform vec4 uNisCfg2;    // kSharpStrengthMin, kSharpStrengthScale, kSharpLimitMin, kSharpLimitScale
uniform vec4 uNisCfg3;    // kScaleX, kScaleY, kSrcNormX, kSrcNormY (zw = 1 / input size)

in vec2 vUV;
out vec4 fragColor;

const int kPhaseCount = 64;

float getY(vec3 rgba)
{
        return 0.2126 * rgba.x + 0.7152 * rgba.y + 0.0722 * rgba.z;
}

// Edge-clamped source fetch via the sampler: exact texel value at the texel
// center, clamping handled by the hardware (kSrcNorm = 1 / input size).
float fetchY(ivec2 texel)
{
        return getY(texture(uSrc, (vec2(texel) + 0.5) * uNisCfg3.zw).rgb);
}

//---------------------------------------------------------------------------------
// Edge map for a 3x3 luma window given row-major as nine values.
// (GetEdgeMap from NIS_Scaler.h, window passed flattened since GLSL 330
//  does not support arrays of arrays.)
//---------------------------------------------------------------------------------
vec4 GetEdgeMap9(
        float p00, float p01, float p02,
        float p10, float p11, float p12,
        float p20, float p21, float p22)
{
        float g_0 = abs(p00 + p01 + p02 - p20 - p21 - p22);
        float g_45 = abs(p10 + p00 + p01 - p21 - p22 - p12);
        float g_90 = abs(p00 + p10 + p20 - p02 - p12 - p22);
        float g_135 = abs(p10 + p20 + p21 - p01 - p02 - p12);

        float g_0_90_max = max(g_0, g_90);
        float g_0_90_min = min(g_0, g_90);
        float g_45_135_max = max(g_45, g_135);
        float g_45_135_min = min(g_45, g_135);

        // Guarded division: when both gradient sums are zero the official shader
        // early-returns vec4(0); here the thresholds below reject the weights
        // instead, and the division never sees a zero denominator.
        float denom = max(g_0_90_max + g_45_135_max, 1e-8);
        float e_0_90 = min(g_0_90_max / denom, 1.0);
        float e_45_135 = 1.0 - e_0_90;

        bool c_0_90 = (g_0_90_max > (g_0_90_min * uNisCfg0.x)) && (g_0_90_max > uNisCfg0.y) && (g_0_90_max > g_45_135_min);
        bool c_45_135 = (g_45_135_max > (g_45_135_min * uNisCfg0.x)) && (g_45_135_max > uNisCfg0.y) && (g_45_135_max > g_0_90_min);
        bool c_g_0_90 = g_0_90_max == g_0;
        bool c_g_45_135 = g_45_135_max == g_45;

        float f_e_0_90 = (c_0_90 && c_45_135) ? e_0_90 : 1.0;
        float f_e_45_135 = (c_0_90 && c_45_135) ? e_45_135 : 1.0;

        float weight_0 = (c_0_90 && c_g_0_90) ? f_e_0_90 : 0.0;
        float weight_90 = (c_0_90 && !c_g_0_90) ? f_e_0_90 : 0.0;
        float weight_45 = (c_45_135 && c_g_45_135) ? f_e_45_135 : 0.0;
        float weight_135 = (c_45_135 && !c_g_45_135) ? f_e_45_135 : 0.0;

        return vec4(weight_0, weight_90, weight_45, weight_135);
}

//---------------------------------------------------------------------------------
// Local tone-preserving tone mapping (contrast adaptive) filter weight.
// (CalcLTI from NIS_Scaler.h)
//---------------------------------------------------------------------------------
float CalcLTI(float p0, float p1, float p2, float p3, float p4, float p5, int phase_index)
{
        bool selector = (phase_index <= kPhaseCount / 2);
        float sel = selector ? p0 : p3;
        float a_min = min(min(p1, p2), sel);
        float a_max = max(max(p1, p2), sel);
        sel = selector ? p2 : p5;
        float b_min = min(min(p3, p4), sel);
        float b_max = max(max(p3, p4), sel);

        float a_cont = a_max - a_min;
        float b_cont = b_max - b_min;

        float cont_ratio = max(a_cont, b_cont) / (min(a_cont, b_cont) + uNisCfg1.y);
        return (1.0 - clamp((cont_ratio - uNisCfg0.z) * uNisCfg0.w, 0.0, 1.0)) * uNisCfg1.x;
}

//---------------------------------------------------------------------------------
// 6-tap directional polynomial evaluation with adaptive USM.
// (EvalPoly6 from NIS_Scaler.h) One texelFetch returns both filter banks
// (scale + unsharp-mask coefficients), halving the coefficient traffic.
//---------------------------------------------------------------------------------
float EvalPoly6(const float pxl[6], int phase_int)
{
        float y = 0.0;
        float y_usm = 0.0;
        for (int i = 0; i < 6; ++i)
        {
                vec2 coef = texelFetch(uCoef, ivec2(i, phase_int), 0).rg;
                y += coef.x * pxl[i];
                y_usm += coef.y * pxl[i];
        }

        // let's compute a piece-wise ramp based on luma
        float y_scale = 1.0 - clamp((y - uNisCfg1.z) * uNisCfg1.w, 0.0, 1.0);

        // scale the ramp to sharpen as a function of luma
        float y_sharpness = y_scale * uNisCfg2.y + uNisCfg2.x;

        y_usm *= y_sharpness;

        // scale the ramp to limit USM as a function of luma
        float y_sharpness_limit = (y_scale * uNisCfg2.w + uNisCfg2.z) * y;

        y_usm = min(y_sharpness_limit, max(-y_sharpness_limit, y_usm));
        // reduce ringing
        y_usm *= CalcLTI(pxl[0], pxl[1], pxl[2], pxl[3], pxl[4], pxl[5], phase_int);

        return y + y_usm;
}

//---------------------------------------------------------------------------------
// Normal (non-directional) 6x6 scaler filter, separable form.
// (FilterNormal from NIS_Scaler.h; p is the 6x6 luma support, flattened
//  row-major: p[i * 6 + j] = source texel (sx - 2 + j, sy - 2 + i))
//---------------------------------------------------------------------------------
float FilterNormal(const float p[36], int phase_x_frac_int, int phase_y_frac_int)
{
        float cX[6];
        float cY[6];
        for (int j = 0; j < 6; ++j)
        {
                cX[j] = texelFetch(uCoef, ivec2(j, phase_x_frac_int), 0).r;
        }
        for (int i = 0; i < 6; ++i)
        {
                cY[i] = texelFetch(uCoef, ivec2(i, phase_y_frac_int), 0).r;
        }

        float h_acc = 0.0;
        for (int j = 0; j < 6; ++j)
        {
                float v_acc = 0.0;
                for (int i = 0; i < 6; ++i)
                {
                        v_acc += p[i * 6 + j] * cY[i];
                }
                h_acc += v_acc * cX[j];
        }

        // let's return the sum unpacked -> we can accumulate it later
        return h_acc;
}

//---------------------------------------------------------------------------------
// Directional filter bank (0 / 45 / 90 / 135 degrees).
// (AddDirFilters from NIS_Scaler.h; p is the 6x6 luma support, flattened
//  row-major. The phase >= 1 shifts use selects instead of loop copies.)
//---------------------------------------------------------------------------------
float AddDirFilters(const float p[36], float phase_x_frac, float phase_y_frac, int phase_x_frac_int, int phase_y_frac_int, vec4 w)
{
        float f = 0;
        if (w.x > 0.0)
        {
                // 0 deg filter
                float interp0Deg[6];
                for (int i = 0; i < 6; ++i)
                {
                        interp0Deg[i] = mix(p[i * 6 + 2], p[i * 6 + 3], phase_x_frac);
                }
                f += EvalPoly6(interp0Deg, phase_y_frac_int) * w.x;
        }
        if (w.y > 0.0)
        {
                // 90 deg filter
                float interp90Deg[6];
                for (int i = 0; i < 6; ++i)
                {
                        interp90Deg[i] = mix(p[2 * 6 + i], p[3 * 6 + i], phase_y_frac);
                }

                f += EvalPoly6(interp90Deg, phase_x_frac_int) * w.y;
        }
        if (w.z > 0.0)
        {
                // 45 deg filter
                float pphase_b45 = 0.5 + 0.5 * (phase_x_frac - phase_y_frac);

                float temp_interp45Deg[7];
                temp_interp45Deg[1] = mix(p[2 * 6 + 1], p[1 * 6 + 2], pphase_b45);
                temp_interp45Deg[3] = mix(p[3 * 6 + 2], p[2 * 6 + 3], pphase_b45);
                temp_interp45Deg[5] = mix(p[4 * 6 + 3], p[3 * 6 + 4], pphase_b45);
                {
                        pphase_b45 = pphase_b45 - 0.5;
                        float a = (pphase_b45 >= 0.0) ? p[0 * 6 + 2] : p[2 * 6 + 0];
                        float b = (pphase_b45 >= 0.0) ? p[1 * 6 + 3] : p[3 * 6 + 1];
                        float c = (pphase_b45 >= 0.0) ? p[2 * 6 + 4] : p[4 * 6 + 2];
                        float d = (pphase_b45 >= 0.0) ? p[3 * 6 + 5] : p[5 * 6 + 3];
                        temp_interp45Deg[0] = mix(p[1 * 6 + 1], a, abs(pphase_b45));
                        temp_interp45Deg[2] = mix(p[2 * 6 + 2], b, abs(pphase_b45));
                        temp_interp45Deg[4] = mix(p[3 * 6 + 3], c, abs(pphase_b45));
                        temp_interp45Deg[6] = mix(p[4 * 6 + 4], d, abs(pphase_b45));
                }

                float interp45Deg[6];
                float pphase_p45 = phase_x_frac + phase_y_frac;
                float shift45 = pphase_p45 >= 1.0 ? 1.0 : 0.0;
                pphase_p45 -= shift45;
                for (int i = 0; i < 6; ++i)
                {
                        interp45Deg[i] = mix(temp_interp45Deg[i], temp_interp45Deg[i + 1], shift45);
                }

                f += EvalPoly6(interp45Deg, int(pphase_p45 * 64.0)) * w.z;
        }
        if (w.w > 0.0)
        {
                // 135 deg filter
                float pphase_b135 = 0.5 * (phase_x_frac + phase_y_frac);

                float temp_interp135Deg[7];
                temp_interp135Deg[1] = mix(p[3 * 6 + 1], p[4 * 6 + 2], pphase_b135);
                temp_interp135Deg[3] = mix(p[2 * 6 + 2], p[3 * 6 + 3], pphase_b135);
                temp_interp135Deg[5] = mix(p[1 * 6 + 3], p[2 * 6 + 4], pphase_b135);
                {
                        pphase_b135 = pphase_b135 - 0.5;
                        float a = (pphase_b135 >= 0.0) ? p[5 * 6 + 2] : p[3 * 6 + 0];
                        float b = (pphase_b135 >= 0.0) ? p[4 * 6 + 3] : p[2 * 6 + 1];
                        float c = (pphase_b135 >= 0.0) ? p[3 * 6 + 4] : p[1 * 6 + 2];
                        float d = (pphase_b135 >= 0.0) ? p[2 * 6 + 5] : p[0 * 6 + 3];
                        temp_interp135Deg[0] = mix(p[4 * 6 + 1], a, abs(pphase_b135));
                        temp_interp135Deg[2] = mix(p[3 * 6 + 2], b, abs(pphase_b135));
                        temp_interp135Deg[4] = mix(p[2 * 6 + 3], c, abs(pphase_b135));
                        temp_interp135Deg[6] = mix(p[1 * 6 + 4], d, abs(pphase_b135));
                }

                float interp135Deg[6];
                float pphase_p135 = 1.0 + (phase_x_frac - phase_y_frac);
                float shift135 = pphase_p135 >= 1.0 ? 1.0 : 0.0;
                pphase_p135 -= shift135;
                for (int i = 0; i < 6; ++i)
                {
                        interp135Deg[i] = mix(temp_interp135Deg[i], temp_interp135Deg[i + 1], shift135);
                }

                f += EvalPoly6(interp135Deg, int(pphase_p135 * 64.0)) * w.w;
        }
        return f;
}

void main()
{
        // output pixel position as the official integer dstX / dstY
        float dstX = gl_FragCoord.x - 0.5;
        float dstY = gl_FragCoord.y - 0.5;

        // x / y coords inside the input image
        float srcX = (0.5 + dstX) * uNisCfg3.x - 0.5;
        float srcY = (0.5 + dstY) * uNisCfg3.y - 0.5;
        float srcXf = floor(srcX);
        float srcYf = floor(srcY);
        int sx = int(srcXf);
        int sy = int(srcYf);
        // fractional part
        float fx = srcX - srcXf;
        float fy = srcY - srcYf;
        // discretized phase
        int fx_int = int(fx * float(kPhaseCount));
        int fy_int = int(fy * float(kPhaseCount));
        // guard against fp rounding pushing the phase to 64
        fx_int = min(fx_int, kPhaseCount - 1);
        fy_int = min(fy_int, kPhaseCount - 1);

        // load 6x6 luma support to registers; p[i * 6 + j] is the source texel
        // (sx - 2 + j, sy - 2 + i), matching the compute shader's shared tile layout
        float p[36];
        for (int i = 0; i < 6; ++i)
        {
                for (int j = 0; j < 6; ++j)
                {
                        p[i * 6 + j] = fetchY(ivec2(sx - 2 + j, sy - 2 + i));
                }
        }

        // generate weights for directional filters: the edge map is evaluated on
        // the 3x3 luma windows centered (sx - 1 + j, sy - 1 + i) inside the support
        vec4 edge00 = GetEdgeMap9(p[1 * 6 + 1], p[1 * 6 + 2], p[1 * 6 + 3], p[2 * 6 + 1], p[2 * 6 + 2], p[2 * 6 + 3], p[3 * 6 + 1], p[3 * 6 + 2], p[3 * 6 + 3]);
        vec4 edge01 = GetEdgeMap9(p[1 * 6 + 2], p[1 * 6 + 3], p[1 * 6 + 4], p[2 * 6 + 2], p[2 * 6 + 3], p[2 * 6 + 4], p[3 * 6 + 2], p[3 * 6 + 3], p[3 * 6 + 4]);
        vec4 edge10 = GetEdgeMap9(p[2 * 6 + 1], p[2 * 6 + 2], p[2 * 6 + 3], p[3 * 6 + 1], p[3 * 6 + 2], p[3 * 6 + 3], p[4 * 6 + 1], p[4 * 6 + 2], p[4 * 6 + 3]);
        vec4 edge11 = GetEdgeMap9(p[2 * 6 + 2], p[2 * 6 + 3], p[2 * 6 + 4], p[3 * 6 + 2], p[3 * 6 + 3], p[3 * 6 + 4], p[4 * 6 + 2], p[4 * 6 + 3], p[4 * 6 + 4]);
        // GetInterpEdgeMap
        vec4 h0 = mix(edge00, edge01, fx);
        vec4 h1 = mix(edge10, edge11, fx);
        vec4 w = mix(h0, h1, fy);

        // weight for luma
        float baseWeight = 1.0 - w.x - w.y - w.z - w.w;

        // final luma is a weighted product of directional & normal filters
        float opY = 0;

        // get traditional scaler filter output
        opY += FilterNormal(p, fx_int, fy_int) * baseWeight;

        // get directional filter bank output
        opY += AddDirFilters(p, fx, fy, fx_int, fy_int, w);

        // do bilinear tap for chroma upscaling
        vec2 coord = vec2((srcX + 0.5) * uNisCfg3.z, (srcY + 0.5) * uNisCfg3.w);
        vec4 op = texture(uSrc, coord);
        float y = getY(op.rgb);

        float corr = opY - y;
        op = vec4(op.rgb + vec3(corr), 1.0);

        // NVCLAMP (saturate) - output is display-referred LDR
        fragColor = vec4(clamp(op.rgb, 0.0, 1.0), 1.0);
}
