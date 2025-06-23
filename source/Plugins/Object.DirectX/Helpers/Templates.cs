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

// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
namespace OpenBve.Formats.DirectX
{
	public enum TemplateID : uint
	{
		Mesh,
		Vector,
		MeshFace,
		MeshMaterialList,
		Material,
		ColorRGB,
		ColorRGBA,
		TextureFilename,
		MeshTextureCoords,
		Coords2d,
		MeshVertexColors,
		MeshNormal,
		MeshNormals,
		VertexColor,
		FrameRoot,
		Frame,
		FrameTransformMatrix,
		Matrix4x4,
		Header,
		IndexedColor,
		Boolean,
		Boolean2d,
		MaterialWrap,
		MeshFaceWraps,
		Template,
		// https://learn.microsoft.com/en-us/windows/win32/direct3d9/fvfdata
		FVFData,
		// https://learn.microsoft.com/en-us/windows/win32/direct3d9/decldata
		DeclData,

		//Templates below this are not in the Microsoft DirectX specification
		//However, the X file format is extensible by declaring the template structure at the top
		//of your object

		//Source: 3DS Max files
		ObjectMatrixComment,
		SkinWeights,
		XSkinMeshHeader,
		AnimTicksPerSecond,
		AnimationSet,

		//Source: Mesquioa zipped txt
		VertexDuplicationIndices,
		
		//Special case handler
		//Not actually a block, just the key of the root block
		TextureKey

	}
}
