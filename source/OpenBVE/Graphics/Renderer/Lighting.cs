using LibRender;
using OpenBveApi.Colors;
using OpenBveApi.Math;
using OpenTK.Graphics.OpenGL;

namespace OpenBve
{
	internal static partial class Renderer
	{
		

		
		/// <summary>Whether dynamic lighting is currently enabled</summary>
		internal static bool DynamicLighting = false;
		/// <summary>The current dynamic cab brightness</summary>
		internal static double DynamicCabBrightness = 255;
		/// <summary>The list of dynamic light definitions</summary>
		internal static LightDefinition[] LightDefinitions;

		/// <summary>Updates the lighting model on a per frame basis</summary>
		internal static void UpdateLighting()
		{
			//Check that we have more than one light definition & that the array is not null
			if (DynamicLighting == false || LightDefinitions == null || LightDefinitions.Length < 2)
			{
				return;
			}
			var Time = Game.SecondsSinceMidnight;
			//Convert to absolute time of day
			//Use a while loop as it's possible to run through two days
			while (Time > 86400)
			{
				Time -= 86400;
			}
			//Run through the array
			int j = 0;
			for (int i = j; i < LightDefinitions.Length; i++)
			{
				
				if (Time < LightDefinitions[i].Time)
				{
					break;
				}
				j = i;

			}

			//We now know that our light definition is between the values defined in j and j + 1 (Or 0 if this is the end of the array)
			int k;

			if (j == 0)
			{
				//Our NEW light is to be the first entry in the array
				//This means the OLD light is the last entry, but j and k do not need reversing
				k = 0;
				j = LightDefinitions.Length - 1;
			}
			else if (j == LightDefinitions.Length - 1)
			{
				//We are wrapping around to the end of the array
				//Reverse j and k, as we have not yet passed the time for the first array entry
				j = 0;
				k = LightDefinitions.Length - 1;
			}
			else
			{
				//Somewhere in the middle, so the NEW light is simply one greater
				k = j + 1;
			}
			int t1 = LightDefinitions[j].Time, t2 = LightDefinitions[k].Time;
			
			double cb1 = LightDefinitions[j].CabBrightness, cb2 = LightDefinitions[k].Time;
			//Calculate, inverting if necessary

			//Ensure we're not about to divide by zero
			if (t2 == 0)
			{
				t2 = 1;
			}
			if (t1 == 0)
			{
				t1 = 1;
			}
			//Calculate the percentage
			double mu;
			if (k == LightDefinitions.Length - 1)
			{
				//Wrapping around
				mu = (86400 - Time + t1) / (86400 - t2 + t1);
				
			}
			else
			{
				mu = (Time - t1) / (t2 - t1);
			}
			//Calculate the final colors and positions
			LibRender.Renderer.OptionDiffuseColor = Color24.CosineInterpolate(LightDefinitions[j].DiffuseColor, LightDefinitions[k].DiffuseColor, mu);
			LibRender.Renderer.OptionAmbientColor = Color24.CosineInterpolate(LightDefinitions[j].AmbientColor, LightDefinitions[k].AmbientColor, mu);
			LibRender.Renderer.OptionLightPosition = Vector3.CosineInterpolate(LightDefinitions[j].LightPosition,LightDefinitions[k].LightPosition, mu);

			//Interpolate the cab brightness value
			var mu2 = (1 - System.Math.Cos(mu * System.Math.PI)) / 2;
			DynamicCabBrightness = (cb1 * (1 - mu2) + cb2 * mu2);
			//Reinitialize the lighting model with the new information
			LibRender.Renderer.InitializeLighting();
			//NOTE: This does not refresh the display lists
			//If we sit in place with extreme time acceleration (1000x) lighting for faces may appear a little inconsistant
		}
	}
}
