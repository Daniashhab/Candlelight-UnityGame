using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BurnAnimation : MonoBehaviour
{

    public Sprite[] burnSprits;
    public float frameDuration = 0.1f;
    public Image image;
    public Button button;
    public TextMeshProUGUI text;
    private bool isBurning = false;
    void Awake()
    {
        image = GetComponent<Image>();
        button = GetComponent<Button>();
        button.onClick.AddListener(StartBurning);
    }

    void Update()
    {
        
    }
    private void StartBurning() 
    {
       if (!isBurning) 
        
            StartCoroutine(BurnAnimationCorontine());
        
       
    }
    IEnumerator BurnAnimationCorontine() 
    {
        isBurning = true;
        foreach (Sprite sprite in burnSprits)
        {
            image.sprite = sprite;
            yield return new WaitForSeconds(frameDuration);
        }
        image.enabled = false;
        button.interactable = false;
        text.enabled = false;
        isBurning=false;
    }
}
