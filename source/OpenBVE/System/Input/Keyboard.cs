using System;
using LibRender2.Menu;
using LibRender2.Screens;
using OpenTK.Input;
using OpenBveApi.Interface;

namespace OpenBve
{
	internal static partial class MainLoop
	{
		/// <summary>Called when a KeyDown event is generated</summary>
		internal static void KeyDownEvent(object sender, KeyboardKeyEventArgs e)
		{
			switch (e.Key)
			{
				case Key.ShiftLeft:
				case Key.ShiftRight:
					PushHeldModifier(KeyboardModifier.Shift);
					break;
				case Key.ControlLeft:
				case Key.ControlRight:
					PushHeldModifier(KeyboardModifier.Ctrl);
					break;
				case Key.AltLeft:
				case Key.AltRight:
					PushHeldModifier(KeyboardModifier.Alt);
					break;
			}
			if (Interface.CurrentOptions.KioskMode && Program.Renderer.CurrentInterface != InterfaceType.GLMainMenu)
			{
				//If in kiosk mode, reset the timer and disable AI on keypress
				kioskModeTimer = 0;
				TrainManager.PlayerTrain.AI = null;
			}
			if (Loading.Complete && e.Key == Key.F4 && e.Alt)
			{
				// Catch standard ALT + F4 quit and push confirmation prompt
				Game.Menu.PushMenu(MenuType.Quit);
				return;
			}

			if (TrainManager.PlayerTrain?.Plugin != null)
			{
				TrainManager.PlayerTrain.Plugin.RawKeyDown((OpenBveApi.Input.Key)e.Key);
			}

			BlockKeyRepeat = true;
			//Check for modifiers
			if (e.Shift) CurrentKeyboardModifier |= KeyboardModifier.Shift;
			if (e.Control) CurrentKeyboardModifier |= KeyboardModifier.Ctrl;
			if (e.Alt) CurrentKeyboardModifier |= KeyboardModifier.Alt;
			HeldKeyboardModifiers |= CurrentKeyboardModifier;
			int capturedModifierOrder = ModifierOrder.GetRank(HeldModifierSequence, HeldModifierSequenceCount, CurrentKeyboardModifier);
			if (Program.Renderer.CurrentInterface >= InterfaceType.Menu && Game.Menu.IsCustomizingControl())
			{
				Game.Menu.SetControlKbdCustomData((OpenBveApi.Input.Key)e.Key, CurrentKeyboardModifier, capturedModifierOrder);
				return;
			}
			//Traverse the controls array
			for (int i = 0; i < Interface.CurrentControls.Length; i++)
			{
				//If we're using keyboard for this input
				if (Interface.CurrentControls[i].Method != ControlMethod.Keyboard) continue;
				//Compare the current and previous keyboard states
				//Only process if they are different
				if (!Enum.IsDefined(typeof(OpenBveApi.Input.Key), Interface.CurrentControls[i].Key)) continue;
				if ((OpenBveApi.Input.Key)e.Key == Interface.CurrentControls[i].Key && Interface.CurrentControls[i].Modifier == CurrentKeyboardModifier && (Interface.CurrentControls[i].ModifierOrder == 0 || Interface.CurrentControls[i].ModifierOrder == capturedModifierOrder))
				{

					Interface.CurrentControls[i].AnalogState = 1.0;
					Interface.CurrentControls[i].DigitalState = DigitalControlState.Pressed;
					//Key repeats should not be added in non-game interface modes, unless they are Menu Up/ Menu Down commands
					if (Program.Renderer.CurrentInterface == InterfaceType.Normal || Interface.CurrentControls[i].Command == Translations.Command.MenuUp || Interface.CurrentControls[i].Command == Translations.Command.MenuDown)
					{
						if (Interface.CurrentControls[i].Command == Translations.Command.CameraInterior |
						    Interface.CurrentControls[i].Command == Translations.Command.CameraExterior |
						    Interface.CurrentControls[i].Command == Translations.Command.CameraFlyBy |
						    Interface.CurrentControls[i].Command == Translations.Command.CameraTrack |
						    Interface.CurrentControls[i].Command == Translations.Command.MiscFullscreen)
						{
							//HACK: We don't want to bounce between camera modes when holding down the mode switch key
							continue;
						}
						AddControlRepeat(i);
					}
				}
			}
			
			if (TrainManager.PlayerTrain != null)
			{
				// Attempt to reset handle spring
				TrainManager.PlayerTrain.Handles.Power.ResetSpring();
				TrainManager.PlayerTrain.Handles.Brake.ResetSpring();
			}
			
			
			BlockKeyRepeat = false;
			//Remember to reset the keyboard modifier after we're done, else it repeats.....
			CurrentKeyboardModifier = KeyboardModifier.None;
		}

