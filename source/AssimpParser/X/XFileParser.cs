// Open Asset Import Library (assimp)
//
// Copyright (c) 2006-2016, assimp team, 2018, The openBVE Project
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms,
// with or without modification, are permitted provided that the
// following conditions are met:
//
// * Redistributions of source code must retain the above
//   copyright notice, this list of conditions and the
//   following disclaimer.
//
// * Redistributions in binary form must reproduce the above
//   copyright notice, this list of conditions and the
//   following disclaimer in the documentation and/or other
//   materials provided with the distribution.
//
// * Neither the name of the assimp team, nor the names of its
//   contributors may be used to endorse or promote products
//   derived from this software without specific prior
//   written permission of the assimp team.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
//
//
// ******************************************************************************
//
// AN EXCEPTION applies to all files in the ./test/models-nonbsd folder.
// These are 3d models for testing purposes, from various free sources
// on the internet. They are - unless otherwise stated - copyright of
// their respective creators, which may impose additional requirements
// on the use of their work. For any of these models, see
// <model-name>.source.txt for more legal information. Contact us if you
// are a copyright holder and believe that we credited you inproperly or
// if you don't want your files to appear in the repository.
//
//
// ******************************************************************************
//
// Poly2Tri Copyright (c) 2009-2010, Poly2Tri Contributors
// http://code.google.com/p/poly2tri/
//
// All rights reserved.
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice,
//   this list of conditions and the following disclaimer.
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
// * Neither the name of Poly2Tri nor the names of its contributors may be
//   used to endorse or promote products derived from this software without specific
//   prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
// EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
// PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
// PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
// LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Globalization;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Math;
using ZlibWithDictionary;
using VectorKey = System.Collections.Generic.KeyValuePair<double, OpenBveApi.Math.Vector3>;
using QuatKey = System.Collections.Generic.KeyValuePair<double, OpenBveApi.Math.Quaternion>;
using MatrixKey = System.Collections.Generic.KeyValuePair<double, OpenBveApi.Math.Matrix4D>;
using Vector2 = OpenBveApi.Math.Vector2;
using Vector3 = OpenBveApi.Math.Vector3;


namespace AssimpNET.X
{
	public class XFileParser
	{
		// Magic identifier for MSZIP compressed data
		private const uint MSZIP_MAGIC = 0x4B43;
		private const uint MSZIP_BLOCK = 32786;

		private readonly byte[] Buffer;

		protected uint MajorVersion, MinorVersion; // version numbers
		protected readonly bool IsBinaryFormat; // true if the file is in binary, false if it's in text form
		protected readonly uint BinaryFloatSize; // float size in bytes, either 4 or 8
		// counter for number arrays in binary format
		protected uint BinaryNumCount;

		// text encoding of text format
		private readonly Encoding textEncoding;

		protected int currentPosition;
		protected int End;

		/// Line number when reading in text format
		protected uint LineNumber;

		/// Imported data
		protected Scene Scene;

