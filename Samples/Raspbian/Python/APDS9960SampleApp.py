#! /usr/bin/python

import time
import APDS9960Sensor

#Initialize
APDS9960Sensor.Init()

try:
    while True:
        #Read & print ambient light
        print "Ambient Light...: " + str(APDS9960Sensor.ReadAmbientLight())

        #Read & print color values
        rgb = APDS9960Sensor.ReadRGBLight()
        print "Red.............: " + str(rgb[0])
        print "Green...........: " + str(rgb[1])
        print "Blue............: " + str(rgb[2])

        #Read & print proximity
        print "Proximity.......: " + str(APDS9960Sensor.ReadProximity())

        #Rest a bit
        print "-----"
        time.sleep(0.5)

#Exit on CTRL+C
except KeyboardInterrupt:
    #Release the resources
    APDS9960Sensor.Dispose()
