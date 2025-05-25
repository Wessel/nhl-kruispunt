// Not imported as type due to it being unusable in the Record.
import type { Lane } from '.';
import {
  type LaneMap, type TrafficlightStateMap, type PriorityVehicleQueue,
  type IntersectionConfig, type BridgeSensorMap, type SpecialSensors,
  type LaneList,
  BridgeState, TrafficlightState, VehicleType
} from '../types';

import {
  PRIORITY_HIGH, PRIORITY_LOW, BRIDGE_LANES,
  PEDESTRIAN_MULTIPLIER, DELAY_REMOVING, DELAY_EMPTY
} from '../constants';

import { PriorityQueue } from '../priorityQueue';
import { ZmqPublisher, ZmqSubscriber } from '../zeromq';

export class Controller {
  private _intersection: IntersectionConfig | null = null;
  private _lanes: LaneMap = {};

  private _publisher: ZmqPublisher;
  private _subscriber: ZmqSubscriber = new ZmqSubscriber();

  private _time: number = 0;

  private _in_cycle = false;
  private _in_bridge_cycle = false;
  private _is_removing = false;

  private _lane_queue: PriorityQueue = new PriorityQueue();

  private _bridge_sensors: BridgeSensorMap = {};
  private _sensors_special: SpecialSensors = {
    brug_wegdek: false,
    brug_water: false,
    brug_file: false
  }

  private _priority_lane?: string;
  private _bridge_state: BridgeState = BridgeState.UNKNOWN;

  constructor(publisher_port: number = 5555, intersection: IntersectionConfig) {
    this._intersection = intersection;
    this._publisher = new ZmqPublisher();

    this._publisher
      .bind(`tcp://*:${publisher_port}`)
      .toggle_heartbeat_loop();
  }

  /* Initialisation Functions */
  connect_to_simulator(address: string): this {
    this._subscriber
      .connect(address)
      // .subscribe('', (t, a) => { try { console.log(t, JSON.parse(a)) } catch (_) { console.log(t, a); } })
      .subscribe('sensoren_rijbaan',  (_, m) => this.handle_topic_sensoren_rijbaan(m))
      .subscribe('sensoren_speciaal', (_, m) => this.handle_topic_sensoren_speciaal(m))
      .subscribe('sensoren_bruggen',  (_, m) => this.handle_topic_sensoren_bruggen(m))
      .subscribe('voorrangsvoertuig', (_, m) => this.handle_topic_voorrangsvoertuig(m))
      .subscribe('tijd',              (_, m) => this.handle_topic_tijd(m))
      .bind();

    return this;
  }

  bind_lane(lane: Lane): this {
    this._lanes[lane.name] = lane;

    lane.on('state_changed', () => this.transmit_state_to_publisher());

    return this;
  }

  /* Helper functions */
  delay_for(ms: number): Promise<void> {
    const targetTime = this._time + ms;

    return new Promise((resolve) => {
      const checkTime = () => {
        if (this._time >= targetTime) {
          resolve();
        } else {
          setTimeout(checkTime, 100);
        }
      };

      checkTime();
    });
  }

  compatible_lanes(lane_name: string): LaneList {
    const allowed: LaneList = [];
    const notAllowed: LaneList = [];

    if (!this._intersection) {
      return allowed;
    }

    const laneData = this._intersection.groups[lane_name];

    for (const lane of Object.keys(this._intersection.groups)) {
      if (laneData.intersects_with.includes(Number(lane)) || notAllowed.includes(lane)) {
        continue;
      }

      for (const nestedLane of this._intersection.groups[lane].intersects_with) {
        notAllowed.push(String(nestedLane));
      }

      allowed.push(lane);
    }

    return allowed;
  }

  get_state_map(): TrafficlightStateMap {
    const stateMap: TrafficlightStateMap = {};

    for (const lane of Object.keys(this._lanes)) {
      for (const trafficlight in this._lanes[lane].get_state_map()) {
        stateMap[`${lane}.${trafficlight}`] = this._lanes[lane].get_state_map()[trafficlight];
      }
    }

    return stateMap;
  }

  cycle_intersection_red(): void {
    for (const lane of Object.keys(this._lanes)) {
      if (!BRIDGE_LANES.includes(lane)) {
        this._lanes[lane].set_state(TrafficlightState.RED);
      }
    }
  }

