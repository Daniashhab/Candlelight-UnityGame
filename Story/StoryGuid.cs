using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class StoryGuid : MonoBehaviour
{
    public TextMeshProUGUI infoText;

    
    
   
    
    
    
    void Start()
    {
      
    }

    void Update()
    {
        
    }
   
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall")) 
        {
            
            
            infoText.gameObject.SetActive(true);
            StartCoroutine(HideText(5f));
           
            }
       
        
        
    }
    IEnumerator HideText(float delay) 
    {
        yield return new WaitForSeconds(delay);
        infoText.gameObject.SetActive(false);

       
    }
    
    


}
