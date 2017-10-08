using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;
using System.IO;

#if UNITY_5_3 || UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif
using OpenCVForUnity;
using DlibFaceLandmarkDetector;
using DlibFaceLandmarkDetectorSample;

namespace Facemoji
{
    /// <summary>
    /// WebCamTexture sample using Dlib face landmark detection and Live2D SDK.
    /// </summary>
    [RequireComponent (typeof(WebCamTextureToMatHelper))]
    public class WebCamTextureLive2DSample : MonoBehaviour
    {
	
        /// <summary>
        /// The colors.
        /// </summary>
        Color32[] colors;

        /// <summary>
        /// The texture.
        /// </summary>
        Texture2D texture;

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
        /// Currently a series of facial features.
        /// </summary>
        private List<Vector2> currentFacePoints;

        /// <summary>
        /// Hide the camera image.
        /// </summary>
        private bool isHideCameraImage = true;

        /// <summary>
        /// The shape_predictor_68_face_landmarks_dat_filepath.
        /// </summary>
        private string shape_predictor_68_face_landmarks_dat_filepath;

        /// <summary>
        /// Model file path.
        /// </summary>
        private string shizuku_moc_filepath;
        private string shizuku_physics_filepath;
        private string shizuku_pose_filepath;
        private string[] texture_filepath = new string[6];


        // Use this for initialization
        void Start ()
        {
            webCamTextureToMatHelper = gameObject.GetComponent<WebCamTextureToMatHelper> ();

#if UNITY_WEBGL && !UNITY_EDITOR
            webCamTextureToMatHelper.flipHorizontal = true;
            StartCoroutine(getFilePathCoroutine());
#else
            // FaceLandmark model filepath
            shape_predictor_68_face_landmarks_dat_filepath = DlibFaceLandmarkDetector.Utils.getFilePath ("shape_predictor_68_face_landmarks.dat");

            // Facemoji model filepath
            shizuku_moc_filepath = OpenCVForUnity.Utils.getFilePath ("shizuku/shizuku.moc.bytes");
            shizuku_physics_filepath = OpenCVForUnity.Utils.getFilePath ("shizuku/shizuku.physics.json");
            shizuku_pose_filepath = OpenCVForUnity.Utils.getFilePath ("shizuku/shizuku.pose.json");
            for (int i = 0; i < texture_filepath.Length; i++) {
                texture_filepath [i] = OpenCVForUnity.Utils.getFilePath ("shizuku/shizuku.1024/texture_0" + i + ".png");
            }

            Run ();
            #endif
        }

        private IEnumerator getFilePathCoroutine ()
        {
            var getFilePathAsync_shape_predictor_68_face_landmarks_dat_filepath_Coroutine = StartCoroutine (DlibFaceLandmarkDetector.Utils.getFilePathAsync ("shape_predictor_68_face_landmarks.dat", (result) => {
                shape_predictor_68_face_landmarks_dat_filepath = result;
            }));
            var getFilePathAsync_shizuku_moc_filepath_Coroutine = StartCoroutine (DlibFaceLandmarkDetector.Utils.getFilePathAsync ("shizuku/shizuku.moc.bytes", (result) => {
                shizuku_moc_filepath = result;
            }));
            var getFilePathAsync_shizuku_physics_filepath_Coroutine = StartCoroutine (DlibFaceLandmarkDetector.Utils.getFilePathAsync ("shizuku/shizuku.physics.json", (result) => {
                shizuku_physics_filepath = result;
            }));
            var getFilePathAsync_shizuku_pose_filepath_Coroutine = StartCoroutine (DlibFaceLandmarkDetector.Utils.getFilePathAsync ("shizuku/shizuku.pose.json", (result) => {
                shizuku_pose_filepath = result;
            }));

            yield return getFilePathAsync_shape_predictor_68_face_landmarks_dat_filepath_Coroutine;
            yield return getFilePathAsync_shizuku_moc_filepath_Coroutine;
            yield return getFilePathAsync_shizuku_physics_filepath_Coroutine;
            yield return getFilePathAsync_shizuku_pose_filepath_Coroutine;

            for (int i = 0; i < texture_filepath.Length; i++) {
                Debug.Log ("tex");
                yield return StartCoroutine (DlibFaceLandmarkDetector.Utils.getFilePathAsync ("shizuku/shizuku.1024/texture_0" + i + ".png", (result) => {
                    texture_filepath [i] = result;
                }));
            }

            Run ();
        }

