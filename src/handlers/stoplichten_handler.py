import json
import re

class StoplichtenHandler():
    def handle(self, message):
        
        possible_states = {'rood', 'groen', 'oranje'}
        possible_keys = r'\b([1-9][0-9]{0,2})\.[1-9]\b'
        
        try:
            data = json.loads(message.content)
            for k, v in data.items():
                if not re.match(possible_keys, k):
                    raise ValueError(f'Key does not match regex {possible_keys}: {{ {k} : {v} }}')
                if not isinstance(v, str) or v not in possible_states:
                    raise ValueError(f'Value is not one of {possible_states}: {{ {k} : {v} }}')
        except (json.JSONDecodeError, ValueError) as e:
            print(f"\033[31mError: {e}\033[0m")
