import time
import Turta_IOPort

#Initialize
#IO Ports 1 and 2 are set to be inputs
#IO Ports 3 and 4 are set to be outputs
io = Turta_IOPort.IOPort(True, True, False, False)

try:
    while 1:
        #Toggle IO pin 3
        io.set_digital(3, not io.read_digital(3))

        #Print IO pin 3's state
        print(io.read_digital(3))

        #Delay 5 seconds
        time.sleep(5.0)

#Exit on CTRL+C
except KeyboardInterrupt:
    print('Bye.')
