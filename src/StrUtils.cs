using System;
using System.Linq;
using System.Text;

namespace Ristretto
{
    public static class StrUtils
    {
        /// <summary>
        /// Converts bytes to a hex string.
        /// </summary>
        /// <param name="raw">the byte[] to be converted.</param>
        /// <returns>the hex representation as a string.</returns>
        public static string bytesToHex(byte[] raw)
        {
            if (raw == null)
            {
                return null;
            }
            StringBuilder hex = new StringBuilder(2 * raw.Length);
            foreach (byte b in raw)
            {
                hex.AppendFormat("{0:x2}", b);
            }
            return hex.ToString();
        }

        public static byte[] hexToBytes(string s)
        {
            // Strip any internal whitespace
            var hexastring = s.Replace(" ", "");

            return Enumerable.Range(0, hexastring.Length)
                     .Where(x => x % 2 == 0)
                     .Select(x => Convert.ToByte(hexastring.Substring(x, 2), 16))
                     .ToArray();
        }

        public static sbyte[] hexToSBytes(string s)
        {
            // Strip any internal whitespace
            var hexastring = s.Replace(" ", "");

            return Enumerable.Range(0, hexastring.Length)
                     .Where(x => x % 2 == 0)
                     .Select(x => Convert.ToSByte(hexastring.Substring(x, 2), 16))
                     .ToArray();
        }
    }
}
