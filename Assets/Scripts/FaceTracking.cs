using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;

#if UNITY_5_3 || UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif
using OpenCVForUnity;
using DlibFaceLandmarkDetector;
using DlibFaceLandmarkDetectorSample;
using Moments;

namespace HuiHut.Facemoji
{
    /// <summary>
    /// FaceTracking, Use Dlib to detect face landmark and use Live2D model to track faces
    /// </summary>
    [AddComponentMenu("Facemoji/FaceTracking")]
    [RequireComponent (typeof(WebCamTextureToMatHelper))]
    public class FaceTracking : MonoBehaviour
    {
        /// <summary>
        /// All Live2D Texture
        /// </summary>
        private enum WhichTexture
        {
            shizuku = 1,
            haru = 2,
        };

        /// <summary>
        /// The selected Live2D Texture now.(Default shizuku)
        /// </summary>
        private WhichTexture selectedTexture = WhichTexture.shizuku;

        /// <summary>
        /// Start Button
        /// </summary>
        public static GameObject startBtn;

        /// <summary>
        /// Finish Button
        /// </summary>
        public static GameObject finishBtn;

        /// <summary>
        /// live2DModel.transform.localScale
        /// </summary>
        public float modelScale = 0.9f;

        /// <summary>
        /// The web cam texture to mat helper.
        /// </summary>
        WebCamTextureToMatHelper webCamTextureToMatHelper;

        /// <summary>
        /// The face landmark detector.
        /// </summary>
        FaceLandmarkDetector faceLandmarkDetector;

        /// <summary>
        /// The live2DModel.
        /// </summary>
        public Live2DModel live2DModel;

        /// <summary>
        /// The frontal face parameter.
        /// </summary>
        FrontalFaceParam frontalFaceParam;

        /// <summary>
        /// The shape_predictor_68_face_landmarks_dat_filepath.
        /// </summary>
        private string shape_predictor_68_face_landmarks_dat_filepath;

        /// <summary>
        /// Model file path.
        /// </summary>
        private string moc_filepath;
        private string physics_filepath;
        private string pose_filepath;
        private string[] texture_filepath = new string[6];


        // Use this for initialization
        void Start()
        {
            startBtn = GameObject.Find("StartButton");
            finishBtn = GameObject.Find("FinishButton");
            startBtn.SetActive(true);
            finishBtn.SetActive(false);

            webCamTextureToMatHelper = gameObject.GetComponent<WebCamTextureToMatHelper>();

#if UNITY_WEBGL && !UNITY_EDITOR
            webCamTextureToMatHelper.flipHorizontal = true;
            StartCoroutine(getFilePathCoroutine());
#else
            // FaceLandmark model filepath
            shape_predictor_68_face_landmarks_dat_filepath = DlibFaceLandmarkDetector.Utils.getFilePath("shape_predictor_68_face_landmarks.dat");

            // Load Texture filepath
            LoadTexture();
            
            Run();
#endif
        }

