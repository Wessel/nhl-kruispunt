import zmq
from models.message import Message

class Subscriber:
    def __init__(self, context, controller_address, simulator_address):
        self.context = context
        self.subscriber = self.context.socket(zmq.SUB)
        self.subscriber.connect(controller_address)
        self.subscriber.connect(simulator_address)
        self.subscriber.setsockopt_string(zmq.SUBSCRIBE, "")

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