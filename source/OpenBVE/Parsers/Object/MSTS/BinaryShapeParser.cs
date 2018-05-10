using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using OpenBveApi.Math;
using OpenBveApi.Colors;
using SharpCompress.Compressor.Deflate;

namespace OpenBve
{
	class BinaryShapeParser
	{
		struct Matrix
		{
			internal string Name;
			internal Vector3 A;
			internal Vector3 B;
			internal Vector3 C;
			internal Vector3 D;
		}

		struct Texture
		{
			internal string fileName;
			internal int filterMode;
			internal int mipmapLODBias;
			internal Color32 borderColor;
		}

		struct PrimitiveState
		{
			internal string Name;
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
		}

		struct VertexStates
		{
			internal uint flags; //Describes specular and some other stuff, unlikely to be supported
			internal int hierarchyID; //The hierarchy ID of the top-level transform matrix, remember that they chain
			internal int lightingMatrixID;
			internal int lightingConfigIdx;
			internal uint lightingFlags;
			internal int matrix2ID; //Optional
		}

		struct VertexSet
		{
			internal int hierarchyIndex;
			internal int startVertex;
			internal int numVerticies;
		}

		class Vertex
		{
			internal Vector3 Coordinates;
			internal Vector3 Normal;
			internal Vector2 TextureCoordinates;
			
			public Vertex(Vector3 c, Vector3 n)
			{
				this.Coordinates = new Vector3(c.X, c.Y, c.Z);
				this.Normal = new Vector3(n.X, n.Y, n.Z);
			}
		}
		
		private class Material {
			internal Color32 Color;
			internal Color24 EmissiveColor;
			internal bool EmissiveColorUsed;
			internal string DaytimeTexture;
			internal string NighttimeTexture;
			internal World.MeshMaterialBlendMode BlendMode;
			internal Textures.OpenGlTextureWrapMode? WrapMode;
			internal ushort GlowAttenuationData;
			internal Material() {
				this.Color = new Color32(255, 255, 255, 255);
				this.EmissiveColor = new Color24(0, 0, 0);
				this.EmissiveColorUsed = false;
				this.DaytimeTexture = null;
				this.NighttimeTexture = null;
				this.BlendMode = World.MeshMaterialBlendMode.Normal;
				this.GlowAttenuationData = 0;
				this.WrapMode = null;
			}
		}
		private class MeshBuilder {
			internal World.Vertex[] Vertices;
			internal World.MeshFace[] Faces;
			internal Material[] Materials;
			internal double LODValue = 0;
			internal MeshBuilder() {
				this.Vertices = new World.Vertex[] { };
				this.Faces = new World.MeshFace[] { };
				this.Materials = new Material[] { new Material() };
			}

			internal void Apply(out ObjectManager.StaticObject Object) {
			Object = new ObjectManager.StaticObject
				{
					Mesh =
					{
						Faces = new World.MeshFace[] {},
						Materials = new World.MeshMaterial[] {},
						Vertices = new World.Vertex[] {}
					}
				};
			if (Faces.Length != 0) {
				int mf = Object.Mesh.Faces.Length;
				int mm = Object.Mesh.Materials.Length;
				int mv = Object.Mesh.Vertices.Length;
				Array.Resize<World.MeshFace>(ref Object.Mesh.Faces, mf + Faces.Length);
				Array.Resize<World.MeshMaterial>(ref Object.Mesh.Materials, mm + Materials.Length);
				Array.Resize<World.Vertex>(ref Object.Mesh.Vertices, mv + Vertices.Length);
				for (int i = 0; i < Vertices.Length; i++) {
					Object.Mesh.Vertices[mv + i] = Vertices[i];
				}
				for (int i = 0; i < Faces.Length; i++) {
					Object.Mesh.Faces[mf + i] = Faces[i];
					for (int j = 0; j < Object.Mesh.Faces[mf + i].Vertices.Length; j++) {
						Object.Mesh.Faces[mf + i].Vertices[j].Index += (ushort)mv;
					}
					Object.Mesh.Faces[mf + i].Material += (ushort)mm;
				}
				for (int i = 0; i < Materials.Length; i++) {
					Object.Mesh.Materials[mm + i].Flags = (byte)((Materials[i].EmissiveColorUsed ? World.MeshMaterial.EmissiveColorMask : 0) | 0);
					Object.Mesh.Materials[mm + i].Color = Materials[i].Color;
					if (Materials[i].DaytimeTexture != null)
					{
						Textures.Texture tday;
						Textures.RegisterTexture(Materials[i].DaytimeTexture, out tday);
						Object.Mesh.Materials[mm + i].DaytimeTexture = tday;
					}
					else
					{
						Object.Mesh.Materials[mm + i].DaytimeTexture = null;
					}
					Object.Mesh.Materials[mm + i].EmissiveColor = Materials[i].EmissiveColor;
					if (Materials[i].NighttimeTexture != null) {
						Textures.Texture tnight;
						Textures.RegisterTexture(Materials[i].NighttimeTexture, out tnight);
						Object.Mesh.Materials[mm + i].NighttimeTexture = tnight;
					} else {
						Object.Mesh.Materials[mm + i].NighttimeTexture = null;
					}
					Object.Mesh.Materials[mm + i].DaytimeNighttimeBlend = 0;
					Object.Mesh.Materials[mm + i].BlendMode = Materials[i].BlendMode;
					Object.Mesh.Materials[mm + i].GlowAttenuationData = Materials[i].GlowAttenuationData;
					Object.Mesh.Materials[mm + i].WrapMode = Materials[i].WrapMode;
				}
			}
		}
		}

		private class MsTsShape
		{
			internal MsTsShape()
			{
				points = new List<Vector3>();
				normals = new List<Vector3>();
				uv_points = new List<Vector2>();
				matrices = new List<Matrix>();
				images = new List<string>();
				textures = new List<Texture>();
				prim_states = new List<PrimitiveState>();
				vtx_states = new List<VertexStates>();
				LODs = new List<LOD>();
			}

			// Global variables used by all LODs

