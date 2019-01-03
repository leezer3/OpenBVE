using System;
using System.IO;
using System.IO.Compression;
using OpenBveApi.Colors;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using System.Linq;
using System.Text;
using OpenBveApi.Interface;
using OpenBveApi.Textures;

namespace OpenBve {
	internal static class XObjectParser {

		// read object
		internal static ObjectManager.StaticObject ReadObject(string FileName, System.Text.Encoding Encoding, ObjectLoadMode LoadMode, bool ForceTextureRepeatX, bool ForceTextureRepeatY) {
			byte[] Data = System.IO.File.ReadAllBytes(FileName);
			if (Data.Length < 16 || Data[0] != 120 | Data[1] != 111 | Data[2] != 102 | Data[3] != 32) {
				// not an x object
				Interface.AddMessage(MessageType.Error, false, "Invalid X object file encountered in " + FileName);
				return null;
			}
			if (Data[4] != 48 | Data[5] != 51 | Data[6] != 48 | Data[7] != 50 & Data[7] != 51) {
				// unrecognized version
				System.Text.ASCIIEncoding Ascii = new System.Text.ASCIIEncoding();
				string s = new string(Ascii.GetChars(Data, 4, 4));
				Interface.AddMessage(MessageType.Error, false, "Unsupported X object file version " + s + " encountered in " + FileName);
			}
			// floating-point format
			int FloatingPointSize;
			if (Data[12] == 48 & Data[13] == 48 & Data[14] == 51 & Data[15] == 50) {
				FloatingPointSize = 32;
			} else if (Data[12] == 48 & Data[13] == 48 & Data[14] == 54 & Data[15] == 52) {
				FloatingPointSize = 64;
			} else {
				Interface.AddMessage(MessageType.Error, false, "Unsupported floating point format encountered in X object file " + FileName);
				return null;
			}
			// supported floating point format
			if (Data[8] == 116 & Data[9] == 120 & Data[10] == 116 & Data[11] == 32) {
				// textual flavor
				return LoadTextualX(FileName, System.IO.File.ReadAllText(FileName), Encoding, LoadMode, ForceTextureRepeatX, ForceTextureRepeatY);
			} else if (Data[8] == 98 & Data[9] == 105 & Data[10] == 110 & Data[11] == 32) {
				// binary flavor
				return LoadBinaryX(FileName, Data, 16, FloatingPointSize, LoadMode, ForceTextureRepeatX, ForceTextureRepeatY);
			} else if (Data[8] == 116 & Data[9] == 122 & Data[10] == 105 & Data[11] == 112) {
				// compressed textual flavor
				#if !DEBUG
				try {
					#endif
					byte[] Uncompressed = Decompress(Data);
					string Text = Encoding.GetString(Uncompressed);
					return LoadTextualX(FileName, Text, Encoding, LoadMode, ForceTextureRepeatX, ForceTextureRepeatY);
					#if !DEBUG
				} catch (Exception ex) {
					Interface.AddMessage(MessageType.Error, false, "An unexpected error occured (" + ex.Message + ") while attempting to decompress the binary X object file encountered in " + FileName);
					return null;
				}
				#endif
			} else if (Data[8] == 98 & Data[9] == 122 & Data[10] == 105 & Data[11] == 112) {
				// compressed binary flavor
				#if !DEBUG
				try {
					#endif
					byte[] Uncompressed = Decompress(Data);
					return LoadBinaryX(FileName, Uncompressed, 0, FloatingPointSize, LoadMode, ForceTextureRepeatX, ForceTextureRepeatY);
					#if !DEBUG
				} catch (Exception ex) {
					Interface.AddMessage(MessageType.Error, false, "An unexpected error occured (" + ex.Message + ") while attempting to decompress the binary X object file encountered in " + FileName);
					return null;
				}
				#endif
			} else {
				// unsupported flavor
				Interface.AddMessage(MessageType.Error, false, "Unsupported X object file encountered in " + FileName);
				return null;
			}
		}

		// ================================

		// decompress
		private static byte[] Decompress(byte[] Data) {
			byte[] Target;
			using (MemoryStream InputStream = new MemoryStream(Data)) {
				InputStream.Position = 26;
				using (DeflateStream Deflate = new DeflateStream(InputStream, CompressionMode.Decompress, true)) {
					using (MemoryStream OutputStream = new MemoryStream()) {
						byte[] Buffer = new byte[4096];
						while (true) {
							int Count = Deflate.Read(Buffer, 0, Buffer.Length);
							if (Count != 0) {
								OutputStream.Write(Buffer, 0, Count);
							}
							if (Count != Buffer.Length) {
								break;
							}
						}
						Target = new byte[OutputStream.Length];
						OutputStream.Position = 0;
						OutputStream.Read(Target, 0, Target.Length);
					}
				}
			}
			return Target;
		}

		// ================================

		// template
		private class Template {
			internal string Name;
			internal string[] Members;
			internal string Key;
			internal Template(string Name, string[] Members) {
				this.Name = Name;
				this.Members = Members;
			}
			internal Template(string Name, string[] Members, string Key)
			{
				this.Name = Name;
				this.Members = Members;
				this.Key = Key;
			}
		}
		private static Template[] Templates = new Template[] {
			new Template("Mesh", new string[] { "DWORD", "Vector[0]", "DWORD", "MeshFace[2]", "[...]" }),
			new Template("Vector", new string[] { "float", "float", "float" }),
			new Template("MeshFace", new string[] { "DWORD", "DWORD[0]" }),
			new Template("MeshMaterialList", new string[] { "DWORD", "DWORD", "DWORD[1]", "[...]" }),
			new Template("Material", new string[] { "ColorRGBA", "float", "ColorRGB", "ColorRGB", "[...]" }),
			new Template("ColorRGBA", new string[] { "float", "float", "float", "float" }),
			new Template("ColorRGB", new string[] { "float", "float", "float" }),
			new Template("TextureFilename", new string[] { "string" }),
			new Template("MeshTextureCoords", new string[] { "DWORD", "Coords2d[0]" }),
			new Template("Coords2d", new string[] { "float", "float" }),
			new Template("MeshVertexColors", new string[] { "DWORD","VertexColor[0]"}),
			new Template("MeshNormals", new string[] { "DWORD", "Vector[0]", "DWORD", "MeshFace[2]" }),
			// Index , ColorRGBA
			new Template("VertexColor", new string[] { "DWORD", "ColorRGBA" }),
			//Root frame around the model itself
			new Template("Frame Root", new string[] { "[...]" }),
			//Presumably appears around each Mesh (??), Blender exported models
			new Template("Frame", new string[] { "[...]" }),
			//Transforms the mesh, UNSUPPORTED
			new Template("FrameTransformMatrix", new string[] { "[???]" }),
		};

		// data
		private class Structure {
			internal string Name;
			internal string Key;
			internal object[] Data;
			internal Structure(string Name, object[] Data, string Key)
			{
				this.Name = Name;
				this.Key = Key;
				this.Data = Data;
			}
		}

		private static bool AlternateStructure;
		private static Structure[] LoadedMaterials;

		// get template
		private static Template GetTemplate(string Name, bool binary) {
			for (int i = 0; i < Templates.Length; i++) {
				if (Templates[i].Name == Name) {
					return Templates[i];
				}
			}
			if (Name.ToLowerInvariant().StartsWith("frame "))
			{
				//Enclosing frame for the model
				//Appears in Blender exported stuff
				return Templates[13];
			}
			
			if (Name.ToLowerInvariant().StartsWith("mesh "))
			{
				//Named material, just ignore the name for the minute
				//Appears in Blender exported stuff
				return Templates[0];
			}
			//Not a default template, so now figure out if it's a named texture
			string[] splitName = Name.Split(' ');
			if (splitName[0].ToLowerInvariant() == "material")
			{
				AlternateStructure = true;
				return new Template("Material", new string[] { "ColorRGBA", "float", "ColorRGB", "ColorRGB", "[...]" }, splitName[1]);
			}
			if (splitName[0].ToLowerInvariant() == "mesh")
			{
				return new Template("Mesh", new string[] { "DWORD", "Vector[0]", "DWORD", "MeshFace[2]", "[...]" });
			}
			return new Template(Name, new string[] { "[???]" });
		}

		// ================================

		// load textual x
		private static ObjectManager.StaticObject LoadTextualX(string FileName, string Text, System.Text.Encoding Encoding, ObjectLoadMode LoadMode, bool ForceTextureRepeatX, bool ForceTextureRepeatY) {
			// load
			string[] Lines = Text.Replace("\u000D\u000A", "\u2028").Split(new char[] { '\u000A', '\u000C', '\u000D', '\u0085', '\u2028', '\u2029' }, StringSplitOptions.None);
			AlternateStructure = false;
			LoadedMaterials = new Structure[] {};
			// strip away comments
			bool Quote = false;
			for (int i = 0; i < Lines.Length; i++) {
				for (int j = 0; j < Lines[i].Length; j++) {
					if (Lines[i][j] == '"') Quote = !Quote;
					if (!Quote) {
						if (Lines[i][j] == '#' || j < Lines[i].Length - 1 && Lines[i].Substring(j, 2) == "//") {
							Lines[i] = Lines[i].Substring(0, j);
							break;
						}
					}
				}
				//Convert runs of whitespace to single
				var list = Lines[i].Split(' ').Where(s => !string.IsNullOrWhiteSpace(s));
				Lines[i] = string.Join(" ", list);
			}
			
			//Preprocess the string array to get the variants to something we understand....
			for (int i = 0; i < Lines.Length; i++)
			{
				string[] splitLine = Lines[i].Split(',');
				if (splitLine.Length == 2 && splitLine[1].Trim().Length > 0)
				{
					if (!splitLine[1].EndsWith(";"))
					{
						splitLine[1] = splitLine[1] + ";";
					}
					else
					{
						splitLine[1] = splitLine[1] + ",";
					}
					Lines[i] = splitLine[0] + ';' + splitLine[1];
				}
				else if (((splitLine.Length >= 4 && Lines[i].EndsWith(",")) || (splitLine.Length >= 3 && Lines[i].EndsWith(";;") && !Lines[i - 1].EndsWith(";,")) || (splitLine.Length >= 3 && Lines[i].EndsWith(";") && Lines[i - 1].Length > 5 && Lines[i - 1].EndsWith(";"))) && !splitLine[splitLine.Length - 2].EndsWith(";") && Lines[i - 1].Length > 5 && !Lines[i - 1].EndsWith("{"))
				{
					Lines[i - 1] = Lines[i - 1].Substring(0, Lines[i - 1].Length - 1) + ";,";
				}

				if ((Lines[i].IndexOf('}') != -1 || Lines[i].IndexOf('{') != -1) && Lines[i - 1].EndsWith(";,"))
				{
					Lines[i - 1] = Lines[i - 1].Substring(0, Lines[i - 1].Length - 2) + ";;";
				}
			}
			
			// strip away header
			if (Lines.Length == 0 || Lines[0].Length < 16) {
				Interface.AddMessage(MessageType.Error, false, "The textual X object file is invalid at line 1 in " + FileName);
				return null;
			}
			Lines[0] = Lines[0].Substring(16);
			// join lines
			StringBuilder Builder = new StringBuilder();
			for (int i = 0; i < Lines.Length; i++) {
				Builder.Append(Lines[i]);
			}
			string Content = Builder.ToString();
			//Horrible hack to make Blender generated materials work
			int idx = Content.IndexOf("Material ", StringComparison.InvariantCultureIgnoreCase);
			while(idx != -1)
			{
				int idx2 = idx + 9;
				if (Content[idx2] != '{')
				{
					int idx3 = idx2;
					while (idx3 < Content.Length)
					{
						idx3++;
						if (Content[idx3] == '{')
						{
							break;
						}

						
					}
					StringBuilder sb = new StringBuilder(Content);
					sb.Remove(idx2, idx3 - idx2);
					Content = sb.ToString();
				}

				idx = Content.IndexOf("Material ", idx + 9, StringComparison.InvariantCultureIgnoreCase);
			}
			// parse file
			int Position = 0;
			Structure Structure;
			if (!ReadTextualTemplate(FileName, Content, ref Position, new Template("", new string[] { "[...]" }), false, out Structure)) {
				return null;
			}
			// process structure
			ObjectManager.StaticObject Object;
			if (!ProcessStructure(FileName, Structure, out Object, LoadMode, ForceTextureRepeatX, ForceTextureRepeatY)) {
				return null;
			}
			return Object;
		}

