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
using System.Diagnostics;
using System.IO;
using System.Threading;
using OpenBveApi.FileSystem;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Objects;

namespace Plugin
{
    public class Plugin : ObjectInterface
    {
	    internal static HostInterface CurrentHost;
	    internal static FileSystem CurrentFileSystem;
	    private static XParsers currentXParser = XParsers.Original;
	    internal static CompatabilityHacks EnabledHacks;

	    public override string[] SupportedStaticObjectExtensions => new[] { ".x" };

	    public override void Load(HostInterface host, FileSystem fileSystem) 
	    {
		    CurrentHost = host;
		    CurrentFileSystem = fileSystem;
	    }

	    public override void SetCompatibilityHacks(CompatabilityHacks enabledHacks)
	    {
		    EnabledHacks = enabledHacks;
	    }
		
	    public override void SetObjectParser(object parserType)
	    {
		    if (parserType is XParsers)
		    {
			    currentXParser = (XParsers) parserType;
			    if (currentXParser == XParsers.Original)
			    {
				    CurrentHost.AddMessage(MessageType.Information, false, "The original X Parser has been deprecated- Using the NewXParser");
			    }
		    }
	    }

	    [ThreadStatic]
	    private static int pathRecursions;

	    private static readonly Stopwatch wallClock = Stopwatch.StartNew();

	    public override bool CanLoadObject(string path)
	    {
		    if (string.IsNullOrEmpty(path) || !File.Exists(path) || pathRecursions > 2)
		    {
			    pathRecursions = 0;
			    return false;
		    }
		    byte[] Data = File.ReadAllBytes(path);
		    if (Data.Length < 16 || Data[0] != 120 | Data[1] != 111 | Data[2] != 102 | Data[3] != 32)
		    {
			    string potentialPath = System.Text.Encoding.ASCII.GetString(Data);
			    if (!OpenBveApi.Path.ContainsInvalidChars(potentialPath) && !string.IsNullOrEmpty(potentialPath))
			    {
				    pathRecursions++;
				    return CanLoadObject(OpenBveApi.Path.CombineFile(Path.GetDirectoryName(path), potentialPath));
			    }
			    // not an x object
			    return false;
		    }

		    if (Data[4] != 48 | Data[5] != 51 | Data[6] != 48 | Data[7] != 50 & Data[7] != 51)
		    {
			    // unrecognized version
			    pathRecursions = 0;
			    return false;
		    }

		    // floating-point format
		    if (Data[12] == 48 & Data[13] == 48 & Data[14] == 51 & Data[15] == 50)
		    {
				//32-bit FP
		    }
		    else if (Data[12] == 48 & Data[13] == 48 & Data[14] == 54 & Data[15] == 52)
		    {
				//64-bit FP
		    }
		    else
		    {
			    pathRecursions = 0;
			    return false;
		    }
		    pathRecursions = 0;
		    return true;
	    }

	    public override bool LoadObject(string path, System.Text.Encoding textEncoding, out UnifiedObject unifiedObject)
	    {
		    switch (currentXParser)
		    {
			    case XParsers.Original:
			    case XParsers.NewXParser:
				    try
				    {
				    Stopwatch sw = Stopwatch.StartNew();
				    unifiedObject = NewXParser.ReadObject(path, textEncoding);
				    sw.Stop();
				    System.Threading.Interlocked.Increment(ref NewXParser.TotalCount);
				    System.Threading.Interlocked.Add(ref NewXParser.TotalLoadObjectMs, sw.Elapsed.Ticks);
				    if (sw.ElapsedMilliseconds > 25)
				    {
					    CurrentFileSystem.AppendToLogFile("Slow .x object: " + path + " (" + sw.ElapsedMilliseconds + " ms)");
				    }
				    if (NewXParser.TotalCount % 100 == 0)
				    {
					    double untimed = NewXParser.TicksToMs(NewXParser.TotalReadObjectMs - (NewXParser.TotalReadMs + NewXParser.TotalPreprocessMs + NewXParser.TotalParseMs + NewXParser.TotalApplyMs));
					    CurrentFileSystem.AppendToLogFile("XParser " + NewXParser.TotalCount + " objects: wall " + wallClock.Elapsed.TotalMilliseconds.ToString("F0") + " ms, load " + NewXParser.TicksToMs(NewXParser.TotalLoadObjectMs).ToString("F0") + " ms, read " + NewXParser.TicksToMs(NewXParser.TotalReadMs).ToString("F0") + " ms, prep " + NewXParser.TicksToMs(NewXParser.TotalPreprocessMs).ToString("F0") + " ms, parse " + NewXParser.TicksToMs(NewXParser.TotalParseMs).ToString("F0") + " ms, apply " + NewXParser.TicksToMs(NewXParser.TotalApplyMs).ToString("F0") + " ms, untimed " + untimed.ToString("F0") + " ms");
					    }
					    return true;
				    }
				    catch
				    {
					    unifiedObject = null;
					    return false;
				    }
			    case XParsers.Assimp:
				    try
				    {
					    unifiedObject = AssimpXParser.ReadObject(path);
					    return true;
				    }
				    catch
				    {
					    unifiedObject = null;
					    return false;
				    }
		    }
			unifiedObject = null;
		    return false;
	    }
    }
}