		// Constructor. Creates a data structure out of the XFile given in the memory block.
		public XFileParser(byte[] buffer)
		{
			Buffer = buffer;
			MajorVersion = MinorVersion = 0;
			IsBinaryFormat = false;
			BinaryNumCount = 0;
			currentPosition = End = -1;
			LineNumber = 0;
			Scene = null;

			// set up memory pointers
			currentPosition = 0;
			//End = P + Buffer.Length - 1;
			End = currentPosition + Buffer.Length;

			// check header
			if (Buffer.Length < 16)
			{
				throw new InvalidDataException("Header mismatch, file is not an XFile.");
			}

			ASCIIEncoding Ascii = new ASCIIEncoding();
			string header = new string(Ascii.GetChars(Buffer, 0, 12));

			if (header.Substring(0, 4) != "xof ")
			{
				throw new InvalidDataException("Header mismatch, file is not an XFile.");
			}

			// read version. It comes in a four byte format such as "0302"
			MajorVersion = (uint)(Buffer[4] - 48) * 10 + (uint)(Buffer[5] - 48);
			MinorVersion = (uint)(Buffer[6] - 48) * 10 + (uint)(Buffer[7] - 48);

			bool compressed = false;

			// txt - pure ASCII text format
			if (header.Substring(8, 4) == "txt ")
			{
				IsBinaryFormat = false;
			}
			// bin - Binary format
			else if (header.Substring(8, 4) == "bin ")
			{
				IsBinaryFormat = true;
			}
			// tzip - Inflate compressed text format
			else if (header.Substring(8, 4) == "tzip")
			{
				IsBinaryFormat = false;
				compressed = true;
			}
			// bzip - Inflate compressed binary format
			else if (header.Substring(8, 4) == "bzip")
			{
				IsBinaryFormat = true;
				compressed = true;
			}
			else
			{
				throw new FormatException("Unsupported xfile format '" + header.Substring(8, 4) + "'");
			}

			// float size
			BinaryFloatSize = (uint)(Buffer[12] - 48) * 1000 + (uint)(Buffer[13] - 48) * 100 + (uint)(Buffer[14] - 48) * 10 + (uint)(Buffer[15] - 48);

			if (BinaryFloatSize != 32 && BinaryFloatSize != 64)
			{
				throw new FormatException("Unknown float size " + BinaryFloatSize + " specified in xfile header.");
			}

			// The x format specifies size in bits, but we work in bytes
			BinaryFloatSize /= 8;

			currentPosition += 16;

			// If this is a compressed X file, apply the inflate algorithm to it
			if (compressed)
			{
				/* ///////////////////////////////////////////////////////////////////////
				 * COMPRESSED X FILE FORMAT
				 * ///////////////////////////////////////////////////////////////////////
				 *    [xhead]
				 *    2 major
				 *    2 minor
				 *    4 type    // bzip,tzip
				 *    [mszip_master_head]
				 *    4 unkn    // checksum?
				 *    2 unkn    // flags? (seems to be constant)
				 *    [mszip_head]
				 *    2 ofs     // offset to next section
				 *    2 magic   // 'CK'
				 *    ... ofs bytes of data
				 *    ... next mszip_head
				 *
				 *  http://www.kdedevelopers.org/node/3181 has been very helpful.
				 * ///////////////////////////////////////////////////////////////////////
				 */

				// Read file size after decompression excluding header
				uint uncompressedFinalSize = BitConverter.ToUInt32(Buffer, currentPosition) - 16;
				currentPosition += 4;

				// Preparing for decompression
				MemoryStream inputStream = new MemoryStream(Buffer);
				MemoryStream outputStream = new MemoryStream();
				int currentBlock = 0;
				byte[] previousBlockBytes = new byte[(int)MSZIP_BLOCK];
				while (currentPosition + 3 < End)
				{
#pragma warning disable CS0219
					// Read compressed block size after decompression
					ushort uncompressedBlockSize = BitConverter.ToUInt16(Buffer, currentPosition);
					currentPosition += 2;
#pragma warning restore CS0219

					// Read compressed block size
					ushort compressedBlockSize = BitConverter.ToUInt16(Buffer, currentPosition);
					currentPosition += 2;

					// Check compressed block size
					if (compressedBlockSize > MSZIP_BLOCK)
					{
						inputStream.Dispose();
						outputStream.Dispose();
						throw new FormatException("Compressed block size is larger than MSZIP standard.");
					}

					if (currentPosition + compressedBlockSize > End + 2)
					{
						inputStream.Dispose();
						outputStream.Dispose();
						throw new InvalidDataException("X: Unexpected EOF in compressed chunk");
					}

					// Check MSZIP signature of compressed block
					ushort signature = BitConverter.ToUInt16(Buffer, currentPosition);

					if (signature != MSZIP_MAGIC)
					{
						inputStream.Dispose();
						outputStream.Dispose();
						throw new InvalidDataException("The compressed block's signature is incorrect.");
					}

					// Skip MSZIP signature
					inputStream.Position = currentPosition + 2;

					// Decompress the compressed block
					byte[] blockBytes = new byte[compressedBlockSize];
					inputStream.Read(blockBytes, 0, compressedBlockSize);
					byte[] decompressedBytes = DeflateCompression.ZlibDecompressWithDictionary(blockBytes, currentBlock == 0 ? null : previousBlockBytes);

					outputStream.Write(decompressedBytes, 0, decompressedBytes.Length);
					previousBlockBytes = decompressedBytes;

					// Preparing to move to the next data block
					currentPosition += compressedBlockSize;
					currentBlock++;
				}

				// ok, update pointers to point to the uncompressed file data
				Buffer = outputStream.ToArray();
				outputStream.Dispose();
				inputStream.Dispose();
				currentPosition = 0;
				End = Buffer.Length;

				if (Buffer.Length != uncompressedFinalSize)
				{
					throw new InvalidDataException("The size after decompression is incorrect.");
				}
			}
			else
			{
				// start reading here
				ReadUntilEndOfLine();
			}

			if (!IsBinaryFormat)
			{
				textEncoding = TextEncoding.GetSystemEncodingFromBytes(Buffer);
			}

			Scene = new Scene();
			ParseFile();

			// filter the imported hierarchy for some degenerated cases
			if (Scene.RootNode != null)
			{
				FilterHierarchy(Scene.RootNode);
			}
		}

		/** Returns the temporary representation of the imported data */
		public Scene GetImportedData()
		{
			return Scene;
		}

		protected void ParseFile()
		{
			while (true)
			{
				// read name of next object
				string objectName = GetNextToken();
				if (objectName.Length == 0)
				{
					break;
				}

				switch (objectName.ToLowerInvariant())
				{
					// parse specific object
					case "template":
						ParseDataObjectTemplate();
						break;
					case "frame":
						ParseDataObjectFrame(null);
						break;
					case "mesh":
						{
							// some meshes have no frames at all
							ParseDataObjectMesh(out Mesh mesh);
							Scene.GlobalMeshes.Add(mesh);
							break;
						}
					case "animtickspersecond":
						ParseDataObjectAnimTicksPerSecond();
						break;
					case "animationset":
						ParseDataObjectAnimationSet();
						break;
					case "material":
						{
							// Material outside of a mesh or node
							ParseDataObjectMaterial(out Material material);
							Scene.GlobalMaterials.Add(material);
							break;
						}
					case "}":
						// whatever?
						Debug.WriteLine("} found in dataObject");
						break;
					case "header":
						ParseUnknownDataObject();
						break;
					default:
						// unknown format
						Debug.WriteLine("Unknown data object in animation of .x file");
						ParseUnknownDataObject();
						break;
				}
			}
		}

		protected void ParseDataObjectTemplate()
		{
			// parse a template data object. Currently not stored.
			// ReSharper disable once NotAccessedVariable
			ReadHeadOfDataObject(out string name);

			// read GUID
			string guid = GetNextToken();

			// read and ignore data members
			while (true)
			{
				string s = GetNextToken();

				if (s == "}")
				{
					break;
				}

				if (s.Length == 0)
				{
					ThrowException("Unexpected end of file reached while parsing template definition");
				}
			}
		}

