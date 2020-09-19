namespace OpenBveApi.Runtime
{
	/// <summary>Represents AI data.</summary>
	public class AIData
	{
		// --- members ---
		/// <summary>The driver handles.</summary>
		private Handles MyHandles;

		/// <summary>The AI response.</summary>
		private AIResponse MyResponse;

		// --- constructors ---
		/// <summary>Creates a new instance of this class.</summary>
		/// <param name="handles">The driver handles.</param>
		public AIData(Handles handles)
		{
			this.MyHandles = handles;
			this.MyResponse = AIResponse.None;
		}

		// --- properties ---
		/// <summary>Gets or sets the driver handles.</summary>
		public Handles Handles
		{
			get
			{
				return this.MyHandles;
			}
			set
			{
				this.MyHandles = value;
			}
		}

		/// <summary>Gets or sets the AI response.</summary>
		public AIResponse Response
		{
			get
			{
				return this.MyResponse;
			}
			set
			{
				this.MyResponse = value;
			}
		}
	}
}
