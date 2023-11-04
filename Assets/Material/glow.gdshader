shader_type canvas_item;

render_mode unshaded, blend_add;

uniform vec3 uv1_scale = vec3(1.0, 1.0, 1.0);
uniform vec3 uv1_offset = vec3(0.0, 0.0, 0.0);
uniform float variation = 0.0;
varying float elapsed_time;
void vertex() {
	elapsed_time = TIME;
	UV = UV*uv1_scale.xy+uv1_offset.xy;
}
float rand(vec2 x) {
    return fract(cos(mod(dot(x, vec2(13.9898, 8.141)), 3.14)) * 43758.5453);
}
vec2 rand2(vec2 x) {
    return fract(cos(mod(vec2(dot(x, vec2(13.9898, 8.141)),
						      dot(x, vec2(3.4562, 17.398))), vec2(3.14))) * 43758.5453);
}
vec3 rand3(vec2 x) {
    return fract(cos(mod(vec3(dot(x, vec2(13.9898, 8.141)),
							  dot(x, vec2(3.4562, 17.398)),
                              dot(x, vec2(13.254, 5.867))), vec3(3.14))) * 43758.5453);
}
float param_rnd(float minimum, float maximum, float seed) {
	return minimum+(maximum-minimum)*rand(vec2(seed));
}
vec3 rgb2hsv(vec3 c) {
	vec4 K = vec4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
	vec4 p = c.g < c.b ? vec4(c.bg, K.wz) : vec4(c.gb, K.xy);
	vec4 q = c.r < p.x ? vec4(p.xyw, c.r) : vec4(c.r, p.yzx);
	float d = q.x - min(q.w, q.y);
	float e = 1.0e-10;
	return vec3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}
