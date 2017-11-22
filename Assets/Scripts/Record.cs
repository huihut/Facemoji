using UnityEngine;
using Moments;

namespace Facemoji
{
    [RequireComponent(typeof(Recorder)), AddComponentMenu("Facemoji Record")]
    public class Record : MonoBehaviour
    {
        Recorder m_Recorder;
        float m_Progress = 0f;
        string m_LastFile = "";
        bool m_IsSaving = false;

        // Is recording?
        public static bool isRecording = false;

        void Start()
        {
            // Get our Recorder instance (there can be only one per camera).
            m_Recorder = GetComponent<Recorder>();

            // If you want to change Recorder settings at runtime, use :
            //m_Recorder.Setup(autoAspect, width, height, fps, bufferSize, repeat, quality);

            // The Recorder starts paused for performance reasons, call Record() to start
            // saving frames to memory. You can pause it at any time by calling Pause().
            m_Recorder.Record();

            // Optional callbacks (see each function for more info).
            m_Recorder.OnPreProcessingDone = OnProcessingDone;
            m_Recorder.OnFileSaveProgress = OnFileSaveProgress;
            m_Recorder.OnFileSaved = OnFileSaved;
        }

        void OnProcessingDone()
        {
            // All frames have been extracted and sent to a worker thread for compression !
            // The Recorder is ready to record again, you can call Record() here if you don't
            // want to wait for the file to be compresse and saved.
            // Pre-processing is done in the main thread, but frame compression & file saving
            // has its own thread, so you can save multiple gif at once.

            m_IsSaving = true;
        }

        void OnFileSaveProgress(int id, float percent)
        {
            // This callback is probably not thread safe so use it at your own risks.
            // Percent is in range [0;1] (0 being 0%, 1 being 100%).
            m_Progress = percent * 100f;
        }

        void OnFileSaved(int id, string filepath)
        {
            // Our file has successfully been compressed & written to disk !
            m_LastFile = filepath;

            m_IsSaving = false;

            // Let's start recording again (note that we could do that as soon as pre-processing
            // is done and actually save multiple gifs at once, see OnProcessingDone().
            m_Recorder.Record();
        }

        void OnDestroy()
        {
            // Memory is automatically flushed when the Recorder is destroyed or (re)setup,
            // but if for some reason you want to do it manually, just call FlushMemory().
            //m_Recorder.FlushMemory();
        }

        void Update()
        {
            // Click OnStartButton to start recording
            if (isRecording)
            {
                // Compress & save the buffered frames to a gif file. We should check the State
                // of the Recorder before saving, but for the sake of this example we won't, so
                // you'll see a warning in the console if you try saving while the Recorder is
                // processing another gif.
                m_Recorder.Save();

                // Recording completed
                // The start button can be pressed, the finish button can not be pressed
                if (m_IsSaving)
                {
                    // Recording completed
                    isRecording = false;
                    // The start button can be pressed, the finish button can not be pressed
                    WebCamTextureLive2DSample.startBtn.SetActive(true);
                    WebCamTextureLive2DSample.finishBtn.SetActive(false);
                }

                m_Progress = 0f;
               
            }
        }

        void OnGUI()
        {       
            Rect rect = new Rect(0, 110, Screen.width, Screen.height);

            // GUIStyle -> fontSize
            GUIStyle style = new GUIStyle();
            style.normal.textColor = new Color(255, 255, 255);
            style.fontSize = 24;

            GUIStyle styleBigger = new GUIStyle();
            styleBigger.normal.textColor = new Color(255, 255, 255);
            styleBigger.fontSize = 35;

            // GUILayout -> Label
            GUILayout.BeginArea(rect);
            GUILayout.Width(Screen.width);
            GUILayout.BeginHorizontal();
            GUILayout.Space(10f);
            GUILayout.BeginVertical();
            GUILayout.Space(10f);

            //GUILayout.Label("Press [StartButton] to export the buffered frames to a gif file.", style);
            GUILayout.Label("Recorder State : " + m_Recorder.State.ToString(), styleBigger);

            if (m_IsSaving)
                GUILayout.Label("Progress Report : " + m_Progress.ToString("F2") + "%", styleBigger);

            if (!string.IsNullOrEmpty(m_LastFile))
                GUILayout.Label("Save to : \n" + m_LastFile, style, GUILayout.MaxWidth(Screen.width), GUILayout.ExpandWidth(false));

            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

    }

}
