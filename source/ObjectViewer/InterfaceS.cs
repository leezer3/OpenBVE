// ╔══════════════════════════════════════════════════════════════╗
// ║ Interface.cs and TrainManager.cs for the Structure Viewer    ║
// ╠══════════════════════════════════════════════════════════════╣
// ║ This file cannot be used in the openBVE main program.        ║
// ║ The files from the openBVE main program cannot be used here. ║
// ╚══════════════════════════════════════════════════════════════╝

using System;

namespace OpenBve {

	// --- TimeTable.cs ---
	internal static class Timetable {
		internal static void AddObjectForCustomTimetable(ObjectManager.AnimatedObject obj) { }
		internal enum TimetableState {
			None = 0,
			Custom = 1,
			Default = 2
		}
		internal static TimetableState CurrentTimetable = TimetableState.None;
		internal static bool CustomTimetableAvailable = false;
	}

	// --- PluginManager.cs ---
	internal static class PluginManager {
		internal static class CurrentPlugin {
			internal static int[] Panel = new int[] { };
		}
	}

#pragma warning disable 0649

	// --- Game.cs ---
	internal static class Game {
		internal static double SecondsSinceMidnight = 0.0;
		internal enum SectionType { ValueBased, IndexBased }
		internal struct SectionAspect {
			internal int Number;
			internal double Speed;
		}
		internal struct Section {
			internal int PreviousSection;
			internal int NextSection;
			internal TrainManager.Train[] Trains;
			internal bool TrainReachedStopPoint;
			internal int StationIndex;
			internal bool Invisible;
			internal int[] SignalIndices;
			internal double TrackPosition;
			internal SectionType Type;
			internal SectionAspect[] Aspects;
			internal int CurrentAspect;
			internal int FreeSections;
		}
		internal static Section[] Sections = new Section[] { };
		internal static int InfoTotalTriangles = 0;
		internal static int InfoTotalTriangleStrip = 0;
		internal static int InfoTotalQuads = 0;
		internal static int InfoTotalQuadStrip = 0;
		internal static int InfoTotalPolygon = 0;
		internal static void Reset() {
			Renderer.Reset();
			ObjectManager.Objects = new ObjectManager.StaticObject[16];
			ObjectManager.ObjectsUsed = 0;
			ObjectManager.ObjectsSortedByStart = new int[] { };
			ObjectManager.ObjectsSortedByEnd = new int[] { };
			ObjectManager.ObjectsSortedByStartPointer = 0;
			ObjectManager.ObjectsSortedByEndPointer = 0;
			ObjectManager.LastUpdatedTrackPosition = 0.0;
			ObjectManager.AnimatedWorldObjects = new ObjectManager.AnimatedWorldObject[4];
			ObjectManager.AnimatedWorldObjectsUsed = 0;
		}
	}
	
	// --- TrackManager.cs ---
	internal static class TrackManager {
		internal struct TrackFollower {
			internal double TrackPosition;
			internal World.Vector3D WorldPosition;
			internal World.Vector3D WorldDirection;
			internal World.Vector3D WorldUp;
			internal World.Vector3D WorldSide;
            internal double CurveRadius;
            internal double CurveCant;
            internal double CantDueToInaccuracy;
		}
		internal static void UpdateTrackFollower(ref TrackFollower Follower, double NewTrackPosition, bool UpdateWorldCoordinates, bool AddTrackInaccurary) { }
	}

	// --- Interface.cs ---
	internal static class Interface {

		// messages
		internal enum MessageType {
			Information = 1,
			Warning = 2,
			Error = 3,
			Critical = 4
		}
		internal struct Message {
			internal MessageType Type;
			internal string Text;
		}
		internal static Message[] Messages = new Message[] { };
		internal static int MessageCount = 0;
		internal static void AddMessage(MessageType Type, bool FileNotFound, string Text) {
			if (MessageCount == 0) {
				Messages = new Message[16];
			} else if (MessageCount >= Messages.Length) {
				Array.Resize<Message>(ref Messages, Messages.Length << 1);
			}
			Messages[MessageCount].Type = Type;
			Messages[MessageCount].Text = Text;
			MessageCount++;
		}
		internal static void ClearMessages() {
			Messages = new Message[] { };
			MessageCount = 0;
		}

