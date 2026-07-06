using System;
using System.Collections.Generic;
using System.Linq;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using OpenBveApi.World;
using RouteManager2.Events;

namespace CsvRwRouteParser
{
	internal class NewPatternObj
	{
		internal NewPatternObj()
		{
			Entries = new SortedDictionary<double, List<Pattern>>();
		}

		internal SortedDictionary<double, List<Pattern>> Entries;

		internal void Create(ObjectDictionary objects, double lastBlock)
		{
			TrackFollower tf = new TrackFollower(Plugin.CurrentHost);
			for (int i = 0; i < Entries.Count; i++)
			{
				double tPos = Entries.ElementAt(i).Key;
				double nextPos = i < Entries.Count - 1 ? Entries.ElementAt(i + 1).Key : lastBlock;
				if (Entries.TryGetValue(tPos, out List<Pattern> p))
				{
					for (int j = 0; j < p.Count; j++)
					{
						p[j].Create(objects, tf, tPos, nextPos);
					}
				}
			}
		}
	}

	internal abstract class Pattern
	{
		internal virtual void Create(ObjectDictionary objects, TrackFollower tf, double tPos, double nextPos)
		{
		}

	}

	internal class PatternStart : Pattern
	{
		internal readonly int[] Types;

		internal readonly double Interval;

		internal readonly double Span;

		internal readonly int RailIndex;

		internal int CurrentType;

		internal Vector2 Position;

		internal PatternStart(int railIndex, double interval, double span, Vector2 position, int[] types)
		{
			RailIndex = railIndex;
			Interval = interval;
			Span = span;
			Position = position;
			Types = types;
		}

		internal override void Create(ObjectDictionary objects, TrackFollower tf, double tPos, double nextPos)
		{

			tf.TrackIndex = RailIndex;
			if (RailIndex == 4)
			{
				int b = 0;
			}
			//tPos -= Interval;
			while (true)
			{
				tf.UpdateAbsolute(tPos, true, false);
				Vector3 startingPos = tf.WorldPosition;

				List<GeneralEvent> e = Plugin.CurrentRoute.Tracks[RailIndex].Elements[tf.LastTrackElement].Events;
				tf.UpdateRelative(Span, true, false);

				Vector3 nextElementPos = tf.WorldPosition;
				double dist = Math.Abs(Math.Sqrt(((startingPos.X - nextElementPos.X) * (startingPos.X - nextElementPos.X)) + ((startingPos.Y - nextElementPos.Y) * (startingPos.Y - nextElementPos.Y))));
				if (dist > Span * 2)
				{
					nextElementPos = startingPos;
				}
				
				Vector3 p = new Vector3(startingPos);
				// find direction, up and side vectors
				Vector3 d = nextElementPos == startingPos ? nextElementPos : new Vector3(nextElementPos - startingPos);
				double t = d.Magnitude();
				d *= t;
				t = 1.0 / Math.Sqrt(d.X * d.X + d.Z * d.Z);
				double ex = d.X * t;
				double ez = d.Z * t;
				Vector3 s = new Vector3(ez, 0.0, -ex);
				Vector3 u = Vector3.Cross(d, s);

				if (Types.Length == 0 || !objects.ContainsKey(Types[CurrentType]))
				{
					return;
				}

				UnifiedObject currentObject = objects[Types[CurrentType]];

				Transformation transform = new Transformation(d, u, s);

				Vector3 v = new Vector3(Position.X, Position.Y, 0);
				p += Position.X * transform.X + Position.Y * transform.Y + 0 * transform.Z;

				currentObject.CreateObject(p, new Transformation(d, u, s), new ObjectCreationParameters(tPos, tPos + 100));

				CurrentType++;
				if (CurrentType > Types.Length - 1)
				{
					CurrentType = 0;
				}

				tPos += Interval;
				if (tPos >= nextPos)
				{
					break;
				}
			}
		}
	}

	internal class PatternEnd : Pattern
	{
		// does nothing!
	}
}
