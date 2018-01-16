using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;

namespace IFLYSpeech
{
  
    [RequireComponent(typeof(AudioSource))]
    public class TextAudioBehaiver : MonoBehaviour
    {
        private UnityAction<string> onComplete;
        private Txt2AudioCtrl t2a { get { return Txt2AudioCtrl.Instance; } }
        private AudioSource audioSource;
        public bool IsOn { get; set; }
        public bool IsMan { get; set; }
        private AudioTimer audioTimer = new AudioTimer();
        void Awake()
        {
            IsOn = true;
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        private void Update()
        {
            if (!IsOn) return;
            audioTimer.Update();
        }
        /// <summary>
        /// 女声
        /// </summary>
        /// <param name="text"></param>
        public void PlayAudio(string text)
        {
            if (!IsOn) return;

            if (audioSource.isPlaying)
            {
                audioSource.Stop();
                audioTimer.Stop();
            }

            StartCoroutine(t2a.GetAudioClip(text, (x) =>
            {
                if (x != null)
                {
                    audioSource.clip = x;
                    audioSource.Play();
                    audioTimer.Init(text, x.length, OnComplete);
                }
            }));
        }


        public void Pause(bool on)
        {
            if (!IsOn) return;
            if (audioTimer == null) return;

            if (!on)
            {
                audioSource.Pause();
                audioTimer.Pause();
            }
            else
            {
                audioSource.UnPause();
                audioTimer.UnPause();
            }
        }

        public void Stop(string data)
        {
            if (!IsOn) return;
            audioSource.Stop();
            audioTimer.Stop();
        }

        public void RegistCallBack(UnityAction<string> onComplete)
        {
            if (!IsOn) return;
            this.onComplete = onComplete;
        }

        private void OnComplete(string text)
        {
            if (onComplete != null) onComplete(text);
        }
    }

}
