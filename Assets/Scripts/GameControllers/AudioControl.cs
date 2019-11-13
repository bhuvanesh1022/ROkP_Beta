using UnityEngine;

public class AudioControl : MonoBehaviour
{
    public static AudioControl audioControl;

    public AudioClip BG_Menu;
    public AudioClip[] BG_Game;
    public AudioClip SpeedBoost;
    public AudioClip Thrower;
    public AudioClip Hit;

    private void Awake()
    {
        if (audioControl == null)
        {
            audioControl = this;
        }
        else
        {
            if (audioControl != this)
            {
                Destroy(audioControl.gameObject);
                audioControl = this;
            }
        }
        DontDestroyOnLoad(gameObject);
    }
}
