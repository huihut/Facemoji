using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;

namespace HuiHut.Turing
{
    [AddComponentMenu("HuiHut/TuringRobot")]
    public class TuringRobot
    {
        #region
        /*
        * 
        * Repository: https://github.com/huihut/TuringRobot
        * Email: huihut@outlook.com
        * 
        * 图灵机器人：http://www.tuling123.com/
        * 帮助文档：http://www.tuling123.com/help/h_cent_webapi.jhtml?nav=doc
        * 
        */
        #endregion

        /// <summary>
        /// 图灵机器人API地址 
        /// </summary>
        private static readonly string API_Address = @"http://www.tuling123.com/openapi/api";

        /// <summary>
        /// 图灵机器人API Key
        /// </summary>
        private string API_Key = @"2ee6e84a755b4ac2b5b2cc25d992b03a";

        /// <summary>
        /// 随机生成用户ID，用于关联上下文语境
        /// </summary>
        private string userID = new System.Random().Next(0, int.MaxValue).ToString();

        /// <summary>
        /// 发送的文本 = 图灵机器人API地址 
        ///     + "?key=" + 图灵机器人API Key 
        ///     + "&info=" + 用户输入消息 
        ///     + "&userid=" + 用户id
        /// </summary>
        private static string sendText = string.Empty;

        /// <summary>
        /// 机器人返回的json格式的文本
        /// </summary>
        private static string returnText = string.Empty;

        /// <summary>
        /// 机器人返回代码
        /// </summary>
        private static string robotCode = string.Empty;

        /// <summary>
        /// 机器人回复的消息
        /// </summary>
        private static string robotMessage = string.Empty;

        /// <summary>
        /// 机器人回复链接列表
        /// </summary>
        private static List<string> robotLinks = new List<string>();

        /// <summary>
        /// 初始化机器人
        /// </summary>
        /// <param name="API_Key">图灵机器人API Key</param>
        /// <param name="userID">用户ID标识，用于关联上下文语境</param>
        /// <returns>返回空</returns>
        public void initRobot(string API_Key, string userID)
        {
            this.API_Key = API_Key;
            this.userID = userID;
        }

        /// <summary>
        /// 与机器人聊天
        /// </summary>
        /// <param name="Message">机器人回复的消息</param>
        /// <param name="Links">机器人回复的链接</param>
        /// <returns>返回空</returns>
        public void Chat(string userMessage, ref string Message, ref List<string> Links)
        {
            // 用户发送空，则回复空
            if (userMessage == null || userMessage.Equals(""))
                return;

            //过滤消息
            if (MessageFilter(ref userMessage))
                return;

            // 连接成要发送的文本
            sendText = API_Address + "?key=" + API_Key + "&info=" + userMessage + "&userid=" + userID;

            // 使用 HTTP POST 给图灵机器人发送信息
            returnText = PostMessage(sendText);

            if (!AnalysisMessage(returnText, ref robotCode, ref robotMessage))
            {
                Debug.Log("无法理解的机器人消息: " + returnText);
                return;
            }

            Message = robotMessage;
            Links = robotLinks;
        }

        /// <summary>
        /// 使用 HTTP POST 给图灵机器人发送信息
        /// </summary>
        /// <param name="strURL">要发送的文本：API_Address + "?key=" + API_Key + "&info=" + userMessage + "&userid=" + userID</param>
        /// <returns>图灵机器人返回的json文本</returns>
        private string PostMessage(string strURL)
        {
            HttpWebRequest request = null;
            HttpWebResponse response = null;
            try
            {
                request = (HttpWebRequest)HttpWebRequest.Create(strURL);
                request.Timeout = 5000;
                request.Method = "POST";
                response = (HttpWebResponse)request.GetResponse();
                StreamReader sr = new StreamReader(response.GetResponseStream());
                string jsonstr = sr.ReadToEnd();

                return jsonstr;
            }
            catch (Exception e)
            {
                Debug.Log("ERROR : " + e.Message);
                return null;
            }
            finally
            {
                response.Close();
            }
        }

        /// <summary>
        /// 用户消息过滤器
        /// </summary>
        /// <param name="Message">用户消息</param>
        /// <returns>是否拦截此消息</returns>
        private static bool MessageFilter(ref string Message)
        {
            string TempMessage = Message.ToLower().Trim();
            switch (TempMessage)
            {
                case "/exit":
                    {
                        Application.Quit();
                        return true;
                    }
                default:
                    {
                        return false;
                    }
            }
        }

