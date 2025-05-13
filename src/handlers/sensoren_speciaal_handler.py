import json

class SensorenSpeciaalHandler():
    def handle(self, message):
     
        data = json.loads(message.content)
        
        if data.get("brug_wegdek") is None \
        or type(data.get("brug_wegdek")) is not bool:
            raise ValueError(f"brug_wegdek is not following the protocol \n{data}")
        
        if data.get("brug_water") is None \
        or type(data.get("brug_water")) is not bool:
            raise ValueError(f"brug_water is not following the protocol \n{data}")
        
        if data.get("brug_file") is None \
        or type(data.get("brug_file")) is not bool:
            raise ValueError(f"brug_file is not following the protocol: \n{data}")