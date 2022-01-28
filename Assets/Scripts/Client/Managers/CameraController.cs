using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] GameObject target;
    [SerializeField] Vector3 offset;
    [SerializeField] LayerMask raycastLayer;
    [SerializeField] bool hideObjects = false;

    List<Transform> hiddenObjects = new List<Transform>();

    void Update()
    {
        if (target == null)
            return;
        
        Vector3 direction = target.transform.position - transform.position;
        float distance = direction.magnitude;

        if (hideObjects)
        {
            RaycastHit[] hits = Physics.RaycastAll(transform.position, direction, distance, raycastLayer);

            foreach (var i in hits)
            {
                Transform currentHit = i.transform;

                if (!hiddenObjects.Contains(currentHit))
                {
                    hiddenObjects.Add(currentHit);
                    if (currentHit.GetComponent<MeshRenderer>() != null)
                        currentHit.GetComponent<MeshRenderer>().enabled = false;
                }
            }

            for (int i = 0; i < hiddenObjects.Count; i++)
            {
                bool isHit = false;

                for (int j = 0; j < hits.Length; j++)
                {
                    if (hits[j].transform == hiddenObjects[i])
                    {
                        isHit = true;
                        break;
                    }
                }

                if (!isHit)
                {
                    Transform wasHidden = hiddenObjects[i];
                    if (wasHidden.GetComponent<MeshRenderer>() != null)
                        wasHidden.GetComponent<MeshRenderer>().enabled = true;
                    hiddenObjects.RemoveAt(i);
                    i--;
                }
            }
        }
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        transform.position = target.transform.position + offset;
        transform.LookAt(target.transform);
    }

    public void SetTarget(GameObject value) { target = value; }
}