        private void Run ()
        {
            Debug.Log ("Run");

            // Load the textureFiles
            live2DModel.textureFiles = new Texture2D[texture_filepath.Length];
            for (int i = 0; i < texture_filepath.Length; i++) {
                if (string.IsNullOrEmpty (texture_filepath [i]))
                    continue;

                Texture2D tex = new Texture2D (2, 2);
                tex.LoadImage (File.ReadAllBytes (texture_filepath [i]));
                live2DModel.textureFiles [i] = tex;
            }
            if (!string.IsNullOrEmpty (shizuku_moc_filepath))
                live2DModel.setMocFileFromBytes (File.ReadAllBytes (shizuku_moc_filepath));
            if (!string.IsNullOrEmpty (shizuku_physics_filepath))
                live2DModel.setPhysicsFileFromBytes (File.ReadAllBytes (shizuku_physics_filepath));
            if (!string.IsNullOrEmpty (shizuku_pose_filepath))
                live2DModel.setPoseFileFromBytes (File.ReadAllBytes (shizuku_pose_filepath));
            

            faceLandmarkDetector = new FaceLandmarkDetector (shape_predictor_68_face_landmarks_dat_filepath);
            frontalFaceParam = new FrontalFaceParam ();

            webCamTextureToMatHelper.Init(null, webCamTextureToMatHelper.requestWidth, webCamTextureToMatHelper.requestHeight, !webCamTextureToMatHelper.requestIsFrontFacing);
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

            colors = new Color32[webCamTextureMat.cols () * webCamTextureMat.rows ()];
            texture = new Texture2D (webCamTextureMat.cols (), webCamTextureMat.rows (), TextureFormat.RGBA32, false);

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
                live2DModel.transform.localScale = new Vector3 (Camera.main.orthographicSize, Camera.main.orthographicSize, 1);
            }

            // Renderer: makes texture appear on the screen
            //gameObject.GetComponent<Renderer> ().material.mainTexture = texture;

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
            //Debug.Log ("OnWebCamTextureToMatHelperErrorOccurred " + webCamTextureToMatHelper.GetErrorCode());

            Debug.Log("OnWebCamTextureToMatHelperErrorOccurred");
        }

