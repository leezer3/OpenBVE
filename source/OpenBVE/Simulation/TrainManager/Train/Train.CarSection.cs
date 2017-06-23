namespace OpenBve
{
    public static partial class TrainManager
    {
        /// <summary>A car section contains an object to be rendered attached to the car (e.g. Exterior, panel etc.)</summary>
        internal struct CarSection
        {
            /// <summary>The object</summary>
            internal ObjectManager.AnimatedObject[] Elements;
            /// <summary>Whether this is to be rendered as an overlay object (Train panel / exterior)</summary>
            internal bool Overlay;
        }
    }
}
