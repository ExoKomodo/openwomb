#version 330 core
out vec4 out_frag_color;

void main()
{
  if (int(gl_FragCoord.y / 100.0f) % 2 == 0)
  {
    out_frag_color = vec4(
      1.0f,
      0.0f,
      0.0f,
      1.0f
    );
  }
  else
  {
    out_frag_color = vec4(
      0.0f,
      1.0f,
      0.0f,
      1.0f
    );
  }
} 
