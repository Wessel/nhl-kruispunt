import type { LaneMap } from "./types/LaneMap";
import { TrafficlightState, TrafficlightStateMap } from "./types/TrafficlightState";

import type { Stopwatch } from "./stopwatch";
import type { Lane } from "./lane";

import { PriorityQueue } from "./priorityQueue";
import { ZmqPublisher } from "./zeromq/publisher";
import { ZmqSubscriber } from "./zeromq/subscriber";
import { BridgeState } from "./types/Bridge";

const PRIORITY_HIGH = 1;
const PRIORITY_LOW = 2;

export class Controller {
  private _heartbeatDelay: number = 1000;
  private _bridgeTrafficlights: number[] = [ 71, 72, 81, 41, 42, 51, 52, 53, 54 ];

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
  public voorrangsLane?;
  public bridgeState: BridgeState = BridgeState.UNKNOWN;
  public specialSensors: any = {
    brug_wegdek: false,
    brug_water: false,
    brug_file: false
  }

  public priorityVehicleQueue: PriorityQueue = new PriorityQueue();
  public laneQueue: PriorityQueue = new PriorityQueue();

  constructor(publisher_port: number = 5555, clock: Stopwatch) {
    this._publisher = new ZmqPublisher(this._heartbeatDelay, clock);

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
          console.log(`Delay for ${ms}ms completed at time ${this.time}`);
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
      for (const group of Object.keys(this._intersection.groups)) {
        if (lanes.includes(group)) { //  group === nextLane.group
          const laneInstance = this.lanes[group];
          if (laneInstance) {
            this.laneQueue.setActive(group, this.time);

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
    // todo!: Ontruimingstijd fiksen
  }

  cycle_intersection_red(): void {
    for (const lane of Object.keys(this.lanes)) {
      if (lane === '41' || lane === '42') {
        this.lanes[lane].set_state(TrafficlightState.GREEN);
      } else {
        this.lanes[lane].set_state(TrafficlightState.RED);
      }
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

    Object.keys(data).forEach(async(key) => {
      const [ group ] = key.split('.');

      // Skip all lanes related to bridges, due to them being handled by the bridge control
      if (this._bridgeTrafficlights.includes(Number(group))) {
        return;
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
    // todo: meer voorrangsvoertuigen werkentd
    const data = JSON.parse(message);

    if (this.voorrangsLane) {
      // todo: check if emergency vehicle
      if (!data.queue.map(v => v.baan).includes(this.voorrangsLane)) {
        this._in_cycle = false;
        this.voorrangsLane = undefined;
      }
    }

      console.log(data);
    for (const entry of data.queue) {
      const [ group ] = entry.baan.split('.');

      switch (entry.prioriteit) {
        case 1: // hulpdiensten
          this._in_cycle = true;
          this.voorrangsLane = group;
          for (const lane of Object.keys(this.lanes)) {
            if (lane === group) {
              this.lanes[lane].set_state(TrafficlightState.GREEN);
            } else {
              this.lanes[lane].set_state(TrafficlightState.RED);
            }
          }
        case 2: // OV
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
  }

  /* Outgoing data functions */
  transmit_state_to_publisher(): this {
    const state_map: TrafficlightStateMap = this.get_state_map();
    const state_message: string = JSON.stringify(state_map);

    this._publisher.send('stoplichten', state_message);

    return this;
  }
}
