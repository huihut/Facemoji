using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;

namespace Wangz.IFly
{
    public class IFlyWin : IFlyBase
    {
        private bool m_initState;
        private AudioClip m_audioClip;
        private bool m_isListening;
        private IntPtr m_sessionId;

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (m_initState)
            {
                int ret = MSCDLL.MSPLogout();
                Debug.Log("MSPLogout : " + ret);
            }
        }

        public override void Init()
        {
            int errorCode = (int)Errors.MSP_SUCCESS;

            // Windows App id
            m_appid = "5a5b0136";

            /* 用户登录 */
            errorCode = MSCDLL.MSPLogin(null, null, InitInfo); //第一个参数是用户名，第二个参数是密码，均传NULL即可，第三个参数是登录参数	
            if ((int)Errors.MSP_SUCCESS != errorCode)
            {
                OnError(string.Format("MSPLogin failed , Error code {0}", errorCode));
                MSCDLL.MSPLogout(); //退出登录
                m_initState = false;
                return;
            }

            Debug.Log("Init Succ");
            m_initState = true;
        }

        public void StartSpeech(int lengthSecond)
        {
            int errorcode = (int)Errors.MSP_SUCCESS;

            if (m_isListening)
            {
                Debug.Log("Speech Recognizer Is Listening!!");
            }

            m_sessionId = MSCDLL.QISRSessionBegin(null, SessionBeginParams, ref errorcode); //听写不需要语法，第一个参数为NULL

            if ((int)Errors.MSP_SUCCESS != errorcode)
            {
                OnError(string.Format("QISRSessionBegin failed! error code : {0}", errorcode));
                return;
            }

            m_audioClip = Microphone.Start(null, false, lengthSecond, 16000);
            //m_audioClip = Resources.Load<AudioClip>("test2");

            OnBegin("");
            m_isListening = true;
        }

        public override void StartSpeech()
        {
            StartSpeech(3);//默认三秒
        }

        public override bool isListening()
        {
            return m_isListening;
        }

        public override void StopSpeech()
        {
            if (m_isListening)
            {
                //Microphone.End(null);
                StartCoroutine("WaitResult");
            }
        }

        protected IEnumerator WaitResult()
        {
            int errorCode = (int)Errors.MSP_SUCCESS;
            var audioState = AudioStatus.MSP_AUDIO_SAMPLE_FIRST;
            var epState = EpStatus.MSP_EP_LOOKING_FOR_SPEECH;
            var recState = RecogStatus.MSP_REC_STATUS_SUCCESS;

            AudioSave.Save(RecordSavePath, m_audioClip);
            var bytes = IFlyUtils.ConvertClipToBytes(m_audioClip);
            errorCode = MSCDLL.QISRAudioWrite(Marshal.PtrToStringAnsi(m_sessionId), bytes, (uint)bytes.Length, audioState, ref epState, ref recState);
            if ((int)Errors.MSP_SUCCESS != errorCode)
            {
                OnError(string.Format("write LAST_SAMPLE failed: {0}", errorCode));
            }
            else
            {
                errorCode = MSCDLL.QISRAudioWrite(Marshal.PtrToStringAnsi(m_sessionId), null, 0, AudioStatus.MSP_AUDIO_SAMPLE_LAST, ref epState, ref recState);
                if ((int)Errors.MSP_SUCCESS != errorCode)
                {
                    OnError(string.Format("write LAST_SAMPLE failed: {0}", errorCode));
                }
                else
                {
                    IntPtr resultPtr = IntPtr.Zero;
                    string result = "";
                    while (recState != RecogStatus.MSP_REC_STATUS_COMPLETE)
                    {
                        resultPtr = MSCDLL.QISRGetResult(Marshal.PtrToStringAnsi(m_sessionId), ref recState, 0, ref errorCode);
                        if ((int)Errors.MSP_SUCCESS != errorCode)
                        {
                            OnError(string.Format("QISRGetResult failed! error code: {0}", errorCode));
                            break;
                        }
                        Debug.Log("Get Result : " + Marshal.PtrToStringAnsi(resultPtr));
                        result += Marshal.PtrToStringAnsi(resultPtr);
                        yield return 0;
                    }
                    if (errorCode == (int)Errors.MSP_SUCCESS)
                    {
                        OnResult(result);
                        MSCDLL.QISRSessionEnd(Marshal.PtrToStringAnsi(m_sessionId), "normal");
                        OnEnd("");
                        Clear();
                    }
                }
            }
        }

        public override void CancelSpeech()
        {
            Microphone.End(null);
            Clear();
        }

        protected void Clear()
        {
            m_audioClip = null;
            m_sessionId = IntPtr.Zero;
            m_isListening = false;
        }

        protected override void OnError(string error)
        {
            if (m_sessionId != IntPtr.Zero)
            {
                MSCDLL.QISRSessionEnd(Marshal.PtrToStringAnsi(m_sessionId), null);
            }
            Clear();
            base.OnError(error);
        }
    }
}
