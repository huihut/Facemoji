using UnityEngine;

namespace HuiHut.IFlyVoice
{
    public class IFlyVoice
    {
        // Your IFly AppID
        const string AppID = "5a5c6bfb";

        //---------------------------------------
        const string SpeechConstant_PARAMS = "params";
        const string SpeechConstant_ENGINE_TYPE = "engine_type";
        const string SpeechConstant_TYPE_CLOUD = "cloud";
        const string SpeechConstant_VOICE_NAME = "voice_name";
        const string SpeechConstant_SPEED = "speed";
        const string SpeechConstant_PITCH = "pitch";
        const string SpeechConstant_VOLUME = "volume";
        const string SpeechConstant_STREAM_TYPE = "stream_type";
        const string SpeechConstant_KEY_REQUEST_FOCUS = "request_audio_focus";
        const string SpeechConstant_AUDIO_FORMAT = "audio_format";
        const string SpeechConstant_TTS_AUDIO_PATH = "tts_audio_path";
        const string SpeechConstant_RESULT_TYPE = "result_type";
        const string SpeechConstant_LANGUAGE = "language";
        const string SpeechConstant_ACCENT = "accent";
        const string SpeechConstant_VAD_BOS = "vad_bos";
        const string SpeechConstant_VAD_EOS = "vad_eos";
        const string SpeechConstant_ASR_PTT = "asr_ptt";
        const string SpeechConstant_ASR_AUDIO_PATH = "asr_audio_path";
        //---------------------------------------

        //AndroidJavaClass
        private static AndroidJavaClass UnityPlayer;
        private static AndroidJavaObject currentActivity;
        private static AndroidJavaClass SpeechSynthesizer;
        private static AndroidJavaClass SpeechRecognizer;

        //AndroidJavaObject
        private static AndroidJavaObject mTts;
        private static AndroidJavaObject mIat;

        private static XfInitListener mInitListener;
        private static XfSynthesizerListener mTtsListener;
        public static XfRecognizerListener mRecognizerListener;

        //to judge if the program has execute initFlyVoice before speak or recognize
        private static bool inited = false;

        private static void initIFlyVoice()
        {
#if UNITY_ANDROID
            //Initialize AndroidJavaClass(Please do not delete the commended codes for that those code are for test and check)
            UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            currentActivity = UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            string param = "appid=" + AppID + ",engine_mode=msc";

            AndroidJavaClass SpeechUtility = new AndroidJavaClass("com.iflytek.cloud.SpeechUtility");

            SpeechUtility.CallStatic<AndroidJavaObject>("createUtility",
                    currentActivity.Call<AndroidJavaObject>("getApplicationContext"),
                    new AndroidJavaObject("java.lang.String", param)
            );

            //Init Listeners
            mInitListener = new XfInitListener();
            mTtsListener = new XfSynthesizerListener();
            mRecognizerListener = new XfRecognizerListener();

            //Init mTts and mIat
            if (mInitListener != null)
            {
                SpeechSynthesizer = new AndroidJavaClass("com.iflytek.cloud.SpeechSynthesizer");
                SpeechRecognizer = new AndroidJavaClass("com.iflytek.cloud.SpeechRecognizer");

                mTts = SpeechSynthesizer.CallStatic<AndroidJavaObject>("createSynthesizer", currentActivity, mInitListener);
                mIat = SpeechRecognizer.CallStatic<AndroidJavaObject>("createRecognizer", currentActivity, mInitListener);
            }
            inited = true;
#endif
        }

        public static void startSpeaking(string text, string voicer = "xiaoyan")
        {
            if (!inited)
            {
                initIFlyVoice();
            }
            setTtsParam(voicer);
            int code = mTts.Call<int>("startSpeaking", text.toJavaString(), mTtsListener);
            if (code != 0)
            {
                Debug.LogError("SpeakFailed,ErrorCode" + code);
            }
        }

        public static void startRecognize(string language = "mandarin")
        {
            if (!inited)
            {
                initIFlyVoice();
            }
            setIatParam(language);//设置识别参数及语种
            int ret = mIat.Call<int>("startListening", mRecognizerListener);
            if (ret != 0)
            {
                Debug.LogError("听写失败,错误码：" + ret);
            }
            else
            {
                //"Please start talking.".showAsToast(currentActivity);
            }
        }

