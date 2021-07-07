#ifdef GL_ES
precision mediump float;
#endif

#define TWO_PI 6.28318530718

uniform vec2 u_resolution;
uniform vec2 u_mouse;
uniform float u_time;
uniform float player_angle;

float sine(float time){
    return abs(sin(time));
}

vec3 hsb2rgb( in vec3 c ){
    vec3 rgb = clamp(abs(mod(c.x*6.0+vec3(0.0,4.0,2.0),
    6.0)-3.0)-1.0,
    0.0,
    1.0 );
    rgb = rgb*rgb*(3.0-2.0*rgb);
    return c.z * mix( vec3(1.0), rgb, c.y);
}

float random (in vec2 st) {
    return fract(sin(dot(st.xy,
    vec2(12.9898,78.233)))*
    43758.5453123);
}

// Based on Morgan McGuire @morgan3d
// https://www.shadertoy.com/view/4dS3Wd
float noise (in vec2 st) {
    vec2 i = floor(st);
    vec2 f = fract(st);

    // Four corners in 2D of a tile
    float a = random(i);
    float b = random(i + vec2(1.0, 0.0));
    float c = random(i + vec2(0.0, 1.0));
    float d = random(i + vec2(1.0, 1.0));

    vec2 u = f * f * (3.0 - 2.0 * f);

    return mix(a, b, u.x) +
    (c - a)* u.y * (1.0 - u.x) +
    (d - b) * u.x * u.y;
}
    #define OCTAVES 6
float fbm (in vec2 st) {
    // Initial values
    float value = 0.0;
    float amplitude = .5;
    float frequency = 0.;
    //
    // Loop of octaves
    for (int i = 0; i < OCTAVES; i++) {
        value += amplitude * noise(st);
        st *= 2.;
        amplitude *= .5;
    }
    return value;
}

void angleColor(){
    vec2 st = vec2(gl_FragCoord.x / 2, gl_FragCoord.y) / u_resolution;

    vec2 toCenter = vec2(0.25)-st;
    float angle = atan(toCenter.y, toCenter.x);
    float radius = length(toCenter) * 2.0;

    vec3 color = hsb2rgb(vec3(((angle + player_angle) / TWO_PI) + 0.5, radius, 1.0));

    gl_FragColor = vec4(color, 0.75);
}

float LERP(float x, float x0, float x1, float y0, float y1){
    return y0 + (x - x0) * (y1 - y0) / (x1 - x0);
}


void noisePattern(){
    vec2 st = (gl_FragCoord.xy / u_resolution.xy);
    //st += st * abs(sin(u_time*0.1)*3.0);
    vec3 color = vec3(0.0);
    vec2 q = vec2(0.);
    q.x = fbm( st + 0.00 * u_time);
    q.y = fbm( st + vec2(1.0));

    vec2 r = vec2(0.);
    r.x = fbm( st + 1.0*q + vec2(1.7,9.2)+ 0.15*u_time + player_angle);
    r.y = fbm( st + 1.0*q + vec2(8.3,2.8)+ 0.126*u_time + player_angle);

    float f = fbm(st+r);

    color = mix(vec3(0.101961,0.619608,0.666667),
    vec3(0.666667,0.666667,0.498039),
    clamp((f*f)*4.0,0.0,1.0));

    color = mix(color,
    vec3(0,0,0.164706),
    clamp(length(q),0.0,1.0));

    color = mix(color,
    vec3(0.666667,1,1),
    clamp(length(r.x),0.0,1.0));

    gl_FragColor = vec4(LERP(gl_FragCoord.y, 0., u_resolution.y, 0.88, 0.27), LERP(gl_FragCoord.y, 0., u_resolution.y, 0.76, 0.52), LERP(gl_FragCoord.y, 0., u_resolution.y, 0.49, 0.82), 0.5) + vec4((f*f*f+.6*f*f+.5*f)*color, 0.4);
}


void main()
{
    noisePattern();
}
