import json
import re

class StoplichtenHandler():
    def handle(self, message):
        
        possible_states = {'rood', 'groen', 'oranje'}
        intersection_car_lights = [1.1, 2.1, 2.2, 3.1, 4.1, 5.1, 6.1, 7.1, 8.1, 8.2, 9.1, 10.1, 11.1, 12.1]
        interssection_pedestrian_lights = [31.1, 31.2, 32.1, 32.2, 33.1, 33.2, 34.1, 34.2, 35.1, 35.2, 36.1, 36.2, 37.1, 37.2, 38.1, 38.2]
        instersection_bike_lights = [21.1, 22.1, 23.1, 24.1, 25.1, 26.1, 27.1, 28.1]
        bridge_car_lights = [41.1, 42.1]
        bridge_pedestrian_bike_lights = [51.1, 52.1, 53.1, 54.1]
        bridge_boat_lights = [71.1, 72.1, 81.1]
        all_lights = intersection_car_lights + interssection_pedestrian_lights + instersection_bike_lights + bridge_car_lights + bridge_pedestrian_bike_lights + bridge_boat_lights
        # print(all_lights) #debugging
        possible_keys = r'\b([1-9][0-9]{0,2})\.[1-9]\b'
        
        try:
            data = json.loads(message.content)
        except json.JSONDecodeError:
            raise ValueError("Invalid JSON format")            
            
        for k, v in data.items():
            if not isinstance(v, str) or v not in possible_states:
                raise ValueError(f'Value is not one of {possible_states}: {{ {k} : {v} }}')
            
            if float(k) not in all_lights:
                raise ValueError(f'light {k} is not in the list of all lights: {{ {k} : {v} }}')
            
            all_lights.remove(float(k))
            
            if not re.match(possible_keys, k):
                raise ValueError(f'Key does not match regex {possible_keys}: {{ {k} : {v} }}')
            if not isinstance(v, str) or v not in possible_states:
                raise ValueError(f'Value is not one of {possible_states}: {{ {k} : {v} }}')
            
        if len(all_lights) > 0:
            raise ValueError(f'Not all lights are in the message {all_lights}')
            
if __name__ == "__main__":
    handler = StoplichtenHandler()
    message = type('Message', (object,), {'content': '{"1.1": "rood", "2.1": "groen"}'})
    handler.handle(message)