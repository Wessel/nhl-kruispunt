export enum BridgeState {
  OPEN = 'open',
  CLOSED = 'dicht',
  UNKNOWN = 'onbekend'
}

export interface BridgeSensorMap {
  [sensor: string]: {
    voor: boolean,
    achter: boolean
  }
};