		protected void ParseDataObjectFrame(Node parent)
		{
			// A coordinate frame, or "frame of reference." The Frame template
			// is open and can contain any object. The Direct3D extensions (D3DX)
			// mesh-loading functions recognize Mesh, FrameTransformMatrix, and
			// Frame template instances as child objects when loading a Frame
			// instance.
			ReadHeadOfDataObject(out string name);

			// create a named node and place it at its parent, if given
			Node node = new Node(name, parent);

			if (parent != null)
			{
				parent.Children.Add(node);
			}
			else
			{
				// there might be multiple root nodes
				if (Scene.RootNode != null)
				{
					// place a dummy root if not there
					if (Scene.RootNode.Name != "$dummy_root")
					{
						Node exroot = Scene.RootNode;
						Scene.RootNode = new Node("$dummy_root");
						Scene.RootNode.Children.Add(exroot);
						exroot.Parent = Scene.RootNode;
					}
					// put the new node as its child instead
					Scene.RootNode.Children.Add(node);
					node.Parent = Scene.RootNode;
				}
				else
				{
					// it's the first node imported. place it as root
					Scene.RootNode = node;
				}
			}

			// Now inside a frame.
			// read tokens until closing brace is reached.
			bool frameFinished = false;
			while (true)
			{
				string objectName = GetNextToken();
				if (objectName.Length == 0)
				{
					ThrowException("Unexpected end of file reached while parsing frame");
				}

				switch (objectName.ToLowerInvariant())
				{
					case "}":
						frameFinished = true;
						break;
					case "frame":
						ParseDataObjectFrame(node); // child frame
						break;
					case "frametransformmatrix":
						ParseDataObjectTransformationMatrix(out node.TrafoMatrix);
						break;
					case "mesh":
						{
							ParseDataObjectMesh(out Mesh mesh);
							node.Meshes.Add(mesh);
							break;
						}
					default:
						Debug.WriteLine("Unknown data object in frame in x file");
						ParseUnknownDataObject();
						break;
				}

				if (frameFinished)
				{
					break;
				}
			}
		}

		protected void ParseDataObjectTransformationMatrix(out Matrix4D matrix)
		{
			// read header, we're not interested if it has a name
			ReadHeadOfDataObject();

			// read its components
			matrix = new Matrix4D();
			matrix.Row0.X = ReadFloat();
			matrix.Row0.Y = ReadFloat();
			matrix.Row0.Z = ReadFloat();
			matrix.Row0.W = ReadFloat();
			matrix.Row1.X = ReadFloat();
			matrix.Row1.Y = ReadFloat();
			matrix.Row1.Z = ReadFloat();
			matrix.Row1.W = ReadFloat();
			matrix.Row2.X = ReadFloat();
			matrix.Row2.Y = ReadFloat();
			matrix.Row2.Z = ReadFloat();
			matrix.Row2.W = ReadFloat();
			matrix.Row3.X = ReadFloat();
			matrix.Row3.Y = ReadFloat();
			matrix.Row3.Z = ReadFloat();
			matrix.Row3.W = ReadFloat();
			// trailing symbols
			CheckForSemicolon();
			CheckForClosingBrace();
		}

		protected void ParseDataObjectMesh(out Mesh mesh)
		{
			mesh = new Mesh();

			// ReSharper disable once NotAccessedVariable
			ReadHeadOfDataObject(out string name);

			// read vertex count
			uint numVertices = ReadInt();
			mesh.Positions = new List<Vector3>((int)numVertices);

			// read vertices
			for (int a = 0; a < (int)numVertices; a++)
			{
				mesh.Positions.Add(ReadVector3());
			}

			if ((int) numVertices == 0)
			{
				TestForSeparator();
			}

			// read position faces
			uint numPosFaces = ReadInt();
			mesh.PosFaces = new List<Face>((int)numPosFaces);
			for (int a = 0; a < (int)numPosFaces; a++)
			{
				// read indices
				uint numIndices = ReadInt();
				Face face = new Face();
				for (int b = 0; b < (int)numIndices; b++)
				{
					face.Indices.Add(ReadInt());
				}
				mesh.PosFaces.Add(face);
				TestForSeparator();
			}

			if ((int) numPosFaces == 0)
			{
				TestForSeparator();
			}

			// here, other data objects may follow
			bool meshFinished = false;
			while (true)
			{
				string objectName = GetNextToken();
				switch (objectName.ToLowerInvariant())
				{
					case "":
						ThrowException("Unexpected end of file while parsing mesh structure");
						break;
					case "}":
						meshFinished = true;
						break;
					case "meshnormals":
						ParseDataObjectMeshNormals(ref mesh);
						break;
					case "meshtexturecoords":
						ParseDataObjectMeshTextureCoords(ref mesh);
						break;
					case "meshvertexcolors":
						ParseDataObjectMeshVertexColors(ref mesh);
						break;
					case "meshmateriallist":
						ParseDataObjectMeshMaterialList(ref mesh);
						break;
					case "vertexduplicationindices":
						ParseUnknownDataObject(); // we'll ignore vertex duplication indices
						break;
					case "xskinmeshheader":
						ParseDataObjectSkinMeshHeader(ref mesh);
						break;
					case "skinweights":
						ParseDataObjectSkinWeights(ref mesh);
						break;
					case "fvfdata":
						ParseUnknownDataObject(true);
						// ignore
						break;
					default:
						Debug.WriteLine("Unknown data object in mesh in x file");
						ParseUnknownDataObject();
						break;
				}
				if (meshFinished)
				{
					break;
				}
			}
		}

