namespace OpenBveApi.Objects
{
	/// <summary>Describes the texture blending types available to be used for a MeshMaterial</summary>
	public enum MeshMaterialBlendMode : byte {
		/// <summary>Normal blending is used</summary>
		Normal = 0,
		/// <summary>Additive blending is used (e.g. signal glow)</summary>
		Additive = 1
	}
}
