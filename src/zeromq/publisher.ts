import type { QueuedMessage } from '../types'

import { Stopwatch } from '.';

import { Publisher } from 'zeromq';

export class ZmqPublisher {
  private _socket: Publisher;
  private _clock: Stopwatch;
  private _heartbeat: NodeJS.Timeout | null = null;
  private _heartbeatDelay: number = 1000;

  private _messageQueue: QueuedMessage[] = [];
  private _isSending: boolean = false;

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

  async send(topic: string, message: string) {
    this._messageQueue.push({ topic, message });

    if (!this._isSending) {
      await this._processQueue();
    }

    return this;
  }

  private async _processQueue(): Promise<void> {
    if (this._messageQueue.length === 0 || this._isSending) {
      return;
    }

    this._isSending = true;

    try {
      while (this._messageQueue.length > 0) {
        const { topic, message } = this._messageQueue[0];

        await this._socket.send([topic, message]);

        this._messageQueue.shift();
      }
    } catch (error) {
      console.error('Error sending message:', error);
    } finally {
      this._isSending = false;

      // If new messages were added during processing, process them
      if (this._messageQueue.length > 0) {
        // Use setTimeout to avoid deep recursion
        setTimeout(() => this._processQueue(), 0);
      }
    }
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
