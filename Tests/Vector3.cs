using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenBveApi.Math;

namespace OpenBve.Tests
{
	using System;

	[TestClass]
	public class Vector3Tests
	{
		/// <summary>Tests the equality operator</summary>
		[TestMethod]
		public void Equals()
		{
			Vector3 v1 = new Vector3(0, 0, 0);
			Vector3 v2 = new Vector3(0, 0, 0);
			Assert.AreEqual(v1, v2);
			Assert.IsTrue(v1 == v2);
			Assert.IsFalse(v1 != v2);

			v2 = new Vector3(3, 1, 5);
			Assert.AreNotEqual(v1, v2);
			Assert.IsFalse(v1 == v2);
			Assert.IsTrue(v1 != v2);
		}

		/// <summary>Tests the addition operators</summary>
		[TestMethod]
		public void Addition()
		{
			//Simple addition
			Vector3 v1 = new Vector3(4, 6, 2);
			Vector3 v2 = new Vector3(1, 5, 7);
			Vector3 v3 = v1 + v2;
			Assert.AreEqual(new Vector3(5, 11, 9), v3);
			v3 = v2 + v1;
			Assert.AreEqual(new Vector3(5, 11, 9), v3);
			
			//Vector and scalar
			v1 = new Vector3(7, 18, 4);
			v1 += 5;
			Assert.AreEqual(new Vector3(12, 23, 9), v1);

			//Scalar and vector
			v1 = new Vector3(5, 13, 9);
			v2 = 5 + v1;
			Assert.AreEqual(new Vector3(10, 18, 14), v2);

		}

		/// <summary>Tests the subtraction operators</summary>
		[TestMethod]
		public void Subtraction()
		{
			//Simple subtraction
			Vector3 v1 = new Vector3(4, 6, 2);
			Vector3 v2 = new Vector3(1, 5, 7);
			Vector3 v3 = v1 - v2;
			Assert.AreEqual(new Vector3(3, 1, -5), v3);
			v3 = v2 - v1;
			Assert.AreEqual(new Vector3(-3, -1, 5), v3);

			//Vector and scalar
			v1 = new Vector3(7, 18, 4);
			v1 -= 5;
			Assert.AreEqual(new Vector3(2, 13, -1), v1);

			//Scalar and vector
			v1 = new Vector3(5, 13, 9);
			v2 = 5 - v1;
			Assert.AreEqual(new Vector3(0, -8, -4), v2);
		}

		/// <summary>Tests the negation operator</summary>
		[TestMethod]
		public void Negation()
		{
			Vector3 v1 = new Vector3(8, 13, 27);
			v1 = -v1;
			Assert.AreEqual(new Vector3(-8, -13, -27), v1);

			v1 = new Vector3(-12, -2, -30);
			v1 = -v1;
			Assert.AreEqual(new Vector3(12, 2, 30), v1);

			v1 = new Vector3(-24, 27, -15);
			v1 = -v1;
			Assert.AreEqual(new Vector3(24, -27, 15), v1);
		}

		/// <summary>Tests the multiplication operator</summary>
		[TestMethod]
		public void Multiplication()
		{
			//Mutliplication
			Vector3 v1 = new Vector3(4, 6, 2);
			Vector3 v2 = new Vector3(1, 5, 7);
			Vector3 v3 = v1 * v2;
			Assert.AreEqual(new Vector3(4, 30, 14), v3);

			v1 = new Vector3(8, 12 , 9);
			v2 = v1 * 12.5;
			Assert.AreEqual(new Vector3(100, 150, 112.5), v2);

			v1 = new Vector3(14, 3.5, 8);
			v2 = 3.5 * v1;
			Assert.AreEqual(new Vector3(49, 12.25, 28), v2);
		}

		/// <summary>Tests the division operator</summary>
		[TestMethod]
		public void Division()
		{
			Vector3 v1 = new Vector3(0, 0, 0);
			Vector3 v2 = new Vector3(1, 5, 7);
			Vector3 v3 = v1 / v2;
			Assert.AreEqual(new Vector3(0, 0, 0), v3);

			v1 = new Vector3(1, 6, 4);
			v2 = new Vector3(2, 3, 5);
			v3 = v1 / v2;
			Assert.AreEqual(new Vector3(0.5, 2, 0.8), v3);

			v1 = new Vector3(15.3, 9, 20.5);
			v2 = v1 / 10;
			Assert.AreEqual(new Vector3(1.53, 0.9, 2.05), v2);
		}

		/// <summary>Tests the division by zero vector exception</summary>
		[TestMethod]
		[ExpectedException(typeof(DivideByZeroException))]
		public void DivisionByZeroVector()
		{
			Vector3 v1 = new Vector3(1, 5, 7);
			Vector3 v2 = new Vector3(0, 0, 0);
			Vector3 v3 = v1 / v2;
		}
		
		/// <summary>Tests the division by zero exception</summary>
		[TestMethod]
		[ExpectedException(typeof(DivideByZeroException))]
		public void DivisionByZero()
		{
			Vector3 v1 = new Vector3(1, 5, 7);
			Vector3 v3 = v1 / 0;
		}
	}
}
