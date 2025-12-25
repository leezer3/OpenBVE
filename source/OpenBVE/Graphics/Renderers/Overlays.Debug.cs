using System;
using TrainManager.BrakeSystems;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenBveApi.Math;
using OpenBveApi.Routes;
using SoundManager;
using TrainManager.Handles;
using TrainManager.Motor;

namespace OpenBve.Graphics.Renderers
{
	internal partial class Overlays
	{
		/// <summary>Renders the debug (F10) overlay</summary>
		private void RenderDebugOverlays()
		{
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			// debug
			renderer.Rectangle.Draw(null, Vector2.Null, new Vector2(renderer.Screen.Width, renderer.Screen.Height), Color128.SemiTransparentGrey);
			// actual handles
			string t = "actual: " + (TrainManager.PlayerTrain.Handles.Reverser.Actual == ReverserPosition.Reverse ? "B" : TrainManager.PlayerTrain.Handles.Reverser.Actual == ReverserPosition.Forwards ? "F" : "N");
			if (TrainManager.PlayerTrain.Handles.HandleType == HandleType.SingleHandle)
			{
				t += " - " + (TrainManager.PlayerTrain.Handles.EmergencyBrake.Actual ? "EMG" : TrainManager.PlayerTrain.Handles.Brake.Actual != 0 ? "B" + TrainManager.PlayerTrain.Handles.Brake.Actual.ToString(Culture) : TrainManager.PlayerTrain.Handles.HoldBrake.Actual ? "HLD" : TrainManager.PlayerTrain.Handles.Power.Actual != 0 ? "P" + TrainManager.PlayerTrain.Handles.Power.Actual.ToString(Culture) : "N");
			}
			else if (TrainManager.PlayerTrain.Handles.Brake is AirBrakeHandle)
			{
				t += " - " + (TrainManager.PlayerTrain.Handles.Power.Actual != 0 ? "P" + TrainManager.PlayerTrain.Handles.Power.Actual.ToString(Culture) : "N");
				t += " - " + (TrainManager.PlayerTrain.Handles.EmergencyBrake.Actual ? "EMG" : TrainManager.PlayerTrain.Handles.Brake.Actual == (int)AirBrakeHandleState.Service ? "SRV" : TrainManager.PlayerTrain.Handles.Brake.Actual == (int)AirBrakeHandleState.Lap ? "LAP" : "REL");
			}
			else
			{
				t += " - " + (TrainManager.PlayerTrain.Handles.Power.Actual != 0 ? "P" + TrainManager.PlayerTrain.Handles.Power.Actual.ToString(Culture) : "N");
				t += " - " + (TrainManager.PlayerTrain.Handles.EmergencyBrake.Actual ? "EMG" : TrainManager.PlayerTrain.Handles.Brake.Actual != 0 ? "B" + TrainManager.PlayerTrain.Handles.Brake.Actual.ToString(Culture) : TrainManager.PlayerTrain.Handles.HoldBrake.Actual ? "HLD" : "N");
			}
			if (TrainManager.PlayerTrain.Handles.HasLocoBrake)
			{
				if (TrainManager.PlayerTrain.Handles.LocoBrake is LocoAirBrakeHandle)
				{
					t += " - " + (TrainManager.PlayerTrain.Handles.LocoBrake.Actual == (int)AirBrakeHandleState.Service ? "SRV" : TrainManager.PlayerTrain.Handles.LocoBrake.Actual == (int)AirBrakeHandleState.Lap ? "LAP" : "REL");
				}
				else
				{
					t += " - " + (TrainManager.PlayerTrain.Handles.LocoBrake.Actual != 0 ? "L" + TrainManager.PlayerTrain.Handles.LocoBrake.Actual.ToString(Culture) : "N");
				}
			}
			renderer.OpenGlString.Draw(renderer.Fonts.SmallFont, t, new Vector2(2, renderer.Screen.Height - 46), TextAlignment.TopLeft, Color128.White, true);
			// safety handles
			t = "safety: ";
			if (TrainManager.PlayerTrain.CurrentDirection == TrackDirection.Reverse)
			{
				t += (TrainManager.PlayerTrain.Handles.Reverser.Actual == ReverserPosition.Reverse ? "F" : TrainManager.PlayerTrain.Handles.Reverser.Actual == ReverserPosition.Forwards ? "B" : "N");
			}
			else
			{
				t += (TrainManager.PlayerTrain.Handles.Reverser.Actual == ReverserPosition.Reverse ? "B" : TrainManager.PlayerTrain.Handles.Reverser.Actual == ReverserPosition.Forwards ? "F" : "N");
			}
			if (TrainManager.PlayerTrain.Handles.HandleType == HandleType.SingleHandle)
			{
				t += " - " + (TrainManager.PlayerTrain.Handles.EmergencyBrake.Safety ? "EMG" : TrainManager.PlayerTrain.Handles.Brake.Safety != 0 ? "B" + TrainManager.PlayerTrain.Handles.Brake.Safety.ToString(Culture) : TrainManager.PlayerTrain.Handles.HoldBrake.Actual ? "HLD" : TrainManager.PlayerTrain.Handles.Power.Safety != 0 ? "P" + TrainManager.PlayerTrain.Handles.Power.Safety.ToString(Culture) : "N");
			}
			else if (TrainManager.PlayerTrain.Handles.Brake is AirBrakeHandle)
			{
				t += " - " + (TrainManager.PlayerTrain.Handles.Power.Safety != 0 ? "P" + TrainManager.PlayerTrain.Handles.Power.Safety.ToString(Culture) : "N");
				t += " - " + (TrainManager.PlayerTrain.Handles.EmergencyBrake.Safety ? "EMG" : TrainManager.PlayerTrain.Handles.Brake.Safety == (int)AirBrakeHandleState.Service ? "SRV" : TrainManager.PlayerTrain.Handles.Brake.Safety == (int)AirBrakeHandleState.Lap ? "LAP" : "REL");
			}
			else
			{
				t += " - " + (TrainManager.PlayerTrain.Handles.Power.Safety != 0 ? "P" + TrainManager.PlayerTrain.Handles.Power.Safety.ToString(Culture) : "N");
				t += " - " + (TrainManager.PlayerTrain.Handles.EmergencyBrake.Safety ? "EMG" : TrainManager.PlayerTrain.Handles.Brake.Safety != 0 ? "B" + TrainManager.PlayerTrain.Handles.Brake.Safety.ToString(Culture) : TrainManager.PlayerTrain.Handles.HoldBrake.Actual ? "HLD" : "N");
			}

			if (TrainManager.PlayerTrain.Handles.HasLocoBrake)
			{
				if (TrainManager.PlayerTrain.Handles.LocoBrake is LocoAirBrakeHandle)
				{
					t += " - " + (TrainManager.PlayerTrain.Handles.LocoBrake.Actual == (int)AirBrakeHandleState.Service ? "SRV" : TrainManager.PlayerTrain.Handles.LocoBrake.Actual == (int)AirBrakeHandleState.Lap ? "LAP" : "REL");
				}
				else
				{
					t += " - " + (TrainManager.PlayerTrain.Handles.LocoBrake.Actual != 0 ? "L" + TrainManager.PlayerTrain.Handles.LocoBrake.Actual.ToString(Culture) : "N");
				}
			}
			renderer.OpenGlString.Draw(renderer.Fonts.SmallFont, t, new Vector2(2, renderer.Screen.Height - 32), TextAlignment.TopLeft, Color128.White, true);
			// driver handles
			t = "driver: ";
			if (TrainManager.PlayerTrain.CurrentDirection == TrackDirection.Reverse)
			{
				t += (TrainManager.PlayerTrain.Handles.Reverser.Driver == ReverserPosition.Reverse ? "F" : TrainManager.PlayerTrain.Handles.Reverser.Driver == ReverserPosition.Forwards ? "B" : "N");
			}
			else
			{
				t += (TrainManager.PlayerTrain.Handles.Reverser.Driver == ReverserPosition.Reverse ? "B" : TrainManager.PlayerTrain.Handles.Reverser.Driver == ReverserPosition.Forwards ? "F" : "N");
			}

			if (TrainManager.PlayerTrain.Handles.HandleType == HandleType.SingleHandle)
			{
				t += " - " + (TrainManager.PlayerTrain.Handles.EmergencyBrake.Driver ? "EMG" : TrainManager.PlayerTrain.Handles.Brake.Driver != 0 ? "B" + TrainManager.PlayerTrain.Handles.Brake.Driver.ToString(Culture) : TrainManager.PlayerTrain.Handles.HoldBrake.Driver ? "HLD" : TrainManager.PlayerTrain.Handles.Power.Driver != 0 ? "P" + TrainManager.PlayerTrain.Handles.Power.Driver.ToString(Culture) : "N");
			}
			else if (TrainManager.PlayerTrain.Handles.Brake is AirBrakeHandle)
			{
				t += " - " + (TrainManager.PlayerTrain.Handles.Power.Driver != 0 ? "P" + TrainManager.PlayerTrain.Handles.Power.Driver.ToString(Culture) : "N");
				t += " - " + (TrainManager.PlayerTrain.Handles.EmergencyBrake.Driver ? "EMG" : TrainManager.PlayerTrain.Handles.Brake.Driver == (int)AirBrakeHandleState.Service ? "SRV" : TrainManager.PlayerTrain.Handles.Brake.Driver == (int)AirBrakeHandleState.Lap ? "LAP" : "REL");
			}
			else
			{
				t += " - " + (TrainManager.PlayerTrain.Handles.Power.Driver != 0 ? "P" + TrainManager.PlayerTrain.Handles.Power.Driver.ToString(Culture) : "N");
				t += " - " + (TrainManager.PlayerTrain.Handles.EmergencyBrake.Driver ? "EMG" : TrainManager.PlayerTrain.Handles.Brake.Driver != 0 ? "B" + TrainManager.PlayerTrain.Handles.Brake.Driver.ToString(Culture) : TrainManager.PlayerTrain.Handles.HoldBrake.Driver ? "HLD" : "N");
			}
			if (TrainManager.PlayerTrain.Handles.HasLocoBrake)
			{
				if (TrainManager.PlayerTrain.Handles.LocoBrake is LocoAirBrakeHandle)
				{
					t += " - " + (TrainManager.PlayerTrain.Handles.LocoBrake.Actual == (int)AirBrakeHandleState.Service ? "SRV" : TrainManager.PlayerTrain.Handles.LocoBrake.Actual == (int)AirBrakeHandleState.Lap ? "LAP" : "REL");
				}
				else
				{
					t += " - " + (TrainManager.PlayerTrain.Handles.LocoBrake.Actual != 0 ? "L" + TrainManager.PlayerTrain.Handles.LocoBrake.Actual.ToString(Culture) : "N");
				}
			}
			renderer.OpenGlString.Draw(renderer.Fonts.SmallFont, t, new Vector2(2, renderer.Screen.Height - 18), TextAlignment.TopLeft, Color128.White, true);
			// debug information
			int texturesRegistered = Program.Renderer.TextureManager.GetNumberOfRegisteredTextures();
			int soundBuffersRegistered = Program.Sounds.GetNumberOfRegisteredBuffers();
			int soundBuffersLoaded = Program.Sounds.GetNumberOfLoadedBuffers();
			int soundSourcesRegistered = Program.Sounds.GetNumberOfRegisteredSources();
			int soundSourcesPlaying = Program.Sounds.GetNumberOfPlayingSources();
			int car = 0;
			for (int i = 0; i < TrainManager.PlayerTrain.Cars.Length; i++)
			{
				if (TrainManager.PlayerTrain.Cars[i].TractionModel.ProvidesPower)
				{
					car = i;
					break;
				}
			}
			double mass = 0.0;
			for (int i = 0; i < TrainManager.PlayerTrain.Cars.Length; i++)
			{
				mass += TrainManager.PlayerTrain.Cars[i].CurrentMass;
			}

			string rainIntensity = TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Windscreen != null && TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Windscreen.legacyRainEvents ? "Legacy beacon based." : TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].FrontAxle.Follower.RainIntensity + "%";
			double elevation = Program.Renderer.Camera.AbsolutePosition.Y + Program.CurrentRoute.Atmosphere.InitialElevation;
			double airTemperature = Program.CurrentRoute.Atmosphere.GetAirTemperature(elevation);
			double airPressure = Program.CurrentRoute.Atmosphere.GetAirPressure(elevation, airTemperature);
			double airDensity = Program.CurrentRoute.Atmosphere.GetAirDensity(airPressure, airTemperature);
			int hours = (int)Program.CurrentRoute.SecondsSinceMidnight / 3600,
				remainder = (int)Program.CurrentRoute.SecondsSinceMidnight % 3600,
				minutes = remainder / 60,
				seconds = remainder % 60;

