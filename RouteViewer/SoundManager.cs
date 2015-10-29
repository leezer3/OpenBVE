using System;
using Tao.OpenAl;

namespace OpenBve {
	internal static class SoundManager {

		// general
		internal struct OpenAlIndex {
			internal int Index;
			internal bool Valid;
			internal OpenAlIndex(int Index, bool Valid) {
				this.Index = Index;
				this.Valid = Valid;
			}
		}

		// sound buffers
		private class SoundBuffer {
			internal string FileName;
			internal double Duration;
			internal OpenAlIndex OpenAlBufferIndex;
			internal bool TriedLoading;
			internal float Radius;
		}
		private static SoundBuffer[] SoundBuffers = new SoundBuffer[16];

		// sound sources
		internal class SoundSource {
			internal World.Vector3D Position;
			internal float[] OpenAlPosition;
			internal float[] OpenAlVelocity;
			internal OpenAlIndex OpenAlSourceIndex;
			internal int SoundBufferIndex;
			internal double Radius;
			internal float Pitch;
			internal float Gain;
			internal bool Looped;
			internal bool Suppressed;
			internal bool FinishedPlaying;
			internal bool HasHandle;
			internal TrainManager.Train Train;
			internal int CarIndex;
		}
		internal static SoundSource[] SoundSources = new SoundSource[16];

		// listener
		private static float[] ListenerPosition = new float[] { 0.0f, 0.0f, 0.0f };
		private static float[] ListenerVelocity = new float[] { 0.0f, 0.0f, 0.0f };
		private static float[] ListenerOrientation = new float[] { 0.0f, 0.0f, 1.0f, 0.0f, 1.0f, 0.0f };
		private static double InternalTimer = 0.0;

		// openal
		private static IntPtr OpenAlDevice = IntPtr.Zero;
		private static IntPtr OpenAlContext = IntPtr.Zero;
		
		// misc
		internal static double OuterRadiusFactor = 8.0;
		private static double OuterRadiusFactorMinimum = 2.0;
		private static double OuterRadiusFactorMaximum = 8.0;
		private static double OuterRadiusSpeed = 0.0;
		private const double OuterRadiusAcceleration = 0.5;
		private const double OuterRadiusDeceleration = 1.0;
		private static int SoundsQueriedPlaying = 0;
		private static int SoundsActuallyPlaying = 0;
		
		// options
		internal static bool Mute = false;
		
		// initialize
		internal static void Initialize() {
			// openal
			OpenAlDevice = Alc.alcOpenDevice(null);
			if (OpenAlDevice != IntPtr.Zero) {
				OpenAlContext = Alc.alcCreateContext(OpenAlDevice, IntPtr.Zero);
				if (OpenAlContext != IntPtr.Zero) {
					Alc.alcMakeContextCurrent(OpenAlContext);
					Al.alSpeedOfSound(343.0f);
					Al.alDistanceModel(Al.AL_NONE);
				} else {
					Alc.alcCloseDevice(OpenAlDevice);
					OpenAlDevice = IntPtr.Zero;
					System.Windows.Forms.MessageBox.Show("The sound device could be opened, but the sound context could not be created.", "openBVE", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Hand);
				}
			} else {
				OpenAlContext = IntPtr.Zero;
				System.Windows.Forms.MessageBox.Show("The sound device could not be opened.", "openBVE", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Hand);
			}
			// outer radius
			switch (Interface.CurrentOptions.SoundRange) {
				case Interface.SoundRange.Low:
					OuterRadiusFactorMinimum = 2.0;
					OuterRadiusFactorMaximum = 8.0;
					break;
				case Interface.SoundRange.Medium:
					OuterRadiusFactorMinimum = 4.0;
					OuterRadiusFactorMaximum = 16.0;
					break;
				case Interface.SoundRange.High:
					OuterRadiusFactorMinimum = 8.0;
					OuterRadiusFactorMaximum = 32.0;
					break;
			}
			OuterRadiusFactor = OuterRadiusFactorMaximum;
		}

