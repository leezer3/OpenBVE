using Formats.OpenBve;
using Formats.OpenBve.XML;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using OpenBveApi.Textures;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CsvRwRouteParser
{
	internal class DynamicBackgroundParser
	{
		//Parses an XML background definition
		public static BackgroundHandle ReadBackgroundXML(string fileName)
		{
			List<StaticBackground> Backgrounds = new List<StaticBackground>();
			XMLFile<DynamicBackgroundSection, DynamicBackgroundKey> xmlFile = new XMLFile<DynamicBackgroundSection, DynamicBackgroundKey>(fileName, "/openBVE", Plugin.CurrentHost);
			while (xmlFile.RemainingSubBlocks > 0)
			{
				Block<DynamicBackgroundSection, DynamicBackgroundKey> backgroundBlock = xmlFile.ReadNextBlock();
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
				//The number of times the texture is repeated around the viewing frustum (if appropriate)
				double repetitions = 6;

				backgroundBlock.TryGetEnumValue(DynamicBackgroundKey.Mode, ref mode);
				if (backgroundBlock.GetPath(DynamicBackgroundKey.Object, Path.GetDirectoryName(fileName), out string objectPath))
				{
					Plugin.CurrentHost.LoadObject(objectPath, System.Text.Encoding.Default, out UnifiedObject obj);
					o = (StaticObject)obj;
				}
				else if (backgroundBlock.GetPath(DynamicBackgroundKey.Texture, Path.GetDirectoryName(fileName), out string texturePath))
				{
					Plugin.CurrentHost.RegisterTexture(texturePath, TextureParameters.NoChange, out t);
					backgroundBlock.TryGetValue(DynamicBackgroundKey.Repetitions, ref repetitions);
				}

				backgroundBlock.TryGetTime(DynamicBackgroundKey.Time, ref DisplayTime);
				backgroundBlock.TryGetValue(DynamicBackgroundKey.TransitionTime, ref TransitionTime);
				backgroundBlock.TryGetValue(DynamicBackgroundKey.FogDistance, ref FogDistance);
				
				if (t != null)
				{
					Backgrounds.Add(new StaticBackground(t, repetitions, false, TransitionTime, mode, DisplayTime));
					Backgrounds[Backgrounds.Count - 1].FogDistance = FogDistance;
					Backgrounds[Backgrounds.Count - 1].BackgroundImageDistance = Plugin.CurrentOptions.ViewingDistance;
				}
				else if (o != null)
				{
					BackgroundObject bo = new BackgroundObject(o, Plugin.CurrentOptions.ViewingDistance);
					bo.FogDistance = FogDistance;
					return bo;
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
			
			//We couldn't find any valid XML, so return false
			throw new InvalidDataException();
		}
	}
}
