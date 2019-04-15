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
using System.Diagnostics;
using System.IO;
using OpenTK;

namespace AssimpNET.Obj
{
	public class ObjFileParser : ObjTools
	{
		/// Default material name
		private const string DEFAULT_MATERIAL = Material.AI_DEFAULT_MATERIAL_NAME;

		private const string DefaultObjName = "defaultobject";

		/// Path to the current model, name of the obj file where the buffer comes from
		private string OriginalObjFileName;

		private string ObjFilePath;

		/// @brief  The default constructor.
		public ObjFileParser()
		{
			Model = null;
			Line = 0;
			OriginalObjFileName = string.Empty;
		}

		/// @brief  Constructor with data array.
		public ObjFileParser(string[] lines, string modelName, string originalObjFileName, string objFilePath)
		{
			Model = null;
			Line = 0;
			OriginalObjFileName = originalObjFileName;
			ObjFilePath = objFilePath;

			// Create the model instance to store all the data
			Model = new Model();
			Model.ModelName = modelName;

			// create default material and store it
			Model.DefaultMaterial = new Material();
			Model.DefaultMaterial.MaterialName = DEFAULT_MATERIAL;
			Model.MaterialLib.Add(DEFAULT_MATERIAL);
			Model.MaterialMap[DEFAULT_MATERIAL] = Model.DefaultMaterial;

			// Start parsing the file
			ParseFile(lines);
		}

		public Model GetModel()
		{
			return Model;
		}

		/// Parse the loaded file
		protected void ParseFile(string[] lines)
		{
			foreach (string buffer in lines)
			{
				Buffer = buffer;
				DataIt = 0;
				DataEnd = Buffer.Length;

				if (Buffer.Length == 0)
				{
					continue;
				}

				// parse line
				switch (Buffer[DataIt])
				{
					case 'v': // Parse a vertex texture coordinate
						{
							++DataIt;
							if (Buffer[DataIt] == ' ' || Buffer[DataIt] == '\t')
							{
								int numComponents = GetNumComponentsInDataDefinition();
								switch (numComponents)
								{
									case 3:
										// read in vertex definition
										GetVector3(Model.Vertices);
										break;
									case 4:
										// read in vertex definition (homogeneous coords)
										GetHomogeneousVector3(Model.Vertices);
										break;
									case 6:
										// read vertex and vertex-color
										GetTwoVectors3(Model.Vertices, Model.VertexColors);
										break;
									default:
										throw new Exception(numComponents + " arguments were supplied. A vertex must supply either 3, 4 or 6 arguments.");
								}
							}
							else if (Buffer[DataIt] == 't')
							{
								// read in texture coordinate ( 2D or 3D )
								++DataIt;
								GetVector(Model.TextureCoord);
							}
							else if (Buffer[DataIt] == 'n')
							{
								// Read in normal vector definition
								++DataIt;
								GetVector3(Model.Normals);
							}
						}
						break;
					case 'p': // Parse a face, line or point statement
					case 'l':
					case 'f':
						{
							GetFace(Buffer[DataIt] == 'f' ? PrimitiveType.PrimitiveType_POLYGON : (Buffer[DataIt] == 'l' ? PrimitiveType.PrimitiveType_LINE : PrimitiveType.PrimitiveType_POINT));
						}
						break;
					case '#': // Parse a comment
						{
							GetComment();
						}
						break;
					case 'u': // Parse a material desc. setter
						{
							string name;

							GetNameNoSpace(DataIt, DataEnd, out name);

							int nextSpace = name.IndexOf(" ", StringComparison.InvariantCulture);
							if (nextSpace != -1)
							{
								name = name.Substring(0, nextSpace);
							}

							if (name == "usemtl")
							{
								GetMaterialDesc();
							}
						}
						break;
					case 'm': // Parse a material library or merging group ('mg')
						{
							string name;

							GetNameNoSpace(DataIt, DataEnd, out name);

							int nextSpace = name.IndexOf(" ", StringComparison.InvariantCulture);
							if (nextSpace != -1)
							{
								name = name.Substring(0, nextSpace);
							}

							if (name == "mg")
							{
								GetGroupNumberAndResolution();
							}
							else if (name == "mtllib")
							{
								GetMaterialLib();
							}
							else
							{
								DataIt = SkipLine(DataIt, DataEnd, ref Line);
							}
						}
						break;
					case 'g': // Parse group name
						{
							GetGroupName();
						}
						break;
					case 's': // Parse group number
						{
							GetGroupNumber();
						}
						break;
					case 'o': // Parse object name
						{
							GetObjectName();
						}
						break;
					default:
						DataIt = SkipLine(DataIt, DataEnd, ref Line);
						break;
				}
			}
		}