		// deinitialize
		internal static void Deinitialize() {
			if (OpenAlContext != IntPtr.Zero) {
				SoundManager.StopAllSounds(true);
				SoundManager.UnuseAllSoundsBuffers();
				Alc.alcMakeContextCurrent(IntPtr.Zero);
				Alc.alcDestroyContext(OpenAlContext);
				OpenAlContext = IntPtr.Zero;
			}
			if (OpenAlDevice != IntPtr.Zero) {
				Alc.alcCloseDevice(OpenAlDevice);
				OpenAlDevice = IntPtr.Zero;
			}
		}

		// update
		internal static void Update(double TimeElapsed) {
			if (OpenAlContext != IntPtr.Zero) {
				// listener
				double vx = World.CameraTrackFollower.WorldDirection.X * World.CameraSpeed;
				double vy = World.CameraTrackFollower.WorldDirection.Y * World.CameraSpeed;
				double vz = World.CameraTrackFollower.WorldDirection.Z * World.CameraSpeed;
				if (World.CameraMode == World.CameraViewMode.Interior | World.CameraMode == World.CameraViewMode.InteriorLookAhead) {
					ListenerVelocity[0] = 0.0f;
					ListenerVelocity[1] = 0.0f;
					ListenerVelocity[2] = 0.0f;
				} else {
					ListenerVelocity[0] = (float)vx;
					ListenerVelocity[1] = (float)vy;
					ListenerVelocity[2] = (float)vz;
				}
				ListenerOrientation[0] = (float)World.AbsoluteCameraDirection.X;
				ListenerOrientation[1] = (float)World.AbsoluteCameraDirection.Y;
				ListenerOrientation[2] = (float)World.AbsoluteCameraDirection.Z;
				ListenerOrientation[3] = (float)-World.AbsoluteCameraUp.X;
				ListenerOrientation[4] = (float)-World.AbsoluteCameraUp.Y;
				ListenerOrientation[5] = (float)-World.AbsoluteCameraUp.Z;
				Al.alListenerfv(Al.AL_POSITION, ListenerPosition);
				Al.alListenerfv(Al.AL_VELOCITY, ListenerVelocity);
				Al.alListenerfv(Al.AL_ORIENTATION, ListenerOrientation);
				double cx = World.AbsoluteCameraPosition.X;
				double cy = World.AbsoluteCameraPosition.Y;
				double cz = World.AbsoluteCameraPosition.Z;
				if (Mute) {
					// mute
					for (int i = 0; i < SoundSources.Length; i++) {
						if (SoundSources[i] != null && !SoundSources[i].FinishedPlaying) {
							if (!SoundSources[i].Suppressed) {
								if (SoundSources[i].Looped) {
									if (SoundSources[i].OpenAlSourceIndex.Valid) {
										int j = SoundSources[i].OpenAlSourceIndex.Index;
										Al.alSourceStop(j);
										Al.alDeleteSources(1, ref j);
									}
									SoundSources[i].OpenAlSourceIndex = new OpenAlIndex(0, false);
									SoundSources[i].Suppressed = true;
								} else {
									StopSound(i, false);
								}
							} else if (!SoundSources[i].Looped) {
								StopSound(i, false);
							}
						}
					}
				} else {
					// outer radius
					int n = Interface.CurrentOptions.SoundNumber - 3;
					if (SoundsActuallyPlaying >= n) {
						OuterRadiusSpeed -= OuterRadiusDeceleration * TimeElapsed;
						if (OuterRadiusSpeed < -1.0) OuterRadiusSpeed = -1.0;
					} else if (SoundsQueriedPlaying < n) {
						OuterRadiusSpeed += OuterRadiusAcceleration * TimeElapsed;
						if (OuterRadiusSpeed > 1.0) OuterRadiusSpeed = 1.0;
					} else {
						OuterRadiusSpeed -= (double)Math.Sign(OuterRadiusSpeed) * OuterRadiusDeceleration * TimeElapsed;
						if (OuterRadiusSpeed * OuterRadiusSpeed <= TimeElapsed * TimeElapsed) {
							OuterRadiusSpeed = 0.0;
						}
					}
					OuterRadiusFactor += OuterRadiusSpeed * TimeElapsed;
					if (OuterRadiusFactor < OuterRadiusFactorMinimum) {
						OuterRadiusFactor = OuterRadiusFactorMinimum;
					} else if (OuterRadiusFactor > OuterRadiusFactorMaximum) {
						OuterRadiusFactor = OuterRadiusFactorMaximum;
					}
					// sources
					SoundsQueriedPlaying = 0;
					SoundsActuallyPlaying = 0;
					for (int i = 0; i < SoundSources.Length; i++) {
						if (SoundSources[i] != null && !SoundSources[i].FinishedPlaying) {
							double rx = SoundSources[i].Position.X;
							double ry = SoundSources[i].Position.Y;
							double rz = SoundSources[i].Position.Z;
							double px, py, pz;
							if (SoundSources[i].Train != null) {
								int c = SoundSources[i].CarIndex;
								double tx, ty, tz;
								TrainManager.CreateWorldCoordinates(SoundSources[i].Train, c, rx, ry, rz, out px, out py, out pz, out tx, out ty, out tz);
								px -= cx; py -= cy; pz -= cz;
								double sp = SoundSources[i].Train.Specs.CurrentAverageSpeed;
								if (World.CameraMode != World.CameraViewMode.Interior & World.CameraMode != World.CameraViewMode.InteriorLookAhead) {
									SoundSources[i].OpenAlVelocity[0] = (float)(tx * sp);
									SoundSources[i].OpenAlVelocity[1] = (float)(ty * sp);
									SoundSources[i].OpenAlVelocity[2] = (float)(tz * sp);
								} else {
									SoundSources[i].OpenAlVelocity[0] = (float)(tx * sp - vx);
									SoundSources[i].OpenAlVelocity[1] = (float)(ty * sp - vy);
									SoundSources[i].OpenAlVelocity[2] = (float)(tz * sp - vz);
								}
							} else {
								px = rx - cx; py = ry - cy; pz = rz - cz;
								if (World.CameraMode != World.CameraViewMode.Interior & World.CameraMode != World.CameraViewMode.InteriorLookAhead) {
									SoundSources[i].OpenAlVelocity[0] = 0.0f;
									SoundSources[i].OpenAlVelocity[1] = 0.0f;
									SoundSources[i].OpenAlVelocity[2] = 0.0f;
								} else {
									SoundSources[i].OpenAlVelocity[0] = (float)-vx;
									SoundSources[i].OpenAlVelocity[1] = (float)-vy;
									SoundSources[i].OpenAlVelocity[2] = (float)-vz;
								}
							}
							// play the sound only if within the outer radius
							double distanceSquared = px * px + py * py + pz * pz;
							double distance = Math.Sqrt(distanceSquared);
							double innerRadius = SoundSources[i].Radius;
							double outerRadius = OuterRadiusFactor * innerRadius;
							double outerRadiusSquared = outerRadius * outerRadius;
							if (distanceSquared < outerRadiusSquared) {
								// sound is in range
								double gain;
								double innerRadiusSquared = innerRadius * innerRadius;
								const double rollOffFactor = 0.9;
								if (distanceSquared < innerRadiusSquared) {
									gain = 1.0 - (1.0 - rollOffFactor) * distanceSquared / innerRadiusSquared;
								} else {
									double value = distance / outerRadius;
									gain = innerRadius * rollOffFactor * (1.0 - value * value * value) / distance;
								}
								SoundsQueriedPlaying++;
								SoundsActuallyPlaying++;
								bool startPlaying = false;
								// play sound if currently suppressed
								if (SoundSources[i].Suppressed) {
									if (SoundSources[i].SoundBufferIndex >= 0) {
										UseSoundBuffer(SoundSources[i].SoundBufferIndex);
										if (SoundBuffers[SoundSources[i].SoundBufferIndex].OpenAlBufferIndex.Valid) {
											int j;
											Al.alGetError();
											Al.alGenSources(1, out j);
											int err = Al.alGetError();
											if (err == Al.AL_NO_ERROR) {
												SoundSources[i].OpenAlSourceIndex = new OpenAlIndex(j, true);
												Al.alSourcei(j, Al.AL_BUFFER, SoundBuffers[SoundSources[i].SoundBufferIndex].OpenAlBufferIndex.Index);
												SoundSources[i].Suppressed = false;
												startPlaying = true;
											} else {
												continue;
											}
										} else {
											StopSound(i, false);
											continue;
										}
									} else {
										StopSound(i, false);
										continue;
									}
								}
								// play or stop sound
								if (startPlaying || IsPlaying(i)) {
									SoundSources[i].OpenAlPosition[0] = (float)px;
									SoundSources[i].OpenAlPosition[1] = (float)py;
									SoundSources[i].OpenAlPosition[2] = (float)pz;
									if (!SoundSources[i].OpenAlSourceIndex.Valid) {
										throw new InvalidOperationException("A bug in the sound manager. (9431)");
									}
									int j = SoundSources[i].OpenAlSourceIndex.Index;
									Al.alSourcefv(j, Al.AL_POSITION, SoundSources[i].OpenAlPosition);
									Al.alSourcefv(j, Al.AL_VELOCITY, SoundSources[i].OpenAlVelocity);
									Al.alSourcef(j, Al.AL_PITCH, SoundSources[i].Pitch);
									float g = SoundSources[i].Gain * SoundSources[i].Gain * (float)gain;
									if (g > 1.0f) g = 1.0f;
									Al.alSourcef(j, Al.AL_GAIN, g);
								} else {
									StopSound(i, false);
									continue;
								}
								// update position and velocity of sound
								if (startPlaying) {
									if (!SoundSources[i].OpenAlSourceIndex.Valid) {
										throw new InvalidOperationException("A bug in the sound manager. (7625)");
									}
									int j = SoundSources[i].OpenAlSourceIndex.Index;
									Al.alSourcei(j, Al.AL_LOOPING, SoundSources[i].Looped ? Al.AL_TRUE : Al.AL_FALSE);
									Al.alSourcef(j, Al.AL_REFERENCE_DISTANCE, SoundBuffers[SoundSources[i].SoundBufferIndex].Radius);
									Al.alSourcePlay(j);
								}
							} else {
								// sound is not in range
								if (!SoundSources[i].Suppressed) {
									if (SoundSources[i].Looped) {
										if (SoundSources[i].OpenAlSourceIndex.Valid) {
											int j = SoundSources[i].OpenAlSourceIndex.Index;
											Al.alSourceStop(j);
											Al.alDeleteSources(1, ref j);
										}
										SoundSources[i].OpenAlSourceIndex = new OpenAlIndex(0, false);
										SoundSources[i].Suppressed = true;
									} else {
										StopSound(i, false);
									}
								} else if (!SoundSources[i].Looped) {
									StopSound(i, false);
								}
							}
						}
					}
				}
				// infrequent updates
				InternalTimer += TimeElapsed;
				if (InternalTimer > 1.0) {
					InternalTimer = 0.0;
					double Elevation = World.AbsoluteCameraPosition.Y + Game.RouteInitialElevation;
					double AirTemperature = Game.GetAirTemperature(Elevation);
					double AirPressure = Game.GetAirPressure(Elevation, AirTemperature);
					double SpeedOfSound = Game.GetSpeedOfSound(AirPressure, AirTemperature);
					Al.alSpeedOfSound((float)SpeedOfSound);
				}
			}
		}