		// ================================

		internal struct Options {
			internal TextureManager.InterpolationMode Interpolation;
            internal Renderer.TransparencyMode TransparencyMode;
			internal int AnisotropicFilteringLevel;
			internal int AnisotropicFilteringMaximum;
		    internal int AntialiasingLevel;
			internal int ObjectOptimizationBasicThreshold;
			internal int ObjectOptimizationFullThreshold;
		}
		internal static Options CurrentOptions;

		// ================================

#pragma warning restore 0649

		// try parse vb6
		internal static bool TryParseDoubleVb6(string Expression, out double Value) {
			Expression = TrimInside(Expression);
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			for (int n = Expression.Length; n > 0; n--) {
				double a;
				if (double.TryParse(Expression.Substring(0, n), System.Globalization.NumberStyles.Float, Culture, out a)) {
					Value = a;
					return true;
				}
			}
			Value = 0.0;
			return false;
		}
		internal static bool TryParseFloatVb6(string Expression, out float Value) {
			Expression = TrimInside(Expression);
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			for (int n = Expression.Length; n > 0; n--) {
				float a;
				if (float.TryParse(Expression.Substring(0, n), System.Globalization.NumberStyles.Float, Culture, out a)) {
					Value = a;
					return true;
				}
			}
			Value = 0.0f;
			return false;
		}
		internal static bool TryParseIntVb6(string Expression, out int Value) {
			Expression = TrimInside(Expression);
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			for (int n = Expression.Length; n > 0; n--) {
				double a;
				if (double.TryParse(Expression.Substring(0, n), System.Globalization.NumberStyles.Float, Culture, out a)) {
					if (a >= -2147483648.0 & a <= 2147483647.0) {
						Value = (int)Math.Round(a);
						return true;
					} else break;
				}
			}
			Value = 0;
			return false;
		}
		internal static bool TryParseByteVb6(string Expression, out byte Value) {
			Expression = TrimInside(Expression);
			System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
			for (int n = Expression.Length; n > 0; n--) {
				double a;
				if (double.TryParse(Expression.Substring(0, n), System.Globalization.NumberStyles.Float, Culture, out a)) {
					if (a >= 0.0 & a <= 255.0) {
						Value = (byte)Math.Round(a);
						return true;
					} else break;
				}
			}
			Value = 0;
			return false;
		}

		// try parse time
		internal static bool TryParseTime(string Expression, out double Value) {
			Expression = TrimInside(Expression);
			if (Expression.Length != 0) {
				System.Globalization.CultureInfo Culture = System.Globalization.CultureInfo.InvariantCulture;
				int i = Expression.IndexOf('.');
				if (i >= 1) {
					int h; if (int.TryParse(Expression.Substring(0, i), System.Globalization.NumberStyles.Integer, Culture, out h)) {
						int n = Expression.Length - i - 1;
						if (n == 1 | n == 2) {
							uint m; if (uint.TryParse(Expression.Substring(i + 1, n), System.Globalization.NumberStyles.None, Culture, out m)) {
								Value = 3600.0 * (double)h + 60.0 * (double)m;
								return true;
							}
						} else if (n == 3 | n == 4) {
							uint m; if (uint.TryParse(Expression.Substring(i + 1, 2), System.Globalization.NumberStyles.None, Culture, out m)) {
								uint s; if (uint.TryParse(Expression.Substring(i + 3, n - 2), System.Globalization.NumberStyles.None, Culture, out s)) {
									Value = 3600.0 * (double)h + 60.0 * (double)m + (double)s;
									return true;
								}
							}
						}
					}
				} else if (i == -1) {
					int h; if (int.TryParse(Expression, System.Globalization.NumberStyles.Integer, Culture, out h)) {
						Value = 3600.0 * (double)h;
						return true;
					}
				}
			}
			Value = 0.0;
			return false;
		}

