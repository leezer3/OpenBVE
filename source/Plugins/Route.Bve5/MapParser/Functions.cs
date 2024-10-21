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
	static partial class Bve5ScenarioParser
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

		private static void SineHalfWavelengthDiminishingTangentCurve(double Length, double TargetRadius, double TargetCant, double CurrentPosition, out double CurrentRadius, out double CurrentCant)
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

			if (TargetRadius != 0.0)
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

			CurrentCant = (1.0 * TargetCant) / 2.0 * (1.0 - Math.Cos(Math.PI / X * CurrentPosition));
		}

		private static void CalcCurveTransition(double StartDistance, double StartRadius, double StartCant, double EndDistance, double EndRadius, double EndCant, double CurrentDistance, out double CurrentRadius, out double CurrentCant)
		{
			if (StartRadius == 0.0 && StartCant == 0.0)
			{
				SineHalfWavelengthDiminishingTangentCurve(EndDistance - StartDistance, EndRadius, EndCant, CurrentDistance - StartDistance, out CurrentRadius, out CurrentCant);
			}
			else if (EndRadius == 0.0 && EndCant == 0.0)
			{
				SineHalfWavelengthDiminishingTangentCurve(EndDistance - StartDistance, StartRadius, StartCant, EndDistance - CurrentDistance, out CurrentRadius, out CurrentCant);
			}
			else
			{
				double Midpoint = (StartDistance + EndDistance) / 2.0;
				if (CurrentDistance < Midpoint)
				{
					SineHalfWavelengthDiminishingTangentCurve(Midpoint - StartDistance, StartRadius, StartCant, Midpoint - CurrentDistance, out CurrentRadius, out CurrentCant);
				}
				else
				{
					SineHalfWavelengthDiminishingTangentCurve(EndDistance - Midpoint, EndRadius, EndCant, CurrentDistance - Midpoint, out CurrentRadius, out CurrentCant);
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
			if (CurveRadius != 0.0 & Pitch != 0.0)
			{
				double d = BlockInterval;
				double p = Pitch;
				double r = CurveRadius;
				double s = d / Math.Sqrt(1.0 + p * p);
				h = s * p;
				double b = s / Math.Abs(r);
				//c = Math.Sqrt(2.0 * r * r * (1.0 - Math.Cos(b)));
				c = 2.0 * Math.Abs(r) * Math.Sin(b / 2.0);
				a = 0.5 * Math.Sign(r) * b;
				Direction.Rotate(-a);
			}
			else if (CurveRadius != 0.0)
			{
				double d = BlockInterval;
				double r = CurveRadius;
				double b = d / Math.Abs(r);
				//c = Math.Sqrt(2.0 * r * r * (1.0 - Math.Cos(b)));
				c = 2.0 * Math.Abs(r) * Math.Sin(b / 2.0);
				a = 0.5 * Math.Sign(r) * b;
				Direction.Rotate(-a);
			}
			else if (Pitch != 0.0)
			{
				double p = Pitch;
				double d = BlockInterval;
				c = d / Math.Sqrt(1.0 + p * p);
				h = c * p;
			}
		}

		/// <summary>Gets the transformation for an object on the primary rail</summary>
		private static void GetPrimaryRailTransformation(Vector3 StartingPosition, IList<Block> Blocks, int StartingBlock, AbstractStructure Structure, Vector2 Direction, out Vector3 ObjectPosition, out Transformation Transformation)
		{
			Direction.Rotate(-Math.Atan(Blocks[StartingBlock].Turn));

			ObjectPosition = StartingPosition;
			Transformation = new Transformation();
			if (Structure.Span == 0)
			{
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
						throw new NotSupportedException("Unknown transform type.");
				}
			}
			else
			{
				int nextBlock = StartingBlock < Blocks.Count - 1 ? StartingBlock + 1 : StartingBlock;
				GetTransformation(StartingPosition, Blocks[StartingBlock], Blocks[nextBlock], "0", Blocks[StartingBlock].Pitch, Structure.TrackPosition, Structure.Type, Structure.Span, Direction, out ObjectPosition, out Transformation);
			}
			
			
		}

		/// <summary>Gets the transformation for an object on a secondary rail</summary>
		private static void GetSecondaryRailTransformation(Vector3 StartingPosition, Vector2 StartingDirection, IList<Block> Blocks, int StartingBlock, string RailKey, AbstractStructure Structure, out Vector3 pos, out Transformation t)
		{
			int nextBlock = StartingBlock < Blocks.Count - 1 ? StartingBlock + 1 : StartingBlock;
			GetTransformation(StartingPosition, Blocks[StartingBlock], Blocks[nextBlock], RailKey, Blocks[StartingBlock].Pitch, Structure.TrackPosition, Structure.Type, Structure.Span, StartingDirection, out pos, out t);
		}

		/// <summary>Gets the transformation between two blocks</summary>
		private static void GetTransformation(Vector3 StartingPosition, Block FirstBlock, Block SecondBlock, string RailKey, double Pitch, double TrackDistance, ObjectTransformType Type, double Span, Vector2 Direction, out Vector3 ObjectPosition, out Transformation Transformation)
		{
			Transformation = new Transformation();
			Direction.Rotate(-Math.Atan(FirstBlock.Turn));

			Vector3 Position = StartingPosition;

			CalcTransformation(FirstBlock.CurrentTrackState.CurveRadius, Pitch, TrackDistance - FirstBlock.StartingDistance, ref Direction, out double a, out double c, out double h);

			Position.X += Direction.X * c;
			Position.Y += h;
			Position.Z += Direction.Y * c;
			Direction.Rotate(-a);

			CalcTransformation(FirstBlock.CurrentTrackState.CurveRadius, Pitch, Span, ref Direction, out a, out c, out h);

			double InterpolateX = GetTrackCoordinate(FirstBlock.StartingDistance, FirstBlock.Rails[RailKey].Position.X, SecondBlock.StartingDistance,SecondBlock.Rails[RailKey].Position.X, FirstBlock.Rails[RailKey].RadiusH, TrackDistance);
			double InterpolateY = GetTrackCoordinate(FirstBlock.StartingDistance, FirstBlock.Rails[RailKey].Position.Y, SecondBlock.StartingDistance, SecondBlock.Rails[RailKey].Position.Y, FirstBlock.Rails[RailKey].RadiusV, TrackDistance);

			Vector3 Offset = new Vector3(Direction.Y * InterpolateX, InterpolateY, -Direction.X * InterpolateX);
			ObjectPosition = Position + Offset;

			Position.X += Direction.X * c;
			Position.Y += h;
			Position.Z += Direction.Y * c;
			Direction.Rotate(-a);

			CalcTransformation(FirstBlock.CurrentTrackState.CurveRadius, Pitch, Span, ref Direction, out _, out _, out _);

			double InterpolateX2 = GetTrackCoordinate(FirstBlock.StartingDistance, FirstBlock.Rails[RailKey].Position.X, SecondBlock.StartingDistance, SecondBlock.Rails[RailKey].Position.X, FirstBlock.Rails[RailKey].RadiusH, TrackDistance + Span);
			double InterpolateY2 = GetTrackCoordinate(FirstBlock.StartingDistance, FirstBlock.Rails[RailKey].Position.Y, SecondBlock.StartingDistance, SecondBlock.Rails[RailKey].Position.Y, FirstBlock.Rails[RailKey].RadiusV, TrackDistance + Span);

			Vector3 Offset2 = new Vector3(Direction.Y * InterpolateX2, InterpolateY2, -Direction.X * InterpolateX2);
			Vector3 ObjectPosition2 = Position + Offset2;

			Vector3 r;
			if (Type == ObjectTransformType.FollowsGradient || Type == ObjectTransformType.FollowsGradientAndCant)
			{
				r = new Vector3(ObjectPosition2.X - ObjectPosition.X, ObjectPosition2.Y - ObjectPosition.Y, ObjectPosition2.Z - ObjectPosition.Z);
			}
			else
			{
				r = new Vector3(ObjectPosition2.X - ObjectPosition.X, 0.0, ObjectPosition2.Z - ObjectPosition.Z);
			}
			r.Normalize();
			Transformation.Z = r;
			Transformation.X = new Vector3(r.Z, 0.0, -r.X);
			Normalize(ref Transformation.X.X, ref Transformation.X.Z);
			Transformation.Y = Vector3.Cross(Transformation.Z, Transformation.X);
			if (Type == ObjectTransformType.FollowsCant || Type == ObjectTransformType.FollowsGradientAndCant)
			{
				Transformation = new Transformation(Transformation, 0.0, 0.0, Math.Atan(FirstBlock.CurrentTrackState.CurveCant));
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