  toggle_bridge_lights(state: TrafficlightState): void {
    for (const lane of BRIDGE_LANES) {
      if (lane !== '71' && lane !== '72' && lane !== '81') {
        this._lanes[String(lane)].set_state(state);
      }
    }
  }

  async wait_for_empty_bridge(): Promise<void> {
    return new Promise<void>((resolve) => {
      const check_road = () => {
        if (!this._sensors_special.brug_wegdek) {
          resolve();
        } else {
          setTimeout(check_road, 100);
        }
      };
      check_road();
    });
  }

  async wait_for_empty_bridge_water(): Promise<void> {
    return new Promise<void>((resolve) => {
      const check_water = () => {
        if (!this._sensors_special.brug_water) {
          resolve();
        } else {
          setTimeout(check_water, 100);
        }
      };
      check_water();
    });
  }

  async wait_for_bridge_closed(): Promise<void> {
    return new Promise<void>((resolve) => {
      const check_bridge = () => {
        if (this._bridge_state === BridgeState.CLOSED) {
          resolve();
        } else {
          setTimeout(check_bridge, 100);
        }
      };
      check_bridge();
    });
  }

  async wait_for_bridge_opened(): Promise<void> {
    return new Promise<void>((resolve) => {
      const check_bridge = () => {
        console.log(`Checking if bridge is opened: ${this._bridge_state}`);
        if (this._bridge_state === BridgeState.OPEN) {
          resolve();
        } else {
          setTimeout(check_bridge, 100);
        }
      };
      check_bridge();
    });
  }

  /* Priority Queue functions */
  async exhaust_lane_queue(): Promise<void> {
    if (this._lane_queue.isEmpty() || this._in_cycle || !this._intersection) {
      return;
    }

    const nextLane = this._lane_queue.peek();
    if (!nextLane) {
      return;
    }

    this._in_cycle = true;

    // Let the intersection empty (leeglooptijd in the law)
    this.cycle_intersection_red();
    await this.delay_for(DELAY_EMPTY);

    const lane = this._lanes[nextLane.group];
    if (lane) {
      const lanes = this.compatible_lanes(nextLane.group);

      console.log(`[time=${this._time}]\t[active=${JSON.stringify(nextLane)}]\t[green=(${lanes.join(',')})]`);

      this._lane_queue.set_active(nextLane.group, this._time);

      for (const group of Object.keys(this._intersection.groups)) {
        if (BRIDGE_LANES.includes(group)) {
          continue;
        }

        if (lanes.includes(group)) {
          const lane = this._lanes[group];
          if (lane) {
            lane.set_state(TrafficlightState.GREEN);
          }
        } else if (this._lane_queue.contains(nextLane.group)) {
          const lane = this._lanes[group];
          if (lane) {
            lane.set_state(TrafficlightState.RED);
          }
        }
      }
    }

    this._in_cycle = false;
  }

  async handle_bridge(): Promise<void> {
    if (this._in_bridge_cycle) return;
    // todo: allow multiple boats to pass, also first do 71 and if any at 72 do 72
    if (this._bridge_sensors['71']?.voor || this._bridge_sensors['72']?.voor) {
      this._in_bridge_cycle = true;

      this.toggle_bridge_lights(TrafficlightState.RED);

      await this.wait_for_empty_bridge();

      this._lanes['81'].set_state(TrafficlightState.GREEN);

      await this.wait_for_bridge_opened();

      this._lanes['71'].set_state(TrafficlightState.GREEN);
      this._lanes['72'].set_state(TrafficlightState.GREEN);

      await this.delay_for(10000);

      this._lanes['71'].set_state(TrafficlightState.RED);
      this._lanes['72'].set_state(TrafficlightState.RED);

      await this.wait_for_empty_bridge_water();

      this._lanes['81'].set_state(TrafficlightState.RED);

      await this.wait_for_bridge_closed();

      this.toggle_bridge_lights(TrafficlightState.GREEN);

      this._in_bridge_cycle = false;

    } else {
      this._lanes['41'].set_state(TrafficlightState.GREEN);
      this.toggle_bridge_lights(TrafficlightState.GREEN);
    }

  }

  /* Incoming data functions */
  handle_topic_tijd(message: string) {
    const data = JSON.parse(message);
    const time = data.simulatie_tijd_ms;

    this._time = time;

    for (const lane of Object.keys(this._lanes)) {
      this._lanes[lane].update_time(time);
    }
  }

  handle_topic_sensoren_speciaal(message: string) {
    this._sensors_special = JSON.parse(message);
  }

