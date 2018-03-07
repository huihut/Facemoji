using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using HuiHut.Turing;
using HuiHut.IFlyVoice;

namespace HuiHut.Facemoji
{
    /// <summary>
    /// Facemoji AI，使用 Live2D 模型做的聊天机器人
    /// 聊天API使用图灵机器人：
    /// </summary>
    [AddComponentMenu("Facemoji/FacemojiAI")]
    //[RequireComponent(typeof(WebCamTextureToMatHelper))]
    public class FacemojiAI : MonoBehaviour
    {
        /// <summary>
        /// 图灵机器人
        /// </summary>
        private static TuringRobot turingRobot = new TuringRobot();

        /// <summary>
        /// 图灵机器人API Key
        /// </summary>
        public string API_Key = @"2ee6e84a755b4ac2b5b2cc25d992b03a";

        /// <summary>
        /// 随机生成用户ID，用于关联上下文语境
        /// </summary>
        private string userID = new System.Random().Next(0, int.MaxValue).ToString();
        
        /// <summary>
        /// 用户输入消息
        /// </summary>
        private static string userMessage = string.Empty;

        /// <summary>
        /// 机器人回复的消息
        /// </summary>
        private static string robotMessage = string.Empty;

        /// <summary>
        /// 机器人返回链接列表
        /// </summary>
        private static List<string> robotLinks = new List<string>();

        /// <summary>
        /// 机器人回复消息、录制消息在界面上的显示Label
        /// </summary>
        public GameObject testMessageLabel;

        /// <summary>
        /// testMessageLabel的文本内容
        /// </summary>
        public static Text testMessageText;


        // Use this for initialization
        void Start()
        {
            // 设置信息标签（messageLabel）位于屏幕偏下位置
            testMessageLabel.transform.Translate(new Vector3(0, - 2 * Screen.height / 3 + 50, 0));

            testMessageText = testMessageLabel.GetComponent<Text>();
            
            // 使用设备唯一标识
            userID = SystemInfo.deviceUniqueIdentifier;

            // 初始化图灵机器人
            turingRobot.initRobot(API_Key, userID);
            
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
        }

        // 发送消息给机器人
        public static void SendToRobot(string userMessage)
        {
            if(!userMessage.Equals(""))
            {
                // 用户消息传入机器人，获取机器人回复信息、回复链接
                turingRobot.Chat(userMessage, ref robotMessage, ref robotLinks);

                //正常的返回结果
                if (robotLinks.Count == 0)  // 机器人回复的消息无链接
                {
                    // 把信息显示在测试的text中
                    testMessageText.text = robotMessage;
                }
                else  // 机器人回复的消息有链接
                {
                    // 把信息显示在测试的text中
                    testMessageText.text = robotMessage;
                    foreach (string Link in robotLinks)
                        testMessageText.text += ("\n" + Link);
                }

                if(!string.IsNullOrEmpty(Menu.speakerLanguageSelected))
                {
                    // 朗读机器人回复的消息
                    IFlyVoice.IFlyVoice.startSpeaking(robotMessage, Menu.speakerLanguageSelected);
                }
                else
                {
                    "Please choose a speaker!".showAsToast();
                }
            }
            else
            {
                "The message sent is empty!".showAsToast();
            }
        }


        public void OnUserMessageSendButton()
        {
            // 获取用户输入的内容
            userMessage = GameObject.Find("userMessageInputField").GetComponent<InputField>().text;
            
            // 发送消息给机器人
            SendToRobot(userMessage);
        }

        public void OnSpeakButton()
        {
            if (!string.IsNullOrEmpty(Menu.userLanguageSelected))
            {
                // 语音识别，识别后自动发送消息给机器人
                IFlyVoice.IFlyVoice.startRecognize(Menu.userLanguageSelected);
            }
            else
            {
                "Please choose your language!".showAsToast();
            }
        }

        public void OnBackButton()
        {
#if UNITY_5_3 || UNITY_5_3_OR_NEWER
            SceneManager.LoadScene("FacemojiStart");
#else
            Application.LoadLevel("FacemojiStart");
#endif
        }

    }
}

