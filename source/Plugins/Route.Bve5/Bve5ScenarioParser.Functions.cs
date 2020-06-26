using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Bve5RouteParser
{
	internal partial class Parser
	{
		/// <summary>Parses a BVE5 format time into OpenBVE's internal time representation</summary>
		/// <param name="Expression">The time to parse</param>
		/// <param name="Value">The number of seconds since midnight on the first day this represents, updated via 'out'</param>
		/// <returns>True if the parse succeeds, false if it does not</returns>
		internal bool TryParseBve5Time(string Expression, out double Value)
		{
			Expression = TrimInside(Expression);
			if (Expression.Length != 0)
			{
				CultureInfo Culture = CultureInfo.InvariantCulture;
				string[] Split = Expression.Split(':');
				int h, m, s;
				switch (Split.Length)
				{
					case 1:
						//Single number - plain hours

						if (int.TryParse(Expression, NumberStyles.Integer, Culture, out h))
						{
							Value = 3600.0 * (double)h;
							return true;
						}
						break;
					case 2:
						//HH:MM
						if (int.TryParse(Split[0], NumberStyles.Integer, Culture, out h) && int.TryParse(Split[1], NumberStyles.Integer, Culture, out m))
						{
							Value = 3600.0 * (double)h + 60.0 * (double)m;
							return true;
						}
						break;
					case 3:
						//HH:MM:SS
						if (int.TryParse(Split[0], NumberStyles.Integer, Culture, out h) && int.TryParse(Split[1], NumberStyles.Integer, Culture, out m) && int.TryParse(Split[2], NumberStyles.Integer, Culture, out s))
						{
							Value = 3600.0 * (double)h + 60.0 * (double)m + (double)s;
							return true;
						}
						break;
				}
			}
			Value = 0.0;
			return false;
		}

		private static string TrimInside(string Expression) {
			System.Text.StringBuilder Builder = new System.Text.StringBuilder(Expression.Length);
			foreach (char c in Expression.Where(c => !char.IsWhiteSpace(c)))
			{
				Builder.Append(c);
			} return Builder.ToString();
		}

		/// <summary>Finds the structure index for the given key</summary>
		static int FindStructureIndex(string key, RouteData Data)
		{
			int sttype = -1;
			for (int k = 0; k < Data.ObjectList.Length; k++)
			{
				if (Data.ObjectList[k].Name.ToLowerInvariant() == key.ToLowerInvariant())
				{
					sttype = k;
					break;
				}
			}
			return sttype;
		}

		/// <summary>Finds the structure path for the given key</summary>
		static string FindStructurePath(string key, RouteData Data)
		{
			for (int k = 0; k < Data.ObjectList.Length; k++)
			{
				if (Data.ObjectList[k].Name.ToLowerInvariant() == key.ToLowerInvariant())
				{
					return Data.ObjectList[k].Path;
				}
			}
			return String.Empty;
		}

		/// <summary>Finds the rail index for the given key</summary>
		static int FindRailIndex(string key, Rail[] Rails)
		{
			if (key.Length == 0)
			{
				return 0;
			}
			for (int i = 0; i < Rails.Length; i++)
			{
				if (Rails[i].Key == key)
				{
					return i;
				}
			}
			return -1;
		}

		/// <summary>Creates any missing blocks between the last used block and the current block</summary>
		/// <param name="Data">The route data, updated via 'ref'</param>
		/// <param name="BlocksUsed">The last used block</param>
		/// <param name="ToIndex">The block index to create up-to</param>
		/// <param name="PreviewOnly">Whether this is a preview only (A preview only relates to rail 0)</param>
		private static void CreateMissingBlocks(ref RouteData Data, ref int BlocksUsed, int ToIndex, bool PreviewOnly)
		{
			if (ToIndex >= BlocksUsed)
			{
				while (Data.Blocks.Length <= ToIndex)
				{
					Array.Resize(ref Data.Blocks, Data.Blocks.Length << 1);
				}
				for (int i = BlocksUsed; i <= Data.Blocks.Length - 1; i++)
				{
					Data.Blocks[i] = new Block();
					if (!PreviewOnly)
					{
						Data.Blocks[i].Background = -1;
						Data.Blocks[i].Brightness = new Brightness[] { };
						Data.Blocks[i].Fog = Data.Blocks[i - 1].Fog;
						Data.Blocks[i].FogDefined = false;
						Data.Blocks[i].Cycle = Data.Blocks[i - 1].Cycle;
						Data.Blocks[i].Height = double.NaN;
						Data.Blocks[i].RunSounds = new TrackSound[1];
						Data.Blocks[i].RunSounds[0] = Data.Blocks[i - 1].RunSounds[Data.Blocks[i - 1].RunSounds.Length - 1];
					}
					Data.Blocks[i].RailType = new int[Data.Blocks[i - 1].RailType.Length];
					for (int j = 0; j < Data.Blocks[i].RailType.Length; j++)
					{
						Data.Blocks[i].RailType[j] = Data.Blocks[i - 1].RailType[j];
					}
					Data.Blocks[i].Rail = new Rail[Data.Blocks[i - 1].Rail.Length];
					for (int j = 0; j < Data.Blocks[i].Rail.Length; j++)
					{
						Data.Blocks[i].Rail[j].RailStart = Data.Blocks[i - 1].Rail[j].RailStart;
						Data.Blocks[i].Rail[j].RailStartX = Data.Blocks[i - 1].Rail[j].RailStartX;
						Data.Blocks[i].Rail[j].RailStartY = Data.Blocks[i - 1].Rail[j].RailStartY;
						Data.Blocks[i].Rail[j].RailStartRefreshed = false;
						Data.Blocks[i].Rail[j].RailEnd = false;
						Data.Blocks[i].Rail[j].RailEndX = Data.Blocks[i - 1].Rail[j].RailStartX;
						Data.Blocks[i].Rail[j].RailEndY = Data.Blocks[i - 1].Rail[j].RailStartY;
						Data.Blocks[i].Rail[j].Key = Data.Blocks[i - 1].Rail[j].Key;
						Data.Blocks[i].Rail[j].HorizontalRadius = Data.Blocks[i - 1].Rail[j].HorizontalRadius;
						Data.Blocks[i].Rail[j].VerticalRadius = Data.Blocks[i - 1].Rail[j].VerticalRadius;
					}
					if (!PreviewOnly)
					{
						Data.Blocks[i].RailFreeObj = new Object[][] { };
						Data.Blocks[i].GroundFreeObj = new Object[] { };
						Data.Blocks[i].Crack = new Crack[] { };
						Data.Blocks[i].Signal = new Signal[] { };
						Data.Blocks[i].Section = new Section[] { };
						CreateMissingRepeaters(ref Data, i);

					}
					Data.Blocks[i].Pitch = Data.Blocks[i - 1].Pitch;
					Data.Blocks[i].Limit = new Limit[] { };
					Data.Blocks[i].Stop = new Stop[] { };
					Data.Blocks[i].Station = -1;
					Data.Blocks[i].StationPassAlarm = false;
					Data.Blocks[i].CurrentTrackState = Data.Blocks[i - 1].CurrentTrackState;
					Data.Blocks[i].Turn = 0.0;
					Data.Blocks[i].Accuracy = Data.Blocks[i - 1].Accuracy;
					Data.Blocks[i].AdhesionMultiplier = Data.Blocks[i - 1].AdhesionMultiplier;
				}
				BlocksUsed = ToIndex + 1;
			}
		}


		private static void CreateMissingRepeaters(ref RouteData Data, int CurrentBlock)
		{
			if (Data.Blocks[CurrentBlock - 1] == null || Data.Blocks[CurrentBlock - 1].Repeaters == null || Data.Blocks[CurrentBlock] == null)
			{
				return;
			}
			Data.Blocks[CurrentBlock].Repeaters = new Repeater[Data.Blocks[CurrentBlock - 1].Repeaters.Length];
			for (int j = 0; j < Data.Blocks[CurrentBlock - 1].Repeaters.Length; j++)
			{
				Data.Blocks[CurrentBlock].Repeaters[j] = new Repeater();
				Data.Blocks[CurrentBlock].Repeaters[j].Name = Data.Blocks[CurrentBlock -1].Repeaters[j].Name;
				if (Data.Blocks[CurrentBlock - 1].Repeaters[j].StructureTypes == null)
				{
					//TODO: Hack to stop crashing, presumably the includes haven't been sorted properly
					continue;
				}
				Data.Blocks[CurrentBlock].Repeaters[j].StructureTypes = new int[Data.Blocks[CurrentBlock - 1].Repeaters[j].StructureTypes.Length];
				Data.Blocks[CurrentBlock].Repeaters[j].TrackPosition = Data.Blocks[CurrentBlock -1].Repeaters[j].TrackPosition + Data.BlockInterval;
				Data.Blocks[CurrentBlock].Repeaters[j].RailIndex = Data.Blocks[CurrentBlock - 1].Repeaters[j].RailIndex;
				Data.Blocks[CurrentBlock].Repeaters[j].Type = Data.Blocks[CurrentBlock - 1].Repeaters[j].Type;
				Data.Blocks[CurrentBlock].Repeaters[j].RepetitionInterval = Data.Blocks[CurrentBlock - 1].Repeaters[j].RepetitionInterval;
				Data.Blocks[CurrentBlock].Repeaters[j].Span = Data.Blocks[CurrentBlock - 1].Repeaters[j].Span;
				Data.Blocks[CurrentBlock].Repeaters[j].Position = Data.Blocks[CurrentBlock - 1].Repeaters[j].Position;
				for (int k = 0; k < Data.Blocks[CurrentBlock - 1].Repeaters[j].StructureTypes.Length; k++)
				{
					Data.Blocks[CurrentBlock].Repeaters[j].StructureTypes[k] = Data.Blocks[CurrentBlock - 1].Repeaters[j].StructureTypes[k];
				}
			}
		}

		private static double GetBrightness(ref RouteData Data, double TrackPosition)
		{
			double tmin = double.PositiveInfinity;
			double tmax = double.NegativeInfinity;
			double bmin = 1.0, bmax = 1.0;
			for (int i = 0; i < Data.Blocks.Length; i++)
			{
				for (int j = 0; j < Data.Blocks[i].Brightness.Length; j++)
				{
					if (Data.Blocks[i].Brightness[j].TrackPosition <= TrackPosition)
					{
						tmin = Data.Blocks[i].Brightness[j].TrackPosition;
						bmin = (double)Data.Blocks[i].Brightness[j].Value;
					}
				}
			}
			for (int i = Data.Blocks.Length - 1; i >= 0; i--)
			{
				for (int j = Data.Blocks[i].Brightness.Length - 1; j >= 0; j--)
				{
					if (Data.Blocks[i].Brightness[j].TrackPosition >= TrackPosition)
					{
						tmax = Data.Blocks[i].Brightness[j].TrackPosition;
						bmax = (double)Data.Blocks[i].Brightness[j].Value;
					}
				}
			}
			if (tmin == double.PositiveInfinity & tmax == double.NegativeInfinity)
			{
				return 1.0;
			}
			else if (tmin == double.PositiveInfinity)
			{
				return (bmax - 1.0) * TrackPosition / tmax + 1.0;
			}
			else if (tmax == double.NegativeInfinity)
			{
				return bmin;
			}
			else if (tmin == tmax)
			{
				return 0.5 * (bmin + bmax);
			}
			else
			{
				double n = (TrackPosition - tmin) / (tmax - tmin);
				return (1.0 - n) * bmin + n * bmax;
			}
		}

		/// <summary>Attempts to determine the System.Text.Encoding value for a given BVE5 file</summary>
		/// <param name="FileName">The filename</param>
		/// <returns>The detected encoding, or UTF-8 if this is not found</returns>
		internal static Encoding DetermineFileEncoding(string FileName)
		{
			using (StreamReader reader = new StreamReader(FileName))
			{
				var firstLine = reader.ReadLine();
				if (firstLine == null)
				{
					return Encoding.UTF8;
				}
				string[] Header = firstLine.Split(':');
				if (Header.Length == 1)
				{
					return Encoding.UTF8;
				}
				string[] Arguments = Header[1].Split(',');
				try { return Encoding.GetEncoding(Arguments[0].ToLowerInvariant().Trim()); }
				catch { return Encoding.UTF8; }
			}
		}

		private static void Normalize(ref double x, ref double y)
		{
			double t = x * x + y * y;
			if (t != 0.0)
			{
				t = 1.0 / Math.Sqrt(t);
				x *= t;
				y *= t;
			}
		}
	}
}
