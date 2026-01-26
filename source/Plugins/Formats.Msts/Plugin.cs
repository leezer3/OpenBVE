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

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using OpenBveApi.Colors;
using OpenBveApi.Math;
using OpenBveApi.World;

// ReSharper disable UnusedMember.Global

namespace OpenBve.Formats.MsTs
{
	/// <summary>An abstract block of data, read from a MSTS file</summary>
	public abstract class Block
	{
		/// <summary>The parent block</summary>
		public Block ParentBlock;

		/// <summary>The token that identifies the contents of this block</summary>
		public KujuTokenID Token;

		/// <summary>The label of this block</summary>
		/// <remarks>Normally blank</remarks>
		public string Label;

		/// <summary>Reads an unsigned 16-bit integer from the block</summary>
		public abstract ushort ReadUInt16();

		/// <summary>Reads an unsigned 32-bit integer from the block</summary>
		public abstract uint ReadUInt32();

		/// <summary>Reads an signed 32-bit integer from the block</summary>
		public abstract int ReadInt32();

		/// <summary>Reads an signed 16-bit integer from the block</summary>
		public abstract int ReadInt16();

		/// <summary>Reads a single-bit precision floating point number from the block</summary>
		public abstract float ReadSingle();

		/// <summary>Reads a Color32 from the block</summary>
		public abstract Color32 ReadColorArgb();

		/// <summary>Reads a Vector2 from the block</summary>
		public Vector2 ReadVector2() => new Vector2(ReadSingle(), ReadSingle());

		/// <summary>Reads a Vector3 from the block</summary>
		public Vector3 ReadVector3() => new Vector3(ReadSingle(), ReadSingle(), ReadSingle());

		/// <summary>Reads a single-bit precision floating point number from the block, and converts it to the desired units</summary>
		public abstract float ReadSingle<TUnitType>(TUnitType desiredUnit, TUnitType? defaultUnit = null) where TUnitType : struct;
		
		/// <summary>Skips <para>length</para> through the block</summary>
		/// <param name="length">The length to skip</param>
		public abstract void Skip(int length);

		/// <summary>Reads a string from the block</summary>
		public abstract string ReadString();

		/// <summary>Reads a path from the block</summary>
		/// <param name="absolute">The platform specific absolute path</param>
		/// <param name="finalPath">The final path</param>
		/// <returns>True if the path is found, false otherwise</returns>
		public abstract bool ReadPath(string absolute, out string finalPath);

		/// <summary>Reads a string array from the block</summary>
		public abstract string[] ReadStringArray();

        /// <summary>Reads an array of enum values from the block</summary>
        /// <typeparam name="TEnumType">The desired enum</typeparam>
        /// <returns>An enum array</returns>
        public abstract TEnumType[] ReadEnumArray<TEnumType>(TEnumType desiredEnumType)  where TEnumType : struct;

		/// <summary>Reads an enum value from the block</summary>
		/// <typeparam name="TEnumType">The desired enum</typeparam>
		/// <returns>An enum array</returns>
		public abstract TEnumType ReadEnumValue<TEnumType>(TEnumType desiredEnumType) where TEnumType : struct;

		/// <summary>Reads a boolean value from the block</summary>
		public bool ReadBool() => ReadInt16() == 1;

		/// <summary>Returns the length of the block</summary>
		public abstract long Length();

		/// <summary>Returns the position of the reader within the block</summary>
		public abstract long Position();

		/// <summary>Creates a new block from the supplied byte array</summary>
		/// <param name="bytes">The block of data</param>
		/// <param name="token">The token for the new block</param>
		/// <returns>The new block</returns>
		/// <remarks>Always creates a BinaryBlock</remarks>
		public static Block ReadBlock(byte[] bytes, KujuTokenID token)
		{
			return new BinaryBlock(bytes, token);
		}

		/// <summary>Creates a new block from the supplied string</summary>
		/// <param name="text">The string</param>
		/// <param name="token">The token for the new block</param>
		/// <returns>The new block</returns>
		/// <remarks>Always creates a TextualBlock</remarks>
		public static Block ReadBlock(string text, KujuTokenID token)
		{
			return new TextualBlock(text, token);
		}

		/// <summary>Reads a sub-block from the enclosing block</summary>
		/// <param name="newToken">The expected token for the new block</param>
		/// <returns>The new block</returns>
		/// <remarks>The type of the new block will always match that of the base block</remarks>
		public abstract Block ReadSubBlock(KujuTokenID newToken);