			bool hasRPM = false;
			string RPM = "Engine RPM: ";
			for (int i = 0; i < TrainManager.PlayerTrain.Cars.Length; i++)
			{
				if (TrainManager.PlayerTrain.Cars[i].TractionModel is DieselEngine dieselEngine)
				{
					RPM += "Car " + i + " " + (int)dieselEngine.CurrentRPM + "rpm ";
					hasRPM = true;
				}
			}

			string[] Lines = {
				"=system",
				"fps: " + Program.Renderer.FrameRate.ToString("0.0", Culture),
				"time:" + hours.ToString("00") +  ":" + minutes.ToString("00") + ":" + seconds.ToString("00"),
				"score: " + Game.CurrentScore.CurrentValue.ToString(Culture),
				"",
				"=train",
				"speed: " + (Math.Abs(TrainManager.PlayerTrain.CurrentSpeed) * 3.6).ToString("0.00", Culture) + " km/h",
				"power (car " + car.ToString(Culture) +  "): " + (TrainManager.PlayerTrain.Cars[car].TractionModel.CurrentAcceleration < 0.0 ? TrainManager.PlayerTrain.Cars[car].TractionModel.CurrentAcceleration * Math.Sign(TrainManager.PlayerTrain.Cars[car].CurrentSpeed) : TrainManager.PlayerTrain.Cars[car].TractionModel.CurrentAcceleration * (double)TrainManager.PlayerTrain.Handles.Reverser.Actual).ToString("0.0000", Culture) + " m/s²",
				"wheelslip (car " + car.ToString(Culture) +  "): " + TrainManager.PlayerTrain.Cars[car].FrontAxle.CurrentWheelSlip,
				"acceleration: " + TrainManager.PlayerTrain.Specs.CurrentAverageAcceleration.ToString("0.0000", Culture) + " m/s²",
				"position: " + TrainManager.PlayerTrain.FrontCarTrackPosition.ToString("0.00", Culture) + " m",
				"rain intensity: " + rainIntensity,
				"elevation: " + (Program.CurrentRoute.Atmosphere.InitialElevation + TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].FrontAxle.Follower.WorldPosition.Y).ToString("0.00", Culture) + " m",
				"temperature: " + (airTemperature - 273.15).ToString("0.00", Culture) + " °C",
				"air pressure: " + (0.001 * airPressure).ToString("0.00", Culture) + " kPa",
				"air density: " + airDensity.ToString("0.0000", Culture) + " kg/m³",
				"speed of sound: " + (Program.CurrentRoute.Atmosphere.GetSpeedOfSound(airDensity) * 3.6).ToString("0.00", Culture) + " km/h",
				"passenger ratio: " + TrainManager.PlayerTrain.CargoRatio.ToString("0.00"),
				"total mass: " + mass.ToString("0.00", Culture) + " kg",
				"" + (hasRPM ? RPM : string.Empty),
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
				"renderer type: " + (Program.Renderer.AvailableNewRenderer ? "openGL 4 (new)" : "openGL 1.2 (old)"),
				"opaque faces: " + Program.Renderer.VisibleObjects.OpaqueFaces.Count.ToString(Culture),
				"alpha faces: " + Program.Renderer.VisibleObjects.AlphaFaces.Count.ToString(Culture),
				"overlay opaque faces: " + Program.Renderer.VisibleObjects.OverlayOpaqueFaces.Count.ToString(Culture),
				"overlay alpha faces: " + Program.Renderer.VisibleObjects.OverlayAlphaFaces.Count.ToString(Culture),
				"textures loaded: " + renderer.TextureManager.GetNumberOfLoadedTextures().ToString(Culture) + " static, " + renderer.TextureManager.GetNumberOfLoadedAnimatedTextures() + " animated / " + texturesRegistered.ToString(Culture) + " total",
				"",
				"=camera",
				"position: " + Program.Renderer.CameraTrackFollower.TrackPosition.ToString("0.00", Culture) + " m",
				"curve radius: " + Program.Renderer.CameraTrackFollower.CurveRadius.ToString("0.00", Culture) + " m",
				"curve cant: " + (1000.0 * Math.Abs(Program.Renderer.CameraTrackFollower.CurveCant)).ToString("0.00", Culture) + " mm" + (Program.Renderer.CameraTrackFollower.CurveCant < 0.0 ? " (left)" : Program.Renderer.CameraTrackFollower.CurveCant > 0.0 ? " (right)" : ""),
				"pitch: " + Program.Renderer.CameraTrackFollower.Pitch.ToString("0.00", Culture),
				"current anchor car: " + TrainManager.PlayerTrain.CameraCar,
				"anchor car track index (front axle): " + TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.CameraCar].FrontAxle.Follower.TrackIndex,
				"anchor car track index (rear axle): " + TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.CameraCar].RearAxle.Follower.TrackIndex,
				"",
				"=sound",
				"sound buffers: " + soundBuffersLoaded.ToString(Culture) + " loaded / " + soundBuffersRegistered.ToString(Culture) + " total",
				"sound sources: " + soundSourcesPlaying.ToString(Culture) + " playing / " + Interface.CurrentOptions.SoundNumber + " max playing / " + soundSourcesRegistered.ToString(Culture) + " total",
				(Interface.CurrentOptions.SoundModel == SoundModels.Inverse ? "log clamp factor: " + Program.Sounds.LogClampFactor.ToString("0.00") : "outer radius factor: " + Program.Sounds.OuterRadiusFactor.ToString("0.00", Culture)),
				"",
				"=debug",
				"bvets hacks: " + (Interface.CurrentOptions.EnableBveTsHacks ? "enabled" : "disabled"),
				"time acceleration: " + MainLoop.TimeFactor + "x",
				"viewing distance: " + Interface.CurrentOptions.ViewingDistance + "m",
				"visibility mode: " + Interface.CurrentOptions.ObjectDisposalMode + (Interface.CurrentOptions.ObjectDisposalMode == ObjectDisposalMode.QuadTree ? ", leaf size " + Interface.CurrentOptions.QuadTreeLeafSize + "m" : string.Empty),
				"train plugin status: " + (TrainManager.PlayerTrain.Plugin != null ? (TrainManager.PlayerTrain.Plugin.PluginValid ? "ok" : "error") : "n/a"),
				"train plugin message: " + (TrainManager.PlayerTrain.Plugin != null ? (TrainManager.PlayerTrain.Plugin.PluginMessage ?? "n/a") : "n/a"),
				"traction message: " + TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].TractionModel.Message,
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
						Vector2 size = renderer.Fonts.SmallFont.MeasureString(text);
						renderer.Rectangle.Draw(null, new Vector2(x, y), new Vector2(size.X + 6.0f, size.Y + 2.0f), new Color128(0.35f, 0.65f, 0.90f, 0.8f));
						renderer.OpenGlString.Draw(renderer.Fonts.SmallFont, text, new Vector2(x + 3, y), TextAlignment.TopLeft, Color128.White);
					}
					else
					{
						renderer.OpenGlString.Draw(renderer.Fonts.SmallFont, Lines[i], new Vector2(x, y), TextAlignment.TopLeft, Color128.White, true);
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
			// debug
			renderer.Rectangle.Draw(null, Vector2.Null, new Vector2(renderer.Screen.Width, renderer.Screen.Height), Color128.SemiTransparentGrey);
			string[] Lines;
			if (TrainManager.PlayerTrain.Plugin?.Panel.Length > 0)
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
						Vector2 size = renderer.Fonts.SmallFont.MeasureString(text);
						renderer.Rectangle.Draw(null, new Vector2(x, y), new Vector2(size.X + 6.0f, size.Y + 2.0f), new Color128(0.35f, 0.65f, 0.9f, 0.8f));
						renderer.OpenGlString.Draw(renderer.Fonts.SmallFont, text, new Vector2(x + 3, y), TextAlignment.TopLeft, Color128.White);
					}
					else
					{
						renderer.OpenGlString.Draw(renderer.Fonts.SmallFont, Lines[i], new Vector2(x, y), TextAlignment.TopLeft, Color128.White, true);
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
						renderer.OpenGlString.Draw(renderer.Fonts.SmallFont, "Brake pipe", new Vector2(x, oy - 16), TextAlignment.TopLeft, Color128.White, true);
						heading[0] = true;
					}
					renderer.Rectangle.Draw(null, new Vector2(x, y), new Vector2(w, h), Color128.Black);
					double p = TrainManager.PlayerTrain.Cars[i].CarBrake.BrakePipe.CurrentPressure;
					double r = p / TrainManager.PlayerTrain.Cars[i].CarBrake.BrakePipe.NormalPressure;
					renderer.Rectangle.Draw(null, new Vector2((float)x, (float)y), new Vector2(r * w, h), Color128.Yellow);
				}
				x += w + 8.0;
				// auxiliary reservoir
				if (TrainManager.PlayerTrain.Cars[i].CarBrake is AutomaticAirBrake | TrainManager.PlayerTrain.Cars[i].CarBrake is ElectromagneticStraightAirBrake)
				{
					if (!heading[1])
					{
						renderer.OpenGlString.Draw(renderer.Fonts.SmallFont, "Auxillary reservoir", new Vector2(x, oy - 16), TextAlignment.TopLeft, Color128.White, true);
						heading[1] = true;
					}
					renderer.Rectangle.Draw(null, new Vector2(x, y), new Vector2(w, h), Color128.Black);
					double p = TrainManager.PlayerTrain.Cars[i].CarBrake.AuxiliaryReservoir.CurrentPressure;
					double r = p / TrainManager.PlayerTrain.Cars[i].CarBrake.AuxiliaryReservoir.MaximumPressure;
					renderer.Rectangle.Draw(null, new Vector2(x, y), new Vector2(r * w, h), Color128.Grey);
				}
				x += w + 8.0;
				// brake cylinder
				{
					if (!heading[2])
					{
						renderer.OpenGlString.Draw(renderer.Fonts.SmallFont, "Brake cylinder", new Vector2(x, oy - 16), TextAlignment.TopLeft, Color128.White, true);
						heading[2] = true;
					}

					if (!(TrainManager.PlayerTrain.Cars[i].CarBrake is ThroughPiped))
					{
						renderer.Rectangle.Draw(null, new Vector2((float)x, (float)y), new Vector2(w, h), Color128.Black);
						double p = TrainManager.PlayerTrain.Cars[i].CarBrake.BrakeCylinder.CurrentPressure;
						double r = p / TrainManager.PlayerTrain.Cars[i].CarBrake.BrakeCylinder.EmergencyMaximumPressure;
						renderer.Rectangle.Draw(null, new Vector2(x, y), new Vector2(r * w, h), new Color128(0.75f, 0.5f, 0.25f, 1.0f));
					}
				}
				x += w + 8.0;
				// main reservoir
				if (TrainManager.PlayerTrain.Cars[i].CarBrake.BrakeType == BrakeType.Main)
				{
					if (!heading[3])
					{
						renderer.OpenGlString.Draw(renderer.Fonts.SmallFont, "Main reservoir", new Vector2(x, oy - 16), TextAlignment.TopLeft, Color128.White, true);
						heading[3] = true;
					}
					renderer.Rectangle.Draw(null, new Vector2(x, y), new Vector2(w, h), Color128.Black);
					double p = TrainManager.PlayerTrain.Cars[i].CarBrake.MainReservoir.CurrentPressure;
					double r = p / TrainManager.PlayerTrain.Cars[i].CarBrake.MainReservoir.MaximumPressure;
					renderer.Rectangle.Draw(null, new Vector2(x, y), new Vector2(r * w, h), Color128.Red);
				}
				x += w + 8.0;
				// equalizing reservoir
				if (TrainManager.PlayerTrain.Cars[i].CarBrake.BrakeType == BrakeType.Main)
				{
					if (!heading[4])
					{
						renderer.OpenGlString.Draw(renderer.Fonts.SmallFont, "Equalizing reservoir", new Vector2(x, oy - 16), TextAlignment.TopLeft, Color128.White, true);
						heading[4] = true;
					}
					renderer.Rectangle.Draw(null, new Vector2(x, y), new Vector2(w, h), Color128.Black);
					double p = TrainManager.PlayerTrain.Cars[i].CarBrake.EqualizingReservoir.CurrentPressure;
					double r = p / TrainManager.PlayerTrain.Cars[i].CarBrake.EqualizingReservoir.NormalPressure;
					renderer.Rectangle.Draw(null, new Vector2(x, y), new Vector2(r * w, h), new Color128(0.0f, 0.75f, 0.0f, 1.0f));
				}
				x += w + 8.0;
				// straight air pipe
				if (TrainManager.PlayerTrain.Cars[i].CarBrake is ElectromagneticStraightAirBrake esb && TrainManager.PlayerTrain.Cars[i].CarBrake.BrakeType == BrakeType.Main)
				{
					if (!heading[5])
					{
						renderer.OpenGlString.Draw(renderer.Fonts.SmallFont, "Straight air pipe", new Vector2(x, oy - 16), TextAlignment.TopLeft, Color128.White, true);
						heading[5] = true;
					}
					renderer.Rectangle.Draw(null, new Vector2(x, y), new Vector2((float)w, (float)h), Color128.Black);
					double p = esb.StraightAirPipe.CurrentPressure;
					double r = p / TrainManager.PlayerTrain.Cars[i].CarBrake.BrakeCylinder.EmergencyMaximumPressure;
					renderer.Rectangle.Draw(null, new Vector2(x, y), new Vector2(r * w, h), Color128.DeepSkyBlue);
				}
				y += h + 8.0;
			}
		}
	}
}
