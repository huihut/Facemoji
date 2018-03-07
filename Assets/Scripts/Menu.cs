using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/*
 * userLanguagesDropdown:
 * 0. 普通话
 * 1. English_US
 * 
 * speakerLanguagesDropdown:
 * 0. 楠楠_普通话
 * 1. 小燕_普通话
 * 2. 小宇_普通话
 * 3. Catherine_en_us
 * 4. Henry_en_us
 * 5. Vimary_en_us
 *
 * */

namespace HuiHut.Facemoji
{
    public class Menu : MonoBehaviour
    {
        public Text userLanguagesText;
        public Text speakerLanguagesText;

        public Dropdown userLanguagesDropdown;
        public Dropdown speakerLanguagesDropdown;
        

        int userIndexSelected;
        int speakerIndexSelected;

        public static string userLanguageSelected = "mandarin";      // default：普通话
        public static string speakerLanguageSelected = "nannan";     // default：楠楠

        // Use this for initialization
        void Start()
        {
            // Set userLanguages position
            userLanguagesText.transform.Translate(new Vector3(0, Screen.height / 2 - 350, 0));
            userLanguagesDropdown.transform.Translate(new Vector3(0, Screen.height / 2 - 400, 0));

            // Set speakerLanguages position
            speakerLanguagesText.transform.Translate(new Vector3(0, Screen.height / 2 - 450, 0));
            speakerLanguagesDropdown.transform.Translate(new Vector3(0, Screen.height / 2 - 500, 0));

            // Set the index of your choice
            userIndexSelected = userLanguagesDropdown.value;
            speakerIndexSelected = speakerLanguagesDropdown.value;
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
#if UNITY_5_3 || UNITY_5_3_OR_NEWER
                SceneManager.LoadScene("FacemojiStart");
#else
                Application.LoadLevel("FacemojiStart");
#endif
            }

            // Change the choice of Dropdown
            if (userIndexSelected != userLanguagesDropdown.value)
            {
                UpdateUserLanguageSelected();
                userIndexSelected = userLanguagesDropdown.value;
            }
            if (speakerIndexSelected != speakerLanguagesDropdown.value)
            {
                UpdateSpeakerLanguageSelected();
                speakerIndexSelected = speakerLanguagesDropdown.value;
            }

        }

        private void UpdateUserLanguageSelected()
        {
            switch (userLanguagesDropdown.value)
            {
                case 0:
                    userLanguageSelected = "mandarin"; 
                    break;
                case 1:
                    userLanguageSelected = "en_us";
                    break;
                default:
                    userLanguageSelected = "mandarin";      // default：普通话
                    break;
            }
        }

        private void UpdateSpeakerLanguageSelected()
        {
            switch (speakerLanguagesDropdown.value)
            {
                case 0:
                    speakerLanguageSelected = "nannan"; 
                    break;
                case 1:
                    speakerLanguageSelected = "xiaoyan"; 
                    break;
                case 2:
                    speakerLanguageSelected = "xiaoyu"; 
                    break;
                case 3:
                    speakerLanguageSelected = "catherine";
                    break;
                case 4:
                    speakerLanguageSelected = "henry";
                    break;
                case 5:
                    speakerLanguageSelected = "vimary";
                    break;
                default:
                    speakerLanguageSelected = "nannan";     // default：楠楠
                    break;
            }
        }
    }

}
