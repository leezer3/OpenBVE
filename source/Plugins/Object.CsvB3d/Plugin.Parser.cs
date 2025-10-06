using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Textures;

namespace Object.CsvB3d
{
	public partial class Plugin
	{
		private static bool IsCommand(string Text, bool IsB3d)
		{
			if (!Enum.TryParse(Text, true, out B3DCsvCommands command))
			{
				// not a valid command
				return false;
			}

			if (IsB3d)
			{
				// Valid command for b3d
				if ((int)command < 100 || (int)command >= 200)
				{
					return true;
				}
			}
			else
			{
				// Valid command for csv
				if ((int)command >= 100)
				{
					return true;
				}
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
			"enablecrossfading",
			"loadlightmap"
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
								// ReSharper disable once RedundantEmptySwitchSection
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
						Command = Arguments[0].Substring(0, j).TrimEnd();
						Arguments[0] = Arguments[0].Substring(j + 1).TrimStart();
					} else {
						Command = Arguments[0];
						bool resetArguments = true;
						if (Arguments.Length != 1) {
							if (!enabledHacks.BveTsHacks || !IsCommand(Command, true))
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
					Enum.TryParse(Command.TrimStart('[').TrimEnd(']'), true, out B3DCsvCommands cmd);
					switch(cmd) {
						case B3DCsvCommands.CreateMeshBuilder:
						case B3DCsvCommands.MeshBuilder:
							{
								firstMeshBuilder = true;
								if (cmd == B3DCsvCommands.CreateMeshBuilder & IsB3D) {
									currentHost.AddMessage(MessageType.Warning, false, "CreateMeshBuilder is not a supported command - did you mean [MeshBuilder]? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else if (cmd == B3DCsvCommands.MeshBuilder & !IsB3D) {
									currentHost.AddMessage(MessageType.Warning, false, "[MeshBuilder] is not a supported command - did you mean CreateMeshBuilder? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length > 0) {
									currentHost.AddMessage(MessageType.Warning, false, "0 arguments are expected in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								Builder.Apply(ref Object, enabledHacks.BveTsHacks);
								Builder = new MeshBuilder(currentHost);
								Normals = new List<Vector3>();
							} break;
						case B3DCsvCommands.AddVertex:
						case B3DCsvCommands.Vertex:
							{
								if (!firstMeshBuilder)
								{
									//https://github.com/leezer3/OpenBVE/issues/448
									currentHost.AddMessage(MessageType.Warning, false, "Attempted to add a vertex without first creating a MeshBuilder - This may produce unexpected results - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (cmd == B3DCsvCommands.AddVertex & IsB3D) {
									currentHost.AddMessage(MessageType.Warning, false, "AddVertex is not a supported command - did you mean Vertex? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else if (cmd == B3DCsvCommands.Vertex & !IsB3D) {
									currentHost.AddMessage(MessageType.Warning, false, "Vertex is not a supported command - did you mean AddVertex? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length > 6) {
									currentHost.AddMessage(MessageType.Warning, false, "At most 6 arguments are expected in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}

								Vertex currentVertex = new Vertex();
								if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out currentVertex.Coordinates.X)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument vX in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									currentVertex.Coordinates.X = 0.0;
								}
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out currentVertex.Coordinates.Y)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument vY in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									currentVertex.Coordinates.Y = 0.0;
								}
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], out currentVertex.Coordinates.Z)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument vZ in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									currentVertex.Coordinates.Z = 0.0;
								}
								Vector3 currentNormal = new Vector3();
								if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[3], out currentNormal.X)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument nX in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									currentNormal.X = 0.0;
								}
								if (Arguments.Length >= 5 && Arguments[4].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[4], out currentNormal.Y)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument nY in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									currentNormal.Y = 0.0;
								}
								if (Arguments.Length >= 6 && Arguments[5].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[5], out currentNormal.Z)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument nZ in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									currentNormal.Z = 0.0;
								}
								currentNormal.Normalize();
								Builder.Vertices.Add(currentVertex);
								Normals.Add(currentNormal);
							} break;
						case B3DCsvCommands.AddFace:
						case B3DCsvCommands.AddFace2:
						case B3DCsvCommands.Face:
						case B3DCsvCommands.Face2:
							{
								if (IsB3D) {
									if (cmd == B3DCsvCommands.AddFace) {
										currentHost.AddMessage(MessageType.Warning, false, "AddFace is not a supported command - did you mean Face? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									} else if (cmd == B3DCsvCommands.AddFace2) {
										currentHost.AddMessage(MessageType.Warning, false, "AddFace2 is not a supported command - did you mean Face2? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									}
								} else {
									if (cmd == B3DCsvCommands.Face) {
										currentHost.AddMessage(MessageType.Warning, false, "Face is not a supported command - did you mean AddFace? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									} else if (cmd == B3DCsvCommands.Face2) {
										currentHost.AddMessage(MessageType.Warning, false, "Face2 is not a supported command - did you mean AddFace2? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									}
								}
								if (Arguments.Length < 3) {
									currentHost.AddMessage(MessageType.Error, false, "At least 3 arguments are required in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else
								{
									bool isFace2 = cmd == B3DCsvCommands.AddFace2 | cmd == B3DCsvCommands.Face2;
									bool q = true;
									int[] a = new int[Arguments.Length];
									for (int j = 0; j < Arguments.Length; j++) {
										if (!NumberFormats.TryParseIntVb6(Arguments[j], out a[j])) {
											if (enabledHacks.BveTsHacks)
											{
												if (j == 0 && string.IsNullOrEmpty(Arguments[j]))
												{
													/*
													* Face ,1,2,3
													* is interpreted by BVE as Face 0,1,2,3
													* Applies to both CSV and B3D files
													*/
													a[j] = 0;
												}
												else if (a.Length > 2)
												{
													/*
													 * AddFace,0,3,2,1,,,,,,1
													 * Viable face command, but junk / comments added at the end													 *
													 */
													currentHost.AddMessage(MessageType.Error, false, "v" + j.ToString(Culture) + " is invalid in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
													break;
												}
												continue;
											}
											currentHost.AddMessage(MessageType.Error, false, "v" + j.ToString(Culture) + " is invalid in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											q = false;
											break;
										} else if (a[j] < 0 | a[j] >= Builder.Vertices.Count) {
											currentHost.AddMessage(MessageType.Error, false, "v" + j.ToString(Culture) + " references a non-existing vertex in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											q = false;
											break;
										} else if (a[j] > 65535) {
											currentHost.AddMessage(MessageType.Error, false, "v" + j.ToString(Culture) + " indexes a vertex above 65535 which is not currently supported in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											q = false;
											break;
										}
									}

									if (enabledHacks.BveTsHacks)
									{
										if (FileName.FileNameContains(new [] {"hira2\\car" }) && a.SequenceEqual(new[] {0, 1, 4, 5}))
										{
											/*
											* Fix glitchy Hirakami railway stock
											*/
											a = new[] {0, 1, 5, 4};
										}

										if (FileName.FileNameEndsWith(new[] { "Ryouso\\BlackRoof.csv"}) && a.SequenceEqual(new[] { 0, 1, 2, 3}))
										{
											// broken roof
											a = new[] { 0, 2, 1, 3 };
										}

										if (FileName.FileNameEndsWith(new[] { "Ryouso\\wall\\wall_ugL.csv", "Ryouso\\wall\\wall_ugR.csv", "Ryouso\\wall\\wall_ug-staL.csv", "Ryouso\\wall\\wall_ug-staR.csv"}) && a.SequenceEqual(new[] { 0, 1, 2, 3 }) && Object.Mesh.Vertices.Length >= 8)
										{
											// broken roof (first face is OK though....)
											a = new[] { 0, 2, 3, 1 };
										}

										if (FileName.FileNameEndsWith(new[] { "Ryouso\\wall\\wall_ug-staL_.csv", "Ryouso\\wall\\wall_ug-staR_.csv" }) && a.SequenceEqual(new[] { 0, 1, 2, 3 }) && (Object.Mesh.Vertices.Length == 4 || Object.Mesh.Vertices.Length == 12))
										{
											// broken roof (first face is OK though....)
											a = new[] { 0, 2, 3, 1 };
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
										MeshFace f = new MeshFace(Arguments.Length);
										for (int j = 0; j < Arguments.Length; j++) {
											f.Vertices[j].Index = a[j];
											if (j < Normals.Count)
											{
												f.Vertices[j].Normal = Normals[a[j]];
											}
											
										}
										if (Builder.isCylinder && enabledHacks.BveTsHacks && enabledHacks.CylinderHack)
										{
											int l = f.Vertices.Length;
											(f.Vertices[l - 2], f.Vertices[l - 1]) = (f.Vertices[l - 1], f.Vertices[l - 2]);
										}
										if (isFace2) {
											f.Flags = FaceFlags.Face2Mask;
										}
										Builder.Faces.Add(f);
									}
								}
							} break;
						case B3DCsvCommands.Cube:
							{
								if (Arguments.Length > 3) {
									currentHost.AddMessage(MessageType.Warning, false, "At most 3 arguments are expected in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								double x = 0.0;
								if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out x)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument HalfWidth in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									x = 1.0;
								}
								double y = x, z = x;
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out y)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument HalfHeight in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									y = 1.0;
								}
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], out z)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument HalfDepth in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									z = 1.0;
								}
								CreateCube(ref Builder, x, y, z);
							} break;
						case B3DCsvCommands.Cylinder:
							{
								if (Arguments.Length > 4) {
									currentHost.AddMessage(MessageType.Warning, false, "At most 4 arguments are expected in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								int n = 8;
								if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out n)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument n in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									n = 8;
								}
								if (n < 2) {
									currentHost.AddMessage(MessageType.Error, false, "n is expected to be at least 2 in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									n = 8;
								}
								double r1 = 0.0, r2 = 0.0, h = 1.0;
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out r1)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument UpperRadius in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									r1 = 1.0;
								}
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], out r2)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument LowerRadius in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									r2 = 1.0;
								}
								if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[3], out h)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument Height in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									h = 1.0;
								}
								CreateCylinder(ref Builder, n, r1, r2, h);
								Builder.isCylinder = true;
							} break;
						case B3DCsvCommands.Translate:
						case B3DCsvCommands.TranslateAll:
							{
								if (Arguments.Length > 3) {
									currentHost.AddMessage(MessageType.Warning, false, "At most 3 arguments are expected in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								double x = 0.0, y = 0.0, z = 0.0;
								if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out x)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument X in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									x = 0.0;
								}
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out y)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument Y in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									y = 0.0;
								}
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], out z)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument Z in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									z = 0.0;
								}
								Builder.ApplyTranslation(x, y, z);
								if (cmd == B3DCsvCommands.TranslateAll) {
									Object.ApplyTranslation(x, y, z);
								}
							} break;
						case B3DCsvCommands.Scale:
						case B3DCsvCommands.ScaleAll:
							{
								if (Arguments.Length > 3) {
									currentHost.AddMessage(MessageType.Warning, false, "At most 3 arguments are expected in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								Vector3 Scale = Vector3.One;
								if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out Scale.X)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument X in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else if (Scale.X == 0.0) {
									currentHost.AddMessage(MessageType.Error, false, "X is required to be different from zero in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									Scale.X = 1.0;
								}
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out Scale.Y)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument Y in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else if (Scale.Y == 0.0) {
									currentHost.AddMessage(MessageType.Error, false, "Y is required to be different from zero in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									Scale.Y = 1.0;
								}
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], out Scale.Z)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument Z in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else if (Scale.Z == 0.0) {
									currentHost.AddMessage(MessageType.Error, false, "Z is required to be different from zero in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									Scale.Z = 1.0;
								}
								Builder.ApplyScale(Scale);
								if (cmd == B3DCsvCommands.ScaleAll) {
									Object.ApplyScale(Scale);
								}
							} break;
						case B3DCsvCommands.Rotate:
						case B3DCsvCommands.RotateAll:
							{
								if (Arguments.Length > 4) {
									currentHost.AddMessage(MessageType.Warning, false, "At most 4 arguments are expected in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								Vector3 r = new Vector3();
								double a = 0;
								if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out r.X)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument X in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									r.X = 0.0;
								}
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out r.Y)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument Y in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									r.Y = 0.0;
								}
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], out r.Z)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument Z in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									r.Z = 0.0;
								}
								if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[3], out a)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument Angle in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									a = 0.0;
								}

								if (Math.Abs(r.X) > 1 || Math.Abs(r.Y) > 1 || Math.Abs(r.Z) > 1)
								{
									currentHost.AddMessage(MessageType.Warning, false, "Potentially incorrect rotational direction vector in " + cmd + "- Angle should be the *last* argument at line " + (i + 1).ToString(Culture) + " in file " + FileName);
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
									if (cmd == B3DCsvCommands.RotateAll) {
										Object.ApplyRotation(r, a);
									}
								}
							} break;
						case B3DCsvCommands.Shear:
						case B3DCsvCommands.ShearAll:
							{
								if (Arguments.Length > 7) {
									currentHost.AddMessage(MessageType.Warning, false, "At most 7 arguments are expected in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								Vector3 d = new Vector3();
								Vector3 s = new Vector3();
								double r = 0.0;
								if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out d.X)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument dX in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out d.Y)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument dY in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], out d.Z)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument dZ in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[3], out s.X)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument sX in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length >= 5 && Arguments[4].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[4], out s.Y)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument sY in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length >= 6 && Arguments[5].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[5], out s.Z)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument sZ in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length >= 7 && Arguments[6].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[6], out r)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument Ratio in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									r = 0.0;
								}
								d.Normalize();
								s.Normalize();
								Builder.ApplyShear(d, s, r);
								if (cmd == B3DCsvCommands.ShearAll) {
									Object.ApplyShear(d, s, r);
								}
							} break;
						case B3DCsvCommands.Mirror:
						case B3DCsvCommands.MirrorAll:
							{
								if (Arguments.Length > 6)
								{
									currentHost.AddMessage(MessageType.Warning, false, "At most 6 arguments are expected in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}

								double vx = 0.0, vy = 0.0, vz = 0.0;
								double nx = 0.0, ny = 0.0, nz = 0.0;
								if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out vx))
								{
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument vX in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									vx = 0.0;
								}
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out vy))
								{
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument vY in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									vy = 0.0;
								}
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], out vz))
								{
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument vZ in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									vz = 0.0;
								}
								if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[3], out nx))
								{
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument nX in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									nx = 0.0;
								}
								if (Arguments.Length >= 5 && Arguments[4].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[4], out ny))
								{
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument nY in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									ny = 0.0;
								}
								if (Arguments.Length >= 6 && Arguments[5].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[5], out nz))
								{
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument nZ in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									nz = 0.0;
								}

								if (Arguments.Length < 4)
								{
									nx = vx;
									ny = vy;
									nz = vz;
								}
								Builder.ApplyMirror(vx != 0, vy != 0, vz != 0, nx != 0, ny != 0, nz != 0);
								if (cmd == B3DCsvCommands.MirrorAll)
								{
									Object.ApplyMirror(vx != 0, vy != 0, vz != 0, nx != 0, ny != 0, nz != 0);
								}

							}
							break;
						case B3DCsvCommands.GenerateNormals:
						case B3DCsvCommands.Texture:
							if (cmd == B3DCsvCommands.GenerateNormals & IsB3D) {
								currentHost.AddMessage(MessageType.Warning, false, "GenerateNormals is not a supported command - did you mean [Texture]? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
							} else if (cmd == B3DCsvCommands.Texture & !IsB3D) {
								currentHost.AddMessage(MessageType.Warning, false, "[Texture] is not a supported command - did you mean GenerateNormals? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
							}
							break;
						case B3DCsvCommands.SetColor:
						case B3DCsvCommands.Color:
						{
								if (cmd == B3DCsvCommands.SetColor & IsB3D) {
									currentHost.AddMessage(MessageType.Warning, false, "SetColor is not a supported command - did you mean Color? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else if (cmd == B3DCsvCommands.Color & !IsB3D) {
									currentHost.AddMessage(MessageType.Warning, false, "Color is not a supported command - did you mean SetColor? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length > 4) {
									currentHost.AddMessage(MessageType.Warning, false, "At most 4 arguments are expected in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								int r = 0, g = 0, b = 0, a = 255;
								if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseByteVb6(Arguments[0], out r)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid value for Red in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseByteVb6(Arguments[1], out g)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid value for Green in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseByteVb6(Arguments[2], out b)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid value for Blue in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseByteVb6(Arguments[3], out a)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid value for Alpha in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									a = 255; // special-case
								}

								if (a == 0 && enabledHacks.BveTsHacks || enabledHacks.DisableSemiTransparentFaces)
								{
									/*
									 * BVE2 didn't support semi-transparent faces at all
									 * BVE4 treats faces with an opacity value of 0 as having an opacity value of 1
									 */
									a = 255;
								}
								if (enabledHacks.BveTsHacks && !string.IsNullOrEmpty(Builder.Materials[0].DaytimeTexture) && Builder.Materials[0].DaytimeTexture.EndsWith("Loop3\\CF_BW.bmp", StringComparison.InvariantCultureIgnoreCase))
								{
									/*
									 * Glitched container color in Loop v3
									 * This appears to actually be a 'true' developer typo (same in BVE Structure Viewer) but looks awful so let's fix
									 */
									break;
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
						case B3DCsvCommands.ColorAll:
						case B3DCsvCommands.SetColorAll:
						{
							{
								if (cmd == B3DCsvCommands.SetColorAll & IsB3D) {
									currentHost.AddMessage(MessageType.Warning, false, "SetColorAll is not a supported command - did you mean ColorAll? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else if (cmd == B3DCsvCommands.ColorAll & !IsB3D) {
									currentHost.AddMessage(MessageType.Warning, false, "ColorAll is not a supported command - did you mean SetColorAll? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length > 4) {
									currentHost.AddMessage(MessageType.Warning, false, "At most 4 arguments are expected in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								int r = 0, g = 0, b = 0, a = 255;
								if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseByteVb6(Arguments[0], out r)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid value for Red in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseByteVb6(Arguments[1], out g)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid value for Green in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} 
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseByteVb6(Arguments[2], out b)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid value for Blue in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									b = 0;
								} 
								if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseByteVb6(Arguments[3], out a)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid value for Alpha in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									a = 255; // special-case
								} 
								Color32 newColor = new Color32((byte)r, (byte)g, (byte)b, (byte)a);
								Builder.ApplyColor(newColor, false);
								Object.ApplyColor(newColor, false);
							}
						} break;
						case B3DCsvCommands.EmissiveColorAll:
						case B3DCsvCommands.SetEmissiveColorAll:
						{
							{
								if (cmd == B3DCsvCommands.SetEmissiveColorAll & IsB3D) {
									currentHost.AddMessage(MessageType.Warning, false, "SetEmissiveColorAll is not a supported command - did you mean EmissiveColorAll? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else if (cmd == B3DCsvCommands.EmissiveColorAll & !IsB3D) {
									currentHost.AddMessage(MessageType.Warning, false, "EmissiveColorAll is not a supported command - did you mean SetEmissiveColorAll? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length > 4) {
									currentHost.AddMessage(MessageType.Warning, false, "At most 4 arguments are expected in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								int r = 0, g = 0, b = 0;
								if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseByteVb6(Arguments[0], out r)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid value for Red in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseByteVb6(Arguments[1], out g)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid value for Green in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseByteVb6(Arguments[2], out b)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid value for Blue in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								Color32 newColor = new Color32((byte)r, (byte)g, (byte)b);
								Builder.ApplyColor(newColor, true);
								Object.ApplyColor(newColor, true);
							}
						} break;
						case B3DCsvCommands.SetEmissiveColor:
						case B3DCsvCommands.EmissiveColor:
							{
								if (cmd == B3DCsvCommands.SetEmissiveColor & IsB3D) {
									currentHost.AddMessage(MessageType.Warning, false, "SetEmissiveColor is not a supported command - did you mean EmissiveColor? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else if (cmd == B3DCsvCommands.EmissiveColor & !IsB3D) {
									currentHost.AddMessage(MessageType.Warning, false, "EmissiveColor is not a supported command - did you mean SetEmissiveColor? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length > 3) {
									currentHost.AddMessage(MessageType.Warning, false, "At most 3 arguments are expected in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								int r = 0, g = 0, b = 0;
								if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseByteVb6(Arguments[0], out r)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid value for Red in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseByteVb6(Arguments[1], out g)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid value for Green in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseByteVb6(Arguments[2], out b)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid value for Blue in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								int m = Builder.Materials.Length;
								Array.Resize(ref Builder.Materials, m << 1);
								for (int j = m; j < Builder.Materials.Length; j++) {
									Builder.Materials[j] = new Material(Builder.Materials[j - m])
									{
										EmissiveColor = new Color24((byte)r, (byte)g, (byte)b),
										Flags = Builder.Materials[0].Flags | MaterialFlags.Emissive,
										BlendMode = Builder.Materials[0].BlendMode,
										GlowAttenuationData = Builder.Materials[0].GlowAttenuationData,
										DaytimeTexture = Builder.Materials[0].DaytimeTexture,
										NighttimeTexture = Builder.Materials[0].NighttimeTexture,
										TransparentColor = Builder.Materials[0].TransparentColor,
										WrapMode = Builder.Materials[0].WrapMode
									};
								}
								for (int j = 0; j < Builder.Faces.Count; j++)
								{
									MeshFace f = Builder.Faces[j];
									f.Material += (ushort)m;
									Builder.Faces[j] = f;
								}
							} break;
						case B3DCsvCommands.SetDecalTransparentColor:
						case B3DCsvCommands.Transparent:
							{
								
								if (cmd == B3DCsvCommands.SetDecalTransparentColor & IsB3D) {
									currentHost.AddMessage(MessageType.Warning, false, "SetDecalTransparentColor is not a supported command - did you mean Transparent? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else if (cmd == B3DCsvCommands.Transparent & !IsB3D) {
									currentHost.AddMessage(MessageType.Warning, false, "Transparent is not a supported command - did you mean SetDecalTransparentColor? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length > 3) {
									currentHost.AddMessage(MessageType.Warning, false, "At most 3 arguments are expected in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else if (Arguments.Length == 0) {
									currentHost.AddMessage(MessageType.Warning, false, "No color was specified in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName + "- This may produce unexpected results.");
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
								if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseByteVb6(Arguments[0], out r)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid value for Red in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} 
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseByteVb6(Arguments[1], out g)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid value for Green in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseByteVb6(Arguments[2], out b)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid value for Blue in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								lastTransparentColor = new Color24((byte) r, (byte) g, (byte) b);
								int ml = Builder.Materials.Length;
								if (enabledHacks.BveTsHacks && !string.IsNullOrEmpty(Builder.Materials[ml -1].DaytimeTexture) && Builder.Materials[ml - 1].DaytimeTexture.StartsWith(CompatibilityFolder) && Builder.Materials[ml - 1].DaytimeTexture.IndexOf("Signals\\Static", StringComparison.InvariantCultureIgnoreCase) != -1)
								{
									// If using a replaced static signal texture ensure the transparent color is correct
									lastTransparentColor = new Color24(0, 0, 255);
								}
								for (int j = 0; j < Builder.Materials.Length; j++) {
									Builder.Materials[j].TransparentColor = lastTransparentColor.Value;
									Builder.Materials[j].Flags |= MaterialFlags.TransparentColor;
								}
							} break;
						case B3DCsvCommands.SetBlendingMode:
						case B3DCsvCommands.SetBlendMode:
						case B3DCsvCommands.BlendingMode:
						case B3DCsvCommands.BlendMode:
						{
								if ((cmd == B3DCsvCommands.SetBlendMode || cmd == B3DCsvCommands.SetBlendingMode) & IsB3D) {
									currentHost.AddMessage(MessageType.Warning, false, "SetBlendMode is not a supported command - did you mean BlendMode? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else if (cmd == B3DCsvCommands.BlendMode & !IsB3D) {
									currentHost.AddMessage(MessageType.Warning, false, "BlendMode is not a supported command - did you mean SetBlendMode? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length > 3) {
									currentHost.AddMessage(MessageType.Warning, false, "At most 3 arguments are expected in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
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
											currentHost.AddMessage(MessageType.Error, false, "The given BlendMode is not supported in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											blendmode = MeshMaterialBlendMode.Normal;
											break;
									}
								}
								double glowhalfdistance = 0.0;
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out glowhalfdistance)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument GlowHalfDistance in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									glowhalfdistance = 0;
								}
								GlowAttenuationMode glowmode = GlowAttenuationMode.DivisionExponent4;
								if (Arguments.Length >= 3 && Arguments[2].Length > 0) {
									switch (Arguments[2].ToLowerInvariant()) {
											case "divideexponent2": glowmode = GlowAttenuationMode.DivisionExponent2; break;
											case "divideexponent4": glowmode = GlowAttenuationMode.DivisionExponent4; break;
										default:
											currentHost.AddMessage(MessageType.Error, false, "The given GlowAttenuationMode is not supported in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											break;
									}
								}
								for (int j = 0; j < Builder.Materials.Length; j++) {
									Builder.Materials[j].BlendMode = blendmode;
									Builder.Materials[j].GlowAttenuationData = Glow.GetAttenuationData(glowhalfdistance, glowmode);
								}
						} break;
						case B3DCsvCommands.SetWrapMode:
						case B3DCsvCommands.WrapMode:
							{
								if (cmd == B3DCsvCommands.SetWrapMode & IsB3D)
								{
									currentHost.AddMessage(MessageType.Warning, false, "SetWrapMode is not a supported command - did you mean WrapMode? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								else if (cmd == B3DCsvCommands.WrapMode & !IsB3D)
								{
									currentHost.AddMessage(MessageType.Warning, false, "WrapMode is not a supported command - did you mean SetWrapMode? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length > 3)
								{
									currentHost.AddMessage(MessageType.Warning, false, "At most 3 arguments are expected in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
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
											currentHost.AddMessage(MessageType.Error, false, "The given WrapMode is not supported in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											wrapmode = null;
											break;
									}
								}
								for (int j = 0; j < Builder.Materials.Length; j++)
								{
									Builder.Materials[j].WrapMode = wrapmode;
								}
							} break;
						case B3DCsvCommands.LoadTexture:
						case B3DCsvCommands.Load:
							{
							/*
							 * Loading a texture should definitely reset the MeshBuilder, otherwise that's insane
							 * Hopefully won't increase error numbers on existing content too much....
							 */
							firstMeshBuilder = false;
								if (cmd == B3DCsvCommands.LoadTexture & IsB3D) {
									currentHost.AddMessage(MessageType.Warning, false, "LoadTexture is not a supported command - did you mean Load? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else if (cmd == B3DCsvCommands.Load & !IsB3D) {
									currentHost.AddMessage(MessageType.Warning, false, "Load is not a supported command - did you mean LoadTexture? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length > 2) {
									currentHost.AddMessage(MessageType.Warning, false, "At most 2 arguments are expected in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								string tday = null, tnight = null;
								if (Arguments.Length >= 1 && Arguments[0].Length != 0) {
									if (Path.ContainsInvalidChars(Arguments[0])) {
										currentHost.AddMessage(MessageType.Error, false, "DaytimeTexture contains illegal characters in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									} else {
										try
										{
											tday = Path.CombineFile(Path.GetDirectoryName(FileName), Arguments[0]);
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
													tday = Path.CombineFile(Path.GetDirectoryName(FileName), Arguments[0]);
													if (System.IO.File.Exists(tday))
													{
														hackFound = true;
													}
												}

												if (Arguments[0].StartsWith("/U5/", StringComparison.InvariantCultureIgnoreCase))
												{
													Arguments[0] = Arguments[0].Substring(4);
													tday = Path.CombineFile(Path.GetDirectoryName(FileName), Arguments[0]);
													if (System.IO.File.Exists(tday))
													{
														hackFound = true;
													}
												}

												if (Arguments[0].StartsWith("U5/", StringComparison.InvariantCultureIgnoreCase))
												{
													Arguments[0] = Arguments[0].Substring(3);
													tday = Path.CombineFile(Path.GetDirectoryName(FileName), Arguments[0]);
													if (System.IO.File.Exists(tday))
													{
														hackFound = true;
													}
												}
											}
											if (!hackFound)
											{
												currentHost.AddMessage(MessageType.Error, true, "DaytimeTexture " + tday + " could not be found in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
												tday = null;
											}
											
										}
									}
								}
								if (Arguments.Length >= 2 && Arguments[1].Length != 0) {
									if (Arguments[0].Length == 0) {
										currentHost.AddMessage(MessageType.Error, true, "DaytimeTexture is required to be specified in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									} else {
										if (Path.ContainsInvalidChars(Arguments[1])) {
											currentHost.AddMessage(MessageType.Error, false, "NighttimeTexture contains illegal characters in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
										} else
										{
											bool ignoreAsInvalid = false;
											try
											{
												tnight = Path.CombineFile(Path.GetDirectoryName(FileName), Arguments[1]);
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
												currentHost.AddMessage(MessageType.Error, true, "The NighttimeTexture " + tnight + " could not be found in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
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
						case B3DCsvCommands.LoadLightMap:
						case B3DCsvCommands.LightMap:
							string lightmap = null;
							if (Arguments.Length >= 1 && Arguments[0].Length != 0)
							{
								if (Path.ContainsInvalidChars(Arguments[0]))
								{
									currentHost.AddMessage(MessageType.Error, false, "LightMap contains illegal characters in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								else
								{
									try
									{
										lightmap = Path.CombineFile(Path.GetDirectoryName(FileName), Arguments[0]);
									}
									catch
									{
										lightmap = null;
									}
								}
								for (int j = 0; j < Builder.Materials.Length; j++) {
									Builder.Materials[j].LightMap = lightmap;
								}
							}
							break;
						case B3DCsvCommands.SetText:
						case B3DCsvCommands.Text:
							{
								if (cmd == B3DCsvCommands.SetText & IsB3D)
								{
									currentHost.AddMessage(MessageType.Warning, false,
									  "SetText is not a supported command - did you mean Text? - at line " + (i + 1).ToString(Culture) + " in file " +
									  FileName);
								}
								else if (cmd == B3DCsvCommands.Text & !IsB3D)
								{
									currentHost.AddMessage(MessageType.Warning, false,
									  "Text is not a supported command - did you mean SetText? - at line " + (i + 1).ToString(Culture) + " in file " +
									  FileName);
								}

								if (Arguments.Length > 2)
								{
									currentHost.AddMessage(MessageType.Warning, false,
									  "At most 2 arguments are expected in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " +
									  FileName);
								}
								string text = Arguments[0];

								for (int j = 0; j < Builder.Materials.Length; j++)
								{
									Builder.Materials[j].Text = text;
								}
							}
							break;
						case B3DCsvCommands.SetTextColor:
						case B3DCsvCommands.TextColor:
							{
								if (cmd == B3DCsvCommands.SetTextColor & IsB3D)
								{
									currentHost.AddMessage(MessageType.Warning, false,
									  "SetTextColor is not a supported command - did you mean TextColor? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								else if (cmd == B3DCsvCommands.TextColor & !IsB3D)
								{
									currentHost.AddMessage(MessageType.Warning, false,
									  "TextColor is not a supported command - did you mean SetTextColor? - at line " + (i + 1).ToString(Culture) + " in file " +
									  FileName);
								}

								if (Arguments.Length != 3)
								{
									currentHost.AddMessage(MessageType.Warning, false,
									  "3 arguments are expected in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								int r = 0, g = 0, b = 0;
								if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !int.TryParse(Arguments[0], out r))
								{
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument R in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !int.TryParse(Arguments[1], out g))
								{
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument G in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !int.TryParse(Arguments[2], out b))
								{
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument B in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								Color24 textColor = new Color24((byte)r, (byte)g, (byte)b);
								for (int j = 0; j < Builder.Materials.Length; j++)
								{
									Builder.Materials[j].TextColor = textColor;
								}
							}
							break;
						case B3DCsvCommands.SetBackgroundColor:
						case B3DCsvCommands.BackgroundColor:
							{
								if (cmd == B3DCsvCommands.SetBackgroundColor & IsB3D)
								{
									currentHost.AddMessage(MessageType.Warning, false,
									  "SetBackgroundColor is not a supported command - did you mean BackgroundColor? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								else if (cmd == B3DCsvCommands.BackgroundColor & !IsB3D)
								{
									currentHost.AddMessage(MessageType.Warning, false,
									  "BackgroundColor is not a supported command - did you mean SetBackgroundColor? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}

								if (Arguments.Length != 3)
								{
									currentHost.AddMessage(MessageType.Warning, false,
									  "3 arguments are expected in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								int r = 0, g = 0, b = 0;
								if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !int.TryParse(Arguments[0], out r))
								{
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument R in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !int.TryParse(Arguments[1], out g))
								{
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument G in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !int.TryParse(Arguments[2], out b))
								{
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument B in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								Color24 backgroundColor = new Color24((byte)r, (byte)g, (byte)b);
								for (int j = 0; j < Builder.Materials.Length; j++)
								{
									Builder.Materials[j].BackgroundColor = backgroundColor;
								}
							}
							break;
						case B3DCsvCommands.SetTextPadding:
						case B3DCsvCommands.TextPadding:
							{
								if (cmd == B3DCsvCommands.SetTextPadding & IsB3D)
								{
									currentHost.AddMessage(MessageType.Warning, false,
									  "SetTextPadding is not a supported command - did you mean TextPadding? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								else if (cmd == B3DCsvCommands.TextPadding & !IsB3D)
								{
									currentHost.AddMessage(MessageType.Warning, false,
									  "TextPadding is not a supported command - did you mean SetTextPadding? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}

								if(!Vector2.TryParse(Arguments, out Vector2 Padding))
								{
									currentHost.AddMessage(MessageType.Warning, false, "Invalid TextPadding at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}

								for (int j = 0; j < Builder.Materials.Length; j++)
								{
									Builder.Materials[j].TextPadding = Padding;
								}
							}
							break;
						case B3DCsvCommands.SetFont:
						case B3DCsvCommands.Font:
							{
								if (cmd == B3DCsvCommands.SetFont & IsB3D)
								{
									currentHost.AddMessage(MessageType.Warning, false,
									  "SetFont is not a supported command - did you mean Font? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								else if (cmd == B3DCsvCommands.Font & !IsB3D)
								{
									currentHost.AddMessage(MessageType.Warning, false,
									  "Font is not a supported command - did you mean SetFont? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}

								if (Arguments.Length > 1)
								{
									currentHost.AddMessage(MessageType.Warning, false,
									  "1 argument is expected in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
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
						case B3DCsvCommands.SetTextureCoordinates:
						case B3DCsvCommands.Coordinates:
							{
								if (cmd == B3DCsvCommands.SetTextureCoordinates & IsB3D) {
									currentHost.AddMessage(MessageType.Warning, false, "SetTextureCoordinates is not a supported command - did you mean Coordinates? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else if (cmd == B3DCsvCommands.Coordinates & !IsB3D) {
									currentHost.AddMessage(MessageType.Warning, false, "Coordinates is not a supported command - did you mean SetTextureCoordinates? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if ((Arguments.Length > 6 && !string.IsNullOrEmpty(Builder.Materials[Builder.Materials.Length - 1].LightMap)) || (Arguments.Length > 3 && string.IsNullOrEmpty(Builder.Materials[Builder.Materials.Length - 1].LightMap))) {
									currentHost.AddMessage(MessageType.Warning, false, "At most 3 arguments are expected in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								int j = 0; float x = 0.0f, y = 0.0f, lx = 0.0f, ly = 0.0f;
								if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out j)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument VertexIndex in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									j = 0;
								}
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseFloatVb6(Arguments[1], out x)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument X in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									x = 0.0f;
								}
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseFloatVb6(Arguments[2], out y)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument Y in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									y = 0.0f;
								}
								if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseFloatVb6(Arguments[3], out lx)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument LX in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									lx = 0.0f;
								}
								if (Arguments.Length >= 5 && Arguments[4].Length > 0 && !NumberFormats.TryParseFloatVb6(Arguments[4], out ly)) {
									currentHost.AddMessage(MessageType.Error, false, "Invalid argument LX in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									ly = 0.0f;
								}
								if (j >= 0 & j < Builder.Vertices.Count) {
									if (!string.IsNullOrEmpty(Builder.Materials[Builder.Materials.Length - 1].LightMap))
									{
										LightMappedVertex vertex = new LightMappedVertex(Builder.Vertices[j] as Vertex)
										{
											TextureCoordinates = new Vector2(x, y),
											LightMapCoordinates = new Vector2(lx, ly)
										};
										Builder.Vertices[j] = vertex;
									}
									else
									{
										Builder.Vertices[j].TextureCoordinates = new Vector2(x, y);
									}
									
								} else {
									currentHost.AddMessage(MessageType.Error, false, "VertexIndex references a non-existing vertex in " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
							} break;
						case B3DCsvCommands.EnableCrossFading:
						case B3DCsvCommands.CrossFading:
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
								}

								
								if (!IsB3D)
								{
									int firstSpace = Math.Max(Command.IndexOf(' '), 0);
									if(Enum.TryParse(Command.Substring(0, firstSpace), true, out cmd))
									{
										currentHost.AddMessage(MessageType.Error, false, "Incorrect argument separator used for CSV format in command " + cmd + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									}
									else
									{
										currentHost.AddMessage(MessageType.Error, false, "The command " + Command + " is not supported at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									}

				
								}
								else
								{
									currentHost.AddMessage(MessageType.Error, false, "The command " + Command + " is not supported at line " + (i + 1).ToString(Culture) + " in file " + FileName);
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
						Object.Mesh.Materials[k].DaytimeTexture.OpenGlTextures[(int)wrap].Used = true;
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
