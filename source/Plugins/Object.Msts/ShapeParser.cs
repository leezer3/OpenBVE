//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2024, Christopher Lees, The OpenBVE Project
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

using OpenBve.Formats.MsTs;
using OpenBveApi.Colors;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Textures;
using SharpCompress.Compressors;
using SharpCompress.Compressors.Deflate;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;


// Stop ReSharper complaining about unused stuff:
// We need to load this sequentially anyway, and
// hopefully this will be used in a later build
// Also disable naming warnings, as we'll use the Kuju textual representations

// ReSharper disable NotAccessedField.Local
// ReSharper disable RedundantAssignment
// ReSharper disable UnusedVariable
// ReSharper disable InconsistentNaming
#pragma warning disable 0219, IDE0059
namespace Plugin
{
	partial class MsTsShapeParser
	{
		class Texture
		{
			internal readonly string fileName;
			internal int filterMode;
			internal int mipmapLODBias;
			internal Color32 borderColor;

			internal Texture(string file)
			{
				fileName = file;
			}
		}

		class PrimitiveState
		{
			internal readonly string Name;
			internal UInt32 Flags;
			internal int Shader;
			internal int[] Textures;
			/*
			 * Unlikely to be able to support these at present
			 * However, read and see if we can hack common ones
			 */
			internal float ZBias;
			internal int vertexStates;
			internal int alphaTestMode;
			internal int lightCfgIdx;
			internal int zBufferMode;

			internal PrimitiveState(string name)
			{
				Name = name;
			}
		}

		class VertexStates
		{
			internal uint flags; //Describes specular and some other stuff, unlikely to be supported
			/// <summary>The hierarchy ID of the top-level transform matrix</summary>
			/// <remarks>Remmeber that matricies transform down a chain</remarks>
			internal readonly int hierarchyID;
			internal int lightingMatrixID;
			internal int lightingConfigIdx;
			internal uint lightingFlags;
			internal int matrix2ID; //Optional

			internal VertexStates(int hierarchy)
			{
				hierarchyID = hierarchy;
			}
		}

		class VertexSet
		{
			internal readonly int hierarchyIndex;
			internal int startVertex;
			internal int numVerticies;

			internal VertexSet(int hierarchy)
			{
				hierarchyIndex = hierarchy;
			}
		}


		struct Animation
		{
			internal int FrameCount;
			internal Dictionary<string, KeyframeAnimation> Nodes;
			/*
			 * WARNING: MSTS glitch / 'feature':
			 * This FPS number is divided by 30 for interior view objects
			 * http://www.elvastower.com/forums/index.php?/topic/29692-animations-in-the-passenger-view-too-fast/page__p__213634
			 */
			internal double FrameRate;
		}

		
		class Vertex
		{
			internal Vector3 Coordinates;
			internal Vector3 Normal;
			internal Vector2 TextureCoordinates;
			internal int[] matrixChain;

			public Vertex(Vector3 c, Vector3 n)
			{
				this.Coordinates = new Vector3(c.X, c.Y, c.Z);
				this.Normal = new Vector3(n.X, n.Y, n.Z);
			}
		}

		private readonly struct Face
		{
			internal readonly int[] Vertices;
			internal readonly int Material;

			internal Face(int[] vertices, int material)
			{
				Vertices = vertices;
				Material = material == -1 ? 0 : material;
			}
		}

		private class MsTsShape
		{
			internal MsTsShape()
			{
				points = new List<Vector3>();
				normals = new List<Vector3>();
				uv_points = new List<Vector2>();
				images = new List<string>();
				textures = new List<Texture>();
				prim_states = new List<PrimitiveState>();
				vtx_states = new List<VertexStates>();
				LODs = new List<LOD>();
				Animations = new List<Animation>();
				AnimatedMatricies = new Dictionary<int, int>();
				Matricies = new List<KeyframeMatrix>();
				MatrixParents = new Dictionary<int, int>();
				ShaderNames = new List<ShaderNames>();
				colors = new List<Color32>();
			}

			// Global variables used by all LODs

			/// <summary>The points used by this shape</summary>
			internal readonly List<Vector3> points;
			/// <summary>The normal vectors used by this shape</summary>
			internal readonly List<Vector3> normals;
			/// <summary>The texture-coordinates used by this shape</summary>
			internal readonly List<Vector2> uv_points;
			/// <summary>The filenames of all textures used by this shape</summary>
			internal readonly List<string> images;
			/// <summary>The textures used, with associated parameters</summary>
			/// <remarks>Allows the alpha testing mode to be set etc. so that the same image file can be reused</remarks>
			internal readonly List<Texture> textures;
			/// <summary>The list of colors available to be applied to verticies</summary>
			internal readonly List<Color32> colors;
			/// <summary>Contains the shader, texture etc. used by the primitive</summary>
			/// <remarks>Largely unsupported other than the texture name at the minute</remarks>
			internal readonly List<PrimitiveState> prim_states;

			internal readonly List<VertexStates> vtx_states;

			internal readonly List<Animation> Animations;
			/// <summary>Dictionary of animated matricies mapped to the final model</summary>
			/// <remarks>Most matricies aren't animated, so don't send excessive numbers to the shader each frame</remarks>
			internal readonly Dictionary<int, int> AnimatedMatricies;
			/// <summary>The matricies within the shape</summary>
			internal readonly List<KeyframeMatrix> Matricies;

			internal readonly Dictionary<int, int> MatrixParents;

			internal readonly List<ShaderNames> ShaderNames;

			// The list of LODs actually containing the objects

			/// <summary>The list of all LODs from the model</summary>
			internal readonly List<LOD> LODs;

			// Control variables

			internal int currentPrimitiveState = -1;
			internal int totalObjects = 0;
		}

		private class LOD
		{
			/// <summary>Creates a new LOD</summary>
			/// <param name="distance">The maximum viewing distance for this LOD</param>
			internal LOD(double distance)
			{
				this.viewingDistance = distance;
				this.subObjects = new List<SubObject>();
			}

			internal readonly double viewingDistance;
			internal List<SubObject> subObjects;
			internal int[] hierarchy;
		}

		private class SubObject
		{
			internal SubObject()
			{
				this.verticies = new List<Vertex>();
				this.vertexSets = new List<VertexSet>();
				this.faces = new List<Face>();
				this.materials = new List<Material>();
				this.hierarchy = new List<int>();
			}