		protected void GetVector(List<Vector3> point3dArray)
		{
			int numComponents = GetNumComponentsInDataDefinition();
			Vector3 v;
			string tmp;
			switch (numComponents)
			{
				case 2:
					CopyNextWord(out tmp);
					v.X = float.Parse(tmp);

					CopyNextWord(out tmp);
					v.Y = float.Parse(tmp);

					v.Z = 0.0f;
					break;
				case 3:
					CopyNextWord(out tmp);
					v.X = float.Parse(tmp);

					CopyNextWord(out tmp);
					v.Y = float.Parse(tmp);

					CopyNextWord(out tmp);
					v.Z = float.Parse(tmp);
					break;
				default:
					throw new Exception(numComponents + " arguments were supplied. A vector must supply either 2 or 3 arguments.");
			}
			point3dArray.Add(v);
			DataIt = SkipLine(DataIt, DataEnd, ref Line);
		}

		protected void GetVector3(List<Vector3> point3dArray)
		{
			Vector3 v;
			string tmp;
			CopyNextWord(out tmp);
			v.X = float.Parse(tmp);

			CopyNextWord(out tmp);
			v.Y = float.Parse(tmp);

			CopyNextWord(out tmp);
			v.Z = float.Parse(tmp);

			point3dArray.Add(v);
			DataIt = SkipLine(DataIt, DataEnd, ref Line);
		}

		protected void GetHomogeneousVector3(List<Vector3> point3dArray)
		{
			Vector3 v;
			string tmp;
			CopyNextWord(out tmp);
			v.X = float.Parse(tmp);

			CopyNextWord(out tmp);
			v.Y = float.Parse(tmp);

			CopyNextWord(out tmp);
			v.Z = float.Parse(tmp);

			CopyNextWord(out tmp);
			float w = float.Parse(tmp);

			Debug.Assert(w != 0);

			v /= w;
			point3dArray.Add(v);
			DataIt = SkipLine(DataIt, DataEnd, ref Line);
		}

		protected void GetTwoVectors3(List<Vector3> point3dArrayA, List<Vector3> point3dArrayB)
		{
			Vector3 a;
			string tmp;
			CopyNextWord(out tmp);
			a.X = float.Parse(tmp);

			CopyNextWord(out tmp);
			a.Y = float.Parse(tmp);

			CopyNextWord(out tmp);
			a.Z = float.Parse(tmp);

			point3dArrayA.Add(a);

			Vector3 b;
			CopyNextWord(out tmp);
			b.X = float.Parse(tmp);

			CopyNextWord(out tmp);
			b.Y = float.Parse(tmp);

			CopyNextWord(out tmp);
			b.Z = float.Parse(tmp);

			point3dArrayB.Add(b);

			DataIt = SkipLine(DataIt, DataEnd, ref Line);
		}

