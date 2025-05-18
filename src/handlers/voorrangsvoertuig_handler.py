import json
import re

class VoorrangsvoertuigHandler():
    
    def handle(self, message):
        possible_lane_names = r'\b([1-9][0-9]{0,2})\.[1-9]\b'
        
        try:
            data = json.loads(message.get_content())
        except json.JSONDecodeError:
            raise ValueError("Invalid JSON format")
        
        if "queue" not in data:
            raise ValueError("Missing 'queue' key")

        for item in data["queue"]:
            if not re.match(possible_lane_names, item.get("baan", "")) \
                or item.get("baan") is None:
                    raise ValueError(f"lane name does not match {possible_lane_names}: {item.get('baan')}")

            if item.get("simulatie_tijd_ms") is None \
                or type(item.get("simulatie_tijd_ms")) is not int:
                raise ValueError(f"Simulatie tijd is not following the protocol: \n{data}")

            if item.get("prioriteit") is None \
                or type(item.get("prioriteit")) is not int \
                or item.get("prioriteit") < 0 \
                or item.get("prioriteit") > 2:
                    raise ValueError(f"Prioriteit is not following the protocol: \n{data}")