			internal void TransformVerticies(MsTsShape shape)
			{
				transformedVertices = new List<Vertex>(verticies);
				
				
				for (int i = 0; i < verticies.Count; i++)
				{
					transformedVertices[i] = new Vertex(verticies[i].Coordinates, verticies[i].Normal);
					for (int j = 0; j < vertexSets.Count; j++)
					{
						if (vertexSets[j].startVertex <= i && vertexSets[j].startVertex + vertexSets[j].numVerticies > i)
						{
							List<int> matrixChain = new List<int>();
							bool staticTransform = true;
							int hi = vertexSets[j].hierarchyIndex;
							if (hi != -1 && hi < shape.Matricies.Count)
							{
								matrixChain.Add(hi);
								if (IsAnimated(shape.Matricies[hi].Name))
								{
									staticTransform = false;
								}
								while (true)
								{
									hi = currentLOD.hierarchy[hi];
									
									if (hi == -1)
									{
										break;
									}
									
									if (hi == matrixChain[matrixChain.Count - 1])
									{
										continue;
									}
									matrixChain.Add(hi);
									if (IsAnimated(shape.Matricies[hi].Name))
									{
										staticTransform = false;
									}
								}
							}
							else
							{
								//Unsure of the cause of this, matrix appears to be invalid
								i = vertexSets[j].startVertex + vertexSets[j].numVerticies;
								matrixChain.Clear();
							}

							if (staticTransform && string.IsNullOrEmpty(wagonFileDirectory)) // if part of a MSTS train, we may need to operate on contained matricies when merging shapes
							{
								for (int k = 0;k < matrixChain.Count; k++)
								{
									transformedVertices[i].Coordinates.Transform(shape.Matricies[matrixChain[k]].Matrix, false);
								}
							}
							else
							{
								// find and store matrix parents for use by the animation function
								for (int k = matrixChain.Count - 1; k > 0; k--)
								{
									if (!shape.MatrixParents.ContainsKey(matrixChain[k - 1]))
									{
										shape.MatrixParents[matrixChain[k - 1]] = matrixChain[k];
									}
								}
								/*
								 * Check if our matricies are in the shape, and copy them there if not
								 *
								 * Note:
								 * ----
								 * This is trading off a slightly slower load time for not copying large numbers of matricies to the shader each time,
								 * so better FPS whilst running the thing
								 */
								for (int k = 0; k < matrixChain.Count; k++)
								{
									if (shape.AnimatedMatricies.ContainsKey(matrixChain[k]))
									{
										// replace with the actual index in the shape matrix array
										matrixChain[k] = shape.AnimatedMatricies[matrixChain[k]];
									}
									else
									{
										// copy matrix to shape and add to our dict
										int matrixIndex = newResult.Matricies.Length;
										Array.Resize(ref newResult.Matricies, matrixIndex + 1);
										newResult.Matricies[matrixIndex] = shape.Matricies[matrixChain[k]];
										shape.AnimatedMatricies.Add(matrixChain[k], matrixIndex);
										matrixChain[k] = matrixIndex;
									}
								}
								
								// used to pack 4 x matrix indicies into a int
								int[] transformChain = new int[] { 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255 };
								matrixChain.CopyTo(transformChain);
								if (matrixChain.Count > 4)
								{
									int b = 0;
								}
								transformedVertices[i].matrixChain = transformChain;
							}
							break;
						}
					}
				}
			}

			internal void Apply(out StaticObject Object, bool useTransformedVertics)
			{
				Object = new StaticObject(Plugin.CurrentHost)
				{
					Mesh =
					{
						Faces = new MeshFace[] { },
						Materials = new MeshMaterial[] { },
						Vertices = new VertexTemplate[] { }
					}
				};
				if (faces.Count != 0)
				{
					int mf = Object.Mesh.Faces.Length;
					int mm = Object.Mesh.Materials.Length;
					int mv = Object.Mesh.Vertices.Length;
					Array.Resize(ref Object.Mesh.Faces, mf + faces.Count);
					Array.Resize(ref Object.Mesh.Materials, mm + materials.Count);
					Array.Resize(ref Object.Mesh.Vertices, mv + verticies.Count);
					for (int i = 0; i < verticies.Count; i++)
					{
						if (useTransformedVertics)
						{
							//Use transformed vertices if we are not animated as will be faster
							if (transformedVertices[i].matrixChain != null)
							{
								Object.Mesh.Vertices[mv + i] = new AnimatedVertex(transformedVertices[i].Coordinates, verticies[i].TextureCoordinates, transformedVertices[i].matrixChain);
							}
							else
							{
								Object.Mesh.Vertices[mv + i] = new OpenBveApi.Objects.Vertex(transformedVertices[i].Coordinates, verticies[i].TextureCoordinates);
							}
							
						}
						else
						{
							Object.Mesh.Vertices[mv + i] = new OpenBveApi.Objects.Vertex(verticies[i].Coordinates, verticies[i].TextureCoordinates);
						}

					}

					int usedFaces = 0;
					for (int i = 0; i < faces.Count; i++)
					{
						bool canSquashFace = false;
						if (i > 0)
						{
							if (verticies[faces[i].Vertices[0]].matrixChain == verticies[faces[i - 1].Vertices[0]].matrixChain && faces[i].Material == faces[i - 1].Material)
							{
								// check the matrix chain of the first vertex of each face, and te 
								canSquashFace = true;
							}
						}

						if (canSquashFace)
						{
							int squashID = mf + usedFaces - 1;
							int oldLength = Object.Mesh.Faces[squashID].Vertices.Length;
							Object.Mesh.Faces[squashID].AppendVerticies(faces[i].Vertices);
							for (int k = 0; k < faces[i].Vertices.Length; k++)
							{
								Object.Mesh.Faces[squashID].Vertices[k + oldLength].Normal = verticies[faces[i].Vertices[k]].Normal;
								Object.Mesh.Faces[squashID].Vertices[k + oldLength].Index += (ushort)mv;
							}
						}
						else
						{
							Object.Mesh.Faces[mf + usedFaces] = new MeshFace(faces[i].Vertices, (ushort)faces[i].Material, FaceFlags.Triangles);
							for (int k = 0; k < faces[i].Vertices.Length; k++)
							{
								Object.Mesh.Faces[mf + usedFaces].Vertices[k].Normal = verticies[faces[i].Vertices[k]].Normal;
								Object.Mesh.Faces[mf + usedFaces].Vertices[k].Index += (ushort)mv;
							}
							
							Object.Mesh.Faces[mf + usedFaces].Material += (ushort)mm;
							usedFaces++;
						}
						
					}

					

					Array.Resize(ref Object.Mesh.Faces, mf + usedFaces);

					for (int i = 0; i < materials.Count; i++)
					{
						Object.Mesh.Materials[mm + i].Flags = materials[i].Flags;
						Object.Mesh.Materials[mm + i].Color = materials[i].Color;
						Object.Mesh.Materials[mm + i].TransparentColor = Color24.Black;
						Object.Mesh.Materials[mm + i].BlendMode = MeshMaterialBlendMode.Normal;
						if (materials[i].DaytimeTexture != null)
						{
							Plugin.CurrentHost.RegisterTexture(materials[i].DaytimeTexture, TextureParameters.NoChange, out OpenBveApi.Textures.Texture tday);
							Object.Mesh.Materials[mm + i].DaytimeTexture = tday;
						}
						else
						{
							Object.Mesh.Materials[mm + i].DaytimeTexture = null;
						}

						Object.Mesh.Materials[mm + i].EmissiveColor = materials[i].EmissiveColor;
						Object.Mesh.Materials[mm + i].NighttimeTexture = null;
						Object.Mesh.Materials[mm + i].GlowAttenuationData = materials[i].GlowAttenuationData;
						Object.Mesh.Materials[mm + i].WrapMode = materials[i].WrapMode;
					}
				}
			}

