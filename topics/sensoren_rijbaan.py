import json

def handle_rijbaan(message):
    
    try:
        data = json.loads(message)
    
        for key, value in data.items():
            if not isinstance(value, dict):
                raise ValueError(f"Error: Value for key '{key}' is not a dictionary.")
            
            # Check for 'voor' and 'achter' keys
            if 'voor' not in value or 'achter' not in value:
                raise ValueError(f"Error: Missing 'voor' or 'achter' keys in '{key}'.")
            
            # Ensure 'voor' and 'achter' are boolean
            if not isinstance(value['voor'], bool) or not isinstance(value['achter'], bool):
                raise ValueError(f"Error: 'voor' and 'achter' must be boolean in '{key}'.")
        
        
        
        
    except (json.JSONDecodeError, ValueError) as e:
        print(f"\033[31mError: {e}\033[0mE")
        return
    
if __name__ == "__main__":
        correct_string = '''{
            "1.1": {
                "voor": false,
                "achter": false
            },
            "22.1": {
                "voor": true,
                "achter": false
            }
        }'''
        
        missing_key = '''{
            "1.1": {
                "voor": false,
                "achter": false
            },
            "22.1": {
                "voor": true
            }
        }'''
        
        wrong_type = '''{
            "1.1": {
                "voor": false,
                "achter": false
            },
            "22.1": {
                "voor": true,
                "achter": 123
            }
        }'''
        
        handle_rijbaan(correct_string)
        handle_rijbaan(missing_key)
        handle_rijbaan(wrong_type)