//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2021, Christopher Lees, The OpenBVE Project
//
//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met:
//
//1. Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
//ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using OpenBveApi.Colors;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Textures;
using Path = OpenBveApi.Path;

namespace Formats.OpenBve
{
    /// <summary>An abstract block of data</summary>
	public abstract class Block
    {
		/// <summary>The host application</summary>
		internal readonly HostInterface currentHost;

		/// <summary>The filename the data was loaded from</summary>
		internal string FileName;

		/// <summary>The token that identifies the contents of this block</summary>
		public object Token;

		/// <summary>The label of this block</summary>
		/// <remarks>Normally blank</remarks>
		public string Label;

		/// <summary>Reads an signed 32-bit integer from the block</summary>
		/// <param name="Key">The key of the KVP pair</param>
		/// <param name="defaultValue">An optional non-standard default value</param>
		public abstract int ReadInt<T>(T Key, int defaultValue = 0) where T : struct, Enum;
		
		/// <summary>Reads a double-bit precision floating point number from the block</summary>
		/// <param name="Key">The key of the KVP pair</param>
		/// <param name="defaultValue">An optional non-standard default value</param>
		public abstract double ReadDouble<T>(T Key, double defaultValue = 0) where T : struct, Enum;

		/// <summary>Reads a string from the block</summary>
		/// <param name="Key">The key of the KVP pair</param>
		/// <param name="defaultValue">An optional non-standard default value</param>
		public abstract string ReadString<T>(T Key, string defaultValue = "") where T : struct, Enum;

		/// <summary>Reads a 2D vector from the block</summary>
		/// <param name="Key">The key of the KVP pair</param>
		/// <param name="defaultValue">An optional non-standard default value</param>
		public abstract Vector2 ReadVector2<T>(T Key, Vector2 defaultValue = new Vector2()) where T : struct, Enum;

		/// <summary>Reads a 3D vector from the block</summary>
		/// <param name="Key">The key of the KVP pair</param>
		/// <param name="defaultValue">An optional non-standard default value</param>
		public abstract Vector3 ReadVector3<T>(T Key, Vector3 defaultValue = new Vector3()) where T : struct, Enum;

		/// <summary>Reads a 24-bit color from the block</summary>
		/// <param name="Key">The key of the KVP pair</param>
		/// <param name="defaultValue">An optional non-standard default value</param>
		public abstract Color24 ReadColor24<T>(T Key, Color24? defaultValue = null) where T : struct, Enum;

		/// <summary>Reads a 32-bit color from the block</summary>
		/// <param name="Key">The key of the KVP pair</param>
		public abstract Color32 ReadColor32<T>(T Key) where T : struct, Enum;

		/// <summary>Reads a string from the block</summary>
		/// <param name="Key">The key of the KVP pair</param>
		/// <param name="texturePath">The texture search path</param>
		/// <param name="textureParameters">The texture parameters to apply</param>
		/// <param name="loadTexture">Whether the texture should be preloaded</param>
		public abstract Texture LoadTexture<T>(T Key, string texturePath, TextureParameters textureParameters, bool loadTexture = false) where T : struct, Enum;

		/// <summary>Creates a new block from the supplied XDocument</summary>
		/// <param name="fileName">The filename</param>
		/// <param name="document">The XDocument</param>
		/// <param name="token">The token for the new block</param>
		/// <returns>The new block</returns>
		/// <remarks>Always creates a XmlBlock</remarks>
		public Block ReadBlock<T>(string fileName, XElement document, T token) where T : struct, Enum
		{
			return new XmlBlock(currentHost, fileName, document, token);
		}

		/// <summary>Creates a new block from the supplied string</summary>
		/// <param name="text">The string</param>
		/// <param name="token">The token for the new block</param>
		/// <returns>The new block</returns>
		/// <remarks>Always creates a TextualBlock</remarks>
		public static Block ReadBlock<T>(string text, T token) where T : struct, Enum
		{
			throw new NotImplementedException();
			//return new TextualBlock(text, token);
		}

