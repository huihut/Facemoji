using UnityEngine;
using System;
using System.Runtime.InteropServices;

namespace Wangz.IFly
{
    public class IFlyUtils
    {
        public static byte[] ConvertClipToBytes(AudioClip clip)
        {
            float[] samples = new float[clip.samples];

            clip.GetData(samples, 0);

            short[] intData = new short[samples.Length];
            //converting in 2 float[] steps to Int16[], //then Int16[] to Byte[]

            byte[] bytesData = new byte[samples.Length * 2];
            //bytesData array is twice the size of
            //dataSource array because a float converted in Int16 is 2 bytes.

            int rescaleFactor = 32767; //to convert float to Int16

            for (int i = 0; i < samples.Length; i++)
            {
                intData[i] = (short)(samples[i] * rescaleFactor);
                byte[] byteArr = new byte[2];
                byteArr = BitConverter.GetBytes(intData[i]);
                byteArr.CopyTo(bytesData, i * 2);
            }

            return bytesData;
        }

        public static IntPtr BytesToIntptr(byte[] bytes)
        {
            int size = bytes.Length;
            IntPtr buffer = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.Copy(bytes, 0, buffer, size);
                return buffer;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
    }
}
