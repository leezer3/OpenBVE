﻿//Simplified BSD License (BSD-2-Clause)
//
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

using System.Collections.Generic;
using TrainManager.Car;

namespace TrainManager.Motor
{
	/// <summary>An abstract engine</summary>
    public abstract class AbstractEngine
    {
		/// <summary>Holds a reference to the base car</summary>
	    internal readonly CarBase BaseCar;
		/// <summary>The fuel supply</summary>
	    public FuelTank FuelTank;
	    /// <summary>Whether the engine is running</summary>
	    public bool IsRunning;
		/// <summary>The components of the engine</summary>
	    public Dictionary<EngineComponent, AbstractComponent> Components;

		/// <summary>Creates a new AbstractEngine</summary>
		protected AbstractEngine(CarBase car)
	    {
		    BaseCar = car;
			Components = new Dictionary<EngineComponent, AbstractComponent>();
	    }



		/// <summary>Called once a frame to update the engine</summary>
		/// <param name="timeElapsed"></param>
	    public abstract void Update(double timeElapsed);
    }
}