			/// <summary>The points used by this shape</summary>
			internal readonly List<Vector3> points;
			/// <summary>The normal vectors used by this shape</summary>
			internal readonly List<Vector3> normals;
			/// <summary>The texture-coordinates used by this shape</summary>
			internal readonly List<Vector2> uv_points;
			/// <summary>The matrices used to transform components of this shape</summary>
			internal List<Matrix> matrices;
			/// <summary>The filenames of all textures used by this shape</summary>
			internal List<string> images;
			/// <summary>The textures used, with associated parameters</summary>
			/// <remarks>Allows the alpha testing mode to be set etc. so that the same image file can be reused</remarks>
			internal List<Texture> textures;
			/// <summary>Contains the shader, texture etc. used by the primitive</summary>
			/// <remarks>Largely unsupported other than the texture name at the minute</remarks>
			internal List<PrimitiveState> prim_states;

			internal List<VertexStates> vtx_states;
			
			// The list of LODs actually containing the objects

			/// <summary>The list of all LODs from the model</summary>
			internal List<LOD> LODs;

			// Control variables

			internal int currentPrimitiveState = -1;
			internal int totalObjects = 0;
		}

		private class LOD
		{
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
				this.faces = new List<int[]>();
				this.meshBuilder = new MeshBuilder();
			}

			internal void TransformVerticies(List<Matrix> matrices)
			{
				//TODO: This moves the verticies
				//We should actually split them and the associated faces and use position within our animated object instead
				//This however works when we have no animation supported as per currently
				for (int i = 0; i < verticies.Count; i++)
				{
					for (int j = 0; j < vertexSets.Count; j++)
					{
						if (vertexSets[j].startVertex <= i && vertexSets[j].startVertex + vertexSets[j].numVerticies > i)
						{
							List<int> matrixChain = new List<int>();
							int hi = vertexSets[j].hierarchyIndex - 1;
							if (hi != -1)
							{
								matrixChain.Add(hi);
								while (hi != -1)
								{
									hi = currentLOD.hierarchy[hi];
									if (hi == -1)
									{
										break;
									}

									matrixChain.Insert(0, hi);
								}
							}

							for (int k = 0; k < matrixChain.Count; k++)
							{
								verticies[i].Coordinates += matrices[matrixChain[k]].D;
							}

							break;
						}
					}
				}
			}

			internal void CreateMeshBuilder(double lodValue)
			{
				meshBuilder.LODValue = lodValue;
				for (int i = 0; i < verticies.Count; i++)
				{
					meshBuilder.Vertices[i].Coordinates = verticies[i].Coordinates;
					meshBuilder.Vertices[i].TextureCoordinates = verticies[i].TextureCoordinates;
					Array.Resize(ref meshBuilder.Faces, faces.Count);
					for (int j = 0; j < faces.Count; j++)
					{
						meshBuilder.Faces[j] = new World.MeshFace(faces[j]);
						for (int k = 0; k < faces[j].Length; k++)
						{
							meshBuilder.Faces[j].Vertices[k].Normal = verticies[faces[j][k]].Normal;
						}
					}
				}
			}
			internal List<Vertex> verticies;
			internal List<VertexSet> vertexSets;
			internal List<int[]> faces;

			internal MeshBuilder meshBuilder;
		}

