using System.Collections.Generic;
using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    public float speed = 5.0f;
    public float mouseSensitivity = 2.0f;
    private CharacterController characterController;
    private Camera playerCamera;
    private float verticalRotation;
    private float horizontalRotation;
    
    private int isAbleToControlCounter;

    private void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerCamera = Camera.main;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (isAbleToControlCounter %2 == 0)
        {
            // Handle mouse look
            var mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            var mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
            verticalRotation -= mouseY;
            verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
            horizontalRotation += mouseX;
            playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
            transform.localRotation = Quaternion.Euler(0, horizontalRotation, 0);

            // Handle movement
            var moveForward = Input.GetAxis("Vertical") * speed;
            var moveSide = Input.GetAxis("Horizontal") * speed;
            var move = transform.forward * moveForward + transform.right * moveSide;
            characterController.Move(move * Time.deltaTime);

            if (Input.GetKey(KeyCode.Space)) transform.position += Vector3.up * (speed * Time.deltaTime);

            if (Input.GetKey(KeyCode.LeftControl)) transform.position += Vector3.down * (speed * Time.deltaTime);
        }
        

        if (Input.GetKeyDown(KeyCode.Escape)) Cursor.lockState = CursorLockMode.None;

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (isAbleToControlCounter % 2 == 0)
            {
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
            isAbleToControlCounter++;
        }
    }
}