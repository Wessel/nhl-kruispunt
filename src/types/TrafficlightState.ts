export enum TrafficlightState {
  RED = 'rood',
  GREEN = 'groen',
  YELLOW = 'geel',
};

export interface TrafficlightStateMap {
  [key: string]: TrafficlightState
};
