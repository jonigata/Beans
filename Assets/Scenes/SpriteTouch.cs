using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class SpriteTouchEvent : UnityEvent<Vector3> {
}

public class SpriteTouch : MonoBehaviour, IPointerClickHandler {
    [SerializeField] BoxCollider boardCollider;
    [SerializeField] SpriteTouchEvent onTouch;

    public void OnPointerClick(PointerEventData eventData) {
        Debug.Log("OnPointerClick");
        onTouch.Invoke(Hit(eventData));
    }

    public Vector3 GetLocalPointFromUV(Vector3 uv) {
        var size = boardCollider.size;
        return new Vector3(
            (uv.x - 0.5f) * size.x,
            (uv.y - 0.5f) * size.y,
            0);
    }

    Vector3 Hit(PointerEventData ped) {
        return Hit(ped.pressEventCamera, ped.position);
    }

    Vector3 Hit(Camera camera, Vector2 screenPosition) { // returns uv
        RaycastHit hit;
        Ray ray = camera.ScreenPointToRay(screenPosition);
        if(boardCollider.Raycast(ray, out hit, 100.0f)) {
            var localHit = 
                boardCollider.transform.InverseTransformPoint(hit.point);
            var bounds = boardCollider.bounds;
            var min = bounds.min;
            var extents = bounds.extents * 2;
            var uv = new Vector3(
                (localHit.x - min.x) / extents.x,
                (localHit.y - min.z) / extents.z,
                0);
            Debug.Log(uv);
            return uv;
        }
        return Vector3.zero;
    }
}
