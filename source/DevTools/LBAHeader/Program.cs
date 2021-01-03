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
			if (!System.IO.File.Exists(f))
			{
				//If we can't find our initial argument, try and locate the openBVE executable in our directory
				string ff = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
				if (ff == null)
				{
					Console.WriteLine("Unable to find the executing assembly path....");
					return;
				}
				f = System.IO.Path.Combine(ff, "OpenBve.exe");
			}
			if (!System.IO.File.Exists(f))
			{
				//Not found, log this rather than crashing
				Console.WriteLine("No suitable executables found....");
				return;
			}
			AddLbaFlag(f);
			Add32BitFlag(f);
			// ReSharper disable once AssignNullToNotNullAttribute
			f = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(f), "RouteViewer.exe");
			if (!System.IO.File.Exists(f))
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
			string finalFileName = fileToProcess.Replace(".exe", "-32.exe");
			Console.WriteLine("Creating 32-bit preferred copy of executable " + fileToProcess);
			var output = Console.Out;
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

		}
	}
}