        /// <summary>
        /// 分析返回的文本
        /// </summary>
        /// <param name="returnMessage">返回消息</param>
        /// <param name="robotCode">返回代码</param>
        /// <param name="robotMessage">机器人消息</param>
        /// <returns>是否能够分析消息</returns>
        private static bool AnalysisMessage(string returnMessage, ref string robotCode, ref string robotMessage)
        {
            Debug.Log("分析返回消息: " + returnMessage);
            robotLinks.Clear();
            try
            {
                string MessagePattern = "{.*?\"code\":(?<RobotCode>.+?),.*?}";
                Regex MessageRegex = new Regex(MessagePattern, RegexOptions.IgnoreCase | RegexOptions.Singleline); ;
                Match MessageMatch = MessageRegex.Match(returnMessage);
                if (MessageMatch.Success)
                {
                    robotCode = MessageMatch.Groups["RobotCode"].Value as string;
                    Debug.Log("获取返回代码: " + robotCode);
                }
                else
                {
                    Debug.Log("无法获取返回代码: " + returnMessage);
                    return false;
                }

                switch (robotCode)
                {
                    //普通消息
                    case "100000":
                        {
                            MessagePattern = "{.*?\"text\":\"(?<RobotMessage>.+?)\".*?}";
                            MessageRegex = new Regex(MessagePattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                            MessageMatch = MessageRegex.Match(returnMessage);
                            if (MessageMatch.Success)
                            {
                                robotMessage = MessageMatch.Groups["RobotMessage"].Value as string;
                                return true;
                            }
                            else
                            {
                                Debug.Log("无法正则匹配的消息: " + returnMessage);
                                return false;
                            }
                        }
                    //链接消息
                    case "200000":
                        {
                            MessagePattern = "{.*?\"text\":\"(?<RobotMessage>.*?)\",\"url\":\"(?<RobotLink>.*?)\".*?}";
                            MessageRegex = new Regex(MessagePattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                            MessageMatch = MessageRegex.Match(returnMessage);
                            if (MessageMatch.Success)
                            {
                                robotMessage = MessageMatch.Groups["RobotMessage"].Value as string;
                                robotLinks.Add(MessageMatch.Groups["RobotLink"].Value as string);
                                return true;
                            }
                            else
                            {
                                Debug.Log("无法正则匹配的消息: " + returnMessage);
                                return false;
                            }
                        }
                    //新闻类
                    case "302000":
                        {
                            MessagePattern = "{.*?\"text\":\"(?<RobotMessage>.*?)\".*?}";
                            MessageRegex = new Regex(MessagePattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                            MessageMatch = MessageRegex.Match(returnMessage);
                            if (MessageMatch.Success)
                            {
                                robotMessage = MessageMatch.Groups["RobotMessage"].Value as string;
                            }
                            else
                            {
                                Debug.Log("无法正则匹配的消息: " + returnMessage);
                                return false;
                            }
                            MessagePattern = "{\"article\":\"(?<NewsTitle>.*?)\",\"source\":\"(?<NewsSource>.*?)\",\"icon\":\"(?<NewsIcon>.*?)\",\"detailurl\":\"(?<NewsLink>.*?)\"}";
                            MessageRegex = new Regex(MessagePattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                            int NewsIndex = 0;
                            foreach (Match NewsMatch in MessageRegex.Matches(returnMessage))
                            {
                                NewsIndex++;
                                robotMessage += string.Format("\n\t[{0}] {1}-(来自：{2})",
                                    NewsIndex,
                                    NewsMatch.Groups["NewsTitle"].Value as string,
                                    NewsMatch.Groups["NewsSource"].Value as string
                                );
                                robotLinks.Add(NewsMatch.Groups["NewsLink"].Value as string);
                            }
                            return true;
                        }
                    //菜谱类
                    case "308000":
                        {
                            MessagePattern = "{.*?\"text\":\"(?<RobotMessage>.*?)\".*?}";
                            MessageRegex = new Regex(MessagePattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                            MessageMatch = MessageRegex.Match(returnMessage);
                            if (MessageMatch.Success)
                            {
                                robotMessage = MessageMatch.Groups["RobotMessage"].Value as string;
                            }
                            else
                            {
                                Debug.Log("无法正则匹配的消息: " + returnMessage);
                                return false;
                            }
                            MessagePattern = "{\"name\":\"(?<FoodName>.*?)\",\"icon\":\"(?<FoodIcon>.*?)\",\"info\":\"(?<FoodInfo>.*?)\",\"detailurl\":\"(?<FoodLink>.*?)\"}";
                            MessageRegex = new Regex(MessagePattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                            int NewsIndex = 0;
                            foreach (Match NewsMatch in MessageRegex.Matches(returnMessage))
                            {
                                NewsIndex++;
                                robotMessage += string.Format("\n\t[{0}] {1}-(来自：{2})",
                                    NewsIndex,
                                    NewsMatch.Groups["FoodName"].Value as string,
                                    NewsMatch.Groups["FoodInfo"].Value as string
                                );
                                robotLinks.Add(NewsMatch.Groups["FoodLink"].Value as string);
                            }
                            return true;
                        }
                    default:
                        {
                            // "40001"/"40002"/"40004"/"40007" : 遇到异常的返回代码
                            Debug.Log("遇到未知的返回代码: " + robotCode);
                            MessagePattern = "{.*?\"text\":\"(?<ErrorMessage>.*?)\".*?}";
                            MessageRegex = new Regex(MessagePattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
                            MessageMatch = MessageRegex.Match(returnMessage);
                            if (MessageMatch.Success)
                            {
                                robotMessage = MessageMatch.Groups["ErrorMessage"].Value as string;
                                return true;
                            }
                            else
                            {
                                Debug.Log("无法正则匹配的消息: " + returnMessage);
                                return false;
                            }
                        }
                }
            }
            catch (Exception ex)
            {
                Debug.Log("分析消息遇到异常: " + ex.Message);
                return false;
            }
        }
    }
}