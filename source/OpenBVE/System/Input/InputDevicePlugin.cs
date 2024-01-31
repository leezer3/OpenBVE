using OpenBveApi.Interface;

namespace OpenBve
{
	internal static partial class MainLoop
	{
		internal static void InputDevicePluginKeyDown(object sender, InputEventArgs e)
		{
			for (int i = 0; i < Interface.CurrentControls.Length; i++)
			{
				if (Interface.CurrentControls[i].Method != ControlMethod.InputDevicePlugin || Interface.CurrentControls[i].Command == Translations.Command.None)
				{
					continue;
				}

				bool enableOption = Translations.CommandInfos[Interface.CurrentControls[i].Command].EnableOption;
				if (e.Control.Command == Interface.CurrentControls[i].Command)
				{
					if (enableOption && e.Control.Option != Interface.CurrentControls[i].Option)
					{
						continue;
					}
					Interface.CurrentControls[i].AnalogState = 1.0;
					Interface.CurrentControls[i].DigitalState = DigitalControlState.Pressed;
					AddControlRepeat(i);
				}
			}
		}

		internal static void InputDevicePluginKeyUp(object sender, InputEventArgs e)
		{
			for (int i = 0; i < Interface.CurrentControls.Length; i++)
			{
				if (Interface.CurrentControls[i].Method != ControlMethod.InputDevicePlugin || Interface.CurrentControls[i].Command == Translations.Command.None)
				{
					continue;
				}
				bool enableOption = Translations.CommandInfos[Interface.CurrentControls[i].Command].EnableOption;
				if (e.Control.Command == Interface.CurrentControls[i].Command)
				{
					if (enableOption && e.Control.Option != Interface.CurrentControls[i].Option)
					{
						continue;
					}
					Interface.CurrentControls[i].AnalogState = 0.0;
					Interface.CurrentControls[i].DigitalState = DigitalControlState.Released;
					RemoveControlRepeat(i);
				}
			}
		}
	}
}
