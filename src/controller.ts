import { Lane } from "./lane";
import { TrafficLightState } from "./trafficLight";

interface LaneMap {
  [key: string]: Lane;
}

export class Controller {
  public lanes: LaneMap[] = [];

  bind_lane(lane: Lane): this {
    this.lanes.push({ [lane.name]: lane });

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

}
