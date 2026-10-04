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

using System;
using System.Collections.Generic;
using System.Globalization;
using OpenBveApi.Math;
using OpenBveApi.World;
// ReSharper disable TooWideLocalVariableScope
// ReSharper disable InlineOutVariableDeclaration

namespace Route.Bve5
{
	internal static partial class Bve5ScenarioParser
	{
		/// <summary>Parses a BVE5 format time into OpenBVE's internal time representation</summary>
		/// <param name="Expression">The time to parse</param>
		/// <param name="Value">The number of seconds since midnight on the first day this represents, updated via 'out'</param>
		/// <returns>True if the parse succeeds, false if it does not</returns>
		private static bool TryParseBve5Time(string Expression, out double Value)
		{
			if (!string.IsNullOrEmpty(Expression))
			{
				CultureInfo Culture = CultureInfo.InvariantCulture;
				string[] Split = Expression.Split(':');

				int h, m, s;
				switch (Split.Length)
				{
					case 1:
						//Single number - plain seconds
						if (int.TryParse(Expression.Trim(), NumberStyles.Integer, Culture, out s))
						{
							Value = s;
							return true;
						}
						break;
					case 3:
						//HH:MM:SS
						if (int.TryParse(Split[0].Trim(), NumberStyles.Integer, Culture, out h) && int.TryParse(Split[1].Trim(), NumberStyles.Integer, Culture, out m) && int.TryParse(Split[2].Trim(), NumberStyles.Integer, Culture, out s))
						{
							Value = 3600.0 * h + 60.0 * m + s;
							return true;
						}
						break;
				}
			}
			Value = 0.0;
			return false;
		}

		private static double Multiple(double x, int y)
		{
			x = Math.Ceiling(x);
			return x % y == 0 ? x : x + (y - x % y);
		}

		private static double LinearInterpolation(double x0, double y0, double x1, double y1, double x)
		{
			return x0 == x1 ? y0 : y0 + (y1 - y0) * (x - x0) / (x1 - x0);
		}

		private static void SineHalfWavelengthDiminishingTangentCurve(double Length, double StartRadius, double StartCant, double TargetRadius, double TargetCant, double CurrentPosition, out double CurrentRadius, out double CurrentCant)
		{
			// Sine Half-Wavelength Diminishing Tangent Curve
			// https://knowledge.autodesk.com/support/autocad-civil-3d/learn-explore/caas/CloudHelp/cloudhelp/2018/ENU/Civil3D-UserGuide/files/GUID-DD7C0EA1-8465-45BA-9A39-FC05106FD822-htm.html

			double X;

			if (TargetRadius != 0.0)
			{
				X = Length * (1.0 - ((2.0 * Math.Pow(Math.PI, 2.0) - 9.0) / (48.0 * Math.PI)) * Math.Pow(Length / TargetRadius, 2.0));
			}
			else
			{
				X = Length;
			}

			if (X == 0.0)
			{
				CurrentRadius = 0.0;
				CurrentCant = 0.0;
				return;
			}

			// https://web.archive.org/web/20060222112024/http://www1.odn.ne.jp/~aaa81350/kaisetu/tenpuku/tenpuku.htm

			if (StartRadius == TargetRadius)
			{
				// Impossible to calculate a transition between two identical values (just returns nonsense values)
				CurrentRadius = StartRadius;
			}
			else if (TargetRadius != 0.0)
			{
				double Curvature = 1.0 / (2.0 * TargetRadius) * (1.0 - Math.Cos(Math.PI / X * CurrentPosition));

				if (Curvature != 0.0)
				{
					CurrentRadius = 1.0 / Curvature;
				}
				else
				{
					CurrentRadius = 0.0;
				}
			}
			else
			{
				CurrentRadius = 0.0;
			}

			if (StartCant == TargetCant)
			{
				// Impossible to calculate a transition between two identical values (just returns nonsense values)
				CurrentCant = StartCant;
			}
			else
			{
				CurrentCant = (1.0 * TargetCant) / 2.0 * (1.0 - Math.Cos(Math.PI / X * CurrentPosition));
			}
		}

