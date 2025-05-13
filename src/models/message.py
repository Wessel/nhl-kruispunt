class Message:
    def __init__(self, topic: str, content: str):
        self.topic = topic
        self.content = content

    def get_topic(self) -> str:
        return self.topic

    def get_content(self) -> str:
        return self.content

    def __str__(self) -> str:
        return f"Message(topic={self.topic}, content={self.content})"