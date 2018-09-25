using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenBve.Formats.MsTs
{
	/// <summary>An abstract block of data, read from a MSTS file</summary>
	public abstract class Block
	{
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

		/// <summary>Reads a sub-block from the enclosing block</summary>
		/// <param name="validTokens">An array containing all possible valid tokens for the new block</param>
		/// <returns>The new block</returns>
		/// <remarks>The type of the new block will always match that of the base block</remarks>
		public abstract Block ReadSubBlock(KujuTokenID[] validTokens);

		/// <summary>Reads any sub-block from the enclosing block</summary>
		/// <returns>The new block</returns>
		/// <remarks>The type of the new block will always match that of the base block</remarks>
		public abstract Block ReadSubBlock();
	}

	/// <inheritdoc />
	public class BinaryBlock : Block
	{
		private readonly BinaryReader myReader;
		private readonly MemoryStream myStream;

		public BinaryBlock(byte[] bytes, KujuTokenID token)
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

				Label = System.Text.Encoding.Unicode.GetString(buff, 0, length * 2);
			}
			else
			{
				Label = string.Empty;
			}
		}

		public override Block ReadSubBlock(KujuTokenID newToken)
		{
			KujuTokenID currentToken = (KujuTokenID) myReader.ReadUInt16();
			if (currentToken != newToken)
			{
				throw new Exception("Expected the " + newToken + " token, got " + currentToken);
			}

			myReader.ReadUInt16();
			uint remainingBytes = myReader.ReadUInt32();
			byte[] newBytes = myReader.ReadBytes((int) remainingBytes);
			return new BinaryBlock(newBytes, newToken);
		}

		public override Block ReadSubBlock(KujuTokenID[] validTokens)
		{
			KujuTokenID currentToken = (KujuTokenID) myReader.ReadUInt16();
			if (!validTokens.Contains(currentToken))
			{
				throw new Exception("Expected one of the following tokens: " + validTokens + ", got " + currentToken);
			}

			myReader.ReadUInt16();
			uint remainingBytes = myReader.ReadUInt32();
			byte[] newBytes = myReader.ReadBytes((int) remainingBytes);
			return new BinaryBlock(newBytes, currentToken);
		}

		public override Block ReadSubBlock()
		{
			KujuTokenID currentToken = (KujuTokenID) myReader.ReadUInt16();
			myReader.ReadUInt16();
			uint remainingBytes = myReader.ReadUInt32();
			byte[] newBytes = myReader.ReadBytes((int) remainingBytes);
			return new BinaryBlock(newBytes, currentToken);
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

				return (Encoding.Unicode.GetString(buff, 0, stringLength * 2));
			}

			return (string.Empty); //Not sure this is valid, but let's be on the safe side
		}

		public override long Length()
		{
			return myStream.Length;
		}

		public override long Position()
		{
			return myStream.Position;
		}
	}

	/// <inheritdoc />
	public class TextualBlock : Block
	{
		private readonly string myText;

		private int currentPosition;

		public TextualBlock(string text, KujuTokenID token)
		{
			myText = text;
			Token = token;
			currentPosition = 0;
			Label = string.Empty;
			while (myText[currentPosition] != '(')
			{
				currentPosition++;
			}

			if (currentPosition == 0)
			{
				throw new Exception("Token " + token + " was not found.");
			}

			string s = myText.Substring(0, currentPosition);
			int ws = s.IndexOf(' ');
			if (ws != -1)
			{
				//The block has the optional label
				Label = s.Substring(ws, s.Length - ws).Trim();
				s = s.Substring(0, ws);
			}

			KujuTokenID currentToken;
			if (!Enum.TryParse(s, true, out currentToken))
			{
				throw new Exception("Invalid token " + s);
			}

			if (currentToken != Token)
			{
				throw new Exception("Expected the " + Token + " token, got " + currentToken);
			}

			currentPosition++;
		}

		public override Block ReadSubBlock(KujuTokenID newToken)
		{
			startPosition = currentPosition;
			string s = String.Empty;
			while (currentPosition < myText.Length)
			{
				if (myText[currentPosition] == '(')
				{
					int l = currentPosition - startPosition;
					s = myText.Substring(startPosition, l).Trim();
					currentPosition++;
					break;
				}

				currentPosition++;
			}

			KujuTokenID currentToken;
			int ws = s.IndexOf(' ');
			if (ws != -1)
			{
				//The block has the optional label
				Label = s.Substring(ws, s.Length - ws).Trim();
				s = s.Substring(0, ws);
			}

			if (!Enum.TryParse(s, true, out currentToken))
			{
				throw new Exception("Unrecognised token " + s);
			}

			if (newToken != currentToken)
			{
				throw new Exception("Expected the " + newToken + " token, got " + currentToken);
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
						return new TextualBlock(myText.Substring(startPosition, currentPosition - startPosition).Trim(), newToken);
					}

					level--;

				}

				currentPosition++;
			}

			throw new Exception("Unexpected end of block in " + Token);
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
					s = myText.Substring(startPosition, l).Trim();
					currentPosition++;
					break;
				}

				currentPosition++;
			}

			KujuTokenID currentToken;
			int ws = s.IndexOf(' ');
			if (ws != -1)
			{
				//The block has the optional label
				Label = s.Substring(ws, s.Length - ws).Trim();
				s = s.Substring(0, ws);
			}

			if (!Enum.TryParse(s, true, out currentToken))
			{
				throw new Exception("Unrecognised token " + s);
			}

			if (!validTokens.Contains(currentToken))
			{
				throw new Exception("Expected one of the following tokens: " + validTokens + " , got " + currentToken);
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
						return new TextualBlock(myText.Substring(startPosition, currentPosition - startPosition).Trim(), currentToken);
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
			string s = String.Empty;
			while (currentPosition < myText.Length)
			{
				if (myText[currentPosition] == '(')
				{
					int l = currentPosition - startPosition;
					s = myText.Substring(startPosition, l).Trim();
					currentPosition++;
					break;
				}

				currentPosition++;
			}

			if (string.IsNullOrWhiteSpace(s))
			{
				throw new Exception();
			}

			KujuTokenID currentToken;
			int ws = s.IndexOf(' ');
			if (ws != -1)
			{
				//The block has the optional label
				Label = s.Substring(ws, s.Length - ws).Trim();
				s = s.Substring(0, ws);
			}

			if (!Enum.TryParse(s, true, out currentToken))
			{
				throw new Exception("Unrecognised token " + s);
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
						return new TextualBlock(myText.Substring(startPosition, currentPosition - startPosition).Trim(), currentToken);
					}

					level--;

				}

				currentPosition++;
			}

			throw new Exception("Unexpected end of block in " + Token);
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
			int val;
			if (int.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out val))
			{
				return (ushort) val;
			}

			throw new Exception("Unable to parse " + s + " to a valid integer in block " + Token);
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
			uint val;
			if (uint.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out val))
			{
				return val;
			}

			throw new Exception("Unable to parse " + s + " to a valid integer in block " + Token);
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
			int val;
			if (int.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out val))
			{
				return val;
			}

			throw new Exception("Unable to parse " + s + " to a valid integer in block " + Token);
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
			while (!char.IsWhiteSpace(myText[currentPosition]))
			{
				currentPosition++;
			}

			string s = getNextValue();
			float val;
			if (float.TryParse(s, NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out val))
			{
				return val;
			}

			throw new Exception("Unable to parse " + s + " to a valid single in block " + Token);
		}

		public override void Skip(int length)
		{
			//Unused at the minute
		}

		public override string ReadString()
		{
			startPosition = currentPosition;
			while (!char.IsWhiteSpace(myText[currentPosition]))
			{
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

			if (char.IsWhiteSpace(myText[currentPosition]))
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
				while (myText[currentPosition] != '"')
				{
					currentPosition++;
				}

				int l = currentPosition - startPosition;
				currentPosition++;
				if (l > 0)
				{
					return myText.Substring(startPosition, l).Trim();
				}
			}
			else
			{
				while (!char.IsWhiteSpace(myText[currentPosition]))
				{
					currentPosition++;
				}

				int l = currentPosition - startPosition;
				if (l > 0)
				{
					return myText.Substring(startPosition, l).Trim();
				}
			}

			return string.Empty;
		}
	}
}
