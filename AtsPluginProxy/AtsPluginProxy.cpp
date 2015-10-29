#include "stdafx.h"

// --- main ---
BOOL APIENTRY DllMain(HANDLE hModule, DWORD ul_reason_for_call, LPVOID lpReserved) {
    return TRUE;
}

// --- structures ---
struct ATS_VEHICLESPEC {
	int BrakeNotches;
	int PowerNotches;
	int AtsNotch;
	int B67Notch;
	int Cars;
};
struct ATS_VEHICLESTATE {
	double Location;
	float Speed;
	int Time;
	float BcPressure;
	float MrPressure;
	float ErPressure;
	float BpPressure;
	float SapPressure;
	float Current;
};
struct ATS_BEACONDATA {
	int Type;
	int Signal;
	float Distance;
	int Optional;
};
struct ATS_HANDLES {
	int Brake;
	int Power;
	int Reverser;
	int ConstantSpeed;
};

#define ATS_API __declspec(dllimport)

// --- handles ---
HMODULE dllhandle = NULL;
typedef ATS_API void (__stdcall *LOAD) (); LOAD load = NULL;
typedef ATS_API void (__stdcall *DISPOSE) (); DISPOSE dispose = NULL;
typedef ATS_API int (__stdcall *GETPLUGINVERSION) (); GETPLUGINVERSION getpluginversion = NULL;
typedef ATS_API void (__stdcall *SETVEHICLESPEC) (ATS_VEHICLESPEC vehicleSpec); SETVEHICLESPEC setvehiclespec = NULL;
typedef ATS_API void (__stdcall *INITIALIZE) (int brake); INITIALIZE initialize = NULL;
typedef ATS_API ATS_HANDLES (__stdcall *ELAPSE) (ATS_VEHICLESTATE vehicleState, int* panel, int* sound); ELAPSE elapse = NULL;
typedef ATS_API void (__stdcall *SETPOWER) (int setpower); SETPOWER setpower = NULL;
typedef ATS_API void (__stdcall *SETBRAKE) (int setbrake); SETBRAKE setbrake = NULL;
typedef ATS_API void (__stdcall *SETREVERSER) (int setreverser); SETREVERSER setreverser = NULL;
typedef ATS_API void (__stdcall *KEYDOWN) (int atsKeyCode); KEYDOWN keydown = NULL;
typedef ATS_API void (__stdcall *KEYUP) (int atsKeyCode); KEYUP keyup = NULL;
typedef ATS_API void (__stdcall *HORNBLOW) (int hornType); HORNBLOW hornblow = NULL;
typedef ATS_API void (__stdcall *DOOROPEN) (); DOOROPEN dooropen = NULL;
typedef ATS_API void (__stdcall *DOORCLOSE) (); DOORCLOSE doorclose = NULL;
typedef ATS_API void (__stdcall *SETSIGNAL) (int signal); SETSIGNAL setsignal = NULL;
typedef ATS_API void (__stdcall *SETBEACONDATA) (ATS_BEACONDATA beaconData); SETBEACONDATA setbeacondata = NULL;

// --- load the plugin ---
int _stdcall LoadDLL(LPCWSTR fileUnicode, LPCSTR fileAnsi) {
	dllhandle = LoadLibraryW(fileUnicode);
	if (dllhandle == NULL) {
		dllhandle = LoadLibraryA(fileAnsi);
		if (dllhandle == NULL) return 0;
	}
	{ // --- Load ---
		FARPROC functionhandle = GetProcAddress(dllhandle, "Load");
		if (functionhandle != NULL) {
			load = (LOAD)functionhandle;
		}
	}
	{ // --- Dispose ---
		FARPROC functionhandle = GetProcAddress(dllhandle, "Dispose");
		if (functionhandle != NULL) {
			dispose = (DISPOSE)functionhandle;
		}
	}
	{ // --- GetPluginVersion ---
		FARPROC functionhandle = GetProcAddress(dllhandle, "GetPluginVersion");
		if (functionhandle != NULL) {
			getpluginversion = (GETPLUGINVERSION)functionhandle;
		}
	}
		{ // --- SetVehicleSpec ---
		FARPROC functionhandle = GetProcAddress(dllhandle, "SetVehicleSpec");
		if (functionhandle != NULL) {
			setvehiclespec = (SETVEHICLESPEC)functionhandle;
		}
	}
	{ // --- Initialize ---
		FARPROC functionhandle = GetProcAddress(dllhandle, "Initialize");
		if (functionhandle != NULL) {
			initialize = (INITIALIZE)functionhandle;
		}
	}
	{ // --- Elapse ---
		FARPROC functionhandle = GetProcAddress(dllhandle, "Elapse");
		if (functionhandle != NULL) {
			elapse = (ELAPSE)functionhandle;
		}
	}
	{ // --- SetPower ---
		FARPROC functionhandle = GetProcAddress(dllhandle, "SetPower");
		if (functionhandle != NULL) {
			setpower = (SETPOWER)functionhandle;
		}
	}
	{ // --- SetBrake ---
		FARPROC functionhandle = GetProcAddress(dllhandle, "SetBrake");
		if (functionhandle != NULL) {
			setbrake = (SETBRAKE)functionhandle;
		}
	}
	{ // --- SetReverser ---
		FARPROC functionhandle = GetProcAddress(dllhandle, "SetReverser");
		if (functionhandle != NULL) {
			setreverser = (SETREVERSER)functionhandle;
		}
	}
	{ // --- KeyDown ---
		FARPROC functionhandle = GetProcAddress(dllhandle, "KeyDown");
		if (functionhandle != NULL) {
			keydown = (KEYDOWN)functionhandle;
		}
	}
	{ // --- KeyUp ---
		FARPROC functionhandle = GetProcAddress(dllhandle, "KeyUp");
		if (functionhandle != NULL) {
			keyup = (KEYUP)functionhandle;
		}
	}
	{ // --- HornBlow ---
		FARPROC functionhandle = GetProcAddress(dllhandle, "HornBlow");
		if (functionhandle != NULL) {
			hornblow = (HORNBLOW)functionhandle;
		}
	}
	{ // --- DoorOpen ---
		FARPROC functionhandle = GetProcAddress(dllhandle, "DoorOpen");
		if (functionhandle != NULL) {
			dooropen = (DOOROPEN)functionhandle;
		}
	}
	{ // --- DoorClose ---
		FARPROC functionhandle = GetProcAddress(dllhandle, "DoorClose");
		if (functionhandle != NULL) {
			doorclose = (DOORCLOSE)functionhandle;
		}
	}
	{ // --- SetSignal ---
		FARPROC functionhandle = GetProcAddress(dllhandle, "SetSignal");
		if (functionhandle != NULL) {
			setsignal = (SETSIGNAL)functionhandle;
		}
	}
	{ // --- SetBeaconData ---
		FARPROC functionhandle = GetProcAddress(dllhandle, "SetBeaconData");
		if (functionhandle != NULL) {
			setbeacondata = (SETBEACONDATA)functionhandle;
		}
	}
	return 1;
}