			internal readonly List<Vertex> verticies;
			internal readonly List<VertexSet> vertexSets;
			internal readonly List<Face> faces;
			internal readonly List<Material> materials;
			internal List<int> hierarchy;
			internal List<Vertex> transformedVertices;
		}

		private static string currentFolder;

		private static KeyframeAnimatedObject newResult;
		internal static string wagonFileDirectory;

		private static readonly string[] wheelsLinkedNodes = { "CON_ROD", "DRIVER_CONROD", "ECCENTRIC_ROD", "PISTON_ROD", "EXPANSION_LINK", "XHEAD_LINK", "COMBINATION_LEVER", "RADIUS_ARM", "EXPANSION_LINK", "W1_ECC", "W2_ECC", "W3_ECC", "W4_ECC" };

		private static bool IsAnimated(string matrixName)
		{
			if (matrixName.StartsWith("WHEELS", StringComparison.InvariantCultureIgnoreCase) || matrixName.StartsWith("ROD", StringComparison.InvariantCultureIgnoreCase) || matrixName.StartsWith("BOGIE", StringComparison.InvariantCultureIgnoreCase) || matrixName.StartsWith("PISTON", StringComparison.InvariantCultureIgnoreCase) || matrixName.StartsWith("PANTOGRAPH", StringComparison.InvariantCultureIgnoreCase) || wheelsLinkedNodes.Contains(matrixName, StringComparer.InvariantCultureIgnoreCase))
			{
				return true;
			}

			return false;
		}

		private static bool IsWheelLinked(string matrixName)
		{
			if (matrixName.StartsWith("WHEEL", StringComparison.InvariantCultureIgnoreCase) || matrixName.StartsWith("ROD", StringComparison.InvariantCultureIgnoreCase) || matrixName.StartsWith("PISTON", StringComparison.InvariantCultureIgnoreCase) || wheelsLinkedNodes.Contains(matrixName, StringComparer.InvariantCultureIgnoreCase))
			{
				return true;
			}

			return false;
		}

		internal static UnifiedObject ReadObject(string fileName)
		{
			MsTsShape shape = new MsTsShape();
			newResult = new KeyframeAnimatedObject(Plugin.CurrentHost);

			currentFolder = Path.GetDirectoryName(fileName);
			Stream fb = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);

			byte[] buffer = new byte[34];
			fb.Read(buffer, 0, 2);

			bool unicode = (buffer[0] == 0xFF && buffer[1] == 0xFE);

			string headerString;
			if (unicode)
			{
				fb.Read(buffer, 0, 32);
				headerString = Encoding.Unicode.GetString(buffer, 0, 16);
			}
			else
			{
				fb.Read(buffer, 2, 14);
				headerString = Encoding.ASCII.GetString(buffer, 0, 8);
			}

			// SIMISA@F  means compressed
			// SIMISA@@  means uncompressed
			if (headerString.StartsWith("SIMISA@F"))
			{
				fb = new ZlibStream(fb, CompressionMode.Decompress);
			}
			else if (headerString.StartsWith("\r\nSIMISA"))
			{
				// ie us1rd2l1000r10d.s, we are going to allow this but warn
				Console.Error.WriteLine("Improper header in " + fileName);
				fb.Read(buffer, 0, 4);
			}
			else if (!headerString.StartsWith("SIMISA@@"))
			{
				throw new Exception("Unrecognized shape file header " + headerString + " in " + fileName);
			}

			string subHeader;
			if (unicode)
			{
				fb.Read(buffer, 0, 32);
				subHeader = Encoding.Unicode.GetString(buffer, 0, 16);
			}
			else
			{
				fb.Read(buffer, 0, 16);
				subHeader = Encoding.ASCII.GetString(buffer, 0, 8);
			}
			if (subHeader[7] == 't')
			{
				using (StreamReader reader = new StreamReader(fb, unicode ? Encoding.Unicode : Encoding.ASCII))
				{
					string s = reader.ReadToEnd();
					TextualBlock block = new TextualBlock(s, KujuTokenID.shape);
					ParseBlock(block, ref shape);
				}

			}
			else if (subHeader[7] != 'b')
			{
				throw new Exception("Unrecognized subHeader \"" + subHeader + "\" in " + fileName);
			}
			else
			{
				using (BinaryReader reader = new BinaryReader(fb))
				{
					KujuTokenID currentToken = (KujuTokenID)reader.ReadUInt16();
					if (currentToken != KujuTokenID.shape)
					{
						throw new Exception(); //Shape definition
					}
					reader.ReadUInt16();
					uint remainingBytes = reader.ReadUInt32();
					byte[] newBytes = reader.ReadBytes((int)remainingBytes);
					BinaryBlock block = new BinaryBlock(newBytes, KujuTokenID.shape);
					ParseBlock(block, ref shape);
				}
			}
			
			

			int LOD = 0;
			double viewingDistance = double.MaxValue;
			for (int i = 0; i < shape.LODs.Count; i++)
			{
				if (shape.LODs[i].viewingDistance < viewingDistance)
				{
					LOD = i;
					viewingDistance = shape.LODs[i].viewingDistance;
				}
			}

			Array.Resize(ref newResult.Objects, shape.LODs[LOD].subObjects.Count);

			for (int j = 0; j < shape.LODs[LOD].subObjects.Count; j++)
			{
				ObjectState aos = new ObjectState();
				shape.LODs[LOD].subObjects[j].Apply(out aos.Prototype, true);
				newResult.Objects[j] = aos;
			}

