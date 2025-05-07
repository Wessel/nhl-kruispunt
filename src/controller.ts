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
  compatible_lanes(lane_name: string) {
    const groups: string[] = [];
    const lane = this._intersection.groups[lane_name];

    for (const group of Object.keys(this._intersection.groups)) {
      if (!lane.intersects_with.includes(group)) {
        groups.push(group);
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
  exhaust_lane_queue(): void {
    if (this.laneQueue.isEmpty()) return;

    const nextLane = this.laneQueue.peek();
    if (!nextLane) return;

    console.log(`Processing lane ${nextLane.group} with priority ${nextLane.priority}`);
    this.laneQueue.setActive(nextLane.group, this.time);

    const lane = this.lanes[nextLane.group];
    if (lane) {
      const lanes = this.compatible_lanes(nextLane.group);

      for (const group of Object.keys(this._intersection.groups)) {
        if (/*lanes.includes(group)*/ group === nextLane.group) {
          const laneInstance = this.lanes[group];
          if (laneInstance) {
            laneInstance.set_state(TrafficlightState.GREEN);
            console.log(`Setting lane ${group} to GREEN`);
          }
        } else {
          const laneInstance = this.lanes[group];
          if (laneInstance) {
            laneInstance.set_state(TrafficlightState.RED);
            console.log(`Setting lane ${group} to RED`);
          }
        }
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
    const data = JSON.parse(message);

    Object.keys(data).forEach((key) => {
      const [group] = key.split('.');
      const sensorData = data[key];

      let priority = 0;

      // Both sensors triggered - highest lane priority (2)
      if (sensorData.voor && sensorData.achter) {
        priority = 1;
        // Only front or back sensor triggered - medium priority (1)
      } else if (sensorData.voor || sensorData.achter) {
        priority = 2;
      }

      if (priority > 0) {
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
      } else if (this.laneQueue.contains(group)) {
        this.laneQueue.remove(group);
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
