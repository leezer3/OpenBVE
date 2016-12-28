## openBVE Source Code - Readme

This source code is an experimental port of openBVE to the OpenTK framework. 

### Compatibility Changes

There have also been various additions, including to the OpenBveApi, and animated objects which are described fully [here](https://github.com/leezer3/OpenBVE/wiki/Compatibility-Notes).

### Fixed Errata

These are described fully [here] (https://github.com/leezer3/OpenBVE/wiki/Errata).

### Nightly Builds

Automatically generated daily builds are available [here](http://vps.bvecornwall.co.uk/OpenBVE/Builds/).

### Developer Documentation

Documentation for development of add-ons (update version of docs originally written by _michelle_) can be found [here](https://github.com/leezer3/OpenBVE/tree/master/Documentation).

### Packages

openBVE now supports the installation of 'package files' , which are intended as a replacement for the now defunct managed content.
These are described fully [here] (http://openbve-project.net/packages/).

### Notes

This build has been tested to compile correctly using VS2013 and MonoDevelop, but should also compile with SharpDevelop.

Joystick hat support does not function correctly in the current release build of OpenTK (1.1.4). Please compile OpenTK yourself from the most recent source if you wish to use this feature.

### Contributors

**OpenBVE**:
- michelle
- odakyufan
- Anthony Bowden
- Paul Sladen

**OpenTK Port and Continuing Development**:

- Christopher Lees
- Jakub Vanek
- Maurizo M. Gavioli
- Connor Fitzgerald

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
