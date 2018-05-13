using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenBve
{
	partial class MsTsShapeParser
	{
		internal abstract class Block
		{
			internal KujuTokenID Token;

			internal string Label;

			internal abstract ushort ReadUInt16();

			internal abstract uint ReadUInt32();

			internal abstract int ReadInt32();

			internal abstract int ReadInt16();

			internal abstract byte ReadByte();

			internal abstract float ReadSingle();

			internal abstract void Skip(int length);

			internal abstract string ReadString();

			internal abstract long Length();

			internal abstract long Position();

			internal static Block ReadBlock(byte[] bytes, KujuTokenID token)
			{
				return new BinaryBlock(bytes, token);
			}

			internal static Block ReadBlock(string text, KujuTokenID token)
			{
				return new TextualBlock(text, token);
			}

			internal abstract Block ReadSubBlock(KujuTokenID newToken);

			internal abstract Block ReadSubBlock(KujuTokenID[] validTokens);
		}

		private class BinaryBlock : Block
		{
			private readonly BinaryReader myReader;
			private readonly MemoryStream myStream;
			internal BinaryBlock(byte[] bytes, KujuTokenID token)
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

			internal override Block ReadSubBlock(KujuTokenID newToken)
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

			internal override Block ReadSubBlock(KujuTokenID[] validTokens)
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

			internal override ushort ReadUInt16()
			{
				return myReader.ReadUInt16();
			}

			internal override uint ReadUInt32()
			{
				return myReader.ReadUInt32();
			}

			internal override int ReadInt32()
			{
				return myReader.ReadInt32();
			}

			internal override int ReadInt16()
			{
				return myReader.ReadInt16();
			}

			internal override byte ReadByte()
			{
				return myReader.ReadByte();
			}

			internal override float ReadSingle()
			{
				return myReader.ReadSingle();
			}

			internal override void Skip(int length)
			{
				myReader.ReadBytes(length);
			}

			internal override string ReadString()
			{
				int imageLength = ReadUInt16();
				if (imageLength > 0)
				{
					byte[] buff = new byte[imageLength * 2];
					int i = 0;
					while (i < imageLength * 2)
					{
						buff[i] = ReadByte();
						i++;
					}

					return(Encoding.Unicode.GetString(buff, 0, imageLength * 2));
				}
				return(string.Empty); //Not sure this is valid, but let's be on the safe side
			}

			internal override long Length()
			{
				return myStream.Length;
			}

			internal override long Position()
			{
				return myStream.Position;
			}
		}

		private class TextualBlock : Block
		{
			private readonly string myText;

			private int currentPosition;

			internal TextualBlock(string text, KujuTokenID token)
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

			internal override Block ReadSubBlock(KujuTokenID newToken)
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

			internal override Block ReadSubBlock(KujuTokenID[] validTokens)
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


			private int startPosition;

			internal override ushort ReadUInt16()
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

			internal override uint ReadUInt32()
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
				if (int.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out val))
				{
					return (ushort) val;
				}

				throw new Exception("Unable to parse " + s + " to a valid integer in block " + Token);
			}

			internal override int ReadInt32()
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

			internal override int ReadInt16()
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

			internal override byte ReadByte()
			{
				throw new NotImplementedException();
			}

			internal override float ReadSingle()
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

				throw new Exception("Unable to parse " + s + " to a valid integer in block " + Token);
			}

			internal override void Skip(int length)
			{
				//Unused at the minute
			}

			internal override string ReadString()
			{
				startPosition = currentPosition;
				while (!char.IsWhiteSpace(myText[currentPosition]))
				{
					currentPosition++;
				}

				return getNextValue();
			}

			internal override long Length()
			{
				return myText.Length;
			}

			internal override long Position()
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
				while (!char.IsWhiteSpace(myText[currentPosition]))
				{
					currentPosition++;
				}

				string s = String.Empty;
				int l = currentPosition - startPosition;
				if (l > 0)
				{
					return myText.Substring(startPosition, l).Trim();
				}

				return string.Empty;
			}
		}
	}
}
