//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2020, Christopher Lees, The OpenBVE Project
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Object.DirectX;
using OpenBve.Formats.DirectX;
using OpenBveApi.Colors;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;

namespace Plugin
{
	internal class NewXParser
	{
		/// <summary>Total time spent reading object files from disk, in ticks.</summary>
		internal static long TotalReadMs;
		/// <summary>Total time spent on text preprocessing, in ticks.</summary>
		internal static long TotalPreprocessMs;
		/// <summary>Total time spent parsing block data, in ticks.</summary>
		internal static long TotalParseMs;
		/// <summary>Total time spent applying meshes to objects, in ticks.</summary>
		internal static long TotalApplyMs;
		/// <summary>Total time spent in ReadObject (read + preprocess + parse + apply), in ticks.</summary>
		internal static long TotalReadObjectMs;
		/// <summary>Total time spent in the plugin load call, in ticks.</summary>
		internal static long TotalLoadObjectMs;
		/// <summary>The number of objects parsed.</summary>
		internal static int TotalCount;
		/// <summary>Converts ticks to milliseconds.</summary>
		internal static double TicksToMs(long ticks)
		{
			return ticks / (double)Stopwatch.Frequency * 1000.0;
		}

		internal static StaticObject ReadObject(string fileName, Encoding encoding)
		{
			XParseState state = new XParseState
			{
				Folder = Path.GetDirectoryName(fileName),
				File = fileName
			};
			Stopwatch readTimer = Stopwatch.StartNew();
			byte[] Data = File.ReadAllBytes(fileName);
			readTimer.Stop();
			System.Threading.Interlocked.Add(ref TotalReadMs, readTimer.Elapsed.Ticks);
			
			if (Data.Length < 16 || Data[0] != 120 | Data[1] != 111 | Data[2] != 102 | Data[3] != 32)
			{
				// Object is actually a single line text file containing relative path to the 'real' X
				// Found in BRSigs\Night
				string relativePath = Encoding.ASCII.GetString(Data);
				if (!OpenBveApi.Path.ContainsInvalidChars(relativePath))
				{
					return ReadObject(OpenBveApi.Path.CombineFile(Path.GetDirectoryName(fileName), relativePath), encoding);
				}
			}

			// floating-point format
			int floatingPointSize;
			if (Data[12] == 48 & Data[13] == 48 & Data[14] == 51 & Data[15] == 50)
			{
				floatingPointSize = 32;
			}
			else if (Data[12] == 48 & Data[13] == 48 & Data[14] == 54 & Data[15] == 52)
			{
				floatingPointSize = 64;
			}
			else
			{
				throw new NotSupportedException();
			}

			// supported floating point format
			if (Data[8] == 116 & Data[9] == 120 & Data[10] == 116 & Data[11] == 32)
			{
				// textual flavor
				// Single pass over the raw text: strip comments (respecting quoted strings),
				// collapse runs of whitespace to a single space and append to a single buffer.
				Stopwatch prepTimer = Stopwatch.StartNew();
				string Text = encoding.GetString(Data);
				// Skip the 17 character "xof 0303txt 0032" file header while building the preprocessed text.
				StringBuilder stripped = new StringBuilder(Text.Length);
				bool Quote = false;
				bool InComment = false;
				for (int i = 17; i < Text.Length; i++)
				{
					char c = Text[i];
					if (InComment)
					{
						if (c == '\n')
						{
							InComment = false;
							Quote = false;
							AppendSeparator(stripped);
						}
						continue;
					}
					if (c == '"')
					{
						Quote = !Quote;
						stripped.Append(c);
						continue;
					}
					if (!Quote && (c == '#' || c == '/' && i + 1 < Text.Length && Text[i + 1] == '/'))
					{
						InComment = true;
						continue;
					}
					if (c == '\n')
					{
						Quote = false;
					}
					if (c == ' ' || c == '\t' || c == '\r' || c == '\n')
					{
						AppendSeparator(stripped);
						continue;
					}
					stripped.Append(c);
				}
				string Content = stripped.ToString();
				prepTimer.Stop();
				System.Threading.Interlocked.Add(ref TotalPreprocessMs, prepTimer.Elapsed.Ticks);
				Stopwatch readObjectTimer = Stopwatch.StartNew();
				StaticObject result = LoadTextualX(Content, true, state);
				readObjectTimer.Stop();
				System.Threading.Interlocked.Add(ref TotalReadObjectMs, readObjectTimer.Elapsed.Ticks);
				return result;
			}

			byte[] newData;
			if (Data[8] == 98 & Data[9] == 105 & Data[10] == 110 & Data[11] == 32)
			{
				//Uncompressed binary, so skip the header
				newData = new byte[Data.Length - 16];
				Array.Copy(Data, 16, newData, 0, Data.Length - 16);
				return LoadBinaryX(newData, floatingPointSize, state);
			}

			if (Data[8] == 116 & Data[9] == 122 & Data[10] == 105 & Data[11] == 112)
			{
				// compressed textual flavor
				newData = MSZip.Decompress(Data);
				string Text = encoding.GetString(newData);
				return LoadTextualX(Text, false, state);
			}

			if (Data[8] == 98 & Data[9] == 122 & Data[10] == 105 & Data[11] == 112)
			{
				//Compressed binary
				//16 bytes of header, then 8 bytes of padding, followed by the actual compressed data
				byte[] uncompressedData = MSZip.Decompress(Data);
				return LoadBinaryX(uncompressedData, floatingPointSize, state);
			}

			// unsupported flavor
			Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Unsupported X object file encountered in " + fileName);
			return null;
		}
		
