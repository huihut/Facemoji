using UnityEngine;

namespace HuiHut.IFlyVoice
{
    public class XfInitListener : AndroidJavaProxy
    {

        public XfInitListener() : base("com.iflytek.cloud.InitListener")
        {

        }

        public void onInit(int code)
        {
            //ErrorCode.SUCCESS=0;
            if (code != 0)
            {
                string error = "Failure of initialization, error code: " + code;
                error.showAsToast();
            }
        }
    }
}