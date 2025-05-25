export enum VehicleType {
  CAR = 'car',
  BOAT = 'boat',
  BIKE = 'bike',
  PEDESTRIAN = 'walk'
}

export interface SensorRequirement {
  type: 'sensor';
  sensor: string;
  sensor_state: boolean;
}

export interface TrafficLightRequirement {
  type: 'other_traffic_light';
  group: number;
  traffic_light_state: string;
}

export type TransitionRequirement = SensorRequirement | TrafficLightRequirement;

export interface LaneProperties {
  is_inverse_of?: string;
  extends_to?: string;
}

export interface LaneGroup {
  intersects_with: number[];
  is_inverse_of: number | false;
  extends_to: number | number[] | false;
  vehicle_type: VehicleType[];
  lanes: Record<string, LaneProperties>;
  is_physical_barrier: boolean;
  transition_requirements?: {
    green?: TransitionRequirement[];
    red?: TransitionRequirement[];
  };
  transition_blockers?: {
    green?: TransitionRequirement[];
    red?: TransitionRequirement[];
  };
}

export interface Sensor {
  vehicles: VehicleType[];
}

export interface IntersectionConfig {
  $schema: string;
  groups: Record<string, LaneGroup>;
  sensors: Record<string, Sensor>;
}
