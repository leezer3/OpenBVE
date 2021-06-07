using System;
using OpenBveApi.Hosts;
using OpenBveApi.Math;

namespace OpenBveApi.Routes
{
	/// <summary>A track (route) is made up of an array of track elements (cells)</summary>
	public class Track
	{
		/// <summary>The elements array</summary>
		public TrackElement[] Elements;

		/// <summary>The rail gauge for this track</summary>
		public double RailGauge = 1.435;

		/// <summary>Gets the innacuracy (Gauge spread and track bounce) for a given track position and routefile innacuracy value</summary>
		/// <param name="position">The track position</param>
		/// <param name="inaccuracy">The openBVE innacuaracy value</param>
		/// <param name="x">The X (horizontal) co-ordinate to update</param>
		/// <param name="y">The Y (vertical) co-ordinate to update</param>
		/// <param name="c">???</param>
		public void GetInaccuracies(double position, double inaccuracy, out double x, out double y, out double c)
		{
			if (inaccuracy <= 0.0)
			{
				x = 0.0;
				y = 0.0;
				c = 0.0;
			}
			else
			{
				double z = System.Math.Pow(0.25 * inaccuracy, 1.2) * position;
				x = 0.14 * System.Math.Sin(0.5843 * z) + 0.82 * System.Math.Sin(0.2246 * z) + 0.55 * System.Math.Sin(0.1974 * z);
				x *= 0.0035 * RailGauge * inaccuracy;
				y = 0.18 * System.Math.Sin(0.5172 * z) + 0.37 * System.Math.Sin(0.3251 * z) + 0.91 * System.Math.Sin(0.3773 * z);
				y *= 0.0020 * RailGauge * inaccuracy;
				c = 0.23 * System.Math.Sin(0.3131 * z) + 0.54 * System.Math.Sin(0.5807 * z) + 0.81 * System.Math.Sin(0.3621 * z);
				c *= 0.0025 * RailGauge * inaccuracy;
			}
		}

		/// <summary>Computes the cant tangents for all elements</summary>
		public void ComputeCantTangents()
		{
			if (Elements.Length == 1)
			{
				Elements[0].CurveCantTangent = 0.0;
			}
			else if (Elements.Length != 0)
			{
				double[] deltas = new double[Elements.Length - 1];
				for (int j = 0; j < Elements.Length - 1; j++)
				{
					deltas[j] = Elements[j + 1].CurveCant - Elements[j].CurveCant;
				}

				double[] tangents = new double[Elements.Length];
				tangents[0] = deltas[0];
				tangents[Elements.Length - 1] = deltas[Elements.Length - 2];
				for (int j = 1; j < Elements.Length - 1; j++)
				{
					tangents[j] = 0.5 * (deltas[j - 1] + deltas[j]);
				}

				for (int j = 0; j < Elements.Length - 1; j++)
				{
					if (deltas[j] == 0.0)
					{
						tangents[j] = 0.0;
						tangents[j + 1] = 0.0;
					}
					else
					{
						double a = tangents[j] / deltas[j];
						double b = tangents[j + 1] / deltas[j];
						if (a * a + b * b > 9.0)
						{
							double t = 3.0 / System.Math.Sqrt(a * a + b * b);
							tangents[j] = t * a * deltas[j];
							tangents[j + 1] = t * b * deltas[j];
						}
					}
				}

				for (int j = 0; j < Elements.Length; j++)
				{
					Elements[j].CurveCantTangent = tangents[j];
				}
			}
		}

