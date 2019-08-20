using OpenBveApi.Objects;

namespace OpenBve
{
	/*
	 * This file contains object related functions used by the CSV & RW route parser
	 */
	internal partial class CsvRwRouteParser
	{
		/// <summary>Creates a transformed copy of the provided prototype object (e.g. Platform top, roof etc.)</summary>
		/// <param name="Prototype">The prototype</param>
		/// /// <param name="NearDistance">The object's width at the start of the block</param>
		/// /// <param name="FarDistance">The object's width at the end of the block</param>
		/// <returns>The transformed copy</returns>
		private static StaticObject GetTransformedStaticObject(StaticObject Prototype, double NearDistance, double FarDistance)
		{
			StaticObject Result = (StaticObject)Prototype.Clone();
			int n = 0;
			double x2 = 0.0, x3 = 0.0, x6 = 0.0, x7 = 0.0;
			for (int i = 0; i < Result.Mesh.Vertices.Length; i++)
			{
				if (n == 2)
				{
					x2 = Result.Mesh.Vertices[i].Coordinates.X;
				}
				else if (n == 3)
				{
					x3 = Result.Mesh.Vertices[i].Coordinates.X;
				}
				else if (n == 6)
				{
					x6 = Result.Mesh.Vertices[i].Coordinates.X;
				}
				else if (n == 7)
				{
					x7 = Result.Mesh.Vertices[i].Coordinates.X;
				}
				n++;
				if (n == 8)
				{
					break;
				}
			}
			if (n >= 4)
			{
				int m = 0;
				for (int i = 0; i < Result.Mesh.Vertices.Length; i++)
				{
					if (m == 0)
					{
						Result.Mesh.Vertices[i].Coordinates.X = NearDistance - x3;
					}
					else if (m == 1)
					{
						Result.Mesh.Vertices[i].Coordinates.X = FarDistance - x2;
						if (n < 8)
						{
							break;
						}
					}
					else if (m == 4)
					{
						Result.Mesh.Vertices[i].Coordinates.X = NearDistance - x7;
					}
					else if (m == 5)
					{
						Result.Mesh.Vertices[i].Coordinates.X = NearDistance - x6;
						break;
					}
					m++;
					if (m == 8)
					{
						break;
					}
				}
			}
			return Result;
		}
	}
}
