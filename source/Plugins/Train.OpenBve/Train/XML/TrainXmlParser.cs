//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2025, Christopher Lees, The OpenBVE Project
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

using Formats.OpenBve;
using Formats.OpenBve.XML;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using TrainManager.Trains;
using Path = OpenBveApi.Path;

namespace Train.OpenBve
{
	partial class TrainXmlParser
	{
		private readonly Plugin Plugin;

		internal TrainXmlParser(Plugin plugin)
		{
			Plugin = plugin;
		}

		private static string currentPath;
		private static bool[] CarObjectsReversed;
		private static bool[] BogieObjectsReversed;
		
		private static readonly char[] separatorChars = { ';', ',' };
		internal static bool[] MotorSoundXMLParsed;
		internal void Parse(string fileName, TrainBase Train, ref UnifiedObject[] carObjects, ref UnifiedObject[] bogieObjects, ref UnifiedObject[] couplerObjects, out bool[] interiorVisible)
		{
			XMLFile<TrainXMLSection, TrainXMLKey> xmlFile = new XMLFile<TrainXMLSection, TrainXMLKey>(fileName, "/openBVE/Train", Plugin.CurrentHost);
			currentPath = Path.GetDirectoryName(fileName);
			MotorSoundXMLParsed = new bool[Train.Cars.Length];
			CarObjectsReversed = new bool[Train.Cars.Length];
			BogieObjectsReversed = new bool[Train.Cars.Length * 2];
			interiorVisible = new bool[Train.Cars.Length];

			// Car + coupler blocks should be processed first
			List<Block<TrainXMLSection, TrainXMLKey>> carBlocks = xmlFile.ReadBlocks(new[] { TrainXMLSection.Car, TrainXMLSection.Coupler });
			int carIndex = 0;
			double perCarProgress = 0.25 / carBlocks.Count;
			for (int i = 0; i < carBlocks.Count; i++)
			{
				Plugin.CurrentProgress = Plugin.LastProgress + perCarProgress * i;
				if (carBlocks[i].Key == TrainXMLSection.Car)
				{
					if (carIndex < Train.Cars.Length)
					{
						ParseCarBlock(carBlocks[i], carIndex, ref Train, ref carObjects, ref bogieObjects, ref interiorVisible[carIndex]);
					}
					carIndex++;
				}
				else
				{
					if (carIndex == 0 || carIndex - 1 > Train.Cars.Length - 2)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Unexpected extra coupler encountered in XML file " + xmlFile.FileName);
						continue;
					}
					
					carBlocks[i].GetValue(TrainXMLKey.Minimum, out Train.Cars[carIndex - 1].Coupler.MinimumDistanceBetweenCars);
					carBlocks[i].GetValue(TrainXMLKey.Maximum, out Train.Cars[carIndex - 1].Coupler.MinimumDistanceBetweenCars);
					if (carBlocks[i].GetPath(TrainXMLKey.Object, currentPath, out string objectPath))
					{
						Plugin.CurrentHost.LoadObject(objectPath, Encoding.Default, out couplerObjects[carIndex - 1]);
					}
					carBlocks[i].GetValue(TrainXMLKey.CanUncouple, out bool canUncouple);
					Train.Cars[carIndex - 1].Coupler.CanUncouple = canUncouple;
					carBlocks[i].GetEnumValue(TrainXMLKey.UncouplingBehaviour, out Train.Cars[carIndex - 1].Coupler.UncouplingBehaviour);
				}
				carBlocks[i].ReportErrors();
			}

