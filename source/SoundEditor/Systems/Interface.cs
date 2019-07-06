using System.Collections.ObjectModel;
using OpenBveApi.Interface;

namespace SoundEditor.Systems
{
	internal static partial class Interface
	{
		internal static ObservableCollection<LogMessage> LogMessages = new ObservableCollection<LogMessage>();

		internal static void AddMessage(MessageType Type, bool FileNotFound, string Text)
		{
			LogMessages.Add(new LogMessage(Type, FileNotFound, Text));
		}
	}
}
