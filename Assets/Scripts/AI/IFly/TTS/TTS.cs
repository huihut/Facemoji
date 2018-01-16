using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections.Generic;
using System;

namespace IFLYSpeech
{
    public class TTS : MonoBehaviour
    {
        public AudioCallBackEvent callBack;
        public Toggle pauseTog;
        public Button clearBtn;
        public Button downLandGroup;
        public Toggle[] toggles;
        private Params parma = new Params();
        IFLYSpeech.Txt2AudioCtrl ctrl;
        public string[] texts = {
        "本接口和QTTSSessionBegin对应，",
        "该句柄对应的相关资源（参数，合成文本，实例等）都会被释放，",
        "调用此接口后，",
        "用户不应再使用该句柄。"
    };
        void Start()
        {
            callBack.Invoke(OnCallBack);
            clearBtn.onClick.AddListener(RemoveLocal);
            downLandGroup.onClick.AddListener(GroupDownLand);
            ctrl = IFLYSpeech.Txt2AudioCtrl.Instance;
            ctrl.onError += OnError;
            for (int i = 0; i < toggles.Length; i++)
            {
                var index = i;
                toggles[index].onValueChanged.AddListener(x => { if (x) ActiveSpeaker(toggles[index].GetComponentInChildren<Text>().text); });
            }
        }

        private void OnCallBack(string arg0)
        {
            Debug.Log("Complete:" + arg0);
        }

        private void GroupDownLand()
        {
            StartCoroutine(ctrl.Downland(texts, (x) => { Debug.Log("下载进度" + x); }, parma));
        }
        private void ActiveSpeaker(string speaker)
        {
            parma.voice_name = speaker;
            //IFLYSpeech.Speakers;
        }
        void RemoveLocal()
        {
            ctrl.CleanUpCatchs();
        }
        void OnError(string err)
        {
            Debug.LogError(err);
        }
    }
}


