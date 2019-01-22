using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Textures;

namespace OpenBve
{
	class DynamicBackgroundParser
	{
		//Parses an XML background definition
		public static BackgroundManager.BackgroundHandle ReadBackgroundXML(string fileName)
		{
			List<BackgroundManager.StaticBackground> Backgrounds = new List<BackgroundManager.StaticBackground>();
			//The current XML file to load
			XmlDocument currentXML = new XmlDocument();
			//Load the object's XML file 
			currentXML.Load(fileName);
			string Path = System.IO.Path.GetDirectoryName(fileName);
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
							//The texture to use (if static)
							Texture t = null;
							//The object to use (if object based)
							ObjectManager.StaticObject o = null;
							//The transition mode between backgrounds
							BackgroundManager.BackgroundTransitionMode mode = BackgroundManager.BackgroundTransitionMode.FadeIn;
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
												mode = BackgroundManager.BackgroundTransitionMode.FadeIn;
												break;
											case "fadeout":
												mode = BackgroundManager.BackgroundTransitionMode.FadeOut;
												break;
											case "none":
												mode = BackgroundManager.BackgroundTransitionMode.None;
												break;
											default:
												Interface.AddMessage(MessageType.Error, true, c.InnerText +  "is not a valid background fade mode in file " + fileName);
												break;
										}
										break;
									case "object":
										string f;
										try
										{
											f = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(fileName), c.InnerText);
										}
										catch
										{
											Interface.AddMessage(MessageType.Error, true, "BackgroundObject FileName is malformed in file " + fileName);
											break;
										}
										if (!System.IO.File.Exists(f))
										{
											Interface.AddMessage(MessageType.Error, true, "FileName " + f + " not found in file " + fileName);
										}
										else
										{
											UnifiedObject b = ObjectManager.LoadObject(f, System.Text.Encoding.Default, false, false, false);
											o = (ObjectManager.StaticObject) b;
										}
										break;
									case "repetitions":
										if (!NumberFormats.TryParseDoubleVb6(Arguments[0], UnitOfLength, out repetitions))
										{
											Interface.AddMessage(MessageType.Error, false, c.InnerText + " does not parse to a valid number of repetitions in " + fileName);
										}
										break;
									case "texture":
										string file;
										try
										{
											file = OpenBveApi.Path.CombineFile(Path, c.InnerText);
										}
										catch
										{
											Interface.AddMessage(MessageType.Error, true, "BackgroundTexture FileName is malformed in file " + fileName);
											break;
										}									
										if (!System.IO.File.Exists(file))
										{
											Interface.AddMessage(MessageType.Error, false, "The background texture file " + c.InnerText + " does not exist in " + fileName);
										}
										else
										{
											Textures.RegisterTexture(file, out t);
										}
										break;
									case "time":
										if (!Interface.TryParseTime(Arguments[0].Trim(), out DisplayTime))
										{
											Interface.AddMessage(MessageType.Error, false, c.InnerText + " does not parse to a valid time in file " + fileName);
										}
										break;
									case "transitiontime":
										if (!NumberFormats.TryParseDoubleVb6(Arguments[0], UnitOfLength, out TransitionTime))
										{
											Interface.AddMessage(MessageType.Error, false, c.InnerText + " is not a valid background transition time in " + fileName);
										}
										break;
								}
							}
							//Create background if texture is not null
							if (t != null && o == null)
							{
								Backgrounds.Add(new BackgroundManager.StaticBackground(t, repetitions, false, TransitionTime, mode, DisplayTime));
							}
							if (t == null && o != null)
							{
								//All other parameters are ignored if an object has been defined
								//TODO: Error message stating they have been ignored
								return new BackgroundManager.BackgroundObject(o);
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
						return new BackgroundManager.DynamicBackground(Backgrounds.ToArray());
					}
				}
			}
			//We couldn't find any valid XML, so return false
			throw new InvalidDataException();
		}
	}
}
