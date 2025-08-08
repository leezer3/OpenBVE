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

// ReSharper disable InconsistentNaming
namespace OpenBve.Formats.DirectX
{
	enum TokenID : short
	{
		NAME = 0x01,
		STRING = 0x02,
		INT = 0x03,
		GUID = 0x05,
		INT_LIST = 0x06,
		FLOAT_LIST = 0x07,
		OBRACE = 0x0a,
		CBRACE = 0x0b,
		OBRACKET = 0x0c,
		CBRACKET = 0x0d,
		OSQUARE = 0x0e,
		CSQUARE = 0x0f,
		LTHAN = 0x10,
		GTHAN = 0x11,
		PERIOD = 0x12,
		COMMA = 0x13,
		SEMICOLON = 0x14,
		TEMPLATE = 0x1f,
		WORD = 0x28,
		DWORD = 0x29,
		FLOAT = 0x2a,
		DOUBLE = 0x2b,
		CHAR = 0x2c,
		UCHAR = 0x2d,
		SWORD = 0x2e,
		SDWORD = 0x2f,
		VOID = 0x30,
		STRING2 = 0x31,
		UNICODE = 0x32,
		CSTRING = 0x33,
		ARRAY = 0x34
	}
}
