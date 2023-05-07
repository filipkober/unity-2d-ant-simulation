using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControls : MonoBehaviour
{
    [Range(0f, 10f)]
    public float cameraPanSpeed = 1f;
    [Range(0f, 10f)]
    public float cameraScaleSpeed = 1f;
    [Range(1f, 10f)]
    public float shiftModifier = 2.5f;

    public bool isControlled = false;
    // Start is called before the first frame update
    void Start()
    {
        if (tag == "MainCamera") isControlled = true;
    }

    // Update is called once per frame
    void Update()
    {
        float verticalMovement = Input.GetAxis("Vertical");
        float horizontalMovement = Input.GetAxis("Horizontal");
        float scalingMovement = -Input.GetAxis("Scale");
        float shiftForce = Input.GetAxis("Shift") * shiftModifier;
        if (shiftForce == 0) shiftForce = 1;
        if (isControlled && (verticalMovement != 0 || horizontalMovement != 0 || scalingMovement != 0))
        {
            transform.position += new Vector3(horizontalMovement * cameraPanSpeed * shiftForce * Time.deltaTime, verticalMovement * cameraPanSpeed * shiftForce * Time.deltaTime);
            GetComponent<Camera>().orthographicSize += cameraScaleSpeed * scalingMovement * shiftForce * Time.deltaTime;
        }
    }
}
