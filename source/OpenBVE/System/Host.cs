using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using OpenBveApi;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using OpenBveApi.Sounds;
using OpenBveApi.Textures;
using OpenBveApi.Trains;
using OpenBveApi.World;
using RouteManager2.MessageManager;

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

		public override void AddMessage(MessageType type, bool FileNotFound, string text)
		{
			Interface.AddMessage(type, FileNotFound, text);
		}

		public override void AddMessage(object Message)
		{
			MessageManager.AddMessage((AbstractMessage)Message);
		}
		
		// --- texture ---
		
		/// <summary>Queries the dimensions of a texture.</summary>
		/// <param name="path">The path to the file or folder that contains the texture.</param>
		/// <param name="width">Receives the width of the texture.</param>
		/// <param name="height">Receives the height of the texture.</param>
		/// <returns>Whether querying the dimensions was successful.</returns>
		public override bool QueryTextureDimensions(string path, out int width, out int height) {
			if (System.IO.File.Exists(path) || System.IO.Directory.Exists(path)) {
				for (int i = 0; i < Program.CurrentHost.Plugins.Length; i++) {
					if (Program.CurrentHost.Plugins[i].Texture != null) {
						try {
							if (Program.CurrentHost.Plugins[i].Texture.CanLoadTexture(path)) {
								try {
									if (Program.CurrentHost.Plugins[i].Texture.QueryTextureDimensions(path, out width, out height)) {
										return true;
									}
									Interface.AddMessage(MessageType.Error, false,
									                     "Plugin " + Program.CurrentHost.Plugins[i].Title + " returned unsuccessfully at QueryTextureDimensions"
									                    );
								} catch (Exception ex) {
									Interface.AddMessage(MessageType.Error, false,
									                     "Plugin " + Program.CurrentHost.Plugins[i].Title + " raised the following exception at QueryTextureDimensions:" + ex.Message
									                    );
								}
							}
						} catch (Exception ex) {
							Interface.AddMessage(MessageType.Error, false,
							                     "Plugin " + Program.CurrentHost.Plugins[i].Title + " raised the following exception at CanLoadTexture:" + ex.Message
							                    );
						}
					}
				}
				FileInfo f = new FileInfo(path);
				if (f.Length == 0)
				{
					Interface.AddMessage(MessageType.Error, false, "Zero-byte texture file encountered at " + path);
				}
				else
				{
					Interface.AddMessage(MessageType.Error, false, "No plugin found that is capable of loading texture " + path);
				}
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
				for (int i = 0; i < Program.CurrentHost.Plugins.Length; i++) {
					if (Program.CurrentHost.Plugins[i].Texture != null) {
						try {
							if (Program.CurrentHost.Plugins[i].Texture.CanLoadTexture(path)) {
								try {
									if (Program.CurrentHost.Plugins[i].Texture.LoadTexture(path, out texture)) {
										texture.CompatibleTransparencyMode = Interface.CurrentOptions.OldTransparencyMode;
										texture = texture.ApplyParameters(parameters);
										return true;
									}
									Interface.AddMessage(MessageType.Error, false, "Plugin " + Program.CurrentHost.Plugins[i].Title + " returned unsuccessfully at LoadTexture");
								} catch (Exception ex) {
									Interface.AddMessage(MessageType.Error, false, "Plugin " + Program.CurrentHost.Plugins[i].Title + " raised the following exception at LoadTexture:" + ex.Message);
								}
							}
						} catch (Exception ex) {
							Interface.AddMessage(MessageType.Error, false, "Plugin " + Program.CurrentHost.Plugins[i].Title + " raised the following exception at CanLoadTexture:" + ex.Message);
						}
					}
				}
				FileInfo f = new FileInfo(path);
				if (f.Length == 0)
				{
					Interface.AddMessage(MessageType.Error, false, "Zero-byte texture file encountered at " + path);
				}
				else
				{
					Interface.AddMessage(MessageType.Error, false, "No plugin found that is capable of loading texture " + path);
				}
			} else {
				ReportProblem(OpenBveApi.Hosts.ProblemType.PathNotFound, path);
			}
			texture = null;
			return false;
		}

		public override bool LoadTexture(Texture Texture, OpenGlTextureWrapMode wrapMode)
		{
			return Program.Renderer.TextureManager.LoadTexture(Texture, wrapMode, CPreciseTimer.GetClockTicks(), Interface.CurrentOptions.Interpolation, Interface.CurrentOptions.AnisotropicFilteringLevel);
		}
		
		/// <summary>Registers a texture and returns a handle to the texture.</summary>
		/// <param name="path">The path to the file or folder that contains the texture.</param>
		/// <param name="parameters">The parameters that specify how to process the texture.</param>
		/// <param name="handle">Receives the handle to the texture.</param>
		/// <returns>Whether loading the texture was successful.</returns>
		public override bool RegisterTexture(string path, TextureParameters parameters, out Texture handle) {
			if (System.IO.File.Exists(path) || System.IO.Directory.Exists(path)) {
				Texture data;
				if (Program.Renderer.TextureManager.RegisterTexture(path, parameters, out data)) {
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
			handle = Program.Renderer.TextureManager.RegisterTexture(texture);
			return true;
		}
		
		public override bool RegisterTexture(Bitmap texture, TextureParameters parameters, out OpenBveApi.Textures.Texture handle)
		{
			handle = new Texture(texture, parameters);
			return true;
		}
		
		// --- sound ---
		
		/// <summary>Loads a sound and returns the sound data.</summary>
		/// <param name="path">The path to the file or folder that contains the sound.</param>
		/// <param name="sound">Receives the sound.</param>
		/// <returns>Whether loading the sound was successful.</returns>
		public override bool LoadSound(string path, out OpenBveApi.Sounds.Sound sound) {
			if (System.IO.File.Exists(path) || System.IO.Directory.Exists(path)) {
				for (int i = 0; i < Program.CurrentHost.Plugins.Length; i++) {
					if (Program.CurrentHost.Plugins[i].Sound != null) {
						try {
							if (Program.CurrentHost.Plugins[i].Sound.CanLoadSound(path)) {
								try {
									if (Program.CurrentHost.Plugins[i].Sound.LoadSound(path, out sound)) {
										return true;
									}
									Interface.AddMessage(MessageType.Error, false, "Plugin " + Program.CurrentHost.Plugins[i].Title + " returned unsuccessfully at LoadSound");
								} catch (Exception ex) {
									Interface.AddMessage(MessageType.Error, false, "Plugin " + Program.CurrentHost.Plugins[i].Title + " raised the following exception at LoadSound:" + ex.Message);
								}
							}
						} catch (Exception ex) {
							Interface.AddMessage(MessageType.Error, false, "Plugin " + Program.CurrentHost.Plugins[i].Title + " raised the following exception at CanLoadSound:" + ex.Message);
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
		public override bool RegisterSound(string path, out OpenBveApi.Sounds.SoundHandle handle)
		{
			if (System.IO.File.Exists(path) || System.IO.Directory.Exists(path))
			{
				handle = Program.Sounds.RegisterBuffer(path, 0.0);
			}
			else
			{
				ReportProblem(OpenBveApi.Hosts.ProblemType.PathNotFound, path);
			}
			handle = null;
			return false;
		}

		/// <summary>Registers a sound and returns a handle to the sound.</summary>
		/// <param name="path">The path to the file or folder that contains the sound.</param>
		/// /// <param name="radius">The sound radius</param>
		/// <param name="handle">Receives a handle to the sound.</param>
		/// <returns>Whether loading the sound was successful.</returns>
		public override bool RegisterSound(string path, double radius, out OpenBveApi.Sounds.SoundHandle handle)
		{
			if (System.IO.File.Exists(path) || System.IO.Directory.Exists(path))
			{
				handle = Program.Sounds.RegisterBuffer(path, radius);
				return true;
			}
			ReportProblem(OpenBveApi.Hosts.ProblemType.PathNotFound, path);
			handle = null;
			return false;
		}

		/// <summary>Registers a sound and returns a handle to the sound.</summary>
		/// <param name="sound">The sound data.</param>
		/// <param name="handle">Receives a handle to the sound.</param>
		/// <returns>Whether loading the sound was successful.</returns>
		public override bool RegisterSound(OpenBveApi.Sounds.Sound sound, out OpenBveApi.Sounds.SoundHandle handle)
		{
			handle = Program.Sounds.RegisterBuffer(sound, 15.0);
			return true;
		}

		public override bool LoadStaticObject(string path, System.Text.Encoding Encoding, bool PreserveVertices, out StaticObject Object)
		{
			if (base.LoadStaticObject(path, Encoding, PreserveVertices, out Object))
			{
				return true;
			}

			if (System.IO.File.Exists(path) || System.IO.Directory.Exists(path)) {
				Encoding = TextEncoding.GetSystemEncodingFromFile(path, Encoding);

				for (int i = 0; i < Program.CurrentHost.Plugins.Length; i++) {
					if (Program.CurrentHost.Plugins[i].Object != null) {
						try {
							if (Program.CurrentHost.Plugins[i].Object.CanLoadObject(path)) {
								try {
									UnifiedObject unifiedObject;
									if (Program.CurrentHost.Plugins[i].Object.LoadObject(path, Encoding, out unifiedObject)) {
										StaticObject staticObject = unifiedObject as StaticObject;
										if (staticObject != null)
										{
											staticObject.OptimizeObject(PreserveVertices, Interface.CurrentOptions.ObjectOptimizationBasicThreshold, Interface.CurrentOptions.ObjectOptimizationVertexCulling);
											Object = staticObject;
											StaticObjectCache.Add(ValueTuple.Create(path, PreserveVertices), Object);
											return true;
										}

										Object = null;
										Interface.AddMessage(MessageType.Error, false, "Attempted to load " + path + " which is an animated object where only static objects are allowed.");
									}
									Interface.AddMessage(MessageType.Error, false, "Plugin " + Program.CurrentHost.Plugins[i].Title + " returned unsuccessfully at LoadObject");
								} catch (Exception ex) {
									Interface.AddMessage(MessageType.Error, false, "Plugin " + Program.CurrentHost.Plugins[i].Title + " raised the following exception at LoadObject:" + ex.Message);
								}
							}
						} catch (Exception ex) {
							Interface.AddMessage(MessageType.Error, false, "Plugin " + Program.CurrentHost.Plugins[i].Title + " raised the following exception at CanLoadObject:" + ex.Message);
						}
					}
				}
				Interface.AddMessage(MessageType.Error, false, "No plugin found that is capable of loading object " + path);
			} else {
				ReportProblem(OpenBveApi.Hosts.ProblemType.PathNotFound, path);
			}
			Object = null;
			return false;
		}

		public override bool LoadObject(string path, System.Text.Encoding Encoding, out UnifiedObject Object)
		{
			if (base.LoadObject(path, Encoding, out Object))
			{
				return true;
			}

			if (System.IO.File.Exists(path) || System.IO.Directory.Exists(path)) {
				Encoding = TextEncoding.GetSystemEncodingFromFile(path, Encoding);

				for (int i = 0; i < Program.CurrentHost.Plugins.Length; i++) {
					if (Program.CurrentHost.Plugins[i].Object != null) {
						try {
							if (Program.CurrentHost.Plugins[i].Object.CanLoadObject(path)) {
								try
								{
									UnifiedObject obj;
									if (Program.CurrentHost.Plugins[i].Object.LoadObject(path, Encoding, out obj)) {
										obj.OptimizeObject(false, Interface.CurrentOptions.ObjectOptimizationBasicThreshold, Interface.CurrentOptions.ObjectOptimizationVertexCulling);
										Object = obj;

										StaticObject staticObject = Object as StaticObject;
										if (staticObject != null)
										{
											StaticObjectCache.Add(ValueTuple.Create(path, false), staticObject);
											return true;
										}

										AnimatedObjectCollection aoc = Object as AnimatedObjectCollection;
										if (aoc != null)
										{
											AnimatedObjectCollectionCache.Add(path, aoc);
										}

										return true;
									}
									Interface.AddMessage(MessageType.Error, false, "Plugin " + Program.CurrentHost.Plugins[i].Title + " returned unsuccessfully at LoadObject");
								} catch (Exception ex) {
									Interface.AddMessage(MessageType.Error, false, "Plugin " + Program.CurrentHost.Plugins[i].Title + " raised the following exception at LoadObject:" + ex.Message);
								}
							}
						} catch (Exception ex) {
							Interface.AddMessage(MessageType.Error, false, "Plugin " + Program.CurrentHost.Plugins[i].Title + " raised the following exception at CanLoadObject:" + ex.Message);
						}
					}
				}
				Interface.AddMessage(MessageType.Error, false, "No plugin found that is capable of loading object " + path);
			} else {
				ReportProblem(OpenBveApi.Hosts.ProblemType.PathNotFound, path);
			}
			Object = null;
			return false;
		}

		public override void ExecuteFunctionScript(OpenBveApi.FunctionScripting.FunctionScript functionScript, AbstractTrain train, int CarIndex, Vector3 Position, double TrackPosition, int SectionIndex, bool IsPartOfTrain, double TimeElapsed, int CurrentState)
		{
			FunctionScripts.ExecuteFunctionScript(functionScript, (TrainManager.Train)train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, CurrentState);
		}

		public override int CreateStaticObject(StaticObject Prototype, Vector3 Position, Transformation BaseTransformation, Transformation AuxTransformation, double AccurateObjectDisposalZOffset, double StartingDistance, double EndingDistance, double TrackPosition, double Brightness)
		{
			return Program.Renderer.CreateStaticObject(Prototype, Position, BaseTransformation, AuxTransformation, Program.CurrentRoute.AccurateObjectDisposal, AccurateObjectDisposalZOffset, StartingDistance, EndingDistance, Program.CurrentRoute.BlockLength, TrackPosition, Brightness);
		}

		public override int CreateStaticObject(StaticObject Prototype, Transformation AuxTransformation, Matrix4D Rotate, Matrix4D Translate, double AccurateObjectDisposalZOffset, double StartingDistance, double EndingDistance, double TrackPosition, double Brightness)
		{
			return Program.Renderer.CreateStaticObject(Prototype, AuxTransformation, Rotate, Translate, Program.CurrentRoute.AccurateObjectDisposal, AccurateObjectDisposalZOffset, StartingDistance, EndingDistance, Program.CurrentRoute.BlockLength, TrackPosition, Brightness);
		}

		public override void CreateDynamicObject(ref ObjectState internalObject)
		{
			Program.Renderer.CreateDynamicObject(ref internalObject);
		}

		public override void AddObjectForCustomTimeTable(AnimatedObject animatedObject)
		{
			Timetable.AddObjectForCustomTimetable(animatedObject);
		}

		public override void ShowObject(ObjectState objectToShow, ObjectType objectType)
		{
			Program.Renderer.VisibleObjects.ShowObject(objectToShow, objectType);
		}

		public override void HideObject(ObjectState objectToHide)
		{
			Program.Renderer.VisibleObjects.HideObject(objectToHide);
		}

		public override bool SoundIsPlaying(object SoundSource)
		{
			return Program.Sounds.IsPlaying(SoundSource);
		}

		public override object PlaySound(SoundHandle buffer, double pitch, double volume, Vector3 position, object parent, bool looped)
		{
			return Program.Sounds.PlaySound(buffer, pitch, volume, position, parent, looped);
		}

		public override void PlayMicSound(OpenBveApi.Math.Vector3 position, double backwardTolerance, double forwardTolerance)
		{
			Program.Sounds.PlayMicSound(position, backwardTolerance, forwardTolerance);
		}

		public override void StopSound(object SoundSource)
		{
			Program.Sounds.StopSound(SoundSource);
		}

		public override bool SimulationSetup
		{
			get
			{
				return Loading.SimulationSetup;
			}
		}

		public override int AnimatedWorldObjectsUsed
		{
			get
			{
				return ObjectManager.AnimatedWorldObjectsUsed;
			}
			set
			{
				int a = ObjectManager.AnimatedWorldObjectsUsed;
				if (ObjectManager.AnimatedWorldObjects.Length -1 == a)
				{
					/*
					 * HACK: We cannot resize an array via an accessor property
					 *       With this in mind, resize it via the indexer instead
					 */
					Array.Resize(ref ObjectManager.AnimatedWorldObjects, ObjectManager.AnimatedWorldObjects.Length << 1);
				}

				ObjectManager.AnimatedWorldObjectsUsed = value;
			}
		}

		public override WorldObject[] AnimatedWorldObjects
		{
			get
			{
				return ObjectManager.AnimatedWorldObjects;
			}
			set
			{
				ObjectManager.AnimatedWorldObjects = value;
			}
		}

		public override Dictionary<int, Track> Tracks
		{
			get
			{
				return Program.CurrentRoute.Tracks;
			}
			set
			{
				Program.CurrentRoute.Tracks = value;
			}
		}

		public override void UpdateCustomTimetable(Texture Daytime, Texture Nighttime)
		{
			Timetable.UpdateCustomTimetable(Daytime, Nighttime);
		}

		public override AbstractTrain ParseTrackFollowingObject(string objectPath, string tfoFile)
		{
			return TrackFollowingObjectParser.ParseTrackFollowingObject(objectPath, tfoFile);
		}

		public override void AddMarker(Texture MarkerTexture)
		{
			Program.Renderer.Marker.AddMarker(MarkerTexture);
		}

		public override void RemoveMarker(Texture MarkerTexture)
		{
			Program.Renderer.Marker.RemoveMarker(MarkerTexture);
		}

		public override void CameraAtWorldEnd()
		{
			Program.Renderer.Camera.AtWorldEnd = !Program.Renderer.Camera.AtWorldEnd;
		}

		public Host() : base(HostApplication.OpenBve)
		{
		}
	}
}
