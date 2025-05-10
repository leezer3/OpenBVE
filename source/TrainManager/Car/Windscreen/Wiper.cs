using System;
using OpenBveApi.Interface;
using SoundManager;

namespace TrainManager.Car
{
	public class WindscreenWiper
	{
		/// <summary>The car sound played when 20% or less drops are visible</summary>
		public CarSound DryWipeSound = new CarSound();
		/// <summary>The car sound played when 20% or more drops are visible</summary>
		public CarSound WetWipeSound = new CarSound();
		/// <summary>The car sound played when the wipers are activated or deactivated</summary>
		public CarSound SwitchSound = new CarSound();
		/// <summary>The time for which the wiper pauses at the hold position</summary>
		internal double HoldTime;
		/// <summary>The wiper rest position</summary>
		internal WiperPosition RestPosition;
		/// <summary>The wiper hold position</summary>
		internal WiperPosition HoldPosition;
		/// <summary>The current wiper position</summary>
		/// <remarks>Range of 0 to 100</remarks>
		public int CurrentPosition;
		/// <summary>The time taken to move 1 position unit in seconds</summary>
		internal double MovementSpeed;
		/// <summary>The current speed</summary>
		public WiperSpeed CurrentSpeed;

		private readonly Windscreen Windscreen;
		private WiperPosition currentDirection;
		private double wiperTimer;
		private double holdTimer;
		private bool soundTriggered;
		private readonly WiperSpeed maxSpeed;

		public WindscreenWiper(Windscreen windscreen, WiperPosition restPosition, WiperPosition holdPosition, double wipeSpeed, double holdTime, bool singleSpeed = false)
		{
			RestPosition = restPosition;
			HoldPosition = holdPosition;
			MovementSpeed = wipeSpeed / 100.0;
			HoldTime = holdTime;
			Windscreen = windscreen;
			CurrentPosition = restPosition == WiperPosition.Left ? 100 : 0;
			CurrentSpeed = WiperSpeed.Off;
			maxSpeed = singleSpeed ? WiperSpeed.Intermittant : WiperSpeed.Fast;
		}

		/// <summary>Changes the wiper speed</summary>
		public void ChangeSpeed(Translations.Command Command)
		{
			SwitchSound.Play(Windscreen.Car, false);
			
			switch (Command)
			{
				case Translations.Command.WiperSpeedUp:
					if (CurrentSpeed < maxSpeed)
					{
						CurrentSpeed++;
					}
					break;
				case Translations.Command.WiperSpeedDown:
					if (CurrentSpeed > WiperSpeed.Off)
					{
						CurrentSpeed--;
					}
					break;
			}
			
		}

		/// <summary>Updates the windscreen wipers</summary>
		/// <param name="TimeElapsed">The time elapsed since the last call to this method</param>
		internal void Update(double TimeElapsed)
		{
			wiperTimer += TimeElapsed;
			if (CurrentSpeed == WiperSpeed.Off)
			{
				if (RestPosition == WiperPosition.Left && CurrentPosition == 100)
				{
					wiperTimer = 0;
					return;
				}
				if(RestPosition == WiperPosition.Right && CurrentPosition == 0)
				{
					wiperTimer = 0;
					return;
				}
			}
			//Move the wiper
			if (wiperTimer > MovementSpeed)
			{
				wiperTimer = 0;
				switch (currentDirection)
				{
					case WiperPosition.Left:
						if (CurrentPosition > 0)
						{
							CurrentPosition--;
						}
						break;
					case WiperPosition.Right:
						if (CurrentPosition < 100)
						{
							CurrentPosition++;
						}
						break;
				}
				soundTriggered = false;
			}
			switch (CurrentPosition)
			{
				//When the wiper is at either end of the travel, determine what to do next
				case 0:
					if (CurrentSpeed > 0)
					{
						if (HoldPosition == WiperPosition.Right && CurrentSpeed != WiperSpeed.Fast)
						{
							holdTimer += TimeElapsed;
							if (holdTimer > HoldTime)
							{
								holdTimer = 0;
								currentDirection = WiperPosition.Right;
							}
						}
						else
						{
							currentDirection = WiperPosition.Right;
						}
					}
					else
					{
						if (RestPosition == WiperPosition.Left)
						{
							if (HoldPosition == WiperPosition.Right)
							{
								holdTimer += TimeElapsed;
								if (holdTimer > HoldTime)
								{
									holdTimer = 0;
									currentDirection = WiperPosition.Right;
								}
							}
							else
							{
								currentDirection = WiperPosition.Right;
							}
						}
					}
					break;
				case 100:
					if (CurrentSpeed > 0)
					{
						if (HoldPosition == WiperPosition.Left && CurrentSpeed != WiperSpeed.Fast)
						{
							holdTimer += TimeElapsed;
							if (holdTimer > HoldTime)
							{
								holdTimer = 0;
								currentDirection = WiperPosition.Left;
							}
						}
						else
						{
							currentDirection = WiperPosition.Left;
						}
					}
					else
					{
						if (RestPosition == WiperPosition.Right)
						{
							if (HoldPosition == WiperPosition.Left)
							{
								holdTimer += TimeElapsed;
								if (holdTimer > HoldTime)
								{
									holdTimer = 0;
									currentDirection = WiperPosition.Left;
								}
							}
							else
							{
								currentDirection = WiperPosition.Left;
							}
						}
					}
					break;
				//Wiper has started moving, determine which sound effect to play
				case 1:
					if (currentDirection == WiperPosition.Right)
					{
						if(Windscreen.currentDrops / (double)Windscreen.RainDrops.Length > 0.8)
						{
							if (soundTriggered == false)
							{
								WetWipeSound.Play(Windscreen.Car, false);
								soundTriggered = true;
							}
						}
						else
						{
							if (soundTriggered == false)
							{
								DryWipeSound.Play(Windscreen.Car, false);
								soundTriggered = true;
							}
						}
					}
					break;
				case 99:
					if (currentDirection == WiperPosition.Left)
					{
						if(Windscreen.currentDrops / (double)Windscreen.RainDrops.Length > 0.8)
						{
							if (soundTriggered == false)
							{
								WetWipeSound.Play(Windscreen.Car, false);
								soundTriggered = true;
							}
						}
						else
						{
							if (soundTriggered == false)
							{
								DryWipeSound.Play(Windscreen.Car, false);
								soundTriggered = true;
							}
						}
					}
					break;
			}
			int dropToRemove = Windscreen.RainDrops.Length - 1 - Math.Min(Windscreen.RainDrops.Length - 1, (int) (CurrentPosition / (100.0 / Windscreen.RainDrops.Length)));
			if (Windscreen.RainDrops[dropToRemove].Visible)
			{
				Windscreen.RainDrops[dropToRemove].Visible = false;
				Windscreen.RainDrops[dropToRemove].IsSnowFlake = false;
				Windscreen.RainDrops[dropToRemove].RemainingLife = 0.5 * TrainManagerBase.RandomNumberGenerator.NextDouble() * Windscreen.DropLife;
				Windscreen.currentDrops--;
			}
		}
	}
}
