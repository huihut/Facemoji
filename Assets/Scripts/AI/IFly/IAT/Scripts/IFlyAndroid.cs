using UnityEngine;
using System.Collections;

namespace Wangz.IFly
{
    public class IFlyAndroid : IFlyBase
    {
        private AndroidJavaClass m_speech;

        public override void Init()
        {
            // Android App id
            m_appid = "5a5c6bfb";

            m_speech = new AndroidJavaClass("com.puhanda.sdk.ifly.SpeechUtility");
            m_speech.CallStatic("Init", m_appid, name);
            // 清空参数
            SetParameter("params", null);
            // 设置听写引擎
            SetParameter("engine_type", "cloud");
            // 设置返回结果格式
            SetParameter("result_type", "plain");
            // 设置英文语言
            SetParameter("language", "en_us");
            // 设置语言区域
            SetParameter("accent", null);
            // 设置语音前端点:静音超时时间，即用户多长时间不说话则当做超时处理
            SetParameter("vad_bos", "4000");
            // 设置语音后端点:后端点静音检测时间，即用户停止说话多长时间内即认为不再输入， 自动停止录音
            SetParameter("vad_eos", "1000");
            // 设置标点符号,设置为"0"返回结果无标点,设置为"1"返回结果有标点
            SetParameter("asr_ptt", "0");
            // 设置音频保存路径，保存音频格式支持pcm、wav，设置路径为sd卡请注意WRITE_EXTERNAL_STORAGE权限
            // 注：AUDIO_FORMAT参数语记需要更新版本才能生效
            SetParameter("audio_format", "wav");
            SetParameter("asr_audio_path", RecordSavePath);
        }

        public void SetParameter(string key, string value)
        {
            m_speech.CallStatic("SetParameter", key, value);
        }


        public override void StartSpeech()
        {
            m_speech.CallStatic<int>("Start");
        }

        public override bool isListening()
        {
            return m_speech.CallStatic<bool>("isListening");
        }

        public override void StopSpeech()
        {
            m_speech.CallStatic<int>("Stop");
        }

        public override void CancelSpeech()
        {
            m_speech.CallStatic<int>("Cancel");
        }
    }
}
