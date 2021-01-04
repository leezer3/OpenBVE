using OpenBveApi.Interface;

namespace OpenBve
{
	/// <summary>The TrainManager is the root class containing functions to load and manage trains within the simulation world.</summary>
	public partial class TrainManager
	{
		internal class CommandEntry
		{
			internal Translations.Command Command;
			internal int Option;
		}
	}
}
