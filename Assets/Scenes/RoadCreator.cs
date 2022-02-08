using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(PathCreator))]
public class RoadCreator : MonoBehaviour
{
    [Range(0.05f, 1)]
    public float spacing = 0.1f;
    public float tiling = 1;
    public float roadWidth = 1;
    public bool autoUpdate;

    public int checkpointSpacing = 2;
    
    public GameObject checkpoint;
    public GameObject checkpointParent;

    public int wallSpacing = 10;
    public GameObject wall;
    public GameObject wallParent;

    private Mesh mesh;
        
    public void UpdateRoad()
    {
        Path path = GetComponent<PathCreator>().path;
        Vector2[] points = path.CalculateEvenlySpacedPoints(spacing);
        GetComponent<MeshFilter>().mesh = CreateRoadMesh(points, path.IsClosed);

        int textureRepeat = Mathf.RoundToInt(tiling * points.Length * spacing * .05f);
        GetComponent<MeshRenderer>().sharedMaterial.mainTextureScale = new Vector2(1, textureRepeat);
        GetComponent<MeshCollider>().sharedMesh = mesh;  
    }

    public Mesh CreateRoadMesh(Vector2[] points, bool isClosed)
    {
        Vector3[] verts = new Vector3[points.Length * 2];
        Vector2[] UVs = new Vector2[verts.Length];
        int numTries = 2 * points.Length - 1 + ((isClosed) ? 2 : 0);
        int[] tris = new int[numTries * 3];
        int vertIndex  = 0;
        int triIndex = 0;


        for (int i = 0; i < points.Length; i++)
        {
            Vector2 forward = Vector2.zero;
            if (i < points.Length - 1 || isClosed)
            {
                forward += points[(i + 1)%points.Length] - points[i];
            }
            else if(i > 0 || isClosed)
            {
                forward += points[i] - points[(i - 1 + points.Length)%points.Length];
            }
            forward.Normalize();
            Vector2 left = new Vector2(-forward.y, forward.x);

            verts[vertIndex] = points[i]+ left*roadWidth*.5f;
            verts[vertIndex + 1] = points[i] - left * roadWidth * .5f;

            float completionPercent = i / (float)(points.Length - 1);
            float v = 1 - Mathf.Abs(completionPercent * 2 -1);
            UVs[vertIndex] = new Vector2(0, v);
            UVs[vertIndex + 1] = new Vector2(1, v);

            if(i < points.Length - 1 || isClosed)
            {
                tris[triIndex] = vertIndex;
                tris[triIndex + 1] = (vertIndex + 2) % verts.Length;
                tris[triIndex + 2] = vertIndex + 1;
            
                tris[triIndex + 3] = vertIndex + 1;
                tris[triIndex + 4] = (vertIndex + 2)%verts.Length;
                tris[triIndex + 5] = (vertIndex + 3)%verts.Length;
            
            }

            vertIndex += 2;
            triIndex+=6;
        }

        //PlaceCheckpoint(verts);

        mesh = new Mesh(); 
        
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = UVs;

        return mesh;
    }

    public void Start()
    {
        PlaceCheckpointsAndWalls(GetComponent<MeshFilter>().sharedMesh.vertices);
        transform.parent.GetChild(1).gameObject.GetComponent<CheckpointManager>().Init();
    }

    public void PlaceCheckpointsAndWalls(Vector3[] verts)
    {
        for(int i = 0;i< verts.Length; i+=(wallSpacing * 2))
        {
            
            GameObject g = Instantiate(wall, wallParent.transform);
            g.transform.position = verts[i] + Vector3.forward * -0.02f;
            g.transform.localScale = Vector3.one * 0.2f + Vector3.up;
            g.transform.eulerAngles = Vector3.right * -90.0f;
            

            g = Instantiate(wall, wallParent.transform);
            g.transform.position = verts[i+1] + Vector3.forward * -0.02f;
            g.transform.localScale = Vector3.one * 0.2f + Vector3.up ;
            g.transform.eulerAngles = Vector3.right * -90.0f;
        }

        for(int i = 0; i < verts.Length - (checkpointSpacing * 2); i +=(checkpointSpacing* 2))
        {
            GameObject c = Instantiate(checkpoint, checkpointParent.transform);
            c.transform.position = (verts[i] + verts[i + 1]) * 0.5f;
            Vector3 checkpointLine = (verts[i + (checkpointSpacing * 2)] - verts[i + (checkpointSpacing * 2) +1]) * 0.5f; 
            c.transform.rotation = Quaternion.LookRotation(checkpointLine, Vector3.forward);
        }

        checkpointParent.transform.Rotate(90, 0, 0);
        wallParent.transform.Rotate(90, 0, 0);
    }
}
