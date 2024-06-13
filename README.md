# BachelorThesisProject
 
This project is a prototype for an augmented reality system to learn and practice playing electric bass in front of a mirror.
It is not intended for actual use and ought to be considered just proof-of-concept.

This video shows how it works in practice:

INSERT VIDEO

This project supports Windows 10 and 11 only with a Varjo headset with Ultraleap hand tracking, no other interaction methods are supported.

## How to use:

* Clone this repository
* Open this project with Unity 2023.2.16f1 (newer versions may work but are untested)
* Let Unity download all dependencies, when prompted to update scripting APIs select `No`
* When all dependencies are done downloading, a prompt to may appear to warn about enabling both old and new input systems, select `Yes`

Once this is done the project is in working state.

Actual use requires Varjo markers to be set up in the training environment according to the placement in the Unity scene (or vice versa), and an external system to convert from the bass' audio output to MIDI input.

## Bundled libraries:

This project uses a modified version of [alphaTab 1.2.3](https://alphatab.net/) whose source code can be found at https://github.com/J0ySF/alphaTab-1.2.3-mod.
Given Unity's reluctance to support NuGet, all external libraries have been included in the `Assets/DLLs` folder, and come from NuGet (aside from the modified alphaTab version).
