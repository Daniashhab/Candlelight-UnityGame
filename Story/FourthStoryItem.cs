using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

public class FourthStoryItem : MonoBehaviour
{
    
    [SerializeField] GameObject fourthStoryImage;
    


    void Start()
    {
        
    }

    void Update()
    {
       
        
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) 
        {
            Destroy(gameObject);

         
            fourthStoryImage.gameObject.SetActive(true);
            
            
        }
        
        
    }
   
       
}

