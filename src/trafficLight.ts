export enum TrafficLightState {
  RED = 'red',
  GREEN = 'green',
  YELLOW = 'yellow',
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

  public id: string = '';
  public state: TrafficLightState = TrafficLightState.RED;
  private transitionDurationMs = 500;

  trafficLight(id: string, transitionDurationMs: number): this {
    this.transitionDurationMs = transitionDurationMs;
    this.id = id;

    return this;
  }

  update_transition_duration(transitionDurationMs: number): this {
    this.transitionDurationMs = transitionDurationMs;

    return this;
  }

  transition_into(state: TrafficLightState): this {
    const nextState = TrafficLight.transitions[state][this.state];

    if (nextState !== null) {
      setTimeout(() => {
        this.state = nextState;
        this.transition_into(state);
      }, this.transitionDurationMs);
    }

    return this;
  }
}
