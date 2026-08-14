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

using System.Collections.Generic;
using OpenBveApi.Trains;

namespace OpenBveApi.Objects
{
    /// <summary>A light definition as per MSTS ENG / WAG file</summary>
    public class MSTSLightDefinition
    {
		/// <summary>Contains the list of light states</summary>
	    public List<SceneLight> States;
		/// <summary>The headlight conditional</summary>
	    public int Headlights;
		/// <summary>The unit conditional</summary>
	    public int Unit;
	    /// <summary>The control conditional</summary>
	    public int Control;
		/// <summary>The type of light</summary>
	    public SceneLightType Type;
		/// <summary>The cycle type</summary>
	    public int Cycle;
		/// <summary>Holds a reference to the car the light is attached to</summary>
	    private readonly AbstractCar BaseCar;
		/// <summary>Holds the currently active light state, or null if no light active</summary>
	    public SceneLight CurrentState;

		// Control variables

		/// <summary>The index of the current state</summary>
		private int currentStateIndex;
		/// <summary>Timer controlling the cycle</summary>
		private double stateTimer;
		/// <summary>The current cycle direction</summary>
		/// <remarks>>TRUE for forwards, FALSE for reverse</remarks>
		private bool cycleDirection;

		/// <summary>Creates a new LightDefinition</summary>
		/// <param name="baseCar"></param>
	    public MSTSLightDefinition(AbstractCar baseCar)
	    {
			States = new List<SceneLight>();
			Headlights = 0;
			Unit = 0;
			BaseCar = baseCar;
			currentStateIndex = 0;
	    }

	    /// <summary>Updates the LightDefinition</summary>
	    /// <param name="timeElapsed">The time elapsed</param>
	    public void Update(double timeElapsed)
	    {
		    bool shouldBeLit = true;
		    
		    // https://www.coalstonewcastle.com.au/physics/or-parameter-lights/
			// maybe move to enums

			dynamic d = BaseCar;

			switch (Headlights)
		    {
				case 0:
					// headlights state is not checked
					break;
				case 1:
				case 2:
				case 3:
					// 1- lit when headlights are *off*
					// 2- lit when headlights are dim
					// 3- lit when headlights are bright
					if (d.baseTrain.SafetySystems.Headlights.CurrentState != Headlights -1)
					{
						shouldBeLit = false;
					}
					break;
				case 4:
					// lit when any headlights state
					if (d.baseTrain.SafetySystems.Headlights.CurrentState == 0)
					{
						shouldBeLit = false;
					}
					break;
				case 5:
					// lit when off or dim
					if (d.baseTrain.SafetySystems.Headlights.CurrentState != 2)
					{
						shouldBeLit = false;
					}
					break;
				case 6:
					// lit when off or bright
					if (d.baseTrain.SafetySystems.Headlights.CurrentState != 1)
					{
						shouldBeLit = false;
					}
					break;
				case 7:
					// always in dim mode
					break;
				case 8:
					// always in bright mode
					break;
		    }

		    switch (Unit)
		    {
				case 0:
					// not checked
					break;
				case 1:
					// part of train (must be in OpenBVE)
					break;
				case 2:
					// lit if front car
					if (BaseCar.Index != 0)
					{
						shouldBeLit = false;
					}
					break;
				case 3:
					// lit if rear car
					if (BaseCar.Index != d.baseTrain.Cars.Length - 1)
					{
						shouldBeLit = false;
					}
					break;
				case 4:
				case 5:
					// lit if at rear and reversed
					// lit if at front and reversed
					// not currently handled
					shouldBeLit = false;
					break;
		    }

		    switch (Control)
		    {
				case 0:
					// not checked
					break;
				case 1:
					// lit if AI train
					if (d.baseTrain.IsPlayerTrain)
					{
						shouldBeLit = false;
					}
					break;
				case 2:
					// lit if player train
					if (!d.baseTrain.IsPlayerTrain)
					{
						shouldBeLit = false;
					}
					break;
		    }

		    if (shouldBeLit == false)
		    {
			    CurrentState = null;
			    return;
		    }

		    if (States[currentStateIndex].Duration != 0)
		    {
			    stateTimer += timeElapsed;
			    if (stateTimer > States[currentStateIndex].Duration)
			    {
					stateTimer = 0;
					
					if (Cycle == 1)
					{
						currentStateIndex++;
						currentStateIndex %= States.Count;
					}
					else
					{
						if (currentStateIndex == States.Count - 1)
						{
							cycleDirection = true;
						}
						else if (currentStateIndex == 0)
						{
							cycleDirection = false;
						}

						if (cycleDirection)
						{
							currentStateIndex--;
						}
						else
						{
							currentStateIndex++;
						}
					}
					
			    }
		    }

		    CurrentState = States[currentStateIndex];
	    }
    }
}
