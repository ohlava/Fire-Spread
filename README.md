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

# Usage
Once you've installed and opened the project in Unity, you can modify and run the simulations.

Press the play button!

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


# Fire Spread Simulator Documentation

## Base classes

### `World` Class

### Properties:

*   `Width`: Get or set the width of the world grid. The minimum value is 0.
*   `Depth`: Get or set the depth of the world grid. The minimum value is 0.

### Methods:

#### `World(int width, int depth)`

Constructor that initializes a new instance of the World class with a given width and depth and initializes the `Grid` array.

#### `GetTileAt(int x, int y) -> Tile`

Returns the tile at the specified position in the grid.

#### `GetNeighborTiles(Tile tile) -> IEnumerable<Tile>`

Returns a list of neighboring tiles given some tile.

#### `Reset()`

Resets the world by setting non-static attributes for all tiles to their default states.

#### `Save()`

Saves the current state of the world to a file in JSON format.

#### `Load() -> World`

Loads the world from a saved file in JSON format.

### `Tile` Class

### Properties:

*   `WidthPosition`: The width position of the tile in the world grid.
*   `DepthPosition`: The depth position of the tile in the world grid.
*   `Height`: The height of the tile. Minimum value is 0.
*   `Moisture`: Moisture level of the tile, with values ranging from 0 to 100.
*   `Vegetation`: The type of vegetation on the tile.
*   `BurnTime`: The time it takes for the tile to burn.
*   `BurningFor`: The duration the tile has been burning for.
*   `IsBurning`: If the tile is currently burning.

### Methods:

#### `Tile(float height, int moisture, VegetationType vegetation, int positionX, int positionY)`

Constructor that initializes a new instance of the Tile class.
#### `Ignite() -> bool`

Attempts to ignite the tile, if conditions allow.
Returns: `true` if ignition was successful, `false` otherwise.

#### `Extinguish()`

Extinguishes the fire on the tile and sets its state to burned.

### `Weather` Class

### Properties:

*   `WindDirection`: Get or set the wind direction in degrees. Values should be between 0 and 359.
*   `WindStrength`: Get or set the strength of the wind in km/h. Values should be between 0 and 100.

### Methods:

#### `Weather(float windDirection, float windStrength)`

Constructor that initializes a new instance of the Weather class.

### `SerializableConversion` Class

### Static Methods:

#### `ConvertToWorldSerializable(World world) -> SerializableWorld`

Converts a World object to a serializable format.

#### `ConvertFromWorldSerializable(SerializableWorld serializableWorld) -> World`

Converts a serialized World object back to its original form.

#### `ConvertToTileSerializable(Tile tile) -> SerializableTile`

Converts a Tile object to a serializable format.

#### `ConvertFromTileSerializable(SerializableTile serializableTile) -> Tile`

Converts a serialized Tile object back to its original form.


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
Handles the movement of the camera. Move camera with WASD keyboard buttons. 

#### `HandleCameraAngleChange(Vector3 rotationChange)`
Handles the rotation change of the camera. Change camera angle with IJKL keyboard buttons. 

#### `HandleTileClick(Tile clickedTile)`
Handles the events when a tile is clicked. It's noteworthy that tiles can be clicked only when the simulation is not running, specifically new World has to be created with new simulation.

#### `HandleEvent(State nextState)`
This method is responsible for handling state transitions in the application based on the user's actions. The transitions are between `NewWorldState`, `RunningState`, `StoppedState`. 

#### `OnNewWorldButtonClicked()`, `OnRunSimulationButtonClicked()`, `OnPauseSimulationButtonClicked()`, `OnShowHideGraphsButtonClicked()`, `OnResetButtonClicked()`
These methods handle the events when respective buttons are clicked in the user interface.

#### `OnGraphButtonClicked()`

Handles the click event for the graph button. Toggles the visibility state of the graph.

#### `OnResetButtonClicked()`

Handles the click event for the reset button. Resets the world's state to `NewWorldState`, clears initialized burning tiles, remakes the visualizer and clears the graph visualizer.

#### `OnNewWorldButtonClicked()`

Handles the click event for the new world button. Sets the current state to `NewWorldState`.

#### `OnRunButtonClicked()`

Handles the click event for the run button. Sets the current state to `RunningState`.

#### `OnPauseButtonClicked()`

Handles the click event for the pause button. Sets the current state to `StoppedState`.

#### `OnImportClicked()`

Handles the click event for the import button. Imports map data either from a PNG heightmap or loads serialized world data, then prepares the imported world.

#### `OnSaveClicked()`

Handles the click event for the save button. Saves the current state of the world.

#### `SetSimulationSpeed(float newSpeed)`

Sets the simulation speed based on the passed `newSpeed` value.

#### `ApplyInputValues()`

