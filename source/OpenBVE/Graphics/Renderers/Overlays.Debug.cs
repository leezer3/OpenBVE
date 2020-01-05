using System;
using System.Drawing;
using OpenBve.BrakeSystems;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using SoundManager;

namespace OpenBve.Graphics.Renderers
{
	internal partial class Overlays
	{
		/// <summary>Renders the debug (F10) overlay</summary>
		private void RenderDebugOverlays()
		{
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			// debug
			renderer.Rectangle.Draw(null, new PointF(0.0f, 0.0f), new SizeF(renderer.Screen.Width, renderer.Screen.Height), new Color128(0.5f, 0.5f, 0.5f, 0.5f));
			// actual handles
			{
				string t = "actual: " + (TrainManager.PlayerTrain.Handles.Reverser.Actual == TrainManager.ReverserPosition.Reverse ? "B" : TrainManager.PlayerTrain.Handles.Reverser.Actual == TrainManager.ReverserPosition.Forwards ? "F" : "N");
				if (TrainManager.PlayerTrain.Handles.SingleHandle)
				{
					t += " - " + (TrainManager.PlayerTrain.Handles.EmergencyBrake.Actual ? "EMG" : TrainManager.PlayerTrain.Handles.Brake.Actual != 0 ? "B" + TrainManager.PlayerTrain.Handles.Brake.Actual.ToString(Culture) : TrainManager.PlayerTrain.Handles.HoldBrake.Actual ? "HLD" : TrainManager.PlayerTrain.Handles.Power.Actual != 0 ? "P" + TrainManager.PlayerTrain.Handles.Power.Actual.ToString(Culture) : "N");
				}
				else if (TrainManager.PlayerTrain.Handles.Brake is TrainManager.AirBrakeHandle)
				{
					t += " - " + (TrainManager.PlayerTrain.Handles.Power.Actual != 0 ? "P" + TrainManager.PlayerTrain.Handles.Power.Actual.ToString(Culture) : "N");
					t += " - " + (TrainManager.PlayerTrain.Handles.EmergencyBrake.Actual ? "EMG" : TrainManager.PlayerTrain.Handles.Brake.Actual == (int)TrainManager.AirBrakeHandleState.Service ? "SRV" : TrainManager.PlayerTrain.Handles.Brake.Actual == (int)TrainManager.AirBrakeHandleState.Lap ? "LAP" : "REL");
				}
				else
				{
					t += " - " + (TrainManager.PlayerTrain.Handles.Power.Actual != 0 ? "P" + TrainManager.PlayerTrain.Handles.Power.Actual.ToString(Culture) : "N");
					t += " - " + (TrainManager.PlayerTrain.Handles.EmergencyBrake.Actual ? "EMG" : TrainManager.PlayerTrain.Handles.Brake.Actual != 0 ? "B" + TrainManager.PlayerTrain.Handles.Brake.Actual.ToString(Culture) : TrainManager.PlayerTrain.Handles.HoldBrake.Actual ? "HLD" : "N");
				}
				if (TrainManager.PlayerTrain.Handles.HasLocoBrake)
				{
					if (TrainManager.PlayerTrain.Handles.LocoBrake is TrainManager.LocoAirBrakeHandle)
					{
						t += " - " + (TrainManager.PlayerTrain.Handles.LocoBrake.Actual == (int)TrainManager.AirBrakeHandleState.Service ? "SRV" : TrainManager.PlayerTrain.Handles.LocoBrake.Actual == (int)TrainManager.AirBrakeHandleState.Lap ? "LAP" : "REL");
					}
					else
					{
						t += " - " + (TrainManager.PlayerTrain.Handles.LocoBrake.Actual != 0 ? "L" + TrainManager.PlayerTrain.Handles.LocoBrake.Actual.ToString(Culture) : "N");
					}
				}
				renderer.OpenGlString.Draw(Fonts.SmallFont, t, new Point(2, renderer.Screen.Height - 46), TextAlignment.TopLeft, Color128.White, true);
			}
			// safety handles
			{
				string t = "safety: " + (TrainManager.PlayerTrain.Handles.Reverser.Actual == TrainManager.ReverserPosition.Reverse ? "B" : TrainManager.PlayerTrain.Handles.Reverser.Actual == TrainManager.ReverserPosition.Forwards ? "F" : "N");
				if (TrainManager.PlayerTrain.Handles.SingleHandle)
				{
					t += " - " + (TrainManager.PlayerTrain.Handles.EmergencyBrake.Safety ? "EMG" : TrainManager.PlayerTrain.Handles.Brake.Safety != 0 ? "B" + TrainManager.PlayerTrain.Handles.Brake.Safety.ToString(Culture) : TrainManager.PlayerTrain.Handles.HoldBrake.Actual ? "HLD" : TrainManager.PlayerTrain.Handles.Power.Safety != 0 ? "P" + TrainManager.PlayerTrain.Handles.Power.Safety.ToString(Culture) : "N");
				}
				else if (TrainManager.PlayerTrain.Handles.Brake is TrainManager.AirBrakeHandle)
				{
					t += " - " + (TrainManager.PlayerTrain.Handles.Power.Safety != 0 ? "P" + TrainManager.PlayerTrain.Handles.Power.Safety.ToString(Culture) : "N");
					t += " - " + (TrainManager.PlayerTrain.Handles.EmergencyBrake.Safety ? "EMG" : TrainManager.PlayerTrain.Handles.Brake.Safety == (int)TrainManager.AirBrakeHandleState.Service ? "SRV" : TrainManager.PlayerTrain.Handles.Brake.Safety == (int)TrainManager.AirBrakeHandleState.Lap ? "LAP" : "REL");
				}
				else
				{
					t += " - " + (TrainManager.PlayerTrain.Handles.Power.Safety != 0 ? "P" + TrainManager.PlayerTrain.Handles.Power.Safety.ToString(Culture) : "N");
					t += " - " + (TrainManager.PlayerTrain.Handles.EmergencyBrake.Safety ? "EMG" : TrainManager.PlayerTrain.Handles.Brake.Safety != 0 ? "B" + TrainManager.PlayerTrain.Handles.Brake.Safety.ToString(Culture) : TrainManager.PlayerTrain.Handles.HoldBrake.Actual ? "HLD" : "N");
				}

				if (TrainManager.PlayerTrain.Handles.HasLocoBrake)
				{
					if (TrainManager.PlayerTrain.Handles.LocoBrake is TrainManager.LocoAirBrakeHandle)
					{
						t += " - " + (TrainManager.PlayerTrain.Handles.LocoBrake.Actual == (int)TrainManager.AirBrakeHandleState.Service ? "SRV" : TrainManager.PlayerTrain.Handles.LocoBrake.Actual == (int)TrainManager.AirBrakeHandleState.Lap ? "LAP" : "REL");
					}
					else
					{
						t += " - " + (TrainManager.PlayerTrain.Handles.LocoBrake.Actual != 0 ? "L" + TrainManager.PlayerTrain.Handles.LocoBrake.Actual.ToString(Culture) : "N");
					}
				}
				renderer.OpenGlString.Draw(Fonts.SmallFont, t, new Point(2, renderer.Screen.Height - 32), TextAlignment.TopLeft, Color128.White, true);
			}
			// driver handles
			{
				string t = "driver: " + (TrainManager.PlayerTrain.Handles.Reverser.Driver == TrainManager.ReverserPosition.Reverse ? "B" : TrainManager.PlayerTrain.Handles.Reverser.Driver == TrainManager.ReverserPosition.Forwards ? "F" : "N");
				if (TrainManager.PlayerTrain.Handles.SingleHandle)
				{
					t += " - " + (TrainManager.PlayerTrain.Handles.EmergencyBrake.Driver ? "EMG" : TrainManager.PlayerTrain.Handles.Brake.Driver != 0 ? "B" + TrainManager.PlayerTrain.Handles.Brake.Driver.ToString(Culture) : TrainManager.PlayerTrain.Handles.HoldBrake.Driver ? "HLD" : TrainManager.PlayerTrain.Handles.Power.Driver != 0 ? "P" + TrainManager.PlayerTrain.Handles.Power.Driver.ToString(Culture) : "N");
				}
				else if (TrainManager.PlayerTrain.Handles.Brake is TrainManager.AirBrakeHandle)
				{
					t += " - " + (TrainManager.PlayerTrain.Handles.Power.Driver != 0 ? "P" + TrainManager.PlayerTrain.Handles.Power.Driver.ToString(Culture) : "N");
					t += " - " + (TrainManager.PlayerTrain.Handles.EmergencyBrake.Driver ? "EMG" : TrainManager.PlayerTrain.Handles.Brake.Driver == (int)TrainManager.AirBrakeHandleState.Service ? "SRV" : TrainManager.PlayerTrain.Handles.Brake.Driver == (int)TrainManager.AirBrakeHandleState.Lap ? "LAP" : "REL");
				}
				else
				{
					t += " - " + (TrainManager.PlayerTrain.Handles.Power.Driver != 0 ? "P" + TrainManager.PlayerTrain.Handles.Power.Driver.ToString(Culture) : "N");
					t += " - " + (TrainManager.PlayerTrain.Handles.EmergencyBrake.Driver ? "EMG" : TrainManager.PlayerTrain.Handles.Brake.Driver != 0 ? "B" + TrainManager.PlayerTrain.Handles.Brake.Driver.ToString(Culture) : TrainManager.PlayerTrain.Handles.HoldBrake.Driver ? "HLD" : "N");
				}
				if (TrainManager.PlayerTrain.Handles.HasLocoBrake)
				{
					if (TrainManager.PlayerTrain.Handles.LocoBrake is TrainManager.LocoAirBrakeHandle)
					{
						t += " - " + (TrainManager.PlayerTrain.Handles.LocoBrake.Actual == (int)TrainManager.AirBrakeHandleState.Service ? "SRV" : TrainManager.PlayerTrain.Handles.LocoBrake.Actual == (int)TrainManager.AirBrakeHandleState.Lap ? "LAP" : "REL");
					}
					else
					{
						t += " - " + (TrainManager.PlayerTrain.Handles.LocoBrake.Actual != 0 ? "L" + TrainManager.PlayerTrain.Handles.LocoBrake.Actual.ToString(Culture) : "N");
					}
				}
				renderer.OpenGlString.Draw(Fonts.SmallFont, t, new Point(2, renderer.Screen.Height - 18), TextAlignment.TopLeft, Color128.White, true);
			}
			// debug information
			int texturesLoaded = renderer.TextureManager.GetNumberOfLoadedTextures();
			int texturesRegistered = renderer.TextureManager.GetNumberOfRegisteredTextures();
			int soundBuffersRegistered = Program.Sounds.GetNumberOfRegisteredBuffers();
			int soundBuffersLoaded = Program.Sounds.GetNumberOfLoadedBuffers();
			int soundSourcesRegistered = Program.Sounds.GetNumberOfRegisteredSources();
			int soundSourcesPlaying = Program.Sounds.GetNumberOfPlayingSources();
			int car = 0;
			for (int i = 0; i < TrainManager.PlayerTrain.Cars.Length; i++)
			{
				if (TrainManager.PlayerTrain.Cars[i].Specs.IsMotorCar)
				{
					car = i;
					break;
				}
			}
			double mass = 0.0;
			for (int i = 0; i < TrainManager.PlayerTrain.Cars.Length; i++)
			{
				mass += TrainManager.PlayerTrain.Cars[i].Specs.MassCurrent;
			}
			int hours = (int)Program.CurrentRoute.SecondsSinceMidnight / 3600,
				remainder = (int)Program.CurrentRoute.SecondsSinceMidnight % 3600,
				minutes = remainder / 60,
				seconds = remainder % 60;
			string[] Lines = new string[] {
				"=system",
				"fps: " + Program.Renderer.FrameRate.ToString("0.0", Culture) + (MainLoop.LimitFramerate ? " (low cpu)" : ""),
				"time:" + hours.ToString("00") +  ":" + minutes.ToString("00") + ":" + seconds.ToString("00"),
				"score: " + Game.CurrentScore.CurrentValue.ToString(Culture),
				"",
				"=train",
				"speed: " + (Math.Abs(TrainManager.PlayerTrain.CurrentSpeed) * 3.6).ToString("0.00", Culture) + " km/h",
				"power (car " + car.ToString(Culture) +  "): " + (TrainManager.PlayerTrain.Cars[car].Specs.CurrentAccelerationOutput < 0.0 ? TrainManager.PlayerTrain.Cars[car].Specs.CurrentAccelerationOutput * Math.Sign(TrainManager.PlayerTrain.Cars[car].CurrentSpeed) : TrainManager.PlayerTrain.Cars[car].Specs.CurrentAccelerationOutput * (double)TrainManager.PlayerTrain.Handles.Reverser.Actual).ToString("0.0000", Culture) + " m/s²",
				"acceleration: " + TrainManager.PlayerTrain.Specs.CurrentAverageAcceleration.ToString("0.0000", Culture) + " m/s²",
				"position: " + TrainManager.PlayerTrain.FrontCarTrackPosition().ToString("0.00", Culture) + " m",
				"elevation: " + (Program.CurrentRoute.Atmosphere.InitialElevation + TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].FrontAxle.Follower.WorldPosition.Y).ToString("0.00", Culture) + " m",
				"temperature: " + (TrainManager.PlayerTrain.Specs.CurrentAirTemperature - 273.15).ToString("0.00", Culture) + " °C",
				"air pressure: " + (0.001 * TrainManager.PlayerTrain.Specs.CurrentAirPressure).ToString("0.00", Culture) + " kPa",
				"air density: " + TrainManager.PlayerTrain.Specs.CurrentAirDensity.ToString("0.0000", Culture) + " kg/m³",
				"speed of sound: " + (Program.CurrentRoute.Atmosphere.GetSpeedOfSound(TrainManager.PlayerTrain.Specs.CurrentAirDensity) * 3.6).ToString("0.00", Culture) + " km/h",
				"passenger ratio: " + TrainManager.PlayerTrain.Passengers.PassengerRatio.ToString("0.00"),
				"total mass: " + mass.ToString("0.00", Culture) + " kg",
				"",
				"=route",
				"track limit: " + (TrainManager.PlayerTrain.CurrentRouteLimit == double.PositiveInfinity ? "unlimited" : ((TrainManager.PlayerTrain.CurrentRouteLimit * 3.6).ToString("0.0", Culture) + " km/h")),
				"signal limit: " + (TrainManager.PlayerTrain.CurrentSectionLimit == double.PositiveInfinity ? "unlimited" : ((TrainManager.PlayerTrain.CurrentSectionLimit * 3.6).ToString("0.0", Culture) + " km/h")),
				"total static objects: " + (Program.Renderer.StaticObjectStates.Count + Program.Renderer.DynamicObjectStates.Count).ToString(Culture),
				"total static GL_TRIANGLES: " + Program.Renderer.InfoTotalTriangles.ToString(Culture),
				"total static GL_TRIANGLE_STRIP: " + Program.Renderer.InfoTotalTriangleStrip.ToString(Culture),
				"total static GL_QUADS: " + Program.Renderer.InfoTotalQuads.ToString(Culture),
				"total static GL_QUAD_STRIP: " + Program.Renderer.InfoTotalQuadStrip.ToString(Culture),
				"total static GL_POLYGON: " + Program.Renderer.InfoTotalPolygon.ToString(Culture),
				"total animated objects: " + ObjectManager.AnimatedWorldObjectsUsed.ToString(Culture),
				"",
				"=renderer",
				"renderer type: " + (Interface.CurrentOptions.IsUseNewRenderer ? "openGL 3.0 (new)" : "openGL 1.2 (old)"),
				"opaque faces: " + Program.Renderer.VisibleObjects.OpaqueFaces.Count.ToString(Culture),
				"alpha faces: " + Program.Renderer.VisibleObjects.AlphaFaces.Count.ToString(Culture),
				"overlay opaque faces: " + Program.Renderer.VisibleObjects.OverlayOpaqueFaces.Count.ToString(Culture),
				"overlay alpha faces: " + Program.Renderer.VisibleObjects.OverlayAlphaFaces.Count.ToString(Culture),
				"textures loaded: " + texturesLoaded.ToString(Culture) + " / " + texturesRegistered.ToString(Culture),
				"",
				"=camera",
				"position: " + World.CameraTrackFollower.TrackPosition.ToString("0.00", Culture) + " m",
				"curve radius: " + World.CameraTrackFollower.CurveRadius.ToString("0.00", Culture) + " m",
				"curve cant: " + (1000.0 * Math.Abs(World.CameraTrackFollower.CurveCant)).ToString("0.00", Culture) + " mm" + (World.CameraTrackFollower.CurveCant < 0.0 ? " (left)" : World.CameraTrackFollower.CurveCant > 0.0 ? " (right)" : ""),
				"pitch: " + World.CameraTrackFollower.Pitch.ToString("0.00", Culture),
				"",
				"=sound",
				"sound buffers: " + soundBuffersLoaded.ToString(Culture) + " / " + soundBuffersRegistered.ToString(Culture),
				"sound sources: " + soundSourcesPlaying.ToString(Culture) + " / " + soundSourcesRegistered.ToString(Culture),
				(Interface.CurrentOptions.SoundModel == SoundModels.Inverse ? "log clamp factor: " + Program.Sounds.LogClampFactor.ToString("0.00") : "outer radius factor: " + Program.Sounds.OuterRadiusFactor.ToString("0.00", Culture)),
				"",
				"=debug",
				"train plugin status: " + (TrainManager.PlayerTrain.Plugin != null ? (TrainManager.PlayerTrain.Plugin.PluginValid ? "ok" : "error") : "n/a"),
				"train plugin message: " + (TrainManager.PlayerTrain.Plugin != null ? (TrainManager.PlayerTrain.Plugin.PluginMessage ?? "n/a") : "n/a"),
				Game.InfoDebugString ?? ""
			};
			double x = 4.0;
			double y = 4.0;
			for (int i = 0; i < Lines.Length; i++)
			{
				if (Lines[i].Length != 0)
				{
					if (Lines[i][0] == '=')
					{
						string text = Lines[i].Substring(1);
						Size size = Fonts.SmallFont.MeasureString(text);
						renderer.Rectangle.Draw(null, new PointF((float)x, (float)y), new SizeF(size.Width + 6.0f, size.Height + 2.0f), new Color128(0.35f, 0.65f, 0.90f, 0.8f));
						renderer.OpenGlString.Draw(Fonts.SmallFont, text, new Point((int)x + 3, (int)y), TextAlignment.TopLeft, Color128.White);
					}
					else
					{
						renderer.OpenGlString.Draw(Fonts.SmallFont, Lines[i], new Point((int)x, (int)y), TextAlignment.TopLeft, Color128.White, true);
					}
					y += 14.0;
				}
				else if (y >= renderer.Screen.Height - 240.0)
				{
					x += 280.0;
					y = 4.0;
				}
				else
				{
					y += 14.0;
				}
			}
		}

