import { Publisher } from "zeromq";
import { Stopwatch } from "../stopwatch";

export class ZmqPublisher {
  private _socket: Publisher;
  private _clock: Stopwatch;
  private _heartbeat: NodeJS.Timeout | null = null;
  private _heartbeatDelay: number = 1000;

  constructor(heartbeatDelay: number = 1000, clock?: Stopwatch) {
    this._socket = new Publisher();
    this._clock = clock || new Stopwatch();
    this._heartbeatDelay = heartbeatDelay;

    return this;
  }

  close() {
    this._socket.close();
  }

  bind(address: string): this {
    this._socket.bind(address);

    return this;
  }

  send(topic: string, message: string): this {
    this._socket.send([topic, message]);

    return this;
  }

  get heartbeatDelay(): number {
    return Math.round(this._heartbeatDelay / this._clock.speed);
  }


  toggle_heartbeat_loop(): this {
    if (this._heartbeat) {
      clearInterval(this._heartbeat);
      this._heartbeat = null;

      return this;
    }

    this._heartbeat = setInterval(() => {
      this.send('heartbeat', this._clock.toString())
    }, this.heartbeatDelay);

    return this;
  }
}
