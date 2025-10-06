using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionToEnd : MonoBehaviour
{
    public Transition transitionObject; // Drag your SceneTransition object here in Inspector

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered trigger zone!");
            transitionObject.TriggerSceneChange();
        }
    }
}