		protected void ParseDataObjectSkinWeights(ref Mesh mesh)
		{
			ReadHeadOfDataObject();

			GetNextTokenAsString(out string transformNodeName);

			Bone bone = new Bone(transformNodeName);

			// read vertex weights
			uint numWeights = ReadInt();
			bone.Weights = new List<BoneWeight>((int)numWeights);

			for (int a = 0; a < (int)numWeights; a++)
			{
				BoneWeight weight = new BoneWeight(ReadInt());
				bone.Weights.Add(weight);
			}

			// read vertex weights
			for (int a = 0; a < (int)numWeights; a++)
			{
				bone.Weights[a].Weight = ReadFloat();
			}

			// read matrix offset
			bone.OffsetMatrix.Row0.X = ReadFloat();
			bone.OffsetMatrix.Row1.X = ReadFloat();
			bone.OffsetMatrix.Row2.X = ReadFloat();
			bone.OffsetMatrix.Row3.X = ReadFloat();
			bone.OffsetMatrix.Row0.Y = ReadFloat();
			bone.OffsetMatrix.Row1.Y = ReadFloat();
			bone.OffsetMatrix.Row2.Y = ReadFloat();
			bone.OffsetMatrix.Row3.Y = ReadFloat();
			bone.OffsetMatrix.Row0.Z = ReadFloat();
			bone.OffsetMatrix.Row1.Z = ReadFloat();
			bone.OffsetMatrix.Row2.Z = ReadFloat();
			bone.OffsetMatrix.Row3.Z = ReadFloat();
			bone.OffsetMatrix.Row0.W = ReadFloat();
			bone.OffsetMatrix.Row1.W = ReadFloat();
			bone.OffsetMatrix.Row2.W = ReadFloat();
			bone.OffsetMatrix.Row3.W = ReadFloat();

			mesh.Bones.Add(bone);

			CheckForSemicolon();
			CheckForClosingBrace();
		}

		protected void ParseDataObjectSkinMeshHeader(ref Mesh mesh)
		{
			ReadHeadOfDataObject();

			/*unsigned int maxSkinWeightsPerVertex =*/
			ReadInt();
			/*unsigned int maxSkinWeightsPerFace =*/
			ReadInt();
			/*unsigned int numBonesInMesh = */
			ReadInt();

			CheckForClosingBrace();
		}

		protected void ParseDataObjectMeshNormals(ref Mesh mesh)
		{
			ReadHeadOfDataObject();

			// read count
			uint numNormals = ReadInt();
			mesh.Normals = new List<Vector3>((int)numNormals);

			// read normal vectors
			for (int a = 0; a < (int)numNormals; a++)
			{
				mesh.Normals.Add(ReadVector3());
			}

			if ((int) numNormals == 0)
			{
				TestForSeparator();
			}

			// read normal indices
			uint numFaces = ReadInt();
			if (numFaces != (uint)mesh.PosFaces.Count)
			{
				ThrowException("Normal face count does not match vertex face count.");
			}

			for (int a = 0; a < (int)numFaces; a++)
			{
				uint numIndices = ReadInt();
				Face face = new Face();

				for (int b = 0; b < (int)numIndices; b++)
				{
					face.Indices.Add(ReadInt());
				}
				mesh.NormFaces.Add(face);

				TestForSeparator();
			}

			if ((int) numFaces == 0)
			{
				TestForSeparator();
			}
			CheckForClosingBrace();
		}

		protected void ParseDataObjectMeshTextureCoords(ref Mesh mesh)
		{
			ReadHeadOfDataObject();

			if (mesh.NumTextures + 1 > Mesh.AI_MAX_NUMBER_OF_TEXTURECOORDS)
			{
				ThrowException("Too many sets of texture coordinates");
			}

			uint numCoords = ReadInt();
			if (numCoords != mesh.Positions.Count)
			{
				ThrowException("Texture coord count does not match vertex count");
			}

			List<Vector2> coords = new List<Vector2>((int)numCoords);
			for (int a = 0; a < (int)numCoords; a++)
			{
				coords.Add(ReadVector2());
			}
			mesh.TexCoords[(int)mesh.NumTextures++] = coords;

			if ((int) numCoords == 0)
			{
				TestForSeparator();
			}
			CheckForClosingBrace();
		}

		protected void ParseDataObjectMeshVertexColors(ref Mesh mesh)
		{
			ReadHeadOfDataObject();
			if (mesh.NumColorSets + 1 > Mesh.AI_MAX_NUMBER_OF_COLOR_SETS)
			{
				ThrowException("Too many colorsets");
			}

			uint numColors = ReadInt();
			if (numColors != mesh.Positions.Count)
			{
				ThrowException("Vertex color count does not match vertex count");
			}

			List<Color128> colors = Enumerable.Repeat(Color128.Black, (int)numColors).ToList();
			for (int a = 0; a < (int)numColors; a++)
			{
				uint index = ReadInt();
				if (index >= mesh.Positions.Count)
				{
					ThrowException("Vertex color index out of bounds");
				}
				colors[(int)index] = ReadRGBA();
				// HACK: (thom) Maxon Cinema XPort plugin puts a third separator here, kwxPort puts a comma.
				// Ignore gracefully.
				if (!IsBinaryFormat)
				{
					FindNextNoneWhiteSpace();
					if (Buffer[currentPosition] == ';' || Buffer[currentPosition] == ',')
					{
						currentPosition++;
					}
				}
			}
			mesh.Colors[(int)mesh.NumColorSets++] = colors;

			if ((int) numColors == 0)
			{
				TestForSeparator();
			}
			CheckForClosingBrace();
		}

