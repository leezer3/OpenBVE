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
		/// <summary>Holds a reference to the car the light is attached to</summary>
	    private readonly AbstractCar BaseCar;
		/// <summary>Holds the currently active light state, or null if no light active</summary>
	    public SceneLight CurrentState;

	    private int lastStateIndex;

	    private double stateTimer;

		/// <summary>Creates a new LightDefinition</summary>
		/// <param name="baseCar"></param>
	    public MSTSLightDefinition(AbstractCar baseCar)
	    {
			States = new List<SceneLight>();
			Headlights = 0;
			Unit = 0;
			BaseCar = baseCar;
			lastStateIndex = 0;
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

		    if (States[lastStateIndex].Duration != 0)
		    {
			    stateTimer += timeElapsed;
			    if (stateTimer > States[lastStateIndex].Duration)
			    {
					stateTimer = 0;
					lastStateIndex++;
					lastStateIndex %= States.Count;
			    }
		    }

		    CurrentState = States[lastStateIndex];
	    }
    }
}
