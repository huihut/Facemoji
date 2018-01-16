using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using DlibFaceLandmarkDetector;
using OpenCVForUnity;

namespace HuiHut.Facemoji
{
    public class FrontalFaceParam : IDisposable
    {

        //Calib3d.solvePnP
        Matrix4x4 transformationM = new Matrix4x4 ();
        MatOfPoint3f objectPoints;
        MatOfPoint2f imagePoints;
        Mat rvec;
        Mat tvec;
        Mat rotM;
        Mat camMatrix;
        MatOfDouble distCoeffs;
        Matrix4x4 invertYM;
        Matrix4x4 invertZM;
        Matrix4x4 ARM;

        //normalize
        float normWidth = 200;
        float normHeight = 200;
        List<Vector2> normPoints;

        public FrontalFaceParam ()
        {
            invertYM = Matrix4x4.TRS (Vector3.zero, Quaternion.identity, new Vector3 (1, -1, 1));
            //Debug.Log ("invertYM " + invertYM.ToString ());

            invertZM = Matrix4x4.TRS (Vector3.zero, Quaternion.identity, new Vector3 (1, 1, -1));
            //Debug.Log ("invertZM " + invertZM.ToString ());

            //set 3d face object points.
            objectPoints = new MatOfPoint3f (
                new Point3 (-31, 72, 86),//l eye
                new Point3 (31, 72, 86),//r eye
                new Point3 (0, 40, 114),//nose
                new Point3 (-20, 15, 90),//l mouth //new Point3(-22, 17, 90),//l mouth
                new Point3 (20, 15, 90),//r mouth //new Point3(22, 17, 90),//r mouth
                new Point3 (-69, 76, -2),//l ear
                new Point3 (69, 76, -2)//r ear
            );
            imagePoints = new MatOfPoint2f ();
            rvec = new Mat ();
            tvec = new Mat ();
            rotM = new Mat (3, 3, CvType.CV_64FC1);


            float max_d = Mathf.Max (normHeight, normWidth);
            camMatrix = new Mat (3, 3, CvType.CV_64FC1);
            camMatrix.put (0, 0, max_d);
            camMatrix.put (0, 1, 0);
            camMatrix.put (0, 2, normWidth / 2.0f);
            camMatrix.put (1, 0, 0);
            camMatrix.put (1, 1, max_d);
            camMatrix.put (1, 2, normHeight / 2.0f);
            camMatrix.put (2, 0, 0);
            camMatrix.put (2, 1, 0);
            camMatrix.put (2, 2, 1.0f);

            distCoeffs = new MatOfDouble (0, 0, 0, 0);
            //Debug.Log("distCoeffs " + distCoeffs.dump());


            normPoints = new List<Vector2> (68);
            for (int i = 0; i < 68; i++) {
                normPoints.Add (new Vector2 (0, 0));
            }
        }

        public void Dispose ()
        {
            objectPoints.Dispose ();
            imagePoints.Dispose ();
            rvec.Dispose ();
            tvec.Dispose ();
            rotM.Dispose ();
            camMatrix.Dispose ();
            distCoeffs.Dispose ();
        }

        /// <summary>
        /// Gets the frontal face angle.
        /// </summary>
        /// <returns>The frontal face angle.</returns>
        /// <param name="points">Points.</param>
        public Vector3 getFrontalFaceAngle (List<Vector2> points)
        {
            if (points.Count != 68)
                throw new ArgumentNullException ("Invalid landmark_points.");

            if (camMatrix == null)
                throw new ArgumentNullException ("Invalid camMatrix.");

            //normalize

            float normScale = Math.Abs (points [30].y - points [8].y) / (normHeight / 2);
            Vector2 normDiff = points [30] * normScale - new Vector2 (normWidth / 2, normHeight / 2);

            for (int i = 0; i < points.Count; i++) {
                normPoints [i] = points [i] * normScale - normDiff;
            }
            //Debug.Log ("points[30] " + points[30]);
            //Debug.Log ("normPoints[30] " + normPoints[30]);
            //Debug.Log ("normScale " + normScale);
            //Debug.Log ("normDiff " + normDiff);

            imagePoints.fromArray (
                new Point ((normPoints [38].x + normPoints [41].x) / 2, (normPoints [38].y + normPoints [41].y) / 2),//l eye
                new Point ((normPoints [43].x + normPoints [46].x) / 2, (normPoints [43].y + normPoints [46].y) / 2),//r eye
                new Point (normPoints [33].x, normPoints [33].y),//nose
                new Point (normPoints [48].x, normPoints [48].y),//l mouth
                new Point (normPoints [54].x, normPoints [54].y), //r mouth                         ,
                new Point (normPoints [0].x, normPoints [0].y),//l ear
                new Point (normPoints [16].x, normPoints [16].y)//r ear
            );


            Calib3d.solvePnP (objectPoints, imagePoints, camMatrix, distCoeffs, rvec, tvec);

            Calib3d.Rodrigues (rvec, rotM);

            transformationM.SetRow (0, new Vector4 ((float)rotM.get (0, 0) [0], (float)rotM.get (0, 1) [0], (float)rotM.get (0, 2) [0], (float)tvec.get (0, 0) [0]));
            transformationM.SetRow (1, new Vector4 ((float)rotM.get (1, 0) [0], (float)rotM.get (1, 1) [0], (float)rotM.get (1, 2) [0], (float)tvec.get (1, 0) [0]));
            transformationM.SetRow (2, new Vector4 ((float)rotM.get (2, 0) [0], (float)rotM.get (2, 1) [0], (float)rotM.get (2, 2) [0], (float)tvec.get (2, 0) [0]));
            transformationM.SetRow (3, new Vector4 (0, 0, 0, 1));

            ARM = invertYM * transformationM * invertZM;

            return ExtractRotationFromMatrix (ref ARM).eulerAngles;
        }