        // Update is called once per frame
        void Update ()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
#if UNITY_5_3 || UNITY_5_3_OR_NEWER
                SceneManager.LoadScene("DlibFaceLandmarkDetectorWithLive2DSample");
#else
            Application.LoadLevel("DlibFaceLandmarkDetectorWithLive2DSample");
#endif  
            }

            if (webCamTextureToMatHelper.IsPlaying () && webCamTextureToMatHelper.DidUpdateThisFrame ()) {

                Mat rgbaMat = webCamTextureToMatHelper.GetMat ();

                OpenCVForUnityUtils.SetImage (faceLandmarkDetector, rgbaMat);

                List<UnityEngine.Rect> detectResult = faceLandmarkDetector.Detect ();

                foreach (var rect in detectResult) {

                    OpenCVForUnityUtils.DrawFaceRect (rgbaMat, rect, new Scalar (255, 0, 0, 255), 2);

                    List<Vector2> points = faceLandmarkDetector.DetectLandmark (rect);

                    if (points.Count > 0) {

                        live2DModelUpdate (points);

                        currentFacePoints = points;

                        break;
                    }
                }



                if (isHideCameraImage)
                    Imgproc.rectangle (rgbaMat, new Point (0, 0), new Point (rgbaMat.width (), rgbaMat.height ()), new Scalar (0, 0, 0, 255), -1);

                if (currentFacePoints != null)
                    OpenCVForUnityUtils.DrawFaceLandmark (rgbaMat, currentFacePoints, new Scalar (0, 255, 0, 255), 2);

                // Mat size
                Imgproc.putText (rgbaMat, "W:" + rgbaMat.width () + " H:" + rgbaMat.height () + " SO:" + Screen.orientation, new Point (5, rgbaMat.rows () - 10), Core.FONT_HERSHEY_SIMPLEX, 0.5, new Scalar (255, 255, 255, 255), 1, Imgproc.LINE_AA, false);

                OpenCVForUnity.Utils.matToTexture2D (rgbaMat, texture, colors);
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
                live2DModel.PARAM_ANGLE.Set (-rotateY, rotateX, -rotateZ);


                // eye_open_L
                float eyeOpen_L = getRaitoOfEyeOpen_L (points);
                if (eyeOpen_L > 0.8f && eyeOpen_L < 1.1f)
                    eyeOpen_L = 1;
                if (eyeOpen_L >= 1.1f)
                    eyeOpen_L = 2;
                if (eyeOpen_L < 0.7f)
                    eyeOpen_L = 0;
                live2DModel.PARAM_EYE_L_OPEN = eyeOpen_L;
                
                // eye_open_R
                float eyeOpen_R = getRaitoOfEyeOpen_R (points);
                if (eyeOpen_R > 0.8f && eyeOpen_R < 1.1f)
                    eyeOpen_R = 1;
                if (eyeOpen_R >= 1.1f)
                    eyeOpen_R = 2;
                if (eyeOpen_R < 0.7f)
                    eyeOpen_R = 0;
                live2DModel.PARAM_EYE_R_OPEN = eyeOpen_R;

                // Make sure your line of sight is always facing the camera
                // eye_ball_X
                live2DModel.PARAM_EYE_BALL_X = rotateY / 60f;
                // eye_ball_Y
                live2DModel.PARAM_EYE_BALL_Y = -rotateX / 60f - 0.25f;

                // brow_L_Y
                float brow_L_Y = getRaitoOfBROW_L_Y (points);
                live2DModel.PARAM_BROW_L_Y = brow_L_Y;

                // brow_R_Y
                float brow_R_Y = getRaitoOfBROW_R_Y (points);
                live2DModel.PARAM_BROW_R_Y = brow_R_Y;

                // mouth_open
                float mouthOpen = getRaitoOfMouthOpen_Y (points) * 2f;
                if (mouthOpen < 0.3f)
                    mouthOpen = 0;
                live2DModel.PARAM_MOUTH_OPEN_Y = mouthOpen;

                // mouth_size
                float mouthSize = getRaitoOfMouthSize (points);
                live2DModel.PARAM_MOUTH_SIZE = mouthSize;

            }
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

            float y = Mathf.Abs (points [24].y - points [27].y) / Mathf.Abs (points [27].y - points [29].y);
            y -= 1;
            y *= 4f;

            return Mathf.Clamp (y, -1.0f, 1.0f);
        }

        private float getRaitoOfBROW_R_Y (List<Vector2> points)
        {
            if (points.Count != 68)
                throw new ArgumentNullException ("Invalid landmark_points.");

            float y = Mathf.Abs (points [19].y - points [27].y) / Mathf.Abs (points [27].y - points [29].y);
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

        /// <summary>
        /// Raises the back button event.
        /// </summary>
//        public void OnBackButton ()
//        {
//            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
//            SceneManager.LoadScene ("DlibFaceLandmarkDetectorWithLive2DSample");
//#else
//            Application.LoadLevel("DlibFaceLandmarkDetectorWithLive2DSample");
//#endif
//        }

        /// <summary>
        /// Raises the play button event.
        /// </summary>
        //public void OnPlayButton ()
        //{
        //    webCamTextureToMatHelper.Play ();
        //}

        /// <summary>
        /// Raises the pause button event.
        /// </summary>
        //public void OnPauseButton ()
        //{
        //    webCamTextureToMatHelper.Pause ();
        //}

        /// <summary>
        /// Raises the stop button event.
        /// </summary>
        //public void OnStopButton ()
        //{
        //    webCamTextureToMatHelper.Stop ();
        //}

        /// <summary>
        /// Raises the change camera button event.
        /// </summary>
        public void OnChangeCameraButton ()
        {
            webCamTextureToMatHelper.Init (null, webCamTextureToMatHelper.requestWidth, webCamTextureToMatHelper.requestHeight, !webCamTextureToMatHelper.requestIsFrontFacing);
        }

        /// <summary>
        /// Raises the hide camera image toggle event.
        /// </summary>
        //public void OnHideCameraImageToggle ()
        //{
        //    if (isHideCameraImage) {
        //        isHideCameraImage = false;
        //    } else {
        //        isHideCameraImage = true;
        //    }
        //}

        public void OnStartButton()
        {
            Global.isStartRecord = true;
        }

        public void OnFinishButton()
        {
            Global.isStartRecord = false;
        }

        public void OnSaveButton()
        {

        }


    }
}