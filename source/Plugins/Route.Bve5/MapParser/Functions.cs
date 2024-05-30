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
using System.Globalization;
using System.IO;
using System.Text;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.World;

namespace Route.Bve5
{
	static partial class Bve5ScenarioParser
	{
		/// <summary>Attempts to determine the System.Text.Encoding value for a given BVE5 file</summary>
		/// <param name="FileName">The filename</param>
		/// <returns>The detected encoding, or UTF-8 if this is not found</returns>
		internal static Encoding DetermineFileEncoding(string FileName)
		{
			using (StreamReader reader = new StreamReader(FileName))
			{
				var firstLine = reader.ReadLine();
				if (firstLine != null)
				{
					string[] Header = firstLine.Split(':');
					if (Header.Length == 1)
					{
						return Encoding.UTF8;
					}
					string[] Arguments = Header[1].Split(',');
					try
					{
						return Encoding.GetEncoding(Arguments[0].ToLowerInvariant().Trim());
					}
					catch
					{
						return Encoding.UTF8;
					}
				}
				return Encoding.UTF8;
			}
		}

		/// <summary>Parses a BVE5 format time into OpenBVE's internal time representation</summary>
		/// <param name="Expression">The time to parse</param>
		/// <param name="Value">The number of seconds since midnight on the first day this represents, updated via 'out'</param>
		/// <returns>True if the parse succeeds, false if it does not</returns>
		private static bool TryParseBve5Time(string Expression, out double Value)
		{
			if (Expression.Length != 0)
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

		private static void GetCenterOfCircle(double x0, double y0, double x1, double y1, double r, out double x, out double y)
		{
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
					x = x2 - dx;
					y = y2 + dy;
				}
				else
				{
					x = x2 + dx;
					y = y2 - dy;
				}
			}
			else
			{
				x = 0.0;
				y = 0.0;
			}
		}

		private static double GetTrackCoordinate(double distance0, double xy0, double distance1, double xy1, double radius, double distance)
		{
			double center_distance;
			double center_xy;
			GetCenterOfCircle(distance0, xy0, distance1, xy1, radius, out center_distance, out center_xy);
			double squaring = Math.Pow(radius, 2.0) - Math.Pow(distance - center_distance, 2.0);
			if (squaring >= 0.0)
			{
				if (radius > 0)
				{
					return center_xy - Math.Sqrt(squaring);
				}

				return center_xy + Math.Sqrt(squaring);
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
				Direction.Rotate(Math.Cos(-a), Math.Sin(-a));
			}
			else if (CurveRadius != 0.0)
			{
				double d = BlockInterval;
				double r = CurveRadius;
				double b = d / Math.Abs(r);
				//c = Math.Sqrt(2.0 * r * r * (1.0 - Math.Cos(b)));
				c = 2.0 * Math.Abs(r) * Math.Sin(b / 2.0);
				a = 0.5 * Math.Sign(r) * b;
				Direction.Rotate(Math.Cos(-a), Math.Sin(-a));
			}
			else if (Pitch != 0.0)
			{
				double p = Pitch;
				double d = BlockInterval;
				c = d / Math.Sqrt(1.0 + p * p);
				h = c * p;
			}
		}

		private static void GetTransformation(Vector3 StartingPosition, double StartingDistance, double Turn, double CurveRadius, double CurveCant, double Pitch, double TrackDistance, int Type, double Span, Vector2 Direction, out Vector3 ObjectPosition, out Transformation Transformation)
		{
			if (Turn != 0.0)
			{
				double ag = -Math.Atan(Turn);
				double cosag = Math.Cos(ag);
				double sinag = Math.Sin(ag);
				Direction.Rotate(cosag, sinag);
			}

			double a, c, h;
			ObjectPosition = StartingPosition;

			CalcTransformation(CurveRadius, Pitch, TrackDistance - StartingDistance, ref Direction, out a, out c, out h);

			ObjectPosition.X += Direction.X * c;
			ObjectPosition.Y += h;
			ObjectPosition.Z += Direction.Y * c;
			if (a != 0.0)
			{
				Direction.Rotate(Math.Cos(-a), Math.Sin(-a));
			}

			CalcTransformation(CurveRadius, Pitch, Span, ref Direction, out a, out c, out h);

			double TrackYaw = Math.Atan2(Direction.X, Direction.Y);
			double TrackPitch = Math.Atan(Pitch);
			double TrackRoll = Math.Atan(CurveCant);

			switch (Type)
			{
				case 1:
					Transformation = new Transformation(TrackYaw, TrackPitch, 0.0);
					break;
				case 2:
					Transformation = new Transformation(TrackYaw, 0.0, TrackRoll);
					break;
				case 3:
					Transformation = new Transformation(TrackYaw, TrackPitch, TrackRoll);
					break;
				default:
					Transformation = new Transformation(TrackYaw, 0.0, 0.0);
					break;
			}
		}