		private void RenderATSDebugOverlay()
		{
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			// debug
			renderer.Rectangle.Draw(null, new PointF(0.0f, 0.0f), new SizeF(renderer.Screen.Width, renderer.Screen.Height), new Color128(0.5f, 0.5f, 0.5f, 0.5f));
			string[] Lines;
			if (TrainManager.PlayerTrain.Plugin.Panel.Length > 0)
			{
				Lines = new string[TrainManager.PlayerTrain.Plugin.Panel.Length + 2];
				Lines[0] = "=ATS Plugin Variables";
				Lines[1] = "";
				for (int i = 2; i < TrainManager.PlayerTrain.Plugin.Panel.Length + 2; i++)
				{
					Lines[i] = (i - 2).ToString("000") + " : " + TrainManager.PlayerTrain.Plugin.Panel[i - 2];
				}
			}
			else
			{
				Lines = new string[3];
				Lines[0] = "=ATS Plugin Variables";
				Lines[1] = "";
				Lines[2] = "No ATS plugin variables set.";
			}
			double x = 4.0;
			double y = 4.0;
			for (int i = 0; i < Lines.Length; i++)
			{
				if (Lines[i].Length != 0)
				{
					if (Lines[i][0] == '=')
					{
						string text = Lines[i].Substring(1);
						Size size = Fonts.SmallFont.MeasureString(text);
						renderer.Rectangle.Draw(null, new PointF((float)x, (float)y), new SizeF(size.Width + 6.0f, size.Height + 2.0f), new Color128(0.35f, 0.65f, 0.9f, 0.8f));
						renderer.OpenGlString.Draw(Fonts.SmallFont, text, new Point((int)x + 3, (int)y), TextAlignment.TopLeft, Color128.White);
					}
					else
					{
						renderer.OpenGlString.Draw(Fonts.SmallFont, Lines[i], new Point((int)x, (int)y), TextAlignment.TopLeft, Color128.White, true);
					}
					y += 14.0;
					if (y > renderer.Screen.Height - 20.0)
					{
						y = 32.0;
						x += 80.0;
					}
				}
				else
				{
					y += 14.0;
				}
			}
		}

