// ReSharper disable UnusedMember.Global
namespace OpenBveApi.Objects
{
	/// <summary>The exporter for any given model</summary>
	/// <remarks>Some exporters use a different handing / origin for texture co-ordinates and vertices</remarks>
	public enum ModelExporter
	{
		/// <summary>Unknown</summary>
		Unknown = -1,
		/// <summary>Unknown</summary>
		/// <remarks>Assumes a right-handed co-ordinate system</remarks>
		UnknownRightHanded,
		/// <summary>Mesquioa</summary>
		Mesquoia,

		// HACK: All left-handed co-ordinate systems should come after this point, as we can then use > to check
		/// <summary>Unknown</summary>
		/// <remarks>Assumes a right-handed co-ordinate system</remarks>
		UnknownLeftHanded = 100,
		/// <summary>Google SketchUp</summary>
		SketchUp,
		/// <summary>BlockBench</summary>
		BlockBench,
		/// <summary>Blender</summary>
		Blender
	}
}