  handle_topic_sensoren_bruggen(message: string) {
    const data = JSON.parse(message);

    for (const key of Object.keys(data)) {
      this._bridge_state = data[key].state;
    }
  }

  handle_topic_sensoren_rijbaan(message: string) {
    if (this._is_removing) return;
    const data = JSON.parse(message);

    let bridge = 0;
    Object.keys(data).forEach(async(key) => {
      const [ group ] = key.split('.');

      // Skip all lanes related to bridges, due to them being handled by the bridge control
      if (BRIDGE_LANES.includes(group)) {
        this._bridge_sensors[group] = data[key];
        bridge++;

        if (bridge > 1) {
          this.handle_bridge();
        }
      }

      let priority;
      const sensorData = data[key];

      if (sensorData.voor && sensorData.achter) {
        priority = PRIORITY_HIGH;
      } else if (sensorData.voor || sensorData.achter) {
        priority = PRIORITY_LOW;
      }

      if (priority) {
        const existingEntry = this._lane_queue.get(group);
        if (existingEntry) {
          if (existingEntry.activeSince > 0) {
            if (this._time > existingEntry.activeSince + 5000) {
              this._lane_queue.remove(group);
              return;
            }

            if (existingEntry.priority < 0) {
              return;
            }

            priority -= 4;
          }

          this._lane_queue.update_priority(group, priority);
        } else {
          this._lane_queue.enqueue(group, priority, this._time);
        }
      } else if (this._lane_queue.contains(group)) {
        this._is_removing = true;

        this._lane_queue.remove(group);

        if (
          this._intersection && (
          this._intersection.groups[group].vehicle_type.includes(VehicleType.PEDESTRIAN)
            || this._intersection.groups[group].vehicle_type.includes(VehicleType.BIKE))
        ) {
          await this.delay_for(DELAY_REMOVING * PEDESTRIAN_MULTIPLIER);
        } else {
          await this.delay_for(DELAY_REMOVING);
        }

        this._is_removing = false;
      }
    });

    this.exhaust_lane_queue();
  }

  handle_topic_voorrangsvoertuig(message: string) {
    const queue: PriorityVehicleQueue = JSON.parse(message)?.queue;

    // Check if any emergency vehicles are no longer in the queue
    if (this._priority_lane) {
      const emergency_vehicles = queue
        .filter(v => v.prioriteit === 1)
        .map(v => v.baan.split('.')[0]);

      if (!emergency_vehicles.includes(this._priority_lane)) {
        this._in_cycle = false;
        this._priority_lane = undefined;
      }
    }

    // Process all priority vehicles
    const emergencyVehicles = queue.filter(v => v.prioriteit === 1 && v.baan.split('.')[0] !== '41' && v.baan.split('.')[0] !== '42');
    const publicTransport = queue.filter(v => v.prioriteit === 2);

    // Handle emergency vehicles (priority 1) - multiple can be active
    if (emergencyVehicles.length > 0) {
      this._in_cycle = true;
      const emergencyLanes = emergencyVehicles.map(v => v.baan.split('.')[0]);
      this._priority_lane = emergencyLanes[0]; // Keep track of first emergency lane for compatibility

      for (const lane of Object.keys(this._lanes)) {
      if (BRIDGE_LANES.includes(lane)) continue;

      if (emergencyLanes.includes(lane)) {
        this._lanes[lane].set_state(TrafficlightState.GREEN);
      } else {
        if (BRIDGE_LANES.includes(lane)) continue;
        this._lanes[lane].set_state(TrafficlightState.RED);
      }
      }
    }

    // Handle public transport (priority 2)
    for (const entry of publicTransport) {
      const [ group ] = entry.baan.split('.');

      // Skip lanes 41 and 42
      if (group === '41' || group === '42') continue;

      let priority = -1;
      const existingEntry = this._lane_queue.get(group);

    if (existingEntry) {
      if (existingEntry.activeSince > 0) {
        priority -= 3;
      }

      this._lane_queue.update_priority(group, priority);
    } else {
        this._lane_queue.enqueue(group, priority, this._time);
      }
    }
  }

  /* Outgoing data functions */
  transmit_state_to_publisher(): this {
    const state_map: TrafficlightStateMap = this.get_state_map();
    const state_message: string = JSON.stringify(state_map);

    this._publisher.send('stoplichten', state_message);

    return this;
  }
}
