import { EventEmitter } from "stream";
import { Stopwatch } from "./stopwatch";

export enum TrafficLightState {
  RED = 'rood',
  GREEN = 'groen',
  YELLOW = 'geel',
};

export class TrafficLight extends EventEmitter {
  static transitions: Record<TrafficLightState, Record<TrafficLightState, TrafficLightState | null>> = {
    [TrafficLightState.RED]: {
      [TrafficLightState.GREEN]: TrafficLightState.YELLOW,
      [TrafficLightState.YELLOW]: TrafficLightState.RED,
      [TrafficLightState.RED]: null,
    },
    [TrafficLightState.GREEN]: {
      [TrafficLightState.RED]: TrafficLightState.GREEN,
      [TrafficLightState.YELLOW]: TrafficLightState.GREEN,
      [TrafficLightState.GREEN]: null,
    },
    [TrafficLightState.YELLOW]: {
      [TrafficLightState.RED]: TrafficLightState.RED,
      [TrafficLightState.GREEN]: TrafficLightState.YELLOW,
      [TrafficLightState.YELLOW]: null,
    },
  };

  private _transitionDurationMs = 3500;
  // private _clock: Stopwatch;

  public id: string = '';
  public state: TrafficLightState = TrafficLightState.RED;
  public time: number | null = null;
  public transitionedTime: number | null = null;

  constructor(id: string) {
    super();

    // this._transitionDurationMs = transitionDurationMs;
    // this._clock = clock;
    this.id = id;

    return this;
  }

  get scaledTransitionDuration(): number {
    return 0;
    // return Math.round(this._transitionDurationMs / this._clock.speed);
  }

  update_transition_duration(transitionDurationMs: number): this {
    this._transitionDurationMs = transitionDurationMs;

    return this;
  }

  update_time(time: number): this {
    this.time = time;

    return this;
  }

  transition_into(state: TrafficLightState): this {
    const nextState = TrafficLight.transitions[state][this.state];

    if (state === TrafficLightState.RED) {
      console.log('red light');
    }

    if (nextState !== null) {
      if (this.state === TrafficLightState.YELLOW && nextState === TrafficLightState.RED) {
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
