import { Subscriber } from "zeromq";

export class ZmqSubscriber {
  socket: Subscriber;
  topics: [string, (topic: string, message: string) => void][] = [];

  constructor() {
    this.socket = new Subscriber();

    return this;
  }

  close() {
    this.socket.close();
  }

  connect(address: string): this {
    this.socket.connect(address);

    return this;
  }

  subscribe(topic: string, func: (topic: string, message: string) => void): this {
    this.socket.subscribe(topic);

    this.topics.push([topic, func]);

    return this;
  }

  bind() {
    void (async () => {
      for await (const [topic, message] of this.socket) {
        const topicStr = topic.toString();
        const messageStr = message.toString();

        const topicFunc = this.topics.find(([topic]) => topic === topicStr);

        if (topicFunc) {
          topicFunc[1](topicStr, messageStr);
        }
      }
    })();

    return this;
  }
}