		private static StaticObject LoadTextualX(string Text, bool preprocessed, XParseState state)
		{
			if (!preprocessed)
			{
				Text = Text.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ").Replace("\t", " ").Trim();
			}
			Stopwatch parseTimer = Stopwatch.StartNew();
			StaticObject obj = new StaticObject(Plugin.CurrentHost);
			MeshBuilder builder = new MeshBuilder(Plugin.CurrentHost);
			Material material = new Material();
			Block block = new TextualBlock(Text);
			while (block.Position() < block.Length() - 5)
			{
				Block subBlock = block.ReadSubBlock();
				ParseSubBlock(subBlock, ref obj, ref builder, ref material, state);
			}
			parseTimer.Stop();
			System.Threading.Interlocked.Add(ref TotalParseMs, parseTimer.Elapsed.Ticks);
			Stopwatch applyTimer = Stopwatch.StartNew();
			builder.Apply(ref obj, false, false);
			obj.Mesh.CreateNormals();
			applyTimer.Stop();
			System.Threading.Interlocked.Add(ref TotalApplyMs, applyTimer.Elapsed.Ticks);
			if (state.RootMatrix != Matrix4D.NoTransformation)
			{
				for (int i = state.TransformStart; i < obj.Mesh.Vertices.Length; i++)
				{
					obj.Mesh.Vertices[i].Coordinates.Transform(state.RootMatrix, false);
				}
			}
			return obj;
		}

		/// <summary>Per-call parse state, allowing multiple objects to be parsed concurrently.</summary>
		private sealed class XParseState
		{
			internal string Folder;
			internal string File;
			internal Matrix4D RootMatrix = Matrix4D.NoTransformation;
			internal int Level;
			internal int TransformStart;
			internal bool MaterialUsed;
		}

		/// <summary>Appends a single space separator, avoiding runs of whitespace.</summary>
		private static void AppendSeparator(StringBuilder sb)
		{
			if (sb.Length == 0 || sb[sb.Length - 1] != ' ')
			{
				sb.Append(' ');
			}
		}

		private static readonly ConcurrentDictionary<string, Material> rootMaterials = new ConcurrentDictionary<string, Material>();

