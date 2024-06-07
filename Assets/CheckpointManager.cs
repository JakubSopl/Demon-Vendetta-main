using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    [SerializeField]
    Transform defaultPosition;

    [SerializeField]
    public GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        var points =  FindObjectsOfType<Checkpoint>().ToList();
        points = points.OrderBy(p => p.id).ToList();

        int lastCheckpoint = PlayerPrefs.GetInt("LastCheckpoint", 0);
        var checkpoint = points.FirstOrDefault(p => p.id == lastCheckpoint);
        if(checkpoint != null)
        {
            player.transform.position = checkpoint.transform.position;
        } else
        {
            player.transform.position = defaultPosition.position;
        }
    }
}
