import json

def handle_bruggen(message):
    try:
        data = json.loads(message)
        for _, value in data.items():
            # Ensure 'state' key exists in the item
            if 'state' not in value:
                raise ValueError(f"Error: Missing 'state' key in item: {value}")
            
            # Ensure 'state' is one of the valid states
            valid_states = ["dicht", "open"]
            if value['state'] not in valid_states:
                raise ValueError(f"Error: Invalid state '{value['state']}' in item: {value}")
            
        
    except (json.JSONDecodeError, ValueError) as e:
        print(f"Error: {e}")
        return
    
if __name__ == "__main__":
    correct_string = '''{
        "81":{
            "state":"dicht"
        }
    }'''
    
    missing_key = '''{ "81": { "yo": "oinkedoink" } }'''
    
    state_not_valid = '''{
        "81":{
            "state":"oooopeeeen"
        }
    }'''
    
    handle_bruggen(correct_string)
    handle_bruggen(missing_key)
    handle_bruggen(state_not_valid)