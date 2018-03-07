using UnityEngine;
using System;
using HuiHut.Facemoji;

namespace HuiHut.IFlyVoice
{
    public class XfRecognizerListener : AndroidJavaProxy
    {
        public string resultString = string.Empty;

        public XfRecognizerListener() : base("com.iflytek.cloud.RecognizerListener")
        {
        }

        public void onVolumeChanged(int volume, byte[] data)
        {
            string showText = "The current volume of speech is: " + volume;
            showText.showAsToast();
            Debug.Log("返回音频数据：" + data.Length);
        }

        // 一次识别会话的结果可能会多次返回（即多次回调此函数），通过参数2判断是否是最后一个结果（isLast==true）
        // 当最后一个结果返回时，本次会话结束，录音也会停止。
        public void onResult(AndroidJavaObject result, bool isLast)
        {
            string text = string.Empty;
            if (null != result)
            {
                try
                {
                    AndroidJavaObject res = result.Call<AndroidJavaObject>("getResultString");
                    byte[] resultByte = res.Call<byte[]>("getBytes");
                    text = System.Text.Encoding.Default.GetString(resultByte);
                }
                catch (Exception error)
                {
                    Debug.LogError(error.ToString());
                }
            }
            resultString = resultString + text;
            if (isLast)
            {
                //TODO 最后的结果

                // 解析Json数据
                string userMessage = ParsingIFlyJson.Parsing(resultString);

                // 发送语言识别的话给机器人
                FacemojiAI.SendToRobot(userMessage);

                Debug.Log(resultString);
            }
        }

        public void onEndOfSpeech()
        {
            // 此回调表示：检测到了语音的尾端点，已经进入识别过程，不再接受语音输入                
            //"End the talk.".showAsToast();
        }

        public void onBeginOfSpeech()
        {
            // 此回调表示：sdk内部录音机已经准备好了，用户可以开始语音输入
            resultString = string.Empty;
        }

        public void onError(AndroidJavaObject error)
        {
            // 错误代码，参照：https://shimo.im/sheet/w3yUy39uNKs0J7DT
            int errorCode = error.Call<int>("getErrorCode");
            // 错误文本
            string errorText = "onError Code：" + errorCode;

            if (errorCode == 10118)
            {
                //error code=10118 代表您没有说话，请查看APP是否被安全软件禁用了录音功能
                "please speak!".showAsToast();
            }
            else
            {
                // 其他错误则输出错误文本
                errorText.showAsToast();
            }
        }

        public void onEvent(int eventType, int arg1, int arg2, AndroidJavaObject BundleObj)
        {
            ////以下代码用于获取与云端的会话id，当业务出错时将会话id提供给讯飞云的技术支持人员，可用于查询会话日志，定位出错原因
            //if (SpeechEvent.EVENT_SESSION_ID == eventType)
            //{
            //    String sid = obj.getString(SpeechEvent.KEY_EVENT_SESSION_ID);
            //    Log.d(TAG, "session id =" + sid);
            //}
        }
    }
}
