using System;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;

namespace OpenBve
{
	/// <summary>The ObjectManager is the root class containing functions to load and manage objects within the simulation world</summary>
	public static partial class ObjectManager
	{
		/// <summary>Loads an object</summary>
		/// <param name="FileName">The file to load</param>
		/// <param name="Encoding">The text endcoding of the base file (Used if the encoding cannot be auto-detected)</param>
		/// <param name="LoadMode"></param>
		/// <param name="PreserveVertices">Whether object should be optimized to remove duplicate vertices</param>
		/// <param name="ForceTextureRepeatX">Whether texture repeat is forced on the X-axis</param>
		/// <param name="ForceTextureRepeatY">Whether texture repeat is forced on the Y-axis</param>
		/// <returns>The new object, or a null reference if loading fails</returns>
		/*
		 * Notes for refactoring:
		 *   * Unused vertices must only be preserved in deformable objects (e.g. Crack and Form)
		 *   * ForceTextureRepeatX / Y is only used for animated objects using the TextureShiftFunction
		 *   * TODO / BUG: ForceTextureRepeat is only supported by the B3D / CSV parser
		 *   * TODO / BUG: No detection of actual file contents, which will make all parsers barf
		 */
		internal static UnifiedObject LoadObject(string FileName, System.Text.Encoding Encoding, ObjectLoadMode LoadMode, bool PreserveVertices, bool ForceTextureRepeatX, bool ForceTextureRepeatY)
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
					var f = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), System.IO.Path.GetFileName(FileName) + ".x");
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
						break;
					}
					f = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), System.IO.Path.GetFileName(FileName) + ".xml");
					if (System.IO.File.Exists(f))
					{
						FileName = f;
						break;
					}
					f = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), System.IO.Path.GetFileName(FileName) + ".l3dgrp");
					if (System.IO.File.Exists(f))
					{
						FileName = f;
						break;
					}
					f = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), System.IO.Path.GetFileName(FileName) + ".l3dobj");
					if (System.IO.File.Exists(f))
					{
						FileName = f;
						break;
					}
				}
			}
			UnifiedObject Result;
			TextEncoding.Encoding newEncoding = TextEncoding.GetEncodingFromFile(FileName);
			if (newEncoding != TextEncoding.Encoding.Unknown)
			{
				switch (newEncoding)
				{
					case TextEncoding.Encoding.Utf7:
						Encoding = System.Text.Encoding.UTF7;
						break;
					case TextEncoding.Encoding.Utf8:
						Encoding = System.Text.Encoding.UTF8;
						break;
					case TextEncoding.Encoding.Utf16Le:
						Encoding = System.Text.Encoding.Unicode;
						break;
					case TextEncoding.Encoding.Utf16Be:
						Encoding = System.Text.Encoding.BigEndianUnicode;
						break;
					case TextEncoding.Encoding.Utf32Le:
						Encoding = System.Text.Encoding.UTF32;
						break;
					case TextEncoding.Encoding.Utf32Be:
						Encoding = System.Text.Encoding.GetEncoding(12001);
						break;
					case TextEncoding.Encoding.Shift_JIS:
						Encoding = System.Text.Encoding.GetEncoding(932);
						break;
					case TextEncoding.Encoding.Windows1252:
						Encoding = System.Text.Encoding.GetEncoding(1252);
						break;
					case TextEncoding.Encoding.Big5:
						Encoding = System.Text.Encoding.GetEncoding(950);
						break;
					case TextEncoding.Encoding.EUC_KR:
						Encoding = System.Text.Encoding.GetEncoding(949);
						break;
				}
			}
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
					Result = CsvB3dObjectParser.ReadObject(FileName, Encoding, LoadMode, ForceTextureRepeatX, ForceTextureRepeatY);
					break;
				case ".x":
					if (Interface.CurrentOptions.CurrentXParser != Interface.XParsers.Original)
					{
						try
						{
							if (Interface.CurrentOptions.CurrentXParser == Interface.XParsers.NewXParser)
							{
								Result = NewXParser.ReadObject(FileName, Encoding, LoadMode);
							}
							else
							{
								Result = AssimpXParser.ReadObject(FileName);
							}
						}
						catch (Exception ex)
						{
							Interface.AddMessage(MessageType.Error, false, "The new X parser raised the following exception: " + ex);
							Result = XObjectParser.ReadObject(FileName, Encoding, LoadMode);
						}
					}
					else
					{
						Result = XObjectParser.ReadObject(FileName, Encoding, LoadMode);
					}
					break;
				case ".animated":
					Result = AnimatedObjectParser.ReadObject(FileName, Encoding, LoadMode);
					break;
				case ".xml":
					Result = XMLParser.ReadObject(FileName, Encoding, LoadMode, ForceTextureRepeatX, ForceTextureRepeatY);
					break;
				case ".l3dgrp":
					Result = Ls3DGrpParser.ReadObject(FileName, Encoding, LoadMode, new Vector3());
					break;
				case ".l3dobj":
					Result = Ls3DObjectParser.ReadObject(FileName, LoadMode, new Vector3());
					break;
				case ".obj":
					Result = WavefrontObjParser.ReadObject(FileName, Encoding, LoadMode, ForceTextureRepeatX, ForceTextureRepeatY);
					break;
				case ".s":
					Result = MsTsShapeParser.ReadObject(FileName);
					break;
				default:
					Interface.AddMessage(MessageType.Error, false, "The file extension is not supported: " + FileName);
					return null;
			}
			if (Result != null)
			{
				Result.OptimizeObject(PreserveVertices);
			}
			return Result;
