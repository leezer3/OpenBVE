using System;

namespace OpenBve {
	internal static partial class ManagedContent {
		
		internal struct KeyValuePair {
			internal string Key;
			internal string Language;
			internal string Value;
			internal KeyValuePair(string key, string value) {
				int index = key.IndexOf('[');
				if (index >= 0 && key.Length != 0 && key[key.Length - 1] == ']') {
					this.Key = key.Substring(0, index).TrimEnd();
					this.Language = key.Substring(index + 1, key.Length - index - 2).Trim();
					this.Value = value;
				} else {
					this.Key = key;
					this.Language = null;
					this.Value = value;
				}
			}
			public override string ToString() {
				if (this.Language != null) {
					return this.Key + "[" + this.Language + "] = " + this.Value;
				} else {
					return this.Key + " = " + this.Value;
				}
			}
		}
		
		internal static string GetMetadata(KeyValuePair[] pairs, string key, string preferredLanguage, string defaultValue) {
			if (preferredLanguage == null) {
				for (int i = 0; i < pairs.Length; i++) {
					if (string.Equals(pairs[i].Key, key, StringComparison.OrdinalIgnoreCase)) {
						return pairs[i].Value;
					}
				}
			} else {
				for (int i = 0; i < pairs.Length; i++) {
					if (string.Equals(pairs[i].Key, key, StringComparison.OrdinalIgnoreCase)) {
						if (pairs[i].Language != null) {
							if (string.Equals(pairs[i].Language, preferredLanguage, StringComparison.OrdinalIgnoreCase)) {
								return pairs[i].Value;
							}
						}
					}
				}
				int index = preferredLanguage.IndexOf('-');
				string preferredFamily = index >= 0 ? preferredLanguage.Substring(0, index) : preferredLanguage;
				for (int i = 0; i < pairs.Length; i++) {
					if (string.Equals(pairs[i].Key, key, StringComparison.OrdinalIgnoreCase)) {
						if (pairs[i].Language != null) {
							index = pairs[i].Language.IndexOf('-');
							string family = index >= 0 ? pairs[i].Language.Substring(0, index) : pairs[i].Language;
							if (string.Equals(family, preferredFamily, StringComparison.OrdinalIgnoreCase)) {
								return pairs[i].Value;
							}
						}
					}
				}
				for (int i = 0; i < pairs.Length; i++) {
					if (string.Equals(pairs[i].Key, key, StringComparison.OrdinalIgnoreCase)) {
						if (pairs[i].Language != null) {
							if (string.Equals(pairs[i].Language, "en-US", StringComparison.OrdinalIgnoreCase)) {
								return pairs[i].Value;
							}
						}
					}
				}
				for (int i = 0; i < pairs.Length; i++) {
					if (string.Equals(pairs[i].Key, key, StringComparison.OrdinalIgnoreCase)) {
						if (pairs[i].Language != null) {
							index = pairs[i].Language.IndexOf('-');
							string family = index >= 0 ? pairs[i].Language.Substring(0, index) : pairs[i].Language;
							if (string.Equals(family, "en", StringComparison.OrdinalIgnoreCase)) {
								return pairs[i].Value;
							}
						}
					}
				}
				for (int i = 0; i < pairs.Length; i++) {
					if (string.Equals(pairs[i].Key, key, StringComparison.OrdinalIgnoreCase)) {
						return pairs[i].Value;
					}
				}
			}
			return defaultValue;
		}
		
	}
}