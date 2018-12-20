#version 140

uniform sampler2D texture;
in vec2 tex_coord;

void main()
{
	gl_FragColor = texture2D(texture, tex_coord);
	//gl_FragColor = vec4(1.0, 0.0, 0.0, 1.0);
}