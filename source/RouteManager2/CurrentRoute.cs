using System.Linq;
using LibRender2;
using OpenBveApi.Colors;
using OpenBveApi.Routes;
using OpenBveApi.Runtime;
using OpenBveApi.Trains;
using RouteManager2.Climate;
using RouteManager2.SignalManager;
using RouteManager2.SignalManager.PreTrain;
using RouteManager2.Stations;

namespace RouteManager2
{
	public class CurrentRoute
	{
		private readonly BaseRenderer renderer;

		/// <summary>The list of tracks available in the simulation.</summary>
		public Track[] Tracks;

		/// <summary>Holds a reference to the base TrainManager.Trains array</summary>
		public AbstractTrain[] Trains;

		/// <summary>Holds all signal sections within the current route</summary>
		public Section[] Sections;

		/// <summary>Holds all stations within the current route</summary>
		public RouteStation[] Stations;

		/// <summary>Holds all .PreTrain instructions for the current route</summary>
		/// <remarks>Must be in distance and time ascending order</remarks>
		public BogusPreTrainInstruction[] BogusPreTrainInstructions;

		/// <summary>Holds all points of interest within the game world</summary>
		public PointOfInterest[] PointsOfInterest;

		/// <summary>The currently displayed background texture</summary>
		public BackgroundHandle CurrentBackground;

		/// <summary>The new background texture (Currently fading in)</summary>
		public BackgroundHandle TargetBackground;

		/// <summary>The start of a region without fog</summary>
		/// <remarks>Must not be below the viewing distance (e.g. 600m)</remarks>
		public float NoFogStart;

		/// <summary>The end of a region without fog</summary>
		public float NoFogEnd;

		/// <summary>Holds the previous fog</summary>
		public Fog PreviousFog;

		/// <summary>Holds the current fog</summary>
		public Fog CurrentFog;

		/// <summary>Holds the next fog</summary>
		public Fog NextFog;

		public Atmosphere Atmosphere;

		/// <summary>The current in game time, expressed as the number of seconds since midnight on the first day</summary>
		public double SecondsSinceMidnight;

		public CurrentRoute(BaseRenderer renderer)
		{
			this.renderer = renderer;

			Tracks = new[] { new Track() };
			Trains = new AbstractTrain[0];
			Sections = new Section[0];
			Stations = new RouteStation[0];
			BogusPreTrainInstructions = new BogusPreTrainInstruction[0];
			PointsOfInterest = new PointOfInterest[0];
			CurrentBackground = new StaticBackground(null, 6, false);
			TargetBackground = new StaticBackground(null, 6, false);
			NoFogStart = 800.0f;
			NoFogEnd = 1600.0f;
			PreviousFog = new Fog(NoFogStart, NoFogEnd, Color24.Grey, 0.0);
			CurrentFog = new Fog(NoFogStart, NoFogEnd, Color24.Grey, 0.5);
			NextFog = new Fog(NoFogStart, NoFogEnd, Color24.Grey, 1.0);
			Atmosphere = new Atmosphere();
			SecondsSinceMidnight = 0.0;
		}

		public void UpdateAllSections()
		{
			UpdateSection(Sections.LastOrDefault());
		}

		/// <summary>Updates the specified signal section</summary>
		/// <param name="SectionIndex"></param>
		public void UpdateSection(int SectionIndex)
		{
			UpdateSection(Sections[SectionIndex]);
		}

