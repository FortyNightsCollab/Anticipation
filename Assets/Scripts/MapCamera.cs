using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCamera : MonoBehaviour
{
    Camera attachedCamera;
    Vector3 rotateStartPosition;
    Vector3 slideStartPosition;
    Vector2 previousMouseScrollPosition;

    // Start is called before the first frame update
    void Start()
    {
        attachedCamera = GetComponentInChildren<Camera>();
    }

    void Update()
    {

        if (Input.GetMouseButtonDown(1))
        {
            slideStartPosition = Input.mousePosition;
      
        }

        else if (Input.GetMouseButton(1))
        {
      
            if (Input.mousePosition.x < slideStartPosition.x)
            {
                transform.Translate(Vector3.right * 5.0f * Time.deltaTime);
            }
        
            else if (Input.mousePosition.x > slideStartPosition.x)
            {
                transform.Translate(Vector3.left * 5.0f * Time.deltaTime);
            }

            if (Input.mousePosition.y < slideStartPosition.y)
            {
                transform.Translate(Vector3.forward * 5.0f * Time.deltaTime);
            }

            else if (Input.mousePosition.y > slideStartPosition.y)
            {
                transform.Translate(Vector3.back * 5.0f * Time.deltaTime);
            }

            
        }
        
        if(Input.GetMouseButtonDown(2))
        {
            rotateStartPosition = Input.mousePosition;
        }

        else if(Input.GetMouseButton(2))
        {
            if (Input.mousePosition.x < rotateStartPosition.x)
            {
                Rotate(-100.0f);
            }

            else if(Input.mousePosition.x > rotateStartPosition.x)
            {
                Rotate(100.0f);
            }
        }

        if(Input.mouseScrollDelta.y > 0.0f)
        {
            Zoom(1.0f);    
        }        

        else if(Input.mouseScrollDelta.y < 0.0f)
        {
            Zoom(-1.0f);
        }
    }

    public void ChangeFocalPoint(Vector3 newLookAt)
    {
        transform.position = newLookAt;
        attachedCamera.transform.LookAt(newLookAt);
    }

    public void Zoom(float zoomAmount)
    {
        attachedCamera.transform.Translate(Vector3.forward * zoomAmount);
    }

    public RaycastHit GetRayHitResult()
    {
        Vector3 mouseScreen = Input.mousePosition;
        RaycastHit hitData;

        Ray rayToCast = attachedCamera.ScreenPointToRay(mouseScreen);
        Physics.Raycast(rayToCast, out hitData, 1000.0f);
        return hitData;
    }

    public void Rotate(float rotateAmount)
    {
        transform.Rotate(new Vector3(0.0f, rotateAmount * Time.deltaTime, 0.0f));
        attachedCamera.transform.LookAt(transform.position);
    }
}
