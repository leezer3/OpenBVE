using System;
using System.Drawing;
using OpenBveApi.Math;
using OpenBveApi.Textures;
using OpenBveApi;

namespace OpenBve {
	internal static class CsvB3dObjectParser {

		// structures
		private class Material {
			internal World.ColorRGBA Color;
			internal World.ColorRGB EmissiveColor;
			internal bool EmissiveColorUsed;
			internal World.ColorRGB TransparentColor;
			internal bool TransparentColorUsed;
			internal string DaytimeTexture;
			internal string NighttimeTexture;
			internal World.MeshMaterialBlendMode BlendMode;
			internal ushort GlowAttenuationData;
			internal Color TextColor;
			internal Color BackgroundColor;
			internal string Font;
			internal Vector2 TextPadding;
			internal string Text;
			internal Material() {
				this.Color = new World.ColorRGBA(255, 255, 255, 255);
				this.EmissiveColor = new World.ColorRGB(0, 0, 0);
				this.EmissiveColorUsed = false;
				this.TransparentColor = new World.ColorRGB(0, 0, 0);
				this.TransparentColorUsed = false;
				this.DaytimeTexture = null;
				this.NighttimeTexture = null;
				this.BlendMode = World.MeshMaterialBlendMode.Normal;
				this.GlowAttenuationData = 0;
				this.TextColor = System.Drawing.Color.Black;
				this.BackgroundColor = System.Drawing.Color.White;
				this.TextPadding = new Vector2(0, 0);
				this.Font = "Arial";
			}
			internal Material(Material Prototype) {
				this.Color = Prototype.Color;
				this.EmissiveColor = Prototype.EmissiveColor;
				this.EmissiveColorUsed = Prototype.EmissiveColorUsed;
				this.TransparentColor = Prototype.TransparentColor;
				this.TransparentColorUsed = Prototype.TransparentColorUsed;
				this.DaytimeTexture = Prototype.DaytimeTexture;
				this.NighttimeTexture = Prototype.NighttimeTexture;
				this.BlendMode = Prototype.BlendMode;
				this.GlowAttenuationData = Prototype.GlowAttenuationData;
				this.TextColor = Prototype.TextColor;
				this.BackgroundColor = Prototype.BackgroundColor;
				this.TextPadding = Prototype.TextPadding;
				this.Font = Prototype.Font;
			}
		}
		private class MeshBuilder {
			internal World.Vertex[] Vertices;
			internal World.MeshFace[] Faces;
			internal Material[] Materials;
			internal MeshBuilder() {
				this.Vertices = new World.Vertex[] { };
				this.Faces = new World.MeshFace[] { };
				this.Materials = new Material[] { new Material() };
			}
		}