		// use sound buffer
		private static void UseSoundBuffer(int SoundBufferIndex) {
			if (OpenAlContext != IntPtr.Zero) {
				if (SoundBufferIndex >= 0) {
					if (!SoundBuffers[SoundBufferIndex].TriedLoading) {
						SoundBuffers[SoundBufferIndex].TriedLoading = true;
						if (!SoundBuffers[SoundBufferIndex].OpenAlBufferIndex.Valid) {
							try{
								WaveParser.WaveData data = WaveParser.LoadFromFile(SoundBuffers[SoundBufferIndex].FileName);
								data = WaveParser.ConvertToMono8Or16(data);
								if (data.Format.BitsPerSample == 8) {
									int buffer;
									Al.alGenBuffers(1, out buffer);
									Al.alBufferData(buffer, Al.AL_FORMAT_MONO8, data.Bytes, data.Bytes.Length, data.Format.SampleRate);
									SoundBuffers[SoundBufferIndex].OpenAlBufferIndex = new OpenAlIndex(buffer, true);
									SoundBuffers[SoundBufferIndex].Duration = (double)data.Bytes.Length / (double)(data.Format.SampleRate);
								} else if (data.Format.BitsPerSample == 16) {
									int buffer;
									Al.alGenBuffers(1, out buffer);
									Al.alBufferData(buffer, Al.AL_FORMAT_MONO16, data.Bytes, data.Bytes.Length, data.Format.SampleRate);
									SoundBuffers[SoundBufferIndex].OpenAlBufferIndex = new OpenAlIndex(buffer, true);
									SoundBuffers[SoundBufferIndex].Duration = (double)data.Bytes.Length / (double)(2 * data.Format.SampleRate);
								} else {
									SoundBuffers[SoundBufferIndex].OpenAlBufferIndex = new OpenAlIndex(0, false);
								}
							} catch {
								SoundBuffers[SoundBufferIndex].OpenAlBufferIndex = new OpenAlIndex(0, false);
							}
						}
					}
				}
			}
		}

