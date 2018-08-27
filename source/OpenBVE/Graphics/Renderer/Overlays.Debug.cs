using System;
using OpenBve.BrakeSystems;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenTK.Graphics.OpenGL;

namespace OpenBve
{
	internal static partial class Renderer
	{
		/// <summary>Renders the debug (F10) overlay</summary>
		private static void RenderDebugOverlays()
		{
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			// debug
			GL.Color4(0.5, 0.5, 0.5, 0.5);
			RenderOverlaySolid(0.0f, 0.0f, (double)Screen.Width, (double)Screen.Height);
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
				DrawString(Fonts.SmallFont, t, new System.Drawing.Point(2, Screen.Height - 46), TextAlignment.TopLeft, Color128.White, true);
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
						t += " - " + (TrainManager.PlayerTrain.Handles.LocoBrake.Actual == (int) TrainManager.AirBrakeHandleState.Service ? "SRV" : TrainManager.PlayerTrain.Handles.LocoBrake.Actual == (int) TrainManager.AirBrakeHandleState.Lap ? "LAP" : "REL");
					}
					else
					{
						t += " - " + (TrainManager.PlayerTrain.Handles.LocoBrake.Actual != 0 ? "L" + TrainManager.PlayerTrain.Handles.LocoBrake.Actual.ToString(Culture) : "N");
					}
				}
				DrawString(Fonts.SmallFont, t, new System.Drawing.Point(2, Screen.Height - 32), TextAlignment.TopLeft, Color128.White, true);
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
						t += " - " + (TrainManager.PlayerTrain.Handles.LocoBrake.Actual == (int) TrainManager.AirBrakeHandleState.Service ? "SRV" : TrainManager.PlayerTrain.Handles.LocoBrake.Actual == (int) TrainManager.AirBrakeHandleState.Lap ? "LAP" : "REL");
					}
					else
					{
						t += " - " + (TrainManager.PlayerTrain.Handles.LocoBrake.Actual != 0 ? "L" + TrainManager.PlayerTrain.Handles.LocoBrake.Actual.ToString(Culture) : "N");
					}
				}
				DrawString(Fonts.SmallFont, t, new System.Drawing.Point(2, Screen.Height - 18), TextAlignment.TopLeft, Color128.White, true);
			}
			// debug information
			int texturesLoaded = Textures.GetNumberOfLoadedTextures();
			int texturesRegistered = Textures.GetNumberOfRegisteredTextures();
			int soundBuffersRegistered = Sounds.GetNumberOfLoadedBuffers();
			int soundBuffersLoaded = Sounds.GetNumberOfLoadedBuffers();
			int soundSourcesRegistered = Sounds.GetNumberOfRegisteredSources();
			int soundSourcesPlaying = Sounds.GetNumberOfPlayingSources();
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
			int hours = (int)Game.SecondsSinceMidnight / 3600, 
				remainder = (int)Game.SecondsSinceMidnight % 3600, 
				minutes = remainder / 60, 
				seconds = remainder % 60;
			string[] Lines = new string[] {
				"=system",
				"fps: " + Game.InfoFrameRate.ToString("0.0", Culture) + (MainLoop.LimitFramerate ? " (low cpu)" : ""),
				"time:" + hours.ToString("00") +  ":" + minutes.ToString("00") + ":" + seconds.ToString("00"),
				"score: " + Game.CurrentScore.CurrentValue.ToString(Culture),
				"",
				"=train",
				"speed: " + (Math.Abs(TrainManager.PlayerTrain.Specs.CurrentAverageSpeed) * 3.6).ToString("0.00", Culture) + " km/h",
				"power (car " + car.ToString(Culture) +  "): " + (TrainManager.PlayerTrain.Cars[car].Specs.CurrentAccelerationOutput < 0.0 ? TrainManager.PlayerTrain.Cars[car].Specs.CurrentAccelerationOutput * (double)Math.Sign(TrainManager.PlayerTrain.Cars[car].Specs.CurrentSpeed) : TrainManager.PlayerTrain.Cars[car].Specs.CurrentAccelerationOutput * (double)TrainManager.PlayerTrain.Handles.Reverser.Actual).ToString("0.0000", Culture) + " m/s²",
				"acceleration: " + TrainManager.PlayerTrain.Specs.CurrentAverageAcceleration.ToString("0.0000", Culture) + " m/s²",
				"position: " + (TrainManager.PlayerTrain.Cars[0].FrontAxle.Follower.TrackPosition - TrainManager.PlayerTrain.Cars[0].FrontAxle.Position + 0.5 * TrainManager.PlayerTrain.Cars[0].Length).ToString("0.00", Culture) + " m",
				"elevation: " + (Game.RouteInitialElevation + TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].FrontAxle.Follower.WorldPosition.Y).ToString("0.00", Culture) + " m",
				"temperature: " + (TrainManager.PlayerTrain.Specs.CurrentAirTemperature - 273.15).ToString("0.00", Culture) + " °C",
				"air pressure: " + (0.001 * TrainManager.PlayerTrain.Specs.CurrentAirPressure).ToString("0.00", Culture) + " kPa",
				"air density: " + TrainManager.PlayerTrain.Specs.CurrentAirDensity.ToString("0.0000", Culture) + " kg/m³",
				"speed of sound: " + (Game.GetSpeedOfSound(TrainManager.PlayerTrain.Specs.CurrentAirDensity) * 3.6).ToString("0.00", Culture) + " km/h",
				"passenger ratio: " + TrainManager.PlayerTrain.Passengers.PassengerRatio.ToString("0.00"),
				"total mass: " + mass.ToString("0.00", Culture) + " kg",
				"",
				"=route",
				"track limit: " + (TrainManager.PlayerTrain.CurrentRouteLimit == double.PositiveInfinity ? "unlimited" : ((TrainManager.PlayerTrain.CurrentRouteLimit * 3.6).ToString("0.0", Culture) + " km/h")),
				"signal limit: " + (TrainManager.PlayerTrain.CurrentSectionLimit == double.PositiveInfinity ? "unlimited" : ((TrainManager.PlayerTrain.CurrentSectionLimit * 3.6).ToString("0.0", Culture) + " km/h")),
				"total static objects: " + ObjectManager.ObjectsUsed.ToString(Culture),
				"total static GL_TRIANGLES: " + Game.InfoTotalTriangles.ToString(Culture),
				"total static GL_TRIANGLE_STRIP: " + Game.InfoTotalTriangleStrip.ToString(Culture),
				"total static GL_QUADS: " + Game.InfoTotalQuads.ToString(Culture),
				"total static GL_QUAD_STRIP: " + Game.InfoTotalQuadStrip.ToString(Culture),
				"total static GL_POLYGON: " + Game.InfoTotalPolygon.ToString(Culture),
				"total animated objects: " + ObjectManager.AnimatedWorldObjectsUsed.ToString(Culture),
				"",
				"=renderer",
				"static opaque faces: " + Game.InfoStaticOpaqueFaceCount.ToString(Culture),
				"dynamic opaque faces: " + DynamicOpaque.FaceCount.ToString(Culture),
				"dynamic alpha faces: " + DynamicAlpha.FaceCount.ToString(Culture),
				"overlay opaque faces: " + OverlayOpaque.FaceCount.ToString(Culture),
				"overlay alpha faces: " + OverlayAlpha.FaceCount.ToString(Culture),
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
				(Interface.CurrentOptions.SoundModel == Sounds.SoundModels.Inverse ? "log clamp factor: " + Sounds.LogClampFactor.ToString("0.00") : "outer radius factor: " + Sounds.OuterRadiusFactor.ToString("0.00", Culture)),
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
						System.Drawing.Size size = MeasureString(Fonts.SmallFont, text);
						GL.Color4(0.35f, 0.65f, 0.90f, 0.8f);
						RenderOverlaySolid(x, y, x + size.Width + 6.0f, y + size.Height + 2.0f);
						DrawString(Fonts.SmallFont, text, new System.Drawing.Point((int)x + 3, (int)y), TextAlignment.TopLeft, Color128.White);
					}
					else
					{
						DrawString(Fonts.SmallFont, Lines[i], new System.Drawing.Point((int)x, (int)y), TextAlignment.TopLeft, Color128.White, true);
					}
					y += 14.0;
				}
				else if (y >= (double)Screen.Height - 240.0)
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

		private static void RenderATSDebugOverlay()
		{
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			// debug
			GL.Color4(0.5, 0.5, 0.5, 0.5);
			RenderOverlaySolid(0.0f, 0.0f, (double)Screen.Width, (double)Screen.Height);
			string[] Lines;
			if (TrainManager.PlayerTrain.Plugin.Panel.Length > 0)
			{
				Lines = new string[TrainManager.PlayerTrain.Plugin.Panel.Length + 2];
				Lines[0] = "=ATS Plugin Variables";
				Lines[1] = "";
				for (int i = 2; i < TrainManager.PlayerTrain.Plugin.Panel.Length + 2; i++)
				{
					Lines[i] = (i -2).ToString("000") + " : " + TrainManager.PlayerTrain.Plugin.Panel[i - 2];
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
						System.Drawing.Size size = MeasureString(Fonts.SmallFont, text);
						GL.Color4(0.35f, 0.65f, 0.90f, 0.8f);
						RenderOverlaySolid(x, y, x + size.Width + 6.0f, y + size.Height + 2.0f);
						DrawString(Fonts.SmallFont, text, new System.Drawing.Point((int)x + 3, (int)y), TextAlignment.TopLeft, Color128.White);
					}
					else
					{
						DrawString(Fonts.SmallFont, Lines[i], new System.Drawing.Point((int)x, (int)y), TextAlignment.TopLeft, Color128.White, true);
					}
					y += 14.0;
					if (y > Screen.Height - 20.0)
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
		private static void RenderBrakeSystemDebug()
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
						DrawString(Fonts.SmallFont, "Brake pipe", new System.Drawing.Point((int)x, (int)(oy - 16)), TextAlignment.TopLeft, Color128.White, true);
						heading[0] = true;
					}
					GL.Color3(0.0f, 0.0f, 0.0f);
					RenderOverlaySolid(x, y, x + w, y + h);
					double p = TrainManager.PlayerTrain.Cars[i].CarBrake.brakePipe.CurrentPressure;
					double r = p / TrainManager.PlayerTrain.Cars[i].CarBrake.brakePipe.NormalPressure;
					GL.Color3(1.0f, 1.0f, 0.0f);
					RenderOverlaySolid(x, y, x + r * w, y + h);
				} x += w + 8.0;
				// auxillary reservoir
				if (TrainManager.PlayerTrain.Cars[i].CarBrake is AutomaticAirBrake | TrainManager.PlayerTrain.Cars[i].CarBrake is ElectromagneticStraightAirBrake)
				{
					if (!heading[1])
					{
						DrawString(Fonts.SmallFont, "Auxillary reservoir", new System.Drawing.Point((int)x, (int)(oy - 16)), TextAlignment.TopLeft, Color128.White, true);
						heading[1] = true;
					}
					GL.Color3(0.0f, 0.0f, 0.0f);
					RenderOverlaySolid(x, y, x + w, y + h);
					double p = TrainManager.PlayerTrain.Cars[i].CarBrake.auxiliaryReservoir.CurrentPressure;
					double r = p / TrainManager.PlayerTrain.Cars[i].CarBrake.auxiliaryReservoir.MaximumPressure;
					GL.Color3(0.5f, 0.5f, 0.5f);
					RenderOverlaySolid(x, y, x + r * w, y + h);
				} x += w + 8.0;
				// brake cylinder
				{
					if (!heading[2])
					{
						DrawString(Fonts.SmallFont, "Brake cylinder", new System.Drawing.Point((int)x, (int)(oy - 16)), TextAlignment.TopLeft, Color128.White, true);
						heading[2] = true;
					}
					GL.Color3(0.0f, 0.0f, 0.0f);
					RenderOverlaySolid(x, y, x + w, y + h);
					double p = TrainManager.PlayerTrain.Cars[i].CarBrake.brakeCylinder.CurrentPressure;
					double r = p / TrainManager.PlayerTrain.Cars[i].CarBrake.brakeCylinder.EmergencyMaximumPressure;
					GL.Color3(0.75f, 0.5f, 0.25f);
					RenderOverlaySolid(x, y, x + r * w, y + h);
				} x += w + 8.0;
				// main reservoir
				if (TrainManager.PlayerTrain.Cars[i].CarBrake.brakeType == BrakeType.Main)
				{
					if (!heading[3])
					{
						DrawString(Fonts.SmallFont, "Main reservoir", new System.Drawing.Point((int)x, (int)(oy - 16)), TextAlignment.TopLeft, Color128.White, true);
						heading[3] = true;
					}
					GL.Color3(0.0f, 0.0f, 0.0f);
					RenderOverlaySolid(x, y, x + w, y + h);
					double p = TrainManager.PlayerTrain.Cars[i].CarBrake.mainReservoir.CurrentPressure;
					double r = p / TrainManager.PlayerTrain.Cars[i].CarBrake.mainReservoir.MaximumPressure;
					GL.Color3(1.0f, 0.0f, 0.0f);
					RenderOverlaySolid(x, y, x + r * w, y + h);
				} x += w + 8.0;
				// equalizing reservoir
				if (TrainManager.PlayerTrain.Cars[i].CarBrake.brakeType == BrakeType.Main)
				{
					if (!heading[4])
					{
						DrawString(Fonts.SmallFont, "Equalizing reservoir", new System.Drawing.Point((int)x, (int)(oy - 16)), TextAlignment.TopLeft, Color128.White, true);
						heading[4] = true;
					}
					GL.Color3(0.0f, 0.0f, 0.0f);
					RenderOverlaySolid(x, y, x + w, y + h);
					double p = TrainManager.PlayerTrain.Cars[i].CarBrake.equalizingReservoir.CurrentPressure;
					double r = p / TrainManager.PlayerTrain.Cars[i].CarBrake.equalizingReservoir.NormalPressure;
					GL.Color3(0.0f, 0.75f, 0.0f);
					RenderOverlaySolid(x, y, x + r * w, y + h);
				} x += w + 8.0;
				// straight air pipe
				if (TrainManager.PlayerTrain.Cars[i].CarBrake is ElectromagneticStraightAirBrake & TrainManager.PlayerTrain.Cars[i].CarBrake.brakeType == BrakeType.Main)
				{
					if (!heading[5])
					{
						DrawString(Fonts.SmallFont, "Straight air pipe", new System.Drawing.Point((int)x, (int)(oy - 16)), TextAlignment.TopLeft, Color128.White, true);
						heading[5] = true;
					}
					GL.Color3(0.0f, 0.0f, 0.0f);
					RenderOverlaySolid(x, y, x + w, y + h);
					double p = TrainManager.PlayerTrain.Cars[i].CarBrake.straightAirPipe.CurrentPressure;
					double r = p / TrainManager.PlayerTrain.Cars[i].CarBrake.brakeCylinder.EmergencyMaximumPressure;
					GL.Color3(0.0f, 0.75f, 1.0f);
					RenderOverlaySolid(x, y, x + r * w, y + h);
				} //x += w + 8.0;
				GL.Color3(0.0f, 0.0f, 0.0f);
				y += h + 8.0;
			}
		}
	}
}
