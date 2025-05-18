from zmq import Context
from subscriber import Subscriber
from handlers.tijd_handler import TijdHandler
from handlers.stoplichten_handler import StoplichtenHandler
from handlers.voorrangsvoertuig_handler import VoorrangsvoertuigHandler
from handlers.sensoren_rijbaan_handler import SensorenRijbaanHandler
from handlers.sensoren_speciaal_handler import SensorenSpeciaalHandler
from handlers.sensoren_bruggen_handler import SensorenBruggenHandler
from datetime import datetime
from gui.gui import Gui

class MainApp:
    def __init__(self, controller_address, simulator_address, gui=None):
        self.controller_address = controller_address
        self.simulator_address = simulator_address
        self.gui = gui

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
        if self.gui:
            self.gui.add_message("🔄 Starting subscriber...")

        try:
            while True:
                message = self.subscriber.receive_message()
                if message is None:
                    continue

                if self.gui:
                    self.gui.add_message(f"📩 Received - Topic: {message.get_topic()}")

                try:
                    handler = self.handlers.get(message.get_topic())
                    if handler:
                        handler.handle(message)
                    else:
                        raise ValueError(f"Unknown topic: {message.get_topic()}")
                except Exception as e:
                    error_msg = f"Error on topic {message.get_topic()}: {e}"
                    if self.gui:
                        self.gui.add_error(error_msg)
                    with open("error.log", "a") as log_file:
                        log_file.write(f"{datetime.now()} - {error_msg}\n")

        except KeyboardInterrupt:
            if self.gui:
                self.gui.add_message("🛑 KeyboardInterrupt received. Shutting down...")
        finally:
            self.subscriber.close()
            self.context.term()
            if self.gui:
                self.gui.add_message("✅ Subscriber closed.")
                
if __name__ == "__main__":

    def start_app(controller_address, simulator_address):
        app = MainApp(controller_address, simulator_address, gui_instance)
        app.run()

    gui_instance = Gui(start_callback=start_app)
    gui_instance.run_gui()