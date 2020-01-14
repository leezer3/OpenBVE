﻿using OpenBveApi.Math;

namespace OpenBveApi.Objects
{
	/// <summary>Properties related to glow</summary>
	public class Glow
	{
		/// <summary>Creates glow attenuation data from a half distance and a mode. The resulting value can be later passed to SplitGlowAttenuationData in order to reconstruct the parameters.</summary>
		/// <param name="HalfDistance">The distance at which the glow is at 50% of its full intensity. The value is clamped to the integer range from 1 to 4096. Values less than or equal to 0 disable glow attenuation.</param>
		/// <param name="Mode">The glow attenuation mode.</param>
		/// <returns>A System.UInt16 packed with the information about the half distance and glow attenuation mode.</returns>
		public static ushort GetAttenuationData(double HalfDistance, GlowAttenuationMode Mode) {
			if (HalfDistance <= 0.0 | Mode == GlowAttenuationMode.None) return 0;
			if (HalfDistance < 1.0) {
				HalfDistance = 1.0;
			} else if (HalfDistance > 4095.0) {
				HalfDistance = 4095.0;
			}
			return (ushort)((int)System.Math.Round(HalfDistance) | ((int)Mode << 12));
		}
		/// <summary>Recreates the half distance and the glow attenuation mode from a packed System.UInt16 that was created by GetGlowAttenuationData.</summary>
		/// <param name="Data">The data returned by GetGlowAttenuationData.</param>
		/// <param name="Mode">The mode of glow attenuation.</param>
		/// <param name="HalfDistance">The half distance of glow attenuation.</param>
		public static void SplitAttenuationData(ushort Data, out GlowAttenuationMode Mode, out double HalfDistance) {
			Mode = (GlowAttenuationMode)(Data >> 12);
			HalfDistance = (double)(Data & 4095);
		}

		/// <summary>Gets the current intensity glow intensity, using the glow attenuation factor</summary>
		/// <param name="ModelMatrix">The model transformation matrix to apply</param>
		/// <param name="Vertices">The verticies to which the glow is to be applied</param>
		/// <param name="Face">The face which these vertices make up</param>
		/// <param name="GlowAttenuationData">The current glow attenuation</param>
		/// <returns></returns>
		public static double GetDistanceFactor(Matrix4D ModelMatrix, VertexTemplate[] Vertices, ref MeshFace Face, ushort GlowAttenuationData)
		{
			if (Face.Vertices.Length == 0)
			{
				return 1.0;
			}
			GlowAttenuationMode mode;
			double halfdistance;
			Glow.SplitAttenuationData(GlowAttenuationData, out mode, out halfdistance);
			int i = (int)Face.Vertices[0].Index;
			Vector3 d = new Vector3(Vertices[i].Coordinates.X, Vertices[i].Coordinates.Y, -Vertices[i].Coordinates.Z);
			d.Transform(ModelMatrix);
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

		/// <summary>Gets the current intensity glow intensity, using the glow attenuation factor</summary>
		/// <param name="ModelMatrix">The model transformation matrix to apply</param>
		/// <param name="Vertices">The verticies to which the glow is to be applied</param>
		/// <param name="Face">The face which these vertices make up</param>
		/// <param name="GlowAttenuationData">The current glow attenuation</param>
		/// <param name="mode">The returned glow attenuation mode</param>
		/// <returns></returns>
		public static double GetDistanceFactor(Matrix4D ModelMatrix, VertexTemplate[] Vertices, ref MeshFace Face, ushort GlowAttenuationData, out GlowAttenuationMode mode)
		{
			mode = GlowAttenuationMode.None;
			if (Face.Vertices.Length == 0)
			{
				return 1.0;
			}
			
			double halfdistance;
			Glow.SplitAttenuationData(GlowAttenuationData, out mode, out halfdistance);
			int i = (int)Face.Vertices[0].Index;
			Vector3 d = new Vector3(Vertices[i].Coordinates.X, Vertices[i].Coordinates.Y, -Vertices[i].Coordinates.Z);
			d.Transform(ModelMatrix);
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
