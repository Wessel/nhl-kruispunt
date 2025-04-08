import zmq
from topics import stoplichten, voorrangsvoertuig, tijd, sensoren_rijbaan, sensoren_speciaal, sensoren_bruggen

# Create a ZeroMQ context
context = zmq.Context()

# Create a SUB (Subscriber) socket
subscriber = context.socket(zmq.SUB)

# Connect to the publisher
controller_address = "tcp://10.121.17.187:5557"
simulator_address = "tcp://10.121.17.187:5557"
subscriber.connect(controller_address)
subscriber.connect(simulator_address)

# Subscribe to all topics
subscriber.setsockopt_string(zmq.SUBSCRIBE, "")

allTopics = ["sensoren_rijbaan", "sensoren_speciaal", "sensoren_bruggen", "voorrangsvoertuig", "tijd", "stoplichten"]

topics = {
    'tijd': tijd.handle_tijd,
    'sensoren rijbaan': sensoren_rijbaan.handle_rijbaan,
    'sensoren_speciaal': sensoren_speciaal.handle_speciaal,
    'sensoren_bruggen': sensoren_bruggen.handle_bruggen,
    'voorrangsvoertuig': voorrangsvoertuig.handle_voorangsvoertuig,
    'stoplichten': stoplichten.handle_stoplichten,
}

print(f"🔄 Waiting for messages from {controller_address}...\n")

try:
    while True:
        # Receive topic and message
        topic = subscriber.recv_string()
        message = subscriber.recv_string()
        
        topics[topic](message) if topic in topics else print(f"Unknown topic: {topic}")
        
        
        # print(f"📩 Received - Topic: {topic}, Message: {message}")

except KeyboardInterrupt:
    print("\n🛑 Shutting down subscriber...")

finally:
    subscriber.close()
    context.term()
    print("✅ Subscriber closed.")