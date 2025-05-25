//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2020, S520, The OpenBVE Project
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
using System.Linq;
using Bve5_Parsing.MapGrammar.EvaluateData;
using OpenBveApi;
using OpenBveApi.Interface;
using OpenBveApi.Objects;
using Path = OpenBveApi.Path;

namespace Route.Bve5
{
	static partial class Bve5ScenarioParser
	{
		private static void LoadStructureList(string FileName, bool PreviewOnly, MapData ParseData, RouteData RouteData)
		{
			RouteData.Objects = new ObjectDictionary();

			if (PreviewOnly || string.IsNullOrEmpty(ParseData.StructureListPath))
			{
				return;
			}

			string structureList = ParseData.StructureListPath;

			if (!File.Exists(structureList))
			{
				structureList = Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), structureList);

				if (!File.Exists(structureList))
				{
					Plugin.CurrentHost.AddMessage(MessageType.Error, true, "The BVE5 Structure List file " + structureList + " was not found.");
					return;
				}
			}

			string BaseDirectory = System.IO.Path.GetDirectoryName(structureList);

			System.Text.Encoding Encoding = Text.DetermineBVE5FileEncoding(structureList);
			string[] Lines = File.ReadAllLines(structureList, Encoding).Select(Line => Line.Trim('"').Trim()).ToArray();
			if (structureList.IndexOf("Tn_E235", StringComparison.InvariantCultureIgnoreCase) != -1 || structureList.IndexOf("TSLSeoul4", StringComparison.InvariantCultureIgnoreCase) != -1)
			{
				// Some routes with badly optimized objects- Use a much lower threshold to avoid killing the renderer
				Plugin.CurrentOptions.ObjectOptimizationBasicThreshold = 2000;
			}
			for (int i = 1; i < Lines.Length; i++)
			{
				//Cycle through the list of objects
				//An object index is formatted as follows:
				// --KEY USED BY ROUTEFILE-- , --PATH TO OBJECT RELATIVE TO STRUCTURE FILE--

				Lines[i] = Lines[i].TrimBVE5Comments();
				if (string.IsNullOrEmpty(Lines[i]))
				{
					continue;
				}

				int a = Lines[i].IndexOf(',');
				string FilePath = Lines[i].Substring(a + 1, Lines[i].Length - a - 1).Trim();

				if (string.IsNullOrEmpty(FilePath) || a == -1)
				{
					// empty object name
					Plugin.CurrentHost.AddMessage(MessageType.Warning, false, "No object file was specified for key " + Lines[i]);
					continue;
				}

				string Key = Lines[i].Substring(0, a).Trim();
				try
				{
					FilePath = Path.CombineFile(BaseDirectory, FilePath);
				}
				catch
				{
					// ignore
				}

				if (!File.Exists(FilePath))
				{
					Plugin.CurrentHost.AddMessage(MessageType.Error, false, "The Object File " + FilePath + " with key " + Key + " was not found.");
					continue;
				}

				System.Text.Encoding ObjectEncoding = TextEncoding.GetSystemEncodingFromFile(FilePath);
				Plugin.CurrentHost.LoadObject(FilePath, ObjectEncoding, out UnifiedObject obj);
				RouteData.Objects.Add(Key, obj);
			}
		}
	}
}
