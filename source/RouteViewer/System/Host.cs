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
using OpenBveApi.Sounds;
using OpenBveApi.Textures;
using OpenBveApi.Trains;
using OpenBveApi.World;
using RouteManager2.MessageManager;
using SoundManager;
using TrainManager.Trains;
using Path = OpenBveApi.Path;

namespace RouteViewer
{
	/// <summary>Represents the host application.</summary>
	internal class Host : HostInterface
	{
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
		public override bool QueryTextureDimensions(string path, out int width, out int height)
		{
			if (File.Exists(path) || Directory.Exists(path))
			{
				for (int i = 0; i < Program.CurrentHost.Plugins.Length; i++)
				{
					if (Program.CurrentHost.Plugins[i].Texture != null)
					{
						try
						{
							if (Program.CurrentHost.Plugins[i].Texture.CanLoadTexture(path))
							{
								try
								{
									if (Program.CurrentHost.Plugins[i].Texture.QueryTextureDimensions(path, out width, out height))
									{
										return true;
									}
									Interface.AddMessage(MessageType.Error, false,
														 "Plugin " + Program.CurrentHost.Plugins[i].Title + " returned unsuccessfully at QueryTextureDimensions"
														);
								}
								catch (Exception ex)
								{
									Interface.AddMessage(MessageType.Error, false,
														 "Plugin " + Program.CurrentHost.Plugins[i].Title + " raised the following exception at QueryTextureDimensions:" + ex.Message
														);
								}
							}
						}
						catch (Exception ex)
						{
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
			}
			else
			{
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
		public override bool LoadTexture(string path, TextureParameters parameters, out Texture texture)
		{
			if (File.Exists(path) || Directory.Exists(path))
			{
				for (int i = 0; i < Program.CurrentHost.Plugins.Length; i++)
				{
					if (Program.CurrentHost.Plugins[i].Texture != null)
					{
						try
						{
							if (Program.CurrentHost.Plugins[i].Texture.CanLoadTexture(path))
							{
								try
								{
									if (Program.CurrentHost.Plugins[i].Texture.LoadTexture(path, out texture))
									{
										texture.CompatibleTransparencyMode = false;
										texture = texture.ApplyParameters(parameters);
										return true;
									}
									if (!FailedTextures.Contains(path))
									{
										FailedTextures.Add(path);
										Interface.AddMessage(MessageType.Error, false, "Plugin " + Program.CurrentHost.Plugins[i].Title + " returned unsuccessfully at LoadTexture");
									}

								}
								catch (Exception ex)
								{
									// exception may be transient
									Interface.AddMessage(MessageType.Error, false, "Plugin " + Program.CurrentHost.Plugins[i].Title + " raised the following exception at LoadTexture:" + ex.Message);
								}
							}
						}
						catch (Exception ex)
						{
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
			}
			else
			{
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

		public override bool RegisterTexture(Bitmap texture, TextureParameters parameters, out Texture handle)
		{
			handle = new Texture(texture, parameters);
			return true;
		}
		
		// --- sound ---

		/// <summary>Loads a sound and returns the sound data.</summary>
		/// <param name="path">The path to the file or folder that contains the sound.</param>
		/// <param name="sound">Receives the sound.</param>
		/// <returns>Whether loading the sound was successful.</returns>
		public override bool LoadSound(string path, out OpenBveApi.Sounds.Sound sound)
		{
			if (File.Exists(path) || Directory.Exists(path))
			{
				for (int i = 0; i < Program.CurrentHost.Plugins.Length; i++)
				{
					if (Program.CurrentHost.Plugins[i].Sound != null)
					{
						try
						{
							if (Program.CurrentHost.Plugins[i].Sound.CanLoadSound(path))
							{
								try
								{
									if (Program.CurrentHost.Plugins[i].Sound.LoadSound(path, out sound))
									{
										return true;
									}
									Interface.AddMessage(MessageType.Error, false, "Plugin " + Program.CurrentHost.Plugins[i].Title + " returned unsuccessfully at LoadSound");
								}
								catch (Exception ex)
								{
									Interface.AddMessage(MessageType.Error, false, "Plugin " + Program.CurrentHost.Plugins[i].Title + " raised the following exception at LoadSound:" + ex.Message);
								}
							}
						}
						catch (Exception ex)
						{
							Interface.AddMessage(MessageType.Error, false, "Plugin " + Program.CurrentHost.Plugins[i].Title + " raised the following exception at CanLoadSound:" + ex.Message);
						}
					}
				}
				Interface.AddMessage(MessageType.Error, false, "No plugin found that is capable of loading sound " + path);
			}
			else
			{
				ReportProblem(ProblemType.PathNotFound, path);
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
			if (File.Exists(path) || Directory.Exists(path))
			{
				handle = Program.Sounds.RegisterBuffer(path, 0.0);
				return true;
			}
			else
			{
				ReportProblem(ProblemType.PathNotFound, path);
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
			if (File.Exists(path) || Directory.Exists(path))
			{
				handle = Program.Sounds.RegisterBuffer(path, radius);
				return true;
			}
			ReportProblem(ProblemType.PathNotFound, path);
			handle = null;
			return false;
		}

		/// <summary>Registers a sound and returns a handle to the sound.</summary>
		/// <param name="sound">The sound data.</param>
		/// <param name="handle">Receives a handle to the sound.</param>
		/// <returns>Whether loading the sound was successful.</returns>
		public override bool RegisterSound(OpenBveApi.Sounds.Sound sound, out OpenBveApi.Sounds.SoundHandle handle)
		{
			handle = Program.Sounds.RegisterBuffer(sound, 0.0);
			return true;
		}

		public override bool LoadStaticObject(string path, System.Text.Encoding Encoding, bool PreserveVertices, out StaticObject Object)
		{
			if (base.LoadStaticObject(path, Encoding, PreserveVertices, out Object))
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
									if (Program.CurrentHost.Plugins[i].Object.LoadObject(path, Encoding, out var unifiedObject))
									{
										if (unifiedObject is StaticObject staticObject)
										{
											staticObject.OptimizeObject(PreserveVertices, Interface.CurrentOptions.ObjectOptimizationBasicThreshold, true);
											Object = staticObject;
											StaticObjectCache.Add(ValueTuple.Create(path, PreserveVertices, File.GetLastWriteTime(path)), Object);
											return true;
										}

										Object = null;
										// may be trying to load in different places, so leave
										Interface.AddMessage(MessageType.Error, false, "Attempted to load " + path + " which is an animated object where only static objects are allowed.");
									}
									if (!FailedObjects.Contains(path))
									{
										FailedObjects.Add(path);
										Interface.AddMessage(MessageType.Error, false, "Plugin " + Program.CurrentHost.Plugins[i].Title + " returned unsuccessfully at LoadObject");
									}

								}
								catch (Exception ex)
								{
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
				if (!FailedObjects.Contains(path))
				{
					FailedObjects.Add(path);
					Interface.AddMessage(MessageType.Error, false, "No plugin found that is capable of loading object " + path);
				}

			}
			else
			{
				ReportProblem(ProblemType.PathNotFound, path);
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
			FunctionScripts.ExecuteFunctionScript(functionScript, (TrainManager.Train)train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, CurrentState);
		}

		public override int CreateStaticObject(StaticObject Prototype, Vector3 Position, Transformation WorldTransformation, Transformation LocalTransformation, double AccurateObjectDisposalZOffset, double StartingDistance, double EndingDistance, double TrackPosition)
		{
			return Program.Renderer.CreateStaticObject(Prototype, Position, WorldTransformation, LocalTransformation, Program.CurrentRoute.AccurateObjectDisposal, AccurateObjectDisposalZOffset, StartingDistance, EndingDistance, Program.CurrentRoute.BlockLength, TrackPosition);
		}

		public override int CreateStaticObject(StaticObject Prototype, Vector3 Position, Transformation LocalTransformation, Matrix4D Rotate, Matrix4D Translate, double AccurateObjectDisposalZOffset, double StartingDistance, double EndingDistance, double TrackPosition)
		{
			return Program.Renderer.CreateStaticObject(Position, Prototype, LocalTransformation, Rotate, Translate, Program.CurrentRoute.AccurateObjectDisposal, AccurateObjectDisposalZOffset, StartingDistance, EndingDistance, Program.CurrentRoute.BlockLength, TrackPosition);
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

		public override bool SoundIsPlaying(object SoundSource)
		{
			return Program.Sounds.IsPlaying(SoundSource);
		}

		public override object PlaySound(SoundHandle buffer, double pitch, double volume, Vector3 position, object parent, bool looped)
		{
			return Program.Sounds.PlaySound(buffer, pitch, volume, position, parent, looped);
		}

		public override void StopSound(object SoundSource)
		{
			Program.Sounds.StopSound(SoundSource as SoundSource);
		}

		public override void StopAllSounds(object parent)
		{
			Program.Sounds.StopAllSounds(parent);
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
			Interface.AddMessage(MessageType.Warning, false, "Track Following Objects are not shown in Route Viewer. Please test using the main simulation.");
			return null;
		}

		// ReSharper disable once CoVariantArrayConversion
		public override IEnumerable<AbstractTrain> Trains => Program.TrainManager.Trains;

		public override AbstractTrain ClosestTrain(AbstractTrain Train)
		{
			AbstractTrain closestTrain = null;
			double bestLocation = double.MaxValue;
			if(Train is TrainBase baseTrain)
			{
				for (int i = 0; i < Program.TrainManager.Trains.Count; i++)
				{
					if (Program.TrainManager.Trains[i] != baseTrain & Program.TrainManager.Trains[i].State == TrainState.Available & baseTrain.Cars.Length > 0)
					{
						int c = Program.TrainManager.Trains[i].Cars.Length - 1;
						double z = Program.TrainManager.Trains[i].Cars[c].RearAxle.Follower.TrackPosition - Program.TrainManager.Trains[i].Cars[c].RearAxle.Position - 0.5 * Program.TrainManager.Trains[i].Cars[c].Length;
						if (z >= baseTrain.FrontCarTrackPosition & z < bestLocation)
						{
							bestLocation = z;
							closestTrain = Program.TrainManager.Trains[i];
						}
					}
				}
			}
			return closestTrain;
		}

		public override AbstractTrain ClosestTrain(double TrackPosition)
		{
			AbstractTrain closestTrain = null;
			double trainDistance = double.MaxValue;
			for (int j = 0; j < Program.TrainManager.Trains.Count; j++)
			{
				if (Program.TrainManager.Trains[j].State == TrainState.Available)
				{
					double distance;
					if (Program.TrainManager.Trains[j].Cars[0].FrontAxle.Follower.TrackPosition < TrackPosition)
					{
						distance = TrackPosition - Program.TrainManager.Trains[j].Cars[0].TrackPosition;
					}
					else if (Program.TrainManager.Trains[j].Cars[Program.TrainManager.Trains[j].Cars.Length - 1].RearAxle.Follower.TrackPosition > TrackPosition)
					{
						distance = Program.TrainManager.Trains[j].Cars[Program.TrainManager.Trains[j].Cars.Length - 1].RearAxle.Follower.TrackPosition - TrackPosition;
					}
					else
					{
						distance = 0;
					}
					if (distance < trainDistance)
					{
						closestTrain = Program.TrainManager.Trains[j];
						trainDistance = distance;
					}
				}
			}
			return closestTrain;
		}

		public Host() : base(HostApplication.RouteViewer)
		{
		}
	}
}
