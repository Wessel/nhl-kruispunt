import { Lane } from "./lane";
import { TrafficLightState } from "./trafficLight";
import { ZmqPublisher } from "./zeromq/publisher";
import { ZmqSubscriber } from "./zeromq/subscriber";
import { Stopwatch } from "./stopwatch";

interface LaneMap {
  [key: string]: Lane;
}

export class Controller {
  private _heartbeatDelay: number = 1000;

  private _clock: Stopwatch;

  private _publisher: ZmqPublisher;
  private _subscriber: ZmqSubscriber = new ZmqSubscriber();

  public lanes: LaneMap[] = [];

  constructor(publisher_port: number = 5557, clock: Stopwatch) {
    this._clock = clock;

    this._publisher = new ZmqPublisher(this._heartbeatDelay, this._clock);

    this._publisher
      .bind('tcp://*:' + publisher_port)
      .toggle_heartbeat_loop();
  }

  connect_to_simulator(address: string): this {
    this._subscriber
      .connect(address)
      .subscribe('', console.log)
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

    return this;
  }

  transmit_state(): this {
    const state_map: { [key: string]: { [key: string]: TrafficLightState } } = this.get_state_map();
    const state_message: string = JSON.stringify(state_map);

    this._publisher.send('stoplichten', state_message);

    return this;
  }

  get_state_map(): { [key: string]: { [key: string]: TrafficLightState } } {
    const state_map: { [key: string]: { [key: string]: TrafficLightState } } = {};

    for (const lane of this.lanes) {
      for (const lane_name in lane) {
        state_map[lane_name] = lane[lane_name].get_state_map();
      }
    }

    return state_map;
  }

  handle_topic_sensoren_rijbaan(message: string) {}
  handle_topic_sensoren_speciaal(message: string) {}
  handle_topic_sensoren_bruggen(message: string) {}
  handle_topic_voorrangsvoertuig(message: string) {}
  handle_topic_tijd(message: string) {}
}
