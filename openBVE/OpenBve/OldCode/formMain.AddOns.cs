using System;
using System.Windows.Forms;

namespace OpenBve {
	internal partial class formMain {
		
		/// <summary>Gets the flag code for the specified country.</summary>
		/// <param name="country">The country in en-US.</param>
		/// <param name="fallback">The fallback in case the flag is not defined.</param>
		/// <returns>The flag.</returns>
		private static string GetFlagFromEnUsCountry(string country, string fallback) {
			if (country != null) {
				switch (country.ToLowerInvariant()) {
						case "austria": return "AT";
						case "belgium": return "BE";
						case "brazil": return "BR";
						case "switzerland": return "CH";
						case "china": return "CN";
						case "czech republic": return "CZ";
						case "germany": return "DE";
						case "spain": return "ES";
						case "france": return "FR";
						case "united kingdom": return "GB";
						case "hong kong": return "HK";
						case "hungary": return "HU";
						case "italy": return "IT";
						case "japan": return "JP";
						case "south korea": return "KR";
						case "netherlands": return "NL";
						case "poland": return "PL";
						case "portugal": return "PT";
						case "romania": return "RO";
						case "russia": return "RU";
						case "singapore": return "SG";
						case "taiwan": return "TW";
						case "united states": return "US";
						default: return fallback;
				}
			} else {
				return fallback;
			}
		}
		
		private class RemoveIfPossibleAttribute { }
		
		private void Group(TreeNodeCollection collection) {
			/* Group folders that have same text */
			for (int i = 1; i < collection.Count; i++) {
				if (collection[i].Tag == null | collection[i].Tag is RemoveIfPossibleAttribute) {
					for (int j = 0; j < i; j++) {
						if (collection[j].Tag == null | collection[j].Tag is RemoveIfPossibleAttribute) {
							if (collection[i].Text == collection[j].Text) {
								TreeNodeCollection elements = collection[i].Nodes;
								collection.RemoveAt(i);
								foreach (TreeNode node in elements) {
									collection[j].Nodes.Add(node);
								}
								i--;
								break;
							}
						}
					}
				}
			}
			/* Recursion */
			foreach (TreeNode node in collection) {
				Group(node.Nodes);
			}
		}
		
		private void Flatten(TreeNodeCollection collection, bool shortify) {
			/* Recursion */
			foreach (TreeNode node in collection) {
				Flatten(node.Nodes, shortify);
			}
			/* Remove empty folders from the collection */
			for (int i = 0; i < collection.Count; i++) {
				if (collection[i].Tag == null && collection[i].Nodes.Count == 0) {
					collection.RemoveAt(i);
					i--;
				}
			}
			if (!shortify) {
				/* If only element is Route/Train/Library/SharedLibrary, then remove it */
				if (collection.Count == 1 && collection[0].Tag is RemoveIfPossibleAttribute) {
					TreeNodeCollection elements = collection[0].Nodes;
					collection.RemoveAt(0);
					foreach (TreeNode element in elements) {
						collection.Add(element);
					}
				}
				/* Expand folders that only contain one element */
				if (collection.Count == 1) {
					if (collection[0].Nodes.Count != 0) {
						collection[0].Expand();
					}
				}
			} else {
				/* Flatten out folders that contain only one element */
				if (collection.Count == 1 && collection[0].Tag == null) {
					TreeNodeCollection elements = collection[0].Nodes;
					collection.RemoveAt(0);
					foreach (TreeNode element in elements) {
						collection.Add(element);
					}
				}
			}
		}
		
		private int Count(TreeNodeCollection collection) {
			int count = collection.Count;
			foreach (TreeNode node in collection) {
				count += Count(node.Nodes);
			}
			return count;
		}
		
		private void Expand(TreeNodeCollection collection, int total) {
			int count = Count(collection);
			if (count <= total) {
				foreach (TreeNode node in collection) {
					node.ExpandAll();
				}
			}
		}
		
	}
}
