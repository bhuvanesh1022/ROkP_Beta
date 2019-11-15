using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectorController : MonoBehaviour
{
    public float speed = 5.0f;
    public enum DirectAs { followCh1, followCh2, followCh3, followCh4, freeCam };
    public DirectAs directAs = DirectAs.followCh1;
    public List<GameObject> targets = new List<GameObject>();
    [SerializeField] private DataManager dataManager;

    private void Awake()
    {
        dataManager = GameObject.FindWithTag("Manager").GetComponent<DataManager>();
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
                    directAs = DirectAs.freeCam;
                }
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    directAs = DirectAs.followCh1;
                }
                if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    directAs = DirectAs.followCh2;
                }
                if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    directAs = DirectAs.followCh3;
                }
                if (Input.GetKeyDown(KeyCode.Alpha4))
                {
                    directAs = DirectAs.followCh4;
                }
            }
        }
    }

    void LateUpdate()
    {
        Vector3 UpdatedPos = Vector3.zero;

        switch (directAs)
        {
            case DirectAs.freeCam:

                if (Math.Abs(Input.GetAxis("Horizontal")) >= 0 && Math.Abs(Input.GetAxis("Vertical")) >= 0)
                {
                    UpdatedPos = new Vector3(Input.GetAxis("Horizontal") * Time.deltaTime * speed,
                                                       Input.GetAxis("Vertical") * Time.deltaTime * speed, 0);
                }
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
        }


    }
}
