using System;
using System.Globalization;
using OpenBveApi.FunctionScripting;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Textures;
using OpenBveApi.World;
using RouteManager2.SignalManager;

namespace CsvRwRouteParser
{
	/// <summary>Defines a BVE 4 standard signal:
	/// A signal has a face based mesh and glow
	/// Textures are then substituted according to the aspect
	/// </summary>
	internal class Bve4SignalData : SignalObject
	{
		internal StaticObject BaseObject;
		internal StaticObject GlowObject;
		internal Texture[] SignalTextures;
		internal Texture[] GlowTextures;

		public override void Create(Vector3 wpos, Transformation RailTransformation, Transformation LocalTransformation, int SectionIndex, double StartingDistance, double EndingDistance, double TrackPosition, double Brightness)
		{
			if (SignalTextures.Length != 0)
			{
				int m = Math.Max(SignalTextures.Length, GlowTextures.Length);
				int zn = 0;
				for (int l = 0; l < m; l++)
				{
					if (l < SignalTextures.Length && SignalTextures[l] != null || l < GlowTextures.Length && GlowTextures[l] != null)
					{
						zn++;
					}
				}

				AnimatedObjectCollection aoc = new AnimatedObjectCollection(Plugin.CurrentHost)
				{
					Objects = new[]
					{
						new AnimatedObject(Plugin.CurrentHost)
						{
							States = new ObjectState[zn]
						}
					}
				};

				int zi = 0;
				string expr = "";
				for (int l = 0; l < m; l++)
				{
					bool qs = l < SignalTextures.Length && SignalTextures[l] != null;
					bool qg = l < GlowTextures.Length && GlowTextures[l] != null;
					StaticObject so = new StaticObject(Plugin.CurrentHost);
					StaticObject go = null;
					if (qs & qg)
					{

						if (BaseObject != null)
						{
							so = BaseObject.Clone(SignalTextures[l], null);
						}

						if (GlowObject != null)
						{
							go = GlowObject.Clone(GlowTextures[l], null);
						}

						so.JoinObjects(go);
						aoc.Objects[0].States[zi] = new ObjectState(so);
					}
					else if (qs)
					{
						if (BaseObject != null)
						{
							so = BaseObject.Clone(SignalTextures[l], null);
						}

						aoc.Objects[0].States[zi] = new ObjectState(so);
					}
					else if (qg)
					{
						if (GlowObject != null)
						{
							go = GlowObject.Clone(GlowTextures[l], null);
						}

						//BUG: Should we join the glow object here? Test what BVE4 does with missing state, but provided glow
						aoc.Objects[0].States[zi] = new ObjectState(so);
					}

					if (qs | qg)
					{
						CultureInfo Culture = CultureInfo.InvariantCulture;
						if (zi < zn - 1)
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

				for (int l = 0; l < zn - 1; l++)
				{
					expr += " ?";
				}

				aoc.Objects[0].StateFunction = new FunctionScript(Plugin.CurrentHost, expr, false);
				aoc.Objects[0].RefreshRate = 1.0 + 0.01 * Plugin.RandomNumberGenerator.NextDouble();
				aoc.CreateObject(wpos, RailTransformation, LocalTransformation, SectionIndex, StartingDistance, EndingDistance, TrackPosition, 1.0);
			}
		}
	}
}
