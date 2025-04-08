import json
import re

possible_lane_names = r'\b([1-9][0-9]{0,2})\.[1-9]\b'

def handle_voorrangsvoertuig(message):
    data = {}

    try:
        data = json.loads(message)
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

    except (json.JSONDecodeError, ValueError) as e:
        print(f"Error: {e}")

if __name__ == "__main__":
    # Test the function with a sample message
    correct_string = '''{
    "queue": [
        {
        "baan": "8.2",
        "simulatie_tijd_ms": 1231456352542,
        "prioriteit": 1
        },
        {
        "baan": "3.1",
        "simulatie_tijd_ms": 1231456650000,
        "prioriteit": 2
        }
    ]
    }'''

    missing_queue = '{}'
    wrong_lane_name = '''{
    "queue": [
        {
        "baan": "1834.2",
        "simulatie_tijd_ms": 1231456352542,
        "prioriteit": 1
        }
    ]
    }'''

    wrong_simulatie_tijd = '''{
    "queue": [
        {
        "baan": "8.2",
        "simulatie_tijd_ms": "1231456352542",
        "prioriteit": 1
        }
    ]
    }'''
    
    wrong_prioriteit = '''{
    "queue": [
        {
        "baan": "8.2",
        "simulatie_tijd_ms": 1231456352542,
        "prioriteit": 3
        }
    ]
    }'''


    handle_voorrangsvoertuig(correct_string)  # Should pass without error
    handle_voorrangsvoertuig(missing_queue)  # Should raise ValueError
    handle_voorrangsvoertuig(wrong_lane_name)  # Should raise ValueError
    handle_voorrangsvoertuig(wrong_simulatie_tijd)  # Should raise ValueError
    handle_voorrangsvoertuig(wrong_prioriteit)  # Should raise ValueError