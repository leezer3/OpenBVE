using System;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Textures;
using OpenBveApi.Trains;

namespace OpenBve {
	/// <summary>Represents the host application.</summary>
	internal class Host : OpenBveApi.Hosts.HostInterface {
		
		// --- functions ---
		
		/// <summary>Reports a problem to the host application.</summary>
		/// <param name="type">The type of problem that is reported.</param>
		/// <param name="text">The textual message that describes the problem.</param>
		public override void ReportProblem(OpenBveApi.Hosts.ProblemType type, string text) {
			Interface.AddMessage(MessageType.Error, false, text);
		}
		
		
		// --- texture ---
		
		/// <summary>Queries the dimensions of a texture.</summary>
		/// <param name="path">The path to the file or folder that contains the texture.</param>
		/// <param name="width">Receives the width of the texture.</param>
		/// <param name="height">Receives the height of the texture.</param>
		/// <returns>Whether querying the dimensions was successful.</returns>
		public override bool QueryTextureDimensions(string path, out int width, out int height) {
			if (System.IO.File.Exists(path) || System.IO.Directory.Exists(path)) {
				for (int i = 0; i < Plugins.LoadedPlugins.Length; i++) {
					if (Plugins.LoadedPlugins[i].Texture != null) {
						try {
							if (Plugins.LoadedPlugins[i].Texture.CanLoadTexture(path)) {
								try {
									if (Plugins.LoadedPlugins[i].Texture.QueryTextureDimensions(path, out width, out height)) {
										return true;
									}
									Interface.AddMessage(MessageType.Error, false,
									                     "Plugin " + Plugins.LoadedPlugins[i].Title + " returned unsuccessfully at QueryTextureDimensions"
									                    );
								} catch (Exception ex) {
									Interface.AddMessage(MessageType.Error, false,
									                     "Plugin " + Plugins.LoadedPlugins[i].Title + " raised the following exception at QueryTextureDimensions:" + ex.Message
									                    );
								}
							}
						} catch (Exception ex) {
							Interface.AddMessage(MessageType.Error, false,
							                     "Plugin " + Plugins.LoadedPlugins[i].Title + " raised the following exception at CanLoadTexture:" + ex.Message
							                    );
						}
					}
				}
				Interface.AddMessage(MessageType.Error, false,
				                     "No plugin found that is capable of loading texture " + path
				                    );
			} else {
				ReportProblem(OpenBveApi.Hosts.ProblemType.PathNotFound, path);
			}
			width = 0;
			height = 0;
			return false;
		}
		
		/// <summary>Loads a texture and returns the texture data.</summary>
		/// <param name="path">The path to the file or folder that contains the texture.</param>
		/// <param name="parameters">The parameters that specify how to process the texture.</param>
		/// <param name="texture">Receives the texture.</param>
		/// <returns>Whether loading the texture was successful.</returns>
		public override bool LoadTexture(string path, TextureParameters parameters, out Texture texture) {
			if (System.IO.File.Exists(path) || System.IO.Directory.Exists(path)) {
				for (int i = 0; i < Plugins.LoadedPlugins.Length; i++) {
					if (Plugins.LoadedPlugins[i].Texture != null) {
						try {
							if (Plugins.LoadedPlugins[i].Texture.CanLoadTexture(path)) {
								try {
									if (Plugins.LoadedPlugins[i].Texture.LoadTexture(path, out texture)) {
										texture.CompatibleTransparencyMode = false;
										texture = texture.ApplyParameters(parameters);
										return true;
									}
									Interface.AddMessage(MessageType.Error, false, "Plugin " + Plugins.LoadedPlugins[i].Title + " returned unsuccessfully at LoadTexture");
								} catch (Exception ex) {
									Interface.AddMessage(MessageType.Error, false, "Plugin " + Plugins.LoadedPlugins[i].Title + " raised the following exception at LoadTexture:" + ex.Message);
								}
							}
						} catch (Exception ex) {
							Interface.AddMessage(MessageType.Error, false, "Plugin " + Plugins.LoadedPlugins[i].Title + " raised the following exception at CanLoadTexture:" + ex.Message);
						}
					}
				}
				Interface.AddMessage(MessageType.Error, false, "No plugin found that is capable of loading texture " + path);
			} else {
				ReportProblem(OpenBveApi.Hosts.ProblemType.PathNotFound, path);
			}
			texture = null;
			return false;
		}
		
