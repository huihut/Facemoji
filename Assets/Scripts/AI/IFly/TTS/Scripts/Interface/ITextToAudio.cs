using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;


namespace IFLYSpeech
{
    public interface ITextToAudio
    {
        event UnityAction<string> onError;
        IEnumerator GetAudioClip(string text, UnityAction<AudioClip> OnGet, Params param = null);
        IEnumerator Downland(string[] text,UnityAction<float> onProgressChanged ,Params param = null);
        void CleanUpCatchs();
    }
}