		private static void ParseSubBlock(Block block, ref StaticObject obj, ref MeshBuilder builder, ref Material material, XParseState state)
		{
			Block subBlock;
			switch (block.Token)
			{
				default:
					return;
				case TemplateID.Template:
					// ReSharper disable once UnusedVariable
					string GUID = block.ReadString();
					/*
					 * Valid Microsoft templates are listed here:
					 * https://docs.microsoft.com/en-us/windows/desktop/direct3d9/dx9-graphics-reference-x-file-format-templates
					 * However, an application may define it's own template (or by the looks of things override another)
					 * by declaring this at the head of the file, and using a unique GUID
					 *
					 * Mesquoia does this by defining a copy of the Boolean template using a WORD as opposed to a DWORD
					 * No practical effect in this case, however be wary of this....
					 */
					return;
				case TemplateID.Header:
					// ReSharper disable once UnusedVariable
					int majorVersion = block.ReadUInt16();
					// ReSharper disable once UnusedVariable
					int minorVersion = block.ReadUInt16();
					int flags = block.ReadUInt16();
					switch (flags)
					{
						/* According to http://paulbourke.net/dataformats/directx/#xfilefrm_Template_Header
						 * it is possible for a file to contain a mix of both binary and textual blocks.
						 *
						 * The Header block controls the format of the file from this point onwards.
						 * majorVersion and minorVersion relate to the legacy Direct3D retained mode API
						 * and can probably be ignored. (Assume that features are cumulative and backwards compatible)
						 * flags sets whether the blocks from this point onwards are binary or textual.
						 *
						 * TODO: Need a mixed mode file sample if we want this to work.
						 * Probably exceedingly uncommon, so low priority
						 */

						case 0:
							if (block is TextualBlock)
							{
								throw new Exception("Mixed-mode text and binary objects are not supported by this parser.");
							}
							break;
						default:
							if (block is BinaryBlock)
							{
								throw new Exception("Mixed-mode text and binary objects are not supported by this parser.");
							}
							break;
					}
					return;
				case TemplateID.Frame:
					state.Level++;
					if (builder.Vertices.Count != 0)
					{
						builder.Apply(ref obj, false, false);
						if (state.RootMatrix != Matrix4D.NoTransformation)
						{
							for (int i = state.TransformStart; i < obj.Mesh.Vertices.Length; i++)
							{
								obj.Mesh.Vertices[i].Coordinates.Transform(state.RootMatrix, false);
							}
						}
						state.TransformStart = obj.Mesh.Vertices.Length;
						state.RootMatrix = Matrix4D.NoTransformation;
						builder = new MeshBuilder(Plugin.CurrentHost);
					}
					while (block.Position() < block.Length() - 5)
					{
						/*
						 * TODO: Whilst https://docs.microsoft.com/en-us/windows/desktop/direct3d9/frame suggests the Frame template should only contain
						 * Mesh, FrameTransformMatrix or Frame templates by default, 3DS Max stuffs all manner of things into here
						 *
						 * It would be nice to get 3DS max stuff detected specifically, especially as we don't support most of this
						 */
						//TemplateID[] validTokens = { TemplateID.Mesh , TemplateID.FrameTransformMatrix, TemplateID.Frame };
						subBlock = block.ReadSubBlock();
						ParseSubBlock(subBlock, ref obj, ref builder, ref material, state);
					}
					state.Level--;
					if (builder.Vertices.Count == 0)
					{
						builder.TransformMatrix = Matrix4D.NoTransformation;
					}
					break;
				case TemplateID.FrameTransformMatrix:
					double[] matrixValues = new double[16];
					for (int i = 0; i < 16; i++)
					{
						matrixValues[i] = block.ReadSingle();
					}

					if (state.Level > 1)
					{
						builder.TransformMatrix = new Matrix4D(matrixValues) * builder.TransformMatrix;
					}
					else
					{
						state.TransformStart = obj.Mesh.Vertices.Length;
						state.RootMatrix = new Matrix4D(matrixValues);
					}
					break;
				case TemplateID.Mesh:
					state.Level++;
					if (builder.Vertices.Count != 0)
					{
						builder.Apply(ref obj, false, false);
						builder = new MeshBuilder(Plugin.CurrentHost);
					}
					int nVerts = block.ReadInt();
					if (nVerts == 0)
					{
						//Some null objects contain an empty mesh
						Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "nVertices should be greater than zero in Mesh " + block.Label);
					}
					for (int i = 0; i < nVerts; i++)
					{
						builder.Vertices.Add(new Vertex(new Vector3(block.ReadSingle(), block.ReadSingle(), block.ReadSingle())));
					}
					int nFaces = block.ReadInt();
					if (nFaces == 0)
					{
						try
						{
							/*
							 * A mesh has been defined with no faces.
							 * If we are not at the end of the block,
							 * attempt to read the next sub-block
							 *
							 * If this fails, the face count is probably incorrect
							 *
							 * NOTE: In this case, the face statement will be an empty string / whitespace
							 * hence the block.ReadString() call
							 */
							block.ReadString();
							if (block.Position() < block.Length() - 5)
							{
								subBlock = block.ReadSubBlock();
								ParseSubBlock(subBlock, ref obj, ref builder, ref material, state);
							}
							goto NoFaces;
						}
						catch
						{
							throw new Exception("nFaces was declared as zero, but unrecognised data remains in the block");
						}
						
					}
					for (int i = 0; i < nFaces; i++)
					{
						int fVerts = block.ReadInt();
						if (fVerts == 0)
						{
							// Assuming here that a face must contain vertices
							Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "fVerts was declared as zero");
							break;
						}
						MeshFace f = new MeshFace(fVerts);
						for (int j = 0; j < fVerts; j++)
						{
							f.Vertices[j].Index = block.ReadInt();
						}
						builder.Faces.Add(f);
					}
					NoFaces:
					while (block.Position() < block.Length() - 5)
					{
						subBlock = block.ReadSubBlock();
						ParseSubBlock(subBlock, ref obj, ref builder, ref material, state);
					}