		private static bool IsDefaultTemplate(string s)
		{
			//X files generated by the original BVE4 convertor don't have the default templates defined, but are hardcoded instead
			//On the other hand, X files which conform to the specification specify all used templates at the start of the file
			//This is a simple textual check, and does not handle unexpected variations.....
			switch (s.ToLowerInvariant())
			{
				case "template meshface":
				case "template vector":
				case "template mesh":
				case "template colorrgba":
				case "template colorrgb":
				case "template material":
				case "template meshmateriallist":
				case "template coords2d":
				case "template meshtexturecoords":
				case "template meshnormals":
				case "template texturefilename":
				case "template meshvertexcolors":
				case "frametransformmatrix":
					return true;
			}
			
			return false;
		}

		private static bool IsTemplate(string s)
		{
			//X files generated by the original BVE4 convertor don't have the default templates defined, but are hardcoded instead
			//On the other hand, X files which conform to the specification specify all used templates at the start of the file
			//This is a simple textual check, and does not handle unexpected variations.....
			switch (s.ToLowerInvariant())
			{
				case "meshface":
				case "vector":
				case "mesh":
				case "colorrgba":
				case "colorrgb":
				case "material":
				case "meshmateriallist":
				case "coords2d":
				case "meshtexturecoords":
				case "meshnormals":
				case "meshvertexcolors":
				case "texturefilename":
				case "frametransformmatrix":
					return true;
			}

			return false;
		}

