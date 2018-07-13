import time
import Turta_RelayController

#Initialize
rc = Turta_RelayController.RelayController()

try:
    while 1:
        #Toggle relay 1
        rc.set_relay(1, not rc.read_relay_state(1))
        print("Relay 1 state: " + ("On" if rc.read_relay_state(1) else "Off"))
        time.sleep(5.0)

        #Toggle relay 2
        rc.set_relay(2, not rc.read_relay_state(2))
        print("Relay 2 state: " + ("On" if rc.read_relay_state(2) else "Off"))
        time.sleep(5.0)

except KeyboardInterrupt:
    print('Bye.')