Applies input values to the world generator. Configures the world generator with parameters like width, depth, number of rivers, and the lake threshold.

#### `GenereteNewWorld()`

Generates a new world using the `worldGenerator` and then prepares the new world.

#### `PrepareForNewWorld()`

Prepares the new world for visualization by setting the initial state, deciding the visualization mode, clearing initialized burning tiles, and remaking the visualizer.

#### `VisulizerRemakeAll()`

Remakes all visualizer components such as tiles, vegetation, and fire. Also sets the camera's position and orientation based on the world's width and depth.

### Subscription in `Awake()`

In the `Awake()` method, various components like `visulizer`, `inputHandler`, and `graphVisulizer` are initialized. Subscriptions to the events from `inputHandler` are also made to handle various functionalities like tile clicks, camera movements, and others. For instance:

*   `OnTileClicked` event is handled by `HandleTileClick`.
*   `OnCameraMove` event is handled by `HandleCameraMove`.
*   `OnGraph` event is handled by `OnGraphButtonClicked`.
  
... and so on for the rest of the methods.

#### `RunEverything()`
Executes all the necessary updates based on the current state of the application.

#### `showGraph()`
Uses GraphVisulizer to draw the graph of all the simulation update states, right now just tiles burning over time.

## State
This enum contains the four possible states that the application can be in:

- `NewWorldState`: The application is in a state where a new world has been or is being generated, here initialBurningTiles can be set by clicking on tiles (Raycast is sent).
- `RunningState`: The fire spread simulation is currently running.
- `StoppedState`: The fire spread simulation has been paused or has finished running.


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
These methods are intended to calculate the factors affecting fire spread based on vegetation, moisture, wind, and slope. 

## EventLogger

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

### Class `WorldGenerator`

#### `WorldGenerator()`

Initializes a new instance of the `WorldGenerator` class.

#### `Generate() -> World`

Generates a world based on various maps such as height map, moisture map, and vegetation map. The method includes steps to adjust and modify these maps before generating the final world representation.

## Related to WorldGenerator

### Class `Map<T>`

#### Properties:

*   **`Data`**: A 2D array that holds the data for the map.
*   **`Width`**: Represents the width of the map.
*   **`Depth`**: Represents the depth of the map.

#### `Map(int width, int depth)`

Initializes a new instance of the `Map` class with the specified width and depth.

#### `FillWithDefault(T defaultValue)`

Fills the map with a specified default value.

#### `To2DArray() -> T[,]`

Returns the 2D array representation of the map.

#### `ToJaggedArray() -> T[][]`

Converts the 2D array map into a jagged array and returns it.

### Class `MapExtensions`

#### `Normalize(this Map<float> map) -> Map<float>`

Normalizes all heights to be between 0 and 1.

#### `ReduceByBeachFactor(this Map<float> map, Map<int> waterMap, float beachFactor) -> Map<float>`

Reduces the height of the neighbors of the specified water map by the given beach factor.

#### `Smooth(this Map<float> map, int iterations = 1) -> Map<float>`

Applies smoothing to the map. Makes transitions between heights or other features more gradual.

#### `Amplify(this Map<float> map, float factor) -> Map<float>`

Modifies the values in the map by multiplying them by a given factor.

#### `RaiseElevation(this Map<float> map, float amount) -> Map<float>`

Raises the elevation of the entire map by a specified amount.

#### `SetBorder(this Map<float> map, int borderWidth, float value) -> Map<float>`

Sets a fixed value for a border of a given width around the map.

#### `GaussianBlur(this Map<float> map) -> Map<float>`

Applies Gaussian blur to the map.

### Class `Array2DExtensions`

#### `GetNeighbours(this Array array, int x, int y) -> IEnumerable<(int, int)>`

Fetches the neighboring coordinates of a given point in the array.

### Class `BaseTerrainGenerator`

#### `Generate() -> Map<float>`

Generates a base terrain map normalized between 0 and 1 using multi-octave Perlin noise.

### Class `LakeMapGenerator`

#### `Generate() -> Map<int>`

Generates a map representing lakes. Values below the specified threshold are considered as part of a lake.

### Class `RiverMapGenerator`

#### `Generate() -> Map<int>`

Generates a simple river map.

### Class `MoistureMapGenerator`

#### `Generate() -> Map<int>`

Generates a moisture map based on proximity to water bodies and a noise function.

### Class `VegetationMapGenerator`

#### `Generate() -> Map<VegetationType>`

Generates a vegetation map.


## Visualizer

The `Visualizer` class is responsible for creating and managing the visual representation of the simulation world. Operates on one of two settings states Standard and Simplified.

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


## InputHandler

#### `HandleTileClick()`

Handles the tile click interaction within the world. When a tile is clicked (and the click is not on a UI object), this method calculates the corresponding world tile and triggers the `OnTileClicked` event.