		// unuse sound buffer
		private static void UnuseSoundBuffer(int SoundBufferIndex) {
			if (OpenAlContext != IntPtr.Zero) {
				if (SoundBuffers[SoundBufferIndex].OpenAlBufferIndex.Valid) {
					Al.alDeleteBuffers(1, ref SoundBuffers[SoundBufferIndex].OpenAlBufferIndex.Index);
					SoundBuffers[SoundBufferIndex].OpenAlBufferIndex = new OpenAlIndex(0, false);
				}
			}
		}
		private static void UnuseAllSoundsBuffers() {
			if (OpenAlContext != IntPtr.Zero) {
				for (int i = 0; i < SoundBuffers.Length; i++) {
					if (SoundBuffers[i] != null) {
						UnuseSoundBuffer(i);
					}
				}
			}
		}

		// load sound
		internal static int LoadSound(string FileName, double Radius) {
			if (OpenAlContext != IntPtr.Zero) {
				int i;
				for (i = 0; i < SoundBuffers.Length; i++) {
					if (SoundBuffers[i] != null && string.Compare(SoundBuffers[i].FileName, FileName, StringComparison.OrdinalIgnoreCase) == 0 & SoundBuffers[i].Radius == Radius) {
						return i;
					}
				}
				if (!FileName.EndsWith(".wav", StringComparison.OrdinalIgnoreCase)) {
					Interface.AddMessage(Interface.MessageType.Warning, false, "The file extension is not recognized - will be assumed to be a .wav file: " + FileName);
				}
				for (i = 0; i < SoundBuffers.Length; i++) {
					if (SoundBuffers[i] == null) break;
				}
				if (i == SoundBuffers.Length) {
					Array.Resize<SoundBuffer>(ref SoundBuffers, SoundBuffers.Length << 1);
				}
				SoundBuffers[i] = new SoundBuffer();
				SoundBuffers[i].FileName = FileName;
				SoundBuffers[i].OpenAlBufferIndex = new OpenAlIndex(0, false);
				SoundBuffers[i].Radius = (float)Radius;
				return i;
			} else {
				return -1;
			}
		}

