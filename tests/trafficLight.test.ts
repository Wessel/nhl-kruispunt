// import { TrafficLight, TrafficLightState } from "../src/trafficLight";
// import { assertEquals } from "https://deno.land/std@0.203.0/testing/asserts.ts";

// const transitionDurationMs = 10;

// const sleep = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));

// Deno.test("TrafficLight initializes with default state RED", () => {
//   const trafficLight = new TrafficLight();
//   assertEquals(trafficLight.state, TrafficLightState.RED);
// });

// Deno.test("TrafficLight updates transition duration", () => {
//   const trafficLight = new TrafficLight();

//   trafficLight.update_transition_duration(1000);
//   assertEquals(trafficLight["transitionDurationMs"], 1000);
// });

// Deno.test("TrafficLight Transitions to orange then red", async () => {
//   const trafficLight = new TrafficLight();
//   trafficLight.update_transition_duration(transitionDurationMs);

//   trafficLight.transition_into(TrafficLightState.GREEN);
//   await sleep(transitionDurationMs);
//   assertEquals(trafficLight.state, TrafficLightState.GREEN);

//   trafficLight.transition_into(TrafficLightState.RED);

//   await sleep(transitionDurationMs);
//   assertEquals(trafficLight.state, TrafficLightState.YELLOW);

//   await sleep(transitionDurationMs);
//   assertEquals(trafficLight.state, TrafficLightState.RED);
// });
