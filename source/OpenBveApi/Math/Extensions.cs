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

	    /// <summary>Returns the tamgent of the specified angle</summary>
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
	}
}
