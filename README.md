# FireSpread Simulation

# Description
The Fire Spread Simulator is an application developed in Unity that models the spread of fire in a procedurally generated world. It enables users to observe and analyze how fire propagates through different environments and conditions. (factors such as vegetation type, moisture levels, weather, height difference) The application is built with a set of classes and methods that generate a world, run the fire spread simulation, and visualize the outcome. (note that in this stage it is very simplified, but easily modifiable and extandable)

This project now offers for example:

- Procedurally generated terrains with vegetation, lakes and rivers
- Customizable terrain sizes and variations
- Moisture map to simulate different climatic conditions
- Vegetation map with different types of vegetation
- Ability to import own custom heigh map from PNG greyscale file
- Fire spread simulation with interactive 3D visualization
- Camera movement and clickable world tiles
- Running and pausing simulation
- Graph of fire spread - serialization into file to make charts later in python for example.

# Installation
This project runs on the Unity Game Engine. Here are the steps to run the project:

1. Download and install the Unity Hub from [here](https://unity3d.com/get-unity/download)
2. In the Unity Hub, install Unity version 2023.2.0 or later
3. Clone this repository
4. Open the cloned repository folder in Unity Hub
5. Press the play button to start the simulation

# Usage
Once you've installed and opened the project in Unity, you can modify and run the simulations.

Press the play button!

In the objects panel, you will see multiple different objects responsible for alternating the program run. Experiment with different values in Inspector panel for those public properties. Or control the program with the buttons with self explanatory texts. Change them ideally before you press play button.

Seting tiles on fire is as simple as clicking on them! Just make sure you are in the game not in the scene view. Also the state of the game has to be in new world state which means that fire simulation must not be running or paused. If the simulation already started, it is prohibited to egnite new tiles manually. (maybe will be added in the future) Click the Reset button or generate new World to be able to manually set tile on fire. 

After you ignite initial tiles, click the button to run the simulation. Pause and restart the simulation whenever you want. It is also convinient to change the speed of the updates. 

Reset the current world or generate completely new one.

Control the camera movement with WASD. Control the camera angle with IJKL anytime.

# General overview

### MainLogic

This class is the central hub for running the application, maintaining the state of the application, handling input events, running the fire spread simulation, and managing the generated world. It's a Unity MonoBehaviour, which allows it to be attached to a game object and make use of Unity's inbuilt methods like Start and Update.

### Terrain Generation

The WorldGenerator class generates a virtual world for the fire spread simulation.

Adjust the worldWidth and worldDepth properties to alter the size of the terrain that will be generated.

To use a custom map, set the useCustomMap boolean to true and connect your custom map to the mapImporterObj property. Example of two heigh maps is included in the project.

Set number of rivers you want to generate etc.

Press the Unity play button to start the simulation and generate the initial terrain. Or later with the buttons.

### FireSpreadSimulation

The FireSpreadSimulation class is responsible for managing the spread of the fire in the simulation. It contains all the necessary methods to run and update the state of the fire spread simulation.

The SimulationCalendar class keeps track of the current time in the simulation.

The EventLogger class is responsible for logging events that occur during the simulation.

The FireEvent class represents a fire event that occurred during the simulation.

### Fire Spread Visualization

The Visualizer class is responsible for creating and managing the visual representation of the  world simulation.

The Visualizer object allows you to visualize the simulation. It has two modes: Standard and Simplified. Simplified is set automatically if size of the world reaches certain number of tiles.

Standard mode generates a 3D representation of the terrain with vegetation and fire. Each tile of the terrain will have a corresponding GameObject instance.
Simplified mode uses color coding on a 2D grid to represent different states.
You can switch between these modes by changing the mode property on the Visualizer object. Note that the mode should not be changed during the simulation.


# Fire Spread Simulator Documentation

## Main Logic 

### some Variables

- `world`: The instance of the `World` object that represents the generated environment.
- `fireSpreadSimulation`: An instance of the `FireSpreadSimulation` class that manages the fire spreading logic.
- `initBurningTiles`: A list of `Tile` objects that are initialized to be burning at the start of the simulation.
- `worldGenerator`, `visulizer`, `inputHandler`
- `elapsed`: A float that measures the elapsed time to control the update frequency of the simulation.
- `speedOfUpdates`: Determines how frequently the simulation should update (in seconds).
- `fireSpreadParams`: A `FireSpreadParameters` object that holds the parameters used by the fire spread simulation.
- `currentState`: A `State` enum that represents the current state of the application.

### Methods

#### `HandleCameraMove(Vector3 direction)` 
Handles the movement of the camera.

#### `HandleCameraAngleChange(Vector3 rotationChange)`
Handles the rotation change of the camera.

#### `HandleTileClick(Tile clickedTile)`
Handles the events when a tile is clicked. It's noteworthy that tiles can be clicked only when the simulation is not running.

#### `HandleEvent(State nextState)`
This method is responsible for handling state transitions in the application based on the user's actions. The transitions are between `NewWorldState`, `RunningState`, `StoppedState`, and `GraphState`. 

#### `OnNewWorldButtonClicked()`, `OnRunSimulationButtonClicked()`, `OnPauseSimulationButtonClicked()`, `OnShowGraphsButtonClicked()`, `OnResetButtonClicked()`
These methods handle the events when respective buttons are clicked in the user interface.

#### `ToggleUseCustomMap()`
Toggles the `useCustomMap` variable, changing whether a custom or generated map is used in the simulation.

#### `GenereteNewWorld()`
Generates a new world and initializes the fire spread simulation for this world.

#### `VisulizerRemakeAllBrandNew()`
Resets and re-creates all visualizations.

#### `SaveGraph()`
Saves the graph data that shows the number of burning tiles over time.

#### `RunEverything()`
Executes all the necessary updates based on the current state of the application.

## Enum: State
This enum contains the four possible states that the application can be in:

- `NewWorldState`: The application is in a state where a new world has been or is being generated, here initialBurningTiles can be set by clicking on tiles (Raycast is sent).
- `RunningState`: The fire spread simulation is currently running.
- `StoppedState`: The fire spread simulation has been paused or has finished running.
- `GraphState`: The application should be in a state where it displays the graph of burning tiles over time. (currently it just saves it to a file)


## FireSpreadSimulation

### Variables

- `_parameters`: An instance of the `FireSpreadParameters` class, which holds the parameters used for fire spreading.
- `_world`: An instance of the `World` class, which represents the simulated world.
- `_burningTiles`: A list of `Tile` objects that are currently burning.
- `_calendar`: An instance of the `SimulationCalendar` class, which keeps track of the current time of the simulation.
- `_eventLogger`: An instance of the `EventLogger` class, which logs events that occur during the simulation.

### Methods

#### `Finished()`
This method checks if the simulation is finished.

#### `Update()`
This method advances the simulation calendar and updates the state of the burning tiles.

#### `GetLastUpdateEvents()`
This method retrieves a list of fire events that occurred during the last update.

#### `GetBurningTilesOverTime()`
This method retrieves a dictionary that maps time to the number of burning tiles at that time.

#### `CalculateFireSpreadProbability(Tile source, Tile target, Weather weather, FireSpreadParameters parameters)`
This method calculates the probability of the fire spreading from the source tile to the target tile.

#### `GetStepFireProbability(float totalProbability, int BurnTime)`
This method calculates the per-step probability of a tile catching fire, such that over the tile's `BurnTime`, the total cumulative probability of catching fire is equal to the specified total probability.

#### `GetVegetationFactor(VegetationType vegetation, float spreadFactor)`, `GetMoistureFactor(float moisture, float spreadFactor)`, `GetWindFactor(Tile source, Tile target, Weather weather, float spreadFactor)`, `GetSlopeFactor(Tile source, Tile target, float spreadFactor)`
These methods are intended to calculate the factors affecting fire spread based on vegetation, moisture, wind, and slope. They are not actively used right now.

## Class: EventLogger

### Variables

- `_events`: A dictionary where the key is the time of an event and the value is a list of `FireEvent` objects that happened at that time.

### Methods

#### `LogEvent(FireEvent evt)`
This method logs a `FireEvent` at the current time.

#### `GetLastUpdateEvents(int time)`
This method retrieves a list of fire events that occurred at a specified time.

#### `GetBurningTilesOverTime()`
This method retrieves a dictionary that maps time to the number of burning tiles at that time.

## SimulationCalendar

### Variables

- `CurrentTime`: The current time in the simulation.

### Methods

#### `AdvanceTime()`
This method advances the current time in the simulation.

## FireEvent

### Variables

- `Time`: The time when the event occurred.
- `Type`: The type of event that occurred, represented by the `EventType` enum.
- `Tile`: The `Tile` object where the event occurred.

## EventType

The `EventType` enum represents the types of fire events that can occur:

- `StartedBurning`: Represents a tile starting to burn.
- `StoppedBurning`: Represents a tile stopping burning.


## WorldGenerator

The `WorldGenerator` class generates a virtual world for the fire spread simulation.

### Methods

#### `GetWorld()`
This method is responsible for generating a world by combining various maps, including a height map, a lake map, a river map, a moisture map, and a vegetation map. If we use custom height map, all of other maps are automatically generated and then all combined.

#### `GenerateBaseTerrain()`
This method generates a base terrain for the world.

#### `GenerateLakes(float[,] heightMap)`
This method generates lakes on the terrain based on the height map.

#### `GenerateRivers(float[,] heightMap, int[,] lakeMap)`
This method generates rivers on the terrain based on the height map and lake map. Rivers ends in lakes.

#### `GenerateCombinedMap(float[,] heightMap, int[,] lakeMap, int[,] riverMap)`
This method combines the height, lake, and river maps into a single map.

#### `GenerateMoistureMap(float[,] heightMap, int[,] lakeMap, int[,] riverMap)`
This method generates a moisture map based on the height, lake, and river maps.

#### `GenerateVegetationMap(int[,] moistureMap)`
This method generates a vegetation map based on the moisture map.

#### `GenerateWorldFromMaps(float[,] heightMap, int[,] moistureMap, VegetationType[,] vegetationMap)`
This method generates the final world from the height, moisture, and vegetation maps.

## Visualizer

The `Visualizer` class is responsible for creating and managing the visual representation of the simulation world.

### Methods

#### `CreateWorldTiles(World world)`
This method creates tiles for the world and assigns appropriate colors based on their features.

#### `MakeTileBurned(Tile tile)`
This method changes the visual representation of a tile to show that it is burned.

#### `SetColorOnTile(Tile tile, Color color)`
This method changes the color of a tile in the visualization.

#### `SetAppropriateColor(Tile tile)`
This method sets the color of a tile based on its features (water or vegetation type).

#### `CreateAllVegetation(World world)`
This method creates the visual representation of vegetation in the world.

#### `CreateVegetationOnTile(Tile tile, VegetationType vegetation)`
This method creates the visual representation of vegetation on a specific tile.

#### `DestroyVegetationOnTile(Tile tile)`
This method destroys the visual representation of vegetation on a specific tile.

#### `CreateFireOnTile(Tile tile)`
This method creates a fire object on a tile in the visualization.

#### `DestroyFireOnTile(Tile tile)`
This method destroys the fire object on a tile in the visualization.

#### `DestroyAllVegetation()`
This method destroys all the vegetation objects in the visualization.

#### `DestroyAllFire()`
This method destroys all the fire objects in the visualization.

#### `DestroyAllTile()`
This method destroys all tile objects in the visualization.

#### `GetWorldTileFromInstance(GameObject instance)`
This method retrieves the `Tile` object associated with a specific GameObject instance.

#### `SetCameraPositionAndOrientation(World world)`
This method sets the position and orientation of the camera in the visualization.



# Roadmap
Future enhancements will include:

- World logger
- Dynamic weather system influencing the fire spread
- More types of vegetation and their unique impact on the fire spread
- Better fire spread calculation based on more factors
- UI changes and enhancements, more settings options implemented by buttons, sliders, toggles...
- and many many more

## Support
For support, please open an issue. You can also contact me via email at ohlava@gmail.com

## Contributing
Contributions are more than welcome. Please fork this repository and create a pull request with your changes. Or just message me your ideas and thoughts.

## Authors and Acknowledgement
Project initiated by [Ondřej Hlava].

## License
None.

## Project Status
The project is in active development with the aim to include more features, simplify current proccesses etc.