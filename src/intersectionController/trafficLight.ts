// Not imported as type due to it being unusable in the Record.
import { TrafficlightState } from '../types';

import { EventEmitter } from 'stream';

export class Trafficlight extends EventEmitter {
  static transitions: Record<TrafficlightState, Record<TrafficlightState, TrafficlightState | null>> = {
    [TrafficlightState.RED]: {
      [TrafficlightState.GREEN]: TrafficlightState.YELLOW,
      [TrafficlightState.YELLOW]: TrafficlightState.RED,
      [TrafficlightState.RED]: null,
    },
    [TrafficlightState.GREEN]: {
      [TrafficlightState.RED]: TrafficlightState.GREEN,
      [TrafficlightState.YELLOW]: TrafficlightState.GREEN,
      [TrafficlightState.GREEN]: null,
    },
    [TrafficlightState.YELLOW]: {
      [TrafficlightState.RED]: TrafficlightState.RED,
      [TrafficlightState.GREEN]: TrafficlightState.YELLOW,
      [TrafficlightState.YELLOW]: null,
    },
  };

  public id: string = '';
  public state: TrafficlightState = TrafficlightState.RED;
  public time: number | null = null;
  public transitionedTime: number | null = null;

  constructor(id: string) {
    super();
    this.id = id;

    return this;
  }

  update_time(time: number): this {
    this.time = time;

    return this;
  }

  transition_into(state: TrafficlightState): this {
    const nextState = Trafficlight.transitions[state][this.state];

    if (nextState !== null) {
      if (this.state === TrafficlightState.YELLOW && nextState === TrafficlightState.RED) {
        // while (this.time! - this.transitionedTime! < this._transitionDurationMs) {
        // } // todo: fix type
          // console.log('waiting for transition to finish...');

        this.state = nextState;
        this.transitionedTime = this.time;
        this.emit('state_changed');
        this.transition_into(state);
      } else {
        this.transitionedTime = this.time;
        this.state = nextState;
        this.emit('state_changed');
        this.transition_into(state);
      }
    }

    return this;
  }
}
