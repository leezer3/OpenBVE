using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using OpenBve.Formats.DirectX;
using OpenBveApi.Colors;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;


namespace OpenBve 
{
	class NewXParser
	{
		internal static ObjectManager.StaticObject ReadObject(string FileName, Encoding Encoding, ObjectLoadMode LoadMode)
		{
			currentFolder = System.IO.Path.GetDirectoryName(FileName);
			currentFile = FileName;
			byte[] Data = System.IO.File.ReadAllBytes(FileName);
			if (Data.Length < 16 || Data[0] != 120 | Data[1] != 111 | Data[2] != 102 | Data[3] != 32)
			{
				// not an x object
				Interface.AddMessage(MessageType.Error, false, "Invalid X object file encountered in " + FileName);
				return null;
			}

			if (Data[4] != 48 | Data[5] != 51 | Data[6] != 48 | Data[7] != 50 & Data[7] != 51)
			{
				// unrecognized version
				System.Text.ASCIIEncoding Ascii = new System.Text.ASCIIEncoding();
				string s = new string(Ascii.GetChars(Data, 4, 4));
				Interface.AddMessage(MessageType.Error, false, "Unsupported X object file version " + s + " encountered in " + FileName);
			}

			// floating-point format
			int FloatingPointSize;
			if (Data[12] == 48 & Data[13] == 48 & Data[14] == 51 & Data[15] == 50)
			{
				FloatingPointSize = 32;
			}
			else if (Data[12] == 48 & Data[13] == 48 & Data[14] == 54 & Data[15] == 52)
			{
				FloatingPointSize = 64;
			}
			else
			{
				Interface.AddMessage(MessageType.Error, false, "Unsupported floating point format encountered in X object file " + FileName);
				return null;
			}

			// supported floating point format
			if (Data[8] == 116 & Data[9] == 120 & Data[10] == 116 & Data[11] == 32)
			{
				// textual flavor
				return LoadTextualX(File.ReadAllText(FileName, Encoding), LoadMode);
			}

			byte[] newData;
			if (Data[8] == 98 & Data[9] == 105 & Data[10] == 110 & Data[11] == 32)
			{
				//Uncompressed binary, so skip the header
				newData = new byte[Data.Length - 16];
				Array.Copy(Data, 16, newData, 0, Data.Length - 16);
				return LoadBinaryX(newData, FloatingPointSize, LoadMode);
			}

			if (Data[8] == 116 & Data[9] == 122 & Data[10] == 105 & Data[11] == 112)
			{
				// compressed textual flavor
				newData = Decompress(Data);
				string Text = Encoding.GetString(newData);
				return LoadTextualX(Text, LoadMode);
			}

			if (Data[8] == 98 & Data[9] == 122 & Data[10] == 105 & Data[11] == 112)
			{
				//Compressed binary
				//16 bytes of header, then 8 bytes of padding, followed by the actual compressed data
				byte[] Uncompressed = Decompress(Data);
				return LoadBinaryX(Uncompressed, FloatingPointSize, LoadMode);
			}

			// unsupported flavor
			Interface.AddMessage(MessageType.Error, false, "Unsupported X object file encountered in " + FileName);
			return null;
		}

