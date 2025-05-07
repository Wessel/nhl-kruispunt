import { EventEmitter } from "stream";
import { TrafficLight, TrafficLightState } from "./trafficLight";

export class Lane extends EventEmitter {
  public name: string = '';
  private _trafficLights: TrafficLight[] = [];

  constructor(name: string) {
    super();

    this.name = name;

    return this;
  }

  bind_trafficlight(trafficlight: TrafficLight): this {
    this._trafficLights.push(trafficlight);

    trafficlight.on('state_changed', () => this.emit('state_changed'));

    return this;
  }

  // handle_state_changed() {

  // }

  update_time(time: number): this {
    this._trafficLights.forEach((trafficLight) => {
      trafficLight.update_time(time);
    });

    return this;
  }

  set_state(state: TrafficLightState) {
    this._trafficLights.forEach((trafficLight) => {
      trafficLight.transition_into(state);
    });
  }

  get_state_map(): { [key: string]: TrafficLightState } {
    const stateMap: { [key: string]: TrafficLightState } = {};

    this._trafficLights.forEach((trafficLight) => {
      stateMap[trafficLight.id] = trafficLight.state;
    });

    return stateMap;
  }

}
