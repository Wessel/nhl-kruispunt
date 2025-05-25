// Not imported as type due to it being unusable in the Record.
import type { Lane } from '.';
import {
  type LaneMap, type TrafficlightStateMap, type PriorityVehicleQueue,
  BridgeState, TrafficlightState
} from '../types';

import { PRIORITY_HIGH, PRIORITY_LOW, BRIDGE_LANES } from '../constants';

import { PriorityQueue } from '../priorityQueue';
import { ZmqPublisher, ZmqSubscriber } from '../zeromq';

export class Controller {
  private _heartbeatDelay: number = 1000;
  private _bridgeSensors: { [sensor: string]: { voor: boolean, achter: boolean }} = {};
  private _bridge_cycle = false;

  private _in_cycle = false;
  private _is_removing = false;

  private _pedestrian_multiplier = 2;
  private _cycle_delay = 3500;
  private _removing_delay = this._cycle_delay * 2;
  private _empty_delay = this._cycle_delay / 2;

  private _intersection: any  = null;

  private _publisher: ZmqPublisher;
  private _subscriber: ZmqSubscriber = new ZmqSubscriber();

  public time: number = 0;

  public lanes: LaneMap = {};
  public voorrangsLane?: string;
  public bridgeState: BridgeState = BridgeState.UNKNOWN;
  public specialSensors: any = {
    brug_wegdek: false,
    brug_water: false,
    brug_file: false
  }

  public priorityVehicleQueue: PriorityQueue = new PriorityQueue();
  public laneQueue: PriorityQueue = new PriorityQueue();

  constructor(publisher_port: number = 5555) {
    this._publisher = new ZmqPublisher(this._heartbeatDelay);

    this._publisher
      .bind(`tcp://*:${publisher_port}`)
      .toggle_heartbeat_loop();
  }

  /* Initialisation Functions */
  register_intersection(intersection: any): this {
    this._intersection = intersection;

    return this;
  }

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
    this.lanes[lane.name] = lane;

    lane.on('state_changed', () => this.transmit_state_to_publisher());

