import json

def handle_tijd(message):
    try:
        data = json.loads(message)

        if "simulatie_tijd_ms" not in data:
            raise ValueError("Missing 'simulatie_tijd_ms' key")
        
        if type(data.get("simulatie_tijd_ms")) is not int:
            raise ValueError("simulatie_tijd_ms is not an integer")
        
        if data.get("simulatie_tijd_ms") < 0:
            raise ValueError("simulatie_tijd_ms cannot be negative")
        
        
        
    except (json.JSONDecodeError, ValueError) as e:
        print(f"Error: {e}")
        
if __name__ == "__main__":
    # Test the function with a sample message
    correct_string = '{"simulatie_tijd_ms": 1231456352542}'
    missing_key = '{}'
    not_integer = '{"simulatie_tijd_ms": "dit is geen int"}'
    negative_integer = '{"simulatie_tijd_ms": -1231456352542}'
    
    handle_tijd(correct_string)
    handle_tijd(missing_key)
    handle_tijd(not_integer)
    handle_tijd(negative_integer)