// calling convention
#define ATS_API __declspec(dllexport)

// plugin version
#define ATS_VERSION 131072

// keys
#define ATS_KEY_S  0
#define ATS_KEY_A1 1
#define ATS_KEY_A2 2
#define ATS_KEY_B1 3
#define ATS_KEY_B2 4
#define ATS_KEY_C1 5
#define ATS_KEY_C2 6
#define ATS_KEY_D  7
#define ATS_KEY_E  8
#define ATS_KEY_F  9
#define ATS_KEY_G  10
#define ATS_KEY_H  11
#define ATS_KEY_I  12
#define ATS_KEY_J  13
#define ATS_KEY_K  14
#define ATS_KEY_L  15

// safety system initialization
#define ATS_INIT_ON_SRV  -1
#define ATS_INIT_ON_EMG   0
#define ATS_INIT_OFF_EMG  1

// sound instructions
#define ATS_SOUND_STOP        -10000
#define ATS_SOUND_PLAYLOOPING 0
#define ATS_SOUND_PLAY        1
#define ATS_SOUND_CONTINUE    2

// horn types
#define ATS_HORN_PRIMARY   0
#define ATS_HORN_SECONDARY 1
#define ATS_HORN_MUSIC     2

// constant speed system instructions
#define ATS_CONSTANTSPEED_CONTINUE 0
#define ATS_CONSTANTSPEED_ENABLE   1
#define ATS_CONSTANTSPEED_DISABLE  2

// vehicle specifications
struct ATS_VEHICLESPEC {
	int BrakeNotches;
	int PowerNotches;
	int AtsNotch;
	int B67Notch;
	int Cars;
};

// vehicle status
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

// beacons
struct ATS_BEACONDATA {
	int Type;
	int Signal;
	float Distance;
	int Optional;
};

// handles
struct ATS_HANDLES {
	int Brake;
	int Power;
	int Reverser;
	int ConstantSpeed;
};