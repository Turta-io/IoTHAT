import time
import RelayController

#Initialize
RelayController.Init()

try:
    while 1:
        #Toggle relay 1
        RelayController.SetRelay(1, not RelayController.ReadRelayState(1))
        time.sleep(5.0)

        #Turn relay 2 on
        RelayController.SetRelay(2, True)
        time.sleep(5.0)

        #Turn relay 2 off
        RelayController.SetRelay(2, False)
        time.sleep(5.0)

except KeyboardInterrupt:
    RelayController.Dispose()