		protected void GetFace(PrimitiveType type)
		{
			DataIt = GetNextToken(DataIt, DataEnd);
			if (DataIt == DataEnd || Buffer[DataIt] == '\0')
			{
				return;
			}

			Face face = new Face(type);
			bool hasNormal = false;

			int vSize = Model.Vertices.Count;
			int vtSize = Model.TextureCoord.Count;
			int vnSize = Model.Normals.Count;

			bool vt = vtSize != 0;
			bool vn = vnSize != 0;
			int iPos = 0;
			while (DataIt != DataEnd)
			{
				int iStep = 1;

				if (IsLineEnd(DataIt))
				{
					break;
				}

				if (Buffer[DataIt] == '/')
				{
					if (type == PrimitiveType.PrimitiveType_POINT)
					{
						Debug.WriteLine("Obj: Separator unexpected in point statement");
					}
					if (iPos == 0)
					{
						//if there are no texture coordinates in the file, but normals
						if (!vt && vn)
						{
							iPos = 1;
							iStep++;
						}
					}
					iPos++;
				}
				else if (IsSpaceOrNewLine(DataIt))
				{
					iPos = 0;
				}
				else
				{
					//OBJ USES 1 Base ARRAYS!!!!
					int iVal = ConvertToInt(DataIt);

					// increment iStep position based off of the sign and # of digits
					int tmp = iVal;
					if (iVal < 0)
					{
						++iStep;
					}
					while ((tmp = tmp / 10) != 0)
					{
						++iStep;
					}

					if (iVal > 0)
					{
						// Store parsed index
						if (iPos == 0)
						{
							face.Vertices.Add((uint)(iVal - 1));
						}
						else if (iPos == 1)
						{
							face.TexturCoords.Add((uint)(iVal - 1));
						}
						else if (iPos == 2)
						{
							face.Normals.Add((uint)(iVal - 1));
						}
						else
						{
							ReportErrorTokenInFace();
						}
					}
					else if (iVal < 0)
					{
						// Store relatively index
						if (iPos == 0)
						{
							face.Vertices.Add((uint)(vSize+iVal));
						}
						else if (iPos == 1)
						{
							face.TexturCoords.Add((uint)(vtSize+iVal));
						}
						else if (iPos == 2)
						{
							face.Normals.Add((uint)(vnSize+iVal));
						}
						else
						{
							ReportErrorTokenInFace();
						}
					}
					else
					{
						//On error, std::atoi will return 0 which is not a valid value
						throw new Exception("OBJ: Invalid face indice");
					}
				}
				DataIt += iStep;
			}

			if (face.Vertices.Count == 0)
			{
				Debug.WriteLine("Obj: Ignoring empty face");
				// skip line and clean up
				DataIt = SkipLine(DataIt, DataEnd, ref Line);
				return;
			}

			// Set active material, if one set
			if (Model.CurrentMaterial != null)
			{
				face.Material = Model.CurrentMaterial;
			}
			else
			{
				face.Material = Model.DefaultMaterial;
			}

			// Create a default object, if nothing is there
			if (Model.Current == null)
			{
				CreateObject(DefaultObjName);
			}

			// Assign face to mesh
			if (Model.CurrentMesh == null)
			{
				CreateMesh(DefaultObjName);
			}

			// Store the face
			Model.CurrentMesh.Faces.Add(face);
			Model.CurrentMesh.NumIndices += (uint)face.Vertices.Count;
			Model.CurrentMesh.UVCoordinates[0] += (uint)face.TexturCoords.Count;
			if (!Model.CurrentMesh.HasNormals && hasNormal)
			{
				Model.CurrentMesh.HasNormals = true;
			}
			// Skip the rest of the line
			DataIt = SkipLine(DataIt, DataEnd, ref Line);
		}

		protected void ReportErrorTokenInFace()
		{
			DataIt = SkipLine(DataIt, DataEnd, ref Line);
			Debug.WriteLine("OBJ: Not supported token in face description detected");
		}

