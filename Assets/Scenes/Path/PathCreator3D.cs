using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathCreator3D : MonoBehaviour
{

    [HideInInspector]
    public Path3D path;

    public Color anchorCol = Color.red;
    public Color controlCol = Color.white;
    public Color segmentCol = Color.green;
    public Color selectedSegmentCol = Color.yellow;
    public float anchorDiameter = .1f;
    public float controlDiameter = .075f;
    public bool displayControlPoints = true;
    public float height = 0;

    public void CreatePath()
    {
        path = new Path3D(transform.position);
    }

    void Reset()
    {
        CreatePath();
    }

}
