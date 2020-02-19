using System;
using System.Text;
using OpenBveApi;
using OpenBveApi.Interface;
using OpenBveApi.Objects;

namespace OpenBve
{
	/// <summary>The ObjectManager is the root class containing functions to load and manage objects within the simulation world</summary>
	public static partial class ObjectManager
	{
		internal static StaticObject LoadStaticObject(string FileName, Encoding Encoding, bool PreserveVertices)
		{
			if (String.IsNullOrEmpty(FileName))
			{
				return null;
			}
#if !DEBUG
			try {
#endif
			if (!System.IO.Path.HasExtension(FileName))
			{
				while (true)
				{
					string f = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), System.IO.Path.GetFileName(FileName) + ".x");
					if (System.IO.File.Exists(f))
					{
						FileName = f;
						break;
					}
					f = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), System.IO.Path.GetFileName(FileName) + ".csv");
					if (System.IO.File.Exists(f))
					{
						FileName = f;
						break;
					}
					f = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), System.IO.Path.GetFileName(FileName) + ".b3d");
					if (System.IO.File.Exists(f))
					{
						FileName = f;
					}
					break;
				}
			}
			StaticObject Result = null;
			string e = System.IO.Path.GetExtension(FileName);
			if (e == null)
			{
				Interface.AddMessage(MessageType.Error, false, "The file " + FileName + " does not have a recognised extension.");
				return null;
			}
			switch (e.ToLowerInvariant())
			{
				case ".csv":
				case ".b3d":
				case ".x":
				case ".obj":
				case ".l3dgrp":
				case ".l3dobj":
				case ".animated":
				case ".s":
					UnifiedObject obj;
					Program.CurrentHost.LoadObject(FileName, Encoding, out obj);
					if (obj is AnimatedObjectCollection)
					{
						Interface.AddMessage(MessageType.Error, false, "Tried to load an animated object even though only static objects are allowed: " + FileName);
						return null;
					}
					Result = (StaticObject)obj;
					break;
				/*
				 * This will require implementing a specific static object load function- Leave alone for the moment
				 * 
			case ".xml":
				Result = XMLParser.ReadObject(FileName, Encoding, ForceTextureRepeatX, ForceTextureRepeatY);
				break;
				 */
				default:
					Interface.AddMessage(MessageType.Error, false, "The file extension is not supported: " + FileName);
					return null;
			}
			if (Result != null)
			{
				Result.OptimizeObject(PreserveVertices, Interface.CurrentOptions.ObjectOptimizationBasicThreshold, Interface.CurrentOptions.ObjectOptimizationVertexCulling);
			}
			return Result;
#if !DEBUG
			} catch (Exception ex) {
				Interface.AddMessage(MessageType.Error, true, "An unexpected error occured (" + ex.Message + ") while attempting to load the file " + FileName);
				return null;
			}
#endif
		}
	}
}
