using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Textures;

namespace Plugin
{
	public partial class Plugin
	{
		private static bool IsCommand(string Text)
		{
			switch (Text.Trim(new char[] { }).ToLowerInvariant())
			{
				case "rotate":
				case "translate":
				case "vertex":
				case "face":
					return true;
				
			}

			return false;
		}

		private static readonly string[] CsvCommands =
		{
			"createmeshbuilder",
			"addvertex",
			"addface", //No need to add Face2, as this is a substring of it
			"setcolor",
			"translate",
			"rotate",
			"generatenormals", //Useless, but might have a second column
			"loadtexture",
			"settexturecoordinates",
			"setemissivecolor",
			"setdecaltransparentcolor",
			"enablecrossfading"
		};

		private static int SecondIndexOfAny(string testString, string[] values)
		{
			if (testString == null)
			{
				return -1;
			}
			int first = -1;
			foreach (string item in values) {
				int offset = testString.IndexOf(item, StringComparison.CurrentCultureIgnoreCase);
				if (offset >= 0) {
					//Found a command in the string
					foreach (string secondItem in values) {
						int secondOffset = testString.IndexOf(secondItem, offset + 1, StringComparison.CurrentCultureIgnoreCase);
						if (secondOffset >= 0) {
							//Found a second command in the string so return this offset
							return secondOffset;
						}
					}
				}
			}
			return first;
		}

