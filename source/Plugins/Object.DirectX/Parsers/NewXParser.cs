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
using System.Collections.Generic;
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
	class NewXParser
	{
		internal static StaticObject ReadObject(string fileName, Encoding encoding)
		{
			rootMatrix = Matrix4D.NoTransformation;
			currentFolder = Path.GetDirectoryName(fileName);
			currentFile = fileName;
			byte[] Data = File.ReadAllBytes(fileName);
			
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
				string[] Lines = File.ReadAllLines(fileName, encoding);
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
					var list = Lines[i].Split().Where(s => !string.IsNullOrWhiteSpace(s));
					Lines[i] = string.Join(" ", list);
				}
				StringBuilder Builder = new StringBuilder();
				for (int i = 0; i < Lines.Length; i++) {
					Builder.Append(Lines[i]);
					Builder.Append(' ');
				}
				string Content = Builder.ToString();
				Content = Content.Substring(17).Trim();
				return LoadTextualX(Content);
			}

			byte[] newData;
			if (Data[8] == 98 & Data[9] == 105 & Data[10] == 110 & Data[11] == 32)
			{
				//Uncompressed binary, so skip the header
				newData = new byte[Data.Length - 16];
				Array.Copy(Data, 16, newData, 0, Data.Length - 16);
				return LoadBinaryX(newData, floatingPointSize);
			}

			if (Data[8] == 116 & Data[9] == 122 & Data[10] == 105 & Data[11] == 112)
			{
				// compressed textual flavor
				newData = MSZip.Decompress(Data);
				string Text = encoding.GetString(newData);
				return LoadTextualX(Text);
			}

			if (Data[8] == 98 & Data[9] == 122 & Data[10] == 105 & Data[11] == 112)
			{
				//Compressed binary
				//16 bytes of header, then 8 bytes of padding, followed by the actual compressed data
				byte[] uncompressedData = MSZip.Decompress(Data);
				return LoadBinaryX(uncompressedData, floatingPointSize);
			}

			// unsupported flavor
			Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Unsupported X object file encountered in " + fileName);
			return null;
		}
		
		private static StaticObject LoadTextualX(string Text)
		{
			Text = Text.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ").Replace("\t", " ").Trim();
			StaticObject obj = new StaticObject(Plugin.CurrentHost);
			MeshBuilder builder = new MeshBuilder(Plugin.CurrentHost);
			Material material = new Material();
			Block block = new TextualBlock(Text);
			while (block.Position() < block.Length() - 5)
			{
				Block subBlock = block.ReadSubBlock();
				ParseSubBlock(subBlock, ref obj, ref builder, ref material);
			}
			builder.Apply(ref obj, false, false);
			obj.Mesh.CreateNormals();
			if (rootMatrix != Matrix4D.NoTransformation)
			{
				for (int i = transformStart; i < obj.Mesh.Vertices.Length; i++)
				{
					obj.Mesh.Vertices[i].Coordinates.Transform(rootMatrix, false);
				}
			}
			return obj;
		}

		private static string currentFolder;
		private static string currentFile;

		private static Matrix4D rootMatrix;
		private static int currentLevel = 0;
		private static int transformStart = 0;
		private static VertexElement[] vertexElements;

		private static readonly Dictionary<string, Material> rootMaterials = new Dictionary<string, Material>();

		private static void ParseSubBlock(Block block, ref StaticObject obj, ref MeshBuilder builder, ref Material material)
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
					currentLevel++;
					if (builder.Vertices.Count != 0)
					{
						builder.Apply(ref obj, false, false);
						if (rootMatrix != Matrix4D.NoTransformation)
						{
							for (int i = transformStart; i < obj.Mesh.Vertices.Length; i++)
							{
								obj.Mesh.Vertices[i].Coordinates.Transform(rootMatrix, false);
							}
						}
						transformStart = obj.Mesh.Vertices.Length;
						rootMatrix = Matrix4D.NoTransformation;
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
						ParseSubBlock(subBlock, ref obj, ref builder, ref material);
					}
					currentLevel--;
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

					if (currentLevel > 1)
					{
						builder.TransformMatrix = new Matrix4D(matrixValues) * builder.TransformMatrix;
					}
					else
					{
						transformStart = obj.Mesh.Vertices.Length;
						rootMatrix = new Matrix4D(matrixValues);
					}
					break;
				case TemplateID.Mesh:
					currentLevel++;
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
								ParseSubBlock(subBlock, ref obj, ref builder, ref material);
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
						ParseSubBlock(subBlock, ref obj, ref builder, ref material);
					}

					currentLevel--;
					break;
				case TemplateID.MeshMaterialList:
					int nMaterials = block.ReadInt();
					int nFaceIndices = block.ReadInt();
					if (nFaceIndices == 1 && builder.Faces.Count > 1)
					{
						//Single material for all faces
						int globalMaterial = block.ReadInt();
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
							// YUCKY: skip bracket strings
							string materialName = block.ReadString();
							if (rootMaterials.ContainsKey(materialName))
							{
								builder.Materials[i + 1] = rootMaterials[materialName];
							}
							else
							{
								Plugin.CurrentHost.AddMessage(MessageType.Information, false, $"Material { materialName } was not found in DirectX binary file { currentFile }");
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
							try
							{
								subBlock = block.ReadSubBlock(new[] { TemplateID.Material, TemplateID.TextureKey });
								ParseSubBlock(subBlock, ref obj, ref builder, ref material);
							}
							catch (Exception ex)
							{
								if (ex is EndOfStreamException)
								{
									Plugin.CurrentHost.AddMessage(MessageType.Information, false, $"{ nMaterials } materials expected, but { i } found in DirectX binary file { currentFile }");
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
					Color24 mSpecular = new Color24(block.ReadColor96);
					newMaterial.EmissiveColor = new Color24(block.ReadColor96);
					newMaterial.Flags |= MaterialFlags.Emissive; //TODO: Check exact behaviour
					if (Plugin.EnabledHacks.BlackTransparency)
					{
						newMaterial.TransparentColor = Color24.Black; //TODO: Check, also can we optimise which faces have the transparent color set?
						newMaterial.Flags |= MaterialFlags.TransparentColor;
					}
					
					if (block.Position() < block.Length() - 5)
					{
						subBlock = block.ReadSubBlock(TemplateID.TextureFilename);
						ParseSubBlock(subBlock, ref obj, ref builder, ref newMaterial);
					}
					if (currentLevel == 0)
					{
						// Key based material definitions
						if (!string.IsNullOrEmpty(block.Label))
						{
							rootMaterials[block.Label] = newMaterial;
						}
					}
					else
					{
						int m = builder.Materials.Length;
						Array.Resize(ref builder.Materials, m + 1);
						builder.Materials[m] = newMaterial;
					}
					break;
				case TemplateID.TextureFilename:
					string texturePath = block.ReadString();
					if (string.IsNullOrEmpty(texturePath))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Information, false, $"An empty texture was specified for material { material.Key }");
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
						material.DaytimeTexture = OpenBveApi.Path.CombineFile(currentFolder, texturePath);
					}
					catch (Exception e)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, $"Texture file path { texturePath } in file { currentFile } has the problem: { e.Message }");
						material.DaytimeTexture = null;
					}

					if (!File.Exists(material.DaytimeTexture) && material.DaytimeTexture != null)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, true, $"Texture { material.DaytimeTexture } was not found in file { currentFile }");
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
							throw new Exception("nVertexNormals must match the number of verticies in the face");
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
					if (rootMaterials.ContainsKey(block.Label))
					{
						builder.Materials[ml] = rootMaterials[block.Label];
					}
					break;
				case TemplateID.DeclData:
					int numTemplates = (int)block.ReadDword();
					vertexElements = new VertexElement[numTemplates];
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

									for (int i = 0; i < builder.Faces.Count; i++)
									{
										for (int j = 0; j < builder.Faces[i].Vertices.Length; j++)
										{
											if (builder.Faces[i].Vertices[j].Index == currentVertex)
											{
												builder.Faces[i].Vertices[j].Normal = normal;
											}
										}
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

		private static StaticObject LoadBinaryX(byte[] objectBytes, int floatingPointSize)
		{
			Block block = new BinaryBlock(objectBytes, floatingPointSize);
			StaticObject obj = new StaticObject(Plugin.CurrentHost);
			MeshBuilder builder = new MeshBuilder(Plugin.CurrentHost);
			Material material = new Material();
			while (block.Position() < block.Length())
			{
				Block subBlock = block.ReadSubBlock();
				ParseSubBlock(subBlock, ref obj, ref builder, ref material);
			}
			builder.Apply(ref obj, false, false);
			obj.Mesh.CreateNormals();
			if (rootMatrix != Matrix4D.NoTransformation)
			{
				for (int i = transformStart; i < obj.Mesh.Vertices.Length; i++)
				{
					obj.Mesh.Vertices[i].Coordinates.Transform(rootMatrix, false);
				}
			}
			return obj;
		}
	}
}
