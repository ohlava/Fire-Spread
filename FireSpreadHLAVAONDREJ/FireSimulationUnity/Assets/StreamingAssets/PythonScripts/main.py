import sys
import json
import math
from enum import Enum
from typing import List

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
            
    def generate_output_array(self):
        output_array = []
        for depth in range(self.depth):
            row = {"rowData": [0.5 for _ in range(self.width)]}
            output_array.append(row)
        
        for tile in self.grid:
            value = 0 if tile.moisture == 100 else 1 if tile.is_initial_burning else 0.5
            output_array[tile.depth_position]['rowData'][tile.width_position] = value
            
        return output_array


def convert_json_to_world(json_str):
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




# Main execution

# Getting input from the file for Debug
#with open("/Users/hlava/test.json", "r") as file:
#    json_str = file.read()

# For testing
# import time
# time.sleep(1)
# exit(1)
# print(sys.argv)

json_str = input()
world = convert_json_to_world(json_str)

output = {
    "data": world.generate_output_array()
}
print(json.dumps(output))