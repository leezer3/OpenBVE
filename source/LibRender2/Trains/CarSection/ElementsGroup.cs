using OpenBveApi.Hosts;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Trains;

namespace LibRender2.Trains
{
	public abstract class ElementsGroup
	{
		/// <summary>Holds a reference to the current host</summary>
		internal readonly HostInterface currentHost;

		/// <summary>The time since the last update of this object</summary>
		public double SecondsSinceLastUpdate;

		internal ElementsGroup(HostInterface host)
		{
			currentHost = host;
		}

		/// <summary>Initializes the ElementsGroup</summary>
		/// <param name="CurrentlyVisible">Whether visible at the time of the call</param>
		/// <param name="Type">The object type</param>
		public abstract void Initialize(bool CurrentlyVisible, ObjectType Type);

		/// <summary>Shows all objects associated with the ElementsGroup</summary>
		public abstract void Show(ObjectType Type);

		/// <summary>Hides all objects associated with the ElementsGroup</summary>
		public abstract void Hide();

		/// <summary>Reverses the contents of the ElementsGroup</summary>
		public abstract void Reverse();

		/// <summary> Updates the position and state of the ElementsGroup</summary>
		/// <param name="Train">The train, or a null reference otherwise</param>
		/// <param name="CarIndex">If this object forms part of a train, the car index it refers to</param>
		/// <param name="TrackPosition"></param>
		/// <param name="Brightness"></param>
		/// <param name="Position"></param>
		/// <param name="Direction"></param>
		/// <param name="Up"></param>
		/// <param name="Side"></param>
		/// <param name="UpdateFunctions">Whether the functions associated with this object should be re-evaluated</param>
		/// <param name="Show"></param>
		/// <param name="TimeElapsed">The time elapsed since this object was last updated</param>
		/// <param name="EnableDamping">Whether damping is to be applied for this call</param>
		/// <param name="IsTouch">Whether Animated Object belonging to TouchElement class.</param>
		/// <param name="Camera"></param>
		public abstract void Update(AbstractTrain Train, int CarIndex, double TrackPosition, byte Brightness, Vector3 Position, Vector3 Direction, Vector3 Up, Vector3 Side, bool UpdateFunctions, bool Show, double TimeElapsed, bool EnableDamping, bool IsTouch = false, dynamic Camera = null);
	}
}
