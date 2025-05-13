from zmq import Context
from subscriber import Subscriber
from handlers.tijd_handler import TijdHandler
from handlers.stoplichten_handler import StoplichtenHandler
from handlers.voorrangsvoertuig_handler import VoorrangsvoertuigHandler
from handlers.sensoren_rijbaan_handler import SensorenRijbaanHandler
from handlers.sensoren_speciaal_handler import SensorenSpeciaalHandler
from handlers.sensoren_bruggen_handler import SensorenBruggenHandler
from config.settings import Settings
from datetime import datetime

class MainApp:
    def __init__(self):
        settings = Settings()
        self.controller_address = Settings.get_controller_address(settings)
        self.simulator_address = Settings.get_simulator_address(settings)
        
        self.context = Context()
        self.subscriber = Subscriber(self.context, self.controller_address, self.simulator_address)
        self.handlers = {
            'tijd': TijdHandler(),
            'stoplichten': StoplichtenHandler(),
            'voorrangsvoertuig': VoorrangsvoertuigHandler(),
            'sensoren_rijbaan': SensorenRijbaanHandler(),
            'sensoren_speciaal': SensorenSpeciaalHandler(),
            'sensoren_bruggen': SensorenBruggenHandler(),
        }

    def run(self):
        print(f"🔄 Waiting for messages...\n")
        try:
            while True:  
                message = self.subscriber.receive_message()
                if message is None:
                    continue
                
                print(f"📩 Received - Topic: {message.topic}")
                
                try:
                    handler = self.handlers.get(message.topic)
                    if handler:
                        handler.handle(message)
                    else:
                        raise ValueError(f"Unknown topic: {message.topic}")
                except Exception as e:
                        print(f"\033[31mError: {e}\033[0m")
                        with open("error.log", "a") as log_file:
                            log_file.write(f"{datetime.now()} - ")
                            log_file.write(f"Error: {e}\n")
        except KeyboardInterrupt:
            print("\n🛑 Shutting down subscriber...")
        finally:
            self.subscriber.close()
            self.context.term()
            print("✅ Subscriber closed.")

if __name__ == "__main__":
    app = MainApp()
    app.run()