        private static void setTtsParam(string voicer)
        {
            if (mTts == null)
            {
                Debug.LogError("mTts=null");
                return;
            }
            //清空参数
            mTts.Call<bool>("setParameter", SpeechConstant_PARAMS.toJavaString(), null);

            //设置合成
            //设置使用云端引擎
            mTts.Call<bool>("setParameter", SpeechConstant_ENGINE_TYPE.toJavaString(), SpeechConstant_TYPE_CLOUD.toJavaString());

            //设置发音人
            mTts.Call<bool>("setParameter", SpeechConstant_VOICE_NAME.toJavaString(), voicer.toJavaString());
            //设置合成语速
            mTts.Call<bool>("setParameter", SpeechConstant_SPEED.toJavaString(), "50".toJavaString());
            //设置合成音调
            mTts.Call<bool>("setParameter", SpeechConstant_PITCH.toJavaString(), "50".toJavaString());
            //设置合成音量
            mTts.Call<bool>("setParameter", SpeechConstant_VOLUME.toJavaString(), "50".toJavaString());
            //设置播放器音频流类型
            mTts.Call<bool>("setParameter", SpeechConstant_STREAM_TYPE.toJavaString(), "3".toJavaString());

            // 设置播放合成音频打断音乐播放，默认为true
            mTts.Call<bool>("setParameter", SpeechConstant_KEY_REQUEST_FOCUS.toJavaString(), "true".toJavaString());

            // 设置音频保存路径，保存音频格式支持pcm、wav，设置路径为sd卡请注意WRITE_EXTERNAL_STORAGE权限
            // 注：AUDIO_FORMAT参数语记需要更新版本才能生效
            mTts.Call<bool>("setParameter", SpeechConstant_AUDIO_FORMAT.toJavaString(), "wav".toJavaString());

            AndroidJavaClass Environment = new AndroidJavaClass("android.os.Environment");
            AndroidJavaObject rootDir = Environment.CallStatic<AndroidJavaObject>("getExternalStorageDirectory").Call<AndroidJavaObject>("toString");
            rootDir = rootDir.Call<AndroidJavaObject>("concat", "/msc/tts.wav".toJavaString());

            mTts.Call<bool>("setParameter", SpeechConstant_TTS_AUDIO_PATH.toJavaString(), rootDir);
        }

        private static void setIatParam(string lag)
        {
            // 清空参数
            mIat.Call<bool>("setParameter", SpeechConstant_PARAMS.toJavaString(), null);
            // 设置引擎
            mIat.Call<bool>("setParameter", SpeechConstant_ENGINE_TYPE.toJavaString(), SpeechConstant_TYPE_CLOUD.toJavaString());
            // 设置返回结果格式
            mIat.Call<bool>("setParameter", SpeechConstant_RESULT_TYPE.toJavaString(), "json".toJavaString());

            if (lag.Equals("en_us"))
            {
                // 设置语言
                mIat.Call<bool>("setParameter", SpeechConstant_LANGUAGE.toJavaString(), "en_us".toJavaString());
            }
            else
            {
                // 设置语言
                mIat.Call<bool>("setParameter", SpeechConstant_LANGUAGE.toJavaString(), "zh_cn".toJavaString());
                // 设置语言区域
                mIat.Call<bool>("setParameter", SpeechConstant_ACCENT.toJavaString(), lag.toJavaString());
            }

            // 设置语音前端点:静音超时时间，即用户多长时间不说话则当做超时处理
            mIat.Call<bool>("setParameter", SpeechConstant_VAD_BOS.toJavaString(), "4000".toJavaString());

            // 设置语音后端点:后端点静音检测时间，即用户停止说话多长时间内即认为不再输入， 自动停止录音
            mIat.Call<bool>("setParameter", SpeechConstant_VAD_EOS.toJavaString(), "1000".toJavaString());

            // 设置标点符号,设置为"0"返回结果无标点,设置为"1"返回结果有标点
            mIat.Call<bool>("setParameter", SpeechConstant_ASR_PTT.toJavaString(), "1".toJavaString());

            // 设置音频保存路径，保存音频格式支持pcm、wav，设置路径为sd卡请注意WRITE_EXTERNAL_STORAGE权限
            // 注：AUDIO_FORMAT参数语记需要更新版本才能生效
            mIat.Call<bool>("setParameter", SpeechConstant_AUDIO_FORMAT.toJavaString(), "wav".toJavaString());

            AndroidJavaClass Environment = new AndroidJavaClass("android.os.Environment");
            AndroidJavaObject rootDir = Environment.CallStatic<AndroidJavaObject>("getExternalStorageDirectory").Call<AndroidJavaObject>("toString");
            rootDir = rootDir.Call<AndroidJavaObject>("concat", "/msc/iat.wav".toJavaString());
            mIat.Call<bool>("setParameter", SpeechConstant_ASR_AUDIO_PATH.toJavaString(), rootDir);
        }
    }
}