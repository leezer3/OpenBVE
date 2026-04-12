#version 410 core

in vec2 vUv;

uniform sampler2D uTexture;
uniform bool uHasTexture;
uniform float uAlphaCutoff;
uniform float uMaterialAlpha; // Material color alpha (0.0–1.0), allows semi-transparent faces to skip shadow casting

out vec4 fragColor;

void main()
{
    // Start with the base material alpha
    float alpha = uMaterialAlpha;

    if (uHasTexture)
    {
        alpha *= texture(uTexture, vUv).a;
    }

    // Discard fragments whose effective alpha is below the cutoff
    // This prevents semi-transparent glass meshes from casting full opaque shadows
    if (alpha <= uAlphaCutoff)
    {
        discard;
    }

    fragColor = vec4(1.0);
}
