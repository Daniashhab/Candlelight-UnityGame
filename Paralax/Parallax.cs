using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    private float startpos, length;
    [SerializeField] private float parallaxEffect;
    public GameObject cam;

    void Start()
    {
        startpos = transform.position.x;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void LateUpdate()
    {
        
        float dist = cam.transform.position.x * parallaxEffect;
        float movement = cam.transform.position.x * (1 - parallaxEffect);

        transform.position = new Vector3(startpos + dist, transform.position.y, transform.position.z);

        // if background has reached the end of its length adjust its position for infinite scrolling
        if (movement > startpos + length) startpos += length;
        else if (movement < startpos - length) startpos -= length;
    }
}