		// get sound length
		internal static double GetSoundLength(int SoundBufferIndex) {
			if (SoundBuffers[SoundBufferIndex].Duration != 0.0) {
				return SoundBuffers[SoundBufferIndex].Duration;
			} else if (OpenAlContext != IntPtr.Zero) {
				UseSoundBuffer(SoundBufferIndex);
				return SoundBuffers[SoundBufferIndex].Duration;
			} else {
				return 1.0;
			}
		}

		// play sound
		internal enum Importance { DontCare, AlwaysPlay }
		internal static void PlaySound(ref int SoundSourceIndex, int SoundBufferIndex, World.Vector3D Position, Importance Important, bool Looped) {
			PlaySound(ref SoundSourceIndex, true, SoundBufferIndex, null, -1, Position, Important, Looped, 1.0, 1.0);
		}
		internal static void PlaySound(int SoundBufferIndex, World.Vector3D Position, Importance Important, bool Looped) {
			int a = -1;
			PlaySound(ref a, false, SoundBufferIndex, null, -1, Position, Important, Looped, 1.0, 1.0);
		}
		internal static void PlaySound(int SoundBufferIndex, TrainManager.Train Train, int CarIndex, World.Vector3D Position, Importance Important, bool Looped) {
			int a = -1;
			PlaySound(ref a, false, SoundBufferIndex, Train, CarIndex, Position, Important, Looped, 1.0, 1.0);
		}
		internal static void PlaySound(ref int SoundSourceIndex, int SoundBufferIndex, TrainManager.Train Train, int CarIndex, World.Vector3D Position, Importance Important, bool Looped) {
			PlaySound(ref SoundSourceIndex, true, SoundBufferIndex, Train, CarIndex, Position, Important, Looped, 1.0, 1.0);
		}
		internal static void PlaySound(int SoundBufferIndex, TrainManager.Train Train, int CarIndex, World.Vector3D Position, Importance Important, bool Looped, double Pitch, double Gain) {
			int a = -1;
			PlaySound(ref a, false, SoundBufferIndex, Train, CarIndex, Position, Important, Looped, Pitch, Gain);
		}
		internal static void PlaySound(ref int SoundSourceIndex, int SoundBufferIndex, TrainManager.Train Train, int CarIndex, World.Vector3D Position, Importance Important, bool Looped, double Pitch, double Gain) {
			PlaySound(ref SoundSourceIndex, true, SoundBufferIndex, Train, CarIndex, Position, Important, Looped, Pitch, Gain);
		}
		private static void PlaySound(ref int SoundSourceIndex, bool ReturnHandle, int SoundBufferIndex, TrainManager.Train Train, int CarIndex, World.Vector3D Position, Importance Important, bool Looped, double Pitch, double Gain) {
			if (OpenAlContext != IntPtr.Zero) {
				if (Game.MinimalisticSimulation & Important == Importance.DontCare | SoundBufferIndex == -1) {
					return;
				}
				if (SoundSourceIndex >= 0) {
					StopSound(ref SoundSourceIndex);
				}
				int i;
				for (i = 0; i < SoundSources.Length; i++) {
					if (SoundSources[i] == null) break;
				}
				if (i >= SoundSources.Length) {
					Array.Resize<SoundSource>(ref SoundSources, SoundSources.Length << 1);
				}
				SoundSources[i] = new SoundSource();
				SoundSources[i].Position = Position;
				SoundSources[i].OpenAlPosition = new float[] { 0.0f, 0.0f, 0.0f };
				SoundSources[i].OpenAlVelocity = new float[] { 0.0f, 0.0f, 0.0f };
				SoundSources[i].SoundBufferIndex = SoundBufferIndex;
				SoundSources[i].Radius = SoundBuffers[SoundBufferIndex].Radius;
				SoundSources[i].Pitch = (float)Pitch;
				SoundSources[i].Gain = (float)Gain;
				SoundSources[i].Looped = Looped;
				SoundSources[i].Suppressed = true;
				SoundSources[i].FinishedPlaying = false;
				SoundSources[i].Train = Train;
				SoundSources[i].CarIndex = CarIndex;
				SoundSources[i].OpenAlSourceIndex = new OpenAlIndex(0, false);
				SoundSources[i].HasHandle = ReturnHandle;
				SoundSourceIndex = i;
			}
		}

