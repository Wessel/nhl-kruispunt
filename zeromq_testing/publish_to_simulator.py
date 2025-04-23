import zmq
import time
import json


def generate_custom_json():
    # Customize this dictionary however you like
    data = {
        "81.1": "rood"
    }
    return json.dumps(data, indent=2)


def main():
    # Create a ZeroMQ context
    context = zmq.Context()

    # Create a PUB (Publisher) socket
    publisher = context.socket(zmq.PUB)

    # Bind the publisher to a test port
    publisher.bind("tcp://localhost:5556")

    # Give subscribers time to connect (important for PUB/SUB)
    time.sleep(1)

    topic = "stoplichten"

    # Generate the latest message
    message = generate_custom_json()

    # Send topic + message
    publisher.send_multipart([topic.encode('utf-8'), message.encode('utf-8')])
    print(f"📩 Message sent on topic '{topic}':\n{message}\n")

    # Clean up (unreachable in this infinite loop unless you add a break/exit)
    publisher.close()
    context.term()
    print("✅ Publisher closed.")


if __name__ == "__main__":
    main()
