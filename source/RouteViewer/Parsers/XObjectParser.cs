using System;
using System.IO;
using System.IO.Compression;

namespace OpenBve {
	internal static class XObjectParser {

		// read object
		internal static ObjectManager.StaticObject ReadObject(string FileName, System.Text.Encoding Encoding, ObjectManager.ObjectLoadMode LoadMode, bool ForceTextureRepeatX, bool ForceTextureRepeatY) {
			byte[] Data = System.IO.File.ReadAllBytes(FileName);
			if (Data.Length < 16 || Data[0] != 120 | Data[1] != 111 | Data[2] != 102 | Data[3] != 32) {
				// not an x object
				Interface.AddMessage(Interface.MessageType.Error, false, "Invalid X object file encountered in " + FileName);
				return null;
			}
			if (Data[4] != 48 | Data[5] != 51 | Data[6] != 48 | Data[7] != 50 & Data[7] != 51) {
				// unrecognized version
				System.Text.ASCIIEncoding Ascii = new System.Text.ASCIIEncoding();
				string s = new string(Ascii.GetChars(Data, 4, 4));
				Interface.AddMessage(Interface.MessageType.Error, false, "Unsupported X object file version " + s + " encountered in " + FileName);
			}
			// floating-point format
			int FloatingPointSize;
			if (Data[12] == 48 & Data[13] == 48 & Data[14] == 51 & Data[15] == 50) {
				FloatingPointSize = 32;
			} else if (Data[12] == 48 & Data[13] == 48 & Data[14] == 54 & Data[15] == 52) {
				FloatingPointSize = 64;
			} else {
				Interface.AddMessage(Interface.MessageType.Error, false, "Unsupported floating point format encountered in X object file " + FileName);
				return null;
			}
			// supported floating point format
			if (Data[8] == 116 & Data[9] == 120 & Data[10] == 116 & Data[11] == 32) {
				// textual flavor
				return LoadTextualX(FileName, System.IO.File.ReadAllText(FileName), Encoding, LoadMode, ForceTextureRepeatX, ForceTextureRepeatY);
			} else if (Data[8] == 98 & Data[9] == 105 & Data[10] == 110 & Data[11] == 32) {
				// binary flavor
				return LoadBinaryX(FileName, Data, 16, Encoding, FloatingPointSize, LoadMode, ForceTextureRepeatX, ForceTextureRepeatY);
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
					Interface.AddMessage(Interface.MessageType.Error, false, "An unexpected error occured (" + ex.Message + ") while attempting to decompress the binary X object file encountered in " + FileName);
					return null;
				}
				#endif
			} else if (Data[8] == 98 & Data[9] == 122 & Data[10] == 105 & Data[11] == 112) {
				// compressed binary flavor
				#if !DEBUG
				try {
					#endif
					byte[] Uncompressed = Decompress(Data);
					return LoadBinaryX(FileName, Uncompressed, 0, Encoding, FloatingPointSize, LoadMode, ForceTextureRepeatX, ForceTextureRepeatY);
					#if !DEBUG
				} catch (Exception ex) {
					Interface.AddMessage(Interface.MessageType.Error, false, "An unexpected error occured (" + ex.Message + ") while attempting to decompress the binary X object file encountered in " + FileName);
					return null;
				}
				#endif
			} else {
				// unsupported flavor
				Interface.AddMessage(Interface.MessageType.Error, false, "Unsupported X object file encountered in " + FileName);
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
			internal Template(string Name, string[] Members) {
				this.Name = Name;
				this.Members = Members;
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
			new Template("MeshNormals", new string[] { "DWORD", "Vector[0]", "DWORD", "MeshFace[2]" })
		};

		// data
		private class Structure {
			internal string Name;
			internal object[] Data;
			internal Structure(string Name, object[] Data) {
				this.Name = Name;
				this.Data = Data;
			}
		}

		// get template
		private static Template GetTemplate(string Name) {
			for (int i = 0; i < Templates.Length; i++) {
				if (Templates[i].Name == Name) {
					return Templates[i];
				}
			}
			return new Template(Name, new string[] { "[???]" });
		}

		// ================================

		// load textual x
		private static ObjectManager.StaticObject LoadTextualX(string FileName, string Text, System.Text.Encoding Encoding, ObjectManager.ObjectLoadMode LoadMode, bool ForceTextureRepeatX, bool ForceTextureRepeatY) {
			// load
			string[] Lines = Text.Replace("\u000D\u000A", "\u2028").Split(new char[] { '\u000A', '\u000C', '\u000D', '\u0085', '\u2028', '\u2029' }, StringSplitOptions.None);
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
			}
			// strip away header
			if (Lines.Length == 0 || Lines[0].Length < 16) {
				Interface.AddMessage(Interface.MessageType.Error, false, "The textual X object file is invalid at line 1 in " + FileName);
				return null;
			}
			Lines[0] = Lines[0].Substring(16);
			// join lines
			System.Text.StringBuilder Builder = new System.Text.StringBuilder();
			for (int i = 0; i < Lines.Length; i++) {
				Builder.Append(Lines[i]);
			}
			string Content = Builder.ToString();
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
					return true;
			}

			return false;
		}

