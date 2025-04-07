import { Controller } from "./controller";
import { Lane } from "./lane";
import { TrafficLight } from "./trafficLight";
import {  Stopwatch } from "./stopwatch";

const clock = new Stopwatch();

clock.set_speed(1);

const lane1: Lane = new Lane("lane1")
  .bind_trafficlight(new TrafficLight('1.1', clock, 5000));

const controller: Controller = new Controller(5557, clock)
  .bind_lane(lane1)
  .connect_to_simulator('tcp://localhost:5557');


setInterval(() => {
  controller.transmit_state();
}, Math.round(1000 / clock.speed));
