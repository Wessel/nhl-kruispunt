import { Lane } from '../../intersectionController/lane';

export type LaneName = string;

export type LaneList = LaneName[]

export interface LaneMap {
  [key: string]: Lane;
}
