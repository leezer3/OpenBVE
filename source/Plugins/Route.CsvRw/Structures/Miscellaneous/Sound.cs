using System;
using System.Linq;
using System.Xml;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Sounds;
using RouteManager2;
using RouteManager2.Events;

namespace CsvRwRouteParser
{
	internal class Sound
	{
		/// <summary>The track position of the sound</summary>
		internal readonly double TrackPosition;
		/// <summary>The sound buffer to play</summary>
		internal readonly SoundHandle SoundBuffer;
		/// <summary>The type of sound</summary>
		internal readonly SoundType Type;
		/// <summary>The relative sound position to the track position</summary>
		internal readonly Vector3 Position;
		/// <summary>The radius of the sound</summary>
		internal readonly double Radius;
		/// <summary>If a dynamic sound, the train speed at which the sound will be played at original speed</summary>
		internal readonly double Speed;
		/// <summary>Whether this is a MicSound</summary>
		private readonly bool IsMicSound;
		/// <summary>The forwards tolerance for triggering the sound</summary>
		internal readonly double ForwardTolerance;
		/// <summary>The backwards tolerance for triggering the sound</summary>
		internal readonly double BackwardTolerance;
		
		internal Sound(double trackPosition, string fileName, double speed, Vector2 position = new Vector2(), double forwardTolerance = 0, double backwardTolerance = 0, bool allCars = false)
		{
			// Radius of 15.0m at full volume as per BVE2 / BVE4
			Radius = 15.0;
			if (string.IsNullOrEmpty(fileName))
			{
				IsMicSound = true;
			}
			else
			{
				Plugin.CurrentHost.RegisterSound(fileName, Radius, out SoundBuffer);
			}
			
			switch (speed)
			{
				case -1:
					Type = SoundType.Ambient;
					break;
				case 0:
					Type = allCars ? SoundType.TrainAllCarStatic : SoundType.TrainStatic;
					break;
				default:
					Type = allCars ? SoundType.TrainAllCarDynamic : SoundType.TrainDynamic;
					break;
			}
			
			TrackPosition = trackPosition;
			Speed = speed;
			Position = new Vector3(position.X, position.Y, 0);
			ForwardTolerance = forwardTolerance;
			BackwardTolerance = backwardTolerance;
		}