#if !DEBUG
			} catch (Exception ex) {
				Interface.AddMessage(MessageType.Error, true, "An unexpected error occured (" + ex.Message + ") while attempting to load the file " + FileName);
				return null;
			}
#endif
		}
		internal static StaticObject LoadStaticObject(string FileName, System.Text.Encoding Encoding, ObjectLoadMode LoadMode, bool PreserveVertices, bool ForceTextureRepeatX, bool ForceTextureRepeatY)
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
			StaticObject Result;
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
					Result = CsvB3dObjectParser.ReadObject(FileName, Encoding, LoadMode, ForceTextureRepeatX, ForceTextureRepeatY);
					break;
				case ".x":
					if (Interface.CurrentOptions.CurrentXParser != Interface.XParsers.Original)
					{
						try
						{
							if (Interface.CurrentOptions.CurrentXParser == Interface.XParsers.NewXParser)
							{
								Result = NewXParser.ReadObject(FileName, Encoding, LoadMode);
							}
							else
							{
								Result = AssimpXParser.ReadObject(FileName);
							}
						}
						catch (Exception ex)
						{
							Interface.AddMessage(MessageType.Error, false, "The new X parser raised the following exception: " + ex);
							Result = XObjectParser.ReadObject(FileName, Encoding, LoadMode);
						}
					}
					else
					{
						Result = XObjectParser.ReadObject(FileName, Encoding, LoadMode);
					}
					break;
				case ".animated":
				case ".s":
					Interface.AddMessage(MessageType.Error, false, "Tried to load an animated object even though only static objects are allowed: " + FileName);
					return null;
				case ".obj":
					Result = WavefrontObjParser.ReadObject(FileName, Encoding, LoadMode, ForceTextureRepeatX, ForceTextureRepeatY);
					break;
				/*
				 * This will require implementing a specific static object load function- Leave alone for the moment
				 * 
			case ".xml":
				Result = XMLParser.ReadObject(FileName, Encoding, LoadMode, ForceTextureRepeatX, ForceTextureRepeatY);
				break;
				 */
				default:
					Interface.AddMessage(MessageType.Error, false, "The file extension is not supported: " + FileName);
					return null;
			}
			if (Result != null)
			{
				Result.OptimizeObject(PreserveVertices);
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
