// ReSharper disable InconsistentNaming
namespace Object.Wavefront
{
	internal enum WavefrontObjCommands
	{
		/// <summary>Unrecognised / unknown command</summary>
		Unknown = 0,
		/// <summary>Adds a vertex</summary>
		V,
		/// <summary>Adds texture co-ordinates for a vertex</summary>
		VT,
		/// <summary>Adds normals for a vertex</summary>
		VN,
		/// <summary>Adds a parameter space vertex</summary>
		VP,
		/// <summary>Adds a face</summary>
		F,
		/// <summary>Creates a new object</summary>
		/// <remarks>Rougly analagous to a new MeshBuilder</remarks>
		O,
		/// <summary>Starts a new face group</summary>
		/// <remarks>Usually applies a new texture</remarks>
		G,
		/// <summary>Changes the smoothing group</summary>
		S,
		/// <summary>Loads a material library</summary>
		MtlLib,
		/// <summary>
		/// Uses a material from the library
		/// </summary>
		UseMtl,
	}

	internal enum WavefrontMtlCommands
	{
		/// <summary>Unrecognised / unknown command</summary>
		Unknown = 0,
		/// <summary>Creates a new material</summary>
		NewMtl,
		/// <summary>Sets the ambient light color for the material</summary>
		Ka,
		/// <summary>Sets the color for the material</summary>
		Kd,
		/// <summary>Sets the specular color for the material</summary>
		Ks,
		/// <summary>Sets the emissive color for the material</summary>
		Ke,
		/// <summary>Sets the dissolution (alpha) value for the material</summary>
		D,
		/// <summary>Sets the ambient light-map texture for the material</summary>
		Map_Ka,
		/// <summary>Sets the texture for the material</summary>
		Map_Kd,
		/// <summary>Sets the emissive light-map texture for the material</summary>
		Map_Ke,
		/// <summary>Sets the illumination mode for the material</summary>
		Illum
	}
}