		protected void ParseDataObjectMeshMaterialList(ref Mesh mesh)
		{
			ReadHeadOfDataObject();

			// read material count
			/*unsigned int numMaterials =*/
			ReadInt();
			// read non triangulated face material index count
			uint numMatIndices = ReadInt();

			// some models have a material index count of 1... to be able to read them we
			// replicate this single material index on every face
			if (numMatIndices != mesh.PosFaces.Count && numMatIndices != 1)
			{
				ThrowException("Per-Face material index count does not match face count.");
			}

			// read per-face material indices
			for (int a = 0; a < (int)numMatIndices; a++)
			{
				mesh.FaceMaterials.Add(ReadInt());
			}

			// in version 03.02, the face indices end with two semicolons.
			// commented out version check, as version 03.03 exported from blender also has 2 semicolons
			if (!IsBinaryFormat) // && MajorVersion == 3 && MinorVersion <= 2)
			{
				if (currentPosition < End && Buffer[currentPosition] == ';')
				{
					++currentPosition;
				}
			}

			// if there was only a single material index, replicate it on all faces
			while (mesh.FaceMaterials.Count < mesh.PosFaces.Count)
			{
				mesh.FaceMaterials.Add(mesh.FaceMaterials.First());
			}

			// read following data objects
			while (true)
			{
				string objectName = GetNextToken();
				if (objectName.Length == 0)
				{
					ThrowException("Unexpected end of file while parsing mesh material list.");
				}
				else if (objectName == "}")
				{
					break; // material list finished
				}
				else if (objectName == "{")
				{
					// template materials
					string matName = GetNextToken();
					Material material = new Material(true, matName);
					//Use default BVE white color so this is visible if the global material is missing, otherwise will be overwritten
					material.Diffuse = Color128.White;
					mesh.Materials.Add(material);

					CheckForClosingBrace(); // skip }
				}
				else if (objectName == "Material")
				{
					ParseDataObjectMaterial(out Material material);
					mesh.Materials.Add(material);
				}
				else if (objectName == ";")
				{
					// ignore
				}
				else
				{
					Debug.WriteLine("Unknown data object in material list in x file");
					ParseUnknownDataObject();
				}
			}
		}

		protected void ParseDataObjectMaterial(out Material material)
		{
			ReadHeadOfDataObject(out string matName);
			if (matName.Length == 0)
			{
				matName = "material" + LineNumber;
			}
			material = new Material(false, matName);

			// read material values
			material.Diffuse = ReadRGBA();
			material.SpecularExponent = ReadFloat();
			material.Specular = ReadRGB();
			material.Emissive = ReadRGB();

			// read other data objects
			while (true)
			{
				string objectName = GetNextToken();
				if (objectName.Length == 0)
				{
					ThrowException("Unexpected end of file while parsing mesh material");
				}
				else if (objectName == "}")
				{
					break; // material finished
				}
				else if (objectName == "TextureFilename" || objectName == "TextureFileName")
				{
					// some exporters write "TextureFileName" instead.
					ParseDataObjectTextureFilename(out string texname);
					material.Textures.Add(new TexEntry(texname));
				}
				else if (objectName == "NormalmapFilename" || objectName == "NormalmapFileName")
				{
					// one exporter writes out the normal map in a separate filename tag
					ParseDataObjectTextureFilename(out string texname);
					material.Textures.Add(new TexEntry(texname, true));
				}
				else
				{
					Debug.WriteLine("Unknown data object in material in x file");
					ParseUnknownDataObject();
				}
			}
		}

		protected void ParseDataObjectAnimTicksPerSecond()
		{
			ReadHeadOfDataObject();
			Scene.AnimTicksPerSecond = ReadInt();
			CheckForClosingBrace();
		}

		protected void ParseDataObjectAnimationSet()
		{
			ReadHeadOfDataObject(out string animName);

			Animation anim = new Animation(animName);

			while (true)
			{
				string objectName = GetNextToken();
				if (objectName.Length == 0)
				{
					ThrowException("Unexpected end of file while parsing animation set.");
				}
				else if (objectName == "}")
				{
					break; // animation set finished
				}
				else if (objectName == "Animation")
				{
					ParseDataObjectAnimation(ref anim);
				}
				else
				{
					Debug.WriteLine("Unknown data object in animation set in x file");
					ParseUnknownDataObject();
				}
			}

			Scene.Anims.Add(anim);
		}

		protected void ParseDataObjectAnimation(ref Animation anim)
		{
			ReadHeadOfDataObject();
			AnimBone banim = new AnimBone();

			while (true)
			{
				string objectName = GetNextToken();

				if (objectName.Length == 0)
				{
					ThrowException("Unexpected end of file while parsing animation.");
				}
				else if (objectName == "}")
				{
					break; // animation finished
				}
				else if (objectName == "AnimationKey")
				{
					ParseDataObjectAnimationKey(ref banim);
				}
				else if (objectName == "AnimationOptions")
				{
					ParseUnknownDataObject(); // not interested
				}
				else if (objectName == "{")
				{
					// read frame name
					banim.BoneName = GetNextToken();
					CheckForClosingBrace();
				}
				else
				{
					Debug.WriteLine("Unknown data object in animation in x file");
					ParseUnknownDataObject();
				}
			}

			anim.Anims.Add(banim);
		}

		protected void ParseDataObjectAnimationKey(ref AnimBone animBone)
		{
			ReadHeadOfDataObject();

			// read key type
			uint keyType = ReadInt();

			// read number of keys
			uint numKeys = ReadInt();

			for (int a = 0; a < (int)numKeys; a++)
			{
				// read time
				uint time = ReadInt();

				// read keys
				switch (keyType)
				{
					case 0: // rotation quaternion
						{
							// read count
							if (ReadInt() != 4)
							{
								ThrowException("Invalid number of arguments for quaternion key in animation");
							}
							Quaternion quat = new Quaternion();
							quat.W = ReadFloat();
							quat.X = ReadFloat();
							quat.Y = ReadFloat();
							quat.Z = ReadFloat();

							QuatKey key = new QuatKey((double)time, quat);
							animBone.RotKeys.Add(key);

							CheckForSemicolon();
						}
						break;
					case 1: // scale vector
					case 2: // position vector
						{
							// read count
							if (ReadInt() != 3)
							{
								ThrowException("Invalid number of arguments for vector key in animation");
							}
							VectorKey key = new VectorKey((double)time, ReadVector3());

							if (keyType == 2)
							{
								animBone.PosKeys.Add(key);
							}
							else
							{
								animBone.ScaleKeys.Add(key);
							}
						}
						break;
					case 3: // combined transformation matrix
					case 4: // denoted both as 3 or as 4
						{
							// read count
							if (ReadInt() != 16)
							{
								ThrowException("Invalid number of arguments for matrix key in animation");
							}

							// read matrix
							Matrix4D matrix = new Matrix4D();
							matrix.Row0.X = ReadFloat();
							matrix.Row0.Y = ReadFloat();
							matrix.Row0.Z = ReadFloat();
							matrix.Row0.W = ReadFloat();
							matrix.Row1.X = ReadFloat();
							matrix.Row1.Y = ReadFloat();
							matrix.Row1.Z = ReadFloat();
							matrix.Row1.W = ReadFloat();
							matrix.Row2.X = ReadFloat();
							matrix.Row2.Y = ReadFloat();
							matrix.Row2.Z = ReadFloat();
							matrix.Row2.W = ReadFloat();
							matrix.Row3.X = ReadFloat();
							matrix.Row3.Y = ReadFloat();
							matrix.Row3.Z = ReadFloat();
							matrix.Row3.W = ReadFloat();

							MatrixKey key = new MatrixKey((double)time, matrix);
							animBone.TrafoKeys.Add(key);

							CheckForSemicolon();
						}
						break;
					default:
						ThrowException("Unknown key type " + keyType + " in animation.");
						break;
				} // end switch

				// key separator
				CheckForSeparator();
			}

			CheckForClosingBrace();
		}

