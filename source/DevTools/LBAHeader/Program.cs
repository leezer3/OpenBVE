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
using System.IO;
using Mono.Cecil;

namespace LBAHeader
{
	static class FixLBAHeader
	{
		static byte[] data;
		static void Main(string[] args)
		{
			string f = string.Empty;
			if (args.Length >= 1)
			{
				f = args[0];
			}
			if (!File.Exists(f))
			{
				//If we can't find our initial argument, try and locate the openBVE executable in our directory
				string ff = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
				if (ff == null)
				{
					Console.WriteLine("Unable to find the executing assembly path....");
					return;
				}
				f = Path.Combine(ff, "OpenBve.exe");
			}
			if (!File.Exists(f))
			{
				//Not found, log this rather than crashing
				Console.WriteLine("No suitable executables found....");
				return;
			}
			AddLbaFlag(f);
			Add32BitFlag(f);
			// ReSharper disable once AssignNullToNotNullAttribute
			f = Path.Combine(Path.GetDirectoryName(f), "RouteViewer.exe");
			if (!File.Exists(f))
			{
				//Not found, log this rather than crashing
				Console.WriteLine("RouteViewer executable not found....");
				return;
			}
			AddLbaFlag(f);
			Add32BitFlag(f);
		}

		/// <summary>Marks an executable as LBA aware</summary>
		static void AddLbaFlag(string f)
		{
			Console.WriteLine("Adding LBA Flag to executable {0}", f);
			try
			{
				data = File.ReadAllBytes(f);
				var offset = BitConverter.ToInt32(data, 0x3c);
				//Set LBA Flag for the file supplied via Arguments[0]
				data[offset + 4 + 18] |= 0x20;
				File.WriteAllBytes(f, data);
			}
			catch
			{
				Console.WriteLine("A problem occured whilst attempting to set the LBA flag for executable " + f);
			}
		}
		
		/// <summary>Marks an ANYCPU executable as 32-bit preferred</summary>
		public static void Add32BitFlag(string fileToProcess)
		{
			string configFile = fileToProcess + ".config";
			string finalFileName = fileToProcess.Replace(".exe", "-32.exe");
			Console.WriteLine("Creating 32-bit preferred copy of executable " + fileToProcess);
			try
			{
				ModuleDefinition modDef = ModuleDefinition.ReadModule(fileToProcess);
				if (modDef == null)
				{
					Console.WriteLine("An unexpected error occured whilst attempting to apply Corflags to executable " + fileToProcess);
					return;
				}
				modDef.Attributes |= ModuleAttributes.Required32Bit;
				try
				{
					modDef.Write(finalFileName);
				}
				catch
				{
					Console.WriteLine("Failed to write 32-bit executable to " + finalFileName);
					throw;
				}
			}
			catch (FileNotFoundException)
			{
				Console.WriteLine("Unable to find file " + fileToProcess);
			}
			catch (BadImageFormatException)
			{
				Console.WriteLine(fileToProcess + " does not appear to have a valid Win32 Managed header.");
			}
			catch
			{
				Console.WriteLine("An unexpected error occured whilst attempting to apply Corflags to executable " + fileToProcess);
			}

			if (File.Exists(configFile))
			{
				//Also copy the application config file, so that LoadFromRemoteSources etc. are preserved
				//No clue why a File.Copy command fails on CI
				try
				{
					byte[] bytes = File.ReadAllBytes(configFile);
					File.WriteAllBytes(configFile.Replace(".exe", "-32.exe"), bytes);
				}
				catch
				{
					Console.WriteLine("An unexpected error occured whilst attempting to create the 32-bit .exe.config file.");
				}
				
			}
		}
	}
}