		/// <summary>Updates the specified signal section</summary>
		/// <param name="Section"></param>
		public void UpdateSection(Section Section)
		{
			if (Section == null)
			{
				return;
			}

			// preparations
			int zeroAspect;
			bool setToRed = false;

			if (Section.Type == SectionType.ValueBased)
			{
				// value-based
				zeroAspect = 0;

				for (int i = 1; i < Section.Aspects.Length; i++)
				{
					if (Section.Aspects[i].Number < Section.Aspects[zeroAspect].Number)
					{
						zeroAspect = i;
					}
				}
			}
			else
			{
				// index-based
				zeroAspect = 0;
			}

			// hold station departure signal at red
			int d = Section.StationIndex;

			if (d >= 0)
			{
				// look for train in previous blocks
				Section l = Section.PreviousSection;
				AbstractTrain train = null;

				while (true)
				{
					if (l != null)
					{
						train = l.GetFirstTrain(false);

						if (train != null)
						{
							break;
						}

						l = l.PreviousSection;
					}
					else
					{
						break;
					}
				}

				if (train == null)
				{
					double b = -double.MaxValue;

					foreach (AbstractTrain t in Trains)
					{
						if (t.State == TrainState.Available)
						{
							if (t.TimetableDelta > b)
							{
								b = t.TimetableDelta;
								train = t;
							}
						}
					}
				}

				// set to red where applicable
				if (train != null)
				{
					if (!Section.TrainReachedStopPoint)
					{
						if (train.Station == d)
						{
							int c = Stations[d].GetStopIndex(train.NumberOfCars);

							if (c >= 0)
							{
								double p0 = train.FrontCarTrackPosition();
								double p1 = Stations[d].Stops[c].TrackPosition - Stations[d].Stops[c].BackwardTolerance;

								if (p0 >= p1)
								{
									Section.TrainReachedStopPoint = true;
								}
							}
							else
							{
								Section.TrainReachedStopPoint = true;
							}
						}
					}

					double t = -15.0;

					if (Stations[d].DepartureTime >= 0.0)
					{
						t = Stations[d].DepartureTime - 15.0;
					}
					else if (Stations[d].ArrivalTime >= 0.0)
					{
						t = Stations[d].ArrivalTime;
					}

					if (train.IsPlayerTrain & Stations[d].Type != StationType.Normal & Stations[d].DepartureTime < 0.0)
					{
						setToRed = true;
					}
					else if (t >= 0.0 & SecondsSinceMidnight < t - train.TimetableDelta)
					{
						setToRed = true;
					}
					else if (!Section.TrainReachedStopPoint)
					{
						setToRed = true;
					}
				}
				else if (Stations[d].Type != StationType.Normal)
				{
					setToRed = true;
				}
			}

			// train in block
			if (!Section.IsFree())
			{
				setToRed = true;
			}

			// free sections
			int newAspect = -1;

			if (setToRed)
			{
				Section.FreeSections = 0;
				newAspect = zeroAspect;
			}
			else
			{
				Section n = Section.NextSection;

				if (n != null)
				{
					if (n.FreeSections == -1)
					{
						Section.FreeSections = -1;
					}
					else
					{
						Section.FreeSections = n.FreeSections + 1;
					}
				}
				else
				{
					Section.FreeSections = -1;
				}
			}

			// change aspect
			if (newAspect == -1)
			{
				if (Section.Type == SectionType.ValueBased)
				{
					// value-based
					Section n = Section.NextSection;
					int a = Section.Aspects.Last().Number;

					if (n != null && n.CurrentAspect >= 0)
					{

						a = n.Aspects[n.CurrentAspect].Number;
					}

					for (int i = Section.Aspects.Length - 1; i >= 0; i--)
					{
						if (Section.Aspects[i].Number > a)
						{
							newAspect = i;
						}
					}

					if (newAspect == -1)
					{
						newAspect = Section.Aspects.Length - 1;
					}
				}
				else
				{
					// index-based
					if (Section.FreeSections >= 0 & Section.FreeSections < Section.Aspects.Length)
					{
						newAspect = Section.FreeSections;
					}
					else
					{
						newAspect = Section.Aspects.Length - 1;
					}
				}
			}

			// apply new aspect
			Section.CurrentAspect = newAspect;

			// update previous section
			UpdateSection(Section.PreviousSection);
		}

		/// <summary>Updates the currently displayed background</summary>
		/// <param name="TimeElapsed">The time elapsed since the previous call to this function</param>
		/// <param name="GamePaused">Whether the game is currently paused</param>
		public void UpdateBackground(double TimeElapsed, bool GamePaused)
		{
			if (GamePaused)
			{
				//Don't update the transition whilst paused
				TimeElapsed = 0.0;
			}

			const float scale = 0.5f;

			// fog
			const float fogDistance = 600.0f;

			if (CurrentFog.Start < CurrentFog.End & CurrentFog.Start < fogDistance)
			{
				float ratio = (float)BackgroundHandle.BackgroundImageDistance / fogDistance;

				renderer.OptionFog = true;
				renderer.Fog.Start = CurrentFog.Start * ratio * scale;
				renderer.Fog.End = CurrentFog.End * ratio * scale;
				renderer.Fog.Color = CurrentFog.Color;
				renderer.SetFogForImmediateMode();
			}
			else
			{
				renderer.OptionFog = false;
			}

			//Update the currently displayed background
			CurrentBackground.UpdateBackground(SecondsSinceMidnight, TimeElapsed, false);

			if (TargetBackground == null || TargetBackground == CurrentBackground)
			{
				//No target background, so call the render function
				renderer.Background.Render(CurrentBackground, scale);
				return;
			}

			//Update the target background
			if (TargetBackground is StaticBackground)
			{
				TargetBackground.Countdown += TimeElapsed;
			}

			TargetBackground.UpdateBackground(SecondsSinceMidnight, TimeElapsed, true);

			switch (TargetBackground.Mode)
			{
				//Render, switching on the transition mode
				case BackgroundTransitionMode.FadeIn:
					renderer.Background.Render(CurrentBackground, 1.0f, scale);
					renderer.Background.Render(TargetBackground, TargetBackground.CurrentAlpha, scale);
					break;
				case BackgroundTransitionMode.FadeOut:
					renderer.Background.Render(TargetBackground, 1.0f, scale);
					renderer.Background.Render(CurrentBackground, TargetBackground.CurrentAlpha, scale);
					break;
			}

			//If our target alpha is greater than or equal to 1.0f, the background is fully displayed
			if (TargetBackground.CurrentAlpha >= 1.0f)
			{
				//Set the current background to the target & reset target to null
				CurrentBackground = TargetBackground;
				TargetBackground = null;
			}
		}
	}
}
