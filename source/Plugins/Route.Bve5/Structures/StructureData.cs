using System.Collections.Generic;
using OpenBveApi.Objects;
using OpenBveApi.Sounds;

namespace Bve5RouteParser
{
	internal struct StructureData
	{
		internal UnifiedObject[] Objects;
		internal Dictionary<int, SoundHandle> Sounds;
		internal Dictionary<int, SoundHandle> Sounds3D;
		internal int[][] Cycle;
		internal int[] Run;
		internal int[] Flange;
	}
}
