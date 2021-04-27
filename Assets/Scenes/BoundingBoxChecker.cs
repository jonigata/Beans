using UnityEngine;
using System.Collections;

public class BoundingBoxChecker : MonoBehaviour
{

    [SerializeField] private Bounds bounds;

    void OnDrawGizmosSelected()
    {
        var collider = GetComponent<Collider>();

        if (collider==null) return;
        var b = collider.bounds;

        Gizmos.color = new Color(1, 0, 0, 0.5F);
        Gizmos.DrawCube(b.center, b.size);
        bounds = b;
    }
}
