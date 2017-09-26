namespace OpenBve
{
	/*
	 * This file contains object related functions used by the CSV & RW route parser
	 */
	internal partial class CsvRwRouteParser
	{
		/// <summary>Creates a mirrored copy of the prototype object (Animated objects)</summary>
		/// <param name="Prototype">The prototype</param>
		/// <returns>The mirrored copy</returns>
		private static ObjectManager.UnifiedObject GetMirroredObject(ObjectManager.UnifiedObject Prototype)
		{
			if (Prototype is ObjectManager.StaticObject)
			{
				ObjectManager.StaticObject s = (ObjectManager.StaticObject)Prototype;
				return GetMirroredStaticObject(s);
			}
			if (Prototype is ObjectManager.AnimatedObjectCollection)
			{
				ObjectManager.AnimatedObjectCollection a = (ObjectManager.AnimatedObjectCollection)Prototype;
				ObjectManager.AnimatedObjectCollection Result = new ObjectManager.AnimatedObjectCollection
				{
					Objects = new ObjectManager.AnimatedObject[a.Objects.Length]
				};
				for (int i = 0; i < a.Objects.Length; i++)
				{
					Result.Objects[i] = a.Objects[i].Clone();
					for (int j = 0; j < a.Objects[i].States.Length; j++)
					{
						Result.Objects[i].States[j].Object = GetMirroredStaticObject(a.Objects[i].States[j].Object);
					}
					Result.Objects[i].TranslateXDirection.X *= -1.0;
					Result.Objects[i].TranslateYDirection.X *= -1.0;
					Result.Objects[i].TranslateZDirection.X *= -1.0;
					Result.Objects[i].RotateXDirection.X *= -1.0;
					Result.Objects[i].RotateYDirection.X *= -1.0;
					Result.Objects[i].RotateZDirection.X *= -1.0;
				}
				return Result;
			}
			return null;
		}

		/// <summary>Creates a mirrored copy of the prototype object</summary>
		/// <param name="Prototype">The prototype</param>
		/// <returns>The mirrored copy</returns>
		private static ObjectManager.StaticObject GetMirroredStaticObject(ObjectManager.StaticObject Prototype)
		{
			ObjectManager.StaticObject Result = Prototype.Clone();
			for (int i = 0; i < Result.Mesh.Vertices.Length; i++)
			{
				Result.Mesh.Vertices[i].Coordinates.X = -Result.Mesh.Vertices[i].Coordinates.X;
			}
			for (int i = 0; i < Result.Mesh.Faces.Length; i++)
			{
				for (int k = 0; k < Result.Mesh.Faces[i].Vertices.Length; k++)
				{
					Result.Mesh.Faces[i].Vertices[k].Normal.X = -Result.Mesh.Faces[i].Vertices[k].Normal.X;
				}
				Result.Mesh.Faces[i].Flip();
			}
			return Result;
		}

		/// <summary>Creates a transformed copy of the provided prototype object (e.g. Platform top, roof etc.)</summary>
		/// <param name="Prototype">The prototype</param>
		/// /// <param name="NearDistance">The object's width at the start of the block</param>
		/// /// <param name="FarDistance">The object's width at the end of the block</param>
		/// <returns>The transformed copy</returns>
		private static ObjectManager.StaticObject GetTransformedStaticObject(ObjectManager.StaticObject Prototype, double NearDistance, double FarDistance)
		{
			ObjectManager.StaticObject Result = Prototype.Clone();
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
							m = 8;
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
						m = 8;
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