		private static string currentFolder;
		internal static ObjectManager.AnimatedObjectCollection ReadObject(string fileName)
		{
			ObjectManager.AnimatedObjectCollection Result;
			MsTsShape shape = new MsTsShape();
			Result = new ObjectManager.AnimatedObjectCollection
			{
				Objects = new ObjectManager.AnimatedObject[4]
			};

			currentFolder = Path.GetDirectoryName(fileName);
			Stream fb = new FileStream(fileName, FileMode.Open, FileAccess.Read);

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
				fb = new ZlibStream(fb, SharpCompress.Compressor.CompressionMode.Decompress);
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

            // Read SubHeader
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

            // Select for binary vs text content
            if (subHeader[7] == 't')
            {
                //return new UnicodeFileReader(fb, file, unicode ? Encoding.Unicode : Encoding.ASCII);
            }
            else if (subHeader[7] != 'b')
            {
                throw new Exception("Unrecognized subHeader \"" + subHeader + "\" in " + fileName);
            }
			KujuTokenID currentToken;
			using (BinaryReader reader = new BinaryReader(fb))
			{
				currentToken = (KujuTokenID) reader.ReadUInt16();
				if (currentToken != KujuTokenID.shape)
				{
					throw new Exception(); //Shape definition
				}

				reader.ReadUInt16();
				reader.ReadUInt32();
				reader.ReadByte();
				currentToken = (KujuTokenID) reader.ReadUInt16();
				if (currentToken != KujuTokenID.shape_header)
				{
					throw new Exception("Expected the shape_header token, got " + currentToken);
				}

				reader.ReadUInt16();
				uint remainingBytes = reader.ReadUInt32();
				reader.ReadBytes((int) remainingBytes); //Should be all zeroes
				currentToken = (KujuTokenID) reader.ReadUInt16();
				if (currentToken != KujuTokenID.volumes)
				{
					throw new Exception("Expected the volumes token, got " + currentToken);
				}

				reader.ReadUInt16();
				remainingBytes = reader.ReadUInt32();
				byte[] newBytes = reader.ReadBytes((int) remainingBytes);
				currentToken = (KujuTokenID) reader.ReadUInt16();
				if (currentToken != KujuTokenID.shader_names)
				{
					throw new Exception("Expected the shader_names token, got " + currentToken);
				}

				reader.ReadUInt16();
				remainingBytes = reader.ReadUInt32();
				newBytes = reader.ReadBytes((int) remainingBytes);
				currentToken = (KujuTokenID) reader.ReadUInt16();
				if (currentToken != KujuTokenID.texture_filter_names)
				{
					throw new Exception("Expected the texture_filter_names token, got " + currentToken);
				}

				reader.ReadUInt16();
				remainingBytes = reader.ReadUInt32();
				newBytes = reader.ReadBytes((int) remainingBytes);
				currentToken = (KujuTokenID) reader.ReadUInt16();
				if (currentToken != KujuTokenID.points)
				{
					throw new Exception("Expected the points token, got " + currentToken);
				}

				reader.ReadUInt16();
				remainingBytes = reader.ReadUInt32();
				newBytes = reader.ReadBytes((int) remainingBytes);
				ReadSubBlock(newBytes, KujuTokenID.points, ref shape);
				currentToken = (KujuTokenID) reader.ReadUInt16();
				if (currentToken != KujuTokenID.uv_points)
				{
					throw new Exception("Expected the uv_points token, got " + currentToken);
				}

				reader.ReadUInt16();
				remainingBytes = reader.ReadUInt32();
				newBytes = reader.ReadBytes((int) remainingBytes);
				ReadSubBlock(newBytes, KujuTokenID.uv_points, ref shape);
				currentToken = (KujuTokenID) reader.ReadUInt16();
				if (currentToken != KujuTokenID.normals)
				{
					throw new Exception("Expected the normals token, got " + currentToken);
				}

				reader.ReadUInt16();
				remainingBytes = reader.ReadUInt32();
				newBytes = reader.ReadBytes((int) remainingBytes);
				ReadSubBlock(newBytes, KujuTokenID.normals, ref shape);
				currentToken = (KujuTokenID) reader.ReadUInt16();
				if (currentToken != KujuTokenID.sort_vectors)
				{
					throw new Exception("Expected the sort_vectors token, got " + currentToken);
				}

				reader.ReadUInt16();
				remainingBytes = reader.ReadUInt32();
				newBytes = reader.ReadBytes((int) remainingBytes);
				currentToken = (KujuTokenID) reader.ReadUInt16();
				if (currentToken != KujuTokenID.colours)
				{
					throw new Exception("Expected the colours token, got " + currentToken);
				}

				reader.ReadUInt16();
				remainingBytes = reader.ReadUInt32();
				newBytes = reader.ReadBytes((int) remainingBytes);
				currentToken = (KujuTokenID) reader.ReadUInt16();
				if (currentToken != KujuTokenID.matrices)
				{
					throw new Exception("Expected the matricies token, got " + currentToken);
				}

				reader.ReadUInt16();
				remainingBytes = reader.ReadUInt32();
				newBytes = reader.ReadBytes((int) remainingBytes);
				ReadSubBlock(newBytes, KujuTokenID.matrices, ref shape);
				currentToken = (KujuTokenID) reader.ReadUInt16();
				if (currentToken != KujuTokenID.images)
				{
					throw new Exception("Expected the images token, got " + currentToken);
				}

				reader.ReadUInt16();
				remainingBytes = reader.ReadUInt32();
				newBytes = reader.ReadBytes((int) remainingBytes);
				ReadSubBlock(newBytes, KujuTokenID.images, ref shape);
				currentToken = (KujuTokenID) reader.ReadUInt16();
				if (currentToken != KujuTokenID.textures)
				{
					throw new Exception("Expected the textures token, got " + currentToken);
				}

				reader.ReadUInt16();
				remainingBytes = reader.ReadUInt32();
				newBytes = reader.ReadBytes((int) remainingBytes);
				ReadSubBlock(newBytes, KujuTokenID.textures, ref shape);
				currentToken = (KujuTokenID) reader.ReadUInt16();
				if (currentToken != KujuTokenID.light_materials)
				{
					throw new Exception("Expected the light_materials token, got " + currentToken);
				}

				reader.ReadUInt16();
				remainingBytes = reader.ReadUInt32();
				newBytes = reader.ReadBytes((int) remainingBytes);
				currentToken = (KujuTokenID) reader.ReadUInt16();
				if (currentToken != KujuTokenID.light_model_cfgs)
				{
					throw new Exception("Expected the light_model_cfgs token, got " + currentToken);
				}

				reader.ReadUInt16();
				remainingBytes = reader.ReadUInt32();
				newBytes = reader.ReadBytes((int) remainingBytes);
				currentToken = (KujuTokenID) reader.ReadUInt16();
				if (currentToken != KujuTokenID.vtx_states)
				{
					throw new Exception("Expected the vtx_states token, got " + currentToken);
				}

				reader.ReadUInt16();
				remainingBytes = reader.ReadUInt32();
				newBytes = reader.ReadBytes((int) remainingBytes);
				ReadSubBlock(newBytes, KujuTokenID.vtx_states, ref shape);
				currentToken = (KujuTokenID) reader.ReadUInt16();
				if (currentToken != KujuTokenID.prim_states)
				{
					throw new Exception("Expected the prim_states token, got " + currentToken);
				}

				reader.ReadUInt16();
				remainingBytes = reader.ReadUInt32();
				newBytes = reader.ReadBytes((int) remainingBytes);
				ReadSubBlock(newBytes, KujuTokenID.prim_states, ref shape);
				currentToken = (KujuTokenID) reader.ReadUInt16();
				if (currentToken != KujuTokenID.lod_controls)
				{
					throw new Exception("Expected the lod_controls token, got " + currentToken);
				}

				reader.ReadUInt16();
				remainingBytes = reader.ReadUInt32();
				newBytes = reader.ReadBytes((int) remainingBytes);
				ReadSubBlock(newBytes, KujuTokenID.lod_controls, ref shape);

			}
			Array.Resize(ref Result.Objects,shape.totalObjects);
			int idx = 0;
			double[] previousLODs = new double[shape.totalObjects];
			for (int i = 0; i < shape.LODs.Count; i++)
			{
				for (int j = 0; j < shape.LODs[i].subObjects.Count; j++)
				{
					Result.Objects[idx] = new ObjectManager.AnimatedObject();
					Result.Objects[idx].States = new ObjectManager.AnimatedObjectState[1];
					ObjectManager.AnimatedObjectState aos = new ObjectManager.AnimatedObjectState();
					shape.LODs[i].subObjects[j].meshBuilder.Apply(out aos.Object);
					aos.Position = new Vector3(0,0,0);
					Result.Objects[idx].States[0] = aos;
					previousLODs[idx] = shape.LODs[i].subObjects[j].meshBuilder.LODValue;
					int k = idx;
					while (k > 0)
					{
						if (previousLODs[k] < shape.LODs[i].subObjects[j].meshBuilder.LODValue)
						{
							break;
						}
						k--;
						
					}
					if (k != 0)
					{
						Result.Objects[idx].StateFunction = FunctionScripts.GetFunctionScriptFromInfixNotation("if[cameraDistance <" + shape.LODs[i].subObjects[j].meshBuilder.LODValue + ",if[cameraDistance >" + previousLODs[k] + ",0,-1],-1]");
					}
					else
					{
						Result.Objects[idx].StateFunction = FunctionScripts.GetFunctionScriptFromInfixNotation("if[cameraDistance <" + shape.LODs[i].subObjects[j].meshBuilder.LODValue + ",0,-1]");
					}

					idx++;
				}
			}
			return Result;
		}