		/// <summary>Reads a sub-block from the enclosing block</summary>
		/// <param name="newToken">The expected token for the new block</param>
		/// <returns>The new block</returns>
		/// <remarks>The type of the new block will always match that of the base block</remarks>
		public abstract Block ReadSubBlock<T>(T newToken) where T : struct, Enum;

		public Block(HostInterface host)
		{
			currentHost = host;
		}
    }

    public class XmlBlock : Block
    {
	    private readonly XElement blockDocument;

		public XmlBlock(HostInterface host, string fileName, XElement document, object token) : base(host)
		{
			FileName = fileName;
		    blockDocument = document;
		    Token = token;
	    }


		public override int ReadInt<T>(T Key, int defaultValue = 0)
		{
			foreach(XElement e in blockDocument.Elements())
			{
				T parsedElement;
				Enum.TryParse(e.Name.LocalName, true, out parsedElement);
				if (parsedElement.Equals(Key))
				{
					if (!NumberFormats.TryParseIntVb6(e.Value, out var i))
					{
						currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + blockDocument.Name + " at line " + ((IXmlLineInfo)e).LineNumber + " in " + FileName);
						return defaultValue;
					}
					return i;
				}
			}
			return defaultValue;
		}

		public override double ReadDouble<T>(T Key, double defaultValue = 0)
		{
			foreach(XElement e in blockDocument.Elements())
			{
				T parsedElement;
				Enum.TryParse(e.Name.LocalName, true, out parsedElement);
				if (parsedElement.Equals(Key))
				{
					if (!NumberFormats.TryParseDoubleVb6(e.Value, out var d))
					{
						currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + blockDocument.Name + " at line " + ((IXmlLineInfo)e).LineNumber + " in " + FileName);
						return defaultValue;
					}
					return d;
				}
			}
			return defaultValue;
		}

		public override string ReadString<T>(T Key, string defaultValue = "")
	    {
		    foreach(XElement e in blockDocument.Elements())
		    {
			    T parsedElement;
			    Enum.TryParse(e.Name.LocalName, true, out parsedElement);
			    if (parsedElement.Equals(Key))
			    {
				    return e.Value;
			    }
		    }
		    return defaultValue;
	    }

		public override Vector2 ReadVector2<T>(T Key, Vector2 defaultValue = new Vector2())
		{
			foreach(XElement e in blockDocument.Elements())
			{
				T parsedElement;
				Enum.TryParse(e.Name.LocalName, true, out parsedElement);
				if (parsedElement.Equals(Key))
				{
					Vector2 newVector = new Vector2();
					string[] splitString = e.Value.Split(',');
					if (splitString.Length != 2)
					{
						currentHost.AddMessage(MessageType.Error, false, "Value should have exactly two arguments in " + Key + " in " + blockDocument.Name + " at line " + ((IXmlLineInfo)e).LineNumber + " in " + FileName);
					}
					for (int i = 0; i < splitString.Length; i++)
					{
						switch (i)
						{
							case 0:
								if (!NumberFormats.TryParseDoubleVb6(splitString[i], out newVector.X))
								{
									currentHost.AddMessage(MessageType.Error, false, "X is invalid in " + Key + " in " + blockDocument.Name + " at line " + ((IXmlLineInfo)e).LineNumber + " in " + FileName);
								}
								break;
							case 1:
								if (!NumberFormats.TryParseDoubleVb6(splitString[i], out newVector.Y))
								{
									currentHost.AddMessage(MessageType.Error, false, "Y is invalid in " + Key + " in " + blockDocument.Name + " at line " + ((IXmlLineInfo)e).LineNumber + " in " + FileName);
								}
								break;
						}
					}
					return newVector;
				}
			}
			return defaultValue;
		}

		public override Vector3 ReadVector3<T>(T Key, Vector3 defaultValue = new Vector3())
		{
			foreach(XElement e in blockDocument.Elements())
			{
				T parsedElement;
				Enum.TryParse(e.Name.LocalName, true, out parsedElement);
				
				if (parsedElement.Equals(Key))
				{
					Vector3 newVector = new Vector3();
					string[] splitString = e.Value.Split(',');
					if (splitString.Length != 3)
					{
						currentHost.AddMessage(MessageType.Error, false, "Value should have exactly three arguments in " + Key + " in " + blockDocument.Name + " at line " + ((IXmlLineInfo)e).LineNumber + " in " + FileName);
					}
					for (int i = 0; i < splitString.Length; i++)
					{
						switch (i)
						{
							case 0:
								if (!NumberFormats.TryParseDoubleVb6(splitString[i], out newVector.X))
								{
									currentHost.AddMessage(MessageType.Error, false, "X is invalid in " + Key + " in " + blockDocument.Name + " at line " + ((IXmlLineInfo)e).LineNumber + " in " + FileName);
								}
								break;
							case 1:
								if (!NumberFormats.TryParseDoubleVb6(splitString[i], out newVector.Y))
								{
									currentHost.AddMessage(MessageType.Error, false, "Y is invalid in " + Key + " in " + blockDocument.Name + " at line " + ((IXmlLineInfo)e).LineNumber + " in " + FileName);
								}
								break;
							case 2:
								if (!NumberFormats.TryParseDoubleVb6(splitString[i], out newVector.Z))
								{
									currentHost.AddMessage(MessageType.Error, false, "Z is invalid in " + Key + " in " + blockDocument.Name + " at line " + ((IXmlLineInfo)e).LineNumber + " in " + FileName);
								}
								break;
						}
					}
					return newVector;
				}
			}
			return defaultValue;
		}

		public override Color24 ReadColor24<T>(T Key, Color24? defaultValue = null)
		{
			Color24 parsedColor = Color24.White;
			if (defaultValue != null)
			{
				// HACK: We can't easily pass a struct as a default value, so use null instead
				// and set the default value if non-null
				parsedColor = (Color24)defaultValue;
			}
			foreach(XElement e in blockDocument.Elements())
			{
				T parsedElement;
				Enum.TryParse(e.Name.LocalName, true, out parsedElement);
				if (parsedElement.Equals(Key))
				{
					string[] splitString = e.Value.Split(',');
					if (splitString.Length > 1)
					{
						//Must be RGB / partial
						if (splitString.Length != 3)
						{
							currentHost.AddMessage(MessageType.Error, false, "Value should have exactly three arguments in " + Key + " in " + blockDocument.Name + " at line " + ((IXmlLineInfo)e).LineNumber + " in " + FileName);
						}

						int r = 0, g = 0, b = 0;
						for (int i = 0; i < splitString.Length; i++)
						{
							switch (i)
							{
								case 0:
									if (!NumberFormats.TryParseIntVb6(splitString[i], out r))
									{
										currentHost.AddMessage(MessageType.Error, false, "Red is invalid in " + Key + " in " + blockDocument.Name + " at line " + ((IXmlLineInfo)e).LineNumber + " in " + FileName);
									}
									else if (r < 0 | r > 255) 
									{
										currentHost.AddMessage(MessageType.Error, false, "Red is required to be within the range of 0 to 255 in " + Key + " in " + blockDocument.Name + " at line " + ((IXmlLineInfo)e).LineNumber + " in " + FileName);
										r = r < 0 ? 0 : 255;
									}
									break;
								case 1:
									if (!NumberFormats.TryParseIntVb6(splitString[i], out g))
									{
										currentHost.AddMessage(MessageType.Error, false, "Green is invalid in " + Key + " in " + blockDocument.Name + " at line " + ((IXmlLineInfo)e).LineNumber + " in " + FileName);
									}
									else if (g < 0 | g > 255) 
									{
										currentHost.AddMessage(MessageType.Error, false, "Green is required to be within the range of 0 to 255 in " + Key + " in " + blockDocument.Name + " at line " + ((IXmlLineInfo)e).LineNumber + " in " + FileName);
										g = g < 0 ? 0 : 255;
									}
									break;
								case 2:
									if (!NumberFormats.TryParseIntVb6(splitString[i], out b))
									{
										currentHost.AddMessage(MessageType.Error, false, "Blue is invalid in " + Key + " in " + blockDocument.Name + " at line " + ((IXmlLineInfo)e).LineNumber + " in " + FileName);
									}
									else if (b < 0 | b > 255) 
									{
										currentHost.AddMessage(MessageType.Error, false, "Blue is required to be within the range of 0 to 255 in " + Key + " in " + blockDocument.Name + " at line " + ((IXmlLineInfo)e).LineNumber + " in " + FileName);
										b = b < 0 ? 0 : 255;
									}
									break;
							}
						}
						parsedColor = new Color24((byte) r, (byte) g, (byte) b);
					}
					else
					{
						//Hex color
						if (!Color24.TryParseHexColor(e.Value, out parsedColor))
						{
							currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + blockDocument.Name + " at line " + ((IXmlLineInfo)e).LineNumber + " in " + FileName);
						}
					}
					return parsedColor;
				}
			}
			return parsedColor;
		}

		public override Color32 ReadColor32<T>(T Key)
		{
			Color32 parsedColor = Color32.White;
			foreach(XElement e in blockDocument.Elements())
			{
				T parsedElement;
				Enum.TryParse(e.Name.LocalName, true, out parsedElement);
				if (parsedElement.Equals(Key))
				{
					string[] splitString = e.Value.Split(',');
					if (splitString.Length > 1)
					{
						//Must be RGB / partial
						if (splitString.Length != 4)
						{
							currentHost.AddMessage(MessageType.Error, false, "Value should have exactly three arguments in " + Key + " in " + blockDocument.Name + " at line " + ((IXmlLineInfo)e).LineNumber + " in " + FileName);
						}

						int r = 0, g = 0, b = 0, a = 0;
						for (int i = 0; i < splitString.Length; i++)
						{
							switch (i)
							{
								case 0:
									if (!NumberFormats.TryParseIntVb6(splitString[i], out r))
									{
										currentHost.AddMessage(MessageType.Error, false, "Red is invalid in " + Key + " in " + blockDocument.Name + " at line " + ((IXmlLineInfo)e).LineNumber + " in " + FileName);
									}
									else if (r < 0 | r > 255) 
									{
										currentHost.AddMessage(MessageType.Error, false, "Red is required to be within the range of 0 to 255 in " + Key + " in " + blockDocument.Name + " at line " + ((IXmlLineInfo)e).LineNumber + " in " + FileName);
										r = r < 0 ? 0 : 255;
									}
									break;
								case 1:
									if (!NumberFormats.TryParseIntVb6(splitString[i], out g))
									{
										currentHost.AddMessage(MessageType.Error, false, "Green is invalid in " + Key + " in " + blockDocument.Name + " at line " + ((IXmlLineInfo)e).LineNumber + " in " + FileName);
									}
									else if (g < 0 | g > 255) 
									{
										currentHost.AddMessage(MessageType.Error, false, "Green is required to be within the range of 0 to 255 in " + Key + " in " + blockDocument.Name + " at line " + ((IXmlLineInfo)e).LineNumber + " in " + FileName);
										g = g < 0 ? 0 : 255;
									}
									break;
								case 2:
									if (!NumberFormats.TryParseIntVb6(splitString[i], out b))
									{
										currentHost.AddMessage(MessageType.Error, false, "Blue is invalid in " + Key + " in " + blockDocument.Name + " at line " + ((IXmlLineInfo)e).LineNumber + " in " + FileName);
									}
									else if (b < 0 | b > 255) 
									{
										currentHost.AddMessage(MessageType.Error, false, "Blue is required to be within the range of 0 to 255 in " + Key + " in " + blockDocument.Name + " at line " + ((IXmlLineInfo)e).LineNumber + " in " + FileName);
										b = b < 0 ? 0 : 255;
									}
									break;
								case 3:
									if (!NumberFormats.TryParseIntVb6(splitString[i], out a))
									{
										currentHost.AddMessage(MessageType.Error, false, "Alpha is invalid in " + Key + " in " + blockDocument.Name + " at line " + ((IXmlLineInfo)e).LineNumber + " in " + FileName);
									}
									else if (a < 0 | a > 255) 
									{
										currentHost.AddMessage(MessageType.Error, false, "Alpha is required to be within the range of 0 to 255 in " + Key + " in " + blockDocument.Name + " at line " + ((IXmlLineInfo)e).LineNumber + " in " + FileName);
										a = a < 0 ? 0 : 255;
									}
									break;
							}
						}
						parsedColor = new Color32((byte) r, (byte) g, (byte) b, (byte) a);
					}
					else
					{
						//Hex color
						if (!Color32.TryParseHexColor(e.Value, out parsedColor))
						{
							currentHost.AddMessage(MessageType.Error, false, "Value is invalid in " + Key + " in " + blockDocument.Name + " at line " + ((IXmlLineInfo)e).LineNumber + " in " + FileName);
						}
					}
					return parsedColor;
				}
			}
			return parsedColor;
		}

		public override Texture LoadTexture<T>(T Key, string texturePath, TextureParameters textureParameters, bool loadTexture = false)
		{
			foreach(XElement e in blockDocument.Elements())
			{
				T parsedElement;
				Enum.TryParse(e.Name.LocalName, true, out parsedElement);
				if (parsedElement.Equals(Key))
				{
					if (Path.ContainsInvalidChars(e.Value))
					{
						currentHost.AddMessage(MessageType.Error, false, "Texture path " + e.Value + " contains invalid characters in " + Key + " in " + blockDocument.Name + " at line " + ((IXmlLineInfo)e).LineNumber + " in " + FileName);
						return null;
					}

					try
					{
						string finalPath = Path.CombineFile(texturePath, e.Value);
						if (!File.Exists(finalPath) && !System.IO.Path.HasExtension(finalPath))
						{
							string candidatePath = Path.CombineFile(finalPath, ".bmp");
							if (!File.Exists(candidatePath))
							{
								candidatePath = Path.CombineFile(finalPath, ".png");
							}
						
							if (File.Exists(candidatePath))
							{
								finalPath = candidatePath;
							}
							else
							{
								currentHost.AddMessage(MessageType.Error, false, "Texture file " + e.Value + " was not found in " + Key + " in " + blockDocument.Name + " at line " + ((IXmlLineInfo)e).LineNumber + " in " + FileName);
								return null;
							}
						}
						Texture loadedTexture;
						currentHost.RegisterTexture(finalPath, textureParameters, out loadedTexture, loadTexture);
						return loadedTexture;
					}
					catch
					{
						currentHost.AddMessage(MessageType.Error, false, "An unexpected error occured whilst loading texture file " + e.Value + " in " + Key + " in " + blockDocument.Name + " at line " + ((IXmlLineInfo)e).LineNumber + " in " + FileName);
						return null;
					}
				}
			}
			return null;
		}


		public override Block ReadSubBlock<T>(T newToken)
		{
			foreach (XElement e in blockDocument.Elements())
			{
				T parsedElement;
				Enum.TryParse(e.Name.LocalName, true, out parsedElement);
				if (parsedElement.Equals(newToken))
				{
					e.Remove();
					return new XmlBlock(currentHost, FileName, e, newToken);
				}
			}
			return null;
		}
    }
}
