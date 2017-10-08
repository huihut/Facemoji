using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

using OpenCVForUnity;
using DlibFaceLandmarkDetector;

namespace DlibFaceLandmarkDetector
{
    /// <summary>
    /// Utility class for the integration of DlibFaceLandmarkDetector and OpenCVForUnity.
    /// </summary>
    public static class OpenCVForUnityUtils
    {

        /// <summary>
        /// Sets the image.
        /// </summary>
        /// <param name="faceLandmarkDetector">Face landmark detector.</param>
        /// <param name="imgMat">Image mat.</param>
        public static void SetImage (FaceLandmarkDetector faceLandmarkDetector, Mat imgMat)
        {
            if (!imgMat.isContinuous ()) {
                throw new ArgumentException ("imgMat.isContinuous() must be true.");
            }
            faceLandmarkDetector.SetImage ((IntPtr)imgMat.dataAddr (), imgMat.width (), imgMat.height (), (int)imgMat.elemSize ());
        }

        /// <summary>
        /// Draws the face rect.
        /// </summary>
        /// <param name="imgMat">Image mat.</param>
        /// <param name="rect">Rect.</param>
        /// <param name="color">Color.</param>
        /// <param name="thickness">Thickness.</param>
        public static void DrawFaceRect (Mat imgMat, UnityEngine.Rect rect, Scalar color, int thickness)
        {
            Imgproc.rectangle (imgMat, new Point (rect.xMin, rect.yMin), new Point (rect.xMax, rect.yMax), color, thickness);
        }

        /// <summary>
        /// Draws the face landmark.
        /// This method supports 68 landmark points.
        /// </summary>
        /// <param name="imgMat">Image mat.</param>
        /// <param name="points">Points.</param>
        /// <param name="color">Color.</param>
        /// <param name="thickness">Thickness.</param>
        public static void DrawFaceLandmark (Mat imgMat, List<Vector2> points, Scalar color, int thickness)
        {
//            //Draw the index number of facelandmark points.
//            for (int i = 0; i < points.Count; i++) {
//                                                                     
//                Imgproc.putText (imgMat, "" + i, new Point (points [i].x, points [i].y), Core.FONT_HERSHEY_SIMPLEX, 0.4, new Scalar (0, 0, 255, 255), 1, Core.LINE_AA, false);
//                                                                      
//            }

            if (points.Count == 68) {
                
                for (int i = 1; i <= 16; ++i)
                    Imgproc.line (imgMat, new Point (points [i].x, points [i].y), new Point (points [i - 1].x, points [i - 1].y), new Scalar (0, 255, 0, 255), thickness);
                
                for (int i = 28; i <= 30; ++i)
                    Imgproc.line (imgMat, new Point (points [i].x, points [i].y), new Point (points [i - 1].x, points [i - 1].y), new Scalar (0, 255, 0, 255), thickness);
                
                for (int i = 18; i <= 21; ++i)
                    Imgproc.line (imgMat, new Point (points [i].x, points [i].y), new Point (points [i - 1].x, points [i - 1].y), new Scalar (0, 255, 0, 255), thickness);
                for (int i = 23; i <= 26; ++i)
                    Imgproc.line (imgMat, new Point (points [i].x, points [i].y), new Point (points [i - 1].x, points [i - 1].y), new Scalar (0, 255, 0, 255), thickness);
                for (int i = 31; i <= 35; ++i)
                    Imgproc.line (imgMat, new Point (points [i].x, points [i].y), new Point (points [i - 1].x, points [i - 1].y), new Scalar (0, 255, 0, 255), thickness);
                Imgproc.line (imgMat, new Point (points [30].x, points [30].y), new Point (points [35].x, points [35].y), new Scalar (0, 255, 0, 255), thickness);
                
                for (int i = 37; i <= 41; ++i)
                    Imgproc.line (imgMat, new Point (points [i].x, points [i].y), new Point (points [i - 1].x, points [i - 1].y), new Scalar (0, 255, 0, 255), thickness);
                Imgproc.line (imgMat, new Point (points [36].x, points [36].y), new Point (points [41].x, points [41].y), new Scalar (0, 255, 0, 255), thickness);
                
                for (int i = 43; i <= 47; ++i)
                    Imgproc.line (imgMat, new Point (points [i].x, points [i].y), new Point (points [i - 1].x, points [i - 1].y), new Scalar (0, 255, 0, 255), thickness);
                Imgproc.line (imgMat, new Point (points [42].x, points [42].y), new Point (points [47].x, points [47].y), new Scalar (0, 255, 0, 255), thickness);
                
                for (int i = 49; i <= 59; ++i)
                    Imgproc.line (imgMat, new Point (points [i].x, points [i].y), new Point (points [i - 1].x, points [i - 1].y), new Scalar (0, 255, 0, 255), thickness);
                Imgproc.line (imgMat, new Point (points [48].x, points [48].y), new Point (points [59].x, points [59].y), new Scalar (0, 255, 0, 255), thickness);
                
                for (int i = 61; i <= 67; ++i)
                    Imgproc.line (imgMat, new Point (points [i].x, points [i].y), new Point (points [i - 1].x, points [i - 1].y), new Scalar (0, 255, 0, 255), thickness);
                Imgproc.line (imgMat, new Point (points [60].x, points [60].y), new Point (points [67].x, points [67].y), new Scalar (0, 255, 0, 255), thickness);
            } else {
                for (int i = 0; i < points.Count; i++) {
                    Imgproc.circle (imgMat, new Point (points [i].x, points [i].y), 2, new Scalar (0, 255, 0, 255), -1);
                }
            }
        }
    }
}
