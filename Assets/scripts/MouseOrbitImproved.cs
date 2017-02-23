using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Camera-Control/Mouse Orbit with zoom")]
public class MouseOrbitImproved : MonoBehaviour {

    public static MouseOrbitImproved Instance { get; private set; }

    public Transform centralTarget;
    public Transform target;
    public float distance = 5.0f;
    public float xSpeed = 5.0f;
    public float ySpeed = 5.0f;
    public float scrollSpeed = 5;
    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;

    public float distanceMin = .5f;
    public float distanceMax = 15f;

    float x = 0.0f;
    float y = 0.0f;

    Camera cam;

    void Awake() {
        Instance = this;
    }

    // Use this for initialization
    void Start() {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
        distance = distanceMax;
        cam = GetComponent<Camera>();
    }

    void LateUpdate() {
        if (!centralTarget)
            return;

        if (target) {
            if (Input.GetKey(KeyCode.Mouse1)) {
                x += Input.GetAxis("Mouse X") * xSpeed * distance * Time.deltaTime;
                y -= Input.GetAxis("Mouse Y") * ySpeed * distance * Time.deltaTime;

                y = ClampAngle(y, yMinLimit, yMaxLimit);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            Quaternion rotation = Quaternion.Euler(y, x, 0);

            distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * scrollSpeed * Time.deltaTime, distanceMin, distanceMax);

            if (Input.GetKeyDown(KeyCode.Mouse0)) {
                Ray ray = cam.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, float.MaxValue)) {
                    target = hit.transform;
                    SpaceObjectScript scr = target.GetComponent<SpaceObjectScript>();
                    if (scr) {
                        scr.SetSelected(true);
                    }
                }
                else {
                    if (target) {
                        SpaceObjectScript scr = target.GetComponent<SpaceObjectScript>();
                        if (scr) {
                            scr.SetSelected(false);
                        }
                    }
                    target = centralTarget;
                }
            }
            Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
            Vector3 position = rotation * negDistance + target.position;

            transform.rotation = rotation;
            transform.position = position;
        }
        else {
            target = centralTarget;
        }
    }

    public static float ClampAngle(float angle, float min, float max) {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}
