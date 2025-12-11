using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Formats.OpenBve
{
	public enum PatchDatabaseKey
	{
		Hash,
		FileName,
		LineEndingFix,
		ColonFix,
		IgnorePitchRoll,
		LogMessage,
		CylinderHack,
		Expression,
		XParser,
		DummyRailTypes,
		DummyGroundTypes,
		Derailments,
		Toppling,
		SignalSet,
		AccurateObjectDisposal,
		SplitLineHack,
		AllowTrackPositionArguments,
		DisableSemiTransparentFaces,
		ReducedColorTransparency,
		ViewingDistance,
		MaxViewingDistance,
		AggressiveRWBrackets,
		Incompatible,
		DelayedAnimatedUpdates,
		AdhesionHack
	}
}
