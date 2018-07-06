#! /usr/bin/python

import time
import VEML6075Sensor

#Initialize
VEML6075Sensor.Init()

try:
    while True:
        #Read & print UV Index
        print "UV Index........: " + str(round(VEML6075Sensor.Calculate_Average_UV_Index(), 4))

        #Read & print UV Index A
        print "UV Index A......: " + str(round(VEML6075Sensor.Calculate_UV_Index_A(), 4))

        #Read & print UV Index B
        print "UV Index B......: " + str(round(VEML6075Sensor.Calculate_UV_Index_B(), 4))

        #Rest a bit
        print "-----"
        time.sleep(10.0)

#Exit on CTRL+C
except KeyboardInterrupt:
    #Release the resources
    VEML6075Sensor.Dispose()