// --- unload the plugin ---
int _stdcall UnloadDLL () {
	if (dllhandle != NULL) {
		load = NULL;
		dispose = NULL;
		getpluginversion = NULL;
		setvehiclespec = NULL;
		initialize = NULL;
		elapse = NULL;
		setpower = NULL;
		setbrake = NULL;
		setreverser = NULL;
		keydown = NULL;
		keyup = NULL;
		hornblow = NULL;
		dooropen = NULL;
		doorclose = NULL;
		setsignal = NULL;
		setbeacondata = NULL;
		return FreeLibrary(dllhandle);
	} else {
		return 1;
	}
}

// --- Load ---
void _stdcall Load () {
	if (load != NULL) load();
}

// --- Dispose ---
void _stdcall Dispose () {
	if (dispose != NULL) dispose();
}

// --- GetPluginVersion ---
int _stdcall GetPluginVersion () {
	if (getpluginversion != NULL) {
		return getpluginversion();
	} else {
		return 0;
	}
}

// --- SetVehicleSpec ---
void _stdcall SetVehicleSpec (ATS_VEHICLESPEC* vehicleSpec) {
	if (setvehiclespec != NULL) setvehiclespec(*vehicleSpec);
}

// --- Initialize ---
void _stdcall Initialize (int brake) {
	if (initialize != NULL) initialize(brake);
}

// --- Elapse ---
void _stdcall Elapse (ATS_HANDLES* atsHandles, ATS_VEHICLESTATE* vehicleState, int* panel, int* sound) {
	if (elapse != NULL) {
		ATS_HANDLES handles = elapse(*vehicleState, panel, sound);
		atsHandles->Brake = handles.Brake;
		atsHandles->Power = handles.Power;
		atsHandles->Reverser = handles.Reverser;
		atsHandles->ConstantSpeed = handles.ConstantSpeed;
	}
}

// --- SetPower ---
void _stdcall SetPower(int notch) {
	if (setpower != NULL) setpower(notch);
}

// --- SetBrake ---
void _stdcall SetBrake(int notch) {
	if (setbrake != NULL) setbrake(notch);
}

// --- SetReverser ---
void _stdcall SetReverser(int pos) {
	if (setreverser != NULL) setreverser(pos);
}

// --- KeyDown ---
void _stdcall KeyDown(int atsKeyCode) {
	if (keydown != NULL) keydown(atsKeyCode);
}

// --- KeyUp ---
void _stdcall KeyUp(int atsKeyCode) {
	if (keyup != NULL) keyup(atsKeyCode);
}

// --- HornBlow ---
void _stdcall HornBlow(int hornType) {
	if (hornblow != NULL) hornblow(hornType);
}

// --- DoorOpen ---
void _stdcall DoorOpen() {
	if (dooropen != NULL) dooropen();
}

// --- DoorClose ---
void _stdcall DoorClose() {
	if (doorclose != NULL) doorclose();
}

// --- SetSignal ---
void _stdcall SetSignal(int signal) {
	if (setsignal != NULL) setsignal(signal);
}

// --- SetBeaconData ---
void _stdcall SetBeaconData(ATS_BEACONDATA* beaconData) {
	if (setbeacondata != NULL) setbeacondata(*beaconData);
}