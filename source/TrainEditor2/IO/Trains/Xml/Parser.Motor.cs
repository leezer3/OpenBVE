using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using SoundManager;
using TrainEditor2.Models.Trains;
using TrainEditor2.Simulation.TrainManager;
using TrainEditor2.Systems;

namespace TrainEditor2.IO.Trains.Xml
{
	internal static partial class TrainXml
	{
		private static Motor ParseMotorNode(string fileName, XElement parent)
		{
			Motor motor = new Motor();

			string section = parent.Name.LocalName;

			foreach (XElement keyNode in parent.Elements())
			{
				string key = keyNode.Name.LocalName;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (key.ToLowerInvariant())
				{
					case "powertracks":
						ParseMotorTracksNode(fileName, keyNode, motor, Motor.TrackType.Power);
						break;
					case "braketracks":
						ParseMotorTracksNode(fileName, keyNode, motor, Motor.TrackType.Brake);
						break;
					default:
						Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {key} encountered in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						break;
				}
			}

			return motor;
		}

		private static void ParseMotorTracksNode(string fileName, XElement parent, Motor baseMotor, Motor.TrackType trackType)
		{
			string section = parent.Name.LocalName;

			foreach (XElement keyNode in parent.Elements())
			{
				string key = keyNode.Name.LocalName;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				if (key.ToLowerInvariant() != "track")
				{
					Interface.AddMessage(MessageType.Error, false, $"Invalid track node {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
					continue;
				}

				baseMotor.Tracks.Add(ParseMotorTrackNode(fileName, keyNode, baseMotor, trackType));
			}
		}

		private static Motor.Track ParseMotorTrackNode(string fileName, XElement parent, Motor baseMotor, Motor.TrackType trackType)
		{
			List<TrainManager.MotorSound.Vertex<float>> pitchVertices = new List<TrainManager.MotorSound.Vertex<float>>();
			List<TrainManager.MotorSound.Vertex<float>> volumeVertices = new List<TrainManager.MotorSound.Vertex<float>>();
			List<TrainManager.MotorSound.Vertex<int, SoundBuffer>> soundIndexVertices = new List<TrainManager.MotorSound.Vertex<int, SoundBuffer>>();

			string section = parent.Name.LocalName;

			foreach (XElement keyNode in parent.Elements())
			{
				string key = keyNode.Name.LocalName;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				switch (key.ToLowerInvariant())
				{
					case "pitch":
						ParseMotorVerticesNode(fileName, keyNode, pitchVertices);
						break;
					case "volume":
						ParseMotorVerticesNode(fileName, keyNode, volumeVertices);
						break;
					case "soundindex":
						ParseMotorVerticesNode(fileName, keyNode, soundIndexVertices);
						break;
					default:
						Interface.AddMessage(MessageType.Warning, false, $"Unsupported key {key} encountered in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						break;
				}
			}

			return Motor.Track.MotorSoundTableToTrack(baseMotor, trackType, new TrainManager.MotorSound.Table { PitchVertices = pitchVertices.ToArray(), GainVertices = volumeVertices.ToArray(), BufferVertices = soundIndexVertices.ToArray() }, x => x, x => x, x => x);
		}

		private static void ParseMotorVerticesNode(string fileName, XElement parent, ICollection<TrainManager.MotorSound.Vertex<float>> vertices)
		{
			string section = parent.Name.LocalName;

			foreach (XElement keyNode in parent.Elements())
			{
				string key = keyNode.Name.LocalName;
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				if (key.ToLowerInvariant() != "vertex")
				{
					Interface.AddMessage(MessageType.Error, false, $"Invalid vertex node {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
					continue;
				}

				if (value.Any())
				{
					string[] values = value.Split(',');

					if (values.Length == 2)
					{
						float x, y;

						if (!NumberFormats.TryParseFloatVb6(values[0], out x) || x < 0.0f)
						{
							Interface.AddMessage(MessageType.Error, false, $"X must be a non-negative floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						}
						else if (!NumberFormats.TryParseFloatVb6(values[1], out y) || y < 0.0f)
						{
							Interface.AddMessage(MessageType.Error, false, $"Y must be a non-negative floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						}
						else
						{
							vertices.Add(new TrainManager.MotorSound.Vertex<float> { X = x, Y = y });
						}
					}
					else
					{
						Interface.AddMessage(MessageType.Error, false, $"Exactly two arguments are expected in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
					}
				}
			}
		}

		private static void ParseMotorVerticesNode(string fileName, XElement parent, ICollection<TrainManager.MotorSound.Vertex<int, SoundBuffer>> vertices)
		{
			string section = parent.Name.LocalName;

			foreach (XElement keyNode in parent.Elements())
			{
				string key = keyNode.Name.LocalName;
				string value = keyNode.Value;
				int lineNumber = ((IXmlLineInfo)keyNode).LineNumber;

				if (key.ToLowerInvariant() != "vertex")
				{
					Interface.AddMessage(MessageType.Error, false, $"Invalid vertex node {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
					continue;
				}

				if (value.Any())
				{
					string[] values = value.Split(',');

					if (values.Length == 2)
					{
						float x;
						int y;

						if (!NumberFormats.TryParseFloatVb6(values[0], out x) || x < 0.0f)
						{
							Interface.AddMessage(MessageType.Error, false, $"X must be a non-negative floating-point number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						}
						else if (!NumberFormats.TryParseIntVb6(values[1], out y))
						{
							Interface.AddMessage(MessageType.Error, false, $"Y must be a integer number in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
						}
						else
						{
							if (y < 0)
							{
								y = -1;
							}

							vertices.Add(new TrainManager.MotorSound.Vertex<int, SoundBuffer> { X = x, Y = y });
						}
					}
					else
					{
						Interface.AddMessage(MessageType.Error, false, $"Exactly two arguments are expected in {key} in {section} at line {lineNumber.ToString(culture)} in {fileName}");
					}
				}
			}
		}
	}
}
