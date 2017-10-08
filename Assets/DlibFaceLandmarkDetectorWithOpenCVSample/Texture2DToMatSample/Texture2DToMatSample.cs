using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System;

#if UNITY_5_3 || UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif
using OpenCVForUnity;
using DlibFaceLandmarkDetector;

namespace DlibFaceLandmarkDetectorSample
{
    /// <summary>
    /// Face Landmark Detection from Texture2DToMat Sample.
    /// </summary>
    public class Texture2DToMatSample : MonoBehaviour
    {
        /// <summary>
        /// The image texture.
        /// </summary>
        public Texture2D imgTexture;

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
            Mat imgMat = new Mat (imgTexture.height, imgTexture.width, CvType.CV_8UC4);
            
            OpenCVForUnity.Utils.texture2DToMat (imgTexture, imgMat);
            Debug.Log ("imgMat dst ToString " + imgMat.ToString ());

            gameObject.transform.localScale = new Vector3 (imgTexture.width, imgTexture.height, 1);
            Debug.Log ("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);
            
            float width = imgMat.width ();
            float height = imgMat.height ();
            
            float widthScale = (float)Screen.width / width;
            float heightScale = (float)Screen.height / height;
            if (widthScale < heightScale) {
                Camera.main.orthographicSize = (width * (float)Screen.height / (float)Screen.width) / 2;
            } else {
                Camera.main.orthographicSize = height / 2;
            }


            FaceLandmarkDetector faceLandmarkDetector = new FaceLandmarkDetector (shape_predictor_68_face_landmarks_dat_filepath);

            OpenCVForUnityUtils.SetImage (faceLandmarkDetector, imgMat);

        
            //detect face rectdetecton
            List<FaceLandmarkDetector.RectDetection> detectResult = faceLandmarkDetector.DetectRectDetection ();
                        
            foreach (var result in detectResult) {
                Debug.Log ("rect : " + result.rect);
                Debug.Log ("detection_confidence : " + result.detection_confidence);
                Debug.Log ("weight_index : " + result.weight_index);
            
                //              Debug.Log ("face : " + rect);

                Imgproc.putText (imgMat, "" + result.detection_confidence, new Point (result.rect.xMin, result.rect.yMin - 20), Core.FONT_HERSHEY_SIMPLEX, 0.5, new Scalar (255, 255, 255, 255), 1, Imgproc.LINE_AA, false);
                Imgproc.putText (imgMat, "" + result.weight_index, new Point (result.rect.xMin, result.rect.yMin - 5), Core.FONT_HERSHEY_SIMPLEX, 0.5, new Scalar (255, 255, 255, 255), 1, Imgproc.LINE_AA, false);

                //detect landmark points
                List<Vector2> points = faceLandmarkDetector.DetectLandmark (result.rect);
                                                
                Debug.Log ("face points count : " + points.Count);
                if (points.Count > 0) {
                    //draw landmark points
                    OpenCVForUnityUtils.DrawFaceLandmark (imgMat, points, new Scalar (0, 255, 0, 255), 2);
                
                }

                //draw face rect
                OpenCVForUnityUtils.DrawFaceRect (imgMat, result.rect, new Scalar (255, 0, 0, 255), 2);
            }

            
            faceLandmarkDetector.Dispose ();


            Texture2D texture = new Texture2D (imgMat.cols (), imgMat.rows (), TextureFormat.RGBA32, false);

            OpenCVForUnity.Utils.matToTexture2D (imgMat, texture);

            gameObject.GetComponent<Renderer> ().material.mainTexture = texture;

        }
    
        // Update is called once per frame
        void Update ()
        {
    
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
