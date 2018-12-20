#version 400

//Heyy renderpasses
subroutine void RenderPass();
subroutine uniform RenderPass currentPass;

in vec3 ex_normal;
in vec3 ex_color;
in vec4 ShadowCoord;
in vec3 lightDir_cameraspace;
in vec3 normal_cameraspace;
in vec3 pos_worldspace;

layout(location=0) out vec4 color;
layout(location=1) out float depth;
layout(location=2) out vec3 normal;
layout(location=3) out vec3 worldPos;

uniform vec3 light_pos;
uniform sampler2D shadowMap;

void main()
{
	//Bootstrap for render pass
	currentPass();
}

//Render passes

subroutine(RenderPass)
void Default()
{
	//Config
	vec3 lightColor = vec3(1.0, 1.0, 1.0);
	float lightIntensity = 1.0;
	//How the fuck do you calculate colors
	vec3 diffuseColor = ex_color;

	vec3 n = normalize( normal_cameraspace );
	// Direction of the light (from the fragment to the light)
	vec3 l = normalize( lightDir_cameraspace );

	float cosTheta = clamp( dot( n,l ), 0,1 );

	//I guess find different light sources and add them?
	//vec3 ambientLight = vec3(0.2, 0.2, 0.2);
	vec3 ambientLight = vec3(0.2, 0.2, 0.2);
	vec3 lightSum = ambientLight;
	float visibility = 1.0;
	float shadowSample = texture( shadowMap, ShadowCoord.xy ).z;

	float bias = 0.005*tan(acos(cosTheta)); // cosTheta is dot( n,l ), clamped between 0 and 1
	bias = clamp(bias, 0.0,0.01);

	if ( shadowSample  <  ShadowCoord.z-bias){ //Added a bias!
		visibility = 0.3;
	}
	//eeh todo opengl lighting

	color = vec4(ambientLight + (diffuseColor*visibility*lightColor*lightIntensity*cosTheta), 1.0);
	//gl_FragColor = vec4((ex_normal+vec3(1.0,1.0,1.0))/2.0, 1.0);
	//gl_FragColor = vec4(1.0, 0.0, 0.0, 1.0);
	depth = gl_FragCoord.z;
	//gl_FragColor = vec4(shadowSample, shadowSample, shadowSample, 1.0);
}

subroutine(RenderPass)
void Deferred()
{
	color = vec4(ex_color, 1.0);
	worldPos = pos_worldspace;
	color = vec4((ex_normal+vec3(1))*0.5, 1.0);
	depth = gl_FragCoord.z;
}

subroutine(RenderPass)
void Depth()
{
	depth = gl_FragCoord.z;
}
