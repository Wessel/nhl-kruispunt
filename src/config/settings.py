class Settings:
    def __init__(self):
        self.simulator_address = "tcp://10.121.17.8:5556"
        self.controller_address = "tcp://10.121.17.8:5555"

    def get_controller_address(self):
        return self.controller_address

    def get_simulator_address(self):
        return self.simulator_address