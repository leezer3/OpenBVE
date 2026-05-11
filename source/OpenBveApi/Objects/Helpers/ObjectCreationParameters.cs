//Copyright (c) 2025, Christopher Lees, The OpenBVE Project
//
//Redistribution and use in source and binary forms, with or without
//modification, are permitted provided that the following conditions are met:
//
//1. Redistributions of source code must retain the above copyright notice, this
//   list of conditions and the following disclaimer.
//2. Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution.
//
//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
//ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
//WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
//DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
//ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
//ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
//(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace OpenBveApi.Objects
{
	/// <summary>Helper struct for parameters to be applied when creating an object</summary>
    public struct ObjectCreationParameters
    {
		/// <summary>The absolute track position at which the object is placed</summary>
	    public double TrackPosition;
		/// <summary>he Z-offset to be applied before calculating the position of the object for disposal</summary>
	    public double AccurateObjectDisposalZOffset;
		/// <summary>The track distance at which this object is displayed by the renderer</summary>
		/// <remarks>Not valid in QuadTree visibility mode</remarks>
	    public double StartingDistance;
	    /// <summary>The track distance at which this object is hidden by the renderer</summary>
	    /// <remarks>Not valid in QuadTree visibility mode</remarks>
		public double EndingDistance;
		/// <summary>The brightness value of this object</summary>
	    public double Brightness;
		/// <summary>Whether shadow casting is disabled for this object </summary>
		public bool DisableShadowCasting;
		/// <summary>The section index the object is linked to</summary>
		public int SectionIndex;

		/// <summary>Creates a new ObjectCreationParameters</summary>
	    public ObjectCreationParameters(double trackPosition, double accurateObjectDisposalZOffset, double startingDistance, double endingDistance, double brightness, int sectionIndex = -1, bool disableShadowCasting = false)
	    {
		    TrackPosition = trackPosition;
		    AccurateObjectDisposalZOffset = accurateObjectDisposalZOffset;
		    StartingDistance = startingDistance;
		    EndingDistance = endingDistance;
			Brightness = brightness;
			SectionIndex = sectionIndex;
			DisableShadowCasting = disableShadowCasting;
			
	    }

	    /// <summary>Creates a new ObjectCreationParameters</summary>
		public ObjectCreationParameters(double trackPosition, double startingDistance, double endingDistance, double brightness, int sectionIndex) :
			this(trackPosition, 0, startingDistance, endingDistance, brightness, sectionIndex)
	    {
	    }

	    /// <summary>Creates a new ObjectCreationParameters</summary>
		public ObjectCreationParameters(double trackPosition, double accurateObjectDisposalZOffset, double startingDistance, double endingDistance) 
			: this(trackPosition, accurateObjectDisposalZOffset, startingDistance, endingDistance, 1.0) { }

	    /// <summary>Creates a new ObjectCreationParameters</summary>
		public ObjectCreationParameters(double trackPosition, double startingDistance, double endingDistance)
			: this(trackPosition, 0, startingDistance, endingDistance, 1.0) { }

	    /// <summary>Creates a new ObjectCreationParameters</summary>
		public ObjectCreationParameters(double startingDistance, double endingDistance)
			: this(startingDistance, 0, startingDistance, endingDistance, 1.0) { }

		/// <summary>Returns a clone of the ObjectCreationParameters with DisableShadowCasting set</summary>
		public ObjectCreationParameters WithoutShadow()
		{
			return new ObjectCreationParameters(TrackPosition, AccurateObjectDisposalZOffset, StartingDistance, EndingDistance, Brightness, -1, true);
		}
	}
}
