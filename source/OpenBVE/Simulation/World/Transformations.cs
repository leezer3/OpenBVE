using System;
using OpenBveApi.Math;
using OpenBveApi.World;

namespace OpenBve
{
	internal static partial class World
	{
		/// <summary>Creates the cross product of two vectors, input as induvidual co-ordinates</summary>
		internal static void Cross(double ax, double ay, double az, double bx, double by, double bz, out double cx, out double cy, out double cz)
		{
			cx = ay * bz - az * by;
			cy = az * bx - ax * bz;
			cz = ax * by - ay * bx;
		}

		/// <summary>Rotates one vector based upon a second vector, input as induvidual co-ordinates</summary>
		/// <param name="p">The vector to rotate</param>
		/// <param name="dx">The X co-ordinate of the second vector</param>
		/// <param name="dy">The Y co-ordinate of the second vector</param>
		/// <param name="dz">The Z co-ordinate of the second vector</param>
		/// <param name="cosa">The Cosine of the angle to rotate by</param>
		/// <param name="sina">The Sine of the angle to rotate by</param>
		internal static void Rotate(ref Vector3 p, double dx, double dy, double dz, double cosa, double sina)
		{
			double t = 1.0 / Math.Sqrt(dx * dx + dy * dy + dz * dz);
			dx *= t; dy *= t; dz *= t;
			double oc = 1.0 - cosa;
			double Opt1 = oc * dx * dy;
			double Opt2 = sina * dz;
			double Opt3 = oc * dy * dz;
			double Opt4 = sina * dx;
			double Opt5 = sina * dy;
			double Opt6 = oc * dx * dz;
			double x = (cosa + oc * dx * dx) * p.X + (Opt1 - Opt2) * p.Y + (Opt6 + Opt5) * p.Z;
			double y = (cosa + oc * dy * dy) * p.Y + (Opt1 + Opt2) * p.X + (Opt3 - Opt4) * p.Z;
			double z = (cosa + oc * dz * dz) * p.Z + (Opt6 - Opt5) * p.X + (Opt3 + Opt4) * p.Y;
			p.X = x; p.Y = y; p.Z = z;
		}

		/// <summary>Rotates one vector based upon a second vector, both input as induvidual co-ordinates</summary>
		/// <param name="px">The X co-ordinate of the first vector</param>
		/// <param name="py">The Y co-ordinate of the first vector</param>
		/// <param name="pz">The Z co-ordinate of the first vector</param>
		/// <param name="dx">The X co-ordinate of the second vector</param>
		/// <param name="dy">The Y co-ordinate of the second vector</param>
		/// <param name="dz">The Z co-ordinate of the second vector</param>
		/// <param name="cosa">The Cosine of the angle to rotate by</param>
		/// <param name="sina">The Sine of the angle to rotate by</param>
		internal static void Rotate(ref double px, ref double py, ref double pz, double dx, double dy, double dz, double cosa, double sina)
		{
			double t = 1.0 / Math.Sqrt(dx * dx + dy * dy + dz * dz);
			dx *= t; dy *= t; dz *= t;
			double oc = 1.0 - cosa;
			double Opt1 = oc * dx * dy;
			double Opt2 = sina * dz;
			double Opt3 = oc * dy * dz;
			double Opt4 = sina * dx;
			double Opt5 = sina * dy;
			double Opt6 = oc * dx * dz;
			double x = (cosa + oc * dx * dx) * px + (Opt1 - Opt2) * py + (Opt6 + Opt5) * pz;
			double y = (cosa + oc * dy * dy) * py + (Opt1 + Opt2) * px + (Opt3 - Opt4) * pz;
			double z = (cosa + oc * dz * dz) * pz + (Opt6 - Opt5) * px + (Opt3 + Opt4) * py;
			px = x; py = y; pz = z;
		}

		internal static void Rotate(ref double px, ref double py, ref double pz, double dx, double dy, double dz, double ux, double uy, double uz, double sx, double sy, double sz)
		{
			var x = sx * px + ux * py + dx * pz;
			var y = sy * px + uy * py + dy * pz;
			var z = sz * px + uz * py + dz * pz;
			px = x; py = y; pz = z;
		}

		internal static void Rotate(ref double px, ref double py, ref double pz, Transformation t)
		{
			var x = t.X.X * px + t.Y.X * py + t.Z.X * pz;
			var y = t.X.Y * px + t.Y.Y * py + t.Z.Y * pz;
			var z = t.X.Z * px + t.Y.Z * py + t.Z.Z * pz;
			px = x; py = y; pz = z;
		}
		internal static void RotatePlane(ref Vector3 Vector, double cosa, double sina)
		{
			double u = Vector.X * cosa - Vector.Z * sina;
			double v = Vector.X * sina + Vector.Z * cosa;
			Vector.X = u;
			Vector.Z = v;
		}
	}
}
