using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class PhysicsCalculations
{
    public static float CalculateOverlapPercentage(Collider2D trigger, Collider2D target)
    {

        Bounds triggerBounds = trigger.bounds;
        Bounds targetBounds = target.bounds;

        Debug.DrawLine(new Vector2(triggerBounds.min.x, triggerBounds.min.y), new Vector2(triggerBounds.max.x, triggerBounds.min.y), Color.red);
        Debug.DrawLine(new Vector2(triggerBounds.min.x, triggerBounds.max.y), new Vector2(triggerBounds.max.x, triggerBounds.max.y), Color.red);
        Debug.DrawLine(new Vector2(triggerBounds.min.x, triggerBounds.min.y), new Vector2(triggerBounds.min.x, triggerBounds.max.y), Color.red);
        Debug.DrawLine(new Vector2(triggerBounds.max.x, triggerBounds.min.y), new Vector2(triggerBounds.max.x, triggerBounds.max.y), Color.red);

        Debug.DrawLine(new Vector2(targetBounds.min.x, targetBounds.min.y), new Vector2(targetBounds.max.x, targetBounds.min.y), Color.red);
        Debug.DrawLine(new Vector2(targetBounds.min.x, targetBounds.max.y), new Vector2(targetBounds.max.x, targetBounds.max.y), Color.red);
        Debug.DrawLine(new Vector2(targetBounds.min.x, targetBounds.min.y), new Vector2(targetBounds.min.x, targetBounds.max.y), Color.red);
        Debug.DrawLine(new Vector2(targetBounds.max.x, targetBounds.min.y), new Vector2(targetBounds.max.x, targetBounds.max.y), Color.red);

        if (!triggerBounds.Intersects(targetBounds)) return 0f; // No overlap

        float xOverlap = Mathf.Max(0, Mathf.Min(triggerBounds.max.x, targetBounds.max.x) - Mathf.Max(triggerBounds.min.x, targetBounds.min.x));
        float yOverlap = Mathf.Max(0, Mathf.Min(triggerBounds.max.y, targetBounds.max.y) - Mathf.Max(triggerBounds.min.y, targetBounds.min.y));
        float overlapArea = xOverlap * yOverlap;

        float triggerArea = triggerBounds.size.x * triggerBounds.size.y;

        return (overlapArea / triggerArea) * 100f; // Percentage of overlap
    }

}
