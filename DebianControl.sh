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
Depends: mono-runtime (>= 3.2.8), libmono-corlib4.5-cil (>= 3.2.8), libmono-system-drawing4.0-cil (>= 1.0), libmono-system-windows-forms4.0-cil (>= 1.0), libmono-system4.0-cil (>= 3.2.8), libmono-i18n4.0-all, libopenal1
Recommends: bve-route, bve-train
Description: realistic 3D train/railway simulator (main program)
Homepage: http://openbve-project.net
EOF