using System;
using System.Text;
using LibRender;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Trains;
using OpenBveApi.World;

namespace OpenBve
{
    internal static class ObjectManager
    {

        // static objects
        internal static StaticObject[] Objects = new StaticObject[16];
        internal static int ObjectsUsed;
        internal static int[] ObjectsSortedByStart = new int[] { };
        internal static int[] ObjectsSortedByEnd = new int[] { };
        internal static int ObjectsSortedByStartPointer = 0;
        internal static int ObjectsSortedByEndPointer = 0;
        internal static double LastUpdatedTrackPosition = 0.0;

        internal class AnimatedObject : AnimatedObjectBase
        {
            internal AnimatedObject Clone()
            {
                AnimatedObject Result = new AnimatedObject();
                Result.States = new AnimatedObjectState[this.States.Length];
                for (int i = 0; i < this.States.Length; i++)
                {
                    Result.States[i].Position = this.States[i].Position;
                    Result.States[i].Object = (StaticObject)this.States[i].Object.Clone();
                }
                Result.StateFunction = this.StateFunction == null ? null : this.StateFunction.Clone();
                Result.CurrentState = this.CurrentState;
                Result.TranslateZDirection = this.TranslateZDirection;
                Result.TranslateYDirection = this.TranslateYDirection;
                Result.TranslateXDirection = this.TranslateXDirection;
                Result.TranslateXFunction = this.TranslateXFunction == null ? null : this.TranslateXFunction.Clone();
                Result.TranslateYFunction = this.TranslateYFunction == null ? null : this.TranslateYFunction.Clone();
                Result.TranslateZFunction = this.TranslateZFunction == null ? null : this.TranslateZFunction.Clone();
                Result.RotateXDirection = this.RotateXDirection;
                Result.RotateYDirection = this.RotateYDirection;
                Result.RotateZDirection = this.RotateZDirection;
                Result.RotateXFunction = this.RotateXFunction == null ? null : this.RotateXFunction.Clone();
                Result.RotateXDamping = this.RotateXDamping == null ? null : this.RotateXDamping.Clone();
                Result.RotateYFunction = this.RotateYFunction == null ? null : this.RotateYFunction.Clone();
                Result.RotateYDamping = this.RotateYDamping == null ? null : this.RotateYDamping.Clone();
                Result.RotateZFunction = this.RotateZFunction == null ? null : this.RotateZFunction.Clone();
                Result.RotateZDamping = this.RotateZDamping == null ? null : this.RotateZDamping.Clone();
                Result.TextureShiftXDirection = this.TextureShiftXDirection;
                Result.TextureShiftYDirection = this.TextureShiftYDirection;
                Result.TextureShiftXFunction = this.TextureShiftXFunction == null ? null : this.TextureShiftXFunction.Clone();
                Result.TextureShiftYFunction = this.TextureShiftYFunction == null ? null : this.TextureShiftYFunction.Clone();
                Result.LEDClockwiseWinding = this.LEDClockwiseWinding;
                Result.LEDInitialAngle = this.LEDInitialAngle;
                Result.LEDLastAngle = this.LEDLastAngle;
                if (this.LEDVectors != null)
                {
                    Result.LEDVectors = new Vector3[this.LEDVectors.Length];
                    for (int i = 0; i < this.LEDVectors.Length; i++)
                    {
                        Result.LEDVectors[i] = this.LEDVectors[i];
                    }
                }
                else
                {
                    Result.LEDVectors = null;
                }
                Result.LEDFunction = this.LEDFunction == null ? null : this.LEDFunction.Clone();
                Result.RefreshRate = this.RefreshRate;
                Result.SecondsSinceLastUpdate = 0.0;
                Result.ObjectIndex = -1;
                return Result;
            }
        }
        internal class AnimatedObjectCollection : UnifiedObject
        {
            internal AnimatedObject[] Objects;
            public override void CreateObject(Vector3 Position, Transformation BaseTransformation, Transformation AuxTransformation, int SectionIndex, bool AccurateObjectDisposal, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition, double Brightness, bool DuplicateMaterials)
            {
	            throw new NotImplementedException();
            }

            public override UnifiedObject Clone()
            {
	            throw new NotSupportedException();
            }

            public override void OptimizeObject(bool PreserveVerticies, int Threshold, bool VertexCulling)
	        {
		        for (int i = 0; i < Objects.Length; i++)
		        {
			        if (Objects[i] != null)
			        {
				        for (int j = 0; j < Objects[i].States.Length; j++)
				        {
					        Objects[i].States[j].Object.OptimizeObject(PreserveVerticies, Threshold, VertexCulling);
				        }
			        }
		        }
			}
        }
        internal static void InitializeAnimatedObject(ref AnimatedObject Object, int StateIndex, bool Overlay, bool Show)
        {
            int i = Object.ObjectIndex;
            Renderer.HideObject(i);
            int t = StateIndex;
            if (t >= 0 && Object.States[t].Object != null)
            {
                int m = Object.States[t].Object.Mesh.Vertices.Length;
                ObjectManager.Objects[i].Mesh.Vertices = new VertexTemplate[m];
                for (int k = 0; k < m; k++)
                {
	                if (Object.States[t].Object.Mesh.Vertices[k] is Vertex)
	                {
		                ObjectManager.Objects[i].Mesh.Vertices[k] = new Vertex((Vertex)Object.States[t].Object.Mesh.Vertices[k]);
	                }
					else if (Object.States[t].Object.Mesh.Vertices[k] is ColoredVertex)
	                {
		                ObjectManager.Objects[i].Mesh.Vertices[k] = new ColoredVertex((ColoredVertex)Object.States[t].Object.Mesh.Vertices[k]);
	                }
                }
                m = Object.States[t].Object.Mesh.Faces.Length;
                ObjectManager.Objects[i].Mesh.Faces = new MeshFace[m];
                for (int k = 0; k < m; k++)
                {
                    ObjectManager.Objects[i].Mesh.Faces[k].Flags = Object.States[t].Object.Mesh.Faces[k].Flags;
                    ObjectManager.Objects[i].Mesh.Faces[k].Material = Object.States[t].Object.Mesh.Faces[k].Material;
                    int o = Object.States[t].Object.Mesh.Faces[k].Vertices.Length;
                    ObjectManager.Objects[i].Mesh.Faces[k].Vertices = new MeshFaceVertex[o];
                    for (int h = 0; h < o; h++)
                    {
                        ObjectManager.Objects[i].Mesh.Faces[k].Vertices[h] = Object.States[t].Object.Mesh.Faces[k].Vertices[h];
                    }
                }
                ObjectManager.Objects[i].Mesh.Materials = Object.States[t].Object.Mesh.Materials;
            }
            else
            {
	            ObjectManager.Objects[i] = new StaticObject(Program.CurrentHost);
            }
            Object.CurrentState = StateIndex;
            if (Show)
            {
                if (Overlay)
                {
                    Renderer.ShowObject(i, ObjectType.Overlay);
                }
                else
                {
                    Renderer.ShowObject(i, ObjectType.Dynamic);
                }
            }
        }

