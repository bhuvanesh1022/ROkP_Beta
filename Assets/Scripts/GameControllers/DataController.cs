using UnityEngine;

public class DataController : MonoBehaviour
{
    public static DataController dataController;

    public string myName;
    public string myCharacter;
    public int trackID;

    private void Awake()
    {
        if (dataController == null)
        {
            dataController = this;
        }
        else
        {
            if (dataController != this)
            {
                Destroy(dataController.gameObject);
                dataController = this;
            }
        }
        DontDestroyOnLoad(gameObject);
    }
}