		/// <summary>Reads the next sub-block from the enclosing block, skipping unexpected values</summary>
		/// <param name="newToken">The expected token for the new block</param>
		/// <returns>The new block</returns>
		/// <remarks>The type of the new block will always match that of the base block</remarks>
		public abstract Block GetSubBlock(KujuTokenID newToken);

		/// <summary>Reads a sub-block from the enclosing block</summary>
		/// <param name="validTokens">An array containing all possible valid tokens for the new block</param>
		/// <returns>The new block</returns>
		/// <remarks>The type of the new block will always match that of the base block</remarks>
		public abstract Block ReadSubBlock(KujuTokenID[] validTokens);

		/// <summary>Reads any sub-block from the enclosing block</summary>
		/// <returns>The new block</returns>
		/// <remarks>The type of the new block will always match that of the base block</remarks>
		public abstract Block ReadSubBlock(bool allowEmptyBlock = false);
	}

	/// <inheritdoc cref="Block" />
	public class BinaryBlock : Block , IDisposable
	{
		private readonly BinaryReader myReader;
		private readonly MemoryStream myStream;

		public BinaryBlock(byte[] bytes, KujuTokenID token, Block parentBlock = null)
		{
			Token = token;
			myStream = new MemoryStream(bytes);
			myReader = new BinaryReader(myStream);
			int length = myReader.ReadByte();
			if (length > 0)
			{
				//Note: For most blocks, the label length will be zero
				byte[] buff = new byte[length * 2];
				int i = 0;
				while (i < length * 2)
				{
					buff[i] = myReader.ReadByte();
					i++;
				}

				Label = Encoding.Unicode.GetString(buff, 0, length * 2);
			}
			else
			{
				Label = string.Empty;
			}

			ParentBlock = parentBlock;
		}

		public override Block ReadSubBlock(KujuTokenID newToken)
		{
			if (myStream.Position == myStream.Length)
			{
				throw new EndOfStreamException("Expected " + newToken + " however, no further data available");
			}
			KujuTokenID currentToken = (KujuTokenID) myReader.ReadUInt16();
			if (currentToken != newToken)
			{
				throw new InvalidDataException("Expected the " + newToken + " token, got " + currentToken);
			}

			myReader.ReadUInt16();
			uint remainingBytes = myReader.ReadUInt32();
			byte[] newBytes = myReader.ReadBytes((int) remainingBytes);
			return new BinaryBlock(newBytes, newToken, this);
		}

		public override Block GetSubBlock(KujuTokenID newToken)
		{
			throw new NotImplementedException();
		}

		public override Block ReadSubBlock(KujuTokenID[] validTokens)
		{
			KujuTokenID currentToken = (KujuTokenID) myReader.ReadUInt16();
			if (!validTokens.Contains(currentToken))
			{
				throw new InvalidDataException("Expected one of the following tokens: " + validTokens + ", got " + currentToken);
			}

			myReader.ReadUInt16();
			uint remainingBytes = myReader.ReadUInt32();
			byte[] newBytes = myReader.ReadBytes((int) remainingBytes);
			return new BinaryBlock(newBytes, currentToken, this);
		}

		public override Block ReadSubBlock(bool allowEmptyBlock = false)
		{
			KujuTokenID currentToken = (KujuTokenID) myReader.ReadUInt16();
			myReader.ReadUInt16();
			uint remainingBytes = myReader.ReadUInt32();
			byte[] newBytes = myReader.ReadBytes((int) remainingBytes);
			return new BinaryBlock(newBytes, currentToken, this);
		}

		public override ushort ReadUInt16()
		{
			return myReader.ReadUInt16();
		}

		public override uint ReadUInt32()
		{
			return myReader.ReadUInt32();
		}

		public override int ReadInt32()
		{
			return myReader.ReadInt32();
		}

		public override int ReadInt16()
		{
			return myReader.ReadInt16();
		}

		public override float ReadSingle()
		{
			return myReader.ReadSingle();
		}

		public override Color32 ReadColorArgb()
		{
			float a = myReader.ReadSingle();
			float r = myReader.ReadSingle();
			float g = myReader.ReadSingle();
			float b = myReader.ReadSingle();
			return new Color32((byte)(r * 255), (byte)(g * 255), (byte)(b * 255), (byte)(a * 255));
		}

		public override float ReadSingle<TUnitType>(TUnitType desiredUnit, TUnitType? defaultUnitType)
		{
			throw new NotImplementedException();
		}

		public override void Skip(int length)
		{
			myReader.ReadBytes(length);
		}