#### `HandleCameraMove()`

Handles the movement of the camera. Move camera with WASD keyboard buttons.

#### `HandleCameraAngleChange()`

Handles the change in camera angles. The keys I/K control the up and down rotation, while J/L control the left and right rotation.

#### `HandleActionButtons()`

Handles the user interactions with action buttons like resetting the camera position and orientation and triggering the run.

#### `TriggerGraph()`

Triggers the event for displaying the graph visualization. This can be called to invoke the `OnGraph` event.

#### `TriggerReset()`

Triggers the event for resetting the world. This can be called to invoke the `OnReset` event.

#### `TriggerGenerateWorld()`

Triggers the event for generating the world. This can be called to invoke the `OnGenerateWorld` event.

#### `TriggerImport()`

Triggers the event for importing data. This can be called to invoke the `OnImport` event.

#### `TriggerSave()`

Triggers the event for saving the current state. This can be called to invoke the `OnSave` event.

#### `TriggerRun()`

Triggers the event for running the simulation. This can be called to invoke the `OnRun` event.

#### `TriggerPause()`

Triggers the event for pausing the simulation. This can be called to invoke the `OnPause` event.

#### `SetWorldWidth(string widthString)`

Sets the world width according to the given string from input text in Unity. The value will be clamped between 1 and `MaxWorldWidth`. The `OnFieldValueChange` event is invoked if the value changes.

#### `SetWorldDepth(string depthString)`

Sets the world depth according to the given string from input text in Unity. The value will be clamped between 1 and `MaxWorldDepth`. The `OnFieldValueChange` event is invoked if the value changes.

#### `SetRivers(string riversString)`

Sets the number of rivers according to the given string from input text in Unity. The value will be clamped between 0 and `MaxRivers`. The `OnFieldValueChange` event is invoked if the value changes.

#### `SetSimulationSpeed(float value)`

Sets the simulation speed according to the given float value. The `onSimulationSpeedChange` event is invoked with the new speed value.

#### `SetLakeThreshold(float value)`

Sets the lake threshold value. This can be called to update the lake threshold value, and it invokes the `OnFieldValueChange` event.


## HeightMapImporter

#### `GetMap(int requiredWidth, int requiredDepth) -> Map<float>`

Imports the heightmap and returns it as a `Map<float>`. The returned map has the specified width and depth dimensions.

#### `GetPlainMap(int requiredWidth, int requiredDepth) -> Map<float>`

Generates a plain heightmap of the specified dimensions. Each data value in the map is set to `1f`.

#### `ImportHeightMap(int requiredWidth, int requiredDepth) -> bool`

Attempts to import a heightmap from a pre-defined path. The imported heightmap should be in PNG format. If the image is smaller than the desired size, the import will fail.

#### `ConvertToHeightmap(Texture2D tex, int requiredWidth, int requiredDepth) -> Map<float>`

Converts a given `Texture2D` object into a heightmap of the specified dimensions. It uses grayscale values of the texture's pixels to determine height values.

#### `HeightMultiplier -> float`

A publicly adjustable property used to scale the height values of the imported heightmap. Increasing this value will make the heightmap more pronounced, while decreasing it will make it flatter.
## GraphVisualizer

### Public Variables

- `panel`: A RectTransform that represents the graph panel where the graph will be drawn.
- `pointPrefab`: A GameObject prefab that represents a data point on the graph.
- `xAxisLabel`: A Text component that displays the label for the X-axis.
- `yAxisLabel`: A Text component that displays the label for the Y-axis.
- `last_maxLabel`: A Text component that displays the last data point's value and the maximum value on the Y-axis.

### Methods

- `DrawGraph(Dictionary<int, int> data, string Y_text = "Y axis", string X_text = "X axis")`: Draws the graph based on the provided data dictionary. It takes an optional Y-axis label and X-axis label as parameters.
- `HideGraph()`: Hides the graph panel and labels.
- `ClearGraph()`: Clears the graph by removing all data points.
- `SaveToFile(Dictionary<int, int> dict, string pathAndName = "Assets/graph.json")`: Static method that saves the provided data dictionary to a JSON file at the specified path.

- `AdjustPointSize(int count)`: Private method that calculates the size of the data points based on the width of the graph panel and the number of data points.

### Serialization Class

The `Serialization<T1, T2>` class is a helper class for serialization. It serializes a dictionary by storing its keys and values in separate lists. This class is used internally to save the data dictionary to a JSON file using the `SaveToFile` method.


# Roadmap
Future enhancements will include:

- More responsive design - text scalling isssues and different monitor size support
- Dynamic weather system influencing the fire spread
- Better fire spread calculation based on more factors - more real
- UI changes and enhancements, more settings options implemented by buttons, sliders, toggles...
- World logger
- Importing map - platform specific import system, right now can be used only through the Unity Engine
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