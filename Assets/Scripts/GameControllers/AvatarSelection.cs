using UnityEngine;
using UnityEngine.UI;

public class AvatarSelection : MonoBehaviour
{
    public bool isSelected;

    public void Update()
    {
        GetComponent<Button>().interactable = !isSelected;
    }
}
