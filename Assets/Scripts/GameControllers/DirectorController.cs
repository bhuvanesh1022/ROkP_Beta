using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class DirectorController : MonoBehaviour
{
    public float speed = 5.0f;
    public enum DirectAs { freeCam, followCh1, followCh2, followCh3, followCh4, followVictim };
    public DirectAs directAs;
    public GameObject victim;
    public List<GameObject> targets = new List<GameObject>();
    public PhotonView pv;
    public float zoomMax = 5;
    public float zoomMin = 15;

    [SerializeField] private DataManager dataManager;
    [SerializeField] private GameController gameController;

    private Vector3 touchStart;

    private void Awake()
    {
        dataManager = GameObject.FindWithTag("Manager").GetComponent<DataManager>();
        gameController = GameObject.FindWithTag("GameController").GetComponent<GameController>();
    }

    private void Start()
    {
        if (pv.IsMine)
        {
            gameController.LocalPlayer = gameObject;
        }
    }

    private void Update()
    {
        if (dataManager.RunnersToFollow.Count != 0)
        {
            if (targets.Count == 0)
            {
                targets = dataManager.RunnersToFollow;
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.Alpha0))
                {
                    DirectorModeChange(0);
                }
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    DirectorModeChange(1);
                }
                if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    DirectorModeChange(2);
                }
                if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    DirectorModeChange(3);
                }
                if (Input.GetKeyDown(KeyCode.Alpha4))
                {
                    DirectorModeChange(4);
                }
                if (Input.GetKeyDown(KeyCode.Alpha5))
                {
                    DirectorModeChange(5);
                }
            }
        }
        else
        {
            directAs = DirectAs.freeCam;
        }
    }

    void FixedUpdate()
    {
        Vector3 UpdatedPos = Vector3.zero;

        switch (directAs)
        {
            case DirectAs.freeCam:

                Vector2 moveTo = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

                if (Math.Abs(Input.GetAxis("Horizontal")) >= 0 && Math.Abs(Input.GetAxis("Vertical")) >= 0)
                {
                    UpdatedPos = new Vector3(moveTo.x * Time.deltaTime * speed, moveTo.y * Time.deltaTime * speed, 0);
                }

                if (Input.GetMouseButtonDown(0))
                {
                    touchStart = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                }

                if (Input.touchCount == 2)
                {
                    Touch touchZero = Input.GetTouch(0);
                    Touch touchOne = Input.GetTouch(1);

                    Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                    Vector2 touchOnePrevPos = touchZero.position - touchOne.deltaPosition;

                    float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                    float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

                    float difference = currentMagnitude - prevMagnitude;

                    Zoom(difference * 0.01f);
                }
                else if (Input.GetMouseButton(0))
                {
                    UpdatedPos = touchStart - Camera.main.ScreenToWorldPoint(Input.mousePosition);
                }

                //Zoom(Input.GetAxis("Mouse ScrollWheel"));
                transform.position += UpdatedPos;
                break;

            case DirectAs.followCh1:

                UpdatedPos = targets[0].transform.position;
                transform.position = UpdatedPos;
                break;

            case DirectAs.followCh2:

                UpdatedPos = targets[1].transform.position;
                transform.position = UpdatedPos;
                break;

            case DirectAs.followCh3:

                UpdatedPos = targets[2].transform.position;
                transform.position = UpdatedPos;
                break;

            case DirectAs.followCh4:

                UpdatedPos = targets[3].transform.position;
                transform.position = UpdatedPos;
                break;

            case DirectAs.followVictim:

                UpdatedPos = victim.transform.position;
                transform.position = UpdatedPos;
                break;
        }
    }

    void Zoom(float increment)
    {
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - increment, zoomMin, zoomMax);
    }

    public void DirectorModeChange(int mode)
    {
        switch (mode)
        {
            case 0:

                directAs = DirectAs.freeCam;
                break;

            case 1:

                directAs = DirectAs.followCh1;
                break;

            case 2:

                directAs = DirectAs.followCh2;
                break;

            case 3:

                directAs = DirectAs.followCh3;
                break;

            case 4:

                directAs = DirectAs.followCh4;
                break;

            case 5:

                directAs = DirectAs.followVictim;
                break;
        }
    }
}
