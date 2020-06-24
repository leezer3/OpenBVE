using System;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.World;

namespace CsvRwRouteParser
{
	internal struct Pole
	{
		/// <summary>Whether the pole exists in the current block</summary>
		internal bool Exists;
		/// <summary>The pole mode</summary>
		internal int Mode;
		/// <summary>The location within the block of the .Pole command</summary>
		internal double Location;
		/// <summary>The repetition interval</summary>
		internal double Interval;
		/// <summary>The structure type</summary>
		internal int Type;

		internal void Create(PoleDictionary Poles, Vector3 WorldPosition, Transformation RailTransformation, Vector2 Direction, double planar, double updown, double StartingDistance, double EndingDistance, double BlockInterval, bool AccurateObjectDisposal)
		{
			double dz = StartingDistance / Interval;
			dz -= Math.Floor(dz + 0.5);
			if (dz >= -0.01 & dz <= 0.01)
			{
				if (Mode == 0)
				{
					if (Location <= 0.0)
					{
						Poles[0][Type].CreateObject(WorldPosition, RailTransformation, Transformation.NullTransformation, AccurateObjectDisposal, StartingDistance, EndingDistance, BlockInterval, StartingDistance);
					}
					else
					{
						UnifiedObject Pole = Poles[0][Type].Mirror();
						Pole.CreateObject(WorldPosition, RailTransformation, Transformation.NullTransformation, AccurateObjectDisposal, StartingDistance, EndingDistance, BlockInterval, StartingDistance);
					}
				}
				else
				{
					int m = Mode;
					double dx = -Location * 3.8;
					double wa = Math.Atan2(Direction.Y, Direction.X) - planar;
					Vector3 w = new Vector3(Math.Cos(wa), Math.Tan(updown), Math.Sin(wa));
					w.Normalize();
					double sx = Direction.Y;
					double sy = 0.0;
					double sz = -Direction.X;
					Vector3 wpos = WorldPosition + new Vector3(sx * dx + w.X * dz, sy * dx + w.Y * dz, sz * dx + w.Z * dz);
					int type = Type;
					Poles[m][type].CreateObject(wpos, RailTransformation, Transformation.NullTransformation, AccurateObjectDisposal, StartingDistance, EndingDistance, BlockInterval, StartingDistance);
				}
			}
		}
	}
}
