import zmq
import time


def main():
  # Create a ZeroMQ context
  context = zmq.Context()

  # Create a PUB (Publisher) socket
  publisher = context.socket(zmq.PUB)

  # Bind the publisher to a test port
  publisher.bind("tcp://localhost:23456")

  # Give subscribers time to connect (important for PUB/SUB)
  time.sleep(1)

  # Define a topic and message
  topic = "python_test"
  message = "Hello from Python!"

  # Send the message as two frames (topic + message)
  publisher.send_multipart([topic.encode('utf-8'), message.encode('utf-8')])
  print(f"📩 Message sent on topic '{topic}': {message}")

  # Allow some time for the message to be sent before shutting down
  time.sleep(1)

  # Clean up
  publisher.close()
  context.term()
  print("✅ Publisher closed.")


if __name__ == "__main__":
  main()
