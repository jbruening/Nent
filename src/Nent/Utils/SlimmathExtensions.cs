using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimMath;

namespace Nent
{
    /// <summary>
    /// Extensions for slimmath
    /// </summary>
    public static class SlimMathExtensions
    {
        /// <summary>
        /// Rotate the vector by the quaternion.
        /// </summary>
        /// <remarks>Ogre and Unity use this as well, from the nvidia sdk
        ///  Fuck if I know how this magic works</remarks>
        /// <param name="quaternion"></param>
        /// <param name="vector3"></param>
        /// <returns></returns>
        public static Vector3 Multiply(this Quaternion quaternion, Vector3 vector3)
        {
            Vector3 uv, uuv;
            Vector3 qvec = new Vector3(quaternion.X, quaternion.Y, quaternion.Z);
            Vector3.Cross(ref qvec, ref vector3, out uv);
            Vector3.Cross(ref qvec, ref uv, out uuv);
            uv *= (2.0f * quaternion.W);
            uuv *= 2.0f;

            return vector3 + uv + uuv;

        }

        /// <summary>
        /// Get the 3 bytes from the Color3
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static byte[] ColorBytes(this Color3 color)
        {
            var intColor = color.ToRgb();

            byte[] bytes = new byte[3];
            unchecked
            {
                bytes[0] = (byte)(intColor >> 16);
                bytes[1] = (byte)(intColor >> 8);
                bytes[2] = (byte)(intColor);
            }

            return bytes;
        }

        /// <summary>
        /// Get the 4 bytes from the Color4
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static byte[] ColorBytes(this Color4 color)
        {
            var intColor = color.ToArgb();

            byte[] bytes = new byte[4];
            unchecked
            {
                bytes[0] = (byte)(intColor >> 24);
                bytes[1] = (byte)(intColor >> 16);
                bytes[2] = (byte)(intColor >> 8);
                bytes[3] = (byte)(intColor);
            }

            return bytes;
        }
    }
}