		public override string ReadString()
		{
			int stringLength = ReadUInt16();
			if (stringLength > 0)
			{
				byte[] buff = new byte[stringLength * 2];
				int i = 0;
				while (i < stringLength * 2)
				{
					buff[i] = myReader.ReadByte();
					i++;
				}

				return Encoding.Unicode.GetString(buff, 0, stringLength * 2);
			}

			return (string.Empty); //Not sure this is valid, but let's be on the safe side
		}

		public override bool ReadPath(string absolute, out string finalPath)
		{
			string relative = ReadString().Replace("\"", "");
			try
			{
				finalPath = OpenBveApi.Path.CombineFile(absolute, relative);
			}
			catch
			{
				finalPath = relative;
				return false;
			}
			if (File.Exists(finalPath))
			{
				return true;
			}

			finalPath = relative;
			return false;
		}

		public override string[] ReadStringArray()
		{
			throw new NotImplementedException();
		}

		public override TEnumType[] ReadEnumArray<TEnumType>(TEnumType desiredEnumType)
		{
			throw new NotImplementedException();
		}

		public override TEnumType ReadEnumValue<TEnumType>(TEnumType desiredEnumType)
		{
			string enumValue = ReadString();
			Enum.TryParse(enumValue, out TEnumType e);
			return e;
		}

		public override long Length()
		{
			return myStream.Length;
		}

