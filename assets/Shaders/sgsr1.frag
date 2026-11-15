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
// Qualcomm Snapdragon Game Super Resolution (SGSR) v1 fragment shader -
// mobile + edge direction variant, ported from the official GLSL release
// (snapdragon-gsr, sgsr/v1/include/glsl/sgsr1_shader_mobile_edge_direction.frag,
// Copyright (c) 2025 Qualcomm Innovation Center, BSD-3-Clause).
//
// Differences from the verbatim file (all verified geometry-preserving):
// - textureGather is expanded into explicit sampler fetches (GL 3.3 has no
//   gather). The gather quads left/right/upDown are replaced by the identical
//   12-tap support lattice of the official kernel: a 4x4 texel block minus its
//   corners around the resolve position (imgCoordPixel + offsets {-1..+2}),
//   which matches fastLanczos2's support radius exactly. The fetches use
//   texture() at exact texel centers, (texel + 0.5) * uTexelStep: LINEAR
//   filtering at a texel center returns that texel with weight 1.0, so the
//   values are identical to texelFetch while the sampler hardware performs the
//   edge clamping (the official shader relies on CLAMP_TO_EDGE) with no
//   per-tap clamp ALU.
// - The filter channel is luma instead of the green channel (the official
//   OperationMode = 1 uses green; the widely-validated luma variant is
//   more accurate on colored edges and costs 3 extra MADs per tap).
// - imgCoord uses vec2(-0.5, -0.5): the official shader is written for the
//   Vulkan y-down frame; this is the pixel-exact equivalent for the
//   OpenGL y-up frame (the vertical mirror of the official mapping).
// - Edge direction is always enabled. The official edge-direction file guards
//   this code path with "#define UseEdgeDirection" (shipped commented-out;
//   Qualcomm's documentation recommends the edge-direction path for quality),
//   so the define is fixed on here - it is the variant's namesake.
//---------------------------------------------------------------------------------

#version 330 core
uniform sampler2D uSrc;         // Low-resolution scene texture (LINEAR, CLAMP_TO_EDGE)
uniform vec2 uInputSize;        // Low-resolution input size in pixels
uniform vec2 uTexelStep;        // 1.0 / input size
uniform float uEdgeSharpness;   // 1.0 (minimum) .. 2.0 (maximum, official default)

in vec2 vUV;
out vec4 fragColor;

// Official configuration constants (mobile + edge direction variant:
// EdgeThreshold 8.0/255.0, EdgeSharpness 2.0, high-contrast clamp +/-23.0/255.0)
const float EdgeThreshold = 8.0 / 255.0;

float getY(vec3 rgba)
{
        return 0.2126 * rgba.x + 0.7152 * rgba.y + 0.0722 * rgba.z;
}

// Edge-clamped luma fetch via the sampler: exact texel value at the texel
// center, clamping handled by the hardware.
float fetchY(ivec2 texel)
{
        return getY(texture(uSrc, (vec2(texel) + 0.5) * uTexelStep).rgb);
}

// Approximate two-lobe Lanczos window over squared distance, official math
float fastLanczos2(float x)
{
        float wA = x - 4.0;
        float wB = x * wA - wA;
        wA *= wA;
        return wB * wA;
}

// Direction-aware filter weight; data = (std, edge direction), official math
vec2 weightY(float dx, float dy, float c, vec3 data)
{
        float std = data.x;
        vec2 dir = data.yz;
        float edgeDis = ((dx * dir.y) + (dy * dir.x));
        float x = ((dx * dx) + (dy * dy)) + ((edgeDis * edgeDis) * ((clamp((c * c) * std, 0.0, 1.0) * 0.7 - 1.0)));
        float w = fastLanczos2(x);
        return vec2(w, w * c);
}

// Normalized edge direction from the two diagonal gradients of the center quad,
// official math (edgeDirection of the left / right gather quads)
vec2 edgeDirection(float g1, float g2)
{
        vec2 delta;
        delta.x = g1 + g2;
        delta.y = g1 - g2;
        return delta * inversesqrt(dot(delta, delta) + 3.075740e-05);
}

