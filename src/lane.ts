import { TrafficLight, TrafficLightState } from "./trafficLight";

export class Lane {
  public name: string = '';
  private _trafficLights: TrafficLight[] = [];

  constructor(name: string) {
    this.name = name;

    return this;
  }

  bind_trafficlight(trafficlight: TrafficLight): this {
    this._trafficLights.push(trafficlight);

    return this;
  }

  get_state_map(): { [key: string]: TrafficLightState } {
    const stateMap: { [key: string]: TrafficLightState } = {};

    this._trafficLights.forEach((trafficLight) => {
      stateMap[trafficLight.id] = trafficLight.state;
    });

    return stateMap;
  }

}