		public override long Position()
		{
			return myStream.Position;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
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

	/// <inheritdoc />
	public class TextualBlock : Block
	{
		private readonly string myText;

		private int currentPosition;

		private readonly LengthConverter lengthConverter = new LengthConverter();
		private readonly WeightConverter weightConverter = new WeightConverter();
		private readonly ForceConverter forceConverter = new ForceConverter();
		private readonly VolumeConverter volumeConverter = new VolumeConverter();
		private readonly CurrentConverter currentConverter = new CurrentConverter();
		private readonly PressureConverter pressureConverter = new PressureConverter();
		private readonly VelocityConverter velocityConvertor = new VelocityConverter();
		private readonly TorqueConverter torqueConvertor = new TorqueConverter();

		private KujuTokenID ParseToken(string s)
		{
			if (Enum.TryParse(s, true, out KujuTokenID parsedToken))
			{
				return parsedToken;
			}

			if (s[0] == '#' || s.StartsWith("_#"))
			{
				return KujuTokenID.Skip;
			}
#if DEBUG 
			// In debug mode, always throw an exception
			// this way, any new parameters can be added to our list for future
			// possible usage
			throw new InvalidDataException("Unrecognised token " + s);
#else
			if (s.StartsWith("ORTS", StringComparison.InvariantCultureIgnoreCase))
			{
				// This is probably an ORTS token that we haven't encountered
				// [the ORTS parameters spreadsheet link is broken]
				// Parse the block, but do nothing at the minute
				return KujuTokenID.ORTSUnknown;
			}

			// Completely unknown token
			// NOTE: A lot of MSTS stuff contains textually edited typos
			// At the minute, typos are being added to the token list
			// Drawback of using an enum method for this :/
			throw new InvalidDataException("Unrecognised token " + s);
#endif
		}

		private TextualBlock(string text, bool textIsClean)
		{
			if (!textIsClean)
			{
				char[] fixedText = new char[text.Length];
				int newTextLength = 0;
				text = text.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ").Replace("\t", " ").Trim(new char[] { });
				text = text.Replace(@"\(", "[").Replace(@"\)", "]");
				bool lastWhiteSpace = false;
				for (int i = 0; i < text.Length; i++)
				{
					if (text[i] == '(' || text[i] == ')')
					{
						if (!lastWhiteSpace)
						{
							fixedText[newTextLength++] = ' ';
						}
						fixedText[newTextLength++] = text[i];
						fixedText[newTextLength++] = ' ';
						lastWhiteSpace = true;
					}
					else if (char.IsWhiteSpace(text[i]))
					{
						if (!lastWhiteSpace)
						{
							fixedText[newTextLength++] = ' ';
							lastWhiteSpace = true;
						}
					}
					else
					{
						fixedText[newTextLength++] = text[i];
						lastWhiteSpace = false;
					}
					if (newTextLength == fixedText.Length - 1 && i != text.Length - 1)
					{
						Array.Resize(ref fixedText, fixedText.Length << 2);
					}
				}
				myText = new string(fixedText, 0, newTextLength);
			}
			else
			{
				myText = text;
			}
			currentPosition = 0;
		}

		public TextualBlock(string text, KujuTokenID token, bool textIsClean = false, Block parentBlock = null)
		{
			if (text.Length == 0)
			{
				return;
			}

			if (!textIsClean)
			{
				char[] fixedText = new char[text.Length];
				int newTextLength = 0;
				text = text.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ").Replace("\t", " ").Trim(new char[] { });
				text = text.Replace(@"\(", "[").Replace(@"\)", "]");
				bool lastWhiteSpace = false;
				for (int i = 0; i < text.Length; i++)
				{
					if (text[i] == '(' || text[i] == ')')
					{
						if (!lastWhiteSpace)
						{
							fixedText[newTextLength++] = ' ';
						}
						fixedText[newTextLength++] = text[i];
						fixedText[newTextLength++] = ' ';
						lastWhiteSpace = true;
					}
					else if (char.IsWhiteSpace(text[i]))
					{
						if (!lastWhiteSpace)
						{
							fixedText[newTextLength++] = ' ';
							lastWhiteSpace = true;
						}
					}
					else
					{
						fixedText[newTextLength++] = text[i];
						lastWhiteSpace = false;
					}
					if (newTextLength == fixedText.Length - 1 && i != text.Length - 1)
					{
						Array.Resize(ref fixedText, fixedText.Length << 2);
					}
				}
				myText = new string(fixedText, 0, newTextLength);
			}
			else
			{
				myText = text;
			}

			Token = token;
			currentPosition = 0;
			Label = string.Empty;
			while (myText[currentPosition] != '(')
			{
				currentPosition++;
			}

			if (currentPosition == 0)
			{
				throw new InvalidDataException("Token " + token + " was not found.");
			}

			string s = myText.Substring(0, currentPosition);
			int ws = s.IndexOf(' ');
			if (ws != -1)
			{
				//The block has the optional label
				Label = s.Substring(ws, s.Length - ws).Trim(new char[] { });
				s = s.Substring(0, ws);
			}

			KujuTokenID currentToken = ParseToken(s);

			if (currentToken != Token)
			{
				throw new InvalidDataException("Expected the " + Token + " token, got " + currentToken);
			}

			currentPosition++;
			ParentBlock = parentBlock;
		}

		public static Dictionary<KujuTokenID, Block> ReadBlocks(string text, KujuTokenID[] validTokens)
		{
			Block b = new TextualBlock(text, false);
			Dictionary<KujuTokenID, Block> readBlocks = new Dictionary<KujuTokenID, Block>();
			while (b.Position() < b.Length() - 1)
			{
				try
				{
					Block sb = b.ReadSubBlock(validTokens);
					readBlocks.Add(sb.Token, sb);
				}
				catch
				{
					// Ignore: Some of these seem to have been textually edited, and MSTS does
				}
			}
			return readBlocks;
		}

		public static List<Block> ReadBlocks(string text)
		{
			Block b = new TextualBlock(text, false);
			List<Block> readBlocks = new List<Block>();
			while (b.Position() < b.Length() - 1)
			{
				try
				{
					Block sb = b.ReadSubBlock(true);
					readBlocks.Add(sb);
				}
				catch
				{
					// Ignore: Some of these seem to have been textually edited, and MSTS does
				}
			}
			return readBlocks;
		}

		public override Block ReadSubBlock(KujuTokenID newToken)
		{
			startPosition = currentPosition;
			string s = string.Empty;
			while (currentPosition < myText.Length)
			{
				if (myText[currentPosition] == '(')
				{
					int l = currentPosition - startPosition;
					s = myText.Substring(startPosition, l).Trim(new char[] { });
					currentPosition++;
					break;
				}

				currentPosition++;
			}

			int ws = s.IndexOf(' ');
			if (ws != -1)
			{
				//The block has the optional label
				Label = s.Substring(ws, s.Length - ws).Trim(new char[] { });
				s = s.Substring(0, ws);
			}

			KujuTokenID currentToken = ParseToken(s);

			if (newToken != currentToken)
			{
				throw new InvalidDataException("Expected the " + newToken + " token, got " + currentToken);
			}

			int level = 0;
			while (currentPosition < myText.Length)
			{
				if (myText[currentPosition] == '(')
				{
					level++;
				}

				if (myText[currentPosition] == ')')
				{
					currentPosition++;
					if (level == 0)
					{
						return new TextualBlock(myText.Substring(startPosition, currentPosition - startPosition).Trim(new char[] { }), newToken, true, this);
					}

					level--;

				}

				currentPosition++;
			}

			throw new InvalidDataException("Unexpected end of block in " + Token);
		}

		public override Block GetSubBlock(KujuTokenID newToken)
		{
			int position = currentPosition;
			while (double.TryParse(getNextValue(), out _))
			{
				position = currentPosition;
			}

			currentPosition = position;
			return ReadSubBlock(newToken);
		}

		public override Block ReadSubBlock(KujuTokenID[] validTokens)
		{
			startPosition = currentPosition;
			string s = String.Empty;
			while (currentPosition < myText.Length)
			{
				if (myText[currentPosition] == '(')
				{
					int l = currentPosition - startPosition;
					s = myText.Substring(startPosition, l).Trim(new char[] { });
					currentPosition++;
					break;
				}

				currentPosition++;
			}

			int ws = s.IndexOf(' ');
			if (ws != -1)
			{
				//The block has the optional label
				Label = s.Substring(ws, s.Length - ws).Trim(new char[] { });
				s = s.Substring(0, ws);
			}

			KujuTokenID currentToken = ParseToken(s);

			if (!validTokens.Contains(currentToken))
			{
				if (currentToken != KujuTokenID.Comment)
				{
					// comment is always valid and will be discarded by the block reader
					throw new InvalidDataException("Expected one of the following tokens: " + validTokens + " , got " + currentToken);
				}
			}

			int level = 0;
			while (currentPosition < myText.Length)
			{
				if (myText[currentPosition] == '(')
				{
					level++;
				}

				if (myText[currentPosition] == ')')
				{
					currentPosition++;
					if (level == 0)
					{
						if (currentToken == KujuTokenID.Comment && !validTokens.Contains(KujuTokenID.Comment))
						{
							// found comment block which we don't want to read- retry
							return ReadSubBlock(validTokens);
						}

						return new TextualBlock(myText.Substring(startPosition, currentPosition - startPosition).Trim(new char[] { }), currentToken, true, this);
					}

					level--;

				}

				currentPosition++;
			}

			throw new InvalidDataException("Unexpected end of block in " + Token);
		}

		public override Block ReadSubBlock(bool allowEmptyBlock = false)
		{
			startPosition = currentPosition;
			string s = string.Empty;
			while (currentPosition < myText.Length)
			{
				if (myText[currentPosition] == '(')
				{
					int l = currentPosition - startPosition;
					s = myText.Substring(startPosition, l).Trim(new char[] { });
					currentPosition++;
					break;
				}

				currentPosition++;
			}

			if (string.IsNullOrWhiteSpace(s) || s == ")")
			{
				// empty string or 'extra' closing brackets
				if (!allowEmptyBlock)
				{
					throw new InvalidDataException("Empty sub-block");
				}

				TextualBlock t = new TextualBlock("", true);
				t.Token = KujuTokenID.Skip;
				return t;
			}

			int sl = s.Length;
			s = s.TrimStart(')').TrimStart(); // remove all other extraneous brackets + whitespace
			startPosition += sl - s.Length;

			string ns = s;
			int ws = s.IndexOf(' ');
			if (ws != -1)
			{
				//The block has the optional label
				Label = s.Substring(ws, s.Length - ws).Trim(new char[] { });
				if (Label[0] == ')')
				{
					if (allowEmptyBlock)
					{
						TextualBlock t = new TextualBlock("", true);
						t.Token = KujuTokenID.Skip;
						return t;
					}
					throw new InvalidDataException("Unexpected extra closing bracket encountered.");
				}
				s = s.Substring(0, ws);
			}

			KujuTokenID currentToken = ParseToken(s);
			
			int level = 0;
			while (currentPosition < myText.Length)
			{
				if (myText[currentPosition] == '(')
				{
					level++;
				}

				if (myText[currentPosition] == ')')
				{
					currentPosition++;
					if (level == 0)
					{
						if (currentToken == KujuTokenID.Skip)
						{
							return new TextualBlock(string.Empty, true);
						}

						return new TextualBlock(myText.Substring(startPosition, currentPosition - startPosition).Trim(new char[] { }), currentToken, true, this);
					}
					level--;
				}
				currentPosition++;
			}

			if (currentPosition == myText.Length)
			{
				// Missing one or more block terminators- PadRight with appropriate number
				return new TextualBlock(myText.Substring(startPosition, currentPosition - startPosition).Trim(new char[] { }).PadRight(currentPosition - startPosition + level, ')'), currentToken, true, this);
			}
			throw new InvalidDataException("Unexpected end of block in " + Token);
		}


		private int startPosition;

		public override ushort ReadUInt16()
		{
			startPosition = currentPosition;
			if (char.IsWhiteSpace(myText[currentPosition]))
			{
				//Skip the rest of the spaces
				while (char.IsWhiteSpace(myText[currentPosition]))
				{
					currentPosition++;
				}
			}

			string s = getNextValue();
			if (int.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out int val))
			{
				return (ushort) val;
			}

			throw new InvalidDataException("Unable to parse " + s + " to a valid integer in block " + Token);
		}

		public override uint ReadUInt32()
		{
			startPosition = currentPosition;
			if (char.IsWhiteSpace(myText[currentPosition]))
			{
				//Skip the rest of the spaces
				while (char.IsWhiteSpace(myText[currentPosition]))
				{
					currentPosition++;
				}
			}

			string s = getNextValue();
			if (uint.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint val))
			{
				return val;
			}

			throw new InvalidDataException("Unable to parse " + s + " to a valid integer in block " + Token);
		}

