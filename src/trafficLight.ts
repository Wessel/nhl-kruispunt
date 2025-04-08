import { Stopwatch } from "./stopwatch";

export enum TrafficLightState {
  RED = 'rood',
  GREEN = 'groen',
  YELLOW = 'geel',
};

export class TrafficLight {
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

  private _transitionDurationMs = 500;
  private _clock: Stopwatch;

  public id: string = '';
  public state: TrafficLightState = TrafficLightState.RED;


  constructor(id: string, clock: Stopwatch, transitionDurationMs: number) {
    this._transitionDurationMs = transitionDurationMs;
    this._clock = clock;
    this.id = id;

    return this;
  }

  get scaledTransitionDuration(): number {
    return Math.round(this._transitionDurationMs / this._clock.speed);
  }

  update_transition_duration(transitionDurationMs: number): this {
    this._transitionDurationMs = transitionDurationMs;

    return this;
  }

  transition_into(state: TrafficLightState): this {
    const nextState = TrafficLight.transitions[state][this.state];

    if (nextState !== null) {
      setTimeout(() => {
        this.state = nextState;
        this.transition_into(state);
      }, this.scaledTransitionDuration);
    }

    return this;
  }
}