		// read textual template
		private static bool ReadTextualTemplate(string FileName, string Content, ref int Position, Template Template, bool Inline, out Structure Structure) {
			Structure = new Structure(Template.Name, new object[] { });
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
								if (!ReadTextualTemplate(FileName, Content, ref Position, GetTemplate(s), false, out o)) {
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
								if (!ReadTextualTemplate(FileName, Content, ref Position, GetTemplate(s), false, out o)) {
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
									Interface.AddMessage(Interface.MessageType.Error, false, "Unexpected closing brace encountered in inlined template " + Template.Name + " in textual X object file " + FileName);
									return false;
								} else {
									Position++;
									return true;
								}
							} else if (Content[Position] == ',') {
								Interface.AddMessage(Interface.MessageType.Error, false, "Unexpected comma encountered in template " + Template.Name + " in textual X object file " + FileName);
								return false;
							} else if (Content[Position] == ';') {
								if (Inline) {
									Position++;
									return true;
								} else {
									Interface.AddMessage(Interface.MessageType.Error, false, "Unexpected semicolon encountered in template " + Template.Name + " in textual X object file " + FileName);
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
							Interface.AddMessage(Interface.MessageType.Error, false, "The internal format description for a template array is invalid in template " + Template.Name + " in textual X object file " + FileName);
							return false;
						}
						if (h < 0 || h >= Structure.Data.Length || !(Structure.Data[h] is int)) {
							Interface.AddMessage(Interface.MessageType.Error, false, "The internal format description for a template array is invalid in template " + Template.Name + " in textual X object file " + FileName);
							return false;
						}
						h = (int)Structure.Data[h];
					} else {
						Interface.AddMessage(Interface.MessageType.Error, false, "The internal format description for a template array is invalid in template " + Template.Name + " in textual X object file " + FileName);
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
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid character encountered while processing an array in template " + Template.Name + " in textual X object file " + FileName);
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
										Interface.AddMessage(Interface.MessageType.Error, false, "Invalid character encountered while processing a DWORD array in template " + Template.Name + " in textual X object file " + FileName);
										return false;
									} else if (Content[Position] == ',') {
										if (k == h - 1) {
											Interface.AddMessage(Interface.MessageType.Error, false, "Invalid character encountered while processing a DWORD array in template " + Template.Name + " in textual X object file " + FileName);
											return false;
										}
										break;
									} else if (Content[Position] == ';') {
										if (k != h - 1) {
											Interface.AddMessage(Interface.MessageType.Error, false, "Invalid character encountered while processing a DWORD array in template " + Template.Name + " in textual X object file " + FileName);
											return false;
										}
										break;
									} Position++;
								} if (Position == Content.Length) {
									Interface.AddMessage(Interface.MessageType.Error, false, "DWORD array was not terminated at the end of the file in template " + Template.Name + " in textual X object file " + FileName);
									return false;
								}
								string s = Content.Substring(i, Position - i);
								Position++;
								i = Position;
								if (!int.TryParse(s, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out o[k])) {
									Interface.AddMessage(Interface.MessageType.Error, false, "DWORD could not be parsed in array in template " + Template.Name + " in textual X object file " + FileName);
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
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid character encountered while processing an array in template " + Template.Name + " in textual X object file " + FileName);
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
										Interface.AddMessage(Interface.MessageType.Error, false, "Invalid character encountered while processing a float array in template " + Template.Name + " in textual X object file " + FileName);
										return false;
									} else if (Content[Position] == ',') {
										if (k == h - 1) {
											Interface.AddMessage(Interface.MessageType.Error, false, "Invalid character encountered while processing a float array in template " + Template.Name + " in textual X object file " + FileName);
											return false;
										}
										break;
									} else if (Content[Position] == ';') {
										if (k != h - 1) {
											Interface.AddMessage(Interface.MessageType.Error, false, "Invalid character encountered while processing a float array in template " + Template.Name + " in textual X object file " + FileName);
											return false;
										}
										break;
									} Position++;
								} if (Position == Content.Length) {
									Interface.AddMessage(Interface.MessageType.Error, false, "float array was not terminated at the end of the file in template " + Template.Name + " in textual X object file " + FileName);
									return false;
								}
								string s = Content.Substring(i, Position - i);
								Position++;
								i = Position;
								if (!double.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out o[k])) {
									Interface.AddMessage(Interface.MessageType.Error, false, "float could not be parsed in array in template " + Template.Name + " in textual X object file " + FileName);
								}
							}
						}
						Array.Resize<object>(ref Structure.Data, Structure.Data.Length + 1);
						Structure.Data[Structure.Data.Length - 1] = o;
					} else {
						// non-primitive array
						Template t = GetTemplate(r);
						Structure[] o = new Structure[h];
						if (h == 0) {
							// empty array
							while (Position < Content.Length) {
								if (Content[Position] == ';') {
									Position++;
									break;
								} else if (!char.IsWhiteSpace(Content, Position)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid character encountered while processing an array in template " + Template.Name + " in textual X object file " + FileName);
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
											Interface.AddMessage(Interface.MessageType.Error, false, "Invalid character encountered while processing an array in template " + Template.Name + " in textual X object file " + FileName);
											return false;
										} else {
											Position++;
										}
									} if (Position == Content.Length) {
										Interface.AddMessage(Interface.MessageType.Error, false, "Array was not continued at the end of the file in template " + Template.Name + " in textual X object file " + FileName);
										return false;
									}
								} else {
									// last element
									while (Position < Content.Length) {
										if (Content[Position] == ';') {
											Position++;
											break;
										} else if (!char.IsWhiteSpace(Content, Position)) {
											Interface.AddMessage(Interface.MessageType.Error, false, "Invalid character encountered while processing an array in template " + Template.Name + " in textual X object file " + FileName);
											return false;
										} else {
											Position++;
										}
									} if (Position == Content.Length) {
										Interface.AddMessage(Interface.MessageType.Error, false, "Array was not terminated at the end of the file in template " + Template.Name + " in textual X object file " + FileName);
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
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid character encountered while processing a DWORD in template " + Template.Name + " in textual X object file " + FileName);
									return false;
								} else if (Content[Position] == ';') {
									string s = Content.Substring(i, Position - i).Trim();
									int a; if (!int.TryParse(s, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out a)) {
										Interface.AddMessage(Interface.MessageType.Error, false, "DWORD could not be parsed in template " + Template.Name + " in textual X object file " + FileName);
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
								if (Content[Position] == '{' | Content[Position] == '}' | Content[Position] == ',' | Content[Position] == '"') {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid character encountered while processing a DWORD in template " + Template.Name + " in textual X object file " + FileName);
									return false;
								} else if (Content[Position] == ';') {
									string s = Content.Substring(i, Position - i).Trim();
									double a; if (!double.TryParse(s, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out a)) {
										Interface.AddMessage(Interface.MessageType.Error, false, "float could not be parsed in template " + Template.Name + " in textual X object file " + FileName);
										return false;
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
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid character encountered while processing a string in template " + Template.Name + " in textual X object file " + FileName);
									return false;
								} else {
									Position++;
								}
							} if (Position >= Content.Length) {
								Interface.AddMessage(Interface.MessageType.Error, false, "Unexpected end of file encountered while processing a string in template " + Template.Name + " in textual X object file " + FileName);
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
								Interface.AddMessage(Interface.MessageType.Error, false, "Unexpected end of file encountered while processing a string in template " + Template.Name + " in textual X object file " + FileName);
								return false;
							}
							string t = Content.Substring(i, Position - i - 1);
							while (Position < Content.Length) {
								if (Content[Position] == ';') {
									Position++;
									break;
								} else if (!char.IsWhiteSpace(Content, Position)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Invalid character encountered while processing a string in template " + Template.Name + " in textual X object file " + FileName);
									return false;
								} else {
									Position++;
								}
							} if (Position >= Content.Length) {
								Interface.AddMessage(Interface.MessageType.Error, false, "Unexpected end of file encountered while processing a string in template " + Template.Name + " in textual X object file " + FileName);
								return false;
							}
							Array.Resize<object>(ref Structure.Data, Structure.Data.Length + 1);
							Structure.Data[Structure.Data.Length - 1] = t;
							i = Position;
							break;
						default:
							{
								Structure o;
								if (!ReadTextualTemplate(FileName, Content, ref Position, GetTemplate(Template.Members[m]), true, out o)) {
									return false;
								}
								while (Position < Content.Length) {
									if (Content[Position] == ';') {
										Position++;
										break;
									} else if (!char.IsWhiteSpace(Content, Position)) {
										Interface.AddMessage(Interface.MessageType.Error, false, "Invalid character encountered while processing an inlined template in template " + Template.Name + " in textual X object file " + FileName);
										return false;
									} else {
										Position++;
									}
								} if (Position >= Content.Length) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Unexpected end of file encountered while processing an inlined template in template " + Template.Name + " in textual X object file " + FileName);
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
					while (Position < Content.Length) {
						if (Content[Position] == '}') {
							Position++;
							break;
						} else if (!char.IsWhiteSpace(Content, Position)) {
							Interface.AddMessage(Interface.MessageType.Error, false, "Invalid character encountered in template " + Template.Name + " in textual X object file " + FileName);
							return false;
						} else {
							Position++;
						}
					} if (Position >= Content.Length) {
						Interface.AddMessage(Interface.MessageType.Error, false, "Unexpected end of file encountered in template " + Template.Name + " in textual X object file " + FileName);
						return false;
					}
					return true;
				}
			} else {
				if (q) {
					Interface.AddMessage(Interface.MessageType.Error, false, "Quotation mark not closed at the end of the file in template " + Template.Name + " in textual X object file " + FileName);
					return false;
				} else if (Template.Name.Length != 0) {
					Interface.AddMessage(Interface.MessageType.Error, false, "Unexpected end of file encountered in template " + Template.Name + " in textual X object file " + FileName);
					return false;
				} else {
					return true;
				}
			}
		}

		// ================================

		// load binary x
		private static ObjectManager.StaticObject LoadBinaryX(string FileName, byte[] Data, int StartingPosition, System.Text.Encoding Encoding, int FloatingPointSize, ObjectManager.ObjectLoadMode LoadMode, bool ForceTextureRepeatX, bool ForceTextureRepeatY) {
			// parse file
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
				Interface.AddMessage(Interface.MessageType.Error, false, "Unhandled error (" + ex.Message + ") encountered in binary X object file " + FileName);
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
			Structure = new Structure(Template.Name, new object[] { });
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			System.Text.ASCIIEncoding Ascii = new System.Text.ASCIIEncoding();
			int m; for (m = 0; m < Template.Members.Length; m++) {
				if (Template.Members[m] == "[???]") {
					// unknown template
					int Level = 0;
					if (Cache.IntegersRemaining != 0) {
						Interface.AddMessage(Interface.MessageType.Error, false, "An integer list was not depleted at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
					} else if (Cache.FloatsRemaining != 0) {
						Interface.AddMessage(Interface.MessageType.Error, false, "A float list was not depleted at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
					}
					short Token = Reader.ReadInt16();
					switch (Token) {
						case TOKEN_NAME:
							{
								Level++;
								int n = Reader.ReadInt32();
								if (n < 1) {
									Interface.AddMessage(Interface.MessageType.Error, false, "count is invalid in TOKEN_NAME at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
									return false;
								}
								Reader.BaseStream.Position += n;
								Token = Reader.ReadInt16();
								if (Token != TOKEN_OBRACE) {
									Interface.AddMessage(Interface.MessageType.Error, false, "TOKEN_OBRACE expected at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
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
									Interface.AddMessage(Interface.MessageType.Error, false, "count is invalid in TOKEN_INTEGER_LIST at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
									return false;
								}
								Reader.BaseStream.Position += 4 * n;
							} break;
						case TOKEN_FLOAT_LIST:
							{
								int n = Reader.ReadInt32();
								if (n < 0) {
									Interface.AddMessage(Interface.MessageType.Error, false, "count is invalid in TOKEN_FLOAT_LIST at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
									return false;
								}
								Reader.BaseStream.Position += (FloatingPointSize >> 3) * n;
							} break;
						case TOKEN_STRING:
							{
								int n = Reader.ReadInt32();
								if (n < 0) {
									Interface.AddMessage(Interface.MessageType.Error, false, "count is invalid in TOKEN_STRING at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
									return false;
								}
								Reader.BaseStream.Position += n;
								Token = Reader.ReadInt16();
								if (Token != TOKEN_COMMA & Token != TOKEN_SEMICOLON) {
									Interface.AddMessage(Interface.MessageType.Error, false, "TOKEN_COMMA or TOKEN_SEMICOLON expected at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
									return false;
								}
							} break;
						case TOKEN_OBRACE:
							Interface.AddMessage(Interface.MessageType.Error, false, "Unexpected token TOKEN_OBRACE encountered at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
							return false;
						case TOKEN_CBRACE:
							if (Level == 0) return true;
							Level--;
							break;
						default:
							Interface.AddMessage(Interface.MessageType.Error, false, "Unknown token encountered at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
							return false;
					} m--;
				} else if (Template.Members[m] == "[...]") {
					// any template
					if (Cache.IntegersRemaining != 0) {
						Interface.AddMessage(Interface.MessageType.Error, false, "An integer list was not depleted at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
					} else if (Cache.FloatsRemaining != 0) {
						Interface.AddMessage(Interface.MessageType.Error, false, "A float list was not depleted at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
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
								Interface.AddMessage(Interface.MessageType.Error, false, "count is invalid in TOKEN_NAME at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
								return false;
							}
							string Name = new string(Ascii.GetChars(Reader.ReadBytes(n)));
							Token = Reader.ReadInt16();
							if (Token != TOKEN_OBRACE) {
								Interface.AddMessage(Interface.MessageType.Error, false, "TOKEN_OBRACE expected at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
								return false;
							}
							Structure o;
							if (!ReadBinaryTemplate(FileName, Reader, FloatingPointSize, GetTemplate(Name), false, ref Cache, out o)) {
								return false;
							}
							Array.Resize<object>(ref Structure.Data, Structure.Data.Length + 1);
							Structure.Data[Structure.Data.Length - 1] = o;
							break;
						case TOKEN_CBRACE:
							if (Template.Name.Length == 0) {
								Interface.AddMessage(Interface.MessageType.Error, false, "Unexpected TOKEN_CBRACE encountered at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
								return false;
							}
							m++;
							break;
						default:
							Interface.AddMessage(Interface.MessageType.Error, false, "TOKEN_NAME or TOKEN_CBRACE expected at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
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
							Interface.AddMessage(Interface.MessageType.Error, false, "The internal format description for a template array is invalid in template " + Template.Name + " in binary X object file " + FileName);
							return false;
						}
						if (h < 0 || h >= Structure.Data.Length || !(Structure.Data[h] is int)) {
							Interface.AddMessage(Interface.MessageType.Error, false, "The internal format description for a template array is invalid in template " + Template.Name + " in binary X object file " + FileName);
							return false;
						}
						h = (int)Structure.Data[h];
					} else {
						Interface.AddMessage(Interface.MessageType.Error, false, "The internal format description for a template array is invalid in template " + Template.Name + " in binary X object file " + FileName);
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
								Interface.AddMessage(Interface.MessageType.Error, false, "A float list was not depleted at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
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
											Interface.AddMessage(Interface.MessageType.Error, false, "count is invalid in TOKEN_INTEGER_LIST at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
											return false;
										}
										if (n != 0) {
											Cache.Integers = new int[n];
											for (int j = 0; j < n; i++) {
												Cache.Integers[n - j - 1] = Reader.ReadInt32();
											}
											Cache.IntegersRemaining = n - 1;
											int a = Cache.Integers[Cache.IntegersRemaining];
											o[i] = a;
											break;
										}
									} else {
										Interface.AddMessage(Interface.MessageType.Error, false, "TOKEN_INTEGER or TOKEN_INTEGER_LIST expected at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
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
								Interface.AddMessage(Interface.MessageType.Error, false, "An integer list was not depleted at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
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
											Interface.AddMessage(Interface.MessageType.Error, false, "count is invalid in TOKEN_FLOAT_LIST at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
											return false;
										}
										if (n != 0) {
											Cache.Floats = new double[n];
											for (int j = 0; j < n; i++) {
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
										Interface.AddMessage(Interface.MessageType.Error, false, "TOKEN_FLOAT_LIST expected at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
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
							ReadBinaryTemplate(FileName, Reader, FloatingPointSize, GetTemplate(r), true, ref Cache, out o[i]);
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
								Interface.AddMessage(Interface.MessageType.Error, false, "A float list was not depleted at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
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
											Interface.AddMessage(Interface.MessageType.Error, false, "count is invalid in TOKEN_INTEGER_LIST at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
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
										Interface.AddMessage(Interface.MessageType.Error, false, "TOKEN_INTEGER or TOKEN_INTEGER_LIST expected at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
										return false;
									}
								}
							} break;
						case "float":
							// float expected
							if (Cache.IntegersRemaining != 0) {
								// cannot use cached integer
								Interface.AddMessage(Interface.MessageType.Error, false, "An integer list was not depleted at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
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
											Interface.AddMessage(Interface.MessageType.Error, false, "count is invalid in TOKEN_FLOAT_LIST at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
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
										Interface.AddMessage(Interface.MessageType.Error, false, "TOKEN_FLOAT_LIST expected at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
										return false;
									}
								}
							} break;
						case "string":
							{
								// string expected
								if (Cache.IntegersRemaining != 0) {
									Interface.AddMessage(Interface.MessageType.Error, false, "An integer list was not depleted at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
								} else if (Cache.FloatsRemaining != 0) {
									Interface.AddMessage(Interface.MessageType.Error, false, "A float list was not depleted at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
								}
								short Token = Reader.ReadInt16();
								if (Token == TOKEN_STRING) {
									int n = Reader.ReadInt32();
									if (n < 0) {
										Interface.AddMessage(Interface.MessageType.Error, false, "count is invalid in TOKEN_STRING at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
										return false;
									}
									string s = new string(Ascii.GetChars(Reader.ReadBytes(n)));
									Array.Resize<object>(ref Structure.Data, Structure.Data.Length + 1);
									Structure.Data[Structure.Data.Length - 1] = s;
									Token = Reader.ReadInt16();
									if (Token != TOKEN_SEMICOLON) {
										Interface.AddMessage(Interface.MessageType.Error, false, "TOKEN_SEMICOLON expected at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
										return false;
									}
								} else {
									Interface.AddMessage(Interface.MessageType.Error, false, "TOKEN_STRING expected at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
									return false;
								}
							} break;
						default:
							// inlined template expected
							Structure o;
							ReadBinaryTemplate(FileName, Reader, FloatingPointSize, GetTemplate(Template.Members[m]), true, ref Cache, out o);
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
						Interface.AddMessage(Interface.MessageType.Error, false, "TOKEN_CBRACE expected at position 0x" + Reader.BaseStream.Position.ToString("X", Culture) + " in binary X object file " + FileName);
						return false;
					}
				}
				return true;
			}
		}

		// ================================

		// structures
		private struct Material {
			internal World.ColorRGBA faceColor;
			internal World.ColorRGB specularColor;
			internal World.ColorRGB emissiveColor;
			internal string TextureFilename;
		}

		// process structure
		private static bool ProcessStructure(string FileName, Structure Structure, out ObjectManager.StaticObject Object, ObjectManager.ObjectLoadMode LoadMode, bool ForceTextureRepeatX, bool ForceTextureRepeatY) {
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			Object = new ObjectManager.StaticObject();
			Object.Mesh.Faces = new World.MeshFace[] { };
			Object.Mesh.Materials = new World.MeshMaterial[] { };
			Object.Mesh.Vertices = new World.Vertex[] { };
			// file
			for (int i = 0; i < Structure.Data.Length; i++) {
				Structure f = Structure.Data[i] as Structure;
				if (f == null) {
					Interface.AddMessage(Interface.MessageType.Error, false, "Top-level inlined arguments are invalid in x object file " + FileName);
					return false;
				}
				switch (f.Name) {
					case "Mesh":
						{
							// mesh
							if (f.Data.Length < 4) {
								Interface.AddMessage(Interface.MessageType.Error, false, "Mesh is expected to have at least 4 arguments in x object file " + FileName);
								return false;
							} else if (!(f.Data[0] is int)) {
								Interface.AddMessage(Interface.MessageType.Error, false, "nVertices is expected to be a DWORD in Mesh in x object file " + FileName);
								return false;
							} else if (!(f.Data[1] is Structure[])) {
								Interface.AddMessage(Interface.MessageType.Error, false, "vertices[nVertices] is expected to be a Vector array in Mesh in x object file " + FileName);
								return false;
							} else if (!(f.Data[2] is int)) {
								Interface.AddMessage(Interface.MessageType.Error, false, "nFaces is expected to be a DWORD in Mesh in x object file " + FileName);
								return false;
							} else if (!(f.Data[3] is Structure[])) {
								Interface.AddMessage(Interface.MessageType.Error, false, "faces[nFaces] is expected to be a MeshFace array in Mesh in x object file " + FileName);
								return false;
							}
							int nVertices = (int)f.Data[0];
							if (nVertices < 0) {
								Interface.AddMessage(Interface.MessageType.Error, false, "nVertices is expected to be non-negative in Mesh in x object file " + FileName);
								return false;
							}
							Structure[] vertices = (Structure[])f.Data[1];
							if (nVertices != vertices.Length) {
								Interface.AddMessage(Interface.MessageType.Error, false, "nVertices does not match with the length of array vertices in Mesh in x object file " + FileName);
								return false;
							}
							int nFaces = (int)f.Data[2];
							if (nFaces < 0) {
								Interface.AddMessage(Interface.MessageType.Error, false, "nFaces is expected to be non-negative in Mesh in x object file " + FileName);
								return false;
							}
							Structure[] faces = (Structure[])f.Data[3];
							if (nFaces != faces.Length) {
								Interface.AddMessage(Interface.MessageType.Error, false, "nFaces does not match with the length of array faces in Mesh in x object file " + FileName);
								return false;
							}
							// collect vertices
							World.Vertex[] Vertices = new World.Vertex[nVertices];
							for (int j = 0; j < nVertices; j++) {
								if (vertices[j].Name != "Vector") {
									Interface.AddMessage(Interface.MessageType.Error, false, "vertices[" + j.ToString(Culture) + "] is expected to be of template Vertex in Mesh in x object file " + FileName);
									return false;
								} else if (vertices[j].Data.Length != 3) {
									Interface.AddMessage(Interface.MessageType.Error, false, "vertices[" + j.ToString(Culture) + "] is expected to have 3 arguments in Mesh in x object file " + FileName);
									return false;
								} else if (!(vertices[j].Data[0] is double)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "x is expected to be a float in vertices[" + j.ToString(Culture) + "] in Mesh in x object file " + FileName);
									return false;
								} else if (!(vertices[j].Data[1] is double)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "y is expected to be a float in vertices[" + j.ToString(Culture) + "] in Mesh in x object file " + FileName);
									return false;
								} else if (!(vertices[j].Data[2] is double)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "z is expected to be a float in vertices[" + j.ToString(Culture) + "] in Mesh in x object file " + FileName);
									return false;
								}
								double x = (double)vertices[j].Data[0];
								double y = (double)vertices[j].Data[1];
								double z = (double)vertices[j].Data[2];
								Vertices[j].Coordinates = new World.Vector3D(x, y, z);
							}
							// collect faces
							int[][] Faces = new int[nFaces][];
							World.Vector3Df[][] FaceNormals = new World.Vector3Df[nFaces][];
							int[] FaceMaterials = new int[nFaces];
							for (int j = 0; j < nFaces; j++) {
								FaceMaterials[j] = -1;
							}
							for (int j = 0; j < nFaces; j++) {
								if (faces[j].Name != "MeshFace") {
									Interface.AddMessage(Interface.MessageType.Error, false, "faces[" + j.ToString(Culture) + "] is expected to be of template MeshFace in Mesh in x object file " + FileName);
									return false;
								} else if (faces[j].Data.Length != 2) {
									Interface.AddMessage(Interface.MessageType.Error, false, "face[" + j.ToString(Culture) + "] is expected to have 2 arguments in Mesh in x object file " + FileName);
									return false;
								} else if (!(faces[j].Data[0] is int)) {
									Interface.AddMessage(Interface.MessageType.Error, false, "nFaceVertexIndices is expected to be a DWORD in face[" + j.ToString(Culture) + "] in Mesh in x object file " + FileName);
									return false;
								} else if (!(faces[j].Data[1] is int[])) {
									Interface.AddMessage(Interface.MessageType.Error, false, "faceVertexIndices[nFaceVertexIndices] is expected to be a DWORD array in face[" + j.ToString(Culture) + "] in Mesh in x object file " + FileName);
									return false;
								}
								int nFaceVertexIndices = (int)faces[j].Data[0];
								if (nFaceVertexIndices < 0) {
									Interface.AddMessage(Interface.MessageType.Error, false, "nFaceVertexIndices is expected to be non-negative in MeshFace in Mesh in x object file " + FileName);
									return false;
								}
								int[] faceVertexIndices = (int[])faces[j].Data[1];
								if (nFaceVertexIndices != faceVertexIndices.Length) {
									Interface.AddMessage(Interface.MessageType.Error, false, "nFaceVertexIndices does not match with the length of array faceVertexIndices in face[" + j.ToString(Culture) + "] in Mesh in x object file " + FileName);
									return false;
								}
								Faces[j] = new int[nFaceVertexIndices];
								FaceNormals[j] = new World.Vector3Df[nFaceVertexIndices];
								for (int k = 0; k < nFaceVertexIndices; k++) {
									if (faceVertexIndices[k] < 0 | faceVertexIndices[k] >= nVertices) {
										Interface.AddMessage(Interface.MessageType.Error, false, "faceVertexIndices[" + k.ToString(Culture) + "] does not reference a valid vertex in face[" + j.ToString(Culture) + "] in Mesh in x object file " + FileName);
										return false;
									}
									Faces[j][k] = faceVertexIndices[k];
									FaceNormals[j][k] = new World.Vector3Df(0.0f, 0.0f, 0.0f);
								}
							}
							// collect additional templates
							Material[] Materials = new Material[] { };
							for (int j = 4; j < f.Data.Length; j++) {
								Structure g = f.Data[j] as Structure;
								if (g == null) {
									Interface.AddMessage(Interface.MessageType.Error, false, "Unexpected inlined argument encountered in Mesh in x object file " + FileName);
									return false;
								}
								switch (g.Name) {
									case "MeshMaterialList":
										{
											// meshmateriallist
											if (g.Data.Length < 3) {
												Interface.AddMessage(Interface.MessageType.Error, false, "MeshMaterialList is expected to have at least 3 arguments in Mesh in x object file " + FileName);
												return false;
											} else if (!(g.Data[0] is int)) {
												Interface.AddMessage(Interface.MessageType.Error, false, "nMaterials is expected to be a DWORD in MeshMaterialList in Mesh in x object file " + FileName);
												return false;
											} else if (!(g.Data[1] is int)) {
												Interface.AddMessage(Interface.MessageType.Error, false, "nFaceIndexes is expected to be a DWORD in MeshMaterialList in Mesh in x object file " + FileName);
												return false;
											} else if (!(g.Data[2] is int[])) {
												Interface.AddMessage(Interface.MessageType.Error, false, "faceIndexes[nFaceIndexes] is expected to be a DWORD array in MeshMaterialList in Mesh in x object file " + FileName);
												return false;
											}
											int nMaterials = (int)g.Data[0];
											if (nMaterials < 0) {
												Interface.AddMessage(Interface.MessageType.Error, false, "nMaterials is expected to be non-negative in MeshMaterialList in Mesh in x object file " + FileName);
												return false;
											}
											int nFaceIndexes = (int)g.Data[1];
											if (nFaceIndexes < 0) {
												Interface.AddMessage(Interface.MessageType.Error, false, "nFaceIndexes is expected to be non-negative in MeshMaterialList in Mesh in x object file " + FileName);
												return false;
											} else if (nFaceIndexes > nFaces) {
												Interface.AddMessage(Interface.MessageType.Error, false, "nFaceIndexes does not reference valid faces in MeshMaterialList in Mesh in x object file " + FileName);
												return false;
											}
											int[] faceIndexes = (int[])g.Data[2];
											if (nFaceIndexes != faceIndexes.Length) {
												Interface.AddMessage(Interface.MessageType.Error, false, "nFaceIndexes does not match with the length of array faceIndexes in face[" + j.ToString(Culture) + "] in Mesh in x object file " + FileName);
												return false;
											}
											for (int k = 0; k < nFaceIndexes; k++) {
												if (faceIndexes[k] < 0 | faceIndexes[k] >= nMaterials) {
													Interface.AddMessage(Interface.MessageType.Error, false, "faceIndexes[" + k.ToString(Culture) + "] does not reference a valid Material template in MeshMaterialList in Mesh in x object file " + FileName);
													return false;
												}
											}
											// collect material templates
											int mn = Materials.Length;
											Array.Resize<Material>(ref Materials, mn + nMaterials);
											for (int k = 0; k < nMaterials; k++) {
												Materials[mn + k].faceColor = new World.ColorRGBA(255, 255, 255, 255);
												Materials[mn + k].specularColor = new World.ColorRGB(0, 0, 0);
												Materials[mn + k].emissiveColor = new World.ColorRGB(0, 0, 0);
												Materials[mn + k].TextureFilename = null;
											}
											int MaterialIndex = mn;
											for (int k = 3; k < g.Data.Length; k++) {
												Structure h = g.Data[k] as Structure;
												if (h == null) {
													Interface.AddMessage(Interface.MessageType.Error, false, "Unexpected inlined argument encountered in MeshMaterialList in Mesh in x object file " + FileName);
													return false;
												} else if (h.Name != "Material") {
													Interface.AddMessage(Interface.MessageType.Error, false, "Material template expected in MeshMaterialList in Mesh in x object file " + FileName);
													return false;
												} else {
													// material
													if (h.Data.Length < 4) {
														Interface.AddMessage(Interface.MessageType.Error, false, "Material is expected to have at least 4 arguments in Material in MeshMaterialList in Mesh in x object file " + FileName);
														return false;
													} else if (!(h.Data[0] is Structure)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "faceColor is expected to be a ColorRGBA in Material in MeshMaterialList in Mesh in x object file " + FileName);
														return false;
													} else if (!(h.Data[1] is double)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "power is expected to be a float in Material in MeshMaterialList in Mesh in x object file " + FileName);
														return false;
													} else if (!(h.Data[2] is Structure)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "specularColor is expected to be a ColorRGBA in Material in MeshMaterialList in Mesh in x object file " + FileName);
														return false;
													} else if (!(h.Data[3] is Structure)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "emissiveColor is expected to be a ColorRGBA in Material in MeshMaterialList in Mesh in x object file " + FileName);
														return false;
													}
													Structure faceColor = (Structure)h.Data[0];
													Structure specularColor = (Structure)h.Data[2];
													Structure emissiveColor = (Structure)h.Data[3];
													double red, green, blue, alpha;
													// collect face color
													if (faceColor.Name != "ColorRGBA") {
														Interface.AddMessage(Interface.MessageType.Error, false, "faceColor is expected to be a ColorRGBA in Material in MeshMaterialList in Mesh in x object file " + FileName);
														return false;
													} else if (faceColor.Data.Length != 4) {
														Interface.AddMessage(Interface.MessageType.Error, false, "faceColor is expected to have 4 arguments in Material in MeshMaterialList in Mesh in x object file " + FileName);
														return false;
													} else if (!(faceColor.Data[0] is double)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "red is expected to be a float in faceColor in Material in MeshMaterialList in Mesh in x object file " + FileName);
														return false;
													} else if (!(faceColor.Data[1] is double)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "green is expected to be a float in faceColor in Material in MeshMaterialList in Mesh in x object file " + FileName);
														return false;
													} else if (!(faceColor.Data[2] is double)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "blue is expected to be a float in faceColor in Material in MeshMaterialList in Mesh in x object file " + FileName);
														return false;
													} else if (!(faceColor.Data[3] is double)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "alpha is expected to be a float in faceColor in Material in MeshMaterialList in Mesh in x object file " + FileName);
														return false;
													}
													red = (double)faceColor.Data[0];
													green = (double)faceColor.Data[1];
													blue = (double)faceColor.Data[2];
													alpha = (double)faceColor.Data[3];
													if (red < 0.0 | red > 1.0) {
														Interface.AddMessage(Interface.MessageType.Error, false, "red is expected to be in the range from 0.0 to 1.0 in faceColor in Material in MeshMaterialList in Mesh in x object file " + FileName);
														red = red < 0.5 ? 0.0 : 1.0;
													}
													if (green < 0.0 | green > 1.0) {
														Interface.AddMessage(Interface.MessageType.Error, false, "green is expected to be in the range from 0.0 to 1.0 in faceColor in Material in MeshMaterialList in Mesh in x object file " + FileName);
														green = green < 0.5 ? 0.0 : 1.0;
													}
													if (blue < 0.0 | blue > 1.0) {
														Interface.AddMessage(Interface.MessageType.Error, false, "blue is expected to be in the range from 0.0 to 1.0 in faceColor in Material in MeshMaterialList in Mesh in x object file " + FileName);
														blue = blue < 0.5 ? 0.0 : 1.0;
													}
													if (alpha < 0.0 | alpha > 1.0) {
														Interface.AddMessage(Interface.MessageType.Error, false, "alpha is expected to be in the range from 0.0 to 1.0 in faceColor in Material in MeshMaterialList in Mesh in x object file " + FileName);
														alpha = alpha < 0.5 ? 0.0 : 1.0;
													}
													Materials[MaterialIndex].faceColor = new World.ColorRGBA((byte)Math.Round(255.0 * red), (byte)Math.Round(255.0 * green), (byte)Math.Round(255.0 * blue), (byte)Math.Round(255.0 * alpha));
													// collect specular color
													if (specularColor.Name != "ColorRGB") {
														Interface.AddMessage(Interface.MessageType.Error, false, "specularColor is expected to be a ColorRGB in Material in MeshMaterialList in Mesh in x object file " + FileName);
														return false;
													} else if (specularColor.Data.Length != 3) {
														Interface.AddMessage(Interface.MessageType.Error, false, "specularColor is expected to have 3 arguments in Material in MeshMaterialList in Mesh in x object file " + FileName);
														return false;
													} else if (!(specularColor.Data[0] is double)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "red is expected to be a float in specularColor in Material in MeshMaterialList in Mesh in x object file " + FileName);
														return false;
													} else if (!(specularColor.Data[1] is double)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "green is expected to be a float in specularColor in Material in MeshMaterialList in Mesh in x object file " + FileName);
														return false;
													} else if (!(specularColor.Data[2] is double)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "blue is expected to be a float in specularColor in Material in MeshMaterialList in Mesh in x object file " + FileName);
														return false;
													}
													red = (double)specularColor.Data[0];
													green = (double)specularColor.Data[1];
													blue = (double)specularColor.Data[2];
													if (red < 0.0 | red > 1.0) {
														Interface.AddMessage(Interface.MessageType.Error, false, "red is expected to be in the range from 0.0 to 1.0 in specularColor in Material in MeshMaterialList in Mesh in x object file " + FileName);
														red = red < 0.5 ? 0.0 : 1.0;
													}
													if (green < 0.0 | green > 1.0) {
														Interface.AddMessage(Interface.MessageType.Error, false, "green is expected to be in the range from 0.0 to 1.0 in specularColor in Material in MeshMaterialList in Mesh in x object file " + FileName);
														green = green < 0.5 ? 0.0 : 1.0;
													}
													if (blue < 0.0 | blue > 1.0) {
														Interface.AddMessage(Interface.MessageType.Error, false, "blue is expected to be in the range from 0.0 to 1.0 in specularColor in Material in MeshMaterialList in Mesh in x object file " + FileName);
														blue = blue < 0.5 ? 0.0 : 1.0;
													}
													Materials[MaterialIndex].specularColor = new World.ColorRGB((byte)Math.Round(255.0 * red), (byte)Math.Round(255.0 * green), (byte)Math.Round(255.0 * blue));
													// collect emissive color
													if (emissiveColor.Name != "ColorRGB") {
														Interface.AddMessage(Interface.MessageType.Error, false, "emissiveColor is expected to be a ColorRGBA in Material in MeshMaterialList in Mesh in x object file " + FileName);
														return false;
													} else if (emissiveColor.Data.Length != 3) {
														Interface.AddMessage(Interface.MessageType.Error, false, "emissiveColor is expected to have 3 arguments in Material in MeshMaterialList in Mesh in x object file " + FileName);
														return false;
													} else if (!(emissiveColor.Data[0] is double)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "red is expected to be a float in emissiveColor in Material in MeshMaterialList in Mesh in x object file " + FileName);
														return false;
													} else if (!(emissiveColor.Data[1] is double)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "green is expected to be a float in emissiveColor in Material in MeshMaterialList in Mesh in x object file " + FileName);
														return false;
													} else if (!(emissiveColor.Data[2] is double)) {
														Interface.AddMessage(Interface.MessageType.Error, false, "blue is expected to be a float in emissiveColor in Material in MeshMaterialList in Mesh in x object file " + FileName);
														return false;
													}
													red = (double)emissiveColor.Data[0];
													green = (double)emissiveColor.Data[1];
													blue = (double)emissiveColor.Data[2];
													if (red < 0.0 | red > 1.0) {
														Interface.AddMessage(Interface.MessageType.Error, false, "red is expected to be in the range from 0.0 to 1.0 in emissiveColor in Material in MeshMaterialList in Mesh in x object file " + FileName);
														red = red < 0.5 ? 0.0 : 1.0;
													}
													if (green < 0.0 | green > 1.0) {
														Interface.AddMessage(Interface.MessageType.Error, false, "green is expected to be in the range from 0.0 to 1.0 in emissiveColor in Material in MeshMaterialList in Mesh in x object file " + FileName);
														green = green < 0.5 ? 0.0 : 1.0;
													}
													if (blue < 0.0 | blue > 1.0) {
														Interface.AddMessage(Interface.MessageType.Error, false, "blue is expected to be in the range from 0.0 to 1.0 in emissiveColor in Material in MeshMaterialList in Mesh in x object file " + FileName);
														blue = blue < 0.5 ? 0.0 : 1.0;
													}
													Materials[MaterialIndex].emissiveColor = new World.ColorRGB((byte)Math.Round(255.0 * red), (byte)Math.Round(255.0 * green), (byte)Math.Round(255.0 * blue));
													// collect additional templates
													for (int l = 4; l < h.Data.Length; l++) {
														Structure e = h.Data[l] as Structure;
														if (e == null) {
															Interface.AddMessage(Interface.MessageType.Error, false, "Unexpected inlined argument encountered in Material in MeshMaterialList in Mesh in x object file " + FileName);
															return false;
														}
														switch (e.Name) {
															case "TextureFilename":
																{
																	// texturefilename
																	if (e.Data.Length != 1) {
																		Interface.AddMessage(Interface.MessageType.Error, false, "filename is expected to have 1 argument in TextureFilename in Material in MeshMaterialList in Mesh in x object file " + FileName);
																		return false;
																	} else if (!(e.Data[0] is string)) {
																		Interface.AddMessage(Interface.MessageType.Error, false, "filename is expected to be a string in TextureFilename in Material in MeshMaterialList in Mesh in x object file " + FileName);
																		return false;
																	}
																	string filename = (string)e.Data[0];
																	if (OpenBveApi.Path.ContainsInvalidChars(filename)) {
																		Interface.AddMessage(Interface.MessageType.Error, false, "filename contains illegal characters in TextureFilename in Material in MeshMaterialList in Mesh in x object file " + FileName);
																	} else {
																		string File = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), filename);
																		if (System.IO.File.Exists(File)) {
																			Materials[MaterialIndex].TextureFilename = File;
																		} else {
																			Interface.AddMessage(Interface.MessageType.Error, true, "The texture file " + File + " could not be found in TextureFilename in Material in MeshMaterialList in Mesh in x object file " + FileName);
																		}
																	}
																} break;
															default:
																// unknown
																Interface.AddMessage(Interface.MessageType.Warning, false, "Unsupported template " + e.Name + " encountered in MeshMaterialList in Mesh in x object file " + FileName);
																break;
														}
													}
													// finish
													MaterialIndex++;
												}
											} if (MaterialIndex != mn + nMaterials) {
												Interface.AddMessage(Interface.MessageType.Error, false, "nMaterials does not match the number of Material templates encountered in Material in MeshMaterialList in Mesh in x object file " + FileName);
												return false;
											}
											// assign materials
											for (int k = 0; k < nFaceIndexes; k++) {
												FaceMaterials[k] = faceIndexes[k];
											}
											if (nMaterials != 0) {
												for (int k = 0; k < nFaces; k++) {
													if (FaceMaterials[k] == -1) {
														FaceMaterials[k] = 0;
													}
												}
											}
										} break;
									case "MeshTextureCoords":
										{
											// meshtexturecoords
											if (g.Data.Length != 2) {
												Interface.AddMessage(Interface.MessageType.Error, false, "MeshTextureCoords is expected to have 2 arguments in Mesh in x object file " + FileName);
												return false;
											} else if (!(g.Data[0] is int)) {
												Interface.AddMessage(Interface.MessageType.Error, false, "nTextureCoords is expected to be a DWORD in MeshTextureCoords in Mesh in x object file " + FileName);
												return false;
											} else if (!(g.Data[1] is Structure[])) {
												Interface.AddMessage(Interface.MessageType.Error, false, "textureCoords[nTextureCoords] is expected to be a Coords2d array in MeshTextureCoords in Mesh in x object file " + FileName);
												return false;
											}
											int nTextureCoords = (int)g.Data[0];
											Structure[] textureCoords = (Structure[])g.Data[1];
											if (nTextureCoords < 0 | nTextureCoords > nVertices) {
												Interface.AddMessage(Interface.MessageType.Error, false, "nTextureCoords does not reference valid vertices in MeshTextureCoords in Mesh in x object file " + FileName);
												return false;
											}
											for (int k = 0; k < nTextureCoords; k++) {
												if (textureCoords[k].Name != "Coords2d") {
													Interface.AddMessage(Interface.MessageType.Error, false, "textureCoords[" + k.ToString(Culture) + "] is expected to be a Coords2d in MeshTextureCoords in Mesh in x object file " + FileName);
													return false;
												} else if (textureCoords[k].Data.Length != 2) {
													Interface.AddMessage(Interface.MessageType.Error, false, "textureCoords[" + k.ToString(Culture) + "] is expected to have 2 arguments in MeshTextureCoords in Mesh in x object file " + FileName);
													return false;
												} else if (!(textureCoords[k].Data[0] is double)) {
													Interface.AddMessage(Interface.MessageType.Error, false, "u is expected to be a float in textureCoords[" + k.ToString(Culture) + "] in MeshTextureCoords in Mesh in x object file " + FileName);
													return false;
												} else if (!(textureCoords[k].Data[1] is double)) {
													Interface.AddMessage(Interface.MessageType.Error, false, "v is expected to be a float in textureCoords[" + k.ToString(Culture) + "] in MeshTextureCoords in Mesh in x object file " + FileName);
													return false;
												}
												double u = (double)textureCoords[k].Data[0];
												double v = (double)textureCoords[k].Data[1];
												Vertices[k].TextureCoordinates = new World.Vector2Df((float)u, (float)v);
											}
										} break;
									case "MeshNormals":
										{
											// meshnormals
											if (g.Data.Length != 4) {
												Interface.AddMessage(Interface.MessageType.Error, false, "MeshNormals is expected to have 4 arguments in Mesh in x object file " + FileName);
												return false;
											} else if (!(g.Data[0] is int)) {
												Interface.AddMessage(Interface.MessageType.Error, false, "nNormals is expected to be a DWORD in MeshNormals in Mesh in x object file " + FileName);
												return false;
											} else if (!(g.Data[1] is Structure[])) {
												Interface.AddMessage(Interface.MessageType.Error, false, "normals is expected to be a Vector array in MeshNormals in Mesh in x object file " + FileName);
												return false;
											} else if (!(g.Data[2] is int)) {
												Interface.AddMessage(Interface.MessageType.Error, false, "nFaceNormals is expected to be a DWORD in MeshNormals in Mesh in x object file " + FileName);
												return false;
											} else if (!(g.Data[3] is Structure[])) {
												Interface.AddMessage(Interface.MessageType.Error, false, "faceNormals is expected to be a MeshFace array in MeshNormals in Mesh in x object file " + FileName);
												return false;
											}
											int nNormals = (int)g.Data[0];
											if (nNormals < 0) {
												Interface.AddMessage(Interface.MessageType.Error, false, "nNormals is expected to be non-negative in MeshNormals in Mesh in x object file " + FileName);
												return false;
											}
											Structure[] normals = (Structure[])g.Data[1];
											if (nNormals != normals.Length) {
												Interface.AddMessage(Interface.MessageType.Error, false, "nNormals does not match with the length of array normals in MeshNormals in Mesh in x object file " + FileName);
												return false;
											}
											int nFaceNormals = (int)g.Data[2];
											if (nFaceNormals < 0 | nFaceNormals > nFaces) {
												Interface.AddMessage(Interface.MessageType.Error, false, "nNormals does not reference valid vertices in MeshNormals in Mesh in x object file " + FileName);
												return false;
											}
											Structure[] faceNormals = (Structure[])g.Data[3];
											if (nFaceNormals != faceNormals.Length) {
												Interface.AddMessage(Interface.MessageType.Error, false, "nFaceNormals does not match with the length of array faceNormals in MeshNormals in Mesh in x object file " + FileName);
												return false;
											}
											// collect normals
											World.Vector3Df[] Normals = new World.Vector3Df[nNormals];
											for (int k = 0; k < nNormals; k++) {
												if (normals[k].Name != "Vector") {
													Interface.AddMessage(Interface.MessageType.Error, false, "normals[" + k.ToString(Culture) + "] is expected to be of template Vertex in MeshNormals in Mesh in x object file " + FileName);
													return false;
												} else if (normals[k].Data.Length != 3) {
													Interface.AddMessage(Interface.MessageType.Error, false, "normals[" + k.ToString(Culture) + "] is expected to have 3 arguments in MeshNormals in Mesh in x object file " + FileName);
													return false;
												} else if (!(normals[k].Data[0] is double)) {
													Interface.AddMessage(Interface.MessageType.Error, false, "x is expected to be a float in normals[" + k.ToString(Culture) + "] in MeshNormals in Mesh in x object file " + FileName);
													return false;
												} else if (!(normals[k].Data[1] is double)) {
													Interface.AddMessage(Interface.MessageType.Error, false, "y is expected to be a float in normals[" + k.ToString(Culture) + " ]in MeshNormals in Mesh in x object file " + FileName);
													return false;
												} else if (!(normals[k].Data[2] is double)) {
													Interface.AddMessage(Interface.MessageType.Error, false, "z is expected to be a float in normals[" + k.ToString(Culture) + "] in MeshNormals in Mesh in x object file " + FileName);
													return false;
												}
												double x = (double)normals[k].Data[0];
												double y = (double)normals[k].Data[1];
												double z = (double)normals[k].Data[2];
												World.Normalize(ref x, ref y, ref z);
												Normals[k] = new World.Vector3Df((float)x, (float)y, (float)z);
											}
											// collect faces
											for (int k = 0; k < nFaceNormals; k++) {
												if (faceNormals[k].Name != "MeshFace") {
													Interface.AddMessage(Interface.MessageType.Error, false, "faceNormals[" + k.ToString(Culture) + "] is expected to be of template MeshFace in MeshNormals in Mesh in x object file " + FileName);
													return false;
												} else if (faceNormals[k].Data.Length != 2) {
													Interface.AddMessage(Interface.MessageType.Error, false, "faceNormals[" + k.ToString(Culture) + "] is expected to have 2 arguments in MeshNormals in Mesh in x object file " + FileName);
													return false;
												} else if (!(faceNormals[k].Data[0] is int)) {
													Interface.AddMessage(Interface.MessageType.Error, false, "nFaceVertexIndices is expected to be a DWORD in faceNormals[" + k.ToString(Culture) + "] in MeshNormals in Mesh in x object file " + FileName);
													return false;
												} else if (!(faceNormals[k].Data[1] is int[])) {
													Interface.AddMessage(Interface.MessageType.Error, false, "faceVertexIndices[nFaceVertexIndices] is expected to be a DWORD array in faceNormals[" + k.ToString(Culture) + "] in MeshNormals in Mesh in x object file " + FileName);
													return false;
												}
												int nFaceVertexIndices = (int)faceNormals[k].Data[0];
												if (nFaceVertexIndices < 0 | nFaceVertexIndices > Faces[k].Length) {
													Interface.AddMessage(Interface.MessageType.Error, false, "nFaceVertexIndices does not reference a valid vertex in MeshFace in MeshNormals in Mesh in x object file " + FileName);
													return false;
												}
												int[] faceVertexIndices = (int[])faceNormals[k].Data[1];
												if (nFaceVertexIndices != faceVertexIndices.Length) {
													Interface.AddMessage(Interface.MessageType.Error, false, "nFaceVertexIndices does not match with the length of array faceVertexIndices in faceNormals[" + k.ToString(Culture) + "] in MeshFace in MeshNormals in Mesh in x object file " + FileName);
													return false;
												}
												for (int l = 0; l < nFaceVertexIndices; l++) {
													if (faceVertexIndices[l] < 0 | faceVertexIndices[l] >= nNormals) {
														Interface.AddMessage(Interface.MessageType.Error, false, "faceVertexIndices[" + l.ToString(Culture) + "] does not reference a valid normal in faceNormals[" + k.ToString(Culture) + "] in MeshFace in MeshNormals in Mesh in x object file " + FileName);
														return false;
													}
													FaceNormals[k][l] = Normals[faceVertexIndices[l]];
												}
											}
										} break;
									default:
										// unknown
										Interface.AddMessage(Interface.MessageType.Warning, false, "Unsupported template " + g.Name + " encountered in Mesh in x object file " + FileName);
										break;

								}
							}
							// default material
							if (Materials.Length == 0) {
								Materials = new Material[1];
								Materials[0].faceColor = new World.ColorRGBA(255, 255, 255, 255);
								Materials[0].emissiveColor = new World.ColorRGB(0, 0, 0);
								Materials[0].specularColor = new World.ColorRGB(0, 0, 0);
								Materials[0].TextureFilename = null;
								for (int j = 0; j < nFaces; j++) {
									FaceMaterials[j] = 0;
								}
							}
							// create mesh
							int mf = Object.Mesh.Faces.Length;
							int mm = Object.Mesh.Materials.Length;
							int mv = Object.Mesh.Vertices.Length;
							Array.Resize<World.MeshFace>(ref Object.Mesh.Faces, mf + nFaces);
							Array.Resize<World.MeshMaterial>(ref Object.Mesh.Materials, mm + Materials.Length);
							Array.Resize<World.Vertex>(ref Object.Mesh.Vertices, mv + Vertices.Length);
							for (int j = 0; j < Materials.Length; j++) {
								bool emissive = Materials[j].emissiveColor.R != 0 | Materials[j].emissiveColor.G != 0 | Materials[j].emissiveColor.B != 0;
								bool transparent;
								if (Materials[j].TextureFilename != null) {
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
										for (int k = 0; k < nFaces; k++) {
											for (int h = 0; h < Faces[k].Length; h++) {
												if (Vertices[Faces[k][h]].TextureCoordinates.X < 0.0 | Vertices[Faces[k][h]].TextureCoordinates.X > 1.0) {
													WrapX = TextureManager.TextureWrapMode.Repeat;
												}
												if (Vertices[Faces[k][h]].TextureCoordinates.Y < 0.0 | Vertices[Faces[k][h]].TextureCoordinates.Y > 1.0) {
													WrapY = TextureManager.TextureWrapMode.Repeat;
												}
											}
										}
									}
									int tday = TextureManager.RegisterTexture(Materials[j].TextureFilename, new World.ColorRGB(0, 0, 0), 1, TextureManager.TextureLoadMode.Normal, WrapX, WrapY, LoadMode != ObjectManager.ObjectLoadMode.Normal, 0, 0, 0, 0);
									Object.Mesh.Materials[mm + j].DaytimeTextureIndex = tday;
									transparent = true;
								} else {
									Object.Mesh.Materials[mm + j].DaytimeTextureIndex = -1;
									transparent = false;
								}
								Object.Mesh.Materials[mm + j].Flags = (byte)((transparent ? World.MeshMaterial.TransparentColorMask : 0) | (emissive ? World.MeshMaterial.EmissiveColorMask : 0));
								Object.Mesh.Materials[mm + j].Color = Materials[j].faceColor;
								Object.Mesh.Materials[mm + j].TransparentColor = new World.ColorRGB(0, 0, 0);
								Object.Mesh.Materials[mm + j].EmissiveColor = Materials[j].emissiveColor;
								Object.Mesh.Materials[mm + j].NighttimeTextureIndex = -1;
								Object.Mesh.Materials[mm + j].BlendMode = World.MeshMaterialBlendMode.Normal;
								Object.Mesh.Materials[mm + j].GlowAttenuationData = 0;
							}
							for (int j = 0; j < nFaces; j++) {
								Object.Mesh.Faces[mf + j].Material = (ushort)FaceMaterials[j];
								Object.Mesh.Faces[mf + j].Vertices = new World.MeshFaceVertex[Faces[j].Length];
								for (int k = 0; k < Faces[j].Length; k++) {
									Object.Mesh.Faces[mf + j].Vertices[mv + k] = new World.MeshFaceVertex(mv + Faces[j][k], FaceNormals[j][k]);
								}
							}
							for (int j = 0; j < Vertices.Length; j++) {
								Object.Mesh.Vertices[mv + j] = Vertices[j];
							}
							break;
						}
					case "Header":
						break;
					default:
						// unknown
						Interface.AddMessage(Interface.MessageType.Warning, false, "Unsupported template " + f.Name + " encountered in x object file " + FileName);
						break;
				}
			}
			// return
			World.CreateNormals(ref Object.Mesh);
			return true;
		}

	}
}