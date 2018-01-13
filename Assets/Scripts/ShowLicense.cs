using UnityEngine;
using System.Collections;

#if UNITY_5_3 || UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

namespace Facemoji
{
    public class ShowLicense : MonoBehaviour
    {

        // Use this for initialization
        void Start ()
        {

        }

        // Update is called once per frame
        void Update ()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                #if UNITY_5_3 || UNITY_5_3_OR_NEWER
                SceneManager.LoadScene("FacemojiStart");
#else
                Application.LoadLevel("FacemojiStart");
#endif
            }
        }

        public void OnBackButton ()
        {
            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
            SceneManager.LoadScene ("FacemojiStart");
#else
            Application.LoadLevel("FacemojiStart");
#endif
        }
    }
}
