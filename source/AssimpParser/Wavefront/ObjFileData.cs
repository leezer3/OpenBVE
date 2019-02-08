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

using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using IndexArray = System.Collections.Generic.List<uint>;
using GroupMap = System.Collections.Generic.SortedDictionary<string, System.Collections.Generic.List<uint>>;

namespace AssimpNET.Obj
{
	// ------------------------------------------------------------------------------------------------
	//! \struct Face
	//! \brief  Data structure for a simple obj-face, describes discredit,l.ation and materials
	// ------------------------------------------------------------------------------------------------
	public class Face
	{
		//! Primitive type
		PrimitiveType PrimitiveType;
		//! Vertex indices
		public IndexArray Vertices = new IndexArray();
		//! Normal indices
		public IndexArray Normals = new IndexArray();
		//! Texture coordinates indices
		public IndexArray TexturCoords = new IndexArray();
		//! Pointer to assigned material
		public Material Material = null;

		//! \brief  Default constructor
		public Face(PrimitiveType pt = PrimitiveType.PrimitiveType_POLYGON)
		{
			PrimitiveType = pt;
		}
	}

	// ------------------------------------------------------------------------------------------------
	//! \struct Object
	//! \brief  Stores all objects of an obj-file object definition
	// ------------------------------------------------------------------------------------------------
	public class Object
	{
		enum ObjectType
		{
			ObjType,
			GroupType
		};

		//! Object name
		public string ObjName = string.Empty;
		//! Transformation matrix, stored in OpenGL format
#pragma warning disable 169
		Matrix4 Transformation;
#pragma warning restore 169
		//! All sub-objects referenced by this object
		List<Object> SubObjects = new List<Object>();
		/// Assigned meshes
		public List<uint> Meshes = new List<uint>();
	}

	// ------------------------------------------------------------------------------------------------
	//! \struct Material
	//! \brief  Data structure to store all material specific data
	// ------------------------------------------------------------------------------------------------
	public class Material
	{
		internal const string AI_DEFAULT_MATERIAL_NAME = "DefaultMaterial";

		//! Name of material description
		public string MaterialName;

		//! Texture names
		public string Texture;
		public string TextureSpecular;
		public string TextureAmbient;
		public string TextureEmissive;
		public string TextureBump;
		public string TextureNormal;
		public string[] TextureReflection = new string[6];
		public string TextureSpecularity;
		public string TextureOpacity;
		public string TextureDisp;

		public enum TextureType
		{
			TextureDiffuseType = 0,
			TextureSpecularType,
			TextureAmbientType,
			TextureEmissiveType,
			TextureBumpType,
			TextureNormalType,
			TextureReflectionSphereType,
			TextureReflectionCubeTopType,
			TextureReflectionCubeBottomType,
			TextureReflectionCubeFrontType,
			TextureReflectionCubeBackType,
			TextureReflectionCubeLeftType,
			TextureReflectionCubeRightType,
			TextureSpecularityType,
			TextureOpacityType,
			TextureDispType,
			TextureTypeCount
		};
		public bool[] Clamp = Enumerable.Repeat(false, (int)TextureType.TextureTypeCount).ToArray();

		//! Ambient color
		public Color4 Ambient;
		//! Diffuse color
		public Color4 Diffuse = new Color4(0.6f, 0.6f, 0.6f, 1.0f);
		//! Specular color
		public Color4 Specular;
		//! Emissive color
		public Color4 Emissive;
		//! Alpha value
		public float Alpha = 1.0f;
		//! Shineness factor
		public float Shineness;
		//! Illumination model
		public int IlluminationModel = 1;
		//! Index of refraction
		public float Ior = 1.0f;
		//! Transparency color
		public Color4 Transparent = new Color4(1.0f, 1.0f, 1.0f, 1.0f);
	}

	// ------------------------------------------------------------------------------------------------
	//! \struct Mesh
	//! \brief  Data structure to store a mesh
	// ------------------------------------------------------------------------------------------------
	public class Mesh
	{
		const uint AI_MAX_NUMBER_OF_TEXTURECOORDS = 0x8;

		public const uint NoMaterial = ~0u;
		/// The name for the mesh
		string Name;
		/// Array with pointer to all stored faces
		public List<Face> Faces = new List<Face>();
		/// Assigned material
		public Material Material = null;
		/// Number of stored indices.
		public uint NumIndices = 0;
		/// Number of UV
		public uint[] UVCoordinates = Enumerable.Repeat(0u, (int)AI_MAX_NUMBER_OF_TEXTURECOORDS).ToArray();
		/// Material index.
		public uint MaterialIndex = NoMaterial;
		/// True, if normals are stored.
		public bool HasNormals = false;
		/// True, if vertex colors are stored.
#pragma warning disable 169
		bool HasVertexColors;
#pragma warning restore 169

		/// Constructor
		public Mesh(string name)
		{
			Name = name;
		}
	}

	// ------------------------------------------------------------------------------------------------
	//! \struct Model
	//! \brief  Data structure to store all obj-specific model datas
	// ------------------------------------------------------------------------------------------------
	public class Model
	{
		//! Model name
		public string ModelName = string.Empty;
		//! List ob assigned objects
		public List<Object> Objects = new List<Object>();
		//! Pointer to current object
		public Object Current = null;
		//! Pointer to current material
		public Material CurrentMaterial = null;
		//! Pointer to default material
		public Material DefaultMaterial = null;
		//! Vector with all generated materials
		public List<string> MaterialLib = new List<string>();
		//! Vector with all generated vertices
		public List<Vector3> Vertices = new List<Vector3>();
		//! vector with all generated normals
		public List<Vector3> Normals = new List<Vector3>();
		//! vector with all vertex colors
		public List<Vector3> VertexColors = new List<Vector3>();
		//! Group map
		public GroupMap Groups = new GroupMap();
		//! Group to face id assignment
		public List<uint> GroupFaceIDs = new List<uint>();
		//! Active group
		public string ActiveGroup = string.Empty;
		//! Vector with generated texture coordinates
		public List<Vector3> TextureCoord = new List<Vector3>();
		//! Current mesh instance
		public Mesh CurrentMesh = null;
		//! Vector with stored meshes
		public List<Mesh> Meshes = new List<Mesh>();
		//! Material map
		public SortedDictionary<string, Material> MaterialMap = new SortedDictionary<string, Material>();
	}
}
