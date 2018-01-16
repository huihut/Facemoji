using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;

namespace IFLYSpeech
{
    public class Txt2AudioCtrl : ITextToAudio
    {
        #region 单例
        private static Txt2AudioCtrl instance = default(Txt2AudioCtrl);
        private static object lockHelper = new object();
        public static Txt2AudioCtrl Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockHelper)
                    {
                        if (instance == null)
                        {

                            instance = new Txt2AudioCtrl();
                        }
                    }
                }
                return instance;
            }
        }
        #endregion
        public event UnityAction<string> onError;
        private const string play_prefKey = "IFLYSpeech_audioHead";
        private AudioHeadCatch audioHead;
        private static string _audipPath;
        private static string AudioPath
        {
            get
            {
                if (_audipPath == null)
                {
                    _audipPath = Application.streamingAssetsPath + "/Audio";
                    if (!Directory.Exists(_audipPath)){
                        Directory.CreateDirectory(_audipPath);
                    }
                }
                return _audipPath;
            }
        }
        private Interanl.TTS tts;
        private Interanl.TTS TTS
        {
            get
            {
                if (tts == null)
                {
                    // App_id
                    if (Application.platform == RuntimePlatform.Android)
                    {
                        Interanl.Config config = new Interanl.Config("5a5c6bfb");
                        tts = new Interanl.TTS(config.ToString());
                    }
                    else if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
                    {
                        Interanl.Config config = new Interanl.Config("5a5b0136");
                        tts = new Interanl.TTS(config.ToString());
                    }
                    else if(Application.platform == RuntimePlatform.IPhonePlayer)
                    {
                        Interanl.Config config = new Interanl.Config("5a5c8258");
                        tts = new Interanl.TTS(config.ToString());
                    }
                    else
                    {
                        Interanl.Config config = new Interanl.Config("");
                        tts = new Interanl.TTS(config.ToString());
                    }
                    
                }
                return tts;
            }
        }
        private Params defultParams;
        private Thread downLandThread;
        private Queue<KeyValuePair<string, Params>> waitSpeekQueue = new Queue<KeyValuePair<string, Params>>();
        private bool connectError;
        private List<string> completed = new List<string>();
        protected Txt2AudioCtrl()
        {
            if (PlayerPrefs.HasKey(play_prefKey)){
                var value = PlayerPrefs.GetString(play_prefKey);
                if (!string.IsNullOrEmpty(value)){
                    audioHead = JsonUtility.FromJson<AudioHeadCatch>(value);
                }
            }
            if (audioHead == null) audioHead = new IFLYSpeech.AudioHeadCatch();
            defultParams = new global::Params();
        }

        /// <summary>
        /// 获取音频
        /// </summary>
        /// <param name="text"></param>
        /// <param name="OnGet"></param>
        /// <returns></returns>
        public IEnumerator GetAudioClip(string text, UnityAction<AudioClip> OnGet, Params paramss = null)
        {
            if (paramss == null)
            {
                paramss = defultParams;
            }
            var audioName = AudioFileName(text, paramss);
            if (!audioHead.Contain(audioName))
            {
                if (File.Exists(Path.Combine(AudioPath, audioName)))
                {
                    RecordToText(audioName);
                    yield return LoadFromFile(audioName, OnGet);
                }
                else
                {
                    yield return DownLandFromWeb(text, paramss, OnGet);
                }
            }
            else
            {
                yield return LoadFromFile(audioName, OnGet);
            }
        }

        IEnumerator DownLandFromWeb(string text, Params paramss, UnityAction<AudioClip> OnGet)
        {
            var path = AudioPath;//Path可能还没有初始化
            string audioName = AudioFileName(text, paramss);
            bool complete = false;
            string error = null;

            TTS_SpeakFinished finishEvent = (result, data) =>
            {
                if (result == text)
                {
                    complete = true;
                }
            };

            TTS_SpeakError errorEvent = (err) =>
            {
                complete = true;
                error = err;
            };

            TTS.tts_SpeakFinishedEvent += finishEvent;
            TTS.ttsSpeakErrorEvent += errorEvent;

            waitSpeekQueue.Enqueue(new KeyValuePair<string, Params>(text, paramss));

            if (downLandThread == null || !downLandThread.IsAlive)
            {
                downLandThread = new Thread(ThreadDownland);
                downLandThread.Start(AudioPath);
            }

            yield return new WaitUntil(() => complete|| connectError);

            if(connectError){
                error = "err:语音联网失败";
            }

            if (error != null)
            {
                if (onError != null)
                {
                    onError.Invoke(error);
                }
                else
                {
                    Debug.LogError(error);
                }
            }
            else
            {
                RecordToText(audioName);
                yield return LoadFromFile(audioName, OnGet);
            }

            TTS.tts_SpeakFinishedEvent -= finishEvent;
            TTS.ttsSpeakErrorEvent -= errorEvent;
        }

        void ThreadDownland(object audioPath)
        {
            float waitTime = 5000;
            while(!TTS.active){
                Thread.Sleep(100);
                waitTime -= 100;
                if(waitTime< 0){
                    connectError = true;
                    return;
                }
            }
            while (waitSpeekQueue.Count > 0)
            {
                var item = waitSpeekQueue.Dequeue();
                TTS.Speak(item.Key, item.Value.ToString(), Path.Combine(audioPath.ToString(), AudioFileName(item.Key, item.Value)));
            }
        }

        /// <summary>
        /// 从本地下载
        /// </summary>
        /// <param name="text"></param>
        /// <param name="OnGet"></param>
        /// <returns></returns>
        IEnumerator LoadFromFile(string audioName, UnityAction<AudioClip> OnGet)
        {
            string path = AudioPath + "/" + audioName;
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                WWW www = new WWW("file:///" + path);
                yield return www;
                if (www.error != null)
                {
                    OnGet(null);
                    Debug.Log(www.error);
                }
                else
                {
                    OnGet(www.GetAudioClip(false, false, AudioType.WAV));
                }
            }
            else
            {
                OnGet(null);
            }
        }

        private void RecordToText(string audioName)
        {
            audioHead.Register(audioName);
            PlayerPrefs.SetString(play_prefKey, audioHead.ToString());
        }

        public string AudioFileName(string text, Params param)
        {
            var source = text + param;
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] data = System.Text.Encoding.UTF8.GetBytes(source);
            byte[] md5Data = md5.ComputeHash(data, 0, data.Length);
            md5.Clear();

            string destString = "";
            for (int i = 0; i < md5Data.Length; i++) {
                destString += System.Convert.ToString(md5Data[i], 16).PadLeft(2, '0');
            }
            destString = destString.PadLeft(32, '0');
            return destString + ".wav";
        }


        public IEnumerator Downland(string[] text, UnityAction<float> onProgressChanged, Params paramss = null)
        {
            if (paramss == null){
                paramss = defultParams;
            }
            List<string> needDownLand = new List<string>();

            foreach (var item in text)
            {
                if (!audioHead.audioKey.Contains(AudioFileName(item, paramss)))
                {
                    needDownLand.Add(item);
                }
            }

            float totalCount = text.Length;
            float currentCount = totalCount - needDownLand.Count;

            if (currentCount > 0 && onProgressChanged != null) onProgressChanged(currentCount / totalCount);
          
            if(needDownLand.Count > 0)
            {
                TTS_SpeakFinished finishEvent = (result, data) =>
                {
                    currentCount++;
                };

                TTS_SpeakError errorEvent = (err) =>
                {
                    currentCount++;
                };

                TTS.tts_SpeakFinishedEvent += finishEvent;
                TTS.ttsSpeakErrorEvent += errorEvent;


                foreach (var item in needDownLand.ToArray())
                {
                    waitSpeekQueue.Enqueue(new KeyValuePair<string, Params>(item, paramss));
                }

                if (downLandThread == null || !downLandThread.IsAlive)
                {
                    downLandThread = new Thread(ThreadDownland);
                    downLandThread.Start(AudioPath);
                }

                var countTemp = currentCount;

                while (currentCount != totalCount && !connectError)
                {
                    if(countTemp != currentCount)
                    {
                        if (onProgressChanged != null) onProgressChanged(currentCount / totalCount);
                        countTemp = currentCount;
                    }
                    yield return null;
                }

                for (int i = 0; i < needDownLand.Count; i++)
                {
                    RecordToText(AudioFileName(needDownLand[i], paramss));
                }

                TTS.tts_SpeakFinishedEvent -= finishEvent;
                TTS.ttsSpeakErrorEvent -= errorEvent;
            }
         
        }

        public void CleanUpCatchs()
        {
            for (int i = 0; i < audioHead.audioKey.Count; i++)
            {
                string path = Path.Combine(AudioPath, audioHead.audioKey[i]);
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            audioHead.audioKey.Clear();
            PlayerPrefs.SetString(play_prefKey, "");
        }
    }
}