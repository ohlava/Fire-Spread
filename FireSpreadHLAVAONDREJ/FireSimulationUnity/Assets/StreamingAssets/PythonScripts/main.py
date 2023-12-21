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
    def __init__(self, height, moisture, vegetation, position_x, position_y):
        self.width_position = position_x
        self.depth_position = position_y
        self.height = max(0, height)
        self.moisture = max(0, min(100, moisture))
        self.vegetation = VegetationType(vegetation)
        self.is_burning = False
        self.has_burned = False
        self.burning_for = 0
        self.burn_time = self._calculate_burn_time()

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

class Weather:
    def __init__(self, wind_direction, wind_speed):
        self.wind_direction = wind_direction
        self.wind_speed = wind_speed

    def reset(self):
        # Random values for wind direction and speed can be assigned here
        self.wind_direction = 0  # Placeholder value
        self.wind_speed = 0      # Placeholder value

class World:
    def __init__(self, width, depth, grid_tiles, weather):
        self.width = width
        self.depth = depth
        self.grid = grid_tiles
        self.weather = weather

    def reset(self):
        self.weather.reset()
        for tile in self.grid:
            tile.reset()
            
    def print_tile_heights(self):
        for tile in self.grid:
            print(f"Tile at ({tile.width_position}, {tile.depth_position}) has a height of {tile.height}")


def convert_json_to_world(json_str):
    data = json.loads(json_str)

    width = data['Width']
    depth = data['Depth']
    grid_data = data['GridTiles']
    grid_tiles = []

    for tile_data in grid_data:
        tile = Tile(tile_data['Height'], tile_data['moisture'], tile_data['Vegetation'], tile_data['widthPosition'], tile_data['depthPosition'])
        grid_tiles.append(tile)

    # Initialize Weather with placeholder values
    weather = Weather(0, 0)  

    return World(width, depth, grid_tiles, weather)

# Main execution
json_str = input()
world = convert_json_to_world(json_str)
print("World loaded successfully!")

world.print_tile_heights()