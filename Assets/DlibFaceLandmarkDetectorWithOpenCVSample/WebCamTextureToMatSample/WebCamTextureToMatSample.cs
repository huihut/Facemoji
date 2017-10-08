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
    /// Face Landmark Detection from WebCamTextureToMat Sample.
    /// </summary>
    [RequireComponent(typeof(WebCamTextureToMatHelper))]
    public class WebCamTextureToMatSample : MonoBehaviour
    {

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

            webCamTextureToMatHelper = gameObject.GetComponent<WebCamTextureToMatHelper> ();
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

                OpenCVForUnityUtils.SetImage (faceLandmarkDetector, rgbaMat);

                //detect face rects
                List<UnityEngine.Rect> detectResult = faceLandmarkDetector.Detect ();
                
                foreach (var rect in detectResult) {

                    //detect landmark points
                    List<Vector2> points = faceLandmarkDetector.DetectLandmark (rect);

                    if (points.Count > 0) {
                        //draw landmark points
                        OpenCVForUnityUtils.DrawFaceLandmark (rgbaMat, points, new Scalar (0, 255, 0, 255), 2);
                    }

                    //draw face rect
                    OpenCVForUnityUtils.DrawFaceRect (rgbaMat, rect, new Scalar (255, 0, 0, 255), 2);
                }

                Imgproc.putText (rgbaMat, "W:" + rgbaMat.width () + " H:" + rgbaMat.height () + " SO:" + Screen.orientation, new Point (5, rgbaMat.rows () - 10), Core.FONT_HERSHEY_SIMPLEX, 0.5, new Scalar (255, 255, 255, 255), 1, Imgproc.LINE_AA, false);

                OpenCVForUnity.Utils.matToTexture2D (rgbaMat, texture, webCamTextureToMatHelper.GetBufferColors ());
            }

        }
    
        /// <summary>
        /// Raises the disable event.
        /// </summary>
        void OnDisable ()
        {
            if(webCamTextureToMatHelper != null)webCamTextureToMatHelper.Dispose ();

            if(faceLandmarkDetector != null)faceLandmarkDetector.Dispose ();
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