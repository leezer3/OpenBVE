using System.IO;
using OpenBve.Formats.MsTs;
using OpenBveApi.World;

namespace Train.MsTs
{
	internal abstract class VigilanceDevice
	{
		/// <summary>The time limit before this device alarms</summary>
		/// <remarks>Set to -1 for triggered devices</remarks>
		internal double TimeLimit;
		/// <summary>The time limit before the device intervenes in alarm state</summary>
		internal double AlarmTimeLimit;
		/// <summary>The time limit before the device applies a penalty</summary>
		internal double PenaltyTimeLimit;
		/// <summary>The critical level of the trigger</summary>
		internal double CriticalLevel;
		/// <summary>The reset level of the trigger</summary>
		internal double ResetLevel;
		/// <summary>Whether full brake is applied on intervention</summary>
		internal bool AppliesFullBrake;
		/// <summary>Whether emergency brake is applied on intervention</summary>
		internal bool AppliesEmergencyBrake;
		/// <summary>Whether power is cut on invervention</summary>
		internal bool CutsPower;
		/// <summary>Whether the engine is shut down on intervention</summary>
		internal bool ShutsDownEngine;
		/// <summary>The reset conditions</summary>
		internal ResetCondition ResetConditions;

		internal static VigilanceDevice CreateVigilanceDevice(KujuTokenID token)
		{
			switch (token)
			{
				case KujuTokenID.AWSMonitor:
					return new AWSMonitor();
				case KujuTokenID.EmergencyStopMonitor:
					return new EmergencyStopMonitor();
				case KujuTokenID.OverspeedMonitor:
					return new OverspeedMonitor();
				case KujuTokenID.VigilanceMonitor:
					return new VigilanceMonitor();
				default:
					throw new InvalidDataException("Not a valid vigilance device");
			}
		}

		internal void ParseBlock(Block block)
		{
			switch (block.Token)
			{
				case KujuTokenID.MonitoringDeviceMonitorTimeLimit:
					TimeLimit = block.ReadSingle();
					break;
				case KujuTokenID.MonitoringDeviceAlarmTimeLimit:
					AlarmTimeLimit = block.ReadSingle();
					break;
				case KujuTokenID.MonitoringDevicePenaltyTimeLimit:
					PenaltyTimeLimit = block.ReadSingle();
					break;
				case KujuTokenID.MonitoringDeviceCriticalLevel:
					if (block.ParentBlock.Token == KujuTokenID.EmergencyStopMonitor)
					{
						// Check exact behaviour
						CriticalLevel = block.ReadSingle(UnitOfVelocity.MetersPerSecond);
					}
					else
					{
						CriticalLevel = block.ReadSingle();
					}
					break;
				case KujuTokenID.MonitoringDeviceResetLevel:
					if (block.ParentBlock.Token == KujuTokenID.EmergencyStopMonitor)
					{
						// Check exact behaviour
						CriticalLevel = block.ReadSingle(UnitOfVelocity.MetersPerSecond);
					}
					else
					{
						CriticalLevel = block.ReadSingle();
					}
					break;
				case KujuTokenID.MonitoringDeviceAppliesFullBrake:
					AppliesFullBrake = block.ReadInt32() == 1;
					break;
				case KujuTokenID.MonitoringDeviceAppliesEmergencyBrake:
					AppliesEmergencyBrake = block.ReadInt32() == 1;
					break;
				case KujuTokenID.MonitoringDeviceAppliesCutsPower:
					CutsPower = block.ReadInt32() == 1;
					break;
				case KujuTokenID.MonitoringDeviceAppliesShutsDownEngine:
					ShutsDownEngine = block.ReadInt32() == 1;
					break;
				case KujuTokenID.MonitoringDeviceResetOnZeroSpeed:
					if (block.ReadInt32() == 1)
					{
						ResetConditions |= ResetCondition.ZeroSpeed;
					}
					break;
				case KujuTokenID.MonitoringDeviceResetOnZeroThrottle:
					if (block.ReadInt32() == 1)
					{
						ResetConditions |= ResetCondition.ZeroThrottle;
					}
					break;
				case KujuTokenID.MonitoringDeviceResetOnDirectionNeutral:
					if (block.ReadInt32() == 1)
					{
						ResetConditions |= ResetCondition.DirectionNeutral;
					}
					break;
				case KujuTokenID.MonitoringDeviceResetOnEngineAtIdle:
					if (block.ReadInt32() == 1)
					{
						ResetConditions |= ResetCondition.EngineAtIdle;
					}
					break;
				case KujuTokenID.MonitoringDeviceResetOnBrakeOff:
					if (block.ReadInt32() == 1)
					{
						ResetConditions |= ResetCondition.BrakeOff;
					}
					break;
				case KujuTokenID.MonitoringDeviceResetOnBrakeFullyOn:
					if (block.ReadInt32() == 1)
					{
						ResetConditions |= ResetCondition.BrakeFullyOn;
					}
					break;
				case KujuTokenID.MonitoringDeviceResetOnDynamicBrakeOff:
					if (block.ReadInt32() == 1)
					{
						ResetConditions |= ResetCondition.DynamicBrakeOff;
					}
					break;
				case KujuTokenID.MonitoringDeviceResetOnDynamicBrakeOn:
					if (block.ReadInt32() == 1)
					{
						ResetConditions |= ResetCondition.DynamicBrakeOn;
					}
					break;
				case KujuTokenID.MonitoringDeviceResetOnResetButton:
					if (block.ReadInt32() == 1)
					{
						ResetConditions |= ResetCondition.ResetButton;
					}
					break;
			}
		}
	}
}
