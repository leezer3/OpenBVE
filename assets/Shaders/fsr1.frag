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
// AMD FidelityFX SUPER RESOLUTION [FSR 1] ::: SPATIAL SCALING & EXTRAS - v1.20210629
// Fused EASU + RCAS single pass port for openBVE.
//
// Copyright (c) 2021 Advanced Micro Devices, Inc. All rights reserved.
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files(the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions :
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// COPYRIGHT OWNER(S) AND CONTRIBUTORS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//---------------------------------------------------------------------------------
// EASU (Edge Adaptive Spatial Upscaling) produces the upscaled center pixel and
// RCAS (Robust Contrast Adaptive Sharpening) is applied on top in the same pass:
// the four RCAS ring taps are bilinear source samples taken one output pixel
// north / west / east / south of the current output pixel, so no intermediate
// framebuffer is needed (~40% bandwidth saving vs. the official two-pass form).
//
// The optional RCAS denoiser is enabled by the host injecting
// "#define FSR_RCAS_DENOISE 1" after the #version line before compilation.
//
// Sampling notes (both accuracy and speed):
// - All source fetches go through texture() at exact texel centers,
//   (texel + 0.5) * uTexelStep. LINEAR filtering at a texel center returns that
//   texel with weight 1.0, so the values are identical to texelFetch, while the
//   sampler hardware performs the edge clamping for free (the reference uses
//   CLAMP_TO_EDGE gather semantics) and no per-tap clamp ALU is needed.
// - ip is the INTEGER output pixel position (ffx_fsr1.h: "Integer pixel position
//   in output"), hence the -0.5 correction of gl_FragCoord.
//---------------------------------------------------------------------------------

#version 330 core
uniform sampler2D uSrc;        // Low-resolution scene texture (LINEAR, CLAMP_TO_EDGE)
uniform vec4  uEasuCon0;       // xy = inputSize / outputSize, zw = 0.5 * inputSize / outputSize - 0.5
uniform vec2  uTexelStep;      // 1.0 / input size (texel step of the source)
uniform vec2  uDstResInv;      // 1.0 / output window size
uniform float uSharpness;      // RCAS sharpness, 0.0 = off, 0.2 = FSR default, 1.0 = crunchy

in vec2 vUV;
out vec4 fragColor;

//---------------------------------------------------------------------------------
// Approximate math functions from ffx_a.h (GLSL variants, kept bit-identical)
//---------------------------------------------------------------------------------
float APrxLoRcpF1(float a)
{
        return uintBitsToFloat(0x7ef07ebbu - floatBitsToUint(a));
}
float APrxMedRcpF1(float a)
{
        float b = uintBitsToFloat(0x7ef19fffu - floatBitsToUint(a));
        return b * (-b * a + 2.0);
}
float APrxLoRsqF1(float a)
{
        return uintBitsToFloat(0x5f347d74u - (floatBitsToUint(a) >> 1u));
}
float ARcpF1(float x)
{
        return 1.0 / x;
}
float ASatF1(float x)
{
        return clamp(x, 0.0, 1.0);
}

//---------------------------------------------------------------------------------
// Edge-clamped source fetch via the sampler: exact texel value at the texel
// center, clamping handled by the hardware.
//---------------------------------------------------------------------------------
vec3 FsrTapFetch(ivec2 texel)
{
        return texture(uSrc, (vec2(texel) + 0.5) * uTexelStep).rgb;
}

//---------------------------------------------------------------------------------
// Filtering for a given tap for the scalar. (verbatim from ffx_fsr1.h)
//---------------------------------------------------------------------------------
void FsrEasuTapF(
        inout vec3 aC,  // Accumulated color, with negative lobe.
        inout float aW, // Accumulated weight.
        vec2 off,       // Pixel offset from resolve position to tap.
        vec2 dir,       // Gradient direction.
        vec2 len,       // Length.
        float lob,      // Negative lobe strength.
        float clp,      // Clipping point.
        vec3 c)         // Tap color.
{
        // Rotate offset by direction.
        vec2 v;
        v.x = (off.x * (dir.x)) + (off.y * dir.y);
        v.y = (off.x * (-dir.y)) + (off.y * dir.x);
        // Anisotropy.
        v *= len;
        // Compute distance^2.
        float d2 = v.x * v.x + v.y * v.y;
        // Limit to the window as at corner, 2 taps can easily be outside.
        d2 = min(d2, clp);
        // Approximation of lancos2 without sin() or rcp(), or sqrt() to get x.
        //  (25/16 * (2/5 * x^2 - 1)^2 - (25/16 - 1)) * (1/4 * x^2 - 1)^2
        //  |_______________________________________|   |_______________|
        //                   base                             window
        float wB = (2.0 / 5.0) * d2 + (-1.0);
        float wA = lob * d2 + (-1.0);
        wB *= wB;
        wA *= wA;
        wB = (25.0 / 16.0) * wB + (-(25.0 / 16.0 - 1.0));
        float w = wB * wA;
        // Do weighted average.
        aC += c * w;
        aW += w;
}

//---------------------------------------------------------------------------------
// Accumulate direction and length. (verbatim from ffx_fsr1.h)
//---------------------------------------------------------------------------------
void FsrEasuSetF(
        inout vec2 dir,
        inout float len,
        vec2 pp,
        bool biS, bool biT, bool biU, bool biV,
        float lA, float lB, float lC, float lD, float lE)
{
        // Compute bilinear weight, branches factor out as predicates are compiler time immediates.
        //  s t
        //  u v
        float w = 0.0;
        if (biS) { w = (1.0 - pp.x) * (1.0 - pp.y); }
        if (biT) { w =           pp.x * (1.0 - pp.y); }
        if (biU) { w = (1.0 - pp.x) *           pp.y;  }
        if (biV) { w =           pp.x *           pp.y;  }
        // Direction is the '+' diff.
        //    a
        //  b c d
        //    e
        // Then takes magnitude from abs average of both sides of 'c'.
        // Length converts gradient reversal to 0, smoothly to non-reversal at 1, shaped, then adding horz and vert terms.
        float dc = lD - lC;
        float cb = lC - lB;
        float lenX = max(abs(dc), abs(cb));
        lenX = APrxLoRcpF1(lenX);
        float dirX = lD - lB;
        dir.x += dirX * w;
        lenX = ASatF1(abs(dirX) * lenX);
        lenX *= lenX;
        len += lenX * w;
        // Repeat for the y axis.
        float ec = lE - lC;
        float ca = lC - lA;
        float lenY = max(abs(ec), abs(ca));
        lenY = APrxLoRcpF1(lenY);
        float dirY = lE - lA;
        dir.y += dirY * w;
        lenY = ASatF1(abs(dirY) * lenY);
        lenY *= lenY;
        len += lenY * w;
}

//---------------------------------------------------------------------------------
// EASU - Edge Adaptive Spatial Upscaling 12-tap kernel.
// The four hardware gathers of the original are replaced by twelve explicit
// sampler fetches using the exact tap layout documented in ffx_fsr1.h
// (b c / e f g h / i j k l / n o around the 'f' tap), which makes the math
// identical while only requiring GL 3.3.
//---------------------------------------------------------------------------------
vec3 FsrEasuF(vec2 ip)
{
        // Get base position of 'f' tap (texel units of the input image).
        vec2 pp = ip * uEasuCon0.xy + uEasuCon0.zw;
        vec2 fp = floor(pp);
        vec2 fracPp = pp - fp;
        ivec2 fi = ivec2(fp);
        // 12-tap kernel.
        //    b c
        //  e f g h
        //  i j k l
        //    n o
        vec3 b  = FsrTapFetch(fi + ivec2(0, -1));
        vec3 c  = FsrTapFetch(fi + ivec2(1, -1));
        vec3 e  = FsrTapFetch(fi + ivec2(-1, 0));
        vec3 f  = FsrTapFetch(fi + ivec2(0, 0));
        vec3 g  = FsrTapFetch(fi + ivec2(1, 0));
        vec3 h  = FsrTapFetch(fi + ivec2(2, 0));
        vec3 iT = FsrTapFetch(fi + ivec2(-1, 1));
        vec3 jT = FsrTapFetch(fi + ivec2(0, 1));
        vec3 kT = FsrTapFetch(fi + ivec2(1, 1));
        vec3 lT = FsrTapFetch(fi + ivec2(2, 1));
        vec3 nT = FsrTapFetch(fi + ivec2(0, 2));
        vec3 oT = FsrTapFetch(fi + ivec2(1, 2));
        // Simplest multi-channel approximate luma possible (luma times 2, in 2 FMA/MAD).
        float bL = b.b * 0.5 + (b.r * 0.5 + b.g);
        float cL = c.b * 0.5 + (c.r * 0.5 + c.g);
        float eL = e.b * 0.5 + (e.r * 0.5 + e.g);
        float fL = f.b * 0.5 + (f.r * 0.5 + f.g);
        float gL = g.b * 0.5 + (g.r * 0.5 + g.g);
        float hL = h.b * 0.5 + (h.r * 0.5 + h.g);
        float iL = iT.b * 0.5 + (iT.r * 0.5 + iT.g);
        float jL = jT.b * 0.5 + (jT.r * 0.5 + jT.g);
        float kL = kT.b * 0.5 + (kT.r * 0.5 + kT.g);
        float lL = lT.b * 0.5 + (lT.r * 0.5 + lT.g);
        float nL = nT.b * 0.5 + (nT.r * 0.5 + nT.g);
        float oL = oT.b * 0.5 + (oT.r * 0.5 + oT.g);
        // Accumulate for bilinear interpolation.
        vec2 dir = vec2(0.0);
        float len = 0.0;
        FsrEasuSetF(dir, len, fracPp, true, false, false, false, bL, eL, fL, gL, jL);
        FsrEasuSetF(dir, len, fracPp, false, true, false, false, cL, fL, gL, hL, kL);
        FsrEasuSetF(dir, len, fracPp, false, false, true, false, fL, iL, jL, kL, nL);
        FsrEasuSetF(dir, len, fracPp, false, false, false, true, gL, jL, kL, lL, oL);
        // Normalize with approximation, and cleanup close to zero.
        vec2 dir2 = dir * dir;
        float dirR = dir2.x + dir2.y;
        bool zro = dirR < (1.0 / 32768.0);
        dirR = APrxLoRsqF1(dirR);
        dirR = zro ? 1.0 : dirR;
        dir.x = zro ? 1.0 : dir.x;
        dir *= vec2(dirR);
        // Transform from {0 to 2} to {0 to 1} range, and shape with square.
        len = len * 0.5;
        len *= len;
        // Stretch kernel {1.0 vert|horz, to sqrt(2.0) on diagonal}.
        float stretch = (dir.x * dir.x + dir.y * dir.y) * APrxLoRcpF1(max(abs(dir.x), abs(dir.y)));
        // Anisotropic length after rotation,
        //  x := 1.0 lerp to 'stretch' on edges
        //  y := 1.0 lerp to 2x on edges
        vec2 len2 = vec2(1.0 + (stretch - 1.0) * len, 1.0 + (-0.5) * len);
        // Based on the amount of 'edge',
        // the window shifts from +/-{sqrt(2.0) to slightly beyond 2.0}.
        float lob = 0.5 + ((1.0 / 4.0 - 0.04) - 0.5) * len;
        // Set distance^2 clipping point to the end of the adjustable window.
        float clp = APrxLoRcpF1(lob);
        // Accumulation mixed with min/max of 4 nearest.
        //    b c
        //  e f g h
        //  i j k l
        //    n o
        vec3 min4 = min(min(min(f, g), jT), kT);
        vec3 max4 = max(max(max(f, g), jT), kT);
        // Accumulation.
        vec3 aC = vec3(0.0);
        float aW = 0.0;
        FsrEasuTapF(aC, aW, vec2(0.0, -1.0) - fracPp, dir, len2, lob, clp, b);   // b
        FsrEasuTapF(aC, aW, vec2(1.0, -1.0) - fracPp, dir, len2, lob, clp, c);   // c
        FsrEasuTapF(aC, aW, vec2(-1.0, 1.0) - fracPp, dir, len2, lob, clp, iT);  // i
        FsrEasuTapF(aC, aW, vec2(0.0, 1.0) - fracPp, dir, len2, lob, clp, jT);   // j
        FsrEasuTapF(aC, aW, vec2(0.0, 0.0) - fracPp, dir, len2, lob, clp, f);    // f
        FsrEasuTapF(aC, aW, vec2(-1.0, 0.0) - fracPp, dir, len2, lob, clp, e);   // e
        FsrEasuTapF(aC, aW, vec2(1.0, 1.0) - fracPp, dir, len2, lob, clp, kT);   // k
        FsrEasuTapF(aC, aW, vec2(2.0, 1.0) - fracPp, dir, len2, lob, clp, lT);   // l
        FsrEasuTapF(aC, aW, vec2(2.0, 0.0) - fracPp, dir, len2, lob, clp, h);    // h
        FsrEasuTapF(aC, aW, vec2(1.0, 0.0) - fracPp, dir, len2, lob, clp, g);    // g
        FsrEasuTapF(aC, aW, vec2(1.0, 2.0) - fracPp, dir, len2, lob, clp, oT);   // o
        FsrEasuTapF(aC, aW, vec2(0.0, 2.0) - fracPp, dir, len2, lob, clp, nT);   // n
        // Normalize and dering.
        return min(max4, max(min4, aC * ARcpF1(aW)));
}

//---------------------------------------------------------------------------------
// RCAS - Robust Contrast Adaptive Sharpening. (verbatim from ffx_fsr1.h)
// The five taps are passed in instead of being loaded via FsrRcasLoadF: the
// center is the EASU result, the four ring taps are bilinear samples of the
// low-resolution texture taken one output pixel north / west / east / south
// of the current output pixel (fused pass - no intermediate texture).
//---------------------------------------------------------------------------------
const float FSR_RCAS_LIMIT = (0.25 - (1.0 / 16.0)); // This just limits RCAS a bit, but compile is disabled

vec3 FsrRcasFused(vec3 b, vec3 d, vec3 e, vec3 f, vec3 h)
{
        // Luma times 2.
        float bL = b.b * 0.5 + (b.r * 0.5 + b.g);
        float dL = d.b * 0.5 + (d.r * 0.5 + d.g);
        float eL = e.b * 0.5 + (e.r * 0.5 + e.g);
        float fL = f.b * 0.5 + (f.r * 0.5 + f.g);
        float hL = h.b * 0.5 + (h.r * 0.5 + h.g);
        // Noise detection.
        float nz = 0.25 * bL + 0.25 * dL + 0.25 * fL + 0.25 * hL - eL;
        nz = ASatF1(abs(nz) * APrxMedRcpF1(max(max(max(max(bL, dL), eL), fL), hL) - min(min(min(min(bL, dL), eL), fL), hL)));
        nz = -0.5 * nz + 1.0;
        // Min and max of ring.
        float mn4R = min(min(min(b.r, d.r), f.r), h.r);
        float mn4G = min(min(min(b.g, d.g), f.g), h.g);
        float mn4B = min(min(min(b.b, d.b), f.b), h.b);
        float mx4R = max(max(max(b.r, d.r), f.r), h.r);
        float mx4G = max(max(max(b.g, d.g), f.g), h.g);
        float mx4B = max(max(max(b.b, d.b), f.b), h.b);
        // Immediate constants for peak range.
        vec2 peakC = vec2(1.0, -1.0 * 4.0);
        // Limiters, these need to be high precision RCPs.
        float hitMinR = min(mn4R, e.r) * ARcpF1(4.0 * mx4R);
        float hitMinG = min(mn4G, e.g) * ARcpF1(4.0 * mx4G);
        float hitMinB = min(mn4B, e.b) * ARcpF1(4.0 * mx4B);
        float hitMaxR = (peakC.x - max(mx4R, e.r)) * ARcpF1(4.0 * mn4R + peakC.y);
        float hitMaxG = (peakC.x - max(mx4G, e.g)) * ARcpF1(4.0 * mn4G + peakC.y);
        float hitMaxB = (peakC.x - max(mx4B, e.b)) * ARcpF1(4.0 * mn4B + peakC.y);
        float lobeR = max(-hitMinR, hitMaxR);
        float lobeG = max(-hitMinG, hitMaxG);
        float lobeB = max(-hitMinB, hitMaxB);
        float lobe = max(-FSR_RCAS_LIMIT, min(max(max(lobeR, lobeG), lobeB), 0.0)) * uSharpness;
        // Apply noise removal.
#if defined(FSR_RCAS_DENOISE) && FSR_RCAS_DENOISE
        lobe *= nz;
#endif
        // Resolve, which needs the medium precision rcp approximation to avoid visible tonality changes.
        float rcpL = APrxMedRcpF1(4.0 * lobe + 1.0);
        return vec3(
                (lobe * b.r + lobe * d.r + lobe * h.r + lobe * f.r + e.r) * rcpL,
                (lobe * b.g + lobe * d.g + lobe * h.g + lobe * f.g + e.g) * rcpL,
                (lobe * b.b + lobe * d.b + lobe * h.b + lobe * f.b + e.b) * rcpL);
}

void main()
{
        // EASU produces the upscaled center pixel (ip = integer output pixel position).
        vec3 e = FsrEasuF(gl_FragCoord.xy - 0.5);
        // Four bilinear neighbors of the output pixel from the low-resolution texture.
        vec3 b = texture(uSrc, vUV + vec2(0.0, -uDstResInv.y)).rgb;
        vec3 d = texture(uSrc, vUV + vec2(-uDstResInv.x, 0.0)).rgb;
        vec3 f = texture(uSrc, vUV + vec2(uDstResInv.x, 0.0)).rgb;
        vec3 h = texture(uSrc, vUV + vec2(0.0, uDstResInv.y)).rgb;
        // RCAS sharpening on the fused neighborhood.
        vec3 rgb = FsrRcasFused(b, d, e, f, h);
        fragColor = vec4(rgb, 1.0);
}