		//  Creates a new object instance
		protected void CreateObject(string objName)
		{
			Debug.Assert(Model != null);

			Model.Current = new Object();
			Model.Current.ObjName = objName;
			Model.Objects.Add(Model.Current);

			CreateMesh(objName);

			if (Model.CurrentMaterial != null)
			{
				Model.CurrentMesh.MaterialIndex = (uint)GetMaterialIndex(Model.CurrentMaterial.MaterialName);
				Model.CurrentMesh.Material = Model.CurrentMaterial;
			}
		}

		protected void CreateMesh(string meshName)
		{
			Debug.Assert(Model != null);

			Model.CurrentMesh = new Mesh(meshName);
			Model.Meshes.Add(Model.CurrentMesh);
			uint meshId = (uint)(Model.Meshes.Count - 1);
			if (Model.Current != null)
			{
				Model.Current.Meshes.Add(meshId);
			}
			else
			{
				Debug.WriteLine("OBJ: No object detected to attach a new mesh instance.");
			}
		}

		protected int GetMaterialIndex(string materialName)
		{
			if (materialName.Length == 0)
			{
				return -1;
			}
			return Model.MaterialLib.IndexOf(materialName);
		}

		//  Get a comment, values will be skipped
		protected void GetComment()
		{
			DataIt = SkipLine(DataIt, DataEnd, ref Line);
		}


		protected void GetMaterialDesc()
		{
			// Get next data for material data
			DataIt = GetNextToken(DataIt, DataEnd);
			if (DataIt == DataEnd)
			{
				return;
			}

			int start = DataIt;
			while (DataIt != DataEnd && !IsLineEnd(DataIt))
			{
				++DataIt;
			}

			// In some cases we should ignore this 'usemtl' command, this variable helps us to do so
			bool skip = false;

			string name = Buffer.Substring(start, DataIt - start);
			name = name.Trim();
			if (name.Length == 0)
			{
				skip = true;
			}

			// If the current mesh has the same material, we simply ignore that 'usemtl' command
			// There is no need to create another object or even mesh here
			if (Model.CurrentMaterial != null && Model.CurrentMaterial.MaterialName == name)
			{
				skip = true;
			}

			if (!skip)
			{
				// Search for material
				Material tmp;
				if (!Model.MaterialMap.TryGetValue(name, out tmp))
				{
					// Not found, so we don't know anything about the material except for its name.
					// This may be the case if the material library is missing. We don't want to lose all
					// materials if that happens, so create a new named material instead of discarding it
					// completely.
					Debug.WriteLine("OBJ: failed to locate material " + name + ", creating new material");
					Model.CurrentMaterial = new Material();
					Model.CurrentMaterial.MaterialName = name;
					Model.MaterialLib.Add(name);
					Model.MaterialMap[name] = Model.CurrentMaterial;
				}
				else
				{
					// Found, using detected material
					Model.CurrentMaterial = tmp;
				}

				if (NeedsNewMesh(name))
				{
					CreateMesh(name);
				}

				Model.CurrentMesh.MaterialIndex = (uint)GetMaterialIndex(name);
			}

			// Skip rest of line
			DataIt = SkipLine(DataIt, DataEnd, ref Line);
		}

		//  Returns true, if a new mesh must be created.
		protected bool NeedsNewMesh(string materialName)
		{
			// If no mesh data yet
			if (Model.CurrentMesh == null)
			{
				return true;
			}
			bool newMat = false;
			uint matIdx = (uint)GetMaterialIndex(materialName);
			uint curMatIdx = Model.CurrentMesh.MaterialIndex;

			// no need create a new mesh if no faces in current
			// lets say 'usemtl' goes straight after 'g'
			if (curMatIdx != Mesh.NoMaterial && curMatIdx != matIdx && Model.CurrentMesh.Faces.Count > 0)
			{
				// New material -> only one material per mesh, so we need to create a new
				// material
				newMat = true;
			}

			return newMat;
		}

		//  Not supported
		protected void GetGroupNumberAndResolution()
		{
			// Not used
			DataIt = SkipLine(DataIt, DataEnd, ref Line);
		}

