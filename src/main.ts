import { ZmqSubscriber } from "./subscriber";
import { ZmqPublisher } from "./publisher";

const subscriber = new ZmqSubscriber()
  .connect("tcp://127.0.0.1:5557")
  .subscribe("topic", (topic, message) => {
    console.log(`Received: [${topic}] ${message}`);
  })
  .bind();

const publisher = new ZmqPublisher()
  .bind("tcp://127.0.0.1:5557");

setInterval(async() => {
  const message = `Message ${Math.random()}`;
  console.log(`Publishing: ${message}`);
  publisher.send("topic", message);
}, 1000);

process.on("SIGINT", async () => {
  publisher.close();
  subscriber.close();
  process.exit();
});
