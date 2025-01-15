using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CSScriptLibrary;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Trains;

namespace OpenBveApi.FunctionScripting {

	/// <summary>Animation script implemented in external C# code.</summary>
	public class CSAnimationScript : AnimationScript {

		/// <summary>The last result returned</summary>
		public double LastResult { get; set; }
		/// <summary>The minimum pinned result or NaN to set no minimum</summary>
		public double Maximum { get; set; } = double.NaN;
		/// <summary>The maximum pinned result or NaN to set no maximum</summary>
		public double Minimum { get; set; } = double.NaN;

		// ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
		private readonly HostInterface currentHost;
		private readonly object scriptObject;
		private readonly MethodInfo executeMethod;
		private readonly int invokeType;

		/// <summary>Load a script with or without arguments.</summary>
		/// <param name="host">A reference to the base application host interface.</param>
		/// <param name="path">The path to the CS file. Suffix similar to URL query string will be parsed as arguments.</param>
		public CSAnimationScript(HostInterface host, string path) : this(host, GetFileNameFromPath(path), GetArgsFromPath(path)) { }

		private static string GetFileNameFromPath(string path) {
			if (path.Contains('?')) {
				return path.Split('?').First();
			} else {
				return path;
			}
		}

		private static Dictionary<string, string> GetArgsFromPath(string path) {
			if (path.Contains('?')) {
				var queryStr = path.Split('?').Last();
				var result = new Dictionary<string, string>();
				foreach (string t in queryStr.Split('&')) {
					if (t.Contains('=')) {
						result.Add(t.Split('=').First(), t.Split('=').Last());
					} else {
						result.Add(t, null);
					}
				}
				return result;
			} else {
				return null;
			}
		}

		/// <summary>Load a script with or without arguments.</summary>
		/// <param name="host">A reference to the base application host interface.</param>
		/// <param name="path">The path to the CS file.</param>
		/// <param name="args">The arguments to pass to the script, or null.</param>
		public CSAnimationScript(HostInterface host, string path, Dictionary<string, string> args) {
			currentHost = host;
			try {
				CSScript.GlobalSettings.TargetFramework = "v4.0";
				Assembly assembly = CSScript.LoadCode(File.ReadAllText(path));
				Type scriptType = assembly.GetTypes().FirstOrDefault(t => t.Name == "OpenBVEScript");
				if (scriptType == null)
					throw new EntryPointNotFoundException("Script file does not contain the type 'OpenBVEScript'");

				foreach (var ctor in scriptType.GetConstructors()) {
					if (ctor.GetParameters().Length == 0) {
						scriptObject = Activator.CreateInstance(scriptType);
						break;
					} else if (ctor.GetParameters().Length == 1 
						&& ctor.GetParameters()[0].ParameterType == typeof(Dictionary<string, string>)) {
						scriptObject = Activator.CreateInstance(scriptType, args);
						break;
					}
				}
				if (scriptObject == null)
					throw new EntryPointNotFoundException("Public constructor must take no parameter or a 'Dictionary<string, string>'");

				foreach (var method in scriptType.GetMethods().Where(m => m.Name == "ExecuteScript" && m.ReturnType == typeof(double))) {
					Type[][] invokeTypes = {
						// The original interface
						new[] {typeof(AbstractTrain), typeof(Vector3), typeof(double), typeof(int), typeof(bool), typeof(double) },
						// The full interface
						new[] {typeof(AbstractTrain), typeof(int), typeof(Vector3), typeof(double), typeof(int), typeof(bool), typeof(double), typeof(int) }
					};
					for (int i = 0; i < invokeTypes.Length; i++) {
						if (method.GetParameters().Select(p => p.ParameterType).SequenceEqual(invokeTypes[i])) {
							executeMethod = method;
							invokeType = i;
							break;
						}
					}
					if (executeMethod != null) break;
				}

				if (executeMethod == null)
					throw new EntryPointNotFoundException("Not containing method with correct signature and name 'ExecuteScript'");

			} catch (Exception ex) {
				currentHost.AddMessage(MessageType.Error, false,
					"An error occcured whilst parsing script " + path + ": " + ex.Message);
				throw;
			}
		}

		/// <summary>Clone this object.</summary>
		/// <returns>A shallow copy.</returns>
		public AnimationScript Clone() {
			return (AnimationScript) MemberwiseClone();
		}

		/// <summary>Performs the function script, and returns the current result</summary>
		public double ExecuteScript(AbstractTrain Train, int CarIndex, Vector3 Position, double TrackPosition, int SectionIndex, bool IsPartOfTrain, double TimeElapsed, int CurrentState) {
			switch (invokeType) {
				case 0:
					LastResult = (double)executeMethod.Invoke(scriptObject, new object[] { Train, Position, TrackPosition,
					SectionIndex, IsPartOfTrain, TimeElapsed });
					break;
				case 1:
					LastResult = (double)executeMethod.Invoke(scriptObject, new object[] { Train, CarIndex, Position, TrackPosition,
					SectionIndex, IsPartOfTrain, TimeElapsed, CurrentState });
					break;
			}
			if (!double.IsNaN(this.Minimum) & this.LastResult < Minimum) {
				return Minimum;
			}
            if (!double.IsNaN(this.Maximum) & this.LastResult > Maximum)
            {
				return Maximum;
			}
			return LastResult;
		}
	}
}
