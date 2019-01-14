using OpenBveApi.Math;
using OpenBveApi.Objects;

namespace OpenBve
{
	internal static partial class Renderer
	{
		/// <summary>Gets the current intensity glow intensity, using the glow attenuation factor</summary>
		/// <param name="Vertices">The verticies to which the glow is to be applied</param>
		/// <param name="Face">The face which these vertices make up</param>
		/// <param name="GlowAttenuationData">The current glow attenuation</param>
		/// <param name="Camera">The camera position</param>
		/// <returns></returns>
		private static double GetDistanceFactor(VertexTemplate[] Vertices, ref World.MeshFace Face, ushort GlowAttenuationData, Vector3 Camera)
		{
			if (Face.Vertices.Length == 0)
			{
				return 1.0;
			}
			GlowAttenuationMode mode;
			double halfdistance;
			World.SplitGlowAttenuationData(GlowAttenuationData, out mode, out halfdistance);
			int i = (int)Face.Vertices[0].Index;
			Vector3 d = new Vector3(Vertices[i].Coordinates - Camera);
			switch (mode)
			{
				case GlowAttenuationMode.DivisionExponent2:
					{
						double t = d.NormSquared();
						return t / (t + halfdistance * halfdistance);
					}
				case GlowAttenuationMode.DivisionExponent4:
					{
						double t = d.NormSquared();
						t *= t;
						halfdistance *= halfdistance;
						return t / (t + halfdistance * halfdistance);
					}
				default:
					return 1.0;
			}
		}
	}
}