    return this;
  }

  /* Helper functions */
  delay_for(ms: number): Promise<void> {
    const targetTime = this.time + ms;

    return new Promise((resolve) => {
      const checkTime = () => {
        if (this.time >= targetTime) {
          // console.log(`Delay for ${ms}ms completed at time ${this.time}`);
          resolve();
        } else {
          setTimeout(checkTime, 10);
        }
      };

      checkTime();
    });
  }

  compatible_lanes(lane_name: string) {
    const groups: string[] = [];
    const not_allowed: string[] = [];
    const lane = this._intersection.groups[lane_name];

    for (const group of Object.keys(this._intersection.groups)) {
      if (!lane.intersects_with.includes(Number(group))) {
        if (!not_allowed.includes(group)) {
          for (const lane of this._intersection.groups[group].intersects_with) {
            not_allowed.push(String(lane));
          }
          groups.push(group);
        }
      }

    }

    return groups;
  }

  get_state_map(): TrafficlightStateMap {
    const state_map: TrafficlightStateMap = {};

    for (const lane of Object.keys(this.lanes)) {
      for (const l in this.lanes[lane].get_state_map()) {
        state_map[`${lane}.${l}`] = this.lanes[lane].get_state_map()[l];
      }
    }

    return state_map;
  }

  /* Priority Queue functions */
  async exhaust_lane_queue(): Promise<void> {
    if (this.laneQueue.isEmpty() || this._in_cycle) return;


    const nextLane = this.laneQueue.peek();
    if (!nextLane) return;

    this._in_cycle = true;

    this.cycle_intersection_red();
    await this.delay_for(this._empty_delay);

    console.log(`Processing lane ${nextLane.group} (${this.laneQueue.contains(nextLane.group)}) with priority ${nextLane.priority}`);

    const lane = this.lanes[nextLane.group];
    if (lane) {
      const lanes = this.compatible_lanes(nextLane.group);

        console.log(`Setting lane ${nextLane.group} to green and (${lanes.join(', ')})`);

      this.laneQueue.setActive(nextLane.group, this.time);

      for (const group of Object.keys(this._intersection.groups)) {
        if (BRIDGE_LANES.includes(Number(group))) continue;

        if (lanes.includes(group)) { //  group === nextLane.group
          const laneInstance = this.lanes[group];
          if (laneInstance) {
            laneInstance.set_state(TrafficlightState.GREEN);
            // console.log(`Setting lane ${group} to GREEN`);
          }
        } else if (this.laneQueue.contains(nextLane.group)) {
          const laneInstance = this.lanes[group];
          if (laneInstance) {
            laneInstance.set_state(TrafficlightState.RED);
            // console.log(`Setting lane ${group} to RED`);
          }
        }
      }
    }

    // await this.delay_for(this._cycle_delay);
    this._in_cycle = false;
  }

  cycle_intersection_red(): void {
    for (const lane of Object.keys(this.lanes)) {
      if (!BRIDGE_LANES.includes(Number(lane))) {
        this.lanes[lane].set_state(TrafficlightState.RED);
      }
    }
  }

  async wait_for_empty_bridge(): Promise<void> {
    return new Promise<void>((resolve) => {
      const check_road = () => {
        console.log(`Checking if bridge road is clear: ${this.specialSensors.brug_wegdek}`);
        if (!this.specialSensors.brug_wegdek) {
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
          console.log(`Checking if bridge water is clear: ${this.specialSensors.brug_water}`);
        if (!this.specialSensors.brug_water) {
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
        console.log(`Checking if bridge is closed: ${this.bridgeState}`);
        if (this.bridgeState === BridgeState.CLOSED) {
          resolve();
        } else {
          setTimeout(check_bridge, 100);
        }
      };
      check_bridge();
    });
  }

  toggle_bridge_lights(state: TrafficlightState): void {
    for (const lane of BRIDGE_LANES) {
      if (lane !== 71 && lane !== 72 && lane !== 81) {
        this.lanes[String(lane)].set_state(state);
      }
    }
  }

  async wait_for_bridge_opened(): Promise<void> {
    return new Promise<void>((resolve) => {
      const check_bridge = () => {
        console.log(`Checking if bridge is opened: ${this.bridgeState}`);
        if (this.bridgeState === BridgeState.OPEN) {
          resolve();
        } else {
          setTimeout(check_bridge, 100);
        }
      };
      check_bridge();
    });
  }

  async handle_bridge(): Promise<void> {
    if (this._bridge_cycle) return;
    // todo: allow multiple boats to pass, also first do 71 and if any at 72 do 72
    if (this._bridgeSensors[71]?.voor || this._bridgeSensors[72]?.voor) {
      this._bridge_cycle = true;

      this.toggle_bridge_lights(TrafficlightState.RED);

      await this.wait_for_empty_bridge();

      this.lanes['81'].set_state(TrafficlightState.GREEN);

      await this.wait_for_bridge_opened();

      this.lanes['71'].set_state(TrafficlightState.GREEN);
      this.lanes['72'].set_state(TrafficlightState.GREEN);

      await this.delay_for(10000);

      this.lanes['71'].set_state(TrafficlightState.RED);
      this.lanes['72'].set_state(TrafficlightState.RED);

      await this.wait_for_empty_bridge_water();

      this.lanes['81'].set_state(TrafficlightState.RED);

      await this.wait_for_bridge_closed();

      this.toggle_bridge_lights(TrafficlightState.GREEN);

      this._bridge_cycle = false;

    } else {
      this.lanes['41'].set_state(TrafficlightState.GREEN);
      this.toggle_bridge_lights(TrafficlightState.GREEN);
    }

  }

  /* Incoming data functions */
  handle_topic_tijd(message: string) {
    const data = JSON.parse(message);
    const time = data.simulatie_tijd_ms;

    this.time = time;

    for (const lane of Object.keys(this.lanes)) {
      this.lanes[lane].update_time(time);
    }
  }

  handle_topic_sensoren_rijbaan(message: string) {
    if (this._is_removing) return;
    const data = JSON.parse(message);

    let bridge = 0;
    Object.keys(data).forEach(async(key) => {
      const [ group ] = key.split('.');
      const groupAsNumber = Number(group);

      // Skip all lanes related to bridges, due to them being handled by the bridge control
      if (BRIDGE_LANES.includes(groupAsNumber)) {
        this._bridgeSensors[groupAsNumber] = data[key];
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
        const existingEntry = this.laneQueue.get(group);
        if (existingEntry) {
          if (existingEntry.activeSince > 0) {
            if (this.time > existingEntry.activeSince + 5000) {
              this.laneQueue.remove(group);
              return;
            }

            if (existingEntry.priority < 0) {
              return;
            }

            priority -= 4;
          }

          this.laneQueue.updatePriority(group, priority);
        } else {
          this.laneQueue.enqueue(group, priority);
        }
      } else if (this.laneQueue.contains(group)) {
        this._is_removing = true;

        this.laneQueue.remove(group);

        if (
          this._intersection.groups[group].vehicle_type.includes('walk')
          || this._intersection.groups[group].vehicle_type.includes('bike')
        ) {
          await this.delay_for(this._removing_delay * this._pedestrian_multiplier);
        } else {
          await this.delay_for(this._removing_delay);
        }

        this._is_removing = false;
      }
    });

    this.exhaust_lane_queue();
  }

  handle_topic_sensoren_speciaal(message: string) {
    this.specialSensors = JSON.parse(message);
  }

  handle_topic_sensoren_bruggen(message: string) {
    const data = JSON.parse(message);

    for (const key of Object.keys(data)) {
      const bridge = data[key];

      this.bridgeState = bridge.state;
      // console.log(`Bridge ${key} state: ${this.bridgeState}`);
    }
    }

  handle_topic_voorrangsvoertuig(message: string) {
    const queue: PriorityVehicleQueue = JSON.parse(message)?.queue;

    // Check if any emergency vehicles are no longer in the queue
    if (this.voorrangsLane) {
      const emergency_vehicles = queue
        .filter(v => v.prioriteit === 1)
        .map(v => v.baan.split('.')[0]);

      if (!emergency_vehicles.includes(this.voorrangsLane)) {
        this._in_cycle = false;
        this.voorrangsLane = undefined;
      }
    }

    // Process all priority vehicles
    const emergencyVehicles = queue.filter(v => v.prioriteit === 1 && v.baan.split('.')[0] !== '41' && v.baan.split('.')[0] !== '42');
    const publicTransport = queue.filter(v => v.prioriteit === 2);

    // Handle emergency vehicles (priority 1) - multiple can be active
    if (emergencyVehicles.length > 0) {
      this._in_cycle = true;
      const emergencyLanes = emergencyVehicles.map(v => v.baan.split('.')[0]);
      this.voorrangsLane = emergencyLanes[0]; // Keep track of first emergency lane for compatibility

      for (const lane of Object.keys(this.lanes)) {
      if (BRIDGE_LANES.includes(Number(lane))) continue;

      if (emergencyLanes.includes(lane)) {
        this.lanes[lane].set_state(TrafficlightState.GREEN);
      } else {
        if (BRIDGE_LANES.includes(Number(lane))) continue;
        this.lanes[lane].set_state(TrafficlightState.RED);
      }
      }
    }

    // Handle public transport (priority 2)
    for (const entry of publicTransport) {
      const [ group ] = entry.baan.split('.');

      // Skip lanes 41 and 42
      if (group === '41' || group === '42') continue;

      let priority = -1;
      const existingEntry = this.laneQueue.get(group);

    if (existingEntry) {
      if (existingEntry.activeSince > 0) {
        priority -= 3;
      }

      this.laneQueue.updatePriority(group, priority);
    } else {
        this.laneQueue.enqueue(group, priority);
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
