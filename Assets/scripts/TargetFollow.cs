using UnityEngine;

public class TargetFollow : MonoBehaviour
{
    //what i want to follow player
    public Transform followTransform;

    void LateUpdate()
    {
        // Safety check actually assigned a target
        if (followTransform != null)
        {
            transform.position = followTransform.position;
        }
    }

}