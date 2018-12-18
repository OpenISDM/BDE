using System;

namespace BDE
{
    public static class Utilities
    {
        /// <summary>
        /// Converts feet to Meters
        /// </summary>
        /// <param name="ftValue"></param>
        /// <returns></returns>
        public static double feetToMeters(double ftValue)
        {
            return ftValue * Constants._feetToMeters;
        }

        /// <summary>
        /// Converts Meters to degrees
        /// 1.11 meters approximately equal to decimal degrees 0.00001 
        /// 0.111 meters approximately equal to decimal degrees 0.000001
        /// </summary>
        /// <param name="ftValue"></param>
        /// <returns></returns>
        public static double MeterToDecimalDegress(double meter)
        {
            return meter * Constants._fifthDecimalPlaces;
        }
    }

    public static class Converter
    {
        public static string ToUUID(LBeacon beacon)
        {
            byte[] lonBytes = BitConverter.GetBytes(beacon.XLocation);
            byte[] latBytes = BitConverter.GetBytes(beacon.YLocation);
            byte[] floorByte = BitConverter.GetBytes(beacon.ZLocation);

            string[] buffer = new string[4];

            for (int i = 0; i < 2; i++)
            {
                string lonHex = Convert.ToString(lonBytes[i * 2], 16).PadLeft(2, '0') + Convert.ToString(lonBytes[i * 2 + 1], 16).PadLeft(2, '0');
                string latHex = Convert.ToString(latBytes[i * 2], 16).PadLeft(2, '0') + Convert.ToString(latBytes[i * 2 + 1], 16).PadLeft(2, '0');
                buffer[i + 2] = lonHex;
                buffer[i] = latHex;
            }

            string floorHex = Convert.ToString(floorByte[0], 16).PadLeft(2, '0') +
                Convert.ToString(floorByte[1], 16).PadLeft(2, '0') +
                Convert.ToString(floorByte[2], 16).PadLeft(2, '0') +
                Convert.ToString(floorByte[3], 16).PadLeft(2, '0');

            return floorHex + "-0000-" + buffer[0] + "-" + buffer[1] + "-0000" + buffer[2] + buffer[3];
        }
    }

}
