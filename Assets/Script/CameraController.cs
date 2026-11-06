using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform camTransform;
    public float moveSpeed = 1f;

    public void moveForward()
    {
        camTransform.position += moveSpeed * camTransform.forward;
    }

    public void moveBackward()
    {
        camTransform.position -= moveSpeed * camTransform.forward;
    }

    public void moveLeft()
    {
        camTransform.position -= moveSpeed * camTransform.right;
    }

    public void moveRight()
    {
        camTransform.position += moveSpeed * camTransform.right;
    }
}
