using System.Collections.Generic;
using OpenBveApi.Objects;
using OpenBveApi.Sounds;

namespace Bve5RouteParser
{
	internal struct StructureData
	{
		internal UnifiedObject[] Objects;
		internal List<SoundHandle> Sounds;
		internal List<SoundHandle> Sounds3D;
		internal int[][] Cycle;
		internal int[] Run;
		internal int[] Flange;
	}
}
