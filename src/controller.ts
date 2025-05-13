import type { LaneMap } from "./types/LaneMap";
import { TrafficlightState, TrafficlightStateMap } from "./types/TrafficlightState";

import type { Stopwatch } from "./stopwatch";
import type { Lane } from "./lane";

import { PriorityQueue } from "./priorityQueue";
import { ZmqPublisher } from "./zeromq/publisher";
import { ZmqSubscriber } from "./zeromq/subscriber";

export class Controller {
  private _heartbeatDelay: number = 1000;

  private _clock: Stopwatch;
  private _in_cycle = false;
  private _cycle_delay = 3500 * 2;
  private _is_removing = false;

  private _intersection: any  = null;

  private _publisher: ZmqPublisher;
  private _subscriber: ZmqSubscriber = new ZmqSubscriber();

  public time: number = 0;

  public lanes: LaneMap = {};

  public priorityVehicleQueue: PriorityQueue = new PriorityQueue();
  public laneQueue: PriorityQueue = new PriorityQueue();

  constructor(publisher_port: number = 5555, clock: Stopwatch) {
    this._clock = clock;

    this._publisher = new ZmqPublisher(this._heartbeatDelay, this._clock);

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
      if (lane.intersects_with.includes(Number(group))) continue;
      if (not_allowed.includes(group)) continue;

      for (const lane of this._intersection.groups[group].intersects_with) {
        not_allowed.push(String(lane));
      }

      groups.push(group);
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

    this._in_cycle = true;

    const nextLane = this.laneQueue.peek();
    if (!nextLane) return;

    console.log(`Processing lane ${nextLane.group} (${this.laneQueue.contains(nextLane.group)}) with priority ${nextLane.priority}`);
    this.laneQueue.setActive(nextLane.group, this.time);

    const lane = this.lanes[nextLane.group];
    if (lane) {
      const lanes = this.compatible_lanes(nextLane.group);

        console.log(`Setting lane ${nextLane.group} to green and (${lanes.join(', ')})`);
      for (const group of Object.keys(this._intersection.groups)) {
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
      const sensorData = data[key];

      let priority;

      // Both sensors triggered - highest lane priority (2)
      if (sensorData.voor && sensorData.achter) {
        priority = 1;
        // Only front or back sensor triggered - medium priority (1)
      } else if (sensorData.voor || sensorData.achter) {
        priority = 2;
      }

      if (priority) {
        const existingEntry = this.laneQueue.get(group);
        if (existingEntry) {
          if (existingEntry.activeSince > 0) {
            priority -= 3;
          }

          this.laneQueue.updatePriority(group, priority);
        } else {
          this.laneQueue.enqueue(group, priority);
        }

        // console.log(`Lane ${group} added to queue with priority ${priority}`);
      } else {
        this._is_removing = true;
        this.laneQueue.remove(group);
        await this.delay_for(this._cycle_delay);
        this._is_removing = false;
      }
    });

    this.exhaust_lane_queue();
  }

  handle_topic_sensoren_speciaal(message: string) { }
  handle_topic_sensoren_bruggen(message: string) { }
  handle_topic_voorrangsvoertuig(message: string) { }

  /* Outgoing data functions */
  transmit_state_to_publisher(): this {
    const state_map: TrafficlightStateMap = this.get_state_map();
    const state_message: string = JSON.stringify(state_map);

    this._publisher.send('stoplichten', state_message);

    return this;
  }
}
