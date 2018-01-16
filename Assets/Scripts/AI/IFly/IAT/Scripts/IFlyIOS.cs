using UnityEngine;
using System.Collections;

namespace Wangz.IFly
{
    public class IFlyIOS : IFlyBase
    {
        public override void Init()
        {
            // iOS App id
            m_appid = "5a5c8258";
        }

        public override void StartSpeech()
        {
        }

        public override bool isListening()
        {
            return false;
        }

        public override void StopSpeech()
        {
        }

        public override void CancelSpeech()
        {
        }
    }
}
