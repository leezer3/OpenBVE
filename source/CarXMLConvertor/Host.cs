using OpenBveApi.Hosts;
using OpenBveApi.Trains;

namespace CarXmlConvertor
{
	/// <summary>Represents the host application.</summary>
	internal class Host : HostInterface
	{
		public Host() : base(HostApplication.CarXMLConvertor) { }

		public override AbstractTrain ParseTrackFollowingObject(string objectPath, string tfoFile)
		{
			throw new System.NotImplementedException();
		}
	}
}
