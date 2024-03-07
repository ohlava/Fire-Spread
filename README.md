# Fire Spread Simulator

# Description
The Fire Spread Simulator is an application developed in Unity that models the spread of fire in a procedurally generated world. It enables users to observe and analyze how fire propagates through different environments and conditions. The application is built with a set of classes and methods that generate a world, run the fire spread simulation, and visualize the outcome.

This project now offers for example:

- Procedurally generated terrains with vegetation, lakes and rivers
- Customizable terrain sizes and variations
- Moisture map to simulate different climatic conditions
- Vegetation map with different types of vegetation
- Ability to import own custom heigh map from PNG greyscale file
- Fire spread simulation with interactive 3D visualization
- Camera movement and angle change using keyboard and mouse clickable world tiles
- Running and pausing the simulation
- Live graph of the size of fire spread

# Installation
This project runs on the Unity Game Engine. Here are the steps to run the project:

1. Download and install the Unity Hub from [here](https://unity3d.com/get-unity/download)
2. In the Unity Hub, install Unity version 2023.2.0 or later
3. Clone this repository
4. Open the cloned repository folder in Unity Hub
5. Press the play button to start the simulation

# Menu Overview

The Fire Spread Simulator features an intuitive menu system that provides quick access to all its features and settings. Here is an overview of the menu items based on the provided screenshot:

## TUTORIAL
The Tutorial button is designed to guide new users through the basics of the Fire Spread Simulator. Clicking this will provide step-by-step instructions on starting a simulation, understanding the various elements of the interface, and learning how to control the simulation parameters.

## PLAY
...To be added...

## SANDBOX
The Sandbox mode allows for a more freeform interaction with the simulation. Users can experiment with various conditions and settings without the constraints of the standard simulation parameters. It's a space for experimentation and testing hypotheses about fire behavior.

### INFO
The Info section provides some basic information about the application, including user support information. This section can be used to understand more about the simulation's capabilities and background.

### SETTINGS
Access the Settings menu to adjust the application to better fit your needs. Here you can configure various options like autosaving and visual quality.

### QUIT
The Quit option is straightforward; it allows you to exit the application safely. Ensure you have saved all necessary data before quitting, as any unsaved progress may be lost.


# Usage
Once you've installed and opened the project in Unity, you can modify and run the simulations.
In the objects panel, you will see multiple different objects responsible for alternating the program run. Experiment with different values in Inspector panel for such public properties. Change them ideally before you press play button. Control the program with the program buttons right during the run of the program. 

Seting tiles on fire is as simple as clicking on them. Just make sure you are in the game view not in the scene view. Also the state of the game has to be the new World state which means that fire simulation must not be running or paused. Initial burning tiles can be ignited only before you click run simulation button. If the simulation already started, it is prohibited to egnite new tiles manually. Click the Reset current world button or generate entire new World to be able to manually set new tiles on fire again.

After you ignite initial tiles, click the button to run the simulation. Pause and restart the simulation whenever you want. It is also convinient to change the speed of the updates. 

Reset the current world or generate completely new one.

Control the camera movement with WASD. Control the camera angle with IJKL anytime.

Reset camera clicking R, run the simulation using space bar.

If you want to store the map for later, just click save and world will be saved to application persistent data directory. Same for the reverse - import, just name the file "worldSave.json" and click import. If map.png is found in the same directory. This greyscale map is prefered for import as a custom height map. Other custom maps will be added later or are easily addable in the script in import function. 

Have a graph of burning tiles displayed right during the simulation on your screen, or after the simulation finishes or at any time just by clicking the Graph button.

Enjoy and play!

# General overview

### MainLogic

This class is the central hub for running the application, maintaining the state of the application, handling input events, running the fire spread simulation, and managing the generated world and displaying it by visulizer accordingly. It's a Unity MonoBehaviour, which allows it to be attached to a game object and make use of Unity's inbuilt methods like Start and Update.

### Terrain Generation

The WorldGenerator class generates a virtual world for the fire spread simulation.

Adjust the world width and world depth properties to alter the size of the terrain that will be generated.

To use a custom map, save a worldSave.json or map.png in application folder and click the import button.
Just simple locating file you want will be added later version since it is platform specific. Example of two heigh maps is included in the project. Same with the json save.

Set number of rivers you want to generate and amount of lake space etc.

Press the Unity play button to start the simulation and generate the initial terrain. Or later regenerate with the in game buttons.

### FireSpreadSimulation

The FireSpreadSimulation class is responsible for managing the spread of the fire in the simulation. It contains all the necessary methods to run and update the state of the fire spread simulation based on the factors you set.

The SimulationCalendar class keeps track of the current time in the simulation.

The EventLogger class is responsible for logging events that occur during the simulation.

The FireEvent class represents a fire event that occurred during the simulation.

### Fire Spread Visualization

The Visualizer class is responsible for creating and managing the visual representation of the  world simulation.

The Visualizer object allows you to visualize the simulation. It has two modes: Standard and Simplified. Simplified is set automatically if size of the world reaches certain number of tiles to reduce lag.

Standard mode generates a 3D representation of the terrain with vegetation and fire. Each tile of the terrain will have a corresponding GameObject instance.
Simplified mode uses color coding on a 2D grid to represent different states.
You can switch between these modes by changing the mode property on the Visualizer object.

The GraphVisualizer class allows to draw graphs based on a given dataset - number of burning tiles in time frame for example. It provides functionality to visualize data points on a graph panel using Unity's UI system.

# Roadmap
Future enhancements will include:

- Better responsive multiplatform design
- Dynamic weather system influencing the fire spread
- Better fire spread calculation based on more factors - more real
- UI changes and enhancements
- World logger
- and many many more

## Support
For support, please open an issue. You can also contact me via email at ohlava@gmail.com

## Contributing
Contributions are more than welcome. Message me your ideas and thoughts. If found online please fork this repository and create a pull request with your changes.

## Authors and Acknowledgement
Project initiated by [Ondřej Hlava].

## License
None.

## Project Status
The project is in active development with the aim to include more features, simplify and speed up current proccesses etc.