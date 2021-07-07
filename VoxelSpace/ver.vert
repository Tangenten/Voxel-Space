#ifdef GL_ES
precision lowp float;
#endif

#define TWO_PI 6.28318530718

uniform vec2 u_resolution;
uniform vec2 u_mouse;
uniform float u_time;
uniform float player_height;
uniform float u_audio_average;

float sine(float time){
    return sin(time);
}

float random (vec2 st) {
    return fract(sin(dot(st.xy, vec2(12.9898,78.233))) * 43758.5453123);
}

void main()
{
    vec2 st = (u_resolution.xy / gl_Vertex.xy);
    vec4 v = gl_Vertex;
    v.x += sine(u_time + (v.y - player_height));
    v.y += (u_audio_average * 16) * st.y;

    gl_Position = gl_ModelViewProjectionMatrix * v;

    gl_TexCoord[0] = gl_TextureMatrix[0] * gl_MultiTexCoord0;

    gl_FrontColor = gl_Color;
}