		/// <summary>Renders the brake system debug overlay</summary>
		private void RenderBrakeSystemDebug()
		{
			double oy = 64.0, y = oy, h = 16.0;
			bool[] heading = new bool[6];
			for (int i = 0; i < TrainManager.PlayerTrain.Cars.Length; i++)
			{
				double x = 96.0, w = 128.0;
				// brake pipe
				if (TrainManager.PlayerTrain.Cars[i].CarBrake is AutomaticAirBrake | TrainManager.PlayerTrain.Cars[i].CarBrake is ElectromagneticStraightAirBrake)
				{
					if (!heading[0])
					{
						renderer.OpenGlString.Draw(Fonts.SmallFont, "Brake pipe", new Point((int)x, (int)(oy - 16)), TextAlignment.TopLeft, Color128.White, true);
						heading[0] = true;
					}
					renderer.Rectangle.Draw(null, new PointF((float)x, (float)y), new SizeF((float)w, (float)h), new Color128(0.0f, 0.0f, 0.0f, 1.0f));
					double p = TrainManager.PlayerTrain.Cars[i].CarBrake.brakePipe.CurrentPressure;
					double r = p / TrainManager.PlayerTrain.Cars[i].CarBrake.brakePipe.NormalPressure;
					renderer.Rectangle.Draw(null, new PointF((float)x, (float)y), new SizeF((float)(r * w), (float)h), new Color128(1.0f, 1.0f, 0.0f, 1.0f));
				}
				x += w + 8.0;
				// auxillary reservoir
				if (TrainManager.PlayerTrain.Cars[i].CarBrake is AutomaticAirBrake | TrainManager.PlayerTrain.Cars[i].CarBrake is ElectromagneticStraightAirBrake)
				{
					if (!heading[1])
					{
						renderer.OpenGlString.Draw(Fonts.SmallFont, "Auxillary reservoir", new Point((int)x, (int)(oy - 16)), TextAlignment.TopLeft, Color128.White, true);
						heading[1] = true;
					}
					renderer.Rectangle.Draw(null, new PointF((float)x, (float)y), new SizeF((float)w, (float)h), new Color128(0.0f, 0.0f, 0.0f, 1.0f));
					double p = TrainManager.PlayerTrain.Cars[i].CarBrake.auxiliaryReservoir.CurrentPressure;
					double r = p / TrainManager.PlayerTrain.Cars[i].CarBrake.auxiliaryReservoir.MaximumPressure;
					renderer.Rectangle.Draw(null, new PointF((float)x, (float)y), new SizeF((float)(r * w), (float)h), new Color128(0.5f, 0.5f, 0.5f, 1.0f));
				}
				x += w + 8.0;
				// brake cylinder
				{
					if (!heading[2])
					{
						renderer.OpenGlString.Draw(Fonts.SmallFont, "Brake cylinder", new Point((int)x, (int)(oy - 16)), TextAlignment.TopLeft, Color128.White, true);
						heading[2] = true;
					}
					renderer.Rectangle.Draw(null, new PointF((float)x, (float)y), new SizeF((float)w, (float)h), new Color128(0.0f, 0.0f, 0.0f, 1.0f));
					double p = TrainManager.PlayerTrain.Cars[i].CarBrake.brakeCylinder.CurrentPressure;
					double r = p / TrainManager.PlayerTrain.Cars[i].CarBrake.brakeCylinder.EmergencyMaximumPressure;
					renderer.Rectangle.Draw(null, new PointF((float)x, (float)y), new SizeF((float)(r * w), (float)h), new Color128(0.75f, 0.5f, 0.25f, 1.0f));
				}
				x += w + 8.0;
				// main reservoir
				if (TrainManager.PlayerTrain.Cars[i].CarBrake.brakeType == BrakeType.Main)
				{
					if (!heading[3])
					{
						renderer.OpenGlString.Draw(Fonts.SmallFont, "Main reservoir", new Point((int)x, (int)(oy - 16)), TextAlignment.TopLeft, Color128.White, true);
						heading[3] = true;
					}
					renderer.Rectangle.Draw(null, new PointF((float)x, (float)y), new SizeF((float)w, (float)h), new Color128(0.0f, 0.0f, 0.0f, 1.0f));
					double p = TrainManager.PlayerTrain.Cars[i].CarBrake.mainReservoir.CurrentPressure;
					double r = p / TrainManager.PlayerTrain.Cars[i].CarBrake.mainReservoir.MaximumPressure;
					renderer.Rectangle.Draw(null, new PointF((float)x, (float)y), new SizeF((float)(r * w), (float)h), new Color128(1.0f, 0.0f, 0.0f, 1.0f));
				}
				x += w + 8.0;
				// equalizing reservoir
				if (TrainManager.PlayerTrain.Cars[i].CarBrake.brakeType == BrakeType.Main)
				{
					if (!heading[4])
					{
						renderer.OpenGlString.Draw(Fonts.SmallFont, "Equalizing reservoir", new Point((int)x, (int)(oy - 16)), TextAlignment.TopLeft, Color128.White, true);
						heading[4] = true;
					}
					renderer.Rectangle.Draw(null, new PointF((float)x, (float)y), new SizeF((float)w, (float)h), new Color128(0.0f, 0.0f, 0.0f, 1.0f));
					double p = TrainManager.PlayerTrain.Cars[i].CarBrake.equalizingReservoir.CurrentPressure;
					double r = p / TrainManager.PlayerTrain.Cars[i].CarBrake.equalizingReservoir.NormalPressure;
					renderer.Rectangle.Draw(null, new PointF((float)x, (float)y), new SizeF((float)(r * w), (float)h), new Color128(0.0f, 0.75f, 0.0f, 1.0f));
				}
				x += w + 8.0;
				// straight air pipe
				if (TrainManager.PlayerTrain.Cars[i].CarBrake is ElectromagneticStraightAirBrake & TrainManager.PlayerTrain.Cars[i].CarBrake.brakeType == BrakeType.Main)
				{
					if (!heading[5])
					{
						renderer.OpenGlString.Draw(Fonts.SmallFont, "Straight air pipe", new Point((int)x, (int)(oy - 16)), TextAlignment.TopLeft, Color128.White, true);
						heading[5] = true;
					}
					renderer.Rectangle.Draw(null, new PointF((float)x, (float)y), new SizeF((float)w, (float)h), new Color128(0.0f, 0.0f, 0.0f, 1.0f));
					double p = TrainManager.PlayerTrain.Cars[i].CarBrake.straightAirPipe.CurrentPressure;
					double r = p / TrainManager.PlayerTrain.Cars[i].CarBrake.brakeCylinder.EmergencyMaximumPressure;
					renderer.Rectangle.Draw(null, new PointF((float)x, (float)y), new SizeF((float)(r * w), (float)h), new Color128(0.0f, 0.75f, 1.0f, 1.0f));
				} //x += w + 8.0;
				y += h + 8.0;
			}
		}
	}
}
