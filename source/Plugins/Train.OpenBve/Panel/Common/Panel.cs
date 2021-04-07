using System;
using LibRender2.Trains;
using OpenBveApi.Colors;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Textures;
using TrainManager.Trains;

namespace Train.OpenBve
{
	internal class Panel
	{
		private readonly Plugin Plugin;
		/// <summary>The distance between animated layer stacks</summary>
		/// <remarks>Required to prevent openGL z-fighting</remarks>
		private const double StackDistance = 0.000001;
		/// <remarks>EyeDistance is required to be 1.0 by UpdateCarSectionElement and by UpdateCameraRestriction, thus cannot be easily changed.</remarks>
		internal const double EyeDistance = 1.0;
		/// <summary>The resolution of the panel image in px</summary>
		internal static double Resolution = 1024.0;
		/// <summary>The top left of the default shown area in px</summary>
		internal static Vector2 TopLeft = new Vector2(0,0);
		/// <summary>The bottom right of the default shown area in px</summary>
		internal static Vector2 BottomRight = new Vector2(1024, 1024);
		/// <summary>The center of the shown area in px</summary>
		internal static Vector2 Center = new Vector2(0, 512);
		/// <summary>The origin of the panel in px</summary>
		internal static Vector2 Origin = new Vector2(0, 512);

		internal Panel(Plugin plugin)
		{
			Plugin = plugin;
		}

		internal int CreateElement(ref ElementsGroup Group, double Left, double Top, Vector2 RelativeRotationCenter, double Distance, Vector3 Driver, Texture DaytimeTexture, Texture NighttimeTexture, bool AddStateToLastElement = false)
		{
			return CreateElement(ref Group, Left, Top, DaytimeTexture.Width, DaytimeTexture.Height, RelativeRotationCenter, Distance, Driver, DaytimeTexture, NighttimeTexture, Color32.White, AddStateToLastElement);
		}

