#version 130

in vec2 vertex;
in vec2 texcoord;

out vec2 tex_coord;

void main()
{
    //gl_Position = transform*gl_Position;
    //gl_Position = (gl_Vertex+vec4(pos.x,0.0,0.0,0.0))*transform;
    //gl_Position = gl_Vertex*transform;
    gl_Position = vec4(vertex, 0.0,1.0);
	tex_coord = texcoord;
    //color = gl_Color.xyz;
}
