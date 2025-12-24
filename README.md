# Unity-3d-Movements
`Unity Movement 3D` is a library of common movement player Controler scripts k. You can use these scripts to help your playerr move around your game.

## Contents
- [x] Walking 
- [x] Slide 
- [x] Crouch
- [x] WallRun
- [x] Wall Jump
- [x] Wall Climb
- [x] Wall Climb
- [ ] Wall Grab
- [ ] Dash
- [ ] Grappling
- [X] For Multiplayer

# Multiplayer Third Person Character Controller

This project is based on an older Unity character controller script originally found on GitHub.
The script has been extended and adapted to work in a multiplayer environment, while preserving the original structure and logic as much as possible.

The main focus of this implementation is to support Third Person movement with basic multiplayer compatibility.

✨ Features & Changes

Adapted for Third Person camera and movement mechanics

Added multiplayer awareness:

Clear distinction between local player and remote players

Input handling and movement logic run only on the local player

Code structure is:

Readable

Maintainable

Easy to extend for future improvements

⚙️ Current State

The controller works overall and follows correct core multiplayer principles

Some aspects are still not fully optimized, including:

Movement synchronization

Network latency handling

Animation synchronization

The implementation should be considered experimental and not production-ready

🎯 Purpose

This project aims to:

Demonstrate how a single-player character controller can be adapted for multiplayer usage

Provide a solid starting point for further development and optimization

Serve as a learning reference for multiplayer character controller logic in Unity

⚠️ Notes

This controller does not include advanced networking features such as client-side prediction, interpolation, or authoritative server logic.
Additional work is recommended for stable and competitive multiplayer gameplay.
