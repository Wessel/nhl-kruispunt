import { Publisher } from "zeromq";

export class ZmqPublisher {
  socket: Publisher;

  constructor() {
    this.socket = new Publisher();

    return this;
  }

  close() {
    this.socket.close();
  }

  bind(address: string): this {
    this.socket.bind(address);

    return this;
  }

  send(topic: string, message: string): this {
    this.socket.send([topic, message]);

    return this;
  }
}