		public override int ReadInt32()
		{
			startPosition = currentPosition;
			if (char.IsWhiteSpace(myText[currentPosition]))
			{
				//Skip the rest of the spaces
				while (char.IsWhiteSpace(myText[currentPosition]))
				{
					currentPosition++;
				}
			}


			string s = getNextValue();
			if (NumberFormats.TryParseIntVb6(s, out int val))
			{
				return val;
			}

			throw new InvalidDataException("Unable to parse " + s + " to a valid integer in block " + Token);
		}

		public override int ReadInt16()
		{
			startPosition = currentPosition;
			if (char.IsWhiteSpace(myText[currentPosition]))
			{
				//Skip the rest of the spaces
				while (char.IsWhiteSpace(myText[currentPosition]))
				{
					currentPosition++;
				}
			}

			string s = getNextValue();
			if (NumberFormats.TryParseIntVb6(s, out int val))
			{
				return val;
			}

			throw new InvalidDataException("Unable to parse " + s + " to a valid integer in block " + Token);
		}

		public override float ReadSingle()
		{
			startPosition = currentPosition;
			while (!char.IsWhiteSpace(myText[currentPosition]))
			{
				currentPosition++;
			}

			string s = getNextValue();
			if (s[s.Length -1] == ',')
			{
                // SMS files contain comma separated numbers in a textual CurvePoints block
				s = s.Substring(0, s.Length - 1);
			}
			if (float.TryParse(s, NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out float val))
			{
				return val;
			}

			// try ignoring numbers after the second comma if applicable
			if (s.Split(',').Length > 2)
			{
				s = s.Substring(0, s.IndexOf(',', 0, 2));
				if (float.TryParse(s, NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out val))
				{
					return val;
				}
			}

			// try ignoring numbers after the second decimal point if applicable
			if (s.Split('.').Length > 2)
			{
				s = s.Substring(0, s.IndexOf('.', s.IndexOf('.') + 1));
				if (float.TryParse(s, NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out val))
				{
					return val;
				}
			}

			throw new InvalidDataException("Unable to parse " + s + " to a valid single in block " + Token);
		}