		/// <summary>Registers a texture and returns a handle to the texture.</summary>
		/// <param name="path">The path to the file or folder that contains the texture.</param>
		/// <param name="parameters">The parameters that specify how to process the texture.</param>
		/// <param name="handle">Receives the handle to the texture.</param>
		/// <returns>Whether loading the texture was successful.</returns>
		public override bool RegisterTexture(string path, TextureParameters parameters, out Texture handle) {
			if (System.IO.File.Exists(path) || System.IO.Directory.Exists(path)) {
				Texture data;
				if (Textures.RegisterTexture(path, parameters, out data)) {
					handle = data;
					return true;
				}
			} else {
				ReportProblem(OpenBveApi.Hosts.ProblemType.PathNotFound, path);
			}
			handle = null;
			return false;
		}
		
		/// <summary>Registers a texture and returns a handle to the texture.</summary>
		/// <param name="texture">The texture data.</param>
		/// <param name="parameters">The parameters that specify how to process the texture.</param>
		/// <param name="handle">Receives the handle to the texture.</param>
		/// <returns>Whether loading the texture was successful.</returns>
		public override bool RegisterTexture(Texture texture, TextureParameters parameters, out Texture handle) {
			texture = texture.ApplyParameters(parameters);
			handle = Textures.RegisterTexture(texture);
			return true;
		}
		
		
		// --- sound ---
		
		/// <summary>Loads a sound and returns the sound data.</summary>
		/// <param name="path">The path to the file or folder that contains the sound.</param>
		/// <param name="sound">Receives the sound.</param>
		/// <returns>Whether loading the sound was successful.</returns>
		public override bool LoadSound(string path, out OpenBveApi.Sounds.Sound sound) {
			if (System.IO.File.Exists(path) || System.IO.Directory.Exists(path)) {
				for (int i = 0; i < Plugins.LoadedPlugins.Length; i++) {
					if (Plugins.LoadedPlugins[i].Sound != null) {
						try {
							if (Plugins.LoadedPlugins[i].Sound.CanLoadSound(path)) {
								try {
									if (Plugins.LoadedPlugins[i].Sound.LoadSound(path, out sound)) {
										return true;
									}
									Interface.AddMessage(MessageType.Error, false, "Plugin " + Plugins.LoadedPlugins[i].Title + " returned unsuccessfully at LoadSound");
								} catch (Exception ex) {
									Interface.AddMessage(MessageType.Error, false, "Plugin " + Plugins.LoadedPlugins[i].Title + " raised the following exception at LoadSound:" + ex.Message);
								}
							}
						} catch (Exception ex) {
							Interface.AddMessage(MessageType.Error, false, "Plugin " + Plugins.LoadedPlugins[i].Title + " raised the following exception at CanLoadSound:" + ex.Message);
						}
					}
				}
				Interface.AddMessage(MessageType.Error, false, "No plugin found that is capable of loading sound " + path);
			} else {
				ReportProblem(OpenBveApi.Hosts.ProblemType.PathNotFound, path);
			}
			sound = null;
			return false;
		}
		
		/// <summary>Registers a sound and returns a handle to the sound.</summary>
		/// <param name="path">The path to the file or folder that contains the sound.</param>
		/// <param name="handle">Receives a handle to the sound.</param>
		/// <returns>Whether loading the sound was successful.</returns>
		public override bool RegisterSound(string path, out OpenBveApi.Sounds.SoundHandle handle) {
			if (System.IO.File.Exists(path) || System.IO.Directory.Exists(path)) {
				// Sounds.SoundBuffer data;
				// data = Sounds.RegisterBuffer(path, 0.0); // TODO
			} else {
				ReportProblem(OpenBveApi.Hosts.ProblemType.PathNotFound, path);
			}
			handle = null;
			return false;
		}
		
		/// <summary>Registers a sound and returns a handle to the sound.</summary>
		/// <param name="sound">The sound data.</param>
		/// <param name="handle">Receives a handle to the sound.</param>
		/// <returns>Whether loading the sound was successful.</returns>
		public override bool RegisterSound(OpenBveApi.Sounds.Sound sound, out OpenBveApi.Sounds.SoundHandle handle) {
			//handle = Sounds.RegisterBuffer(sound, 0.0); // TODO
			handle = null;
			return true;
		}

		public override void ExecuteFunctionScript(OpenBveApi.FunctionScripting.FunctionScript functionScript, AbstractTrain train, int CarIndex, Vector3 Position, double TrackPosition, int SectionIndex, bool IsPartOfTrain, double TimeElapsed, int CurrentState)
		{
			FunctionScripts.ExecuteFunctionScript(functionScript, (TrainManager.Train)train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, CurrentState);
		}
		
	}
}
