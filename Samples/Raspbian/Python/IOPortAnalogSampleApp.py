import time
import Turta_IOPort

#Initialize
io = Turta_IOPort.IOPort(True, True, True, True)

try:
    while 1:
        #Read analog input 1
        print(io.read_analog(1))
        
        #Delay 5 seconds
        time.sleep(5.0)

#Exit on CTRL+C
except KeyboardInterrupt:
    print('Bye.')
