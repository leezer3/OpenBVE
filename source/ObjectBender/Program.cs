using System;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace ObjectBender {
	/// <summary>The class with the program's entry point.</summary>
	internal static class Program {

		/// <summary>The main procedure.</summary>
		[STAThread]
		private static void Main(string[] args) {
			if (args.Length == 0) {
				/*
				 * Show the GUI.
				 * */
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(new MainForm());
			} else {
				/*
				 * Use the command-line arguments.
				 * */
				CultureInfo culture = CultureInfo.InvariantCulture;
				/*
				 * Default values.
				 * */
				Bender.Options options = new Bender.Options();
				options.NumberOfSegments = 1;
				bool help = false;
				/*
				 * Parse the arguments.
				 * */
				for (int i = 0; i < args.Length; i++) {
					if (args[i] == "-?") {
						help = true;
					} else if (args[i].Equals("-a", StringComparison.OrdinalIgnoreCase)) {
						options.AppendToOutputFile = true;
					} else if (args.Length != 0 && args[i][0] == '-') {
						if (args[i].Length >= 3 && args[i][2] == '=') {
							string value = args[i].Substring(3);
							switch (args[i][1]) {
								case 'n':
									if (!int.TryParse(value, NumberStyles.Integer, culture, out options.NumberOfSegments)) {
										Console.WriteLine("Invalid integer in -n parameter.");
										return;
									}
									break;
								case 's':
									if (!double.TryParse(value, NumberStyles.Float, culture, out options.SegmentLength)) {
										Console.WriteLine("Invalid float in -s parameter.");
										return;
									}
									break;
								case 'b':
									if (!double.TryParse(value, NumberStyles.Float, culture, out options.BlockLength)) {
										Console.WriteLine("Invalid float in -b parameter.");
										return;
									}
									break;
								case 'r':
									if (!double.TryParse(value, NumberStyles.Float, culture, out options.Radius)) {
										Console.WriteLine("Invalid float in -r parameter.");
										return;
									}
									break;
								case 'g':
									if (!double.TryParse(value, NumberStyles.Float, culture, out options.RailGauge)) {
										Console.WriteLine("Invalid float in -g parameter.");
										return;
									} else {
										options.RailGauge *= 0.001;
									}
									break;
								case 'u':
									if (!double.TryParse(value, NumberStyles.Float, culture, out options.InitialCant)) {
										Console.WriteLine("Invalid float in -u parameter.");
										return;
									} else {
										options.InitialCant *= 0.001;
									}
									break;
								case 'v':
									if (!double.TryParse(value, NumberStyles.Float, culture, out options.FinalCant)) {
										Console.WriteLine("Invalid float in -v parameter.");
										return;
									} else {
										options.FinalCant *= 0.001;
									}
									break;
								default:
									help = true;
									break;
							}
						} else {
							help = true;
						}
					} else {
						if (options.InputFile == null) {
							options.InputFile = args[i];
						} else if (options.OutputFile == null) {
							options.OutputFile = args[i];
						} else {
							help = true;
						}
					}
				}
				/*
				 * Process.
				 * */
				ConsoleColor originalColor = Console.ForegroundColor;
				if (options.NumberOfSegments != 1 & options.SegmentLength == 0.0) {
					Console.WriteLine("If -n is used, -s must also be used.");
				} else if (options.BlockLength != 0.0 & options.Radius == 0.0) {
					Console.WriteLine("If -b is used, -r must also be used.");
				} else if (options.InitialCant != 0.0 & options.SegmentLength == 0.0) {
					Console.WriteLine("If -u is used, -s must also be used.");
				} else if (options.FinalCant != 0.0 & options.SegmentLength == 0.0) {
					Console.WriteLine("If -v is used, -s must also be used.");
				} else if (options.InitialCant != 0.0 & options.RailGauge == 0.0) {
					Console.WriteLine("If -u is used, -g must also be used.");
				} else if (options.FinalCant != 0.0 & options.RailGauge == 0.0) {
					Console.WriteLine("If -v is used, -g must also be used.");
				} else if (help) {
					Console.WriteLine();
					Console.WriteLine("ObjectBender <inputFile> <outputFile> <options>");
					Console.WriteLine();
					Console.WriteLine("<inputFile>: The input file in CSV format.");
					Console.WriteLine("<outputfile>: The output file in CSV format.");
					Console.WriteLine();
					Console.WriteLine("Options:");
					Console.WriteLine("-n=<numSegments>: The number of segments.");
					Console.WriteLine("-s=<segmentLength>: The length of each segment in meters.");
					Console.WriteLine("-b=<blockLength>: The block length in meters for use as a rail object.");
					Console.WriteLine("-r=<radius>: The radius in meters. Negative is left, positive is right.");
					Console.WriteLine("-g=<railGauge>: The rail gauge in meters.");
					Console.WriteLine("-u=<initialCant>: The cant at the beginning in millimeters.");
					Console.WriteLine("-v=<finalCant>: The cant at the end in millimeters.");
					Console.WriteLine("-a: Appends to the output file instead of overwriting it.");
					Console.WriteLine();
					Console.WriteLine("Example:");
					Console.WriteLine("ObjectBender in.csv out.csv -n=5 -s=5 -b=25 -r=150");
					Console.WriteLine();
				} else {
					if (options.AppendToOutputFile) {
						Console.Write(Path.GetFileName(options.InputFile) + " ++> " + Path.GetFileName(options.OutputFile) + "   ");
					} else {
						Console.Write(Path.GetFileName(options.InputFile) + " --> " + Path.GetFileName(options.OutputFile) + "   ");
					}
					try {
						Bender.BendObject(options);
						Console.ForegroundColor = ConsoleColor.Green;
						Console.WriteLine("[OK]");
					} catch (Exception ex) {
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine("[ERROR]");
						Console.ForegroundColor = ConsoleColor.Yellow;
						Console.WriteLine(ex.Message);
					}
				}
				Console.ForegroundColor = originalColor;
			}
		}
		
	}
}