		public override Color32 ReadColorArgb()
		{
			float a = 1.0f, r = 1.0f, g = 1.0f, b = 1.0f;
			try
			{
				a = ReadSingle();
				r = ReadSingle();
				g = ReadSingle();
				b = ReadSingle();
			}
			catch
			{
				// ignored
			}
			return new Color32((byte)(r * 255), (byte)(g * 255), (byte)(b * 255), (byte)(a * 255));
		}

		public override float ReadSingle<TUnitType>(TUnitType desiredUnit, TUnitType? defaultUnits)
		{
			string s = ReadString();
			int hash = s.IndexOf('#');
			if (hash != -1)
			{
				// In unit deliminated strings, hash acts as a comment separator (despite being valid as a string member elsewhere)
				s = s.Substring(0, hash).Trim();
			}
			int c;
			for (c = 0; c < s.Length; c++)
			{
				if (!char.IsNumber(s[c]) && s[c] != '.' && s[c] != 'e' && s[c] != '-')
				{
					break;
				}
			}


			string Unit = s.Substring(c).ToLowerInvariant().Replace("/", string.Empty);

			if (string.IsNullOrEmpty(Unit))
			{
				// assume that if no units are specified, our number is already in the desired unit e.g. Dash9.eng
				Unit = (defaultUnits != null ? defaultUnits.ToString(): desiredUnit.ToString()).ToLowerInvariant();
			}
			s = s.Substring(0, c);
			
			if (!float.TryParse(s, out float parsedNumber))
			{
				throw new InvalidDataException("Unable to parse " + s + " to a valid single in block " + Token);
			}

			if (desiredUnit is UnitOfLength)
			{
				if (!LengthConverter.KnownUnits.ContainsKey(Unit))
				{
					throw new InvalidDataException("Unknown or unexpected length unit " + Unit + " encountered in block " + Token);
				}

				parsedNumber = (float)lengthConverter.Convert(parsedNumber, LengthConverter.KnownUnits[Unit], (UnitOfLength)(object)desiredUnit);
			}
			else if (desiredUnit is UnitOfWeight)
			{
				if (!WeightConverter.KnownUnits.ContainsKey(Unit))
				{
					throw new InvalidDataException("Unknown or unexpected weight unit " + Unit + " encountered in block " + Token);
				}

				parsedNumber = (float)weightConverter.Convert(parsedNumber, WeightConverter.KnownUnits[Unit], (UnitOfWeight)(object)desiredUnit);
			}
			else if (desiredUnit is UnitOfForce)
			{
				if (!ForceConverter.KnownUnits.ContainsKey(Unit))
				{
					throw new InvalidDataException("Unknown or unexpected force unit " + Unit + " encountered in block " + Token);
				}

				parsedNumber = (float)forceConverter.Convert(parsedNumber, ForceConverter.KnownUnits[Unit], (UnitOfForce)(object)desiredUnit);
			}
			else if (desiredUnit is UnitOfVolume)
			{
				if (!VolumeConverter.KnownUnits.ContainsKey(Unit))
				{
					throw new InvalidDataException("Unknown or unexpected volume unit " + Unit + " encountered in block " + Token);
				}

				parsedNumber = (float)volumeConverter.Convert(parsedNumber, VolumeConverter.KnownUnits[Unit], (UnitOfVolume)(object)desiredUnit);
			}
			else if (desiredUnit is UnitOfCurrent)
			{
				if (!CurrentConverter.KnownUnits.ContainsKey(Unit))
				{
					throw new InvalidDataException("Unknown or unexpected current unit " + Unit + " encountered in block " + Token);
				}

				parsedNumber = (float)currentConverter.Convert(parsedNumber, CurrentConverter.KnownUnits[Unit], (UnitOfCurrent)(object)desiredUnit);
			}
			else if (desiredUnit is UnitOfPressure)
			{
				if (!PressureConverter.KnownUnits.ContainsKey(Unit))
				{
					throw new InvalidDataException("Unknown or unexpected pressure unit " + Unit + " encountered in block " + Token);
				}

				parsedNumber = (float)pressureConverter.Convert(parsedNumber, PressureConverter.KnownUnits[Unit], (UnitOfPressure)(object)desiredUnit);

			}
			else if (desiredUnit is UnitOfVelocity)
			{
				if (!VelocityConverter.KnownUnits.ContainsKey(Unit))
				{
					throw new InvalidDataException("Unknown or unexpected velocity unit " + Unit + " encountered in block " + Token);
				}

				parsedNumber = (float)velocityConvertor.Convert(parsedNumber, VelocityConverter.KnownUnits[Unit], (UnitOfVelocity)(object)desiredUnit);
			}
			else if (desiredUnit is UnitOfTorque)
			{
				if (!TorqueConverter.KnownUnits.ContainsKey(Unit))
				{
					if (VelocityConverter.KnownUnits.ContainsKey(Unit))
					{
						/*
						 * HACK: If we're using a velocity unit here, torque is usually expressed as a units in 1 figure.
						 *		 Therefore, the velocity to m/s
						 */
						parsedNumber = (float)velocityConvertor.Convert(parsedNumber, VelocityConverter.KnownUnits[Unit], UnitOfVelocity.MetersPerSecond);
						Unit = "nms";
					}
					else if (Unit == "s")
					{
						// ?? assume that plain s is actually newton meters / s ??
						Unit = "nms";
					}
					else
					{
						throw new InvalidDataException("Unknown or unexpected torque unit " + Unit + " encountered in block " + Token);
					}
				}

				parsedNumber = (float)torqueConvertor.Convert(parsedNumber, TorqueConverter.KnownUnits[Unit], (UnitOfTorque)(object)desiredUnit);
			}
			return parsedNumber;
		}
		
