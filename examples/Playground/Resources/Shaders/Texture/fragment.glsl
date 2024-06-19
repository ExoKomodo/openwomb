#version 330 core

in vec2 out_texture_coordinate;

out vec4 out_frag_color;

uniform vec2 in_viewport;
uniform sampler2D in_texture;

void main()
{
  // out_frag_color = texture(in_texture, TexCoord)
  out_frag_color = vec4(
    1.0f - out_texture_coordinate.x,
    1.0f - out_texture_coordinate.y,
    0.0f,
    1.0f
  );
} 