			if (newResult.Animations.Count == 0)
			{
				// some objects have default wheels matricies defined but no animations e.g. default UK MK1 coaches
				// add them now, as MSTS animates these (yuck)
				for (int i = 0; i < shape.Matricies.Count; i++)
				{

					if (shape.Matricies[i].Name.StartsWith("WHEELS"))
					{
						KeyframeAnimation newAnimation = new KeyframeAnimation(newResult, -1, shape.Matricies[i].Name, 8, 60, shape.Matricies[i].Matrix, true);
						newAnimation.AnimationControllers = new[]
						{
							new TcbKey(shape.Matricies[i].Name, defaultWheelRotationFrames)
						};
						newResult.Animations.Add(i, newAnimation);
					}
				}
			}
			// extract pivots
			for (int i = 0; i < shape.Matricies.Count; i++)
			{
				string matrixName = shape.Matricies[i].Name;
				if (matrixName.StartsWith("BOGIE"))
				{
					// yuck: we seem to ignore anything after an underscore when looking at matrix names to animate
					// G84_ETR_521_1
					if (shape.Matricies[i].Name.IndexOf('_') != -1)
					{
						matrixName = matrixName.Substring(0, shape.Matricies[i].Name.IndexOf('_'));
					}

					int bogieIndex = 0;
					if (int.TryParse(matrixName.Substring(5), out int temp))
					{
						bogieIndex = temp;
					}

					double minWheel = 0, maxWheel = 0;
					for (int j = 0; j < shape.Matricies.Count; j++)
					{
						if (shape.Matricies[j].Name.Length == 8 && shape.Matricies[j].Name.StartsWith("WHEELS" + bogieIndex))
						{
							double z = shape.Matricies[j].Matrix.ExtractTranslation().Z;
							minWheel = Math.Min(z, minWheel);
							maxWheel = Math.Max(z, maxWheel);
						}
					}

					if (minWheel == 0 && maxWheel == 0)
					{
						// as wheel translation may not be specified
						newResult.Pivots.Add(shape.Matricies[i].Name, new PivotPoint(shape.Matricies[i].Name, shape.Matricies[i].Matrix.ExtractTranslation().Z, -2, 2));
					}
					else
					{
						newResult.Pivots.Add(shape.Matricies[i].Name, new PivotPoint(shape.Matricies[i].Name, shape.Matricies[i].Matrix.ExtractTranslation().Z, minWheel, maxWheel));
					}
						
					
				}
			}
			return newResult;
		}

		private static LOD currentLOD;

		private static void ParseBlock(Block block, ref MsTsShape shape)
		{
			Vertex v = null; //Crappy, but there we go.....
			int[] t = null;
			KeyframeAnimation node = null;
			ParseBlock(block, ref shape, ref v, ref t, ref node);
		}

		private static void ParseBlock(Block block, ref MsTsShape shape, ref int[] array)
		{
			Vertex v = null; //Crappy, but there we go.....
			KeyframeAnimation node = null;
			ParseBlock(block, ref shape, ref v, ref array, ref node);
		}

		private static void ParseBlock(Block block, ref MsTsShape shape, ref Vertex v)
		{
			int[] t = null;
			KeyframeAnimation node = null;
			KeyframeAnimation animation = null;
			ParseBlock(block, ref shape, ref v, ref t, ref animation);
		}

		private static void ParseBlock(Block block, ref MsTsShape shape, ref KeyframeAnimation n)
		{
			Vertex v = null;
			int[] t = null;
			ParseBlock(block, ref shape, ref v, ref t, ref n);
		}

		private static int NumFrames;

		private static QuaternionFrame[] quaternionFrames;

		private static VectorFrame[] vectorFrames;

		private static int controllerIndex;

		private static int currentFrame;

		private static int currentAnimationNode;

