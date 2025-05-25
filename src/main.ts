// Wessel T <contact@wessel.gg> (https://wessel.gg/)
//
// 'Build your own tower.
// A kingdom freed from malice.
// Create a world of bounty, peace and beauty.'
// ⠀⣠⣶⣶⣤⣁
// ⢰⣷⡟⠻⣏⠻⣧⣀⠐⠈⠀⡈⠠⠀⠂⠀⢁⠈⠀⠄⠀⠁⣠⣴⣤⡈
// ⠀⣿⢿⡀⠘⢦⡈⢻⣦⠐⠀⠀⠄⠀⠁⠈⠀⡀⠠⠀⣠⡿⣿⣯⣽⡇
// ⠀⢻⣿⠛⢦⣄⣹⠦⣌⣳⡀⠀⣠⠈⠀⢾⠀⠀⢀⣼⢯⠞⢡⣿⣿⠁
// ⠄⠘⣿⣷⣤⡀⠙⣆⠈⠻⣿⡄⠘⣇⠀⣾⠀⣴⠿⢲⣋⣤⣿⡿⠃⠀
// ⠀⡀⠹⣿⣦⡉⠛⠚⣆⠀⠈⠻⣆⢻⢠⣇⡾⠃⢠⣟⣠⣾⡞⠃⠀
// ⠂⡀⠄⠹⣿⣏⠛⠒⠾⠷⣄⠀⠙⣞⣿⠋⣀⣴⣋⣽⡿⠋
// ⠂⠠⠀⠀⣨⣿⢿⣶⣒⠲⢮⣿⣶⣼⣧⣾⣭⣿⠟⠉
// ⠐⠀⠁⣰⣿⠓⠒⣛⣻⠟⠛⣩⣿⣯⠙⡯⣿⡆
// ⠐⠀⠄⠸⣿⡟⢉⡽⢛⣿⡿⠉⠀⢸⣧⡷⣾⡇
// ⠀⢂⠀⠄⠹⢿⣿⣴⣯⠏⠀⠀⠀⣼⢸⣽⣷⠇
// ⠠⠀⠂⢀⠀⢀⠈⠉⠀⠀⠀⠂⡀⠹⠿⠛⠁⠀⠀
import type { IntersectionConfig } from './types';

import { HELP_STRING } from './constants';

import larg from './lib/larg'
import { Controller, Lane, Trafficlight } from './intersectionController';

import { readFileSync } from 'fs';

const args = larg(process.argv);

if (args.help) {
  console.log(HELP_STRING);
  process.exit(0)
}

const intersectionFilePath = args.intersection ? String(args.intersection) : './static/intersection/lanes.json';
const intersectionFile = readFileSync(intersectionFilePath, 'utf-8');
const intersectionData: IntersectionConfig = JSON.parse(intersectionFile);

const controller: Controller = new Controller(5555)
  .register_intersection(intersectionData);

for (const [key, value] of Object.entries(intersectionData.groups)) {
  const lane: Lane = new Lane(key);

  for (const trafficlight of Object.keys(value.lanes)) {
    lane.bind_trafficlight(new Trafficlight(trafficlight));
  }

  controller.bind_lane(lane);
}

let connection_string = `tcp://localhost:5556`;

if (args.ip) connection_string = connection_string.replace(/localhost/, String(args.ip));
if (args.port) connection_string = connection_string.replace(/5556/, String(args.port));
if (args.cstr) connection_string = String(args.cstr);

console.log(`Simulator connection string:\t ${connection_string}`)

controller.connect_to_simulator(connection_string);
