using System.Collections.Generic;
using UnityEngine;

namespace IFLYSpeech
{
    /// <summary>
    /// 本地音乐
    /// </summary>
    [System.Serializable]
    public class AudioHeadCatch
    {
        public List<string> audioKey = new List<string>();
        public void Register(string text)
        {
            if(!audioKey.Contains(text))
            {
                audioKey.Add(text);
            }
        }
        public void Remove(string text)
        {
            if (audioKey.Contains(text))
            {
                audioKey.Remove(text);
            }
        }
        public bool Contain(string text)
        {
            return audioKey.Contains(text);
        }
     
        public override string ToString()
        {
            return JsonUtility.ToJson(this);
        }
    }
}