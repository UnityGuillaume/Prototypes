using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BezierPath : ISerializationCallbackReceiver
{
    public class BezierCurve
    {
        public Vector3 startPoint { get { return owner.points[startIdx]; } }
        public Vector3 endPoint {  get { return owner.points[endIdx]; } }
        public Vector3 startControlPoint {  get { return owner.points[startCtrlPtsIdx]; } }
        public Vector3 endControlPoint {  get { return owner.points[endCtrlPtsIdx]; } }

        public int startIdx;
        public int endIdx;
        public int startCtrlPtsIdx;
        public int endCtrlPtsIdx;

        protected BezierPath owner;

        public BezierCurve(int startPts, int endPts, BezierPath ownerPath)
        {
            startIdx = startPts;
            endIdx = endPts;

            owner = ownerPath;

            startCtrlPtsIdx = endPts + 1;
            endCtrlPtsIdx = endPts + 2;
        }

        //TODO do something better than bruteforce discrete addition...
        public float Length()
        {
            const int steps = 20;
            float inc = 1.0f / steps;

            float length = 0;

            for(int i = 0; i < steps; ++i)
            {
                length += Vector3.Distance(EvaluatePoint(i * inc), EvaluatePoint(i * inc + inc));
            }

            return length;
        }

        public Vector3 EvaluatePoint(float t)
        {
            float u = 1.0f - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            Vector3 p = uuu * owner.points[startIdx]; //first term
            p += 3 * uu * t * owner.points[startCtrlPtsIdx]; //second term
            p += 3 * u * tt * owner.points[endCtrlPtsIdx]; //third term
            p += ttt * owner.points[endIdx]; //fourth term

            return p;
        }

        public Matrix4x4 GetBasis(float t)
        {
            //TODO: better mathematical model using bezier curve derivative for proper normal/tangents
            Vector3 startTangent = Vector3.Normalize(startControlPoint - startPoint);
            Vector3 endTangent = Vector3.Normalize(endControlPoint - endPoint);

            Vector3 forward;

            if(t + 0.05f >= 1.0f) 
                forward = Vector3.Normalize(EvaluatePoint(t) - EvaluatePoint(t - 0.05f));
            else
                forward = Vector3.Normalize(EvaluatePoint(t + 0.05f) - EvaluatePoint(t));

            Vector3 tangent = Vector3.Normalize(Vector3.Lerp(startTangent, endTangent, t));
            Vector3 up = Vector3.Normalize(Vector3.Cross(forward, tangent));
            tangent = Vector3.Normalize(Vector3.Cross(up, forward));

            Matrix4x4 matrix = new Matrix4x4();
            matrix.SetColumn(0, tangent);
            matrix.SetColumn(1, up);
            matrix.SetColumn(2, forward);
            matrix.SetColumn(3, Vector3.zero);

            return matrix;
        }
    }

    public List<Vector3> points;
    public List<BezierCurve> curves;

    [SerializeField]
    protected byte[] serializedData;

    public BezierPath(Vector3 start, Vector3 end)
    {
        points = new List<Vector3>();
        curves = new List<BezierCurve>();

        points.Add(start);
        AddPoint(end);
    }

    public Vector3 Evaluate(float t)
    {
        float curveRatio = 1.0f / curves.Count;
        int curve = Mathf.FloorToInt(t / curveRatio);

        if (curve < curves.Count)
        {
            float t01 = (t - curveRatio * curve) / curveRatio;
            Vector3 pts = curves[curve].EvaluatePoint(t01);

            return pts;
        }

        return points[points.Count < 3 ? points.Count - 1 : points.Count - 3];
    }

    public float Length()
    {
        float length = 0;
        for(int i = 0; i < curves.Count; ++i)
        {
            length += curves[i].Length();
        }

        return length;
    }

    public Matrix4x4 GetBasis(float t)
    {
        float curveRatio = 1.0f / curves.Count;
        int curve = Mathf.FloorToInt(t / curveRatio);

        if (curve < curves.Count)
        {
            float t01 = (t - curveRatio * curve) / curveRatio;
            Matrix4x4 basis = curves[curve].GetBasis(t01);

            return basis;
        }

        return curves[curves.Count-1].GetBasis(1.0f);
    }

    public void AddPoint(Vector3 point)
    {
        points.Add(point);
        int previous = points.Count < 3 ? points.Count - 2 : points.Count - 4;

        Vector3 tangentDirection = Vector3.Cross(Vector3.up, Vector3.Normalize(point - points[previous]));

        points.Add(points[previous] + tangentDirection);
        points.Add(point + tangentDirection);


        NewCurveAtIndex(points.Count - 3);
    }

    protected void NewCurveAtIndex(int index)
    {
        curves.Add(new BezierCurve(index < 3 ? index - 1 : index - 3, index, this));
    }

    public void OnBeforeSerialize()
    {

    }

    public void OnAfterDeserialize()
    {
        curves = new List<BezierCurve>();
        for(int i = 1; i < points.Count; i+=3)
        {
            NewCurveAtIndex(i);
        }
    }
}