		internal Sound(double trackPosition, string xmlFile)
		{
			double trailingSilence = 0;
			TrackPosition = trackPosition;
			bool looped = false;
			string fn = string.Empty;
			XmlDocument currentXML = new XmlDocument();
			//Load the object's XML file 
			currentXML.Load(xmlFile);
			string Path = System.IO.Path.GetDirectoryName(xmlFile);
			//Check for null
			if (currentXML.DocumentElement != null)
			{
				XmlNodeList DocumentNodes = currentXML.DocumentElement.SelectNodes("/openBVE/WorldSound");
				if (DocumentNodes != null)
				{
					foreach (XmlNode n in DocumentNodes)
					{
						if (n.ChildNodes.OfType<XmlElement>().Any())
						{
							foreach (XmlNode c in n.ChildNodes)
							{
								switch (c.Name.ToLowerInvariant())
								{
									case "filename":
										fn = OpenBveApi.Path.CombineFile(Path, c.InnerText);
										if (!System.IO.File.Exists(fn))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Sound " + c.InnerText + " does not exist in XML sound " + xmlFile);
											return;
										}
										break;
									case "radius":
										if (!NumberFormats.TryParseDoubleVb6(c.InnerText, out Radius))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Sound radius was invalid in XML sound " + xmlFile);
										}
										break;
									case "position":
										string[] splitString = c.InnerText.Split(',');
										if (!NumberFormats.TryParseDoubleVb6(splitString[0], out Position.X))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Sound Position X was invalid in XML sound " + xmlFile);
										}

										if (!NumberFormats.TryParseDoubleVb6(splitString[1], out Position.Y))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Sound Position Y was invalid in XML sound " + xmlFile);
										}

										if (!NumberFormats.TryParseDoubleVb6(splitString[2], out Position.Z))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Sound Position Z was invalid in XML sound " + xmlFile);
										}

										break;
									case "looped":
										if (c.InnerText.ToLowerInvariant() == "1" || c.InnerText.ToLowerInvariant() == "true")
										{
											looped = true;
										}
										else
										{
											looped = false;
										}

										break;
									case "trainspeed":
										if (!NumberFormats.TryParseDoubleVb6(c.InnerText, out Speed))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Sound speed was invalid in XML sound " + xmlFile);
										}
										break;
									case "repetitioninterval":
										if (!NumberFormats.TryParseDoubleVb6(c.InnerText, out trailingSilence))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "Repetition interval was invalid in XML sound " + xmlFile);
										}
										break;
								}
							}
						}
					}
				}
			}

			//Data has been collected so setup the actual sound
			Plugin.CurrentHost.RegisterSound(fn, Radius, out SoundBuffer);
			if (looped)
			{
				Type = SoundType.Ambient;
			}
			else
			{
				if (Speed == 0.0)
				{
					Type = SoundType.TrainStatic;
				}
				else
				{
					Type = SoundType.TrainDynamic;
				}
			}
		}

		internal void CreateAmbient(Vector3 pos, double StartingDistance, Vector2 Direction, double planar, double updown)
		{
			if (Type != SoundType.Ambient)
			{
				return;
			}

			if (SoundBuffer == null && !IsMicSound)
			{
				return;
			}

			double d = TrackPosition - StartingDistance;
			double dx = Position.X;
			double dy = Position.Y;
			double wa = Math.Atan2(Direction.Y, Direction.X) - planar;
			Vector3 w = new Vector3(Math.Cos(wa), Math.Tan(updown), Math.Sin(wa));
			w.Normalize();
			Vector3 s = new Vector3(Direction.Y, 0.0, -Direction.X);
			Vector3 u = Vector3.Cross(w, s);
			Vector3 wpos = pos + new Vector3(s.X * dx + u.X * dy + w.X * d, s.Y * dx + u.Y * dy + w.Y * d, s.Z * dx + u.Z * dy + w.Z * d);
			if (IsMicSound)
			{
				Plugin.CurrentHost.PlayMicSound(wpos, BackwardTolerance, ForwardTolerance);
			}
			else
			{
				Plugin.CurrentHost.PlaySound(SoundBuffer, 1.0, 1.0, wpos, null, true);
			}
		}

		internal void CreateEvent(CurrentRoute CurrentRoute, int CurrentElement, double StartingDistance)
		{
			if (Type == SoundType.Ambient)
			{
				return;
			}

			int m = CurrentRoute.Tracks[0].Elements[CurrentElement].Events.Length;
			Array.Resize(ref CurrentRoute.Tracks[0].Elements[CurrentElement].Events, m + 1);
			double d = TrackPosition - StartingDistance;
			switch (Type)
			{
				case SoundType.TrainStatic:
					CurrentRoute.Tracks[0].Elements[CurrentElement].Events[m] = new SoundEvent(Plugin.CurrentHost, d, SoundBuffer, true, Type, false, Position);
					break;
				case SoundType.TrainAllCarStatic:
					CurrentRoute.Tracks[0].Elements[CurrentElement].Events[m] = new SoundEvent(Plugin.CurrentHost, d, SoundBuffer, true, Type, true, Position);
					break;
				case SoundType.TrainDynamic:
					CurrentRoute.Tracks[0].Elements[CurrentElement].Events[m] = new SoundEvent(Plugin.CurrentHost, d, SoundBuffer, false, Type, false, Position, Speed);
					break;
				case SoundType.TrainAllCarDynamic:
					CurrentRoute.Tracks[0].Elements[CurrentElement].Events[m] = new SoundEvent(Plugin.CurrentHost, d, SoundBuffer, false, Type, false, Position, Speed);
					break;
			}
		}
	}
}
