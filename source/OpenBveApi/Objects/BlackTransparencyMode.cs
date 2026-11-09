namespace OpenBveApi
{
	/// <summary>The types of black transparency to apply</summary>
	public enum BlackTransparencyMode
    {
		/*
		 * Notes on black transparency
		 * ---------------------------
		 *
		 * BVE4 introduced the X file format.
		 * This only supported BMP textures.
		 *
		 * When OpenBVE was built, PNG textures were made available. 
		 * Unfortunately, the black transparency flag was kept for all
		 * textures used in X files, and some models rely on this.
		 *
		 * BVE5 supports both PNG and BMP format textures. Oddly, it
		 * only applies black transparency to BMP format textures.
		 */
		/// <summary>No black transparency is used</summary>
		None = 0,
		/// <summary>Black transparency is used in BMP format files only</summary>
		/// <remarks>For BVE5</remarks>
		BMPOnly = 1,
		/// <summary>Black transparency is used in all files</summary>
		AllFiles = 2
    }
}