		private static void CalcCurveTransition(double StartDistance, double StartRadius, double StartCant, double EndDistance, double EndRadius, double EndCant, double CurrentDistance, out double CurrentRadius, out double CurrentCant)
		{
			if (StartRadius == 0.0 && StartCant == 0.0)
			{
				SineHalfWavelengthDiminishingTangentCurve(EndDistance - StartDistance, StartRadius, StartCant, EndRadius, EndCant, CurrentDistance - StartDistance, out CurrentRadius, out CurrentCant);
			}
			else if (EndRadius == 0.0 && EndCant == 0.0)
			{
				SineHalfWavelengthDiminishingTangentCurve(EndDistance - StartDistance, EndRadius, EndCant, StartRadius, StartCant, EndDistance - CurrentDistance, out CurrentRadius, out CurrentCant);
			}
			else
			{
				double Midpoint = (StartDistance + EndDistance) / 2.0;
				if (CurrentDistance < Midpoint)
				{
					SineHalfWavelengthDiminishingTangentCurve(Midpoint - StartDistance, EndRadius, EndCant, StartRadius, StartCant, Midpoint - CurrentDistance, out CurrentRadius, out CurrentCant);
				}
				else
				{
					SineHalfWavelengthDiminishingTangentCurve(EndDistance - Midpoint, StartRadius, StartCant, EndRadius, EndCant, CurrentDistance - Midpoint, out CurrentRadius, out CurrentCant);
				}
			}
		}

		private static Vector2 GetCenterOfCircle(double x0, double y0, double x1, double y1, double r)
		{
			Vector2 result = Vector2.Null;
			double x2 = (x0 + x1) / 2.0;
			double y2 = (y0 + y1) / 2.0;
			double l = Math.Pow(x1 - x2, 2.0) + Math.Pow(y1 - y2, 2.0);
			if (l <= Math.Pow(r, 2.0))
			{
				double d = Math.Sqrt(Math.Pow(r, 2.0) / l - 1.0);
				double dx = d * (y1 - y2);
				double dy = d * (x1 - x2);
				if (r >= 0)
				{
					result.X = x2 - dx;
					result.Y = y2 + dy;
				}
				else
				{
					result.X = x2 + dx;
					result.Y = y2 - dy;
				}
			}

			return result;
		}

		private static double GetTrackCoordinate(double distance0, double xy0, double distance1, double xy1, double radius, double distance)
		{
			if (distance0 == 0)
			{
				// first block, no interpolation possible
				return xy0;
			}
			Vector2 center = GetCenterOfCircle(distance0, xy0, distance1, xy1, radius);
			double squaring = Math.Pow(radius, 2.0) - Math.Pow(distance - center.X, 2.0);
			if (squaring >= 0.0)
			{
				if (radius > 0)
				{
					return center.Y - Math.Sqrt(squaring);
				}

				return center.Y + Math.Sqrt(squaring);
			}

			return LinearInterpolation(distance0, xy0, distance1, xy1, distance);
		}

		private static double RZtoRoll(double RY, double RZ)
		{
			RY = Math.Abs(RY % 360.0);

			if (RY > 90.0 && RY <= 270.0)
			{
				return RZ;
			}

			return RZ * -1;
		}

		private static void CalcTransformation(double CurveRadius, double Pitch, double BlockInterval, ref Vector2 Direction, out double a, out double c, out double h)
		{
			if (BlockInterval == 0)
			{
				// Attempting to transform a zero-length block will never work- Just use an arbitrary number here (Some objects will trigger this)
				BlockInterval = 0.01;
			}
			a = 0.0;
			c = BlockInterval;
			h = 0.0;
			if (CurveRadius != 0.0 && Pitch != 0.0)
			{
				double s = BlockInterval / Math.Sqrt(1.0 + Pitch * Pitch);
				h = s * Pitch;
				double b = s / Math.Abs(CurveRadius);
				c = 2.0 * Math.Abs(CurveRadius) * Math.Sin(b / 2.0);
				a = 0.5 * Math.Sign(CurveRadius) * b;
				Direction.Rotate(-a);
			}
			else if (CurveRadius != 0.0)
			{
				double b = BlockInterval / Math.Abs(CurveRadius);
				c = 2.0 * Math.Abs(CurveRadius) * Math.Sin(b / 2.0);
				a = 0.5 * Math.Sign(CurveRadius) * b;
				Direction.Rotate(-a);
			}
			else if (Pitch != 0.0)
			{
				c = BlockInterval / Math.Sqrt(1.0 + Pitch * Pitch);
				h = c * Pitch;
			}
		}

