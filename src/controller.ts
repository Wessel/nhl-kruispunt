import { Lane } from "./lane";
import { TrafficLightState } from "./trafficLight";
import { ZmqPublisher } from "./zeromq/publisher";
import { ZmqSubscriber } from "./zeromq/subscriber";
import { Stopwatch } from "./stopwatch";

import {  PriorityQueue } from "./priorityQueue";

interface LaneMap {
  [key: string]: Lane;
}

export class Controller {
  private _heartbeatDelay: number = 1000;

  private _clock: Stopwatch;

  private _intersection: any  = null;

  private _publisher: ZmqPublisher;
  private _subscriber: ZmqSubscriber = new ZmqSubscriber();

  public lanes: LaneMap[] = [];

  public priorityVehicleQueue: PriorityQueue = new PriorityQueue();
  public laneQueue: PriorityQueue = new PriorityQueue();


  constructor(publisher_port: number = 5557, clock: Stopwatch) {
    this._clock = clock;

    this._publisher = new ZmqPublisher(this._heartbeatDelay, this._clock);

    this._publisher
      .bind('tcp://*:' + publisher_port)
      .toggle_heartbeat_loop();
  }

  register_intersection(intersection: any): this {
    this._intersection = intersection;

    return this;
  }

  connect_to_simulator(address: string): this {
    this._subscriber
      .connect(address)
      .subscribe('', (t, a) => { try { console.log(t, JSON.parse(a)) } catch (_) { console.log(t, a); } })
      .subscribe('sensoren_rijbaan',  (_, m) => this.handle_topic_sensoren_rijbaan(m))
      .subscribe('sensoren_speciaal', (_, m) => this.handle_topic_sensoren_speciaal(m))
      .subscribe('sensoren_bruggen',  (_, m) => this.handle_topic_sensoren_bruggen(m))
      .subscribe('voorrangsvoertuig', (_, m) => this.handle_topic_voorrangsvoertuig(m))
      .subscribe('tijd',              (_, m) => this.handle_topic_tijd(m))
      .bind();

    return this;
  }

  bind_lane(lane: Lane): this {
    this.lanes.push({ [lane.name]: lane });

    lane.on('state_changed', () => this.handle_lane_state_change());

    return this;
  }

  handle_lane_state_change() {
    this.transmit_state();
  }

  change_lane_state(lane_name: string, state: TrafficLightState) {
    for (const lane of this.lanes) {
      if (lane[lane_name]) {
        lane[lane_name].set_state(state);
        return;
      }
    }
    throw new Error(`Lane ${lane_name} not found`);
  }

  transmit_state(): this {
    const state_map: { [key: string]: TrafficLightState } = this.get_state_map();
    const state_message: string = JSON.stringify(state_map);

    this._publisher.send('stoplichten', state_message);

    return this;
  }

  get_state_map(): { [key: string]: TrafficLightState } {
    const state_map: { [key: string]: TrafficLightState } = {};

    for (const lane of this.lanes) {
      for (const lane_name in lane) {
        for (const l in lane[lane_name].get_state_map()) {
          state_map[`${lane_name}.${l}`] = lane[lane_name].get_state_map()[l];
        }
      }
    }

    return state_map;
  }

  handle_topic_sensoren_rijbaan(message: string) {
    const data = JSON.parse(message);

    Object.keys(data).forEach((key) => {
      const [ group ] = key.split('.');
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
          if (existingEntry.activeSince) {
            priority -= 3;
          }

          this.laneQueue.updatePriority(group, priority);
        } else {
          this.laneQueue.enqueue(group, priority);
        }

        console.log(`Lane ${group} added to queue with priority ${priority}`);
      }
    });

    this.exhaust_lane_queue();
  }

  exhaust_lane_queue(): void {
    if (!this.laneQueue.isEmpty()) {
      const nextLane = this.laneQueue.peek();

      if (nextLane) {
        console.log(`Processing lane ${nextLane.group} with priority ${nextLane.priority}`);
        // Logic to handle the lane with highest priority
        // This would likely change traffic light states based on the priority
      }
    }
  }

  handle_topic_sensoren_speciaal(message: string) {}
  handle_topic_sensoren_bruggen(message: string) {}
  handle_topic_voorrangsvoertuig(message: string) {}
  handle_topic_tijd(message: string) {}

  // start() {
  // }
}
