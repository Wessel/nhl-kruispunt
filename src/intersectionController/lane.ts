import type { TrafficlightState, TrafficlightStateMap } from '../types';
import type { Trafficlight } from '.';

import { EventEmitter } from 'stream';

export class Lane extends EventEmitter {
  public name: string = '';
  private _trafficlights: Trafficlight[] = [];

  constructor(name: string) {
    super();

    this.name = name;

    return this;
  }

  bind_trafficlight(trafficlight: Trafficlight): this {
    this._trafficlights.push(trafficlight);

    trafficlight.on('state_changed', () => this.emit('state_changed'));

    return this;
  }

  update_time(time: number): this {
    this._trafficlights.forEach((trafficLight) => {
      trafficLight.update_time(time);
    });

    return this;
  }

  set_state(state: TrafficlightState) {
    this._trafficlights.forEach((trafficLight) => {
      trafficLight.transition_into(state);
    });
  }

  get_state_map(): TrafficlightStateMap {
    const stateMap: TrafficlightStateMap = {};

    this._trafficlights.forEach((trafficLight) => {
      stateMap[trafficLight.id] = trafficLight.state;
    });

    return stateMap;
  }
}
