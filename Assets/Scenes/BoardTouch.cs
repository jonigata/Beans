using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public abstract class BoardTouch : MonoBehaviour {
    public abstract Vector3 GetLocalPointFromUV(Vector2 uv);
}
