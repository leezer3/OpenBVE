namespace OpenBveApi.Runtime
{
	/// <summary>Represents to which extent the plugin supports the AI.</summary>
	public enum AISupport
	{
		/// <summary>The plugin does not support the AI. Calls to PerformAI will not be made. Non-player trains will not use the plugin.</summary>
		None = 0,
		/// <summary>The plugin complements the built-in AI by performing only functions specific to the plugin.</summary>
		Basic = 1,
		/// <summary>The host program contains code to perform the AI functionality for this plugin</summary>
		Program = 2
	}
}
