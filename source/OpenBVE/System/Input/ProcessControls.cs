using System;
using System.Linq;
using LibRender2.Cameras;
using LibRender2.Screens;
using LibRender2.Trains;
using LibRender2.Viewports;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Graphics;
using OpenBveApi.Runtime;
using OpenBveApi.Interface;
using RouteManager2.MessageManager;
using TrainManager;

namespace OpenBve
{
	internal static partial class MainLoop
	{
		/// <summary>The ProcessControls function should be called once a frame, and updates the simulation accordingly</summary>
		/// <param name="TimeElapsed">The time elapsed in ms since the last call to this function</param>
		internal static void ProcessControls(double TimeElapsed)
		{
			for (int i = 0; i < Program.Joysticks.AttachedJoysticks.Count; i++)
			{
				/*
				 * Prequisite checks:
				 * Is our joystick connected?
				 * Have we already detected the disconnection?
				 */
				Guid guid = Program.Joysticks.AttachedJoysticks.ElementAt(i).Key;
				if (!Program.Joysticks.AttachedJoysticks[guid].IsConnected() && Program.Joysticks.AttachedJoysticks[guid].Disconnected == false)
				{
					Program.Joysticks.AttachedJoysticks[guid].Disconnected = true;
					for (int j = 0; j < Interface.CurrentControls.Length; j++)
					{
						if (Interface.CurrentControls[j].Method == ControlMethod.Joystick && Interface.CurrentControls[i].Device == guid)
						{
							//This control is bound to our disconnected joystick, so let's kick into pause mode
							Program.Renderer.CurrentInterface = InterfaceType.Pause;
						}
					}
				}
				else if (Program.Joysticks.AttachedJoysticks[guid].IsConnected() && Program.Joysticks.AttachedJoysticks[guid].Disconnected)
				{
					//Reconnected, so kick out of pause mode
					Program.Renderer.CurrentInterface = InterfaceType.Normal;
					Program.Joysticks.AttachedJoysticks[guid].Disconnected = false;
				}

			}
			if (Interface.CurrentOptions.KioskMode)
			{
				kioskModeTimer += TimeElapsed;
				if (kioskModeTimer > Interface.CurrentOptions.KioskModeTimer && TrainManager.PlayerTrain.AI == null)
				{
					/*
					 * We are in kiosk mode, and the timer has expired (NOTE: The AI null check saves us time)
					 *
					 * Therefore:
					 * ==> Switch back to the interior view of the driver's car
					 * ==> Enable AI
					 */
					TrainManager.PlayerTrain.CameraCar = TrainManager.PlayerTrain.DriverCar;
					MainLoop.SaveCameraSettings();
					bool lookahead = false;
					if (Program.Renderer.Camera.CurrentMode != CameraViewMode.InteriorLookAhead & (Program.Renderer.Camera.CurrentRestriction == CameraRestrictionMode.NotAvailable || Program.Renderer.Camera.CurrentRestriction == CameraRestrictionMode.Restricted3D))
					{
						MessageManager.AddMessage(Translations.GetInterfaceString("notification_interior_lookahead"),
							MessageDependency.CameraView, GameMode.Expert,
							MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 2.0, null);
						lookahead = true;
					}
					else
					{
						MessageManager.AddMessage(Translations.GetInterfaceString("notification_interior"),
							MessageDependency.CameraView, GameMode.Expert,
							MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 2.0, null);
					}

					Program.Renderer.Camera.CurrentMode = CameraViewMode.Interior;
					MainLoop.RestoreCameraSettings();
					bool returnToCab = false;
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
					TrainManager.PlayerTrain.AI = new Game.SimpleHumanDriverAI(TrainManager.PlayerTrain, Double.PositiveInfinity);
					if (TrainManager.PlayerTrain.Plugin != null && TrainManager.PlayerTrain.Plugin.SupportsAI == AISupport.None)
					{
						MessageManager.AddMessage(Translations.GetInterfaceString("notification_aiunable"), MessageDependency.None, GameMode.Expert, MessageColor.White, Program.CurrentRoute.SecondsSinceMidnight + 10.0, null);
					}

				}
			}
			//If we are currently blocking key repeat events from firing, return
			if (BlockKeyRepeat) return;
			if (Program.Renderer.CurrentInterface == InterfaceType.Normal)
			{
				// IRawInput allows train plugins to take control of the keyboard
				// However, if in pause / menu, this could lockup & the train may be null in the GL3 menu
				if (TrainManagerBase.PlayerTrain.Plugin != null && TrainManagerBase.PlayerTrain.Plugin.BlockingInput) return;
			}
			
			switch (Program.Renderer.CurrentInterface)
			{
				case InterfaceType.Pause:
					// pause
					kioskModeTimer = 0;
					for (int i = 0; i < Interface.CurrentControls.Length; i++)
					{
						if (Interface.CurrentControls[i].InheritedType == Translations.CommandType.Digital)
						{
							if (Interface.CurrentControls[i].DigitalState == DigitalControlState.Pressed)
							{
								Interface.CurrentControls[i].DigitalState =
									DigitalControlState.PressedAcknowledged;
								switch (Interface.CurrentControls[i].Command)
								{
									case Translations.Command.MiscPause:
										Program.Renderer.CurrentInterface = InterfaceType.Normal;
										break;
									case Translations.Command.MenuActivate:
										Game.Menu.PushMenu(MenuType.Top);
										break;
									case Translations.Command.MiscQuit:
										Game.Menu.PushMenu(MenuType.Quit);
										break;
									case Translations.Command.MiscFullscreen:
										Screen.ToggleFullscreen();
										Interface.CurrentControls[i].AnalogState = 0.0;
										Interface.CurrentControls[i].DigitalState = DigitalControlState.Released;
										RemoveControlRepeat(i);
										break;
									case Translations.Command.MiscMute:
										Program.Sounds.GlobalMute = !Program.Sounds.GlobalMute;
										Program.Sounds.Update(TimeElapsed, Interface.CurrentOptions.SoundModel);
										break;
									case Translations.Command.SwitchMenu:
										Program.Renderer.CurrentInterface = InterfaceType.SwitchChangeMap;
										Game.switchChangeDialog.Show();
										break;
								}
							}
						}
					}
					break;
/*
				case InterfaceType.CustomiseControl:
					break;
*/
				case InterfaceType.Menu:			// MENU
				case InterfaceType.GLMainMenu:
					kioskModeTimer = 0;
					for (int i = 0; i < Interface.CurrentControls.Length; i++)
					{
						if (Interface.CurrentControls[i].InheritedType == Translations.CommandType.Digital
								&& Interface.CurrentControls[i].DigitalState == DigitalControlState.Pressed)
						{
							Interface.CurrentControls[i].DigitalState =
									DigitalControlState.PressedAcknowledged;
							Game.Menu.ProcessCommand(Interface.CurrentControls[i].Command, TimeElapsed);
						}
					}
					break;

				case InterfaceType.Normal:
					// normal
					for (int i = 0; i < Interface.CurrentControls.Length; i++)
					{
						if (Interface.CurrentControls[i].InheritedType == Translations.CommandType.AnalogHalf |
							Interface.CurrentControls[i].InheritedType == Translations.CommandType.AnalogFull)
						{
							ProcessAnalogControl(TimeElapsed, ref Interface.CurrentControls[i]);
						}
						else if (Interface.CurrentControls[i].InheritedType == Translations.CommandType.Digital)
						{
							ProcessDigitalControl(TimeElapsed, ref Interface.CurrentControls[i]);
						}
					}
					break;
			}
		}

	}
}
