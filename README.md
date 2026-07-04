# Von Gamification zu Game-Based Learning
### Konzeption und Implementierung eines mobilen Lernspiels für den Mehrtaktsprozessor

This repository contains the source code and Unity project for my Bachelor's Thesis at **TU Darmstadt**. 

The project is an educational mobile game designed to teach the fundamentals of computer organization, specifically focusing on the multi-cycle processor (Mehrtaktprozessor).

---

## Play the Game
If you want to play the game without opening the project in Unity, you can find the pre-built versions (APK for Android and EXE for Windows) here:

**[Download Latest Release (v0.7.6)](https://github.com/GGalya1/Bachelor-project-at-TU-Darmstadt/releases/tag/v0.7.6)**

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

## Project Structure
* `Assets/Scripts`: All C# logic and gameplay systems.
* `Assets/Prefabs`: Reusable game objects and UI elements.
* `Assets/Scenes`: The 8 chapters and 31 levels.
* `ProjectSettings`: Necessary engine configurations (Input System, Tags, Layers).