			// process all other blocks
			while (xmlFile.RemainingSubBlocks > 0)
			{
				Block<TrainXMLSection, TrainXMLKey> subBlock = xmlFile.ReadNextBlock();
				switch (subBlock.Key)
				{
					case TrainXMLSection.DriverBody:
						double shoulderHeight = Train.DriverBody.ShoulderHeight;
						double headHeight = Train.DriverBody.HeadHeight;
						subBlock.TryGetValue(TrainXMLKey.ShoulderHeight, ref shoulderHeight);
						subBlock.TryGetValue(TrainXMLKey.HeadHeight, ref headHeight);
						Train.DriverBody = new DriverBody(Train, shoulderHeight, headHeight);
						break;
					case TrainXMLSection.NotchDescriptions:
						if (subBlock.TryGetStringArray(TrainXMLKey.Power, separatorChars, ref Train.Handles.Power.NotchDescriptions))
						{
							for (int j = 0; j < Train.Handles.Power.NotchDescriptions.Length; j++)
							{
								Vector2 s = Plugin.Renderer.Fonts.NormalFont.MeasureString(Train.Handles.Power.NotchDescriptions[j]);
								if (s.X > Train.Handles.Power.MaxWidth)
								{
									Train.Handles.Power.MaxWidth = s.X;
								}
							}
						}
						if (subBlock.TryGetStringArray(TrainXMLKey.Brake, separatorChars, ref Train.Handles.Brake.NotchDescriptions))
						{
							for (int j = 0; j < Train.Handles.Brake.NotchDescriptions.Length; j++)
							{
								Vector2 s = Plugin.Renderer.Fonts.NormalFont.MeasureString(Train.Handles.Brake.NotchDescriptions[j]);
								if (s.X > Train.Handles.Brake.MaxWidth)
								{
									Train.Handles.Brake.MaxWidth = s.X;
								}
							}
						}
						if (subBlock.TryGetStringArray(TrainXMLKey.Brake, separatorChars, ref Train.Handles.LocoBrake.NotchDescriptions))
						{
							for (int j = 0; j < Train.Handles.LocoBrake.NotchDescriptions.Length; j++)
							{
								Vector2 s = Plugin.Renderer.Fonts.NormalFont.MeasureString(Train.Handles.LocoBrake.NotchDescriptions[j]);
								if (s.X > Train.Handles.LocoBrake.MaxWidth)
								{
									Train.Handles.LocoBrake.MaxWidth = s.X;
								}
							}
						}
						if (subBlock.GetValue(TrainXMLKey.Reverser, out string reverser))
						{
							Train.Handles.Reverser.NotchDescriptions = reverser.Split(separatorChars);
							for (int j = 0; j < Train.Handles.Reverser.NotchDescriptions.Length; j++)
							{
								Vector2 s = Plugin.Renderer.Fonts.NormalFont.MeasureString(Train.Handles.Reverser.NotchDescriptions[j]);
								if (s.X > Train.Handles.Reverser.MaxWidth)
								{
									Train.Handles.Reverser.MaxWidth = s.X;
								}
							}
						}
						break;
					case TrainXMLSection.Plugin:
						subBlock.GetValue(TrainXMLKey.LoadForAI, out bool loadForAI);
						if (subBlock.GetPath(TrainXMLKey.File, currentPath, out string pluginFile) && (loadForAI || Train.IsPlayerTrain))
						{
							if (!Train.LoadPlugin(pluginFile, currentPath))
							{
								Train.Plugin = null;
							}
						}
						break;
				}
				subBlock.ReportErrors();
			}

			xmlFile.GetValue(TrainXMLKey.HeadlightStates, out int headlightStates);
			Train.SafetySystems.Headlights = new LightSource(Train, headlightStates);
			if (xmlFile.GetPath(TrainXMLKey.Plugin, currentPath, out string plugin))
			{
				// older format, without AI control
				if (!Train.LoadPlugin(plugin, currentPath))
				{
					Train.Plugin = null;
				}
			}

			/*
			 * Add final properties and stuff
			 */
			for (int i = 0; i < Train.Cars.Length; i++)
			{
				if (carObjects[i] != null)
				{
					if (CarObjectsReversed[i])
					{
						{
							// reverse axle positions
							double temp = Train.Cars[i].FrontAxle.Position;
							Train.Cars[i].FrontAxle.Position = -Train.Cars[i].RearAxle.Position;
							Train.Cars[i].RearAxle.Position = -temp;
						}
						if (carObjects[i] is StaticObject)
						{
							StaticObject obj = (StaticObject)carObjects[i].Clone();
							obj.ApplyScale(-1.0, 1.0, -1.0);
							carObjects[i] = obj;
						}
						else if (carObjects[i] is AnimatedObjectCollection)
						{
							AnimatedObjectCollection obj = (AnimatedObjectCollection)carObjects[i].Clone();
							obj.Reverse();
							carObjects[i] = obj;
						}
						else
						{
							throw new NotImplementedException();
						}
					}
				}
			}

			//Check for bogie objects and reverse if necessary.....
			for (int i = 0; i < Train.Cars.Length * 2; i++)
			{
				bool isOdd = (i % 2 != 0);
				int index = i / 2;
				if (bogieObjects[i] != null)
				{
					if (BogieObjectsReversed[i])
					{
						{
							// reverse axle positions
							if (isOdd)
							{
								double temp = Train.Cars[index].FrontBogie.FrontAxle.Position;
								Train.Cars[index].FrontBogie.FrontAxle.Position = -Train.Cars[index].FrontBogie.RearAxle.Position;
								Train.Cars[index].FrontBogie.RearAxle.Position = -temp;
							}
							else
							{
								double temp = Train.Cars[index].RearBogie.FrontAxle.Position;
								Train.Cars[index].RearBogie.FrontAxle.Position = -Train.Cars[index].RearBogie.RearAxle.Position;
								Train.Cars[index].RearBogie.RearAxle.Position = -temp;
							}
						}
						if (bogieObjects[i] is StaticObject)
						{
							StaticObject obj = (StaticObject)bogieObjects[i].Clone();
							obj.ApplyScale(-1.0, 1.0, -1.0);
							bogieObjects[i] = obj;
						}
						else if (bogieObjects[i] is AnimatedObjectCollection)
						{
							AnimatedObjectCollection obj = (AnimatedObjectCollection)bogieObjects[i].Clone();
							obj.Reverse();
							bogieObjects[i] = obj;
						}
						else
						{
							throw new NotImplementedException();
						}
					}
				}
			}
		}
	}
}
