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

using OpenBveApi.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenBveApi.Math;
using TrainManager.Motor;

namespace Train.OpenBve
{
	internal partial class VehicleTxtParser
	{
		public Bve5PerformanceTable ParsePerformanceTable(string fileName, double minVersion, double maxVersion)
		{
			// bvets vehicle performance table 0.01
			// SPEED KEY ==> P1 --Pn
			// https://note.com/like_a_lake_com/n/nf04df0645f54

			// bvets vehicle performance table 1.00 (K_SEI3500R)
			// SPEED KEY ==> S1a,S1,S2,S3,S4,S5,S6,S7,P1 -- Pn,WF1,WF2,WF3,WF4

			// BveTs Vehicle Performance Table 2.00
			// SPEED KEY ==> P1 -- Pn

			// For power / brake, figure is given in newtons of acceleration (per car?) for each notch
			// For power / brake current tables, figure is given in amps for each notch

			string[] lines = File.ReadAllLines(fileName); // whilst encoding may be specified for these, it shouldn't matter with all numbers
			bool headerOK = false;

			double Version = 0;
			Tuple<double, double[]>[] CurveEntries = null;
			List<Tuple<double, double[]>> tempEntries = new List<Tuple<double, double[]>>();
			for (int i = 0; i < lines.Length; i++)
			{
				int j = lines[i].IndexOf('#');
				if (j >= 0)
				{
					lines[i] = lines[i].Substring(0, j).Trim().Trim(',');
				}
				else
				{
					lines[i] = lines[i].Trim().Trim(',');
				}

				if (headerOK == false)
				{
					int vi = lines[i].Length - 1;
					if (minVersion > 0 || maxVersion > 0)
					{
						while (char.IsDigit(lines[i][vi]) || lines[i][vi] == '.')
						{
							vi--;
						}

						Version = double.Parse(lines[i].Substring(vi));
						lines[i] = lines[i].Substring(0, vi);
						if (Version < minVersion || Version > maxVersion)
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false,
								"Expected a version between " + minVersion + " and " + maxVersion + " , found " +
								Version);
						}
					}

					if (!string.IsNullOrEmpty(lines[i]) && string.Compare(lines[i], "bvets vehicle performance table", StringComparison.OrdinalIgnoreCase) == 0)
					{
						headerOK = true;
					}
				}

				if (lines[i].Length > 0 && char.IsDigit(lines[i][0]))
				{
					if (!headerOK)
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "BVE5: The expected Vehicle Performance Table header was not found.");
						headerOK = true;
					}

					string[] splitLine = lines[i].Split(',');
					if (NumberFormats.TryParseDoubleVb6(splitLine[0], out double speedValue))
					{
						if (splitLine.Length >= 2)
						{
							double[] notchValues = new double[splitLine.Length - 1];
							for (int k = 1; k < splitLine.Length; k++)
							{
								if (!NumberFormats.TryParseDoubleVb6(splitLine[k], out notchValues[k - 1]))
								{
									notchValues[k - 1] = 0;
									Plugin.CurrentHost.AddMessage(MessageType.Error, false, "BVE5: Entry for notch " + (k - 1) + " is not valid at Line " + i + " of Vehicle Performance Table " + fileName);
								}
							}
							tempEntries.Add(new Tuple<double, double[]>(speedValue, notchValues));
						}
						else
						{
							Plugin.CurrentHost.AddMessage(MessageType.Error, false, "BVE5: Entry is not valid at Line " + i + " of Vehicle Performance Table " + fileName);
						}
					}
					else
					{
						Plugin.CurrentHost.AddMessage(MessageType.Error, false, "BVE5: Speed value " + splitLine[0] + " is not valid at Line " + i + " of Vehicle Performance Table " + fileName);
					}
				}
				// sort entries
				CurveEntries = tempEntries.OrderByDescending(x => x.Item1).ToArray();
			}
			return new Bve5PerformanceTable(CurveEntries, Version);
		}
	}
}
