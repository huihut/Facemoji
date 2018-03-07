using UnityEngine;
using HuiHut.Facemoji;

namespace Moments
{
    [RequireComponent(typeof(Recorder)), AddComponentMenu("Record")]
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
                    FaceTracking.startBtn.SetActive(true);
                    FaceTracking.finishBtn.SetActive(false);
                }

                m_Progress = 0f;
               
            }
        }

        void OnGUI()
        {
            if (m_Recorder.State != RecorderState.Recording || !string.IsNullOrEmpty(m_LastFile))
            {
                FacemojiAI.testMessageText.text = "Recorder State : " + m_Recorder.State.ToString();

                if (m_IsSaving)
                    FacemojiAI.testMessageText.text += "\nProgress Report : " + m_Progress.ToString("F2") + "%";

                if (!string.IsNullOrEmpty(m_LastFile))
                {
                    FacemojiAI.testMessageText.text = "Recording finished!";
                    m_LastFile = string.Empty;
                }
            }
        }
    }
}
