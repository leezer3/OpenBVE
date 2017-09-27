namespace OpenBveApi.Packages
{
	/// <summary>Provides functions for manipulating OpenBVE packages</summary>
	public static partial class Manipulation
	{
		/// <summary>The list of files which are to be skipped during package extraction</summary>
		private static readonly string[] filesToSkip = new[]
		{
			"package.xml",
			"package.png",
			"package.rtf",
			"thumbs.db",
			"packageinfo.xml"
		};
	}
}