		protected void ParseDataObjectTextureFilename(out string name)
		{
			ReadHeadOfDataObject();
			GetNextTokenAsString(out name);
			CheckForClosingBrace();

			// FIX: some files (e.g. AnimationTest.x) have "" as texture file name
			if (name.Length == 0)
			{
				Debug.WriteLine("Length of texture file name is zero. Skipping this texture.");
			}

			// some exporters write double backslash paths out. We simply replace them if we find them
			name = name.Replace("\\\\", "\\");
		}

		protected void ParseUnknownDataObject(bool allowEmptyName = false)
		{
			// find opening delimiter
			while (true)
			{
				string t = GetNextToken();
				if (t.Length == 0 && !allowEmptyName) // fvfdata allows empty name
				{
					ThrowException("Unexpected end of file while parsing unknown segment.");
				}

				if (t == "{")
				{
					break;
				}
			}

			uint counter = 1;

			// parse until closing delimiter
			while (counter > 0)
			{
				string t = GetNextToken();
				if (t.Length == 0)
				{
					ThrowException("Unexpected end of file while parsing unknown segment.");
				}

				if (t == "{")
				{
					++counter;
				}
				else if (t == "}")
				{
					--counter;
				}
			}
		}

		//! places pointer to next begin of a token, and ignores comments
		protected void FindNextNoneWhiteSpace()
		{
			if (IsBinaryFormat)
			{
				return;
			}

			while (true)
			{
				while (currentPosition < End && char.IsWhiteSpace((char)Buffer[currentPosition]))
				{
					if (Buffer[currentPosition] == '\n')
					{
						LineNumber++;
					}
					++currentPosition;
				}

				if (currentPosition >= End)
				{
					return;
				}

				// check if this is a comment
				if ((Buffer[currentPosition] == '/' && Buffer[currentPosition + 1] == '/') || Buffer[currentPosition] == '#')
				{
					ReadUntilEndOfLine();
				}
				else
				{
					break;
				}
			}
		}

		//! returns next parseable token. Returns empty string if no token there
		protected string GetNextToken()
		{
			string s = string.Empty;

			// process binary-formatted file
			if (IsBinaryFormat)
			{
				// in binary mode it will only return NAME and STRING token
				// and (correctly) skip over other tokens.
				if (End - currentPosition < 2)
				{
					return s;
				}
				uint tok = ReadBinWord();
				uint len;
				ASCIIEncoding Ascii = new ASCIIEncoding();

				// standalone tokens
				switch (tok)
				{
					case 1:
						// name token
						if (End - currentPosition < 4)
						{
							return s;
						}
						len = ReadBinDWord();
						if (End - currentPosition < (int)len)
						{
							return s;
						}
						s = Ascii.GetString(Buffer, currentPosition, (int)len);
						currentPosition += (int)len;
						return s;
					case 2:
						// string token
						if (End - currentPosition < 4)
						{
							return s;
						}
						len = ReadBinDWord();
						if (End - currentPosition < (int)len)
						{
							return s;
						}
						s = Ascii.GetString(Buffer, currentPosition, (int)len);
						currentPosition += (int)(len + 2);
						return s;
					case 3:
						// integer token
						currentPosition += 4;
						return "<integer>";
					case 5:
						// GUID token
						currentPosition += 16;
						return "<guid>";
					case 6:
						if (End - currentPosition < 4) return s;
						len = ReadBinDWord();
						currentPosition += (int)(len * 4);
						return "<int_list>";
					case 7:
						if (End - currentPosition < 4) return s;
						len = ReadBinDWord();
						currentPosition += (int)(len * BinaryFloatSize);
						return "<flt_list>";
					case 0x0a:
						return "{";
					case 0x0b:
						return "}";
					case 0x0c:
						return "(";
					case 0x0d:
						return ")";
					case 0x0e:
						return "[";
					case 0x0f:
						return "]";
					case 0x10:
						return "<";
					case 0x11:
						return ">";
					case 0x12:
						return ".";
					case 0x13:
						return ",";
					case 0x14:
						return ";";
					case 0x1f:
						return "template";
					case 0x28:
						return "WORD";
					case 0x29:
						return "DWORD";
					case 0x2a:
						return "FLOAT";
					case 0x2b:
						return "DOUBLE";
					case 0x2c:
						return "CHAR";
					case 0x2d:
						return "UCHAR";
					case 0x2e:
						return "SWORD";
					case 0x2f:
						return "SDWORD";
					case 0x30:
						return "void";
					case 0x31:
						return "string";
					case 0x32:
						return "unicode";
					case 0x33:
						return "cstring";
					case 0x34:
						return "array";
				}
			}
			// process text-formatted file
			else
			{
				FindNextNoneWhiteSpace();
				if (currentPosition > End)
				{
					return s;
				}

				while (currentPosition < End && !char.IsWhiteSpace((char)Buffer[currentPosition]))
				{
					// either keep token delimiters when already holding a token, or return if first valid char
					if (Buffer[currentPosition] == ';' || Buffer[currentPosition] == '}' || Buffer[currentPosition] == '{' || Buffer[currentPosition] == ',')
					{
						if (s.Length == 0)
						{
							s += (char)Buffer[currentPosition++];
						}
						break; // stop for delimiter
					}
					s += (char)Buffer[currentPosition++];
				}
			}
			return s;
		}

