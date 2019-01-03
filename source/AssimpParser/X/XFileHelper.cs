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
using OpenTK;
using OpenTK.Graphics;
using VectorKey = System.Collections.Generic.KeyValuePair<double, OpenTK.Vector3>;
using QuatKey = System.Collections.Generic.KeyValuePair<double, OpenTK.Quaternion>;
using MatrixKey = System.Collections.Generic.KeyValuePair<double, OpenTK.Matrix4>;

namespace AssimpNET.X
{
	/** Helper structure representing a XFile mesh face */
	public class Face
	{
		public List<uint> Indices = new List<uint>();
	}

	/** Helper structure representing a texture filename inside a material and its potential source */
	public class TexEntry
	{
		public string Name;
		public bool IsNormalMap; // true if the texname was specified in a NormalmapFilename tag

		public TexEntry(string name, bool isNormalMap = false)
		{
			Name = name;
			IsNormalMap = isNormalMap;
		}
	}

	/** Helper structure representing a XFile material */
	public class Material
	{
		public string Name;
		public bool IsReference; // if true, mName holds a name by which the actual material can be found in the material list
		public Color4 Diffuse;
		public float SpecularExponent;
		public Color4 Specular;
		public Color4 Emissive;
		public List<TexEntry> Textures = new List<TexEntry>();

		public uint SceneIndex; // the index under which it was stored in the scene's material list
	}

	/** Helper structure to represent a bone weight */
	public class BoneWeight
	{
		public uint Vertex;
		public float Weight;
	}

	/** Helper structure to represent a bone in a mesh */
	public class Bone
	{
		public string Name;
		public List<BoneWeight> Weights = new List<BoneWeight>();
		public Matrix4 OffsetMatrix;
	}

	/** Helper structure to represent an XFile mesh */
	public class Mesh
	{
		public const uint AI_MAX_NUMBER_OF_TEXTURECOORDS = 0x8;
		public const uint AI_MAX_NUMBER_OF_COLOR_SETS = 0x8;

		public string Name;
		public List<Vector3> Positions = new List<Vector3>();
		public List<Face> PosFaces = new List<Face>();
		public List<Vector3> Normals = new List<Vector3>();
		public List<Face> NormFaces = new List<Face>();
		public uint NumTextures = 0;
		public List<Vector2>[] TexCoords = new List<Vector2>[AI_MAX_NUMBER_OF_TEXTURECOORDS];
		public uint NumColorSets = 0;
		public List<Color4>[] Colors = new List<Color4>[AI_MAX_NUMBER_OF_COLOR_SETS];

		public List<uint> FaceMaterials = new List<uint>();
		public List<Material> Materials = new List<Material>();

		public List<Bone> Bones = new List<Bone>();

		public Mesh(string name = "")
		{
			Name = name;
		}
	}

	/** Helper structure to represent a XFile frame */
	public class Node
	{
		public string Name;
		public Matrix4 TrafoMatrix;
		public Node Parent;
		public List<Node> Children = new List<Node>();
		public List<Mesh> Meshes = new List<Mesh>();

		public Node(Node parent = null)
		{
			Parent = parent;
		}
	}

	/** Helper structure representing a single animated bone in a XFile */
	public class AnimBone
	{
		public string BoneName;
		public List<VectorKey> PosKeys = new List<VectorKey>();  // either three separate key sequences for position, rotation, scaling
		public List<QuatKey> RotKeys = new List<QuatKey>();
		public List<VectorKey> ScaleKeys = new List<VectorKey>();
		public List<MatrixKey> TrafoKeys = new List<MatrixKey>(); // or a combined key sequence of transformation matrices.
	}

	/** Helper structure to represent an animation set in a XFile */
	public class Animation
	{
		public string Name;
		public List<AnimBone> Anims = new List<AnimBone>();
	}

	/** Helper structure analogue to aiScene */
	public class Scene
	{
		public Node RootNode = null;
		public List<Mesh> GlobalMeshes = new List<Mesh>(); // global meshes found outside of any frames
		public List<Material> GlobalMaterials = new List<Material>(); // global materials found outside of any meshes.

		public List<Animation> Anims = new List<Animation>();
		public uint AnimTicksPerSecond = 0;
	}
}
