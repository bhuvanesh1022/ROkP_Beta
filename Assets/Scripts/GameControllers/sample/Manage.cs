using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manage : MonoBehaviour
{
    public GameObject player1;
    public GameObject player2;
    public ctrl Ctrl;

    // Start is called before the first frame update
    void Start()
    {
        Instantiate(player1);
        Instantiate(player2);

        Ctrl.targets.Add(player1);
        Ctrl.targets.Add(player2);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
