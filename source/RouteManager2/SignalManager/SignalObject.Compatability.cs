using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using OpenBveApi.FunctionScripting;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.World;
using Path = OpenBveApi.Path;

namespace RouteManager2.SignalManager
{
	/// <summary>Defines a default Japanese signal (See the documentation)</summary>
	public class CompatibilitySignalObject : SignalObject
	{
		/// <summary>The aspect numbers associated with each state</summary>
		public readonly int[] AspectNumbers;
		/// <summary>The object states</summary>
		public readonly StaticObject[] Objects;

		private readonly HostInterface currentHost;
		public CompatibilitySignalObject(int[] aspectNumbers, StaticObject[] Objects, HostInterface Host)
		{
			this.AspectNumbers = aspectNumbers;
			this.Objects = Objects;
			this.currentHost = Host;
		}

		public override void Create(Vector3 wpos, Transformation railTransformation, Transformation localTransformation, int sectionIndex, double startingDistance, double endingDistance, double trackPosition, double brightness)
		{
			if (AspectNumbers.Length != 0)
			{
				AnimatedObjectCollection aoc = new AnimatedObjectCollection(currentHost)
				{
					Objects = new[]
					{
						new AnimatedObject(currentHost)
					}
				};
				aoc.Objects[0].States = new ObjectState[AspectNumbers.Length];
				for (int l = 0; l < AspectNumbers.Length; l++)
				{
					aoc.Objects[0].States[l] = new ObjectState((StaticObject)Objects[l].Clone());
				}
				CultureInfo Culture = CultureInfo.InvariantCulture;
				string expr = "";
				for (int l = 0; l < AspectNumbers.Length - 1; l++)
				{
					expr += "section " + AspectNumbers[l].ToString(Culture) + " <= " + l.ToString(Culture) + " ";
				}
				expr += (AspectNumbers.Length - 1).ToString(Culture);
				for (int l = 0; l < AspectNumbers.Length - 1; l++)
				{
					expr += " ?";
				}
				aoc.Objects[0].StateFunction = new FunctionScript(currentHost, expr, false);
				aoc.Objects[0].RefreshRate = 1.0 + 0.01 * new Random().NextDouble();
				aoc.CreateObject(wpos, railTransformation, localTransformation, sectionIndex, startingDistance, endingDistance, trackPosition, brightness);
			}
		}

