import json
import re

possible_states = {'rood', 'groen', 'geel'}
possible_keys = r'\b([1-9][0-9]{0,2})\.[1-9]\b'

def handle_stoplichten(message):
    try:
        data = json.loads(message)
        for k, v in data.items():
            if not re.match(possible_keys, k):
                raise ValueError(f'Key does not match regex {possible_keys}: {{ {k} : {v} }}')
            if not isinstance(v, str) or v not in possible_states:
                raise ValueError(f'Value is not one of {possible_states}: {{ {k} : {v} }}')
    except (json.JSONDecodeError, ValueError) as e:
        print(f"\033[31mError: {e}\033[0m")

if __name__ == "__main__":
    test_message = '{"123.1": "rood", "456.2": "groen", "789.3": "geel"}'
    test_message_invalid_val = '{"123.1": "rood", "456.2": "paars"}'
    test_message_invalid_key = '{"1234.1": "rood", "456.2": "groen"}'

    handle_stoplichten(test_message)
    handle_stoplichten(test_message_invalid_val)
    handle_stoplichten(test_message_invalid_key)