		private static void GetTransformation(Vector3 StartingPosition, double StartingDistance, double Turn, double CurveRadius, double Pitch, double X, double Y, double RadiusH, double RadiusV, double CurveCant, double NextStartingDistance, double NextX, double NextY, double TrackDistance, int Type, double Span, Vector2 Direction, out Vector3 ObjectPosition, out Transformation Transformation)
		{
			Transformation = new Transformation();
			if (Turn != 0.0)
			{
				double ag = -Math.Atan(Turn);
				double cosag = Math.Cos(ag);
				double sinag = Math.Sin(ag);
				Direction.Rotate(cosag, sinag);
			}

			double a, c, h;
			Vector3 Position = StartingPosition;

			CalcTransformation(CurveRadius, Pitch, TrackDistance - StartingDistance, ref Direction, out a, out c, out h);

			Position.X += Direction.X * c;
			Position.Y += h;
			Position.Z += Direction.Y * c;
			if (a != 0.0)
			{
				Direction.Rotate(Math.Cos(-a), Math.Sin(-a));
			}

			CalcTransformation(CurveRadius, Pitch, Span, ref Direction, out a, out c, out h);

			double InterpolateX = GetTrackCoordinate(StartingDistance, X, NextStartingDistance, NextX, RadiusH, TrackDistance);
			double InterpolateY = GetTrackCoordinate(StartingDistance, Y, NextStartingDistance, NextY, RadiusV, TrackDistance);

			Vector3 Offset = new Vector3(Direction.Y * InterpolateX, InterpolateY, -Direction.X * InterpolateX);
			ObjectPosition = Position + Offset;

			Position.X += Direction.X * c;
			Position.Y += h;
			Position.Z += Direction.Y * c;
			if (a != 0.0)
			{
				Direction.Rotate(Math.Cos(-a), Math.Sin(-a));
			}

			CalcTransformation(CurveRadius, Pitch, Span, ref Direction, out a, out c, out h);

			double InterpolateX2 = GetTrackCoordinate(StartingDistance, X, NextStartingDistance, NextX, RadiusH, TrackDistance + Span);
			double InterpolateY2 = GetTrackCoordinate(StartingDistance, Y, NextStartingDistance, NextY, RadiusV, TrackDistance + Span);

			Vector3 Offset2 = new Vector3(Direction.Y * InterpolateX2, InterpolateY2, -Direction.X * InterpolateX2);
			Vector3 ObjectPosition2 = Position + Offset2;

			Vector3 r;
			if (Type == 1 || Type == 3)
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
			if (Type == 2 || Type == 3)
			{
				Transformation = new Transformation(Transformation, 0.0, 0.0, Math.Atan(CurveCant));
			}
		}

		private static StaticObject GetTransformedStaticObject(StaticObject Prototype, double NearDistance, double FarDistance)
		{
			StaticObject Result = (StaticObject)Prototype.Clone();
			int n = 0;
			double x2 = 0.0, x3 = 0.0, x6 = 0.0, x7 = 0.0;
			for (int i = 0; i < Result.Mesh.Vertices.Length; i++)
			{
				if (n == 2)
				{
					x2 = Result.Mesh.Vertices[i].Coordinates.X;
				}
				else if (n == 3)
				{
					x3 = Result.Mesh.Vertices[i].Coordinates.X;
				}
				else if (n == 6)
				{
					x6 = Result.Mesh.Vertices[i].Coordinates.X;
				}
				else if (n == 7)
				{
					x7 = Result.Mesh.Vertices[i].Coordinates.X;
				}
				n++;
				if (n == 8)
				{
					break;
				}
			}
			if (n >= 4)
			{
				int m = 0;
				for (int i = 0; i < Result.Mesh.Vertices.Length; i++)
				{
					if (m == 0)
					{
						Result.Mesh.Vertices[i].Coordinates.X = NearDistance - x3;
					}
					else if (m == 1)
					{
						Result.Mesh.Vertices[i].Coordinates.X = FarDistance - x2;
						if (n < 8)
						{
							m = 8;
							break;
						}
					}
					else if (m == 4)
					{
						Result.Mesh.Vertices[i].Coordinates.X = NearDistance - x7;
					}
					else if (m == 5)
					{
						Result.Mesh.Vertices[i].Coordinates.X = NearDistance - x6;
						m = 8;
						break;
					}
					m++;
					if (m == 8)
					{
						break;
					}
				}
			}
			return Result;
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
	}
}