		// try parse hex color
		internal static bool TryParseHexColor(string Expression, out World.ColorRGB Color) {
			if (Expression.StartsWith("#")) {
				string a = Expression.Substring(1).TrimStart();
				int x; if (int.TryParse(a, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out x)) {
					int r = (x >> 16) & 0xFF;
					int g = (x >> 8) & 0xFF;
					int b = x & 0xFF;
					if (r >= 0 & r <= 255 & g >= 0 & g <= 255 & b >= 0 & b <= 255) {
						Color = new World.ColorRGB((byte)r, (byte)g, (byte)b);
						return true;
					} else {
						Color = new World.ColorRGB(0, 0, 255);
						return false;
					}
				} else {
					Color = new World.ColorRGB(0, 0, 255);
					return false;
				}
			} else {
				Color = new World.ColorRGB(0, 0, 255);
				return false;
			}
		}
		internal static bool TryParseHexColor(string Expression, out World.ColorRGBA Color) {
			if (Expression.StartsWith("#")) {
				string a = Expression.Substring(1).TrimStart();
				int x; if (int.TryParse(a, System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture, out x)) {
					int r = (x >> 16) & 0xFF;
					int g = (x >> 8) & 0xFF;
					int b = x & 0xFF;
					if (r >= 0 & r <= 255 & g >= 0 & g <= 255 & b >= 0 & b <= 255) {
						Color = new World.ColorRGBA((byte)r, (byte)g, (byte)b, 255);
						return true;
					} else {
						Color = new World.ColorRGBA(0, 0, 255, 255);
						return false;
					}
				} else {
					Color = new World.ColorRGBA(0, 0, 255, 255);
					return false;
				}
			} else {
				Color = new World.ColorRGBA(0, 0, 255, 255);
				return false;
			}
		}

		// try parse vb6 (with unit factors)
		internal static bool TryParseDoubleVb6(string Expression, double[] UnitFactors, out double Value) {
			double a; if (double.TryParse(Expression, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out a)) {
				Value = a;
				return true;
			} else {
				int j = 0, n = 0; Value = 0;
				for (int i = 0; i < Expression.Length; i++) {
					if (Expression[i] == ':') {
						string t = Expression.Substring(j, i - j);
						if (TryParseDoubleVb6(t, out a)) {
							if (n < UnitFactors.Length) {
								Value += a * UnitFactors[n];
							} else {
								return n > 0;
							}
						} else {
							return n > 0;
						} j = i + 1; n++;
					}
				}
				{
					string t = Expression.Substring(j);
					if (TryParseDoubleVb6(t, out a)) {
						if (n < UnitFactors.Length) {
							Value += a * UnitFactors[n];
							return true;
						} else {
							return n > 0;
						}
					} else {
						return n > 0;
					}
				}
			}
		}

		// trim inside
		private static string TrimInside(string Expression) {
			System.Text.StringBuilder Builder = new System.Text.StringBuilder(Expression.Length);
			for (int i = 0; i < Expression.Length; i++) {
				char c = Expression[i];
				if (!char.IsWhiteSpace(c)) {
					Builder.Append(c);
				}
			} return Builder.ToString();
		}

		// ================================

		// round to power of two
		internal static int RoundToPowerOfTwo(int Value) {
			Value -= 1;
			for (int i = 1; i < sizeof(int) * 8; i *= 2) {
				Value = Value | Value >> i;
			} return Value + 1;
		}

		// convert newlines to crlf
		internal static string ConvertNewlinesToCrLf(string Text) {
			System.Text.StringBuilder Builder = new System.Text.StringBuilder();
			for (int i = 0; i < Text.Length; i++) {
				int a = char.ConvertToUtf32(Text, i);
				if (a == 0xD & i < Text.Length - 1) {
					int b = char.ConvertToUtf32(Text, i + 1);
					if (b == 0xA) {
						Builder.Append("\r\n");
						i++;
					} else {
						Builder.Append("\r\n");
					}
				} else if (a == 0xA | a == 0xC | a == 0xD | a == 0x85 | a == 0x2028 | a == 0x2029) {
					Builder.Append("\r\n");
				} else if (a < 0x10000) {
					Builder.Append(Text[i]);
				} else {
					Builder.Append(Text.Substring(i, 2));
					i++;
				}
			} return Builder.ToString();
		}
	}
}