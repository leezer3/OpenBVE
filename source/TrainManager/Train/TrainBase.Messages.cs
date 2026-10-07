using LibRender2.Screens;
using LibRender2.Trains;
using OpenBveApi;
using OpenBveApi.Colors;
using OpenBveApi.Hosts;
using OpenBveApi.Interface;
using OpenBveApi.Math;
using OpenBveApi.Motor;
using OpenBveApi.Routes;
using OpenBveApi.Runtime;
using OpenBveApi.Trains;
using RouteManager2.MessageManager;
using RouteManager2.SignalManager;
using RouteManager2.Stations;
using SoundManager;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TrainManager.BrakeSystems;
using TrainManager.Car;
using TrainManager.Handles;
using TrainManager.SafetySystems;
using SafetySystem = TrainManager.SafetySystems.SafetySystem;

namespace TrainManager.Trains
{
	/*
	 * TEMPORARY NAME AND CLASS TO ALLOW FOR MOVE IN PARTS
	 */
	public partial class TrainBase : AbstractTrain
	{
		/// <inheritdoc/>
		public override void SectionChange()
		{
			if (CurrentSectionLimit == 0.0 && TrainManagerBase.currentHost.SimulationState != SimulationState.MinimalisticSimulation)
			{
				TrainManagerBase.currentHost.AddMessage(Translations.GetInterfaceString(HostApplication.OpenBve, new [] {"message","signal_stop"}), MessageDependency.PassedRedSignal, GameMode.Normal, MessageColor.Red, double.PositiveInfinity, null);
			}
			else if (CurrentSpeed > CurrentSectionLimit)
			{
				TrainManagerBase.currentHost.AddMessage(Translations.GetInterfaceString(HostApplication.OpenBve, new [] {"message","signal_overspeed"}), MessageDependency.SectionLimit, GameMode.Normal, MessageColor.Orange, double.PositiveInfinity, null);
			}
		}

		public void ContactSignaller()
		{
			Section sct = TrainManagerBase.CurrentRoute.Sections[CurrentSectionIndex].NextSection;
			if (sct.Type != SectionType.PermissiveValueBased && sct.Type != SectionType.PermissiveIndexBased)
			{
				// not a valid section to access
				string s = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "message", "signal_access_invalid" });
				TrainManagerBase.currentHost.AddMessage(s, MessageDependency.None, GameMode.Expert, MessageColor.White, 10.0, null);
			}
			else
			{
				if (sct.IsFree())
				{
					// section is free of trains, so can be permissively accessed
					string s = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "message", "signal_access_granted" });
					TrainManagerBase.currentHost.AddMessage(s, MessageDependency.None, GameMode.Expert, MessageColor.White, 10.0, null);
					sct.SignallerPermission = true;
				}
				else
				{
					// not free, access denied
					string s = Translations.GetInterfaceString(HostApplication.OpenBve, new[] { "message", "signal_access_denied" });
					TrainManagerBase.currentHost.AddMessage(s, MessageDependency.None, GameMode.Expert, MessageColor.Red, 10.0, null);
					sct.SignallerPermission = false;
				}
			}
		}
	}
}
