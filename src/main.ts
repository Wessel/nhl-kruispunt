import { Controller } from "./controller";
import { Lane } from "./lane";
import { TrafficLight, TrafficLightState } from "./trafficLight";
import {  Stopwatch } from "./stopwatch";

import { readFileSync } from "fs";

const intersectionData = JSON.parse(readFileSync('./static/intersection/lanes.json', 'utf-8'));

const clock = new Stopwatch();
const transitionDurationMs = 250;

clock.set_speed(1);

const lane0: Lane = new Lane("0")
  .bind_trafficlight(new TrafficLight('1', clock, transitionDurationMs))
  .bind_trafficlight(new TrafficLight('2', clock, transitionDurationMs))
  .bind_trafficlight(new TrafficLight('3', clock, transitionDurationMs))
  .bind_trafficlight(new TrafficLight('4', clock, transitionDurationMs))
  .bind_trafficlight(new TrafficLight('5', clock, transitionDurationMs))
  .bind_trafficlight(new TrafficLight('6', clock, transitionDurationMs))
  .bind_trafficlight(new TrafficLight('7', clock, transitionDurationMs))
  .bind_trafficlight(new TrafficLight('8', clock, transitionDurationMs))
  .bind_trafficlight(new TrafficLight('9', clock, transitionDurationMs));


const lane1: Lane = new Lane("1")
  .bind_trafficlight(new TrafficLight('1', clock, transitionDurationMs))
  .bind_trafficlight(new TrafficLight('2', clock, transitionDurationMs))
  .bind_trafficlight(new TrafficLight('3', clock, transitionDurationMs));

const lane2: Lane = new Lane("2")
  .bind_trafficlight(new TrafficLight('1', clock, transitionDurationMs))
  .bind_trafficlight(new TrafficLight('2', clock, transitionDurationMs))
  .bind_trafficlight(new TrafficLight('3', clock, transitionDurationMs))
  .bind_trafficlight(new TrafficLight('4', clock, transitionDurationMs))
  .bind_trafficlight(new TrafficLight('5', clock, transitionDurationMs))
  .bind_trafficlight(new TrafficLight('6', clock, transitionDurationMs))
  .bind_trafficlight(new TrafficLight('7', clock, transitionDurationMs))
  .bind_trafficlight(new TrafficLight('8', clock, transitionDurationMs))

const lane3: Lane = new Lane("3")
  .bind_trafficlight(new TrafficLight('1', clock, transitionDurationMs))
  .bind_trafficlight(new TrafficLight('2', clock, transitionDurationMs))
  .bind_trafficlight(new TrafficLight('3', clock, transitionDurationMs))
  .bind_trafficlight(new TrafficLight('4', clock, transitionDurationMs))
  .bind_trafficlight(new TrafficLight('5', clock, transitionDurationMs))
  .bind_trafficlight(new TrafficLight('6', clock, transitionDurationMs))
  .bind_trafficlight(new TrafficLight('7', clock, transitionDurationMs))
  .bind_trafficlight(new TrafficLight('8', clock, transitionDurationMs));

const lane4 = new Lane("4")
  .bind_trafficlight(new TrafficLight('4.1', clock, transitionDurationMs))
  .bind_trafficlight(new TrafficLight('4.2', clock, transitionDurationMs))


const lane5 = new Lane("5")
  .bind_trafficlight(new TrafficLight('1', clock, transitionDurationMs))
  .bind_trafficlight(new TrafficLight('2', clock, transitionDurationMs))
  .bind_trafficlight(new TrafficLight('3', clock, transitionDurationMs))
  .bind_trafficlight(new TrafficLight('4', clock, transitionDurationMs))

const lane6: Lane = new Lane("6")
  .bind_trafficlight(new TrafficLight('1', clock, transitionDurationMs))
  .bind_trafficlight(new TrafficLight('2', clock, transitionDurationMs))
  .bind_trafficlight(new TrafficLight('3', clock, transitionDurationMs))
  .bind_trafficlight(new TrafficLight('4', clock, transitionDurationMs))


const  lane7: Lane = new Lane("7")
  .bind_trafficlight(new TrafficLight('1', clock, transitionDurationMs))
  .bind_trafficlight(new TrafficLight('2', clock, transitionDurationMs));

const controller: Controller = new Controller(5555, clock)
  .register_intersection(intersectionData)
  .bind_lane(lane1)
  .bind_lane(lane2)
  .bind_lane(lane3)
  .bind_lane(lane4)
  .bind_lane(lane5)
  .bind_lane(lane6)
  .bind_lane(lane7)
  .bind_lane(lane0)
  .connect_to_simulator('tcp://localhost:5555'); //5556


setInterval(() => {
  const state = controller.get_state_map();
  const passedLanes: string[] = [];

  Object.keys(state).forEach((key) => {
    const laneKey = key.split('.')[0];
    if (!passedLanes.includes(laneKey)) {
      passedLanes.push(laneKey);
      const lane = state[key];

      controller.change_lane_state(laneKey, lane === TrafficLightState.GREEN ? TrafficLightState.RED : TrafficLightState.GREEN);
    }
  });


  // controller.transmit_state();
}, Math.round(1000 / clock.speed));
