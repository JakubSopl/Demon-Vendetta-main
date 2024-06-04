using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private Vector3 checkpointPosition;
    private bool checkpointSet = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, 5f))
            {
                if (hit.collider.CompareTag("Checkpoint"))
                {
                    checkpointPosition = hit.collider.transform.position;
                    checkpointSet = true;
                    Debug.Log("Checkpoint set at " + checkpointPosition);
                }
            }
        }
    }

    public void Respawn()
    {
        if (checkpointSet)
        {
            transform.position = checkpointPosition;
        }
        else
        {
            // Original spawn position (you can set this to whatever the original spawn position is)
            transform.position = Vector3.zero;
        }
    }
}
