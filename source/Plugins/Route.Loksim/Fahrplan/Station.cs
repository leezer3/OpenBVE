//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2026, Christopher Lees, The OpenBVE Project
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
using System.Xml;
using OpenBveApi.Interface;

namespace LokSimRouteParser
{
	internal class Station
	{
		/// <summary>The arrival time</summary>
		internal int Arrival;
		/// <summary>The departure time</summary>
		internal int Departure;
		/// <summary>Whether this is a stop point</summary>
		internal bool ShouldStop;
		/// <summary>Whether this is a normal stop</summary>
		internal bool NormalStop;
		/// <summary>Whether this is a request stop</summary>
		internal bool RequestStop;
		/// <summary>The minimum time stopped</summary>
		internal int StopTime;
		/// <summary>The station name</summary>
		internal string Name;
		/// <summary>Whether the station forces a red signal until departure</summary>
		internal bool ForcedRedSignal;
		/// <summary>The distance at which the pre-station stop alarm is sounded</summary>
		internal int StopAlarmDistance;
		/// <summary>The departure sound</summary>
		internal string DepartureSound;
		/// <summary>The arrival sound</summary>
		internal string ArrivalSound;
		/// <summary>Whether the doors are to open</summary>
		internal bool OpenDoors;

		internal Station(XmlNode node)
		{
			if (node.Attributes != null)
			{
				for (int i = 0; i < node.Attributes.Count; i++)
				{
					if (!Enum.TryParse(node.Attributes[i].Name, true, out PropsAttribute props))
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Loksim3D: Unrecognised Props attribute " +node.Attributes[i].Name + " in station");
						continue;
					}
					switch (props)
					{
						case PropsAttribute.Abfahrt:
							Arrival = int.Parse(node.Attributes[i].InnerText);
							break;
						case PropsAttribute.Ankunft:
							Departure = int.Parse(node.Attributes[i].InnerText);
							break;
						case PropsAttribute.Haltepunkt:
							if (node.InnerText.ToLowerInvariant() == "true")
							{
								ShouldStop = true;
							}
							break;
						case PropsAttribute.Bedarfshalt:
							if (node.InnerText.ToLowerInvariant() == "true")
							{
								RequestStop = true;
							}
							break;
						case PropsAttribute.Betriebshalt:
							if (node.InnerText.ToLowerInvariant() == "true")
							{
								NormalStop = true;
							}
							break;
						case PropsAttribute.SignalVorAusfahrt:
							if (node.InnerText.ToLowerInvariant() == "true")
							{
								ForcedRedSignal = true;
							}
							break;
						case PropsAttribute.Haltdauer:
							StopTime = int.Parse(node.Attributes[i].InnerText);
							break;
						case PropsAttribute.Name:
							Name = node.Attributes[i].InnerText;
							break;
						case PropsAttribute.DistanzSoundVorBedarfsHalt:
							StopAlarmDistance = int.Parse(node.Attributes[i].InnerText);
							break;
						case PropsAttribute.SoundAnkunft:
							ArrivalSound = node.Attributes[i].InnerText;
							break;
						case PropsAttribute.SoundAbfahrt:
							DepartureSound = node.Attributes[i].InnerText;
							break;
						case PropsAttribute.SoundAnsage:
							// announcement
							break;
						case PropsAttribute.SoundHalt:
							// stop
							break;
						case PropsAttribute.SoundVorBedarfsHalt:
							// stop alarm sound
							break;
						case PropsAttribute.Tueren:
							if (node.InnerText.ToLowerInvariant() == "true")
							{
								OpenDoors = true;
							}
							break;
						case PropsAttribute.Zugfolgestelle:
							/*
							 * Train Following Point:
							 * ----------------------
							 *
							 * Section of track ahead may only be released if it is free of vehicles and not in use by train
							 * in opposite direction
							 *
							 * PZB related, probably interacts with the forced red signal.
							 */
							break;
					}
				}
			}
		}
	}
}
