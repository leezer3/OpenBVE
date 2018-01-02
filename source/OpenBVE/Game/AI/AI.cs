using System;

namespace OpenBve
{
    internal static partial class Game
    {
        /// <summary>An abstract class representing a general purpose AI</summary>
        internal abstract class GeneralAI
        {
            internal abstract void Trigger(TrainManager.Train Train, double TimeElapsed);
        }

	    internal static bool InitialAIDriver;

       

        // bogus pretrain
        
    }
}
