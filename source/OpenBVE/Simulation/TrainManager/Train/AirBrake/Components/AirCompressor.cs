using OpenBveApi.Math;

namespace OpenBve.BrakeSystems
{
	public partial class AirBrake
	{
		/// <summary>Represents an air compressor</summary>
		internal class AirCompressor
		{
			/// <summary>Whether the compressor is enabled</summary>
			internal bool Enabled;
			/// <summary>The minimum pressure at which the compressor activates in Pa</summary>
			internal double MinimumPressure;
			/// <summary>The maximum pressure at which the compressor de-activates in Pa</summary>
			internal double MaximumPressure;
			/// <summary>The rate at which the compressor operates in Pa per second</summary>
			internal double Rate;
			/// <summary>The parent air-brake</summary>
			private readonly CarAirBrake AirBrake;
			/// <summary>Whether the loop has started</summary>
			private bool LoopStarted = false;
			/// <summary>The time the current loop started</summary>
			private double TimeStarted = 0.0;
			/// <summary>The start sound</summary>
			internal Sounds.SoundBuffer StartSound;
			/// <summary>The loop sound</summary>
			internal Sounds.SoundBuffer LoopSound;
			/// <summary>The end sound</summary>
			internal Sounds.SoundBuffer EndSound;
			/// <summary>The sound position</summary>
			internal Vector3 SoundPosition;

			/// <summary>Creates a new air compressor</summary>
			/// <param name="airBrake">The parent air-brake system</param>
			internal AirCompressor(CarAirBrake airBrake)
			{
				this.Enabled = false;
				this.MinimumPressure = 0.0;
				this.MaximumPressure = 0.0;
				this.Rate = 0.0;
				this.AirBrake = airBrake;
			}

			/// <summary>Updates the compressor</summary>
			/// <param name="Train">The train</param>
			/// <param name="CarIndex">The car the air brake compressor is situated in (Used for sounds)</param>
			/// <param name="TimeElapsed">The time elapsed since the last call to this function</param>
			internal void Update(TrainManager.Train Train, int CarIndex, double TimeElapsed)
			{
				//Check whether the air compressor is currently running
				if (Enabled)
				{
					//Check wwhether the main reservoir pressure is at max
					if (AirBrake.MainReservoir.CurrentPressure > MaximumPressure)
					{
						//Disable compressor and stop sound
						Enabled = false;
						LoopStarted = false;
						if (StartSound != null)
						{
							Sounds.PlaySound(StartSound, 1.0, 1.0, SoundPosition, Train, CarIndex, false);
						}
						if (LoopSound != null)
						{
							Sounds.StopSound(Train.Cars[CarIndex].Sounds.Compressor);
						}
					}
					else
					{
						//Increase main reservoir pressure
						AirBrake.MainReservoir.CurrentPressure += Rate * TimeElapsed;
						//After 5s from activation, play the compressor loop sound
						if (!LoopStarted && Game.SecondsSinceMidnight > TimeStarted + 5.0)
						{
							LoopStarted = true;
							if (LoopSound != null)
							{
								Train.Cars[CarIndex].Sounds.Compressor = Sounds.PlaySound(LoopSound, 1.0, 1.0, SoundPosition, Train, CarIndex, true);
							}
						}
					}
				}
				else
				{
					//Check whether the main reservoir pressure is under the minimum pressure for activation of the compressor
					if (AirBrake.MainReservoir.CurrentPressure < MinimumPressure)
					{
						//Activate compressor
						Enabled = true;
						//Set time that the compressor was started
						TimeStarted = Game.SecondsSinceMidnight;
						//Play compressor start sound
						if (StartSound != null)
						{
							Sounds.PlaySound(StartSound, 1.0, 1.0, SoundPosition, Train, CarIndex, false);
						}
					}
				}
			}
		}
	}
}