void main()
{
        // Bilinear center color: the actual resolved pixel before enhancement
        vec4 color = texture(uSrc, vUV);
        float yc = getY(color.rgb);

        // Resolve position in input texel space (OpenGL y-up frame)
        vec2 imgCoord = vUV * uInputSize + vec2(-0.5, -0.5);
        vec2 imgCoordPixel = floor(imgCoord);
        vec2 pl = imgCoord - imgCoordPixel;
        ivec2 p = ivec2(imgCoordPixel);

        // The center 2x2 quad of input texels around the resolve position
        float q00 = fetchY(p);                    // (px,     py)
        float q01 = fetchY(p + ivec2(0, 1));      // (px,     py + 1)
        float q10 = fetchY(p + ivec2(1, 0));      // (px + 1, py)
        float q11 = fetchY(p + ivec2(1, 1));      // (px + 1, py + 1)

        // Edge vote on the vertical center pair, official math (luma channel)
        float edgeVote = abs(q01 - q00) + abs(yc - q00) + abs(yc - q01);
        if (edgeVote > EdgeThreshold)
        {
                // 12-tap support: 4x4 block minus corners, official lattice.
                // Tap value = luma - mean; the mean of the center quad is
                // subtracted everywhere exactly like the official shader.
                // Three of the twelve taps are members of the center quad and
                // reuse the values fetched above.
                float mean = (q00 + q01 + q10 + q11) * 0.25;
                float leftY = q01 - mean;
                float leftZ = q00 - mean;
                float rightX = q11 - mean;
                float rightW = q10 - mean;
                float leftX = fetchY(p + ivec2(-1, 1)) - mean;
                float leftW = fetchY(p + ivec2(-1, 0)) - mean;
                float rightY = fetchY(p + ivec2(2, 1)) - mean;
                float rightZ = fetchY(p + ivec2(2, 0)) - mean;
                float upDownX = fetchY(p + ivec2(0, -1)) - mean;
                float upDownY = fetchY(p + ivec2(1, -1)) - mean;
                float upDownZ = fetchY(p + ivec2(1, 2)) - mean;
                float upDownW = fetchY(p + ivec2(0, 2)) - mean;

                float sum = (abs(leftX) + abs(leftY) + abs(leftZ) + abs(leftW)) +
                        (abs(rightX) + abs(rightY) + abs(rightZ) + abs(rightW)) +
                        (abs(upDownX) + abs(upDownY) + abs(upDownZ) + abs(upDownW));
                float sumMean = 1.014185e+01 / sum;
                float std = sumMean * sumMean;
                // Diagonal gradients of the center quad (45 degree basis)
                float g1 = q11 - q00;
                float g2 = q10 - q01;
                vec3 data = vec3(std, edgeDirection(g1, g2));

                vec2 aWY = weightY(pl.x, pl.y + 1.0, upDownX, data);
                aWY += weightY(pl.x - 1.0, pl.y + 1.0, upDownY, data);
                aWY += weightY(pl.x - 1.0, pl.y - 2.0, upDownZ, data);
                aWY += weightY(pl.x, pl.y - 2.0, upDownW, data);
                aWY += weightY(pl.x + 1.0, pl.y - 1.0, leftX, data);
                aWY += weightY(pl.x, pl.y - 1.0, leftY, data);
                aWY += weightY(pl.x, pl.y, leftZ, data);
                aWY += weightY(pl.x + 1.0, pl.y, leftW, data);
                aWY += weightY(pl.x - 1.0, pl.y - 1.0, rightX, data);
                aWY += weightY(pl.x - 2.0, pl.y - 1.0, rightY, data);
                aWY += weightY(pl.x - 2.0, pl.y, rightZ, data);
                aWY += weightY(pl.x - 1.0, pl.y, rightW, data);

                float finalY = aWY.y / aWY.x;
                float maxY = max(max(leftY, leftZ), max(rightX, rightW));
                float minY = min(min(leftY, leftZ), min(rightX, rightW));
                // color.w of the official shader = bilinear center luma minus the quad mean
                float deltaY = clamp(uEdgeSharpness * finalY, minY, maxY) - (yc - mean);

                // smooth high contrast input
                deltaY = clamp(deltaY, -23.0 / 255.0, 23.0 / 255.0);

                color.x = clamp((color.x + deltaY), 0.0, 1.0);
                color.y = clamp((color.y + deltaY), 0.0, 1.0);
                color.z = clamp((color.z + deltaY), 0.0, 1.0);
        }

        color.w = 1.0; // alpha channel is not used
        fragColor = color;
}