		/// <summary>Gets the transformation for an object on the primary rail</summary>
		private static bool GetRailTransformation(string RailKey, Vector3 StartingPosition, IList<Block> Blocks, int StartingBlock, AbstractStructure Structure, Vector2 Direction, out Vector3 ObjectPosition, out Transformation Transformation)
		{
			ObjectPosition = StartingPosition;
			Transformation = new Transformation();
			if (Structure.Span == 0)
			{
				Direction.Rotate(-Math.Atan(Blocks[StartingBlock].Turn));

				if (RailKey != "0")
				{
					int nextBlock = StartingBlock < Blocks.Count - 1 ? StartingBlock + 1 : StartingBlock;
					double InterpolateX = GetTrackCoordinate(Blocks[StartingBlock].StartingDistance, Blocks[StartingBlock].Rails[RailKey].Position.X, Blocks[nextBlock].StartingDistance, Blocks[nextBlock].Rails[RailKey].Position.X, Blocks[StartingBlock].Rails[RailKey].RadiusH, Structure.TrackPosition);
					double InterpolateY = GetTrackCoordinate(Blocks[StartingBlock].StartingDistance, Blocks[StartingBlock].Rails[RailKey].Position.Y, Blocks[nextBlock].StartingDistance, Blocks[nextBlock].Rails[RailKey].Position.Y, Blocks[StartingBlock].Rails[RailKey].RadiusV, Structure.TrackPosition);
					Vector3 Offset = new Vector3(Direction.Y * InterpolateX, InterpolateY, -Direction.X * InterpolateX);
					ObjectPosition += Offset;
				}

				double radius = Blocks[StartingBlock].CurrentTrackState.CurveRadius;
				double pitch = Blocks[StartingBlock].Pitch;
				double cant = Blocks[StartingBlock].CurrentTrackState.CurveCant;
				CalcTransformation(radius, pitch, Structure.TrackPosition - Blocks[StartingBlock].StartingDistance, ref Direction, out double a, out double c, out double h);
				ObjectPosition.X += Direction.X * c;
				ObjectPosition.Y += h;
				ObjectPosition.Z += Direction.Y * c;
				Direction.Rotate(-a);

				CalcTransformation(radius, pitch, Structure.Span, ref Direction, out _, out _, out _);
				double TrackYaw = Math.Atan2(Direction.X, Direction.Y);
				double TrackPitch = Math.Atan(pitch);
				double TrackRoll = Math.Atan(cant);

				switch (Structure.Type)
				{
					case ObjectTransformType.FollowsGradient:
						Transformation = new Transformation(TrackYaw, TrackPitch, 0.0);
						break;
					case ObjectTransformType.FollowsCant:
						Transformation = new Transformation(TrackYaw, 0.0, TrackRoll);
						break;
					case ObjectTransformType.FollowsGradientAndCant:
						Transformation = new Transformation(TrackYaw, TrackPitch, TrackRoll);
						break;
					case ObjectTransformType.Horizontal:
						Transformation = new Transformation(TrackYaw, 0.0, 0.0);
						break;
					default:
						return false;
				}
			}
			else
			{
				GetTransformation(StartingPosition, Blocks, StartingBlock, RailKey, Structure.TrackPosition, Structure.Type, Structure.Span, Direction, out ObjectPosition, out Transformation);
			}
			
			return true;
		}

