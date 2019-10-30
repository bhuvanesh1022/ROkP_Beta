using UnityEngine;

public class CameraFollow : MonoBehaviour 
{
    public Transform target;
    public Transform[] bgElements;
    public float smoothSpeed = 0.125f;
    public Vector3 offset;

    private void FixedUpdate()
    {
        if (target!=null) 
        {   
            Vector3 desiredPos = target.position + offset;
            Vector3 smooothedPos = Vector3.Lerp(transform.position, desiredPos, smoothSpeed);
            transform.position = smooothedPos;
        }

        bgElements[0].position = new Vector3(transform.position.x * .02f, bgElements[0].position.y, bgElements[0].position.z);
        bgElements[1].position = bgElements[0].position;

        for (int i = 2; i < bgElements.Length; i++)
        {
            bgElements[i].position = new Vector3(transform.position.x * i / 8, bgElements[i].position.y, bgElements[i].position.z);
        }
    }


}
