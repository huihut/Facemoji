using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.InteropServices;

#if UNITY_5_3 || UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif
using OpenCVForUnity;
using DlibFaceLandmarkDetector;

namespace DlibFaceLandmarkDetectorSample
{
    /// <summary>
    /// OptimizationSample (Resize Frame+Skip frame)
    /// http://www.learnopencv.com/speeding-up-dlib-facial-landmark-detector/
    /// </summary>
    [RequireComponent(typeof(OptimizationWebCamTextureToMatHelper))]
    public class OptimizationSample : MonoBehaviour
    {

        /// <summary>
        /// The texture.
        /// </summary>
        Texture2D texture;

        /// <summary>
        /// The web cam texture to mat helper.
        /// </summary>
        OptimizationWebCamTextureToMatHelper webCamTextureToMatHelper;

        /// <summary>
        /// The face landmark detector.
        /// </summary>
        FaceLandmarkDetector faceLandmarkDetector;

        /// <summary>
        /// The detect result.
        /// </summary>
        List<UnityEngine.Rect> detectResult;

        /// <summary>
        /// The shape_predictor_68_face_landmarks_dat_filepath.
        /// </summary>
        private string shape_predictor_68_face_landmarks_dat_filepath;

        // Use this for initialization
        void Start ()
        {
            #if UNITY_WEBGL && !UNITY_EDITOR
            StartCoroutine(DlibFaceLandmarkDetector.Utils.getFilePathAsync("shape_predictor_68_face_landmarks.dat", (result) => {
                shape_predictor_68_face_landmarks_dat_filepath = result;
                Run ();
            }));
            #else
            shape_predictor_68_face_landmarks_dat_filepath = DlibFaceLandmarkDetector.Utils.getFilePath ("shape_predictor_68_face_landmarks.dat");
            Run ();
            #endif
        }

        private void Run ()
        {
            faceLandmarkDetector = new FaceLandmarkDetector (shape_predictor_68_face_landmarks_dat_filepath);

            webCamTextureToMatHelper = gameObject.GetComponent<OptimizationWebCamTextureToMatHelper> ();
            webCamTextureToMatHelper.Init ();
        }

        /// <summary>
        /// Raises the web cam texture to mat helper inited event.
        /// </summary>
        public void OnWebCamTextureToMatHelperInited ()
        {
            Debug.Log ("OnWebCamTextureToMatHelperInited");

            Mat webCamTextureMat = webCamTextureToMatHelper.GetMat ();

            texture = new Texture2D (webCamTextureMat.cols (), webCamTextureMat.rows (), TextureFormat.RGBA32, false);

            gameObject.GetComponent<Renderer> ().material.mainTexture = texture;

            gameObject.transform.localScale = new Vector3 (webCamTextureMat.cols (), webCamTextureMat.rows (), 1);
            Debug.Log ("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);
                                    
            float width = webCamTextureMat.width ();
            float height = webCamTextureMat.height ();
                                    
            float widthScale = (float)Screen.width / width;
            float heightScale = (float)Screen.height / height;
            if (widthScale < heightScale) {
                Camera.main.orthographicSize = (width * (float)Screen.height / (float)Screen.width) / 2;
            } else {
                Camera.main.orthographicSize = height / 2;
            }

            detectResult = new List<UnityEngine.Rect> ();
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
        /// <param name="errorCode">Error code.</param>
        public void OnWebCamTextureToMatHelperErrorOccurred(WebCamTextureToMatHelper.ErrorCode errorCode){
            Debug.Log ("OnWebCamTextureToMatHelperErrorOccurred " + errorCode);
        }

        // Update is called once per frame
        void Update ()
        {

            if (webCamTextureToMatHelper.IsPlaying () && webCamTextureToMatHelper.DidUpdateThisFrame ()) {

                Mat rgbaMat = webCamTextureToMatHelper.GetMat ();

                Mat downScaleRgbaMat = webCamTextureToMatHelper.GetDownScaleMat(rgbaMat);

                OpenCVForUnityUtils.SetImage (faceLandmarkDetector, downScaleRgbaMat);

                // Detect faces on resize image
                if (!webCamTextureToMatHelper.IsSkipFrame()) {
                    //detect face rects
                    detectResult = faceLandmarkDetector.Detect ();
                }

                int DOWNSCALE_RATIO = webCamTextureToMatHelper.DOWNSCALE_RATIO;
                
                foreach (var rect in detectResult) {

                    //detect landmark points
                    List<Vector2> points = faceLandmarkDetector.DetectLandmark (rect);

                    if (points.Count > 0) {
                        List<Vector2> originalPoints = new List<Vector2> (points.Count);
                        foreach (var point in points) {
                            originalPoints.Add (new Vector2 (point.x * DOWNSCALE_RATIO, point.y * DOWNSCALE_RATIO));
                        }

                        //draw landmark points
                        OpenCVForUnityUtils.DrawFaceLandmark (rgbaMat, originalPoints, new Scalar (0, 255, 0, 255), 2);
                    }

                    UnityEngine.Rect originalRect = new UnityEngine.Rect (rect.x * DOWNSCALE_RATIO, rect.y * DOWNSCALE_RATIO, rect.width * DOWNSCALE_RATIO, rect.height * DOWNSCALE_RATIO);
                    //draw face rect
                    OpenCVForUnityUtils.DrawFaceRect (rgbaMat, originalRect, new Scalar (255, 0, 0, 255), 2);
                }

                Imgproc.putText (rgbaMat, "Original: (" + rgbaMat.width () + "," + rgbaMat.height () + ") DownScale; (" + downScaleRgbaMat.width () + "," + downScaleRgbaMat.height () + ") SkipFrames: " + webCamTextureToMatHelper.SKIP_FRAMES, new Point (5, rgbaMat.rows () - 10), Core.FONT_HERSHEY_SIMPLEX, 1.0, new Scalar (255, 255, 255, 255), 2, Imgproc.LINE_AA, false);

                OpenCVForUnity.Utils.matToTexture2D (rgbaMat, texture, webCamTextureToMatHelper.GetBufferColors ());

            }

        }
    
        /// <summary>
        /// Raises the disable event.
        /// </summary>
        void OnDisable ()
        {
            if (webCamTextureToMatHelper != null)
                webCamTextureToMatHelper.Dispose ();

            if (faceLandmarkDetector != null)
                faceLandmarkDetector.Dispose ();
        }

        /// <summary>
        /// Raises the back button event.
        /// </summary>
        public void OnBackButton ()
        {
            #if UNITY_5_3 || UNITY_5_3_OR_NEWER
            SceneManager.LoadScene ("DlibFaceLandmarkDetectorSample");
            #else
            Application.LoadLevel ("DlibFaceLandmarkDetectorSample");
            #endif
        }

        /// <summary>
        /// Raises the play button event.
        /// </summary>
        public void OnPlayButton ()
        {
            webCamTextureToMatHelper.Play ();
        }

        /// <summary>
        /// Raises the pause button event.
        /// </summary>
        public void OnPauseButton ()
        {
            webCamTextureToMatHelper.Pause ();
        }

        /// <summary>
        /// Raises the stop button event.
        /// </summary>
        public void OnStopButton ()
        {
            webCamTextureToMatHelper.Stop ();
        }

        /// <summary>
        /// Raises the change camera button event.
        /// </summary>
        public void OnChangeCameraButton ()
        {
            webCamTextureToMatHelper.Init (null, webCamTextureToMatHelper.requestWidth, webCamTextureToMatHelper.requestHeight, !webCamTextureToMatHelper.requestIsFrontFacing);
        }
        
    }
}