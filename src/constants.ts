export const PRIORITY_HIGH = 1;
export const PRIORITY_LOW = 2;

export const PEDESTRIAN_MULTIPLIER = 4;

export const DELAY_CYCLE = 3500;
export const DELAY_REMOVING = DELAY_CYCLE * 2;
export const DELAY_EMPTY = DELAY_CYCLE / 2;

export const MAX_BOATS_PER_PASSING = 3;
export const MAX_TIME_GREEN = 5000;

export const BRIDGE_LANES = [ '71', '72', '81', '41', '42', '51', '52', '53', '54' ];

export const HELP_STRING = `
+------------------------------------------------------+
| Trafficlight controller for NHL Stenden              |
+------------------------------------------------------+
USAGE:
  icontroller [OPTIONS]

OPTIONS:
  --help                Show this help message and exit
  --intersection=PATH   Path to the intersection JSON file
                        Default: ./static/intersection/lanes.json
  --ip=ADDRESS          IP address of the simulator
                        Default: localhost
  --port=PORT           Port number of the simulator
                        Default: 5556
  --cstr=CONNECTION     Full connection string (overrides ip and port)
                        Default: tcp://localhost:5556
`;