		/// <summary>Smooths all curves / turns</summary>
		public void SmoothTurns(int subdivisions, HostInterface currentHost)
		{
			if (subdivisions < 2)
			{
				throw new InvalidOperationException();
			}

			// subdivide track
			int length = Elements.Length;
			int newLength = (length - 1) * subdivisions + 1;
			double[] midpointsTrackPositions = new double[newLength];
			Vector3[] midpointsWorldPositions = new Vector3[newLength];
			Vector3[] midpointsWorldDirections = new Vector3[newLength];
			Vector3[] midpointsWorldUps = new Vector3[newLength];
			Vector3[] midpointsWorldSides = new Vector3[newLength];
			double[] midpointsCant = new double[newLength];
			TrackFollower follower = new TrackFollower(currentHost);
			for (int i = 0; i < newLength; i++)
			{
				int m = i % subdivisions;
				if (m != 0)
				{
					int q = i / subdivisions;
					
					double r = (double) m / (double) subdivisions;
					double p = (1.0 - r) * Elements[q].StartingTrackPosition + r * Elements[q + 1].StartingTrackPosition;
					follower.UpdateAbsolute(-1.0, true, false);
					follower.UpdateAbsolute(p, true, false);
					midpointsTrackPositions[i] = p;
					midpointsWorldPositions[i] = follower.WorldPosition;
					midpointsWorldDirections[i] = follower.WorldDirection;
					midpointsWorldUps[i] = follower.WorldUp;
					midpointsWorldSides[i] = follower.WorldSide;
					midpointsCant[i] = follower.CurveCant;
				}
			}

			Array.Resize(ref Elements, newLength);
			for (int i = length - 1; i >= 1; i--)
			{
				Elements[subdivisions * i] = Elements[i];
			}

			for (int i = 0; i < Elements.Length; i++)
			{
				int m = i % subdivisions;
				if (m != 0)
				{
					int q = i / subdivisions;
					int j = q * subdivisions;
					Elements[i] = Elements[j];
					Elements[i].Events = new GeneralEvent[] { };
					Elements[i].StartingTrackPosition = midpointsTrackPositions[i];
					Elements[i].WorldPosition = midpointsWorldPositions[i];
					Elements[i].WorldDirection = midpointsWorldDirections[i];
					Elements[i].WorldUp = midpointsWorldUps[i];
					Elements[i].WorldSide = midpointsWorldSides[i];
					Elements[i].CurveCant = midpointsCant[i];
					Elements[i].CurveCantTangent = 0.0;
				}
			}

			// find turns
			bool[] isTurn = new bool[Elements.Length];
			{
				for (int i = 1; i < Elements.Length - 1; i++)
				{
					int m = i % subdivisions;
					if (m == 0)
					{
						double p = 0.00000001 * Elements[i - 1].StartingTrackPosition + 0.99999999 * Elements[i].StartingTrackPosition;
						follower.UpdateAbsolute(p, true, false);
						Vector3 d1 = Elements[i].WorldDirection;
						Vector3 d2 = follower.WorldDirection;
						Vector3 d = d1 - d2;
						double t = d.X * d.X + d.Z * d.Z;
						const double e = 0.0001;
						if (t > e)
						{
							isTurn[i] = true;
						}
					}
				}
			}
			// replace turns by curves
			for (int i = 0; i < Elements.Length; i++)
			{
				if (isTurn[i])
				{
					// estimate radius
					Vector3 AP = Elements[i - 1].WorldPosition;
					Vector3 BP = Elements[i + 1].WorldPosition;
					Vector3 S = Elements[i - 1].WorldSide - Elements[i + 1].WorldSide;
					double rx;
					if (S.X * S.X > 0.000001)
					{
						rx = (BP.X - AP.X) / S.X;
					}
					else
					{
						rx = 0.0;
					}

					double rz;
					if (S.Z * S.Z > 0.000001)
					{
						rz = (BP.Z - AP.Z) / S.Z;
					}
					else
					{
						rz = 0.0;
					}

					if (rx != 0.0 | rz != 0.0)
					{
						double r;
						if (rx != 0.0 & rz != 0.0)
						{
							if (System.Math.Sign(rx) == System.Math.Sign(rz))
							{
								double f = rx / rz;
								if (f > -1.1 & f < -0.9 | f > 0.9 & f < 1.1)
								{
									r = System.Math.Sqrt(System.Math.Abs(rx * rz)) * System.Math.Sign(rx);
								}
								else
								{
									r = 0.0;
								}
							}
							else
							{
								r = 0.0;
							}
						}
						else if (rx != 0.0)
						{
							r = rx;
						}
						else
						{
							r = rz;
						}

						if (r * r > 1.0)
						{
							// apply radius
							Elements[i - 1].CurveRadius = r;
							double p = 0.00000001 * Elements[i - 1].StartingTrackPosition + 0.99999999 * Elements[i].StartingTrackPosition;
							follower.UpdateAbsolute(p - 1.0, true, false);
							follower.UpdateAbsolute(p, true, false);
							Elements[i].CurveRadius = r;
							Elements[i].WorldPosition = follower.WorldPosition;
							Elements[i].WorldDirection = follower.WorldDirection;
							Elements[i].WorldUp = follower.WorldUp;
							Elements[i].WorldSide = follower.WorldSide;
							// iterate to shorten track element length
							p = 0.00000001 * Elements[i].StartingTrackPosition + 0.99999999 * Elements[i + 1].StartingTrackPosition;
							follower.UpdateAbsolute(p - 1.0, true, false);
							follower.UpdateAbsolute(p, true, false);
							Vector3 d = Elements[i + 1].WorldPosition - follower.WorldPosition;
							double bestT = d.NormSquared();
							int bestJ = 0;
							int n = 1000;
							double a = 1.0 / (double) n * (Elements[i + 1].StartingTrackPosition - Elements[i].StartingTrackPosition);
							for (int j = 1; j < n - 1; j++)
							{
								follower.UpdateAbsolute(Elements[i + 1].StartingTrackPosition - (double) j * a, true, false);
								d = Elements[i + 1].WorldPosition - follower.WorldPosition;
								double t = d.NormSquared();
								if (t < bestT)
								{
									bestT = t;
									bestJ = j;
								}
								else
								{
									break;
								}
							}

							double s = (double) bestJ * a;
							for (int j = i + 1; j < Elements.Length; j++)
							{
								Elements[j].StartingTrackPosition -= s;
							}

							// introduce turn to compensate for curve
							p = 0.00000001 * Elements[i].StartingTrackPosition + 0.99999999 * Elements[i + 1].StartingTrackPosition;
							follower.UpdateAbsolute(p - 1.0, true, false);
							follower.UpdateAbsolute(p, true, false);
							Vector3 AB = Elements[i + 1].WorldPosition - follower.WorldPosition;
							Vector3 AC = Elements[i + 1].WorldPosition - Elements[i].WorldPosition;
							Vector3 BC = follower.WorldPosition - Elements[i].WorldPosition;
							double sa = System.Math.Sqrt(BC.X * BC.X + BC.Z * BC.Z);
							double sb = System.Math.Sqrt(AC.X * AC.X + AC.Z * AC.Z);
							double sc = System.Math.Sqrt(AB.X * AB.X + AB.Z * AB.Z);
							double denominator = 2.0 * sa * sb;
							if (denominator != 0.0)
							{
								double originalAngle;
								{
									double value = (sa * sa + sb * sb - sc * sc) / denominator;
									if (value < -1.0)
									{
										originalAngle = System.Math.PI;
									}
									else if (value > 1.0)
									{
										originalAngle = 0;
									}
									else
									{
										originalAngle = System.Math.Acos(value);
									}
								}
								TrackElement originalTrackElement = Elements[i];
								bestT = double.MaxValue;
								bestJ = 0;
								for (int j = -1; j <= 1; j++)
								{
									double g = (double) j * originalAngle;
									Elements[i] = originalTrackElement;
									Elements[i].WorldDirection.Rotate(Vector3.Down, g);
									Elements[i].WorldUp.Rotate(Vector3.Down, g);
									Elements[i].WorldSide.Rotate(Vector3.Down, g);
									p = 0.00000001 * Elements[i].StartingTrackPosition + 0.99999999 * Elements[i + 1].StartingTrackPosition;
									follower.UpdateAbsolute(p - 1.0, true, false);
									follower.UpdateAbsolute(p, true, false);
									d = Elements[i + 1].WorldPosition - follower.WorldPosition;
									double t = d.NormSquared();
									if (t < bestT)
									{
										bestT = t;
										bestJ = j;
									}
								}

								{
									double newAngle = (double) bestJ * originalAngle;
									Elements[i] = originalTrackElement;
									Elements[i].WorldDirection.Rotate(Vector3.Down, newAngle);
									Elements[i].WorldUp.Rotate(Vector3.Down, newAngle);
									Elements[i].WorldSide.Rotate(Vector3.Down, newAngle);
								}
								// iterate again to further shorten track element length
								p = 0.00000001 * Elements[i].StartingTrackPosition + 0.99999999 * Elements[i + 1].StartingTrackPosition;
								follower.UpdateAbsolute(p - 1.0, true, false);
								follower.UpdateAbsolute(p, true, false);
								d = Elements[i + 1].WorldPosition - follower.WorldPosition;
								bestT = d.NormSquared();
								bestJ = 0;
								n = 1000;
								a = 1.0 / (double) n * (Elements[i + 1].StartingTrackPosition - Elements[i].StartingTrackPosition);
								for (int j = 1; j < n - 1; j++)
								{
									follower.UpdateAbsolute(Elements[i + 1].StartingTrackPosition - (double) j * a, true, false);
									d = Elements[i + 1].WorldPosition - follower.WorldPosition;
									double t = d.NormSquared();
									if (t < bestT)
									{
										bestT = t;
										bestJ = j;
									}
									else
									{
										break;
									}
								}

								s = (double) bestJ * a;
								for (int j = i + 1; j < Elements.Length; j++)
								{
									Elements[j].StartingTrackPosition -= s;
								}
							}

							// compensate for height difference
							p = 0.00000001 * Elements[i].StartingTrackPosition + 0.99999999 * Elements[i + 1].StartingTrackPosition;
							follower.UpdateAbsolute(p - 1.0, true, false);
							follower.UpdateAbsolute(p, true, false);
							Vector3 d1 = Elements[i + 1].WorldPosition - Elements[i].WorldPosition;
							double a1 = System.Math.Atan(d1.Y / System.Math.Sqrt(d1.X * d1.X + d1.Z * d1.Z));
							Vector3 d2 = follower.WorldPosition - Elements[i].WorldPosition;
							double a2 = System.Math.Atan(d2.Y / System.Math.Sqrt(d2.X * d2.X + d2.Z * d2.Z));
							double b = a2 - a1;
							if (b * b > 0.00000001)
							{
								Elements[i].WorldDirection.Rotate(Elements[i].WorldSide, b);
								Elements[i].WorldUp.Rotate(Elements[i].WorldSide, b);
							}
						}
					}
				}
			}

			// correct events
			for (int i = 0; i < Elements.Length - 1; i++)
			{
				double startingTrackPosition = Elements[i].StartingTrackPosition;
				double endingTrackPosition = Elements[i + 1].StartingTrackPosition;
				for (int j = 0; j < Elements[i].Events.Length; j++)
				{
					GeneralEvent e = Elements[i].Events[j];
					double p = startingTrackPosition + e.TrackPositionDelta;
					if (p >= endingTrackPosition)
					{
						int len = Elements[i + 1].Events.Length;
						Array.Resize(ref Elements[i + 1].Events, len + 1);
						Elements[i + 1].Events[len] = Elements[i].Events[j];
						e = Elements[i + 1].Events[len];
						e.TrackPositionDelta += startingTrackPosition - endingTrackPosition;
						for (int k = j; k < Elements[i].Events.Length - 1; k++)
						{
							Elements[i].Events[k] = Elements[i].Events[k + 1];
						}

						len = Elements[i].Events.Length;
						Array.Resize(ref Elements[i].Events, len - 1);
						j--;
					}
				}
			}
		}
	}
}
