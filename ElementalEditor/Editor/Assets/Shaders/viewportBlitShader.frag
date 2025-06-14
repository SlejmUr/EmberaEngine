#version 440 core

layout (location = 0) out vec4 FragColor;

// blit_shader.frag
uniform sampler2D INPUT_TEXTURE;
uniform vec4 sourceDimensions; // (x0, y0, x1, y1) in source texture
uniform vec4 destinationDimensions; // (x0, y0, x1, y1) in screen-space

in vec2 texCoords;

void main() {
    // Where are we in screen-space?
    vec2 screenPos = texCoords * (destinationDimensions.zw - destinationDimensions.xy) + destinationDimensions.xy;

    // Normalize screenPos to [0,1] within dst rect
    vec2 dstUV = (screenPos - destinationDimensions.xy) / (destinationDimensions.zw - destinationDimensions.xy);

    // Use that to interpolate source rect
    vec2 srcUV = mix(sourceDimensions.xy, sourceDimensions.zw, dstUV);

    // Sample using srcUV in [0,1] space
    FragColor = texture(INPUT_TEXTURE, srcUV);
}
