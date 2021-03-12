//Simplified BSD License (BSD-2-Clause)
//
//Copyright (c) 2020, Christopher Lees, The OpenBVE Project
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
using MechanikRouteParser;
using OpenBveApi.FunctionScripting;
using OpenBveApi.Math;
using OpenBveApi.Objects;

namespace Route.Mechanik
{
	internal class Semaphore
	{
		/// <summary>The signal aspect applying to the current section</summary>
		internal readonly SignalAspect CurrentAspect;
		/// <summary>The signal aspect applying to the next section</summary>
		internal readonly SignalAspect NextAspect;
		/// <summary>Whether the signal is held at red until station departure</summary>
		internal readonly bool HeldAtRed;
		/// <summary>The position of the signal object</summary>
		internal readonly Vector3 Position;
		/// <summary>Gets the speed limit for the section</summary>
		internal double SpeedLimit
		{
			get
			{
				switch (CurrentAspect)
				{
					case SignalAspect.NotDisplayed:
						return Double.MaxValue;
					case SignalAspect.Stop:
						return 0;
					case SignalAspect.Slow:
						return 40;
					case SignalAspect.Medium:
						return 60;
					case SignalAspect.Fast:
						return 100;
					default:
						return Double.MaxValue;
				}
			}
		}

		internal Semaphore(SignalAspect currentAspect, SignalAspect nextAspect, bool heldAtRed, Vector3 position)
		{
			CurrentAspect = currentAspect;
			NextAspect = nextAspect;
			HeldAtRed = heldAtRed;
			Position = position;
		}