		internal int CreateElement(ref ElementsGroup Group, double Left, double Top, double Width, double Height, Vector2 RelativeRotationCenter, double Distance, Vector3 Driver, Texture DaytimeTexture, Texture NighttimeTexture, Color32 Color, bool AddStateToLastElement = false)
		{
			if (Width == 0 || Height == 0)
			{
				Plugin.currentHost.AddMessage(MessageType.Error, false, "Attempted to create an invalid size element");
			}

			Distance *= StackDistance;
			double WorldWidth, WorldHeight;
			if (Plugin.Renderer.Screen.Width >= Plugin.Renderer.Screen.Height) {
				WorldWidth = 2.0 * Math.Tan(0.5 * Plugin.Renderer.Camera.HorizontalViewingAngle) * EyeDistance;
				WorldHeight = WorldWidth / Plugin.Renderer.Screen.AspectRatio;
			} else {
				WorldHeight = 2.0 * Math.Tan(0.5 * Plugin.Renderer.Camera.VerticalViewingAngle) * EyeDistance / Plugin.Renderer.Screen.AspectRatio;
				WorldWidth = WorldHeight * Plugin.Renderer.Screen.AspectRatio;
			}
			double x0 = Left / Resolution;
			double x1 = (Left + Width) / Resolution;
			double y0 = (BottomRight.Y - Top) / Resolution * Plugin.Renderer.Screen.AspectRatio;
			double y1 = (BottomRight.Y - (Top + Height)) / Resolution * Plugin.Renderer.Screen.AspectRatio;
			double xd = 0.5 - Center.X / Resolution;
			x0 += xd; x1 += xd;
			double yt = BottomRight.Y - Resolution / Plugin.Renderer.Screen.AspectRatio;
			double yd = (Center.Y - yt) / (BottomRight.Y - yt) - 0.5;
			y0 += yd; y1 += yd;
			x0 = (x0 - 0.5) * WorldWidth;
			x1 = (x1 - 0.5) * WorldWidth;
			y0 = (y0 - 0.5) * WorldHeight;
			y1 = (y1 - 0.5) * WorldHeight;
			double xm = x0 * (1.0 - RelativeRotationCenter.X) + x1 * RelativeRotationCenter.X;
			double ym = y0 * (1.0 - RelativeRotationCenter.Y) + y1 * RelativeRotationCenter.Y;
			Vector3[] v = new Vector3[4];
			v[0] = new Vector3(x0 - xm, y1 - ym, 0);
			v[1] = new Vector3(x0 - xm, y0 - ym, 0);
			v[2] = new Vector3(x1 - xm, y0 - ym, 0);
			v[3] = new Vector3(x1 - xm, y1 - ym, 0);
			Vertex t0 = new Vertex(v[0], new Vector2(0.0f, 1.0f));
			Vertex t1 = new Vertex(v[1], new Vector2(0.0f, 0.0f));
			Vertex t2 = new Vertex(v[2], new Vector2(1.0f, 0.0f));
			Vertex t3 = new Vertex(v[3], new Vector2(1.0f, 1.0f));
			StaticObject Object = new StaticObject(Plugin.currentHost);
			Object.Mesh.Vertices = new VertexTemplate[] { t0, t1, t2, t3 };
			Object.Mesh.Faces = new[] { new MeshFace(new[] { 0, 1, 2, 3 }) };
			Object.Mesh.Materials = new MeshMaterial[1];
			Object.Mesh.Materials[0].Flags = new MaterialFlags();
			if (DaytimeTexture != null)
			{
				Object.Mesh.Materials[0].Flags |= MaterialFlags.TransparentColor;

				if (NighttimeTexture != null)
				{
					// In BVE4 and versions of OpenBVE prior to v1.7.1.0, elements with NighttimeImage defined are rendered with lighting disabled.
					Object.Mesh.Materials[0].Flags |= MaterialFlags.DisableLighting;
				}
			}
			Object.Mesh.Materials[0].Color = Color;
			Object.Mesh.Materials[0].TransparentColor = Color24.Blue;
			Object.Mesh.Materials[0].DaytimeTexture = DaytimeTexture;
			Object.Mesh.Materials[0].NighttimeTexture = NighttimeTexture;
			Object.Dynamic = true;
			// calculate offset
			Vector3 o;
			o.X = xm + Driver.X;
			o.Y = ym + Driver.Y;
			o.Z = EyeDistance - Distance + Driver.Z;
			// add object
			if (AddStateToLastElement) {
				int n = Group.Elements.Length - 1;
				int j = Group.Elements[n].States.Length;
				Array.Resize(ref Group.Elements[n].States, j + 1);
				Group.Elements[n].States[j] = new ObjectState
				{
					Translation = Matrix4D.CreateTranslation(o.X, o.Y, -o.Z),
					Prototype = Object
				};
				return n;
			} else {
				int n = Group.Elements.Length;
				Array.Resize(ref Group.Elements, n + 1);
				Group.Elements[n] = new AnimatedObject(Plugin.currentHost);
				Group.Elements[n].States = new[] { new ObjectState() };
				Group.Elements[n].States[0].Translation = Matrix4D.CreateTranslation(o.X, o.Y, -o.Z);
				Group.Elements[n].States[0].Prototype = Object;
				Group.Elements[n].CurrentState = 0;
				Group.Elements[n].internalObject = new ObjectState { Prototype = Object };
				Plugin.currentHost.CreateDynamicObject(ref Group.Elements[n].internalObject);
				return n;
			}
		}

