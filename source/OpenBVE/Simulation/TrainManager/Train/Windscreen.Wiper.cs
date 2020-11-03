using System;
using SoundManager;

namespace OpenBve
{
	internal class WindscreenWiper
	{
		/// <summary>The car sound played when 20% or less drops are visible</summary>
		internal CarSound DryWipeSound;
		/// <summary>The car sound played when 20% or more drops are visible</summary>
		internal CarSound WetWipeSound;
		/// <summary>The car sound played when the wipers are activated or deactivated</summary>
		internal CarSound WiperSwitchSound;
		/// <summary>The time for which the wiper pauses at the hold position</summary>
		internal double WiperHoldTime = 0;
		/// <summary>The wiper rest position</summary>
		internal WiperPosition RestPosition;
		/// <summary>The wiper hold position</summary>
		internal WiperPosition HoldPosition;
		/// <summary>The current wiper position</summary>
		/// <remarks>Range of 0 to 100</remarks>
		internal int CurrentPosition;
		/// <summary>The time taken to move 1 position unit in milliseconds</summary>
		internal double MovementSpeed = 10;
		/// <summary>The current speed</summary>
		internal WiperSpeed CurrentSpeed;

		private readonly Windscreen Windscreen;
		private WiperPosition currentDirection;
		private double wiperTimer;
		private double holdTimer;
		private bool soundTriggered;

		internal WindscreenWiper(Windscreen windscreen, WiperPosition restPosition, WiperPosition holdPosition)
		{
			RestPosition = restPosition;
			HoldPosition = holdPosition;
			Windscreen = windscreen;
		}

		internal void Update(double TimeElapsed)
		{
			wiperTimer += TimeElapsed;
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
						if (HoldPosition == WiperPosition.Left && CurrentSpeed != WiperSpeed.Fast)
						{
							holdTimer += TimeElapsed;
							if (holdTimer > WiperHoldTime)
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
						if (RestPosition == WiperPosition.Right)
						{
							if (HoldPosition == WiperPosition.Left)
							{
								holdTimer += TimeElapsed;
								if (holdTimer > WiperHoldTime)
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
						if (HoldPosition == WiperPosition.Right && CurrentSpeed != WiperSpeed.Fast)
						{
							holdTimer += TimeElapsed;
							if (holdTimer > WiperHoldTime)
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
						if (RestPosition == WiperPosition.Left)
						{
							if (HoldPosition == WiperPosition.Right)
							{
								holdTimer += TimeElapsed;
								if (holdTimer > WiperHoldTime)
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
								Program.Sounds.PlayCarSound(WetWipeSound, 1.0, 1.0, Windscreen.Car, false);
								soundTriggered = true;
							}
						}
						else
						{
							if (soundTriggered == false)
							{
								Program.Sounds.PlayCarSound(DryWipeSound, 1.0, 1.0, Windscreen.Car, false);
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
								Program.Sounds.PlayCarSound(WetWipeSound, 1.0, 1.0, Windscreen.Car, false);
								soundTriggered = true;
							}
						}
						else
						{
							if (soundTriggered == false)
							{
								Program.Sounds.PlayCarSound(DryWipeSound, 1.0, 1.0, Windscreen.Car, false);
								soundTriggered = true;
							}
						}
					}
					break;
			}
			int dropToRemove = Math.Min(Windscreen.RainDrops.Length - 1, (int) (CurrentPosition / (100.0 / Windscreen.RainDrops.Length)));
			if (Windscreen.RainDrops[dropToRemove])
			{
				Windscreen.RainDrops[dropToRemove] = false;
				Windscreen.currentDrops--;
			}
		}
	}
}
