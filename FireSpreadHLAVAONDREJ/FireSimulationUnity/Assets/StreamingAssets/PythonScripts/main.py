import sys
import json
import math
from enum import Enum
from typing import List, Dict, Any

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
            



class PredictionModel:
    def __init__(self, model_path: str):
        self.model = self.load_model(model_path)

    def load_model(self, model_path: str):
        # Load machine learning model here
        pass

    def preprocess_data(self, world: World) -> Any:
        # Convert World data to a format suitable for your model
        pass

    def predict(self, data: Any) -> Any:
        # Make a prediction with the model
        pass

    def generate_prediction_array(self, world: World) -> List[List[float]]:
        prediction_array = [[0 for _ in range(world.width)] for _ in range(world.depth)]
        for tile in world.grid:
            value = self.determine_tile_value(tile)
            prediction_array[tile.depth_position][tile.width_position] = value
            
        return prediction_array
    
    def determine_tile_value(self, tile: Tile) -> float:
        if tile.moisture == 100:
            return 0
        elif tile.is_initial_burning:
            return 1
        return 0.3


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



def get_input():
    input_string = input()
    # Or getting input from the file for Debug purposes
    #with open("/Users/hlava/test.json", "r") as file:
    #   json_str = file.read()
    return input_string

# Main execution
def main():
    json_str = get_input()
    world = JSONUtility.convert_json_to_world(json_str)
    prediction_model = PredictionModel(model_path="./model")
    prediction_array = prediction_model.generate_prediction_array(world)
    output = {"data": JSONUtility.generate_output_array(prediction_array)}
    print(json.dumps(output))

if __name__ == "__main__":
    main()
    
# For testing purposes
# import time
# time.sleep(1)
# exit(1)
# print(sys.argv)