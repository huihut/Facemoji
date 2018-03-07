using LitJson;

namespace HuiHut.IFlyVoice
{
    class ParsingIFlyJson
    {
        public static string Parsing(string json)
        {
            if (json.Equals(""))
                return string.Empty;

            string text = string.Empty;

            JsonData paragraphObject = JsonMapper.ToObject(json);
            JsonData wsArray = paragraphObject["ws"];

            for (int i = 0; i < wsArray.Count; i++)
            {
                JsonData wsObject = wsArray[i];
                JsonData cwArray = wsObject["cw"];

                for (int j = 0; j < cwArray.Count; j++)
                {
                    JsonData cwObject = cwArray[j];
                    string w = cwObject["w"].ToString();
                    text = text + w;
                }
            }

            return text;
        }
    }
}

