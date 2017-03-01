#include "plugin.h"
#include <windows.h>

extern "C" {

	// called when the plugin is loaded
	ATS_API void WINAPI Load () {
	}

	// called when the plugin is unloaded
	ATS_API void WINAPI Dispose () {
	}

	// called to query the API version
	ATS_API int WINAPI GetPluginVersion () {
		return ATS_VERSION;
	}

	// called to inform the plugin about the specs of the train
	ATS_API void WINAPI SetVehicleSpec (ATS_VEHICLESPEC vehicleSpec) {
	}

	// called to inform the plugin about the starting mode of the security system
	ATS_API void WINAPI Initialize (int brake) {
	}

	// called every frame
	ATS_API ATS_HANDLES WINAPI Elapse (ATS_VEHICLESTATE vehicleState, int* panel, int* sound) {
	}

	// called when the power notch is changed
	ATS_API void WINAPI SetPower (int notch) {
	}

	// called when the brake notch is changed
	ATS_API void WINAPI SetBrake (int notch) {
	}

	// called when the reverser is changed
	ATS_API void WINAPI SetReverser (int pos) {
	}

	// called when a plugin key is pressed
	ATS_API void WINAPI KeyDown (int atsKeyCode) {
	}

	// called when a plugin key is released
	ATS_API void WINAPI KeyUp (int atsKeyCode) {
	}

	// called when a horn is played (or stopped)
	ATS_API void WINAPI HornBlow (int hornType) {
	}

	// called when the doors start opening
	ATS_API void WINAPI DoorOpen () {
	}

	// called when the doors have closed
	ATS_API void WINAPI DoorClose () {
	}

	// called when the state of the upcoming signal changes
	ATS_API void WINAPI SetSignal (int signal) {
	}

	// called when a beacon is passed
	ATS_API void WINAPI SetBeaconData (ATS_BEACONDATA beaconData) {
	}

}