		// modulate sound
		internal static void ModulateSound(int SoundSourceIndex, double Pitch, double Gain) {
			if (OpenAlContext != IntPtr.Zero) {
				if (SoundSourceIndex >= 0 && SoundSources[SoundSourceIndex] != null) {
					SoundSources[SoundSourceIndex].Pitch = (float)Pitch;
					SoundSources[SoundSourceIndex].Gain = (float)Gain;
				}
			}
		}

		// stop sound
		private static void StopSound(int SoundSourceIndex, bool InvalidateHandle) {
			if (OpenAlContext != IntPtr.Zero) {
				if (SoundSources[SoundSourceIndex].HasHandle & !InvalidateHandle) {
					SoundSources[SoundSourceIndex].FinishedPlaying = true;
				} else {
					StopSound(ref SoundSourceIndex);
				}
			}
		}
		internal static void StopSound(ref int SoundSourceIndex) {
			if (OpenAlContext != IntPtr.Zero) {
				if (SoundSourceIndex >= 0 && SoundSourceIndex < SoundSources.Length && SoundSources[SoundSourceIndex] != null) {
					if (SoundSources[SoundSourceIndex].OpenAlSourceIndex.Valid) {
						int i = SoundSources[SoundSourceIndex].OpenAlSourceIndex.Index;
						Al.alSourceStop(i);
						Al.alDeleteSources(1, ref i);
					}
					SoundSources[SoundSourceIndex] = null;
				}
				SoundSourceIndex = -1;
			}
		}
		internal static void StopAllSounds(bool InvalidateHandles) {
			if (OpenAlContext != IntPtr.Zero) {
				for (int i = 0; i < SoundSources.Length; i++) {
					if (SoundSources[i] != null) {
						StopSound(i, InvalidateHandles);
					}
				}
			}
		}
		internal static void StopAllSounds(TrainManager.Train Train, bool InvalidateHandles) {
			if (OpenAlContext != IntPtr.Zero) {
				for (int i = 0; i < SoundSources.Length; i++) {
					if (SoundSources[i] != null && SoundSources[i].Train == Train) {
						StopSound(i, InvalidateHandles);
					}
				}
			}
		}

		// is playing
		internal static bool IsPlaying(int SoundSourceIndex) {
			if (OpenAlContext != IntPtr.Zero) {
				if (SoundSourceIndex >= 0 && SoundSourceIndex < SoundSources.Length && SoundSources[SoundSourceIndex] != null) {
					if (SoundSources[SoundSourceIndex].Suppressed) {
						return true;
					} else {
						if (SoundSources[SoundSourceIndex].OpenAlSourceIndex.Valid) {
							int i = SoundSources[SoundSourceIndex].OpenAlSourceIndex.Index;
							int state;
							Al.alGetSourcei(i, Al.AL_SOURCE_STATE, out state);
							return state == Al.AL_PLAYING;
						} else {
							return false;
						}
					}
				} else {
					return false;
				}
			} else {
				return false;
			}
		}

		// has finished playing
		internal static bool HasFinishedPlaying(int SoundSourceIndex) {
			if (OpenAlContext != IntPtr.Zero) {
				if (SoundSourceIndex >= 0 && SoundSources[SoundSourceIndex] != null) {
					return SoundSources[SoundSourceIndex].FinishedPlaying;
				} else {
					return true;
				}
			} else {
				return false;
			}
		}

	}
}