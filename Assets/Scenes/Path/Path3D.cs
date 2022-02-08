using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Path3D
{
    [SerializeField, HideInInspector]
    List<Vector3> points;
    [SerializeField, HideInInspector]
    bool isClosed;
    [SerializeField, HideInInspector]
    bool autoSetControlPoints;
    [SerializeField, HideInInspector]
    bool fixHeight;
    [SerializeField, HideInInspector]
    float height;

    public Path3D(Vector3 center)
    {
        points = new List<Vector3>
        {
            center + Vector3.left,
            center + (Vector3.left + Vector3.up) * .5f,
            center + Vector3.right,
            center + (Vector3.right + Vector3.down) * .5f
        };
    }

    public Vector3 this[int i]
    {
        get { return points[i]; }
    }

    public int NumPoints
    {
        get { return points.Count; }
    }

    public int NumSegments
    {
        get { return points.Count / 3; }
    }

    public bool AutoSetControlPoints
    {
        get
        { return autoSetControlPoints; }
        set
        {
            if (autoSetControlPoints != value)
            {
                autoSetControlPoints = value;
                if (autoSetControlPoints)
                {
                    AutoSetAllControlPoints();
                }
            }
        }
    }

    public bool IsClosed
    {
        get
        {
            return isClosed;
        }
        set
        {
            if (isClosed != value)
            {
                isClosed = value;

                if (isClosed)
                {
                    points.Add(points[points.Count - 1] * 2 - points[points.Count - 2]);
                    points.Add(points[0] * 2 - points[1]);
                    if (autoSetControlPoints)
                    {
                        AutoSetAnchorControlPoints(0);
                        AutoSetAnchorControlPoints(points.Count - 3);
                    }
                }
                else
                {
                    points.RemoveRange(points.Count - 2, 2);
                    if (autoSetControlPoints)
                    {
                        AutoSetStartAndEndControlPoints();
                    }
                }
            }
        }
    }

    public float Height
    {
        get { return height; }
        set { height = value; }
    }

    public bool FixHeight
    {
        get
        {
            return fixHeight;
        }
        set
        {
            if(fixHeight != value)
            {
                fixHeight = value;
                if (fixHeight)
                {
                    for(int i = 0; i < points.Count;i++)
                        points[i] = new Vector3(points[i].x, height, points[i].z); 
                }
                if(autoSetControlPoints)
                    AutoSetAllControlPoints();
            }
        }
    }

    public void AddSegment(Vector3 anchorPos)
    {
        Debug.Log(anchorPos);
        anchorPos.y = height; 
        points.Add(points[points.Count - 1] * 2 - points[points.Count - 2]);
        points.Add((points[points.Count - 1] + anchorPos) * .5f);
        points.Add(anchorPos);

        if (autoSetControlPoints)
        {
            AutoSetAllAfftectedAnchorPoints(points.Count - 1);
        }
    }

    public void SplitSegment(Vector3 anchorPos, int segmentIndex)
    {
        points.InsertRange(segmentIndex * 3 + 2, new Vector3[] { Vector3.zero, anchorPos, Vector3.zero });
        if (autoSetControlPoints)
        {
            AutoSetAllAfftectedAnchorPoints(segmentIndex * 3 + 3);
        }
        else
        {
            AutoSetAnchorControlPoints(segmentIndex * 3 + 3);
        }
    }

    public void DeleteSegment(int anchorIndex)
    {
        if (NumSegments > 2 || !isClosed && NumSegments > 1)
        {
            if (anchorIndex == 0)
            {
                if (isClosed)
                {
                    points[points.Count - 1] = points[2];
                }
                points.RemoveRange(0, 3);
            }
            else if (anchorIndex == points.Count - 1 && !isClosed)
            {
                points.RemoveRange(anchorIndex - 2, 3);
            }
            else
            {
                points.RemoveRange(anchorIndex - 1, 3);
            }
        }
    }

    public Vector3[] GetPointsInSegment(int i)
    {
        return new Vector3[] { points[i * 3], points[i * 3 + 1], points[i * 3 + 2], points[LoopIndex(i * 3 + 3)] };
    }

    public void MovePoint(int i, Vector3 pos)
    {
        pos.y = height;
        Vector3 deltaMove = pos - points[i];
        if (i % 3 == 0 || !autoSetControlPoints)
        {
            points[i] = pos;


            if (autoSetControlPoints)
            {
                AutoSetAllAfftectedAnchorPoints(i);
            }
            else
            {

                if (i % 3 == 0)
                {
                    if (i + 1 < points.Count || isClosed)
                    {
                        points[LoopIndex(i + 1)] += deltaMove;
                    }
                    if (i - 1 > -1 || isClosed)
                    {
                        points[LoopIndex(i - 1)] += deltaMove;
                    }
                }
                else
                {
                    bool nextPointIsAnchor = (i + 1) % 3 == 0;
                    int correspondingControlPoint = nextPointIsAnchor ? i + 2 : i - 2;
                    int anchorPoint = nextPointIsAnchor ? i + 1 : i - 1;

                    if (InRange(correspondingControlPoint, 0, points.Count - 1) || isClosed)
                    {
                        float dst = (points[LoopIndex(anchorPoint)] - points[LoopIndex(correspondingControlPoint)]).magnitude;
                        Vector3 dir = (points[LoopIndex(anchorPoint)] - pos).normalized;

                        points[LoopIndex(correspondingControlPoint)] = points[LoopIndex(anchorPoint)] + dir * dst;
                    }
                }
            }
        }
    }

    int LoopIndex(int i)
    {
        return (i + points.Count) % points.Count;
    }

    bool InRange(int i, int start, int end)
    {
        return i >= start && i <= end;
    }

    void AutoSetAllAfftectedAnchorPoints(int updatedAnchorIndex)
    {
        for (int i = updatedAnchorIndex - 3; i <= updatedAnchorIndex + 3; i += 3)
        {
            if (InRange(i, 0, points.Count - 1) || isClosed)
            {
                AutoSetAnchorControlPoints(LoopIndex(i));
            }
        }
        AutoSetStartAndEndControlPoints();
    }

    void AutoSetAllControlPoints()
    {
        for (int i = 0; i < points.Count; i += 3)
        {
            AutoSetAnchorControlPoints(i);
        }
        AutoSetStartAndEndControlPoints();
    }

    void AutoSetAnchorControlPoints(int anchorIndex)
    {
        Debug.Assert(InRange(anchorIndex, 0, points.Count - 1) == true);
        Vector3 anchorPos = points[anchorIndex];
        Vector3 dir = Vector3.zero;
        float[] neighbourDistances = new float[2];

        if (anchorIndex - 3 >= 0 || isClosed)
        {
            Vector3 offset = points[LoopIndex(anchorIndex - 3)] - anchorPos;
            dir += offset.normalized;
            neighbourDistances[0] = offset.magnitude;
        }
        if (anchorIndex + 3 >= 0 || isClosed)
        {
            Vector3 offset = points[LoopIndex(anchorIndex + 3)] - anchorPos;
            dir -= offset.normalized;
            neighbourDistances[1] = -offset.magnitude;
        }

        dir.Normalize();

        for (int i = 0; i < 2; i++)
        {
            int controlIndex = anchorIndex + i * 2 - 1;
            if (InRange(controlIndex, 0, points.Count - 1) || isClosed)
            {
                points[LoopIndex(controlIndex)] = anchorPos + dir * neighbourDistances[i] * .5f;
            }
        }
    }
    void AutoSetStartAndEndControlPoints()
    {
        if (!isClosed)
        {
            points[1] = (points[0] + points[2]) * .5f;
            points[points.Count - 2] = (points[points.Count - 1] + points[points.Count - 3]) * .5f;
        }
    }

    public Vector3[] CalculateEvenlySpacedPoints(float spacing, float resolution = 1)
    {
        List<Vector3> evenlySpacedPoints = new List<Vector3>();
        evenlySpacedPoints.Add(points[0]);
        Vector3 previousPoint = points[0];
        float dstSinceLastEvenPoint = 0;

        for (int segmentIndex = 0; segmentIndex < NumSegments; segmentIndex++)
        {
            Vector3[] p = GetPointsInSegment(segmentIndex);
            float controlNetLength = Vector3.Distance(p[0], p[1]) + Vector3.Distance(p[1], p[2]) + Vector3.Distance(p[2], p[3]);
            float estimateCurveLength = Vector3.Distance(p[0], p[3]) + controlNetLength / 2f;
            int divisions = Mathf.CeilToInt(estimateCurveLength * resolution * 10f);
            float t = 0;

            while (t <= 1)
            {
                t += 1f / divisions;
                Vector3 pointOnCurve = Bezier.EvaluateCubic(p[0], p[1], p[2], p[3], t);
                dstSinceLastEvenPoint += Vector3.Distance(previousPoint, pointOnCurve);

                while (dstSinceLastEvenPoint >= spacing)
                {
                    float overshootDst = dstSinceLastEvenPoint - spacing;
                    Vector3 newEvenlySpacedPoint = pointOnCurve + (previousPoint - pointOnCurve).normalized * overshootDst;
                    evenlySpacedPoints.Add(newEvenlySpacedPoint);
                    dstSinceLastEvenPoint = overshootDst;
                    previousPoint = newEvenlySpacedPoint;
                }

                previousPoint = pointOnCurve;
            }
        }

        return evenlySpacedPoints.ToArray();

    }

}
