using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using OpenBveApi.Textures;

namespace CsvRwRouteParser
{
	internal class DynamicBackgroundParser
	{
		//Parses an XML background definition
		public static BackgroundHandle ReadBackgroundXML(string fileName)
		{
			List<StaticBackground> Backgrounds = new List<StaticBackground>();
			//The current XML file to load
			XmlDocument currentXML = new XmlDocument();
			//Load the object's XML file 
			currentXML.Load(fileName);
			string filePath = Path.GetDirectoryName(fileName);
			double[] UnitOfLength = { 1.0 };
			//Check for null
			if (currentXML.DocumentElement != null)
			{
				XmlNodeList DocumentNodes = currentXML.DocumentElement.SelectNodes("/openBVE/Background");
				//Check this file actually contains OpenBVE light definition nodes
				if (DocumentNodes != null)
				{
					foreach (XmlNode n in DocumentNodes)
					{
						if (n.ChildNodes.OfType<XmlElement>().Any())
						{
							double DisplayTime = -1;
							//The time to transition between backgrounds in seconds
							double TransitionTime = 0.8;
							double FogDistance = Plugin.CurrentOptions.ViewingDistance;
							//The texture to use (if static)
							Texture t = null;
							//The object to use (if object based)
							StaticObject o = null;
							//The transition mode between backgrounds
							BackgroundTransitionMode mode = BackgroundTransitionMode.FadeIn;
							//The number of times the texture is repeated around the viewing frustrum (if appropriate)
							double repetitions = 6;
							foreach (XmlNode c in n.ChildNodes)
							{
								
								string[] Arguments = c.InnerText.Split(',');
								switch (c.Name.ToLowerInvariant())
								{
									case "mode":
										switch (c.InnerText.ToLowerInvariant())
										{
											case "fadein":
												mode = BackgroundTransitionMode.FadeIn;
												break;
											case "fadeout":
												mode = BackgroundTransitionMode.FadeOut;
												break;
											case "none":
												mode = BackgroundTransitionMode.None;
												break;
											default:
												Plugin.CurrentHost.AddMessage(MessageType.Error, true, c.InnerText +  "is not a valid background fade mode in file " + fileName);
												break;
										}
										break;
									case "object":
										string f;
										try
										{
											f = OpenBveApi.Path.CombineFile(Path.GetDirectoryName(fileName), c.InnerText);
										}
										catch
										{
											Plugin.CurrentHost.AddMessage(MessageType.Error, true, "BackgroundObject FileName is malformed in file " + fileName);
											break;
										}
										if (!File.Exists(f))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Error, true, "FileName " + f + " not found in file " + fileName);
										}
										else
										{
											Plugin.CurrentHost.LoadObject(f, System.Text.Encoding.Default, out UnifiedObject obj);
											o = (StaticObject) obj;
										}
										break;
									case "repetitions":
										if (!NumberFormats.TryParseDoubleVb6(Arguments[0], UnitOfLength, out repetitions))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Error, false, c.InnerText + " does not parse to a valid number of repetitions in " + fileName);
										}
										break;
									case "texture":
										string file;
										try
										{
											file = OpenBveApi.Path.CombineFile(filePath, c.InnerText);
										}
										catch
										{
											Plugin.CurrentHost.AddMessage(MessageType.Error, true, "BackgroundTexture FileName is malformed in file " + fileName);
											break;
										}									
										if (!File.Exists(file))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Error, false, "The background texture file " + c.InnerText + " does not exist in " + fileName);
										}
										else
										{
											Plugin.CurrentHost.RegisterTexture(file, TextureParameters.NoChange, out t);
										}
										break;
									case "time":
										if (!Parser.TryParseTime(Arguments[0].Trim(), out DisplayTime))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Error, false, c.InnerText + " does not parse to a valid time in file " + fileName);
										}
										break;
									case "transitiontime":
										if (!NumberFormats.TryParseDoubleVb6(Arguments[0], UnitOfLength, out TransitionTime))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Error, false, c.InnerText + " is not a valid background transition time in " + fileName);
										}
										break;
									case "fogdistance":
										if (!NumberFormats.TryParseDoubleVb6(Arguments[0], UnitOfLength, out FogDistance))
										{
											Plugin.CurrentHost.AddMessage(MessageType.Error, false, c.InnerText + " is not a valid background fog distance in " + fileName);
										}
										break;
								}
							}
							//Create background if texture is not null
							if (t != null && o == null)
							{
								Backgrounds.Add(new StaticBackground(t, repetitions, false, TransitionTime, mode, DisplayTime));
								Backgrounds[Backgrounds.Count - 1].FogDistance = FogDistance;
								Backgrounds[Backgrounds.Count - 1].BackgroundImageDistance = Plugin.CurrentOptions.ViewingDistance;
							}
							if (t == null && o != null)
							{
								//All other parameters are ignored if an object has been defined
								//TODO: Error message stating they have been ignored
								BackgroundObject bo = new BackgroundObject(o, Plugin.CurrentOptions.ViewingDistance);
								bo.FogDistance = FogDistance;
								return bo;
							}
							
						}
					}
					if (Backgrounds.Count == 1)
					{
						return Backgrounds[0];
					}
					if (Backgrounds.Count > 1)
					{
						//Sort list- Not worried about when they start or end, so use simple LINQ
						Backgrounds = Backgrounds.OrderBy(o => o.Time).ToList();
						//If more than 2 backgrounds, convert to array and return a new dynamic background
						return new DynamicBackground(Backgrounds.ToArray());
					}
				}
			}
			//We couldn't find any valid XML, so return false
			throw new InvalidDataException();
		}
	}
}