        /// <summary>
        /// Gets the frontal face rate.
        /// </summary>
        /// <returns>The frontal face rate.</returns>
        /// <param name="points">Points.</param>
        public float getFrontalFaceRate (List<Vector2> points)
        {

            Vector3 angles = getFrontalFaceAngle (points);
            float rotateX = (angles.x > 180) ? angles.x - 360 : angles.x;
            float rotateY = (angles.y > 180) ? angles.y - 360 : angles.y;

            //Debug.Log ("angles " + angles);
            //Debug.Log ("ratio " + (1.0f - (Mathf.Max (Mathf.Abs (rotateX), Mathf.Abs (rotateY)) / 90)));

            return 1.0f - (Mathf.Max (Mathf.Abs (rotateX), Mathf.Abs (rotateY)) / 90);
        }

        /// <summary>
        /// Extract translation from transform matrix.
        /// </summary>
        /// <param name="matrix">Transform matrix. This parameter is passed by reference
        /// to improve performance; no changes will be made to it.</param>
        /// <returns>
        /// Translation offset.
        /// </returns>
        public static Vector3 ExtractTranslationFromMatrix (ref Matrix4x4 matrix)
        {
            Vector3 translate;
            translate.x = matrix.m03;
            translate.y = matrix.m13;
            translate.z = matrix.m23;
            return translate;
        }

        /// <summary>
        /// Extract rotation quaternion from transform matrix.
        /// </summary>
        /// <param name="matrix">Transform matrix. This parameter is passed by reference
        /// to improve performance; no changes will be made to it.</param>
        /// <returns>
        /// Quaternion representation of rotation transform.
        /// </returns>
        public static Quaternion ExtractRotationFromMatrix (ref Matrix4x4 matrix)
        {
            Vector3 forward;
            forward.x = matrix.m02;
            forward.y = matrix.m12;
            forward.z = matrix.m22;

            Vector3 upwards;
            upwards.x = matrix.m01;
            upwards.y = matrix.m11;
            upwards.z = matrix.m21;

            return Quaternion.LookRotation (forward, upwards);
        }

        /// <summary>
        /// Extract scale from transform matrix.
        /// </summary>
        /// <param name="matrix">Transform matrix. This parameter is passed by reference
        /// to improve performance; no changes will be made to it.</param>
        /// <returns>
        /// Scale vector.
        /// </returns>
        public static Vector3 ExtractScaleFromMatrix (ref Matrix4x4 matrix)
        {
            Vector3 scale;
            scale.x = new Vector4 (matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude;
            scale.y = new Vector4 (matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude;
            scale.z = new Vector4 (matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude;
            return scale;
        }

        /// <summary>
        /// Extract position, rotation and scale from TRS matrix.
        /// </summary>
        /// <param name="matrix">Transform matrix. This parameter is passed by reference
        /// to improve performance; no changes will be made to it.</param>
        /// <param name="localPosition">Output position.</param>
        /// <param name="localRotation">Output rotation.</param>
        /// <param name="localScale">Output scale.</param>
        public static void DecomposeMatrix (ref Matrix4x4 matrix, out Vector3 localPosition, out Quaternion localRotation, out Vector3 localScale)
        {
            localPosition = ExtractTranslationFromMatrix (ref matrix);
            localRotation = ExtractRotationFromMatrix (ref matrix);
            localScale = ExtractScaleFromMatrix (ref matrix);
        }

        /// <summary>
        /// Set transform component from TRS matrix.
        /// </summary>
        /// <param name="transform">Transform component.</param>
        /// <param name="matrix">Transform matrix. This parameter is passed by reference
        /// to improve performance; no changes will be made to it.</param>
        public static void SetTransformFromMatrix (Transform transform, ref Matrix4x4 matrix)
        {
            transform.localPosition = ExtractTranslationFromMatrix (ref matrix);
            transform.localRotation = ExtractRotationFromMatrix (ref matrix);
            transform.localScale = ExtractScaleFromMatrix (ref matrix);
        }
    }
}
