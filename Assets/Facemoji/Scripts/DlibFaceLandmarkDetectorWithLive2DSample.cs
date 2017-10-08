using UnityEngine;
using System.Collections;
using UnityEngine.UI;

#if UNITY_5_3 || UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

namespace Facemoji
{
    public class DlibFaceLandmarkDetectorWithLive2DSample : MonoBehaviour
    {
        bool isPress = false;
        float pressTimes = 0;

        // Use this for initialization
        void Start ()
        {

        }

        // Update is called once per frame
        void Update ()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // Press the first time
                if (!isPress)
                {
                    pressTimes = Time.time;
                    isPress = true;
                }
                // Press the second time
                else
                {
                    // Press twice in two seconds
                    if (Time.time - pressTimes < 2.0)
                    {
                        Application.Quit();
                    }
                    else
                    {
                        isPress = false;
                    }
                }
            }
        }

        public void OnShowLicenseButton ()
        {
            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
            SceneManager.LoadScene ("ShowLicense");
            #else
            Application.LoadLevel ("ShowLicense");
            #endif
        }

        public void OnWebCamTextureLive2DSample ()
        {
            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
            SceneManager.LoadScene ("WebCamTextureLive2DSample");
            #else
            Application.LoadLevel ("WebCamTextureLive2DSample");
            #endif
        }

        public void OnWebCamTextureLive2DSample320x240 ()
        {
            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
            SceneManager.LoadScene ("WebCamTextureLive2DSample320x240");
            #else
            Application.LoadLevel ("WebCamTextureLive2DSample320x240");
            #endif
        }
    }
}
