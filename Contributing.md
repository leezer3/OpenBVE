### Contributing Code to openBVE

**Please Note:**
This file is a work in progress, and is intended to serve as a general set of guidelines, not hard and fast rules!

#### Code Style Guidelines

- Please use Unix (CR LF) line terminators.
- Please use tabs, not spaces for indentation for all new code.
- When only changing documentation, include `[ci skip]` in the commit description

#### Code Licencing

The basic licence for openBVE is Public Domain.
Please only contribute code which is licenced under either Public Domain, or loose permissive licences (e.g. BSD-2 and compatible)

### Backwards Compatibility

- Unless not possible (e.g. Plugin interface changes) , backwards compatibility with versions of openBVE below 1.5.0 should be maintained. An error message is fine, but a program crash is not!
- When adding a completely new routefile feature, it is generally preferable to add a new command, rather than extending an existing, as this may be misinterpreted by older versions.

### List of Contributors

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
- Marc Riera
- s520

### Third Party Code

Some parts of OpenBVE and the underlying libraries have been adapted from third party code. If it is covered by a different licence, this and the contributors for this code will be noted in the header of any such file.
A list is also provided here:

- Animated GIF decoder based upon work by Kevin Weiner, FM Software; LZW decoder adapted from John Cristy's ImageMagick
- Some Matrix & Quaternion math code derived from work by the OpenTK team
- DDS decoder based upon work by NVIDIA Corporation
