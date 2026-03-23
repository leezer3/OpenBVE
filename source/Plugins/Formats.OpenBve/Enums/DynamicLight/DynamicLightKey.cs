namespace Formats.OpenBve
{
	public enum DynamicLightKey
	{
		Unknown = 0,
		CabLighting,
		Time,
		AmbientLight,
		DirectionalLight,
		CartesianLightDirection,
		LightDirection = CartesianLightDirection,
		SphericalLightDirection
	}
}
