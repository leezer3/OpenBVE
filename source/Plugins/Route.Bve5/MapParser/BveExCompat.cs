// BveEX (https://bveex.okaoka-depot.com/) extends the BVE5/6 map grammar with
// custom statements that the vanilla BVE5 parser does not understand:
//
// - BveEx.MapPlugin.Load('...') / LoadAssembly('...') (and the old AtsEx.* names)
// - include '<BveEx::...>' / '<AtsEx::...>' pseudo-headers
// - ex_using / ex_include / ex_if / ex_elif / ex_else / ex_while control syntax
// - underscore statement aliases (ex_using _alias = ...) and custom plugin
//   statements such as BveEx.User.... (element names unknown to BVE/OpenBVE)
//
// Bve5_Parsing also crashes on procedure calls used as assignment values,
// e.g. $CrossNoise = JointNoise.Play(50); (RuntimeBinderException), which
// real BVE routes use. These are rewritten to execute the call as a plain
// statement while defining the variable as 0.
//
// OpenBVE cannot execute BveEX .NET plugins (they patch the BVE process at
// runtime), so the goal here is graceful degradation: strip the BveEX-only
// syntax before handing the map to Bve5_Parsing so the static route still
// loads instead of failing with a wall of syntax errors. Dynamic behaviour
// provided by the plugins is skipped with a single informational message.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Route.Bve5
{
	internal static class BveExCompat
	{
		// Matches include '<BveEx::...>' / "<AtsEx::...>" pseudo-headers, including
		// the legacy '<BveEx::READDEPTH>1' form where text follows the brackets.
		private static readonly Regex IncludeSpecialPattern = new Regex(
			@"(?i)\binclude\s*['""]<[^'""]+>[^'""]*['""]\s*;?",
			RegexOptions.Compiled);

		// BveEx.MapPlugin.Load('...'); / AtsEx.MapPlugin.LoadAssembly('...'); etc.
		private static readonly Regex MapPluginLoadPattern = new Regex(
			@"(?i)\b(?:bveex|atsex)\.mapplugin\.(?:load|loadassembly)\s*\([^;]*?\)\s*;?",
			RegexOptions.Compiled);

		// ex_using _alias = Some.Chain; (alias expansion is unnecessary: alias
		// usages are blanked together with every other unknown statement below)
		private static readonly Regex ExUsingPattern = new Regex(
			@"(?i)\bex_using\b[^;]*?;",
			RegexOptions.Compiled);

		// ex_include 'file', args...; (converted to a plain include, see below)
		private static readonly Regex ExIncludePattern = new Regex(
			@"(?i)\bex_include\b([^;]*?;)",
			RegexOptions.Compiled);

		private static readonly Regex QuotedPathPattern = new Regex(
			@"'([^']*)'|""([^""]*)""",
			RegexOptions.Compiled);

		// Any remaining BveEX-namespaced statement, e.g. BveEx.User.Foo.Bar(...);
		private static readonly Regex NamespacedStatementPattern = new Regex(
			@"(?i)\b(?:bveex|atsex)\.[^;]*?;",
			RegexOptions.Compiled);

		// Underscore alias usages, e.g. _myalias.Put(...);
		// Group 1 keeps a preceding statement separator so the distance
		// statement in front (e.g. "100;") stays terminated.
		private static readonly Regex UnderscoreStatementPattern = new Regex(
			@"(?m)(;|^)\s*_[A-Za-z0-9_]*\.[^;]*;",
			RegexOptions.Compiled);

		// ex_arg0 / ex_argdistance / ex_relativedir system variables that may
		// appear inside expressions of ex_include-d files.
		private static readonly Regex ExVariablePattern = new Regex(
			@"(?i)\bex_argdistance\b|\bex_relativedir\b|\bex_arg\d+\b",
			RegexOptions.Compiled);

		// Standard include directives (recursed into when mirroring files).
		private static readonly Regex IncludePattern = new Regex(
			@"(?i)\binclude\s*(?:'([^']*)'|""([^""]*)"")",
			RegexOptions.Compiled);

		// ex_include targets (resolved against the root map directory, as BveEX does).
		private static readonly Regex ExIncludeCollectPattern = new Regex(
			@"(?i)\bex_include\s*['""]([^'""]+)['""]",
			RegexOptions.Compiled);

		// A line containing only a brace, left over from ex_if/ex_while blocks.
		// The vanilla map grammar has no brace syntax, so these can only be
		// BveEX block leftovers (applied only to files with ex_ markers).
		private static readonly Regex LoneBraceLinePattern = new Regex(
			@"(?m)^[ \t]*[{}][ \t]*$",
			RegexOptions.Compiled);

		private static readonly Regex ExBlockMarkerPattern = new Regex(
			@"(?i)\bex_if\b|\bex_elif\b|\bex_else\b|\bex_while\b",
			RegexOptions.Compiled);

		// Assignments of a dotted procedure call, e.g. $var = JointNoise.Play(50);
		// Bve5_Parsing cannot evaluate these (the call returns no value) and
		// throws. Value-returning functions (floor, rand, ...) are never
		// dotted, so a dotted call on the right-hand side is always rewritten.
		private static readonly Regex AssignmentCallPattern = new Regex(
			@"(?im)(^|;)(\s*)(\$[A-Za-z0-9_]+)(\s*=\s*)((?:Curve|Gauge|Gradient|Track|Structure|Repeater|Background|Station|Section|Signal|SpeedLimit|Beacon|Pretrain|Light|Fog|DrawDistance|CabIlluminance|Irregularity|Adhesion|Sound|Sound3d|RollingNoise|FlangeNoise|JointNoise|Train|Legacy)\.[A-Za-z]+\s*\()",
			RegexOptions.Compiled);

		private const int MaxFiles = 256;

		/// <summary>
		/// Creates sanitised copies of a map file and all of its recursive
		/// includes under a temp directory, with BveEX-only syntax blanked out.
		/// </summary>
		/// <param name="rootFile">The original root map file.</param>
		/// <param name="sanitizedRootFile">The sanitised root file to parse instead (null when not needed).</param>
		/// <param name="tempRoot">The temp directory holding the copies (null when not needed).</param>
		/// <param name="hadBveEx">Whether any BveEX syntax was found.</param>
		/// <returns>False when sanitising failed; the caller must parse the original file.</returns>
		internal static bool TryCreateSanitizedCopy(string rootFile, out string sanitizedRootFile, out string tempRoot, out bool hadBveEx)
		{
			sanitizedRootFile = null;
			tempRoot = null;
			hadBveEx = false;
			try
			{
				if (string.IsNullOrEmpty(rootFile) || !File.Exists(rootFile))
				{
					return false;
				}
				string rootDir = Path.GetDirectoryName(Path.GetFullPath(rootFile));
				// Pass 1: collect the include closure and look for BveEX markers.
				List<string> files = CollectIncludeClosure(rootFile, rootDir);
				bool found = false;
				Dictionary<string, string> originalTexts = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
				Dictionary<string, Encoding> encodings = new Dictionary<string, Encoding>(StringComparer.OrdinalIgnoreCase);
				Dictionary<string, bool> hadBom = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
				foreach (string file in files)
				{
					if (!TryReadMapFile(file, out string text, out Encoding encoding, out bool bom))
					{
						continue;
					}
					originalTexts[file] = text;
					encodings[file] = encoding;
					hadBom[file] = bom;
					if (!found && NeedsSanitizing(text))
					{
						found = true;
					}
				}
				if (!found)
				{
					return false;
				}
				hadBveEx = true;
				// Pass 2: write sanitised mirrors preserving the directory layout.
				string temp = Path.Combine(Path.GetTempPath(), "OpenBVE", "BveEx_" + Guid.NewGuid().ToString("N"));
				Directory.CreateDirectory(temp);
				tempRoot = temp;
				foreach (KeyValuePair<string, string> pair in originalTexts)
				{
					string tempFile = MapToTemp(pair.Key, rootDir, temp);
					string currentDir = Path.GetDirectoryName(Path.GetFullPath(pair.Key));
					string sanitized = Sanitize(pair.Value, rootDir, currentDir);
					sanitized = RewriteIncludes(sanitized, currentDir, rootDir, temp);
					Directory.CreateDirectory(Path.GetDirectoryName(tempFile));
					WriteMapFile(tempFile, sanitized, encodings[pair.Key], hadBom[pair.Key]);
				}
				sanitizedRootFile = MapToTemp(Path.GetFullPath(rootFile), rootDir, temp);
				return true;
			}
			catch
			{
				// Never break route loading because of the compatibility shim.
				try
				{
					if (tempRoot != null && Directory.Exists(tempRoot))
					{
						Directory.Delete(tempRoot, true);
					}
				}
				catch
				{
					// ignored
				}
				sanitizedRootFile = null;
				tempRoot = null;
				hadBveEx = false;
				return false;
			}
		}

		internal static void Cleanup(string tempRoot)
		{
			try
			{
				if (!string.IsNullOrEmpty(tempRoot) && Directory.Exists(tempRoot))
				{
					Directory.Delete(tempRoot, true);
				}
			}
			catch
			{
				// ignored
			}
		}

		/// <summary>Maps a temp-mirror path back to the original route directory for display.</summary>
		internal static string UnmapTempPath(string tempPath, string tempRoot, string originalDir)
		{
			try
			{
				if (string.IsNullOrEmpty(tempPath) || string.IsNullOrEmpty(tempRoot) || string.IsNullOrEmpty(originalDir))
				{
					return tempPath;
				}
				string full = Path.GetFullPath(tempPath);
				string root = Path.GetFullPath(tempRoot);
				string prefix = root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
				if (full.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
				{
					string relative = full.Substring(prefix.Length);
					if (!relative.StartsWith("_external", StringComparison.OrdinalIgnoreCase))
					{
						return Path.GetFullPath(Path.Combine(originalDir, relative));
					}
				}
			}
			catch
			{
				// ignored, fall through
			}
			return tempPath;
		}

		/// <summary>Whether an error message stems from leftover BveEX syntax.</summary>
		internal static bool IsBveExError(string message)
		{
			if (string.IsNullOrEmpty(message))
			{
				return false;
			}
			string m = message.ToLowerInvariant();
			if (m.Contains("bveex") || m.Contains("atsex") || m.Contains("ex_using") ||
				m.Contains("ex_include") || m.Contains("ex_if") || m.Contains("ex_elif") ||
				m.Contains("ex_else") || m.Contains("ex_while") || m.Contains("ex_arg") ||
				m.Contains("ex_relativedir"))
			{
				return true;
			}
			// Leftover block braces or underscore aliases quoted as offending tokens.
			if ((m.Contains("extraneous input") || m.Contains("mismatched input")) && (m.Contains("{") || m.Contains("}")))
			{
				return true;
			}
			if (m.Contains("'") && (m.Contains("'_") || m.Contains("\"_")))
			{
				return true;
			}
			return false;
		}

		/// <summary>Removes BveEX-only syntax from map text, preserving line breaks.</summary>
		private static string Sanitize(string text, string rootDir, string currentDir)
		{
			// Checked up-front: the keywords below are blanked out first.
			bool hasBlocks = ExBlockMarkerPattern.IsMatch(text);
			string result = BlankRegex(text, IncludeSpecialPattern);
			result = BlankRegex(result, MapPluginLoadPattern);
			result = BlankRegex(result, ExUsingPattern);
			result = ConvertExIncludes(result, rootDir, currentDir);
			result = RewriteStatementCallAssignments(result);
			result = BlankExConditions(result);
			result = BlankRegex(result, NamespacedStatementPattern);
			result = UnderscoreStatementPattern.Replace(result, match => match.Groups[1].Value + Blank(match.Value.Substring(match.Groups[1].Value.Length)));
			result = ExVariablePattern.Replace(result, "0");
			if (hasBlocks)
			{
				result = BlankRegex(result, LoneBraceLinePattern);
			}
			return result;
		}

		/// <summary>
		/// Converts ex_include 'file', args...; into a plain include.
		/// BveEX resolves the path against the root map directory (not the
		/// current file), so it is re-based to keep resolving identically.
		/// Arguments are dropped and ex_arg* variables become 0 (see above).
		/// </summary>
		private static string ConvertExIncludes(string text, string rootDir, string currentDir)
		{
			return ExIncludePattern.Replace(text, match =>
			{
				string args = match.Groups[1].Value;
				Match pathMatch = QuotedPathPattern.Match(args);
				if (!pathMatch.Success)
				{
					return Blank(match.Value);
				}
				string path = pathMatch.Groups[1].Success ? pathMatch.Groups[1].Value : pathMatch.Groups[2].Value;
				if (string.IsNullOrWhiteSpace(path) || path.StartsWith("<"))
				{
					return Blank(match.Value);
				}
				string replacement;
				try
				{
					string abs = Path.GetFullPath(Path.Combine(rootDir, path));
					string rebased = GetRelativePath(currentDir, abs);
					replacement = "include '" + rebased + "';";
				}
				catch
				{
					return Blank(match.Value);
				}
				// Keep the original length so columns after this point don't shift.
				if (replacement.Length < match.Value.Length)
				{
					replacement = replacement + new string(' ', match.Value.Length - replacement.Length);
				}
				return replacement;
			});
		}

		/// <summary>Blanks ex_if/ex_elif/ex_while conditions and ex_else keywords.</summary>
		/// <remarks>
		/// Conditions are deliberately not evaluated: OpenBVE has no access to
		/// the BveEX expression state, so both branches are kept and parsed as
		/// static route data. Plugin-only statements inside them are blanked
		/// separately above.
		/// </remarks>
		private static string BlankExConditions(string text)
		{
			StringBuilder sb = new StringBuilder(text);
			BlankKeywordConditions(sb, "ex_if");
			BlankKeywordConditions(sb, "ex_elif");
			BlankKeywordConditions(sb, "ex_while");
			BlankKeyword(sb, "ex_else");
			return sb.ToString();
		}

		private static void BlankKeywordConditions(StringBuilder sb, string keyword)
		{
			string s = sb.ToString();
			int index = 0;
			while (TryFindKeyword(s, keyword, index, out int start))
			{
				int pos = start + keyword.Length;
				while (pos < s.Length && char.IsWhiteSpace(s[pos]))
				{
					pos++;
				}
				if (pos >= s.Length || s[pos] != '(')
				{
					BlankRange(sb, start, keyword.Length);
					s = sb.ToString();
					index = start + keyword.Length;
					continue;
				}
				int depth = 0;
				bool inSingle = false;
				bool inDouble = false;
				int end = -1;
				for (int i = pos; i < s.Length; i++)
				{
					char c = s[i];
					if (inSingle)
					{
						if (c == '\'') inSingle = false;
						continue;
					}
					if (inDouble)
					{
						if (c == '"') inDouble = false;
						continue;
					}
					if (c == '\'') inSingle = true;
					else if (c == '"') inDouble = true;
					else if (c == '(') depth++;
					else if (c == ')')
					{
						depth--;
						if (depth == 0)
						{
							end = i;
							break;
						}
					}
					else if (c == ';' && depth == 0)
					{
						break;
					}
				}
				if (end == -1)
				{
					BlankRange(sb, start, keyword.Length);
					s = sb.ToString();
					index = start + keyword.Length;
					continue;
				}
				int blankEnd = end + 1;
				while (blankEnd < s.Length && char.IsWhiteSpace(s[blankEnd]) && s[blankEnd] != '\r' && s[blankEnd] != '\n')
				{
					blankEnd++;
				}
				if (blankEnd < s.Length && s[blankEnd] == '{')
				{
					blankEnd++;
				}
				BlankRange(sb, start, blankEnd - start);
				s = sb.ToString();
				index = blankEnd;
			}
		}

		private static void BlankKeyword(StringBuilder sb, string keyword)
		{
			string s = sb.ToString();
			int index = 0;
			while (TryFindKeyword(s, keyword, index, out int start))
			{
				int end = start + keyword.Length;
				while (end < s.Length && char.IsWhiteSpace(s[end]) && s[end] != '\r' && s[end] != '\n')
				{
					end++;
				}
				if (end < s.Length && s[end] == '{')
				{
					end++;
				}
				BlankRange(sb, start, end - start);
				s = sb.ToString();
				index = end;
			}
		}

		private static bool TryFindKeyword(string text, string keyword, int startIndex, out int found)
		{
			found = -1;
			int index = text.IndexOf(keyword, startIndex, StringComparison.OrdinalIgnoreCase);
			while (index != -1)
			{
				bool headOk = index == 0 || (!char.IsLetterOrDigit(text[index - 1]) && text[index - 1] != '_');
				int after = index + keyword.Length;
				bool tailOk = after >= text.Length || (!char.IsLetterOrDigit(text[after]) && text[after] != '_');
				if (headOk && tailOk)
				{
					found = index;
					return true;
				}
				index = text.IndexOf(keyword, index + 1, StringComparison.OrdinalIgnoreCase);
			}
			return false;
		}

		/// <summary>
		/// Rewrites includes so they resolve to the mirrored temp copies.
		/// Bve5_Parsing resolves every include relative to the root map file
		/// (verified empirically; BVE itself accepts the same convention), so
		/// rewritten paths are expressed relative to the temp mirror root.
		/// Original paths are resolved like BVE does: relative to the file
		/// containing the statement first, then relative to the root map
		/// directory (routes in the wild rely on the latter for nested files).
		/// </summary>
		private static string RewriteIncludes(string text, string currentDir, string rootDir, string tempRoot)
		{
			return IncludePattern.Replace(text, match =>
			{
				bool single = match.Groups[1].Success;
				string path = single ? match.Groups[1].Value : match.Groups[2].Value;
				if (string.IsNullOrWhiteSpace(path) || path.StartsWith("<"))
				{
					return match.Value;
				}
				string abs = ResolveInclude(path, currentDir, rootDir);
				if (abs == null)
				{
					return match.Value;
				}
				string tempTarget = MapToTemp(abs, rootDir, tempRoot);
				string rebased;
				try
				{
					rebased = GetRelativePath(tempRoot, tempTarget);
				}
				catch
				{
					return match.Value;
				}
				string quote = single ? "'" : "\"";
				string replacement = "include " + quote + rebased + quote;
				if (replacement.Length < match.Value.Length)
				{
					replacement = replacement + new string(' ', match.Value.Length - replacement.Length);
				}
				return replacement;
			});
		}

		/// <summary>Resolves an include path: including-file directory first, then the root map directory.</summary>
		private static string ResolveInclude(string path, string currentDir, string rootDir)
		{
			try
			{
				string candidate = Path.GetFullPath(Path.Combine(currentDir, path));
				if (File.Exists(candidate))
				{
					return candidate;
				}
			}
			catch
			{
				// ignored
			}
			try
			{
				string candidate = Path.GetFullPath(Path.Combine(rootDir, path));
				if (File.Exists(candidate))
				{
					return candidate;
				}
			}
			catch
			{
				// ignored
			}
			return null;
		}

		private static List<string> CollectIncludeClosure(string rootFile, string rootDir)
		{
			List<string> files = new List<string>();
			HashSet<string> visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			Queue<string> pending = new Queue<string>();
			pending.Enqueue(Path.GetFullPath(rootFile));
			while (pending.Count != 0 && files.Count < MaxFiles)
			{
				string file = pending.Dequeue();
				string full;
				try
				{
					full = Path.GetFullPath(file);
				}
				catch
				{
					continue;
				}
				if (!visited.Add(full) || !File.Exists(full))
				{
					continue;
				}
				files.Add(full);
				string text;
				try
				{
					if (!TryReadMapFile(full, out text, out _, out _))
					{
						continue;
					}
				}
				catch
				{
					continue;
				}
				string dir = Path.GetDirectoryName(full);
				foreach (Match match in IncludePattern.Matches(text))
				{
					string path = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;
					if (string.IsNullOrWhiteSpace(path) || path.StartsWith("<"))
					{
						continue;
					}
					string resolved = ResolveInclude(path, dir, rootDir);
					if (resolved != null)
					{
						pending.Enqueue(resolved);
					}
				}
				// ex_include targets live outside the standard include closure.
				foreach (Match match in ExIncludeCollectPattern.Matches(text))
				{
					string path = match.Groups[1].Value;
					if (string.IsNullOrWhiteSpace(path) || path.StartsWith("<"))
					{
						continue;
					}
					try
					{
						string resolved = Path.GetFullPath(Path.Combine(rootDir, path));
						if (File.Exists(resolved))
						{
							pending.Enqueue(resolved);
						}
					}
					catch
					{
						// ignored
					}
				}
			}
			return files;
		}

		private static bool NeedsSanitizing(string text)
		{
			string code = StripComments(text);
			return ContainsBveExMarkers(code) || AssignmentCallPattern.IsMatch(code);
		}

		private static bool ContainsBveExMarkers(string code)
		{
			if (code.IndexOf("bveex", StringComparison.OrdinalIgnoreCase) != -1) return true;
			if (code.IndexOf("atsex", StringComparison.OrdinalIgnoreCase) != -1) return true;
			if (code.IndexOf("ex_using", StringComparison.OrdinalIgnoreCase) != -1) return true;
			if (code.IndexOf("ex_include", StringComparison.OrdinalIgnoreCase) != -1) return true;
			if (code.IndexOf("ex_if", StringComparison.OrdinalIgnoreCase) != -1) return true;
			if (code.IndexOf("ex_elif", StringComparison.OrdinalIgnoreCase) != -1) return true;
			if (code.IndexOf("ex_else", StringComparison.OrdinalIgnoreCase) != -1) return true;
			if (code.IndexOf("ex_while", StringComparison.OrdinalIgnoreCase) != -1) return true;
			if (code.IndexOf("ex_arg", StringComparison.OrdinalIgnoreCase) != -1) return true;
			return false;
		}

		/// <summary>Removes # and // comments (respecting quoted strings).</summary>
		private static string StripComments(string text)
		{
			// Ignore comments: file names or notes may otherwise mention BveEX
			// without using any of its syntax (e.g. the '// BveEX' header line
			// from the official plugin template).
			StringBuilder sb = new StringBuilder(text.Length);
			bool inSingle = false;
			bool inDouble = false;
			for (int i = 0; i < text.Length; i++)
			{
				char c = text[i];
				if (inSingle)
				{
					sb.Append(c);
					if (c == '\'') inSingle = false;
				}
				else if (inDouble)
				{
					sb.Append(c);
					if (c == '"') inDouble = false;
				}
				else if (c == '\'' || c == '"')
				{
					sb.Append(c);
					if (c == '\'') inSingle = true;
					else inDouble = true;
				}
				else if (c == '#')
				{
					while (i < text.Length && text[i] != '\n')
					{
						i++;
					}
					sb.Append('\n');
				}
				else if (c == '/' && i + 1 < text.Length && text[i + 1] == '/')
				{
					while (i < text.Length && text[i] != '\n')
					{
						i++;
					}
					sb.Append('\n');
				}
				else
				{
					sb.Append(c);
				}
			}
			string code = sb.ToString();
			return code;
		}

		/// <summary>
		/// Rewrites procedure-call assignments ($var = Element.Func(args);)
		/// into a plain statement plus a zero definition
		/// (Element.Func(args); $var = 0;). Line breaks are preserved.
		/// </summary>
		private static string RewriteStatementCallAssignments(string text)
		{
			List<CallAssignment> found = new List<CallAssignment>();
			foreach (Match match in AssignmentCallPattern.Matches(text))
			{
				int callOpen = match.Groups[5].Index + match.Groups[5].Length - 1; // the '('
				int callEnd = FindClosingParen(text, callOpen);
				if (callEnd == -1)
				{
					continue;
				}
				int pos = callEnd + 1;
				while (pos < text.Length && char.IsWhiteSpace(text[pos]))
				{
					pos++;
				}
				if (pos >= text.Length || text[pos] != ';')
				{
					continue;
				}
				found.Add(new CallAssignment
				{
					VarStart = match.Groups[3].Index,
					CallStart = match.Groups[5].Index,
					VarText = match.Groups[3].Value,
					Semicolon = pos
				});
			}
			// Apply back to front so earlier positions stay valid.
			StringBuilder sb = new StringBuilder(text);
			for (int i = found.Count - 1; i >= 0; i--)
			{
				CallAssignment item = found[i];
				BlankRange(sb, item.VarStart, item.CallStart - item.VarStart);
				sb.Insert(item.Semicolon + 1, " " + item.VarText + " = 0;");
			}
			return sb.ToString();
		}

		private struct CallAssignment
		{
			internal int VarStart;
			internal int CallStart;
			internal string VarText;
			internal int Semicolon;
		}

		/// <summary>Finds the parenthesis closing the one at openIndex (respecting quotes), or -1.</summary>
		private static int FindClosingParen(string text, int openIndex)
		{
			int depth = 0;
			bool inSingle = false;
			bool inDouble = false;
			for (int i = openIndex; i < text.Length; i++)
			{
				char c = text[i];
				if (inSingle)
				{
					if (c == '\'') inSingle = false;
				}
				else if (inDouble)
				{
					if (c == '"') inDouble = false;
				}
				else if (c == '\'')
				{
					inSingle = true;
				}
				else if (c == '"')
				{
					inDouble = true;
				}
				else if (c == '(')
				{
					depth++;
				}
				else if (c == ')')
				{
					depth--;
					if (depth == 0)
					{
						return i;
					}
				}
				else if (c == ';' && depth == 0)
				{
					return -1;
				}
			}
			return -1;
		}

		private static string BlankRegex(string text, Regex regex)
		{
			return regex.Replace(text, match => Blank(match.Value));
		}

		private static string Blank(string value)
		{
			char[] chars = value.ToCharArray();
			for (int i = 0; i < chars.Length; i++)
			{
				if (chars[i] != '\r' && chars[i] != '\n')
				{
					chars[i] = ' ';
				}
			}
			return new string(chars);
		}

		private static void BlankRange(StringBuilder sb, int start, int length)
		{
			int end = Math.Min(sb.Length, start + length);
			for (int i = start; i < end; i++)
			{
				if (sb[i] != '\r' && sb[i] != '\n')
				{
					sb[i] = ' ';
				}
			}
		}

		private static string MapToTemp(string absPath, string rootDir, string tempRoot)
		{
			string full = Path.GetFullPath(absPath);
			string rootFull = Path.GetFullPath(rootDir);
			string prefix = rootFull.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
			if (full.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
			{
				return Path.Combine(tempRoot, full.Substring(prefix.Length));
			}
			if (string.Equals(full, rootFull, StringComparison.OrdinalIgnoreCase))
			{
				return Path.Combine(tempRoot, Path.GetFileName(full));
			}
			string safe = full.Replace(':', '_');
			while (safe.StartsWith(Path.DirectorySeparatorChar.ToString()) || safe.StartsWith(Path.AltDirectorySeparatorChar.ToString()))
			{
				safe = safe.Substring(1);
			}
			return Path.Combine(tempRoot, "_external", safe);
		}

		private static string GetRelativePath(string fromDir, string toPath)
		{
			Uri from = new Uri(AppendSlash(Path.GetFullPath(fromDir)));
			Uri to = new Uri(Path.GetFullPath(toPath));
			string relative = Uri.UnescapeDataString(from.MakeRelativeUri(to).ToString());
			return relative.Replace('/', Path.DirectorySeparatorChar);
		}

		private static string AppendSlash(string path)
		{
			if (!path.EndsWith(Path.DirectorySeparatorChar.ToString()) && !path.EndsWith(Path.AltDirectorySeparatorChar.ToString()))
			{
				return path + Path.DirectorySeparatorChar;
			}
			return path;
		}

		private static bool TryReadMapFile(string path, out string text, out Encoding encoding, out bool hadBom)
		{
			text = null;
			encoding = null;
			hadBom = false;
			byte[] bytes = File.ReadAllBytes(path);
			int offset = 0;
			if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
			{
				encoding = new UTF8Encoding(true);
				offset = 3;
				hadBom = true;
			}
			else if (bytes.Length >= 2 && bytes[0] == 0xFF && bytes[1] == 0xFE)
			{
				encoding = Encoding.Unicode;
				offset = 2;
				hadBom = true;
			}
			else if (bytes.Length >= 2 && bytes[0] == 0xFE && bytes[1] == 0xFF)
			{
				encoding = Encoding.BigEndianUnicode;
				offset = 2;
				hadBom = true;
			}
			if (hadBom)
			{
				return TryStrictDecode(bytes, offset, encoding, out text);
			}
			// No BOM. Route files in the wild are often saved as UTF-8 without
			// a BOM and without a header encoding label (or with a wrong one),
			// so probe strict decoders in an order that cannot silently
			// mojibake: UTF-8 first (strict validation is reliable), then the
			// content-detected encoding, then the header label, then Shift-JIS.
			if (TryStrictDecode(bytes, 0, GetUtf8NoBom(), out text))
			{
				encoding = GetUtf8NoBom();
				return true;
			}
			Encoding detected = null;
			try
			{
				detected = OpenBveApi.TextEncoding.GetSystemEncodingFromFile(path, null);
			}
			catch
			{
				// ignored, fall through to the remaining candidates
			}
			if (detected != null && TryStrictDecode(bytes, 0, detected, out text))
			{
				encoding = detected;
				return true;
			}
			Encoding declared = GetDeclaredMapEncoding(bytes);
			if (declared != null && TryStrictDecode(bytes, 0, declared, out text))
			{
				encoding = declared;
				return true;
			}
			Encoding shiftJis = null;
			try
			{
				shiftJis = Encoding.GetEncoding("shift_jis");
			}
			catch
			{
				// ignored
			}
			if (shiftJis != null && TryStrictDecode(bytes, 0, shiftJis, out text))
			{
				encoding = shiftJis;
				return true;
			}
			return false;
		}

		private static Encoding GetUtf8NoBom()
		{
			return new UTF8Encoding(false);
		}

		private static bool TryStrictDecode(byte[] bytes, int offset, Encoding encoding, out string text)
		{
			text = null;
			try
			{
				Encoding strict = Encoding.GetEncoding(encoding.WebName, EncoderExceptionFallback.ExceptionFallback, DecoderExceptionFallback.ExceptionFallback);
				text = strict.GetString(bytes, offset, bytes.Length - offset);
				return true;
			}
			catch
			{
				return false;
			}
		}

		private static void WriteMapFile(string path, string text, Encoding encoding, bool hadBom)
		{
			Encoding output = encoding;
			if (hadBom && encoding is UTF8Encoding)
			{
				output = new UTF8Encoding(true);
			}
			File.WriteAllText(path, text, output);
		}

		private static Encoding GetDeclaredMapEncoding(byte[] bytes)
		{
			try
			{
				int length = 0;
				while (length < bytes.Length && bytes[length] != '\r' && bytes[length] != '\n')
				{
					length++;
				}
				string firstLine = Encoding.ASCII.GetString(bytes, 0, length);
				string[] header = firstLine.Split(':');
				if (header.Length == 1)
				{
					return null;
				}
				return Encoding.GetEncoding(header[1].Split(',')[0].ToLowerInvariant().Trim());
			}
			catch
			{
				return null;
			}
		}
	}
}
