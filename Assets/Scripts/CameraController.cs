using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    Camera m_MainCamera;
    private Transform CamTrans;

    // public variables (mostly just speed of everything)
    public float CamTargetSpeed;
    public float CamTargetRadius;
    public float ObjectCamTargetMaxSpeed;
    public float CamRotationSpeed;
    public float zoomSpeed;
    public float camTargetDrag; // affects how slide-y the camera feels - when you let go how much does it move

    // the selected object
    public GameObject selectedObject;

    private GameObject ObjectCamTarget;
    private Rigidbody ObjectCamTargetRigidbody;
    
    private Vector3 CamTarget;
    private Vector3 CamTargetEulerRotation;

    // camera rotation around CamTargetTrans
    private float CamRotation;

    // velocities
    private float xTargetVelocity;
    private float zTargetVelocity;

    // zoom
    private float zoomLevel;
    private float ZoomLevel {
        get {
            return zoomLevel;
        }
        set {
            zoomLevel = Mathf.Clamp(value, 0f, 5f);
        }
    }

    // random junk
    private Vector2 MouseDragStartPos;
    private bool HasRightMouseDown;
    private Vector2 LastFrameMousePos;

    private void SnapToUnit () {
        ObjectCamTarget.transform.position = selectedObject.transform.position;
    }

    void Start() {
        m_MainCamera = Camera.main;
        
        // create empty game object cam target and add rigidbody
        ObjectCamTarget = new GameObject();
        ObjectCamTarget.AddComponent<Rigidbody>();
        ObjectCamTarget.name = "Cam Target";
        ObjectCamTarget.GetComponent<Rigidbody>().useGravity = false;
        ObjectCamTarget.GetComponent<Rigidbody>().drag = camTargetDrag;
        CamTrans = m_MainCamera.gameObject.GetComponent<Transform>();
        ObjectCamTargetRigidbody = ObjectCamTarget.GetComponent<Rigidbody>();
        
    }

    void Update() {
        CamTarget = ObjectCamTarget.transform.position; // saves alot of time 
        // target movement
        ObjectCamTarget.transform.rotation = Quaternion.Euler(0, CamTrans.localEulerAngles.y, 0);
        // backward
        if (Input.GetAxisRaw("Vertical") < -0.2f) {
            ObjectCamTargetRigidbody.AddRelativeForce(Vector3.back * CamTargetSpeed);
        }
        // forward
        else if (Input.GetAxisRaw("Vertical") > 0.2f) {
            ObjectCamTargetRigidbody.AddRelativeForce(Vector3.forward * CamTargetSpeed);
        }
        // right
        if (Input.GetAxisRaw("Horizontal") > 0.2f) {
            ObjectCamTargetRigidbody.AddRelativeForce(Vector3.right * CamTargetSpeed);
        }
        // left
        else if (Input.GetAxisRaw("Horizontal") < -0.2f) {
            ObjectCamTargetRigidbody.AddRelativeForce(Vector3.left * CamTargetSpeed);
        };

        if (ObjectCamTargetRigidbody.velocity.magnitude > ObjectCamTargetMaxSpeed) {
            ObjectCamTargetRigidbody.velocity = Vector3.ClampMagnitude(ObjectCamTargetRigidbody.velocity, ObjectCamTargetMaxSpeed);
        }

        if (Input.GetKeyDown(KeyCode.LeftShift)) {
            ObjectCamTargetMaxSpeed *= 2f;
            CamTargetSpeed *= 2f;
        }

        if (Input.GetKeyUp(KeyCode.LeftShift)) {
            ObjectCamTargetMaxSpeed /= 2f;
            CamTargetSpeed /= 2f;
        }

        if (Input.GetMouseButtonDown(1)) {
            HasRightMouseDown = true;
            MouseDragStartPos = Input.mousePosition;
            LastFrameMousePos = MouseDragStartPos;
        }

        if (Input.GetMouseButtonUp(1)) {
            HasRightMouseDown = false;
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            SnapToUnit();
        }

        if (Input.GetAxis("Mouse ScrollWheel") < 0f) {
            ZoomLevel += zoomSpeed;
        } else if (Input.GetAxis("Mouse ScrollWheel") > 0f) {
            ZoomLevel -= zoomSpeed;
        }

        // translate x mouse movement in camera rotation
        if (HasRightMouseDown && (LastFrameMousePos.x != Input.mousePosition.x)) {
            CamRotation += Mathf.Clamp(Input.mousePosition.x - MouseDragStartPos.x, -1, 1) / CamRotationSpeed;
        } else if (LastFrameMousePos.x == Input.mousePosition.x) {
            MouseDragStartPos = Input.mousePosition;
        }

        LastFrameMousePos.x = Input.mousePosition.x;
        LastFrameMousePos.y = Input.mousePosition.y;

        // set camera position
        var xCamPos = Mathf.Lerp((Mathf.Sin(CamRotation) * CamTargetRadius) + CamTarget.x, CamTrans.position.x, Time.deltaTime);
        var yCamPos = Mathf.Lerp(CamTrans.position.y, Mathf.Pow(ZoomLevel, 2) + 2, Time.deltaTime * 5);
        var zCamPos = Mathf.Lerp((Mathf.Cos(CamRotation) * CamTargetRadius) + CamTarget.z, CamTrans.position.z, Time.deltaTime);

        CamTrans.position = new Vector3(xCamPos, yCamPos, zCamPos);

        // make camera look at the camera target
        CamTrans.LookAt(new Vector3(CamTarget.x, CamTarget.y - .5f, CamTarget.z));
    }

    private void SetPos(float x, float y, float z, GameObject target) {
        target.GetComponent<Transform>().position = new Vector3(x, y, z);
    }
};
