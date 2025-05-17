import json

class SensorenRijbaanHandler():
    def handle(self, message):
    
        data = json.loads(message.content)

        for key, value in data.items():
            if not isinstance(value, dict):
                raise ValueError(f"Error: Value for key '{key}' is not a dictionary.")
            
            # Check for 'voor' and 'achter' keys
            if 'voor' not in value or 'achter' not in value:
                raise ValueError(f"Error: Missing 'voor' or 'achter' keys in '{key}'.")
            
            # Ensure 'voor' and 'achter' are boolean
            if not isinstance(value['voor'], bool) or not isinstance(value['achter'], bool):
                raise ValueError(f"Error: 'voor' and 'achter' must be boolean in '{key}'.")