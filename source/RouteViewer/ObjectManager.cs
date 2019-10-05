using System;
using System.Text;
using OpenBveApi.Interface;
using OpenBveApi.Objects;
using OpenBveApi.Routes;
using OpenBveApi.Trains;

namespace OpenBve {
	internal static class ObjectManager {

		internal static WorldObject[] AnimatedWorldObjects = new WorldObject[4];
		internal static int AnimatedWorldObjectsUsed = 0;
		
		internal static void UpdateAnimatedWorldObjects(double TimeElapsed, bool ForceUpdate) {
			for (int i = 0; i < AnimatedWorldObjectsUsed; i++)
			{
				TrainManager.Train train = null;
				const double extraRadius = 10.0;
				double z = AnimatedWorldObjects[i].Object.TranslateZFunction == null ? 0.0 : AnimatedWorldObjects[i].Object.TranslateZFunction.LastResult;
				double pa = AnimatedWorldObjects[i].TrackPosition + z - AnimatedWorldObjects[i].Radius - extraRadius;
				double pb = AnimatedWorldObjects[i].TrackPosition + z + AnimatedWorldObjects[i].Radius + extraRadius;
				double ta = World.CameraTrackFollower.TrackPosition + Program.Renderer.Camera.Alignment.Position.Z - Program.CurrentRoute.CurrentBackground.BackgroundImageDistance - Program.Renderer.Camera.ExtraViewingDistance;
				double tb = World.CameraTrackFollower.TrackPosition + Program.Renderer.Camera.Alignment.Position.Z + Program.CurrentRoute.CurrentBackground.BackgroundImageDistance + Program.Renderer.Camera.ExtraViewingDistance;
				bool visible = pb >= ta & pa <= tb;
				if (visible | ForceUpdate)
				{
					//Find the closest train
					double trainDistance = double.MaxValue;
					for (int j = 0; j < TrainManager.Trains.Length; j++)
					{
						if (TrainManager.Trains[j].State == TrainState.Available)
						{
							double distance;
							if (TrainManager.Trains[j].Cars[0].FrontAxle.Follower.TrackPosition < AnimatedWorldObjects[i].TrackPosition)
							{
								distance = AnimatedWorldObjects[i].TrackPosition - TrainManager.Trains[j].Cars[0].TrackPosition;
							}
							else if (TrainManager.Trains[j].Cars[TrainManager.Trains[j].Cars.Length - 1].RearAxle.Follower.TrackPosition > AnimatedWorldObjects[i].TrackPosition)
							{
								distance = TrainManager.Trains[j].Cars[TrainManager.Trains[j].Cars.Length - 1].RearAxle.Follower.TrackPosition - AnimatedWorldObjects[i].TrackPosition;
							}
							else
							{
								distance = 0;
							}
							if (distance < trainDistance)
							{
								train = TrainManager.Trains[j];
								trainDistance = distance;
							}
						}
					}
				}
				AnimatedWorldObjects[i].Update(train, TimeElapsed, ForceUpdate, visible);
			}
		}

		// load object
		internal static UnifiedObject LoadObject(string FileName, Encoding Encoding, bool PreserveVertices) {
			#if !DEBUG
			try {
				#endif
				if (!System.IO.Path.HasExtension(FileName)) {
					while (true) {
						string f = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), System.IO.Path.GetFileName(FileName) + ".x");
						if (System.IO.File.Exists(f)) {
							FileName = f;
							break;
						}
						f = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), System.IO.Path.GetFileName(FileName) + ".csv");
						if (System.IO.File.Exists(f)) {
							FileName = f;
							break;
						}
						f = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), System.IO.Path.GetFileName(FileName) + ".b3d");
						if (System.IO.File.Exists(f)) {
							FileName = f;
							break;
						}
						break;
					}
				}
				UnifiedObject Result;
				switch (System.IO.Path.GetExtension(FileName).ToLowerInvariant()) {
					case ".csv":
					case ".b3d":
					case ".x":
					case ".obj":
					case ".animated":
					case ".l3dobj":
					case ".l3dgrp":
					case ".s":
						Program.CurrentHost.LoadObject(FileName, Encoding, out Result);
						break;
					default:
						Interface.AddMessage(MessageType.Error, false, "The file extension is not supported: " + FileName);
						return null;
				}

				if (Result != null)
				{
					Result.OptimizeObject(PreserveVertices, Interface.CurrentOptions.ObjectOptimizationBasicThreshold, false);
				}
				return Result;
				#if !DEBUG
			} catch (Exception ex) {
				Interface.AddMessage(MessageType.Error, true, "An unexpected error occured (" + ex.Message + ") while attempting to load the file " + FileName);
				return null;
			}
			#endif
		}
		internal static StaticObject LoadStaticObject(string FileName, Encoding Encoding, bool PreserveVertices) {
			#if !DEBUG
			try {
				#endif
				if (!System.IO.Path.HasExtension(FileName)) {
					while (true) {
						string f = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), System.IO.Path.GetFileName(FileName) + ".x");
						if (System.IO.File.Exists(f)) {
							FileName = f;
							break;
						}
						f = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), System.IO.Path.GetFileName(FileName) + ".csv");
						if (System.IO.File.Exists(f)) {
							FileName = f;
							break;
						}
						f = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), System.IO.Path.GetFileName(FileName) + ".b3d");
						if (System.IO.File.Exists(f)) {
							FileName = f;
							break;
						}
						break;
					}
				}
				StaticObject Result;
				UnifiedObject obj;
				switch (System.IO.Path.GetExtension(FileName).ToLowerInvariant()) {
					case ".csv":
					case ".b3d":
					case ".x":
					case ".obj":
					case ".animated":
					case ".l3dobj":
					case ".l3dgrp":
					case ".s":
						Program.CurrentHost.LoadObject(FileName, Encoding, out obj);
						if (obj is AnimatedObjectCollection)
						{
							Interface.AddMessage(MessageType.Error, false, "Tried to load an animated object even though only static objects are allowed: " + FileName);
							return null;
						}
						Result = (StaticObject)obj;
						break;
					default:
						Interface.AddMessage(MessageType.Error, false, "The file extension is not supported: " + FileName);
						return null;
				}

				if (Result != null)
				{
					Result.OptimizeObject(PreserveVertices, Interface.CurrentOptions.ObjectOptimizationBasicThreshold, false);
				}
				return Result;
				#if !DEBUG
			} catch (Exception ex) {
				Interface.AddMessage(MessageType.Error, true, "An unexpected error occured (" + ex.Message + ") while attempting to load the file " + FileName);
				return null;
			}
			#endif
		}
	}
}
