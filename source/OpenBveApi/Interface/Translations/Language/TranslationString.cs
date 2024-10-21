//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2023, Christopher Lees, The OpenBVE Project
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
using System.Xml.Linq;

namespace OpenBveApi.Interface
{
	/// <summary>A translation string</summary>
	public class TranslationString
	{
		/// <summary>The source string</summary>
		internal string Source;
		/// <summary>The translated string</summary>
		internal string Translation;

		/// <summary>Creates a new TranslationString</summary>
		/// <param name="xmlns">The parent XMLNamespace</param>
		/// <param name="stringElement">The XElement containing the translation string</param>
		public TranslationString(XNamespace xmlns, XElement stringElement)
		{
			XElement source = stringElement.Element(xmlns + "source");
			XElement target = stringElement.Element(xmlns + "target");

			if (source != null)
			{
				Source = ((string)source).Replace("\\r\\n", Environment.NewLine).Replace("\\x20", " ");
			}

			if (target != null)
			{
				Translation =  ((string)target).Replace("\\r\\n", Environment.NewLine).Replace("\\x20", " ");
			}
		}
	}
}
