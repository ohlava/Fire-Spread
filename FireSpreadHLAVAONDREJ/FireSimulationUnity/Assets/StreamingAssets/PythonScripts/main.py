import sys
import json
import math
from enum import Enum
from typing import List, Dict, Any
from collections import deque

class VegetationType(Enum):
    Grass = 0
    Sparse = 1
    Forest = 2
    Swamp = 3

class Tile:
    def __init__(self, height, moisture, vegetation, position_x, position_y, is_initial_burning=False):
        self.width_position = position_x
        self.depth_position = position_y
        self.height = max(0, height)
        self.moisture = max(0, min(100, moisture))
        self.vegetation = VegetationType(vegetation)
        self.is_burning = False
        self.has_burned = False
        self.burning_for = 0
        self.burn_time = self._calculate_burn_time()
        self.is_initial_burning = is_initial_burning

    def _calculate_burn_time(self):
        burn_time = {
            VegetationType.Grass: 1,
            VegetationType.Sparse: 2,
            VegetationType.Forest: 4,
            VegetationType.Swamp: 3
        }.get(self.vegetation, 0)

        if self.moisture >= 50:
            burn_time += 1

        return burn_time

    def reset(self):
        self.is_burning = False
        self.has_burned = False
        self.burning_for = 0

class World:
    def __init__(self, width, depth, grid_tiles, initial_burn_map):
        self.width = width
        self.depth = depth
        self.grid = grid_tiles
        self.initial_burn_map = initial_burn_map

    def reset(self):
        self.weather.reset()
        for tile in self.grid:
            tile.reset()
            
    def print_tile_heights(self):
        for tile in self.grid:
            print(f"Tile at ({tile.width_position}, {tile.depth_position}) has a height of {tile.height}")
            



class Predictor:
    def __init__(self, model_path: str):
        self.model = self.load_model(model_path)

    def load_model(self, model_path: str):
        # Load machine learning model here
        pass

    def preprocess_data(self, world: World) -> Any:
        # Convert World data to a format suitable for your model
        pass

    def predict(self, world: World) -> List[List[float]]:
        prediction_array = [[0 for _ in range(world.width)] for _ in range(world.depth)]
        
        # Queue for BFS: (x, y, distance from initial burning tile)
        burning_tiles_queue = deque()
        for tile in world.grid:
            if tile.is_initial_burning:
                prediction_array[tile.depth_position][tile.width_position] = 1.0
                burning_tiles_queue.append((tile.width_position, tile.depth_position, 0))

        # BFS to propagate decreasing probability from initially burning tiles
        while burning_tiles_queue:
            x, y, distance = burning_tiles_queue.popleft()
            for dx, dy in [(0, 1), (1, 0), (0, -1), (-1, 0)]:
                nx, ny = x + dx, y + dy
                if 0 <= nx < world.width and 0 <= ny < world.depth:
                    new_distance = distance + 1
                    new_probability = max(1.0 - new_distance * 0.1, 0)  # Decrease probability with distance
                    if prediction_array[ny][nx] < new_probability:
                        prediction_array[ny][nx] = new_probability
                        burning_tiles_queue.append((nx, ny, new_distance))

        return prediction_array

class HeatMap:
    def __init__(self, data):
        self.data = data

    @staticmethod
    def from_output_data(output_data):
        heatmap_rows = [row_data['rowData'] for row_data in output_data['data']]
        return HeatMap(heatmap_rows)
    
class JSONUtility:
    @staticmethod
    def generate_output_array(prediction_array: List[List[float]]) -> List[Dict[str, List[float]]]:
        output_array = [{"rowData": row} for row in prediction_array]
        return output_array

    @staticmethod
    def convert_json_to_world(json_str: str) -> World:
        data = json.loads(json_str)

        width = data['World']['Width']
        depth = data['World']['Depth']
        initial_burn_map = data['InitialBurnMap']
        grid_data = data['World']['GridTiles']
        grid_tiles = []

        for index, tile_data in enumerate(grid_data):
            is_initial_burning = initial_burn_map[index]
            tile = Tile(tile_data['Height'], tile_data['moisture'], tile_data['Vegetation'], tile_data['widthPosition'], tile_data['depthPosition'], is_initial_burning)
            grid_tiles.append(tile)

        return World(width, depth, grid_tiles, initial_burn_map)

    @staticmethod
    def load_multiple_jsons(file_path):
        with open(file_path, 'r') as file:
            data = []
            for line in file:
                # Each line contains a separate JSON object
                json_object = json.loads(line)
                data.append(json_object)
        return data
    
    @staticmethod
    def convert_json_to_world_and_heatmap(data) -> (World, HeatMap):
        world_data = data['World']
        heatmap_data = data['HeatMap']
        
        width = world_data['Width']
        depth = world_data['Depth']
        grid_tiles = []

        for tile_data in world_data['GridTiles']:
            tile = Tile(
                height=tile_data['Height'], 
                moisture=tile_data['moisture'], 
                vegetation=tile_data['Vegetation'], 
                position_x=tile_data['widthPosition'], 
                position_y=tile_data['depthPosition']
            )
            grid_tiles.append(tile)

        world = World(width, depth, grid_tiles, initial_burn_map=[])

        heatmap = HeatMap.from_output_data(heatmap_data)

        return world, heatmap


def get_input():
    input_string = input()
    # Or getting input from the file for Debug purposes
    #with open("/Users/hlava/test.json", "r") as file:
    #   input_string = file.read()
    return input_string

def load_generated_data():
    data = JSONUtility.load_multiple_jsons("./datafile.json")
    loaded_data = []
    for d in data:
        world, heatmap = JSONUtility.convert_json_to_world_and_heatmap(d)
        loaded_data.append((world, heatmap))
    #print(loaded_data)
    return
    

# Main execution
def main():
    json_str = get_input()
    world = JSONUtility.convert_json_to_world(json_str)
    predictor = Predictor(model_path="./model")
    prediction_array = predictor.predict(world)
    output = {"data": JSONUtility.generate_output_array(prediction_array)}
    print(json.dumps(output))




###

def trigger_division_by_zero():
    return 1 / 0

# Call this function to simulate an error
# trigger_division_by_zero()

# For other testing purposes
# import time
# time.sleep(1)
# exit(1)
# print(sys.argv)

if __name__ == "__main__":
    #load_generated_data()
    main()