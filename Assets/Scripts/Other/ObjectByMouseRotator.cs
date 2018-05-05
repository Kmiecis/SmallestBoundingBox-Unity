using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Camera))]
public class ObjectByMouseRotator : MonoBehaviour
{
    public Transform referenceObject;
    
    Vector3 mouseReferencePosition;


    private void Update()
    {
        if(EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            mouseReferencePosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(0))
        {
            Vector3 deltaPosition = Input.mousePosition - mouseReferencePosition;

            if (deltaPosition != Vector3.zero)
            {
                deltaPosition = new Vector3(deltaPosition.y, -deltaPosition.x, 0f);
                referenceObject.Rotate(Quaternion.Inverse(referenceObject.rotation) * deltaPosition);

                mouseReferencePosition = Input.mousePosition;
            }
        }

        if (Input.mouseScrollDelta != Vector2.zero)
        {
            transform.Translate(new Vector3(0f, 0f, Input.mouseScrollDelta.y) * Time.deltaTime * 10);
        }
    }
}