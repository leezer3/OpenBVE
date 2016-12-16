using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace ObjectBender {
	/// <summary>This class contains methods for bending objects.</summary>
	internal static class Bender {

		/// <summary>Represents options for the BendObject procedure.</summary>
		internal class Options {
			/// <summary>The input file in B3D/CSV format.</summary>
			internal string InputFile;
			/// <summary>The output file in B3D/CSV format.</summary>
			internal string OutputFile;
			/// <summary>Whether to append to the output file instead of overwriting it.</summary>
			internal bool AppendToOutputFile;
			/// <summary>The number of segments.</summary>
			internal int NumberOfSegments;
			/// <summary>The length of each segment in meters.</summary>
			internal double SegmentLength;
			/// <summary>The block length in meters if the object is to be rotated for use as a rail object. Should be left at zero otherwise.</summary>
			internal double BlockLength;
			/// <summary>The curve radius in meters, or zero to not curve.</summary>
			internal double Radius;
			/// <summary>The rail gauge in meters. Positive values represent superelevation, negative ones subelevation, and zero symmetric tilting.</summary>
			internal double RailGauge;
			/// <summary>The cant at the beginning of the object in meters. Positive values represent inward cant, negative ones outward cant. On a straight piece of track, negative values represent the left, positive ones the right.</summary>
			internal double InitialCant;
			/// <summary>The cant at the end of the object in meters. Positive values represent inward cant, negative ones outward cant. On a straight piece of track, negative values represent the left, positive ones the right.</summary>
			internal double FinalCant;
		}
		
		/// <summary>Bends a CSV object.</summary>
		/// <param name="options">The options.</param>
		internal static void BendObject(Options options) {
			/*
			 * Read the input file.
			 * */
			CultureInfo culture = CultureInfo.InvariantCulture;
			string[] lines = File.ReadAllLines(options.InputFile, new UTF8Encoding());
			bool isB3d = options.InputFile.EndsWith(".b3d", StringComparison.OrdinalIgnoreCase);
			string[][] cells = new string[lines.Length][];
			for (int i = 0; i < lines.Length; i++) {
				if (lines[i].Length != 0) {
					string comments;
					int semicolon = lines[i].IndexOf(';');
					if (semicolon >= 0) {
						comments = lines[i].Substring(semicolon + 1);
						lines[i] = lines[i].Substring(0, semicolon).Trim();
					} else {
						comments = string.Empty;
						lines[i] = lines[i].Trim();
					}
					cells[i] = lines[i].Split(',');
					if (isB3d & cells[i].Length != 0) {
						/*
						 * In B3D files, the space is the separator between the command and the
						 * argument sequence. If the first cell contains a space, then split it in two.
						 * */
						int space = cells[i][0].IndexOf(' ');
						if (space >= 0) {
							string command = cells[i][0].Substring(0, space);
							cells[i][0] = cells[i][0].Substring(space + 1);
							string[] temp = new string[cells[i].Length + 1];
							temp[0] = command;
							for (int j = 0; j < cells[i].Length; j++) {
								temp[j + 1] = cells[i][j];
							}
							cells[i] = temp;
						}
					}
					for (int j = 0; j < cells[i].Length; j++) {
						cells[i][j] = cells[i][j].Trim();
					}
					int n = 0;
					for (int j = cells[i].Length - 1; j >= 0; j--) {
						if (cells[i][j].Length != 0) {
							n = j + 1;
							break;
						}
					}
					/*
					 * Parse comments for the special SetTextureCoordinates markup.
					 * */
					if (cells[i].Length != 0 && (cells[i][0].Equals("SetTextureCoordinates", StringComparison.OrdinalIgnoreCase) || cells[i][0].Equals("Coordinates", StringComparison.OrdinalIgnoreCase))) {
						double dx = 0.0;
						double dy = 0.0;
						for (int j = 0; j < comments.Length; j++) {
							if (comments[j] == '{') {
								int start = j + 1;
								j++;
								while (j < comments.Length) {
									if (comments[j] == '}') {
										string command = comments.Substring(start, j - start).Trim();
										int equals = command.IndexOf('=');
										if (equals >= 0) {
											string arg = command.Substring(equals + 1).TrimStart();
											command = command.Substring(0, equals).TrimEnd();
											switch (command.ToLowerInvariant()) {
												case "x":
													if (!double.TryParse(arg, NumberStyles.Float, culture, out dx)) {
														dx = 0.0;
													}
													break;
												case "y":
													if (!double.TryParse(arg, NumberStyles.Float, culture, out dy)) {
														dy = 0.0;
													}
													break;
											}
										}
										break;
									} else {
										j++;
									}
								}
							}
						}
						Array.Resize<string>(ref cells[i], 6);
						cells[i][4] = dx.ToString("R", culture);
						cells[i][5] = dy.ToString("R", culture);
					} else if (n != cells[i].Length) {
						Array.Resize<string>(ref cells[i], n);
					}
				} else {
					cells[i] = new string[] { };
				}
			}
			/*
			 * Substitute Cube and Cylinder commands for their
			 * corresponding AddVertex and AddFace commands.
			 * */
			int numVertices = 0;
			for (int i = 0; i < cells.Length; i++) {
				if (cells[i].Length != 0) {
					switch (cells[i][0].ToLowerInvariant()) {
						case "createmeshbuilder":
						case "[meshbuilder]":
							numVertices = 0;
							break;
						case "addvertex":
						case "vertex":
							numVertices++;
							break;
						case "cube":
							{
								double x = cells[i].Length >= 2 ? double.Parse(cells[i][1], culture) : 0.0;
								double y = cells[i].Length >= 3 ? double.Parse(cells[i][2], culture) : x;
								double z = cells[i].Length >= 4 ? double.Parse(cells[i][3], culture) : x;
								string[][] code = CreateCube(x, y, z, ref numVertices);
								Array.Resize<string[]>(ref cells, cells.Length + code.Length - 1);
								for (int j = cells.Length - 1; j >= i + code.Length; j--) {
									cells[j] = cells[j - code.Length + 1];
								}
								for (int j = 0; j < code.Length; j++) {
									cells[i + j] = code[j];
								}
								i--;
							}
							break;
						case "cylinder":
							{
								int n = cells[i].Length >= 2 ? int.Parse(cells[i][1], culture) : -1;
								double u = cells[i].Length >= 3 ? double.Parse(cells[i][2], culture) : 0.0;
								double l = cells[i].Length >= 4 ? double.Parse(cells[i][3], culture) : 0.0;
								double h = cells[i].Length >= 5 ? double.Parse(cells[i][4], culture) : 0.0;
								string[][] code = CreateCylinder(n, u, l, h, ref numVertices);
								Array.Resize<string[]>(ref cells, cells.Length + code.Length - 1);
								for (int j = cells.Length - 1; j >= i + code.Length; j--) {
									cells[j] = cells[j - code.Length + 1];
								}
								for (int j = 0; j < code.Length; j++) {
									cells[i + j] = code[j];
								}
								i--;
							}
							break;
					}
				}
			}
			/*
			 * Apply the Translate(All), Scale(All), Rotate(All) and Shear(All)
			 * commands to the corresponding previous AddVertex commands.
			 * */
			for (int i = 0; i < cells.Length; i++) {
				if (cells[i].Length != 0) {
					switch (cells[i][0].ToLowerInvariant()) {
						case "translate":
						case "translateall":
							{
								double x = cells[i].Length >= 2 ? double.Parse(cells[i][1], culture) : 0.0;
								double y = cells[i].Length >= 3 ? double.Parse(cells[i][2], culture) : 0.0;
								double z = cells[i].Length >= 4 ? double.Parse(cells[i][3], culture) : 0.0;
								bool endAtMeshBuilder = cells[i][0].Equals("Translate", StringComparison.OrdinalIgnoreCase);
								for (int j = i - 1; j >= 0; j--) {
									if (cells[j].Length != 0) {
										if (cells[j][0].Equals("AddVertex", StringComparison.OrdinalIgnoreCase) || cells[j][0].Equals("Vertex", StringComparison.OrdinalIgnoreCase)) {
											double vx = cells[j].Length >= 2 ? double.Parse(cells[j][1], culture) : 0.0;
											double vy = cells[j].Length >= 3 ? double.Parse(cells[j][2], culture) : 0.0;
											double vz = cells[j].Length >= 4 ? double.Parse(cells[j][3], culture) : 0.0;
											double nx = cells[j].Length >= 5 ? double.Parse(cells[j][4], culture) : 0.0;
											double ny = cells[j].Length >= 6 ? double.Parse(cells[j][5], culture) : 0.0;
											double nz = cells[j].Length >= 7 ? double.Parse(cells[j][6], culture) : 0.0;
											Normalize(ref nx, ref ny, ref nz);
											vx += x;
											vy += y;
											vz += z;
											cells[j] = new string[] { isB3d ? "Vertex" : "AddVertex", vx.ToString("R", culture), vy.ToString("R", culture), vz.ToString("R", culture), nx.ToString("R", culture), ny.ToString("R", culture), nz.ToString("R", culture) };
										} else if (endAtMeshBuilder && (cells[j][0].Equals("CreateMeshBuilder", StringComparison.OrdinalIgnoreCase) || cells[j][0].Equals("[MeshBuilder]", StringComparison.OrdinalIgnoreCase))) {
											break;
										}
									}
								}
								cells[i] = new string[] { };
							}
							break;
						case "scale":
						case "scaleall":
							{
								double x = cells[i].Length >= 2 ? double.Parse(cells[i][1], culture) : 1.0;
								double y = cells[i].Length >= 3 ? double.Parse(cells[i][2], culture) : 1.0;
								double z = cells[i].Length >= 4 ? double.Parse(cells[i][3], culture) : 1.0;
								bool reverseFaceOrder = x * y * z < 0.0;
								bool endAtMeshBuilder = cells[i][0].Equals("Scale", StringComparison.OrdinalIgnoreCase);
								for (int j = i - 1; j >= 0; j--) {
									if (cells[j].Length != 0) {
										if (cells[j][0].Equals("AddVertex", StringComparison.OrdinalIgnoreCase) || cells[j][0].Equals("Vertex", StringComparison.OrdinalIgnoreCase)) {
											double vx = cells[j].Length >= 2 ? double.Parse(cells[j][1], culture) : 0.0;
											double vy = cells[j].Length >= 3 ? double.Parse(cells[j][2], culture) : 0.0;
											double vz = cells[j].Length >= 4 ? double.Parse(cells[j][3], culture) : 0.0;
											double nx = cells[j].Length >= 5 ? double.Parse(cells[j][4], culture) : 0.0;
											double ny = cells[j].Length >= 6 ? double.Parse(cells[j][5], culture) : 0.0;
											double nz = cells[j].Length >= 7 ? double.Parse(cells[j][6], culture) : 0.0;
											Normalize(ref nx, ref ny, ref nz);
											Scale(ref vx, ref vy, ref vz, ref nx, ref ny, ref nz, x, y, z);
											cells[j] = new string[] { isB3d ? "Vertex" : "AddVertex", vx.ToString("R", culture), vy.ToString("R", culture), vz.ToString("R", culture), nx.ToString("R", culture), ny.ToString("R", culture), nz.ToString("R", culture) };
										} else if (reverseFaceOrder && (cells[j][0].Equals("AddFace", StringComparison.OrdinalIgnoreCase) || cells[j][0].Equals("AddFace2", StringComparison.OrdinalIgnoreCase) || cells[j][0].Equals("Face", StringComparison.OrdinalIgnoreCase) || cells[j][0].Equals("Face2", StringComparison.OrdinalIgnoreCase))) {
											for (int k = 1; k <= (cells[j].Length - 1) / 2; k++) {
												int l = cells[j].Length - k;
												string temp = cells[j][l];
												cells[j][l] = cells[j][k];
												cells[j][k] = temp;
											}
										} else if (endAtMeshBuilder && (cells[j][0].Equals("CreateMeshBuilder", StringComparison.OrdinalIgnoreCase) || cells[j][0].Equals("[MeshBuilder]", StringComparison.OrdinalIgnoreCase))) {
											break;
										}
									}
								}
								cells[i] = new string[] { };
							}
							break;
						case "rotate":
						case "rotateall":
							{
								double x = cells[i].Length >= 2 ? double.Parse(cells[i][1], culture) : 0.0;
								double y = cells[i].Length >= 3 ? double.Parse(cells[i][2], culture) : 0.0;
								double z = cells[i].Length >= 4 ? double.Parse(cells[i][3], culture) : 0.0;
								double t = x * x + y * y + z * z;
								if (t != 0.0) {
									t = 1.0 / Math.Sqrt(t);
									x *= t;
									y *= t;
									z *= t;
								} else {
									x = 1.0;
									y = 0.0;
									z = 0.0;
								}
								double a = cells[i].Length >= 5 ? double.Parse(cells[i][4], culture) : 0.0;
								const double degrees = 0.0174532925199433;
								a *= degrees;
								bool endAtMeshBuilder = cells[i][0].Equals("Rotate", StringComparison.OrdinalIgnoreCase);
								for (int j = i - 1; j >= 0; j--) {
									if (cells[j].Length != 0) {
										if (cells[j][0].Equals("AddVertex", StringComparison.OrdinalIgnoreCase) || cells[j][0].Equals("Vertex", StringComparison.OrdinalIgnoreCase)) {
											double vx = cells[j].Length >= 2 ? double.Parse(cells[j][1], culture) : 0.0;
											double vy = cells[j].Length >= 3 ? double.Parse(cells[j][2], culture) : 0.0;
											double vz = cells[j].Length >= 4 ? double.Parse(cells[j][3], culture) : 0.0;
											double nx = cells[j].Length >= 5 ? double.Parse(cells[j][4], culture) : 0.0;
											double ny = cells[j].Length >= 6 ? double.Parse(cells[j][5], culture) : 0.0;
											double nz = cells[j].Length >= 7 ? double.Parse(cells[j][6], culture) : 0.0;
											Normalize(ref nx, ref ny, ref nz);
											Rotate(ref vx, ref vy, ref vz, x, y, z, Math.Cos(a), Math.Sin(a));
											Rotate(ref nx, ref ny, ref nz, x, y, z, Math.Cos(a), Math.Sin(a));
											cells[j] = new string[] { isB3d ? "Vertex" : "AddVertex", vx.ToString("R", culture), vy.ToString("R", culture), vz.ToString("R", culture), nx.ToString("R", culture), ny.ToString("R", culture), nz.ToString("R", culture) };
										} else if (endAtMeshBuilder && (cells[j][0].Equals("CreateMeshBuilder", StringComparison.OrdinalIgnoreCase) || cells[j][0].Equals("[MeshBuilder]", StringComparison.OrdinalIgnoreCase))) {
											break;
										}
									}
								}
								cells[i] = new string[] { };
							}
							break;
						case "shear":
						case "shearall":
							{
								double dx = cells[i].Length >= 2 ? double.Parse(cells[i][1], culture) : 0.0;
								double dy = cells[i].Length >= 3 ? double.Parse(cells[i][2], culture) : 0.0;
								// double dz = cells[i].Length >= 4 ? double.Parse(cells[i][3], culture) : 0.0;
								double sx = cells[i].Length >= 5 ? double.Parse(cells[i][4], culture) : 0.0;
								double sy = cells[i].Length >= 6 ? double.Parse(cells[i][5], culture) : 0.0;
								double sz = cells[i].Length >= 7 ? double.Parse(cells[i][6], culture) : 0.0;
								double r = cells[i].Length >= 8 ? double.Parse(cells[i][7], culture) : 0.0;
								bool endAtMeshBuilder = cells[i][0].Equals("Shear", StringComparison.OrdinalIgnoreCase);
								for (int j = i - 1; j >= 0; j--) {
									if (cells[j].Length != 0) {
										if (cells[j][0].Equals("AddVertex", StringComparison.OrdinalIgnoreCase) || cells[j][0].Equals("Vertex", StringComparison.OrdinalIgnoreCase)) {
											double vx = cells[j].Length >= 2 ? double.Parse(cells[j][1], culture) : 0.0;
											double vy = cells[j].Length >= 3 ? double.Parse(cells[j][2], culture) : 0.0;
											double vz = cells[j].Length >= 4 ? double.Parse(cells[j][3], culture) : 0.0;
											double nx = cells[j].Length >= 5 ? double.Parse(cells[j][4], culture) : 0.0;
											double ny = cells[j].Length >= 6 ? double.Parse(cells[j][5], culture) : 0.0;
											double nz = cells[j].Length >= 7 ? double.Parse(cells[j][6], culture) : 0.0;
											Normalize(ref nx, ref ny, ref nz);
											Shear(ref vx, ref vy, ref vz, ref nx, ref ny, ref nz, dx, dy, dx, sx, sy, sz, r);
											cells[j] = new string[] { isB3d ? "Vertex" : "AddVertex", vx.ToString("R", culture), vy.ToString("R", culture), vz.ToString("R", culture), nx.ToString("R", culture), ny.ToString("R", culture), nz.ToString("R", culture) };
										} else if (endAtMeshBuilder && (cells[j][0].Equals("CreateMeshBuilder", StringComparison.OrdinalIgnoreCase) || cells[j][0].Equals("[MeshBuilder]", StringComparison.OrdinalIgnoreCase))) {
											break;
										}
									}
								}
								cells[i] = new string[] { };
							}
							break;
					}
				}
			}
			/*
			 * Now that all geometry-related commands have been
			 * taken care of, create the output file by duplicating
			 * and bending the post-processed input file.
			 * */
			double cosB, sinB;
			bool rotateB;
			if (options.BlockLength * options.Radius != 0.0) {
				double b = -0.5 * options.BlockLength / options.Radius;
				cosB = Math.Cos(b);
				sinB = Math.Sin(b);
				rotateB = true;
			} else {
				cosB = 0.0;
				sinB = 0.0;
				rotateB = false;
			}
			double initialCantAngle, finalCantAngle;
			bool cant, quadratic;
			double quadraticThreshold;
			if ((options.InitialCant != 0.0 | options.FinalCant != 0.0) & options.NumberOfSegments > 0 & options.SegmentLength != 0.0 & options.RailGauge != 0.0) {
				initialCantAngle = Math.Asin(options.InitialCant / options.RailGauge);
				finalCantAngle = Math.Asin(options.FinalCant / options.RailGauge);
				cant = true;
				quadratic = Math.Sign(options.InitialCant) == -Math.Sign(options.FinalCant);
				if (quadratic) {
					quadraticThreshold = options.InitialCant / (options.InitialCant - options.FinalCant);
				} else {
					quadraticThreshold = 0.0;
				}
			} else {
				initialCantAngle = 0.0;
				finalCantAngle = 0.0;
				cant = false;
				quadratic = false;
				quadraticThreshold = 0.0;
			}
			StringBuilder builder = new StringBuilder();
			for (int i = 0; i < options.NumberOfSegments; i++) {
				if (i != 0) {
					builder.AppendLine();
				}
				builder.AppendLine("; --- segment " + i.ToString(culture) + " ---");
				builder.AppendLine();
				for (int j = 0; j < cells.Length; j++) {
					if (cells[j].Length != 0) {
						switch (cells[j][0].ToLowerInvariant()) {
							case "vertex":
							case "addvertex":
								{
									double vx = cells[j].Length >= 2 ? double.Parse(cells[j][1], culture) : 0.0;
									double vy = cells[j].Length >= 3 ? double.Parse(cells[j][2], culture) : 0.0;
									double vz = cells[j].Length >= 4 ? double.Parse(cells[j][3], culture) : 0.0;
									double nx = cells[j].Length >= 5 ? double.Parse(cells[j][4], culture) : 0.0;
									double ny = cells[j].Length >= 6 ? double.Parse(cells[j][5], culture) : 0.0;
									double nz = cells[j].Length >= 7 ? double.Parse(cells[j][6], culture) : 0.0;
									Normalize(ref nx, ref ny, ref nz);
									vz += (double)i * options.SegmentLength;
									if (cant) {
										double f = vz / ((double)options.NumberOfSegments * options.SegmentLength);
										if (quadratic) {
											/*
											 * If the initial and final cants are of opposite sign,
											 * we use a special interpolation to smoothen out the
											 * transition.
											 * */
											if (f < quadraticThreshold) {
												f = f * (2.0 - f / quadraticThreshold);
											} else {
												f = (quadraticThreshold - 2.0 * quadraticThreshold * f + f * f) / (1.0 - quadraticThreshold);
											}
										}
										double c = (1.0 - f) * initialCantAngle + f * finalCantAngle;
										if (options.Radius < 0.0) {
											c = -c;
										}
										double sign = (double)Math.Sign(c);
										double cosC = Math.Cos(c);
										double sinC = Math.Sin(c);
										vx -= 0.5 * options.RailGauge * sign;
										Rotate(ref vx, ref vy, ref vz, 0.0, 0.0, -1.0, cosC, sinC);
										Rotate(ref nx, ref ny, ref nz, 0.0, 0.0, -1.0, cosC, sinC);
										vx += 0.5 * options.RailGauge * sign;
									}
									if (options.Radius != 0.0) {
										double r = options.Radius - vx;
										double a = vz / options.Radius;
										double cosA = Math.Cos(a);
										double sinA = Math.Sin(a);
										vx = options.Radius - cosA * r;
										vz = sinA * r;
										Rotate(ref nx, ref ny, ref nz, 0.0, 1.0, 0.0, cosA, sinA);
									}
									if (rotateB) {
										Rotate(ref vx, ref vy, ref vz, 0.0, 1.0, 0.0, cosB, sinB);
										Rotate(ref nx, ref ny, ref nz, 0.0, 1.0, 0.0, cosB, sinB);
									}
									if (isB3d) {
										if (nx == 0.0 & ny == 0.0 & nz == 0.0) {
											builder.AppendLine("Vertex " + vx.ToString("0.000", culture) + "," + vy.ToString("0.000", culture) + "," + vz.ToString("0.000", culture));
										} else {
											builder.AppendLine("Vertex " + vx.ToString("0.000", culture) + "," + vy.ToString("0.000", culture) + "," + vz.ToString("0.000", culture) + "," + nx.ToString("0.000", culture) + "," + ny.ToString("0.000", culture) + "," + nz.ToString("0.000", culture));
										}
									} else {
										if (nx == 0.0 & ny == 0.0 & nz == 0.0) {
											builder.AppendLine("AddVertex," + vx.ToString("0.000", culture) + "," + vy.ToString("0.000", culture) + "," + vz.ToString("0.000", culture));
										} else {
											builder.AppendLine("AddVertex," + vx.ToString("0.000", culture) + "," + vy.ToString("0.000", culture) + "," + vz.ToString("0.000", culture) + "," + nx.ToString("0.000", culture) + "," + ny.ToString("0.000", culture) + "," + nz.ToString("0.000", culture));
										}
									}
								}
								break;
							case "settexturecoordinates":
							case "coordinates":
								{
									int k = cells[j].Length >= 2 ? int.Parse(cells[j][1], culture) : 0;
									double x = cells[j].Length >= 3 ? double.Parse(cells[j][2], culture) : 0.0;
									double y = cells[j].Length >= 4 ? double.Parse(cells[j][3], culture) : 0.0;
									double dx = cells[j].Length >= 5 ? double.Parse(cells[j][4], culture) : 0.0;
									double dy = cells[j].Length >= 6 ? double.Parse(cells[j][5], culture) : 0.0;
									x += (double)i * dx;
									y += (double)i * dy;
									if (isB3d) {
										builder.AppendLine("Coordinates " + k.ToString(culture) + "," + x.ToString("0.000", culture) + "," + y.ToString("0.000", culture));
									} else {
										builder.AppendLine("SetTextureCoordinates," + k.ToString(culture) + "," + x.ToString("0.000", culture) + "," + y.ToString("0.000", culture));
									}
								}
								break;
							default:
								if (isB3d) {
									if (cells[j].Length == 0) {
										builder.AppendLine();
									} else if (cells[j].Length == 1) {
										builder.AppendLine(cells[j][0]);
									} else {
										builder.AppendLine(cells[j][0] + " " + string.Join(",", cells[j], 1, cells[j].Length - 1));
									}
								} else {
									builder.AppendLine(string.Join(",", cells[j]));
								}
								break;
						}
					}
				}
			}
			/*
			 * Write the output file.
			 * */
			if (options.AppendToOutputFile && File.Exists(options.OutputFile)) {
				string text = File.ReadAllText(options.OutputFile, new UTF8Encoding());
				StringBuilder temp = new StringBuilder();
				temp.AppendLine(text);
				temp.Append(builder);
				File.WriteAllText(options.OutputFile, temp.ToString(), new UTF8Encoding(true));
			} else {
				File.WriteAllText(options.OutputFile, builder.ToString(), new UTF8Encoding(true));
			}
		}

		/// <summary>Creates CSV code for a cube.</summary>
		/// <param name="x">The half-width.</param>
		/// <param name="y">The half-height.</param>
		/// <param name="z">The half-depth.</param>
		/// <param name="numVertices">The number of vertices used before in the current mesh builder.</param>
		/// <returns>The cells of the CSV code for the cube.</returns>
		private static string[][] CreateCube(double x, double y, double z, ref int numVertices) {
			CultureInfo culture = CultureInfo.InvariantCulture;
			string px = x.ToString("R", culture);
			string mx = (-x).ToString("R", culture);
			string py = y.ToString("R", culture);
			string my = (-y).ToString("R", culture);
			string pz = z.ToString("R", culture);
			string mz = (-z).ToString("R", culture);
			string v0 = (numVertices + 0).ToString(culture);
			string v1 = (numVertices + 1).ToString(culture);
			string v2 = (numVertices + 2).ToString(culture);
			string v3 = (numVertices + 3).ToString(culture);
			string v4 = (numVertices + 4).ToString(culture);
			string v5 = (numVertices + 5).ToString(culture);
			string v6 = (numVertices + 6).ToString(culture);
			string v7 = (numVertices + 7).ToString(culture);
			numVertices += 8;
			return new string[][] {
				new string[] { "AddVertex", px, py, mz },
				new string[] { "AddVertex", px, my, mz },
				new string[] { "AddVertex", mx, my, mz },
				new string[] { "AddVertex", mx, py, mz },
				new string[] { "AddVertex", px, py, pz },
				new string[] { "AddVertex", px, my, pz },
				new string[] { "AddVertex", mx, my, pz },
				new string[] { "AddVertex", mx, py, pz },
				new string[] { "AddFace", v0, v1, v2, v3 },
				new string[] { "AddFace", v0, v4, v5, v1 },
				new string[] { "AddFace", v0, v3, v7, v4 },
				new string[] { "AddFace", v6, v5, v4, v7 },
				new string[] { "AddFace", v6, v7, v3, v2 },
				new string[] { "AddFace", v6, v2, v1, v5 }
			};
		}

		/// <summary>Creates CSV code for a cylinder.</summary>
		/// <param name="segments">The number of segments.</param>
		/// <param name="upperRadius">The upper radius.</param>
		/// <param name="lowerRadius">The lower radius.</param>
		/// <param name="height">The height.</param>
		/// <param name="numVertices">The number of vertices used before in the current mesh builder.</param>
		/// <returns>The cells of the CSV code for the cylinder.</returns>
		private static string[][] CreateCylinder(int segments, double upperRadius, double lowerRadius, double height, ref int numVertices) {
			CultureInfo culture = CultureInfo.InvariantCulture;
			bool upperCap = upperRadius > 0.0;
			bool lowerCap = lowerRadius > 0.0;
			upperRadius = Math.Abs(upperRadius);
			lowerRadius = Math.Abs(lowerRadius);
			double angleCurrent = 0.0;
			double angleDelta = 2.0 * Math.PI / (double)segments;
			double slope = height != 0.0 ? Math.Atan((lowerRadius - upperRadius) / height) : 0.0;
			double cosSlope = Math.Cos(slope);
			double sinSlope = Math.Sin(slope);
			double halfHeight = 0.5 * height;
			double signHeight = (double)Math.Sign(height);
			string[][] cells = new string[3 * segments + (upperCap ? 1 : 0) + (lowerCap ? 1 : 0)][];
			int cellPointer = 0;
			/* Vertices */
			for (int i = 0; i < segments; i++) {
				double dirX = Math.Cos(angleCurrent);
				double dirZ = Math.Sin(angleCurrent);
				double lowerX = dirX * lowerRadius;
				double lowerZ = dirZ * lowerRadius;
				double upperX = dirX * upperRadius;
				double upperZ = dirZ * upperRadius;
				double nx = dirX * signHeight;
				double ny = 0.0;
				double nz = dirZ * signHeight;
				double ux = 0.0;
				double uy = 1.0;
				double uz = 0.0;
				double rx, ry, rz;
				Cross(nx, ny, nz, ux, uy, uz, out rx, out ry, out rz);
				Rotate(ref nx, ref ny, ref nz, rx, ry, rz, cosSlope, sinSlope);
				cells[cellPointer + 0] = new string[] { "AddVertex", upperX.ToString("R", culture), halfHeight.ToString("R", culture), upperZ.ToString("R", culture), nx.ToString("R", culture), ny.ToString("R", culture), nz.ToString("R", culture) };
				cells[cellPointer + 1] = new string[] { "AddVertex", lowerX.ToString("R", culture), (-halfHeight).ToString("R", culture), lowerZ.ToString("R", culture), nx.ToString("R", culture), ny.ToString("R", culture), nz.ToString("R", culture) };
				cellPointer += 2;
				angleCurrent += angleDelta;
			}
			/* Faces for cylindric wall */
			for (int i = 0; i < segments; i++) {
				int i0 = numVertices + ((2 * i + 2) % (2 * segments));
				int i1 = numVertices + ((2 * i + 3) % (2 * segments));
				int i2 = numVertices + (2 * i + 1);
				int i3 = numVertices + (2 * i);
				cells[cellPointer] = new string[] { "AddFace", i0.ToString(culture), i1.ToString(culture), i2.ToString(culture), i3.ToString(culture) };
				cellPointer++;
			}
			/* Face for upper cap */
			if (upperCap) {
				cells[cellPointer] = new string[1 + segments];
				cells[cellPointer][0] = "AddFace";
				for (int i = 0; i < segments; i++) {
					cells[cellPointer][1 + i] = (numVertices + 2 * (segments - i - 1)).ToString(culture);
				}
				cellPointer++;
			}
			/* Face for lower cap */
			if (lowerCap) {
				cells[cellPointer] = new string[1 + segments];
				cells[cellPointer][0] = "AddFace";
				for (int i = 0; i < segments; i++) {
					cells[cellPointer][1 + i] = (numVertices + 2 * i + 1).ToString(culture);
				}
				cellPointer++;
			}
			return cells;
		}
		
		/// <summary>Normalizes a vector.</summary>
		private static void Normalize(ref double x, ref double y, ref double z) {
			double t = x * x + y * y + z * z;
			if (t != 0.0) {
				t = 1.0 / Math.Sqrt(t);
				x *= t;
				y *= t;
				z *= t;
			}
		}
		
		/// <summary>Crosses two vectors.</summary>
		private static void Cross(double ax, double ay, double az, double bx, double by, double bz, out double cx, out double cy, out double cz) {
			cx = -az * by + ay * bz;
			cy = az * bx - ax * bz;
			cz = -ay * bx + ax * by;
		}

		/// <summary>Rotates a vector.</summary>
		private static void Rotate(ref double vx, ref double vy, ref double vz, double dx, double dy, double dz, double cos, double sin) {
			double versin = 1.0 - cos;
			double x = (cos + versin * dx * dx) * vx + (versin * dx * dy - sin * dz) * vy + (versin * dx * dz + sin * dy) * vz;
			double y = (cos + versin * dy * dy) * vy + (versin * dx * dy + sin * dz) * vx + (versin * dy * dz - sin * dx) * vz;
			double z = (cos + versin * dz * dz) * vz + (versin * dx * dz - sin * dy) * vx + (versin * dy * dz + sin * dx) * vy;
			vx = x;
			vy = y;
			vz = z;
		}
		
		/// <summary>Scales a vector.</summary>
		private static void Scale(ref double vx, ref double vy, ref double vz, ref double nx, ref double ny, ref double nz, double x, double y, double z) {
			vx *= x;
			vy *= y;
			vz *= z;
			double nx2 = nx * nx;
			double ny2 = ny * ny;
			double nz2 = nz * nz;
			double t = nx2 / (x * x) + ny2 / (y * y) + nz2 / (z * z);
			if (t != 0.0) {
				t = Math.Sqrt((nx2 + ny2 + nz2) / t);
				nx *= t / x;
				ny *= t / y;
				nz *= t / z;
			}
		}
		
		/// <summary>Shears a vector.</summary>
		private static void Shear(ref double vx, ref double vy, ref double vz, ref double nx, ref double ny, ref double nz, double dx, double dy, double dz, double sx, double sy, double sz, double r) {
			/* Shear the vector */
			double factor = r * (vx * dx + vy * dy + vz * dz);
			vx += sx * factor;
			vy += sy * factor;
			vz += sz * factor;
			/* Shear the normal */
			factor = r * (nx * sx + ny * sy + nz * sz);
			nx += dx * factor;
			ny += dy * factor;
			nz += dz * factor;
			double t = nx * nx + ny * ny + nz * nz;
			if (t != 0.0) {
				t = 1.0 / Math.Sqrt(t);
				nx *= t;
				ny *= t;
				nz *= t;
			}
		}

	}
}