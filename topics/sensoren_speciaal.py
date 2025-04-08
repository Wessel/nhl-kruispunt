import json

def handle_speciaal(message):
    try:
        data = json.loads(message)
        
        if data.get("brug_wegdek") is None \
        or type(data.get("brug_wegdek")) is not bool:
            raise ValueError(f"brug_wegdek is not following the protocol \n{data}")
        
        if data.get("brug_water") is None \
        or type(data.get("brug_water")) is not bool:
            raise ValueError(f"brug_water is not following the protocol \n{data}")
        
        if data.get("brug_file") is None \
        or type(data.get("brug_file")) is not bool:
            raise ValueError(f"brug_file is not following the protocol: \n{data}")
        
    except (json.JSONDecodeError, ValueError) as e:
        print(f"Error: {e}")
        return
    
if __name__ == "__main__":
    correct_string = '''{
        "brug_wegdek": true,
        "brug_water": false,
        "brug_file": true
    }'''
    
    missing_key = '''{
        "brug_wegdek": true,
        "brug_water": false
    }'''
    
    wrong_type = '''{
        "brug_wegdek": true,
        "brug_water": false,
        "brug_file": 123
    }'''
    
    handle_speciaal(correct_string)
    handle_speciaal(missing_key)
    handle_speciaal(wrong_type)