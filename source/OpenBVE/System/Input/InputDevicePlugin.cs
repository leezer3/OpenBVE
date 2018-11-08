using OpenBveApi.Interface;

namespace OpenBve
{
	internal static partial class MainLoop
	{
		internal static void InputDevicePluginKeyDown(object sender, InputEventArgs e)
		{
			for (int i = 0; i < Interface.CurrentControls.Length; i++)
			{
				if (Interface.CurrentControls[i].Method != Interface.ControlMethod.InputDevicePlugin)
				{
					continue;
				}
				bool enableOption = false;
				for (int j = 0; j < Translations.CommandInfos.Length; j++)
				{
					if (Interface.CurrentControls[i].Command == Translations.CommandInfos[j].Command)
					{
						enableOption = Translations.CommandInfos[j].EnableOption;
						break;
					}
				}
				if (e.Control.Command == Interface.CurrentControls[i].Command)
				{
					if (enableOption && e.Control.Option != Interface.CurrentControls[i].Option)
					{
						continue;
					}
					Interface.CurrentControls[i].AnalogState = 1.0;
					Interface.CurrentControls[i].DigitalState = Interface.DigitalControlState.Pressed;
					AddControlRepeat(i);
				}
			}
		}

		internal static void InputDevicePluginKeyUp(object sender, InputEventArgs e)
		{
			for (int i = 0; i < Interface.CurrentControls.Length; i++)
			{
				if (Interface.CurrentControls[i].Method != Interface.ControlMethod.InputDevicePlugin)
				{
					continue;
				}
				bool enableOption = false;
				for (int j = 0; j < Translations.CommandInfos.Length; j++)
				{
					if (Interface.CurrentControls[i].Command == Translations.CommandInfos[j].Command)
					{
						enableOption = Translations.CommandInfos[j].EnableOption;
						break;
					}
				}
				if (e.Control.Command == Interface.CurrentControls[i].Command)
				{
					if (enableOption && e.Control.Option != Interface.CurrentControls[i].Option)
					{
						continue;
					}
					Interface.CurrentControls[i].AnalogState = 0.0;
					Interface.CurrentControls[i].DigitalState = Interface.DigitalControlState.Released;
					RemoveControlRepeat(i);
				}
			}
		}
	}
}
