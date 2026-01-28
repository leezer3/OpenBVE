namespace OpenBveApi.Math
{
    /// <summary>Provides extension methods for some System.Math functions</summary>
    public static class Extensions
    {
	    /// <summary>Returns the natural (base e) logarithm of a number</summary>
	    /// <remarks>This function will return 0 if the result would be Complex or NonReal</remarks>
	    public static double LogC(double X)
	    {
		    if (X <= 0.0)
		    {
			    return 0.0;
		    }
		    return System.Math.Log(X);
	    }

	    /// <summary>Returns the square root of a number</summary>
	    /// <remarks>This function will return 0 if the result would be NonReal</remarks>
	    public static double SqrtC(double X)
	    {
		    if (X < 0.0)
		    {
			    return 0.0;
		    }
		    return System.Math.Sqrt(X);
	    }

	    /// <summary>Returns the tangent of the specified angle</summary>
	    /// <remarks>This function will return 0 if the result would be ComplexInfinity</remarks>
	    public static double TanC(double X)
	    {
		    double c = X / System.Math.PI;
		    double d = c - System.Math.Floor(c) - 0.5;
		    double e = System.Math.Floor(X >= 0.0 ? X : -X) * 1.38462643383279E-16;
		    if (d >= -e & d <= e)
		    {
			    return 0.0;
		    }
		    return System.Math.Tan(X);
	    }

		/// <summary>Linearly interpolates a value in a range, using keys</summary>
		/// <param name="x0">The first number</param>
		/// <param name="y0">The first key</param>
		/// <param name="x1">The second number</param>
		/// <param name="y1">The second key</param>
		/// <param name="x">The interpolation value between keys</param>
		/// <returns>The interpolated number</returns>
	    public static double LinearInterpolation(double x0, double y0, double x1, double y1, double x)
	    {
		    // ReSharper disable once CompareOfFloatsByEqualityOperator
		    return x0 == x1 ? y0 : y0 + (y1 - y0) * (x - x0) / (x1 - x0);
	    }
	}
}
