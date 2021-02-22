
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using LibRender2.Trains;
using OpenBve.Parsers.Panel;
using OpenBveApi.Interface;
using OpenBveApi.Trains;
using OpenBveApi.Graphics;
using OpenBveApi.Objects;
using TrainManager.Trains;

namespace OpenBve
{
	/// <summary>The TrainManager is the root class containing functions to load and manage trains within the simulation world.</summary>
	public partial class TrainManager
	{
		/// <summary>The root class for a train within the simulation</summary>
		public partial class Train : TrainBase
		{
			
			public Train(TrainState state) : base(state)
			{
			}

			

		}
	}
}
