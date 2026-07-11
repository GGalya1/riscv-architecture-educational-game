# Von Gamification zu Game-Based Learning
### Konzeption und Implementierung eines mobilen Lernspiels für den Mehrtaktsprozessor

[![Unity Version](https://img.shields.io/badge/Unity-6-rtl.svg?logo=unity)](https://unity.com/)
[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0)
![Platform](https://img.shields.io/badge/Platform-Android%20%7C%20Windows%20%7C%20Linux-blueviolet)

This repository contains the source code and Unity project for my Bachelor's Thesis at **TU Darmstadt**. 

The project is an educational mobile game designed to teach the fundamentals of computer organization, specifically focusing on the multi-cycle processor (Mehrtaktprozessor).

---

## Play the Game
If you want to play the game without opening the project in Unity, you can find the pre-built versions (APK for Android and EXE for Windows) here:

**[Download Latest Release (v0.8.2)](https://github.com/GGalya1/Bachelor-project-at-TU-Darmstadt/releases/tag/v0.8.2)**

---

## Project Overview

**Mehrtakt-Abenteuer** is an adventure through the layers of computer architecture. Players must master various components to eventually understand and build the "Great and Terrible" Multi-cycle Processor.

### Key Features:
* **31 Levels** across **8 Educational Chapters**:
    1.  **Register** – The basics of data storage.
    2.  **Multiplexer** – The art of selection.
    3.  **ALU** – The core of calculations.
    4.  **Memory** – Where instructions and data reside.
    5.  **Extender** – Preparing data for processing.
    6.  **Register File** – Managing multiple registers.
    7.  **Single-Cycle Processor** – Putting it all together in a single cycle. *(Work in Progress)*
    8.  **Multi-Cycle Processor** – The ultimate goal!
* **Companion System:** Meet **Charlie**, your digital assistant. She provides hints, explains theory, and allows players to choose between deep-dive explanations or jumping straight into the action.
* **Interactive Learning:** Concepts are taught through gameplay mechanics rather than just passive reading.

---

## Technical Specifications

* **Engine:** Unity 6 (Version: 6000.4.10f1)
* **Target Platforms:** Android (Primary), Windows (Evaluation Build)
* **Render Pipeline:** Universal Render Pipeline (URP)
* **Version Control:** Transitioned from Unity Version Control to Git.

---

## How to Open the Project

To explore the source code or build the project yourself:

1.  Install **Unity Hub**.
2.  Install **Unity 6 (6000.4.10f1)**.
3.  Clone this repository:  
    `git clone https://github.com/GGalya1/Bachelor-project-at-TU-Darmstadt.git`
4.  Add the project to Unity Hub and open it.
5.  *Note:* The initial import may take several minutes as Unity generates the `Library` folder.

---

## Building the Android APK Yourself

Android builds require a signing keystore, which is **not included** in this repository for security reasons (it is tied to the published Google Play identity). To build the project yourself, you have two options:

**Option A: Quick test build (recommended for just trying it out):**
1. Open `Edit -> Project Settings -> Player -> Publishing Settings`.
2. Make sure **Custom Keystore** is unchecked.
3. Build the project as usual, Unity will use a default debug keystore automatically.

**Option B: Build with your own signing key:**
1. Open `Edit -> Project Settings -> Player -> Publishing Settings`.
2. Check **Custom Keystore**, then open **Keystore Manager > Create New** and set your own keystore/alias passwords.
3. Build the project, the resulting APK will be signed with your own key.

*Note:* Builds you create yourself will not be signed with the same key as the official release, so they cannot be used to update an installation of the Play Store version -- but they will install and run normally as a standalone APK.

---

## Project Structure
* `Assets/Scripts`: All C# logic and gameplay systems.
* `Assets/Prefabs`: Reusable game objects and UI elements.
* `Assets/Scenes`: The 8 chapters and 31 levels.
* `ProjectSettings`: Necessary engine configurations (Input System, Tags, Layers).
