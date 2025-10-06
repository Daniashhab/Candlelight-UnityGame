using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SecondStoryItem : MonoBehaviour
{
    [SerializeField] Image secondStoryImage;
    private float scalingTimeDuration = 1f;
   
    
    void Start()
    {
        
    }

    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player")) 
        {

           
            secondStoryImage.gameObject.SetActive(true);
            StartCoroutine(ScaleAnim());
            
            
        }
     
    }
    IEnumerator ScaleAnim() 
    {
        yield return new WaitForSeconds(scalingTimeDuration);

        float duration = 1f;
        float elapsedTime = 0f;
        Vector3 startingScale = secondStoryImage.rectTransform.localScale;

        Vector3 newScale = new Vector3(20, 20, 20);
        while (elapsedTime < duration) 
        {
            secondStoryImage.rectTransform.localScale = Vector3.Lerp(startingScale, newScale, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        
        }
        secondStoryImage.rectTransform.localScale = newScale;
        Destroy(gameObject);
    }
    
}
