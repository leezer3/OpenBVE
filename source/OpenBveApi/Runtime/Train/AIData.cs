namespace OpenBveApi.Runtime
{
	/// <summary>Represents AI data.</summary>
	public class AIData
	{
		/// <summary>The driver handles.</summary>
		// ReSharper disable once FieldCanBeMadeReadOnly.Local - Modifiable by plugins
		private Handles MyHandles;

		/// <summary>The AI response.</summary>
		private AIResponse MyResponse;

		/// <summary>The elapsed time</summary>
		public readonly double TimeElapsed;

		/// <summary>Creates a new instance of this class.</summary>
		/// <param name="handles">The driver handles.</param>
		public AIData(Handles handles)
		{
			this.MyHandles = handles;
			this.MyResponse = AIResponse.None;
			this.TimeElapsed = 0;
		}

		/// <summary>Creates a new instance of this class.</summary>
		/// <param name="handles">The driver handles.</param>
		/// <param name="timeElapsed">The elapsed time</param>
		public AIData(Handles handles, double timeElapsed)
		{
			this.MyHandles = handles;
			this.MyResponse = AIResponse.None;
			this.TimeElapsed = timeElapsed;
		}

		/// <summary>Gets or sets the driver handles.</summary>
		public Handles Handles => MyHandles;

		/// <summary>Gets or sets the AI response.</summary>
		public AIResponse Response
		{
			get => MyResponse;
			set => MyResponse = value;
		}
	}
}
