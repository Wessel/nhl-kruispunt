import zmq
from topics import stoplichten, voorrangsvoertuig, tijd, sensoren_rijbaan, sensoren_speciaal, sensoren_bruggen

# Create a ZeroMQ context
context = zmq.Context()

# Create a SUB (Subscriber) socket
subscriber = context.socket(zmq.SUB)

# Connect to the publishers
controller_address = "tcp://10.121.17.145:3001"
simulator_address = "tcp://10.121.17.181:5555"
subscriber.connect(controller_address)
subscriber.connect(simulator_address)

# Subscribe to all topics
# subscriber.setsockopt(zmq.RCVTIMEO, 1000)
subscriber.setsockopt_string(zmq.SUBSCRIBE, "")

allTopics = ["sensoren_rijbaan", "sensoren_speciaal", "sensoren_bruggen", "voorrangsvoertuig", "tijd", "stoplichten"]

topics = {
    'tijd': tijd.handle_tijd,
    'sensoren_rijbaan': sensoren_rijbaan.handle_rijbaan,
    'sensoren_speciaal': sensoren_speciaal.handle_speciaal,
    'sensoren_bruggen': sensoren_bruggen.handle_bruggen,
    'voorrangsvoertuig': voorrangsvoertuig.handle_voorrangsvoertuig,
    'stoplichten': stoplichten.handle_stoplichten,
}

print(f"🔄 Waiting for messages from {simulator_address} and {controller_address}...\n")

try:
    while True:
        try:
            topic = subscriber.recv_string()
            message = subscriber.recv_string()
            
            # print(f"📩 Received - Topic: {topic} - Message: {message}")
            print(f"📩 Received - Topic: {topic}")
            
            
            topics[topic](message) if topic in topics else print(f"Unknown topic: {topic}")
        except zmq.Again:
            continue

except KeyboardInterrupt:
    print("\n🛑 Shutting down subscriber...")

finally:
    subscriber.close()
    context.term()
    print("✅ Subscriber closed.")