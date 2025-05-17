import zmq
from models.message import Message

class Subscriber:
    def __init__(self, context, controller_address, simulator_address):
        
        try:
            self.context = context
            self.subscriber = self.context.socket(zmq.SUB)
            self.subscriber.connect(controller_address)
            self.subscriber.connect(simulator_address)
            self.subscriber.setsockopt_string(zmq.SUBSCRIBE, "")
        except zmq.ZMQError as e:
            error = f"❌ Error connecting to addresses: {e}"
            print(error)
            raise ValueError(error)

    def receive_message(self):
        
        try:
            topic = self.subscriber.recv_string()
            message = self.subscriber.recv_string()
        except zmq.Again:
            return None
        return Message(topic, message)

    def close(self):
        self.subscriber.close()
        self.context.term()
        print("✅ Subscriber closed.")