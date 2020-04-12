using OpenBveApi.Objects;

namespace OpenBve
{
	/// <summary>Defines a default auto-generated Japanese signal (See the documentation)</summary>
	internal class CompatibilitySignalData : SignalData
	{
		internal readonly int[] Numbers;
		internal readonly StaticObject[] Objects;

		internal CompatibilitySignalData(int[] Numbers, StaticObject[] Objects)
		{
			this.Numbers = Numbers;
			this.Objects = Objects;
		}
	}
}
