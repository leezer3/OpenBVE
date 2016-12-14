using System;
using System.Globalization;

namespace OpenBve {
	internal static partial class ManagedContent {
		
		/*
		 * This module provides functions for comparing version numbers.
		 * A version number is a string consisting of a series of decimal
		 * digit groups, followed by an optional textual suffix.
		 * 
		 * Examples:
		 * 1.1
		 * 1.2.3
		 * 1.2.3alpha
		 * 1.2.3beta
		 * 1.2.4
		 * 1.3
		 * */
		
		/// <summary>Compares two versions and returns the relationship between them.</summary>
		/// <param name="a">The first version.</param>
		/// <param name="b">The second version.</param>
		/// <returns>-1 if the first version is less than the second version, 0 if both versions are equal, and 1 if the first version is greater than the second version.</returns>
		internal static int CompareVersions(string a, string b) {
			if (a == b) return 0;
			int[] digitsA, digitsB;
			string suffixA, suffixB;
			SplitVersion(a, out digitsA, out suffixA);
			SplitVersion(b, out digitsB, out suffixB);
			int length = Math.Max(digitsA.Length, digitsB.Length);
			for (int i = 0; i < length; i++) {
				int digitA = i < digitsA.Length ? digitsA[i] : 0;
				int digitB = i < digitsB.Length ? digitsB[i] : 0;
				if (digitA < digitB) {
					return -1;
				} else if (digitA > digitB) {
					return 1;
				}
			}
			return Math.Sign(string.Compare(suffixA, suffixB, StringComparison.OrdinalIgnoreCase));
		}
		
		/// <summary>Splits a version into its numeric digits and textual suffix.</summary>
		/// <param name="version">The version to split.</param>
		/// <param name="digits">Receives the numeric digits of the version.</param>
		/// <param name="suffix">Receives the textual suffix of the version.</param>
		private static void SplitVersion(string version, out int[] digits, out string suffix) {
			digits = new int[version.Length];
			int digitCount = 0;
			int start = 0;
			for (int i = 0; i <= version.Length; i++) {
				if (i == version.Length || version[i] < 48 || version[i] > 57) {
					if (i == start) {
						break;
					} else {
						string digit = version.Substring(start, i - start);
						digits[digitCount] = int.Parse(digit, NumberStyles.Integer, CultureInfo.InvariantCulture);
						digitCount++;
					}
					if (i < version.Length && version[i] == '.') {
						start = i + 1;
					} else {
						start = i;
						break;
					}
				}
			}
			if (start < version.Length) {
				suffix = version.Substring(start);
			} else {
				suffix = string.Empty;
			}
			Array.Resize<int>(ref digits, digitCount);
		}
		
	}
}