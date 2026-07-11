using Formats.OpenBve;
using OpenBveApi.FunctionScripting;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Sounds;
using OpenBveApi.Textures;
using System;
using System.IO;
using Path = OpenBveApi.Path;

namespace Plugin
{
	public partial class Plugin
	{
		/// <summary>Loads a collection of animated objects from a file.</summary>
		/// <param name="FileName">The text file to load the animated object from. Must be an absolute file name.</param>
		/// <param name="Encoding">The encoding the file is saved in. If the file uses a byte order mark, the encoding indicated by the byte order mark is used and the Encoding parameter is ignored.</param>
		/// <returns>The collection of animated objects.</returns>
		private static AnimatedObjectCollection ReadObject(string FileName, System.Text.Encoding Encoding)
		{
			AnimatedObjectCollection Result = new AnimatedObjectCollection(currentHost)
			{
				Objects = new AnimatedObject[4],
				Sounds = new WorldObject[4]
			};
			int ObjectCount = 0;
			int SoundCount = 0;
			// load file

			ConfigFile<AnimatedSection, AnimatedKey> cfg = new ConfigFile<AnimatedSection, AnimatedKey>(File.ReadAllLines(FileName, Encoding), FileName, Plugin.currentHost);

			while (cfg.RemainingSubBlocks > 0)
			{
				var Block = cfg.ReadNextBlock();
				Vector3 Position = Vector3.Zero;
				string Folder = Path.GetDirectoryName(FileName);
				switch (Block.Key)
				{
					case AnimatedSection.Include:
						UnifiedObject[] includeObjects = new UnifiedObject[4];
						int includeObjectsCount = 0;
						if (!Block.GetAllVector3s(AnimatedKey.Position, ',', out Vector3[] includePositions))
						{
							includePositions = new[] { Vector3.Zero };
						}
						while (Block.RemainingDataValues > 0 && Block.GetNextPath(Folder, out string file))
						{
							if (includeObjects.Length == includeObjectsCount)
							{
								Array.Resize(ref includeObjects, includeObjects.Length << 1);
							}

							currentHost.LoadObject(file, Encoding, out includeObjects[includeObjectsCount]);
							includeObjectsCount++;
						}
						for (int j = 0; j < includeObjectsCount; j++)
						{
							if (includeObjects[j] != null)
							{
								for (int p = 0; p < includePositions.Length; p++)
								{
									Position = includePositions[p];
									if (includeObjects[j] is StaticObject s)
									{
										s.Dynamic = true;
										if (ObjectCount >= Result.Objects.Length)
										{
											Array.Resize(ref Result.Objects, Result.Objects.Length << 1);
										}

										AnimatedObject a = new AnimatedObject(currentHost, FileName);
										ObjectState aos = new ObjectState
										{
											Prototype = s,
											Translation = Matrix4D.CreateTranslation(Position.X, Position.Y, -Position.Z)
										};
										a.States = new[] { aos };
										Result.Objects[ObjectCount] = a;
										ObjectCount++;
									}
									else if (includeObjects[j] is AnimatedObjectCollection)
									{
										AnimatedObjectCollection a = (AnimatedObjectCollection)includeObjects[j].Clone();
										for (int k = 0; k < a.Objects.Length; k++)
										{
											if (ObjectCount >= Result.Objects.Length)
											{
												Array.Resize(ref Result.Objects, Result.Objects.Length << 1);
											}

											for (int h = 0; h < a.Objects[k].States.Length; h++)
											{
												a.Objects[k].States[h].Translation *= Matrix4D.CreateTranslation(Position.X, Position.Y, -Position.Z);
											}

											Result.Objects[ObjectCount] = a.Objects[k];
											ObjectCount++;
										}

										for (int kk = 0; kk < a.Sounds.Length; kk++)
										{
											if (SoundCount >= Result.Sounds.Length)
											{
												Array.Resize(ref Result.Sounds, Result.Sounds.Length << 1);
											}

											Result.Sounds[SoundCount] = a.Sounds[kk];
											SoundCount++;
										}
									}
									else if (includeObjects[j] is KeyframeAnimatedObject k)
									{
										currentHost.AddMessage(MessageType.Warning, false, "Including MSTS Shape in an AnimatedObject- Contained animations may be lost. In the Section " + Block.Key + " in file " + FileName);
										StaticObject so = (StaticObject)k;
										so.Dynamic = true;
										if (ObjectCount >= Result.Objects.Length)
										{
											Array.Resize(ref Result.Objects, Result.Objects.Length << 1);
										}

										AnimatedObject a = new AnimatedObject(currentHost, FileName);
										ObjectState aos = new ObjectState
										{
											Prototype = so,
											Translation = Matrix4D.CreateTranslation(Position.X, Position.Y, -Position.Z)
										};
										a.States = new[] { aos };
										Result.Objects[ObjectCount] = a;
										ObjectCount++;
									}
								}
							}
						}

						break;
					case AnimatedSection.Object:
						AnimatedObject template = new AnimatedObject(currentHost, FileName)
						{
							CurrentState = -1,
							TranslateXDirection = Vector3.Right,
							TranslateYDirection = Vector3.Down,
							TranslateZDirection = Vector3.Forward,
							RotateXDirection = Vector3.Right,
							RotateYDirection = Vector3.Down,
							RotateZDirection = Vector3.Forward,
							TextureShiftXDirection = Vector2.Right,
							TextureShiftYDirection = Vector2.Down,
							RefreshRate = 0.0,
						};
						string[] stateFiles = { };
						if (Block.GetPathArray(AnimatedKey.States, ',', Folder, ref stateFiles))
						{
							if (!Block.GetAllVector3s(AnimatedKey.Position, ',', out Vector3[] objectPositions))
							{
								objectPositions = new[] { Vector3.Zero };
							}
							Block.GetFunctionScript(new[] { AnimatedKey.RotateXFunction, AnimatedKey.RotateXFunctionRPN, AnimatedKey.RotateXScript }, Folder, out template.RotateXFunction);
							Block.GetFunctionScript(new[] { AnimatedKey.RotateYFunction, AnimatedKey.RotateYFunctionRPN, AnimatedKey.RotateYScript }, Folder, out template.RotateYFunction);
							Block.GetFunctionScript(new[] { AnimatedKey.RotateZFunction, AnimatedKey.RotateZFunctionRPN, AnimatedKey.RotateZScript }, Folder, out template.RotateZFunction);
							Block.GetFunctionScript(new[] { AnimatedKey.TranslateXFunction, AnimatedKey.TranslateXFunctionRPN, AnimatedKey.TranslateXScript }, Folder, out template.TranslateXFunction);
							Block.GetFunctionScript(new[] { AnimatedKey.TranslateYFunction, AnimatedKey.TranslateYFunctionRPN, AnimatedKey.TranslateYScript }, Folder, out template.TranslateYFunction);
							Block.GetFunctionScript(new[] { AnimatedKey.TranslateZFunction, AnimatedKey.TranslateZFunctionRPN, AnimatedKey.TranslateZScript }, Folder, out template.TranslateZFunction);
							Block.GetFunctionScript(new[] { AnimatedKey.StateFunction, AnimatedKey.StateFunctionRPN, AnimatedKey.StateScript }, Folder, out template.StateFunction);
							Block.GetFunctionScript(new[] { AnimatedKey.TextureShiftXFunction, AnimatedKey.TextureShiftXFunctionRPN, AnimatedKey.TextureShiftXScript }, Folder, out template.TextureShiftXFunction);
							Block.GetFunctionScript(new[] { AnimatedKey.TextureShiftYFunction, AnimatedKey.TextureShiftYFunctionRPN, AnimatedKey.TextureShiftYScript }, Folder, out template.TextureShiftYFunction);
							// n.b. For unknown reasons, the ScaleFunction never had a RPN listing in the animated file. Michelle listed these as obsolete, and I've seen *one* use of them. As they're not really supposed to be used
							// don't add them to the new parser
							Block.GetFunctionScript(new[] { AnimatedKey.ScaleXFunction, AnimatedKey.ScaleXScript }, Folder, out template.ScaleXFunction);
							Block.GetFunctionScript(new[] { AnimatedKey.ScaleYFunction, AnimatedKey.ScaleYScript }, Folder, out template.ScaleYFunction);
							Block.GetFunctionScript(new[] { AnimatedKey.ScaleZFunction, AnimatedKey.ScaleZScript }, Folder, out template.ScaleZFunction);
							Block.TryGetVector3(AnimatedKey.TranslateXDirection, ',', ref template.TranslateXDirection);
							Block.TryGetVector3(AnimatedKey.TranslateYDirection, ',', ref template.TranslateYDirection);
							Block.TryGetVector3(AnimatedKey.TranslateZDirection, ',', ref template.TranslateZDirection);
							Block.TryGetVector3(AnimatedKey.RotateXDirection, ',', ref template.RotateXDirection);
							Block.TryGetVector3(AnimatedKey.RotateYDirection, ',', ref template.RotateYDirection);
							Block.TryGetVector3(AnimatedKey.RotateZDirection, ',', ref template.RotateZDirection);
							Block.TryGetVector2(AnimatedKey.TextureShiftXDirection, ',', ref template.TextureShiftXDirection);
							Block.TryGetVector2(AnimatedKey.TextureShiftYDirection, ',', ref template.TextureShiftYDirection);
							if (Block.GetVector2(AnimatedKey.Axles, ',', out Vector2 axleLocations))
							{
								if (axleLocations.X <= axleLocations.Y)
								{
									currentHost.AddMessage(MessageType.Error, false, "Rear is expected to be less than Front in " + AnimatedKey.Axles + " in file " + FileName);
									axleLocations = new Vector2(0.5, -0.5);
								}
								template.FrontAxlePosition = axleLocations.X;
								template.RearAxlePosition = axleLocations.Y;
							}
							
							Block.GetFunctionScript(new[] { AnimatedKey.TrackFollowerFunction, AnimatedKey.TrackFollowerScript }, Folder, out template.TrackFollowerFunction);
							Block.GetDamping(AnimatedKey.RotateXDamping, ',', out template.RotateXDamping);
							Block.GetDamping(AnimatedKey.RotateYDamping, ',', out template.RotateYDamping);
							Block.GetDamping(AnimatedKey.RotateZDamping, ',', out template.RotateZDamping);

							if (Block.GetValue(AnimatedKey.TextureOverride, out string textureOverride))
							{
								switch (textureOverride.ToLowerInvariant())
								{
									case "none":
										break;
									case "timetable":
										currentHost.AddObjectForCustomTimeTable(template);
										template.StateFunction = new FunctionScript(currentHost, "timetable", true);
										break;
									default:
										currentHost.AddMessage(MessageType.Error, false, "Unknown texture override type " + textureOverride + " in Section " + Block.Key + " in File " + FileName);
										break;
								}
							}

							if (Block.GetValue(AnimatedKey.RefreshRate, out double refreshRate, NumberRange.NonNegative))
							{
								template.RefreshRate = refreshRate;
							}

							template.States = new ObjectState[stateFiles.Length];
							bool forceTextureRepeatX = template.TextureShiftXFunction != null & template.TextureShiftXDirection.X != 0.0 |
							                           template.TextureShiftYFunction != null & template.TextureShiftYDirection.X != 0.0;
							bool forceTextureRepeatY = template.TextureShiftXFunction != null & template.TextureShiftXDirection.Y != 0.0 |
							                           template.TextureShiftYFunction != null & template.TextureShiftYDirection.Y != 0.0;
							for (int k = 0; k < stateFiles.Length; k++)
							{
								template.States[k] = new ObjectState();
								if (stateFiles[k] != null)
								{
									currentHost.LoadObject(stateFiles[k], Encoding, out UnifiedObject currentObject);
									if (currentObject is StaticObject staticObject)
									{
										template.States[k].Prototype = staticObject;
									}
									else if (currentObject is KeyframeAnimatedObject keyframeObject)
									{
										currentHost.AddMessage(MessageType.Warning, false, "Using MSTS Shape " + stateFiles[k] + " as an AnimatedObject State- Contained animations may be lost. In the Section " + Block.Key + " in file " + FileName);
										template.States[k].Prototype = keyframeObject;
									}
									else if (currentObject is AnimatedObjectCollection)
									{
										currentHost.AddMessage(MessageType.Error, false, "Attempted to load the animated object " + stateFiles[k] + " where only static objects are allowed in the Section " + Block.Key + " in file " + FileName);
										continue;
									}

									/*
									 * NOTE: If loading the object fails, or the prototype is null (empty), then we will get a
									 * non-shown state
									 */
									if (template.States[k].Prototype != null)
									{
										template.States[k].Prototype.Dynamic = true;
										for (int l = 0; l < template.States[k].Prototype.Mesh.Materials.Length; l++)
										{
											if (forceTextureRepeatX && forceTextureRepeatY)
											{
												template.States[k].Prototype.Mesh.Materials[l].WrapMode = OpenGlTextureWrapMode.RepeatRepeat;
											}
											else if (forceTextureRepeatX)
											{

												switch (template.States[k].Prototype.Mesh.Materials[l].WrapMode)
												{
													case OpenGlTextureWrapMode.ClampRepeat:
														template.States[k].Prototype.Mesh.Materials[l].WrapMode = OpenGlTextureWrapMode.RepeatRepeat;
														break;
													case OpenGlTextureWrapMode.ClampClamp:
														template.States[k].Prototype.Mesh.Materials[l].WrapMode = OpenGlTextureWrapMode.RepeatClamp;
														break;
												}
											}
											else if (forceTextureRepeatY)
											{

												switch (template.States[k].Prototype.Mesh.Materials[l].WrapMode)
												{
													case OpenGlTextureWrapMode.RepeatClamp:
														template.States[k].Prototype.Mesh.Materials[l].WrapMode = OpenGlTextureWrapMode.RepeatRepeat;
														break;
													case OpenGlTextureWrapMode.ClampClamp:
														template.States[k].Prototype.Mesh.Materials[l].WrapMode = OpenGlTextureWrapMode.ClampRepeat;
														break;
												}
											}
										}
									}

								}
								else
								{
									template.States[k].Prototype = null;
								}
							}

							for (int p = 0; p < objectPositions.Length; p++)
							{
								if (Result.Objects.Length == ObjectCount)
								{
									Array.Resize(ref Result.Objects, Result.Objects.Length << 1);
								}
								AnimatedObject clone = template.Clone();
								for (int k = 0; k < clone.States.Length; k++)
								{
									clone.States[k].Translation = Matrix4D.CreateTranslation(objectPositions[p].X, objectPositions[p].Y, -objectPositions[p].Z);
								}
								Result.Objects[ObjectCount] = clone;
								ObjectCount++;
							}
						}
						else
						{
							Result.Objects[ObjectCount].States = new ObjectState[] { };
							ObjectCount++;
						}
						break;
					case AnimatedSection.Sound:
						if (Block.GetPath(AnimatedKey.FileName, Folder, out string worldSoundPath))
						{
							if (!Block.GetAllVector3s(AnimatedKey.Position, ',', out Vector3[] soundPositions))
							{
								soundPositions = new[] { Vector3.Zero };
							}
							double worldSoundPitch = 1.0, worldSoundVolume = 1.0, worldSoundRadius = 30;
							Block.TryGetValue(AnimatedKey.Radius, ref worldSoundRadius, NumberRange.NonNegative);
							Block.GetFunctionScript(new[] { AnimatedKey.Pitch, AnimatedKey.PitchFunction }, Folder, out AnimationScript pitchFunction);
							Block.TryGetValue(AnimatedKey.Pitch, ref worldSoundPitch, NumberRange.NonNegative);
							Block.GetFunctionScript(new[] { AnimatedKey.VolumeFunction }, Folder, out AnimationScript volumeFunction);
							Block.TryGetValue(AnimatedKey.Volume, ref worldSoundVolume, NumberRange.NonNegative);
							Block.GetFunctionScript(new[] { AnimatedKey.TrackFollowerFunction }, Folder, out AnimationScript trackFollowerFunction);
							currentHost.RegisterSound(worldSoundPath, worldSoundRadius, out SoundHandle currentSound);
							
							for (int p = 0; p < soundPositions.Length; p++)
							{
								if (Result.Sounds.Length <= SoundCount)
								{
									Array.Resize(ref Result.Sounds, Result.Sounds.Length << 1);
								}
								WorldSound snd = new WorldSound(currentHost, currentSound)
								{
									currentPitch = worldSoundPitch,
									currentVolume = worldSoundVolume,
									Position = soundPositions[p],
									TrackFollowerFunction = trackFollowerFunction,
									PitchFunction = pitchFunction,
									VolumeFunction = volumeFunction
								};
								Result.Sounds[SoundCount] = snd;
								SoundCount++;
							}
						}
						break;
					case AnimatedSection.StateChangeSound:
						string[] stateChangeFileNames = new string[1]; // hack to allow both
						bool isSingleBuffer = Block.GetPath(AnimatedKey.FileName, Folder, out stateChangeFileNames[0]);
						if (isSingleBuffer || Block.GetPathArray(AnimatedKey.FileNames, ',', Folder, ref stateChangeFileNames))
						{
							if (!Block.GetAllVector3s(AnimatedKey.Position, ',', out Vector3[] stateChangePositions))
							{
								stateChangePositions = new[] { Vector3.Zero };
							}
							double stateChangePitch = 1.0, stateChangeVolume = 1.0, stateChangeRadius = 30;
							Block.TryGetValue(AnimatedKey.Pitch, ref stateChangePitch, NumberRange.NonNegative);
							Block.TryGetValue(AnimatedKey.Volume, ref stateChangeVolume, NumberRange.NonNegative);
							Block.TryGetValue(AnimatedKey.Radius, ref stateChangeRadius, NumberRange.NonNegative);
							bool playOnShow = true, playOnHide = true;
							Block.TryGetValue(AnimatedKey.PlayOnShow, ref playOnShow);
							Block.TryGetValue(AnimatedKey.PlayOnHide, ref playOnHide);

							if (ObjectCount > 0)
							{
								// NOTE: This attaches to the PREVIOUS object. 
								// If that object had multiple positions, we should probably attach to ALL of them?
								// But the current logic removes the object from Result.Objects and replaces it with a world sound object.
								
								// Wait, the original code only handled ONE object.
								// If there were multiple objects created by the previous section, what happens?
								// The original code only deceremented ObjectCount once.
								
								// For now, let's keep the logic consistent with how it was, but support multiple sound positions if specified.
								for (int p = 0; p < stateChangePositions.Length; p++)
								{
									if (Result.Sounds.Length <= SoundCount)
									{
										Array.Resize(ref Result.Sounds, Result.Sounds.Length << 1);
									}
									AnimatedWorldObjectStateSound snd = new AnimatedWorldObjectStateSound(currentHost)
									{
										Object = Result.Objects[ObjectCount - 1].Clone(),
										Buffers = new SoundHandle[stateChangeFileNames.Length]
									};
									for (int j = 0; j < stateChangeFileNames.Length; j++)
									{
										if (stateChangeFileNames[j] != null)
										{
											currentHost.RegisterSound(stateChangeFileNames[j], stateChangeRadius, out snd.Buffers[j]);
										}
									}

									snd.CurrentPitch = stateChangePitch;
									snd.CurrentVolume = stateChangeVolume;
									snd.SoundPosition = stateChangePositions[p];
									snd.PlayOnShow = playOnShow;
									snd.PlayOnHide = playOnHide;
									snd.SingleBuffer = isSingleBuffer;
									Result.Sounds[SoundCount] = snd;
									SoundCount++;
								}
								Result.Objects[ObjectCount - 1] = null;
								ObjectCount--;
							}
						}

						break;
				}
				Block.ReportErrors();
			}

			Array.Resize(ref Result.Objects, ObjectCount);
			return Result;
		}
	}
}
