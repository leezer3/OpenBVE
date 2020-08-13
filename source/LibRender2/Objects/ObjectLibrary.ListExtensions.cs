using System.Collections.Generic;
using OpenBveApi.Objects;

namespace LibRender2.Objects
{
	public static class ListExtensions
	{
		/// <summary>Disposes of all resources and clears the list</summary>
		public static void DisposeAndClear(this List<ObjectState> objectStates)
		{
			for (int i = 0; i < objectStates.Count; i++)
			{
				objectStates[i].Dispose();
			}
			objectStates.Clear();
		}

		/// <summary>Disposes of all resources and clears the list</summary>
		public static void DisposeAndClear(this List<FaceState> faceStates)
		{
			for (int i = 0; i < faceStates.Count; i++)
			{
				faceStates[i].Dispose();
			}
			faceStates.Clear();
		}
	}
}
