shader_type canvas_item;
uniform vec2 NodePos;
uniform vec2 NodePos2;
uniform float radius;
uniform sampler2D mask;

varying mat4 matrix;

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;

vec3 rgb2hsv(vec3 c) {
    vec4 K = vec4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    vec4 p = mix(vec4(c.bg, K.wz), vec4(c.gb, K.xy), step(c.b, c.g));
    vec4 q = mix(vec4(p.xyw, c.r), vec4(c.r, p.yzx), step(p.x, c.r));

    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10;
    return vec3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

vec3 hsv2rgb(vec3 c) {
    vec4 K = vec4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

void vertex() {
    matrix = inverse(CANVAS_MATRIX);
}

void fragment() {
  	vec4 UVColor = texture(screen_texture, SCREEN_UV);
  	vec4 maskColor = texture(mask, UV);
	vec3 UVColorHSV = rgb2hsv(UVColor.xyz);
	UVColorHSV.y = 0.0f;
	vec4 grayScaleUVColor = vec4(hsv2rgb(UVColorHSV), 1.0f);
	
	vec4 mushColor = grayScaleUVColor;
			
	if (maskColor.g == 1.0f)
	{
		mushColor = vec4(1.0f,1.0f,1.0f,1.0f) - UVColor;
		mushColor.a = 1.0f;
	}
	else if (maskColor.r == 1.0f)
	{
		mushColor = UVColor;
	}
	
	vec4 grayScaleColor = grayScaleUVColor - vec4(0.1,0.1,0.1,0);
	COLOR = mix(mushColor, grayScaleColor, maskColor.b);	
}
