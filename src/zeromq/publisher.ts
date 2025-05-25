import type { QueuedMessage } from '../types'

import { Stopwatch } from '.';

import { Publisher } from 'zeromq';

export class ZmqPublisher {
  private _socket: Publisher;
  private _clock: Stopwatch;
  private _heartbeat: NodeJS.Timeout | null = null;
  private _heartbeat_delay: number = 1000;

  private _message_queue: QueuedMessage[] = [];
  private _is_sending: boolean = false;

  constructor(heartbeatDelay: number = 1000, clock?: Stopwatch) {
    this._socket = new Publisher();
    this._clock = clock || new Stopwatch();
    this._heartbeat_delay = heartbeatDelay;

    return this;
  }

  close() {
    this._socket.close();
  }

  bind(address: string): this {
    this._socket.bind(address);

    return this;
  }

  async send(topic: string, message: string) {
    this._message_queue.push({ topic, message });

    if (!this._is_sending) {
      await this._process_queue();
    }

    return this;
  }

  private async _process_queue(): Promise<void> {
    if (this._message_queue.length === 0 || this._is_sending) {
      return;
    }

    this._is_sending = true;

    try {
      while (this._message_queue.length > 0) {
        const { topic, message } = this._message_queue[0];

        await this._socket.send([topic, message]);

        this._message_queue.shift();
      }
    } catch (error) {
      console.error('Error sending message:', error);
    } finally {
      this._is_sending = false;

      // If new messages were added during processing, process them
      if (this._message_queue.length > 0) {
        // Use setTimeout to avoid deep recursion
        setTimeout(() => this._process_queue(), 0);
      }
    }
  }

  get heartbeatDelay(): number {
    return Math.round(this._heartbeat_delay / this._clock.speed);
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