		internal AnimatedObject Object()
		{
			/*
			 * WARNING NOTE:
			 * This may in some cases look odd in a 3D world
			 * As the default red signal isn't always seen (e.g. if on the move)
			 * then developers didn't always care about the appearance of the signal
			 * after it'd been passed & so edited some of the bitmap set
			 *
			 * Not a lot we can do about this, *not* a bug
			 */
			AnimatedObject signal = new AnimatedObject(Plugin.CurrentHost)
			{
				States = new[]
				{
					new ObjectState(), new ObjectState(), new ObjectState()
				},
				RefreshRate = 0.5
			};
			switch (CurrentAspect)
			{
				case SignalAspect.NotDisplayed:
					signal.States[0] = new ObjectState(Parser.CreateStaticObject(Position, 1, 81, true));
					switch (NextAspect)
					{
						case SignalAspect.Slow:
							signal.States[1] = new ObjectState(Parser.CreateStaticObject(Position, 1, 80, true));
							signal.States[2] = new ObjectState(Parser.CreateStaticObject(Position, 1, 83, true));
							signal.StateFunction = new FunctionScript(Plugin.CurrentHost, "if[section == 0, 0, 1 + mod[value + 2, 2]]", true);
							break;
						case SignalAspect.Medium:
							signal.States[1] = new ObjectState(Parser.CreateStaticObject(Position, 1, 80, true));
							signal.States[2] = new ObjectState(Parser.CreateStaticObject(Position, 1, 83, true));
							signal.StateFunction = new FunctionScript(Plugin.CurrentHost, "if[section == 0, 0, 1 + mod[value + 2, 2]]", true);
							break;
						default:
							signal.States[1] = new ObjectState(Parser.CreateStaticObject(Position, 1, 80, true));
							signal.StateFunction = new FunctionScript(Plugin.CurrentHost, "section", true);
							break;
					}
					break;
				case SignalAspect.Stop:
					signal.States[0] = new ObjectState(Parser.CreateStaticObject(Position, 1, 81, true));
					signal.States[1] = new ObjectState(Parser.CreateStaticObject(Position, 1, 80, true));
					signal.StateFunction = new FunctionScript(Plugin.CurrentHost, "section", true);
					break;
				case SignalAspect.Slow:
					signal.States[0] = new ObjectState(Parser.CreateStaticObject(Position, 1, 80, true));
					switch (NextAspect)
					{
						case SignalAspect.NotDisplayed:
							signal.States[1] = new ObjectState(Parser.CreateStaticObject(Position, 1, 80, true));
							signal.StateFunction = new FunctionScript(Plugin.CurrentHost, "section", true);
							break;
						case SignalAspect.Stop:
							signal.States[1] = new ObjectState(Parser.CreateStaticObject(Position, 1, 88, true));
							signal.StateFunction = new FunctionScript(Plugin.CurrentHost, "section", true);
							break;
						case SignalAspect.Slow:
						case SignalAspect.Medium:
							signal.States[1] = new ObjectState(Parser.CreateStaticObject(Position, 1, 88, true));
							signal.States[2] = new ObjectState(Parser.CreateStaticObject(Position, 1, 92, true));
							signal.StateFunction = new FunctionScript(Plugin.CurrentHost, "if[section == 0, 0, 1 + mod[value + 2, 2]]", true);
							break;
						case SignalAspect.Fast:
							signal.States[1] = new ObjectState(Parser.CreateStaticObject(Position, 1, 87, true));
							signal.States[2] = new ObjectState(Parser.CreateStaticObject(Position, 1, 92, true));
							signal.StateFunction = new FunctionScript(Plugin.CurrentHost, "if[section == 0, 0, mod[value + 2, 2]]", true);
							break;
						case SignalAspect.Unlimited:
							signal.States[1] = new ObjectState(Parser.CreateStaticObject(Position, 1, 86, true));
							signal.StateFunction = new FunctionScript(Plugin.CurrentHost, "section", true);
							break;
					}
					break;
				case SignalAspect.Medium:
					signal.States[0] = new ObjectState(Parser.CreateStaticObject(Position, 1, 80, true));
					switch (NextAspect)
					{
						case SignalAspect.NotDisplayed:
							signal.States[1] = new ObjectState(Parser.CreateStaticObject(Position, 1, 80, true));
							signal.StateFunction = new FunctionScript(Plugin.CurrentHost, "section", true);
							break;
						case SignalAspect.Stop:
							signal.States[1] = new ObjectState(Parser.CreateStaticObject(Position, 1, 89, true));
							signal.StateFunction = new FunctionScript(Plugin.CurrentHost, "section", true);
							break;
						case SignalAspect.Slow:
							signal.States[1] = new ObjectState(Parser.CreateStaticObject(Position, 1, 89, true));
							signal.States[2] = new ObjectState(Parser.CreateStaticObject(Position, 1, 91, true));
							signal.StateFunction = new FunctionScript(Plugin.CurrentHost, "if[section == 0, 0, 1 + mod[value + 2, 2]]", true);
							break;
						case SignalAspect.Medium:
							signal.States[1] = new ObjectState(Parser.CreateStaticObject(Position, 1, 85, true));
							signal.States[2] = new ObjectState(Parser.CreateStaticObject(Position, 1, 90, true));
							signal.StateFunction = new FunctionScript(Plugin.CurrentHost, "if[section == 0, 0, 1 + mod[value + 2, 2]]", true);
							break;
						case SignalAspect.Fast:
							signal.States[1] = new ObjectState(Parser.CreateStaticObject(Position, 1, 87, true));
							signal.States[2] = new ObjectState(Parser.CreateStaticObject(Position, 1, 91, true));
							signal.StateFunction = new FunctionScript(Plugin.CurrentHost, "if[section == 0, 0, mod[value + 2, 2]]", true);
							break;
						case SignalAspect.Unlimited:
							signal.States[1] = new ObjectState(Parser.CreateStaticObject(Position, 1, 87, true));
							signal.StateFunction = new FunctionScript(Plugin.CurrentHost, "section", true);
							break;
					}
					break;
				case SignalAspect.Fast:
					signal.States[0] = new ObjectState(Parser.CreateStaticObject(Position, 1, 80, true));
					switch (NextAspect)
					{
						case SignalAspect.NotDisplayed:
							signal.States[1] = new ObjectState(Parser.CreateStaticObject(Position, 1, 80, true));
							signal.StateFunction = new FunctionScript(Plugin.CurrentHost, "section", true);
							break;
						case SignalAspect.Stop:
							signal.States[1] = new ObjectState(Parser.CreateStaticObject(Position, 1, 85, true));
							signal.StateFunction = new FunctionScript(Plugin.CurrentHost, "section", true);
							break;
						case SignalAspect.Slow:
						case SignalAspect.Medium:
							signal.States[1] = new ObjectState(Parser.CreateStaticObject(Position, 1, 85, true));
							signal.States[2] = new ObjectState(Parser.CreateStaticObject(Position, 1, 90, true));
							signal.StateFunction = new FunctionScript(Plugin.CurrentHost, "if[section == 0, 0, 1 + mod[value + 2, 2]]", true);
							break;
						case SignalAspect.Fast:
							signal.States[1] = new ObjectState(Parser.CreateStaticObject(Position, 1, 94, true));
							signal.States[2] = new ObjectState(Parser.CreateStaticObject(Position, 1, 90, true));
							signal.StateFunction = new FunctionScript(Plugin.CurrentHost, "if[section == 0, 0, mod[value + 2, 2]]", true);
							break;
						case SignalAspect.Unlimited:
							signal.States[1] = new ObjectState(Parser.CreateStaticObject(Position, 1, 84, true));
							signal.StateFunction = new FunctionScript(Plugin.CurrentHost, "section", true);
							break;
					}
					break;
				case SignalAspect.Unlimited:
					signal.States[0] = new ObjectState(Parser.CreateStaticObject(Position, 1, 80, true));
					switch (NextAspect)
					{
						case SignalAspect.NotDisplayed:
							signal.States[1] = new ObjectState(Parser.CreateStaticObject(Position, 1, 82, true));
							signal.StateFunction = new FunctionScript(Plugin.CurrentHost, "section", true);
							break;
						case SignalAspect.Stop:
						case SignalAspect.Slow:
						case SignalAspect.Medium:
							signal.States[1] = new ObjectState(Parser.CreateStaticObject(Position, 1, 80, true));
							signal.StateFunction = new FunctionScript(Plugin.CurrentHost, "section", true);
							break;
						case SignalAspect.Fast:
							signal.States[1] = new ObjectState(Parser.CreateStaticObject(Position, 1, 80, true));
							signal.States[2] = new ObjectState(Parser.CreateStaticObject(Position, 1, 82, true));
							signal.StateFunction = new FunctionScript(Plugin.CurrentHost, "if[section == 0, 0, mod[value + 2, 2]]", true);
							break;
						case SignalAspect.Unlimited:
							signal.States[1] = new ObjectState(Parser.CreateStaticObject(Position, 1, 80, true));
							signal.StateFunction = new FunctionScript(Plugin.CurrentHost, "section", true);
							break;
					}
					break;
			}
			return signal;
		}
	}
}
