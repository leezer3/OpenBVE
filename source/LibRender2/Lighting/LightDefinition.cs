using OpenBveApi.Colors;
using OpenBveApi.Math;

namespace LibRender2.Lightings
{
	public struct LightDefinition
	{
		/// <summary>The ambient lighting color</summary>
		public Color24 AmbientColor;
		/// <summary>The color of the light emitted by the sun (Directional light)</summary>
		public Color24 DiffuseColor;
		/// <summary>The position of the sun in the sky (Directional light)</summary>
		public Vector3 LightPosition;
		/// <summary>The time in seconds since midnight</summary>
		public int Time;
		/// <summary>The cab brightness value to be applied</summary>
		public double CabBrightness;

		public LightDefinition(Color24 ambientColor, Color24 diffuseColor, Vector3 lightPosition, int time, double cab)
		{
			AmbientColor = ambientColor;
			DiffuseColor = diffuseColor;
			LightPosition = lightPosition;
			Time = time;
			CabBrightness = cab;
		}
	}
}