		private static LOD currentLOD;

		private static void ReadSubBlock(byte[] blockBytes, KujuTokenID blockToken, ref MsTsShape shape)
		{
			Vertex v = null; //Crappy, but there we go.....
			ReadSubBlock(blockBytes, blockToken, ref shape, ref v);
		}

		private static void ReadSubBlock(byte[] blockBytes, KujuTokenID blockToken, ref MsTsShape shape, ref Vertex vertex)
		{
			float x, y, z;
			Vector3 point;
			KujuTokenID currentToken = KujuTokenID.error;
			uint remainingBytes = 0;
			byte[] newBytes;
			uint flags;
			string blockLabel = string.Empty;
			using (MemoryStream stream = new MemoryStream(blockBytes))
			{
				using (BinaryReader reader = new BinaryReader(stream))
				{
					int length = reader.ReadByte();
					if (length > 0)
					{
						//Note: For most blocks, the label length will be zero
						byte[] buff = new byte[length * 2];
						int i = 0;
						while (i < length * 2)
						{
							buff[i] = reader.ReadByte();
							i++;
						}

						blockLabel = System.Text.Encoding.Unicode.GetString(buff, 0, length * 2);
					}

					switch (blockToken)
					{
						case KujuTokenID.vtx_state:
							flags = reader.ReadUInt32();
							int matrix1 = reader.ReadInt32();
							int lightMaterialIdx = reader.ReadInt32();
							int lightStateCfgIdx = reader.ReadInt32();
							uint lightFlags = reader.ReadUInt32();
							int matrix2 = -1;
							if (stream.Length - stream.Position > 1)
							{
								matrix2 = reader.ReadInt32();
							}

							VertexStates vs = new VertexStates();
							vs.flags = flags;
							vs.hierarchyID = matrix1;
							vs.lightingMatrixID = lightMaterialIdx;
							vs.lightingConfigIdx = lightStateCfgIdx;
							vs.lightingFlags = lightFlags;
							vs.matrix2ID = matrix2;
							shape.vtx_states.Add(vs);
							break;
						case KujuTokenID.vtx_states:
							int vtxStateCount = reader.ReadUInt16();
							reader.ReadUInt16();
							while (vtxStateCount > 0)
							{
								currentToken = (KujuTokenID) reader.ReadUInt16();
								if (currentToken != KujuTokenID.vtx_state)
								{
									throw new Exception("Expected the vtx_state token, got " + currentToken);
								}

								reader.ReadUInt16();
								remainingBytes = reader.ReadUInt32();
								newBytes = reader.ReadBytes((int) remainingBytes);
								ReadSubBlock(newBytes, KujuTokenID.vtx_state, ref shape);
								vtxStateCount--;
							}

							break;
						case KujuTokenID.prim_state:
							flags = reader.ReadUInt32();
							int shader = reader.ReadInt32();
							currentToken = (KujuTokenID) reader.ReadUInt16();
							if (currentToken != KujuTokenID.tex_idxs)
							{
								throw new Exception("Expected the tex_idxs token, got " + currentToken);
							}

							reader.ReadUInt16();
							reader.ReadUInt32();
							reader.ReadByte();
							int[] texIdxs = new int[reader.ReadInt32()];
							for (int i = 0; i < texIdxs.Length; i++)
							{
								texIdxs[i] = reader.ReadInt32();
							}

							float zBias = reader.ReadSingle();
							int vertexStates = reader.ReadInt32();
							int alphaTestMode = reader.ReadInt32();
							int lightCfgIdx = reader.ReadInt32();
							int zBufferMode = reader.ReadInt32();
							PrimitiveState p = new PrimitiveState();
							p.Name = blockLabel;
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
						case KujuTokenID.prim_states:
							int primStateCount = reader.ReadUInt16();
							reader.ReadUInt16();
							while (primStateCount > 0)
							{
								currentToken = (KujuTokenID) reader.ReadUInt16();
								if (currentToken != KujuTokenID.prim_state)
								{
									throw new Exception("Expected the prim_state token, got " + currentToken);
								}

								reader.ReadUInt16();
								remainingBytes = reader.ReadUInt32();
								newBytes = reader.ReadBytes((int) remainingBytes);
								ReadSubBlock(newBytes, KujuTokenID.prim_state, ref shape);
								primStateCount--;
							}

							break;
						case KujuTokenID.texture:
							int imageIDX = (int) reader.ReadUInt32();
							int filterMode = (int) reader.ReadUInt32();
							float mipmapLODBias = reader.ReadSingle();
							uint borderColor = 0xff000000U;
							if (stream.Length - stream.Position > 1)
							{
								borderColor = reader.ReadUInt32();
							}

							//Unpack border color
							float r, g, b, a;
							r = borderColor % 256;
							g = (borderColor / 256) % 256;
							b = (borderColor / 256 / 256) % 256;
							a = (borderColor / 256 / 256 / 256) % 256;
							Texture t = new Texture();
							t.fileName = shape.images[imageIDX];
							t.filterMode = filterMode;
							t.mipmapLODBias = (int) mipmapLODBias;
							t.borderColor = new Color32((byte) r, (byte) g, (byte) b, (byte) a);
							shape.textures.Add(t);
							break;
						case KujuTokenID.textures:
							int textureCount = reader.ReadUInt16();
							reader.ReadUInt16();
							while (textureCount > 0)
							{
								currentToken = (KujuTokenID) reader.ReadUInt16();
								if (currentToken != KujuTokenID.texture)
								{
									throw new Exception("Expected the texture token, got " + currentToken);
								}

								reader.ReadUInt16();
								remainingBytes = reader.ReadUInt32();
								newBytes = reader.ReadBytes((int) remainingBytes);
								ReadSubBlock(newBytes, KujuTokenID.texture, ref shape);
								textureCount--;
							}

							break;
						case KujuTokenID.image:
							int imageLength = reader.ReadUInt16();
							if (imageLength > 0)
							{
								//Note: For most blocks, the label length will be zero
								byte[] buff = new byte[imageLength * 2];
								int i = 0;
								while (i < imageLength * 2)
								{
									buff[i] = reader.ReadByte();
									i++;
								}

								shape.images.Add(System.Text.Encoding.Unicode.GetString(buff, 0, imageLength * 2));
							}
							else
							{
								shape.images.Add(string.Empty); //Not sure this is valid, but let's be on the safe side
							}

							break;
						case KujuTokenID.images:
							int imageCount = reader.ReadUInt16();
							reader.ReadUInt16();
							while (imageCount > 0)
							{
								currentToken = (KujuTokenID) reader.ReadUInt16();
								if (currentToken != KujuTokenID.image)
								{
									throw new Exception("Expected the image token, got " + currentToken);
								}

								reader.ReadUInt16();
								remainingBytes = reader.ReadUInt32();
								newBytes = reader.ReadBytes((int) remainingBytes);
								ReadSubBlock(newBytes, KujuTokenID.image, ref shape);
								imageCount--;
							}

							break;
						case KujuTokenID.cullable_prims:
							int numPrims = reader.ReadInt32();
							int numFlatSections = reader.ReadInt32();
							int numPrimIdxs = reader.ReadInt32();
							break;
						case KujuTokenID.geometry_info:
							int faceNormals = reader.ReadInt32();
							int txLightCommands = reader.ReadInt32();
							int nodeXTrilistIdxs = reader.ReadInt32();
							int trilistIdxs = reader.ReadInt32();
							int lineListIdxs = reader.ReadInt32();
							nodeXTrilistIdxs = reader.ReadInt32(); //Duped, or is the first one actually something else?
							int trilists = reader.ReadInt32();
							int lineLists = reader.ReadInt32();
							int pointLists = reader.ReadInt32();
							int nodeXTrilists = reader.ReadInt32();
							currentToken = (KujuTokenID) reader.ReadUInt16();
							if (currentToken != KujuTokenID.geometry_nodes)
							{
								throw new Exception("Expected the geometry_nodes token, got " + currentToken);
							}

							reader.ReadUInt16();
							remainingBytes = reader.ReadUInt32();
							newBytes = reader.ReadBytes((int) remainingBytes);
							ReadSubBlock(newBytes, KujuTokenID.geometry_nodes, ref shape);
							currentToken = (KujuTokenID) reader.ReadUInt16();
							if (currentToken != KujuTokenID.geometry_node_map)
							{
								throw new Exception("Expected the geometry_node_map token, got " + currentToken);
							}

							reader.ReadUInt16();
							remainingBytes = reader.ReadUInt32();
							newBytes = reader.ReadBytes((int) remainingBytes);
							ReadSubBlock(newBytes, KujuTokenID.geometry_node_map, ref shape);
							break;
						case KujuTokenID.geometry_node_map:
							int[] geometryNodes = new int[reader.ReadInt32()];
							for (int i = 0; i < geometryNodes.Length; i++)
							{
								geometryNodes[i] = reader.ReadInt32();
							}

							break;
						case KujuTokenID.geometry_node:
							int n_txLightCommands = reader.ReadInt32();
							int n_nodeXTxLightCmds = reader.ReadInt32();
							int n_trilists = reader.ReadInt32();
							int n_lineLists = reader.ReadInt32();
							int n_pointLists = reader.ReadInt32();
							currentToken = (KujuTokenID) reader.ReadUInt16();
							if (currentToken != KujuTokenID.cullable_prims)
							{
								throw new Exception("Expected the cullable_prims token, got " + currentToken);
							}

							reader.ReadUInt16();
							remainingBytes = reader.ReadUInt32();
							newBytes = reader.ReadBytes((int) remainingBytes);
							ReadSubBlock(newBytes, KujuTokenID.cullable_prims, ref shape);
							break;
						case KujuTokenID.geometry_nodes:
							int geometryNodeCount = reader.ReadUInt16();
							reader.ReadUInt16();
							while (geometryNodeCount > 0)
							{
								currentToken = (KujuTokenID) reader.ReadUInt16();
								if (currentToken != KujuTokenID.geometry_node)
								{
									throw new Exception("Expected the geometry_node token, got " + currentToken);
								}

								reader.ReadUInt16();
								remainingBytes = reader.ReadUInt32();
								newBytes = reader.ReadBytes((int) remainingBytes);
								ReadSubBlock(newBytes, KujuTokenID.geometry_node, ref shape);
								geometryNodeCount--;
							}

							break;
						case KujuTokenID.point:
							x = reader.ReadSingle();
							y = reader.ReadSingle();
							z = reader.ReadSingle();
							point = new Vector3(x, y, z);
							shape.points.Add(point);
							break;
						case KujuTokenID.vector:
							x = reader.ReadSingle();
							y = reader.ReadSingle();
							z = reader.ReadSingle();
							point = new Vector3(x, y, z);
							shape.normals.Add(point);
							break;
						case KujuTokenID.points:
							int pointCount = reader.ReadUInt16();
							reader.ReadUInt16();
							while (pointCount > 0)
							{
								currentToken = (KujuTokenID) reader.ReadUInt16();
								if (currentToken != KujuTokenID.point)
								{
									throw new Exception("Expected the point token, got " + currentToken);
								}

								reader.ReadUInt16();
								remainingBytes = reader.ReadUInt32();
								newBytes = reader.ReadBytes((int) remainingBytes);
								ReadSubBlock(newBytes, KujuTokenID.point, ref shape);
								pointCount--;
							}

							break;
						case KujuTokenID.uv_point:
							x = reader.ReadSingle();
							y = reader.ReadSingle();
							var uv_point = new Vector2(x, y);
							shape.uv_points.Add(uv_point);
							break;
						case KujuTokenID.uv_points:
							int uvPointCount = reader.ReadUInt16();
							reader.ReadUInt16();
							while (uvPointCount > 0)
							{
								currentToken = (KujuTokenID) reader.ReadUInt16();
								if (currentToken != KujuTokenID.uv_point)
								{
									throw new Exception("Expected the uv_point token, got " + currentToken);
								}

								reader.ReadUInt16();
								remainingBytes = reader.ReadUInt32();
								newBytes = reader.ReadBytes((int) remainingBytes);
								ReadSubBlock(newBytes, KujuTokenID.uv_point, ref shape);
								uvPointCount--;
							}

							break;
						case KujuTokenID.matrices:
							int matrixCount = reader.ReadUInt16();
							reader.ReadUInt16();
							while (matrixCount > 0)
							{
								currentToken = (KujuTokenID) reader.ReadUInt16();
								if (currentToken != KujuTokenID.matrix)
								{
									throw new Exception("Expected the matrix token, got " + currentToken);
								}

								reader.ReadUInt16();
								remainingBytes = reader.ReadUInt32();
								newBytes = reader.ReadBytes((int) remainingBytes);
								ReadSubBlock(newBytes, KujuTokenID.matrix, ref shape);
								matrixCount--;
							}

							break;
						case KujuTokenID.matrix:
							Matrix currentMatrix = new Matrix();
							currentMatrix.Name = blockLabel;
							currentMatrix.A = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
							currentMatrix.B = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
							currentMatrix.C = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
							currentMatrix.D = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
							shape.matrices.Add(currentMatrix);
							break;
						case KujuTokenID.normals:
							int normalCount = reader.ReadUInt16();
							reader.ReadUInt16();
							while (normalCount > 0)
							{
								currentToken = (KujuTokenID) reader.ReadUInt16();
								if (currentToken != KujuTokenID.vector)
								{
									throw new Exception("Expected the vector token, got " + currentToken);
								}

								reader.ReadUInt16();
								remainingBytes = reader.ReadUInt32();
								newBytes = reader.ReadBytes((int) remainingBytes);
								ReadSubBlock(newBytes, KujuTokenID.vector, ref shape);
								normalCount--;
							}

							break;
						case KujuTokenID.distance_levels_header:
							int DLevBias = reader.ReadInt16();
							break;
						case KujuTokenID.distance_levels:
							int distanceLevelCount = reader.ReadInt16();
							reader.ReadUInt16();
							while (distanceLevelCount > 0)
							{
								currentToken = (KujuTokenID) reader.ReadUInt16();
								if (currentToken != KujuTokenID.distance_level)
								{
									throw new Exception("Expected the distance_level token, got " + currentToken);
								}

								reader.ReadUInt16();
								remainingBytes = reader.ReadUInt32();
								newBytes = reader.ReadBytes((int) remainingBytes);
								ReadSubBlock(newBytes, KujuTokenID.distance_level, ref shape);
								distanceLevelCount--;
							}

							break;
						case KujuTokenID.distance_level_header:
							currentToken = (KujuTokenID) reader.ReadUInt16();
							if (currentToken != KujuTokenID.dlevel_selection)
							{
								throw new Exception("Expected the dlevel_selection token, got " + currentToken);
							}

							reader.ReadUInt16();
							remainingBytes = reader.ReadUInt32();
							newBytes = reader.ReadBytes((int) remainingBytes);
							ReadSubBlock(newBytes, KujuTokenID.dlevel_selection, ref shape);
							currentToken = (KujuTokenID) reader.ReadUInt16();
							if (currentToken != KujuTokenID.hierarchy)
							{
								throw new Exception("Expected the hierarchy token, got " + currentToken);
							}

							reader.ReadUInt16();
							remainingBytes = reader.ReadUInt32();
							newBytes = reader.ReadBytes((int) remainingBytes);
							ReadSubBlock(newBytes, KujuTokenID.hierarchy, ref shape);
							break;
						case KujuTokenID.distance_level:
							currentToken = (KujuTokenID) reader.ReadUInt16();
							if (currentToken != KujuTokenID.distance_level_header)
							{
								throw new Exception("Expected the distance_level_header token, got " + currentToken);
							}

							reader.ReadUInt16();
							remainingBytes = reader.ReadUInt32();
							newBytes = reader.ReadBytes((int) remainingBytes);
							ReadSubBlock(newBytes, KujuTokenID.distance_level_header, ref shape);
							currentToken = (KujuTokenID) reader.ReadUInt16();
							if (currentToken != KujuTokenID.sub_objects)
							{
								throw new Exception("Expected the sub_objects token, got " + currentToken);
							}

							reader.ReadUInt16();
							remainingBytes = reader.ReadUInt32();
							newBytes = reader.ReadBytes((int) remainingBytes);
							ReadSubBlock(newBytes, KujuTokenID.sub_objects, ref shape);
							shape.LODs.Add(currentLOD);
							break;
						case KujuTokenID.dlevel_selection:
							currentLOD = new LOD(reader.ReadSingle());
							break;
						case KujuTokenID.hierarchy:
							currentLOD.hierarchy = new int[reader.ReadInt32()];
							for (int i = 0; i < currentLOD.hierarchy.Length; i++)
							{
								currentLOD.hierarchy[i] = reader.ReadInt32();
							}

							break;
						case KujuTokenID.lod_control:
							currentToken = (KujuTokenID) reader.ReadUInt16();
							if (currentToken != KujuTokenID.distance_levels_header)
							{
								throw new Exception("Expected the distance_levels_header token, got " + currentToken);
							}

							reader.ReadUInt16();
							remainingBytes = reader.ReadUInt32();
							newBytes = reader.ReadBytes((int) remainingBytes);
							ReadSubBlock(newBytes, KujuTokenID.distance_levels_header, ref shape);
							currentToken = (KujuTokenID) reader.ReadUInt16();
							if (currentToken != KujuTokenID.distance_levels)
							{
								throw new Exception("Expected the distance_levels token, got " + currentToken);
							}

							reader.ReadUInt16();
							remainingBytes = reader.ReadUInt32();
							newBytes = reader.ReadBytes((int) remainingBytes);
							ReadSubBlock(newBytes, KujuTokenID.distance_levels, ref shape);
							break;
						case KujuTokenID.lod_controls:
							int lodCount = reader.ReadInt16();
							reader.ReadUInt16();
							while (lodCount > 0)
							{
								currentToken = (KujuTokenID) reader.ReadUInt16();
								if (currentToken != KujuTokenID.lod_control)
								{
									throw new Exception("Expected the lod_control token, got " + currentToken);
								}

								reader.ReadUInt16();
								remainingBytes = reader.ReadUInt32();
								newBytes = reader.ReadBytes((int) remainingBytes);
								ReadSubBlock(newBytes, KujuTokenID.lod_control, ref shape);
								lodCount--;
							}

							break;
						case KujuTokenID.primitives:
							int capacity = reader.ReadInt32(); //Count of the number of entries in the block, not the number of primitives
							while (capacity > 0)
							{
								currentToken = (KujuTokenID) reader.ReadUInt16();
								reader.ReadUInt16();
								remainingBytes = reader.ReadUInt32();
								newBytes = reader.ReadBytes((int) remainingBytes);
								switch (currentToken)
								{
									case KujuTokenID.prim_state_idx:
										ReadSubBlock(newBytes, KujuTokenID.prim_state_idx, ref shape);
										break;
									case KujuTokenID.indexed_trilist:
										ReadSubBlock(newBytes, KujuTokenID.indexed_trilist, ref shape);
										break;
									default:
										throw new Exception("Unexpected primitive type, got " + currentToken);
								}

								capacity--;
							}
							if (shape.currentPrimitiveState != -1)
							{
								//TODO: Only supports the first texture
								currentLOD.subObjects[currentLOD.subObjects.Count -1].meshBuilder.Materials[0].DaytimeTexture = OpenBveApi.Path.CombineFile(currentFolder,shape.textures[shape.prim_states[shape.currentPrimitiveState].Textures[0]].fileName + ".png");
								currentLOD.subObjects[currentLOD.subObjects.Count -1].meshBuilder.Materials[0].NighttimeTexture = OpenBveApi.Path.CombineFile(currentFolder,shape.textures[shape.prim_states[shape.currentPrimitiveState].Textures[0]].fileName + ".png");
							}
							break;
						case KujuTokenID.prim_state_idx:
							shape.currentPrimitiveState = reader.ReadInt32();
							break;
						case KujuTokenID.indexed_trilist:
							currentToken = (KujuTokenID) reader.ReadUInt16();
							if (currentToken != KujuTokenID.vertex_idxs)
							{
								throw new Exception("Expected the vertex_idxs token, got " + currentToken);
							}

							reader.ReadUInt16();
							remainingBytes = reader.ReadUInt32();
							newBytes = reader.ReadBytes((int) remainingBytes);
							ReadSubBlock(newBytes, KujuTokenID.vertex_idxs, ref shape);
							currentToken = (KujuTokenID) reader.ReadUInt16();
							if (currentToken != KujuTokenID.normal_idxs)
							{
								throw new Exception("Expected the normal_idxs token, got " + currentToken);
							}

							reader.ReadUInt16();
							remainingBytes = reader.ReadUInt32();
							newBytes = reader.ReadBytes((int) remainingBytes);
							ReadSubBlock(newBytes, KujuTokenID.normal_idxs, ref shape);
							break;
						case KujuTokenID.sub_object:
							currentToken = (KujuTokenID) reader.ReadUInt16();
							if (currentToken != KujuTokenID.sub_object_header)
							{
								throw new Exception("Expected the sub_object_header token, got " + currentToken);
							}

							reader.ReadUInt16();
							remainingBytes = reader.ReadUInt32();
							newBytes = reader.ReadBytes((int) remainingBytes);
							ReadSubBlock(newBytes, KujuTokenID.sub_object_header, ref shape);
							currentToken = (KujuTokenID) reader.ReadUInt16();
							if (currentToken != KujuTokenID.vertices)
							{
								throw new Exception("Expected the vertices token, got " + currentToken);
							}

							reader.ReadUInt16();
							remainingBytes = reader.ReadUInt32();
							newBytes = reader.ReadBytes((int) remainingBytes);
							ReadSubBlock(newBytes, KujuTokenID.vertices, ref shape);
							currentToken = (KujuTokenID) reader.ReadUInt16();
							if (currentToken != KujuTokenID.vertex_sets)
							{
								throw new Exception("Expected the vertex_sets token, got " + currentToken);
							}

							reader.ReadUInt16();
							remainingBytes = reader.ReadUInt32();
							newBytes = reader.ReadBytes((int) remainingBytes);
							ReadSubBlock(newBytes, KujuTokenID.vertex_sets, ref shape);
							currentToken = (KujuTokenID) reader.ReadUInt16();
							if (currentToken != KujuTokenID.primitives)
							{
								throw new Exception("Expected the primitives token, got " + currentToken);
							}

							reader.ReadUInt16();
							remainingBytes = reader.ReadUInt32();
							newBytes = reader.ReadBytes((int) remainingBytes);
							ReadSubBlock(newBytes, KujuTokenID.primitives, ref shape);
							break;
						case KujuTokenID.sub_objects:
							int subObjectCount = reader.ReadInt16();
							reader.ReadUInt16();
							while (subObjectCount > 0)
							{
								currentToken = (KujuTokenID) reader.ReadUInt16();
								if (currentToken != KujuTokenID.sub_object)
								{
									throw new Exception("Expected the sub_object token, got " + currentToken);
								}

								reader.ReadUInt16();
								remainingBytes = reader.ReadUInt32();
								newBytes = reader.ReadBytes((int) remainingBytes);
								ReadSubBlock(newBytes, KujuTokenID.sub_object, ref shape);
								subObjectCount--;
							}
							break;
						case KujuTokenID.sub_object_header:
							currentLOD.subObjects.Add(new SubObject());
							shape.totalObjects++;
							flags = reader.ReadUInt32();
							int sortVectorIdx = reader.ReadInt32();
							int volIdx = reader.ReadInt32();
							uint sourceVertexFormatFlags = reader.ReadUInt32();
							uint destinationVertexFormatFlags = reader.ReadUInt32();
							currentToken = (KujuTokenID) reader.ReadUInt16();
							if (currentToken != KujuTokenID.geometry_info)
							{
								throw new Exception("Expected the geometry_info token, got " + currentToken);
							}

							reader.ReadUInt16();
							remainingBytes = reader.ReadUInt32();
							newBytes = reader.ReadBytes((int) remainingBytes);
							ReadSubBlock(newBytes, KujuTokenID.geometry_info, ref shape);
							/*
							 * Optional stuff, need to check if we're running off the end of the stream before reading each block
							 */
							if (stream.Length - stream.Position > 1)
							{
								currentToken = (KujuTokenID) reader.ReadUInt16();
								if (currentToken != KujuTokenID.subobject_shaders)
								{
									throw new Exception("Expected the subobject_shaders token, got " + currentToken);
								}

								reader.ReadUInt16();
								remainingBytes = reader.ReadUInt32();
								newBytes = reader.ReadBytes((int) remainingBytes);
								ReadSubBlock(newBytes, KujuTokenID.subobject_shaders, ref shape);
							}

							if (stream.Length - stream.Position > 1)
							{
								currentToken = (KujuTokenID) reader.ReadUInt16();
								if (currentToken != KujuTokenID.subobject_light_cfgs)
								{
									throw new Exception("Expected the subobject_light_cfgs token, got " + currentToken);
								}

								reader.ReadUInt16();
								remainingBytes = reader.ReadUInt32();
								newBytes = reader.ReadBytes((int) remainingBytes);
								ReadSubBlock(newBytes, KujuTokenID.subobject_light_cfgs, ref shape);
							}

							if (stream.Length - stream.Position > 1)
							{
								int subObjectID = reader.ReadInt32();
							}

							break;
						case KujuTokenID.subobject_light_cfgs:
							int[] subobject_light_cfgs = new int[reader.ReadInt32()];
							for (int i = 0; i < subobject_light_cfgs.Length; i++)
							{
								subobject_light_cfgs[i] = reader.ReadInt32();
							}

							break;
						case KujuTokenID.subobject_shaders:
							int[] subobject_shaders = new int[reader.ReadInt32()];
							for (int i = 0; i < subobject_shaders.Length; i++)
							{
								subobject_shaders[i] = reader.ReadInt32();
							}

							break;
						case KujuTokenID.vertices:
							int vertexCount = reader.ReadInt16();
							reader.ReadUInt16();
							while (vertexCount > 0)
							{
								currentToken = (KujuTokenID) reader.ReadUInt16();
								if (currentToken != KujuTokenID.vertex)
								{
									throw new Exception("Expected the vertex token, got " + currentToken);
								}

								reader.ReadUInt16();
								remainingBytes = reader.ReadUInt32();
								newBytes = reader.ReadBytes((int) remainingBytes);
								ReadSubBlock(newBytes, KujuTokenID.vertex, ref shape);
								vertexCount--;
							}

							break;
						case KujuTokenID.vertex:
							flags = reader.ReadUInt32(); //Various control variables, not supported
							int myPoint = reader.ReadInt32(); //Index to points array
							int myNormal = reader.ReadInt32(); //Index to normals array

							Vertex v = new Vertex(shape.points[myPoint], shape.normals[myNormal]);
							
							uint Color1 = reader.ReadUInt32();
							uint Color2 = reader.ReadUInt32();
							currentToken = (KujuTokenID) reader.ReadUInt16();
							if (currentToken != KujuTokenID.vertex_uvs)
							{
								throw new Exception("Expected the vertex_uvs token, got " + currentToken);
							}
							reader.ReadUInt16();
							remainingBytes = reader.ReadUInt32();
							newBytes = reader.ReadBytes((int) remainingBytes);
							ReadSubBlock(newBytes, KujuTokenID.vertex_uvs, ref shape, ref v);
							currentLOD.subObjects[currentLOD.subObjects.Count -1].verticies.Add(v);
							break;
						case KujuTokenID.vertex_idxs:
							int remainingVertex = reader.ReadInt32() / 3;
							int idx = currentLOD.subObjects[currentLOD.subObjects.Count -1].meshBuilder.Faces.Length;
							Array.Resize(ref currentLOD.subObjects[currentLOD.subObjects.Count -1].meshBuilder.Faces, remainingVertex + idx);
							while (remainingVertex > 0)
							{
								int v1 = reader.ReadInt32();
								int v2 = reader.ReadInt32();
								int v3 = reader.ReadInt32();
								currentLOD.subObjects[currentLOD.subObjects.Count -1].faces.Add(new int[] { v1, v2, v3 });
								remainingVertex--;
								idx++;
							}
							currentLOD.subObjects[currentLOD.subObjects.Count -1].CreateMeshBuilder(currentLOD.viewingDistance);
							break;
						case KujuTokenID.vertex_set:
							int hierarchyIndex = reader.ReadInt32(); //Index to the final hiearchy chain member
							int setStartVertexIndex = reader.ReadInt32(); //First vertex
							int setVertexCount = reader.ReadInt32(); //Total number of vert
							VertexSet vts = new VertexSet();
							vts.hierarchyIndex = hierarchyIndex;
							vts.startVertex = setStartVertexIndex;
							vts.numVerticies = setVertexCount;
							currentLOD.subObjects[currentLOD.subObjects.Count -1].vertexSets.Add(vts);
							break;
						case KujuTokenID.vertex_sets:
							int vertexSetCount = reader.ReadInt16();
							reader.ReadUInt16();
							while (vertexSetCount > 0)
							{
								currentToken = (KujuTokenID) reader.ReadUInt16();
								if (currentToken != KujuTokenID.vertex_set)
								{
									throw new Exception("Expected the vertex_set token, got " + currentToken);
								}

								reader.ReadUInt16();
								remainingBytes = reader.ReadUInt32();
								newBytes = reader.ReadBytes((int) remainingBytes);
								ReadSubBlock(newBytes, KujuTokenID.vertex_set, ref shape);
								vertexSetCount--;
							}

							//We now need to transform our verticies
							currentLOD.subObjects[currentLOD.subObjects.Count -1].TransformVerticies(shape.matrices);

							Array.Resize(ref currentLOD.subObjects[currentLOD.subObjects.Count -1].meshBuilder.Vertices, currentLOD.subObjects[currentLOD.subObjects.Count -1].verticies.Count);
							for (int i = 0; i < currentLOD.subObjects[currentLOD.subObjects.Count -1].verticies.Count; i++)
							{
								currentLOD.subObjects[currentLOD.subObjects.Count -1].meshBuilder.Vertices[i].Coordinates = currentLOD.subObjects[currentLOD.subObjects.Count -1].verticies[i].Coordinates;
								currentLOD.subObjects[currentLOD.subObjects.Count -1].meshBuilder.Vertices[i].TextureCoordinates = currentLOD.subObjects[currentLOD.subObjects.Count -1].verticies[i].TextureCoordinates;
							}
							break;
						case KujuTokenID.vertex_uvs:
							int[] vertex_uvs = new int[reader.ReadInt32()];
							for (int i = 0; i < vertex_uvs.Length; i++)
							{
								vertex_uvs[i] = reader.ReadInt32();
							}
							//Looks as if vertex_uvs should always be of length 1, thus:
							vertex.TextureCoordinates = shape.uv_points[vertex_uvs[0]];
							break;
					}
				}
			}
		}
	}
}
