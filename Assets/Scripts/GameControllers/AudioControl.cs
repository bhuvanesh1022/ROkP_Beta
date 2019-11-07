using UnityEngine;

public class AudioControl : MonoBehaviour
{
    public AudioClip BG_Menu;
    public AudioClip[] BG_Game;
    public AudioClip SpeedBoost;
    public AudioClip Thrower;
    public AudioClip Hit;

    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
}
