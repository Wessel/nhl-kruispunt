# NHL Kruispunt Project

## Overview
The NHL Kruispunt project is designed to facilitate communication between a traffic simulation system and a control system using ZeroMQ. This project implements an object-oriented architecture to handle various types of messages related to traffic management.

## Project Structure
```
nhl-kruispunt
├── src
│   ├── __init__.py
│   ├── main.py
│   ├── subscriber.py
│   ├── handlers
│   │   ├── __init__.py
│   │   ├── base_handler.py
│   │   ├── tijd_handler.py
│   │   ├── stoplichten_handler.py
│   │   ├── voorrangsvoertuig_handler.py
│   │   ├── sensoren_rijbaan_handler.py
│   │   ├── sensoren_speciaal_handler.py
│   │   └── sensoren_bruggen_handler.py
│   ├── models
│   │   ├── __init__.py
│   │   └── message.py
│   └── config
│       ├── __init__.py
│       └── settings.py
├── tests
│   ├── __init__.py
│   └── test_subscriber.py
├── requirements.txt
└── README.md
```

## Installation
To install the required dependencies, run:
```
pip install -r requirements.txt
```

## Usage
To start the application, run the `main.py` file:
```
python src/main.py
```

## Components
- **Subscriber**: The main component that connects to the publishers and listens for messages.
- **Handlers**: Each type of message has a corresponding handler that processes the message. Handlers inherit from a base handler to ensure a consistent interface.
- **Models**: Defines the structure of messages used in the application.
- **Configuration**: Contains settings such as publisher addresses.

## Testing
Unit tests for the `Subscriber` class can be found in the `tests` directory. To run the tests, use:
```
pytest tests/
```

## Contributing
Contributions are welcome! Please submit a pull request or open an issue for any enhancements or bug fixes.

## License
This project is licensed under the MIT License. See the LICENSE file for more details.