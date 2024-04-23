using System.Globalization;
using OpenBveApi.FunctionScripting;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.World;
using RouteManager2.SignalManager;

namespace Bve5RouteParser
{
	/// <summary>Defines a BVE 4 standard signal:
	/// A signal has a face based mesh and glow
	/// Textures are then substituted according to the aspect
	/// </summary>
	internal class CompatibilitySignalData : SignalObject
	{
		internal readonly int[] Numbers;
		internal readonly StaticObject[] Objects;
		internal readonly string Key;
		internal CompatibilitySignalData(int[] Numbers, StaticObject[] Objects, string Key)
		{
			this.Numbers = Numbers;
			this.Objects = Objects;
			this.Key = Key;
		}

		public override void Create(Vector3 wpos, Transformation RailTransformation, Transformation AuxTransformation, int SectionIndex, double StartingDistance, double EndingDistance, double TrackPosition, double Brightness)
		{
			AnimatedObjectCollection aoc = new AnimatedObjectCollection(Plugin.CurrentHost)
			{
				Objects = new[]
				{
					new AnimatedObject(Plugin.CurrentHost)
					{
						States = new ObjectState[Objects.Length]
					}
				}
			};

			int zi = 0;
			string expr = "";
			for (int l = 0; l < Objects.Length; l++)
			{
				if (Objects[zi] != null)
				{
					aoc.Objects[0].States[zi] = new ObjectState {Prototype = Objects[zi]};
				}
				else
				{
					aoc.Objects[0].States[zi] = new ObjectState();
				}
				
				if (Objects[zi] != null)
				{
					CultureInfo Culture = CultureInfo.InvariantCulture;
					if (zi < Objects.Length - 1)
					{
						expr += "section " + l.ToString(Culture) + " <= " + zi.ToString(Culture) + " ";
					}
					else
					{
						expr += zi.ToString(Culture);
					}

					zi++;
				}
			}

			for (int l = 0; l < Objects.Length - 1; l++)
			{
				expr += " ?";
			}

			aoc.Objects[0].StateFunction = new FunctionScript(Plugin.CurrentHost, expr, false);
			aoc.Objects[0].RefreshRate = 1.0 + 0.01 * Plugin.RandomNumberGenerator.NextDouble();
			aoc.CreateObject(wpos, RailTransformation, AuxTransformation, SectionIndex, StartingDistance, EndingDistance, TrackPosition, 1.0);
		}
	}
}
