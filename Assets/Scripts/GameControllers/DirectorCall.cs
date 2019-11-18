using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectorCall : MonoBehaviour
{
    private GameController gameController;
    public List<GameObject> charactersToFollow = new List<GameObject>();
    public GameObject director;

    private void Awake()
    {
        gameController = GetComponent<GameController>();
    }

    public void ChangeDirectorMode(int mode)
    {
        if (gameController.LocalPlayer.GetComponent<DirectorController>())
        {
            gameController.LocalPlayer.GetComponent<DirectorController>().DirectorModeChange(mode);
        }
    }

}
