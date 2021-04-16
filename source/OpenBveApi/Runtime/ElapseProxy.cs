using System.Runtime.Serialization;

namespace OpenBveApi.Runtime
{
	/// <summary>Represents Elapse data passed to a plugin proxy</summary>
	[DataContract]
	public class ElapseProxy
	{
		/// <summary>The elapse data being passed</summary>
		[DataMember]
		public ElapseData Data;
		/// <summary>The panel array being passed</summary>
		[DataMember]
		public int[] Panel;
		/// <summary>The sound handle array being passed</summary>
		[DataMember]
		public int[] Sound;
		/// <summary>Creates a new ElapseProxy from an ElapseData</summary>
		public ElapseProxy(ElapseData data)
		{
			this.Data = data;
			Panel = new int[256];
			Sound = new int[256];
		}
	}
}
