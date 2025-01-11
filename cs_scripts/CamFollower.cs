using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamFollower : MonoBehaviour
{
    public GameObject player;
    private Vector3 offset = new Vector3(-10f, 5f, 10f);

    public float distance; // Distance from the target
    public float minDistance = 5f; // Minimum allowed distance
    public float maxDistance = 50f; // Maximum allowed distance
    public float zoomSpeed = 2f; // Speed of zoom adjustment
    public float mouseSensitivity = 300f; // Mouse sensitivity
    private float xRotation = 0f; // Vertical rotation
    private float yRotation = 0f; // Horizontal rotation

    // Start is called before the first frame update
    void Start()
    {
        // This will be called on start. If no UIManager is present, this values will be used (for testing purposes)
        // Initial offset vector  (camera-player) will be kept for testing.
        if (GameObject.Find("EZGliderPlanePrefab_test") != null)
        {
            offset = transform.position - GameObject.Find("EZGliderPlanePrefab_test").GetComponent<Rigidbody>().transform.position;
        }
        distance = offset.magnitude;
    }

    // Update is called once per frame
    void Update()
    {
        if (player != null)
        {
            // Handle camera zoom with mouse wheel
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");
            distance -= scrollInput * zoomSpeed;
            distance = Mathf.Clamp(distance, minDistance, maxDistance); // Ensure distance is within bounds

            // Calculate mouse movement for rotation
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            yRotation += mouseX;
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -80f, 80f); // Limit vertical rotation to prevent flipping

            // Calculate the rotation and position of the camera
            Quaternion rotation = Quaternion.Euler(xRotation, yRotation, 0);
            Vector3 position = player.transform.position - rotation * Vector3.forward * distance;

            // Set camera position and rotation
            transform.position = position;
            transform.LookAt(player.transform.position); // Look at the target
        }
    }

    public void Follow_player(GameObject aircraft)
    {
        this.player = aircraft;
    }

    public void set_camera(Vector3 offset_i, Vector3 rotation_i)
    {
        offset = offset_i;

        Quaternion newRotation = Quaternion.Euler(rotation_i);

        // Apply the new rotation to the transform
        transform.rotation = newRotation;
    }
}
