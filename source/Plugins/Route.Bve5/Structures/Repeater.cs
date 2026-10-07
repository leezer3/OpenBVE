//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2020, S520, The OpenBVE Project
//
//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met:
//
//1. Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
//ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using OpenBveApi.World;
using System;
using System.Collections.Generic;
using System.Linq;
using static Route.Bve5.Bve5ScenarioParser;

namespace Route.Bve5
{
	internal class NewRepeater
	{
		internal SortedDictionary<double, RepeaterEntry> Entries;

		internal NewRepeater()
		{
			Entries = new SortedDictionary<double, RepeaterEntry>();
		}

		internal void Create(RouteData RouteData, ObjectDictionary objects, double lastBlock)
		{
			TrackFollower tf = new TrackFollower(Plugin.CurrentHost);
			for (int i = 0; i < Entries.Count; i++)
			{
				double tPos = Entries.ElementAt(i).Key;
				double nextPos = i < Entries.Count - 1 ? Entries.ElementAt(i + 1).Key : lastBlock;
				if (Entries.TryGetValue(tPos, out RepeaterEntry n))
				{
					n.Create(RouteData, objects, tf, tPos, nextPos);
				}
			}
		}
	}

	internal abstract class RepeaterEntry
	{


		internal virtual void Create(RouteData ParseData, ObjectDictionary objects, TrackFollower tf, double tPos, double nextPos)
		{
		}
	}

	internal class RepeaterStart : RepeaterEntry
	{
		internal readonly string[] Types;

		internal readonly double Interval;

		internal readonly double Span;

		internal readonly string RailKey;

		internal int CurrentType;

		internal Vector3 Position;

		internal double Yaw;

		internal double Pitch;

		internal double Roll;

		internal ObjectTransformType Transform;

		internal RepeaterStart(string railKey, string[] types, double interval, double span, Vector3 position, double yaw, double pitch, double roll, ObjectTransformType transform)
		{
			RailKey = railKey;
			Types = types;
			Interval = interval;
			Span = span;
			Position = position;
			Yaw = yaw;
			Pitch = pitch;
			Roll = roll;
			Transform = transform;
		}

		internal override void Create(RouteData ParseData, ObjectDictionary objects, TrackFollower tf, double tPos, double nextPos)
		{
			tf.TrackIndex = ParseData.TrackKeyList.IndexOf(RailKey, StringComparison.OrdinalIgnoreCase);
			while (true)
			{
				tf.UpdateAbsolute(tPos, true, false);
				Vector3 startingPos = tf.WorldPosition;

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
				transform = new Transformation(transform, Yaw, Pitch, Roll);

				p += Position.X * transform.X + Position.Y * transform.Y + Position.Z * transform.Z;

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

	internal class RepeaterEnd : RepeaterEntry
	{
		// does nothing!
	}

}