					state.Level--;
					break;
				case TemplateID.MeshMaterialList:
					int nMaterials = block.ReadInt();
					bool[] materialsUsed = new bool[nMaterials];
					int nFaceIndices = block.ReadInt();
					if (nFaceIndices == 1 && builder.Faces.Count > 1)
					{
						//Single material for all faces
						int globalMaterial = block.ReadInt();
						materialsUsed[globalMaterial] = true;
						for (int i = 0; i < builder.Faces.Count; i++)
						{
							MeshFace f = builder.Faces[i];
							f.Material = (ushort)(globalMaterial + 1);
							builder.Faces[i] = f;
						}
					}
					else if(nFaceIndices == builder.Faces.Count)
					{
						for (int i = 0; i < nFaceIndices; i++)
						{
							int fMaterial = block.ReadInt();
							materialsUsed[fMaterial] = true;
							MeshFace f = builder.Faces[i];
							f.Material = (ushort) (fMaterial + 1);
							builder.Faces[i] = f;
						}
					}
					else
					{
						throw new Exception("nFaceIndices must match the number of faces in the mesh");
					}

					if (block is BinaryBlock && block.ReadString() == "{")
					{
						// reference based materials
						Array.Resize(ref builder.Materials, nMaterials + 1);
						for (int i = 0; i < nMaterials; i++)
						{
							state.MaterialUsed = materialsUsed[i];
							// YUCKY: skip bracket strings
							string materialName = block.ReadString();
							if (!rootMaterials.TryGetValue(materialName, out builder.Materials[i + 1]))
							{
								Plugin.CurrentHost.AddMessage(MessageType.Information, false, $"Material {materialName} was not found in DirectX binary file {state.File}");
								builder.Materials[i + 1] = new Material();
							}
							
							block.ReadString();
							if (i < nMaterials - 1)
							{
								block.ReadString();
							}
							
						}
					}
					else
					{
						for (int i = 0; i < nMaterials; i++)
						{
							state.MaterialUsed = materialsUsed[i];
							try
							{
								subBlock = block.ReadSubBlock(new[] { TemplateID.Material, TemplateID.TextureKey });
								ParseSubBlock(subBlock, ref obj, ref builder, ref material, state);
							}
							catch (Exception ex)
							{
								if (ex is EndOfStreamException)
								{
									Plugin.CurrentHost.AddMessage(MessageType.Information, false, $"{ nMaterials } materials expected, but { i } found in DirectX binary file { state.File }");
								}
								break;
							}
						}
					}
					
