//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2022, Christopher Lees, The OpenBVE Project
//
//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met:
//
//1. Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
//ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenBveApi.Math;

namespace TrainManager.TractionModels.Steam
{
	public partial class SteamEngine
	{
		public override void RenderOverlay(ref double yOffset)
		{
			if (ShowOverlay == false)
			{
				return;
			}
			string engineCar = "Steam Engine: Car " + Car.Index;
			TrainManagerBase.Renderer.OpenGlString.Draw(TrainManagerBase.Renderer.Fonts.SmallFont, engineCar, new Vector2(TrainManagerBase.Renderer.Screen.Width - 250, 5 + yOffset), TextAlignment.TopLeft, Color128.White, true);
			string boilerPressure = "Boiler Pressure: " + Boiler.SteamPressure.ToString("0.00") + "psi, max " + Boiler.MaxSteamPressure + "psi";
			TrainManagerBase.Renderer.OpenGlString.Draw(TrainManagerBase.Renderer.Fonts.SmallFont, boilerPressure, new Vector2(TrainManagerBase.Renderer.Screen.Width - 250, 20 + yOffset), TextAlignment.TopLeft, Color128.White, true);
			string steamGenerationRate = "Steam Generation Rate: " + (Boiler.SteamGenerationRate * 10000).ToString("0.00") + "psi per minute";
			TrainManagerBase.Renderer.OpenGlString.Draw(TrainManagerBase.Renderer.Fonts.SmallFont, steamGenerationRate, new Vector2(TrainManagerBase.Renderer.Screen.Width - 250, 35 + yOffset), TextAlignment.TopLeft, Color128.White, true);
			string steamUsageRate = "Steam Usage Rate: " + CylinderChest.PressureUse.ToString("0.00") + "psi per stroke";
			TrainManagerBase.Renderer.OpenGlString.Draw(TrainManagerBase.Renderer.Fonts.SmallFont, steamUsageRate, new Vector2(TrainManagerBase.Renderer.Screen.Width - 250, 50 + yOffset), TextAlignment.TopLeft, Color128.White, true);
			string fireMass = "Fire Mass: " + Boiler.Firebox.FireMass.ToString("0.00") + "kg";
			TrainManagerBase.Renderer.OpenGlString.Draw(TrainManagerBase.Renderer.Fonts.SmallFont, fireMass, new Vector2(TrainManagerBase.Renderer.Screen.Width - 250, 65 + yOffset), TextAlignment.TopLeft, Color128.White, true);
			string fireTemp = "Fire Temperature: " + Boiler.Firebox.Temperature.ToString("0.00") + "°c";
			TrainManagerBase.Renderer.OpenGlString.Draw(TrainManagerBase.Renderer.Fonts.SmallFont, fireTemp, new Vector2(TrainManagerBase.Renderer.Screen.Width - 250, 80 + yOffset), TextAlignment.TopLeft, Color128.White, true);
			string fireArea = "Fire Area: " + Boiler.Firebox.FireArea.ToString("0.00" + "m²");
			TrainManagerBase.Renderer.OpenGlString.Draw(TrainManagerBase.Renderer.Fonts.SmallFont, fireArea, new Vector2(TrainManagerBase.Renderer.Screen.Width - 250, 95 + yOffset), TextAlignment.TopLeft, Color128.White, true);
			string blowers = "Blowers: " + (Boiler.Blowers.Active ? "true" : "false");
			TrainManagerBase.Renderer.OpenGlString.Draw(TrainManagerBase.Renderer.Fonts.SmallFont, blowers, new Vector2(TrainManagerBase.Renderer.Screen.Width - 250, 110 + yOffset), TextAlignment.TopLeft, Color128.White, true);
			string cylinderCocks = "Cylinder Cocks: " + (CylinderChest.CylinderCocks.Open ? "true" : "false");
			TrainManagerBase.Renderer.OpenGlString.Draw(TrainManagerBase.Renderer.Fonts.SmallFont, cylinderCocks, new Vector2(TrainManagerBase.Renderer.Screen.Width - 250, 125 + yOffset), TextAlignment.TopLeft, Color128.White, true);
			string automaticFireman = "Automatic Fireman: " + (Fireman.Active ? "true" : "false");
			TrainManagerBase.Renderer.OpenGlString.Draw(TrainManagerBase.Renderer.Fonts.SmallFont, automaticFireman, new Vector2(TrainManagerBase.Renderer.Screen.Width - 250, 140 + yOffset), TextAlignment.TopLeft, Color128.White, true);
			// Add the final offset to our return value, as this will now be passed back to the next set of drawn overlays
			yOffset += 155;
		}
	}
}
