import json

class TijdHandler():
    def handle(self, message):
       
        try:
            data = json.loads(message.get_content())
        except json.JSONDecodeError:
            raise ValueError("Invalid JSON format")

        if "simulatie_tijd_ms" not in data:
            raise ValueError("Missing 'simulatie_tijd_ms' key")

        if type(data.get("simulatie_tijd_ms")) is not int:
            raise ValueError("simulatie_tijd_ms is not an integer")

        if data.get("simulatie_tijd_ms") < 0:
            raise ValueError("simulatie_tijd_ms cannot be negative")