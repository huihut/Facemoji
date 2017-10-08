using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_5_3 || UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif
using OpenCVForUnity;
using DlibFaceLandmarkDetector;

namespace DlibFaceLandmarkDetectorSample
{
    /// <summary>
    /// Face Landmark Detection from VideoCapture Sample.
    /// </summary>
    public class VideoCaptureSample : MonoBehaviour
    {

        /// <summary>
        /// The capture.
        /// </summary>
        VideoCapture capture;

        /// <summary>
        /// The rgb mat.
        /// </summary>
        Mat rgbMat;

        /// <summary>
        /// The colors.
        /// </summary>
        Color32[] colors;

        /// <summary>
        /// The texture.
        /// </summary>
        Texture2D texture;

        /// <summary>
        /// The face landmark detector.
        /// </summary>
        FaceLandmarkDetector faceLandmarkDetector;

        /// <summary>
        /// The shape_predictor_68_face_landmarks_dat_filepath.
        /// </summary>
        private string shape_predictor_68_face_landmarks_dat_filepath;

        /// <summary>
        /// The couple_avi_filepath.
        /// </summary>
        private string couple_avi_filepath;

        // Use this for initialization
        void Start ()
        {
            #if UNITY_WEBGL && !UNITY_EDITOR
            StartCoroutine(getFilePathCoroutine());
            #else
            shape_predictor_68_face_landmarks_dat_filepath = DlibFaceLandmarkDetector.Utils.getFilePath ("shape_predictor_68_face_landmarks.dat");
            couple_avi_filepath = OpenCVForUnity.Utils.getFilePath ("couple.avi");
            Run ();
            #endif
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        private IEnumerator getFilePathCoroutine ()
        {
            var getFilePathAsync_shape_predictor_68_face_landmarks_dat_filepath_Coroutine = StartCoroutine (DlibFaceLandmarkDetector.Utils.getFilePathAsync ("shape_predictor_68_face_landmarks.dat", (result) => {
                shape_predictor_68_face_landmarks_dat_filepath = result;
            }));
            var getFilePathAsync_couple_avi_filepath_Coroutine = StartCoroutine (DlibFaceLandmarkDetector.Utils.getFilePathAsync ("couple.avi", (result) => {
                couple_avi_filepath = result;
            }));
            
            yield return getFilePathAsync_shape_predictor_68_face_landmarks_dat_filepath_Coroutine;
            yield return getFilePathAsync_couple_avi_filepath_Coroutine;
            
            Run ();
        }
#endif

        private void Run ()
        {
            faceLandmarkDetector = new FaceLandmarkDetector (shape_predictor_68_face_landmarks_dat_filepath);
            
            rgbMat = new Mat ();
            
            capture = new VideoCapture ();
            capture.open (couple_avi_filepath);
            
            if (capture.isOpened ()) {
                Debug.Log ("capture.isOpened() true");
            } else {
                Debug.Log ("capture.isOpened() false");
            }


            Debug.Log ("CAP_PROP_FORMAT: " + capture.get (Videoio.CAP_PROP_FORMAT));
            Debug.Log ("CV_CAP_PROP_PREVIEW_FORMAT: " + capture.get (Videoio.CV_CAP_PROP_PREVIEW_FORMAT));
            Debug.Log ("CAP_PROP_POS_MSEC: " + capture.get (Videoio.CAP_PROP_POS_MSEC));
            Debug.Log ("CAP_PROP_POS_FRAMES: " + capture.get (Videoio.CAP_PROP_POS_FRAMES));
            Debug.Log ("CAP_PROP_POS_AVI_RATIO: " + capture.get (Videoio.CAP_PROP_POS_AVI_RATIO));
            Debug.Log ("CAP_PROP_FRAME_COUNT: " + capture.get (Videoio.CAP_PROP_FRAME_COUNT));
            Debug.Log ("CAP_PROP_FPS: " + capture.get (Videoio.CAP_PROP_FPS));
            Debug.Log ("CAP_PROP_FRAME_WIDTH: " + capture.get (Videoio.CAP_PROP_FRAME_WIDTH));
            Debug.Log ("CAP_PROP_FRAME_HEIGHT: " + capture.get (Videoio.CAP_PROP_FRAME_HEIGHT));

            capture.grab ();
            capture.retrieve (rgbMat, 0);
            int frameWidth = rgbMat.cols ();
            int frameHeight = rgbMat.rows ();
            colors = new Color32[frameWidth * frameHeight];
            texture = new Texture2D (frameWidth, frameHeight, TextureFormat.RGBA32, false);
            gameObject.transform.localScale = new Vector3 ((float)frameWidth, (float)frameHeight, 1);
            float widthScale = (float)Screen.width / (float)frameWidth;
            float heightScale = (float)Screen.height / (float)frameHeight;
            if (widthScale < heightScale) {
                Camera.main.orthographicSize = ((float)frameWidth * (float)Screen.height / (float)Screen.width) / 2;
            } else {
                Camera.main.orthographicSize = (float)frameHeight / 2;
            }
            capture.set (Videoio.CAP_PROP_POS_FRAMES, 0);

            
            gameObject.GetComponent<Renderer> ().material.mainTexture = texture;
        }
        
        // Update is called once per frame
        void Update ()
        {
            if (capture == null)
                return;

            //Loop play
            if (capture.get (Videoio.CAP_PROP_POS_FRAMES) >= capture.get (Videoio.CAP_PROP_FRAME_COUNT))
                capture.set (Videoio.CAP_PROP_POS_FRAMES, 0);

            //error PlayerLoop called recursively! on iOS.reccomend WebCamTexture.
            if (capture.grab ()) {

                capture.retrieve (rgbMat, 0);

                Imgproc.cvtColor (rgbMat, rgbMat, Imgproc.COLOR_BGR2RGB);
                //Debug.Log ("Mat toString " + rgbMat.ToString ());


                OpenCVForUnityUtils.SetImage (faceLandmarkDetector, rgbMat);
                
                //detect face rects
                List<UnityEngine.Rect> detectResult = faceLandmarkDetector.Detect ();
                
                foreach (var rect in detectResult) {

                    //detect landmark points
                    List<Vector2> points = faceLandmarkDetector.DetectLandmark (rect);
                    
                    if (points.Count > 0) {
                        //draw landmark points
                        OpenCVForUnityUtils.DrawFaceLandmark (rgbMat, points, new Scalar (0, 255, 0), 2);
                    }

                    //draw face rect
                    OpenCVForUnityUtils.DrawFaceRect (rgbMat, rect, new Scalar (255, 0, 0), 2);
                }
                
                Imgproc.putText (rgbMat, "W:" + rgbMat.width () + " H:" + rgbMat.height () + " SO:" + Screen.orientation, new Point (5, rgbMat.rows () - 10), Core.FONT_HERSHEY_SIMPLEX, 0.5, new Scalar (255, 255, 255), 1, Imgproc.LINE_AA, false);

                OpenCVForUnity.Utils.matToTexture2D (rgbMat, texture, colors);
                
            }
        }
        
        void OnDestroy ()
        {
            if (capture != null)
                capture.release ();

            if (rgbMat != null)
                rgbMat.Dispose ();

            if (faceLandmarkDetector != null)
                faceLandmarkDetector.Dispose ();
        }
        
        public void OnBackButton ()
        {
            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
            SceneManager.LoadScene ("DlibFaceLandmarkDetectorSample");
            #else
            Application.LoadLevel ("DlibFaceLandmarkDetectorSample");
            #endif
        }
    }
    
}