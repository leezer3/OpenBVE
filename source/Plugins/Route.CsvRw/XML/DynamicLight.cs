using Formats.OpenBve;
using Formats.OpenBve.XML;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Routes;
using System;
using System.Xml;

namespace CsvRwRouteParser
{
	internal class DynamicLightParser
	{
		//Parses an XML dynamic lighting definition
		public static bool ReadLightingXML(string fileName, out LightDefinition[] LightDefinitions)
		{
			//Prep
			LightDefinitions = new LightDefinition[0];
		
			XMLFile<DynamicLightSection, DynamicLightKey> xmlFile = new XMLFile<DynamicLightSection, DynamicLightKey>(fileName, "/openBVE", Plugin.CurrentHost);
			bool defined = false;
			while(xmlFile.RemainingSubBlocks > 0)
			{
				Block<DynamicLightSection, DynamicLightKey> lightBlock = xmlFile.ReadNextBlock();
				LightDefinition currentLight = new LightDefinition();
				bool tf = false, al = false, dl = false, ld = false, cb = false;
				double ts = double.MaxValue;
				if (lightBlock.GetValue(DynamicLightKey.CabLighting, out double b))
				{
					if (b > 255 || b < 0)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, b + " is not a valid brightness value in file " + fileName);
						currentLight.CabBrightness = 255;
					}
					else
					{
						currentLight.CabBrightness = b;
						cb = true;
					}
				}

				if(lightBlock.TryGetTime(DynamicLightKey.Time, ref currentLight.Time))
				{
					ts = currentLight.Time;
					tf = true;
				}
				
				al = lightBlock.TryGetColor24(DynamicLightKey.AmbientLight, ref currentLight.AmbientColor);
				dl = lightBlock.TryGetColor24(DynamicLightKey.DirectionalLight, ref currentLight.DiffuseColor);
				ld = lightBlock.TryGetVector3(DynamicLightKey.LightDirection, ',', ref currentLight.LightPosition);
				if (!ld && lightBlock.GetVector2(DynamicLightKey.SphericalLightDirection, ',', out Vector2 temp))
				{
					currentLight.LightPosition = new Vector3(Math.Cos(temp.X) * Math.Sin(temp.Y), -Math.Sin(temp.X), Math.Cos(temp.X) * Math.Cos(temp.Y));
					ld = true;
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
							if (ts != double.MaxValue)
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
			//We couldn't find any valid XML, so return false
			return defined;
		}
	}
}
