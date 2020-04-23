using System.Globalization;
using OpenBveApi.FunctionScripting;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.World;

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

		internal override void Create(Vector3 wpos, Transformation railTransformation, Transformation auxTransformation, int sectionIndex, bool accurateObjectDisposal, double startingDistance, double endingDistance, double blockInterval, double trackPosition, double brightness)
		{
			if (Numbers.Length != 0)
			{
				AnimatedObjectCollection aoc = new AnimatedObjectCollection(Program.CurrentHost);
				aoc.Objects = new AnimatedObject[1];
				aoc.Objects[0] = new AnimatedObject(Program.CurrentHost);
				aoc.Objects[0].States = new ObjectState[Numbers.Length];
				for (int l = 0; l < Numbers.Length; l++)
				{
					aoc.Objects[0].States[l] = new ObjectState { Prototype = (StaticObject)Objects[l].Clone() };
				}
				CultureInfo Culture = CultureInfo.InvariantCulture;
				string expr = "";
				for (int l = 0; l < Numbers.Length - 1; l++)
				{
					expr += "section " + Numbers[l].ToString(Culture) + " <= " + l.ToString(Culture) + " ";
				}
				expr += (Numbers.Length - 1).ToString(Culture);
				for (int l = 0; l < Numbers.Length - 1; l++)
				{
					expr += " ?";
				}
				aoc.Objects[0].StateFunction = new FunctionScript(Program.CurrentHost, expr, false);
				aoc.Objects[0].RefreshRate = 1.0 + 0.01 * Program.RandomNumberGenerator.NextDouble();
				aoc.CreateObject(wpos, railTransformation, auxTransformation, sectionIndex, accurateObjectDisposal, startingDistance, endingDistance, blockInterval, trackPosition, brightness, false);
			}
		}
	}
}