        internal static void UpdateAnimatedObject(ref AnimatedObject Object, bool IsPartOfTrain, AbstractTrain Train, int CarIndex, int SectionIndex, double TrackPosition, Vector3 Position, Vector3 Direction, Vector3 Up, Vector3 Side, bool Overlay, bool UpdateFunctions, bool Show, double TimeElapsed)
        {
            int s = Object.CurrentState;
            int i = Object.ObjectIndex;
            // state change
            if (Object.StateFunction != null & UpdateFunctions)
            {
                double sd = Object.StateFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, Object.CurrentState);
                int si = (int)Math.Round(sd);
                int sn = Object.States.Length;
                if (si < 0 | si >= sn) si = -1;
                if (s != si)
                {
                    InitializeAnimatedObject(ref Object, si, Overlay, Show);
                    s = si;
                }
            }
            if (s == -1) return;
            // translation
            if (Object.TranslateXFunction != null)
            {
                double x;
                if (UpdateFunctions)
                {
                    x = Object.TranslateXFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, Object.CurrentState);
                }
                else
                {
                    x = Object.TranslateXFunction.LastResult;
                }
	            Vector3 translationVector = new Vector3(Object.TranslateXDirection); //Must clone
	            translationVector.Rotate(Direction, Up, Side);
	            translationVector *= x;
	            Position += translationVector;
            }
            if (Object.TranslateYFunction != null)
            {
                double y;
                if (UpdateFunctions)
                {
                    y = Object.TranslateYFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, Object.CurrentState);
                }
                else
                {
                    y = Object.TranslateYFunction.LastResult;
                }
	            Vector3 translationVector = new Vector3(Object.TranslateYDirection); //Must clone
	            translationVector.Rotate(Direction, Up, Side);
	            translationVector *= y;
	            Position += translationVector;
            }
            if (Object.TranslateZFunction != null)
            {
                double z;
                if (UpdateFunctions)
                {
                    z = Object.TranslateZFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, Object.CurrentState);
                }
                else
                {
                    z = Object.TranslateZFunction.LastResult;
                }
	            Vector3 translationVector = new Vector3(Object.TranslateZDirection); //Must clone
	            translationVector.Rotate(Direction, Up, Side);
	            translationVector *= z;
	            Position += translationVector;
            }
            // rotation
            bool rotateX = Object.RotateXFunction != null;
            bool rotateY = Object.RotateYFunction != null;
            bool rotateZ = Object.RotateZFunction != null;
            double cosX, sinX;
            if (rotateX)
            {
                double a;
                if (UpdateFunctions)
                {
                    a = Object.RotateXFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, Object.CurrentState);
                }
                else
                {
                    a = Object.RotateXFunction.LastResult;
                }
				if (Object.RotateXDamping != null)
	            {
		            Object.RotateXDamping.Update(TimeElapsed, ref a, true);
	            }
                cosX = Math.Cos(a);
                sinX = Math.Sin(a);
            }
            else
            {
                cosX = 0.0; sinX = 0.0;
            }
            double cosY, sinY;
            if (rotateY)
            {
                double a;
                if (UpdateFunctions)
                {
                    a = Object.RotateYFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, Object.CurrentState);
                }
                else
                {
                    a = Object.RotateYFunction.LastResult;
                }
				if (Object.RotateYDamping != null)
	            {
		            Object.RotateYDamping.Update(TimeElapsed, ref a, true);
	            }
                cosY = Math.Cos(a);
                sinY = Math.Sin(a);
            }
            else
            {
                cosY = 0.0; sinY = 0.0;
            }
            double cosZ, sinZ;
            if (rotateZ)
            {
                double a;
                if (UpdateFunctions)
                {
                    a = Object.RotateZFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, Object.CurrentState);
                }
                else
                {
                    a = Object.RotateZFunction.LastResult;
                }
				if (Object.RotateZDamping != null)
	            {
		            Object.RotateZDamping.Update(TimeElapsed, ref a, true);
	            }
                cosZ = Math.Cos(a);
                sinZ = Math.Sin(a);
            }
            else
            {
                cosZ = 0.0; sinZ = 0.0;
            }
            // texture shift
            bool shiftx = Object.TextureShiftXFunction != null;
            bool shifty = Object.TextureShiftYFunction != null;
            if ((shiftx | shifty) & UpdateFunctions)
            {
                for (int k = 0; k < ObjectManager.Objects[i].Mesh.Vertices.Length; k++)
					{
						ObjectManager.Objects[i].Mesh.Vertices[k].TextureCoordinates = Object.States[s].Object.Mesh.Vertices[k].TextureCoordinates;
					}
					if (shiftx)
					{
						double x = Object.TextureShiftXFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, Object.CurrentState);
						x -= Math.Floor(x);
						for (int k = 0; k < ObjectManager.Objects[i].Mesh.Vertices.Length; k++)
						{
							ObjectManager.Objects[i].Mesh.Vertices[k].TextureCoordinates.X += (float)(x * Object.TextureShiftXDirection.X);
							ObjectManager.Objects[i].Mesh.Vertices[k].TextureCoordinates.Y += (float)(x * Object.TextureShiftXDirection.Y);
						}
					}
					if (shifty)
					{
						double y = Object.TextureShiftYFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, Object.CurrentState);
						y -= Math.Floor(y);
						for (int k = 0; k < ObjectManager.Objects[i].Mesh.Vertices.Length; k++)
						{
							ObjectManager.Objects[i].Mesh.Vertices[k].TextureCoordinates.X += (float)(y * Object.TextureShiftYDirection.X);
							ObjectManager.Objects[i].Mesh.Vertices[k].TextureCoordinates.Y += (float)(y * Object.TextureShiftYDirection.Y);
						}
					}
            }
            // led
            bool led = Object.LEDFunction != null;
            double ledangle;
            if (led)
            {
                if (UpdateFunctions)
                {
                    // double lastangle = Object.LEDFunction.LastResult;
                    ledangle = Object.LEDFunction.Perform(Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, Object.CurrentState);
                }
                else
                {
                    ledangle = Object.LEDFunction.LastResult;
                }
            }
            else
            {
                ledangle = 0.0;
            }
            // null object
            if (Object.States[s].Object == null)
            {
                return;
            }
            // initialize vertices
            for (int k = 0; k < Object.States[s].Object.Mesh.Vertices.Length; k++)
            {
	            ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates = Object.States[s].Object.Mesh.Vertices[k].Coordinates;
            }
            // led
            if (led)
            {
                /*
                 * Edges:         Vertices:
                 * 0 - bottom     0 - bottom-left
                 * 1 - left       1 - top-left
                 * 2 - top        2 - top-right
                 * 3 - right      3 - bottom-right
                 *                4 - center
                 * */
                int v = 1;
                if (Object.LEDClockwiseWinding)
                {
                    /* winding is clockwise*/
                    if (ledangle < Object.LEDInitialAngle)
                    {
                        ledangle = Object.LEDInitialAngle;
                    }
                    if (ledangle < Object.LEDLastAngle)
                    {
                        double currentEdgeFloat = Math.Floor(0.636619772367582 * (ledangle + 0.785398163397449));
                        int currentEdge = ((int)currentEdgeFloat % 4 + 4) % 4;
                        double lastEdgeFloat = Math.Floor(0.636619772367582 * (Object.LEDLastAngle + 0.785398163397449));
                        int lastEdge = ((int)lastEdgeFloat % 4 + 4) % 4;
                        if (lastEdge < currentEdge | lastEdge == currentEdge & Math.Abs(currentEdgeFloat - lastEdgeFloat) > 2.0)
                        {
                            lastEdge += 4;
                        }
                        if (currentEdge == lastEdge)
                        {
                            /* current angle to last angle */
                            {
                                double t = 0.5 + (0.636619772367582 * ledangle) - currentEdgeFloat;
                                if (t < 0.0)
                                {
                                    t = 0.0;
                                }
                                else if (t > 1.0)
                                {
                                    t = 1.0;
                                }
                                t = 0.5 * (1.0 - Math.Tan(0.25 * (Math.PI - 2.0 * Math.PI * t)));
                                double cx = (1.0 - t) * Object.LEDVectors[(currentEdge + 3) % 4].X + t * Object.LEDVectors[currentEdge].X;
                                double cy = (1.0 - t) * Object.LEDVectors[(currentEdge + 3) % 4].Y + t * Object.LEDVectors[currentEdge].Y;
                                double cz = (1.0 - t) * Object.LEDVectors[(currentEdge + 3) % 4].Z + t * Object.LEDVectors[currentEdge].Z;
                                Object.States[s].Object.Mesh.Vertices[v].Coordinates = new Vector3(cx, cy, cz);
                                v++;
                            }
                            {
                                double t = 0.5 + (0.636619772367582 * Object.LEDLastAngle) - lastEdgeFloat;
                                if (t < 0.0)
                                {
                                    t = 0.0;
                                }
                                else if (t > 1.0)
                                {
                                    t = 1.0;
                                }
                                t = 0.5 * (1.0 - Math.Tan(0.25 * (Math.PI - 2.0 * Math.PI * t)));
                                double lx = (1.0 - t) * Object.LEDVectors[(lastEdge + 3) % 4].X + t * Object.LEDVectors[lastEdge].X;
                                double ly = (1.0 - t) * Object.LEDVectors[(lastEdge + 3) % 4].Y + t * Object.LEDVectors[lastEdge].Y;
                                double lz = (1.0 - t) * Object.LEDVectors[(lastEdge + 3) % 4].Z + t * Object.LEDVectors[lastEdge].Z;
                                Object.States[s].Object.Mesh.Vertices[v].Coordinates = new Vector3(lx, ly, lz);
                                v++;
                            }
                        }
                        else
                        {
                            {
                                /* current angle to square vertex */
                                double t = 0.5 + (0.636619772367582 * ledangle) - currentEdgeFloat;
                                if (t < 0.0)
                                {
                                    t = 0.0;
                                }
                                else if (t > 1.0)
                                {
                                    t = 1.0;
                                }
                                t = 0.5 * (1.0 - Math.Tan(0.25 * (Math.PI - 2.0 * Math.PI * t)));
                                double cx = (1.0 - t) * Object.LEDVectors[(currentEdge + 3) % 4].X + t * Object.LEDVectors[currentEdge].X;
                                double cy = (1.0 - t) * Object.LEDVectors[(currentEdge + 3) % 4].Y + t * Object.LEDVectors[currentEdge].Y;
                                double cz = (1.0 - t) * Object.LEDVectors[(currentEdge + 3) % 4].Z + t * Object.LEDVectors[currentEdge].Z;
                                Object.States[s].Object.Mesh.Vertices[v + 0].Coordinates = new Vector3(cx, cy, cz);
                                Object.States[s].Object.Mesh.Vertices[v + 1].Coordinates = Object.LEDVectors[currentEdge];
                                v += 2;
                            }
                            for (int j = currentEdge + 1; j < lastEdge; j++)
                            {
                                /* square-vertex to square-vertex */
                                Object.States[s].Object.Mesh.Vertices[v + 0].Coordinates = Object.LEDVectors[(j + 3) % 4];
                                Object.States[s].Object.Mesh.Vertices[v + 1].Coordinates = Object.LEDVectors[j % 4];
                                v += 2;
                            }
                            {
                                /* square vertex to last angle */
                                double t = 0.5 + (0.636619772367582 * Object.LEDLastAngle) - lastEdgeFloat;
                                if (t < 0.0)
                                {
                                    t = 0.0;
                                }
                                else if (t > 1.0)
                                {
                                    t = 1.0;
                                }
                                t = 0.5 * (1.0 - Math.Tan(0.25 * (Math.PI - 2.0 * Math.PI * t)));
                                double lx = (1.0 - t) * Object.LEDVectors[(lastEdge + 3) % 4].X + t * Object.LEDVectors[lastEdge % 4].X;
                                double ly = (1.0 - t) * Object.LEDVectors[(lastEdge + 3) % 4].Y + t * Object.LEDVectors[lastEdge % 4].Y;
                                double lz = (1.0 - t) * Object.LEDVectors[(lastEdge + 3) % 4].Z + t * Object.LEDVectors[lastEdge % 4].Z;
                                Object.States[s].Object.Mesh.Vertices[v + 0].Coordinates = Object.LEDVectors[(lastEdge + 3) % 4];
                                Object.States[s].Object.Mesh.Vertices[v + 1].Coordinates = new Vector3(lx, ly, lz);
                                v += 2;
                            }
                        }
                    }
                }
                else
                {
                    /* winding is counter-clockwise*/
                    if (ledangle > Object.LEDInitialAngle)
                    {
                        ledangle = Object.LEDInitialAngle;
                    }
                    if (ledangle > Object.LEDLastAngle)
                    {
                        double currentEdgeFloat = Math.Floor(0.636619772367582 * (ledangle + 0.785398163397449));
                        int currentEdge = ((int)currentEdgeFloat % 4 + 4) % 4;
                        double lastEdgeFloat = Math.Floor(0.636619772367582 * (Object.LEDLastAngle + 0.785398163397449));
                        int lastEdge = ((int)lastEdgeFloat % 4 + 4) % 4;
                        if (currentEdge < lastEdge | lastEdge == currentEdge & Math.Abs(currentEdgeFloat - lastEdgeFloat) > 2.0)
                        {
                            currentEdge += 4;
                        }
                        if (currentEdge == lastEdge)
                        {
                            /* current angle to last angle */
                            {
                                double t = 0.5 + (0.636619772367582 * Object.LEDLastAngle) - lastEdgeFloat;
                                if (t < 0.0)
                                {
                                    t = 0.0;
                                }
                                else if (t > 1.0)
                                {
                                    t = 1.0;
                                }
                                t = 0.5 * (1.0 - Math.Tan(0.25 * (Math.PI - 2.0 * Math.PI * t)));
                                double lx = (1.0 - t) * Object.LEDVectors[(lastEdge + 3) % 4].X + t * Object.LEDVectors[lastEdge].X;
                                double ly = (1.0 - t) * Object.LEDVectors[(lastEdge + 3) % 4].Y + t * Object.LEDVectors[lastEdge].Y;
                                double lz = (1.0 - t) * Object.LEDVectors[(lastEdge + 3) % 4].Z + t * Object.LEDVectors[lastEdge].Z;
                                Object.States[s].Object.Mesh.Vertices[v].Coordinates = new Vector3(lx, ly, lz);
                                v++;
                            }
                            {
                                double t = 0.5 + (0.636619772367582 * ledangle) - currentEdgeFloat;
                                if (t < 0.0)
                                {
                                    t = 0.0;
                                }
                                else if (t > 1.0)
                                {
                                    t = 1.0;
                                }
                                t = t - Math.Floor(t);
                                t = 0.5 * (1.0 - Math.Tan(0.25 * (Math.PI - 2.0 * Math.PI * t)));
                                double cx = (1.0 - t) * Object.LEDVectors[(currentEdge + 3) % 4].X + t * Object.LEDVectors[currentEdge].X;
                                double cy = (1.0 - t) * Object.LEDVectors[(currentEdge + 3) % 4].Y + t * Object.LEDVectors[currentEdge].Y;
                                double cz = (1.0 - t) * Object.LEDVectors[(currentEdge + 3) % 4].Z + t * Object.LEDVectors[currentEdge].Z;
                                Object.States[s].Object.Mesh.Vertices[v].Coordinates = new Vector3(cx, cy, cz);
                                v++;
                            }
                        }
                        else
                        {
                            {
                                /* current angle to square vertex */
                                double t = 0.5 + (0.636619772367582 * ledangle) - currentEdgeFloat;
                                if (t < 0.0)
                                {
                                    t = 0.0;
                                }
                                else if (t > 1.0)
                                {
                                    t = 1.0;
                                }
                                t = 0.5 * (1.0 - Math.Tan(0.25 * (Math.PI - 2.0 * Math.PI * t)));
                                double cx = (1.0 - t) * Object.LEDVectors[(currentEdge + 3) % 4].X + t * Object.LEDVectors[currentEdge % 4].X;
                                double cy = (1.0 - t) * Object.LEDVectors[(currentEdge + 3) % 4].Y + t * Object.LEDVectors[currentEdge % 4].Y;
                                double cz = (1.0 - t) * Object.LEDVectors[(currentEdge + 3) % 4].Z + t * Object.LEDVectors[currentEdge % 4].Z;
                                Object.States[s].Object.Mesh.Vertices[v + 0].Coordinates = Object.LEDVectors[(currentEdge + 3) % 4];
                                Object.States[s].Object.Mesh.Vertices[v + 1].Coordinates = new Vector3(cx, cy, cz);
                                v += 2;
                            }
                            for (int j = currentEdge - 1; j > lastEdge; j--)
                            {
                                /* square-vertex to square-vertex */
                                Object.States[s].Object.Mesh.Vertices[v + 0].Coordinates = Object.LEDVectors[(j + 3) % 4];
                                Object.States[s].Object.Mesh.Vertices[v + 1].Coordinates = Object.LEDVectors[j % 4];
                                v += 2;
                            }
                            {
                                /* square vertex to last angle */
                                double t = 0.5 + (0.636619772367582 * Object.LEDLastAngle) - lastEdgeFloat;
                                if (t < 0.0)
                                {
                                    t = 0.0;
                                }
                                else if (t > 1.0)
                                {
                                    t = 1.0;
                                }
                                t = 0.5 * (1.0 - Math.Tan(0.25 * (Math.PI - 2.0 * Math.PI * t)));
                                double lx = (1.0 - t) * Object.LEDVectors[(lastEdge + 3) % 4].X + t * Object.LEDVectors[lastEdge].X;
                                double ly = (1.0 - t) * Object.LEDVectors[(lastEdge + 3) % 4].Y + t * Object.LEDVectors[lastEdge].Y;
                                double lz = (1.0 - t) * Object.LEDVectors[(lastEdge + 3) % 4].Z + t * Object.LEDVectors[lastEdge].Z;
                                Object.States[s].Object.Mesh.Vertices[v + 0].Coordinates = new Vector3(lx, ly, lz);
                                Object.States[s].Object.Mesh.Vertices[v + 1].Coordinates = Object.LEDVectors[lastEdge % 4];
                                v += 2;
                            }
                        }
                    }
                }
                for (int j = v; v < 11; v++)
                {
                    Object.States[s].Object.Mesh.Vertices[j].Coordinates = Object.LEDVectors[4];
                }
            }
            // update vertices
            for (int k = 0; k < Object.States[s].Object.Mesh.Vertices.Length; k++)
            {
                // rotate
                if (rotateX)
                {
	                ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.Rotate(Object.RotateXDirection, cosX, sinX);
                }
                if (rotateY)
                {
	                ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.Rotate(Object.RotateYDirection, cosY, sinY);
                }
                if (rotateZ)
                {
	                ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.Rotate(Object.RotateZDirection, cosZ, sinZ);
                }
                // translate
	            if (Overlay & World.CameraRestriction != CameraRestrictionMode.NotAvailable)
	            {
		            ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates += Object.States[s].Position - Position;
		            ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.Rotate(World.AbsoluteCameraDirection, World.AbsoluteCameraUp, World.AbsoluteCameraSide);
		            double dx = -Math.Tan(World.CameraCurrentAlignment.Yaw) - World.CameraCurrentAlignment.Position.X;
		            double dy = -Math.Tan(World.CameraCurrentAlignment.Pitch) - World.CameraCurrentAlignment.Position.Y;
		            double dz = -World.CameraCurrentAlignment.Position.Z;
		            ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates += World.AbsoluteCameraPosition + dx * World.AbsoluteCameraSide + dy * World.AbsoluteCameraUp + dz * World.AbsoluteCameraDirection;
	            }
	            else
	            {
		            ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates += Object.States[s].Position;
		            ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates.Rotate(Direction, Up, Side);
		            ObjectManager.Objects[i].Mesh.Vertices[k].Coordinates += Position;
                }
            }
            // update normals
            for (int k = 0; k < Object.States[s].Object.Mesh.Faces.Length; k++)
            {
                for (int h = 0; h < Object.States[s].Object.Mesh.Faces[k].Vertices.Length; h++)
                {
                    ObjectManager.Objects[i].Mesh.Faces[k].Vertices[h].Normal = Object.States[s].Object.Mesh.Faces[k].Vertices[h].Normal;
	                if (!Vector3.IsZero(Object.States[s].Object.Mesh.Faces[k].Vertices[h].Normal))
	                {
		                if (rotateX)
		                {
			                ObjectManager.Objects[i].Mesh.Faces[k].Vertices[h].Normal.Rotate(Object.RotateXDirection, cosX, sinX);
		                }
		                if (rotateY)
		                {
			                ObjectManager.Objects[i].Mesh.Faces[k].Vertices[h].Normal.Rotate(Object.RotateYDirection, cosY, sinY);
		                }
		                if (rotateZ)
		                {
			                ObjectManager.Objects[i].Mesh.Faces[k].Vertices[h].Normal.Rotate(Object.RotateZDirection, cosZ, sinZ);
		                }
		                ObjectManager.Objects[i].Mesh.Faces[k].Vertices[h].Normal.Rotate(Direction, Up, Side);
	                }
                }
                // visibility changed
                if (Show)
                {
                    if (Overlay)
                    {
                        Renderer.ShowObject(i, ObjectType.Overlay);
                    }
                    else
                    {
                        Renderer.ShowObject(i, ObjectType.Dynamic);
                    }
                }
                else
                {
                    Renderer.HideObject(i);
                }
            }
        }

        

        // animated world object
        internal class AnimatedWorldObject : WorldObject
        {
            internal AnimatedObject Object;
            internal int SectionIndex;
            internal double Radius;

            public override void Update(double TimeElapsed, bool ForceUpdate)
            {
	            throw new NotImplementedException();
            }
        }
        internal static AnimatedWorldObject[] AnimatedWorldObjects = new AnimatedWorldObject[4];
        internal static int AnimatedWorldObjectsUsed = 0;
        internal static void CreateAnimatedWorldObjects(AnimatedObject[] Prototypes, Vector3 Position, Transformation BaseTransformation, Transformation AuxTransformation, int SectionIndex, bool AccurateObjectDisposal, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition, double Brightness, bool DuplicateMaterials)
        {
            bool[] free = new bool[Prototypes.Length];
            bool anyfree = false;
            for (int i = 0; i < Prototypes.Length; i++)
            {
				if (Prototypes[i] == null)
				{
					free[i] = true;
					continue;
				}
                free[i] = Prototypes[i].IsFreeOfFunctions();
                if (free[i]) anyfree = true;
            }
            if (anyfree)
            {
                for (int i = 0; i < Prototypes.Length; i++)
                {
					if (Prototypes[i] == null)
					{
						continue;
					}
                    if (Prototypes[i].States.Length != 0)
                    {
                        if (free[i])
                        {
                            Vector3 p = Position;
                            Transformation t = new Transformation(BaseTransformation, AuxTransformation);
                            Vector3 s = t.X;
                            Vector3 u = t.Y;
                            Vector3 d = t.Z;
                            p += Prototypes[i].States[0].Position.X * s + Prototypes[i].States[0].Position.Y * u + Prototypes[i].States[0].Position.Z * d;
                            double zOffset = Prototypes[i].States[0].Position.Z;
                            CreateStaticObject(Prototypes[i].States[0].Object, p, BaseTransformation, AuxTransformation, AccurateObjectDisposal, zOffset, StartingDistance, EndingDistance, BlockLength, TrackPosition, Brightness);
                        }
                        else
                        {
                            CreateAnimatedWorldObject(Prototypes[i], Position, BaseTransformation, AuxTransformation, SectionIndex, TrackPosition, Brightness);
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < Prototypes.Length; i++)
                {
                    if (Prototypes[i].States.Length != 0)
                    {
                        CreateAnimatedWorldObject(Prototypes[i], Position, BaseTransformation, AuxTransformation, SectionIndex, TrackPosition, Brightness);
                    }
                }
            }
        }
        internal static int CreateAnimatedWorldObject(AnimatedObject Prototype, Vector3 Position, Transformation BaseTransformation, Transformation AuxTransformation, int SectionIndex, double TrackPosition, double Brightness)
        {
            int a = AnimatedWorldObjectsUsed;
            if (a >= AnimatedWorldObjects.Length)
            {
                Array.Resize<AnimatedWorldObject>(ref AnimatedWorldObjects, AnimatedWorldObjects.Length << 1);
            }
            Transformation FinalTransformation = new Transformation(BaseTransformation, AuxTransformation);
            AnimatedWorldObjects[a] = new AnimatedWorldObject();
            AnimatedWorldObjects[a].Position = Position;
            AnimatedWorldObjects[a].Direction = FinalTransformation.Z;
            AnimatedWorldObjects[a].Up = FinalTransformation.Y;
            AnimatedWorldObjects[a].Side = FinalTransformation.X;
            AnimatedWorldObjects[a].Object = Prototype.Clone();
            AnimatedWorldObjects[a].Object.ObjectIndex = CreateDynamicObject();
            AnimatedWorldObjects[a].SectionIndex = SectionIndex;
            AnimatedWorldObjects[a].TrackPosition = TrackPosition;
            for (int i = 0; i < AnimatedWorldObjects[a].Object.States.Length; i++)
            {
                if (AnimatedWorldObjects[a].Object.States[i].Object == null)
                {
                    AnimatedWorldObjects[a].Object.States[i].Object = new StaticObject(Program.CurrentHost);
                    AnimatedWorldObjects[a].Object.States[i].Object.RendererIndex = -1;
                }
            }
            double r = 0.0;
            for (int i = 0; i < AnimatedWorldObjects[a].Object.States.Length; i++)
            {
                for (int j = 0; j < AnimatedWorldObjects[a].Object.States[i].Object.Mesh.Materials.Length; j++)
                {
                    AnimatedWorldObjects[a].Object.States[i].Object.Mesh.Materials[j].Color.R = (byte)Math.Round((double)Prototype.States[i].Object.Mesh.Materials[j].Color.R * Brightness);
                    AnimatedWorldObjects[a].Object.States[i].Object.Mesh.Materials[j].Color.G = (byte)Math.Round((double)Prototype.States[i].Object.Mesh.Materials[j].Color.G * Brightness);
                    AnimatedWorldObjects[a].Object.States[i].Object.Mesh.Materials[j].Color.B = (byte)Math.Round((double)Prototype.States[i].Object.Mesh.Materials[j].Color.B * Brightness);
                }
                for (int j = 0; j < AnimatedWorldObjects[a].Object.States[i].Object.Mesh.Vertices.Length; j++)
                {
                    double t = Prototype.States[i].Object.Mesh.Vertices[j].Coordinates.NormSquared();
                    if (t > r) r = t;
                }
            }
            AnimatedWorldObjects[a].Radius = Math.Sqrt(r);
            AnimatedWorldObjects[a].Visible = false;
            InitializeAnimatedObject(ref AnimatedWorldObjects[a].Object, 0, false, false);
            AnimatedWorldObjectsUsed++;
            return a;
        }
        internal static void UpdateAnimatedWorldObjects(double TimeElapsed, bool ForceUpdate)
        {
            for (int i = 0; i < AnimatedWorldObjectsUsed; i++)
            {
                const double extraRadius = 10.0;
                double z = AnimatedWorldObjects[i].Object.TranslateZFunction == null ? 0.0 : AnimatedWorldObjects[i].Object.TranslateZFunction.LastResult;
                double pa = AnimatedWorldObjects[i].TrackPosition + z - AnimatedWorldObjects[i].Radius - extraRadius;
                double pb = AnimatedWorldObjects[i].TrackPosition + z + AnimatedWorldObjects[i].Radius + extraRadius;
                double ta = World.CameraTrackFollower.TrackPosition - World.BackgroundImageDistance - World.ExtraViewingDistance;
                double tb = World.CameraTrackFollower.TrackPosition + World.BackgroundImageDistance + World.ExtraViewingDistance;
                bool visible = pb >= ta & pa <= tb;
                if (visible | ForceUpdate)
                {
                    if (AnimatedWorldObjects[i].Object.SecondsSinceLastUpdate >= AnimatedWorldObjects[i].Object.RefreshRate | ForceUpdate)
                    {
                        double timeDelta = AnimatedWorldObjects[i].Object.SecondsSinceLastUpdate + TimeElapsed;
                        AnimatedWorldObjects[i].Object.SecondsSinceLastUpdate = 0.0;
                        TrainManager.Train train = null;
                        double trainDistance = double.MaxValue;
                        for (int j = 0; j < TrainManager.Trains.Length; j++)
                        {
                            if (TrainManager.Trains[j].State == TrainState.Available)
                            {
                                double distance;
                                if (TrainManager.Trains[j].Cars[0].FrontAxle.Follower.TrackPosition < AnimatedWorldObjects[i].TrackPosition)
                                {
                                    distance = AnimatedWorldObjects[i].TrackPosition - TrainManager.Trains[j].Cars[0].FrontAxle.Follower.TrackPosition;
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
                        UpdateAnimatedObject(ref AnimatedWorldObjects[i].Object, false, train, train == null ? 0 : train.DriverCar, AnimatedWorldObjects[i].SectionIndex, AnimatedWorldObjects[i].TrackPosition, AnimatedWorldObjects[i].Position, AnimatedWorldObjects[i].Direction, AnimatedWorldObjects[i].Up, AnimatedWorldObjects[i].Side, false, true, true, timeDelta);
                    }
                    else
                    {
                        AnimatedWorldObjects[i].Object.SecondsSinceLastUpdate += TimeElapsed;
                    }
                    if (!AnimatedWorldObjects[i].Visible)
                    {
                        Renderer.ShowObject(AnimatedWorldObjects[i].Object.ObjectIndex, ObjectType.Dynamic);
                        AnimatedWorldObjects[i].Visible = true;
                    }
                }
                else
                {
                    AnimatedWorldObjects[i].Object.SecondsSinceLastUpdate += TimeElapsed;
                    if (AnimatedWorldObjects[i].Visible)
                    {
                        Renderer.HideObject(AnimatedWorldObjects[i].Object.ObjectIndex);
                        AnimatedWorldObjects[i].Visible = false;
                    }
                }
            }
        }

        // load object
		internal static UnifiedObject LoadObject(string FileName, Encoding Encoding, bool PreserveVertices, bool ForceTextureRepeatX, bool ForceTextureRepeatY)
		{
			return LoadObject(FileName, Encoding, PreserveVertices, ForceTextureRepeatX, ForceTextureRepeatY, new Vector3());
		}

		internal static UnifiedObject LoadObject(string FileName, Encoding Encoding, bool PreserveVertices, bool ForceTextureRepeatX, bool ForceTextureRepeatY, Vector3 Rotation)
        {
			if (FileName == null)
			{
				return null;
			}
#if !DEBUG
			try {
#endif
            if (!System.IO.Path.HasExtension(FileName))
            {
                while (true)
                {
                    string f;
                    f = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), System.IO.Path.GetFileName(FileName) + ".x");
                    if (System.IO.File.Exists(f))
                    {
                        FileName = f;
                        break;
                    }
                    f = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), System.IO.Path.GetFileName(FileName) + ".csv");
                    if (System.IO.File.Exists(f))
                    {
                        FileName = f;
                        break;
                    }
                    f = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), System.IO.Path.GetFileName(FileName) + ".b3d");
                    if (System.IO.File.Exists(f))
                    {
                        FileName = f;
                        break;
                    }
                    break;
                }
            }
            UnifiedObject Result;
            string e = System.IO.Path.GetExtension(FileName);
            if (e == null)
            {
	            Interface.AddMessage(MessageType.Error, false, "The file " + FileName + " does not have a recognised extension.");
	            return null;
            }
            switch (e.ToLowerInvariant())
            {
                case ".csv":
                case ".b3d":
                case ".x":
                case ".obj":
	                Program.CurrentHost.LoadObject(FileName, Encoding, out Result);
                    break;
                case ".animated":
                    Result = AnimatedObjectParser.ReadObject(FileName, Encoding);
                    break;
                case ".l3dobj":
                    Result = Ls3DObjectParser.ReadObject(FileName, Rotation);
                    break;
                case ".l3dgrp":
                    Result = Ls3DGrpParser.ReadObject(FileName, Encoding, Rotation);
                    break;
				case ".s":
					Result = MsTsShapeParser.ReadObject(FileName);
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
        internal static StaticObject LoadStaticObject(string FileName, Encoding Encoding, bool PreserveVertices, bool ForceTextureRepeatX, bool ForceTextureRepeatY)
        {
#if !DEBUG
			try {
#endif
            if (!System.IO.Path.HasExtension(FileName))
            {
                while (true)
                {
                    string f;
                    f = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), System.IO.Path.GetFileName(FileName) + ".x");
                    if (System.IO.File.Exists(f))
                    {
                        FileName = f;
                        break;
                    }
                    f = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), System.IO.Path.GetFileName(FileName) + ".csv");
                    if (System.IO.File.Exists(f))
                    {
                        FileName = f;
                        break;
                    }
                    f = OpenBveApi.Path.CombineFile(System.IO.Path.GetDirectoryName(FileName), System.IO.Path.GetFileName(FileName) + ".b3d");
                    if (System.IO.File.Exists(f))
                    {
                        FileName = f;
                    }
                    break;
                }
            }
            StaticObject Result;
            string e = System.IO.Path.GetExtension(FileName);
            if (e == null)
            {
	            Interface.AddMessage(MessageType.Error, false, "The file " + FileName + " does not have a recognised extension.");
	            return null;
            }
            UnifiedObject obj;
            switch (e.ToLowerInvariant())
            {
                case ".csv":
                case ".b3d":
                case ".x":
                case ".obj":
	                Program.CurrentHost.LoadObject(FileName, Encoding, out obj);
	                Result = (StaticObject)obj;
                    break;
                case ".l3dobj":
                    Result = Ls3DObjectParser.ReadObject(FileName, new Vector3());
                    if (Result == null)
                    {
                        return null;
                    }
                    break;
                case ".animated":
                    Interface.AddMessage(MessageType.Error, false, "Tried to load an animated object even though only static objects are allowed: " + FileName);
                    return null;
				default:
                    Interface.AddMessage(MessageType.Error, false, "The file extension is not supported: " + FileName);
                    return null;
            }
            Result.OptimizeObject(PreserveVertices, Interface.CurrentOptions.ObjectOptimizationBasicThreshold, false);
            return Result;
