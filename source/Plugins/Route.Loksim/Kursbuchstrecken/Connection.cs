//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2026, Christopher Lees, The OpenBVE Project
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
using Formats.OpenBve;

namespace LokSimRouteParser
{
	internal class Connection
	{
		internal bool End1;
		internal bool End2;
		internal string Platform1;
		internal string Platform2;
		internal string RouteFile1;
		internal string RouteFile2;
		internal Connection(Block<LoksimNode, LoksimAttribute> connectionBlock, Kursbuchstrecken kursbuchstrecken)
		{
			connectionBlock.GetValue(LoksimAttribute.Ende1, out End1);
			connectionBlock.GetValue(LoksimAttribute.Ende2, out End2);
			connectionBlock.GetValue(LoksimAttribute.Gleis1, out Platform1);
			connectionBlock.GetValue(LoksimAttribute.Gleis2, out Platform2);
			if (!connectionBlock.GetValue(LoksimAttribute.Strecke1, out RouteFile1) || !kursbuchstrecken.Strecken.ContainsKey(RouteFile1))
			{
				throw new Exception("Loksim3D: Strecke1 was missing from the Kursbuchstecken");
			}

			if (!connectionBlock.GetValue(LoksimAttribute.Strecke2, out RouteFile2) || !kursbuchstrecken.Strecken.ContainsKey(RouteFile2))
			{
				throw new Exception("Loksim3D: Strecke2 was missing from the Kursbuchstecken");
			}
		}
	}
}
