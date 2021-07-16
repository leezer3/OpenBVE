#!/usr/bin/env bash
#Set base development branch revision numbers
#This needs to be bumped once we have a stable release branch
MajorVersion=1
MinorVersion=5

# cd to correct directory
cd -P -- "$(dirname -- "$0")"

#If we're a tagged commit
if (git describe --tags --exact-match 2> /dev/null) then
	Version=$(git describe --tags)
	InfoVersion=$(git describe --tags)
else
	# determine revision and build numbers
	if [[ "$OSTYPE" == "darwin"* ]]; then
		#OSX
		Revision=$(((($(date +%s) - $(date -jf "%Y-%m-%d" "2016-03-08" $B +%s))/86400 )+40 ))
		Minutes=$(( $(date "+10#%H * 60 + 10#%M") ))
	else
		#Linux & Cygwin
		Revision=$(( ( ($(date "+%s") - $(date --date="2016-03-08" +%s))/(60*60*24) )+40 ))
		Minutes=$(( ( $(date "+%s") - $(date -d "today 0" +%s))/60 ))
	fi

	Version=$MajorVersion.$MinorVersion.$Revision.$Minutes
	InfoVersion=$MajorVersion.$MinorVersion.$Revision.$Minutes-$USER
fi

cat > installers/debian/DEBIAN/control << EOF
Package: openbve
Priority: optional
Section: universe/games
Installed-Size: 14950
Maintainer: leezer3 <leezer3@gmail.com>
Architecture: all
Version: $Version
Provides: bve-engine
Depends: debhelper (>= 9), mono-runtime (>= 5.20.1), libmono-corlib4.5-cil (>= 5.20.1), libmono-system-drawing4.0-cil (>= 1.0), libmono-system-windows-forms4.0-cil (>= 1.0), libmono-system4.0-cil (>= 5.20.1), libmono-system-xml-linq4.0-cil (>= 5.20.1), libmono-i18n4.0-all, libmono-microsoft-csharp4.0-cil (>= 5.20.1), libopenal1, libusb-1.0-0
Recommends: bve-route, bve-train
Homepage: http://openbve-project.net
Description: realistic 3D train/railway simulator (main program)
 OpenBVE is a railway train-driving simulator with an emphasis on
 in-cab driving, realistic physics, braking system and train safety
 system modelling.
 .
 Technically, the simulator handles detailed per-car simulation of the
 brake systems, friction, air resistance, toppling and more. In trains
 supplied with 3D cabs, the driving experience is augmented with
 forces that shake the driver's simulated body upon acceleration and
 braking, as well as through curves.
 .
 Compared to other rail-based simulators, OpenBVE has its main focus on
 realism---not necessarily on user-friendliness. There may be a need
 to study operational manuals for the routes and trains chosen, rather
 than merely memorising a few keystrokes.
 .
 The simulator is designed to be backwards-compatible with existing
 'BVE Trainsim' routes and cab interiors, allowing a wide range of
 existing scenarios to be loaded by a single-program (BVE1, BVE2,
 BVE4 and extended OpenBVE route formats).
 .
 OpenBVE uses OpenGL for 3D graphics rendering, OpenAL for positional
 surround sound, and is written in the C# language.  Note that binary
 train extension plugins are not currently supported on Linux/Unix,
 because these would require Win32 emulation.
EOF