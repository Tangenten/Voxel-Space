#ifdef GL_ES
precision mediump float;
#endif

#define TWO_PI 6.28318530718

uniform vec2 u_resolution;
uniform vec2 u_mouse;
uniform float u_time;
uniform float u_audio_average;

void main()
{
    vec2 st = vec2(gl_FragCoord.x / 2, gl_FragCoord.y) / u_resolution;

    gl_FragColor = vec4(vec3(gl_Color.r - (u_audio_average / (6. - st.y)), gl_Color.g - (u_audio_average / (6. - st.y)), gl_Color.b - (u_audio_average / (6. - st.y))), gl_Color.a);
}
