using System;

namespace TrainManager.SafetySystems
{
	/// <summary>The default safety-systems supported by the inbuilt plugin</summary>
	/// 
	/// NOTE: These are the default Japanese safety systems as supported by BVE1 ==> 4
	[Flags]
	public enum DefaultSafetySystems
	{
		/// <summary>ATS-SN</summary>
		AtsSn = 1,
		/// <summary>AST-P</summary>
		AtsP = 2,
		/// <summary>ATC</summary>
		Atc = 4,
		/// <summary>EB</summary>
		Eb = 8
	}
}
