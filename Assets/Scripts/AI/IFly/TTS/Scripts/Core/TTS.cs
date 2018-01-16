using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;

namespace IFLYSpeech.Interanl
{
    public class TTS
    {
        private const string HENRY = "henry";
        private string sessionID;
        private string config;
        public event TTS_SpeakFinished tts_SpeakFinishedEvent;
        public event TTS_SpeakError ttsSpeakErrorEvent;
        public bool active;

        public TTS(string config)
        {
            this.config = config;
            int ret = MSPAPI.MSPLogin(null, null, config);
            if (ret != 0)
            {
                if (ttsSpeakErrorEvent != null) ttsSpeakErrorEvent.Invoke("登录TTS引擎错误，错误代码：" + ret);
            }
            else
            {
                active = true;
            }
        }
        ~TTS()
        {
            var ret = MSPAPI.MSPLogout();
            if (ret != 0)
            {
                if (ttsSpeakErrorEvent != null) ttsSpeakErrorEvent.Invoke("退出TTS引擎错误，错误代码：" + ret);
            }
        }
        /// <summary>
        /// wave文件头
        /// </summary>
        private struct WAVE_Header
        {
            public int RIFF_ID;           //4 byte , 'RIFF'
            public int File_Size;         //4 byte , 文件长度
            public int RIFF_Type;         //4 byte , 'WAVE'

            public int FMT_ID;            //4 byte , 'fmt'
            public int FMT_Size;          //4 byte , 数值为16或18，18则最后又附加信息
            public short FMT_Tag;          //2 byte , 编码方式，一般为0x0001
            public ushort FMT_Channel;     //2 byte , 声道数目，1--单声道；2--双声道
            public int FMT_SamplesPerSec;//4 byte , 采样频率
            public int AvgBytesPerSec;   //4 byte , 每秒所需字节数,记录每秒的数据量
            public ushort BlockAlign;      //2 byte , 数据块对齐单位(每个采样需要的字节数)
            public ushort BitsPerSample;   //2 byte , 每个采样需要的bit数

            public int DATA_ID;           //4 byte , 'data'
            public int DATA_Size;         //4 byte , 
        }

        /// <summary>
        /// 根据数据段的长度，生产文件头
        /// </summary>
        /// <param name="data_len">音频数据长度</param>
        /// <returns>返回wav文件头结构体</returns>
        WAVE_Header getWave_Header(int data_len)
        {
            WAVE_Header wav_Header = new WAVE_Header();
            wav_Header.RIFF_ID = 0x46464952;        //字符RIFF
            wav_Header.File_Size = data_len + 36;
            wav_Header.RIFF_Type = 0x45564157;      //字符WAVE

            wav_Header.FMT_ID = 0x20746D66;         //字符fmt
            wav_Header.FMT_Size = 16;
            wav_Header.FMT_Tag = 0x0001;
            wav_Header.FMT_Channel = 1;             //单声道
            wav_Header.FMT_SamplesPerSec = 16000;   //采样频率
            wav_Header.AvgBytesPerSec = 32000;      //每秒所需字节数
            wav_Header.BlockAlign = 2;              //每个采样1个字节
            wav_Header.BitsPerSample = 16;           //每个采样8bit

            wav_Header.DATA_ID = 0x61746164;        //字符data
            wav_Header.DATA_Size = data_len;

            return wav_Header;
        }

        /// <summary>
        /// 把结构体转化为字节序列
        /// </summary>
        /// <param name="structure">被转化的结构体</param>
        /// <returns>返回字节序列</returns>
        Byte[] StructToBytes(Object structure)
        {
            Int32 size = Marshal.SizeOf(structure);
            IntPtr buffer = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(structure, buffer, false);
                Byte[] bytes = new Byte[size];
                Marshal.Copy(buffer, bytes, 0, size);
                return bytes;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        /// <summary>
        /// 把文字转化为声音,单路配置，一种语音
        /// </summary>
        /// <param name="speekText">要转化成语音的文字</param>
        /// <param name="outWaveFlie">把声音转为文件，默认为不生产wave文件</param>
        public void Speak(string speekText, string szParams, string outWaveFlie)
        {
            byte[] bytes = null;
            int ret = 0;
            try
            {
                sessionID = Marshal.PtrToStringAuto(MSPAPI.QTTSSessionBegin(szParams, ref ret));
                if (ret != 0)
                {
                    if (ttsSpeakErrorEvent != null) ttsSpeakErrorEvent.Invoke("初始化TTS引会话错误，错误代码：" + ret);
                    return;
                }
                ret = MSPAPI.QTTSTextPut(sessionID, speekText, (uint)Encoding.Unicode.GetByteCount(speekText), string.Empty);
                if (ret != 0)
                {
                    if (ttsSpeakErrorEvent != null) ttsSpeakErrorEvent.Invoke("向服务器发送数据，错误代码：" + ret);
                    return;
                }
                IntPtr audio_data;
                int audio_len = 0;
                SynthStatus synth_status = SynthStatus.MSP_TTS_FLAG_STILL_HAVE_DATA;
                using (MemoryStream ms = new MemoryStream())
                {
                    ms.Write(new byte[44], 0, 44);
                    //写44字节的空文件头
                    while (synth_status == SynthStatus.MSP_TTS_FLAG_STILL_HAVE_DATA)
                    {
                        audio_data = MSPAPI.QTTSAudioGet(sessionID, ref audio_len, ref synth_status, ref ret);
                        if (audio_data != IntPtr.Zero)
                        {
                            byte[] data = new byte[audio_len];
                            Marshal.Copy(audio_data, data, 0, audio_len);
                            ms.Write(data, 0, data.Length);
                            if (synth_status == SynthStatus.MSP_TTS_FLAG_DATA_END || ret != 0)
                            {
                                if (ret != 0)
                                {
                                    if (ttsSpeakErrorEvent != null) ttsSpeakErrorEvent.Invoke("下载TTS文件错误，错误代码：" + ret);
                                    return;
                                }
                                break;
                            }
                        }
                        Thread.Sleep(150);
                    }
                    System.Diagnostics.Debug.WriteLine("wav header");
                    WAVE_Header header = getWave_Header((int)ms.Length - 44);     //创建wav文件头
                    byte[] headerByte = StructToBytes(header);                         //把文件头结构转化为字节数组                      //写入文件头
                    ms.Position = 0;                                                        //定位到文件头
                    ms.Write(headerByte, 0, headerByte.Length);                             //写入文件头
                    bytes = ms.ToArray();
                    ms.Close();
                }

                if (outWaveFlie != null)
                {
                    if (File.Exists(outWaveFlie))
                    {
                        File.Delete(outWaveFlie);
                    }
                    File.WriteAllBytes(outWaveFlie, bytes);
                }
            }
            catch (Exception ex)
            {
                if (ttsSpeakErrorEvent != null) ttsSpeakErrorEvent.Invoke("Error：" + ex.Message);
                return;
            }
            finally
            {
                ret = MSPAPI.QTTSSessionEnd(sessionID, "");
                if (ret != 0)
                {
                    if (ttsSpeakErrorEvent != null) ttsSpeakErrorEvent.Invoke("结束TTS会话错误，错误代码：" + ret);
                }
                else
                {
                    if (tts_SpeakFinishedEvent != null) tts_SpeakFinishedEvent.Invoke(speekText, bytes);
                }
            }
        }
    }
}
