import { Controller } from "./controller";
import { Lane } from "./lane";
import { TrafficLight, TrafficLightState } from "./trafficLight";
import {  Stopwatch } from "./stopwatch";

import { readFileSync } from "fs";

const intersectionData = JSON.parse(readFileSync('./static/intersection/lanes.json', 'utf-8'));

const clock = new Stopwatch();
const transitionDurationMs = 250;

clock.set_speed(1);

const controller: Controller = new Controller(5555, clock)
  .register_intersection(intersectionData);

for (const [key, value] of Object.entries(intersectionData.groups)) {
  const lane: Lane = new Lane(key);

  for (const trafficlight of Object.keys(value.lanes)) {
    lane.bind_trafficlight(new TrafficLight(trafficlight));
  }

  controller.bind_lane(lane);
}

controller.connect_to_simulator('tcp://localhost:5556'); //5556


// setInterval(() => {
//   const state = controller.get_state_map();
//   const passedLanes: string[] = [];

//   Object.keys(state).forEach((key) => {
//     const laneKey = key.split('.')[0];
//     if (!passedLanes.includes(laneKey)) {
//       passedLanes.push(laneKey);
//       const lane = state[key];

//       controller.change_lane_state(laneKey, lane === TrafficLightState.GREEN ? TrafficLightState.RED : TrafficLightState.GREEN);
//     }
//   });


//   // controller.transmit_state();
// }, 10000);
