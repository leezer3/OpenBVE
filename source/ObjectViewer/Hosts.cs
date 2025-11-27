using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using OpenBveApi;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using OpenBveApi.Textures;
using OpenBveApi.Trains;
using OpenBveApi.World;
using TrainManager;
using TrainManager.Trains;
using Path = OpenBveApi.Path;

namespace ObjectViewer {
	/// <summary>Represents the host application.</summary>
	internal class Host : HostInterface {
		
		// --- functions ---
		
		/// <summary>Reports a problem to the host application.</summary>
		/// <param name="type">The type of problem that is reported.</param>
		/// <param name="text">The textual message that describes the problem.</param>
		public override void ReportProblem(ProblemType type, string text) {
			switch (type)
			{
				case ProblemType.DirectoryNotFound:
				case ProblemType.FileNotFound:
				case ProblemType.PathNotFound:
					if (!MissingFiles.Contains(text))
					{
						Interface.AddMessage(MessageType.Error, true, type + " : " + text);
					}
					break;
				default:
					Interface.AddMessage(MessageType.Error, false, type + " : " + text);
					break;
			}
		}

		public override void AddMessage(MessageType type, bool FileNotFound, string text)
		{
			Interface.AddMessage(type, FileNotFound, text);
		}

		// --- texture ---
		
