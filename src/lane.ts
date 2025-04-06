import { TrafficLight, TrafficLightState } from "./trafficLight";

export class Lane {
  public name: string = '';
  public trafficLights: TrafficLight[] = [];

  lane(name: string): this {
    this.name = name;

    return this;
  }

  bind_trafficlight(trafficlight: TrafficLight): this {
    this.trafficLights.push(trafficlight);

    return this;
  }

  get_state_map(): { [key: string]: TrafficLightState } {
    const stateMap: { [key: string]: TrafficLightState } = {};

    this.trafficLights.forEach((trafficLight) => {
      stateMap[trafficLight.id] = trafficLight.state;
    });

    return stateMap;
  }

}