		// read textual template
		private static bool ReadTextualTemplate(string FileName, string Content, ref int Position, Template Template, bool Inline, out Structure Structure) {
			if (Template.Name == "MeshMaterialList" && AlternateStructure)
			{
				Template = new Template("MeshMaterialList", new string[] { "DWORD", "DWORD", "DWORD[1]", "string2", "[...]" });
			}
			Structure = new Structure(Template.Name, new object[] { }, Template.Key);
			int i = Position; bool q = false;
			int m; for (m = 0; m < Template.Members.Length; m++) {
				if (Position >= Content.Length) break;
				if (Template.Members[m] == "[???]") {
					// unknown data accepted
					while (Position < Content.Length) {
						if (q) {
							if (Content[Position] == '"') q = false;
						} else {
							if (Content[Position] == '"') {
								q = true;
							} else if (Content[Position] == ',' | Content[Position] == ';') {
								i = Position + 1;
							} else if (Content[Position] == '{') {
								string s = Content.Substring(i, Position - i).Trim();
								Structure o;
								Position++;
								if (!ReadTextualTemplate(FileName, Content, ref Position, GetTemplate(s, false), false, out o)) {
									return false;
								} Position--;
								i = Position + 1;
							} else if (Content[Position] == '}') {
								Position++;
								return true;
							}
						} Position++;
					} m--;
				} else if (Template.Members[m] == "[...]") {
					// any template accepted
					while (Position < Content.Length) {
						if (q) {
							if (Content[Position] == '"') q = false;
						} else {
							if (Content[Position] == '"') {
								q = true;
							} else if (Content[Position] == '{') {
								string s = Content.Substring(i, Position - i).Trim();
								Structure o;
								Position++;
								if (!ReadTextualTemplate(FileName, Content, ref Position, GetTemplate(s, false), false, out o)) {
									return false;
								} Position--;
								if (!IsDefaultTemplate(s))
								{
									Array.Resize<object>(ref Structure.Data, Structure.Data.Length + 1);
									Structure.Data[Structure.Data.Length - 1] = o;
								}
								i = Position + 1;
							} else if (Content[Position] == '}') {
								if (Inline) {
									Interface.AddMessage(MessageType.Error, false, "Unexpected closing brace encountered in inlined template " + Template.Name + " in textual X object file " + FileName);
									return false;
								} else {
									Position++;
									return true;
								}
							} else if (Content[Position] == ',') {
								Interface.AddMessage(MessageType.Error, false, "Unexpected comma encountered in template " + Template.Name + " in textual X object file " + FileName);
								return false;
							} else if (Content[Position] == ';') {
								if (Inline) {
									Position++;
									return true;
								} else {
									if (Template.Name == "MeshMaterialList")
									{
										//A MeshMaterialList can also end with two semi-colons
										Position++;
										i++;
										continue;
									}
									Interface.AddMessage(MessageType.Error, false, "Unexpected semicolon encountered in template " + Template.Name + " in textual X object file " + FileName);
									return false;
								}
							}
						} Position++;
					} m--;
				} else if (Template.Members[m].EndsWith("]", StringComparison.Ordinal)) {
					// inlined array expected
					string r = Template.Members[m].Substring(0, Template.Members[m].Length - 1);
					int h = r.IndexOf('[');
					if (h >= 0) {
						string z = r.Substring(h + 1, r.Length - h - 1);
						r = r.Substring(0, h);
						if (!int.TryParse(z, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out h)) {
							Interface.AddMessage(MessageType.Error, false, "The internal format description for a template array is invalid in template " + Template.Name + " in textual X object file " + FileName);
							return false;
						}
						if (h < 0 || h >= Structure.Data.Length || !(Structure.Data[h] is int)) {
							Interface.AddMessage(MessageType.Error, false, "The internal format description for a template array is invalid in template " + Template.Name + " in textual X object file " + FileName);
							return false;
						}
						h = (int)Structure.Data[h];
					} else {
						Interface.AddMessage(MessageType.Error, false, "The internal format description for a template array is invalid in template " + Template.Name + " in textual X object file " + FileName);
						return false;
					}
					if (r == "DWORD") {
						// dword array
						int[] o = new int[h];
						if (h == 0) {
							// empty array
							while (Position < Content.Length) {
								if (Content[Position] == ';') {
									Position++;
									break;
								} else if (!char.IsWhiteSpace(Content, Position)) {
									Interface.AddMessage(MessageType.Error, false, "Invalid character encountered while processing an array in template " + Template.Name + " in textual X object file " + FileName);
									return false;
								} else {
									Position++;
								}
							}
						} else {
							// non-empty array
							for (int k = 0; k < h; k++) {
								while (Position < Content.Length) {
									if (Content[Position] == '{' | Content[Position] == '}' | Content[Position] == '"') {
										Interface.AddMessage(MessageType.Error, false, "Invalid character encountered while processing a DWORD array in template " + Template.Name + " in textual X object file " + FileName);
										return false;
									} else if (Content[Position] == ',') {
										if (k == h - 1) {
											Interface.AddMessage(MessageType.Error, false, "Invalid character encountered while processing a DWORD array in template " + Template.Name + " in textual X object file " + FileName);
											return false;
										}
										break;
									} else if (Content[Position] == ';') {
										if (k != h - 1) {
											Interface.AddMessage(MessageType.Error, false, "Invalid character encountered while processing a DWORD array in template " + Template.Name + " in textual X object file " + FileName);
											return false;
										}
										break;
									} Position++;
								} if (Position == Content.Length) {
									Interface.AddMessage(MessageType.Error, false, "DWORD array was not terminated at the end of the file in template " + Template.Name + " in textual X object file " + FileName);
									return false;
								}
								string s = Content.Substring(i, Position - i);
								Position++;
								i = Position;
								if (!int.TryParse(s, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out o[k])) {
									Interface.AddMessage(MessageType.Error, false, "DWORD could not be parsed in array in template " + Template.Name + " in textual X object file " + FileName);
								}
							}
						}
						Array.Resize<object>(ref Structure.Data, Structure.Data.Length + 1);
						Structure.Data[Structure.Data.Length - 1] = o;
					} else if (r == "float") {
						// float array
						double[] o = new double[h];
						if (h == 0) {
							// empty array
							while (Position < Content.Length) {
								if (Content[Position] == ';') {
									Position++;
									break;
								} else if (!char.IsWhiteSpace(Content, Position)) {
									Interface.AddMessage(MessageType.Error, false, "Invalid character encountered while processing an array in template " + Template.Name + " in textual X object file " + FileName);
									return false;
								} else {
									Position++;
								}
							}
						} else {
							// non-empty array
							for (int k = 0; k < h; k++) {
								while (Position < Content.Length) {
									if (Content[Position] == '{' | Content[Position] == '}' | Content[Position] == '"') {
										Interface.AddMessage(MessageType.Error, false, "Invalid character encountered while processing a float array in template " + Template.Name + " in textual X object file " + FileName);
										return false;
									} else if (Content[Position] == ',') {
										if (k == h - 1) {
											Interface.AddMessage(MessageType.Error, false, "Invalid character encountered while processing a float array in template " + Template.Name + " in textual X object file " + FileName);
											return false;
										}
										break;
									} else if (Content[Position] == ';') {
										if (k != h - 1) {
											Interface.AddMessage(MessageType.Error, false, "Invalid character encountered while processing a float array in template " + Template.Name + " in textual X object file " + FileName);
											return false;
										}
										break;
									} Position++;
								} if (Position == Content.Length) {
									Interface.AddMessage(MessageType.Error, false, "float array was not terminated at the end of the file in template " + Template.Name + " in textual X object file " + FileName);
									return false;
								}
								string s = Content.Substring(i, Position - i);
								Position++;
								i = Position;
								if (!double.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out o[k])) {
									Interface.AddMessage(MessageType.Error, false, "float could not be parsed in array in template " + Template.Name + " in textual X object file " + FileName);
								}
							}
						}
						Array.Resize<object>(ref Structure.Data, Structure.Data.Length + 1);
						Structure.Data[Structure.Data.Length - 1] = o;
					} else {
						// non-primitive array
						Template t = GetTemplate(r, false);
						Structure[] o = new Structure[h];
						if (h == 0) {
							// empty array
							while (Position < Content.Length) {
								if (Content[Position] == ';') {
									Position++;
									break;
								} else if (Content[Position] == '}' && Position == Content.Length -1 && Template.Members[m].StartsWith("MeshFace")) {
									// A mesh has been provided, but no faces etc.
									// Usually found in null objects
									break;
								} else if (!char.IsWhiteSpace(Content, Position)) {
									Interface.AddMessage(MessageType.Error, false, "Invalid character encountered while processing an array in template " + Template.Name + " in textual X object file " + FileName);
									return false;
								} else {
									Position++;
								}
							}
						} else {
							int k;
							for (k = 0; k < h; k++) {
								if (!ReadTextualTemplate(FileName, Content, ref Position, t, true, out o[k])) {
									return false;
								}
								if (k < h - 1) {
									// most elements
									while (Position < Content.Length) {
										if (Content[Position] == ',') {
											Position++;
											break;
										} else if (!char.IsWhiteSpace(Content, Position)) {
											Interface.AddMessage(MessageType.Error, false, "Invalid character encountered while processing an array in template " + Template.Name + " in textual X object file " + FileName);
											return false;
										} else {
											Position++;
										}
									} if (Position == Content.Length) {
										Interface.AddMessage(MessageType.Error, false, "Array was not continued at the end of the file in template " + Template.Name + " in textual X object file " + FileName);
										return false;
									}
								} else {
									// last element
									while (Position < Content.Length) {
										if (Content[Position] == ';') {
											Position++;
											break;
										} else if (!char.IsWhiteSpace(Content, Position)) {
											Interface.AddMessage(MessageType.Error, false, "Invalid character encountered while processing an array in template " + Template.Name + " in textual X object file " + FileName);
											return false;
										} else {
											Position++;
										}
									} if (Position == Content.Length) {
										Interface.AddMessage(MessageType.Error, false, "Array was not terminated at the end of the file in template " + Template.Name + " in textual X object file " + FileName);
										return false;
									}
								}
							} if (k < h) {
								return false;
							}
						}
						Array.Resize<object>(ref Structure.Data, Structure.Data.Length + 1);
						Structure.Data[Structure.Data.Length - 1] = o;
					}
					i = Position;
				} else {
					// inlined template or primitive expected
					switch (Template.Members[m]) {
						case "DWORD":
							while (Position < Content.Length) {
								if (Content[Position] == '{' | Content[Position] == '}' | Content[Position] == ',' | Content[Position] == '"') {
									Interface.AddMessage(MessageType.Error, false, "Invalid character encountered while processing a DWORD in template " + Template.Name + " in textual X object file " + FileName);
									return false;
								} else if (Content[Position] == ';') {
									string s = Content.Substring(i, Position - i).Trim();
									int a; if (!int.TryParse(s, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out a)) {
										Interface.AddMessage(MessageType.Error, false, "DWORD could not be parsed in template " + Template.Name + " in textual X object file " + FileName);
										return false;
									}
									Array.Resize<object>(ref Structure.Data, Structure.Data.Length + 1);
									Structure.Data[Structure.Data.Length - 1] = a;
									Position++;
									i = Position;
									break;
								} Position++;
							} break;
						case "float":
							while (Position < Content.Length) {
								if (Content[Position] == '{' | Content[Position] == '}' |  Content[Position] == '"') {
									Interface.AddMessage(MessageType.Error, false, "Invalid character encountered while processing a DWORD in template " + Template.Name + " in textual X object file " + FileName);
									return false;
								} else if (Content[Position] == ';' || Content[Position] == ',') {
									string s = Content.Substring(i, Position - i).Trim();
									double a; if (!double.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out a)) {
										if (s != string.Empty)
										{
											//Handle omitted entries which are still in a valid format
											//May break elsewhere, but if so we're no further back anyways
											Interface.AddMessage(MessageType.Error, false, "float could not be parsed in template " + Template.Name + " in textual X object file " + FileName);
											return false;
										}
										else
										{
											a = 0.0;
										}
									}
									Array.Resize<object>(ref Structure.Data, Structure.Data.Length + 1);
									Structure.Data[Structure.Data.Length - 1] = a;
									Position++;
									i = Position;
									break;
								} Position++;
							} break;
						case "string":
							while (Position < Content.Length) {
								if (Content[Position] == '"') {
									Position++;
									break;
								} else if (!char.IsWhiteSpace(Content, Position)) {
									Interface.AddMessage(MessageType.Error, false, "Invalid character encountered while processing a string in template " + Template.Name + " in textual X object file " + FileName);
									return false;
								} else {
									Position++;
								}
							} if (Position >= Content.Length) {
								Interface.AddMessage(MessageType.Error, false, "Unexpected end of file encountered while processing a string in template " + Template.Name + " in textual X object file " + FileName);
								return false;
							}
							i = Position;
							while (Position < Content.Length) {
								if (Content[Position] == '"') {
									Position++;
									break;
								} else {
									Position++;
								}
							} if (Position >= Content.Length) {
								Interface.AddMessage(MessageType.Error, false, "Unexpected end of file encountered while processing a string in template " + Template.Name + " in textual X object file " + FileName);
								return false;
							}
							string t = Content.Substring(i, Position - i - 1);
							while (Position < Content.Length) {
								if (Content[Position] == ';') {
									Position++;
									break;
								} else if (!char.IsWhiteSpace(Content, Position)) {
									Interface.AddMessage(MessageType.Error, false, "Invalid character encountered while processing a string in template " + Template.Name + " in textual X object file " + FileName);
									return false;
								} else {
									Position++;
								}
							} if (Position >= Content.Length) {
								Interface.AddMessage(MessageType.Error, false, "Unexpected end of file encountered while processing a string in template " + Template.Name + " in textual X object file " + FileName);
								return false;
							}
							Array.Resize<object>(ref Structure.Data, Structure.Data.Length + 1);
							Structure.Data[Structure.Data.Length - 1] = t;
							i = Position;
							break;
						case "string2":
							int OldPosition = Position;
							if (Position < Content.Length - 1)
							{
								Position++;
							}
							bool SF = false;
							while (char.IsWhiteSpace(Content[Position]) || Content[Position] == '{' && Position < Content.Length)
							{
								Position++;
							}
							i = Position;
							if (Position >= Content.Length)
							{
								Interface.AddMessage(MessageType.Error, false, "Unexpected end of file encountered while processing a string in template " + Template.Name + " in textual X object file " + FileName);
								return false;
							}
							while (Position <= Content.Length)
							{
								if (Content[Position] == ';')
								{
									SF = true;
									break;
								}
								else if (Content[Position] == '}')
								{
									SF = true;
									break;
								}
								else if (Content[Position] == ',')
								{
									break;
								}
								Position++;
							}
							if (Position >= Content.Length)
							{
								Interface.AddMessage(MessageType.Error, false, "Unexpected end of file encountered while processing a string in template " + Template.Name + " in textual X object file " + FileName);
								return false;
							}
							if (SF == true)
							{


								t = Content.Substring(i, Position - i).Trim();
								if (IsTemplate(t.Split(' ')[0]) || t.Length == 0)
								{
									//HACK: Check if the found string starts with a template name to determine whether we should discard it
									SF = false;
									Position = OldPosition;
								}
								else
								{
									Array.Resize<object>(ref Structure.Data, Structure.Data.Length + 1);
									Structure.Data[Structure.Data.Length - 1] = t;
								}
							}
							else
							{
								//String wasn't found
								Position = OldPosition;
							}
							if (Position >= Content.Length)
							{
								Interface.AddMessage(MessageType.Error, false, "Unexpected end of file encountered while processing a string in template " + Template.Name + " in textual X object file " + FileName);
								return false;
							}

							/*
							 * There could potentially be any number of strings, so add another to the array
							 */
							if (SF)
							{
								Array.Resize(ref Template.Members, Template.Members.Length + 1);
								for (int a = m; a < Template.Members.Length - 1; a++)
								{
									Template.Members[a + 1] = Template.Members[a];
								}
								Template.Members[m] = "string2";
							}
							break;
						default:
							{
								Structure o;
								if (!ReadTextualTemplate(FileName, Content, ref Position, GetTemplate(Template.Members[m], false), true, out o)) {
									return false;
								}
								while (Position < Content.Length) {
									if (Content[Position] == ';') {
										Position++;
										break;
									} else if (!char.IsWhiteSpace(Content, Position)) {
										Interface.AddMessage(MessageType.Error, false, "Invalid character encountered while processing an inlined template in template " + Template.Name + " in textual X object file " + FileName);
										return false;
									} else {
										Position++;
									}
								} if (Position >= Content.Length) {
									Interface.AddMessage(MessageType.Error, false, "Unexpected end of file encountered while processing an inlined template in template " + Template.Name + " in textual X object file " + FileName);
									return false;
								}
								Array.Resize<object>(ref Structure.Data, Structure.Data.Length + 1);
								Structure.Data[Structure.Data.Length - 1] = o;
								i = Position;
							} break;
					}
				}
			}
			if (m >= Template.Members.Length) {
				if (Inline) {
					return true;
				} else {
					// closed non-inline template
					while (Position < Content.Length)
					{
						if (Content[Position] == ';')
						{
							Position++;
						}
						else if (Content[Position] == '}')
						{
							Position++;
							break;
						}
						else if (!char.IsWhiteSpace(Content, Position))
						{
							Interface.AddMessage(MessageType.Error, false, "Invalid character encountered in template " + Template.Name + " in textual X object file " + FileName);
							return false;
						}
						else
						{
							Position++;
						}
					} if (Position >= Content.Length)
					{
						Interface.AddMessage(MessageType.Error, false, "Unexpected end of file encountered in template " + Template.Name + " in textual X object file " + FileName);
						return false;
					}
					return true;
				}
			} else {
				if (q) {
					Interface.AddMessage(MessageType.Error, false, "Quotation mark not closed at the end of the file in template " + Template.Name + " in textual X object file " + FileName);
					return false;
				} else if (Template.Name.Length != 0) {
					Interface.AddMessage(MessageType.Error, false, "Unexpected end of file encountered in template " + Template.Name + " in textual X object file " + FileName);
					return false;
				} else {
					return true;
				}
			}
		}

		// ================================

		// load binary x
		private static ObjectManager.StaticObject LoadBinaryX(string FileName, byte[] Data, int StartingPosition, int FloatingPointSize, ObjectLoadMode LoadMode, bool ForceTextureRepeatX, bool ForceTextureRepeatY) {
			// parse file
			AlternateStructure = false;
			LoadedMaterials = new Structure[] {};
			Structure Structure;
			try {
				bool Result;
				using (System.IO.MemoryStream Stream = new System.IO.MemoryStream(Data)) {
					using (System.IO.BinaryReader Reader = new System.IO.BinaryReader(Stream)) {
						Stream.Position = StartingPosition;
						BinaryCache Cache = new BinaryCache();
						Cache.IntegersRemaining = 0;
						Cache.FloatsRemaining = 0;
						Result = ReadBinaryTemplate(FileName, Reader, FloatingPointSize, new Template("", new string[] { "[...]" }), false, ref Cache, out Structure);
						Reader.Close();
					}
					Stream.Close();
				} if (!Result) {
					return null;
				}
			} catch (Exception ex) {
				Interface.AddMessage(MessageType.Error, false, "Unhandled error (" + ex.Message + ") encountered in binary X object file " + FileName);
				return null;
			}
			// process structure
			ObjectManager.StaticObject Object;
			if (!ProcessStructure(FileName, Structure, out Object, LoadMode, ForceTextureRepeatX, ForceTextureRepeatY)) {
				return null;
			} return Object;
		}

		// read binary template
		private struct BinaryCache {
			internal int[] Integers;
			internal int IntegersRemaining;
			internal double[] Floats;
			internal int FloatsRemaining;
		}
		private static bool ReadBinaryTemplate(string FileName, System.IO.BinaryReader Reader, int FloatingPointSize, Template Template, bool Inline, ref BinaryCache Cache, out Structure Structure) {
			const short TOKEN_NAME = 0x1;
			const short TOKEN_STRING = 0x2;
			const short TOKEN_INTEGER = 0x3;
			const short TOKEN_INTEGER_LIST = 0x6;
			const short TOKEN_FLOAT_LIST = 0x7;
			const short TOKEN_OBRACE = 0xA;
			const short TOKEN_CBRACE = 0xB;
			const short TOKEN_COMMA = 0x13;
			const short TOKEN_SEMICOLON = 0x14;
			Structure = new Structure(Template.Name, new object[] { }, Template.Key);
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			System.Text.ASCIIEncoding Ascii = new System.Text.ASCIIEncoding();
			int m; for (m = 0; m < Template.Members.Length; m++) {
				if (Template.Members[m] == "[???]") {
					// unknown template
					int Level = 0;
					if (Cache.IntegersRemaining != 0) {
						Interface.AddMessage(MessageType.Error, false, "An integer list was not depleted at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
					} else if (Cache.FloatsRemaining != 0) {
						Interface.AddMessage(MessageType.Error, false, "A float list was not depleted at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
					}
					short Token = Reader.ReadInt16();
					switch (Token) {
						case TOKEN_NAME:
							{
								Level++;
								int n = Reader.ReadInt32();
								if (n < 1) {
									Interface.AddMessage(MessageType.Error, false, "count is invalid in TOKEN_NAME at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
									return false;
								}
								Reader.BaseStream.Position += n;
								Token = Reader.ReadInt16();
								if (Token != TOKEN_OBRACE) {
									Interface.AddMessage(MessageType.Error, false, "TOKEN_OBRACE expected at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
									return false;
								}
							} break;
						case TOKEN_INTEGER:
							{
								Reader.BaseStream.Position += 4;
							} break;
						case TOKEN_INTEGER_LIST:
							{
								int n = Reader.ReadInt32();
								if (n < 0) {
									Interface.AddMessage(MessageType.Error, false, "count is invalid in TOKEN_INTEGER_LIST at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
									return false;
								}
								Reader.BaseStream.Position += 4 * n;
							} break;
						case TOKEN_FLOAT_LIST:
							{
								int n = Reader.ReadInt32();
								if (n < 0) {
									Interface.AddMessage(MessageType.Error, false, "count is invalid in TOKEN_FLOAT_LIST at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
									return false;
								}
								Reader.BaseStream.Position += (FloatingPointSize >> 3) * n;
							} break;
						case TOKEN_STRING:
							{
								int n = Reader.ReadInt32();
								if (n < 0) {
									Interface.AddMessage(MessageType.Error, false, "count is invalid in TOKEN_STRING at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
									return false;
								}
								Reader.BaseStream.Position += n;
								Token = Reader.ReadInt16();
								if (Token != TOKEN_COMMA & Token != TOKEN_SEMICOLON) {
									Interface.AddMessage(MessageType.Error, false, "TOKEN_COMMA or TOKEN_SEMICOLON expected at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
									return false;
								}
							} break;
						case TOKEN_OBRACE:
							Interface.AddMessage(MessageType.Error, false, "Unexpected token TOKEN_OBRACE encountered at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
							return false;
						case TOKEN_CBRACE:
							if (Level == 0) return true;
							Level--;
							break;
						default:
							Interface.AddMessage(MessageType.Error, false, "Unknown token encountered at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
							return false;
					} m--;
				} else if (Template.Members[m] == "[...]") {
					// any template
					if (Cache.IntegersRemaining != 0) {
						Interface.AddMessage(MessageType.Error, false, "An integer list was not depleted at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
					} else if (Cache.FloatsRemaining != 0) {
						Interface.AddMessage(MessageType.Error, false, "A float list was not depleted at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
					}
					if (Template.Name.Length == 0 && Reader.BaseStream.Position == Reader.BaseStream.Length) {
						// end of file
						return true;
					}
					short Token = Reader.ReadInt16();
					switch (Token) {
						case TOKEN_NAME:
							int n = Reader.ReadInt32();
							if (n < 1) {
								Interface.AddMessage(MessageType.Error, false, "count is invalid in TOKEN_NAME at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
								return false;
							}
							string Name = new string(Ascii.GetChars(Reader.ReadBytes(n)));
							Token = Reader.ReadInt16();
							if (Token != TOKEN_OBRACE) {
								Interface.AddMessage(MessageType.Error, false, "TOKEN_OBRACE expected at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
								return false;
							}
							Structure o;
							if (!ReadBinaryTemplate(FileName, Reader, FloatingPointSize, GetTemplate(Name, true), false, ref Cache, out o)) {
								return false;
							}
							Array.Resize<object>(ref Structure.Data, Structure.Data.Length + 1);
							Structure.Data[Structure.Data.Length - 1] = o;
							break;
						case TOKEN_CBRACE:
							if (Template.Name.Length == 0) {
								Interface.AddMessage(MessageType.Error, false, "Unexpected TOKEN_CBRACE encountered at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
								return false;
							}
							m++;
							break;
						default:
							Interface.AddMessage(MessageType.Error, false, "TOKEN_NAME or TOKEN_CBRACE expected at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
							return false;
					} m--;
				} else if (Template.Members[m].EndsWith("]", StringComparison.Ordinal)) {
					// inlined array expected
					string r = Template.Members[m].Substring(0, Template.Members[m].Length - 1);
					int h = r.IndexOf('[');
					if (h >= 0) {
						string z = r.Substring(h + 1, r.Length - h - 1);
						r = r.Substring(0, h);
						if (!int.TryParse(z, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out h)) {
							Interface.AddMessage(MessageType.Error, false, "The internal format description for a template array is invalid in template " + Template.Name + " in binary X object file " + FileName);
							return false;
						}
						if (h < 0 || h >= Structure.Data.Length || !(Structure.Data[h] is int)) {
							Interface.AddMessage(MessageType.Error, false, "The internal format description for a template array is invalid in template " + Template.Name + " in binary X object file " + FileName);
							return false;
						}
						h = (int)Structure.Data[h];
					} else {
						Interface.AddMessage(MessageType.Error, false, "The internal format description for a template array is invalid in template " + Template.Name + " in binary X object file " + FileName);
						return false;
					}
					if (r == "DWORD") {
						// dword array
						int[] o = new int[h];
						for (int i = 0; i < h; i++) {
							if (Cache.IntegersRemaining != 0) {
								// use cached integer
								int a = Cache.Integers[Cache.IntegersRemaining - 1];
								Cache.IntegersRemaining--;
								o[i] = a;
							} else if (Cache.FloatsRemaining != 0) {
								// cannot use cached float
								Interface.AddMessage(MessageType.Error, false, "A float list was not depleted at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
								return false;
							} else {
								while (true) {
									short Token = Reader.ReadInt16();
									if (Token == TOKEN_INTEGER) {
										int a = Reader.ReadInt32();
										o[i] = a; break;
									} else if (Token == TOKEN_INTEGER_LIST) {
										int n = Reader.ReadInt32();
										if (n < 0) {
											Interface.AddMessage(MessageType.Error, false, "count is invalid in TOKEN_INTEGER_LIST at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
											return false;
										}
										if (n != 0) {
											Cache.Integers = new int[n];
											for (int j = 0; j < n; j++) {
												Cache.Integers[n - j - 1] = Reader.ReadInt32();
											}
											Cache.IntegersRemaining = n - 1;
											int a = Cache.Integers[Cache.IntegersRemaining];
											o[i] = a;
											break;
										}
									} else {
										Interface.AddMessage(MessageType.Error, false, "TOKEN_INTEGER or TOKEN_INTEGER_LIST expected at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
										return false;
									}
								}
							}
						}
						Array.Resize<object>(ref Structure.Data, Structure.Data.Length + 1);
						Structure.Data[Structure.Data.Length - 1] = o;
					} else if (r == "float") {
						// float array
						double[] o = new double[h];
						for (int i = 0; i < h; i++) {
							if (Cache.IntegersRemaining != 0) {
								// cannot use cached integer
								Interface.AddMessage(MessageType.Error, false, "An integer list was not depleted at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
								return false;
							} else if (Cache.FloatsRemaining != 0) {
								// use cached float
								double a = Cache.Floats[Cache.FloatsRemaining - 1];
								Cache.FloatsRemaining--;
								o[i] = a;
							} else {
								while (true) {
									short Token = Reader.ReadInt16();
									if (Token == TOKEN_FLOAT_LIST) {
										int n = Reader.ReadInt32();
										if (n < 0) {
											Interface.AddMessage(MessageType.Error, false, "count is invalid in TOKEN_FLOAT_LIST at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
											return false;
										}
										if (n != 0) {
											Cache.Floats = new double[n];
											for (int j = 0; j < n; j++) {
												if (FloatingPointSize == 32) {
													Cache.Floats[n - j - 1] = (double)Reader.ReadSingle();
												} else if (FloatingPointSize == 64) {
													Cache.Floats[n - j - 1] = Reader.ReadDouble();
												}
											}
											Cache.FloatsRemaining = n - 1;
											double a = Cache.Floats[Cache.FloatsRemaining];
											o[i] = a;
											break;
										}
									} else {
										Interface.AddMessage(MessageType.Error, false, "TOKEN_FLOAT_LIST expected at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
										return false;
									}
								}
							}
						}
						Array.Resize<object>(ref Structure.Data, Structure.Data.Length + 1);
						Structure.Data[Structure.Data.Length - 1] = o;
					} else {
						// template array
						Structure[] o = new Structure[h];
						for (int i = 0; i < h; i++) {
							ReadBinaryTemplate(FileName, Reader, FloatingPointSize, GetTemplate(r, true), true, ref Cache, out o[i]);
						}
						Array.Resize<object>(ref Structure.Data, Structure.Data.Length + 1);
						Structure.Data[Structure.Data.Length - 1] = o;
					}
				} else {
					// inlined template or primitive expected
					switch (Template.Members[m]) {
						case "DWORD":
							// dword expected
							if (Cache.IntegersRemaining != 0) {
								// use cached integer
								int a = Cache.Integers[Cache.IntegersRemaining - 1];
								Cache.IntegersRemaining--;
								Array.Resize<object>(ref Structure.Data, Structure.Data.Length + 1);
								Structure.Data[Structure.Data.Length - 1] = a;
							} else if (Cache.FloatsRemaining != 0) {
								// cannot use cached float
								Interface.AddMessage(MessageType.Error, false, "A float list was not depleted at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
								return false;
							} else {
								// read new data
								while (true) {
									short Token = Reader.ReadInt16();
									if (Token == TOKEN_INTEGER) {
										int a = Reader.ReadInt32();
										Array.Resize<object>(ref Structure.Data, Structure.Data.Length + 1);
										Structure.Data[Structure.Data.Length - 1] = a;
										break;
									} else if (Token == TOKEN_INTEGER_LIST) {
										int n = Reader.ReadInt32();
										if (n < 0) {
											Interface.AddMessage(MessageType.Error, false, "count is invalid in TOKEN_INTEGER_LIST at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
											return false;
										}
										if (n != 0) {
											Cache.Integers = new int[n];
											for (int i = 0; i < n; i++) {
												Cache.Integers[n - i - 1] = Reader.ReadInt32();
											}
											Cache.IntegersRemaining = n - 1;
											int a = Cache.Integers[Cache.IntegersRemaining];
											Array.Resize<object>(ref Structure.Data, Structure.Data.Length + 1);
											Structure.Data[Structure.Data.Length - 1] = a;
											break;
										}
									} else {
										Interface.AddMessage(MessageType.Error, false, "TOKEN_INTEGER or TOKEN_INTEGER_LIST expected at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
										return false;
									}
								}
							} break;
						case "float":
							// float expected
							if (Cache.IntegersRemaining != 0) {
								// cannot use cached integer
								Interface.AddMessage(MessageType.Error, false, "An integer list was not depleted at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
								return false;
							} else if (Cache.FloatsRemaining != 0) {
								// use cached float
								double a = Cache.Floats[Cache.FloatsRemaining - 1];
								Cache.FloatsRemaining--;
								Array.Resize<object>(ref Structure.Data, Structure.Data.Length + 1);
								Structure.Data[Structure.Data.Length - 1] = a;
							} else {
								// read new data
								while (true) {
									short Token = Reader.ReadInt16();
									if (Token == TOKEN_FLOAT_LIST) {
										int n = Reader.ReadInt32();
										if (n < 0) {
											Interface.AddMessage(MessageType.Error, false, "count is invalid in TOKEN_FLOAT_LIST at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
											return false;
										}
										if (n != 0) {
											Cache.Floats = new double[n];
											for (int i = 0; i < n; i++) {
												if (FloatingPointSize == 32) {
													Cache.Floats[n - i - 1] = (double)Reader.ReadSingle();
												} else if (FloatingPointSize == 64) {
													Cache.Floats[n - i - 1] = Reader.ReadDouble();
												}
											}
											Cache.FloatsRemaining = n - 1;
											double a = Cache.Floats[Cache.FloatsRemaining];
											Array.Resize<object>(ref Structure.Data, Structure.Data.Length + 1);
											Structure.Data[Structure.Data.Length - 1] = a;
											break;
										}
									} else {
										Interface.AddMessage(MessageType.Error, false, "TOKEN_FLOAT_LIST expected at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
										return false;
									}
								}
							} break;
						case "string":
							{
								// string expected
								if (Cache.IntegersRemaining != 0) {
									Interface.AddMessage(MessageType.Error, false, "An integer list was not depleted at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
								} else if (Cache.FloatsRemaining != 0) {
									Interface.AddMessage(MessageType.Error, false, "A float list was not depleted at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
								}
								short Token = Reader.ReadInt16();
								if (Token == TOKEN_STRING) {
									int n = Reader.ReadInt32();
									if (n < 0) {
										Interface.AddMessage(MessageType.Error, false, "count is invalid in TOKEN_STRING at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
										return false;
									}
									string s = new string(Ascii.GetChars(Reader.ReadBytes(n)));
									Array.Resize<object>(ref Structure.Data, Structure.Data.Length + 1);
									Structure.Data[Structure.Data.Length - 1] = s;
									Token = Reader.ReadInt16();
									if (Token != TOKEN_SEMICOLON) {
										Interface.AddMessage(MessageType.Error, false, "TOKEN_SEMICOLON expected at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
										return false;
									}
								} else {
									Interface.AddMessage(MessageType.Error, false, "TOKEN_STRING expected at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
									return false;
								}
							} break;
						default:
							// inlined template expected
							Structure o;
							ReadBinaryTemplate(FileName, Reader, FloatingPointSize, GetTemplate(Template.Members[m], true), true, ref Cache, out o);
							Array.Resize<object>(ref Structure.Data, Structure.Data.Length + 1);
							Structure.Data[Structure.Data.Length - 1] = o;
							break;
					}
				}
			}
			if (Inline) {
				return true;
			} else {
				string s = Template.Members[Template.Members.Length - 1];
				if (s != "[???]" & s != "[...]") {
					int Token = Reader.ReadInt16();
					if (Token != TOKEN_CBRACE) {
						Interface.AddMessage(MessageType.Error, false, "TOKEN_CBRACE expected at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
						return false;
					}
				}
				return true;
			}
		}

		// ================================

		// structures
		private struct Material {
			internal Color32 faceColor;
			internal Color24 specularColor;
			internal Color24 emissiveColor;
			internal string TextureFilename;
		}

		// process structure
		private static bool ProcessStructure(string FileName, Structure Structure, out ObjectManager.StaticObject Object, ObjectLoadMode LoadMode, bool ForceTextureRepeatX, bool ForceTextureRepeatY) {
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			Object = new ObjectManager.StaticObject();
			Object.Mesh.Faces = new World.MeshFace[] { };
			Object.Mesh.Materials = new World.MeshMaterial[] { };
			Object.Mesh.Vertices = new VertexTemplate[] { };
			// file
			for (int i = 0; i < Structure.Data.Length; i++) {
				Structure f = Structure.Data[i] as Structure;
				if (f == null) {
					Interface.AddMessage(MessageType.Error, false, "Top-level inlined arguments are invalid in x object file " + FileName);
					return false;
				}
				switch (f.Name) {
					case "Frame Root":
					case "Frame":
						//This is just a placeholder around the other templates
						ProcessStructure(FileName, f, out Object, LoadMode, ForceTextureRepeatX, ForceTextureRepeatX);
						break;
					case "Mesh":
						{
							// mesh
							if (f.Data.Length < 4)
							{
								Interface.AddMessage(MessageType.Error, false, "Mesh is expected to have at least 4 arguments in x object file " + FileName);
								return false;
							}
							else if (!(f.Data[0] is int))
							{
								Interface.AddMessage(MessageType.Error, false, "nVertices is expected to be a DWORD in Mesh in x object file " + FileName);
								return false;
							}
							else if (!(f.Data[1] is Structure[]))
							{
								Interface.AddMessage(MessageType.Error, false, "vertices[nVertices] is expected to be a Vector array in Mesh in x object file " + FileName);
								return false;
							}
							else if (!(f.Data[2] is int))
							{
								Interface.AddMessage(MessageType.Error, false, "nFaces is expected to be a DWORD in Mesh in x object file " + FileName);
								return false;
							}
							else if (!(f.Data[3] is Structure[]))
							{
								Interface.AddMessage(MessageType.Error, false, "faces[nFaces] is expected to be a MeshFace array in Mesh in x object file " + FileName);
								return false;
							}
							int nVertices = (int)f.Data[0];
							if (nVertices < 0)
							{
								Interface.AddMessage(MessageType.Error, false, "nVertices is expected to be non-negative in Mesh in x object file " + FileName);
								return false;
							}
							Structure[] vertices = (Structure[])f.Data[1];
							if (nVertices != vertices.Length)
							{
								Interface.AddMessage(MessageType.Error, false, "nVertices does not match with the length of array vertices in Mesh in x object file " + FileName);
								return false;
							}
							int nFaces = (int)f.Data[2];
							if (nFaces < 0)
							{
								Interface.AddMessage(MessageType.Error, false, "nFaces is expected to be non-negative in Mesh in x object file " + FileName);
								return false;
							}
							Structure[] faces = (Structure[])f.Data[3];
							if (nFaces != faces.Length)
							{
								Interface.AddMessage(MessageType.Error, false, "nFaces does not match with the length of array faces in Mesh in x object file " + FileName);
								return false;
							}
							// collect vertices
							VertexTemplate[] Vertices = new VertexTemplate[nVertices];
							for (int j = 0; j < nVertices; j++)
							{
								if (vertices[j].Name != "Vector")
								{
									Interface.AddMessage(MessageType.Error, false, "vertices[" + j.ToString(Culture) + "] is expected to be of template Vertex in Mesh in x object file " + FileName);
									return false;
								}
								else if (vertices[j].Data.Length != 3)
								{
									Interface.AddMessage(MessageType.Error, false, "vertices[" + j.ToString(Culture) + "] is expected to have 3 arguments in Mesh in x object file " + FileName);
									return false;
								}
								else if (!(vertices[j].Data[0] is double))
								{
									Interface.AddMessage(MessageType.Error, false, "x is expected to be a float in vertices[" + j.ToString(Culture) + "] in Mesh in x object file " + FileName);
									return false;
								}
								else if (!(vertices[j].Data[1] is double))
								{
									Interface.AddMessage(MessageType.Error, false, "y is expected to be a float in vertices[" + j.ToString(Culture) + "] in Mesh in x object file " + FileName);
									return false;
								}
								else if (!(vertices[j].Data[2] is double))
								{
									Interface.AddMessage(MessageType.Error, false, "z is expected to be a float in vertices[" + j.ToString(Culture) + "] in Mesh in x object file " + FileName);
									return false;
								}
								double x = (double)vertices[j].Data[0];
								double y = (double)vertices[j].Data[1];
								double z = (double)vertices[j].Data[2];
								Vertices[j] = new Vertex(new Vector3(x,y,z));
							}
							// collect faces
							int[][] Faces = new int[nFaces][];
							Vector3[][] FaceNormals = new Vector3[nFaces][];
							int[] FaceMaterials = new int[nFaces];
							for (int j = 0; j < nFaces; j++)
							{
								FaceMaterials[j] = -1;
							}
							for (int j = 0; j < nFaces; j++)
							{
								if (faces[j].Name != "MeshFace")
								{
									Interface.AddMessage(MessageType.Error, false, "faces[" + j.ToString(Culture) + "] is expected to be of template MeshFace in Mesh in x object file " + FileName);
									return false;
								}
								else if (faces[j].Data.Length != 2)
								{
									Interface.AddMessage(MessageType.Error, false, "face[" + j.ToString(Culture) + "] is expected to have 2 arguments in Mesh in x object file " + FileName);
									return false;
								}
								else if (!(faces[j].Data[0] is int))
								{
									Interface.AddMessage(MessageType.Error, false, "nFaceVertexIndices is expected to be a DWORD in face[" + j.ToString(Culture) + "] in Mesh in x object file " + FileName);
									return false;
								}
								else if (!(faces[j].Data[1] is int[]))
								{
									Interface.AddMessage(MessageType.Error, false, "faceVertexIndices[nFaceVertexIndices] is expected to be a DWORD array in face[" + j.ToString(Culture) + "] in Mesh in x object file " + FileName);
									return false;
								}
								int nFaceVertexIndices = (int)faces[j].Data[0];
								if (nFaceVertexIndices < 0)
								{
									Interface.AddMessage(MessageType.Error, false, "nFaceVertexIndices is expected to be non-negative in MeshFace in Mesh in x object file " + FileName);
									return false;
								}
								int[] faceVertexIndices = (int[])faces[j].Data[1];
								if (nFaceVertexIndices != faceVertexIndices.Length)
								{
									Interface.AddMessage(MessageType.Error, false, "nFaceVertexIndices does not match with the length of array faceVertexIndices in face[" + j.ToString(Culture) + "] in Mesh in x object file " + FileName);
									return false;
								}
								Faces[j] = new int[nFaceVertexIndices];
								FaceNormals[j] = new Vector3[nFaceVertexIndices];
								for (int k = 0; k < nFaceVertexIndices; k++)
								{
									if (faceVertexIndices[k] < 0 | faceVertexIndices[k] >= nVertices)
									{
										Interface.AddMessage(MessageType.Error, false, "faceVertexIndices[" + k.ToString(Culture) + "] does not reference a valid vertex in face[" + j.ToString(Culture) + "] in Mesh in x object file " + FileName);
										return false;
									}
									Faces[j][k] = faceVertexIndices[k];
									FaceNormals[j][k] = new Vector3(0.0f, 0.0f, 0.0f);
								}
							}
							int ds = 4;
							if (AlternateStructure == true)
							{
								ds = f.Data.Length - 1;
								//If this file has the alternate structure, find the templates (if existing) after the mesh declaration
								bool cf = false, cn = false;
								for (int g = i + 1; g < Structure.Data.Length; g++)
								{
									int dl = f.Data.Length;
									Structure h = Structure.Data[g] as Structure;
									if (h == null)
									{
										continue;
									}
									if (cf && cn)
									{
										//A set of texture co-ords and normal co-ords has been found, so break the loop
										break;
									}
									switch (h.Name)
									{
										case "MeshTextureCoords":
											if (!cf)
											{
												cf = true;
												//Insert into the structure array
												Array.Resize(ref f.Data, dl + 1);
												f.Data[dl] = h;
												//Remove from the main array
												for (int k = g + 1; k < Structure.Data.Length; k++)
												{
													Structure.Data[k - 1] = Structure.Data[k];
												}
												Array.Resize(ref Structure.Data, Structure.Data.Length - 1);
												g--;
											}
											break;
										case "MeshNormals":
											if (!cn)
											{
												cn = true;
												//Insert into the structure array
												Array.Resize(ref f.Data, dl + 1);
												f.Data[dl] = h;
												//Remove from the main array
												for (int k = g + 1; k < Structure.Data.Length; k++)
												{
													Structure.Data[k - 1] = Structure.Data[k];
												}
												Array.Resize(ref Structure.Data, Structure.Data.Length - 1);
												g--;
											}
											break;
										case "Mesh":
											//If we've found a mesh, assume that the normals and co-ords have been declared or omitted for the previous mesh
											cf = true;
											cn = true;
											break;
										case "MeshVertexColors":
											break;
										default:
											continue;
									}

								}
							}
							// collect additional templates
							Material[] Materials = new Material[] { };
							for (int j = ds; j < f.Data.Length; j++)
							{
								Structure g = f.Data[j] as Structure;
								if (g == null)
								{
									Interface.AddMessage(MessageType.Error, false, "Unexpected inlined argument encountered in Mesh in x object file " + FileName);
									return false;
								}
								switch (g.Name)
								{
									case "MeshMaterialList":
										{
											// meshmateriallist
											if (g.Data.Length < 3)
											{
												Interface.AddMessage(MessageType.Error, false, "MeshMaterialList is expected to have at least 3 arguments in Mesh in x object file " + FileName);
												return false;
											}
											else if (!(g.Data[0] is int))
											{
												Interface.AddMessage(MessageType.Error, false, "nMaterials is expected to be a DWORD in MeshMaterialList in Mesh in x object file " + FileName);
												return false;
											}
											else if (!(g.Data[1] is int))
											{
												Interface.AddMessage(MessageType.Error, false, "nFaceIndexes is expected to be a DWORD in MeshMaterialList in Mesh in x object file " + FileName);
												return false;
											}
											else if (!(g.Data[2] is int[]))
											{
												Interface.AddMessage(MessageType.Error, false, "faceIndexes[nFaceIndexes] is expected to be a DWORD array in MeshMaterialList in Mesh in x object file " + FileName);
												return false;
											}
											int nMaterials = (int)g.Data[0];
											if (nMaterials < 0)
											{
												Interface.AddMessage(MessageType.Error, false, "nMaterials is expected to be non-negative in MeshMaterialList in Mesh in x object file " + FileName);
												return false;
											}
											int nFaceIndexes = (int)g.Data[1];
											if (nFaceIndexes < 0)
											{
												Interface.AddMessage(MessageType.Error, false, "nFaceIndexes is expected to be non-negative in MeshMaterialList in Mesh in x object file " + FileName);
												return false;
											}
											else if (nFaceIndexes > nFaces)
											{
												Interface.AddMessage(MessageType.Error, false, "nFaceIndexes does not reference valid faces in MeshMaterialList in Mesh in x object file " + FileName);
												return false;
											}
											int[] faceIndexes = (int[])g.Data[2];
											if (nFaceIndexes != faceIndexes.Length)
											{
												Interface.AddMessage(MessageType.Error, false, "nFaceIndexes does not match with the length of array faceIndexes in face[" + j.ToString(Culture) + "] in Mesh in x object file " + FileName);
												return false;
											}
											for (int k = 0; k < nFaceIndexes; k++)
											{
												if (faceIndexes[k] < 0 | faceIndexes[k] >= nMaterials)
												{
													Interface.AddMessage(MessageType.Error, false, "faceIndexes[" + k.ToString(Culture) + "] does not reference a valid Material template in MeshMaterialList in Mesh in x object file " + FileName);
													return false;
												}
											}
											if (g.Data.Length >= 4 && g.Data[3] is String)
											{
												for (int m = 3; m < g.Data.Length; m++)
												{
													for (int n = 0; n < LoadedMaterials.Length; n++)
													{
														if ((string)g.Data[m] == LoadedMaterials[n].Key)
														{
															g.Data[m] = LoadedMaterials[n];
															break;
														}
													}
												}
											}

											{
												// collect material templates
												int mn = Materials.Length;
												Array.Resize<Material>(ref Materials, mn + nMaterials);
												for (int k = 0; k < nMaterials; k++)
												{
													Materials[mn + k].faceColor = Color32.White;
													Materials[mn + k].specularColor = Color24.Black;
													Materials[mn + k].emissiveColor = Color24.Black;
													Materials[mn + k].TextureFilename = null;
												}
												int MaterialIndex = mn;
												for (int k = 3; k < g.Data.Length; k++)
												{
													Structure h = g.Data[k] as Structure;
													if (h == null)
													{
														Interface.AddMessage(MessageType.Error, false, "Unexpected inlined argument encountered in MeshMaterialList in Mesh in x object file " + FileName);
														return false;
													}
													else if (h.Name != "Material")
													{
														Interface.AddMessage(MessageType.Error, false, "Material template expected in MeshMaterialList in Mesh in x object file " + FileName);
														return false;
													}
													else
													{
														// material
														if (h.Data.Length < 4)
														{
															Interface.AddMessage(MessageType.Error, false, "Material is expected to have at least 4 arguments in Material in MeshMaterialList in Mesh in x object file " +
																FileName);
															return false;
														}
														else if (!(h.Data[0] is Structure))
														{
															Interface.AddMessage(MessageType.Error, false, "faceColor is expected to be a ColorRGBA in Material in MeshMaterialList in Mesh in x object file " +
																FileName);
															return false;
														}
														else if (!(h.Data[1] is double))
														{
															Interface.AddMessage(MessageType.Error, false, "power is expected to be a float in Material in MeshMaterialList in Mesh in x object file " + FileName);
															return false;
														}
														else if (!(h.Data[2] is Structure))
														{
															Interface.AddMessage(MessageType.Error, false, "specularColor is expected to be a ColorRGBA in Material in MeshMaterialList in Mesh in x object file " +
																FileName);
															return false;
														}
														else if (!(h.Data[3] is Structure))
														{
															Interface.AddMessage(MessageType.Error, false, "emissiveColor is expected to be a ColorRGBA in Material in MeshMaterialList in Mesh in x object file " +
																FileName);
															return false;
														}
														Structure faceColor = (Structure)h.Data[0];
														Structure specularColor = (Structure)h.Data[2];
														Structure emissiveColor = (Structure)h.Data[3];
														double red, green, blue, alpha;
														// collect face color
														if (faceColor.Name != "ColorRGBA")
														{
															Interface.AddMessage(MessageType.Error, false, "faceColor is expected to be a ColorRGBA in Material in MeshMaterialList in Mesh in x object file " +
																FileName);
															return false;
														}
														else if (faceColor.Data.Length != 4)
														{
															Interface.AddMessage(MessageType.Error, false, "faceColor is expected to have 4 arguments in Material in MeshMaterialList in Mesh in x object file " +
																FileName);
															return false;
														}
														else if (!(faceColor.Data[0] is double))
														{
															Interface.AddMessage(MessageType.Error, false, "red is expected to be a float in faceColor in Material in MeshMaterialList in Mesh in x object file " +
																FileName);
															return false;
														}
														else if (!(faceColor.Data[1] is double))
														{
															Interface.AddMessage(MessageType.Error, false, "green is expected to be a float in faceColor in Material in MeshMaterialList in Mesh in x object file " +
																FileName);
															return false;
														}
														else if (!(faceColor.Data[2] is double))
														{
															Interface.AddMessage(MessageType.Error, false, "blue is expected to be a float in faceColor in Material in MeshMaterialList in Mesh in x object file " +
																FileName);
															return false;
														}
														else if (!(faceColor.Data[3] is double))
														{
															Interface.AddMessage(MessageType.Error, false, "alpha is expected to be a float in faceColor in Material in MeshMaterialList in Mesh in x object file " +
																FileName);
															return false;
														}
														red = (double)faceColor.Data[0];
														green = (double)faceColor.Data[1];
														blue = (double)faceColor.Data[2];
														alpha = (double)faceColor.Data[3];
														if (red < 0.0 | red > 1.0)
														{
															Interface.AddMessage(MessageType.Error, false, "red is expected to be in the range from 0.0 to 1.0 in faceColor in Material in MeshMaterialList in Mesh in x object file " +
																FileName);
															red = red < 0.5 ? 0.0 : 1.0;
														}
														if (green < 0.0 | green > 1.0)
														{
															Interface.AddMessage(MessageType.Error, false, "green is expected to be in the range from 0.0 to 1.0 in faceColor in Material in MeshMaterialList in Mesh in x object file " +
																FileName);
															green = green < 0.5 ? 0.0 : 1.0;
														}
														if (blue < 0.0 | blue > 1.0)
														{
															Interface.AddMessage(MessageType.Error, false, "blue is expected to be in the range from 0.0 to 1.0 in faceColor in Material in MeshMaterialList in Mesh in x object file " +
																FileName);
															blue = blue < 0.5 ? 0.0 : 1.0;
														}
														if (alpha < 0.0 | alpha > 1.0)
														{
															Interface.AddMessage(MessageType.Error, false, "alpha is expected to be in the range from 0.0 to 1.0 in faceColor in Material in MeshMaterialList in Mesh in x object file " +
																FileName);
															alpha = alpha < 0.5 ? 0.0 : 1.0;
														}
														Materials[MaterialIndex].faceColor = new Color32((byte)Math.Round(255.0 * red),
															(byte)Math.Round(255.0 * green), (byte)Math.Round(255.0 * blue), (byte)Math.Round(255.0 * alpha));
														// collect specular color
														if (specularColor.Name != "ColorRGB")
														{
															Interface.AddMessage(MessageType.Error, false, "specularColor is expected to be a ColorRGB in Material in MeshMaterialList in Mesh in x object file " +
																FileName);
															return false;
														}
														else if (specularColor.Data.Length != 3)
														{
															Interface.AddMessage(MessageType.Error, false, "specularColor is expected to have 3 arguments in Material in MeshMaterialList in Mesh in x object file " +
																FileName);
															return false;
														}
														else if (!(specularColor.Data[0] is double))
														{
															Interface.AddMessage(MessageType.Error, false, "red is expected to be a float in specularColor in Material in MeshMaterialList in Mesh in x object file " +
																FileName);
															return false;
														}
														else if (!(specularColor.Data[1] is double))
														{
															Interface.AddMessage(MessageType.Error, false, "green is expected to be a float in specularColor in Material in MeshMaterialList in Mesh in x object file " +
																FileName);
															return false;
														}
														else if (!(specularColor.Data[2] is double))
														{
															Interface.AddMessage(MessageType.Error, false, "blue is expected to be a float in specularColor in Material in MeshMaterialList in Mesh in x object file " +
																FileName);
															return false;
														}
														red = (double)specularColor.Data[0];
														green = (double)specularColor.Data[1];
														blue = (double)specularColor.Data[2];
														if (red < 0.0 | red > 1.0)
														{
															Interface.AddMessage(MessageType.Error, false, "red is expected to be in the range from 0.0 to 1.0 in specularColor in Material in MeshMaterialList in Mesh in x object file " +
																FileName);
															red = red < 0.5 ? 0.0 : 1.0;
														}
														if (green < 0.0 | green > 1.0)
														{
															Interface.AddMessage(MessageType.Error, false, "green is expected to be in the range from 0.0 to 1.0 in specularColor in Material in MeshMaterialList in Mesh in x object file " +
																FileName);
															green = green < 0.5 ? 0.0 : 1.0;
														}
														if (blue < 0.0 | blue > 1.0)
														{
															Interface.AddMessage(MessageType.Error, false, "blue is expected to be in the range from 0.0 to 1.0 in specularColor in Material in MeshMaterialList in Mesh in x object file " +
																FileName);
															blue = blue < 0.5 ? 0.0 : 1.0;
														}
														Materials[MaterialIndex].specularColor = new Color24((byte)Math.Round(255.0 * red),
															(byte)Math.Round(255.0 * green), (byte)Math.Round(255.0 * blue));
														// collect emissive color
														if (emissiveColor.Name != "ColorRGB")
														{
															Interface.AddMessage(MessageType.Error, false, "emissiveColor is expected to be a ColorRGBA in Material in MeshMaterialList in Mesh in x object file " +
																FileName);
															return false;
														}
														else if (emissiveColor.Data.Length != 3)
														{
															Interface.AddMessage(MessageType.Error, false, "emissiveColor is expected to have 3 arguments in Material in MeshMaterialList in Mesh in x object file " +
																FileName);
															return false;
														}
														else if (!(emissiveColor.Data[0] is double))
														{
															Interface.AddMessage(MessageType.Error, false, "red is expected to be a float in emissiveColor in Material in MeshMaterialList in Mesh in x object file " +
																FileName);
															return false;
														}
														else if (!(emissiveColor.Data[1] is double))
														{
															Interface.AddMessage(MessageType.Error, false, "green is expected to be a float in emissiveColor in Material in MeshMaterialList in Mesh in x object file " +
																FileName);
															return false;
														}
														else if (!(emissiveColor.Data[2] is double))
														{
															Interface.AddMessage(MessageType.Error, false, "blue is expected to be a float in emissiveColor in Material in MeshMaterialList in Mesh in x object file " +
																FileName);
															return false;
														}
														red = (double)emissiveColor.Data[0];
														green = (double)emissiveColor.Data[1];
														blue = (double)emissiveColor.Data[2];
														if (red < 0.0 | red > 1.0)
														{
															Interface.AddMessage(MessageType.Error, false, "red is expected to be in the range from 0.0 to 1.0 in emissiveColor in Material in MeshMaterialList in Mesh in x object file " +
																FileName);
															red = red < 0.5 ? 0.0 : 1.0;
														}
														if (green < 0.0 | green > 1.0)
														{
															Interface.AddMessage(MessageType.Error, false, "green is expected to be in the range from 0.0 to 1.0 in emissiveColor in Material in MeshMaterialList in Mesh in x object file " +
																FileName);
															green = green < 0.5 ? 0.0 : 1.0;
														}
														if (blue < 0.0 | blue > 1.0)
														{
															Interface.AddMessage(MessageType.Error, false, "blue is expected to be in the range from 0.0 to 1.0 in emissiveColor in Material in MeshMaterialList in Mesh in x object file " +
																FileName);
															blue = blue < 0.5 ? 0.0 : 1.0;
														}
														Materials[MaterialIndex].emissiveColor = new Color24((byte)Math.Round(255.0 * red),
															(byte)Math.Round(255.0 * green), (byte)Math.Round(255.0 * blue));
														// collect additional templates
														for (int l = 4; l < h.Data.Length; l++)
														{
															Structure e = h.Data[l] as Structure;
															if (e == null)
															{
																Interface.AddMessage(MessageType.Error, false, "Unexpected inlined argument encountered in Material in MeshMaterialList in Mesh in x object file " +
																	FileName);
																return false;
															}
															switch (e.Name)
															{
																case "TextureFilename":
																	{
																		// texturefilename
																		if (e.Data.Length != 1)
																		{
																			Interface.AddMessage(MessageType.Error, false, "filename is expected to have 1 argument in TextureFilename in Material in MeshMaterialList in Mesh in x object file " +
																				FileName);
																			return false;
																		}
																		else if (!(e.Data[0] is string))
																		{
																			Interface.AddMessage(MessageType.Error, false, "filename is expected to be a string in TextureFilename in Material in MeshMaterialList in Mesh in x object file " +
																				FileName);
																			return false;
																		}
																		string filename = (string)e.Data[0];
																		if (OpenBveApi.Path.ContainsInvalidChars(filename))
																		{
																			Interface.AddMessage(MessageType.Error, false, "filename contains illegal characters in TextureFilename in Material in MeshMaterialList in Mesh in x object file " +
																				FileName);
																		}
																		else
																		{
																			string File = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), filename);
																			if (System.IO.File.Exists(File))
																			{
																				Materials[MaterialIndex].TextureFilename = File;
																			}
																			else
																			{
																				Interface.AddMessage(MessageType.Error, true, "The texture file " + File + " could not be found in TextureFilename in Material in MeshMaterialList in Mesh in x object file " +
																					FileName);
																			}
																		}
																	}
																	break;
																default:
																	// unknown
																	Interface.AddMessage(MessageType.Warning, false, "Unsupported template " + e.Name + " encountered in MeshMaterialList in Mesh in x object file " +
																		FileName);
																	break;
															}
														}
														// finish
														MaterialIndex++;
													}
												}
												if (MaterialIndex != mn + nMaterials)
												{
													Interface.AddMessage(MessageType.Error, false, "nMaterials does not match the number of Material templates encountered in Material in MeshMaterialList in Mesh in x object file " +
														FileName);
													return false;
												}
											}
											// assign materials
											for (int k = 0; k < nFaceIndexes; k++)
											{
												FaceMaterials[k] = faceIndexes[k];
											}
											if (nMaterials != 0)
											{
												for (int k = 0; k < nFaces; k++)
												{
													if (FaceMaterials[k] == -1)
													{
														FaceMaterials[k] = 0;
													}
												}
											}
										}
										break;
									case "MeshTextureCoords":
										{
											// meshtexturecoords
											if (g.Data.Length != 2)
											{
												Interface.AddMessage(MessageType.Error, false, "MeshTextureCoords is expected to have 2 arguments in Mesh in x object file " + FileName);
												return false;
											}
											else if (!(g.Data[0] is int))
											{
												Interface.AddMessage(MessageType.Error, false, "nTextureCoords is expected to be a DWORD in MeshTextureCoords in Mesh in x object file " + FileName);
												return false;
											}
											else if (!(g.Data[1] is Structure[]))
											{
												Interface.AddMessage(MessageType.Error, false, "textureCoords[nTextureCoords] is expected to be a Coords2d array in MeshTextureCoords in Mesh in x object file " + FileName);
												return false;
											}
											int nTextureCoords = (int)g.Data[0];
											Structure[] textureCoords = (Structure[])g.Data[1];
											if (nTextureCoords < 0 | nTextureCoords > nVertices)
											{
												Interface.AddMessage(MessageType.Error, false, "nTextureCoords does not reference valid vertices in MeshTextureCoords in Mesh in x object file " + FileName);
												return false;
											}
											for (int k = 0; k < nTextureCoords; k++)
											{
												if (textureCoords[k].Name != "Coords2d")
												{
													Interface.AddMessage(MessageType.Error, false, "textureCoords[" + k.ToString(Culture) + "] is expected to be a Coords2d in MeshTextureCoords in Mesh in x object file " + FileName);
													return false;
												}
												else if (textureCoords[k].Data.Length != 2)
												{
													Interface.AddMessage(MessageType.Error, false, "textureCoords[" + k.ToString(Culture) + "] is expected to have 2 arguments in MeshTextureCoords in Mesh in x object file " + FileName);
													return false;
												}
												else if (!(textureCoords[k].Data[0] is double))
												{
													Interface.AddMessage(MessageType.Error, false, "u is expected to be a float in textureCoords[" + k.ToString(Culture) + "] in MeshTextureCoords in Mesh in x object file " + FileName);
													return false;
												}
												else if (!(textureCoords[k].Data[1] is double))
												{
													Interface.AddMessage(MessageType.Error, false, "v is expected to be a float in textureCoords[" + k.ToString(Culture) + "] in MeshTextureCoords in Mesh in x object file " + FileName);
													return false;
												}
												double u = (double)textureCoords[k].Data[0];
												double v = (double)textureCoords[k].Data[1];
												Vertices[k].TextureCoordinates = new Vector2((float)u, (float)v);
											}
										}
										break;
									case "MeshVertexColors":
										if (g.Data.Length == 0)
										{
											continue;
										}
										if (g.Data.Length != 2)
										{
											Interface.AddMessage(MessageType.Error, false, "MeshVertexColors is expected to have 2 arguments in Mesh in x object file " + FileName);
											return false;
										}
										else if (!(g.Data[0] is int))
										{
											Interface.AddMessage(MessageType.Error, false, "nVertexColors is expected to be a DWORD in MeshTextureCoords in Mesh in x object file " + FileName);
											return false;
										}
										else if (!(g.Data[1] is Structure[]))
										{
											Interface.AddMessage(MessageType.Error, false, "vertexColors[nVertexColors] is expected to be a Structure array in MeshVertexColors in Mesh in x object file " + FileName);
											return false;
										}

										Structure[] structures = g.Data[1] as Structure[];
										for (int k = 0; k < structures.Length; k++)
										{
											if (!(structures[k].Data[0] is int))
											{
												//Message: Expected to be vertex index
												continue;
											}
											int idx = (int)structures[k].Data[0];
											if (!(structures[k].Data[1] is Structure))
											{
												Interface.AddMessage(MessageType.Error, false, "vertexColors[" + idx + "] is expected to be a ColorRGBA array in MeshVertexColors in Mesh in x object file " + FileName);
												continue;
											}

											Structure colorStructure = structures[k].Data[1] as Structure;
											if (colorStructure.Data.Length != 4)
											{
												Interface.AddMessage(MessageType.Error, false, "vertexColors[" + idx + "] is expected to have 4 arguments in MeshVertexColors in Mesh in x object file " + FileName);
												continue;
											}
											if (!(colorStructure.Data[0] is double))
											{
												Interface.AddMessage(MessageType.Error, false, "R is expected to be a float in MeshVertexColors[" + k.ToString(Culture) + "] in in Mesh in x object file " + FileName);
												continue;
											}
											if (!(colorStructure.Data[1] is double))
											{
												Interface.AddMessage(MessageType.Error, false, "G is expected to be a float in MeshVertexColors[" + k.ToString(Culture) + "] in in Mesh in x object file " + FileName);
												continue;
											}
											if (!(colorStructure.Data[2] is double))
											{
												Interface.AddMessage(MessageType.Error, false, "B is expected to be a float in MeshVertexColors[" + k.ToString(Culture) + "] in in Mesh in x object file " + FileName);
												continue;
											}
											if (!(colorStructure.Data[3] is double))
											{
												Interface.AddMessage(MessageType.Error, false, "A is expected to be a float in MeshVertexColors[" + k.ToString(Culture) + "] in in Mesh in x object file " + FileName);
												continue;
											}
											
											double red = (double)colorStructure.Data[0], green = (double)colorStructure.Data[1], blue = (double)colorStructure.Data[2], alpha = (double)colorStructure.Data[3];

											OpenBveApi.Colors.Color128 c = new Color128((float)red, (float)green, (float)blue, (float)alpha);
											Vertices[idx] = new ColoredVertex((Vertex)Vertices[idx], c);
										}
										break;
									case "MeshNormals":
										{
											// meshnormals
											if (g.Data.Length != 4)
											{
												Interface.AddMessage(MessageType.Error, false, "MeshNormals is expected to have 4 arguments in Mesh in x object file " + FileName);
												return false;
											}
											else if (!(g.Data[0] is int))
											{
												Interface.AddMessage(MessageType.Error, false, "nNormals is expected to be a DWORD in MeshNormals in Mesh in x object file " + FileName);
												return false;
											}
											else if (!(g.Data[1] is Structure[]))
											{
												Interface.AddMessage(MessageType.Error, false, "normals is expected to be a Vector array in MeshNormals in Mesh in x object file " + FileName);
												return false;
											}
											else if (!(g.Data[2] is int))
											{
												Interface.AddMessage(MessageType.Error, false, "nFaceNormals is expected to be a DWORD in MeshNormals in Mesh in x object file " + FileName);
												return false;
											}
											else if (!(g.Data[3] is Structure[]))
											{
												Interface.AddMessage(MessageType.Error, false, "faceNormals is expected to be a MeshFace array in MeshNormals in Mesh in x object file " + FileName);
												return false;
											}
											int nNormals = (int)g.Data[0];
											if (nNormals < 0)
											{
												Interface.AddMessage(MessageType.Error, false, "nNormals is expected to be non-negative in MeshNormals in Mesh in x object file " + FileName);
												return false;
											}
											Structure[] normals = (Structure[])g.Data[1];
											if (nNormals != normals.Length)
											{
												Interface.AddMessage(MessageType.Error, false, "nNormals does not match with the length of array normals in MeshNormals in Mesh in x object file " + FileName);
												return false;
											}
											int nFaceNormals = (int)g.Data[2];
											if (nFaceNormals < 0 | nFaceNormals > nFaces)
											{
												Interface.AddMessage(MessageType.Error, false, "nNormals does not reference valid vertices in MeshNormals in Mesh in x object file " + FileName);
												return false;
											}
											Structure[] faceNormals = (Structure[])g.Data[3];
											if (nFaceNormals != faceNormals.Length)
											{
												Interface.AddMessage(MessageType.Error, false, "nFaceNormals does not match with the length of array faceNormals in MeshNormals in Mesh in x object file " + FileName);
												return false;
											}
											// collect normals
											Vector3[] Normals = new Vector3[nNormals];
											for (int k = 0; k < nNormals; k++)
											{
												if (normals[k].Name != "Vector")
												{
													Interface.AddMessage(MessageType.Error, false, "normals[" + k.ToString(Culture) + "] is expected to be of template Vertex in MeshNormals in Mesh in x object file " + FileName);
													return false;
												}
												else if (normals[k].Data.Length != 3)
												{
													Interface.AddMessage(MessageType.Error, false, "normals[" + k.ToString(Culture) + "] is expected to have 3 arguments in MeshNormals in Mesh in x object file " + FileName);
													return false;
												}
												else if (!(normals[k].Data[0] is double))
												{
													Interface.AddMessage(MessageType.Error, false, "x is expected to be a float in normals[" + k.ToString(Culture) + "] in MeshNormals in Mesh in x object file " + FileName);
													return false;
												}
												else if (!(normals[k].Data[1] is double))
												{
													Interface.AddMessage(MessageType.Error, false, "y is expected to be a float in normals[" + k.ToString(Culture) + " ]in MeshNormals in Mesh in x object file " + FileName);
													return false;
												}
												else if (!(normals[k].Data[2] is double))
												{
													Interface.AddMessage(MessageType.Error, false, "z is expected to be a float in normals[" + k.ToString(Culture) + "] in MeshNormals in Mesh in x object file " + FileName);
													return false;
												}
												double x = (double)normals[k].Data[0];
												double y = (double)normals[k].Data[1];
												double z = (double)normals[k].Data[2];
												Normals[k] = new Vector3((float)x, (float)y, (float)z);
												Normals[k].Normalize();
											}
											// collect faces
											for (int k = 0; k < nFaceNormals; k++)
											{
												if (faceNormals[k].Name != "MeshFace")
												{
													Interface.AddMessage(MessageType.Error, false, "faceNormals[" + k.ToString(Culture) + "] is expected to be of template MeshFace in MeshNormals in Mesh in x object file " + FileName);
													return false;
												}
												else if (faceNormals[k].Data.Length != 2)
												{
													Interface.AddMessage(MessageType.Error, false, "faceNormals[" + k.ToString(Culture) + "] is expected to have 2 arguments in MeshNormals in Mesh in x object file " + FileName);
													return false;
												}
												else if (!(faceNormals[k].Data[0] is int))
												{
													Interface.AddMessage(MessageType.Error, false, "nFaceVertexIndices is expected to be a DWORD in faceNormals[" + k.ToString(Culture) + "] in MeshNormals in Mesh in x object file " + FileName);
													return false;
												}
												else if (!(faceNormals[k].Data[1] is int[]))
												{
													Interface.AddMessage(MessageType.Error, false, "faceVertexIndices[nFaceVertexIndices] is expected to be a DWORD array in faceNormals[" + k.ToString(Culture) + "] in MeshNormals in Mesh in x object file " + FileName);
													return false;
												}
												int nFaceVertexIndices = (int)faceNormals[k].Data[0];
												if (nFaceVertexIndices < 0 | nFaceVertexIndices > Faces[k].Length)
												{
													Interface.AddMessage(MessageType.Error, false, "nFaceVertexIndices does not reference a valid vertex in MeshFace in MeshNormals in Mesh in x object file " + FileName);
													return false;
												}
												int[] faceVertexIndices = (int[])faceNormals[k].Data[1];
												if (nFaceVertexIndices != faceVertexIndices.Length)
												{
													Interface.AddMessage(MessageType.Error, false, "nFaceVertexIndices does not match with the length of array faceVertexIndices in faceNormals[" + k.ToString(Culture) + "] in MeshFace in MeshNormals in Mesh in x object file " + FileName);
													return false;
												}
												for (int l = 0; l < nFaceVertexIndices; l++)
												{
													if (faceVertexIndices[l] < 0 | faceVertexIndices[l] >= nNormals)
													{
														Interface.AddMessage(MessageType.Error, false, "faceVertexIndices[" + l.ToString(Culture) + "] does not reference a valid normal in faceNormals[" + k.ToString(Culture) + "] in MeshFace in MeshNormals in Mesh in x object file " + FileName);
														return false;
													}
													FaceNormals[k][l] = Normals[faceVertexIndices[l]];
												}
											}
										}
										break;
									default:
										// unknown
										Interface.AddMessage(MessageType.Warning, false, "Unsupported template " + g.Name + " encountered in Mesh in x object file " + FileName);
										break;

								}
							}
							// default material
							if (Materials.Length == 0)
							{
								Materials = new Material[1];
								Materials[0].faceColor = Color32.White;
								Materials[0].emissiveColor = Color24.Black;
								Materials[0].specularColor = Color24.Black;
								Materials[0].TextureFilename = null;
								for (int j = 0; j < nFaces; j++)
								{
									FaceMaterials[j] = 0;
								}
							}
							// create mesh
							int mf = Object.Mesh.Faces.Length;
							int mm = Object.Mesh.Materials.Length;
							int mv = Object.Mesh.Vertices.Length;
							Array.Resize<World.MeshFace>(ref Object.Mesh.Faces, mf + nFaces);
							Array.Resize<World.MeshMaterial>(ref Object.Mesh.Materials, mm + Materials.Length);
							Array.Resize<VertexTemplate>(ref Object.Mesh.Vertices, mv + Vertices.Length);
							for (int j = 0; j < Materials.Length; j++)
							{
								bool emissive = Materials[j].emissiveColor.R != 0 | Materials[j].emissiveColor.G != 0 | Materials[j].emissiveColor.B != 0;
								bool transparent;
								if (Materials[j].TextureFilename != null)
								{
									OpenGlTextureWrapMode WrapX, WrapY;
									if (ForceTextureRepeatX)
									{
										WrapX = OpenGlTextureWrapMode.RepeatRepeat;
									}
									else
									{
										WrapX = OpenGlTextureWrapMode.ClampClamp;
									}
									if (ForceTextureRepeatY)
									{
										WrapY = OpenGlTextureWrapMode.RepeatRepeat;
									}
									else
									{
										WrapY = OpenGlTextureWrapMode.ClampClamp;
									}
									if (WrapX != OpenGlTextureWrapMode.RepeatRepeat | WrapY != OpenGlTextureWrapMode.RepeatRepeat)
									{
										for (int k = 0; k < nFaces; k++)
										{
											for (int h = 0; h < Faces[k].Length; h++)
											{
												if (Vertices[Faces[k][h]].TextureCoordinates.X < 0.0 | Vertices[Faces[k][h]].TextureCoordinates.X > 1.0)
												{
													WrapX = OpenGlTextureWrapMode.RepeatRepeat;
												}
												if (Vertices[Faces[k][h]].TextureCoordinates.Y < 0.0 | Vertices[Faces[k][h]].TextureCoordinates.Y > 1.0)
												{
													WrapY = OpenGlTextureWrapMode.RepeatRepeat;
												}
											}
										}
									}
									int tday = TextureManager.RegisterTexture(Materials[j].TextureFilename, new Color24(0, 0, 0), 1, TextureManager.TextureLoadMode.Normal, WrapX, WrapY, LoadMode != ObjectLoadMode.Normal, 0, 0, 0, 0);
									Object.Mesh.Materials[mm + j].DaytimeTextureIndex = tday;
									transparent = true;
								}
								else
								{
									Object.Mesh.Materials[mm + j].DaytimeTextureIndex = -1;
									transparent = false;
								}
								Object.Mesh.Materials[mm + j].Flags = (byte)((transparent ? World.MeshMaterial.TransparentColorMask : 0) | (emissive ? World.MeshMaterial.EmissiveColorMask : 0));
								Object.Mesh.Materials[mm + j].Color = Materials[j].faceColor;
								Object.Mesh.Materials[mm + j].TransparentColor = Color24.Black;
								Object.Mesh.Materials[mm + j].EmissiveColor = Materials[j].emissiveColor;
								Object.Mesh.Materials[mm + j].NighttimeTextureIndex = -1;
								Object.Mesh.Materials[mm + j].BlendMode = World.MeshMaterialBlendMode.Normal;
								Object.Mesh.Materials[mm + j].GlowAttenuationData = 0;
							}
							for (int j = 0; j < nFaces; j++)
							{
								Object.Mesh.Faces[mf + j].Material = (ushort)FaceMaterials[j];
								Object.Mesh.Faces[mf + j].Vertices = new World.MeshFaceVertex[Faces[j].Length];
								for (int k = 0; k < Faces[j].Length; k++)
								{
									Object.Mesh.Faces[mf + j].Vertices[k] = new World.MeshFaceVertex(mv + Faces[j][k], FaceNormals[j][k]);
								}
							}
							for (int j = 0; j < Vertices.Length; j++)
							{
								Object.Mesh.Vertices[mv + j] = Vertices[j];
							}
							break;
						}
					case "Header":
						break;
					default:
						// unknown
						if (f.Name == "Material" && f.Key != String.Empty)
						{
							Array.Resize(ref LoadedMaterials, LoadedMaterials.Length + 1);
							LoadedMaterials[LoadedMaterials.Length - 1] = f;
							break;
						}
						Interface.AddMessage(MessageType.Warning, false, "Unsupported template " + f.Name + " encountered in x object file " + FileName);
						break;
				}
			}
			// return
			Object.Mesh.CreateNormals();
			return true;
		}

	}
}