		internal void CreateTouchElement(ElementsGroup Group, Vector2 Location, Vector2 Size, int ScreenIndex, int[] SoundIndices, CommandEntry[] CommandEntries, Vector2 RelativeRotationCenter, double Distance, Vector3 Driver)
		{
			Distance *= StackDistance;
			double WorldWidth, WorldHeight;
			if (Plugin.Renderer.Screen.Width >= Plugin.Renderer.Screen.Height)
			{
				WorldWidth = 2.0 * Math.Tan(0.5 * Plugin.Renderer.Camera.HorizontalViewingAngle) * EyeDistance;
				WorldHeight = WorldWidth / Plugin.Renderer.Screen.AspectRatio;
			}
			else
			{
				WorldHeight = 2.0 * Math.Tan(0.5 * Plugin.Renderer.Camera.VerticalViewingAngle) * EyeDistance / Plugin.Renderer.Screen.AspectRatio;
				WorldWidth = WorldHeight * Plugin.Renderer.Screen.AspectRatio;
			}
			double x0 = Location.X / Resolution;
			double x1 = (Location.X + Size.X) / Resolution;
			double y0 = (BottomRight.Y - Location.Y) / Resolution * Plugin.Renderer.Screen.AspectRatio;
			double y1 = (BottomRight.Y - (Location.Y + Size.Y)) / Resolution * Plugin.Renderer.Screen.AspectRatio;
			double xd = 0.5 - Center.X / Resolution;
			x0 += xd;
			x1 += xd;
			double yt = BottomRight.Y - Resolution / Plugin.Renderer.Screen.AspectRatio;
			double yd = (Center.Y - yt) / (BottomRight.Y - yt) - 0.5;
			y0 += yd;
			y1 += yd;
			x0 = (x0 - 0.5) * WorldWidth;
			x1 = (x1 - 0.5) * WorldWidth;
			y0 = (y0 - 0.5) * WorldHeight;
			y1 = (y1 - 0.5) * WorldHeight;
			double xm = x0 * (1.0 - RelativeRotationCenter.X) + x1 * RelativeRotationCenter.X;
			double ym = y0 * (1.0 - RelativeRotationCenter.Y) + y1 * RelativeRotationCenter.Y;
			Vector3[] v = new Vector3[4];
			v[0] = new Vector3(x0 - xm, y1 - ym, 0);
			v[1] = new Vector3(x0 - xm, y0 - ym, 0);
			v[2] = new Vector3(x1 - xm, y0 - ym, 0);
			v[3] = new Vector3(x1 - xm, y1 - ym, 0);
			Vertex t0 = new Vertex(v[0], new Vector2(0.0f, 1.0f));
			Vertex t1 = new Vertex(v[1], new Vector2(0.0f, 0.0f));
			Vertex t2 = new Vertex(v[2], new Vector2(1.0f, 0.0f));
			Vertex t3 = new Vertex(v[3], new Vector2(1.0f, 1.0f));
			StaticObject Object = new StaticObject(Plugin.currentHost);
			Object.Mesh.Vertices = new VertexTemplate[] { t0, t1, t2, t3 };
			Object.Mesh.Faces = new[] { new MeshFace(new[] { 0, 1, 2, 3 }) };
			Object.Mesh.Materials = new MeshMaterial[1];
			Object.Mesh.Materials[0].Flags = 0;
			Object.Mesh.Materials[0].Color = Color32.White;
			Object.Mesh.Materials[0].TransparentColor = Color24.Blue;
			Object.Mesh.Materials[0].DaytimeTexture = null;
			Object.Mesh.Materials[0].NighttimeTexture = null;
			Object.Dynamic = true;
			// calculate offset
			Vector3 o;
			o.X = xm + Driver.X;
			o.Y = ym + Driver.Y;
			o.Z = EyeDistance - Distance + Driver.Z;
			if (Group.TouchElements == null)
			{
				Group.TouchElements = new TouchElement[0];
			}
			int n = Group.TouchElements.Length;
			Array.Resize(ref Group.TouchElements, n + 1);
			Group.TouchElements[n] = new TouchElement
			{
				Element = new AnimatedObject(Plugin.currentHost),
				JumpScreenIndex = ScreenIndex,
				SoundIndices = SoundIndices,
				ControlIndices = new int[CommandEntries.Length]
			};
			Group.TouchElements[n].Element.States = new[] { new ObjectState() };
			Group.TouchElements[n].Element.States[0].Translation = Matrix4D.CreateTranslation(o.X, o.Y, -o.Z);
			Group.TouchElements[n].Element.States[0].Prototype = Object;
			Group.TouchElements[n].Element.CurrentState = 0;
			Group.TouchElements[n].Element.internalObject = new ObjectState(Object);
			Plugin.currentHost.CreateDynamicObject(ref Group.TouchElements[n].Element.internalObject);
			int m = Plugin.CurrentControls.Length;
			Array.Resize(ref Plugin.CurrentControls, m + CommandEntries.Length);
			for (int i = 0; i < CommandEntries.Length; i++)
			{
				Plugin.CurrentControls[m + i].Command = CommandEntries[i].Command;
				Plugin.CurrentControls[m + i].Method = ControlMethod.Touch;
				Plugin.CurrentControls[m + i].Option = CommandEntries[i].Option;
				Group.TouchElements[n].ControlIndices[i] = m + i;
			}
		}
	}
}
