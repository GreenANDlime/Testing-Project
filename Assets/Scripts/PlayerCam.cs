using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NewMonoBehaviourScript : MonoBehaviour
{
    // Camera sensitivity in X and Y direction
    public float sensX; 
    public float sensY;

    public Transform orientation; // orientation transformation

    float xRotation;
    float yRotation;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked; // locks cursor in middle of screen
        Cursor.visible = false; // sets cursor invisible
    }

    private void Update()
    {
        // get mouse input
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        yRotation += mouseX;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); // prevents rotation beyond 90 degrees

        // rotate cam and orientation
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }


}
