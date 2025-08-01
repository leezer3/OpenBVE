using System;
using LibRender2.Cameras;
using LibRender2.Menu;
using LibRender2.Overlays;
using LibRender2.Screens;
using LibRender2.Trains;
using LibRender2.Viewports;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Routes;
using OpenBveApi.Runtime;
using RouteManager2.MessageManager;
using RouteManager2.SignalManager;
using RouteManager2.Stations;
using TrainManager;
using TrainManager.Car;
using TrainManager.Car.Systems;
using TrainManager.Handles;

namespace OpenBve
{
	internal static partial class MainLoop
	{
		private static void ProcessDigitalControl(double TimeElapsed, ref Control Control)
		{
			bool lookahead = false;
			bool returnToCab = false;
			// digital control
			if (Control.DigitalState == DigitalControlState.Pressed)
			{
				// pressed
				Control.DigitalState =
					DigitalControlState.PressedAcknowledged;
				TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].DSD?.ControlDown(Control.Command);

				if (Translations.SecurityToVirtualKey(Control.Command, out VirtualKeys key))
				{
					TrainManager.PlayerTrain.Plugin?.KeyDown(key);
				}

				switch (Control.Command)
				{
					case Translations.Command.MiscQuit:
						// quit
						Game.Menu.PushMenu(MenuType.Quit);
						break;
					case Translations.Command.CameraInterior:
						// camera: interior
						MainLoop.SaveCameraSettings();
						if (Program.Renderer.Camera.CurrentMode != CameraViewMode.InteriorLookAhead & Program.Renderer.Camera.CurrentRestriction == CameraRestrictionMode.NotAvailable)
						{
							MessageManager.AddMessage(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"notification","interior_lookahead"}),
								MessageDependency.CameraView, GameMode.Expert,
								MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 2.0, null);
							lookahead = true;
						}
						else
						{
							MessageManager.AddMessage(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"notification","interior"}),
								MessageDependency.CameraView, GameMode.Expert,
								MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 2.0, null);
						}

						Program.Renderer.Camera.CurrentMode = CameraViewMode.Interior;
						MainLoop.RestoreCameraSettings();
						for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
						{
							if (j == TrainManager.PlayerTrain.CameraCar)
							{
								if (TrainManager.PlayerTrain.Cars[j].HasInteriorView)
								{
									TrainManager.PlayerTrain.Cars[j].ChangeCarSection(CarSectionType.Interior);
									Program.Renderer.Camera.CurrentRestriction = TrainManager.PlayerTrain.Cars[j].CameraRestrictionMode;
								}
								else
								{
									TrainManager.PlayerTrain.Cars[j].ChangeCarSection(CarSectionType.NotVisible, true);
									returnToCab = true;
								}
							}
							else
							{
								TrainManager.PlayerTrain.Cars[j].ChangeCarSection(CarSectionType.NotVisible, true);
							}
						}

						if (returnToCab)
						{
							//If our selected car does not have an interior view, we must store this fact, and return to the driver car after the loop has finished
							TrainManager.PlayerTrain.CameraCar = TrainManager.PlayerTrain.DriverCar;
							TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].ChangeCarSection(CarSectionType.Interior);
							Program.Renderer.Camera.CurrentRestriction = TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CameraRestrictionMode;
						}

						//Hide bogies
						for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
						{
							TrainManager.PlayerTrain.Cars[j].FrontBogie.ChangeSection(-1);
							TrainManager.PlayerTrain.Cars[j].RearBogie.ChangeSection(-1);
							TrainManager.PlayerTrain.Cars[j].Coupler.ChangeSection(-1);
						}

						Program.Renderer.Camera.AlignmentDirection = new CameraAlignment();
						Program.Renderer.Camera.AlignmentSpeed = new CameraAlignment();
						Program.Renderer.UpdateViewport(ViewportChangeMode.NoChange);
						World.UpdateAbsoluteCamera(TimeElapsed);
						Program.Renderer.UpdateViewingDistances(Program.CurrentRoute.CurrentBackground.BackgroundImageDistance);
						if (Program.Renderer.Camera.CurrentRestriction != CameraRestrictionMode.NotAvailable)
						{
							if (!Program.Renderer.Camera.PerformRestrictionTest(TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CameraRestriction))
							{
								World.InitializeCameraRestriction();
							}
						}

						if (lookahead)
						{
							Program.Renderer.Camera.CurrentMode = CameraViewMode.InteriorLookAhead;
						}
						break;
					case Translations.Command.CameraInteriorNoPanel:
						// camera: interior
						MainLoop.SaveCameraSettings();
						if (Program.Renderer.Camera.CurrentMode != CameraViewMode.InteriorLookAhead & Program.Renderer.Camera.CurrentRestriction == CameraRestrictionMode.NotAvailable)
						{
							MessageManager.AddMessage(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"notification","interior_lookahead"}),
								MessageDependency.CameraView, GameMode.Expert,
								MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 2.0, null);
							lookahead = true;
						}
						else
						{
							MessageManager.AddMessage(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"notification","interior"}),
								MessageDependency.CameraView, GameMode.Expert,
								MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 2.0, null);
						}

						Program.Renderer.Camera.CurrentMode = CameraViewMode.Interior;
						MainLoop.RestoreCameraSettings();
						for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
						{
							if (j == TrainManager.PlayerTrain.CameraCar)
							{
								if (TrainManager.PlayerTrain.Cars[j].HasInteriorView)
								{
									TrainManager.PlayerTrain.Cars[j].ChangeCarSection(CarSectionType.Interior);
									Program.Renderer.Camera.CurrentRestriction = TrainManager.PlayerTrain.Cars[j].CameraRestrictionMode;
								}
								else
								{
									TrainManager.PlayerTrain.Cars[j].ChangeCarSection(CarSectionType.NotVisible, true);
									returnToCab = true;
								}
							}
							else
							{
								TrainManager.PlayerTrain.Cars[j].ChangeCarSection(CarSectionType.NotVisible, true);
							}
						}

						if (returnToCab)
						{
							//If our selected car does not have an interior view, we must store this fact, and return to the driver car after the loop has finished
							TrainManager.PlayerTrain.CameraCar = TrainManager.PlayerTrain.DriverCar;
							TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].ChangeCarSection(CarSectionType.Interior);
							Program.Renderer.Camera.CurrentRestriction = TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CameraRestrictionMode;
						}

						//Hide interior and bogies
						for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
						{
							TrainManager.PlayerTrain.Cars[j].ChangeCarSection(CarSectionType.NotVisible, true);
							TrainManager.PlayerTrain.Cars[j].FrontBogie.ChangeSection(-1);
							TrainManager.PlayerTrain.Cars[j].RearBogie.ChangeSection(-1);
							TrainManager.PlayerTrain.Cars[j].Coupler.ChangeSection(-1);
						}

						Program.Renderer.Camera.AlignmentDirection = new CameraAlignment();
						Program.Renderer.Camera.AlignmentSpeed = new CameraAlignment();
						Program.Renderer.UpdateViewport(ViewportChangeMode.NoChange);
						World.UpdateAbsoluteCamera(TimeElapsed);
						Program.Renderer.UpdateViewingDistances(Program.CurrentRoute.CurrentBackground.BackgroundImageDistance);
						if (Program.Renderer.Camera.CurrentRestriction != CameraRestrictionMode.NotAvailable)
						{
							if (!Program.Renderer.Camera.PerformRestrictionTest(TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CameraRestriction))
							{
								World.InitializeCameraRestriction();
							}
						}

						if (lookahead)
						{
							Program.Renderer.Camera.CurrentMode = CameraViewMode.InteriorLookAhead;
						}
						break;
					case Translations.Command.CameraExterior:
						// camera: exterior
						if (TrainManager.PlayerTrain.CurrentDirection == TrackDirection.Reverse)
						{
							MessageManager.AddMessage(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"notification","exterior"}) + " " + (TrainManager.PlayerTrain.Cars.Length - TrainManager.PlayerTrain.CameraCar), MessageDependency.CameraView, GameMode.Expert,
								MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 2.0, null);
						}
						else
						{
							MessageManager.AddMessage(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"notification","exterior"}) + " " + (TrainManager.PlayerTrain.CameraCar + 1), MessageDependency.CameraView, GameMode.Expert,
								MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 2.0, null);
						}

						SaveCameraSettings();
						Program.Renderer.Camera.CurrentMode = CameraViewMode.Exterior;
						RestoreCameraSettings();
						for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
						{
							TrainManager.PlayerTrain.Cars[j].ChangeCarSection(CarSectionType.Exterior);
							TrainManager.PlayerTrain.Cars[j].FrontBogie.ChangeSection(0);
							TrainManager.PlayerTrain.Cars[j].RearBogie.ChangeSection(0);
							TrainManager.PlayerTrain.Cars[j].Coupler.ChangeSection(0);
						}

						Program.Renderer.Camera.AlignmentDirection = new CameraAlignment();
						Program.Renderer.Camera.AlignmentSpeed = new CameraAlignment();
						Program.Renderer.UpdateViewport(ViewportChangeMode.NoChange);
						World.UpdateAbsoluteCamera(TimeElapsed);
						Program.Renderer.UpdateViewingDistances(Program.CurrentRoute.CurrentBackground.BackgroundImageDistance);
						break;
					case Translations.Command.CameraTrack:
					case Translations.Command.CameraFlyBy:
						// camera: track / fly-by
					{
						SaveCameraSettings();
						if (Control.Command == Translations.Command.CameraTrack)
						{
							Program.Renderer.Camera.CurrentMode = CameraViewMode.Track;
							MessageManager.AddMessage(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"notification","track"}),
								MessageDependency.CameraView, GameMode.Expert,
								MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 2.0, null);
						}
						else
						{
							if (Program.Renderer.Camera.CurrentMode == CameraViewMode.FlyBy)
							{
								Program.Renderer.Camera.CurrentMode = CameraViewMode.FlyByZooming;
								MessageManager.AddMessage(
									Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"notification","flybyzooming"}),
									MessageDependency.CameraView, GameMode.Expert,
									MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 2.0, null);
							}
							else
							{
								Program.Renderer.Camera.CurrentMode = CameraViewMode.FlyBy;
								MessageManager.AddMessage(
									Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"notification","flybynormal"}),
									MessageDependency.CameraView, GameMode.Expert,
									MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 2.0, null);
							}
						}

						RestoreCameraSettings();
						for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
						{
							TrainManager.PlayerTrain.Cars[j].ChangeCarSection(CarSectionType.Exterior);
							TrainManager.PlayerTrain.Cars[j].FrontBogie.ChangeSection(0);
							TrainManager.PlayerTrain.Cars[j].RearBogie.ChangeSection(0);
							TrainManager.PlayerTrain.Cars[j].Coupler.ChangeSection(0);
						}

						Program.Renderer.Camera.AlignmentDirection = new CameraAlignment();
						Program.Renderer.Camera.AlignmentSpeed = new CameraAlignment();
						Program.Renderer.UpdateViewport(ViewportChangeMode.NoChange);
						World.UpdateAbsoluteCamera(TimeElapsed);
						Program.Renderer.UpdateViewingDistances(Program.CurrentRoute.CurrentBackground.BackgroundImageDistance);
					}
						break;
					case Translations.Command.CameraPOIPrevious:
						//If we are in the exterior train view, shift down one car until we hit the last car
						if (Program.Renderer.Camera.CurrentMode == CameraViewMode.Exterior)
						{
							TrainManager.PlayerTrain.ChangeCameraCar(false);
							return;
						}

						//Otherwise, check if we can move down to the previous POI
						if (Program.CurrentRoute.ApplyPointOfInterest(TrackDirection.Reverse))
						{
							World.UpdateAbsoluteCamera();
							if (Program.Renderer.Camera.CurrentMode < CameraViewMode.Exterior)
							{
								SaveCameraSettings();
								Program.Renderer.Camera.CurrentMode = CameraViewMode.Track;
								MessageManager.AddMessage(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"notification","track"}),
									MessageDependency.CameraView, GameMode.Expert,
									MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 2.0, null);
							}

							double z = Program.Renderer.Camera.Alignment.Position.Z;
							Program.Renderer.Camera.Alignment.Position =
								new OpenBveApi.Math.Vector3(Program.Renderer.Camera.Alignment.Position.X,
									Program.Renderer.Camera.Alignment.Position.Y, 0.0);
							Program.Renderer.Camera.Alignment.Zoom = 0.0;
							Program.Renderer.Camera.AlignmentDirection = new CameraAlignment();
							Program.Renderer.Camera.AlignmentSpeed = new CameraAlignment();
							for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
							{
								TrainManager.PlayerTrain.Cars[j].ChangeCarSection(CarSectionType.Exterior);
								TrainManager.PlayerTrain.Cars[j].FrontBogie.ChangeSection(0);
								TrainManager.PlayerTrain.Cars[j].RearBogie.ChangeSection(0);
								TrainManager.PlayerTrain.Cars[j].Coupler.ChangeSection(0);
							}

							Program.Renderer.CameraTrackFollower.UpdateRelative(z, true, false);
							Program.Renderer.Camera.Alignment.TrackPosition = Program.Renderer.CameraTrackFollower.TrackPosition;
							Program.Renderer.Camera.VerticalViewingAngle = Program.Renderer.Camera.OriginalVerticalViewingAngle;
							Program.Renderer.UpdateViewport(ViewportChangeMode.NoChange);
							World.UpdateAbsoluteCamera(TimeElapsed);
							Program.Renderer.UpdateViewingDistances(Program.CurrentRoute.CurrentBackground.BackgroundImageDistance);
						}

						break;
					case Translations.Command.CameraPOINext:
						//If we are in the exterior train view, shift up one car until we hit index 0
						if (Program.Renderer.Camera.CurrentMode == CameraViewMode.Exterior)
						{
							TrainManagerBase.PlayerTrain.ChangeCameraCar(true);
							return;
						}

						//Otherwise, check if we can move up to the next POI
						if (Program.CurrentRoute.ApplyPointOfInterest(TrackDirection.Forwards))
						{
							World.UpdateAbsoluteCamera();
							if (Program.Renderer.Camera.CurrentMode < CameraViewMode.Exterior)
							{
								SaveCameraSettings();
								Program.Renderer.Camera.CurrentMode = CameraViewMode.Track;
								MessageManager.AddMessage(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"notification","track"}),
									MessageDependency.CameraView, GameMode.Expert,
									MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 2.0, null);
							}

							double z = Program.Renderer.Camera.Alignment.Position.Z;
							Program.Renderer.Camera.Alignment.Position =
								new OpenBveApi.Math.Vector3(Program.Renderer.Camera.Alignment.Position.X,
									Program.Renderer.Camera.Alignment.Position.Y, 0.0);
							Program.Renderer.Camera.Alignment.Zoom = 0.0;
							Program.Renderer.Camera.AlignmentDirection = new CameraAlignment();
							Program.Renderer.Camera.AlignmentSpeed = new CameraAlignment();
							for (int j = 0; j < TrainManager.PlayerTrain.Cars.Length; j++)
							{
								TrainManager.PlayerTrain.Cars[j].ChangeCarSection(CarSectionType.Exterior);
								TrainManager.PlayerTrain.Cars[j].FrontBogie.ChangeSection(0);
								TrainManager.PlayerTrain.Cars[j].RearBogie.ChangeSection(0);
								TrainManager.PlayerTrain.Cars[j].Coupler.ChangeSection(0);
							}

							Program.Renderer.CameraTrackFollower.UpdateRelative(z, true, false);
							Program.Renderer.Camera.Alignment.TrackPosition =
								Program.Renderer.CameraTrackFollower.TrackPosition;
							Program.Renderer.Camera.VerticalViewingAngle = Program.Renderer.Camera.OriginalVerticalViewingAngle;
							Program.Renderer.UpdateViewport(ViewportChangeMode.NoChange);
							World.UpdateAbsoluteCamera(TimeElapsed);
							Program.Renderer.UpdateViewingDistances(Program.CurrentRoute.CurrentBackground.BackgroundImageDistance);
						}

						break;
					case Translations.Command.CameraReset:
						// camera: reset
						if (Program.Renderer.Camera.CurrentMode == CameraViewMode.Interior |
						    Program.Renderer.Camera.CurrentMode == CameraViewMode.InteriorLookAhead)
						{
							Program.Renderer.Camera.Alignment.Position = new OpenBveApi.Math.Vector3(0.0, 0.0,
								0.0);
						}

						Program.Renderer.Camera.Alignment.Yaw = TrainManager.PlayerTrain.CurrentDirection == TrackDirection.Reverse ? 180 / 57.2957795130824 : 0;
						Program.Renderer.Camera.Alignment.Pitch = 0.0;
						Program.Renderer.Camera.Alignment.Roll = 0.0;
						if (Program.Renderer.Camera.CurrentMode == CameraViewMode.Track)
						{
							Program.Renderer.CameraTrackFollower.UpdateAbsolute(
								TrainManager.PlayerTrain.Cars[0].TrackPosition, true,
								false);
						}
						else if (Program.Renderer.Camera.CurrentMode == CameraViewMode.FlyBy |
						         Program.Renderer.Camera.CurrentMode == CameraViewMode.FlyByZooming)
						{
							if (TrainManager.PlayerTrain.CurrentSpeed >= 0.0)
							{
								double d = 30.0 +
								           4.0 * TrainManager.PlayerTrain.CurrentSpeed;
								Program.Renderer.CameraTrackFollower.UpdateAbsolute(
									TrainManager.PlayerTrain.Cars[0].FrontAxle.Follower
										.TrackPosition + d, true, false);
							}
							else
							{
								double d = 30.0 -
								           4.0 * TrainManager.PlayerTrain.CurrentSpeed;
								Program.Renderer.CameraTrackFollower.UpdateAbsolute(
									TrainManager.PlayerTrain.Cars[
											TrainManager.PlayerTrain.Cars.Length - 1].RearAxle.Follower
										.TrackPosition - d, true, false);
							}
						}

						Program.Renderer.Camera.Alignment.TrackPosition =
							Program.Renderer.CameraTrackFollower.TrackPosition;
						Program.Renderer.Camera.Alignment.Zoom = 0.0;
						Program.Renderer.Camera.VerticalViewingAngle = Program.Renderer.Camera.OriginalVerticalViewingAngle;
						Program.Renderer.Camera.AlignmentDirection = new CameraAlignment();
						Program.Renderer.Camera.AlignmentSpeed = new CameraAlignment();
						Program.Renderer.UpdateViewport(ViewportChangeMode.NoChange);
						World.UpdateAbsoluteCamera(TimeElapsed);
						Program.Renderer.UpdateViewingDistances(Program.CurrentRoute.CurrentBackground.BackgroundImageDistance);
						if ((Program.Renderer.Camera.CurrentMode == CameraViewMode.Interior |
						     Program.Renderer.Camera.CurrentMode == CameraViewMode.InteriorLookAhead) &
						    (Program.Renderer.Camera.CurrentRestriction == CameraRestrictionMode.On || Program.Renderer.Camera.CurrentRestriction == CameraRestrictionMode.Restricted3D))
						{
							if (!Program.Renderer.Camera.PerformRestrictionTest(TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CameraRestriction))
							{
								World.InitializeCameraRestriction();
							}
						}

						break;
					case Translations.Command.CameraRestriction:
						// camera: restriction
						switch (Program.Renderer.Camera.CurrentRestriction)
						{
							case CameraRestrictionMode.Restricted3D:
								Program.Renderer.Camera.CurrentRestriction = CameraRestrictionMode.NotAvailable;
								MessageManager.AddMessage(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"notification","camerarestriction_off"}),
									MessageDependency.CameraView, GameMode.Expert,
									MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 2.0, null);
								break;
							case CameraRestrictionMode.NotAvailable:
								Program.Renderer.Camera.CurrentRestriction = TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CameraRestrictionMode;
								if (Program.Renderer.Camera.CurrentRestriction == CameraRestrictionMode.Restricted3D)
								{
									MessageManager.AddMessage(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"notification","camerarestriction_on"}),
										MessageDependency.CameraView, GameMode.Expert,
										MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 2.0, null);
								}

								break;
							default:
								Program.Renderer.Camera.CurrentRestriction = Program.Renderer.Camera.CurrentRestriction == CameraRestrictionMode.Off ? TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].CameraRestrictionMode : CameraRestrictionMode.Off;
								World.InitializeCameraRestriction();
								if (Program.Renderer.Camera.CurrentRestriction == CameraRestrictionMode.Off)
								{
									MessageManager.AddMessage(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"notification","camerarestriction_off"}),
										MessageDependency.CameraView, GameMode.Expert,
										MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 2.0, null);
								}
								else
								{
									MessageManager.AddMessage(Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"notification","camerarestriction_on"}),
										MessageDependency.CameraView, GameMode.Expert,
										MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 2.0, null);
								}

								break;
						}

						break;
					case Translations.Command.SinglePower:
						// single power
						if (TrainManager.PlayerTrain.Handles.HandleType == HandleType.SingleHandle)
						{
							int b = TrainManager.PlayerTrain.Handles.Brake.Driver;
							if (TrainManager.PlayerTrain.Handles.EmergencyBrake.Driver)
							{
								TrainManager.PlayerTrain.Handles.EmergencyBrake.Release();
							}
							else if (b == 1 & TrainManager.PlayerTrain.Handles.HasHoldBrake)
							{
								TrainManager.PlayerTrain.Handles.Brake.ApplyState(0, false);
								TrainManager.PlayerTrain.Handles.HoldBrake.ApplyState(true);
							}
							else if (TrainManager.PlayerTrain.Handles.HoldBrake.Driver)
							{
								TrainManager.PlayerTrain.Handles.HoldBrake.ApplyState(false);
							}
							else if (b > 0)
							{
								TrainManager.PlayerTrain.Handles.Brake.ApplyState(-1, true);
							}
							else
							{
								int p = TrainManager.PlayerTrain.Handles.Power.Driver;
								if (p < TrainManager.PlayerTrain.Handles.Power.MaximumNotch)
								{
									TrainManager.PlayerTrain.Handles.Power.ApplyState(1, true);
								}
							}
						}

						TrainManager.PlayerTrain.Handles.Power.ContinuousMovement = true;
						break;
					case Translations.Command.SingleNeutral:
						// single neutral
						if (TrainManager.PlayerTrain.Handles.HandleType == HandleType.SingleHandle)
						{
							int p = TrainManager.PlayerTrain.Handles.Power.Driver;
							if (p > 0)
							{
								TrainManager.PlayerTrain.Handles.Power.ApplyState(-1, true);
								TrainManager.PlayerTrain.Handles.Power.ContinuousMovement = true;
							}
							else
							{
								int b = TrainManager.PlayerTrain.Handles.Brake.Driver;
								if (TrainManager.PlayerTrain.Handles.EmergencyBrake.Driver)
								{
									TrainManager.PlayerTrain.Handles.EmergencyBrake.Release();
								}
								else if (b == 1 & TrainManager.PlayerTrain.Handles.HasHoldBrake)
								{
									TrainManager.PlayerTrain.Handles.Brake.ApplyState(0, false);
									TrainManager.PlayerTrain.Handles.HoldBrake.ApplyState(true);
								}
								else if (TrainManager.PlayerTrain.Handles.HoldBrake.Driver)
								{
									TrainManager.PlayerTrain.Handles.HoldBrake.ApplyState(false);
								}
								else if (b > 0)
								{
									TrainManager.PlayerTrain.Handles.Brake.ApplyState(-1, true);
								}

								TrainManager.PlayerTrain.Handles.Brake.ContinuousMovement = true;
							}
						}

						break;
					case Translations.Command.SingleBrake:
						// single brake
						if (TrainManager.PlayerTrain.Handles.HandleType == HandleType.SingleHandle)
						{
							int p = TrainManager.PlayerTrain.Handles.Power.Driver;
							if (p > 0)
							{
								TrainManager.PlayerTrain.Handles.Power.ApplyState(-1, true);
							}
							else
							{
								int b = TrainManager.PlayerTrain.Handles.Brake.Driver;
								if (TrainManager.PlayerTrain.Handles.HasHoldBrake & b == 0 &
								    !TrainManager.PlayerTrain.Handles.HoldBrake.Driver)
								{
									TrainManager.PlayerTrain.Handles.HoldBrake.ApplyState(true);
								}
								else if (b < TrainManager.PlayerTrain.Handles.Brake.MaximumNotch)
								{
									TrainManager.PlayerTrain.Handles.Brake.ApplyState(1, true);
									TrainManager.PlayerTrain.Handles.HoldBrake.ApplyState(false);
								}
							}
						}

						//Set the brake handle fast movement bool at the end of the call in order to not catch it on the first movement
						TrainManager.PlayerTrain.Handles.Brake.ContinuousMovement = true;
						break;
					case Translations.Command.SingleEmergency:
						// single emergency
						if (TrainManager.PlayerTrain.Handles.HandleType == HandleType.SingleHandle)
						{
							TrainManager.PlayerTrain.Handles.EmergencyBrake.Apply();
						}

						break;
					case Translations.Command.PowerIncrease:
						// power increase
						if (TrainManager.PlayerTrain.Handles.HandleType != HandleType.SingleHandle)
						{
							int p = TrainManager.PlayerTrain.Handles.Power.Driver;
							if (p < TrainManager.PlayerTrain.Handles.Power.MaximumNotch)
							{
								TrainManager.PlayerTrain.Handles.Power.ApplyState(1, true);
							}
						}

						TrainManager.PlayerTrain.Handles.Power.ContinuousMovement = true;
						break;
					case Translations.Command.PowerDecrease:
						// power decrease
						if (TrainManager.PlayerTrain.Handles.HandleType != HandleType.SingleHandle)
						{
							int p = TrainManager.PlayerTrain.Handles.Power.Driver;
							if (p > 0)
							{
								TrainManager.PlayerTrain.Handles.Power.ApplyState(-1, true);
							}
						}

						TrainManager.PlayerTrain.Handles.Power.ContinuousMovement = true;
						break;
					case Translations.Command.BrakeIncrease:
						// brake increase
						if (TrainManager.PlayerTrain.Handles.HandleType != HandleType.SingleHandle)
						{
							if (TrainManager.PlayerTrain.Handles.Brake is AirBrakeHandle)
							{
								if (TrainManager.PlayerTrain.Handles.HasHoldBrake &
								    TrainManager.PlayerTrain.Handles.Brake.Driver ==
								    (int)AirBrakeHandleState.Release &
								    !TrainManager.PlayerTrain.Handles.HoldBrake.Driver)
								{
									TrainManager.PlayerTrain.Handles.HoldBrake.ApplyState(true);
								}
								else if (TrainManager.PlayerTrain.Handles.HoldBrake.Driver)
								{
									TrainManager.PlayerTrain.Handles.Brake.ApplyState(AirBrakeHandleState.Lap);
									TrainManager.PlayerTrain.Handles.HoldBrake.ApplyState(false);
								}
								else if (TrainManager.PlayerTrain.Handles.Brake.Driver ==
								         (int)AirBrakeHandleState.Lap)
								{
									TrainManager.PlayerTrain.Handles.Brake.ApplyState(AirBrakeHandleState.Service);
								}
								else if (TrainManager.PlayerTrain.Handles.Brake.Driver ==
								         (int)AirBrakeHandleState.Release)
								{
									TrainManager.PlayerTrain.Handles.Brake.ApplyState(AirBrakeHandleState.Lap);
								}
							}
							else
							{
								int b = TrainManager.PlayerTrain.Handles.Brake.Driver;
								if (TrainManager.PlayerTrain.Handles.HasHoldBrake & b == 0 &
								    !TrainManager.PlayerTrain.Handles.HoldBrake.Driver)
								{
									TrainManager.PlayerTrain.Handles.HoldBrake.ApplyState(true);
								}
								else if (b < TrainManager.PlayerTrain.Handles.Brake.MaximumNotch)
								{
									TrainManager.PlayerTrain.Handles.Brake.ApplyState(1, true);
									TrainManager.PlayerTrain.Handles.HoldBrake.ApplyState(false);
								}
							}
						}

						TrainManager.PlayerTrain.Handles.Brake.ContinuousMovement = true;
						break;
					case Translations.Command.BrakeDecrease:
						// brake decrease
						if (TrainManager.PlayerTrain.Handles.HandleType != HandleType.SingleHandle)
						{
							if (TrainManager.PlayerTrain.Handles.Brake is AirBrakeHandle)
							{
								if (TrainManager.PlayerTrain.Handles.EmergencyBrake.Driver)
								{
									TrainManager.PlayerTrain.Handles.EmergencyBrake.Release();
								}
								else if (TrainManager.PlayerTrain.Handles.HasHoldBrake &
								         TrainManager.PlayerTrain.Handles.Brake.Driver ==
								         (int)AirBrakeHandleState.Lap &
								         !TrainManager.PlayerTrain.Handles.HoldBrake.Driver)
								{
									TrainManager.PlayerTrain.Handles.HoldBrake.ApplyState(true);
								}
								else if (TrainManager.PlayerTrain.Handles.HoldBrake.Driver)
								{
									TrainManager.PlayerTrain.Handles.Brake.ApplyState(AirBrakeHandleState.Release);
									TrainManager.PlayerTrain.Handles.HoldBrake.ApplyState(false);
								}
								else if (TrainManager.PlayerTrain.Handles.Brake.Driver ==
								         (int)AirBrakeHandleState.Lap)
								{
									TrainManager.PlayerTrain.Handles.Brake.ApplyState(AirBrakeHandleState.Release);
								}
								else if (TrainManager.PlayerTrain.Handles.Brake.Driver ==
								         (int)AirBrakeHandleState.Service)
								{
									TrainManager.PlayerTrain.Handles.Brake.ApplyState(AirBrakeHandleState.Lap);
								}
							}
							else
							{
								int b = TrainManager.PlayerTrain.Handles.Brake.Driver;
								if (TrainManager.PlayerTrain.Handles.EmergencyBrake.Driver)
								{
									TrainManager.PlayerTrain.Handles.EmergencyBrake.Release();
								}
								else if (b == 1 & TrainManager.PlayerTrain.Handles.HasHoldBrake)
								{
									TrainManager.PlayerTrain.Handles.Brake.ApplyState(0, false);
									TrainManager.PlayerTrain.Handles.HoldBrake.ApplyState(true);
								}
								else if (TrainManager.PlayerTrain.Handles.HoldBrake.Driver)
								{
									TrainManager.PlayerTrain.Handles.HoldBrake.ApplyState(false);
								}
								else if (b > 0)
								{
									TrainManager.PlayerTrain.Handles.Brake.ApplyState(-1, true);
								}
							}
						}

						TrainManager.PlayerTrain.Handles.Brake.ContinuousMovement = true;
						break;
					case Translations.Command.LocoBrakeIncrease:
						if (TrainManager.PlayerTrain.Handles.LocoBrake is LocoAirBrakeHandle)
						{
							if (TrainManager.PlayerTrain.Handles.LocoBrake.Driver == (int)AirBrakeHandleState.Lap)
							{
								TrainManager.PlayerTrain.Handles.LocoBrake.ApplyState(AirBrakeHandleState.Service);
							}
							else if (TrainManager.PlayerTrain.Handles.LocoBrake.Driver == (int)AirBrakeHandleState.Release)
							{
								TrainManager.PlayerTrain.Handles.LocoBrake.ApplyState(AirBrakeHandleState.Lap);
							}
						}
						else
						{
							TrainManager.PlayerTrain.Handles.LocoBrake.ApplyState(1, true);
						}
						break;
					case Translations.Command.LocoBrakeDecrease:
						if (TrainManager.PlayerTrain.Handles.LocoBrake is LocoAirBrakeHandle)
						{
							if (TrainManager.PlayerTrain.Handles.LocoBrake.Driver == (int)AirBrakeHandleState.Lap)
							{
								TrainManager.PlayerTrain.Handles.LocoBrake.ApplyState(AirBrakeHandleState.Release);
							}
							else if (TrainManager.PlayerTrain.Handles.LocoBrake.Driver == (int)AirBrakeHandleState.Service)
							{
								TrainManager.PlayerTrain.Handles.LocoBrake.ApplyState(AirBrakeHandleState.Lap);
							}
						}
						else
						{
							TrainManager.PlayerTrain.Handles.LocoBrake.ApplyState(-1, true);
						}
						break;
					case Translations.Command.BrakeEmergency:
						// brake emergency
						TrainManager.PlayerTrain.Handles.EmergencyBrake.Apply();
						break;
					case Translations.Command.DeviceConstSpeed:
						// const speed
						if (TrainManager.PlayerTrain.Specs.HasConstSpeed)
						{
							TrainManager.PlayerTrain.Specs.CurrentConstSpeed =
								!TrainManager.PlayerTrain.Specs.CurrentConstSpeed;
						}

						break;
					case Translations.Command.PowerAnyNotch:
						if (TrainManager.PlayerTrain.Handles.HandleType == HandleType.SingleHandle && TrainManager.PlayerTrain.Handles.EmergencyBrake.Driver)
						{
							TrainManager.PlayerTrain.Handles.EmergencyBrake.Release();
						}

						TrainManager.PlayerTrain.Handles.Brake.ApplyState(0, TrainManager.PlayerTrain.Handles.HandleType != HandleType.SingleHandle);
						TrainManager.PlayerTrain.Handles.Power.ApplyState(Control.Option, false);
						break;
					case Translations.Command.BrakeAnyNotch:
						if (TrainManager.PlayerTrain.Handles.Brake is AirBrakeHandle)
						{
							if (TrainManager.PlayerTrain.Handles.EmergencyBrake.Driver)
							{
								TrainManager.PlayerTrain.Handles.EmergencyBrake.Release();
							}

							TrainManager.PlayerTrain.Handles.HoldBrake.ApplyState(false);
							if (Control.Option <= (int)AirBrakeHandleState.Release)
							{
								TrainManager.PlayerTrain.Handles.Brake.ApplyState(AirBrakeHandleState.Release);
							}
							else if (Control.Option == (int)AirBrakeHandleState.Lap)
							{
								TrainManager.PlayerTrain.Handles.Brake.ApplyState(AirBrakeHandleState.Lap);
							}
							else
							{
								TrainManager.PlayerTrain.Handles.Brake.ApplyState(AirBrakeHandleState.Service);
							}
						}
						else
						{
							if (TrainManager.PlayerTrain.Handles.EmergencyBrake.Driver)
							{
								TrainManager.PlayerTrain.Handles.EmergencyBrake.Release();
							}

							TrainManager.PlayerTrain.Handles.HoldBrake.ApplyState(false);
							TrainManager.PlayerTrain.Handles.Brake.ApplyState(Control.Option, false);
							TrainManager.PlayerTrain.Handles.Power.ApplyState(0, TrainManager.PlayerTrain.Handles.HandleType != HandleType.SingleHandle);
						}
						break;
					case Translations.Command.ReverserAnyPosition:
						TrainManager.PlayerTrain.Handles.Reverser.ApplyState((ReverserPosition)Control.Option);
						break;
					case Translations.Command.HoldBrake:
						if (TrainManager.PlayerTrain.Handles.HasHoldBrake && (TrainManager.PlayerTrain.Handles.Brake.Driver == 0 || TrainManager.PlayerTrain.Handles.Brake.Driver == 1) && !TrainManager.PlayerTrain.Handles.HoldBrake.Driver)
						{
							TrainManager.PlayerTrain.Handles.Brake.ApplyState(0, false);
							TrainManager.PlayerTrain.Handles.Power.ApplyState(0, TrainManager.PlayerTrain.Handles.HandleType != HandleType.SingleHandle);
							TrainManager.PlayerTrain.Handles.HoldBrake.ApplyState(true);
						}

						break;
					case Translations.Command.ReverserForward:
						if (TrainManager.PlayerTrain.Handles.Reverser.Driver < ReverserPosition.Forwards)
						{
							TrainManager.PlayerTrain.Handles.Reverser.ApplyState(1, true);
						}

						break;
					case Translations.Command.ReverserBackward:
						// reverser backward
						if (TrainManager.PlayerTrain.Handles.Reverser.Driver > ReverserPosition.Reverse)
						{
							TrainManager.PlayerTrain.Handles.Reverser.ApplyState(-1, true);
						}

						break;
					case Translations.Command.HornPrimary:
					case Translations.Command.HornSecondary:
					case Translations.Command.HornMusic:
						{
							int j = Control.Command == Translations.Command.HornPrimary
								? 0 : Control.Command == Translations.Command.HornSecondary ? 1 : 2;
							int d = TrainManager.PlayerTrain.DriverCar;
							if (TrainManager.PlayerTrain.Cars[d].Horns.Length > j)
							{
								TrainManager.PlayerTrain.Cars[d].Horns[j].Play();
								TrainManager.PlayerTrain.Plugin?.HornBlow(j == 0 ? HornTypes.Primary : j == 1 ? HornTypes.Secondary : HornTypes.Music);
							}
						}
						break;
					case Translations.Command.DoorsLeft:
						if (TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Doors[0].ButtonPressed)
						{
							return;
						}

						if ((TrainManager.PlayerTrain.GetDoorsState(true, false) &
						     TrainDoorState.Opened) == 0)
						{
							if (TrainManager.PlayerTrain.Specs.DoorOpenMode != DoorMode.Automatic)
							{
								TrainManager.PlayerTrain.OpenDoors(true, false);
							}
						}
						else
						{
							if (TrainManager.PlayerTrain.Specs.DoorCloseMode != DoorMode.Automatic)
							{
								TrainManager.PlayerTrain.CloseDoors(true, false);
							}
						}
						//Set door button to pressed in the driver's car
						TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Doors[0].ButtonPressed = true;
						break;
					case Translations.Command.DoorsRight:
						if (TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Doors[1].ButtonPressed)
						{
							return;
						}

						if ((TrainManager.PlayerTrain.GetDoorsState(false, true) &
						     TrainDoorState.Opened) == 0)
						{
							if (TrainManager.PlayerTrain.Specs.DoorOpenMode != DoorMode.Automatic)
							{
								TrainManager.PlayerTrain.OpenDoors(false, true);
							}
						}
						else
						{
							if (TrainManager.PlayerTrain.Specs.DoorCloseMode != DoorMode.Automatic)
							{
								TrainManager.PlayerTrain.CloseDoors(false, true);
							}
						}

						TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Doors[1].ButtonPressed = true;
						break;
					case Translations.Command.PlayMicSounds:
						Program.Sounds.IsPlayingMicSounds = !Program.Sounds.IsPlayingMicSounds;
						break;
					case Translations.Command.Headlights:
						TrainManager.PlayerTrain.SafetySystems.Headlights.ChangeState();
						break;
					case Translations.Command.MainBreaker:
						break;
					case Translations.Command.WiperSpeedUp:
					case Translations.Command.WiperSpeedDown:
						TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Windscreen?.Wipers.ChangeSpeed(Control.Command);
						break;
					case Translations.Command.Sanders:
						for (int c = 0; c < TrainManager.PlayerTrain.Cars.Length; c++)
						{
							if (TrainManager.PlayerTrain.Cars[c].ReAdhesionDevice is Sanders sanders)
							{
								if(sanders.Type == SandersType.PressAndHold) {
									sanders.SetActive(true);
								} else {
									sanders.Toggle();
								}
							}
						}
						break;
					case Translations.Command.UncoupleFront:
						/*
						 * Need to remember that the coupler we're talking about here is actually that on the rear of the preceeding car, i.e. IDX - 1
						 * This connects to the following car and is therefore it's front coupler
						 *
						 * However, car indexing is zero-based, so we need to *add* one to get the correct display number
						 *
						 */
						if (Program.Renderer.Camera.CurrentMode != CameraViewMode.Exterior)
						{
							MessageManager.AddMessage(
								Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"notification","switchexterior_uncouple"}),
								MessageDependency.None, GameMode.Expert,
								MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 5.0, null);
							return;
						}

						if (TrainManager.PlayerTrain.CameraCar == 0)
						{
							// Unable to uncouple front of first car
							MessageManager.AddMessage(
								Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"notification","unable_uncouple"}),
								MessageDependency.None, GameMode.Expert,
								MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 5.0, null);
							return;
						}

						if (TrainManager.PlayerTrain.CameraCar - 1 >= TrainManager.PlayerTrain.Cars.Length || !TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.CameraCar - 1].Coupler.CanUncouple)
						{
							MessageManager.AddMessage(
								Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"notification","fixed_uncouple"}),
								MessageDependency.None, GameMode.Expert,
								MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 5.0, null);
							return;
						}
						MessageManager.AddMessage(
							Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"notification","exterior_uncouplefront"}) + " " + (TrainManager.PlayerTrain.CameraCar + 1),
							MessageDependency.None, GameMode.Expert,
							MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 5.0, null);
						TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.CameraCar].Uncouple(true, false);
						break;
					case Translations.Command.UncoupleRear:
						/*
						 * At least this one is simpler, as the rear coupler is that on our camera car
						 *
						 * Still need to add one for the message to display indices however
						 */
						if (Program.Renderer.Camera.CurrentMode != CameraViewMode.Exterior)
						{
							MessageManager.AddMessage(
								Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"notification","switchexterior_uncouple"}),
								MessageDependency.None, GameMode.Expert,
								MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 5.0, null);
							return;
						}

						if (TrainManager.PlayerTrain.CameraCar == TrainManager.PlayerTrain.Cars.Length - 1)
						{
							MessageManager.AddMessage(
								Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"notification","unable_uncouple"}),
								MessageDependency.None, GameMode.Expert,
								MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 5.0, null);
							return;
						}

						if (!TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.CameraCar].Coupler.CanUncouple)
						{
							MessageManager.AddMessage(
								Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"notification","fixed_uncouple"}),
								MessageDependency.None, GameMode.Expert,
								MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 5.0, null);
							return;
						}
						MessageManager.AddMessage(
							Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"notification","exterior_uncouplerear"}) + " " + (TrainManager.PlayerTrain.CameraCar + 1),
							MessageDependency.None, GameMode.Expert,
							MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 5.0, null);
						TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.CameraCar].Uncouple(false, true);
						break;
					case Translations.Command.TimetableToggle:
						// option: timetable
						if (Interface.CurrentOptions.TimeTableStyle == TimeTableMode.None)
						{
							//We have selected not to display a timetable at all
							break;
						}

						if (Interface.CurrentOptions.TimeTableStyle == TimeTableMode.AutoGenerated || !Timetable.CustomTimetableAvailable)
						{
							//Either the auto-generated timetable has been selected as the preference, or no custom timetable has been supplied
							switch (Program.Renderer.CurrentTimetable)
							{
								case DisplayedTimetable.Default:
									Program.Renderer.CurrentTimetable = DisplayedTimetable.None;
									break;
								default:
									Program.Renderer.CurrentTimetable = DisplayedTimetable.Default;
									break;
							}

							break;
						}

						if (Interface.CurrentOptions.TimeTableStyle == TimeTableMode.PreferCustom)
						{
							//We have already determined that a custom timetable is available in the if above
							Program.Renderer.CurrentTimetable = Program.Renderer.CurrentTimetable != DisplayedTimetable.Custom ? DisplayedTimetable.Custom : DisplayedTimetable.None;
							break;
						}

						//Fallback legacy behaviour- Cycles between custom, default and none
						switch (Program.Renderer.CurrentTimetable)
						{
							case DisplayedTimetable.Custom:
								Program.Renderer.CurrentTimetable = DisplayedTimetable.Default;
								break;
							case DisplayedTimetable.Default:
								Program.Renderer.CurrentTimetable = DisplayedTimetable.None;
								break;
							default:
								Program.Renderer.CurrentTimetable = DisplayedTimetable.Custom;
								break;
						}
						break;
					case Translations.Command.DebugWireframe:
						Program.Renderer.OptionWireFrame = !Program.Renderer.OptionWireFrame;
						break;
					case Translations.Command.DebugNormals:
						// option: normals
						Program.Renderer.OptionNormals = !Program.Renderer.OptionNormals;
						break;
					case Translations.Command.DebugTouchMode:
						Program.Renderer.DebugTouchMode = !Program.Renderer.DebugTouchMode;
						break;
					case Translations.Command.ShowEvents:
						Interface.CurrentOptions.ShowEvents = !Interface.CurrentOptions.ShowEvents;
						break;
					case Translations.Command.DebugRendererMode:
						Interface.CurrentOptions.IsUseNewRenderer = !Interface.CurrentOptions.IsUseNewRenderer;
						MessageManager.AddMessage($"Renderer mode: {(Program.Renderer.AvailableNewRenderer ? "New renderer" : "Original renderer")}", MessageDependency.None, GameMode.Expert, MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 10.0, null);
						break;
					case Translations.Command.MiscAI:
						// option: AI
						if (Interface.CurrentOptions.GameMode == GameMode.Expert)
						{
							MessageManager.AddMessage(
								Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"notification","notavailableexpert"}),
								MessageDependency.None, GameMode.Expert,
								MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 5.0, null);
						}
						else
						{
							if (TrainManager.PlayerTrain.AI == null)
							{
								TrainManager.PlayerTrain.AI =
									new Game.SimpleHumanDriverAI(TrainManager.PlayerTrain, Double.PositiveInfinity);
								if (TrainManager.PlayerTrain.Plugin != null && TrainManager.PlayerTrain.Plugin.SupportsAI == AISupport.None)
								{
									MessageManager.AddMessage(
										Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"notification","aiunable"}),
										MessageDependency.None, GameMode.Expert,
										MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 10.0, null);
								}
							}
							else
							{
								TrainManager.PlayerTrain.AI = null;
							}
						}
						break;
					case Translations.Command.MiscInterface:
						// option: debug
						switch (Program.Renderer.CurrentOutputMode)
						{
							case OutputMode.Default:
								Program.Renderer.CurrentOutputMode = Interface.CurrentOptions.GameMode ==
								                                     GameMode.Expert
									? OutputMode.None
									: OutputMode.Debug;
								break;
							case OutputMode.Debug:
								Program.Renderer.CurrentOutputMode = OutputMode.None;
								break;
							case OutputMode.DebugATS:
								Program.Renderer.CurrentOutputMode = Program.Renderer.PreviousOutputMode;
								break;
							default:
								Program.Renderer.CurrentOutputMode = OutputMode.Default;
								break;
						}
						Program.Renderer.PreviousOutputMode = Program.Renderer.CurrentOutputMode;
						break;
					case Translations.Command.DebugATS:
						if (Program.Renderer.CurrentOutputMode == OutputMode.DebugATS)
						{
							Program.Renderer.CurrentOutputMode = Program.Renderer.PreviousOutputMode;
						}
						else
						{
							Program.Renderer.PreviousOutputMode = Program.Renderer.CurrentOutputMode;
							Program.Renderer.CurrentOutputMode = OutputMode.DebugATS;
						}
						break;
					case Translations.Command.MiscBackface:
						// option: backface culling
						Program.Renderer.OptionBackFaceCulling = !Program.Renderer.OptionBackFaceCulling;
						MessageManager.AddMessage(
							Translations.GetInterfaceString(HostApplication.OpenBve, Program.Renderer.OptionBackFaceCulling
								? new[] {"notification","backfaceculling_on"}
								: new[] {"notification","backfaceculling_off"}), MessageDependency.None,
							GameMode.Expert, MessageColor.White,
							Program.CurrentRoute.SecondsSinceMidnight + 2.0, null);
						break;
					case Translations.Command.MiscCPUMode:
						// option: limit frame rate
						LimitFramerate = !LimitFramerate;
						MessageManager.AddMessage(
							Translations.GetInterfaceString(HostApplication.OpenBve, LimitFramerate
								? new[] {"notification","cpu_low"}
								: new[] {"notification","cpu_normal"}), MessageDependency.None,
							GameMode.Expert, MessageColor.White,
							Program.CurrentRoute.SecondsSinceMidnight + 2.0, null);
						break;
					case Translations.Command.DebugBrakeSystems:
						// option: brake systems
						if (Interface.CurrentOptions.GameMode == GameMode.Expert)
						{
							MessageManager.AddMessage(
								Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"notification","notavailableexpert"}),
								MessageDependency.None, GameMode.Expert,
								MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 5.0, null);
						}
						else
						{
							Program.Renderer.OptionBrakeSystems = !Program.Renderer.OptionBrakeSystems;
						}
						break;
					case Translations.Command.MenuActivate:
						// menu
						Game.Menu.PushMenu(MenuType.Top);
						break;
					case Translations.Command.MiscPause:
						// pause
						Program.Renderer.CurrentInterface = InterfaceType.Pause;
						break;
					case Translations.Command.MiscClock:
						// clock
						Program.Renderer.OptionClock = !Program.Renderer.OptionClock;
						break;
					case Translations.Command.MiscTimeFactor:
						// time factor
						if (TrainManager.PlayerTrain.Plugin != null && !TrainManager.PlayerTrain.Plugin.DisableTimeAcceleration)
						{
							if (Interface.CurrentOptions.GameMode == GameMode.Expert)
							{
								MessageManager.AddMessage(
									Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"notification","notavailableexpert"}),
									MessageDependency.None, GameMode.Expert,
									MessageColor.White,
									Program.CurrentRoute.SecondsSinceMidnight + 5.0, null);
							}
							else
							{
								TimeFactor = TimeFactor == 1 ? Interface.CurrentOptions.TimeAccelerationFactor : 1;
								MessageManager.AddMessage(
									TimeFactor.ToString(
										System.Globalization.CultureInfo.InvariantCulture) + "x",
									MessageDependency.None, GameMode.Expert,
									MessageColor.White,
									Program.CurrentRoute.SecondsSinceMidnight + 5.0 * TimeFactor, null);
							}
						}
						break;
					case Translations.Command.MiscSpeed:
						// speed
						if (Interface.CurrentOptions.GameMode == GameMode.Expert)
						{
							MessageManager.AddMessage(
								Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"notification","notavailableexpert"}),
								MessageDependency.None, GameMode.Expert,
								MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 5.0, null);
						}
						else
						{
							Program.Renderer.OptionSpeed++;
							if ((int)Program.Renderer.OptionSpeed >= 3) Program.Renderer.OptionSpeed = 0;
						}
						break;
					case Translations.Command.MiscGradient:
						// gradient
						if (Interface.CurrentOptions.GameMode == GameMode.Expert)
						{
							MessageManager.AddMessage(
								Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"notification","notavailableexpert"}),
								MessageDependency.None, GameMode.Expert,
								MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 5.0, null);
						}
						else
						{
							Program.Renderer.OptionGradient++;
							if ((int)Program.Renderer.OptionGradient >= 4) Program.Renderer.OptionGradient = 0;
						}
						break;
					case Translations.Command.MiscDistNextStation:
						if (Interface.CurrentOptions.GameMode == GameMode.Expert)
						{
							MessageManager.AddMessage(
								Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"notification","notavailableexpert"}),
								MessageDependency.None, GameMode.Expert,
								MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 5.0, null);
						}
						else
						{
							Program.Renderer.OptionDistanceToNextStation++;
							if ((int)Program.Renderer.OptionDistanceToNextStation >= 3) Program.Renderer.OptionDistanceToNextStation = 0;
						}
						break;
					case Translations.Command.MiscFps:
						// fps
						Program.Renderer.OptionFrameRates = !Program.Renderer.OptionFrameRates;
						break;
					case Translations.Command.MiscFullscreen:
						// toggle fullscreen
						Screen.ToggleFullscreen();
						Control.AnalogState = 0.0;
						Control.DigitalState = DigitalControlState.Released;
						break;
					case Translations.Command.MiscMute:
						// mute
						Program.Sounds.GlobalMute = !Program.Sounds.GlobalMute;
						Program.Sounds.Update(TimeElapsed, Interface.CurrentOptions.SoundModel);
						break;
					case Translations.Command.RouteInformation:
						Game.RouteInfoOverlay.ProcessCommand(Translations.Command.RouteInformation);
						break;
					case Translations.Command.AccessibilityCurrentSpeed:
						string s = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"message","train_currentspeed"}).Replace("[speed]", $"{TrainManagerBase.PlayerTrain.CurrentSpeed * 3.6:0.0}") + "km/h";
						Program.CurrentHost.AddMessage(s, MessageDependency.AccessibilityHelper, GameMode.Normal, MessageColor.White, Program.CurrentHost.InGameTime + 10.0, null);
						break;
					case Translations.Command.AccessibilityNextSignal:
						Section nextSection = TrainManagerBase.CurrentRoute.NextSection(TrainManagerBase.PlayerTrain.FrontCarTrackPosition);
						if (nextSection != null)
						{
							double tPos = nextSection.TrackPosition - TrainManagerBase.PlayerTrain.FrontCarTrackPosition;
							string st = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"message","route_nextsection_aspect"}).Replace("[distance]", $"{tPos:0.0}") + "m".Replace("[aspect]", nextSection.CurrentAspect.ToString());
							Program.CurrentHost.AddMessage(st, MessageDependency.AccessibilityHelper, GameMode.Normal, MessageColor.White, Program.CurrentHost.InGameTime + 10.0, null);
						}
						break;
					case Translations.Command.AccessibilityNextStation:
						RouteStation nextStation = TrainManagerBase.CurrentRoute.NextStation(TrainManagerBase.PlayerTrain.FrontCarTrackPosition);
						if (nextStation != null)
						{
							//If we find an appropriate signal, and the distance to it is less than 500m, announce if screen reader is present
							//Aspect announce to be triggered via a separate keybind
							double tPos = nextStation.DefaultTrackPosition - TrainManagerBase.PlayerTrain.FrontCarTrackPosition;
							string stt = Translations.GetInterfaceString(HostApplication.OpenBve, new[] {"message","route_nextstation"}).Replace("[distance]", $"{tPos:0.0}") + "m".Replace("[name]", nextStation.Name);
							Program.CurrentHost.AddMessage(stt, MessageDependency.AccessibilityHelper, GameMode.Normal, MessageColor.White, Program.CurrentHost.InGameTime + 10.0, null);
							nextStation.AccessibilityAnnounced = true;
						}
						break;
					case Translations.Command.SwitchMenu:
						switch (Program.Renderer.CurrentInterface)
						{
							case InterfaceType.Normal:
								Program.Renderer.CurrentInterface = InterfaceType.SwitchChangeMap;
								Game.SwitchChangeDialog.Show();
								break;
							case InterfaceType.SwitchChangeMap:
								Game.SwitchChangeDialog.Close(null, null);
								break;
						}
						break;
					case Translations.Command.AccessPermissiveSection:
						TrainManager.PlayerTrain.ContactSignaller();
						break;
				}
			}
			else if (Control.DigitalState == DigitalControlState.Released)
			{
				// released
				Control.DigitalState =
					DigitalControlState.ReleasedAcknowledged;
				TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].DSD?.ControlUp(Control.Command);

				if (Translations.SecurityToVirtualKey(Control.Command, out VirtualKeys key))
				{
					TrainManager.PlayerTrain.Plugin?.KeyUp(key);
				}
				switch (Control.Command)
				{
					case Translations.Command.SingleBrake:
					case Translations.Command.BrakeIncrease:
					case Translations.Command.BrakeDecrease:
						TrainManager.PlayerTrain.Handles.Brake.ContinuousMovement = false;
						break;
					case Translations.Command.SinglePower:
					case Translations.Command.PowerIncrease:
					case Translations.Command.PowerDecrease:
						TrainManager.PlayerTrain.Handles.Power.ContinuousMovement = false;
						break;
					case Translations.Command.SingleNeutral:
						TrainManager.PlayerTrain.Handles.Brake.ContinuousMovement = false;
						TrainManager.PlayerTrain.Handles.Power.ContinuousMovement = false;
						break;
					case Translations.Command.HornPrimary:
					case Translations.Command.HornSecondary:
					case Translations.Command.HornMusic:
						// horn
						int j = Control.Command == Translations.Command.HornPrimary
							? 0
							: Control.Command == Translations.Command.HornSecondary
								? 1
								: 2;
						int d = TrainManager.PlayerTrain.DriverCar;
						if (TrainManager.PlayerTrain.Cars[d].Horns.Length > j)
						{
							//Required for detecting the end of the loop and triggering the stop sound
							TrainManager.PlayerTrain.Cars[d].Horns[j].Stop();
						}
						break;
					case Translations.Command.DoorsLeft:
						TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Doors[0].ButtonPressed = false;
						break;
					case Translations.Command.DoorsRight:
						TrainManager.PlayerTrain.Cars[TrainManager.PlayerTrain.DriverCar].Doors[1].ButtonPressed = false;
						break;
					case Translations.Command.RailDriverSpeedUnits:
						Interface.CurrentOptions.RailDriverMPH = !Interface.CurrentOptions.RailDriverMPH;
						break;
					case Translations.Command.Sanders:
						for (int c = 0; c < TrainManager.PlayerTrain.Cars.Length; c++)
						{
							if (TrainManager.PlayerTrain.Cars[c].ReAdhesionDevice is Sanders sanders)
							{
								if (sanders.Type == SandersType.PressAndHold)
								{
									sanders.SetActive(false);
								} else {
									sanders.Toggle();
								}
							}
						}
						break;
				}
			}
		}
	}
}
