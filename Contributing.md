### Contributing Code to openBVE

**Please Note:**
This file is a work in progress, and is intended to serve as a general set of guidelines, not hard and fast rules!

#### Code Style Guidelines

- Please use Unix (CR LF) line terminators.
- Please use tabs, not spaces for indentation for all new code.
- When only changing documentation, include `[ci skip]` in the commit description

#### Code Licencing

The original codebase for OpenBVE is licenced under the public domain. (Please see https://github.com/leezer3/OpenBVE/issues/305  for further details and discussion)

However, over the last 10+ years, we've found that not having a recognised OSS licence can be more of a hindrance than a help.


With this in mind, we've chosen to move to using the BSD-2 clause licence for newer code contributions.

We're also happy to accept code under similar loose, permissive licences that maintain the spirit of the original public domain aims. 


New code should clearly state the author and licence (if applicable) in the file header.


Please do not contribute code under the GPL or simiar 'viral' or restrictive licences.


### Backwards Compatibility

- Unless not possible (e.g. Plugin interface changes) , backwards compatibility with versions of OpenBVE below 1.5.0 should be maintained. An error message is fine, but a program crash is not!
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
- Kenny Hui

### Third Party Code

Some parts of OpenBVE and the underlying libraries have been adapted from third party code. If it is covered by a different licence, this and the contributors for this code will be noted in the header of any such file.
A list is also provided here:

- Animated GIF decoder based upon work by Kevin Weiner, FM Software; LZW decoder adapted from John Cristy's ImageMagick
- Some Matrix & Quaternion math code derived from work by the OpenTK team
- DDS decoder based upon work by NVIDIA Corporation
