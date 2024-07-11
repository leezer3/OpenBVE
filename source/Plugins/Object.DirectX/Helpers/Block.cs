//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2020, Christopher Lees, The OpenBVE Project
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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenBve.Formats.DirectX
{
	/// <summary>An abstract block of data, read from a DirectX file</summary>
	public abstract class Block
	{
		/// <summary>The token that identifies the contents of this block</summary>
		public TemplateID Token;

		/// <summary>The label of this block</summary>
		/// <remarks>Normally blank</remarks>
		public string Label;

		public int FloatingPointSize;

		/// <summary>Reads an integer from the block</summary>
		public abstract int ReadUInt();

		/// <summary>Reads an unsigned 16-bit integer from the block</summary>
		public abstract ushort ReadUInt16();

		/// <summary>Reads a single-bit precision floating point number from the block</summary>
		public abstract float ReadSingle();

		/// <summary>Skips <para>length</para> through the block</summary>
		/// <param name="length">The length to skip</param>
		public abstract void Skip(int length);

		/// <summary>Reads a string from the block</summary>
		public abstract string ReadString();

		/// <summary>Returns the length of the block</summary>
		public abstract long Length();

		/// <summary>Returns the position of the reader within the block</summary>
		public abstract long Position();

		/// <summary>Creates a new block from the supplied byte array</summary>
		/// <param name="bytes">The block of data</param>
		/// <param name="token">The token for the new block</param>
		/// <returns>The new block</returns>
		/// <remarks>Always creates a BinaryBlock</remarks>
		public static Block ReadBlock(byte[] bytes, TemplateID token)
		{
			return new BinaryBlock(bytes, token);
		}

		/// <summary>Creates a new block from the supplied string</summary>
		/// <param name="text">The string</param>
		/// <param name="token">The token for the new block</param>
		/// <returns>The new block</returns>
		/// <remarks>Always creates a TextualBlock</remarks>
		public static Block ReadBlock(string text, TemplateID token)
		{
			return new TextualBlock(text, token);
		}

		/// <summary>Reads a sub-block from the enclosing block</summary>
		/// <param name="newToken">The expected token for the new block</param>
		/// <returns>The new block</returns>
		/// <remarks>The type of the new block will always match that of the base block</remarks>
		public abstract Block ReadSubBlock(TemplateID newToken);

		/// <summary>Reads a sub-block from the enclosing block</summary>
		/// <param name="validTokens">An array containing all possible valid tokens for the new block</param>
		/// <returns>The new block</returns>
		/// <remarks>The type of the new block will always match that of the base block</remarks>
		public abstract Block ReadSubBlock(TemplateID[] validTokens);

		/// <summary>Reads any sub-block from the enclosing block</summary>
		/// <returns>The new block</returns>
		/// <remarks>The type of the new block will always match that of the base block</remarks>
		public abstract Block ReadSubBlock();
	}

	/// <inheritdoc />
	public class TextualBlock : Block
	{
		private readonly string myText;

		private int currentPosition;

		public TextualBlock(string text)
		{
			myText = text;
			currentPosition = 0;
		}

		public TextualBlock(string text, TemplateID token)
		{
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < text.Length; i++)
			{
				//Convert multiple whitespace chars to single
				//whilst building new string
				if (!char.IsWhiteSpace(text[i]))
				{
					sb.Append(text[i]);
				}
				else
				{
					if (char.IsWhiteSpace(text[i + 1]))
					{
						continue;
					}
					sb.Append(' ');
				}
			}
			myText = sb.ToString();
			Token = token;
			currentPosition = 0;
			Label = string.Empty;
			while (myText[currentPosition] != '{')
			{
				currentPosition++;
			}
			
			if (token == TemplateID.TextureKey)
			{
				currentPosition++;
				int p = currentPosition;
				while (myText[currentPosition] != '}')
				{
					currentPosition++;
				}

				if (currentPosition - 1 - p > 0)
				{
					// reference based material name in textual X
					string l = myText.Substring(p, currentPosition - 1);
					Label = l.Trim(new char[] { });
					return;
				}
				if (currentPosition == text.Length - 1)
				{
					// Null rail converted by BVE5 : Contains texture co-ords, but the key is missing...
					return;
				}
				string s = myText.Substring(p, currentPosition - 1);
				Label = s.Trim(new char[] { });
			}
			else
			{
				if (currentPosition == 0)
				{
					throw new Exception("Token " + token + " was not found.");
				}
				string s = myText.Substring(0, currentPosition);
				int ws = s.IndexOf(' ');
				if (ws != -1)
				{
					Label = s.Substring(ws, s.Length - ws).Trim(new char[] { });
					s = s.Substring(0, ws);
				}

				TemplateID currentToken;
				if (!Enum.TryParse(s, true, out currentToken))
				{
					throw new Exception("Invalid token " + s);
				}

				if (currentToken != Token)
				{
					throw new Exception("Expected the " + Token + " token, got " + currentToken);
				}
			}
			

			currentPosition++;
		}

		public override Block ReadSubBlock(TemplateID newToken)
		{
			startPosition = currentPosition;
			while (myText[startPosition] == ';')
			{
				//Arrays with 'incorrect' additional terminator
				//e.g. Mesquioa
				currentPosition++;
				startPosition++;
			}
			string s = String.Empty;
			while (currentPosition < myText.Length)
			{
				if (myText[currentPosition] == '{')
				{
					int l = currentPosition - startPosition;
					s = myText.Substring(startPosition, l).Trim(new char[] { });
					currentPosition++;
					break;
				}
				if (myText[currentPosition] == '}')
				{
					//Incorrectly closed previous block
					startPosition = currentPosition + 1;
				}
				currentPosition++;
			}

			if (string.IsNullOrWhiteSpace(s))
			{
				throw new Exception();
			}

			TemplateID currentToken;
			int ws = s.IndexOf(' ');
			if (ws != -1)
			{
				//The block has the optional label
				Label = s.Substring(ws, s.Length - ws).Trim(new char[] { });
				s = s.Substring(0, ws);
			}

			if (!Enum.TryParse(s, true, out currentToken))
			{
				if (newToken == TemplateID.TextureKey)
				{
					currentToken = TemplateID.TextureKey;
				}
				else
				{
					throw new Exception("Unrecognised token " + s);	
				}
			}

			if (newToken != currentToken)
			{
				throw new Exception("Expected the " + newToken + " token, got " + currentToken);
			}
			
			int level = 0;
			while (currentPosition < myText.Length)
			{
				if (myText[currentPosition] == '{')
				{
					level++;
				}

				if (myText[currentPosition] == '}')
				{
					currentPosition++;
					if (level == 0)
					{
						return new TextualBlock(myText.Substring(startPosition, currentPosition - startPosition).Trim(new char[] { }), currentToken);
					}

					level--;

				}

				currentPosition++;
			}

			throw new Exception("Unexpected end of block in " + Token);
		}

		public override Block ReadSubBlock(TemplateID[] validTokens)
		{
			startPosition = currentPosition;
			while (myText[startPosition] == ';')
			{
				//Arrays with 'incorrect' additional terminator
				//e.g. Mesquioa
				currentPosition++;
				startPosition++;
			}
			string s = String.Empty;
			while (currentPosition < myText.Length)
			{
				if (myText[currentPosition] == '{')
				{
					int l = currentPosition - startPosition;
					s = myText.Substring(startPosition, l).Trim(new char[] { });
					currentPosition++;
					break;
				}
				if (myText[currentPosition] == '}')
				{
					//Incorrectly closed previous block
					startPosition = currentPosition + 1;
				}
				currentPosition++;
			}

			TemplateID currentToken;
			int ws = s.IndexOf(' ');
			if (ws != -1)
			{
				//The block has the optional label
				Label = s.Substring(ws, s.Length - ws).Trim(new char[] { });
				s = s.Substring(0, ws);
			}

			if (!Enum.TryParse(s, true, out currentToken))
			{
				if (validTokens.Contains(TemplateID.TextureKey))
				{
					currentToken = TemplateID.TextureKey;
				}
				else
				{
					throw new Exception("Unrecognised token " + s);	
				}
				
			}

			if (!validTokens.Contains(currentToken))
			{
				throw new Exception("Expected one of the following tokens: " + validTokens + " , got " + currentToken);
			}

			int level = 0;
			while (currentPosition < myText.Length)
			{
				if (myText[currentPosition] == '{')
				{
					level++;
				}

				if (myText[currentPosition] == '}')
				{
					currentPosition++;
					if (level == 0)
					{
						return new TextualBlock(myText.Substring(startPosition, currentPosition - startPosition).Trim(new char[] { }), currentToken);
					}

					level--;

				}

				currentPosition++;
			}

			throw new Exception("Unexpected end of block in " + Token);
		}

		public override Block ReadSubBlock()
		{
			startPosition = currentPosition;
			while (myText[startPosition] == ';')
			{
				//Arrays with 'incorrect' additional terminator
				//e.g. Mesquioa
				currentPosition++;
				startPosition++;
			}
			string s = string.Empty;
			while (currentPosition < myText.Length)
			{
				if (myText[currentPosition] == '{')
				{
					int l = currentPosition - startPosition;
					s = myText.Substring(startPosition, l).Trim(new char[] { });
					currentPosition++;
					break;
				}
				if (myText[currentPosition] == '}')
				{
					//Incorrectly closed previous block
					startPosition = currentPosition + 1;
				}
				currentPosition++;
			}

			if (string.IsNullOrWhiteSpace(s))
			{
				throw new Exception();
			}

			TemplateID currentToken;
			int ws = s.IndexOf(' ');
			if (ws != -1)
			{
				//The block has the optional label
				Label = s.Substring(ws, s.Length - ws).Trim(new char[] { });
				s = s.Substring(0, ws).Trim(new char[] { });
			}

			if (!Enum.TryParse(s, true, out currentToken))
			{
				throw new Exception("Unrecognised token " + s);
			}
			
			int level = 0;
			while (currentPosition < myText.Length)
			{
				if (myText[currentPosition] == '{')
				{
					level++;
				}

				if (myText[currentPosition] == '}')
				{
					currentPosition++;
					if (level == 0)
					{
						return new TextualBlock(myText.Substring(startPosition, currentPosition - startPosition).Trim(new char[] { }), currentToken);
					}

					level--;

				}

				currentPosition++;
			}

			throw new Exception("Unexpected end of block in " + Token);
		}


		private int startPosition;

		public override int ReadUInt()
		{
			startPosition = currentPosition;
			string s = getNextValue();
			if (char.IsWhiteSpace(myText[currentPosition]))
			{
				while (char.IsWhiteSpace(myText[currentPosition]))
				{
					currentPosition++;
				}
			}
			int val;
			if (int.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out val))
			{
				return val;
			}
			throw new Exception("Unable to parse " + s + " to a valid integer in block " + Token);
		}

		public override ushort ReadUInt16()
		{
			startPosition = currentPosition;
			string s = getNextValue();
			if (char.IsWhiteSpace(myText[currentPosition]))
			{
				while (char.IsWhiteSpace(myText[currentPosition]))
				{
					currentPosition++;
				}
			}
			int val;
			if (int.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out val))
			{
				return (ushort) val;
			}
			throw new Exception("Unable to parse " + s + " to a valid integer in block " + Token);
		}

		public override float ReadSingle()
		{
			startPosition = currentPosition;
			string s = getNextValue();
			currentPosition++;
			float val;
			if (float.TryParse(s, NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out val))
			{
				return val;
			}

			throw new Exception("Unable to parse " + s + " to a valid float in block " + Token);
		}

		public override void Skip(int length)
		{
			//Unused at the minute
		}

		public override string ReadString()
		{
			startPosition = currentPosition;
			while (currentPosition < myText.Length)
			{
				if (!char.IsWhiteSpace(myText[currentPosition]))
				{
					break;
				}
				currentPosition++;
			}
			return getNextValue();
		}

		public override long Length()
		{
			return myText.Length;
		}

		public override long Position()
		{
			return currentPosition;
		}

		private string getNextValue()
		{

			if (char.IsWhiteSpace(myText[currentPosition]) || myText[currentPosition] == ';'  || myText[currentPosition] == ',')
			{
				while (char.IsWhiteSpace(myText[currentPosition]) || myText[currentPosition] == ';'  || myText[currentPosition] == ',')
				{
					currentPosition++;
				}
			}

			startPosition = currentPosition;
			if (myText[currentPosition] == '"')
			{
				//Quote enclosed string
				currentPosition++;
				startPosition++;
				while (myText[currentPosition] != '"')
				{
					currentPosition++;
				}

				int l = currentPosition - startPosition;
				currentPosition++;
				if (l > 0)
				{
					return myText.Substring(startPosition, l).Trim(new char[] { });
				}
			}
			else if (myText[currentPosition] == '<')
			{
				//Template GUID
				currentPosition++;
				startPosition++;
				while (myText[currentPosition] != '>')
				{
					currentPosition++;
				}

				int l = currentPosition - startPosition;
				currentPosition++;
				if (l > 0)
				{
					return myText.Substring(startPosition, l).Trim(new char[] { });
				}
			}
			else
			{
				while (char.IsDigit(myText[currentPosition]) || myText[currentPosition] == '.' || myText[currentPosition] == '+' || myText[currentPosition] == '-')
				{
					currentPosition++;
				}

				int l = currentPosition - startPosition;
				if (l > 0)
				{
					return myText.Substring(startPosition, l).Trim(new char[] { });
				}
			}

			return string.Empty;
		}
	}

	public class BinaryBlock : Block, IDisposable
	{
		private readonly BinaryReader myReader;
		private readonly MemoryStream myStream;

		private int currentLevel = 0;

		private readonly List<int> cachedIntegers = new List<int>();
		private readonly List<double> cachedFloats = new List<double>();
		private readonly List<string> cachedStrings = new List<string>();

		public BinaryBlock(byte[] bytes, TemplateID token)
		{
			myStream = new MemoryStream(bytes);
			myReader = new BinaryReader(myStream);
			while (myStream.Position < myStream.Length)
			{
				string currentToken = getNextToken();
				switch (currentToken)
				{
					case "int_list":
						int integerCount = (int) myReader.ReadInt32();
						for (int i = 0; i < integerCount; i++)
						{
							cachedIntegers.Add(myReader.ReadInt16());
						}
						myStream.Position += integerCount * 4;
						break;
					case "float_list":
						int floatCount = (int) myReader.ReadInt32();
						switch (FloatingPointSize)
						{
							case 32:
								for (int i = 0; i < floatCount; i++)
								{
									cachedFloats.Add(myReader.ReadSingle());
								}

								break;
							case 64:
								for (int i = 0; i < floatCount; i++)
								{
									cachedFloats.Add(myReader.ReadDouble());
								}
								break;
							default:
								throw new Exception("Unsupported Floating Point Size");
						}
						break;

				}
			}
		}

		public BinaryBlock(byte[] bytes, int floatingPointSize)
		{
			FloatingPointSize = floatingPointSize;
			myStream = new MemoryStream(bytes);
			myReader = new BinaryReader(myStream);
			long startPosition;
			while (myStream.Position < myStream.Length)
			{
				startPosition = myStream.Position;
				string currentToken = getNextToken();
				switch (currentToken)
				{
					case "int_list":
						int integerCount = (int) myReader.ReadInt32();
						for (int i = 0; i < integerCount; i++)
						{
							cachedIntegers.Add(myReader.ReadInt16());
						}
						break;
					case "float_list":
						int floatCount = (int) myReader.ReadInt32();
						switch (FloatingPointSize)
						{
							case 32:
								for (int i = 0; i < floatCount; i++)
								{
									cachedFloats.Add(myReader.ReadSingle());
								}

								break;
							case 64:
								for (int i = 0; i < floatCount; i++)
								{
									cachedFloats.Add(myReader.ReadDouble());
								}
								break;
							default:
								throw new Exception("Unsupported Floating Point Size");
						}
						break;
					default:
						cachedStrings.Add(currentToken);
						TemplateID newBlockToken;
						if (Enum.TryParse(currentToken, true, out newBlockToken) && newBlockToken != TemplateID.Header)
						{
							myStream.Position = startPosition;
							return;
						}
						break;

				}
			}
		}

		private BinaryBlock(byte[] bytes, TemplateID token, int floatingPointSize)
		{
			FloatingPointSize = floatingPointSize;
			myStream = new MemoryStream(bytes);
			myReader = new BinaryReader(myStream);
			Token = token;
			long startPosition;
			while (myStream.Position < myStream.Length)
			{
				startPosition = myStream.Position;
				string currentToken = getNextToken();
				switch (currentToken)
				{
					case "int_list":
						int integerCount = (int) myReader.ReadInt32();
						for (int i = 0; i < integerCount; i++)
						{
							cachedIntegers.Add(myReader.ReadInt32());
						}
						break;
					case "float_list":
						int floatCount = (int) myReader.ReadInt32();
						switch (FloatingPointSize)
						{
							case 32:
								for (int i = 0; i < floatCount; i++)
								{
									cachedFloats.Add(myReader.ReadSingle());
								}

								break;
							case 64:
								for (int i = 0; i < floatCount; i++)
								{
									cachedFloats.Add(myReader.ReadDouble());
								}
								break;
							default:
								throw new Exception("Unsupported Floating Point Size");
						}
						break;
					default:
						cachedStrings.Add(currentToken);
						TemplateID newBlockToken;
						if (Enum.TryParse(currentToken, true, out newBlockToken))
						{
							myStream.Position = startPosition;
							return;
						}
						break;

				}
			}
		}

		public override int ReadUInt()
		{
			int u = cachedIntegers[0];
			cachedIntegers.RemoveAt(0);
			return u;
		}

		public override ushort ReadUInt16()
		{
			ushort u = (ushort)cachedIntegers[0];
			cachedIntegers.RemoveAt(0);
			return u;
		}

		public override float ReadSingle()
		{
			float f = (float)cachedFloats[0];
			cachedFloats.RemoveAt(0);
			return f;
		}

		public override void Skip(int length)
		{
			myReader.ReadBytes(length);
		}

		public override string ReadString()
		{
			if (cachedStrings.Count > 0)
			{
				string s = cachedStrings[0];
				cachedStrings.RemoveAt(0);
				return s;
			}
			return string.Empty;
		}

		public override long Length()
		{
			return myStream.Length;
		}

		public override long Position()
		{
			return myStream.Position;
		}

		public override Block ReadSubBlock(TemplateID newToken)
		{
			Block b = ReadSubBlock();
			if (b.Token != newToken)
			{
				throw new Exception();
			}

			return b;
		}

		public override Block ReadSubBlock(TemplateID[] validTokens)
		{
			Block b = ReadSubBlock();
			if (!validTokens.Contains(b.Token))
			{
				throw new Exception();
			}

			return b;
		}

		public override Block ReadSubBlock()
		{
			string blockName = getNextToken();
			TemplateID newToken;
			
			if (!Enum.TryParse(blockName, true, out newToken))
			{
				throw new Exception("Unable to parse " + blockName + " into a valid token.");
			}

			long blockStart = 0;
			int integerCount, floatCount;
			while (myStream.Position < myStream.Length)
			{
				string currentToken = getNextToken();
				
				switch (currentToken)
				{
					case "{":
						if (currentLevel == 0)
						{
							blockStart = myStream.Position;
						}
						currentLevel++;
						break;
					case "}":
						currentLevel--;
						if (currentLevel == 0)
						{
							long newBlockLength = myStream.Position - blockStart;
							myStream.Position -= newBlockLength;
							byte[] newBlockBytes = myReader.ReadBytes((int)newBlockLength);
							BinaryBlock b = new BinaryBlock(newBlockBytes, newToken, FloatingPointSize);
							b.Label = Label;
							return b;
						}
						break;
					case "int_list":
						integerCount = myReader.ReadInt32();
						myStream.Position += integerCount * 4;
						break;
					case "float_list":
						floatCount = myReader.ReadInt32();
						switch (FloatingPointSize)
						{
							case 32:
								myStream.Position += floatCount * 4;
								break;
							case 64:
								myStream.Position += floatCount * 8;
								break;
							default:
								throw new Exception("Unsupported Floating Point Size");
						}
						break;
					default:
						if (currentLevel == 0 && newToken == TemplateID.Material)
						{
							Label = currentToken;
						}
						break;
				}
			}
			throw new Exception("Reached the end of the binary block, but did not find the block terminator.");
		}

		private string getNextToken()
		{
			TokenID token = (TokenID)myReader.ReadInt16();
			int byteCount;
			switch (token)
			{
				case 0x0:
					myReader.ReadInt32();
					/*
					 * Appears to be padding, e.g. iiyama2060_obj
					 * TODO: MAY BE BROKEN SOMEWHERE
					 */
					break;
				case TokenID.NAME:
					if (myStream.Length - myStream.Position < 4)
					{
						return string.Empty;
					}
					byteCount = (int)myReader.ReadInt32();
					byte[] nameBytes = myReader.ReadBytes(byteCount);
					return Encoding.ASCII.GetString(nameBytes);
				case TokenID.STRING:
					if (myStream.Length - myStream.Position < 4)
					{
						return string.Empty;
					}
					byteCount = (int)myReader.ReadInt32();
					if (myStream.Length - (myStream.Position + byteCount) < 4)
					{
						return string.Empty;
					}
					byte[] stringBytes = myReader.ReadBytes(byteCount);
					return Encoding.ASCII.GetString(stringBytes);
				case TokenID.INT:
					myReader.ReadBytes(4);
					break;
				case TokenID.GUID:
					myReader.ReadBytes(16);
					break;
				case TokenID.INT_LIST:
					if (myStream.Length - myStream.Position < 4)
					{
						return string.Empty;
					}
					return "int_list";
				case TokenID.FLOAT_LIST:
					if (myStream.Length - myStream.Position < 4)
					{
						return string.Empty;
					}
					return "float_list";
				case TokenID.OBRACE:
					return "{";
				case TokenID.CBRACE:
					return "}";
				case TokenID.OBRACKET:
					return "(";
				case TokenID.CBRACKET:
					return ")";
				case TokenID.OSQUARE:
					return "[";
				case TokenID.CSQUARE:
					return "]";
				case TokenID.LTHAN:
					return "<";
				case TokenID.GTHAN:
					return ">";
				case TokenID.PERIOD:
					return ".";
				case TokenID.COMMA:
					return ",";
				case TokenID.SEMICOLON:
					return ";";
				case TokenID.TEMPLATE:
					return "template";
				case TokenID.WORD:
					return "WORD";
				case TokenID.DWORD:
					return "DWORD";
				case TokenID.FLOAT:
					return "FLOAT";
				case TokenID.DOUBLE:
					return "DOUBLE";
				case TokenID.CHAR:
					return "CHAR";
				case TokenID.UCHAR:
					return "UCHAR";
				case TokenID.SWORD:
					return "SWORD";
				case TokenID.SDWORD:
					return "SDWORD";
				case TokenID.VOID:
					return "void";
				case TokenID.STRING2:
					return "string";
				case TokenID.UNICODE:
					return "unicode";
				case TokenID.CSTRING:
					return "cstring";
				case TokenID.ARRAY:
					return "array";
			}
			return string.Empty;
		}

		public void Dispose()
		{
			Dispose(true);
		}

		private void Dispose(bool currentlyDisposing)
		{
			if(currentlyDisposing)
			{
				myReader.Dispose();
				myStream.Dispose();
			}
		}
	}
}
