using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Objects;
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
		public CompatibilitySignalObject(int[] aspectNumbers, StaticObject[] Objects)
		{
			this.AspectNumbers = aspectNumbers;
			this.Objects = Objects;
		}

		/// <summary>Loads a list of compatibility signal objects</summary>
		/// <param name="currentHost">The host application interface</param>
		/// <param name="fileName">The file name of the object list</param>
		/// <param name="signalPost">Sets the default signal post</param>
		/// <returns>An array of compatability signal objects</returns>
		public static CompatibilitySignalObject[] ReadCompatibilitySignalXML(HostInterface currentHost, string fileName, out UnifiedObject signalPost)
		{
			//TODO: It should be possible to add default limits for these too to overwrite the Japanese ones
			signalPost = new StaticObject(currentHost);
			CompatibilitySignalObject[] objects = new CompatibilitySignalObject[9];
			XmlDocument currentXML = new XmlDocument();
			currentXML.Load(fileName);
			string currentPath = System.IO.Path.GetDirectoryName(fileName);
			if (currentXML.DocumentElement != null)
			{
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

									int aspect = 0;
									if (n.Attributes != null)
									{
										int.TryParse(n.Attributes["Number"].Value, out aspect);
									}

									aspectList.Add(aspect);
									string objectFile = Path.CombineFile(currentPath, n.InnerText);
									StaticObject staticObject = new StaticObject(currentHost);
									if (File.Exists(objectFile))
									{
										currentHost.LoadStaticObject(objectFile, Encoding.UTF8, false, out staticObject);
									}
									else
									{
										currentHost.AddMessage(MessageType.Error, true, "Compatibility signal file " + objectFile + " not found in " + fileName);
									}

									objectList.Add(staticObject);
								}
							}
						}
						catch
						{
							currentHost.AddMessage(MessageType.Error, true, "An unexpected error was encountered whilst processing the compatability signal list in " + fileName);
						}
						objects[index] = new CompatibilitySignalObject(aspectList.ToArray(), objectList.ToArray());
						index++;
					}
				}
				
				string signalPostFile = Path.CombineFile(currentPath, "Japanese\\signal_post.csv"); //default plain post
				try
				{
					XmlNode node = currentXML.SelectSingleNode("/openBVE/CompatibilitySignals/SignalPost");
					if (node != null)
					{
						string newFile = Path.CombineFile(currentPath, node.InnerText);
						if (System.IO.File.Exists(newFile))
						{
							signalPostFile = newFile;
						}
					}
					currentHost.LoadObject(signalPostFile, Encoding.UTF8, out signalPost);
				}
				catch
				{
					currentHost.AddMessage(MessageType.Error, true, "An unexpected error was encountered whilst processing the compatability signal list in " + fileName);
				}
			}
			return objects;
		}
	}
}
