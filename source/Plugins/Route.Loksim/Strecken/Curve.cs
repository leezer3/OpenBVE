//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2026, Christopher Lees, The OpenBVE Project
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

using System;
using System.Collections.Generic;
using System.Xml;
using OpenBveApi.Math;

namespace LokSimRouteParser
{
	internal class Curve : TrackElement
	{
		/// <summary>The angle of turn of the curve</summary>
		internal double Angle;

		internal Curve(XmlNode node)
		{
			if (node.Attributes == null)
			{
				return;
			}

			for (int i = 0; i < node.Attributes.Count; i++)
			{
				switch (node.Attributes[i].Name)
				{
					case "Start":
						StartingDistance = double.Parse(node.Attributes[i].InnerText);
						break;
					case "Ende":
						EndingDistance = double.Parse(node.Attributes[i].InnerText);
						break;
					case "Winkel":
						Angle = double.Parse(node.Attributes[i].InnerText);
						break;
					case "SanfterWinkel":
						SoftTransistion = node.Attributes[i].InnerText.ToLowerInvariant() == "true";
						break;
				}
			}

			// from Loksim documentation:
			// ((𝐸nde – 𝑆tart) ∗ 180) / (𝜋𝜋 ∗ 𝑅adius) = 𝑊inkel
			// Convert this to curve radius by rearranging equation 
			Angle = 180 * (EndingDistance - StartingDistance) / (2 * Math.PI * Angle);
		}

		internal override void Create(ref List<OpenBveApi.Routes.TrackElement> track, ref Vector3 Position, ref Vector2 Direction)
		{
			OpenBveApi.Routes.TrackElement lastElement = track[track.Count - 1];
			double currentDistance = lastElement.StartingTrackPosition;

			// for convenience add all 'missing' 1m track elements before the first
			while (currentDistance < StartingDistance)
			{
				OpenBveApi.Routes.TrackElement tS = new OpenBveApi.Routes.TrackElement(currentDistance)
				{
					CurveRadius = 0,
					WorldPosition = Position,
					WorldDirection = Vector3.GetVector3(Direction, 0),
					WorldSide = new Vector3(Direction.Y, 0.0, -Direction.X),
				};

				tS.WorldUp = Vector3.Cross(tS.WorldDirection, tS.WorldSide);
				track.Add(tS);
				currentDistance += 1;
				Position.X += Direction.X;
				Position.Z += Direction.Y;
			}

			currentDistance = StartingDistance;
			while (currentDistance < EndingDistance)
			{
				OpenBveApi.Routes.TrackElement tS = new OpenBveApi.Routes.TrackElement(currentDistance)
				{
					CurveRadius = Angle,
					WorldPosition = Position,
					WorldDirection = Vector3.GetVector3(Direction, 0),
					WorldSide = new Vector3(Direction.Y, 0.0, - Direction.X),
				};

				tS.WorldUp = Vector3.Cross(tS.WorldDirection, tS.WorldSide);
				track.Add(tS);
				currentDistance += 1;
				Position.X += Direction.X;
				Position.Z += Direction.Y;

				double r = Angle;
				double b = 1.0 / Math.Abs(r);
				double a = 0.5 * Math.Sign(r) * b;
				Direction.Rotate(Math.Cos(-a), Math.Sin(-a));
			}
		}
	}
}
