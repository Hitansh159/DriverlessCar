using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    [HideInInspector]
    public List<GameObject> walls;

    public void Init()
    {
        foreach(Transform child in transform)
        {
            if(child != null)
            {
                walls.Add(child.gameObject);
            }
        }
    }


    public float CheckpointCrossReward(Vector3 carPos, int checkpointNum)
    {
        return (3 - (carPos - CheckpointCenter(checkpointNum)).magnitude);
    } 

    public Vector3 CheckpointCenter(int index)
    {
        return (walls[2*index].transform.position +  walls[2*index+1].transform.position) * 0.5f;
    }
}