		private static byte[] Decompress(byte[] Data) {
			byte[] Target;
			using (Stream InputStream = new MemoryStream(Data)) {
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

		private static ObjectManager.StaticObject LoadTextualX(string Text, ObjectLoadMode LoadMode)
		{
			
			Text = Text.Substring(17);
			Text = Text.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ").Replace("\t", " ").Trim();
			ObjectManager.StaticObject obj = new ObjectManager.StaticObject();
			obj.Mesh.Faces = new World.MeshFace[] { };
			obj.Mesh.Materials = new World.MeshMaterial[] { };
			obj.Mesh.Vertices = new VertexTemplate[] { };
			MeshBuilder builder = new MeshBuilder();
			Material material = new Material();
			Block block = new TextualBlock(Text);
			while (block.Position() < block.Length())
			{
				Block subBlock = block.ReadSubBlock();
				ParseSubBlock(subBlock, ref obj, ref builder, ref material);
			}
			builder.Apply(ref obj);
			obj.Mesh.CreateNormals();
			return obj;
		}

		private static string currentFolder;
		private static string currentFile;

		private static void ParseSubBlock(Block block, ref ObjectManager.StaticObject obj, ref MeshBuilder builder, ref Material material)
		{
			Block subBlock;
			if (block.Label == "template")
			{
				return;
			}
			switch (block.Token)
			{
				default:
					return;
				case TemplateID.Header:
					int majorVersion = block.ReadUInt16();
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
					while (block.Position() < block.Length() - 5)
					{
						subBlock = block.ReadSubBlock();
						ParseSubBlock(subBlock, ref obj, ref builder, ref material);
					}
					break;
				case TemplateID.FrameTransformMatrix:
					double[] frameTransformMatrix = new double[16];
					for (int i = 0; i < 16; i++)
					{
						frameTransformMatrix[i] = block.ReadSingle();
					}
					break;
				case TemplateID.Mesh:
					if (builder.Vertices.Length != 0)
					{
						builder.Apply(ref obj);
						builder = new MeshBuilder();
					}
					int nVerts = block.ReadUInt16();
					if (nVerts == 0)
					{
						throw new Exception("nVertices must be greater than zero");
					}
					int v = builder.Vertices.Length;
					Array.Resize(ref builder.Vertices, v + nVerts);
					for (int i = 0; i < nVerts; i++)
					{
						builder.Vertices[v + i] = new Vertex(new Vector3(block.ReadSingle(), block.ReadSingle(), block.ReadSingle()));
					}
					int nFaces = block.ReadUInt16();
					if (nFaces == 0)
					{
						throw new Exception("nFaces must be greater than zero");
					}
					int f = builder.Faces.Length;
					Array.Resize(ref builder.Faces, f + nFaces);
					for (int i = 0; i < nFaces; i++)
					{
						int fVerts = block.ReadUInt16();
						if (nFaces == 0)
						{
							throw new Exception("fVerts must be greater than zero");
						}
						builder.Faces[f + i] = new World.MeshFace();
						builder.Faces[f + i].Vertices = new World.MeshFaceVertex[fVerts];
						for (int j = 0; j < fVerts; j++)
						{
							builder.Faces[f + i].Vertices[j].Index = block.ReadUInt16();
						}
					}
					while (block.Position() < block.Length() - 5)
					{
						subBlock = block.ReadSubBlock();
						ParseSubBlock(subBlock, ref obj, ref builder, ref material);
					}
					break;
				case TemplateID.MeshMaterialList:
					int nMaterials = block.ReadUInt16();
					int nFaceIndices = block.ReadUInt16();
					for (int i = 0; i < nFaceIndices; i++)
					{
						int fMaterial = block.ReadUInt16();
						builder.Faces[i].Material = (ushort) (fMaterial + 1);
					}

					for (int i = 0; i < nMaterials; i++)
					{
						subBlock = block.ReadSubBlock(TemplateID.Material);
						ParseSubBlock(subBlock, ref obj, ref builder, ref material);
					}
					break;
				case TemplateID.Material:
					int m = builder.Materials.Length;
					Array.Resize(ref builder.Materials, m + 1);
					builder.Materials[m] = new Material();
					builder.Materials[m].Color = new Color32((byte)(255 * block.ReadSingle()), (byte)(255 * block.ReadSingle()), (byte)(255 * block.ReadSingle()),(byte)(255 * block.ReadSingle()));
					double mPower = block.ReadSingle(); //TODO: Unsure what this does...
					Color24 mSpecular = new Color24((byte)block.ReadSingle(), (byte)block.ReadSingle(), (byte)block.ReadSingle());
					builder.Materials[m].EmissiveColor = new Color24((byte)(255 *block.ReadSingle()), (byte)(255 * block.ReadSingle()), (byte)(255 * block.ReadSingle()));
					builder.Materials[m].EmissiveColorUsed = true; //TODO: Check exact behaviour
					builder.Materials[m].TransparentColor = Color24.Black; //TODO: Check, also can we optimise which faces have the transparent color set?
					builder.Materials[m].TransparentColorUsed = true;
					if (block.Position() < block.Length() - 5)
					{
						subBlock = block.ReadSubBlock(TemplateID.TextureFilename);
						ParseSubBlock(subBlock, ref obj, ref builder, ref builder.Materials[m]);
					}
					break;
				case TemplateID.TextureFilename:
					material.DaytimeTexture = OpenBveApi.Path.CombineFile(currentFolder, block.ReadString());
					if (!System.IO.File.Exists(material.DaytimeTexture))
					{
						Interface.AddMessage(MessageType.Error, true, "Texure " + material.DaytimeTexture + " was not found in file " + currentFile);
						material.DaytimeTexture = null;
					}
					break;
				case TemplateID.MeshTextureCoords:
					int nCoords = block.ReadUInt16();
					for (int i = 0; i < nCoords; i++)
					{
						builder.Vertices[i].TextureCoordinates = new Vector2(block.ReadSingle(), block.ReadSingle());
					}
					break;
				case TemplateID.MeshNormals:
					int nNormals = block.ReadUInt16();
					Vector3[] normals = new Vector3[nNormals];
					for (int i = 0; i < nNormals; i++)
					{
						normals[i] = new Vector3(block.ReadSingle(), block.ReadSingle(), block.ReadSingle());
						normals[i].Normalize();
					}
					int nFaceNormals = block.ReadUInt16();
					if (nFaceNormals != builder.Faces.Length)
					{
						throw new Exception("nFaceNormals must match the number of faces in the mesh");
					}
					for (int i = 0; i < nFaceNormals; i++)
					{
						int nVertexNormals = block.ReadUInt16();
						if (nVertexNormals != builder.Faces[i].Vertices.Length)
						{
							throw new Exception("nVertexNormals must match the number of verticies in the face");
						}
						for (int j = 0; j < nVertexNormals; j++)
						{
							builder.Faces[i].Vertices[j].Normal = normals[block.ReadUInt16()];
						}
					}
					break;
				case TemplateID.MeshVertexColors:
					int nVertexColors = block.ReadUInt16();
					for (int i = 0; i < nVertexColors; i++)
					{
						builder.Vertices[i] = new ColoredVertex((Vertex)builder.Vertices[i], new Color128(block.ReadSingle(), block.ReadSingle(), block.ReadSingle()));
					}
					break;
			}
		}

		private static ObjectManager.StaticObject LoadBinaryX(byte[] Data, int FloatingPointSize, ObjectLoadMode LoadMode)
		{
			Block block = new BinaryBlock(Data, FloatingPointSize);
			block.FloatingPointSize = FloatingPointSize;
			ObjectManager.StaticObject obj = new ObjectManager.StaticObject();
			obj.Mesh.Faces = new World.MeshFace[] { };
			obj.Mesh.Materials = new World.MeshMaterial[] { };
			obj.Mesh.Vertices = new VertexTemplate[] { };
			MeshBuilder builder = new MeshBuilder();
			Material material = new Material();
			Block subBlock = block.ReadSubBlock(); //Mesh
			ParseSubBlock(subBlock, ref obj, ref builder, ref material);
			builder.Apply(ref obj);
			obj.Mesh.CreateNormals();
			return obj;
		}
	}
}