		/// <summary>
		/// Walks the player track forwards from the start of a block to a track position, stepping off
		/// to the requested rail on the way.
		/// </summary>
		/// <remarks>
		/// A structure with a span (an overhead line, a platform roof, a length of ballast) covers several
		/// blocks, and both the alignment of the track and the lateral offset of the rail can change
		/// underneath it. Reading those out of the block the structure happens to start in leaves its far
		/// end off the rail, which is most visible where consecutive segments should meet: the catenary
		/// wires at Chiba used to stop ~0.35m short of each other at the 260m pole.
		/// Walking the track is the same work ApplyRouteData does to lay the track out in the first place,
		/// so both ends of the chord land exactly on the rail.
		/// </remarks>
		private static Vector3 GetRailPosition(Vector3 StartingPosition, IList<Block> Blocks, int StartingBlock, string RailKey, Vector2 Direction, double TargetDistance)
		{
			Vector3 position = StartingPosition;
			Vector2 direction = Direction;
			direction.Rotate(-Math.Atan(Blocks[StartingBlock].Turn));

			// position always follows the player track, so the rail offset gets recalculated for every block rather than added on top of the previous one
			Vector3 offset = Vector3.Zero;
			double distance = Blocks[StartingBlock].StartingDistance;

			for (int i = StartingBlock; i < Blocks.Count; i++)
			{
				Block block = Blocks[i];
				bool HasNextBlock = i < Blocks.Count - 1;
				double blockEnd = HasNextBlock ? Blocks[i + 1].StartingDistance : distance + InterpolateInterval;
				double step = Math.Min(TargetDistance, blockEnd) - distance;

				offset = Vector3.Zero;

				if (RailKey != "0" && block.Rails.ContainsKey(RailKey))
				{
					// Interpolating within this block only keeps us from dragging one block's offset slope across the whole span
					Rail rail = block.Rails[RailKey];
					bool NextBlockHasRail = HasNextBlock && Blocks[i + 1].Rails.ContainsKey(RailKey);
					double InterpolateX = GetTrackCoordinate(block.StartingDistance, rail.Position.X, blockEnd, NextBlockHasRail ? Blocks[i + 1].Rails[RailKey].Position.X : rail.Position.X, rail.RadiusH, distance);
					double InterpolateY = GetTrackCoordinate(block.StartingDistance, rail.Position.Y, blockEnd, NextBlockHasRail ? Blocks[i + 1].Rails[RailKey].Position.Y : rail.Position.Y, rail.RadiusV, distance);
					offset = new Vector3(direction.Y * InterpolateX, InterpolateY, -direction.X * InterpolateX);
				}

				if (step <= 0.0)
				{
					// We've arrived, and the offset we just worked out is the one which applies here
					break;
				}

				CalcTransformation(block.CurrentTrackState.CurveRadius, block.Pitch, step, ref direction, out double a, out double c, out double h);

				position.X += direction.X * c;
				position.Y += h;
				position.Z += direction.Y * c;
				direction.Rotate(-a);

				distance = blockEnd;

				if (HasNextBlock)
				{
					direction.Rotate(-Math.Atan(Blocks[i + 1].Turn));
				}
			}

			return position + offset;
		}

		/// <summary>Gets the transformation for a structure which spans a chord of the rail, e.g. an overhead line</summary>
		/// <remarks>
		/// The structure is rigid, so it is placed along the straight line between the two ends of its
		/// span rather than being bent to follow the curve. Where the track genuinely curves over the span
		/// that chord is very slightly shorter than the track it covers, which is expected.
		/// </remarks>
		private static void GetTransformation(Vector3 StartingPosition, IList<Block> Blocks, int StartingBlock, string RailKey, double TrackDistance, ObjectTransformType Type, double Span, Vector2 Direction, out Vector3 ObjectPosition, out Transformation Transformation)
		{
			Transformation = new Transformation();

			// Point the structure along the chord between the two ends of its span
			ObjectPosition = GetRailPosition(StartingPosition, Blocks, StartingBlock, RailKey, Direction, TrackDistance);
			Vector3 SpanEnd = GetRailPosition(StartingPosition, Blocks, StartingBlock, RailKey, Direction, TrackDistance + Span);

			Vector3 r;
			if (Type == ObjectTransformType.FollowsGradient || Type == ObjectTransformType.FollowsGradientAndCant)
			{
				r = new Vector3(SpanEnd.X - ObjectPosition.X, SpanEnd.Y - ObjectPosition.Y, SpanEnd.Z - ObjectPosition.Z);
			}
			else
			{
				// These transform types don't follow the gradient, so the span stays level
				r = new Vector3(SpanEnd.X - ObjectPosition.X, 0.0, SpanEnd.Z - ObjectPosition.Z);
			}
			r.Normalize();
			Transformation.Z = r;
			Transformation.X = new Vector3(r.Z, 0.0, -r.X);
			Normalize(ref Transformation.X.X, ref Transformation.X.Z);
			Transformation.Y = Vector3.Cross(Transformation.Z, Transformation.X);
			if (Type == ObjectTransformType.FollowsCant || Type == ObjectTransformType.FollowsGradientAndCant)
			{
				Transformation = new Transformation(Transformation, 0.0, 0.0, Math.Atan(Blocks[StartingBlock].CurrentTrackState.CurveCant));
			}
		}
		private static void Normalize(ref double x, ref double y)
		{
			double t = x * x + y * y;
			if (t != 0.0)
			{
				t = 1.0 / Math.Sqrt(t);
				x *= t;
				y *= t;
			}
		}

		public static int IndexOf<T>(this List<string> source, T value, StringComparison stringComparison)
		{
			if (typeof(T) == typeof(string))
			{
				return source.FindIndex(x => x.Equals(value as string, stringComparison));
			}
			return -1;
		}
	}
}
