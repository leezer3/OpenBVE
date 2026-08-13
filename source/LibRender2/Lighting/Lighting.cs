using System;
using OpenBveApi.Colors;
using OpenBveApi.Math;
using OpenBveApi.Routes;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace LibRender2.Lightings
{
	public class Lighting
	{
		private readonly BaseRenderer renderer;

		/// <summary>Whether the lighting model should re-initialize this frame</summary>
		public bool ShouldInitialize = false;

		/// <summary>The current dynamic cab brightness</summary>
		public double DynamicCabBrightness = 255;

		/// <summary>The current ambient light color</summary>
		public Color24 OptionAmbientColor = Color24.LightGrey;

		/// <summary>The current diffuse light color</summary>
		public Color24 OptionDiffuseColor = Color24.LightGrey;

		public Color24 OptionSpecularColor = Color24.Black;  // TODO
		
		/// <summary>The current ambient light position</summary>
		public Vector3 OptionLightPosition = new Vector3(0.223606797749979f, 0.86602540378444f, -0.447213595499958f);

		/// <summary>The absolute current lighting value</summary>
		/// <remarks>0.0f represents no light, 1.0f represents full brightness</remarks>
		public float OptionLightingResultingAmount;

		/// <summary>The light model parameters to be passed to openGL</summary>
		/// <remarks>BEWARE: This is NOT the default set by GL1.2</remarks>
		public Vector4 LightModel = new Vector4(0.0, 0.0, 0.0, 1.0);

		internal Lighting(BaseRenderer Renderer)
		{
			renderer = Renderer;
		}

		/// <summary>Updates the lighting model on a per frame basis</summary>
		public void Initialize()
		{
			float x = OptionAmbientColor.R + (float)OptionAmbientColor.G + OptionAmbientColor.B;
			float y = OptionDiffuseColor.R + (float)OptionDiffuseColor.G + OptionDiffuseColor.B;

			if (x < y)
			{
				x = y;
			}

			OptionLightingResultingAmount = 0.00208333333333333f * x;

			if (OptionLightingResultingAmount > 1.0f)
			{
				OptionLightingResultingAmount = 1.0f;
			}
		}

		/// <summary>Updates the lighting model on a per frame basis</summary>
		public void UpdateLighting(double Time, LightDefinition[] LightDefinitions)
		{
			//Check that we have more than one light definition & that the array is not null
			if (LightDefinitions == null || LightDefinitions.Length < 2)
			{
				return;
			}

			//Convert to absolute time of day
			//Use a while loop as it's possible to run through two days
			while (Time > 86400)
			{
				Time -= 86400;
			}

			//Run through the array
			// Find the current time block index j and next time block index k.
			// Handled wrapping around midnight using modulo. Supports > 2 time blocks.
			int j = LightDefinitions.Length - 1;
			for (int i = 0; i < LightDefinitions.Length; i++)
			{
				if (Time < LightDefinitions[i].Time)
				{
					break;
				}
				j = i;
			}
			int k = (j + 1) % LightDefinitions.Length;

			double t1 = LightDefinitions[j].Time, t2 = LightDefinitions[k].Time;
			// Fixed typo: cb2 previously assigned LightDefinitions[k].Time instead of CabBrightness
			double cb1 = LightDefinitions[j].CabBrightness, cb2 = LightDefinitions[k].CabBrightness;

			double mu;
			// Calculate the interpolation factor mu (handles wrap-around when j > k)
			if (j > k)
			{
				double duration = 86400 - t1 + t2;
				double elapsed = Time >= t1 ? Time - t1 : 86400 - t1 + Time;
				mu = elapsed / duration;
			}
			else
			{
				mu = (Time - t1) / (t2 - t1);
			}

			//Calculate the final colors and positions
			OptionDiffuseColor = Color24.CosineInterpolate(LightDefinitions[j].DiffuseColor, LightDefinitions[k].DiffuseColor, mu);
			OptionAmbientColor = Color24.CosineInterpolate(LightDefinitions[j].AmbientColor, LightDefinitions[k].AmbientColor, mu);
			OptionLightPosition = Vector3.CosineInterpolate(LightDefinitions[j].LightPosition, LightDefinitions[k].LightPosition, mu);

			//Interpolate the cab brightness value
			double mu2 = (1 - Math.Cos(mu * Math.PI)) / 2;
			DynamicCabBrightness = (cb1 * (1 - mu2) + cb2 * mu2);

			//Reinitialize the lighting model with the new information
			Initialize();

			//NOTE: This does not refresh the display lists
			//If we sit in place with extreme time acceleration (1000x) lighting for faces may appear a little inconsistant
		}
	}
}
