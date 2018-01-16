using UnityEngine;
using System.Collections;

[System.Serializable]
public class Params {
    public string engine_type = "cloud";
    public string voice_name = "xiaoyan";
    public byte speed = 50;
    public byte volume = 50;
    public byte pitch = 50;
    public byte rdn = 0;
    public byte rcn = 1;
    public string text_encoding = "Unicode";
    public short sample_rate = 16000;
    public byte background_sound = 0;
    public string aue = "speex-wb;7";
    public string ttp = "text";

    public override string ToString()
    {
        var fields = typeof(Params).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.GetField | System.Reflection.BindingFlags.Instance);
        var param = new string[fields.Length];
        for (int i = 0; i < fields.Length; i++)
        {
            param[i] = fields[i].Name + "=" + fields[i].GetValue(this);
        }
        return string.Join(",",param);
    }
}
