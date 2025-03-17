using TrainManager.Car;

namespace TrainManager.Motor
{
	/// <summary>An abstract engine</summary>
    public abstract class AbstractEngine
    {
		/// <summary>Holds a reference to the base car</summary>
	    internal readonly CarBase BaseCar;
		/// <summary>The fuel supply</summary>
	    public FuelTank FuelTank;
	    /// <summary>Whether the engine is running</summary>
	    public bool IsRunning;

		/// <summary>Creates a new AbstractEngine</summary>
		protected AbstractEngine(CarBase car)
	    {
		    BaseCar = car;
	    }



		/// <summary>Called once a frame to update the engine</summary>
		/// <param name="timeElapsed"></param>
	    public abstract void Update(double timeElapsed);
    }
}
