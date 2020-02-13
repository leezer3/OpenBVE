### Building openBVE from source

This file is a WIP set of instructions for building openBVE from source.
Note that as Nuget packages are used, the first-time build requires an internet connection.

## Prequisites

# Windows
Either .Net 4.6.1 or Mono 5.20.1 or later, x86 or x64

# Linux
Mono 5.20.1 or later, and the following packages:
mono-runtime
libmono-corlib4.5-cil
libmono-system-drawing4.0-cil
libmono-system-windows-forms4.0-cil
libmono-system4.0-cil
libmono-system-xml-linq4.0-cil
libmono-i18n4.0-all
libmono-microsoft-csharp4.0-cil
debhelper
libopenal1

# Mac
Mono 5.20.1 or later, x86 only.


## Building

# Windows
Open the main OpenBVE.sln file with Visual Studio.
Build the required project, allowing nuget to restore the packages as required.

# Mac / Linux

Open the main openBVE source directory in the terminal.
You may either build using the makefile, which supports the following options:
* make - Restores the nuget packages only.
* make all-release - Creates a release build.
* make all-debug - Creates a debug build.
* make debian - On Debian and compatibles, creates an installable deb package.
* make publish - On Mac, creates an app package.
