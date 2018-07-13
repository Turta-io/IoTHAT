#! /usr/bin/python

import time
import Turta_VEML6075

#Initialize
veml6075 = Turta_VEML6075.VEML6075Sensor()

#Wait 1 second for the first sensor readings.
time.sleep(1.0)

try:
    while True:
        #Read & print UV Index
        print("UV Index........: " + str(round(veml6075.calculate_average_uv_index(), 4)))

        #Read & print UV Index A
        print("UV Index A......: " + str(round(veml6075.calculate_uv_index_a(), 4)))

        #Read & print UV Index B
        print("UV Index B......: " + str(round(veml6075.calculate_uv_index_b(), 4)))

        #Rest a bit
        print("-----")
        time.sleep(10.0)

#Exit on CTRL+C
except KeyboardInterrupt:
    print('Bye.')