		/// <summary>Loads a list of compatibility signal objects</summary>
		/// <param name="currentHost">The host application interface</param>
		/// <param name="fileName">The file name of the object list</param>
		/// <param name="objects">The returned array of speed limits</param>
		/// <param name="signalPost">Sets the default signal post</param>
		/// <param name="speedLimits">The array of signal speed limits</param>
		/// <returns>An array of compatability signal objects</returns>
		public static void ReadCompatibilitySignalXML(HostInterface currentHost, string fileName, out CompatibilitySignalObject[] objects, out UnifiedObject signalPost, out double[] speedLimits)
		{
			signalPost = new StaticObject(currentHost);
			objects = new CompatibilitySignalObject[9];
			//Default Japanese speed limits converted to m/s
			speedLimits = new[] { 0.0, 6.94444444444444, 15.2777777777778, 20.8333333333333, double.PositiveInfinity, double.PositiveInfinity };
			XmlDocument currentXML = new XmlDocument();
			currentXML.Load(fileName);
			string currentPath = System.IO.Path.GetDirectoryName(fileName);
			if (currentXML.DocumentElement != null)
			{
				XmlNode node = currentXML.SelectSingleNode("/openBVE/CompatibilitySignals/SignalSetName");
				if (node != null)
				{
					currentHost.AddMessage(MessageType.Information, false, "INFO: Using the " + node.InnerText + " compatibility signal set.");
				}
				XmlNodeList DocumentNodes = currentXML.DocumentElement.SelectNodes("/openBVE/CompatibilitySignals/Signal");
				if (DocumentNodes != null)
				{
					int index = 0;
					foreach (XmlNode nn in DocumentNodes)
					{
						List<StaticObject> objectList = new List<StaticObject>();
						List<int> aspectList = new List<int>();
						try
						{
							if (nn.HasChildNodes)
							{
								foreach (XmlNode n in nn.ChildNodes)
								{
									if (n.Name != "Aspect")
									{
										continue;
									}

									int aspect;
									if (!NumberFormats.TryParseIntVb6(n.Attributes["Number"].Value, out aspect))
									{
										currentHost.AddMessage(MessageType.Error, true, "Invalid aspect number " + aspect + " in the signal object list in the compatability signal file " + fileName);
										continue;
									}

									aspectList.Add(aspect);
									
									StaticObject staticObject = new StaticObject(currentHost);
									if (n.InnerText.ToLowerInvariant() != "null")
									{
										string objectFile = Path.CombineFile(currentPath, n.InnerText);
										if (File.Exists(objectFile))
										{
											currentHost.LoadStaticObject(objectFile, Encoding.UTF8, false, out staticObject);
										}
										else
										{
											currentHost.AddMessage(MessageType.Error, true, "Compatibility signal file " + objectFile + " not found in " + fileName);
										}
									}
									objectList.Add(staticObject);
								}
							}
						}
						catch
						{
							currentHost.AddMessage(MessageType.Error, true, "An unexpected error was encountered whilst processing the compatability signal file " + fileName);
						}
						objects[index] = new CompatibilitySignalObject(aspectList.ToArray(), objectList.ToArray(), currentHost);
						index++;
					}
				}
				
				string signalPostFile = Path.CombineFile(currentPath, "Japanese\\signal_post.csv"); //default plain post
				try
				{
					node = currentXML.SelectSingleNode("/openBVE/CompatibilitySignals/SignalPost");
					if (node != null)
					{
						string newFile = Path.CombineFile(currentPath, node.InnerText);
						if (File.Exists(newFile))
						{
							signalPostFile = newFile;
						}
					}
					currentHost.LoadObject(signalPostFile, Encoding.UTF8, out signalPost);
				}
				catch
				{
					currentHost.AddMessage(MessageType.Error, true, "An unexpected error was encountered whilst processing the compatability signal file " + fileName);
				}

				DocumentNodes = currentXML.DocumentElement.SelectNodes("/openBVE/CompatibilitySignals/SpeedLimits");
				if (DocumentNodes != null)
				{
					foreach (XmlNode nn in DocumentNodes)
					{
						try
						{
							if (nn.HasChildNodes)
							{
								foreach (XmlNode n in nn.ChildNodes)
								{
									if (n.Name != "Aspect")
									{
										continue;
									}

									int aspect = 0;
									if (n.Attributes != null)
									{
										if (!NumberFormats.TryParseIntVb6(n.Attributes["Number"].Value, out aspect))
										{
											currentHost.AddMessage(MessageType.Error, true, "Invalid aspect number " + aspect + " in the speed limit list in the compatability signal file " + fileName);
											continue;
										}
									}

									if (aspect <= speedLimits.Length)
									{
										int l = speedLimits.Length;
										Array.Resize(ref speedLimits, aspect + 1);
										for (int i = l; i < speedLimits.Length; i++)
										{
											speedLimits[i] = double.PositiveInfinity;
										}

										if (!NumberFormats.TryParseDoubleVb6(n.InnerText, out speedLimits[aspect]))
										{
											speedLimits[aspect] = double.MaxValue;
											if (n.InnerText.ToLowerInvariant() != "unlimited")
											{
												currentHost.AddMessage(MessageType.Error, true, "Invalid speed limit provided for aspect " + aspect + " in the compatability signal file " + fileName);
											}
										}
										else
										{
											//convert to m/s as that's what we use internally
											speedLimits[aspect] *= 0.277777777777778;
										}
									}
								}
							}
						}
						catch
						{
							currentHost.AddMessage(MessageType.Error, true, "An unexpected error was encountered whilst processing the compatability signal file " + fileName);
						}
					}
				}
			}
		}
	}
}
