import tkinter as tk
from tkinter import scrolledtext
import threading

class Gui:
    def __init__(self, start_callback=None):
        self.root = tk.Tk()
        self.messages_text = None
        self.error_text = None
        self.start_callback = start_callback
        self.cnt_entry = None
        self.sim_entry = None
        self.start_btn = None

    def add_message(self, msg):
        self.messages_text.insert(tk.END, msg + "\n")
        self.messages_text.see(tk.END)

    def add_error(self, err):
        self.error_text.insert(tk.END, err + "\n")
        self.error_text.see(tk.END)

    def get_addresses(self):
        cnt_ip = self.cnt_entry.get().strip()
        cnt_port = self.cnt_port_entry.get().strip()
        sim_ip = self.sim_entry.get().strip()
        sim_port = self.sim_port_entry.get().strip()

        if not all([cnt_ip, cnt_port, sim_ip, sim_port]):
            self.add_error("All address fields must be filled.")
            return None, None

        controller_address = f"tcp://{cnt_ip}:{cnt_port}"
        simulator_address = f"tcp://{sim_ip}:{sim_port}"
        return controller_address, simulator_address

    def on_start_pressed(self):
        if self.start_callback:
            cnt, sim = self.get_addresses()
            
            if not cnt or not sim:
                return
            
            # Run MainApp in a thread to keep GUI responsive
            threading.Thread(target=self.start_callback, args=(cnt, sim), daemon=True).start()
            
            self.start_btn.config(state=tk.DISABLED)

    def run_gui(self):
        self.root.title("Regressie tester groep 5")

# left frame
        left_frame = tk.Frame(self.root)
        left_frame.grid(row=0, column=0, padx=10, pady=10, sticky="n")

        tk.Label(left_frame, text="Controller address:").pack(anchor="w")
        self.cnt_entry = tk.Entry(left_frame, width=30)
        self.cnt_entry.insert(0, "10.121.17.XXX")
        self.cnt_entry.pack()
        
        tk.Label(left_frame, text="Controller Port:").pack(anchor="w")
        self.cnt_port_entry = tk.Entry(left_frame, width=30)
        self.cnt_port_entry.insert(0, "5555")
        self.cnt_port_entry.pack()

        tk.Label(left_frame, text="Simulator address:").pack(anchor="w", pady=(10, 0))
        self.sim_entry = tk.Entry(left_frame, width=30)
        self.sim_entry.insert(0, "10.121.17.XXX")
        self.sim_entry.pack()
        
        tk.Label(left_frame, text="Simulator Port:").pack(anchor="w")
        self.sim_port_entry = tk.Entry(left_frame, width=30)
        self.sim_port_entry.insert(0, "5556")
        self.sim_port_entry.pack()

        self.start_btn = tk.Button(left_frame, text="Start", width=10, command=self.on_start_pressed)
        self.start_btn.pack(pady=(10, 0))

# messages frame
        messages_frame = tk.Frame(self.root, bd=2, relief=tk.SOLID)
        messages_frame.grid(row=0, column=1, padx=5, pady=10, sticky="nsew")

        tk.Label(messages_frame, text="Messages").pack(anchor="w")
        self.messages_text = scrolledtext.ScrolledText(messages_frame, width=50, height=40, font=("Consolas", 8))
        self.messages_text.pack()

        error_frame = tk.Frame(self.root, bd=2, relief=tk.SOLID)
        error_frame.grid(row=0, column=2, padx=5, pady=10, sticky="nsew")
        
# error frame
        tk.Label(error_frame, text="Error log").pack(anchor="w")
        self.error_text = scrolledtext.ScrolledText(error_frame, width=100, height=40, fg="red", font=("Consolas", 8))
        self.error_text.pack()

        self.root.grid_columnconfigure(1, weight=1)
        self.root.grid_columnconfigure(2, weight=1)

        self.root.mainloop()