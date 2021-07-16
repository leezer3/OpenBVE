using System;
using OpenBveApi.Routes;
using RouteManager2.Events;
using RouteManager2.MessageManager;

namespace CsvRwRouteParser
{
	internal class Marker
	{
		/// <summary>The track position at which the marker will be displayed</summary>
		internal readonly double StartingPosition;
		/// <summary>The track position at which the marker will be removed</summary>
		internal readonly double EndingPosition;
		/// <summary>The abstract message container</summary>
		/// <remarks>May either contain a marker image or marker text</remarks>
		internal readonly AbstractMessage Message;

		internal Marker(double startingPosition, double endingPosition, AbstractMessage message)
		{
			StartingPosition = startingPosition;
			EndingPosition = endingPosition;
			Message = message;
		}

		internal void CreateEvent(double StartingDistance, double EndingDistance, ref TrackElement Element)
		{
			if (StartingPosition >= StartingDistance & StartingPosition < EndingDistance)
			{
				int m = Element.Events.Length;
				Array.Resize(ref Element.Events, m + 1);
				double d = StartingPosition - StartingDistance;
				if (Message != null)
				{
					Element.Events[m] = new MarkerStartEvent(Plugin.CurrentHost, d, Message);
				}
			}
			if (EndingPosition >= StartingDistance & EndingPosition < EndingDistance)
			{
				int m = Element.Events.Length;
				Array.Resize(ref Element.Events, m + 1);
				double d = EndingPosition - StartingDistance;
				if (Message != null)
				{
					Element.Events[m] = new MarkerEndEvent(Plugin.CurrentHost, d, Message);
				}
			}
		}
	}
}