		/// <summary>Loads a CSV or B3D object from a file.</summary>
		/// <param name="FileName">The text file to load the CSB or B3D object from. Must be an absolute file name.</param>
		/// <param name="Encoding">The encoding the file is saved in. If the file uses a byte order mark, the encoding indicated by the byte order mark is used and the Encoding parameter is ignored.</param>
		/// <returns>The object loaded.</returns>
		private static StaticObject ReadObject(string FileName, Encoding Encoding) {
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			bool IsB3D = string.Equals(System.IO.Path.GetExtension(FileName), ".b3d", StringComparison.OrdinalIgnoreCase);
			// initialize object
			StaticObject Object = new StaticObject(currentHost);
			string fileHash = Path.GetChecksum(FileName);
			bool multiColumn = !IsB3D;
			switch (fileHash)
			{
				case "C605832CCDC73883AC4A557EB3AD0D3EAAB678D64B2659D6A342EAD5BEECD2B9":
				case "5283E317C51CBAB6015C178D425C06724225605D0662530AE93B850A908FE55A":
				case "3078438BCF679CB1DE2F2372A8791A2C10D29FAED6871C25348EE5C2F977C2E6":
				case "0F928CE5BC277C056E7174545C8DA9326D99463DA59C541317FC831D3443E273":
					/*
					 * RailJet 2012 objects
					 * Total mess....
					 */
					multiColumn = false;
					break;
			}
			// read lines
			List<string> Lines = System.IO.File.ReadAllLines(FileName, Encoding).ToList();
			if (enabledHacks.BveTsHacks && multiColumn)
			{
				/*
				 * Handles multi-column CSV objects [Hide behind the hacks option in the main program]
				 */
				for (int i = 0; i < Lines.Count; i++)
				{
					int idx = SecondIndexOfAny(Lines[i], CsvCommands);
					if (idx != -1)
					{
						string s = Lines[i].Substring(idx);
						string e = System.IO.Path.GetExtension(s).TrimEnd(',');
						if (!string.IsNullOrEmpty(e))
						{
							switch (e.ToLowerInvariant())
							{
								case ".bmp":
								case ".png":
								case ".gif":
								case ".ace":
									/*
									 * Texture file named as a command:
									 * https://github.com/leezer3/OpenBVE/issues/613
									 */
									continue;
								default:
									//Carry on
									break;
							}
						}
						Lines[i] = Lines[i].Substring(0, idx);
						Lines.Add(s);
					}
				}
			}
			// parse lines
			bool firstMeshBuilder = false;
			MeshBuilder Builder = new MeshBuilder(currentHost);
			List<Vector3> Normals = new List<Vector3>();
			bool CommentStarted = false;
			Color24? lastTransparentColor = null;
			for (int i = 0; i < Lines.Count; i++) {
				{
					// Strip OpenBVE original standard comments
					int j = Lines[i].IndexOf(';');
					if (j >= 0)
					{
						Lines[i] = Lines[i].Substring(0, j);
					}
					// Strip double backslash comments
					int k = Lines[i].IndexOf("//", StringComparison.Ordinal);
					if (k >= 0)
					{
						if (!IsPotentialPath(Lines[i]))
						{
							//HACK: Handles malformed potential paths
							Lines[i] = Lines[i].Substring(0, k);
						}
						
					}
					//Strip star backslash comments
					if (!CommentStarted)
					{
						int l = Lines[i].IndexOf("/*", StringComparison.Ordinal);
						if (l >= 0)
						{
							CommentStarted = true;
							string Part1 = Lines[i].Substring(0, l);
							int m = Lines[i].IndexOf("*/", StringComparison.Ordinal);
							string Part2 = "";
							if (m >= 0)
							{
								Part2 = Lines[i].Substring(m + 2, Lines[i].Length - 2);
							}
							Lines[i] = String.Concat(Part1, Part2);
						}
					}
					else
					{
						int l = Lines[i].IndexOf("*/", StringComparison.Ordinal);
						if (l >= 0)
						{
							CommentStarted = false;
							if (l + 2 != Lines[i].Length)
							{
								Lines[i] = Lines[i].Substring(l + 2, (Lines[i].Length - 2));
							}
							else
							{
								Lines[i] = "";
							}
						}
						else
						{
							Lines[i] = "";
						}
					}
				}
				// collect arguments
				string[] Arguments = Lines[i].Split(new[] { ',' }, StringSplitOptions.None);
				for (int j = 0; j < Arguments.Length; j++) {
					Arguments[j] = Arguments[j].Trim(new char[] { });
				}
				{
					// remove unused arguments at the end of the chain
					int j;
					for (j = Arguments.Length - 1; j >= 0; j--) {
						if (Arguments[j].Length != 0) break;
					}
					Array.Resize(ref Arguments, j + 1);
				}
				// style
				string Command;
				if (IsB3D & Arguments.Length != 0) {
					// b3d
					int j = Arguments[0].IndexOf(' ');
					if (j >= 0)
					{
						Command = Arguments[0].Substring(0, j).TrimEnd(new char[] { });
						Arguments[0] = Arguments[0].Substring(j + 1).TrimStart(new char[] { });
					} else {
						Command = Arguments[0];
						bool resetArguments = true;
						if (Arguments.Length != 1) {
							if (!enabledHacks.BveTsHacks || !IsCommand(Command))
							{
								currentHost.AddMessage(MessageType.Error, false, "Invalid syntax at line " + (i + 1).ToString(Culture) + " in file " + FileName);
							}
							else
							{
								resetArguments = false;
							}
						}
						if (resetArguments)
						{
							Arguments = new string[] { };
						}
						else
						{
							/*
							 * This handles object files where the author has omitted the FIRST number in a statement, e.g.
							 * rotate ,,1,30
							 *
							 * As this is missing, we don't detect it as a command
							 *
							 * Traffic light poles broken in the Neustadt tram routes without this
							 */
							Arguments[0] = string.Empty;
						}

					}
				} else if (Arguments.Length != 0) {
					// csv
					Command = Arguments[0];
					for (int j = 0; j < Arguments.Length - 1; j++) {
						Arguments[j] = Arguments[j + 1];
					}
					Array.Resize(ref Arguments, Arguments.Length - 1);
				} else {
					// empty
					Command = null;
				}
				// parse terms
				if (Command != null)
				{
					string cmd = Command.ToLowerInvariant();
					switch(cmd) {
						case "createmeshbuilder":
						case "[meshbuilder]":
							{
								firstMeshBuilder = true;
								if (cmd == "createmeshbuilder" & IsB3D) {
									currentHost.AddMessage(MessageType.Warning, false, "CreateMeshBuilder is not a supported command - did you mean [MeshBuilder]? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else if (cmd == "[meshbuilder]" & !IsB3D) {
									currentHost.AddMessage(MessageType.Warning, false, "[MeshBuilder] is not a supported command - did you mean CreateMeshBuilder? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length > 0) {
									currentHost.AddMessage(MessageType.Warning, false, "0 arguments are expected in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								Builder.Apply(ref Object, enabledHacks.BveTsHacks);
								Builder = new MeshBuilder(currentHost);
								Normals = new List<Vector3>();
							} break;
						case "addvertex":
						case "vertex":
							{
								if (!firstMeshBuilder)
								{
									//https://github.com/leezer3/OpenBVE/issues/448
									currentHost.AddMessage(MessageType.Warning, false, "Attempted to add a vertex without first creating a MeshBuilder - This may produce unexpected results - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (cmd == "addvertex" & IsB3D) {
									currentHost.AddMessage(MessageType.Warning, false, "AddVertex is not a supported command - did you mean Vertex? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else if (cmd == "vertex" & !IsB3D) {
									currentHost.AddMessage(MessageType.Warning, false, "Vertex is not a supported command - did you mean AddVertex? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length > 6) {
									currentHost.AddMessage(MessageType.Warning, false, "At most 6 arguments are expected in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}

								Vertex currentVertex = new Vertex();
								if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out currentVertex.Coordinates.X)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument vX in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									currentVertex.Coordinates.X = 0.0;
								}
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out currentVertex.Coordinates.Y)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument vY in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									currentVertex.Coordinates.Y = 0.0;
								}
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], out currentVertex.Coordinates.Z)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument vZ in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									currentVertex.Coordinates.Z = 0.0;
								}
								Vector3 currentNormal = new Vector3();
								if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[3], out currentNormal.X)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument nX in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									currentNormal.X = 0.0;
								}
								if (Arguments.Length >= 5 && Arguments[4].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[4], out currentNormal.Y)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument nY in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									currentNormal.Y = 0.0;
								}
								if (Arguments.Length >= 6 && Arguments[5].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[5], out currentNormal.Z)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument nZ in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									currentNormal.Z = 0.0;
								}
								currentNormal.Normalize();
								Builder.Vertices.Add(currentVertex);
								Normals.Add(currentNormal);
							} break;
						case "addface":
						case "addface2":
						case "face":
						case "face2":
							{
								if (IsB3D) {
									if (cmd == "addface") {
										currentHost.AddMessage(MessageType.Warning, false, "AddFace is not a supported command - did you mean Face? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									} else if (cmd == "addface2") {
										currentHost.AddMessage(MessageType.Warning, false, "AddFace2 is not a supported command - did you mean Face2? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									}
								} else {
									if (cmd == "face") {
										currentHost.AddMessage(MessageType.Warning, false, "Face is not a supported command - did you mean AddFace? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									} else if (cmd == "face2") {
										currentHost.AddMessage(MessageType.Warning, false, "Face2 is not a supported command - did you mean AddFace2? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									}
								}
								if (Arguments.Length < 3) {
									currentHost.AddMessage(MessageType.Error, false, "At least 3 arguments are required in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else
								{
									bool isFace2 = cmd == "addface2" | cmd == "face2";
									bool q = true;
									int[] a = new int[Arguments.Length];
									for (int j = 0; j < Arguments.Length; j++) {
										if (!NumberFormats.TryParseIntVb6(Arguments[j], out a[j])) {
											if (enabledHacks.BveTsHacks)
											{
												if (IsB3D && j == 0 && Arguments[j] == string.Empty)
												{
													/*
													* Face ,1,2,3
													* is interpreted by BVE as Face 0,1,2,3
													*/
													a[j] = 0;
												}
												else if (a.Length > 2)
												{
													/*
													 * AddFace,0,3,2,1,,,,,,1
													 * Viable face command, but junk / comments added at the end													 *
													 */
													currentHost.AddMessage(MessageType.Error, false, "v" + j.ToString(Culture) + " is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													break;
												}
												continue;
											}
											currentHost.AddMessage(MessageType.Error, false, "v" + j.ToString(Culture) + " is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											q = false;
											break;
										} else if (a[j] < 0 | a[j] >= Builder.Vertices.Count) {
											currentHost.AddMessage(MessageType.Error, false, "v" + j.ToString(Culture) + " references a non-existing vertex in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											q = false;
											break;
										} else if (a[j] > 65535) {
											currentHost.AddMessage(MessageType.Error, false, "v" + j.ToString(Culture) + " indexes a vertex above 65535 which is not currently supported in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											q = false;
											break;
										}
									}

									if (enabledHacks.BveTsHacks)
									{
										if ((FileName.ToLowerInvariant().Contains("hira2\\car") || FileName.ToLowerInvariant().Contains("hira2/car")) && a.SequenceEqual(new[] {0, 1, 4, 5}))
										{
											/*
											* Fix glitchy Hirakami railway stock
											*/
											a = new[] {0, 1, 5, 4};
										}

										int[] vertexIndices = (int[])a.Clone();
										Array.Sort(vertexIndices);
										for (int k = 0; k < Builder.Faces.Count; k++)
										{
											/*
											 * Some BVE2 content declares a Face2 twice with the vertices in a differing order, e.g.
											 *
											 * Face2 0, 1, 3, 2
											 * Face2 1, 0, 2, 3
											 *
											 * Doing this in OpenBVE causes some very funky glitches with Z-fighting,
											 * as it attempts to render both faces in the same space
											 *
											 * With this hack, the lighting may be off but the Z-fighting is gone
											 * (BVE2 / BVE4 operate in a strict draw-order, so the most recent face is always on top,
											 * wheras OpenBVE has no fixed draw order)
											 */
											MeshFace currentFace = Builder.Faces[k];
											if (!isFace2 || (currentFace.Flags & FaceFlags.Face2Mask) == 0)
											{
												continue;
											}

											if (currentFace.Vertices.Length != a.Length)
											{
												continue;
											}

											int[] faceVertexIndices = new int[a.Length];
											for (int v = 0; v < currentFace.Vertices.Length; v++)
											{
												faceVertexIndices[v] = currentFace.Vertices[v].Index;
											}
											Array.Sort(faceVertexIndices);
											if (vertexIndices.SequenceEqual(faceVertexIndices))
											{
												q = false;
											}
											
										}
									}
									if (q) {
										MeshFace f = new MeshFace {Vertices = new MeshFaceVertex[Arguments.Length]};
										for (int j = 0; j < Arguments.Length; j++) {
											f.Vertices[j].Index = (ushort)a[j];
											if (j < Normals.Count)
											{
												f.Vertices[j].Normal = Normals[a[j]];
											}
											
										}
										if (Builder.isCylinder && enabledHacks.BveTsHacks && enabledHacks.CylinderHack)
										{
											int l = f.Vertices.Length;
											MeshFaceVertex v = f.Vertices[l - 1];
											f.Vertices[l - 1] = f.Vertices[l - 2];
											f.Vertices[l - 2] = v;
										}
										if (isFace2) {
											f.Flags = FaceFlags.Face2Mask;
										}
										Builder.Faces.Add(f);
									}
								}
							} break;
						case "cube":
							{
								if (Arguments.Length > 3) {
									currentHost.AddMessage(MessageType.Warning, false, "At most 3 arguments are expected in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								double x = 0.0;
								if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out x)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument HalfWidth in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									x = 1.0;
								}
								double y = x, z = x;
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out y)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument HalfHeight in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									y = 1.0;
								}
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], out z)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument HalfDepth in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									z = 1.0;
								}
								CreateCube(ref Builder, x, y, z);
							} break;
						case "cylinder":
							{
								if (Arguments.Length > 4) {
									currentHost.AddMessage(MessageType.Warning, false, "At most 4 arguments are expected in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								int n = 8;
								if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out n)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument n in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									n = 8;
								}
								if (n < 2) {
									currentHost.AddMessage(MessageType.Error, false, "n is expected to be at least 2 in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									n = 8;
								}
								double r1 = 0.0, r2 = 0.0, h = 1.0;
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out r1)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument UpperRadius in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									r1 = 1.0;
								}
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], out r2)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument LowerRadius in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									r2 = 1.0;
								}
								if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[3], out h)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument Height in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									h = 1.0;
								}
								CreateCylinder(ref Builder, n, r1, r2, h);
								Builder.isCylinder = true;
							} break;
						case "translate":
						case "translateall":
							{
								if (Arguments.Length > 3) {
									currentHost.AddMessage(MessageType.Warning, false, "At most 3 arguments are expected in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								double x = 0.0, y = 0.0, z = 0.0;
								if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out x)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument X in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									x = 0.0;
								}
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out y)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument Y in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									y = 0.0;
								}
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], out z)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument Z in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									z = 0.0;
								}
								Builder.ApplyTranslation(x, y, z);
								if (cmd == "translateall") {
									Object.ApplyTranslation(x, y, z);
								}
							} break;
						case "scale":
						case "scaleall":
							{
								if (Arguments.Length > 3) {
									currentHost.AddMessage(MessageType.Warning, false, "At most 3 arguments are expected in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								double x = 1.0, y = 1.0, z = 1.0;
								if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out x)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument X in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									x = 1.0;
								} else if (x == 0.0) {
									currentHost.AddMessage(MessageType.Error, false, "X is required to be different from zero in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									x = 1.0;
								}
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out y)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument Y in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									y = 1.0;
								} else if (y == 0.0) {
									currentHost.AddMessage(MessageType.Error, false, "Y is required to be different from zero in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									y = 1.0;
								}
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], out z)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument Z in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									z = 1.0;
								} else if (z == 0.0) {
									currentHost.AddMessage(MessageType.Error, false, "Z is required to be different from zero in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									z = 1.0;
								}
								Builder.ApplyScale(x, y, z);
								if (cmd == "scaleall") {
									Object.ApplyScale(x, y, z);
								}
							} break;
						case "rotate":
						case "rotateall":
							{
								if (Arguments.Length > 4) {
									currentHost.AddMessage(MessageType.Warning, false, "At most 4 arguments are expected in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								Vector3 r = new Vector3();
								double a = 0;
								if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out r.X)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument X in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									r.X = 0.0;
								}
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out r.Y)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument Y in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									r.Y = 0.0;
								}
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], out r.Z)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument Z in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									r.Z = 0.0;
								}
								if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[3], out a)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument Angle in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									a = 0.0;
								}

								double t = r.NormSquared();
								if (t == 0.0) {
									r = Vector3.Right;
									t = 1.0;
								}
								if (a != 0.0) {
									t = 1.0 / Math.Sqrt(t);
									r *= t;
									a = a.ToRadians();
									Builder.ApplyRotation(r, a);
									if (cmd == "rotateall") {
										Object.ApplyRotation(r, a);
									}
								}
							} break;
						case "shear":
						case "shearall":
							{
								if (Arguments.Length > 7) {
									currentHost.AddMessage(MessageType.Warning, false, "At most 7 arguments are expected in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								Vector3 d = new Vector3();
								Vector3 s = new Vector3();
								double r = 0.0;
								if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out d.X)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument dX in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out d.Y)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument dY in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], out d.Z)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument dZ in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[3], out s.X)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument sX in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length >= 5 && Arguments[4].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[4], out s.Y)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument sY in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length >= 6 && Arguments[5].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[5], out s.Z)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument sZ in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length >= 7 && Arguments[6].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[6], out r)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument Ratio in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									r = 0.0;
								}
								d.Normalize();
								s.Normalize();
								Builder.ApplyShear(d, s, r);
								if (cmd == "shearall") {
									Object.ApplyShear(d, s, r);
								}
							} break;
						case "mirror":
						case "mirrorall":
							{
								if (Arguments.Length > 6)
								{
									currentHost.AddMessage(MessageType.Warning, false, "At most 6 arguments are expected in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}

								double vx = 0.0, vy = 0.0, vz = 0.0;
								double nx = 0.0, ny = 0.0, nz = 0.0;
								if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out vx))
								{
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument vX in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									vx = 0.0;
								}
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out vy))
								{
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument vY in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									vy = 0.0;
								}
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], out vz))
								{
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument vZ in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									vz = 0.0;
								}
								if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[3], out nx))
								{
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument nX in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									nx = 0.0;
								}
								if (Arguments.Length >= 5 && Arguments[4].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[4], out ny))
								{
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument nY in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									ny = 0.0;
								}
								if (Arguments.Length >= 6 && Arguments[5].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[5], out nz))
								{
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument nZ in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									nz = 0.0;
								}

								if (Arguments.Length < 4)
								{
									nx = vx;
									ny = vy;
									nz = vz;
								}
								Builder.ApplyMirror(vx != 0, vy != 0, vz != 0, nx != 0, ny != 0, nz != 0);
								if (cmd == "mirrorall")
								{
									Object.ApplyMirror(vx != 0, vy != 0, vz != 0, nx != 0, ny != 0, nz != 0);
								}

							}
							break;
						case "generatenormals":
						case "[texture]":
							if (cmd == "generatenormals" & IsB3D) {
								currentHost.AddMessage(MessageType.Warning, false, "GenerateNormals is not a supported command - did you mean [Texture]? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
							} else if (cmd == "[texture]" & !IsB3D) {
								currentHost.AddMessage(MessageType.Warning, false, "[Texture] is not a supported command - did you mean GenerateNormals? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
							}
							break;
						case "setcolor":
						case "color":
						{
								if (cmd == "setcolor" & IsB3D) {
									currentHost.AddMessage(MessageType.Warning, false, "SetColor is not a supported command - did you mean Color? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else if (cmd == "color" & !IsB3D) {
									currentHost.AddMessage(MessageType.Warning, false, "Color is not a supported command - did you mean SetColor? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length > 4) {
									currentHost.AddMessage(MessageType.Warning, false, "At most 4 arguments are expected in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								int r = 0, g = 0, b = 0, a = 255;
								if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out r)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument Red in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									r = 0;
								} else if (r < 0 | r > 255) {
									currentHost.AddMessage(MessageType.Error, false, "Red is required to be within the range from 0 to 255 in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									r = r < 0 ? 0 : 255;
								}
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[1], out g)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument Green in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									g = 0;
								} else if (g < 0 | g > 255) {
									currentHost.AddMessage(MessageType.Error, false, "Green is required to be within the range from 0 to 255 in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									g = g < 0 ? 0 : 255;
								}
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[2], out b)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument Blue in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									b = 0;
								} else if (b < 0 | b > 255) {
									currentHost.AddMessage(MessageType.Error, false, "Blue is required to be within the range from 0 to 255 in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									b = b < 0 ? 0 : 255;
								}
								if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[3], out a)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument Alpha in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									a = 255;
								} else if (a < 0 | a > 255) {
									currentHost.AddMessage(MessageType.Error, false, "Alpha is required to be within the range from 0 to 255 in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									a = a < 0 ? 0 : 255;
								}

								if (a == 0 && enabledHacks.BveTsHacks || enabledHacks.DisableSemiTransparentFaces)
								{
									/*
									 * BVE2 didn't support semi-transparent faces at all
									 * BVE4 treats faces with an opacity value of 0 as having an opacity value of 1
									 */
									a = 255;
								}
								int m = Builder.Materials.Length;
								Array.Resize(ref Builder.Materials, m << 1);
								for (int j = m; j < Builder.Materials.Length; j++) {
									Builder.Materials[j] = new Material(Builder.Materials[j - m])
									{
										Color = new Color32((byte) r, (byte) g, (byte) b, (byte) a),
										BlendMode = Builder.Materials[0].BlendMode,
										GlowAttenuationData = Builder.Materials[0].GlowAttenuationData,
										DaytimeTexture = Builder.Materials[0].DaytimeTexture,
										NighttimeTexture = Builder.Materials[0].NighttimeTexture,
										TransparentColor = Builder.Materials[0].TransparentColor,
										Flags =  Builder.Materials[0].Flags,
										WrapMode = Builder.Materials[0].WrapMode
									};
								}
								for (int j = 0; j < Builder.Faces.Count; j++)
								{
									MeshFace f = Builder.Faces[j];
									f.Material += (ushort) m;
									Builder.Faces[j] = f;
								}
							} break;
						case "colorall":
						case "setcolorall":
						{
							{
								if (cmd == "setcolorall" & IsB3D) {
									currentHost.AddMessage(MessageType.Warning, false, "SetColorAll is not a supported command - did you mean ColorAll? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else if (cmd == "colorall" & !IsB3D) {
									currentHost.AddMessage(MessageType.Warning, false, "ColorAll is not a supported command - did you mean SetColorAll? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length > 4) {
									currentHost.AddMessage(MessageType.Warning, false, "At most 4 arguments are expected in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								int r = 0, g = 0, b = 0, a = 255;
								if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out r)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument Red in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									r = 0;
								} else if (r < 0 | r > 255) {
									currentHost.AddMessage(MessageType.Error, false, "Red is required to be within the range from 0 to 255 in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									r = r < 0 ? 0 : 255;
								}
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[1], out g)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument Green in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									g = 0;
								} else if (g < 0 | g > 255) {
									currentHost.AddMessage(MessageType.Error, false, "Green is required to be within the range from 0 to 255 in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									g = g < 0 ? 0 : 255;
								}
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[2], out b)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument Blue in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									b = 0;
								} else if (b < 0 | b > 255) {
									currentHost.AddMessage(MessageType.Error, false, "Blue is required to be within the range from 0 to 255 in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									b = b < 0 ? 0 : 255;
								}
								if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[3], out a)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument Alpha in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									a = 255;
								} else if (a < 0 | a > 255) {
									currentHost.AddMessage(MessageType.Error, false, "Alpha is required to be within the range from 0 to 255 in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									a = a < 0 ? 0 : 255;
								}
								Color32 newColor = new Color32((byte)r, (byte)g, (byte)b, (byte)a);
								Object.ApplyColor(newColor, false);
							}
						} break;
						case "emissivecolorall":
						case "setemissivecolorall":
						{
							{
								if (cmd == "setemissivecolorall" & IsB3D) {
									currentHost.AddMessage(MessageType.Warning, false, "SetEmissiveColorAll is not a supported command - did you mean EmissiveColorAll? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else if (cmd == "emissivecolorall" & !IsB3D) {
									currentHost.AddMessage(MessageType.Warning, false, "EmissiveColorAll is not a supported command - did you mean SetEmissiveColorAll? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length > 4) {
									currentHost.AddMessage(MessageType.Warning, false, "At most 4 arguments are expected in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								int r = 0, g = 0, b = 0;
								if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out r)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument Red in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									r = 0;
								} else if (r < 0 | r > 255) {
									currentHost.AddMessage(MessageType.Error, false, "Red is required to be within the range from 0 to 255 in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									r = r < 0 ? 0 : 255;
								}
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[1], out g)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument Green in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									g = 0;
								} else if (g < 0 | g > 255) {
									currentHost.AddMessage(MessageType.Error, false, "Green is required to be within the range from 0 to 255 in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									g = g < 0 ? 0 : 255;
								}
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[2], out b)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument Blue in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									b = 0;
								} else if (b < 0 | b > 255) {
									currentHost.AddMessage(MessageType.Error, false, "Blue is required to be within the range from 0 to 255 in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									b = b < 0 ? 0 : 255;
								}
								Color32 newColor = new Color32((byte)r, (byte)g, (byte)b, (byte)255);
								Object.ApplyColor(newColor, true);
							}
						} break;
						case "setemissivecolor":
						case "emissivecolor":
							{
								if (cmd == "setemissivecolor" & IsB3D) {
									currentHost.AddMessage(MessageType.Warning, false, "SetEmissiveColor is not a supported command - did you mean EmissiveColor? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else if (cmd == "emissivecolor" & !IsB3D) {
									currentHost.AddMessage(MessageType.Warning, false, "EmissiveColor is not a supported command - did you mean SetEmissiveColor? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length > 3) {
									currentHost.AddMessage(MessageType.Warning, false, "At most 3 arguments are expected in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								int r = 0, g = 0, b = 0;
								if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out r)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument Red in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									r = 0;
								} else if (r < 0 | r > 255) {
									currentHost.AddMessage(MessageType.Error, false, "Red is required to be within the range from 0 to 255 in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									r = r < 0 ? 0 : 255;
								}
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[1], out g)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument Green in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									g = 0;
								} else if (g < 0 | g > 255) {
									currentHost.AddMessage(MessageType.Error, false, "Green is required to be within the range from 0 to 255 in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									g = g < 0 ? 0 : 255;
								}
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[2], out b)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument Blue in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									b = 0;
								} else if (b < 0 | b > 255) {
									currentHost.AddMessage(MessageType.Error, false, "Blue is required to be within the range from 0 to 255 in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									b = b < 0 ? 0 : 255;
								}
								int m = Builder.Materials.Length;
								Array.Resize(ref Builder.Materials, m << 1);
								for (int j = m; j < Builder.Materials.Length; j++) {
									Builder.Materials[j] = new Material(Builder.Materials[j - m]);
									Builder.Materials[j].EmissiveColor = new Color24((byte)r, (byte)g, (byte)b);
									Builder.Materials[j].Flags = Builder.Materials[0].Flags | MaterialFlags.Emissive;
									Builder.Materials[j].BlendMode = Builder.Materials[0].BlendMode;
									Builder.Materials[j].GlowAttenuationData = Builder.Materials[0].GlowAttenuationData;
									Builder.Materials[j].DaytimeTexture = Builder.Materials[0].DaytimeTexture;
									Builder.Materials[j].NighttimeTexture = Builder.Materials[0].NighttimeTexture;
									Builder.Materials[j].TransparentColor = Builder.Materials[0].TransparentColor;
									Builder.Materials[j].WrapMode = Builder.Materials[0].WrapMode;
								}
								for (int j = 0; j < Builder.Faces.Count; j++)
								{
									MeshFace f = Builder.Faces[j];
									f.Material += (ushort)m;
									Builder.Faces[j] = f;
								}
							} break;
						case "setdecaltransparentcolor":
						case "transparent":
							{
								
								if (cmd == "setdecaltransparentcolor" & IsB3D) {
									currentHost.AddMessage(MessageType.Warning, false, "SetDecalTransparentColor is not a supported command - did you mean Transparent? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else if (cmd == "transparent" & !IsB3D) {
									currentHost.AddMessage(MessageType.Warning, false, "Transparent is not a supported command - did you mean SetDecalTransparentColor? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length > 3) {
									currentHost.AddMessage(MessageType.Warning, false, "At most 3 arguments are expected in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else if (Arguments.Length == 0) {
									currentHost.AddMessage(MessageType.Warning, false, "No color was specified in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName + "- This may produce unexpected results.");
									if (enabledHacks.BveTsHacks && lastTransparentColor != null)
									{
										// The BVE2 / BVE4 parser uses the *last* transparent color if no transparent color is specified
										for (int j = 0; j < Builder.Materials.Length; j++) { 
											Builder.Materials[j].TransparentColor = lastTransparentColor.Value;
											Builder.Materials[j].Flags |= MaterialFlags.TransparentColor;
										}
										break;
									}
								}
								int r = 0, g = 0, b = 0;
								if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out r)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument Red in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									r = 0;
								} else if (r < 0 | r > 255) {
									currentHost.AddMessage(MessageType.Error, false, "Red is required to be within the range from 0 to 255 in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									r = r < 0 ? 0 : 255;
								}
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[1], out g)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument Green in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									g = 0;
								} else if (g < 0 | g > 255) {
									currentHost.AddMessage(MessageType.Error, false, "Green is required to be within the range from 0 to 255 in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									g = g < 0 ? 0 : 255;
								}
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[2], out b)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument Blue in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									b = 0;
								} else if (b < 0 | b > 255) {
									currentHost.AddMessage(MessageType.Error, false, "Blue is required to be within the range from 0 to 255 in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									b = b < 0 ? 0 : 255;
								}
								lastTransparentColor = new Color24((byte) r, (byte) g, (byte) b);
								for (int j = 0; j < Builder.Materials.Length; j++) {
									Builder.Materials[j].TransparentColor = lastTransparentColor.Value;
									Builder.Materials[j].Flags |= MaterialFlags.TransparentColor;
								}
							} break;
						case "setblendingmode":
						case "setblendmode":
						case "blendmode":
							{
								if ((cmd == "setblendmode" || cmd == "setblendingmode") & IsB3D) {
									currentHost.AddMessage(MessageType.Warning, false, "SetBlendMode is not a supported command - did you mean BlendMode? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else if (cmd == "blendmode" & !IsB3D) {
									currentHost.AddMessage(MessageType.Warning, false, "BlendMode is not a supported command - did you mean SetBlendMode? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length > 3) {
									currentHost.AddMessage(MessageType.Warning, false, "At most 3 arguments are expected in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								MeshMaterialBlendMode blendmode = MeshMaterialBlendMode.Normal;
								if (Arguments.Length >= 1 && Arguments[0].Length > 0) {
									switch (Arguments[0].ToLowerInvariant()) {
										case "normal":
											blendmode = MeshMaterialBlendMode.Normal;
											break;
										case "additive":
										case "glow":
											blendmode = MeshMaterialBlendMode.Additive;
											break;
										default:
											currentHost.AddMessage(MessageType.Error, false, "The given BlendMode is not supported in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											blendmode = MeshMaterialBlendMode.Normal;
											break;
									}
								}
								double glowhalfdistance = 0.0;
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out glowhalfdistance)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument GlowHalfDistance in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									glowhalfdistance = 0;
								}
								GlowAttenuationMode glowmode = GlowAttenuationMode.DivisionExponent4;
								if (Arguments.Length >= 3 && Arguments[2].Length > 0) {
									switch (Arguments[2].ToLowerInvariant()) {
											case "divideexponent2": glowmode = GlowAttenuationMode.DivisionExponent2; break;
											case "divideexponent4": glowmode = GlowAttenuationMode.DivisionExponent4; break;
										default:
											currentHost.AddMessage(MessageType.Error, false, "The given GlowAttenuationMode is not supported in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											break;
									}
								}
								for (int j = 0; j < Builder.Materials.Length; j++) {
									Builder.Materials[j].BlendMode = blendmode;
									Builder.Materials[j].GlowAttenuationData = Glow.GetAttenuationData(glowhalfdistance, glowmode);
								}
							} break;
						case "setwrapmode":
						case "wrapmode":
							{
								if (cmd == "setwrapmode" & IsB3D)
								{
									currentHost.AddMessage(MessageType.Warning, false, "SetWrapMode is not a supported command - did you mean WrapMode? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								else if (cmd == "wrapmode" & !IsB3D)
								{
									currentHost.AddMessage(MessageType.Warning, false, "WrapMode is not a supported command - did you mean SetWrapMode? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length > 3)
								{
									currentHost.AddMessage(MessageType.Warning, false, "At most 3 arguments are expected in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								OpenGlTextureWrapMode? wrapmode = null;
								if (Arguments.Length >= 1 && Arguments[0].Length > 0)
								{
									switch (Arguments[0].ToLowerInvariant())
									{
										case "clampclamp":
											wrapmode = OpenGlTextureWrapMode.ClampClamp;
											break;
										case "clamprepeat":
											wrapmode = OpenGlTextureWrapMode.ClampRepeat;
											break;
										case "repeatclamp":
											wrapmode = OpenGlTextureWrapMode.RepeatClamp;
											break;
										case "repeatrepeat":
											wrapmode = OpenGlTextureWrapMode.RepeatRepeat;
											break;
										default:
											currentHost.AddMessage(MessageType.Error, false, "The given WrapMode is not supported in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											wrapmode = null;
											break;
									}
								}
								for (int j = 0; j < Builder.Materials.Length; j++)
								{
									Builder.Materials[j].WrapMode = wrapmode;
								}
							} break;
						case "loadtexture":
						case "load":
							{
							/*
							 * Loading a texture should definitely reset the MeshBuilder, otherwise that's insane
							 * Hopefully won't increase error numbers on existing content too much....
							 */
							firstMeshBuilder = false;
								if (cmd == "loadtexture" & IsB3D) {
									currentHost.AddMessage(MessageType.Warning, false, "LoadTexture is not a supported command - did you mean Load? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else if (cmd == "load" & !IsB3D) {
									currentHost.AddMessage(MessageType.Warning, false, "Load is not a supported command - did you mean LoadTexture? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length > 2) {
									currentHost.AddMessage(MessageType.Warning, false, "At most 2 arguments are expected in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								string tday = null, tnight = null;
								if (Arguments.Length >= 1 && Arguments[0].Length != 0) {
									if (Path.ContainsInvalidChars(Arguments[0])) {
										currentHost.AddMessage(MessageType.Error, false, "DaytimeTexture contains illegal characters in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									} else {
										try
										{
											tday = Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), Arguments[0]);
										}
										catch
										{
											tday = null;
										}
										
										if (!System.IO.File.Exists(tday))
										{
											bool hackFound = false;
											if (enabledHacks.BveTsHacks)
											{
												//Original BVE2 signal graphics
												Match m = Regex.Match(tday, @"(signal\d{1,2}\.bmp)", RegexOptions.IgnoreCase);
												if (m.Success)
												{
													string s = "Signals\\Static\\" + m.Groups[0].Value.Replace(".bmp", ".png");
													tday = Path.CombineFile(CompatibilityFolder, s);
													hackFound = true;
												}

												if (Arguments[0].StartsWith("swiss1/", StringComparison.InvariantCultureIgnoreCase))
												{
													Arguments[0] = Arguments[0].Substring(7);
													tday = Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), Arguments[0]);
													if (System.IO.File.Exists(tday))
													{
														hackFound = true;
													}
												}
											}
											if (!hackFound)
											{
												currentHost.AddMessage(MessageType.Error, true, "DaytimeTexture " + tday + " could not be found in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
												tday = null;
											}
											
										}
									}
								}
								if (Arguments.Length >= 2 && Arguments[1].Length != 0) {
									if (Arguments[0].Length == 0) {
										currentHost.AddMessage(MessageType.Error, true, "DaytimeTexture is required to be specified in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									} else {
										if (Path.ContainsInvalidChars(Arguments[1])) {
											currentHost.AddMessage(MessageType.Error, false, "NighttimeTexture contains illegal characters in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
										} else
										{
											bool ignoreAsInvalid = false;
											try
											{
												tnight = Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), Arguments[1]);
											}
											catch
											{
												tnight = null;
												switch (Arguments[1])
												{
													case ".":
														// Meguro route - Misplaced period in several platform objects
														ignoreAsInvalid = true;
														break;
												}
											}
											
											if (!System.IO.File.Exists(tnight) && !ignoreAsInvalid) {
												currentHost.AddMessage(MessageType.Error, true, "The NighttimeTexture " + tnight + " could not be found in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
												tnight = null;
											}
											
										}
									}
								}
								for (int j = 0; j < Builder.Materials.Length; j++) {
									Builder.Materials[j].DaytimeTexture = tday;
									Builder.Materials[j].NighttimeTexture = tnight;
								}
							} break;
						case "settext":
						case "text":
							{
								if (cmd == "settext" & IsB3D)
								{
									currentHost.AddMessage(MessageType.Warning, false,
									  "SetText is not a supported command - did you mean Text? - at line " + (i + 1).ToString(Culture) + " in file " +
									  FileName);
								}
								else if (cmd == "text" & !IsB3D)
								{
									currentHost.AddMessage(MessageType.Warning, false,
									  "Text is not a supported command - did you mean SetText? - at line " + (i + 1).ToString(Culture) + " in file " +
									  FileName);
								}

								if (Arguments.Length > 2)
								{
									currentHost.AddMessage(MessageType.Warning, false,
									  "At most 2 arguments are expected in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " +
									  FileName);
								}
								string text = Arguments[0];

								for (int j = 0; j < Builder.Materials.Length; j++)
								{
									Builder.Materials[j].Text = text;
								}
							}
							break;
						case "settextcolor":
						case "textcolor":
							{
								if (cmd == "settextcolor" & IsB3D)
								{
									currentHost.AddMessage(MessageType.Warning, false,
									  "SetTextColor is not a supported command - did you mean TextColor? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								else if (cmd == "textcolor" & !IsB3D)
								{
									currentHost.AddMessage(MessageType.Warning, false,
									  "TextColor is not a supported command - did you mean SetTextColor? - at line " + (i + 1).ToString(Culture) + " in file " +
									  FileName);
								}

								if (Arguments.Length != 3)
								{
									currentHost.AddMessage(MessageType.Warning, false,
									  "3 arguments are expected in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								int r = 0, g = 0, b = 0;
								if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !int.TryParse(Arguments[0], out r))
								{
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument R in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !int.TryParse(Arguments[1], out g))
								{
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument G in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !int.TryParse(Arguments[2], out b))
								{
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument B in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								Color textColor = Color.FromArgb(r, g, b);
								for (int j = 0; j < Builder.Materials.Length; j++)
								{
									Builder.Materials[j].TextColor = textColor;
								}
							}
							break;
						case "setbackgroundcolor":
						case "backgroundcolor":
							{
								if (cmd == "setbackgroundcolor" & IsB3D)
								{
									currentHost.AddMessage(MessageType.Warning, false,
									  "SetBackgroundColor is not a supported command - did you mean BackgroundColor? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								else if (cmd == "backgroundcolor" & !IsB3D)
								{
									currentHost.AddMessage(MessageType.Warning, false,
									  "BackgroundColor is not a supported command - did you mean SetBackgroundColor? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}

								if (Arguments.Length != 3)
								{
									currentHost.AddMessage(MessageType.Warning, false,
									  "3 arguments are expected in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								int r = 0, g = 0, b = 0;
								if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !int.TryParse(Arguments[0], out r))
								{
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument R in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !int.TryParse(Arguments[1], out g))
								{
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument G in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !int.TryParse(Arguments[2], out b))
								{
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument B in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								Color backgroundColor = Color.FromArgb(r, g, b);
								for (int j = 0; j < Builder.Materials.Length; j++)
								{
									Builder.Materials[j].BackgroundColor = backgroundColor;
								}
							}
							break;
						case "settextpadding":
						case "textpadding":
							{
								if (cmd == "settextpadding" & IsB3D)
								{
									currentHost.AddMessage(MessageType.Warning, false,
									  "SetTextPadding is not a supported command - did you mean TextPadding? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								else if (cmd == "backgroundcolor" & !IsB3D)
								{
									currentHost.AddMessage(MessageType.Warning, false,
									  "TextPadding is not a supported command - did you mean SetTextPadding? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}

								if (Arguments.Length > 2)
								{
									currentHost.AddMessage(MessageType.Warning, false,
									  "At most 2 arguments are expected in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								Vector2 Padding = new Vector2(0, 0);
								double.TryParse(Arguments[0], out Padding.X);
								double.TryParse(Arguments[1], out Padding.Y);
								for (int j = 0; j < Builder.Materials.Length; j++)
								{
									Builder.Materials[j].TextPadding = Padding;
								}
							}
							break;
						case "setfont":
						case "font":
							{
								if (cmd == "setfont" & IsB3D)
								{
									currentHost.AddMessage(MessageType.Warning, false,
									  "SetFont is not a supported command - did you mean Font? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								else if (cmd == "backgroundcolor" & !IsB3D)
								{
									currentHost.AddMessage(MessageType.Warning, false,
									  "Font is not a supported command - did you mean SetFont? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}

								if (Arguments.Length > 1)
								{
									currentHost.AddMessage(MessageType.Warning, false,
									  "1 argument is expected in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (!TextOverlay.FontAvailable(Arguments[0]))
								{
									currentHost.AddMessage(MessageType.Warning, false,
									  "Font " + Arguments[0] + "is not available at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								for (int j = 0; j < Builder.Materials.Length; j++)
								{
									Builder.Materials[j].Font = Arguments[0];
								}

							}
							break; 
						case "settexturecoordinates":
						case "coordinates":
							{
								if (cmd == "settexturecoordinates" & IsB3D) {
									currentHost.AddMessage(MessageType.Warning, false, "SetTextureCoordinates is not a supported command - did you mean Coordinates? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else if (cmd == "coordinates" & !IsB3D) {
									currentHost.AddMessage(MessageType.Warning, false, "Coordinates is not a supported command - did you mean SetTextureCoordinates? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length > 3) {
									currentHost.AddMessage(MessageType.Warning, false, "At most 3 arguments are expected in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								int j = 0; float x = 0.0f, y = 0.0f;
								if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out j)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument VertexIndex in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									j = 0;
								}
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseFloatVb6(Arguments[1], out x)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument X in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									x = 0.0f;
								}
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseFloatVb6(Arguments[2], out y)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument Y in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									y = 0.0f;
								}
								if (j >= 0 & j < Builder.Vertices.Count) {
									Builder.Vertices[j].TextureCoordinates = new Vector2(x, y);
								} else {
									currentHost.AddMessage(MessageType.Error, false, "VertexIndex references a non-existing vertex in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
							} break;
						case "enablecrossfading":
						case "crossfading":
							{
								if (Arguments.Length > 1)
								{
									currentHost.AddMessage(MessageType.Warning, false, $"At most 1 arguments are expected in {Command} at line {(i + 1).ToString(Culture)} in file {FileName}");
								}

								bool value = false;

								if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !bool.TryParse(Arguments[0], out value))
								{
									currentHost.AddMessage(MessageType.Error, false, $"Invalid argument Value in {Command} at line {(i + 1).ToString(Culture)} in file {FileName}");
									value = false;
								}

								foreach (Material material in Builder.Materials)
								{
									if (value)
									{
										material.Flags |= MaterialFlags.CrossFadeTexture;
									}
									else
									{
										material.Flags &= ~MaterialFlags.CrossFadeTexture;
									}
								}
							}
							break;
						default:
							if (Command.Length != 0) {
								if (IsUtf(Encoding))
								{
									//CreateMeshBuilder misinterpreted as UTF
									//As this character sequence is gibberish, we can assume our file is NOT actually UTF
									//so re-read with the default ANSI charset
									if (Command.IndexOf("牃慥整敍桳畂汩敤", StringComparison.Ordinal) != -1 || Command.IndexOf("䵛獥䉨極摬牥", StringComparison.Ordinal) != -1 || Command.IndexOf("浛獥扨極摬牥", StringComparison.Ordinal) != -1)
									{
										Object = ReadObject(FileName, Encoding.Default);
										return Object;
									}
									else
									{
										//Don't log the error message if we figure out it's misdetected
										currentHost.AddMessage(MessageType.Error, false, "The command " + Command + " is not supported at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									}
								}
							}
							break;
					}
				}
			}
			// finalize object
			Builder.Apply(ref Object, enabledHacks.BveTsHacks);
			Object.Mesh.CreateNormals();
			for (int i = 0; i < Object.Mesh.Faces.Length; i++)
			{
				int k = Object.Mesh.Faces[i].Material;
				OpenGlTextureWrapMode wrap = OpenGlTextureWrapMode.ClampClamp;
				if (Object.Mesh.Materials[k].DaytimeTexture != null | Object.Mesh.Materials[k].NighttimeTexture != null)
				{
					if (Object.Mesh.Materials[k].WrapMode == null)
					{
						for (int v = 0; v < Object.Mesh.Vertices.Length; v++)
						{
							if (Object.Mesh.Vertices[v].TextureCoordinates.X < 0.0f | Object.Mesh.Vertices[v].TextureCoordinates.X > 1.0f)
							{
								wrap |= OpenGlTextureWrapMode.RepeatClamp;
							}
							if (Object.Mesh.Vertices[v].TextureCoordinates.Y < 0.0f | Object.Mesh.Vertices[v].TextureCoordinates.Y > 1.0f)
							{
								wrap |= OpenGlTextureWrapMode.ClampRepeat;
							}
						}
						Object.Mesh.Materials[k].WrapMode = wrap;
					}
				}
			}

			return Object;
		}

		private static bool IsPotentialPath(string Line)
		{
			string[] Images = {".bmp", ".gif" ,".jpg", ".jpeg", ".png"};
			for (int i = 0; i < Images.Length; i++)
			{
				if (Line.IndexOf(Images[i], StringComparison.OrdinalIgnoreCase) >= 0)
				{
					return true;
				}
			}
			return false;
		}

		// create cube
		private static void CreateCube(ref MeshBuilder Builder, double sx, double sy, double sz)
		{
			int v = Builder.Vertices.Count;
			Builder.Vertices.Add(new Vertex(sx, sy, -sz));
			Builder.Vertices.Add(new Vertex(sx, -sy, -sz));
			Builder.Vertices.Add(new Vertex(-sx, -sy, -sz));
			Builder.Vertices.Add(new Vertex(-sx, sy, -sz));
			Builder.Vertices.Add(new Vertex(sx, sy, sz));
			Builder.Vertices.Add(new Vertex(sx, -sy, sz));
			Builder.Vertices.Add(new Vertex(-sx, -sy, sz));
			Builder.Vertices.Add(new Vertex(-sx, sy, sz));
			MeshFace f0 = new MeshFace(new[] {v + 0, v + 1, v + 2, v + 0, v + 2, v + 3}, FaceFlags.Triangles);
			Builder.Faces.Add(f0);
			MeshFace f1 = new MeshFace(new[] {v + 0, v + 4, v + 5, v + 0, v + 5, v + 1}, FaceFlags.Triangles);
			Builder.Faces.Add(f1);
			MeshFace f2 = new MeshFace(new[] {v + 0, v + 3, v + 7, v + 0, v + 7, v + 4}, FaceFlags.Triangles);
			Builder.Faces.Add(f2);
			MeshFace f3 = new MeshFace(new[] {v + 6, v + 5, v + 4, v + 6, v + 4, v + 7}, FaceFlags.Triangles);
			Builder.Faces.Add(f3);
			MeshFace f4 = new MeshFace(new[] {v + 6, v + 7, v + 3, v + 6, v + 3, v + 2}, FaceFlags.Triangles);
			Builder.Faces.Add(f4);
			MeshFace f5 = new MeshFace(new[] {v + 6, v + 2, v + 1, v + 6, v + 1, v + 5}, FaceFlags.Triangles);
			Builder.Faces.Add(f5);
			
		}

		// create cylinder
		private static void CreateCylinder(ref MeshBuilder Builder, int n, double r1, double r2, double h)
		{
			// parameters
			bool uppercap = r1 > 0.0;
			bool lowercap = r2 > 0.0;
			int m = (uppercap ? 1 : 0) + (lowercap ? 1 : 0);
			r1 = Math.Abs(r1);
			r2 = Math.Abs(r2);
			double ns = h >= 0.0 ? 1.0 : -1.0;
			// initialization
			Vector3[] Normals = new Vector3[2 * n];
			double d = 2.0 * Math.PI / n;
			double g = 0.5 * h;
			double t = 0.0;
			double a = h != 0.0 ? Math.Atan((r2 - r1) / h) : 0.0;
			// vertices and normals
			int v = Builder.Vertices.Count;
			for (int i = 0; i < n; i++) {
				double dx = Math.Cos(t);
				double dz = Math.Sin(t);
				double lx = dx * r2;
				double lz = dz * r2;
				double ux = dx * r1;
				double uz = dz * r1;
				Builder.Vertices.Add(new Vertex(ux, g, uz));
				Builder.Vertices.Add(new Vertex(lx, -g, lz));
				Vector3 normal = new Vector3(dx * ns, 0.0, dz * ns);
				Vector3 s = Vector3.Cross(normal, Vector3.Down);
				normal.Rotate(s, a);
				Normals[2 * i + 0] = new Vector3(normal);
				Normals[2 * i + 1] = new Vector3(normal);
				t += d;
			}
			// faces

			for (int i = 0; i < n; i++) {
				int i0 = (2 * i + 2) % (2 * n);
				int i1 = (2 * i + 3) % (2 * n);
				int i2 = 2 * i + 1;
				int i3 = 2 * i;
				MeshFace f = new MeshFace(new[] {new MeshFaceVertex(v + i0, Normals[i0]), new MeshFaceVertex(v + i1, Normals[i1]), new MeshFaceVertex(v + i2, Normals[i2]), new MeshFaceVertex(v + i0, Normals[i0]), new MeshFaceVertex(v + i2, Normals[i2]), new MeshFaceVertex(v + i3, Normals[i3])}, 0);
				f.Flags = FaceFlags.Triangles;
				Builder.Faces.Add(f);
			}

			for (int i = 0; i < m; i++) {
				List<MeshFaceVertex> verts = new List<MeshFaceVertex>();
				for (int j = 0; j < n; j++)
				{
					if (verts.Count > 2)
					{
						verts.Add(verts[0]);
						verts.Add(verts[verts.Count - 2]);
					}
					if (i == 0 & lowercap) {
						// lower cap
						verts.Add(new MeshFaceVertex(v + 2 * j + 1));
					} else {
						// upper cap
						verts.Add(new MeshFaceVertex(v + 2 * (n - j - 1)));
					}
				}
				verts.Add(verts[0]);
				verts.Add(verts[verts.Count - 1]);
				verts.Add(verts[1]);
				MeshFace f = new MeshFace(verts.ToArray(), 0);
				f.Flags = FaceFlags.Triangles;
				Builder.Faces.Add(f);
			}

		}
		
		/// <summary>Checks whether the specified System.Text.Encoding is Unicode</summary>
		/// <param name="Encoding">The Encoding</param>
		private static bool IsUtf(Encoding Encoding)
		{
			switch (Encoding.WindowsCodePage)
			{
				//UTF codepage numbers
				case 1200:
				case 1201:
				case 65000:
				case 65001:
					return true;
			}
			return false;
		}
	}
}
