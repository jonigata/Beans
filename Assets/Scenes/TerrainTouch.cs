using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
public class TerrainTouchEvent : UnityEvent<Vector2> {
}

public class TerrainTouch : BoardTouch, IPointerClickHandler {
    [SerializeField] TerrainCollider boardCollider;
    [SerializeField] TerrainTouchEvent onTouch;

    public void OnPointerClick(PointerEventData eventData) {
        Debug.Log("OnPointerClick");
        if (eventData.button == PointerEventData.InputButton.Left) { 
            onTouch.Invoke(Hit(eventData));
        }
    }

    public override Vector3 GetLocalPointFromUV(Vector2 uv) {
        var size = boardCollider.bounds.extents * 2;
        return new Vector3(
            (uv.x - 0.5f) * size.x,
            0,
            (uv.y - 0.5f) * size.z);
    }

    Vector2 Hit(PointerEventData ped) {
        return Hit(ped.pressEventCamera, ped.position);
    }

    Vector2 Hit(Camera camera, Vector2 screenPosition) { // returns uv
        RaycastHit hit;
        Ray ray = camera.ScreenPointToRay(screenPosition);
        if(boardCollider.Raycast(ray, out hit, 200.0f)) {
            var localHit = 
                boardCollider.transform.InverseTransformPoint(hit.point);
            var bounds = boardCollider.bounds;
            var extents = bounds.extents * 2;
            var uv = new Vector2(
                localHit.x / extents.x,
                localHit.z / extents.z);
            Debug.Log($"bounds.min = {bounds.min}, extents = {extents}, hit.point = {hit.point}, localHit = {localHit}, uv = {uv}");
            return uv;
        }
        return Vector2.zero;
    }
}
