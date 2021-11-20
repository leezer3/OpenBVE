namespace Plugin
{
	/// <summary>Control codes controlling the next action of the GIF decoder</summary>
	internal enum ControlCode
	{
		/// <summary>Start of a new image</summary>
		ImageSeparator = 0x2C,
		/// <summary>Start of an extension block</summary>
		ExtensionBlock = 0x21,
		/// <summary>EOF marker</summary>
		Terminator = 0x3b,
		/*
		 * Extension block identifiers
		 */
		/// <summary>The extension block contains a Graphics Extension</summary>
		GraphicsExtension = 0xf9,
		/// <summary>The extension block contains an Application Extension</summary>
		ApplicationExtension = 0xff,
		/// <summary>The extension block overlays text onto the image</summary>
		TextOverlay = 0x01,
		/// <summary>The extension block contains a comment</summary>
		Comment = 0xfe,
		/*
		 * Misc
		 */
		/// <summary>Not as per the specification, but probably harmless, so skip</summary>
		BadByte = 0x00
	}
}
