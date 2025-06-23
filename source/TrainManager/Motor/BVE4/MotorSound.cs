using System;
using OpenBveApi.Hosts;
using SoundManager;
using TrainManager.Car;

namespace TrainManager.Motor
{
	public class BVEMotorSound : AbstractMotorSound
	{
		/// <summary>The motor sound tables</summary>
		public BVEMotorSoundTable[] Tables;
		/// <summary>The speed conversion factor</summary>
		public readonly double SpeedConversionFactor;
		/// <summary>The current direction of acceleration of the train</summary>
		private int CurrentAccelerationDirection;
		/*
		 * BVE2 / BVE4 magic numbers
		 */
		public const int MotorP1 = 0;
		public const int MotorP2 = 1;
		public const int MotorB1 = 2;
		public const int MotorB2 = 3;

		public bool PlayFirstTrack;
		public bool PlaySecondTrack;


		public BVEMotorSound(CarBase car, double speedConversionFactor) : base(car)
		{

			SpeedConversionFactor = speedConversionFactor;
			Tables = new BVEMotorSoundTable[4];
			for (int j = 0; j < 4; j++)
			{
				Tables[j].Entries = new BVEMotorSoundTableEntry[] { };
			}
		}

		public BVEMotorSound(CarBase car, double speedConversionFactor, BVEMotorSoundTable[] tables) : base(car)
		{
			SpeedConversionFactor = speedConversionFactor;
			Tables = new BVEMotorSoundTable[4];
			for (int j = 0; j < 4; j++)
			{
				Tables[j].Entries = new BVEMotorSoundTableEntry[tables[j].Entries.Length];
				for (int k = 0; k < tables[j].Entries.Length; k++)
				{
					Tables[j].Entries[k] = tables[j].Entries[k];
				}
			}
		}

		public override void Update(double timeElapsed)
		{
			if (!Car.TractionModel.ProvidesPower)
			{
				return;
			}
			double speed = Math.Abs(Car.Specs.PerceivedSpeed);
			int idx = (int) Math.Round(speed * SpeedConversionFactor);
			int odir = CurrentAccelerationDirection;
			int ndir = Math.Sign(Car.TractionModel.CurrentAcceleration);
			for (int h = 0; h < 2; h++)
			{
				int j = h == 0 ? BVEMotorSound.MotorP1 : BVEMotorSound.MotorP2;
				int k = h == 0 ? BVEMotorSound.MotorB1 : BVEMotorSound.MotorB2;
				if (odir > 0 & ndir <= 0)
				{
					if (j < Tables.Length)
					{
						TrainManagerBase.currentHost.StopSound(Tables[j].Source);
						Tables[j].Source = null;
						Tables[j].Buffer = null;
					}
				}
				else if (odir < 0 & ndir >= 0)
				{
					if (k < Tables.Length)
					{
						TrainManagerBase.currentHost.StopSound(Tables[k].Source);
						Tables[k].Source = null;
						Tables[k].Buffer = null;
					}
				}

				if (ndir != 0)
				{
					if (ndir < 0) j = k;
					if (j < Tables.Length)
					{
						int idx2 = idx;
						if (idx2 >= Tables[j].Entries.Length)
						{
							idx2 = Tables[j].Entries.Length - 1;
						}

						if (TrainManagerBase.currentHost.Application != HostApplication.OpenBve && ((!PlayFirstTrack && h == 0) || (!PlaySecondTrack && h == 1)))
						{
							// Used in TrainEditor2 to play a single track whilst editing
							idx2 = -1;
						}

						if (idx2 >= 0)
						{
							SoundBuffer obuf = Tables[j].Buffer;
							SoundBuffer nbuf = Tables[j].Entries[idx2].Buffer;
							double pitch = Tables[j].Entries[idx2].Pitch;
							double gain = Tables[j].Entries[idx2].Gain;
							if (ndir == 1)
							{
								// power
								if (Car.TractionModel.MaximumPossibleAcceleration != 0.0)
								{
									double cur = Car.TractionModel.CurrentAcceleration;
									if (cur < 0.0) cur = 0.0;
									gain *= Math.Pow(cur / Car.TractionModel.MaximumPossibleAcceleration, 0.25);
								}
							}
							else if (ndir == -1)
							{
								// brake
								double max = -Car.TractionModel.CurrentAcceleration;
								if (Car.baseTrain != null)
								{
									// train / brake system not simulated in TrainEditor2
									max = Car.CarBrake.DecelerationAtServiceMaximumPressure(Car.baseTrain.Handles.Brake.Actual, Car.CurrentSpeed);
								}
								if (max != 0.0)
								{
									double cur = -Car.TractionModel.CurrentAcceleration;
									if (cur < 0.0) cur = 0.0;
									gain *= Math.Pow(cur / max, 0.25);
								}
							}

							if (obuf != nbuf)
							{
								TrainManagerBase.currentHost.StopSound(Tables[j].Source);
								if (nbuf != null)
								{
									Tables[j].Source = (SoundSource) TrainManagerBase.currentHost.PlaySound(nbuf, pitch, gain, Position, Car, true);
									Tables[j].Buffer = nbuf;
								}
								else
								{
									Tables[j].Source = null;
									Tables[j].Buffer = null;
								}
							}
							else if (nbuf != null)
							{
								if (Tables[j].Source != null)
								{
									Tables[j].Source.Pitch = pitch;
									Tables[j].Source.Volume = gain;
								}
							}
							else
							{
								TrainManagerBase.currentHost.StopSound(Tables[j].Source);
								Tables[j].Source = null;
								Tables[j].Buffer = null;
							}
						}
						else
						{
							TrainManagerBase.currentHost.StopSound(Tables[j].Source);
							Tables[j].Source = null;
							Tables[j].Buffer = null;
						}
					}
				}
			}
			CurrentAccelerationDirection = ndir;
		}
	}
}
