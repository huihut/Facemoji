using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;
using System;

namespace Wangz.IFly
{
    public abstract class IFlyBase : MonoBehaviour
    {
        public string m_appid;
        protected string InitInfo { get { return string.Format("appid={0}", m_appid); } }
        protected const string SessionBeginParams = "sub = iat, ptt = 0, domain = iat, language = en_us, accent = mandarin, sample_rate = 16000, result_type = plain, result_encoding = utf8";
        protected string RecordSavePath { get { return Application.temporaryCachePath + "/iflyRecord.wav"; } }
        #region Speech Callback
        public event Action<string> OnErrorEvent;
        public event Action OnBeginEvent;
        public event Action OnEndEvent;
        public event Action<string> OnResultEvent;
        #endregion

        #region Unity Method
        protected AudioSource m_audioPlay;

        protected virtual void Awake()
        {
            m_audioPlay = gameObject.AddComponent<AudioSource>();
        }

        protected virtual void Start()
        {
            Init();
        }

        protected virtual void OnDestroy()
        {

        }
        #endregion

        #region iFly Method
        public abstract void Init();

        public abstract void StartSpeech();

        public abstract bool isListening();

        public abstract void StopSpeech();

        public abstract void CancelSpeech();

        #region Speech Callback
        protected virtual void OnError(string error)
        {
            Debug.Log("OnSpeechError : " + error);
            if (OnErrorEvent != null)
                OnErrorEvent(error);
        }

        protected void OnBegin(string empty)
        {
            Debug.Log("OnSpeechBegin");
            if (OnBeginEvent != null)
                OnBeginEvent();
        }

        protected void OnEnd(string empty)
        {
            Debug.Log("OnSpeechEnd");
            if (OnEndEvent != null)
                OnEndEvent();
        }

        protected void OnResult(string result)
        {
            Debug.Log("OnSpeechEnd : " + result);
            if (OnResultEvent != null)
                OnResultEvent(result);
        }
        #endregion
        #endregion

        #region Audio Method
        protected static bool m_isPlaying;

        public virtual void Play(string filepath)
        {
            StartCoroutine("YieldLoadAudio", filepath);
        }

        protected IEnumerator YieldLoadAudio(string filePath)
        {
            var www = new WWW("file:///" + filePath);
            yield return www;
            if (string.IsNullOrEmpty(www.error))
            {
                m_audioPlay.clip = www.GetAudioClip();
                m_audioPlay.Play();
                m_isPlaying = true;
            }
            www.Dispose();
        }

        public virtual bool IsPlaying()
        {
            return m_isPlaying;
        }

        public virtual void StopPlay()
        {
            m_audioPlay.Stop();
            m_audioPlay.clip = null;
            m_isPlaying = false;
        }
        #endregion
    }
}
