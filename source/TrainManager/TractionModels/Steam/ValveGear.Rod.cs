namespace TrainManager.TractionModels.Steam
{
	internal class ValveGearRod
	{
		/// <summary>The radius of this rod's rotation</summary>
		internal readonly double Radius;
		/// <summary>The length of this rod</summary>
		internal readonly double Length;
		/// <summary>The current angle of this rod</summary>
		internal double Angle;
		/// <summary>The current position offset of this rod</summary>
		internal double Position;

		internal ValveGearRod(double radius, double length)
		{
			Radius = radius;
			Length = length;
		}
	}
}
