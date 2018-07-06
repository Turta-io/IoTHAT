#! /usr/bin/python

import time
import MMA8491QSensor

#Initialize
MMA8491QSensor.Init()

try:
    while True:
        #Read & print X, Y and Z-Axis G values in one shot
        xyz = MMA8491QSensor.ReadXYZAxis()
        print "X-Axis..........: " + str(round(xyz[0], 2)) + "G"
        print "Y-Axis..........: " + str(round(xyz[1], 2)) + "G"
        print "Z-Axis..........: " + str(round(xyz[2], 2)) + "G"

        #Read & print tilt state
        print "Tilt............: " + ("Tilt detected." if MMA8491QSensor.ReadTiltState() else "No tilt.")

        #Rest a bit
        print "-----"
        time.sleep(0.5)

#Exit on CTRL+C
except KeyboardInterrupt:
    #Release the resources
    MMA8491QSensor.Dispose()
