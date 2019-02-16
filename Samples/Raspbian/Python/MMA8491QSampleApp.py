#! /usr/bin/python

import time
from turta_iothat import Turta_MMA8491Q
#Install IoT HAT library with "pip install turta-iothat"

#Initialize
mma8491q = Turta_MMA8491Q.MMA8491QSensor()

try:
    while True:
        #Read & print X, Y and Z-Axis G values in one shot
        xyz = mma8491q.read_xyz_axis()
        print("X-Axis..........: " + str(round(xyz[0], 2)) + "G")
        print("Y-Axis..........: " + str(round(xyz[1], 2)) + "G")
        print("Z-Axis..........: " + str(round(xyz[2], 2)) + "G")

        #Read & print tilt state
        print("Tilt............: " + ("Tilt detected." if mma8491q.read_tilt_state() else "No tilt."))

        #Rest a bit
        print("-----")
        time.sleep(0.5)

#Exit on CTRL+C
except KeyboardInterrupt:
    print('Bye.')