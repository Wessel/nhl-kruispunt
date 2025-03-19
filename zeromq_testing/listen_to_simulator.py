import zmq

# Create a ZeroMQ context
context = zmq.Context()

# Create a SUB (Subscriber) socket
subscriber = context.socket(zmq.SUB)

# Connect to the Unity publisher
unity_address = "tcp://localhost:12345"
subscriber.connect(unity_address)

# Subscribe to all topics
subscriber.setsockopt_string(zmq.SUBSCRIBE, "")

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
