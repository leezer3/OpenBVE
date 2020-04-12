namespace OpenBveApi.Trains
{
	/// <summary>An abstract class representing a general purpose AI</summary>
	public abstract class GeneralAI
	{
		/// <summary>Triggers the AI processing</summary>
		/// <param name="TimeElapsed">The time elapsed since the previous call to the AI</param>
		public abstract void Trigger(double TimeElapsed);
	}
}
