import json

class SensorenBruggenHandler():
    def handle(self, message):
        try:
            data = json.loads(message.get_content())
        except json.JSONDecodeError:
            raise ValueError("Invalid JSON format")
        
        for _, value in data.items():
            # Ensure 'state' key exists in the item
            if 'state' not in value:
                raise ValueError(f"Error: Missing 'state' key in item: {value}")
            
            # Ensure 'state' is one of the valid states
            valid_states = ["dicht", "open","onbekend"]
            if value['state'] not in valid_states:
                raise ValueError(f"Error: Invalid state '{value['state']}' in item: {value}")