        private void LoadTexture()
        {
            // Load model filepath
            switch (selectedTexture)
            {
                case WhichTexture.haru:
                    {
                        // Load haru model file
                        moc_filepath = OpenCVForUnity.Utils.getFilePath("haru/haru_01.moc.bytes");
                        physics_filepath = OpenCVForUnity.Utils.getFilePath("haru/haru.physics.json");
                        pose_filepath = OpenCVForUnity.Utils.getFilePath("haru/haru.pose.json");
                        for (int i = 0; i < texture_filepath.Length; i++)
                        {
                            texture_filepath[i] = OpenCVForUnity.Utils.getFilePath("haru/haru_01.1024/texture_0" + i + ".png");
                        }
                        break;
                    }
                case WhichTexture.shizuku:
                    {
                        // Load shizuku model file
                        moc_filepath = OpenCVForUnity.Utils.getFilePath("shizuku/shizuku.moc.bytes");
                        physics_filepath = OpenCVForUnity.Utils.getFilePath("shizuku/shizuku.physics.json");
                        pose_filepath = OpenCVForUnity.Utils.getFilePath("shizuku/shizuku.pose.json");
                        for (int i = 0; i < texture_filepath.Length; i++)
                        {
                            texture_filepath[i] = OpenCVForUnity.Utils.getFilePath("shizuku/shizuku.1024/texture_0" + i + ".png");
                        }
                        break;
                    }
                default:
                    {
                        // Load shizuku model file
                        moc_filepath = OpenCVForUnity.Utils.getFilePath("shizuku/shizuku.moc.bytes");
                        physics_filepath = OpenCVForUnity.Utils.getFilePath("shizuku/shizuku.physics.json");
                        pose_filepath = OpenCVForUnity.Utils.getFilePath("shizuku/shizuku.pose.json");
                        for (int i = 0; i < texture_filepath.Length; i++)
                        {
                            texture_filepath[i] = OpenCVForUnity.Utils.getFilePath("shizuku/shizuku.1024/texture_0" + i + ".png");
                        }
                        break;
                    }
            }

            live2DModel.textureFiles = new Texture2D[texture_filepath.Length];
            for (int i = 0; i < texture_filepath.Length; i++)
            {
                if (string.IsNullOrEmpty(texture_filepath[i]))
                    continue;

                Texture2D tex = new Texture2D(2, 2);
                tex.LoadImage(File.ReadAllBytes(texture_filepath[i]));
                live2DModel.textureFiles[i] = tex;
            }
            if (!string.IsNullOrEmpty(moc_filepath))
                live2DModel.setMocFileFromBytes(File.ReadAllBytes(moc_filepath));
            if (!string.IsNullOrEmpty(physics_filepath))
                live2DModel.setPhysicsFileFromBytes(File.ReadAllBytes(physics_filepath));
            if (!string.IsNullOrEmpty(pose_filepath))
                live2DModel.setPoseFileFromBytes(File.ReadAllBytes(pose_filepath));

        }

        private void Run ()
        {
            Debug.Log ("Run");

            faceLandmarkDetector = new FaceLandmarkDetector (shape_predictor_68_face_landmarks_dat_filepath);
            frontalFaceParam = new FrontalFaceParam ();

            // Use the front camera to Init
            webCamTextureToMatHelper.Init(null, webCamTextureToMatHelper.requestWidth, webCamTextureToMatHelper.requestHeight, !webCamTextureToMatHelper.requestIsFrontFacing);

            //// Default initialization
            //webCamTextureToMatHelper.Init();
            //webCamTextureToMatHelper.Init(null, 320, 240, false);
        }

