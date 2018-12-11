// Open Asset Import Library (assimp)
//
// Copyright (c) 2006-2016, assimp team, 2018, The openBVE Project
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms,
// with or without modification, are permitted provided that the
// following conditions are met:
//
// * Redistributions of source code must retain the above
//   copyright notice, this list of conditions and the
//   following disclaimer.
//
// * Redistributions in binary form must reproduce the above
//   copyright notice, this list of conditions and the
//   following disclaimer in the documentation and/or other
//   materials provided with the distribution.
//
// * Neither the name of the assimp team, nor the names of its
//   contributors may be used to endorse or promote products
//   derived from this software without specific prior
//   written permission of the assimp team.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
//
//
// ******************************************************************************
//
// AN EXCEPTION applies to all files in the ./test/models-nonbsd folder.
// These are 3d models for testing purposes, from various free sources
// on the internet. They are - unless otherwise stated - copyright of
// their respective creators, which may impose additional requirements
// on the use of their work. For any of these models, see
// <model-name>.source.txt for more legal information. Contact us if you
// are a copyright holder and believe that we credited you inproperly or
// if you don't want your files to appear in the repository.
//
//
// ******************************************************************************
//
// Poly2Tri Copyright (c) 2009-2010, Poly2Tri Contributors
// http://code.google.com/p/poly2tri/
//
// All rights reserved.
// Redistribution and use in source and binary forms, with or without modification,
// are permitted provided that the following conditions are met:
//
// * Redistributions of source code must retain the above copyright notice,
//   this list of conditions and the following disclaimer.
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
// * Neither the name of Poly2Tri nor the names of its contributors may be
//   used to endorse or promote products derived from this software without specific
//   prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
// CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
// EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
// PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
// PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
// LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
// NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.Linq;

namespace AssimpNET.Obj
{
	public class ObjTools
	{
		protected string Buffer;

		//! Iterator to current position in buffer
		protected int DataIt;

		//! Iterator to end position of buffer
		protected int DataEnd;

		//! Pointer to model instance
		protected Model Model;

		//! Current line (for debugging)
		protected uint Line;

		protected bool IsLineEnd(int tmp)
		{
			return tmp == DataEnd;
		}

		protected bool IsSpaceOrNewLine(int tmp)
		{
			return IsLineEnd(tmp) || char.IsWhiteSpace(Buffer[tmp]);
		}

		protected bool SkipSpaces(int tmp, out int result)
		{
			while (!IsLineEnd(tmp) && (Buffer[tmp] == ' ' || Buffer[tmp] == '\t'))
			{
				++tmp;
			}
			result = tmp;
			return !IsLineEnd(tmp);
		}

		protected bool SkipSpaces(ref int tmp)
		{
			return SkipSpaces(tmp, out tmp);
		}

		protected bool IsDataDefinitionEnd(int tmp)
		{
			if (Buffer[tmp] == '\\')
			{
				tmp++;
				if (IsLineEnd(tmp))
				{
					tmp++;
					return true;
				}
			}
			return false;
		}

		protected void SkipToken(ref int tmp)
		{
			SkipSpaces(ref tmp);
			while (!IsSpaceOrNewLine(tmp))
			{
				++tmp;
			}
		}

		protected int SkipLine(int tmp, int end, ref uint line)
		{
			while (!IsEndOfBuffer(tmp, end) && !IsLineEnd(tmp))
			{
				++tmp;
			}

			if (tmp != end)
			{
				++tmp;
				++line;
			}
			// fix .. from time to time there are spaces at the beginning of a material line
			while (tmp != end && (Buffer[tmp] == '\t' || Buffer[tmp] == ' '))
			{
				++tmp;
			}

			return tmp;
		}

		protected bool IsEndOfBuffer(int tmp, int end)
		{
			if (tmp == end)
			{
				return true;
			}
			else
			{
				--end;
			}
			return tmp == end;
		}

		/// Get the number of components in a line.
		protected int GetNumComponentsInDataDefinition()
		{
			int numComponents = 0;
			int tmp = DataIt;
			bool end_of_definition = false;
			while (!end_of_definition)
			{
				if (IsDataDefinitionEnd(tmp))
				{
					tmp += 2;
				}
				else if (IsLineEnd(tmp))
				{
					end_of_definition = true;
				}
				if (!SkipSpaces(ref tmp))
				{
					break;
				}
				bool isNum = char.IsDigit(Buffer[tmp]) || Buffer[tmp] == '+' || Buffer[tmp] == '-';
				SkipToken(ref tmp);
				if (isNum)
				{
					++numComponents;
				}
				if (!SkipSpaces(ref tmp))
				{
					break;
				}
			}
			return numComponents;
		}

