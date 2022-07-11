
uniform float2 _MinUVBounds;
uniform float2 _MaxUVBounds;
uniform  sampler2D _UnlockableTexture;

float remapRange (float low1, float high1, float low2, float high2, float value)
{
	return low2 + (value - low1) * (high2 - low2) / (high1 - low1);
}

float GetRemappedUVColor(float2 uv)
{
	float color = 0;
	
	if(uv.x>=_MinUVBounds.x && uv.x<=_MaxUVBounds.x && uv.y>=_MinUVBounds.y && uv.y<=_MaxUVBounds.y)
	{
		float v = remapRange(_MinUVBounds.y, _MaxUVBounds.y, 0, 1, uv.y);
		float u = remapRange(_MinUVBounds.x, _MaxUVBounds.x, 0, 1, uv.x);
		color = tex2D(_UnlockableTexture,float2(u,v)).r;
	}

	return color;
}