#if !DEBUG
			} catch (Exception ex) {
				Interface.AddMessage(MessageType.Error, true, "An unexpected error occured (" + ex.Message + ") while attempting to load the file " + FileName);
				return null;
			}
#endif
        }
        
        // create object
        internal static void CreateObject(UnifiedObject Prototype, Vector3 Position, Transformation BaseTransformation, Transformation AuxTransformation, bool AccurateObjectDisposal, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition)
        {
		if (Prototype != null)
		{
			CreateObject(Prototype, Position, BaseTransformation, AuxTransformation, -1, AccurateObjectDisposal, StartingDistance, EndingDistance, BlockLength, TrackPosition, 1.0, false);
		}
		else
		{
			int a = ObjectsUsed;
			if (a >= Objects.Length)
			{
				Array.Resize<StaticObject>(ref Objects, Objects.Length << 1);
			}
			ObjectsUsed++;
		}
        }
        internal static void CreateObject(UnifiedObject Prototype, Vector3 Position, Transformation BaseTransformation, Transformation AuxTransformation, int SectionIndex, bool AccurateObjectDisposal, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition, double Brightness, bool DuplicateMaterials)
        {
            if (Prototype is StaticObject)
            {
                StaticObject s = (StaticObject)Prototype;
                CreateStaticObject(s, Position, BaseTransformation, AuxTransformation, AccurateObjectDisposal, 0.0, StartingDistance, EndingDistance, BlockLength, TrackPosition, Brightness);
            }
            else if (Prototype is AnimatedObjectCollection)
            {
                AnimatedObjectCollection a = (AnimatedObjectCollection)Prototype;
                CreateAnimatedWorldObjects(a.Objects, Position, BaseTransformation, AuxTransformation, SectionIndex, AccurateObjectDisposal, StartingDistance, EndingDistance, BlockLength, TrackPosition, Brightness, DuplicateMaterials);
            }
        }

        internal static int CreateStaticObject(StaticObject Prototype, Vector3 Position, Transformation BaseTransformation, Transformation AuxTransformation, bool AccurateObjectDisposal, double AccurateObjectDisposalZOffset, double StartingDistance, double EndingDistance, double BlockLength, double TrackPosition, double Brightness)
        {
            int a = ObjectsUsed;
            if (a >= Objects.Length)
            {
                Array.Resize<StaticObject>(ref Objects, Objects.Length << 1);
            }

            Objects[a] = new StaticObject(Program.CurrentHost);
            Objects[a].ApplyData(Prototype, Position, BaseTransformation, AuxTransformation, AccurateObjectDisposal, AccurateObjectDisposalZOffset, StartingDistance, EndingDistance, BlockLength, TrackPosition, Brightness, 600);
            for (int i = 0; i < Prototype.Mesh.Faces.Length; i++)
            {
                switch (Prototype.Mesh.Faces[i].Flags & MeshFace.FaceTypeMask)
                {
                    case MeshFace.FaceTypeTriangles:
                        Game.InfoTotalTriangles++;
                        break;
                    case MeshFace.FaceTypeTriangleStrip:
                        Game.InfoTotalTriangleStrip++;
                        break;
                    case MeshFace.FaceTypeQuads:
                        Game.InfoTotalQuads++;
                        break;
                    case MeshFace.FaceTypeQuadStrip:
                        Game.InfoTotalQuadStrip++;
                        break;
                    case MeshFace.FaceTypePolygon:
                        Game.InfoTotalPolygon++;
                        break;
                }
            }
            ObjectsUsed++;
            return a;
        }
        
        // create dynamic object
        internal static int CreateDynamicObject()
        {
            int a = ObjectsUsed;
            if (a >= Objects.Length)
            {
                Array.Resize<StaticObject>(ref Objects, Objects.Length << 1);
            }
            Objects[a] = new StaticObject(Program.CurrentHost);
            Objects[a].Dynamic = true;
            ObjectsUsed++;
            return a;
        }

        // finish creating objects
        internal static void FinishCreatingObjects()
        {
            Array.Resize<StaticObject>(ref Objects, ObjectsUsed);
            Array.Resize<AnimatedWorldObject>(ref AnimatedWorldObjects, AnimatedWorldObjectsUsed);
        }

        // initialize visibility
        internal static void InitializeVisibility()
        {
            // sort objects
            ObjectsSortedByStart = new int[ObjectsUsed];
            ObjectsSortedByEnd = new int[ObjectsUsed];
            double[] a = new double[ObjectsUsed];
            double[] b = new double[ObjectsUsed];
            int n = 0;
            for (int i = 0; i < ObjectsUsed; i++)
            {
                if (Objects[i] != null && !Objects[i].Dynamic)
                {
                    ObjectsSortedByStart[n] = i;
                    ObjectsSortedByEnd[n] = i;
                    a[n] = Objects[i].StartingDistance;
                    b[n] = Objects[i].EndingDistance;
                    n++;
                }
            }
            Array.Resize<int>(ref ObjectsSortedByStart, n);
            Array.Resize<int>(ref ObjectsSortedByEnd, n);
            Array.Resize<double>(ref a, n);
            Array.Resize<double>(ref b, n);
            Array.Sort<double, int>(a, ObjectsSortedByStart);
            Array.Sort<double, int>(b, ObjectsSortedByEnd);
            ObjectsSortedByStartPointer = 0;
            ObjectsSortedByEndPointer = 0;
            // initial visiblity
            double p = World.CameraTrackFollower.TrackPosition + World.CameraCurrentAlignment.Position.Z;
            for (int i = 0; i < ObjectsUsed; i++)
            {
                if (Objects[i] != null && !Objects[i].Dynamic)
                {
                    if (Objects[i].StartingDistance <= p + World.ForwardViewingDistance & Objects[i].EndingDistance >= p - World.BackwardViewingDistance)
                    {
                        Renderer.ShowObject(i, ObjectType.Static);
                    }
                }
            }
        }

        // update visibility
        internal static void UpdateVisibility(double TrackPosition, bool ViewingDistanceChanged)
        {
            if (ViewingDistanceChanged)
            {
                UpdateVisibility(TrackPosition);
                UpdateVisibility(TrackPosition - 0.001);
                UpdateVisibility(TrackPosition + 0.001);
                UpdateVisibility(TrackPosition);
            }
            else
            {
                UpdateVisibility(TrackPosition);
            }
        }
        internal static void UpdateVisibility(double TrackPosition)
        {
            double d = TrackPosition - LastUpdatedTrackPosition;
            int n = ObjectsSortedByStart.Length;
            double p = World.CameraTrackFollower.TrackPosition + World.CameraCurrentAlignment.Position.Z;
            if (d < 0.0)
            {
                if (ObjectsSortedByStartPointer >= n) ObjectsSortedByStartPointer = n - 1;
                if (ObjectsSortedByEndPointer >= n) ObjectsSortedByEndPointer = n - 1;
                // dispose
                while (ObjectsSortedByStartPointer >= 0)
                {
                    int o = ObjectsSortedByStart[ObjectsSortedByStartPointer];
                    if (Objects[o].StartingDistance > p + World.ForwardViewingDistance)
                    {
                        Renderer.HideObject(o);
                        ObjectsSortedByStartPointer--;
                    }
                    else
                    {
                        break;
                    }
                }
                // introduce
                while (ObjectsSortedByEndPointer >= 0)
                {
                    int o = ObjectsSortedByEnd[ObjectsSortedByEndPointer];
                    if (Objects[o].EndingDistance >= p - World.BackwardViewingDistance)
                    {
                        if (Objects[o].StartingDistance <= p + World.ForwardViewingDistance)
                        {
                            Renderer.ShowObject(o, ObjectType.Static);
                        }
                        ObjectsSortedByEndPointer--;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else if (d > 0.0)
            {
                if (ObjectsSortedByStartPointer < 0) ObjectsSortedByStartPointer = 0;
                if (ObjectsSortedByEndPointer < 0) ObjectsSortedByEndPointer = 0;
                // dispose
                while (ObjectsSortedByEndPointer < n)
                {
                    int o = ObjectsSortedByEnd[ObjectsSortedByEndPointer];
                    if (Objects[o].EndingDistance < p - World.BackwardViewingDistance)
                    {
                        Renderer.HideObject(o);
                        ObjectsSortedByEndPointer++;
                    }
                    else
                    {
                        break;
                    }
                }
                // introduce
                while (ObjectsSortedByStartPointer < n)
                {
                    int o = ObjectsSortedByStart[ObjectsSortedByStartPointer];
                    if (Objects[o].StartingDistance <= p + World.ForwardViewingDistance)
                    {
                        if (Objects[o].EndingDistance >= p - World.BackwardViewingDistance)
                        {
                            Renderer.ShowObject(o, ObjectType.Static);
                        }
                        ObjectsSortedByStartPointer++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            LastUpdatedTrackPosition = TrackPosition;
        }

    }
}