		/// <summary>Queries the dimensions of a texture.</summary>
		/// <param name="path">The path to the file or folder that contains the texture.</param>
		/// <param name="width">Receives the width of the texture.</param>
		/// <param name="height">Receives the height of the texture.</param>
		/// <returns>Whether querying the dimensions was successful.</returns>
		public override bool QueryTextureDimensions(string path, out int width, out int height) {
			if (File.Exists(path) || Directory.Exists(path)) {
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
				ReportProblem(ProblemType.PathNotFound, path);
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
			if (File.Exists(path) || Directory.Exists(path)) {
				for (int i = 0; i < Program.CurrentHost.Plugins.Length; i++) {
					if (Program.CurrentHost.Plugins[i].Texture != null) {
						try {
							if (Program.CurrentHost.Plugins[i].Texture.CanLoadTexture(path)) {
								try {
									if (Program.CurrentHost.Plugins[i].Texture.LoadTexture(path, out texture)) {
										texture.CompatibleTransparencyMode = false;
										texture = texture.ApplyParameters(parameters);
										return true;
									}
									if(!FailedTextures.Contains(path))
									{
										FailedTextures.Add(path);
										Interface.AddMessage(MessageType.Error, false, "Plugin " + Program.CurrentHost.Plugins[i].Title + " returned unsuccessfully at LoadTexture");
									}
									
								} catch (Exception ex) {
									// exception may be transient
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
					if (!FailedTextures.Contains(path))
					{
						FailedTextures.Add(path);
						Interface.AddMessage(MessageType.Error, false, "Zero-byte texture file encountered at " + path);
					}
				}
				else
				{
					if (!FailedTextures.Contains(path))
					{
						FailedTextures.Add(path);
						Interface.AddMessage(MessageType.Error, false, "No plugin found that is capable of loading texture " + path);
					}
				}
			} else {
				ReportProblem(ProblemType.PathNotFound, path);
			}
			texture = null;
			return false;
		}
		
		public override bool LoadTexture(ref Texture Texture, OpenGlTextureWrapMode wrapMode)
		{
			return Program.Renderer.TextureManager.LoadTexture(ref Texture, wrapMode, CPreciseTimer.GetClockTicks(), Interface.CurrentOptions.Interpolation, Interface.CurrentOptions.AnisotropicFilteringLevel);
		}

		public override bool RegisterTexture(string path, TextureParameters parameters, out Texture handle, bool loadTexture = false, int timeout = 1000) {
			if (File.Exists(path) || Directory.Exists(path)) {
				if (Program.Renderer.TextureManager.RegisterTexture(path, parameters, out Texture data)) {
					handle = data;
					if (loadTexture)
					{
						LoadTexture(ref data, OpenGlTextureWrapMode.ClampClamp);
					}
					return true;
				}
			} else {
				ReportProblem(ProblemType.PathNotFound, path);
			}
			handle = null;
			return false;
		}

		public override bool RegisterTexture(Bitmap texture, TextureParameters parameters, out Texture handle)
		{
			handle = new Texture(texture, parameters);
			return true;
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
		
		
		// --- sound ---
		
		/// <summary>Loads a sound and returns the sound data.</summary>
		/// <param name="path">The path to the file or folder that contains the sound.</param>
		/// <param name="sound">Receives the sound.</param>
		/// <returns>Whether loading the sound was successful.</returns>
		public override bool LoadSound(string path, out OpenBveApi.Sounds.Sound sound) {
			if (File.Exists(path) || Directory.Exists(path)) {
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
				ReportProblem(ProblemType.PathNotFound, path);
			}
			sound = null;
			return false;
		}
		
		/// <summary>Registers a sound and returns a handle to the sound.</summary>
		/// <param name="path">The path to the file or folder that contains the sound.</param>
		/// <param name="handle">Receives a handle to the sound.</param>
		/// <returns>Whether loading the sound was successful.</returns>
		public override bool RegisterSound(string path, out OpenBveApi.Sounds.SoundHandle handle) {
			if (File.Exists(path) || Directory.Exists(path)) {
				// Sounds.SoundBuffer data;
				// data = Sounds.RegisterBuffer(path, 0.0); // TODO
			} else {
				ReportProblem(ProblemType.PathNotFound, path);
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

		public override bool LoadObject(string path, System.Text.Encoding Encoding, out UnifiedObject Object)
		{
			if (base.LoadObject(path, Encoding, out Object))
			{
				return true;
			}

			if (File.Exists(path) || Directory.Exists(path))
			{
				Encoding = TextEncoding.GetSystemEncodingFromFile(path, Encoding);

				for (int i = 0; i < Program.CurrentHost.Plugins.Length; i++)
				{
					if (Program.CurrentHost.Plugins[i].Object != null)
					{
						try
						{
							if (Program.CurrentHost.Plugins[i].Object.CanLoadObject(path))
							{
								try
								{
									if (Program.CurrentHost.Plugins[i].Object.LoadObject(path, Encoding, out UnifiedObject obj))
									{
										if (obj == null)
										{
											continue;
										}
										obj.OptimizeObject(false, Interface.CurrentOptions.ObjectOptimizationBasicThreshold, true);
										Object = obj;

										if (Object is StaticObject staticObject)
										{
											StaticObjectCache.Add(ValueTuple.Create(path, false, File.GetLastWriteTime(path)), staticObject);
											return true;
										}

										if (Object is AnimatedObjectCollection aoc)
										{
											AnimatedObjectCollectionCache.Add(path, aoc);
										}

										return true;
									}
									if (!FailedObjects.Contains(path))
									{
										FailedObjects.Add(path);
										Interface.AddMessage(MessageType.Error, false, "Plugin " + Program.CurrentHost.Plugins[i].Title + " returned unsuccessfully at LoadObject");
									}

								}
								catch (Exception ex)
								{
									// exception may be transient
									Interface.AddMessage(MessageType.Error, false, "Plugin " + Program.CurrentHost.Plugins[i].Title + " raised the following exception at LoadObject:" + ex.Message);
								}
							}
						}
						catch (Exception ex)
						{
							Interface.AddMessage(MessageType.Error, false, "Plugin " + Program.CurrentHost.Plugins[i].Title + " raised the following exception at CanLoadObject:" + ex.Message);
						}
					}
				}
				FileInfo f = new FileInfo(path);
				if (f.Length == 0)
				{
					if (!NullFiles.Contains(Path.GetFileNameWithoutExtension(path).ToLowerInvariant()) && !FailedObjects.Contains(path))
					{
						FailedObjects.Add(path);
						Interface.AddMessage(MessageType.Error, false, "Zero-byte object file encountered at " + path);
					}
				}
				else
				{
					if (!NullFiles.Contains(Path.GetFileNameWithoutExtension(path).ToLowerInvariant()) && !FailedObjects.Contains(path))
					{
						FailedObjects.Add(path);
						Interface.AddMessage(MessageType.Error, false, "No plugin found that is capable of loading object " + path);
					}
				}
			}
			else
			{
				ReportProblem(ProblemType.PathNotFound, path);
			}
			Object = null;
			return false;
		}

		public override void ExecuteFunctionScript(OpenBveApi.FunctionScripting.FunctionScript functionScript, AbstractTrain train, int CarIndex, Vector3 Position, double TrackPosition, int SectionIndex, bool IsPartOfTrain, double TimeElapsed, int CurrentState)
		{
			FunctionScripts.ExecuteFunctionScript(functionScript, (TrainBase)train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, CurrentState);
		}

		public override int CreateStaticObject(StaticObject Prototype, Vector3 Position, Transformation WorldTransformation, Transformation LocalTransformation, double AccurateObjectDisposalZOffset, double StartingDistance, double EndingDistance, double TrackPosition)
		{
			return Program.Renderer.CreateStaticObject(Prototype, Position, WorldTransformation, LocalTransformation, ObjectDisposalMode.Accurate, AccurateObjectDisposalZOffset, StartingDistance, EndingDistance, 25.0, TrackPosition);
		}

		public override int CreateStaticObject(StaticObject Prototype, Vector3 Position, Transformation LocalTransformation, Matrix4D Rotate, Matrix4D Translate, double AccurateObjectDisposalZOffset, double StartingDistance, double EndingDistance, double TrackPosition)
		{
			return Program.Renderer.CreateStaticObject(Position, Prototype, LocalTransformation, Rotate, Translate, ObjectDisposalMode.Accurate, AccurateObjectDisposalZOffset, StartingDistance, EndingDistance, 25.0, TrackPosition);
		}

		public override void CreateDynamicObject(ref ObjectState internalObject)
		{
			Program.Renderer.CreateDynamicObject(ref internalObject);
		}

		public override void ShowObject(ObjectState objectToShow, ObjectType objectType)
		{
			Program.Renderer.VisibleObjects.ShowObject(objectToShow, objectType);
		}

		public override void HideObject(ObjectState objectToHide)
		{
			Program.Renderer.VisibleObjects.HideObject(objectToHide);
		}

		public override int AnimatedWorldObjectsUsed
		{
			get => ObjectManager.AnimatedWorldObjectsUsed;
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
			get => ObjectManager.AnimatedWorldObjects;
			set => ObjectManager.AnimatedWorldObjects = value;
		}

		public override Dictionary<int, Track> Tracks
		{
			get => Program.CurrentRoute.Tracks;
			set => Program.CurrentRoute.Tracks = value;
		}

		public override AbstractTrain ParseTrackFollowingObject(string objectPath, string tfoFile)
		{
			throw new NotImplementedException();
		}

		// ReSharper disable once CoVariantArrayConversion
		public override IEnumerable<AbstractTrain> Trains => Program.TrainManager.Trains;

		public override AbstractTrain ClosestTrain(AbstractTrain Train)
		{
			/*
			 * At present, there should only ever be the single player train ref present in ObjectViewer
			 * This may change at some point, but for the minute it's fixed....
			 */
			return TrainManagerBase.PlayerTrain;
		}

		public override AbstractTrain ClosestTrain(Vector3 worldPosition)
		{
			return TrainManagerBase.PlayerTrain;
		}

		public Host() : base(HostApplication.ObjectViewer)
		{
		}
	}
}
