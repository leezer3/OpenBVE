using LibRender2.Primitives;
using OpenBveApi.Colors;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Textures;

namespace OpenBve
{
	public sealed partial class Menu
	{
		private static readonly Picturebox controlPictureBox = new Picturebox(Program.Renderer);
		private static readonly Textbox controlTextBox = new Textbox(Program.Renderer, Program.Renderer.Fonts.NormalFont, Color128.White, Color128.Black);

		/// <summary>Builds the description string for a control</summary>
		private static string GetControlDescription(int idx)
		{
			// get code name and description
			Control loadedControl = Interface.CurrentControls[idx];
			string str = "";
			switch (loadedControl.Method)
			{
				case ControlMethod.Keyboard:
					string keyName = loadedControl.Key.ToString();
					if (Translations.TranslatedKeys.ContainsKey(loadedControl.Key))
					{
						keyName = Translations.TranslatedKeys[loadedControl.Key].Description;
					}

					if (loadedControl.Modifier != KeyboardModifier.None)
					{
						str = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "menu", "keyboard" }) + " [" + loadedControl.Modifier + "-" + keyName + "]";
					}
					else
					{
						str = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "menu", "keyboard" }) + " [" + keyName + "]";
					}
					break;
				case ControlMethod.Joystick:
					str = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "menu", "joystick" }) + " [" + loadedControl.Component + " " + loadedControl.Element + "]";
					switch (loadedControl.Component)
					{
						case JoystickComponent.FullAxis:
						case JoystickComponent.Axis:
							str += " " + (loadedControl.Direction == 1 ? Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "menu", "joystickdirection_positive" }) : Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "menu", "joystickdirection_negative" }));
							break;
//					case Interface.JoystickComponent.Button:	// NOTHING TO DO FOR THIS CASE!
//						str = str;
//						break;
					case JoystickComponent.Hat:
						str += " " + (OpenTK.Input.HatPosition)loadedControl.Direction;
						break;
					case JoystickComponent.Invalid:
						str = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "menu", "joystick_notavailable" });
						break;
					}

					break;
				case ControlMethod.RailDriver:
					str = "RailDriver [" + loadedControl.Component + " " + loadedControl.Element + "]";
					switch (loadedControl.Component)
					{
						case JoystickComponent.FullAxis:
						case JoystickComponent.Axis:
							str += " " + (loadedControl.Direction == 1 ? Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "menu", "joystickdirection_positive" }) : Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "menu", "joystickdirection_negative" }));
							break;
						case JoystickComponent.Invalid:
							str = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "menu", "joystick_notavailable" });
							break;
					}

					break;
				case ControlMethod.Invalid:
					str = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "menu", "joystick_notavailable" });
					break;
			}

			return str;
		}
	}
}
