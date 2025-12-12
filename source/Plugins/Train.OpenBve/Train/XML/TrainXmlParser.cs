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
using System.Xml;
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
		internal void Parse(string fileName, TrainBase Train, ref UnifiedObject[] CarObjects, ref UnifiedObject[] BogieObjects, ref UnifiedObject[] CouplerObjects, out bool[] interiorVisible)
		{

			XMLFile<TrainXMLSection, TrainXMLKey> xmlFile = new XMLFile<TrainXMLSection, TrainXMLKey>(fileName, "/openBVE/Train", Plugin.CurrentHost);
			
			//The current XML file to load
			XmlDocument currentXML = new XmlDocument();
			//Load the marker's XML file 
			currentXML.Load(fileName);
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
					ParseCarBlock(carBlocks[i], fileName, carIndex, ref Train, ref CarObjects, ref BogieObjects, ref interiorVisible[carIndex]);
					carIndex++;
				}
				else
				{
					if (carIndex - 1 > Train.Cars.Length - 2)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Unexpected extra coupler encountered in XML file " + fileName);
						continue;
					}

					carBlocks[i].GetValue(TrainXMLKey.Minimum, out Train.Cars[carIndex - 1].Coupler.MinimumDistanceBetweenCars);
					carBlocks[i].GetValue(TrainXMLKey.Maximum, out Train.Cars[carIndex - 1].Coupler.MinimumDistanceBetweenCars);
					if (carBlocks[i].GetPath(TrainXMLKey.Object, currentPath, out string objectPath))
					{
						Plugin.CurrentHost.LoadObject(objectPath, Encoding.Default, out CouplerObjects[carIndex - 1]);
					}
					carBlocks[i].GetValue(TrainXMLKey.CanUncouple, out bool canUncouple);
					Train.Cars[carIndex - 1].Coupler.CanUncouple = canUncouple;
					carBlocks[i].GetEnumValue(TrainXMLKey.UncouplingBehaviour, out Train.Cars[carIndex - 1].Coupler.UncouplingBehaviour);
				}
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
						if (subBlock.GetValue(TrainXMLKey.Power, out string power))
						{
							Train.Handles.Power.NotchDescriptions = power.Split(separatorChars);
							for (int j = 0; j < Train.Handles.Power.NotchDescriptions.Length; j++)
							{
								Vector2 s = Plugin.Renderer.Fonts.NormalFont.MeasureString(Train.Handles.Power.NotchDescriptions[j]);
								if (s.X > Train.Handles.Power.MaxWidth)
								{
									Train.Handles.Power.MaxWidth = s.X;
								}
							}
						}
						if (subBlock.GetValue(TrainXMLKey.Brake, out string brake))
						{
							Train.Handles.Brake.NotchDescriptions = brake.Split(separatorChars);
							for (int j = 0; j < Train.Handles.Brake.NotchDescriptions.Length; j++)
							{
								Vector2 s = Plugin.Renderer.Fonts.NormalFont.MeasureString(Train.Handles.Brake.NotchDescriptions[j]);
								if (s.X > Train.Handles.Brake.MaxWidth)
								{
									Train.Handles.Brake.MaxWidth = s.X;
								}
							}
						}
						if (subBlock.GetValue(TrainXMLKey.Brake, out string locoBrake))
						{
							Train.Handles.LocoBrake.NotchDescriptions = locoBrake.Split(separatorChars);
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
				if (CarObjects[i] != null)
				{
					if (CarObjectsReversed[i])
					{
						{
							// reverse axle positions
							double temp = Train.Cars[i].FrontAxle.Position;
							Train.Cars[i].FrontAxle.Position = -Train.Cars[i].RearAxle.Position;
							Train.Cars[i].RearAxle.Position = -temp;
						}
						if (CarObjects[i] is StaticObject)
						{
							StaticObject obj = (StaticObject)CarObjects[i].Clone();
							obj.ApplyScale(-1.0, 1.0, -1.0);
							CarObjects[i] = obj;
						}
						else if (CarObjects[i] is AnimatedObjectCollection)
						{
							AnimatedObjectCollection obj = (AnimatedObjectCollection)CarObjects[i].Clone();
							obj.Reverse();
							CarObjects[i] = obj;
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
				bool IsOdd = (i % 2 != 0);
				int CarIndex = i / 2;
				if (BogieObjects[i] != null)
				{
					if (BogieObjectsReversed[i])
					{
						{
							// reverse axle positions
							if (IsOdd)
							{
								double temp = Train.Cars[CarIndex].FrontBogie.FrontAxle.Position;
								Train.Cars[CarIndex].FrontBogie.FrontAxle.Position = -Train.Cars[CarIndex].FrontBogie.RearAxle.Position;
								Train.Cars[CarIndex].FrontBogie.RearAxle.Position = -temp;
							}
							else
							{
								double temp = Train.Cars[CarIndex].RearBogie.FrontAxle.Position;
								Train.Cars[CarIndex].RearBogie.FrontAxle.Position = -Train.Cars[CarIndex].RearBogie.RearAxle.Position;
								Train.Cars[CarIndex].RearBogie.RearAxle.Position = -temp;
							}
						}
						if (BogieObjects[i] is StaticObject)
						{
							StaticObject obj = (StaticObject)BogieObjects[i].Clone();
							obj.ApplyScale(-1.0, 1.0, -1.0);
							BogieObjects[i] = obj;
						}
						else if (BogieObjects[i] is AnimatedObjectCollection)
						{
							AnimatedObjectCollection obj = (AnimatedObjectCollection)BogieObjects[i].Clone();
							obj.Reverse();
							BogieObjects[i] = obj;
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
