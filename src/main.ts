import { ZmqSubscriber } from "./zeromq/subscriber";
import { ZmqPublisher } from "./zeromq/publisher";

const subscriber = new ZmqSubscriber()
  .connect("tcp://192.168.56.243:5557")
  .subscribe('python_test', (topic, message) => {
    console.log(`Received: [${topic}] ${message}`);
  })
  .bind();

const publisher = new ZmqPublisher()
  .bind("tcp://*:5557");

setInterval(async() => {
  const message = `Message ${Math.random()}`;
  console.log(`Publishing: ${message}`);
  publisher.send("python_test", message);
}, 1000);

process.on("SIGINT", async () => {
  publisher.close();
  subscriber.close();
  process.exit();
});