		/// <summary>Called when a KeyUp event is generated</summary>
		internal static void KeyUpEvent(object sender, KeyboardKeyEventArgs e)
		{
			if (Interface.CurrentOptions.KioskMode && Program.Renderer.CurrentInterface != InterfaceType.GLMainMenu)
			{
				//If in kiosk mode, reset the timer and disable AI on keypress
				kioskModeTimer = 0;
				TrainManager.PlayerTrain.AI = null;
			}
			if (Program.Renderer.PreviousInterface >= InterfaceType.Menu & Program.Renderer.CurrentInterface == InterfaceType.Normal)
			{
				//Set again to block the first keyup event after the menu has been closed, as this may produce unwanted effects
				//if the menu select key is also mapped in-game
				Program.Renderer.CurrentInterface = InterfaceType.Normal;
				return;
			}

			if (TrainManager.PlayerTrain?.Plugin != null)
			{
				TrainManager.PlayerTrain.Plugin.RawKeyUp((OpenBveApi.Input.Key)e.Key);
			}
			//Track held modifiers so mouse bindings requiring them can be matched
			BlockKeyRepeat = true;
			switch (e.Key)
			{
				case Key.ShiftLeft:
				case Key.ShiftRight:
					HeldKeyboardModifiers &= ~KeyboardModifier.Shift;
					RemoveHeldModifier(KeyboardModifier.Shift);
					break;
				case Key.ControlLeft:
				case Key.ControlRight:
					HeldKeyboardModifiers &= ~KeyboardModifier.Ctrl;
					RemoveHeldModifier(KeyboardModifier.Ctrl);
					break;
				case Key.AltLeft:
				case Key.AltRight:
					HeldKeyboardModifiers &= ~KeyboardModifier.Alt;
					RemoveHeldModifier(KeyboardModifier.Alt);
					break;
			}
			//Traverse the controls array
			for (int i = 0; i < Interface.CurrentControls.Length; i++)
			{
				//If we're using keyboard for this input
				if (Interface.CurrentControls[i].Method == ControlMethod.Keyboard)
				{
					//Compare the current and previous keyboard states
					//Only process if they are different
					if (!Enum.IsDefined(typeof(OpenBveApi.Input.Key), Interface.CurrentControls[i].Key)) continue;
					if ((OpenBveApi.Input.Key)e.Key == Interface.CurrentControls[i].Key && Interface.CurrentControls[i].AnalogState == 1.0 && Interface.CurrentControls[i].DigitalState > DigitalControlState.Released)
					{
						Interface.CurrentControls[i].AnalogState = 0.0;
						Interface.CurrentControls[i].DigitalState = DigitalControlState.Released;
						RemoveControlRepeat(i);
					}
				}
			}

			// Attempt to reset handle spring
			if (TrainManager.PlayerTrain != null)
			{
				TrainManager.PlayerTrain.Handles.Power.ResetSpring();
				TrainManager.PlayerTrain.Handles.Brake.ResetSpring();
			}
			BlockKeyRepeat = false;
		}
	}
}
