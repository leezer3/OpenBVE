namespace OpenBveApi.Hosts
{
	/// <summary>Represents the type of problem that is reported to the host.</summary>
	public enum ProblemType
	{
		/// <summary>Indicates that a file could not be found.</summary>
		FileNotFound = 1,
		/// <summary>Indicates that a directory could not be found.</summary>
		DirectoryNotFound = 2,
		/// <summary>Indicates that a file or directory could not be found.</summary>
		PathNotFound = 3,
		/// <summary>Indicates invalid data in a file or directory.</summary>
		InvalidData = 4,
		/// <summary>Indicates an invalid operation.</summary>
		InvalidOperation = 5,
		/// <summary>Indicates an unexpected exception.</summary>
		UnexpectedException = 6,
		/// <summary>Indicates that the data was recognised, but is not supported</summary>
		UnsupportedData,
	}
}
