This file is not intended to provide a complete changelog (and does not contain every release for that matter!), but rather
is designed to contain an overview of the significant features added with each release.

A large number of bugs, both inherited and self-inflicted have also been fixed in the various releases.
If you find anything we've missed, please consider submitting an upstream bug directly here:
https://github.com/leezer3/OpenBVE/issues

Please also see the following pages:
------------------------------------

https://github.com/leezer3/OpenBVE/releases - The GitHub releases pages contains a full list of changes from each build.
https://github.com/leezer3/OpenBVE/wiki/Errata - A list of fixed errata which affect the behaviour of existing content.


-------------------------------------------------------------------------------------------------------------------------

2016.06.21- openBVE v1.5.0.1
* New: Graphical/ audio backend ported to openTK.
* New: Replacement Package Managment system added.
* New: Correctly moving bogies added.
* New: Added a 'bug report' dialog for easy access to the in-game logs & crash logs.
* New: in-game map & gradient profile.
* New: Timetable display options improved.

2016.12.15- openBVE v1.5.0.6
* New: Added a Recent Folders option to the route / train browsers.
* New: Added the TrackFollowerFunction for use in animated objects.
* New: Allow the RailType command to be used as a cycle member.

2016.09.25 - openBVE v1.5.0.7
* New: Add the Panel2 type LinearGauge.

2016.11.21- openBVE v1.5.0.8
* New: Dynamic lighting & dynamic backgrounds added.

2016.12.15- openBVE v1.5.0.9
* New: Add several command line switches to allow setting the station, time etc. on load.

2017.03.01- openBVE v1.5.0.10
* New: The 'compatibility' objects have been expanded to include open-source copies of all of the original Uchibo objects, as these are used by many older routes.
* New: Animated files now allow times written in the __HH:MMss__ format to be used in expressions.
* New: The parameter __WrapMode__ (b3d) / __SetWrapMode__  (csv) has been added to allow object authors to control the texture wrapping mode used by the game engine.
* New: The command __Route.StartTime__ has been added. This allows route authors to set the starting time independently of the arrival time at the first station.
* New: Plugins may now interlock the state of the train doors.
* New: Plugins may now add or subtract a score value.

2017.03.31- openBVE v1.5.1.0
* New: Three-part horn sounds are now supported.
* New: A needle declared within a Panel2.cfg file, with the subject of __hour__ , __min__ or __sec__ will now accept the additional parameter __Smoothed__ , to use either smooth or stepped rotations.
* New: Switch sounds based upon the current run index are now supported.
* New: The subjects __Klaxon__ , __PrimaryKlaxon__ , __SecondaryKlaxon__ and __MusicKlaxon__ are now available for animations.
* New: In-game menus may be scrolled using the mouse.
* New: Auto-detection of charsets.

2017.05.05- openBVE v1.5.1.1
* New: Added an in-game gradient display option. (Must be added under Controls for existing users. New installs are set to CTRL + N by default)

2017.05.12- openBVE v1.5.1.2
* New: BIG5 and Windows-1252 charsets added to those auto-detected.

2017.06.28- openBVE v1.5.1.3
* New: XML based markers ( <http://openbve-project.net/documentation/HTML/route_marker.html> ), allowing for text messages and basic time dependent scripting.
* New: When a door button is pressed, *VirtualKeys.DoorsLeft* / *VirtualKeys.DoorsRight* is raised.
* New: Panel2.cfg supports the subjects *doorbuttonl* & *doorbuttonr*
* New: Animated objects support the subject *leftDoorsButton* & *rightDoorsButton*
* New: Joystick buttons limit increased to 64, plus various other changes from openTK backend.

2017.07.22- openBVE v1.5.1.5
* New: Bumped openTK version, better support for various joysticks (e.g. Thrustmaster HOTAS-X)
* New: Object Viewer and Route Viewer support drag + drop.

2017.08.08- openBVE v1.5.1.6
* New: openBVE will load many Loksim3D objects.

2017.09.08- openBVE v1.5.2.0
* New: Add __signal3.csv__ & __crossing.wav__ to the Uchibo compatibility list
* New: Add XML format stations
* New: Add 'Hacks' - Older BVE2 / BVE4 content may require special handling in order to fix issues.

2017.10.01- openBVE v1.5.2.1
* New: Add Legacy Korean (CP949) to auto-detected charsets.

2017.10.19- openBVE v1.5.2.2
* New: Added a parser for objects in the Wavefront .obj format.
* New: Considerable improvements to the .X parser.

2017.11.11- openBVE v1.5.2.3
* New: Handle _BVE1200000_ and _BVE1210000_ format train.dat files.
* New: Add Uchibo V8 objects to the compatibility lists.

2018.01.02- openBVE v1.5.3.0
* New: It is now possible to implement multiple interior views via the train.xml format. (Experimental)
* New: Animated objects now support basic sounds. See here: http://bveworldwide.unlimitedboard.com/t1314p25-animated-objects-following-the-track-now-with-sound
* New: openBVE now supports an event marker overlay (As per Route Viewer)
* New: Route Viewer will now load FLAC sounds.

2018.02.09- openBVE v1.5.3.1
* New: Mirror command for B3D / CSV objects. http://bveworldwide.unlimitedboard.com/t1475-suggestion-mirrored-face-command#16040

2018.03.15- openBVE v1.5.3.2
* New: Added the command **Route.InitialViewpoint** to control the camera view at the start of the sim.
* New: **Destination** variable for train animations & associated routefile command.

2018.03.26- openBVE v1.5.3.3
* New: Object Viewer now uses the texture loader plugins.
* New: Added the EbHandleBehaviour parameter to Train.dat & Train Editor.

2018.04.26- openBVE v1.5.3.4
* New: Allow a delay value to be set for each power / brake notch in Train.dat
* New: Parser for the MSTS Shape (.s) format
* New: Support the MeshVertexColors template in .x files
* New: Basic Kiosk mode
* New: DDS texture loader plugin.

2018.06.13- openBVE v1.5.3.5
* New: Add plugin variable debug display / key trigger. (CTRL+F10 for new installs)

2018.07.03- openBVE v1.5.3.7
* New: Handle MP3 format sounds & WAVE encapsulated MP3.
* New: Add **BVE2060000** to known train.dat formats.
* New: Support offset vertex indexing in the Wavefront obj parser.

2018.08.23- openBVE v1.5.3.8
* New: Add HUD size slider, minimal and large HUD options.
* New: Add the function ***Pitch[CarIndex]*** to animated objects.
* New: Add PerMil to the gradient display options.

2018.10.10- openBVE v1.5.3.9
* New: Added a keybinding to show the distance to the next station stop.
* New: Translation is available for Train Editor.
* New: Added new commands with an optional parameter for brake and power. These allow a specific power or brake notch to be assigned to a button/ keypress.

2018.11.11- openBVE v1.5.3.11
* New: The Pass Alarm sound may be set via sound.cfg
* New: Add input plugins- These are intended to allow a plugin to directly interface with the simulation controls (e.g. desktop controller / sim board)
* New: Add SanYingInput to handle the OH-PC01 cab controller for BVE5. (Ported from the MIT Licensed BVE5 version)

2019.01.05- openBVE v1.5.4.0
* New: Add TGA image loader plugin.
* New: Add two alternative X object parsers.
* New: Add alternative OBJ parser.
* New: Add alternate sounds for when a power / brake handle is moved continously.
* New: Add microphone sound input & associated routefile sound-source.