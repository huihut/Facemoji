using UnityEngine;
using UnityEngine.Events;
namespace IFLYSpeech
{
    [System.Serializable]
    public class AudioCallBackEvent : UnityEvent<UnityAction<string>> { }
    public class AudioTimer
    {
        public string text;
        public float timer;
        public float audioTime;
        public UnityAction<string> onComplete;
        private bool isPause;
        private bool completed;
        public void Init(string text, float audioTime, UnityAction<string> onComplete)
        {
            this.text = text;
            this.timer = 0;
            this.audioTime = audioTime;
            this.onComplete = onComplete;
            isPause = false;
            completed = false;
        }
        public void Update()
        {
            if (isPause) return;
            if (completed) return;
            if (timer > audioTime)
            {
                Stop();
            }
            else
            {
                timer += Time.deltaTime;
            }
        }
        public void Stop()
        {
            if (completed) return;
            if (onComplete != null)
                onComplete.Invoke(text);
            completed = true;
        }
        public void Pause()
        {
            isPause = true;
        }
        public void UnPause()
        {
            isPause = false;
        }
    }
}