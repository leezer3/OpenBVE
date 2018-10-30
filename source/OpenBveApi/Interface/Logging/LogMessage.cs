namespace OpenBveApi.Interface
{
	/// <summary>A logged textual message</summary>
	public class LogMessage
	{
		/// <summary>The message type</summary>
		public readonly MessageType Type;
		/// <summary>Whether this message pertains to a missing file</summary>
		public readonly bool FileNotFound;
		/// <summary>The message text</summary>
		public readonly string Text;

		/// <summary>Creates a new logged textual message</summary>
		public LogMessage(MessageType type, bool fileNotFound, string text)
		{
			Type = type;
			FileNotFound = fileNotFound;
			Text = text;
		}
	}
}
