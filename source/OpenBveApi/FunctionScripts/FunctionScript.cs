using System;
using OpenBveApi.Hosts;
using OpenBveApi.Math;

namespace OpenBveApi.FunctionScripting
{
	/// <summary>The base abstract function script which consumers must implement</summary>
	public class FunctionScript
	{
		private HostInterface currentHost;
		/// <summary>The instructions to perform</summary>
		public Instructions[] Instructions;
		/// <summary>The stack for the script</summary>
		public double[] Stack;
		/// <summary>All constants used for the script</summary>
		public double[] Constants;
		/// <summary>The last result returned</summary>
		public double LastResult;
		/// <summary>The minimum pinned result or NaN to set no minimum</summary>
		public double Maximum = Double.NaN;
		/// <summary>The maximum pinned result or NaN to set no maximum</summary>
		public double Minimum = Double.NaN;

		/// <summary>Performs the function script, and returns the current result</summary>
		public double Perform(Train Train, int CarIndex, Vector3 Position, double TrackPosition, int SectionIndex, bool IsPartOfTrain, double TimeElapsed, int CurrentState)
		{
			currentHost.ExecuteFunctionScript(this, Train, CarIndex, Position, TrackPosition, SectionIndex, IsPartOfTrain, TimeElapsed, CurrentState);

			//Allows us to pin the result, but keep the underlying figure
			if (this.Minimum != Double.NaN & this.LastResult < Minimum)
			{
				return Minimum;
			}
			if (this.Maximum != Double.NaN & this.LastResult > Maximum)
			{
				return Maximum;
			}
			return this.LastResult;
		}

		/// <summary>Checks whether the specified function will return a constant result</summary>
		public bool ConstantResult()
		{
			if (Instructions.Length == 1 && Instructions[0] == FunctionScripting.Instructions.SystemConstant)
			{
				return true;
			}
			for (int i = 0; i < Instructions.Length; i++)
			{
				if ((int) Instructions[i] >= (int) FunctionScripting.Instructions.LogicalXor)
				{
					return false;
				}
			}
			return true;
		}

		/// <summary>Creates a new function script</summary>
		/// <param name="Host">A reference to the base application host interface</param>
		public FunctionScript(HostInterface Host)
		{
			currentHost = Host;
		}

		/// <summary>Clones the function script</summary>
		public FunctionScript Clone()
		{
			return (FunctionScript) this.MemberwiseClone();
		}
	}
}
