// ReSharper disable UnusedMember.Global
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
		VertexDuplicationIndices
	}
}
