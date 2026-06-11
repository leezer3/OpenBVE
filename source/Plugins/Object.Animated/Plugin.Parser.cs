using Formats.OpenBve;
using OpenBveApi.Colors;
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
						UnifiedObject[] obj = new UnifiedObject[4];
						int objCount = 0;
						Block.GetVector3(AnimatedKey.Position, ',', out Position);
						while (Block.RemainingDataValues > 0 && Block.GetNextPath(Folder, out string file))
						{
							if (obj.Length == objCount)
							{
								Array.Resize(ref obj, obj.Length << 1);
							}

							currentHost.LoadObject(file, Encoding, out obj[objCount]);
							objCount++;
						}
						for (int j = 0; j < objCount; j++)
						{
							if (obj[j] != null)
							{
								if (obj[j] is StaticObject s)
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
								else if (obj[j] is AnimatedObjectCollection)
								{
									AnimatedObjectCollection a = (AnimatedObjectCollection)obj[j].Clone();
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
								else if (obj[j] is KeyframeAnimatedObject k)
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

						break;
					case AnimatedSection.Object:
						if (Result.Objects.Length == ObjectCount)
						{
							Array.Resize(ref Result.Objects, Result.Objects.Length << 1);
						}
						Result.Objects[ObjectCount] = new AnimatedObject(currentHost, FileName)
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
							Block.GetVector3(AnimatedKey.Position, ',', out Position);
							Block.GetFunctionScript(new[] { AnimatedKey.RotateXFunction, AnimatedKey.RotateXFunctionRPN, AnimatedKey.RotateXScript }, Folder, out Result.Objects[ObjectCount].RotateXFunction);
							Block.GetFunctionScript(new[] { AnimatedKey.RotateYFunction, AnimatedKey.RotateYFunctionRPN, AnimatedKey.RotateYScript }, Folder, out Result.Objects[ObjectCount].RotateYFunction);
							Block.GetFunctionScript(new[] { AnimatedKey.RotateZFunction, AnimatedKey.RotateZFunctionRPN, AnimatedKey.RotateZScript }, Folder, out Result.Objects[ObjectCount].RotateZFunction);
							Block.GetFunctionScript(new[] { AnimatedKey.TranslateXFunction, AnimatedKey.TranslateXFunctionRPN, AnimatedKey.TranslateXScript }, Folder, out Result.Objects[ObjectCount].TranslateXFunction);
							Block.GetFunctionScript(new[] { AnimatedKey.TranslateYFunction, AnimatedKey.TranslateYFunctionRPN, AnimatedKey.TranslateYScript }, Folder, out Result.Objects[ObjectCount].TranslateYFunction);
							Block.GetFunctionScript(new[] { AnimatedKey.TranslateZFunction, AnimatedKey.TranslateZFunctionRPN, AnimatedKey.TranslateZScript }, Folder, out Result.Objects[ObjectCount].TranslateZFunction);
							Block.GetFunctionScript(new[] { AnimatedKey.StateFunction, AnimatedKey.StateFunctionRPN, AnimatedKey.StateScript }, Folder, out Result.Objects[ObjectCount].StateFunction);
							Block.GetFunctionScript(new[] { AnimatedKey.TextureShiftXFunction, AnimatedKey.TextureShiftXFunctionRPN, AnimatedKey.TextureShiftXScript }, Folder, out Result.Objects[ObjectCount].TextureShiftXFunction);
							Block.GetFunctionScript(new[] { AnimatedKey.TextureShiftYFunction, AnimatedKey.TextureShiftYFunctionRPN, AnimatedKey.TextureShiftYScript }, Folder, out Result.Objects[ObjectCount].TextureShiftYFunction);
							// n.b. For unknown reasons, the ScaleFunction never had a RPN listing in the animated file. Michelle listed these as obsolete, and I've seen *one* use of them. As they're not really supposed to be used
							// don't add them to the new parser
							Block.GetFunctionScript(new[] { AnimatedKey.ScaleXFunction, AnimatedKey.ScaleXScript }, Folder, out Result.Objects[ObjectCount].ScaleXFunction);
							Block.GetFunctionScript(new[] { AnimatedKey.ScaleYFunction, AnimatedKey.ScaleYScript }, Folder, out Result.Objects[ObjectCount].ScaleYFunction);
							Block.GetFunctionScript(new[] { AnimatedKey.ScaleZFunction, AnimatedKey.ScaleZScript }, Folder, out Result.Objects[ObjectCount].ScaleZFunction);
							Block.TryGetVector3(AnimatedKey.TranslateXDirection, ',', ref Result.Objects[ObjectCount].TranslateXDirection);
							Block.TryGetVector3(AnimatedKey.TranslateYDirection, ',', ref Result.Objects[ObjectCount].TranslateYDirection);
							Block.TryGetVector3(AnimatedKey.TranslateZDirection, ',', ref Result.Objects[ObjectCount].TranslateZDirection);
							Block.TryGetVector3(AnimatedKey.RotateXDirection, ',', ref Result.Objects[ObjectCount].RotateXDirection);
							Block.TryGetVector3(AnimatedKey.RotateYDirection, ',', ref Result.Objects[ObjectCount].RotateYDirection);
							Block.TryGetVector3(AnimatedKey.RotateZDirection, ',', ref Result.Objects[ObjectCount].RotateZDirection);
							Block.TryGetVector2(AnimatedKey.TextureShiftXDirection, ',', ref Result.Objects[ObjectCount].TextureShiftXDirection);
							Block.TryGetVector2(AnimatedKey.TextureShiftYDirection, ',', ref Result.Objects[ObjectCount].TextureShiftYDirection);
							if (Block.GetVector2(AnimatedKey.Axles, ',', out Vector2 axleLocations))
							{
								if (axleLocations.X <= axleLocations.Y)
								{
									currentHost.AddMessage(MessageType.Error, false, "Rear is expected to be less than Front in " + AnimatedKey.Axles + " in file " + FileName);
									axleLocations = new Vector2(0.5, -0.5);
								}
								Result.Objects[ObjectCount].FrontAxlePosition = axleLocations.X;
								Result.Objects[ObjectCount].RearAxlePosition = axleLocations.Y;
							}
							
							Block.GetFunctionScript(new[] { AnimatedKey.TrackFollowerFunction, AnimatedKey.TrackFollowerScript }, Folder, out Result.Objects[ObjectCount].TrackFollowerFunction);
							Block.GetDamping(AnimatedKey.RotateXDamping, ',', out Result.Objects[ObjectCount].RotateXDamping);
							Block.GetDamping(AnimatedKey.RotateYDamping, ',', out Result.Objects[ObjectCount].RotateYDamping);
							Block.GetDamping(AnimatedKey.RotateZDamping, ',', out Result.Objects[ObjectCount].RotateZDamping);

							if (Block.GetValue(AnimatedKey.TextureOverride, out string textureOverride))
							{
								switch (textureOverride.ToLowerInvariant())
								{
									case "none":
										break;
									case "timetable":
										currentHost.AddObjectForCustomTimeTable(Result.Objects[ObjectCount]);
										Result.Objects[ObjectCount].StateFunction = new FunctionScript(currentHost, "timetable", true);
										break;
									default:
										currentHost.AddMessage(MessageType.Error, false, "Unknown texture override type " + textureOverride + " in Section " + Block.Key + " in File " + FileName);
										break;
								}
							}

							if (Block.GetValue(AnimatedKey.RefreshRate, out double refreshRate, NumberRange.NonNegative))
							{
								Result.Objects[ObjectCount].RefreshRate = refreshRate;
							}

							Result.Objects[ObjectCount].States = new ObjectState[stateFiles.Length];
							bool forceTextureRepeatX = Result.Objects[ObjectCount].TextureShiftXFunction != null & Result.Objects[ObjectCount].TextureShiftXDirection.X != 0.0 |
							                           Result.Objects[ObjectCount].TextureShiftYFunction != null & Result.Objects[ObjectCount].TextureShiftYDirection.X != 0.0;
							bool forceTextureRepeatY = Result.Objects[ObjectCount].TextureShiftXFunction != null & Result.Objects[ObjectCount].TextureShiftXDirection.Y != 0.0 |
							                           Result.Objects[ObjectCount].TextureShiftYFunction != null & Result.Objects[ObjectCount].TextureShiftYDirection.Y != 0.0;
							for (int k = 0; k < stateFiles.Length; k++)
							{
								Result.Objects[ObjectCount].States[k] = new ObjectState();
								Result.Objects[ObjectCount].States[k].Translation = Matrix4D.CreateTranslation(Position.X, Position.Y, -Position.Z);
								if (stateFiles[k] != null)
								{
									currentHost.LoadObject(stateFiles[k], Encoding, out UnifiedObject currentObject);
									if (currentObject is StaticObject staticObject)
									{
										Result.Objects[ObjectCount].States[k].Prototype = staticObject;
									}
									else if (currentObject is KeyframeAnimatedObject keyframeObject)
									{
										currentHost.AddMessage(MessageType.Warning, false, "Using MSTS Shape " + stateFiles[k] + " as an AnimatedObject State- Contained animations may be lost. In the Section " + Block.Key + " in file " + FileName);
										Result.Objects[ObjectCount].States[k].Prototype = keyframeObject;
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
									if (Result.Objects[ObjectCount].States[k].Prototype != null)
									{
										Result.Objects[ObjectCount].States[k].Prototype.Dynamic = true;
										for (int l = 0; l < Result.Objects[ObjectCount].States[k].Prototype.Mesh.Materials.Length; l++)
										{
											if (forceTextureRepeatX && forceTextureRepeatY)
											{
												Result.Objects[ObjectCount].States[k].Prototype.Mesh.Materials[l].WrapMode = OpenGlTextureWrapMode.RepeatRepeat;
											}
											else if (forceTextureRepeatX)
											{

												switch (Result.Objects[ObjectCount].States[k].Prototype.Mesh.Materials[l].WrapMode)
												{
													case OpenGlTextureWrapMode.ClampRepeat:
														Result.Objects[ObjectCount].States[k].Prototype.Mesh.Materials[l].WrapMode = OpenGlTextureWrapMode.RepeatRepeat;
														break;
													case OpenGlTextureWrapMode.ClampClamp:
														Result.Objects[ObjectCount].States[k].Prototype.Mesh.Materials[l].WrapMode = OpenGlTextureWrapMode.RepeatClamp;
														break;
												}
											}
											else if (forceTextureRepeatY)
											{

												switch (Result.Objects[ObjectCount].States[k].Prototype.Mesh.Materials[l].WrapMode)
												{
													case OpenGlTextureWrapMode.RepeatClamp:
														Result.Objects[ObjectCount].States[k].Prototype.Mesh.Materials[l].WrapMode = OpenGlTextureWrapMode.RepeatRepeat;
														break;
													case OpenGlTextureWrapMode.ClampClamp:
														Result.Objects[ObjectCount].States[k].Prototype.Mesh.Materials[l].WrapMode = OpenGlTextureWrapMode.ClampRepeat;
														break;
												}
											}
										}
									}

								}
								else
								{
									Result.Objects[ObjectCount].States[k].Prototype = null;
								}
							}
						}
						else
						{
							Result.Objects[ObjectCount].States = new ObjectState[] { };
						}

						ObjectCount++;
						break;
					case AnimatedSection.Sound:
						if (Result.Sounds.Length >= SoundCount)
						{
							Array.Resize(ref Result.Sounds, Result.Sounds.Length << 1);
						}
						if (Block.GetPath(AnimatedKey.FileName, Folder, out string soundPath))
						{
							double pitch = 1.0, volume = 1.0, radius = 30;
							Block.GetVector3(AnimatedKey.Position, ',', out Position);
							Block.TryGetValue(AnimatedKey.Radius, ref radius, NumberRange.NonNegative);
							Block.GetFunctionScript(new[] { AnimatedKey.Pitch, AnimatedKey.PitchFunction }, Folder, out AnimationScript pitchFunction);
							Block.TryGetValue(AnimatedKey.Pitch, ref pitch, NumberRange.NonNegative);
							Block.GetFunctionScript(new[] { AnimatedKey.VolumeFunction }, Folder, out AnimationScript volumeFunction);
							Block.TryGetValue(AnimatedKey.Volume, ref volume, NumberRange.NonNegative);
							Block.GetFunctionScript(new[] { AnimatedKey.TrackFollowerFunction }, Folder, out AnimationScript trackFollowerFunction);
							currentHost.RegisterSound(soundPath, radius, out SoundHandle currentSound);
							WorldSound snd = new WorldSound(currentHost, currentSound)
							{
								currentPitch = pitch,
								currentVolume = volume,
								Position = Position,
								TrackFollowerFunction = trackFollowerFunction,
								PitchFunction = pitchFunction,
								VolumeFunction = volumeFunction
							};
							Result.Sounds[SoundCount] = snd;
							SoundCount++;
						}
						break;
					case AnimatedSection.StateChangeSound:
						if (Result.Sounds.Length == SoundCount)
						{
							Array.Resize(ref Result.Objects, Result.Sounds.Length << 1);
						}
						string[] fileNames = new string[1]; // hack to allow both
						bool singleBuffer = Block.GetPath(AnimatedKey.FileName, Folder, out fileNames[0]);
						if (singleBuffer || Block.GetPathArray(AnimatedKey.FileNames, ',', Folder, ref fileNames))
						{
							double pitch = 1.0, volume = 1.0, radius = 30;
							Block.GetVector3(AnimatedKey.Position, ',', out Position);
							Block.TryGetValue(AnimatedKey.Pitch, ref pitch, NumberRange.NonNegative);
							Block.TryGetValue(AnimatedKey.Volume, ref volume, NumberRange.NonNegative);
							Block.TryGetValue(AnimatedKey.Radius, ref radius, NumberRange.NonNegative);
							bool playOnShow = true, playOnHide = true;
							Block.TryGetValue(AnimatedKey.PlayOnShow, ref playOnShow);
							Block.TryGetValue(AnimatedKey.PlayOnHide, ref playOnHide);

							if (ObjectCount > 0)
							{
								AnimatedWorldObjectStateSound snd = new AnimatedWorldObjectStateSound(currentHost)
								{
									Object = Result.Objects[ObjectCount - 1].Clone(),
									Buffers = new SoundHandle[fileNames.Length]
								};
								for (int j = 0; j < fileNames.Length; j++)
								{
									if (fileNames[j] != null)
									{
										currentHost.RegisterSound(fileNames[j], radius, out snd.Buffers[j]);
									}
								}

								snd.CurrentPitch = pitch;
								snd.CurrentVolume = volume;
								snd.SoundPosition = Position;
								snd.SingleBuffer = fileNames.Length != 1;
								snd.PlayOnShow = playOnShow;
								snd.PlayOnHide = playOnHide;
								snd.SingleBuffer = singleBuffer;
								Result.Sounds[SoundCount] = snd;
								SoundCount++;
								Result.Objects[ObjectCount - 1] = null;
								ObjectCount--;
							}
						}

						break;
					case AnimatedSection.Light:
						{
							if (Result.Objects.Length == ObjectCount)
							{
								Array.Resize(ref Result.Objects, Result.Objects.Length << 1);
							}
							AnimatedObject animatedObj = new AnimatedObject(currentHost, FileName)
							{
								CurrentState = 0,
								States = new ObjectState[] { new ObjectState() },
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
							animatedObj.States[0].Prototype = new StaticObject(currentHost);
							animatedObj.States[0].Prototype.Dynamic = true;
							animatedObj.States[0].Translation = Matrix4D.Identity;

							SceneLight light = new SceneLight();

							if (Block.GetEnumValue(AnimatedKey.Type, out SceneLightType type))
							{
								light.Type = type;
							}

							Block.TryGetVector3(AnimatedKey.Position, ',', ref light.Position);
							Block.TryGetVector3(AnimatedKey.Direction, ',', ref light.Direction);
							light.Direction.Normalize();

							Color32 color32 = Color32.White;
							if (Block.TryGetColor32(AnimatedKey.Color, ref color32))
							{
								light.Color = new Color128(color32.R / 255.0f, color32.G / 255.0f, color32.B / 255.0f, color32.A / 255.0f);
							}

							double range = 10.0;
							if (Block.TryGetValue(AnimatedKey.Range, ref range, NumberRange.Positive))
							{
								light.Range = (float)range;
								light.RangeSquared = light.Range * light.Range;
							}

							bool visual = false;
							if (Block.TryGetValue(AnimatedKey.Visual, ref visual))
							{
								light.Visual = visual;
								light.ShowCone = visual;
							}

							double power = 12.5663706;
							if (Block.TryGetValue(AnimatedKey.Power, ref power, NumberRange.Positive))
							{
								light.Power = (float)power;
							}

							double exposure = 0.0;
							if (Block.TryGetValue(AnimatedKey.Exposure, ref exposure))
							{
								light.Exposure = (float)exposure;
							}

							if (Block.GetValue(AnimatedKey.Normalize, out string normStr))
							{
								string s = normStr.ToLowerInvariant().Trim();
								light.NormalizeCone = (s == "1" || s == "true");
							}

							double radius = 0.0;
							if (Block.TryGetValue(AnimatedKey.Radius, ref radius, NumberRange.NonNegative))
							{
								light.Radius = (float)radius;
							}

							if (Block.GetValue(AnimatedKey.SoftFalloff, out string sfStr))
							{
								string s = sfStr.ToLowerInvariant().Trim();
								light.SoftFalloff = (s == "1" || s == "true");
							}

							double angle = 45.0;
							if (Block.TryGetValue(AnimatedKey.Angle, ref angle, NumberRange.Positive))
							{
								light.Angle = (float)angle;
								light.SpotCutoff = (float)System.Math.Cos((angle / 2.0) * System.Math.PI / 180.0);
							}

							double softness = 1.0;
							if (Block.TryGetValue(AnimatedKey.Softness, ref softness, NumberRange.NonNegative))
							{
								light.Softness = (float)softness;
							}

							if (Block.GetValue(AnimatedKey.ShowCone, out string scStr))
							{
								string s = scStr.ToLowerInvariant().Trim();
								light.ShowCone = (s == "1" || s == "true");
								light.Visual = light.ShowCone;
							}

							Block.TryGetVector2(AnimatedKey.Size, ',', ref light.AreaSize);

							animatedObj.Lights.Add(light);
							animatedObj.Light = light;

							// Parse animation functions for the light object itself
							Block.GetFunctionScript(new[] { AnimatedKey.RotateXFunction, AnimatedKey.RotateXFunctionRPN, AnimatedKey.RotateXScript }, Folder, out animatedObj.RotateXFunction);
							Block.GetFunctionScript(new[] { AnimatedKey.RotateYFunction, AnimatedKey.RotateYFunctionRPN, AnimatedKey.RotateYScript }, Folder, out animatedObj.RotateYFunction);
							Block.GetFunctionScript(new[] { AnimatedKey.RotateZFunction, AnimatedKey.RotateZFunctionRPN, AnimatedKey.RotateZScript }, Folder, out animatedObj.RotateZFunction);
							Block.GetFunctionScript(new[] { AnimatedKey.TranslateXFunction, AnimatedKey.TranslateXFunctionRPN, AnimatedKey.TranslateXScript }, Folder, out animatedObj.TranslateXFunction);
							Block.GetFunctionScript(new[] { AnimatedKey.TranslateYFunction, AnimatedKey.TranslateYFunctionRPN, AnimatedKey.TranslateYScript }, Folder, out animatedObj.TranslateYFunction);
							Block.GetFunctionScript(new[] { AnimatedKey.TranslateZFunction, AnimatedKey.TranslateZFunctionRPN, AnimatedKey.TranslateZScript }, Folder, out animatedObj.TranslateZFunction);
							Block.TryGetVector3(AnimatedKey.TranslateXDirection, ',', ref animatedObj.TranslateXDirection);
							Block.TryGetVector3(AnimatedKey.TranslateYDirection, ',', ref animatedObj.TranslateYDirection);
							Block.TryGetVector3(AnimatedKey.TranslateZDirection, ',', ref animatedObj.TranslateZDirection);
							Block.TryGetVector3(AnimatedKey.RotateXDirection, ',', ref animatedObj.RotateXDirection);
							Block.TryGetVector3(AnimatedKey.RotateYDirection, ',', ref animatedObj.RotateYDirection);
							Block.TryGetVector3(AnimatedKey.RotateZDirection, ',', ref animatedObj.RotateZDirection);
							Block.GetDamping(AnimatedKey.RotateXDamping, ',', out animatedObj.RotateXDamping);
							Block.GetDamping(AnimatedKey.RotateYDamping, ',', out animatedObj.RotateYDamping);
							Block.GetDamping(AnimatedKey.RotateZDamping, ',', out animatedObj.RotateZDamping);

							Result.Objects[ObjectCount] = animatedObj;
							ObjectCount++;
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
