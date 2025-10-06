using UnityEngine;

[ExecuteInEditMode]
public class ParallaxCamera : MonoBehaviour
{
    public delegate void ParallaxCameraDelegate(Vector2 deltaMovement);
    public ParallaxCameraDelegate onCameraTranslate;

    private Vector2 oldPosition;

    void Start()
    {
        oldPosition = new Vector2(transform.position.x, transform.position.y);
    }

    void Update()
    {
        Vector2 newPosition = new Vector2(transform.position.x, transform.position.y);
        if (newPosition != oldPosition)
        {
            if (onCameraTranslate != null)
            {
                Vector2 delta = oldPosition - newPosition;
                onCameraTranslate(delta);
            }
            oldPosition = newPosition;
        }
    }
}