		protected int GetNextWord(int tmp, int end)
		{
			while (!IsEndOfBuffer(tmp, end))
			{
				if (!IsSpaceOrNewLine(tmp))
				{
					break;
				}
				tmp++;
			}
			return tmp;
		}

		protected void CopyNextWord(out string result)
		{
			result = string.Empty;
			DataIt = GetNextWord(DataIt, DataEnd);
			if (Buffer[DataIt] == '\\')
			{
				DataIt += 2;
				DataIt = GetNextWord(DataIt, DataEnd);
			}
			while (DataIt != DataEnd && !IsSpaceOrNewLine(DataIt))
			{
				result += Buffer[DataIt];
				++DataIt;
			}
		}

		protected void GetFloat(out float result)
		{
			string tmp;
			CopyNextWord(out tmp);
			result = float.Parse(tmp);
		}

		protected int GetNextToken(int tmp, int end)
		{
			while (!IsEndOfBuffer(tmp, end))
			{
				if (IsSpaceOrNewLine(tmp))
				{
					break;
				}
				tmp++;
			}
			return GetNextWord(tmp, end);
		}

		protected int GetName(int tmp, int end, out string name)
		{
			name = string.Empty;
			if (IsEndOfBuffer(tmp, end))
			{
				return end;
			}

			int start = tmp;
			while (!IsEndOfBuffer(tmp, end) && !IsLineEnd(tmp))
			{
				++tmp;
			}

			while (char.IsWhiteSpace(Buffer[tmp]))
			{
				--tmp;
			}
			++tmp;

			// Get name
			// if there is no name, and the previous char is a separator, come back to start
			while (tmp < start)
			{
				++tmp;
			}
			name = Buffer.Substring(start, tmp - start);
			return tmp;
		}

		protected void GetNameNoSpace(int tmp, int end, out string name)
		{
			name = string.Empty;
			if (IsEndOfBuffer(tmp, end))
			{
				return;
			}

			int start = tmp;
			while (!IsEndOfBuffer(tmp, end) && !IsLineEnd(tmp) && !IsSpaceOrNewLine(tmp))
			{
				++tmp;
			}

			while (IsEndOfBuffer(tmp, end) || IsLineEnd(tmp) || IsSpaceOrNewLine(tmp))
			{
				--tmp;
			}
			++tmp;

			// Get name
			// if there is no name, and the previous char is a separator, come back to start
			while (tmp < start)
			{
				++tmp;
			}
			name = Buffer.Substring(start, tmp - start);
		}

		protected int Tokenize(string str, out List<string> tokens, string delimiters)
		{
			tokens = new List<string>();

			// Skip delimiters at beginning.
			int lastPos = str.IndexOf(str.FirstOrDefault(ch => !delimiters.Contains(ch)));

			if (lastPos < 0)
			{
				return 0;
			}

			// Find first "non-delimiter".
			int pos = str.IndexOf(str.FirstOrDefault(ch => delimiters.Contains(ch)), lastPos);

			if (pos < 0)
			{
				return 0;
			}

			while (true)
			{
				// Found a token, add it to the vector.
				string tmp = str.Substring(lastPos, pos - lastPos);
				if (tmp.Length != 0 && tmp[0] != ' ')
				{
					tokens.Add(tmp);
				}

				// Skip delimiters.  Note the "not_of"
				tmp = str.Substring(pos);
				lastPos = tmp.IndexOf(tmp.FirstOrDefault(ch => !delimiters.Contains(ch)));

				if (lastPos < 0)
				{
					lastPos = tmp.Length;
				}
				lastPos += pos;

				// Find next "non-delimiter"
				tmp = str.Substring(lastPos);
				pos = tmp.IndexOf(tmp.FirstOrDefault(ch => delimiters.Contains(ch)));

				if (pos < 0)
				{
					pos = tmp.Length;
				}
				pos += lastPos;

				if (pos == lastPos)
				{
					break;
				}
			}

			return tokens.Count;
		}

		protected int ConvertToInt(int position)
		{
			int i = 0;

			bool inv = (Buffer[position] == '-');
			if (inv || Buffer[position] == '+')
			{
				++position;
			}

			if (!char.IsDigit(Buffer[position]))
			{
				throw new Exception("Cannot parse string as real number: does not start with digit or decimal point followed by digit.");
			}

			string tmp = string.Empty;
			while (!IsLineEnd(position))
			{
				if (char.IsDigit((char)Buffer[position]))
				{
					tmp += (char)Buffer[position];
					++position;
				}
				else
				{
					break;
				}
			}

			i = int.Parse(tmp);

			if (inv)
			{
				i = -i;
			}
			return i;
		}
	}
}
