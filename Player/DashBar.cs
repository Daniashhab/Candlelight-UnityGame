using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashBar : MonoBehaviour
{
    public Transform dashFill; // Reference to the bar fill
    private float maxScaleX; // Max scale of the bar
    private float currentFill; // Current fill value (0 to 1)
    // Start is called before the first frame update

    void Start()
    {
        maxScaleX = dashFill.localScale.x; // Store the original size
    }

    public void SetDashBar(float value)
    {
        currentFill = Mathf.Clamp01(value); // Ensure between 0 and 1
        dashFill.localScale = new Vector3(maxScaleX * currentFill, dashFill.localScale.y, dashFill.localScale.z);
    }
}