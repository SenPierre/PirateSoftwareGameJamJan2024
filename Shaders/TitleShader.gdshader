shader_type canvas_item;

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

void fragment() {
	vec4 UVColor = texture(TEXTURE, UV);
	vec3 UVColorHSV = rgb2hsv(UVColor.xyz);
	UVColorHSV.y = 0.0f;
	vec4 grayScaleUVColor = vec4(hsv2rgb(UVColorHSV), 1.0f);
			
	float mask = sin((UV.x + TIME / 50.0f) * 10.0f);
			
	if (mask > 0.0f)
	{
		COLOR = mix(grayScaleUVColor, vec4(1.0f,1.0f,1.0f,1.0f) - UVColor, mask);
		COLOR.a = UVColor.a;
	}
	else 
	{
		COLOR = mix(grayScaleUVColor, UVColor, -mask);
		COLOR.a = UVColor.a;
	}
}