		public override void Skip(int length)
		{
			//Unused at the minute
		}

		public override string ReadString()
		{
			startPosition = currentPosition;
			while (!char.IsWhiteSpace(myText[currentPosition]) && currentPosition < myText.Length - 1)
			{
				currentPosition++;
			}

			return getNextValue();
		}

		public override bool ReadPath(string absolute, out string finalPath)
		{
			string relative = ReadString().Replace("\"", "");
			try
			{
				finalPath = OpenBveApi.Path.CombineFile(absolute, relative);
			}
			catch
			{
				finalPath = relative;
				return false;
			}
			
			if (File.Exists(finalPath))
			{
				return true;
			}

			// n.b. Sound files may also be loaded from the global SOUND directory

			// ReSharper disable once InconsistentNaming
			string MSTSInstallDirectory = string.Empty;

			try
			{
				object o = Registry.GetValue("HKEY_LOCAL_MACHINE\\Software\\Microsoft Games\\Train Simulator\\1.0", "Path", null);
				if (o is string msts)
				{
					MSTSInstallDirectory = msts;
					
				}
				o = Registry.GetValue("HKEY_CURRENT_USER\\Software\\OpenRails\\ORTS\\Folders", "Train Simulator", null);
				if (o is string orts)
				{
					MSTSInstallDirectory = orts;
				}
			}
			catch
			{
				// ignored
			}

			absolute = OpenBveApi.Path.CombineDirectory(MSTSInstallDirectory, "SOUND");
			try
			{
				finalPath = OpenBveApi.Path.CombineFile(absolute, relative);
			}
			catch
			{
				finalPath = relative;
				return false;
			}

			if (File.Exists(finalPath))
			{
				return true;
			}

			finalPath = relative;
			return false;
		}