		private static void ParseBlock(Block block, ref MsTsShape shape, ref Vertex vertex, ref int[] intArray, ref KeyframeAnimation animationNode)
		{
			float x, y, z;
			Vector3 point;
			KujuTokenID currentToken = KujuTokenID.error;
			Block newBlock;
			uint flags;

			float a;
			float r;
			float g;
			float b;
			switch (block.Token)
			{
				case KujuTokenID.shape:
					newBlock = block.ReadSubBlock(KujuTokenID.shape_header);
					ParseBlock(newBlock, ref shape);
					newBlock = block.ReadSubBlock(KujuTokenID.volumes);
					ParseBlock(newBlock, ref shape);
					newBlock = block.ReadSubBlock(KujuTokenID.shader_names);
					ParseBlock(newBlock, ref shape);
					newBlock = block.ReadSubBlock(KujuTokenID.texture_filter_names);
					ParseBlock(newBlock, ref shape);
					newBlock = block.ReadSubBlock(KujuTokenID.points);
					ParseBlock(newBlock, ref shape);
					newBlock = block.ReadSubBlock(KujuTokenID.uv_points);
					ParseBlock(newBlock, ref shape);
					newBlock = block.ReadSubBlock(KujuTokenID.normals);
					ParseBlock(newBlock, ref shape);
					newBlock = block.ReadSubBlock(KujuTokenID.sort_vectors);
					ParseBlock(newBlock, ref shape);
					newBlock = block.ReadSubBlock(KujuTokenID.colours);
					ParseBlock(newBlock, ref shape);
					newBlock = block.ReadSubBlock(KujuTokenID.matrices);
					ParseBlock(newBlock, ref shape);
					newBlock = block.ReadSubBlock(KujuTokenID.images);
					ParseBlock(newBlock, ref shape);
					newBlock = block.ReadSubBlock(KujuTokenID.textures);
					ParseBlock(newBlock, ref shape);
					newBlock = block.ReadSubBlock(KujuTokenID.light_materials);
					ParseBlock(newBlock, ref shape);
					newBlock = block.ReadSubBlock(KujuTokenID.light_model_cfgs);
					ParseBlock(newBlock, ref shape);
					newBlock = block.ReadSubBlock(KujuTokenID.vtx_states);
					ParseBlock(newBlock, ref shape);
					newBlock = block.ReadSubBlock(KujuTokenID.prim_states);
					ParseBlock(newBlock, ref shape);
					newBlock = block.ReadSubBlock(KujuTokenID.lod_controls);
					ParseBlock(newBlock, ref shape);

					if ((block is BinaryBlock && block.Length() - block.Position() > 0) || (block is TextualBlock && block.Length() - block.Position() > 3))
					{
						try
						{
							newBlock = block.ReadSubBlock(KujuTokenID.animations);
							ParseBlock(newBlock, ref shape);
						}
						catch (EndOfStreamException)
						{
							// Animation controllers are optional
						}
					}
					break;
				case KujuTokenID.shape_header:
				case KujuTokenID.volumes:
				case KujuTokenID.texture_filter_names:
				case KujuTokenID.sort_vectors:
				case KujuTokenID.light_materials:
				case KujuTokenID.light_model_cfgs:
					//Unsupported stuff, so just read to the end at the minute
					block.Skip((int)block.Length());
					break;
				case KujuTokenID.colours:
					int numColors = block.ReadInt32();
					while (numColors > 0)
					{
						block.ReadSubBlock(KujuTokenID.colour);
						numColors--;
					}
					break;
				case KujuTokenID.colour:
					// NOTE: ARGB
					shape.colors.Add(block.ReadColorArgb());
					break;
				case KujuTokenID.shader_names:
					int numShaders = block.ReadInt32();
					while (numShaders > 0)
					{
						newBlock = block.ReadSubBlock(KujuTokenID.named_shader);
						ParseBlock(newBlock, ref shape);
						numShaders--;
					}
					break;
				case KujuTokenID.named_shader:
					shape.ShaderNames.Add(block.ReadEnumValue(default(ShaderNames)));
					break;
				case KujuTokenID.vtx_state:
					flags = block.ReadUInt32();
					VertexStates vs = new VertexStates(block.ReadInt32());
					int lightMaterialIdx = block.ReadInt32();
					int lightStateCfgIdx = block.ReadInt32();
					uint lightFlags = block.ReadUInt32();
					int matrix2 = -1;
					if ((block is BinaryBlock && block.Length() - block.Position() > 1) || (block is TextualBlock && block.Length() - block.Position() > 2))
					{
						matrix2 = block.ReadInt32();
					}

					vs.flags = flags;
					vs.lightingMatrixID = lightMaterialIdx;
					vs.lightingConfigIdx = lightStateCfgIdx;
					vs.lightingFlags = lightFlags;
					vs.matrix2ID = matrix2;
					shape.vtx_states.Add(vs);
					break;
				case KujuTokenID.vtx_states:
					int vtxStateCount = block.ReadUInt16();
					if (block is BinaryBlock)
					{
						block.ReadUInt16();
					}
					while (vtxStateCount > 0)
					{
						newBlock = block.ReadSubBlock(KujuTokenID.vtx_state);
						ParseBlock(newBlock, ref shape);
						vtxStateCount--;
					}

					break;
				case KujuTokenID.prim_state:
					flags = block.ReadUInt32();
					int shader = block.ReadInt32();
					int[] texIdxs = { };
					newBlock = block.ReadSubBlock(KujuTokenID.tex_idxs);
					ParseBlock(newBlock, ref shape, ref texIdxs);

					float zBias = block.ReadSingle();
					int vertexStates = block.ReadInt32();
					int alphaTestMode = block.ReadInt32();
					int lightCfgIdx = block.ReadInt32();
					int zBufferMode = block.ReadInt32();
					PrimitiveState p = new PrimitiveState(block.Label);
					p.Flags = flags;
					p.Shader = shader;
					p.Textures = texIdxs;
					p.ZBias = zBias;
					p.vertexStates = vertexStates;
					p.alphaTestMode = alphaTestMode;
					p.lightCfgIdx = lightCfgIdx;
					p.zBufferMode = zBufferMode;
					shape.prim_states.Add(p);
					break;
				case KujuTokenID.tex_idxs:
					int texIdxCount = block.ReadUInt16();
					if (block is BinaryBlock)
					{
						block.ReadUInt16();
					}
					Array.Resize(ref intArray, texIdxCount);
					int idx = 0;
					while (texIdxCount > 0)
					{
						intArray[idx] = block.ReadUInt16();
						idx++;
						texIdxCount--;
					}
					break;
				case KujuTokenID.prim_states:
					int primStateCount = block.ReadUInt16();
					if (block is BinaryBlock)
					{
						block.ReadUInt16();
					}
					while (primStateCount > 0)
					{
						newBlock = block.ReadSubBlock(KujuTokenID.prim_state);
						ParseBlock(newBlock, ref shape);
						primStateCount--;
					}

					break;
				case KujuTokenID.texture:
					Texture t = new Texture(shape.images[block.ReadInt32()]);
					int filterMode = (int)block.ReadUInt32();
					float mipmapLODBias = block.ReadSingle();
					uint borderColor = 0xff000000U;
					if (block.Length() - block.Position() > 1)
					{
						borderColor = block.ReadUInt32();
					}

					// Unpack border color
					// NOTE: RGBA
					r = borderColor % 256;
					g = (borderColor / 256) % 256;
					b = (borderColor / 256 / 256) % 256;
					a = (borderColor / 256 / 256 / 256) % 256;
					t.filterMode = filterMode;
					t.mipmapLODBias = (int)mipmapLODBias;
					t.borderColor = new Color32((byte)r, (byte)g, (byte)b, (byte)a);
					shape.textures.Add(t);
					break;
				case KujuTokenID.textures:
					int textureCount = block.ReadUInt16();
					if (block is BinaryBlock)
					{
						block.ReadUInt16();
					}
					while (textureCount > 0)
					{
						newBlock = block.ReadSubBlock(KujuTokenID.texture);
						ParseBlock(newBlock, ref shape);
						textureCount--;
					}

					break;
				case KujuTokenID.image:
					shape.images.Add(block.ReadString());
					break;
				case KujuTokenID.images:
					int imageCount = block.ReadUInt16();
					if (block is BinaryBlock)
					{
						block.ReadUInt16();
					}
					while (imageCount > 0)
					{
						newBlock = block.ReadSubBlock(KujuTokenID.image);
						ParseBlock(newBlock, ref shape);
						imageCount--;
					}

					break;
				case KujuTokenID.cullable_prims:
					int numPrims = block.ReadInt32();
					int numFlatSections = block.ReadInt32();
					int numPrimIdxs = block.ReadInt32();
					break;
				case KujuTokenID.geometry_info:
					int faceNormals = block.ReadInt32();
					int txLightCommands = block.ReadInt32();
					int nodeXTrilistIdxs = block.ReadInt32();
					int trilistIdxs = block.ReadInt32();
					int lineListIdxs = block.ReadInt32();
					nodeXTrilistIdxs = block.ReadInt32(); //Duped, or is the first one actually something else?
					int trilists = block.ReadInt32();
					int lineLists = block.ReadInt32();
					int pointLists = block.ReadInt32();
					int nodeXTrilists = block.ReadInt32();
					newBlock = block.ReadSubBlock(KujuTokenID.geometry_nodes);
					ParseBlock(newBlock, ref shape);
					newBlock = block.ReadSubBlock(KujuTokenID.geometry_node_map);
					ParseBlock(newBlock, ref shape);
					break;
				case KujuTokenID.geometry_node_map:
					int[] geometryNodes = new int[block.ReadInt32()];
					for (int i = 0; i < geometryNodes.Length; i++)
					{
						geometryNodes[i] = block.ReadInt32();
					}

					break;
				case KujuTokenID.geometry_node:
					int n_txLightCommands = block.ReadInt32();
					int n_nodeXTxLightCmds = block.ReadInt32();
					int n_trilists = block.ReadInt32();
					int n_lineLists = block.ReadInt32();
					int n_pointLists = block.ReadInt32();
					newBlock = block.ReadSubBlock(KujuTokenID.cullable_prims);
					ParseBlock(newBlock, ref shape);
					break;
				case KujuTokenID.geometry_nodes:
					int geometryNodeCount = block.ReadUInt16();
					if (block is BinaryBlock)
					{
						block.ReadUInt16();
					}
					while (geometryNodeCount > 0)
					{
						newBlock = block.ReadSubBlock(KujuTokenID.geometry_node);
						ParseBlock(newBlock, ref shape);
						geometryNodeCount--;
					}

					break;
				case KujuTokenID.point:
					shape.points.Add(block.ReadVector3());
					break;
				case KujuTokenID.vector:
					shape.normals.Add(block.ReadVector3());
					break;
				case KujuTokenID.points:
					int pointCount = block.ReadUInt16();
					if (block is BinaryBlock)
					{
						block.ReadUInt16();
					}
					while (pointCount > 0)
					{
						newBlock = block.ReadSubBlock(KujuTokenID.point);
						ParseBlock(newBlock, ref shape);
						pointCount--;
					}

					break;
				case KujuTokenID.uv_point:
					shape.uv_points.Add(block.ReadVector2());
					break;
				case KujuTokenID.uv_points:
					int uvPointCount = block.ReadUInt16();
					if (block is BinaryBlock)
					{
						block.ReadUInt16();
					}
					while (uvPointCount > 0)
					{
						newBlock = block.ReadSubBlock(KujuTokenID.uv_point);
						ParseBlock(newBlock, ref shape);
						uvPointCount--;
					}

					break;
				case KujuTokenID.matrices:
					int matrixCount = block.ReadUInt16();
					if (block is BinaryBlock)
					{
						block.ReadUInt16();
					}
					while (matrixCount > 0)
					{
						newBlock = block.ReadSubBlock(KujuTokenID.matrix);
						ParseBlock(newBlock, ref shape);
						matrixCount--;
					}
					break;
				case KujuTokenID.matrix:
					Matrix4D currentMatrix = new Matrix4D
					{
						Row0 = new Vector4(block.ReadSingle(), block.ReadSingle(), block.ReadSingle(), 0),
						Row1 = new Vector4(block.ReadSingle(), block.ReadSingle(), block.ReadSingle(), 0),
						Row2 = new Vector4(block.ReadSingle(), block.ReadSingle(), block.ReadSingle(), 0),
						Row3 = new Vector4(block.ReadSingle(), block.ReadSingle(), block.ReadSingle(), 0)
					};
					shape.Matricies.Add(new KeyframeMatrix(newResult, shape.Matricies.Count, block.Label, currentMatrix));
					break;
				case KujuTokenID.normals:
					int normalCount = block.ReadUInt16();
					if (block is BinaryBlock)
					{
						block.ReadUInt16();
					}
					while (normalCount > 0)
					{
						newBlock = block.ReadSubBlock(KujuTokenID.vector);
						ParseBlock(newBlock, ref shape);
						normalCount--;
					}

					break;
				case KujuTokenID.distance_levels_header:
					int DLevBias = block.ReadInt16();
					break;
				case KujuTokenID.distance_levels:
					int distanceLevelCount = block.ReadInt16();
					if (block is BinaryBlock)
					{
						block.ReadUInt16();
					}
					while (distanceLevelCount > 0)
					{
						newBlock = block.ReadSubBlock(KujuTokenID.distance_level);
						ParseBlock(newBlock, ref shape);
						distanceLevelCount--;
					}

					break;
				case KujuTokenID.distance_level_header:
					newBlock = block.ReadSubBlock(KujuTokenID.dlevel_selection);
					ParseBlock(newBlock, ref shape);
					newBlock = block.ReadSubBlock(KujuTokenID.hierarchy);
					ParseBlock(newBlock, ref shape);
					break;
				case KujuTokenID.distance_level:
					newBlock = block.ReadSubBlock(KujuTokenID.distance_level_header);
					ParseBlock(newBlock, ref shape);
					newBlock = block.ReadSubBlock(KujuTokenID.sub_objects);
					ParseBlock(newBlock, ref shape);
					shape.LODs.Add(currentLOD);
					break;
				case KujuTokenID.dlevel_selection:
					currentLOD = new LOD(block.ReadSingle());
					break;
				case KujuTokenID.hierarchy:
					currentLOD.hierarchy = new int[block.ReadInt32()];
					for (int i = 0; i < currentLOD.hierarchy.Length; i++)
					{
						currentLOD.hierarchy[i] = block.ReadInt32();
					}

					break;
				case KujuTokenID.lod_control:
					newBlock = block.ReadSubBlock(KujuTokenID.distance_levels_header);
					ParseBlock(newBlock, ref shape);
					newBlock = block.ReadSubBlock(KujuTokenID.distance_levels);
					ParseBlock(newBlock, ref shape);
					break;
				case KujuTokenID.lod_controls:
					int lodCount = block.ReadInt16();
					if (block is BinaryBlock)
					{
						block.ReadUInt16();
					}
					while (lodCount > 0)
					{
						newBlock = block.ReadSubBlock(KujuTokenID.lod_control);
						ParseBlock(newBlock, ref shape);
						lodCount--;
					}

					break;
				case KujuTokenID.primitives:
					int capacity = block.ReadInt32(); //Count of the number of entries in the block, not the number of primitives
					while (capacity > 0)
					{
						newBlock = block.ReadSubBlock(new[] { KujuTokenID.prim_state_idx, KujuTokenID.indexed_trilist });
						switch (newBlock.Token)
						{
							case KujuTokenID.prim_state_idx:
								ParseBlock(newBlock, ref shape);
								string txF = null;
								try
								{
									if (shape.prim_states[shape.currentPrimitiveState].Textures.Length > 0)
									{
										txF = OpenBveApi.Path.CombineFile(currentFolder, shape.textures[shape.prim_states[shape.currentPrimitiveState].Textures[0]].fileName);
										if (!File.Exists(txF) && !string.IsNullOrEmpty(wagonFileDirectory))
										{
											// yuck: MSTS texture paths resolve relative to the WAG / ENG file if part of a train, even if the S file is not in the same directory
											//		Only try this if we can't find the texture file by a simple relative combine
											txF = OpenBveApi.Path.CombineFile(wagonFileDirectory, shape.textures[shape.prim_states[shape.currentPrimitiveState].Textures[0]].fileName);
										}
										if (!File.Exists(txF))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, true, "Texture file " + shape.textures[shape.prim_states[shape.currentPrimitiveState].Textures[0]].fileName + " was not found.");
											txF = null;
										}
									}
								}
								catch
								{
									Plugin.CurrentHost.AddMessage(MessageType.Warning, true, "Texture file path " + shape.textures[shape.prim_states[shape.currentPrimitiveState].Textures[0]].fileName + " was invalid.");
								}

								Material mat = new Material(txF);
								switch (shape.ShaderNames[shape.prim_states[shape.currentPrimitiveState].Shader])
								{
									case ShaderNames.Tex:
										mat.Flags |= MaterialFlags.DisableTextureAlpha;
										mat.Flags |= MaterialFlags.DisableLighting;
										break;
									case ShaderNames.TexDiff:
										mat.Flags |= MaterialFlags.DisableTextureAlpha;
										break;
									case ShaderNames.BlendATex:
										mat.Flags |= MaterialFlags.DisableLighting;
										break;
									case ShaderNames.BlendATexDiff:
										// Default material
										break;
									case ShaderNames.AddATex:
										mat.Flags |= MaterialFlags.Emissive;
										mat.Flags |= MaterialFlags.DisableLighting;
										break;
									case ShaderNames.AddATexDiff:
										mat.Flags |= MaterialFlags.Emissive;
										break;
								}
								currentLOD.subObjects[currentLOD.subObjects.Count - 1].materials.Add(mat);
								break;
							case KujuTokenID.indexed_trilist:
								ParseBlock(newBlock, ref shape);
								break;
							default:
								throw new Exception("Unexpected primitive type, got " + currentToken);
						}

						capacity--;
					}

