using OpenBveApi.Math;

namespace OpenBve
{
    public static partial class TrainManager
    {
        /// <summary>A car section contains an object to be rendered attached to the car (e.g. Exterior, panel etc.)</summary>
        internal class CarSection
        {
            /// <summary>The object</summary>
            internal ObjectManager.AnimatedObject[] Elements;
            /// <summary>Whether this is to be rendered as an overlay object (Train panel / exterior)</summary>
            internal bool Overlay;

            internal void Initialize(bool CurrentlyVisible)
            {
                for (int j = 0; j < Elements.Length; j++)
                {
                    for (int k = 0; k < Elements[j].States.Length; k++)
                    {
                        ObjectManager.InitializeAnimatedObject(ref Elements[j], k, Overlay, CurrentlyVisible);
                    }
                }
            }
        }
    }
}