		// read object
		/// <summary>Loads a CSV or B3D object from a file.</summary>
		/// <param name="FileName">The text file to load the animated object from. Must be an absolute file name.</param>
		/// <param name="Encoding">The encoding the file is saved in. If the file uses a byte order mark, the encoding indicated by the byte order mark is used and the Encoding parameter is ignored.</param>
		/// <param name="LoadMode">The texture load mode.</param>
		/// <param name="ForceTextureRepeatX">Whether to force TextureWrapMode.Repeat for referenced textures on the X-axis.</param>
		/// <param name="ForceTextureRepeatY">Whether to force TextureWrapMode.Repeat for referenced textures on the Y-axis.</param>
		/// <returns>The object loaded.</returns>
		internal static ObjectManager.StaticObject ReadObject(string FileName, System.Text.Encoding Encoding, ObjectManager.ObjectLoadMode LoadMode, bool ForceTextureRepeatX, bool ForceTextureRepeatY) {
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			bool IsB3D = string.Equals(System.IO.Path.GetExtension(FileName), ".b3d", StringComparison.OrdinalIgnoreCase);
			// initialize object
			ObjectManager.StaticObject Object = new ObjectManager.StaticObject();
			Object.Mesh.Faces = new World.MeshFace[] { };
			Object.Mesh.Materials = new World.MeshMaterial[] { };
			Object.Mesh.Vertices = new World.Vertex[] { };
			// read lines
			string[] Lines = System.IO.File.ReadAllLines(FileName, Encoding);
			// parse lines
			MeshBuilder Builder = new MeshBuilder();
			World.Vector3Df[] Normals = new World.Vector3Df[4];
			bool CommentStarted = false;
			for (int i = 0; i < Lines.Length; i++) {
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
						Lines[i] = Lines[i].Substring(0, k);
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
				string[] Arguments = Lines[i].Split(new char[] { ',' }, StringSplitOptions.None);
				for (int j = 0; j < Arguments.Length; j++) {
					Arguments[j] = Arguments[j].Trim();
				}
				{
					// remove unused arguments at the end of the chain
					int j;
					for (j = Arguments.Length - 1; j >= 0; j--) {
						if (Arguments[j].Length != 0) break;
					}
					Array.Resize<string>(ref Arguments, j + 1);
				}
				// style
				string Command;
				if (IsB3D & Arguments.Length != 0) {
					// b3d
					int j = Arguments[0].IndexOf(' ');
					if (j >= 0) {
						Command = Arguments[0].Substring(0, j).TrimEnd();
						Arguments[0] = Arguments[0].Substring(j + 1).TrimStart();
					} else {
						Command = Arguments[0];
						Arguments = new string[] { };
					}
				} else if (Arguments.Length != 0) {
					// csv
					Command = Arguments[0];
					for (int j = 0; j < Arguments.Length - 1; j++) {
						Arguments[j] = Arguments[j + 1];
					}
					Array.Resize<string>(ref Arguments, Arguments.Length - 1);
				} else {
					// empty
					Command = null;
				}
				// parse terms
				if (Command != null) {
					string cmd = Command.ToLowerInvariant();
					switch(cmd) {
						case "createmeshbuilder":
						case "[meshbuilder]":
							{
								if (cmd == "createmeshbuilder" & IsB3D) {
									Interface.AddMessage(Interface.MessageType.Warning, false, "CreateMeshBuilder is not a supported command - did you mean [MeshBuilder]? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else if (cmd == "[meshbuilder]" & !IsB3D) {
									Interface.AddMessage(Interface.MessageType.Warning, false, "[MeshBuilder] is not a supported command - did you mean CreateMeshBuilder? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length > 0) {
									Interface.AddMessage(Interface.MessageType.Warning, false, "0 arguments are expected in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								ApplyMeshBuilder(ref Object, Builder, LoadMode, ForceTextureRepeatX, ForceTextureRepeatY);
								Builder = new MeshBuilder();
								Normals = new World.Vector3Df[4];
							} break;
						case "addvertex":
						case "vertex":
							{
								if (cmd == "addvertex" & IsB3D) {
									Interface.AddMessage(Interface.MessageType.Warning, false, "AddVertex is not a supported command - did you mean Vertex? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else if (cmd == "vertex" & !IsB3D) {
									Interface.AddMessage(Interface.MessageType.Warning, false, "Vertex is not a supported command - did you mean AddVertex? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length > 6) {
									Interface.AddMessage(Interface.MessageType.Warning, false, "At most 6 arguments are expected in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								double vx = 0.0, vy = 0.0, vz = 0.0;
								if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out vx)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument vX in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									vx = 0.0;
								}
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out vy)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument vY in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									vy = 0.0;
								}
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], out vz)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument vZ in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									vz = 0.0;
								}
								double nx = 0.0, ny = 0.0, nz = 0.0;
								if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[3], out nx)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument nX in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									nx = 0.0;
								}
								if (Arguments.Length >= 5 && Arguments[4].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[4], out ny)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument nY in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									ny = 0.0;
								}
								if (Arguments.Length >= 6 && Arguments[5].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[5], out nz)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument nZ in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									nz = 0.0;
								}
								World.Normalize(ref nx, ref ny, ref nz);
								Array.Resize<World.Vertex>(ref Builder.Vertices, Builder.Vertices.Length + 1);
								while (Builder.Vertices.Length >= Normals.Length) {
									Array.Resize<World.Vector3Df>(ref Normals, Normals.Length << 1);
								}
								Builder.Vertices[Builder.Vertices.Length - 1].Coordinates = new World.Vector3D(vx, vy, vz);
								Normals[Builder.Vertices.Length - 1] = new World.Vector3Df((float)nx, (float)ny, (float)nz);
							} break;
						case "addface":
						case "addface2":
						case "face":
						case "face2":
							{
								if (IsB3D) {
									if (cmd == "addface") {
										Interface.AddMessage(Interface.MessageType.Warning, false, "AddFace is not a supported command - did you mean Face? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									} else if (cmd == "addface2") {
										Interface.AddMessage(Interface.MessageType.Warning, false, "AddFace2 is not a supported command - did you mean Face2? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									}
								} else {
									if (cmd == "face") {
										Interface.AddMessage(Interface.MessageType.Warning, false, "Face is not a supported command - did you mean AddFace? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									} else if (cmd == "face2") {
										Interface.AddMessage(Interface.MessageType.Warning, false, "Face2 is not a supported command - did you mean AddFace2? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									}
								}
								if (Arguments.Length < 3) {
									Interface.AddMessage(Interface.MessageType.Error, false, "At least 3 arguments are required in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else {
									bool q = true;
									int[] a = new int[Arguments.Length];
									for (int j = 0; j < Arguments.Length; j++) {
										if (!NumberFormats.TryParseIntVb6(Arguments[j], out a[j])) {
											Interface.AddMessage(Interface.MessageType.Error, false, "v" + j.ToString(Culture) + " is invalid in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											q = false;
											break;
										} else if (a[j] < 0 | a[j] >= Builder.Vertices.Length) {
											Interface.AddMessage(Interface.MessageType.Error, false, "v" + j.ToString(Culture) + " references a non-existing vertex in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											q = false;
											break;
										} else if (a[j] > 65535) {
											Interface.AddMessage(Interface.MessageType.Error, false, "v" + j.ToString(Culture) + " indexes a vertex above 65535 which is not currently supported in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											q = false;
											break;
										}
									}
									if (q) {
										int f = Builder.Faces.Length;
										Array.Resize<World.MeshFace>(ref Builder.Faces, f + 1);
										Builder.Faces[f] = new World.MeshFace();
										Builder.Faces[f].Vertices = new World.MeshFaceVertex[Arguments.Length];
										while (Builder.Vertices.Length > Normals.Length) {
											Array.Resize<World.Vector3Df>(ref Normals, Normals.Length << 1);
										}
										for (int j = 0; j < Arguments.Length; j++) {
											Builder.Faces[f].Vertices[j].Index = (ushort)a[j];
											Builder.Faces[f].Vertices[j].Normal = Normals[a[j]];
										}
										if (cmd == "addface2" | cmd == "face2") {
											Builder.Faces[f].Flags = (byte)World.MeshFace.Face2Mask;
										}
									}
								}
							} break;
						case "cube":
							{
								if (Arguments.Length > 3) {
									Interface.AddMessage(Interface.MessageType.Warning, false, "At most 3 arguments are expected in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								double x = 0.0;
								if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out x)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument HalfWidth in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									x = 1.0;
								}
								double y = x, z = x;
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out y)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument HalfHeight in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									y = 1.0;
								}
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], out z)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument HalfDepth in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									z = 1.0;
								}
								CreateCube(ref Builder, x, y, z);
							} break;
						case "cylinder":
							{
								if (Arguments.Length > 4) {
									Interface.AddMessage(Interface.MessageType.Warning, false, "At most 4 arguments are expected in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								int n = 8;
								if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out n)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument n in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									n = 8;
								}
								if (n < 2) {
									Interface.AddMessage(Interface.MessageType.Error, false, "n is expected to be at least 2 in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									n = 8;
								}
								double r1 = 0.0, r2 = 0.0, h = 1.0;
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out r1)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument UpperRadius in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									r1 = 1.0;
								}
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], out r2)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument LowerRadius in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									r2 = 1.0;
								}
								if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[3], out h)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument Height in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									h = 1.0;
								}
								CreateCylinder(ref Builder, n, r1, r2, h);
							} break;
						case "translate":
						case "translateall":
							{
								if (Arguments.Length > 3) {
									Interface.AddMessage(Interface.MessageType.Warning, false, "At most 3 arguments are expected in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								double x = 0.0, y = 0.0, z = 0.0;
								if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out x)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument X in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									x = 0.0;
								}
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out y)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument Y in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									y = 0.0;
								}
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], out z)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument Z in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									z = 0.0;
								}
								ApplyTranslation(Builder, x, y, z);
								if (cmd == "translateall") {
									ApplyTranslation(Object, x, y, z);
								}
							} break;
						case "scale":
						case "scaleall":
							{
								if (Arguments.Length > 3) {
									Interface.AddMessage(Interface.MessageType.Warning, false, "At most 3 arguments are expected in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								double x = 1.0, y = 1.0, z = 1.0;
								if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out x)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument X in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									x = 1.0;
								} else if (x == 0.0) {
									Interface.AddMessage(Interface.MessageType.Error, false, "X is required to be different from zero in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									x = 1.0;
								}
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out y)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument Y in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									y = 1.0;
								} else if (y == 0.0) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Y is required to be different from zero in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									y = 1.0;
								}
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], out z)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument Z in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									z = 1.0;
								} else if (z == 0.0) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Z is required to be different from zero in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									z = 1.0;
								}
								ApplyScale(Builder, x, y, z);
								if (cmd == "scaleall") {
									ApplyScale(Object, x, y, z);
								}
							} break;
						case "rotate":
						case "rotateall":
							{
								if (Arguments.Length > 4) {
									Interface.AddMessage(Interface.MessageType.Warning, false, "At most 4 arguments are expected in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								double x = 0.0, y = 0.0, z = 0.0, a = 0.0;
								if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out x)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument X in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									x = 0.0;
								}
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out y)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument Y in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									y = 0.0;
								}
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], out z)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument Z in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									z = 0.0;
								}
								if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[3], out a)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument Angle in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									a = 0.0;
								}
								double t = x * x + y * y + z * z;
								if (t == 0.0) {
									x = 1.0;
									y = 0.0;
									z = 0.0;
									t = 1.0;
								}
								if (a != 0.0) {
									t = 1.0 / Math.Sqrt(t);
									x *= t;
									y *= t;
									z *= t;
									a *= 0.0174532925199433;
									ApplyRotation(Builder, x, y, z, a);
									if (cmd == "rotateall") {
										ApplyRotation(Object, x, y, z, a);
									}
								}
							} break;
						case "shear":
						case "shearall":
							{
								if (Arguments.Length > 7) {
									Interface.AddMessage(Interface.MessageType.Warning, false, "At most 7 arguments are expected in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								double dx = 0.0, dy = 0.0, dz = 0.0;
								double sx = 0.0, sy = 0.0, sz = 0.0;
								double r = 0.0;
								if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[0], out dx)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument dX in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									dx = 0.0;
								}
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out dy)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument dY in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									dy = 0.0;
								}
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[2], out dz)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument dZ in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									dz = 0.0;
								}
								if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[3], out sx)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument sX in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									sx = 0.0;
								}
								if (Arguments.Length >= 5 && Arguments[4].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[4], out sy)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument sY in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									sy = 0.0;
								}
								if (Arguments.Length >= 6 && Arguments[5].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[5], out sz)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument sZ in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									sz = 0.0;
								}
								if (Arguments.Length >= 7 && Arguments[6].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[6], out r)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument Ratio in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									r = 0.0;
								}
								World.Normalize(ref dx, ref dy, ref dz);
								World.Normalize(ref sx, ref sy, ref sz);
								ApplyShear(Builder, dx, dy, dz, sx, sy, sz, r);
								if (cmd == "shearall") {
									ApplyShear(Object, dx, dy, dz, sx, sy, sz, r);
								}
							} break;
						case "generatenormals":
						case "[texture]":
							if (cmd == "generatenormals" & IsB3D) {
								Interface.AddMessage(Interface.MessageType.Warning, false, "GenerateNormals is not a supported command - did you mean [Texture]? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
							} else if (cmd == "[texture]" & !IsB3D) {
								Interface.AddMessage(Interface.MessageType.Warning, false, "[Texture] is not a supported command - did you mean GenerateNormals? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
							}
							break;
						case "setcolor":
						case "color":
							{
								if (cmd == "setcolor" & IsB3D) {
									Interface.AddMessage(Interface.MessageType.Warning, false, "SetColor is not a supported command - did you mean Color? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else if (cmd == "color" & !IsB3D) {
									Interface.AddMessage(Interface.MessageType.Warning, false, "Color is not a supported command - did you mean SetColor? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length > 4) {
									Interface.AddMessage(Interface.MessageType.Warning, false, "At most 4 arguments are expected in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								int r = 0, g = 0, b = 0, a = 255;
								if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out r)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument Red in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									r = 0;
								} else if (r < 0 | r > 255) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Red is required to be within the range from 0 to 255 in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									r = r < 0 ? 0 : 255;
								}
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[1], out g)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument Green in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									g = 0;
								} else if (g < 0 | g > 255) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Green is required to be within the range from 0 to 255 in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									g = g < 0 ? 0 : 255;
								}
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[2], out b)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument Blue in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									b = 0;
								} else if (b < 0 | b > 255) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Blue is required to be within the range from 0 to 255 in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									b = b < 0 ? 0 : 255;
								}
								if (Arguments.Length >= 4 && Arguments[3].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[3], out a)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument Alpha in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									a = 255;
								} else if (a < 0 | a > 255) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Alpha is required to be within the range from 0 to 255 in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									a = a < 0 ? 0 : 255;
								}
								int m = Builder.Materials.Length;
								Array.Resize<Material>(ref Builder.Materials, m << 1);
								for (int j = m; j < Builder.Materials.Length; j++) {
									Builder.Materials[j] = new Material(Builder.Materials[j - m]);
									Builder.Materials[j].Color = new World.ColorRGBA((byte)r, (byte)g, (byte)b, (byte)a);
									Builder.Materials[j].BlendMode = Builder.Materials[0].BlendMode;
									Builder.Materials[j].GlowAttenuationData = Builder.Materials[0].GlowAttenuationData;
									Builder.Materials[j].DaytimeTexture = Builder.Materials[0].DaytimeTexture;
									Builder.Materials[j].NighttimeTexture = Builder.Materials[0].NighttimeTexture;
									Builder.Materials[j].TransparentColor = Builder.Materials[0].TransparentColor;
									Builder.Materials[j].TransparentColorUsed = Builder.Materials[0].TransparentColorUsed;
								}
								for (int j = 0; j < Builder.Faces.Length; j++) {
									Builder.Faces[j].Material += (ushort)m;
								}
							} break;
						case "setemissivecolor":
						case "emissivecolor":
							{
								if (cmd == "setemissivecolor" & IsB3D) {
									Interface.AddMessage(Interface.MessageType.Warning, false, "SetEmissiveColor is not a supported command - did you mean EmissiveColor? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else if (cmd == "emissivecolor" & !IsB3D) {
									Interface.AddMessage(Interface.MessageType.Warning, false, "EmissiveColor is not a supported command - did you mean SetEmissiveColor? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length > 3) {
									Interface.AddMessage(Interface.MessageType.Warning, false, "At most 3 arguments are expected in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								int r = 0, g = 0, b = 0;
								if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out r)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument Red in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									r = 0;
								} else if (r < 0 | r > 255) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Red is required to be within the range from 0 to 255 in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									r = r < 0 ? 0 : 255;
								}
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[1], out g)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument Green in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									g = 0;
								} else if (g < 0 | g > 255) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Green is required to be within the range from 0 to 255 in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									g = g < 0 ? 0 : 255;
								}
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[2], out b)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument Blue in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									b = 0;
								} else if (b < 0 | b > 255) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Blue is required to be within the range from 0 to 255 in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									b = b < 0 ? 0 : 255;
								}
								int m = Builder.Materials.Length;
								Array.Resize<Material>(ref Builder.Materials, m << 1);
								for (int j = m; j < Builder.Materials.Length; j++) {
									Builder.Materials[j] = new Material(Builder.Materials[j - m]);
									Builder.Materials[j].EmissiveColor = new World.ColorRGB((byte)r, (byte)g, (byte)b);
									Builder.Materials[j].EmissiveColorUsed = true;
									Builder.Materials[j].BlendMode = Builder.Materials[0].BlendMode;
									Builder.Materials[j].GlowAttenuationData = Builder.Materials[0].GlowAttenuationData;
									Builder.Materials[j].DaytimeTexture = Builder.Materials[0].DaytimeTexture;
									Builder.Materials[j].NighttimeTexture = Builder.Materials[0].NighttimeTexture;
									Builder.Materials[j].TransparentColor = Builder.Materials[0].TransparentColor;
									Builder.Materials[j].TransparentColorUsed = Builder.Materials[0].TransparentColorUsed;
								}
								for (int j = 0; j < Builder.Faces.Length; j++) {
									Builder.Faces[j].Material += (ushort)m;
								}
							} break;
						case "setdecaltransparentcolor":
						case "transparent":
							{
								if (cmd == "setdecaltransparentcolor" & IsB3D) {
									Interface.AddMessage(Interface.MessageType.Warning, false, "SetDecalTransparentColor is not a supported command - did you mean Transparent? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else if (cmd == "transparent" & !IsB3D) {
									Interface.AddMessage(Interface.MessageType.Warning, false, "Transparent is not a supported command - did you mean SetDecalTransparentColor? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length > 3) {
									Interface.AddMessage(Interface.MessageType.Warning, false, "At most 3 arguments are expected in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								int r = 0, g = 0, b = 0;
								if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out r)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument Red in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									r = 0;
								} else if (r < 0 | r > 255) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Red is required to be within the range from 0 to 255 in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									r = r < 0 ? 0 : 255;
								}
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[1], out g)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument Green in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									g = 0;
								} else if (g < 0 | g > 255) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Green is required to be within the range from 0 to 255 in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									g = g < 0 ? 0 : 255;
								}
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[2], out b)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument Blue in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									b = 0;
								} else if (b < 0 | b > 255) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Blue is required to be within the range from 0 to 255 in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									b = b < 0 ? 0 : 255;
								}
								for (int j = 0; j < Builder.Materials.Length; j++) {
									Builder.Materials[j].TransparentColor = new World.ColorRGB((byte)r, (byte)g, (byte)b);
									Builder.Materials[j].TransparentColorUsed = true;
								}
							} break;
						case "setblendingmode":
						case "setblendmode":
						case "blendmode":
							{
								if ((cmd == "setblendmode" || cmd == "setblendingmode") & IsB3D)
								{
									Interface.AddMessage(Interface.MessageType.Warning, false, "SetBlendMode is not a supported command - did you mean BlendMode? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else if (cmd == "blendmode" & !IsB3D) {
									Interface.AddMessage(Interface.MessageType.Warning, false, "BlendMode is not a supported command - did you mean SetBlendMode? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length > 3) {
									Interface.AddMessage(Interface.MessageType.Warning, false, "At most 3 arguments are expected in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								World.MeshMaterialBlendMode blendmode = World.MeshMaterialBlendMode.Normal;
								if (Arguments.Length >= 1 && Arguments[0].Length > 0) {
									switch (Arguments[0].ToLowerInvariant()) {
										case "normal":
											blendmode = World.MeshMaterialBlendMode.Normal;
											break;
										case "additive":
										case "glow":
											blendmode = World.MeshMaterialBlendMode.Additive;
											break;
										default:
											Interface.AddMessage(Interface.MessageType.Error, false, "The given BlendMode is not supported in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											blendmode = World.MeshMaterialBlendMode.Normal;
											break;
									}
								}
								double glowhalfdistance = 0.0;
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseDoubleVb6(Arguments[1], out glowhalfdistance)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument GlowHalfDistance in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									glowhalfdistance = 0;
								}
								World.GlowAttenuationMode glowmode = World.GlowAttenuationMode.DivisionExponent4;
								if (Arguments.Length >= 3 && Arguments[2].Length > 0) {
									switch (Arguments[2].ToLowerInvariant()) {
											case "divideexponent2": glowmode = World.GlowAttenuationMode.DivisionExponent2; break;
											case "divideexponent4": glowmode = World.GlowAttenuationMode.DivisionExponent4; break;
										default:
											Interface.AddMessage(Interface.MessageType.Error, false, "The given GlowAttenuationMode is not supported in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											break;
									}
								}
								for (int j = 0; j < Builder.Materials.Length; j++) {
									Builder.Materials[j].BlendMode = blendmode;
									Builder.Materials[j].GlowAttenuationData = World.GetGlowAttenuationData(glowhalfdistance, glowmode);
								}
							} break;
						case "loadtexture":
						case "load":
							{
								if (cmd == "loadtexture" & IsB3D) {
									Interface.AddMessage(Interface.MessageType.Warning, false, "LoadTexture is not a supported command - did you mean Load? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else if (cmd == "load" & !IsB3D) {
									Interface.AddMessage(Interface.MessageType.Warning, false, "Load is not a supported command - did you mean LoadTexture? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length > 2) {
									Interface.AddMessage(Interface.MessageType.Warning, false, "At most 2 arguments are expected in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								string tday = null, tnight = null;
								if (Arguments.Length >= 1 && Arguments[0].Length != 0) {
									if (Path.ContainsInvalidChars(Arguments[0])) {
										Interface.AddMessage(Interface.MessageType.Error, false, "DaytimeTexture contains illegal characters in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									} else {
										tday = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), Arguments[0]);
										if (!System.IO.File.Exists(tday)) {
											Interface.AddMessage(Interface.MessageType.Error, true, "DaytimeTexture " + tday + " could not be found in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
											tday = null;
										}
									}
								}
								if (Arguments.Length >= 2 && Arguments[1].Length != 0) {
									if (Arguments[0].Length == 0) {
										Interface.AddMessage(Interface.MessageType.Error, true, "DaytimeTexture is required to be specified in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									} else {
										if (Path.ContainsInvalidChars(Arguments[1])) {
											Interface.AddMessage(Interface.MessageType.Error, false, "NighttimeTexture contains illegal characters in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
										} else {
											tnight = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), Arguments[1]);
											if (!System.IO.File.Exists(tnight)) {
												Interface.AddMessage(Interface.MessageType.Error, true, "The NighttimeTexture " + tnight + " could not be found in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
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
								Interface.AddMessage(Interface.MessageType.Information, false, "" + cmd + " is only supported in OpenBVE versions 1.4.4.0 and above at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								if (cmd == "settext" & IsB3D)
								{
									Interface.AddMessage(Interface.MessageType.Warning, false,
									  "SetText is not a supported command - did you mean Text? - at line " + (i + 1).ToString(Culture) + " in file " +
									  FileName);
								}
								else if (cmd == "text" & !IsB3D)
								{
									Interface.AddMessage(Interface.MessageType.Warning, false,
									  "Text is not a supported command - did you mean SetText? - at line " + (i + 1).ToString(Culture) + " in file " +
									  FileName);
								}

								if (Arguments.Length > 2)
								{
									Interface.AddMessage(Interface.MessageType.Warning, false,
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
									Interface.AddMessage(Interface.MessageType.Warning, false,
									  "SetTextColor is not a supported command - did you mean TextColor? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								else if (cmd == "textcolor" & !IsB3D)
								{
									Interface.AddMessage(Interface.MessageType.Warning, false,
									  "TextColor is not a supported command - did you mean SetTextColor? - at line " + (i + 1).ToString(Culture) + " in file " +
									  FileName);
								}

								if (Arguments.Length != 3)
								{
									Interface.AddMessage(Interface.MessageType.Warning, false,
									  "3 arguments are expected in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								int r = 0, g = 0, b = 0;
								int.TryParse(Arguments[0], out r);
								int.TryParse(Arguments[1], out g);
								int.TryParse(Arguments[2], out b);
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
									Interface.AddMessage(Interface.MessageType.Warning, false,
									  "SetBackgroundColor is not a supported command - did you mean TextColor? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								else if (cmd == "backgroundcolor" & !IsB3D)
								{
									Interface.AddMessage(Interface.MessageType.Warning, false,
									  "BackgroundColor is not a supported command - did you mean SetBackgroundColor? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}

								if (Arguments.Length != 3)
								{
									Interface.AddMessage(Interface.MessageType.Warning, false,
									  "3 arguments are expected in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								int r = 0, g = 0, b = 0;
								int.TryParse(Arguments[0], out r);
								int.TryParse(Arguments[1], out g);
								int.TryParse(Arguments[2], out b);
								Color textColor = Color.FromArgb(r, g, b);
								for (int j = 0; j < Builder.Materials.Length; j++)
								{
									Builder.Materials[j].BackgroundColor = textColor;
								}
							}
							break;
						case "settextpadding":
						case "textpadding":
							{
								if (cmd == "settextpadding" & IsB3D)
								{
									Interface.AddMessage(Interface.MessageType.Warning, false,
									  "SetTextPadding is not a supported command - did you mean TextPadding? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								else if (cmd == "backgroundcolor" & !IsB3D)
								{
									Interface.AddMessage(Interface.MessageType.Warning, false,
									  "TextPadding is not a supported command - did you mean SetTextPadding? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}

								if (Arguments.Length > 2)
								{
									Interface.AddMessage(Interface.MessageType.Warning, false,
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
									Interface.AddMessage(Interface.MessageType.Warning, false,
									  "SetFont is not a supported command - did you mean Font? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								else if (cmd == "backgroundcolor" & !IsB3D)
								{
									Interface.AddMessage(Interface.MessageType.Warning, false,
									  "Font is not a supported command - did you mean SetFont? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}

								if (Arguments.Length > 1)
								{
									Interface.AddMessage(Interface.MessageType.Warning, false,
									  "1 argument is expected in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (!FontAvailable(Arguments[0]))
								{
									Interface.AddMessage(Interface.MessageType.Warning, false,
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
									Interface.AddMessage(Interface.MessageType.Warning, false, "SetTextureCoordinates is not a supported command - did you mean Coordinates? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								} else if (cmd == "coordinates" & !IsB3D) {
									Interface.AddMessage(Interface.MessageType.Warning, false, "Coordinates is not a supported command - did you mean SetTextureCoordinates? - at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								if (Arguments.Length > 3) {
									Interface.AddMessage(Interface.MessageType.Warning, false, "At most 3 arguments are expected in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
								int j = 0; float x = 0.0f, y = 0.0f;
								if (Arguments.Length >= 1 && Arguments[0].Length > 0 && !NumberFormats.TryParseIntVb6(Arguments[0], out j)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument VertexIndex in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									j = 0;
								}
								if (Arguments.Length >= 2 && Arguments[1].Length > 0 && !NumberFormats.TryParseFloatVb6(Arguments[1], out x)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument X in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									x = 0.0f;
								}
								if (Arguments.Length >= 3 && Arguments[2].Length > 0 && !NumberFormats.TryParseFloatVb6(Arguments[2], out y)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid argument Y in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
									y = 0.0f;
								}
								if (j >= 0 & j < Builder.Vertices.Length) {
									Builder.Vertices[j].TextureCoordinates = new World.Vector2Df(x, y);
								} else {
									Interface.AddMessage(Interface.MessageType.Error, false, "VertexIndex references a non-existing vertex in " + Command + " at line " + (i + 1).ToString(Culture) + " in file " + FileName);
								}
							} break;
						default:
							if (Command.Length != 0) {
								Interface.AddMessage(Interface.MessageType.Error, false, "The command " + Command + " is not supported at line " + (i + 1).ToString(Culture) + " in file " + FileName);
							}
							break;
					}
				}
			}
			// finalize object
			ApplyMeshBuilder(ref Object, Builder, LoadMode, ForceTextureRepeatX, ForceTextureRepeatY);
			World.CreateNormals(ref Object.Mesh);
			return Object;
		}

		// create cube
		private static void CreateCube(ref MeshBuilder Builder, double sx, double sy, double sz) {
			int v = Builder.Vertices.Length;
			Array.Resize<World.Vertex>(ref Builder.Vertices, v + 8);
			Builder.Vertices[v + 0].Coordinates = new World.Vector3D(sx, sy, -sz);
			Builder.Vertices[v + 1].Coordinates = new World.Vector3D(sx, -sy, -sz);
			Builder.Vertices[v + 2].Coordinates = new World.Vector3D(-sx, -sy, -sz);
			Builder.Vertices[v + 3].Coordinates = new World.Vector3D(-sx, sy, -sz);
			Builder.Vertices[v + 4].Coordinates = new World.Vector3D(sx, sy, sz);
			Builder.Vertices[v + 5].Coordinates = new World.Vector3D(sx, -sy, sz);
			Builder.Vertices[v + 6].Coordinates = new World.Vector3D(-sx, -sy, sz);
			Builder.Vertices[v + 7].Coordinates = new World.Vector3D(-sx, sy, sz);
			int f = Builder.Faces.Length;
			Array.Resize<World.MeshFace>(ref Builder.Faces, f + 6);
			Builder.Faces[f + 0].Vertices = new World.MeshFaceVertex[] { new World.MeshFaceVertex(v + 0), new World.MeshFaceVertex(v + 1), new World.MeshFaceVertex(v + 2), new World.MeshFaceVertex(v + 3) };
			Builder.Faces[f + 1].Vertices = new World.MeshFaceVertex[] { new World.MeshFaceVertex(v + 0), new World.MeshFaceVertex(v + 4), new World.MeshFaceVertex(v + 5), new World.MeshFaceVertex(v + 1) };
			Builder.Faces[f + 2].Vertices = new World.MeshFaceVertex[] { new World.MeshFaceVertex(v + 0), new World.MeshFaceVertex(v + 3), new World.MeshFaceVertex(v + 7), new World.MeshFaceVertex(v + 4) };
			Builder.Faces[f + 3].Vertices = new World.MeshFaceVertex[] { new World.MeshFaceVertex(v + 6), new World.MeshFaceVertex(v + 5), new World.MeshFaceVertex(v + 4), new World.MeshFaceVertex(v + 7) };
			Builder.Faces[f + 4].Vertices = new World.MeshFaceVertex[] { new World.MeshFaceVertex(v + 6), new World.MeshFaceVertex(v + 7), new World.MeshFaceVertex(v + 3), new World.MeshFaceVertex(v + 2) };
			Builder.Faces[f + 5].Vertices = new World.MeshFaceVertex[] { new World.MeshFaceVertex(v + 6), new World.MeshFaceVertex(v + 2), new World.MeshFaceVertex(v + 1), new World.MeshFaceVertex(v + 5) };
		}

		// create cylinder
		private static void CreateCylinder(ref MeshBuilder Builder, int n, double r1, double r2, double h) {
			// parameters
			bool uppercap = r1 > 0.0;
			bool lowercap = r2 > 0.0;
			int m = (uppercap ? 1 : 0) + (lowercap ? 1 : 0);
			r1 = Math.Abs(r1);
			r2 = Math.Abs(r2);
			double ns = h >= 0.0 ? 1.0 : -1.0;
			// initialization
			int v = Builder.Vertices.Length;
			Array.Resize<World.Vertex>(ref Builder.Vertices, v + 2 * n);
			World.Vector3Df[] Normals = new World.Vector3Df[2 * n];
			double d = 2.0 * Math.PI / (double)n;
			double g = 0.5 * h;
			double t = 0.0;
			double a = h != 0.0 ? Math.Atan((r2 - r1) / h) : 0.0;
			double cosa = Math.Cos(a);
			double sina = Math.Sin(a);
			// vertices and normals
			for (int i = 0; i < n; i++) {
				double dx = Math.Cos(t);
				double dz = Math.Sin(t);
				double lx = dx * r2;
				double lz = dz * r2;
				double ux = dx * r1;
				double uz = dz * r1;
				Builder.Vertices[v + 2 * i + 0].Coordinates = new World.Vector3D(ux, g, uz);
				Builder.Vertices[v + 2 * i + 1].Coordinates = new World.Vector3D(lx, -g, lz);
				double nx = dx * ns, ny = 0.0, nz = dz * ns;
				double sx, sy, sz;
				World.Cross(nx, ny, nz, 0.0, 1.0, 0.0, out sx, out sy, out sz);
				World.Rotate(ref nx, ref ny, ref nz, sx, sy, sz, cosa, sina);
				Normals[2 * i + 0] = new World.Vector3Df((float)nx, (float)ny, (float)nz);
				Normals[2 * i + 1] = new World.Vector3Df((float)nx, (float)ny, (float)nz);
				t += d;
			}
			// faces
			int f = Builder.Faces.Length;
			Array.Resize<World.MeshFace>(ref Builder.Faces, f + n + m);
			for (int i = 0; i < n; i++) {
				Builder.Faces[f + i].Flags = 0;
				int i0 = (2 * i + 2) % (2 * n);
				int i1 = (2 * i + 3) % (2 * n);
				int i2 = 2 * i + 1;
				int i3 = 2 * i;
				Builder.Faces[f + i].Vertices = new World.MeshFaceVertex[] { new World.MeshFaceVertex(v + i0, Normals[i0]), new World.MeshFaceVertex(v + i1, Normals[i1]), new World.MeshFaceVertex(v + i2, Normals[i2]), new World.MeshFaceVertex(v + i3, Normals[i3]) };
			}
			for (int i = 0; i < m; i++) {
				Builder.Faces[f + n + i].Vertices = new World.MeshFaceVertex[n];
				for (int j = 0; j < n; j++) {
					if (i == 0 & lowercap) {
						// lower cap
						Builder.Faces[f + n + i].Vertices[j] = new World.MeshFaceVertex(v + 2 * j + 1);
					} else {
						// upper cap
						Builder.Faces[f + n + i].Vertices[j] = new World.MeshFaceVertex(v + 2 * (n - j - 1));
					}
				}
			}
		}

		// apply translation
		private static void ApplyTranslation(MeshBuilder Builder, double x, double y, double z) {
			for (int i = 0; i < Builder.Vertices.Length; i++) {
				Builder.Vertices[i].Coordinates.X += x;
				Builder.Vertices[i].Coordinates.Y += y;
				Builder.Vertices[i].Coordinates.Z += z;
			}
		}
		private static void ApplyTranslation(ObjectManager.StaticObject Object, double x, double y, double z) {
			for (int i = 0; i < Object.Mesh.Vertices.Length; i++) {
				Object.Mesh.Vertices[i].Coordinates.X += x;
				Object.Mesh.Vertices[i].Coordinates.Y += y;
				Object.Mesh.Vertices[i].Coordinates.Z += z;
			}
		}

		// apply scale
		private static void ApplyScale(MeshBuilder Builder, double x, double y, double z) {
			float rx = (float)(1.0 / x);
			float ry = (float)(1.0 / y);
			float rz = (float)(1.0 / z);
			float rx2 = rx * rx;
			float ry2 = ry * ry;
			float rz2 = rz * rz;
			for (int i = 0; i < Builder.Vertices.Length; i++) {
				Builder.Vertices[i].Coordinates.X *= x;
				Builder.Vertices[i].Coordinates.Y *= y;
				Builder.Vertices[i].Coordinates.Z *= z;
			}
			for (int i = 0; i < Builder.Faces.Length; i++) {
				for (int j = 0; j < Builder.Faces[i].Vertices.Length; j++) {
					float nx2 = Builder.Faces[i].Vertices[j].Normal.X * Builder.Faces[i].Vertices[j].Normal.X;
					float ny2 = Builder.Faces[i].Vertices[j].Normal.Y * Builder.Faces[i].Vertices[j].Normal.Y;
					float nz2 = Builder.Faces[i].Vertices[j].Normal.Z * Builder.Faces[i].Vertices[j].Normal.Z;
					float u = nx2 * rx2 + ny2 * ry2 + nz2 * rz2;
					if (u != 0.0) {
						u = (float)Math.Sqrt((double)((nx2 + ny2 + nz2) / u));
						Builder.Faces[i].Vertices[j].Normal.X *= rx * u;
						Builder.Faces[i].Vertices[j].Normal.Y *= ry * u;
						Builder.Faces[i].Vertices[j].Normal.Z *= rz * u;
					}
				}
			}
			if (x * y * z < 0.0) {
				for (int i = 0; i < Builder.Faces.Length; i++) {
					Builder.Faces[i].Flip();
				}
			}
		}
		internal static void ApplyScale(ObjectManager.StaticObject Object, double x, double y, double z) {
			float rx = (float)(1.0 / x);
			float ry = (float)(1.0 / y);
			float rz = (float)(1.0 / z);
			float rx2 = rx * rx;
			float ry2 = ry * ry;
			float rz2 = rz * rz;
			bool reverse = x * y * z < 0.0;
			for (int j = 0; j < Object.Mesh.Vertices.Length; j++) {
				Object.Mesh.Vertices[j].Coordinates.X *= x;
				Object.Mesh.Vertices[j].Coordinates.Y *= y;
				Object.Mesh.Vertices[j].Coordinates.Z *= z;
			}
			for (int j = 0; j < Object.Mesh.Faces.Length; j++) {
				for (int k = 0; k < Object.Mesh.Faces[j].Vertices.Length; k++) {
					float nx2 = Object.Mesh.Faces[j].Vertices[k].Normal.X * Object.Mesh.Faces[j].Vertices[k].Normal.X;
					float ny2 = Object.Mesh.Faces[j].Vertices[k].Normal.Y * Object.Mesh.Faces[j].Vertices[k].Normal.Y;
					float nz2 = Object.Mesh.Faces[j].Vertices[k].Normal.Z * Object.Mesh.Faces[j].Vertices[k].Normal.Z;
					float u = nx2 * rx2 + ny2 * ry2 + nz2 * rz2;
					if (u != 0.0) {
						u = (float)Math.Sqrt((double)((nx2 + ny2 + nz2) / u));
						Object.Mesh.Faces[j].Vertices[k].Normal.X *= rx * u;
						Object.Mesh.Faces[j].Vertices[k].Normal.Y *= ry * u;
						Object.Mesh.Faces[j].Vertices[k].Normal.Z *= rz * u;
					}
				}
			}
			if (reverse) {
				for (int j = 0; j < Object.Mesh.Faces.Length; j++) {
					Object.Mesh.Faces[j].Flip();
				}
			}
		}

		// apply rotation
		private static void ApplyRotation(MeshBuilder Builder, double x, double y, double z, double a) {
			double cosa = Math.Cos(a);
			double sina = Math.Sin(a);
			for (int i = 0; i < Builder.Vertices.Length; i++) {
				World.Rotate(ref Builder.Vertices[i].Coordinates.X, ref Builder.Vertices[i].Coordinates.Y, ref Builder.Vertices[i].Coordinates.Z, x, y, z, cosa, sina);
			}
			for (int i = 0; i < Builder.Faces.Length; i++) {
				for (int j = 0; j < Builder.Faces[i].Vertices.Length; j++) {
					World.Rotate(ref Builder.Faces[i].Vertices[j].Normal.X, ref Builder.Faces[i].Vertices[j].Normal.Y, ref Builder.Faces[i].Vertices[j].Normal.Z, x, y, z, cosa, sina);
				}
			}
		}
		private static void ApplyRotation(ObjectManager.StaticObject Object, double x, double y, double z, double a) {
			double cosa = Math.Cos(a);
			double sina = Math.Sin(a);
			for (int j = 0; j < Object.Mesh.Vertices.Length; j++) {
				World.Rotate(ref Object.Mesh.Vertices[j].Coordinates.X, ref Object.Mesh.Vertices[j].Coordinates.Y, ref Object.Mesh.Vertices[j].Coordinates.Z, x, y, z, cosa, sina);
			}
			for (int j = 0; j < Object.Mesh.Faces.Length; j++) {
				for (int k = 0; k < Object.Mesh.Faces[j].Vertices.Length; k++) {
					World.Rotate(ref Object.Mesh.Faces[j].Vertices[k].Normal.X, ref Object.Mesh.Faces[j].Vertices[k].Normal.Y, ref Object.Mesh.Faces[j].Vertices[k].Normal.Z, x, y, z, cosa, sina);
				}
			}
		}

		// apply shear
		private static void ApplyShear(MeshBuilder Builder, double dx, double dy, double dz, double sx, double sy, double sz, double r) {
			for (int j = 0; j < Builder.Vertices.Length; j++) {
				double n = r * (dx * Builder.Vertices[j].Coordinates.X + dy * Builder.Vertices[j].Coordinates.Y + dz * Builder.Vertices[j].Coordinates.Z);
				Builder.Vertices[j].Coordinates.X += sx * n;
				Builder.Vertices[j].Coordinates.Y += sy * n;
				Builder.Vertices[j].Coordinates.Z += sz * n;
			}
			for (int j = 0; j < Builder.Faces.Length; j++) {
				for (int k = 0; k < Builder.Faces[j].Vertices.Length; k++) {
					if (Builder.Faces[j].Vertices[k].Normal.X != 0.0f | Builder.Faces[j].Vertices[k].Normal.Y != 0.0f | Builder.Faces[j].Vertices[k].Normal.Z != 0.0f) {
						double nx = (double)Builder.Faces[j].Vertices[k].Normal.X;
						double ny = (double)Builder.Faces[j].Vertices[k].Normal.Y;
						double nz = (double)Builder.Faces[j].Vertices[k].Normal.Z;
						double n = r * (sx * nx + sy * ny + sz * nz);
						nx -= dx * n;
						ny -= dy * n;
						nz -= dz * n;
						World.Normalize(ref nx, ref ny, ref nz);
						Builder.Faces[j].Vertices[k].Normal.X = (float)nx;
						Builder.Faces[j].Vertices[k].Normal.Y = (float)ny;
						Builder.Faces[j].Vertices[k].Normal.Z = (float)nz;
					}
				}
			}
		}
		private static void ApplyShear(ObjectManager.StaticObject Object, double dx, double dy, double dz, double sx, double sy, double sz, double r) {
			for (int j = 0; j < Object.Mesh.Vertices.Length; j++) {
				double n = r * (dx * Object.Mesh.Vertices[j].Coordinates.X + dy * Object.Mesh.Vertices[j].Coordinates.Y + dz * Object.Mesh.Vertices[j].Coordinates.Z);
				Object.Mesh.Vertices[j].Coordinates.X += sx * n;
				Object.Mesh.Vertices[j].Coordinates.Y += sy * n;
				Object.Mesh.Vertices[j].Coordinates.Z += sz * n;
			}
			double ux, uy, uz;
			World.Cross(sx, sy, sz, dx, dy, dz, out ux, out uy, out uz);
			for (int j = 0; j < Object.Mesh.Faces.Length; j++) {
				for (int k = 0; k < Object.Mesh.Faces[j].Vertices.Length; k++) {
					if (Object.Mesh.Faces[j].Vertices[k].Normal.X != 0.0f | Object.Mesh.Faces[j].Vertices[k].Normal.Y != 0.0f | Object.Mesh.Faces[j].Vertices[k].Normal.Z != 0.0f) {
						double nx = (double)Object.Mesh.Faces[j].Vertices[k].Normal.X;
						double ny = (double)Object.Mesh.Faces[j].Vertices[k].Normal.Y;
						double nz = (double)Object.Mesh.Faces[j].Vertices[k].Normal.Z;
						double n = r * (sx * nx + sy * ny + sz * nz);
						nx -= dx * n;
						ny -= dy * n;
						nz -= dz * n;
						World.Normalize(ref nx, ref ny, ref nz);
						Object.Mesh.Faces[j].Vertices[k].Normal.X = (float)nx;
						Object.Mesh.Faces[j].Vertices[k].Normal.Y = (float)ny;
						Object.Mesh.Faces[j].Vertices[k].Normal.Z = (float)nz;
					}
				}
			}
		}
		
		// apply mesh builder
		private static void ApplyMeshBuilder(ref ObjectManager.StaticObject Object, MeshBuilder Builder, ObjectManager.ObjectLoadMode LoadMode, bool ForceTextureRepeatX, bool ForceTextureRepeatY) {
			if (Builder.Faces.Length != 0) {
				int mf = Object.Mesh.Faces.Length;
				int mm = Object.Mesh.Materials.Length;
				int mv = Object.Mesh.Vertices.Length;
				Array.Resize<World.MeshFace>(ref Object.Mesh.Faces, mf + Builder.Faces.Length);
				Array.Resize<World.MeshMaterial>(ref Object.Mesh.Materials, mm + Builder.Materials.Length);
				Array.Resize<World.Vertex>(ref Object.Mesh.Vertices, mv + Builder.Vertices.Length);
				for (int i = 0; i < Builder.Vertices.Length; i++) {
					Object.Mesh.Vertices[mv + i] = Builder.Vertices[i];
				}
				for (int i = 0; i < Builder.Faces.Length; i++) {
					Object.Mesh.Faces[mf + i] = Builder.Faces[i];
					for (int j = 0; j < Object.Mesh.Faces[mf + i].Vertices.Length; j++) {
						Object.Mesh.Faces[mf + i].Vertices[j].Index += (ushort)mv;
					}
					Object.Mesh.Faces[mf + i].Material += (ushort)mm;
				}
				for (int i = 0; i < Builder.Materials.Length; i++) {
					Object.Mesh.Materials[mm + i].Flags = (byte)((Builder.Materials[i].EmissiveColorUsed ? World.MeshMaterial.EmissiveColorMask : 0) | (Builder.Materials[i].TransparentColorUsed ? World.MeshMaterial.TransparentColorMask : 0));
					Object.Mesh.Materials[mm + i].Color = Builder.Materials[i].Color;
					Object.Mesh.Materials[mm + i].TransparentColor = Builder.Materials[i].TransparentColor;
					TextureManager.TextureWrapMode WrapX, WrapY;
					if (ForceTextureRepeatX) {
						WrapX = TextureManager.TextureWrapMode.Repeat;
					} else {
						WrapX = TextureManager.TextureWrapMode.ClampToEdge;
					}
					if (ForceTextureRepeatY) {
						WrapY = TextureManager.TextureWrapMode.Repeat;
					} else {
						WrapY = TextureManager.TextureWrapMode.ClampToEdge;
					}
					if (WrapX != TextureManager.TextureWrapMode.Repeat | WrapY != TextureManager.TextureWrapMode.Repeat) {
						for (int j = 0; j < Builder.Vertices.Length; j++) {
							if (Builder.Vertices[j].TextureCoordinates.X < 0.0 | Builder.Vertices[j].TextureCoordinates.X > 1.0) {
								WrapX = TextureManager.TextureWrapMode.Repeat;
							}
							if (Builder.Vertices[j].TextureCoordinates.Y < 0.0 | Builder.Vertices[j].TextureCoordinates.Y > 1.0) {
								WrapY = TextureManager.TextureWrapMode.Repeat;
							}
						}
					}
					if (Builder.Materials[i].DaytimeTexture != null || Builder.Materials[i].Text != null)
					{
						int tday;
						if (Builder.Materials[i].Text != null)
						{
							Bitmap bitmap = null;
							if (Builder.Materials[i].DaytimeTexture != null)
							{
								bitmap = new Bitmap(Builder.Materials[i].DaytimeTexture);
							}
							Bitmap texture = TextOverlay.AddTextToBitmap(bitmap, Builder.Materials[i].Text, Builder.Materials[i].Font, 12, Builder.Materials[i].BackgroundColor, Builder.Materials[i].TextColor, Builder.Materials[i].TextPadding);
							tday = TextureManager.RegisterTexture(texture, Builder.Materials[i].TransparentColor);
						}
						else
						{
							tday = TextureManager.RegisterTexture(Builder.Materials[i].DaytimeTexture, Builder.Materials[i].TransparentColor, Builder.Materials[i].TransparentColorUsed ? (byte)1 : (byte)0, WrapX, WrapY, LoadMode != ObjectManager.ObjectLoadMode.Normal);

						}
						Object.Mesh.Materials[mm + i].DaytimeTextureIndex = tday;
					}
					else
					{
						Object.Mesh.Materials[mm + i].DaytimeTextureIndex = -1;
					}
					Object.Mesh.Materials[mm + i].EmissiveColor = Builder.Materials[i].EmissiveColor;
					if (Builder.Materials[i].NighttimeTexture != null) {
						int tnight = TextureManager.RegisterTexture(Builder.Materials[i].NighttimeTexture, Builder.Materials[i].TransparentColor, Builder.Materials[i].TransparentColorUsed ? (byte)1 : (byte)0, WrapX, WrapY, LoadMode != ObjectManager.ObjectLoadMode.Normal);
						Object.Mesh.Materials[mm + i].NighttimeTextureIndex = tnight;
					} else {
						Object.Mesh.Materials[mm + i].NighttimeTextureIndex = -1;
					}
					Object.Mesh.Materials[mm + i].DaytimeNighttimeBlend = 0;
					Object.Mesh.Materials[mm + i].BlendMode = Builder.Materials[i].BlendMode;
					Object.Mesh.Materials[mm + i].GlowAttenuationData = Builder.Materials[i].GlowAttenuationData;
				}
			}
		}

		private static Bitmap TextToBitmap(string txt, string fontname, int fontsize, Color bgcolor, Color fcolor, Vector2 Padding)
		{
			SizeF size;
			Bitmap bmp = new Bitmap(1024, 1024);
			using (Graphics graphics = Graphics.FromImage(bmp))
			{
				Font font = new Font(fontname, fontsize);
				size = graphics.MeasureString(txt, font);
				graphics.FillRectangle(new SolidBrush(bgcolor), 0, 0, size.Width + (int)Padding.X * 2, size.Height + (int)Padding.Y * 2);
				graphics.DrawString(txt, font, new SolidBrush(fcolor), (int)Padding.X, (int)Padding.Y);
				graphics.Flush();
				font.Dispose();
				graphics.Dispose();
			}
			Rectangle cropArea = new Rectangle(0, 0, (int)size.Width + (int)Padding.X * 2, (int)size.Height + (int)Padding.Y * 2);
			return bmp.Clone(cropArea, bmp.PixelFormat);
		}

		private static bool FontAvailable(string fontName)
		{
			using (var testFont = new Font(fontName, 8))
			{
				return 0 == string.Compare(
				  fontName,
				  testFont.Name,
				  StringComparison.InvariantCultureIgnoreCase);
			}
		}
	}
}