					break;
				case KujuTokenID.prim_state_idx:
					shape.currentPrimitiveState = block.ReadInt32();
					break;
				case KujuTokenID.indexed_trilist:
					newBlock = block.ReadSubBlock(KujuTokenID.vertex_idxs);
					ParseBlock(newBlock, ref shape);
					newBlock = block.ReadSubBlock(KujuTokenID.normal_idxs);
					ParseBlock(newBlock, ref shape);
					break;
				case KujuTokenID.sub_object:
					newBlock = block.ReadSubBlock(KujuTokenID.sub_object_header);
					ParseBlock(newBlock, ref shape);
					newBlock = block.ReadSubBlock(KujuTokenID.vertices);
					ParseBlock(newBlock, ref shape);
					newBlock = block.ReadSubBlock(KujuTokenID.vertex_sets);
					ParseBlock(newBlock, ref shape);
					newBlock = block.ReadSubBlock(KujuTokenID.primitives);
					ParseBlock(newBlock, ref shape);
					break;
				case KujuTokenID.sub_objects:
					int subObjectCount = block.ReadInt16();
					if (block is BinaryBlock)
					{
						block.ReadUInt16();
					}
					while (subObjectCount > 0)
					{
						newBlock = block.ReadSubBlock(KujuTokenID.sub_object);
						ParseBlock(newBlock, ref shape);
						subObjectCount--;
					}

