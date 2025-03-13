namespace OpenBveApi.Objects
{
    public class PivotPoint
    {
		/// <summary>The name of the matrix to pivot</summary>
	    public readonly string MatrixName;
		/// <summary>The front point</summary>
	    public readonly double FrontPoint;
		/// <summary>The rear point</summary>
	    public readonly double RearPoint;
		/// <summary>The relative position within the object</summary>
	    public readonly double Position;

	    /// <summary>Creates a new pivot point</summary>
		public PivotPoint(string matrixName, double position, double frontPoint, double rearPoint)
        {
            MatrixName = matrixName;
            Position = position;
            FrontPoint = frontPoint;
            RearPoint = rearPoint;
        }
    }
}
