using System;
using System.Xml;
using OpenBveApi.Colors;
using OpenBveApi.Math;
using System.Linq;
using OpenBveApi.Interface;
using OpenBveApi.Routes;

namespace CsvRwRouteParser
{
	internal class DynamicLightParser
	{
		//Parses an XML dynamic lighting definition
		public static bool ReadLightingXML(string fileName, out LightDefinition[] LightDefinitions)
		{
			//Prep
			LightDefinitions = new LightDefinition[0];
			//The current XML file to load
			XmlDocument currentXML = new XmlDocument();
			//Load the object's XML file 
			try
			{
				currentXML.Load(fileName);
			}
			catch
			{
				return false;
			}
			
			bool defined = false;
			//Check for null
			if (currentXML.DocumentElement != null)
			{
				XmlNodeList DocumentNodes = currentXML.DocumentElement.SelectNodes("/openBVE/Brightness");
				//Check this file actually contains OpenBVE light definition nodes
				if (DocumentNodes != null)
				{
					foreach (XmlNode n in DocumentNodes)
					{
						LightDefinition currentLight = new LightDefinition();
						if (n.ChildNodes.OfType<XmlElement>().Any())
						{
							bool tf = false, al = false, dl = false, ld = false, cb = false;
							string ts = null;
							foreach (XmlNode c in n.ChildNodes)
							{
								string[] Arguments = c.InnerText.Split(',');
								switch (c.Name.ToLowerInvariant())
								{
									case "cablighting":
										if (NumberFormats.TryParseDoubleVb6(Arguments[0].Trim(), out var b))
										{
											cb = true;
										}
										if (b > 255 || b < 0)
										{
											Plugin.CurrentHost.AddMessage(MessageType.Error, false, c.InnerText + " is not a valid brightness value in file " + fileName);
											currentLight.CabBrightness = 255;
											break;
										}
										currentLight.CabBrightness = b;
										break;
									case "time":
										if (Parser.TryParseTime(Arguments[0].Trim(), out var t))
										{
											currentLight.Time = (int)t;
											tf = true;
											//Keep back for error report later
											ts = Arguments[0];
										}
										else
										{
											Plugin.CurrentHost.AddMessage(MessageType.Error, false, c.InnerText + " does not parse to a valid time in file " + fileName);
										}
										break;
									case "ambientlight":
										switch (Arguments.Length)
										{
											case 1:
												if (Color24.TryParseHexColor(Arguments[0], out currentLight.AmbientColor))
												{
													al = true;
													break;
												}
												Plugin.CurrentHost.AddMessage(MessageType.Error, false, c.InnerText + " does not parse to a valid color in file " + fileName);
												break;
											case 3:
												if (NumberFormats.TryParseDoubleVb6(Arguments[0].Trim(), out var R) && NumberFormats.TryParseDoubleVb6(Arguments[1].Trim(), out var G) && NumberFormats.TryParseDoubleVb6(Arguments[2].Trim(), out var B))
												{
													currentLight.AmbientColor = new Color24((byte)R,(byte)G,(byte)B);
													al = true;
													break;
												}
												Plugin.CurrentHost.AddMessage(MessageType.Error, false, c.InnerText + " does not parse to a valid color in file " + fileName);
												break;
											default:
												Plugin.CurrentHost.AddMessage(MessageType.Error, false, c.InnerText + " must be either a hexadecimal or RGB color in file  " + fileName);
												break;
										}
										break;
									case "directionallight":
										switch (Arguments.Length)
										{
											case 1:
												if (Color24.TryParseHexColor(Arguments[0], out currentLight.DiffuseColor))
												{
													dl = true;
													break;
												}
												Plugin.CurrentHost.AddMessage(MessageType.Error, false, c.InnerText + " does not parse to a valid color in file " + fileName);
												break;
											case 3:
												if (NumberFormats.TryParseDoubleVb6(Arguments[0].Trim(), out var R) && NumberFormats.TryParseDoubleVb6(Arguments[1].Trim(), out var G) && NumberFormats.TryParseDoubleVb6(Arguments[2].Trim(), out var B))
												{
													currentLight.DiffuseColor = new Color24((byte)R,(byte)G,(byte)B);
													dl = true;
													break;
												}
												Plugin.CurrentHost.AddMessage(MessageType.Error, false, c.InnerText + " does not parse to a valid color in file " + fileName);
												break;
											default:
												Plugin.CurrentHost.AddMessage(MessageType.Error, false, c.InnerText + " must be either a hexadecimal or RGB color in file  " + fileName);
												break;
										}
										break;
									case "cartesianlightdirection":
									case "lightdirection":
										if (Arguments.Length == 3)
										{
											if (NumberFormats.TryParseDoubleVb6(Arguments[0].Trim(), out var X) && NumberFormats.TryParseDoubleVb6(Arguments[1].Trim(), out var Y) && NumberFormats.TryParseDoubleVb6(Arguments[2].Trim(), out var Z))
											{
												currentLight.LightPosition = new Vector3(X, Y, Z);
												ld = true;
												break;
											}
											Plugin.CurrentHost.AddMessage(MessageType.Error, false, c.InnerText + " does not parse to a valid direction in file " + fileName);
											break;
										}
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, c.InnerText + " does not contain three arguments in file " + fileName);
										break;
									case "sphericallightdirection":
										if (Arguments.Length == 2)
										{
											if (NumberFormats.TryParseDoubleVb6(Arguments[0].Trim(), out var theta) && NumberFormats.TryParseDoubleVb6(Arguments[1].Trim(), out var phi))
											{
												currentLight.LightPosition = new Vector3(Math.Cos(theta) * Math.Sin(phi), -Math.Sin(theta), Math.Cos(theta) * Math.Cos(phi));
												ld = true;
												break;
											}
											Plugin.CurrentHost.AddMessage(MessageType.Error, false, c.InnerText + " does not parse to a valid direction in file " + fileName);
											break;
										}
										Plugin.CurrentHost.AddMessage(MessageType.Error, false, c.InnerText + " does not contain two arguments in file " + fileName);
										break;
								}
							}
							//We want to be able to add a completely default light element,  but not one that's not been defined in the XML properly
							if (tf || al || ld || dl || cb)
							{
								//HACK: No way to break out of the first loop and continue with the second, so we've got to use a variable
								bool Break = false;
								int l = LightDefinitions.Length;
								for (int i = 0; i < l; i++)
								{
									if (LightDefinitions[i].Time == currentLight.Time)
									{
										Break = true;
										if (ts == null)
										{
											Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Multiple undefined times were encountered in file " + fileName);
										}
										else
										{
											Plugin.CurrentHost.AddMessage(MessageType.Error, false, "Duplicate time found: " + ts + " in file " + fileName);
										}
										break;
									}
								}
								if (Break)
								{
									continue;
								}
								//We've got there, so now figure out where to add the new light into our list of light definitions
								int t = 0;
								if (l == 1)
								{
									t = currentLight.Time > LightDefinitions[0].Time ? 1 : 0;
								}
								else if (l > 1)
								{
									for (int i = 1; i < l; i++)
									{
										t = i + 1;
										if (currentLight.Time > LightDefinitions[i - 1].Time && currentLight.Time < LightDefinitions[i].Time)
										{
											break;
										}
									}
								}
								//Resize array
								defined = true;
								Array.Resize(ref LightDefinitions, l + 1);
								if (t == l)
								{
									//Straight insert at the end of the array
									LightDefinitions[l] = currentLight;
								}
								else
								{
									for (int u = t; u < l; u++)
									{
										//Otherwise, shift all elements to compensate
										LightDefinitions[u + 1] = LightDefinitions[u];
									}
									LightDefinitions[t] = currentLight;
								}
								
							}
						}
					}
				}
			}
			//We couldn't find any valid XML, so return false
			return defined;
		}
	}
}