					break;
				case KujuTokenID.sub_object_header:
					currentLOD.subObjects.Add(new SubObject());
					shape.totalObjects++;
					flags = block.ReadUInt32();
					int sortVectorIdx = block.ReadInt32();
					int volIdx = block.ReadInt32();
					uint sourceVertexFormatFlags = block.ReadUInt32();
					uint destinationVertexFormatFlags = block.ReadUInt32();
					newBlock = block.ReadSubBlock(KujuTokenID.geometry_info);
					ParseBlock(newBlock, ref shape);
					/*
					 * Optional stuff, need to check if we're running off the end of the stream before reading each block
					 */
					if (block.Length() - block.Position() > 1)
					{
						newBlock = block.ReadSubBlock(KujuTokenID.subobject_shaders);
						ParseBlock(newBlock, ref shape);
					}

					if (block.Length() - block.Position() > 1)
					{
						newBlock = block.ReadSubBlock(KujuTokenID.subobject_light_cfgs);
						ParseBlock(newBlock, ref shape);
					}

					if (block.Length() - block.Position() > 1)
					{
						int subObjectID = block.ReadInt32();
					}

					break;
				case KujuTokenID.subobject_light_cfgs:
					int[] subobject_light_cfgs = new int[block.ReadInt32()];
					for (int i = 0; i < subobject_light_cfgs.Length; i++)
					{
						subobject_light_cfgs[i] = block.ReadInt32();
					}

					break;
				case KujuTokenID.subobject_shaders:
					int[] subobject_shaders = new int[block.ReadInt32()];
					for (int i = 0; i < subobject_shaders.Length; i++)
					{
						subobject_shaders[i] = block.ReadInt32();
					}

					break;
				case KujuTokenID.vertices:
					int vertexCount = block.ReadInt16();
					if (block is BinaryBlock)
					{
						block.ReadUInt16();
					}
					while (vertexCount > 0)
					{
						newBlock = block.ReadSubBlock(KujuTokenID.vertex);
						ParseBlock(newBlock, ref shape);
						vertexCount--;
					}

					break;
				case KujuTokenID.vertex:
					flags = block.ReadUInt32(); //Various control variables, not supported
					int myPoint = block.ReadInt32(); //Index to points array
					int myNormal = block.ReadInt32(); //Index to normals array

					Vertex v = new Vertex(shape.points[myPoint], shape.normals[myNormal]);
					/*
					 * colors used in lighting (integer representation of color)
					 * Color1 fades to Color2 with distance
					 * TODO: not yet implemented
					 */
					uint Color1 = block.ReadUInt32();
					uint Color2 = block.ReadUInt32();
					newBlock = block.ReadSubBlock(KujuTokenID.vertex_uvs);
					ParseBlock(newBlock, ref shape, ref v);
					currentLOD.subObjects[currentLOD.subObjects.Count - 1].verticies.Add(v);
					break;
				case KujuTokenID.vertex_idxs:
					int remainingVertex = block.ReadInt32() / 3;
					while (remainingVertex > 0)
					{
						int v1 = block.ReadInt32();
						int v2 = block.ReadInt32();
						int v3 = block.ReadInt32();

						currentLOD.subObjects[currentLOD.subObjects.Count - 1].faces.Add(new Face(new[] { v1, v2, v3 }, currentLOD.subObjects[currentLOD.subObjects.Count - 1].materials.Count - 1));
						remainingVertex--;
					}

					break;
				case KujuTokenID.vertex_set:
					int vertexStateIndex = block.ReadInt32(); //Index to the vtx_states member
					VertexSet vts = new VertexSet(shape.vtx_states[vertexStateIndex].hierarchyID); //Now pull the hierachy ID out
					int setStartVertexIndex = block.ReadInt32(); //First vertex
					int setVertexCount = block.ReadInt32(); //Total number of vert
					vts.startVertex = setStartVertexIndex;
					vts.numVerticies = setVertexCount;
					currentLOD.subObjects[currentLOD.subObjects.Count - 1].vertexSets.Add(vts);
					break;
				case KujuTokenID.vertex_sets:
					int vertexSetCount = block.ReadInt16();
					if (block is BinaryBlock)
					{
						block.ReadUInt16();
					}
					while (vertexSetCount > 0)
					{
						newBlock = block.ReadSubBlock(KujuTokenID.vertex_set);
						ParseBlock(newBlock, ref shape);
						vertexSetCount--;
					}

					//We now need to transform our verticies
					currentLOD.subObjects[currentLOD.subObjects.Count - 1].TransformVerticies(shape);
					break;
				case KujuTokenID.vertex_uvs:
					int[] vertex_uvs = new int[block.ReadInt32()];
					for (int i = 0; i < vertex_uvs.Length; i++)
					{
						vertex_uvs[i] = block.ReadInt32();
					}

					if (vertex_uvs.Length > 0 && vertex_uvs[0] <= shape.uv_points.Count)
					{
						//Looks as if vertex_uvs should always be of length 1, thus:
						vertex.TextureCoordinates = shape.uv_points[vertex_uvs[0]];
					}
					break;

				/*
				 * ANIMATION CONTROLLERS AND RELATED STUFF
				 */