					break;
				case TemplateID.Material:
					Material newMaterial = new Material();
					newMaterial.Color = new Color32(block.ReadColor128);
					double mPower = block.ReadSingle(); //TODO: Unsure what this does...
					try
					{
						newMaterial.SpecularColor = new Color24(block.ReadColor96);
					}
					catch
					{
						Plugin.CurrentHost.AddMessage(MessageType.Information, false, $"Specular color is invalid for material {material.Key}");
						newMaterial.SpecularColor = Color24.Black;
					}
					
					if (newMaterial.SpecularColor != Color24.Black)
					{
						newMaterial.Flags |= MaterialFlags.Specular;
					}
					// Convert Color96 → Color24 → Color32; alpha defaults to 255 (opaque)
					try
					{
						newMaterial.EmissiveColor = new Color32(new Color24(block.ReadColor96));
					}
					catch
					{
						Plugin.CurrentHost.AddMessage(MessageType.Information, false, $"Emissive color is invalid for material {material.Key}");
						newMaterial.EmissiveColor = Color32.Black;
					}
					
					if (newMaterial.EmissiveColor != Color32.Black)
					{
						newMaterial.Flags |= MaterialFlags.Emissive;
					}
					
					if (Plugin.EnabledHacks.BlackTransparency)
					{
						newMaterial.TransparentColor = Color24.Black; //TODO: Check, also can we optimise which faces have the transparent color set?
						newMaterial.Flags |= MaterialFlags.TransparentColor;
					}
					
