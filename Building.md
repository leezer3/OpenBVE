# Building openBVE from source

This file is a WIP set of instructions for building OpenBVE from source.

## Prerequisites

### Windows

#### When to use .NET Framework

- Visual Studio 2017 or later
- .NET Framework 4.7.2 or later

#### When to use Mono

- Mono 6.8.0 or later
- NuGet client 2.16 or later

### Linux

- Mono 6.8.0 or later
- NuGet client 2.16 or later
- OpenAL
- GNU Make
- debhelper (Debian and compatibles only)

#### Debian and compatibles

NOTE: You need to get Mono from [the Mono project repository](https://www.mono-project.com/download/stable/#download-lin), not the distribution repository.

```bash
sudo apt install build-essential mono-devel libmono-i18n4.0-all nuget libopenal1 debhelper
```

#### RHEL and compatibles

NOTE: You need to get Mono from [the Mono project repository](https://www.mono-project.com/download/stable/#download-lin), not the distribution repository.

```bash
sudo dnf install @"Development Tools" mono-devel mono-locale-extras nuget openal-soft
```

#### Reference information

You can install the latest NuGet client using the command below.

```bash
sudo curl -o /usr/local/lib/nuget.exe https://dist.nuget.org/win-x86-commandline/latest/nuget.exe
echo -e '#!/bin/sh\nexec /usr/bin/mono /usr/local/lib/nuget.exe "$@"' | sudo tee /usr/local/bin/nuget
sudo chmod 755 /usr/local/bin/nuget
```

### Required Mono Components

NOTE: Depending on the install type you select, Mono will not install all components by default. These are required:

- mono-runtime
- libmono-corlib4.5-cil
- libmono-system-drawing4.0-cil
- libmono-system-windows-forms4.0-cil
- libmono-system4.0-cil
- libmono-system-xml-linq4.0-cil
- libmono-i18n4.0-all
- libmono-microsoft-csharp4.0-cil

#### Required Additional System Libraries

- libusb-1.0
- fonts-noto-cjk [Optional- Gives a better Unicode glyph set in menus etc.]
- libsdl2 [Optional- Required if SDL2 backend is to be used]


### Mac

- Mono 6.8.0 or later
- NuGet client 2.16 or later
- OpenAL
- GNU Make

## Building

**Note that as NuGet packages are used, the first-time build requires an internet connection.**

### Windows

#### When to use .NET Framework

1. Open the main OpenBVE.sln file with Visual Studio.
2. Build the required project, allowing NuGet to restore the packages as required.

#### When to use Mono

1. Start "Open Mono Command Prompt" from the start menu.
2. Open the main openBVE source directory in this terminal.
3. Restore the packages as required. `nuget restore OpenBVE.sln`
4. Build the solution. `msbuild OpenBVE.sln`

### Mac / Linux

1. Open the main openBVE source directory in the terminal.
2. You may either build using the makefile, which supports the following options:
   - `make` - Restores the NuGet packages only.
   - `make all-release` - Creates a release build.
   - `make all-debug` - Creates a debug build.
   - `make clean-all` - Cleans release and debug builds.
   - `make openbve-release` - Creates a release build without tools.
   - `make openbve-debug` - Creates a debug build without tools.
   - `make debian` - On Debian and compatibles, creates an installable deb package.
   - `make publish` - On Mac, creates an app package.
