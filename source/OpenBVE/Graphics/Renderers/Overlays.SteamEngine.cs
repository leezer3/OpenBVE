using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenBveApi.Math;
using TrainManager.TractionModels.Steam;

namespace OpenBve.Graphics.Renderers
{
	internal partial class Overlays
	{
		internal void RenderSteamEngineOverlay()
		{
			SteamEngine engine = null;
			for (int i = 0; i < TrainManager.PlayerTrain.Cars.Length; i++)
			{
				if (TrainManager.PlayerTrain.Cars[i].TractionModel is SteamEngine)
				{
					engine = TrainManager.PlayerTrain.Cars[i].TractionModel as SteamEngine;
					break;
				}
			}

			if (engine == null || engine.ShowOverlay == false)
			{
				return;
			}

			string boilerPressure = "Boiler Pressure: " + engine.Boiler.SteamPressure.ToString("0.00") + "psi, maximum " + engine.Boiler.MaxSteamPressure + "psi";
			renderer.OpenGlString.Draw(renderer.Fonts.SmallFont, boilerPressure, new Vector2(renderer.Screen.Width - 250, 5), TextAlignment.TopLeft, Color128.White, true);
			string steamGenerationRate = "Steam Generation Rate: " + (engine.Boiler.SteamGenerationRate * 10000).ToString("0.00") + "psi per minute";
			renderer.OpenGlString.Draw(renderer.Fonts.SmallFont, steamGenerationRate, new Vector2(renderer.Screen.Width - 250, 20), TextAlignment.TopLeft, Color128.White, true);
			string steamUsageRate = "Steam Usage Rate: " + engine.CylinderChest.PressureUse.ToString("0.00") + "psi per stroke";
			renderer.OpenGlString.Draw(renderer.Fonts.SmallFont, steamUsageRate, new Vector2(renderer.Screen.Width - 250, 35), TextAlignment.TopLeft, Color128.White, true);
			string fireMass = "Fire Mass: " + engine.Boiler.Firebox.FireMass.ToString("0.00") + "kg";
			renderer.OpenGlString.Draw(renderer.Fonts.SmallFont, fireMass, new Vector2(renderer.Screen.Width - 250, 50), TextAlignment.TopLeft, Color128.White, true);
			string fireTemp = "Fire Temperature: " + engine.Boiler.Firebox.Temperature.ToString("0.00") + "°c";
			renderer.OpenGlString.Draw(renderer.Fonts.SmallFont, fireTemp, new Vector2(renderer.Screen.Width - 250, 65), TextAlignment.TopLeft, Color128.White, true);
			string fireArea = "Fire Area: " + engine.Boiler.Firebox.FireArea.ToString("0.00" + "m²");
			renderer.OpenGlString.Draw(renderer.Fonts.SmallFont, fireArea, new Vector2(renderer.Screen.Width - 250, 80), TextAlignment.TopLeft, Color128.White, true);
			string blowers = "Blowers: " + (engine.Boiler.Blowers.Active ? "true" : "false");
			renderer.OpenGlString.Draw(renderer.Fonts.SmallFont, blowers, new Vector2(renderer.Screen.Width - 250, 95), TextAlignment.TopLeft, Color128.White, true);
			string cylinderCocks = "Cylinder Cocks: " + (engine.CylinderChest.CylinderCocks.Open ? "true" : "false");
			renderer.OpenGlString.Draw(renderer.Fonts.SmallFont, cylinderCocks, new Vector2(renderer.Screen.Width - 250, 110), TextAlignment.TopLeft, Color128.White, true);
			string automaticFireman = "Automatic Fireman: " + (engine.Fireman.Active ? "true" : "false");
			renderer.OpenGlString.Draw(renderer.Fonts.SmallFont, automaticFireman, new Vector2(renderer.Screen.Width - 250, 125), TextAlignment.TopLeft, Color128.White, true);
		}
	}
}