		public override string[] ReadStringArray()
		{
			List<string> strings = new List<string>();
			while (true)
			{
				if (Position() == Length())
				{
					break;
				}
				string s = ReadString();
				if (s != string.Empty)
				{
					strings.Add(s);
				}
				else
				{
					break;
				}
			}

			if (strings.Count == 1 && strings[0].IndexOf(',') != -1)
			{
				// Quote enclosed, comma separated as opposed to whitespace separated
				return strings[0].Split(',');
			}
			return strings.ToArray();

		}

		public override TEnumType[] ReadEnumArray<TEnumType>(TEnumType desiredEnumType)
		{
			string[] strings = ReadStringArray();
			TEnumType[] returnArray = new TEnumType[strings.Length];
			for (int i = 0; i < strings.Length; i++)
			{
				strings[i] = strings[i].Replace("Handbrake Only", "Handbrake"); // UKTS BR_Bolster_D
				if (!Enum.TryParse(strings[i], true, out returnArray[i]))
				{
					if (string.IsNullOrEmpty(strings[i]))
					{
						returnArray[i] = default(TEnumType); // MT Class 101
					}
					else
					{
						throw new InvalidDataException("Expected " + strings[i] + " to be a value member of the specified enum.");
					}
					
				}
			}
			return returnArray;
		}

		public override TEnumType ReadEnumValue<TEnumType>(TEnumType desiredEnumType)
		{
			string s = ReadString();
			s = s.Replace("/", "_");
			s = Replace(s, "METRES", "METERS"); // accept both spellings of meter
			s = Replace(s, "SEC_SEC", "SEC"); // 3DTS Class 108
			s = Replace(s, "12", "TWELVE"); // can't start an enum value with a number, so replace with textual
			s = Replace(s, "24", "TWENTYFOUR"); // ditto
			if (!Enum.TryParse(s, true, out TEnumType e))
			{
				throw new InvalidDataException("Expected " + s + " to be a value member of the specified enum.");
			}
			return e;
		}

		private static string Replace(string str, string oldValue, string newValue)
		{
			// Helper method to case invariant replace string within a string
			// This isn't available before .Net Core
			if(string.IsNullOrEmpty(oldValue))
			{
				return str;
			}
			
			StringBuilder sb = new StringBuilder();

			int previousIndex = 0;
			int index = str.IndexOf(oldValue, StringComparison.InvariantCultureIgnoreCase);
			while (index != -1)
			{
				sb.Append(str.Substring(previousIndex, index - previousIndex));
				sb.Append(newValue);
				index += oldValue.Length;

				previousIndex = index;
				index = str.IndexOf(oldValue, index, StringComparison.InvariantCultureIgnoreCase);
			}
			sb.Append(str.Substring(previousIndex));

			return sb.ToString();
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

			if (char.IsWhiteSpace(myText[currentPosition]) && currentPosition < myText.Length - 1)
			{
				while (char.IsWhiteSpace(myText[currentPosition]))
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
				
				while (myText[currentPosition] != '"' && currentPosition < myText.Length - 1)
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
				while (!char.IsWhiteSpace(myText[currentPosition]) && currentPosition < myText.Length - 1)
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
}
