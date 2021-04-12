using OpenBveApi.Interface;

namespace TrainManager.Trains
{
	/// <summary>A command entry activated by a touch sensitive panel element</summary>
	public class CommandEntry
	{
		/// <summary>The command to perform</summary>
		public Translations.Command Command;
		/// <summary>The option to supply to the command</summary>
		public int Option;
	}
}
