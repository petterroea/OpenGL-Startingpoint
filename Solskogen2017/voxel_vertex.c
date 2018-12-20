#version 130

uniform mat4 V;
uniform mat4 P;
uniform mat4 LightV;
uniform mat4 LightP;
uniform mat4 M;
uniform vec3 lightPos;

out vec3 ex_color;
out vec3 ex_normal;
out vec4 ShadowCoord;

out vec3 lightDir_cameraspace;
out vec3 normal_cameraspace;
out vec3 pos_worldspace;

in vec3 pos;
in vec3 color;
in vec3 normal;

void main()
{
	mat4 bias = mat4(0.5, 0.0, 0.0, 0.0,
		0.0, 0.5, 0.0, 0.0,
		0.0, 0.0, 0.5, 0.0,
		0.5, 0.5, 0.5, 1.0);
    //gl_Position = transform*gl_Position;
    //gl_Position = (gl_Vertex+vec4(pos.x,0.0,0.0,0.0))*transform;
    //gl_Position = gl_Vertex*transform;
	ex_color = color;
	ex_normal = normal;
    gl_Position = P*V*M*vec4(pos,1.0);
	pos_worldspace = (M*vec4(pos, 1.0)).xyz;
	ShadowCoord = bias*LightP*LightV*M*vec4(pos, 1.0);

	//Here, we send shadows and shit
	vec3 worldspace_pos = (M*vec4(pos, 1.0)).xyz;
	vec3 eyedir_cameraspace = -1.0*(V*M*vec4(pos, 1.0)).xyz;
	vec3 lightpos_cameraspace = (V*vec4(lightPos, 1.0)).xyz;
	lightDir_cameraspace = lightpos_cameraspace+eyedir_cameraspace;

	normal_cameraspace = (V*M*vec4(normal, 0)).xyz;
}
