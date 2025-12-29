[![Build Status](https://dev.azure.com/leezer3/OpenBVE/_apis/build/status/leezer3.OpenBVE?branchName=master)](https://dev.azure.com/leezer3/OpenBVE/_build/latest?definitionId=1&branchName=master)

[![Build status](https://ci.appveyor.com/api/projects/status/p4d983eclo738hjo?svg=true)](https://ci.appveyor.com/project/leezer3/openbve)

## OpenBVE Source Code - Readme

This repository contains the source code for the Train Simulator OpenBVE, a 3D cab based simulator.

Supported route formats:
* Native CSV / RW.
* BVE5 TXT format.
* Mechanik DAT format.

Supported train formats:
* BVE2 / BVE4, with native OpenBVE extensions.
* Microsoft Train Simulator (MSTS) - Partial support, work in progress.

OpenBVE is built in OpenGL, using the OpenTK framework for windowing.

### Fixed Errata

These are described fully [here](https://github.com/leezer3/OpenBVE/wiki/Errata).

### Nightly Builds

Automatically generated daily builds are available [here](http://vps.bvecornwall.co.uk/OpenBVE/Builds/).

### Developer Documentation
Documentation for development of add-ons (update version of docs originally written by _michelle_) can be found [here](https://openbve-project.net/documentation_hugo/en/).

### Packages

OpenBVE now supports the installation of 'package files' , which are intended as a replacement for the now defunct managed content. These are described fully [here](http://openbve-project.net/packages/).

### How to build

This build has been tested to compile correctly using Visual Studio 2017 onwards and MonoDevelop. These are described fully [here](Building.md).

### Contributing

Please see the [Contributors File](Contributing.md) for a list of contributors, and basic guidelines for contributing to the development of OpenBVE.


### Links

**Project Website**:

https://openbve-project.net

Project Source Code on GitHub: https://github.com/leezer3/OpenBVE

**Discussion Boards**:

- [BVE Worldwide](http://bveworldwide.forumotion.com)

**Official Project Forum:**

http://bveworldwide.forumotion.com/f14-the-sim-in-time-general-discussion

### License

The original founder of this project, Michelle intended for this program to be placed in the public domain. 
In practice over the last 10+ years I've been maintaining this, we've found that whilst public domain was a noble idea, having no recognised licence and attempting to disclaim copyright tends to produce many of it's own challenges.

As a result, all new code is licenced under BSD-2 or a similar permissive licence (as appropriate)- Please see the source headers.

It is our belief that BSD-2 on **new** code keeps most of original philosophical aims intact, whilst at the same time giving the benfits of a recognised licence.
At some point, the project will probably have to re-licence entirely, but that is somewhere in the future.

In practical terms, what this means is that you can make any modifications to the source like and share your modifications with others.
Please also see the following issue for further discussion on the topic: https://github.com/leezer3/OpenBVE/issues/305

**Third-Party Libraries**

- OpenBVE uses [**CoreFX**](https://github.com/dotnet/corefx). This is licensed under the _MIT License_, which may be found in [here](licenses/CoreFX.txt).
- OpenBVE uses [**CS Script**](https://github.com/oleg-shilo/cs-script) for animation scripting. This is licensed under the _MIT License_, which may be found in [here](licenses/CS-Script.txt).
- OpenBVE uses [**DotNetZip**](https://github.com/haf/DotNetZip.Semverd) for loading compressed DirectX file. This is licensed under the *Microsoft Public License*, which may be found in [here](licenses/DotNetZip.txt).
- OpenBVE uses [**NAudio**](https://github.com/naudio/NAudio) for decoding sound file. This is licensed under the *Microsoft Public License*, which may be found in [here](licenses/NAudio.txt).
- OpenBVE uses [**NAudio.Vorbis**](https://github.com/naudio/Vorbis) for decoding Vorbis file. This is licensed under the *Microsoft Public License*, which may be found in [here](licenses/NAudio.Vorbis.txt).
- OpenBVE uses [**NLayer**](https://github.com/naudio/NLayer) for decoding MP3 file. This is licensed under the *MIT License*, which may be found in [here](licenses/NLayer.txt).
- OpenBVE uses [**NVorbis**](https://github.com/NVorbis/NVorbis) for decoding Vorbis file. This is licensed under the *Microsoft Public License*, which may be found in [here](licenses/NVorbis.txt).
- OpenBVE uses [**OpenTK**](https://github.com/opentk/opentk) library for windowing and input handling. This is licensed under the _Open Toolkit Library License_, which may be found in [here](licenses/OpenTK.txt).
- OpenBVE uses [**ReactiveProperty**](https://github.com/runceel/ReactiveProperty) This is licensed under the *MIT License*, which may be found in [here](licenses/ReactiveProperty.txt).
- OpenBVE uses [**SharpCompress**](https://github.com/adamhathcock/sharpcompress) for archive handling. This is licensed under the _MIT License_, which may be found in [here](licenses/SharpCompress.txt).
- OpenBVE uses [**Reactive Extensions**](https://github.com/dotnet/reactive) This is licensed under the *Apache License, Version 2.0*, which may be found in [here](licenses/ReactiveExtensions.txt).
- OpenBVE uses [**Ude**](https://github.com/yinyue200/ude) for character set detection. This is tri-licensed under the _Mozilla Public License v1.1_, _GPL 2.0_ and _LGPL 2.0_, which may be found in [here](licenses/Ude.txt).
- OpenBVE uses [**XamlBehaviors for WPF**](https://github.com/microsoft/XamlBehaviorsWpf) This is licensed under the *MIT License*, which may be found in [here](licenses/XamlBehaviorsForWPF.txt).
- OpenBVE uses [**TOLK**](https://github.com/dkager/tolk) for screen reader API access. This is licenced under the *LGPL V3* which may be found in [here](licences/TOLK.txt)
- OpenBVE uses [**BVE_Parsing**](https://github.com/leezer3/bve5_parsing) for parsing BVE5 map grammar. This is licenced under the *MIT Licence* which may be found in [here](licences/BVE5.txt)