				case KujuTokenID.animations:
					int numAnimations = block.ReadInt32();
					for (int i = 0; i < numAnimations; i++)
					{
						newBlock = block.ReadSubBlock(KujuTokenID.animation);
						ParseBlock(newBlock, ref shape);
					}
					break;
				case KujuTokenID.animation:
					Animation animation = new Animation
					{
						FrameCount = block.ReadInt32(),
						FrameRate = block.ReadInt32(),
						Nodes = new Dictionary<string, KeyframeAnimation>()
					};
					shape.Animations.Add(animation);
					newBlock = block.ReadSubBlock(KujuTokenID.anim_nodes);
					ParseBlock(newBlock, ref shape);
					break;
				case KujuTokenID.anim_nodes:
					int numNodes = block.ReadInt32();
					for (int i = 0; i < numNodes; i++)
					{
						// index for currentAnimationNode maps to the main shape matricies
						currentAnimationNode = i;
						newBlock = block.ReadSubBlock(KujuTokenID.anim_node);
						ParseBlock(newBlock, ref shape);
					}
					break;
				case KujuTokenID.anim_node:
					Matrix4D matrix = shape.Matricies[currentAnimationNode].Matrix;

					int parentAnimation = -1;
					if (shape.MatrixParents.ContainsKey(currentAnimationNode))
					{
						if (block.Label.StartsWith("WHEEL", StringComparison.InvariantCultureIgnoreCase))
						{
							// WHEELS cannot have a parent animation
							parentAnimation = -1;
						}
						else
						{
							parentAnimation = shape.MatrixParents[currentAnimationNode];
						}
					}
					else
					{
						if (block.Label != "MAIN")
						{
							if (block.Label.StartsWith("ROD", StringComparison.InvariantCultureIgnoreCase) || block.Label.StartsWith("PISTON", StringComparison.InvariantCultureIgnoreCase) || wheelsLinkedNodes.Contains(block.Label, StringComparer.InvariantCultureIgnoreCase))
							{
								// Undocumented 'feature': rod and piston, if not linked to a parent in the shape
								// file seem to link to WHEELS1 to determine animation key
								// see also the list in wheelsLinkedNodes (OR + MSTSBin)
								for (int i = 0; i < shape.Matricies.Count; i++)
								{
									if (shape.Matricies[i].Name.Equals("WHEELS1", StringComparison.InvariantCultureIgnoreCase))
									{
										parentAnimation = i;
										break;
									}
								}
							}
							else
							{
								parentAnimation = 0;
							}
							
						}
					}
					KeyframeAnimation currentNode = new KeyframeAnimation(newResult, parentAnimation, block.Label, shape.Animations[shape.Animations.Count - 1].FrameCount, shape.Animations[shape.Animations.Count - 1].FrameRate, matrix, IsWheelLinked(block.Label));
					newBlock = block.ReadSubBlock(KujuTokenID.controllers);
					ParseBlock(newBlock, ref shape, ref currentNode);
					if (currentNode.AnimationControllers.Length != 0)
					{
						if (!newResult.Animations.ContainsKey(currentAnimationNode))
						{
							newResult.Animations.Add(currentAnimationNode, currentNode);
						}
						else
						{
							// Duplicate 'real' animation frame encountered- see note about single identity quaternion below
							Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Duplicate animation key " + block.Label + " encountered in MSTS Object file.");
						}
					}
					else
					{
						if (currentNode.Name.StartsWith("WHEELS", StringComparison.InvariantCultureIgnoreCase))
						{
							// some objects, e.g. the default Class 50 provide a wheel animation with no controllers
							// re-create and add a default 8 frame animation (as in some with this 'issue' the frame count / rate is also wrong...)
							currentNode = new KeyframeAnimation(newResult, parentAnimation, block.Label, 8, 60, matrix, true);
							currentNode.AnimationControllers = new AbstractAnimation[]
							{
								new TcbKey(currentNode.Name, defaultWheelRotationFrames)
							};
							
							newResult.Animations.Add(currentAnimationNode, currentNode);
						}
					}
					break;
				case KujuTokenID.controllers:
					int numControllers = block.ReadInt32();
					animationNode.AnimationControllers = new AbstractAnimation[numControllers];
					controllerIndex = 0;
					for (int i = 0; i < numControllers; i++)
					{
						newBlock = block.ReadSubBlock(new[] { KujuTokenID.tcb_rot, KujuTokenID.linear_pos });
						ParseBlock(newBlock, ref shape, ref animationNode);
					}
					Array.Resize(ref animationNode.AnimationControllers, controllerIndex);
					break;
				case KujuTokenID.tcb_rot:
					NumFrames = block.ReadInt32();
					quaternionFrames = new QuaternionFrame[NumFrames];
					bool isSlerpRot = false;
					for (currentFrame = 0; currentFrame < NumFrames; currentFrame++)
					{
						newBlock = block.ReadSubBlock(new[] { KujuTokenID.tcb_key, KujuTokenID.slerp_rot });
						ParseBlock(newBlock, ref shape, ref animationNode);
						if (newBlock.Token == KujuTokenID.slerp_rot)
						{
							isSlerpRot = true;
						}
					}

					if (isSlerpRot)
					{
						animationNode.AnimationControllers[controllerIndex] = new SlerpRot(animationNode.Name, quaternionFrames);
					}
					else
					{
						if (quaternionFrames.Length == 1 && quaternionFrames[0].Quaternion == Quaternion.DirectXIdentity)
						{
							// If we contain a single frame containing the Identity quaternion, this isn't actually an animation
							// but is just a frame allowing sub-parts of the object to be grouped
							// Don't add it to the list of animations in this case
							// Found in BR_9f_92150.s stated to be built with TSM / polymaster
							break;
						}
						animationNode.AnimationControllers[controllerIndex] = new TcbKey(animationNode.Name, quaternionFrames);
					}

					controllerIndex++;
					break;
				case KujuTokenID.tcb_key:
					// Frame index
					int frameIndex = block.ReadInt32();
					// n.b. we need to negate the W components to get to GL format as opposed to DX
					Quaternion q = new Quaternion(block.ReadSingle(), block.ReadSingle(), block.ReadSingle(), -block.ReadSingle());
					quaternionFrames[currentFrame] = new QuaternionFrame(frameIndex, q);
					/* 4 more floats:
					 * TENSION
					 * CONTINUITY
					 * IN
					 * OUT
					 */
					break;
				case KujuTokenID.slerp_rot:
					// Frame index
					frameIndex = block.ReadInt32();
					q = new Quaternion(block.ReadSingle(), block.ReadSingle(), block.ReadSingle(), -block.ReadSingle());
					quaternionFrames[currentFrame] = new QuaternionFrame(frameIndex, q);
					break;
				case KujuTokenID.linear_pos:
					NumFrames = block.ReadInt32();
					vectorFrames = new VectorFrame[NumFrames];
					for (currentFrame = 0; currentFrame < NumFrames; currentFrame++)
					{
						newBlock = block.ReadSubBlock(KujuTokenID.linear_key);
						ParseBlock(newBlock, ref shape, ref animationNode);
					}
					animationNode.AnimationControllers[controllerIndex] = new LinearKey(animationNode.Name, vectorFrames);
					controllerIndex++;
					break;
				case KujuTokenID.linear_key:
					// Frame index
					frameIndex = block.ReadInt32();
					vectorFrames[currentFrame] = new VectorFrame(frameIndex, block.ReadVector3());
					break;
			}
		}
	}
}
#pragma warning restore 0219
