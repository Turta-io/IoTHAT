#! /usr/bin/python

import time
from turta_iothat import Turta_APDS9960
#Install IoT HAT library with "pip install turta-iothat"

#Initialize
apds9960 = Turta_APDS9960.APDS9960Sensor()

try:
    while True:
        #Read & print ambient light
        print("Ambient Light...: " + str(apds9960.read_ambient_light()))

        #Read & print color values
        rgb = apds9960.read_rgb_light()
        print("Red.............: " + str(rgb[0]))
        print("Green...........: " + str(rgb[1]))
        print("Blue............: " + str(rgb[2]))

        #Read & print proximity
        print("Proximity.......: " + str(apds9960.read_proximity()))

        #Rest a bit
        print("-----")
        time.sleep(0.5)

#Exit on CTRL+C
except KeyboardInterrupt:
    print('Bye.')