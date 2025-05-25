export interface PriorityVehicle {
  prioriteit: number;
  baan: string;
  simulatie_tijd_ms: number;
}

export type PriorityVehicleQueue = PriorityVehicle[];