        /// <summary>
        /// Raises the web cam texture to mat helper inited event.
        /// </summary>
        public void OnWebCamTextureToMatHelperInited ()
        {
            Debug.Log ("OnWebCamTextureToMatHelperInited");

            Mat webCamTextureMat = webCamTextureToMatHelper.GetMat ();
            
            gameObject.transform.localScale = new Vector3 (webCamTextureMat.cols (), webCamTextureMat.rows (), 1);

            Debug.Log ("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);

            float width = gameObject.transform.localScale.x;
            float height = gameObject.transform.localScale.y;

            float widthScale = (float)Screen.width / width;
            float heightScale = (float)Screen.height / height;
            if (widthScale < heightScale) {
                Camera.main.orthographicSize = (width * (float)Screen.height / (float)Screen.width) / 2;
            } else {
                Camera.main.orthographicSize = height / 2;
            }

            if (live2DModel != null) {
                // Set live2DModel localScale
                live2DModel.transform.localScale = new Vector3 (Camera.main.orthographicSize * modelScale, Camera.main.orthographicSize * modelScale, 1);
            }
            
        }

        /// <summary>
        /// Raises the web cam texture to mat helper disposed event.
        /// </summary>
        public void OnWebCamTextureToMatHelperDisposed ()
        {
            Debug.Log ("OnWebCamTextureToMatHelperDisposed");

        }

        /// <summary>
        /// Raises the web cam texture to mat helper error occurred event.
        /// </summary>
        public void OnWebCamTextureToMatHelperErrorOccurred ()
        {
            Debug.Log("OnWebCamTextureToMatHelperErrorOccurred");
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

            if (webCamTextureToMatHelper.IsPlaying () && webCamTextureToMatHelper.DidUpdateThisFrame ()) {

                Mat rgbaMat = webCamTextureToMatHelper.GetMat();

                OpenCVForUnityUtils.SetImage(faceLandmarkDetector, rgbaMat);

                List<UnityEngine.Rect> detectResult = faceLandmarkDetector.Detect ();

                foreach (var rect in detectResult) {
                    
                    List<Vector2> points = faceLandmarkDetector.DetectLandmark (rect);

                    if (points.Count > 0) {

                        live2DModelUpdate (points);

                        //currentFacePoints = points;

                        break;
                    }
                }
            }

        }

        private void live2DModelUpdate (List<Vector2> points)
        {

            if (live2DModel != null) {

                // angle
                Vector3 angles = frontalFaceParam.getFrontalFaceAngle (points);
                float rotateX = (angles.x > 180) ? angles.x - 360 : angles.x;
                float rotateY = (angles.y > 180) ? angles.y - 360 : angles.y;
                float rotateZ = (angles.z > 180) ? angles.z - 360 : angles.z;
                // Coordinate transformation
                live2DModel.PARAM_ANGLE.Set(-rotateY, rotateX, -rotateZ);
                //live2DModel.PARAM_ANGLE.Set(-Roundf(rotateY, 0.5f), Roundf(rotateX, 0.5f), -Roundf(rotateZ, 0.5f));
                
                // eye_open_L
                float eyeOpen_L = getRaitoOfEyeOpen_L (points);
                if (eyeOpen_L > 0.6f && eyeOpen_L < 1.1f)
                    eyeOpen_L = 1;
                else if (eyeOpen_L >= 1.1f)
                    eyeOpen_L = 2;
                else if (eyeOpen_L <= 0.6f)
                    eyeOpen_L = 0;
                live2DModel.PARAM_EYE_L_OPEN = eyeOpen_L;

                // eye_open_R
                float eyeOpen_R = getRaitoOfEyeOpen_R (points);
                if (eyeOpen_R > 0.6f && eyeOpen_R < 1.1f)
                    eyeOpen_R = 1;
                else if (eyeOpen_R >= 1.1f)
                    eyeOpen_R = 2;
                else if (eyeOpen_R < 0.6f)
                    eyeOpen_R = 0;
                live2DModel.PARAM_EYE_R_OPEN = eyeOpen_R;

                // Make sure your line of sight is always facing the camera
                // eye_ball_X
                live2DModel.PARAM_EYE_BALL_X = rotateY / 60f;
                // eye_ball_Y
                live2DModel.PARAM_EYE_BALL_Y = -rotateX / 60f - 0.25f;

                // brow_L_Y
                float brow_L_Y = getRaitoOfBROW_L_Y (points);
                // Keep three decimal places to reduce the vibration
                live2DModel.PARAM_BROW_L_Y = Roundf(brow_L_Y, 1000f);
                //live2DModel.PARAM_BROW_L_Y = (float)Math.Round(brow_L_Y, 2);

                // brow_R_Y
                float brow_R_Y = getRaitoOfBROW_R_Y (points);
                // Keep three decimal places to reduce the vibration
                live2DModel.PARAM_BROW_R_Y = Roundf(brow_R_Y, 1000f);
                //live2DModel.PARAM_BROW_R_Y = (float)Math.Round(brow_R_Y, 2);

                // mouth_open
                float mouthOpen = getRaitoOfMouthOpen_Y (points) * 2f;
                if (mouthOpen < 0.6f)
                    mouthOpen = 0;
                live2DModel.PARAM_MOUTH_OPEN_Y = mouthOpen;

                // mouth_size
                float mouthSize = getRaitoOfMouthSize (points);
                live2DModel.PARAM_MOUTH_SIZE = mouthSize;

            }
        }

        // Keep decimal places to reduce the vibration
        private float Roundf(float f, float multiple)
        {
            if (multiple == 0)
                return f;
            int i = (int)(f * multiple);
            return i / multiple;
        }

        // Calculate the degree of eye opening
        private float getRaitoOfEyeOpen_L (List<Vector2> points)
        {
            if (points.Count != 68)
                throw new ArgumentNullException ("Invalid landmark_points.");

            return Mathf.Clamp (Mathf.Abs (points [43].y - points [47].y) / (Mathf.Abs (points [43].x - points [44].x) * 0.75f), -0.1f, 2.0f);
        }

        private float getRaitoOfEyeOpen_R (List<Vector2> points)
        {
            if (points.Count != 68)
                throw new ArgumentNullException ("Invalid landmark_points.");

            return Mathf.Clamp (Mathf.Abs (points [38].y - points [40].y) / (Mathf.Abs (points [37].x - points [38].x) * 0.75f), -0.1f, 2.0f);
        }

        // Eyebrows move up and down
        private float getRaitoOfBROW_L_Y (List<Vector2> points)
        {
            if (points.Count != 68)
                throw new ArgumentNullException ("Invalid landmark_points.");

            //float y = Mathf.Ceil(Mathf.Abs(points[24].y - points[27].y)) / Mathf.Abs (points [27].y - points [29].y);
            float y = Mathf.Abs(points[24].y - points[27].y) / Mathf.Abs(points[27].y - points[29].y);
            y -= 1;
            y *= 4f;
            
            return Mathf.Clamp (y, -1.0f, 1.0f);
        }

        private float getRaitoOfBROW_R_Y (List<Vector2> points)
        {
            if (points.Count != 68)
                throw new ArgumentNullException ("Invalid landmark_points.");

            //float y = Mathf.Ceil(Mathf.Abs(points[19].y - points[27].y)) / Mathf.Abs(points[27].y - points[29].y);
            float y = Mathf.Abs(points[19].y - points[27].y) / Mathf.Abs(points[27].y - points[29].y);
            y -= 1;
            y *= 4f;

            return Mathf.Clamp (y, -1.0f, 1.0f);
        }

        // Calculate the degree of mouth opening
        private float getRaitoOfMouthOpen_Y (List<Vector2> points)
        {
            if (points.Count != 68)
                throw new ArgumentNullException ("Invalid landmark_points.");

            return Mathf.Clamp01 (Mathf.Abs (points [62].y - points [66].y) / (Mathf.Abs (points [51].y - points [62].y) + Mathf.Abs (points [66].y - points [57].y)));
        }

        // Calculate the width of the mouth
        private float getRaitoOfMouthSize (List<Vector2> points)
        {
            if (points.Count != 68)
                throw new ArgumentNullException ("Invalid landmark_points.");

            float size = Mathf.Abs (points [48].x - points [54].x) / (Mathf.Abs (points [31].x - points [35].x) * 1.8f);
            size -= 1;
            size *= 4f;

            return Mathf.Clamp (size, -1.0f, 1.0f);
        }

        /// <summary>
        /// Raises the disable event.
        /// </summary>
        void OnDisable ()
        {
            if(webCamTextureToMatHelper != null) webCamTextureToMatHelper.Dispose ();

            if(faceLandmarkDetector != null) faceLandmarkDetector.Dispose ();

            if(frontalFaceParam != null) frontalFaceParam.Dispose ();
        }

        public void OnBackButton()
        {
#if UNITY_5_3 || UNITY_5_3_OR_NEWER
            SceneManager.LoadScene("FacemojiStart");
#else
            Application.LoadLevel("FacemojiStart");
#endif
        }

        /// <summary>
        /// Raises the change camera button event.
        /// </summary>
        public void OnChangeCameraButton ()
        {
            webCamTextureToMatHelper.Init (null, webCamTextureToMatHelper.requestWidth, webCamTextureToMatHelper.requestHeight, !webCamTextureToMatHelper.requestIsFrontFacing);
        }

        public void OnStartButton()
        {
            startBtn.SetActive(false);
            if (!Record.isRecording)
            {
                //Start recording
                Record.isRecording = true;
                finishBtn.SetActive(true);
            }
        }

        public void OnFinishButton()
        {
            finishBtn.SetActive(false);
            if (Record.isRecording)
            {
                //Finish recording
                Record.isRecording = false;
                startBtn.SetActive(true);
            }
        }

        public void OnShizukuButton()
        {
            if(selectedTexture != WhichTexture.shizuku)
            {
                selectedTexture = WhichTexture.shizuku;
                LoadTexture();
            }
        }

        public void OnHaruButton()
        {
            if (selectedTexture != WhichTexture.haru)
            {
                selectedTexture = WhichTexture.haru;
                LoadTexture();
            }
        }

    }
}
