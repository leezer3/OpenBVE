namespace OpenBve {
	/// <summary>Provides functions for dealing with joysticks.</summary>
	internal static class Joysticks {
		
		// --- structures ---
		
		/// <summary>Represents a joystick.</summary>
		internal struct Joystick {
			// --- members ---
			/// <summary>The textual representation of the joystick.</summary>
			internal string Name;
			/// <summary>The handle to the joystick.</summary>
			internal int OpenTKHandle;

			// --- constructors ---
			/// <summary>Creates a new joystick.</summary>
			/// <param name="name">The textual representation of the joystick.</param>
			/// <param name="openTKHandle">The openTK handle to the joystick.</param>
			internal Joystick(string name, int openTKHandle) {
				this.Name = name;
				this.OpenTKHandle = openTKHandle;
			}
		}
		
		
		// --- members ---
		
		/// <summary>Holds all joysticks currently attached to the computer.</summary>
		internal static Joystick[] AttachedJoysticks = new Joystick[] { };

		private static string lastDescription;
		
		// --- functions ---

		/// <returns>Call this function to refresh the list of available joysticks and thier capabilities</returns>
		internal static void RefreshJoysticks()
		{

			int j = 0;
			for (int i = 0; i < 10; i++)
			{
				//This *only* instanciates the first joystick, as OpenTK seems to loop
				//the first joystick into any unused slots, and doesn't provide a name
				//we can test against to see if it's the same
				//Must be fixable, need to think on it....
				var state = OpenTK.Input.Joystick.GetState(i);
				var description = OpenTK.Input.Joystick.GetCapabilities(i).ToString();
				if (description == "{Axes: 0; Buttons: 0; Hats: 0; IsConnected: True}")
				{
					break;
				}
				lastDescription = description;
				if (state.IsConnected)
				{
					j++;
				}
			}
			for (int i = 0; i < j; i++)
			{
				AttachedJoysticks = new Joystick[j];
				var state = OpenTK.Input.Joystick.GetState(i);
				if (state.IsConnected)
				{
					Joystick newJoystick = new Joystick
					{
						Name = "Joystick" + i,
						OpenTKHandle = i,
					};
					AttachedJoysticks[i] = newJoystick;

				}

			}
		}
		
	}
}