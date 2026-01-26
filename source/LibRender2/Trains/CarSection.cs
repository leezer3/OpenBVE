using OpenBveApi.Hosts;
using OpenBveApi.Objects;
using OpenBveApi.Trains;
using System;
using OpenBveApi.Math;
using OpenBveApi.World;

namespace LibRender2.Trains
{
	/// <summary>An animated object attached to a car (Exterior, cab etc.)</summary>
	public class CarSection
	{
		/// <summary>Holds a reference to the current host</summary>
		private readonly HostInterface currentHost;
		/// <summary>The groups of animated objects</summary>
		public ElementsGroup[] Groups;
		/// <summary>The current additional group (touch etc.)</summary>
		public int CurrentAdditionalGroup;
		/// <summary>Whether this is visible from internal views</summary>
		public readonly bool VisibleFromInterior;
		/// <summary>Whether this is to be shown in overlay mode (e.g. panel)</summary>
		public readonly ObjectType Type;
		/// <summary>If an interior view, the transformation to be used</summary>
		/// <remarks>Allows rotation of a 2D panel etc.</remarks>
		public Transformation ViewDirection;

		/// <summary>Creates a new CarSection</summary>
		/// <param name="Host">The host</param>
		/// <param name="ObjectType">The object type</param>
		/// <param name="visibleFromInterior">Whether the object is visible from the interior</param>
		/// <param name="baseCar">The base car</param>
		/// <param name="Object">The object</param>
		public CarSection(HostInterface Host, ObjectType ObjectType, bool visibleFromInterior, AbstractCar baseCar = null, UnifiedObject Object = null)
		{
			currentHost = Host;
			Groups = new ElementsGroup[1];
			Groups[0] = new ElementsGroup();
			VisibleFromInterior = visibleFromInterior;
			if (Object is StaticObject s)
			{
				Groups[0].Elements = new AnimatedObject[1];
				Groups[0].Elements[0] = new AnimatedObject(Host)
				{
					States = new[] {new ObjectState(s)},
					CurrentState = 0,
					IsPartOfTrain = true
				};
				currentHost.CreateDynamicObject(ref Groups[0].Elements[0].internalObject);
			}
			else if (Object is AnimatedObjectCollection a)
			{
				Groups[0].Elements = new AnimatedObject[a.Objects.Length];
				for (int h = 0; h < a.Objects.Length; h++)
				{
					Groups[0].Elements[h] = a.Objects[h].Clone();
					Groups[0].Elements[h].IsPartOfTrain = true;
					currentHost.CreateDynamicObject(ref Groups[0].Elements[h].internalObject);
				}
			}
			else if (Object is KeyframeAnimatedObject k)
			{
				k.BaseCar = baseCar;
				Groups[0].Keyframes = k;
				for (int h = 0; h < Groups[0].Keyframes.Objects.Length; h++)
				{
					currentHost.CreateDynamicObject(ref Groups[0].Keyframes.Objects[h]);
				}
			}
			Type = ObjectType;
		}

		public void CorrectCarIndices(int offset)
		{
			foreach (ElementsGroup e in Groups)
			{
				foreach (AnimatedObject a in e.Elements)
				{
					a.CorrectCarIndices(offset);
				}
			}
		}

		/// <summary>Appends an object to the CarSection</summary>
		/// <param name="Host">The host</param>
		/// <param name="objectPosition">The relative position of the object to add</param>
		/// <param name="baseCar">The base car</param>
		/// <param name="Object">The object</param>
		public void AppendObject(HostInterface Host, Vector3 objectPosition, AbstractCar baseCar = null, UnifiedObject Object = null)
		{
			int gl = Groups.Length;
			Array.Resize(ref Groups, gl + 1);
			Groups[gl] = new ElementsGroup();
			if (Object is StaticObject s)
			{
				Groups[gl].Elements = new AnimatedObject[1];
				Groups[gl].Elements[0] = new AnimatedObject(Host)
				{
					States = new[] { new ObjectState(s) },
					CurrentState = 0,
					IsPartOfTrain = true
				};
				currentHost.CreateDynamicObject(ref Groups[gl].Elements[0].internalObject);
			}
			else if (Object is AnimatedObjectCollection a)
			{
				Groups[gl].Elements = new AnimatedObject[a.Objects.Length];
				for (int h = 0; h < a.Objects.Length; h++)
				{
					Groups[gl].Elements[h] = a.Objects[h].Clone();
					Groups[gl].Elements[h].IsPartOfTrain = true;
					currentHost.CreateDynamicObject(ref Groups[gl].Elements[h].internalObject);
				}
			}
			else if (Object is KeyframeAnimatedObject k)
			{
				for (int i = 0; i < k.Objects.Length; i++)
				{
					k.Objects[i].Prototype = (StaticObject)k.Objects[i].Prototype.Clone();
					k.ApplyTranslation(objectPosition.X, objectPosition.Y, objectPosition.Z, true);
				}
				k.BaseCar = baseCar;
				Groups[gl].Keyframes = k;
				for (int h = 0; h < Groups[gl].Keyframes.Objects.Length; h++)
				{
					currentHost.CreateDynamicObject(ref Groups[gl].Keyframes.Objects[h]);
				}
			}

			CurrentAdditionalGroup = gl -1;
		}

		/// <summary>Initalizes the CarSection</summary>
		/// <param name="CurrentlyVisible">Whether visible at the time of this call</param>
		public void Initialize(bool CurrentlyVisible)
		{
			for (int i = 0; i < Groups.Length; i++)
			{
				Groups[i].Initialize(CurrentlyVisible, Type);
			}
		}

		/// <summary>Shows all objects associated with the car section</summary>
		public void Show()
		{
			if (Groups.Length > 0)
			{
				for (int i = 0; i < Groups[0].Elements.Length; i++)
				{
					currentHost.ShowObject(Groups[0].Elements[i].internalObject, Type);
				}

				if (Groups[0].Keyframes != null)
				{
					for (int i = 0; i < Groups[0].Keyframes.Objects.Length; i++)
					{
						currentHost.ShowObject(Groups[0].Keyframes.Objects[i], Type);
					}
				}
			}

			int add = CurrentAdditionalGroup + 1;
			if (add < Groups.Length)
			{
				for (int i = 0; i < Groups[add].Elements.Length; i++)
				{
					currentHost.ShowObject(Groups[add].Elements[i].internalObject, Type);
					
				}

				if (Groups[add].Keyframes != null)
				{
					for (int i = 0; i < Groups[add].Keyframes.Objects.Length; i++)
					{
						currentHost.ShowObject(Groups[add].Keyframes.Objects[i], Type);
					}
				}
			}
		}
	}
}
