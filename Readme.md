[![Build Status](https://travis-ci.org/leezer3/OpenBVE.svg?branch=master)](https://travis-ci.org/leezer3/OpenBVE)

[![Build status](https://ci.appveyor.com/api/projects/status/p4d983eclo738hjo?svg=true)](https://ci.appveyor.com/project/leezer3/openbve)

## openBVE Source Code - Readme

This source code started out life as a port of openBVE to the OpenTK framework, but now has begun major development work to improve the sim and the possibilities it presents.

### Compatibility Changes

A general overview of compatibility changes, including to the OpenBveApi, and animated objects are described fully [here](https://github.com/leezer3/OpenBVE/wiki/Compatibility-Notes).

### Fixed Errata

These are described fully [here](https://github.com/leezer3/OpenBVE/wiki/Errata).

### Nightly Builds

Automatically generated daily builds are available [here](http://vps.bvecornwall.co.uk/OpenBVE/Builds/).

### Developer Documentation

Documentation for development of add-ons (update version of docs originally written by _michelle_) can be found [here](https://github.com/leezer3/OpenBVE/tree/master/Documentation).

### Packages

openBVE now supports the installation of 'package files' , which are intended as a replacement for the now defunct managed content.
These are described fully [here](http://openbve-project.net/packages/).

### Notes

This build has been tested to compile correctly using VS2013 and MonoDevelop, but should also compile with SharpDevelop.

Joystick hat support does not function correctly in the current release build of OpenTK (1.1.4). Please compile OpenTK yourself from the most recent source if you wish to use this feature.

### Contributing

Please see the [Contributors File](Contributing.md) for a list of contributors, and basic guidelines for contributing to the development of openBVE.


### Links

**Project Website**:

http://www.openbve-project.net

Project Source Code on GitHub: https://github.com/leezer3/OpenBVE

**Discussion Boards**:

- [BVE Worldwide](http://bveworldwide.unlimitedboard.com)
- [BVE-Terminus](http://www.bve-terminus.org/forum)
- [UKTrainsim](http://forums.uktrainsim.com/viewforum.php?f=66)

**Official Project Forum:**

http://bveworldwide.unlimitedboard.com/f14-the-sim-in-time-general-discussion

### License

Michelle intended for this program to be placed in the public domain. This means that you can make any modifications to it you like and share your modifications with others.

**Third-Pary Libraries**

- openBVE uses the **OpenTK** library for windowing and input handling. This is licenced under the _Open Toolkit Library Licence_, which may be found in `OpenTKLicence.txt`.
- openBVE uses the **CS Script Library** for animation scripting. This is licened under the _MIT Licence_, which may be found in `CSScriptLicence.txt`.
- openBVE uses **SharpCompress** for archive handling. This is licenced under the _MIT Licence_, which may be found in `SharpCompressLicence.txt`.
- openBVE uses **NUinversalCharDetect** for character set detection. This is tri-licenced under the _Mozilla Public Licence v1.1_, _GPL 2.0_ and _LGPL 2.0_
