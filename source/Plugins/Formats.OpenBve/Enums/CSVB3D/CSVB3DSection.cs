namespace Formats.OpenBve
{
	/// <summary>The sections of a CSV or B3D format file</summary>
	public enum CSVB3DSection
	{
		/// <summary>Creates a new MeshBuilder section</summary>
		MeshBuilder = int.MinValue,
		CreateMeshBuilder = MeshBuilder,
		/// <summary>Starts a Texture section</summary>
		Texture,
		LoadTexture = Texture
	}
}
