using System.Collections.Generic;
using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    public float speed = 5.0f;
    public float mouseSensitivity = 2.0f;
    private CharacterController characterController;
    private Camera playerCamera;
    private float verticalRotation = 0;
    private float horizontalRotation = 0;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        playerCamera = Camera.main;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Handle mouse look
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -90f, 90f);
        horizontalRotation += mouseX;
        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
        transform.localRotation = Quaternion.Euler(0, horizontalRotation, 0);

        // Handle movement
        float moveForward = Input.GetAxis("Vertical") * speed;
        float moveSide = Input.GetAxis("Horizontal") * speed;
        Vector3 move = transform.forward * moveForward + transform.right * moveSide;
        characterController.Move(move * Time.deltaTime);
        
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }
}