		protected void ReadHeadOfDataObject()
		{
			string nameOrBrace = GetNextToken();
			if (nameOrBrace != "{")
			{
				if (GetNextToken() != "{")
				{
					ThrowException("Opening brace expected.");
				}
			}
		}

		protected void ReadHeadOfDataObject(out string name)
		{
			name = string.Empty;

			string nameOrBrace = GetNextToken();
			if (nameOrBrace != "{")
			{
				name = nameOrBrace;

				if (GetNextToken() != "{")
				{
					ThrowException("Opening brace expected.");
				}
			}
		}

		//! checks for closing curly brace
		protected void CheckForClosingBrace()
		{
			if (GetNextToken() != "}")
			{
				ThrowException("Closing brace expected.");
			}
		}

		//! checks for one following semicolon
		protected void CheckForSemicolon()
		{
			if (IsBinaryFormat)
			{
				return;
			}

			if (GetNextToken() != ";")
			{
				ThrowException("Semicolon expected.");
			}
		}

		//! checks for a separator char, either a ',' or a ';'
		protected void CheckForSeparator()
		{
			if (IsBinaryFormat)
			{
				return;
			}

			string token = GetNextToken();
			if (token != "," && token != ";")
			{
				ThrowException("Separator character (';' or ',') expected.");
			}
		}

		// tests and possibly consumes a separator char, but does nothing if there was no separator
		protected void TestForSeparator()
		{
			if (IsBinaryFormat)
			{
				return;
			}

			FindNextNoneWhiteSpace();
			if (currentPosition > End)
			{
				return;
			}

			// test and skip
			if (Buffer[currentPosition] == ';' || Buffer[currentPosition] == ',')
			{
				currentPosition++;
			}
		}

		protected void GetNextTokenAsString(out string token)
		{
			if (IsBinaryFormat)
			{
				token = GetNextToken();
				return;
			}

			FindNextNoneWhiteSpace();
			if (currentPosition >= End)
			{
				ThrowException("Unexpected end of file while parsing string");
			}

			if (Buffer[currentPosition] != '"')
			{
				ThrowException("Expected quotation mark.");
			}
			++currentPosition;

			List<byte> textBuffer = new List<byte>();
			while (currentPosition < End && Buffer[currentPosition] != '"')
			{
				textBuffer.Add(Buffer[currentPosition++]);
			}
			token = textEncoding.GetString(textBuffer.ToArray());

			if (currentPosition >= End - 1)
			{
				ThrowException("Unexpected end of file while parsing string");
			}

			if (Buffer[currentPosition + 1] != ';' || Buffer[currentPosition] != '"')
			{
				ThrowException("Expected quotation mark and semicolon at the end of a string.");
			}
			currentPosition += 2;
		}

		protected void ReadUntilEndOfLine()
		{
			if (IsBinaryFormat)
			{
				return;
			}

			while (currentPosition < End)
			{
				if (Buffer[currentPosition] == '\n' || Buffer[currentPosition] == '\r')
				{
					++currentPosition;
					LineNumber++;
					return;
				}

				++currentPosition;
			}
		}

		protected ushort ReadBinWord()
		{
			Debug.Assert(End - currentPosition >= 2);
			ushort tmp = BitConverter.ToUInt16(Buffer, currentPosition);
			currentPosition += 2;
			return tmp;
		}

		protected uint ReadBinDWord()
		{
			Debug.Assert(End - currentPosition >= 4);
			uint tmp = BitConverter.ToUInt32(Buffer, currentPosition);
			currentPosition += 4;
			return tmp;
		}

		protected uint ReadInt()
		{
			if (IsBinaryFormat)
			{
				if (BinaryNumCount == 0 && End - currentPosition >= 2)
				{
					ushort tmp = ReadBinWord(); // 0x06 or 0x03
					// array of ints follows
					if (tmp == 0x06 && End - currentPosition >= 4)
					{
						BinaryNumCount = ReadBinDWord();
					}
					// single int follows
					else
					{
						BinaryNumCount = 1;
					}
				}

				--BinaryNumCount;
				if (End - currentPosition >= 4)
				{
					return ReadBinDWord();
				}
				else
				{
					currentPosition = End;
					return 0;
				}
			}
			else
			{
				FindNextNoneWhiteSpace();

				// TODO: consider using strtol10 instead???

				// check preceding minus sign
				bool isNegative = false;
				if (Buffer[currentPosition] == '-')
				{
					isNegative = true;
					currentPosition++;
				}

				// at least one digit expected
				if (!char.IsDigit((char)Buffer[currentPosition]))
				{
					ThrowException("Number expected.");
				}

				// read digits
				uint number = 0;
				while (currentPosition < End)
				{
					if (!char.IsDigit((char)Buffer[currentPosition]))
					{
						break;
					}
					number = number * 10 + (uint)(Buffer[currentPosition] - 48);
					currentPosition++;
				}

				CheckForSeparator();
				return isNegative ? (uint)(-(int)number) : number;
			}
		}