					if (block.Position() < block.Length() - 5)
					{
						subBlock = block.ReadSubBlock(TemplateID.TextureFilename);
						ParseSubBlock(subBlock, ref obj, ref builder, ref newMaterial, state);
					}
					if (state.Level == 0)
					{
						// Key based material definitions
						if (!string.IsNullOrEmpty(block.Label))
						{
							rootMaterials[block.Label] = newMaterial;
						}
					}
					else
					{
						// Optimize: Use a list for materials and only update the builder at the end if needed 
						// but to keep it simple, we check if current material matches before resizing
						int m = builder.Materials.Length;
						Array.Resize(ref builder.Materials, m + 1);
						builder.Materials[m] = newMaterial;
					}
					break;
				case TemplateID.TextureFilename:
					string texturePath = block.ReadString();
					if (string.IsNullOrEmpty(texturePath))
					{
						if (state.MaterialUsed)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Information, false, $"An empty texture was specified for material {material.Key}");
						}
						else
						{
							Plugin.CurrentHost.AddMessage(MessageType.Information, false, $"Referenced, but unused material {material.Key} specifies an empty texture");
						}
						material.DaytimeTexture = null;
						break;
					}
					// If the specified file name is an absolute path, make it the file name only.
					// Some object files specify absolute paths.
					// And BVE4/5 doesn't allow textures to be placed in a different directory than the object file.
					if (Plugin.EnabledHacks.BveTsHacks && OpenBveApi.Path.IsAbsolutePath(texturePath))
					{
						texturePath = texturePath.Split('/', '\\').Last();
					}

					try
					{
						material.DaytimeTexture = OpenBveApi.Path.CombineFile(state.Folder, texturePath);
					}
					catch (Exception e)
					{
						if (state.MaterialUsed)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, $"Texture file path {texturePath} in file {state.File} has the problem: {e.Message}");
						}
						else
						{
							Plugin.CurrentHost.AddMessage(MessageType.Warning, false, $"Referenced, but unused Texture file path {texturePath} for material {material.Key} in file {state.File} has the problem: {e.Message}");
						}
						material.DaytimeTexture = null;
					}


					if (Plugin.EnabledHacks.BveTsHacks && !File.Exists(material.DaytimeTexture))
					{
						// XOF doesn't have a way to specify text encoding, and some (more common with BVE5) stuff is using shift_jis
						try
						{
							byte[] stringBytes = Encoding.GetEncoding(0).GetBytes(texturePath);
							string shift_jis_string = Encoding.GetEncoding("shift_jis").GetString(stringBytes);
							material.DaytimeTexture = OpenBveApi.Path.CombineFile(state.Folder, shift_jis_string);
						}
						catch
						{
							// ignore
						}
					}

					if (!File.Exists(material.DaytimeTexture) && material.DaytimeTexture != null)
					{
						if (state.MaterialUsed)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, true, $"Texture {material.DaytimeTexture} for material {material.Key} was not found in file {state.File}");
						}
						else
						{
							Plugin.CurrentHost.AddMessage(MessageType.Warning, true, $"Referenced, but unused Texture {material.DaytimeTexture} for material {material.Key} was not found in file {state.File}");
						}
						material.DaytimeTexture = null;
					}
					break;
				case TemplateID.MeshTextureCoords:
					int nCoords = block.ReadInt();
					for (int i = 0; i < nCoords; i++)
					{
						builder.Vertices[i].TextureCoordinates = new Vector2(block.ReadSingle(), block.ReadSingle());
					}
					break;
				case TemplateID.MeshNormals:
					int nNormals = block.ReadInt();
					Vector3[] normals = new Vector3[nNormals];
					for (int i = 0; i < nNormals; i++)
					{
						normals[i] = new Vector3(block.ReadSingle(), block.ReadSingle(), block.ReadSingle());
						normals[i].Normalize();
					}
					int nFaceNormals = block.ReadInt();
					if (nFaceNormals != builder.Faces.Count)
					{
						throw new Exception("nFaceNormals must match the number of faces in the mesh");
					}
					for (int i = 0; i < nFaceNormals; i++)
					{
						int nVertexNormals = block.ReadInt();
						if (nVertexNormals != builder.Faces[i].Vertices.Length)
						{
							throw new Exception("nVertexNormals must match the number of vertices in the face");
						}
						for (int j = 0; j < nVertexNormals; j++)
						{
							int normalIdx = block.ReadInt();
							if (normalIdx < normals.Length)
							{
								// Check normal index is valid
								builder.Faces[i].Vertices[j].Normal = normals[normalIdx];
							}
						}
					}
					break;
				case TemplateID.MeshVertexColors:
					int nVertexColors = block.ReadInt();
					for (int i = 0; i < nVertexColors; i++)
					{
						int idx = block.ReadInt();
						if (idx >= builder.Vertices.Count)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Warning, false, $"MeshVertexColors index { idx } should be less than nVertices in Mesh { block.Label }");
							continue;
						}
						ColoredVertex c = builder.Vertices[idx] as ColoredVertex;
						if (c != null)
						{
							c.Color.R = block.ReadSingle();
							c.Color.G = block.ReadSingle();
							c.Color.B = block.ReadSingle();
							c.Color.A = block.ReadSingle();
						}
						else
						{
							builder.Vertices[idx] = new ColoredVertex((Vertex)builder.Vertices[idx], new Color128(block.ReadSingle(), block.ReadSingle(), block.ReadSingle(), block.ReadSingle()));
						}
					}
					break;
				case TemplateID.MeshFaceWraps:
					int nMeshFaceWraps = block.ReadInt();
					if (nMeshFaceWraps != builder.Faces.Count)
					{
						throw new Exception("nMeshFaceWraps must match the number of faces in the mesh");
					}
					/*
					 * MeshFaceWraps is a 2 * boolean array, representing the clamping on X / Y axis for each face
					 * The current engine only supports clamping on a per-texture basis & this was discontinued in
					 * later versions of DirectX so just validate this is structurally valid and ignore for the minute
					 */
					break;
				case TemplateID.TextureKey:
					if (string.IsNullOrEmpty(block.Label))
					{
						break;
					}
					int ml = builder.Materials.Length;
					Array.Resize(ref builder.Materials, ml + 1);
					builder.Materials[ml] = new Material();
					rootMaterials.TryGetValue(block.Label, out builder.Materials[ml]);
					break;
				case TemplateID.DeclData:
					int numTemplates = (int)block.ReadDword();
					VertexElement[] vertexElements = new VertexElement[numTemplates];
					for (int i = 0; i < numTemplates; i++)
					{
						vertexElements[i] = new VertexElement(block.ReadDword(), block.ReadDword(), block.ReadDword(), block.ReadDword());
					}

					int currentElement = 0;
					int currentVertex = 0;
					unsafe
					{
						// unsafe to convert dwords back to floats (used in this context as no precision problem)
						int numRemainingDwords = (int)block.ReadDword();
						while (numRemainingDwords > 0)
						{
							switch (vertexElements[currentElement].Usage)
							{
								default:
									throw new NotImplementedException(vertexElements[currentElement].Usage + " is not implemented by this decoder.");
								case D3DDeclUsage.D3DDECLUSAGE_NORMAL:
									uint x = block.ReadDword();
									uint y = block.ReadDword();
									uint z = block.ReadDword();
									Vector3 normal = new Vector3(*(float*)&x, *(float*)&y, *(float*)&z);

									// Optimize: Avoid O(N^2) search by updating only relevant facial vertices
									for (int i = 0; i < builder.Faces.Count; i++)
									{
										MeshFace f = builder.Faces[i];
										for (int j = 0; j < f.Vertices.Length; j++)
										{
											if (f.Vertices[j].Index == currentVertex)
											{
												f.Vertices[j].Normal = normal;
											}
										}
										builder.Faces[i] = f;
									}
									numRemainingDwords -= 3;
									break;
								case D3DDeclUsage.D3DDECLUSAGE_TEXCOORD:
									x = block.ReadDword();
									y = block.ReadDword();
									Vector2 texCoords = new Vector2(*(float*)&x, *(float*)&y);
									if (vertexElements[currentElement].UsageIndex == 0)
									{
										// as additional D3DDECLUSAGE_TEXCOORD may also be used to store other user shader data per MSDN
										builder.Vertices[currentVertex].TextureCoordinates = texCoords;
									}
									numRemainingDwords -= 2;
									break;
								case D3DDeclUsage.D3DDECLUSAGE_COLOR:
									uint usageIndex = block.ReadDword();
									uint r = block.ReadDword();
									uint g = block.ReadDword();
									uint b = block.ReadDword();
									uint a = block.ReadDword();
									if (usageIndex == 0)
									{
										// diffuse color
										ColoredVertex c = builder.Vertices[currentVertex] as ColoredVertex;
										if (c != null)
										{
											c.Color = new Color128(*(float*)&r, *(float*)&g, *(float*)&b, *(float*)&a);
										}
										else
										{
											builder.Vertices[currentVertex] = new ColoredVertex((Vertex)builder.Vertices[currentVertex], new Color128(*(float*)&r, *(float*)&g, *(float*)&b, *(float*)&a));
										}
									}
									break;
							}
							
							currentElement++;
							if (currentElement > vertexElements.Length - 1)
							{
								// move to next vertex
								currentElement = 0;
								currentVertex++;
							}

						}
					}

					break;
			}
		}

		private static StaticObject LoadBinaryX(byte[] objectBytes, int floatingPointSize, XParseState state)
		{
			Block block = new BinaryBlock(objectBytes, floatingPointSize);
			StaticObject obj = new StaticObject(Plugin.CurrentHost);
			MeshBuilder builder = new MeshBuilder(Plugin.CurrentHost);
			Material material = new Material();
			while (block.Position() < block.Length())
			{
				Block subBlock = block.ReadSubBlock();
				ParseSubBlock(subBlock, ref obj, ref builder, ref material, state);
			}
			builder.Apply(ref obj, false, false);
			obj.Mesh.CreateNormals();
			if (state.RootMatrix != Matrix4D.NoTransformation)
			{
				for (int i = state.TransformStart; i < obj.Mesh.Vertices.Length; i++)
				{
					obj.Mesh.Vertices[i].Coordinates.Transform(state.RootMatrix, false);
				}
			}
			return obj;
		}
	}
}
