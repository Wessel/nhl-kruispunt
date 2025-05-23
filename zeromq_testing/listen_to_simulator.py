import zmq

# Create a ZeroMQ context
context = zmq.Context()

# Create a SUB (Subscriber) socket
subscriber = context.socket(zmq.SUB)

# Connect to the publisher
unity_address = "tcp://10.121.17.7:5556"
subscriber.connect(unity_address)

# Subscribe to all topics
subscriber.setsockopt_string(zmq.SUBSCRIBE, "sensoren_speciaal")

print(f"🔄 Waiting for messages from {unity_address}...\n")

try:
    while True:
        # Receive topic and message
        topic = subscriber.recv_string()
        message = subscriber.recv_string()
        print(f"📩 Received - Topic: {topic}, Message: {message}")

except KeyboardInterrupt:
    print("\n🛑 Shutting down subscriber...")

finally:
    subscriber.close()
    context.term()
    print("✅ Subscriber closed.")
