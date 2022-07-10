using OpenBveApi.Hosts;
using OpenBveApi.Math;
using OpenBveApi.Objects;
using OpenBveApi.Trains;

namespace LibRender2.Trains
{
	class HierarchyElementsGroup : ElementsGroup
	{

		internal HierarchyAnimatedObject Object;

		public HierarchyElementsGroup(HostInterface host, BaseRenderer renderer) : base(host, renderer)
		{
		}

		public HierarchyElementsGroup(HostInterface host, BaseRenderer renderer, HierarchyAnimatedObject hierarchyObject) : base(host, renderer)
		{
			Object = hierarchyObject;
		}

		public override void Initialize(bool CurrentlyVisible, ObjectType Type)
		{
			if (CurrentlyVisible)
			{
				for (int i = 0; i < Object.Objects.Length; i++)
				{
					currentHost.ShowObject(Object.Objects[i].State, Type);
				}
			}
		}

		public override void Show(ObjectType Type)
		{
			for (int i = 0; i < Object.Objects.Length; i++)
			{
				currentHost.ShowObject(Object.Objects[i].State, Type);
			}
		}

		public override void Hide()
		{
			for (int i = 0; i < Object.Objects.Length; i++)
			{
				currentHost.HideObject(Object.Objects[i].State);
			}
		}

		public override void Reverse()
		{
			
		}

		public override void Update(AbstractTrain Train, int CarIndex, double TrackPosition, byte Brightness, Vector3 Position, Vector3 Direction, Vector3 Up, Vector3 Side, bool UpdateFunctions, bool Show, double TimeElapsed, bool EnableDamping, bool IsTouch = false, dynamic Camera = null)
		{
			Object.Update(true, Train, CarIndex, -1, TrackPosition, Position, Direction, Up, Side, UpdateFunctions, Show, TimeElapsed, EnableDamping);
		}
	}
}