		protected float ReadFloat()
		{
			float result;

			if (IsBinaryFormat)
			{
				if (BinaryNumCount == 0 && End - currentPosition >= 2)
				{
					ushort tmp = ReadBinWord(); // 0x07 or 0x42
					// array of floats following
					if (tmp == 0x07 && End - currentPosition >= 4)
					{
						BinaryNumCount = ReadBinDWord();
					}
					// single float following
					else
					{
						BinaryNumCount = 1;
					}
				}

				--BinaryNumCount;
				if (BinaryFloatSize == 8)
				{
					if (End - currentPosition >= 8)
					{
						result = (float)BitConverter.ToDouble(Buffer, currentPosition);
						currentPosition += 8;
						return result;
					}

					currentPosition = End;
					return 0.0f;
				}

				if (End - currentPosition >= 4)
				{
					result = BitConverter.ToSingle(Buffer, currentPosition);
					currentPosition += 4;
					return result;
				}

				currentPosition = End;
				return 0.0f;
			}

			// text version
			FindNextNoneWhiteSpace();
			// check for various special strings to allow reading files from faulty exporters
			// I mean you, Blender!
			// Reading is safe because of the terminating zero
			ASCIIEncoding Ascii = new ASCIIEncoding();
			if (Ascii.GetString(Buffer, currentPosition, 9) == "-1.#IND00" || Ascii.GetString(Buffer, currentPosition, 8) == "1.#IND00")
			{
				currentPosition += 9;
				CheckForSeparator();
				return 0.0f;
			}
			else if (Ascii.GetString(Buffer, currentPosition, 8) == "1.#QNAN0")
			{
				currentPosition += 8;
				CheckForSeparator();
				return 0.0f;
			}

			currentPosition = ConvertToFloat(currentPosition, out result);

			CheckForSeparator();

			return result;
		}

		protected Vector2 ReadVector2()
		{
			Vector2 vector;
			vector.X = ReadFloat();
			vector.Y = ReadFloat();
			TestForSeparator();

			return vector;
		}

		protected Vector3 ReadVector3()
		{
			Vector3 vector;
			vector.X = ReadFloat();
			vector.Y = ReadFloat();
			vector.Z = ReadFloat();
			TestForSeparator();

			return vector;
		}

		protected Color128 ReadRGBA()
		{
			Color128 color;
			color.R = ReadFloat();
			color.G = ReadFloat();
			color.B = ReadFloat();
			color.A = ReadFloat();
			TestForSeparator();

			return color;
		}

		protected Color128 ReadRGB()
		{
			Color128 color;
			color.R = ReadFloat();
			color.G = ReadFloat();
			color.B = ReadFloat();
			color.A = 1.0f;
			TestForSeparator();

			return color;
		}

		protected void ThrowException(string text)
		{
			if (IsBinaryFormat)
			{
				throw new Exception(text);
			}
			throw new Exception("Line " + LineNumber + ": " + text);
		}

		// Filters the imported hierarchy for some degenerated cases that some exporters produce.
		protected void FilterHierarchy(Node node)
		{
			// if the node has just a single unnamed child containing a mesh, remove
			// the anonymous node between. The 3DSMax kwXport plugin seems to produce this
			// mess in some cases
			if (node.Children.Count == 1 && node.Meshes.Count == 0)
			{
				Node child = node.Children.First();
				if (child.Name.Length == 0 && child.Meshes.Count > 0)
				{
					// transfer its meshes to us
					for (int a = 0; a < child.Meshes.Count; a++)
					{
						node.Meshes.Add(child.Meshes[a]);
					}
					child.Meshes.Clear();

					// transfer the transform as well
					node.TrafoMatrix *= child.TrafoMatrix;

					// then kill it
					node.Children.Clear();
				}
			}

			// recurse
			for (int a = 0; a < node.Children.Count; a++)
			{
				FilterHierarchy(node.Children[a]);
			}
		}

		protected int ConvertToFloat(int position, out float result, bool check_comma = true)
		{
			ASCIIEncoding Ascii = new ASCIIEncoding();

			float f = 0.0f;

			bool inv = (Buffer[position] == '-');
			if (inv || Buffer[position] == '+')
			{
				++position;
			}

			if (string.Compare(Ascii.GetString(Buffer, position, 3), "nan", StringComparison.OrdinalIgnoreCase) == 0)
			{
				result = float.NaN;
				position += 3;
				return position;
			}

			if (string.Compare(Ascii.GetString(Buffer, position, 3), "inf", StringComparison.OrdinalIgnoreCase) == 0)
			{
				result = float.PositiveInfinity;
				if (inv)
				{
					result = -result;
				}
				position += 3;
				if (string.Compare(Ascii.GetString(Buffer, position, 5), "inity", StringComparison.OrdinalIgnoreCase) == 0)
				{
					position += 5;
				}
				return position;
			}

			if (!char.IsDigit((char)Buffer[position]) && !((Buffer[position] == '.' || (check_comma && Buffer[position] == ',')) && char.IsDigit((char)Buffer[position + 1])))
			{
				ThrowException("Cannot parse string as real number: does not start with digit or decimal point followed by digit.");
			}

			string tmp = string.Empty;

			while (position < End)
			{
				if (char.IsDigit((char)Buffer[position]) || Buffer[position] == '.' || Buffer[position] == 'e' || Buffer[position] == 'E' || Buffer[position] == '+' || Buffer[position] == '-')
				{
					tmp += (char)Buffer[position];
					++position;
				}
				else
				{
					break;
				}
			}

			try
			{
				f = float.Parse(tmp, NumberStyles.Number, CultureInfo.InvariantCulture);
			}
			catch (Exception e)
			{
				ThrowException(e.Message);
			}

			if (inv)
			{
				f = -f;
			}

			result = f;
			return position;
		}
	}
}
