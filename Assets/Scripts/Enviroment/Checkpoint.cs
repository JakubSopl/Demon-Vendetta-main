using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public int id;
    public bool passed = false;

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            passed = true;
            PlayerPrefs.SetInt("LastCheckpoint", id);
        }
    }
}
