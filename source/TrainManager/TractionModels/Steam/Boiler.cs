//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2022, Christopher Lees, The OpenBVE Project
//
//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met:
//
//1. Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
//ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using SoundManager;

namespace TrainManager.TractionModels.Steam
{
	public class Boiler
	{
		/// <summary>Holds a reference to the base engine</summary>
		private readonly SteamEngine Engine;
		/// <summary>The boiler water level</summary>
		public double WaterLevel;
		/// <summary>The maximum water level</summary>
		public readonly double MaxWaterLevel;

		/// <summary>The steam pressure level</summary>
		public double SteamPressure
		{
			get => _SteamPressure;
			set => _SteamPressure = Math.Max(0, value); // ensure we can't go below zero pressure
		}
		/// <summary>Backing property for steam pressure</summary>
		private double _SteamPressure;
		/// <summary>The maximum steam pressure</summary>
		public readonly double MaxSteamPressure;
		/// <summary>The minimum working steam pressure</summary>
		public readonly double MinWorkingSteamPressure;
		/// <summary>The base water to steam conversion rate</summary>
		private readonly double BaseSteamGenerationRate;
		/// <summary>The live steam injector</summary>
		public LiveSteamInjector LiveSteamInjector;
		/// <summary>The exhaust steam injector</summary>
		public ExhaustSteamInjector ExhaustSteamInjector;
		/// <summary>The firebox</summary>
		public Firebox Firebox;
		/// <summary>The blowers</summary>
		public readonly Blowers Blowers;
		/// <summary>The blowoff pressure</summary>
		public double BlowoffPressure;
		/// <summary>The pressure at which the blowoff resets</summary>
		public double BlowoffEndPressure;
		/// <summary>The rate of steam loss via the blowoff</summary>
		public double BlowoffRate;
		/// <summary>The blowoff start sound</summary>
		public CarSound BlowoffStartSound;
		/// <summary>The blowoff loop sound</summary>
		public CarSound BlowoffLoopSound;
		/// <summary>The blowoff end sound</summary>
		public CarSound BlowoffEndSound;
		/// <summary>The rate at water is converted to steam</summary>
		/// <remarks>Units per millisecond</remarks>
		public double SteamGenerationRate => BaseSteamGenerationRate * Firebox.ConversionRate;
		

		private bool startSoundPlayed;
		private bool blowoff;

		public Boiler(SteamEngine engine, double waterLevel, double maxWaterLevel, double steamPressure, double maxSteamPressure, double blowoffPressure, double minWorkingSteamPressure, double baseSteamGenerationRate)
		{
			Engine = engine;
			WaterLevel = waterLevel;
			MaxWaterLevel = maxWaterLevel;
			SteamPressure = steamPressure;
			MaxSteamPressure = maxSteamPressure;
			BlowoffPressure = blowoffPressure;
			BlowoffEndPressure = blowoffPressure * 0.8;
			MinWorkingSteamPressure = minWorkingSteamPressure;
			BaseSteamGenerationRate = baseSteamGenerationRate;
			LiveSteamInjector = new LiveSteamInjector(engine, 3.0);
			ExhaustSteamInjector = new ExhaustSteamInjector(engine, 3.0);
			Firebox = new Firebox(engine, 7, 10, 1000, 0.1, 3);
			Blowers = new Blowers(engine, 2, 1);
			BlowoffRate = 10.0;
		}

		internal void Update(double timeElapsed)
		{
			// injectors update first
			LiveSteamInjector.Update(timeElapsed);
			ExhaustSteamInjector.Update(timeElapsed);
			// now firebox
			Firebox.Update(timeElapsed);
			// convert water to steam pressure
			WaterLevel -= SteamGenerationRate;
			SteamPressure += SteamGenerationRate;
			// handle blowoff
			if (SteamPressure > BlowoffPressure)
			{
				blowoff = true;
			}
			if(blowoff)
			{
				SteamPressure -= BlowoffRate;
				if (!startSoundPlayed)
				{
					if (BlowoffStartSound != null)
					{
						BlowoffStartSound.Play(Engine.Car, false);
					}
					startSoundPlayed = true;
				}
				else if (BlowoffLoopSound != null && !BlowoffStartSound.IsPlaying)
				{
					if (BlowoffLoopSound != null)
					{
						BlowoffLoopSound.Play(Engine.Car, true);
					}
				}

				if (SteamPressure < BlowoffEndPressure)
				{
					blowoff = false;
				}
			}
			else
			{
				if (startSoundPlayed)
				{
					if (BlowoffLoopSound != null && BlowoffLoopSound.IsPlaying)
					{
						BlowoffLoopSound.Stop();
					}
					if (BlowoffEndSound != null)
					{
						BlowoffEndSound.Play(Engine.Car, false);
					}
					startSoundPlayed = false;
				}
			}
			Blowers.Update(timeElapsed);
		}
	}
}
