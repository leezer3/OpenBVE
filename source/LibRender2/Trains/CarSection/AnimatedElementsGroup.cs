using OpenBveApi.Hosts;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Trains;

namespace LibRender2.Trains
{
	/// <summary>A group of elements</summary>
	public class AnimatedElementsGroup : ElementsGroup
	{
		/// <summary>The animated objects</summary>
		public AnimatedObject[] Elements;
		/// <summary>The touch elements if applicable</summary>
		public TouchElement[] TouchElements;

		private readonly BaseRenderer Renderer;

		public AnimatedElementsGroup(HostInterface Host, BaseRenderer renderer) : base(Host)
		{
			Elements = new AnimatedObject[] { };
			Renderer = renderer;
		}

		public AnimatedElementsGroup(HostInterface Host, BaseRenderer renderer, StaticObject Object) : base(Host)
		{
			Elements = new AnimatedObject[1];
			Elements[0] = new AnimatedObject(Host)
			{
				States = new[] { new ObjectState(Object) },
				CurrentState = 0,
				IsPartOfTrain = true
			};
			Renderer = renderer;
			currentHost.CreateDynamicObject(ref Elements[0].internalObject);
		}

		public AnimatedElementsGroup(HostInterface Host, BaseRenderer renderer, AnimatedObjectCollection ObjectCollection) : base(Host)
		{
			Elements = new AnimatedObject[ObjectCollection.Objects.Length];
			Renderer = renderer;
			for (int h = 0; h < ObjectCollection.Objects.Length; h++)
			{
				Elements[h] = ObjectCollection.Objects[h].Clone();
				Elements[h].IsPartOfTrain = true;
				currentHost.CreateDynamicObject(ref Elements[h].internalObject);
			}
		}

		public override void Initialize(bool CurrentlyVisible, ObjectType Type)
		{
			for (int i = 0; i < Elements.Length; i++)
			{
				for (int j = 0; j < Elements[i].States.Length; j++)
				{
					Elements[i].Initialize(j, Type, CurrentlyVisible);
				}
			}
		}

		public override void Show(ObjectType Type)
		{
			for (int i = 0; i < Elements.Length; i++)
			{
				currentHost.ShowObject(Elements[i].internalObject, Type);
			}
		}

		public override void Hide()
		{
			for (int i = 0; i < Elements.Length; i++)
			{
				currentHost.HideObject(Elements[i].internalObject);
			}
		}

		public override void Reverse()
		{
			for (int i = 0; i < Elements.Length; i++)
			{
				Elements[i].Reverse();
			}
		}

		public override void Update(AbstractTrain Train, int CarIndex, double TrackPosition, byte Brightness, Vector3 Position, Vector3 Direction, Vector3 Up, Vector3 Side, bool ForceUpdate, bool Show, double TimeElapsed, bool EnableDamping, bool IsTouch = false, dynamic Camera = null)
		{
			for (int i = 0; i < Elements.Length; i++)
			{
				double timeDelta;
				bool updatefunctions;

				if (Elements[i].RefreshRate != 0.0)
				{
					if (Elements[i].SecondsSinceLastUpdate >= Elements[i].RefreshRate)
					{
						timeDelta = Elements[i].SecondsSinceLastUpdate;
						Elements[i].SecondsSinceLastUpdate = TimeElapsed;
						updatefunctions = true;
					}
					else
					{
						timeDelta = TimeElapsed;
						Elements[i].SecondsSinceLastUpdate += TimeElapsed;
						updatefunctions = false;
					}
				}
				else
				{
					timeDelta = Elements[i].SecondsSinceLastUpdate;
					Elements[i].SecondsSinceLastUpdate = TimeElapsed;
					updatefunctions = true;
				}

				if (ForceUpdate)
				{
					updatefunctions = true;
				}
				Elements[i].Update(Train, CarIndex, TrackPosition, Position, Direction, Up, Side, updatefunctions, Show, timeDelta, EnableDamping, IsTouch, Camera);
				if (Elements[i].internalObject != null)
				{
					Elements[i].internalObject.DaytimeNighttimeBlend = Brightness;
				}
				if (!Renderer.ForceLegacyOpenGL && Elements[i].UpdateVAO)
				{
					VAOExtensions.CreateVAO(ref Elements[i].internalObject.Prototype.Mesh, true, Renderer.DefaultShader.VertexLayout, Renderer);
				}
			}

			if (TouchElements == null)
			{
				return;
			}
			for (int i = 0; i < TouchElements.Length; i++)
			{
				double timeDelta;
				bool updatefunctions;

				if (TouchElements[i].Element.RefreshRate != 0.0)
				{
					if (TouchElements[i].Element.SecondsSinceLastUpdate >= Elements[i].RefreshRate)
					{
						timeDelta = TouchElements[i].Element.SecondsSinceLastUpdate;
						TouchElements[i].Element.SecondsSinceLastUpdate = TimeElapsed;
						updatefunctions = true;
					}
					else
					{
						timeDelta = TimeElapsed;
						TouchElements[i].Element.SecondsSinceLastUpdate += TimeElapsed;
						updatefunctions = false;
					}
				}
				else
				{
					timeDelta = TouchElements[i].Element.SecondsSinceLastUpdate;
					TouchElements[i].Element.SecondsSinceLastUpdate = TimeElapsed;
					updatefunctions = true;
				}

				if (ForceUpdate)
				{
					updatefunctions = true;
				}
				TouchElements[i].Element.Update(Train, CarIndex, TrackPosition, Position, Direction, Up, Side, updatefunctions, Show, timeDelta, EnableDamping, IsTouch, Camera);
				if (!Renderer.ForceLegacyOpenGL && TouchElements[i].Element.UpdateVAO)
				{
					VAOExtensions.CreateVAO(ref TouchElements[i].Element.internalObject.Prototype.Mesh, true, Renderer.DefaultShader.VertexLayout, Renderer);
				}
			}
		}
	}
}