vec3 hsv2rgb(vec3 c) {
	vec4 K = vec4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
	vec3 p = abs(fract(c.xxx + K.xyz) * 6.0 - K.www);
	return c.z * mix(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}
vec2 sdLine(vec2 p, vec2 a, vec2 b) {
	vec2 pa = p-a, ba = b-a;
	float h = clamp(dot(pa,ba)/dot(ba,ba), 0.0, 1.0);
	return vec2(length(pa-ba*h), h);
}
float ThickLine(vec2 uv, vec2 posA, vec2 posB, float radiusInv)
{
	return clamp(1.1-20.0*sdLine(uv, posA, posB).x, 0.0, 1.0);
}
// makes a rune in the 0..1 uv space. Seed is which rune to draw.
// passes back gray in x and derivates for lighting in yz
float Rune(vec2 uv, float s) {
	float finalLine = 0.0;
	vec2 seed = floor(uv)-rand2(vec2(s));
	uv = fract(uv);
	for (int i = 0; i < 4; i++)	// number of strokes
	{
		vec2 posA = rand2(floor(seed+0.5));
		vec2 posB = rand2(floor(seed+1.5));
		seed += 2.0;
		// expand the range and mod it to get a nicely distributed random number - hopefully. :)
		posA = fract(posA * 128.0);
		posB = fract(posB * 128.0);
		// each rune touches the edge of its box on all 4 sides
		if (i == 0) posA.y = 0.0;
		if (i == 1) posA.x = 0.999;
		if (i == 2) posA.x = 0.0;
		if (i == 3) posA.y = 0.999;
		// snap the random line endpoints to a grid 2x3
		vec2 snaps = vec2(2.0, 3.0);
		posA = (floor(posA * snaps) + 0.5) / snaps;	// + 0.5 to center it in a grid cell
		posB = (floor(posB * snaps) + 0.5) / snaps;
		//if (distance(posA, posB) < 0.0001) continue;	// eliminate dots.
		// Dots (degenerate lines) are not cross-GPU safe without adding 0.001 - divide by 0 error.
		finalLine = max(finalLine, ThickLine(uv, posA, posB + 0.001, 20.0));
	}
	return finalLine;
}
vec2 transform2_clamp(vec2 uv) {
	return clamp(uv, vec2(0.0), vec2(1.0));
}
vec2 transform2(vec2 uv, vec2 translate, float rotate, vec2 scale) {
 	vec2 rv;
	uv -= translate;
	uv -= vec2(0.5);
	rv.x = cos(rotate)*uv.x + sin(rotate)*uv.y;
	rv.y = -sin(rotate)*uv.x + cos(rotate)*uv.y;
	rv /= scale;
	rv += vec2(0.5);
	return rv;	
}
float pingpong(float a, float b)
{
  return (b != 0.0) ? abs(fract((a - b) / (b * 2.0)) * b * 2.0 - b) : 0.0;
}
vec2 curve(float t, vec2 a, vec2 b, vec2 c, vec2 d) {
    vec2 bc = c-b;
    vec2 da = a-d;
    return (b+0.5*t*(bc+b-a+t*(a-b+bc+t*3.0*(bc*3.0+da+t*5.0/3.0*(-bc*3.0-da+t*0.4*(bc*3.0+da))))));
}
vec2 dcurve(float t, vec2 a, vec2 b, vec2 c, vec2 d) {
	return (curve(t+0.0001, a, b, c, d)-curve(t, a, b, c, d))/0.0001;
}
vec2 solve_cubic(vec2 uv, vec2 a, vec2 b, vec2 c, vec2 d) {
	float t = 0.5;
	vec2 pos, dv;
	float current_sign = 0.0;
	float dt = 1.0;
	for (int i = 0; i < 20; ++i) {
		pos = curve(t, a, b, c, d);
		dv = dcurve(t, a, b, c, d);
		float new_sign = sign(dot(normalize(uv - pos), dv));
		if (new_sign != current_sign) {
			current_sign = new_sign;
			dt *= 0.5;
		}
        t += dt*current_sign;
	}
    t = clamp(t, 0.0, 1.0);
	return vec2(t, length(uv-curve(t, a, b, c, d)));
}
float shape_circle(vec2 uv, float sides, float size, float edge) {
	uv = 2.0*uv-1.0;
	edge = max(edge, 1.0e-8);
	float distance = length(uv);
	return clamp((1.0-distance/size)/edge, 0.0, 1.0);
}
float shape_polygon(vec2 uv, float sides, float size, float edge) {
	uv = 2.0*uv-1.0;
	edge = max(edge, 1.0e-8);
	float angle = atan(uv.x, uv.y)+3.14159265359;
	float slice = 6.28318530718/sides;
	return clamp((1.0-(cos(floor(0.5+angle/slice)*slice-angle)*length(uv))/size)/edge, 0.0, 1.0);
}
float shape_star(vec2 uv, float sides, float size, float edge) {
	uv = 2.0*uv-1.0;
	edge = max(edge, 1.0e-8);
	float angle = atan(uv.x, uv.y);
	float slice = 6.28318530718/sides;
	return clamp((1.0-(cos(floor(angle*sides/6.28318530718-0.5+2.0*step(fract(angle*sides/6.28318530718), 0.5))*slice-angle)*length(uv))/size)/edge, 0.0, 1.0);
}
float shape_curved_star(vec2 uv, float sides, float size, float edge) {
	uv = 2.0*uv-1.0;
	edge = max(edge, 1.0e-8);
	float angle = 2.0*(atan(uv.x, uv.y)+3.14159265359);
	float slice = 6.28318530718/sides;
	return clamp((1.0-cos(floor(0.5+0.5*angle/slice)*2.0*slice-angle)*length(uv)/size)/edge, 0.0, 1.0);
}
float shape_rays(vec2 uv, float sides, float size, float edge) {
	uv = 2.0*uv-1.0;
	edge = 0.5*max(edge, 1.0e-8)*size;
	float slice = 6.28318530718/sides;
	float angle = mod(atan(uv.x, uv.y)+3.14159265359, slice)/slice;
	return clamp(min((size-angle)/edge, angle/edge), 0.0, 1.0);
}
vec2 rotate(vec2 uv, float rotate) {
 	vec2 rv;
	rv.x = cos(rotate)*uv.x + sin(rotate)*uv.y;
	rv.y = -sin(rotate)*uv.x + cos(rotate)*uv.y;
	return rv;	
}
const float p_o79026_gradient_0_pos = 0.000000000;
const vec4 p_o79026_gradient_0_col = vec4(0.000000000, 0.000000000, 0.000000000, 1.000000000);
const float p_o79026_gradient_1_pos = 0.840622000;
const vec4 p_o79026_gradient_1_col = vec4(0.593690991, 0.937500000, 0.216064006, 1.000000000);
const float p_o79026_gradient_2_pos = 1.000000000;
const vec4 p_o79026_gradient_2_col = vec4(0.537508011, 0.945312023, 0.524353027, 1.000000000);
vec4 o79026_gradient_gradient_fct(float x) {
  if (x < p_o79026_gradient_0_pos) {
    return p_o79026_gradient_0_col;
  } else if (x < p_o79026_gradient_1_pos) {
    return mix(p_o79026_gradient_0_col, p_o79026_gradient_1_col, ((x-p_o79026_gradient_0_pos)/(p_o79026_gradient_1_pos-p_o79026_gradient_0_pos)));
  } else if (x < p_o79026_gradient_2_pos) {
    return mix(p_o79026_gradient_1_col, p_o79026_gradient_2_col, ((x-p_o79026_gradient_1_pos)/(p_o79026_gradient_2_pos-p_o79026_gradient_1_pos)));
  }
  return p_o79026_gradient_2_col;
}
const float p_o79043_default_in1 = 0.000000000;
const float p_o79043_default_in2 = 0.000000000;
const float p_o79013_value = 0.580000000;
const float p_o79013_width = 0.470000000;
const float p_o79013_contrast = 0.000000000;
const float p_o79045_repeat = 1.000000000;
float o79045_input_width_map(vec2 uv, float _seed_variation_) {
return 1.0;
}
vec2 bezier_uv_o79045(vec2 uv, float _seed_variation_) {	
	vec2 v[] = {vec2(0.310039997, 0.761157990), vec2(0.499318004, 0.180080995), vec2(0.702592015, 0.755797982), vec2(0.210095003, 0.410641015), vec2(0.807455003, 0.405629009)};
	int l = v.length();
	vec2 best_bezier = vec2(0, 1000.0);
	float offset = 0.0;
	int i;
	for (i = 0; i < l; i++) {
		vec2 bezier = solve_cubic(uv, v[i], v[(i+1)%l], v[(i+2)%l], v[(i+3)%l]);
		if (abs(bezier.y) < abs(best_bezier).y) {
			best_bezier = bezier;
			offset = float(i)+1.0;
		}
	}
	
	vec2 local_uv = vec2(best_bezier.x, best_bezier.y+0.5);
	vec2 uvtest = step(vec2(0.5), abs(local_uv-vec2(0.5)));
	float k = local_uv.x;
	if (true) {
		k += offset;
		k /= float(l+1);
	}
	k *= p_o79045_repeat;
	return mix(vec2(fract(k), local_uv.y), vec2(0.0), max(uvtest.x, uvtest.y));
}const float p_o79024_default_in1 = 0.000000000;
const float p_o79024_default_in2 = 0.000000000;
const float p_o79025_translate_x = 0.000000000;
const float p_o79025_translate_y = 0.000000000;
const float p_o79025_rotate = 0.000000000;
const float p_o79025_scale_x = 0.990000000;
const float p_o79025_scale_y = 0.035000000;
const float seed_o79014 = 0.000000000;
const float p_o79014_columns = 301.000000000;
const float p_o79014_rows = 1.000000000;
const float p_o79036_translate_x = 1.000000000;
const float p_o79036_translate_y = 0.000000000;
const float p_o79036_rotate = 0.000000000;
const float p_o79036_scale_x = 0.425000000;
const float p_o79036_scale_y = 1.000000000;
const float p_o79039_default_in1 = 0.000000000;
const float p_o79039_default_in2 = 0.000000000;
const float p_o79038_color = 0.230000000;
const float p_o79015_repeat = 1.000000000;
const float p_o79015_rotate = 0.000000000;
const float p_o79015_gradient_0_pos = 0.000000000;
const vec4 p_o79015_gradient_0_col = vec4(0.000000000, 0.000000000, 0.000000000, 1.000000000);
const float p_o79015_gradient_1_pos = 1.000000000;
const vec4 p_o79015_gradient_1_col = vec4(1.000000000, 1.000000000, 1.000000000, 1.000000000);
vec4 o79015_gradient_gradient_fct(float x) {
  if (x < p_o79015_gradient_0_pos) {
    return p_o79015_gradient_0_col;
  } else if (x < p_o79015_gradient_1_pos) {
    return mix(p_o79015_gradient_0_col, p_o79015_gradient_1_col, ((x-p_o79015_gradient_0_pos)/(p_o79015_gradient_1_pos-p_o79015_gradient_0_pos)));
  }
  return p_o79015_gradient_1_col;
}
const float p_o79044_default_in1 = 0.000000000;
const float p_o79044_default_in2 = 1.000000000;
const float p_o79042_cx = 0.000000000;
const float p_o79042_cy = 0.000000000;
const float p_o79042_rotate = 36.000000000;
const float p_o79041_value = 0.400000000;
const float p_o79041_width = 0.100000000;
const float p_o79041_contrast = 0.000000000;
const float p_o79040_sides = 9.000000000;
const float p_o79040_radius = 1.540000000;
const float p_o79040_edge = 1.090000000;
void fragment() {
	float _seed_variation_ = variation;
	vec2 uv = fract(UV);
float o79014_0_1_f = Rune(vec2(p_o79014_columns, p_o79014_rows)*(transform2_clamp(transform2((bezier_uv_o79045((uv), _seed_variation_)), vec2(p_o79025_translate_x*(2.0*1.0-1.0), p_o79025_translate_y*(2.0*1.0-1.0)), p_o79025_rotate*0.01745329251*(2.0*1.0-1.0), vec2(p_o79025_scale_x*(2.0*1.0-1.0), p_o79025_scale_y*(2.0*1.0-1.0))))), float((seed_o79014+fract(_seed_variation_))));
vec4 o79025_0_1_rgba = vec4(vec3(o79014_0_1_f), 1.0);
float o79037_0_1_f = (elapsed_time);
float o79038_0_1_f = p_o79038_color;
float o79039_0_clamp_false = o79037_0_1_f*o79038_0_1_f;
float o79039_0_clamp_true = clamp(o79039_0_clamp_false, 0.0, 1.0);
float o79039_0_1_f = o79039_0_clamp_false;
float o79015_0_r = 0.5+(cos(p_o79015_rotate*0.01745329251)*((transform2((bezier_uv_o79045((uv), _seed_variation_)), vec2(p_o79036_translate_x*(2.0*o79039_0_1_f-1.0), p_o79036_translate_y*(2.0*1.0-1.0)), p_o79036_rotate*0.01745329251*(2.0*1.0-1.0), vec2(p_o79036_scale_x*(2.0*1.0-1.0), p_o79036_scale_y*(2.0*1.0-1.0)))).x-0.5)+sin(p_o79015_rotate*0.01745329251)*((transform2((bezier_uv_o79045((uv), _seed_variation_)), vec2(p_o79036_translate_x*(2.0*o79039_0_1_f-1.0), p_o79036_translate_y*(2.0*1.0-1.0)), p_o79036_rotate*0.01745329251*(2.0*1.0-1.0), vec2(p_o79036_scale_x*(2.0*1.0-1.0), p_o79036_scale_y*(2.0*1.0-1.0)))).y-0.5))/(cos(abs(mod(p_o79015_rotate, 90.0)-45.0)*0.01745329251)*1.41421356237);vec4 o79015_0_1_rgba = o79015_gradient_gradient_fct(fract(o79015_0_r*p_o79015_repeat));
vec4 o79036_0_1_rgba = o79015_0_1_rgba;
float o79024_0_clamp_false = (dot((o79025_0_1_rgba).rgb, vec3(1.0))/3.0)*(dot((o79036_0_1_rgba).rgb, vec3(1.0))/3.0);
float o79024_0_clamp_true = clamp(o79024_0_clamp_false, 0.0, 1.0);
float o79024_0_1_f = o79024_0_clamp_false;
vec4 o79045_0_1_rgba = vec4(vec3(o79024_0_1_f), 1.0);
float o79013_0_step = clamp(((dot((o79045_0_1_rgba).rgb, vec3(1.0))/3.0) - (p_o79013_value))/max(0.0001, p_o79013_width)+0.5, 0.0, 1.0);
float o79013_0_false = clamp((min(o79013_0_step, 1.0-o79013_0_step) * 2.0) / (1.0 - p_o79013_contrast), 0.0, 1.0);
float o79013_0_true = 1.0-o79013_0_false;float o79013_0_1_f = o79013_0_false;
vec2 o79042_0_c = vec2(0.5+p_o79042_cx, 0.5+p_o79042_cy);float o79040_0_1_f = shape_polygon((rotate((uv)-o79042_0_c, p_o79042_rotate*0.01745329251)+o79042_0_c), p_o79040_sides, p_o79040_radius*1.0, p_o79040_edge*1.0);
float o79041_0_step = clamp((o79040_0_1_f - (p_o79041_value))/max(0.0001, p_o79041_width)+0.5, 0.0, 1.0);
float o79041_0_false = clamp((min(o79041_0_step, 1.0-o79041_0_step) * 2.0) / (1.0 - p_o79041_contrast), 0.0, 1.0);
float o79041_0_true = 1.0-o79041_0_false;float o79041_0_1_f = o79041_0_false;
vec4 o79042_0_1_rgba = vec4(vec3(o79041_0_1_f), 1.0);
float o79044_0_clamp_false = (dot((o79042_0_1_rgba).rgb, vec3(1.0))/3.0)*p_o79044_default_in2;
float o79044_0_clamp_true = clamp(o79044_0_clamp_false, 0.0, 1.0);
float o79044_0_2_f = o79044_0_clamp_false;
float o79043_0_clamp_false = o79013_0_1_f+o79044_0_2_f;
float o79043_0_clamp_true = clamp(o79043_0_clamp_false, 0.0, 1.0);
float o79043_0_1_f = o79043_0_clamp_false;
vec4 o79026_0_1_rgba = o79026_gradient_gradient_fct(o79043_0_1_f);

	vec4 color_tex = o79026_0_1_rgba;
	color_tex = mix(pow((color_tex + vec4(0.055)) * (1.0 / (1.0 + 0.055)),vec4(2.4)),color_tex * (1.0 / 12.92),lessThan(color_tex,vec4(0.04045)));
	COLOR = color_tex;

}



