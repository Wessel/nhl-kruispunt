import { Subscriber } from "zeromq";

export class ZmqSubscriber {
  private _socket: Subscriber;
  private _topics: [string, (topic: string, message: string) => void][] = [];

  constructor() {
    this._socket = new Subscriber();

    return this;
  }

  close() {
    this._socket.close();
  }

  connect(address: string): this {
    this._socket.connect(address);

    return this;
  }

  subscribe(topic: string, func: (topic: string, message: string) => void): this {
    this._socket.subscribe(topic);

    this._topics.push([topic, func]);

    return this;
  }

  bind() {
    void (async () => {
      for await (const [topic, message] of this._socket) {
        const topicStr = topic.toString();
        const messageStr = message.toString();

        const topicFunc = this._topics.find(([topic]) => topic === topicStr || topic === "");

        if (topicFunc) {
          topicFunc[1](topicStr, messageStr);
        }
      }
    })();

    return this;
  }
}
