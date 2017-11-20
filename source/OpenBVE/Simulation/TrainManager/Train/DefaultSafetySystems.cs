using System;

namespace OpenBve
{
	/// <summary>The TrainManager is the root class containing functions to load and manage trains within the simulation world.</summary>
	public static partial class TrainManager
	{
		/// <summary>The default safety-systems supported by the inbuilt plugin</summary>
		/// 
		/// NOTE: These are the default Japanese safety systems as supported by BVE1 ==> 4
		[Flags]
		internal enum DefaultSafetySystems
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
}
