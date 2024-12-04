using OpenBveApi.Math;

namespace OpenBveApi.Objects
{
	/// <summary>An abstract animation</summary>
    public abstract class AbstractAnimation
    {
		/// <summary>The name of the animation</summary>
	    public readonly string Name;

		/// <summary>Creates an abstract animation</summary>
		protected AbstractAnimation(string name)
        {
			Name = name;
        }

		/// <summary>Updates the animation</summary>
        public virtual void Update(double animationKey, double timeElapsed, ref Matrix4D matrix)
        {

        }

	}
}