		//  Get material library from file.
		protected void GetMaterialLib()
		{
			// Translate tuple
			DataIt = GetNextToken(DataIt, DataEnd);
			if (DataIt == DataEnd)
			{
				return;
			}

			int start = DataIt;
			while (DataIt != DataEnd && !IsLineEnd(DataIt))
			{
				++DataIt;
			}

			// Check for existence
			string matName = Buffer.Substring(start, DataIt - start);
			string absName;

			// Check if directive is valid.
			if (matName.Length == 0)
			{
				Debug.WriteLine("OBJ: no name for material library specified.");
				return;
			}

			string dir = Path.GetDirectoryName(ObjFilePath);
			if (dir != null)
			{
				absName = Path.Combine(dir, matName);
			}
			else
			{
				Debug.WriteLine("OBJ: no name for material library specified.");
				return;
			}

			if (!File.Exists(absName))
			{
				Debug.WriteLine("OBJ: Unable to locate material file " + matName);
				string matFallbackName = OriginalObjFileName.Substring(0, OriginalObjFileName.Length - 3) + "mtl";
				Debug.WriteLine("OBJ: Opening fallback material file " + matFallbackName);
				if (!File.Exists(matFallbackName))
				{
					Debug.WriteLine("OBJ: Unable to locate fallback material file " + matFallbackName);
					DataIt = SkipLine(DataIt, DataEnd, ref Line);
					return;
				}
				absName = matFallbackName;
			}

			// Import material library data from file.
			// Some exporters (e.g. Silo) will happily write out empty
			// material files if the model doesn't use any materials, so we
			// allow that.
			ObjFileMtlImporter importer = new ObjFileMtlImporter(File.ReadAllLines(absName), ref Model);
		}

		//  Getter for a group name.
		protected void GetGroupName()
		{
			string groupName;

			// here we skip 'g ' from line
			DataIt = GetNextToken(DataIt, DataEnd);
			DataIt = GetName(DataIt, DataEnd, out groupName);
			if (IsEndOfBuffer(DataIt, DataEnd))
			{
				return;
			}

			// Change active group, if necessary
			if (Model.ActiveGroup != groupName)
			{
				// We are mapping groups into the object structure
				CreateObject(groupName);

				// Search for already existing entry
				// New group name, creating a new entry
				List<uint> tmp;
				if (!Model.Groups.TryGetValue(groupName, out tmp))
				{
					List<uint> faceIDArray = new List<uint>();
					Model.Groups[groupName] = faceIDArray;
					Model.GroupFaceIDs = faceIDArray;
				}
				else
				{
					Model.GroupFaceIDs = tmp;
				}
				Model.ActiveGroup = groupName;
			}
			DataIt = SkipLine(DataIt, DataEnd, ref Line);
		}

		//  Not supported
		protected void GetGroupNumber()
		{
			// Not used
			DataIt = SkipLine(DataIt, DataEnd, ref Line);
		}

		//  Stores values for a new object instance, name will be used to identify it.
		protected void GetObjectName()
		{
			DataIt = GetNextToken(DataIt, DataEnd);
			if (DataIt == DataEnd)
			{
				return;
			}
			int start = DataIt;
			while (DataIt != DataEnd && !IsSpaceOrNewLine(DataIt))
			{
				++DataIt;
			}

			string objectName = Buffer.Substring(start, DataIt - start);
			if (objectName.Length != 0)
			{
				// Reset current object
				Model.Current = null;

				// Search for actual object
				foreach (var tmp in Model.Objects)
				{
					if (tmp.ObjName == objectName)
					{
						Model.Current = tmp;
						break;
					}
				}

				// Allocate a new object, if current one was not found before
				if (Model.Current == null)
				{
					CreateObject(objectName);
				}
			}

			DataIt = SkipLine(DataIt, DataEnd, ref Line);
		}
	}
}
