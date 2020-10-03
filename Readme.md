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

openBVE now supports the installation of 'package files' , which are intended as a replacement for the now defunct managed content. These are described fully [here](http://openbve-project.net/packages/).

### How to build

This build has been tested to compile correctly using Visual Studio 2017 onwards and MonoDevelop. These are described fully [here](Building.md).

### Contributing

Please see the [Contributors File](Contributing.md) for a list of contributors, and basic guidelines for contributing to the development of openBVE.


### Links

**Project Website**:

https://openbve-project.net

Project Source Code on GitHub: https://github.com/leezer3/OpenBVE

**Discussion Boards**:

- [BVE Worldwide](http://bveworldwide.forumotion.com)
- [BVE-Terminus](http://www.bve-terminus.org/forum)
- [UKTrainsim](http://forums.uktrainsim.com/viewforum.php?f=66)

**Official Project Forum:**

http://bveworldwide.forumotion.com/f14-the-sim-in-time-general-discussion

### License

Michelle intended for this program to be placed in the public domain. This means that you can make any modifications to it you like and share your modifications with others.
Please also see the following issue for further discussion on the topic: https://github.com/leezer3/OpenBVE/issues/305

**Third-Party Libraries**

openBVE uses the following third party libraries.

| Name                                                                        | License                                                               | Usage                           |
| --------------------------------------------------------------------------- | --------------------------------------------------------------------- | ------------------------------- |
| [**CS Script**](https://github.com/oleg-shilo/cs-script)                    | [MIT License](licenses/CS-Script.txt)                                 | Animation scripting             |
| [**.NET Runtime**](https://github.com/dotnet/runtime)                       | [MIT License](licenses/dotnet_runtime.txt)                            |                                 |
| [**DotNetZip**](https://github.com/haf/DotNetZip.Semverd)                   | [Microsoft Public License](licenses/DotNetZip.txt)                    | Loading compressed DirectX file |
| [**Json.NET**](https://github.com/JamesNK/Newtonsoft.Json)                  | [MIT License](licenses/Newtonsoft.Json.txt)                           | Loading JSON file               |
| [**Namotion.Reflection**](https://github.com/RicoSuter/Namotion.Reflection) | [MIT License](licenses/Namotion.Reflection.txt)                       |                                 |
| [**NAudio**](https://github.com/naudio/NAudio)                              | [Microsoft Public License](licenses/NAudio.txt)                       | Decoding sound file             |
| [**NAudio.Vorbis**](https://github.com/naudio/Vorbis)                       | [Microsoft Public License](licenses/NAudio.Vorbis.txt)                | Decoding Vorbis file            |
| [**NJsonSchema for .NET**](https://github.com/RicoSuter/NJsonSchema)        | [MIT License](licenses/NJsonSchema)                                   | Validating JSON file            |
| [**NLayer**](https://github.com/naudio/NLayer)                              | [MIT License](licenses/NLayer.txt)                                    | Decoding MP3 file               |
| [**NVorbis**](https://github.com/NVorbis/NVorbis)                           | [Microsoft Public License](licenses/NVorbis.txt)                      | Decoding Vorbis file            |
| [**OpenAL Soft**](https://github.com/kcat/openal-soft)                      | [LGPL 2.0](licenses/OpenALSoft.txt)                                   | Playing sound                   |
| [**OpenTK**](https://github.com/opentk/opentk)                              | [Open Toolkit Library License](licenses/OpenTK.txt)                   | Windowing and input handling    |
| [**Prism**](https://github.com/PrismLibrary/Prism)                          | [MIT License](licenses/Prism.txt)                                     |                                 |
| [**Reactive Extensions**](https://github.com/dotnet/reactive)               | [Apache License, Version 2.0](licenses/ReactiveExtensions.txt)        |                                 |
| [**ReactiveProperty**](https://github.com/runceel/ReactiveProperty)         | [MIT License](licenses/ReactiveProperty.txt)                          |                                 |
| [**SharpCompress**](https://github.com/adamhathcock/sharpcompress)          | [MIT License](licenses/SharpCompress.txt)                             | Archive handling                |
| [**Ude**](https://github.com/yinyue200/ude)                                 | [Mozilla Public License v1.1, GPL 2.0 and LGPL 2.0](licenses/Ude.txt) | Character set detection         |

