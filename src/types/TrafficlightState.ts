export enum TrafficlightState {
  RED = 'rood',
  GREEN = 'groen',
  YELLOW = 'oranje',
};

export interface TrafficlightStateMap {
  [key: string]: TrafficlightState
};
