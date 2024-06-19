#version 330 core

layout (location = 0) in vec3 in_position;   // the in_position variable has attribute position 0
layout (location = 1) in vec2 in_tex_coordinate;   // the in_tex_coordinate variable has attribute position 1

out vec2 out_texture_coordinate;

void main()
{
    gl_Position = vec4(in_position.x, in_position.y, in_position.z, 1.0);
    out_texture_coordinate = in_tex_coordinate;
}
