import { Controller } from "./controller";
import { Lane } from "./lane";
import { TrafficLight } from "./trafficLight";
import {  Stopwatch } from "./stopwatch";

const clock = new Stopwatch();
const transitionDurationMs = 1000;

clock.set_speed(1);

const lane1: Lane = new Lane("lane1")
  .bind_trafficlight(new TrafficLight('1.1', clock, transitionDurationMs));

const lane2: Lane = new Lane("lane2")
  .bind_trafficlight(new TrafficLight('2.1', clock, transitionDurationMs))
  .bind_trafficlight(new TrafficLight('2.2', clock, transitionDurationMs));

const controller: Controller = new Controller(5557, clock)
  .bind_lane(lane1)
  .bind_lane(lane2)
  .connect_to_simulator('tcp://localhost:5557');


setInterval(() => {
  controller.transmit_state();
}, Math.round(500 / clock.speed));
