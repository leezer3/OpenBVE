using System;

namespace OpenBve
{
    internal static partial class MainLoop
    {
        /// <summary>Defines a repeating control</summary>
        private struct ControlRepeat
        {
            /// <summary>The control index to repeat</summary>
            internal readonly int ControlIndex;
            /// <summary>The countdown time remaining in ms until the next repeat event is sent</summary>
            internal double Countdown;
            internal ControlRepeat(int controlIndex, double countdown)
            {
                this.ControlIndex = controlIndex;
                this.Countdown = countdown;
            }
        }

        /// <summary>Stores the currently repeating controls</summary>
        private static ControlRepeat[] RepeatControls = new ControlRepeat[16];
        /// <summary>The nuber of repeating controls in use (Maximum 16)</summary>
        private static int RepeatControlsUsed = 0;

        /// <summary>Adds a control to be repeated</summary>
        /// <param name="controlIndex">The control index to repeat</param>
        internal static void AddControlRepeat(int controlIndex)
        {
            for(int i = 0; i < RepeatControlsUsed; i++)
            {
                if(RepeatControls[i].ControlIndex == controlIndex)
                {
					// this control is in repeat array already
                    return;
                }
            }
            if (RepeatControls.Length == RepeatControlsUsed)
            {
                Array.Resize<ControlRepeat>(ref RepeatControls, RepeatControls.Length << 1);
            }
            RepeatControls[RepeatControlsUsed] = new ControlRepeat(controlIndex, Interface.CurrentOptions.KeyRepeatDelay);
            RepeatControlsUsed++;
        }

        /// <summary>Removes a control repeat</summary>
        /// <param name="controlIndex">The control index to stop repeating</param>
        internal static void RemoveControlRepeat(int controlIndex)
        {
            for (int i = 0; i < RepeatControlsUsed; i++)
            {
                if (RepeatControls[i].ControlIndex == controlIndex)
                {
                    RepeatControls[i] = RepeatControls[RepeatControlsUsed - 1];
                    RepeatControlsUsed--;
                    break;
                }
            }
        }

        /// <summary>Should be called once a frame, to update the status of all repeating controls</summary>
        /// <param name="timeElapsed">The time elapsed in milliseconds since the last call to this function</param>
        internal static void UpdateControlRepeats(double timeElapsed)
        {
            for (int i = 0; i < RepeatControlsUsed; i++)
            {
                RepeatControls[i].Countdown -= timeElapsed;
                if (RepeatControls[i].Countdown <= 0.0)
                {
                    int j = RepeatControls[i].ControlIndex;
                    Interface.CurrentControls[j].AnalogState = 1.0;
                    Interface.CurrentControls[j].DigitalState = Interface.DigitalControlState.Pressed;
                    RepeatControls[i].Countdown += Interface.CurrentOptions.KeyRepeatInterval;
                }
            }
        }
    }
}
