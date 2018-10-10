namespace OpenBveApi.Graphics
{
	/// <summary>Describes the available openGL interpolation modes</summary>
	public enum InterpolationMode
	{
		/// <summary>Nearest Neighbour</summary>
		NearestNeighbor,
		/// <summary>Bilinear Filtering</summary>
		Bilinear,
		/// <summary>Nearest Neighbour with Mipmapping</summary>
		NearestNeighborMipmapped,
		/// <summary>Bilinear Filtering with Mipmapping</summary>
		BilinearMipmapped,
		/// <summary>
		/// Trilinear Filtering with Mipmapping</summary>
		TrilinearMipmapped,
		/// <summary>Anisotropic Filtering</summary>
		AnisotropicFiltering
	}
}
