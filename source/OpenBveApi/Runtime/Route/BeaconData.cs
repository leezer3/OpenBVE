using System.Runtime.Serialization;

namespace OpenBveApi.Runtime
{
	/// <summary>Represents data trasmitted by a beacon.</summary>
	[DataContract]
	public class BeaconData
	{
		/// <summary>The type of beacon.</summary>
		[DataMember]
		private readonly int MyType;

		/// <summary>Optional data the beacon transmits.</summary>
		[DataMember]
		private readonly int MyOptional;

		/// <summary>The section the beacon is attached to.</summary>
		[DataMember]
		private readonly SignalData MySignal;

		/// <summary>Gets the type of beacon.</summary>
		public int Type => MyType;

		/// <summary>Gets optional data the beacon transmits.</summary>
		public int Optional => MyOptional;

		/// <summary>Gets the section the beacon is attached to.</summary>
		public SignalData Signal => MySignal;

		/// <summary>Creates a new instance of this class.</summary>
		/// <param name="type">The type of beacon.</param>
		/// <param name="optional">Optional data the beacon transmits.</param>
		/// <param name="signal">The section the beacon is attached to.</param>
		public BeaconData(int type, int optional, SignalData signal)
		{
			MyType = type;
			MyOptional = optional;
			MySignal = signal;
		}
	}
}
