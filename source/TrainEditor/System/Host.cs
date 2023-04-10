using System;
using OpenBveApi.Hosts;
using OpenBveApi.Trains;


namespace TrainEditor
{
	/// <summary>Represents the host application.</summary>
	internal class Host : HostInterface
	{
		public Host() : base(HostApplication.TrainEditor) { }
		public override AbstractTrain ParseTrackFollowingObject(string objectPath, string tfoFile)
		{
			throw new NotImplementedException();
		}
	}
}
