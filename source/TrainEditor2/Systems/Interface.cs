using System.Collections.ObjectModel;
using OpenBveApi.Interface;

namespace TrainEditor2.Systems
{
	internal static partial class Interface
	{
		internal static ObservableCollection<LogMessage> LogMessages = new ObservableCollection<LogMessage>();

		internal static void AddMessage(MessageType messageType, bool fileNotFound, string messageText)
		{
			LogMessages.Add(new LogMessage(messageType, fileNotFound, messageText));
		}
	}
}
