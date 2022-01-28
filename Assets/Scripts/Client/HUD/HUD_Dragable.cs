using UnityEngine;
using UnityEngine.EventSystems;

public class HUD_Dragable : MonoBehaviour, IDragHandler
{
    public void OnDrag(PointerEventData eventData)
    {
        transform.position += new Vector3(eventData.delta.x, eventData.delta.y);
    }
}