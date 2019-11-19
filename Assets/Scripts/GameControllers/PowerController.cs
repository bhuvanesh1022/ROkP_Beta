using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PowerController : MonoBehaviourPun, IPunObservable
{
    public enum PowerupTypes { SpeedBoost, Throwable, Thrown };
    public PowerupTypes powerup;

    public GameObject Thrower;
    public string ThrowerName;
    private GameObject CollidedWith;
    private SpriteRenderer _sprite;
    private GameController gameController;
    private PhotonView pv;

    private void Awake()
    {
        _sprite = GetComponentInChildren<SpriteRenderer>();
        gameController = GameObject.FindWithTag("GameController").GetComponent<GameController>();
        pv = GetComponent<PhotonView>();
    }

    void Start()
    {
        if (powerup == PowerupTypes.Thrown)
        {
            GetComponent<Rigidbody2D>().velocity = new Vector2(50, 0);
            Destroy(gameObject, 3.0f);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        CollidedWith = collision.gameObject;

        if (CollidedWith.CompareTag("Player") && CollidedWith.GetComponent<PlayerController>().pv.IsMine)
        {
            switch (powerup)
            {
                case PowerupTypes.SpeedBoost:

                    if (!gameController.SpeedPoweredRunners.Contains(CollidedWith))
                    {
                        gameController.SpeedPoweredRunners.Add(CollidedWith);
                    }
                    if (gameController.ThrowPoweredRunners.Contains(CollidedWith))
                    {
                        gameController.ThrowPoweredRunners.Remove(CollidedWith);
                    }

                    for (int i = 0; i < gameController.PowerUpBtns.Count; i++)
                    {
                        if (gameController.PowerUpBtns[i].activeInHierarchy)
                        {
                            gameController.PowerUpBtns[i].SetActive(false);
                        }
                    }
                    gameController.PowerUpBtns[0].SetActive(true);


                    pv.RPC("DisablePickup", RpcTarget.AllBuffered, null);
                    break;

                case PowerupTypes.Throwable:

                    if (!gameController.ThrowPoweredRunners.Contains(CollidedWith))
                    {
                        gameController.ThrowPoweredRunners.Add(CollidedWith);
                    }
                    if (gameController.SpeedPoweredRunners.Contains(CollidedWith))
                    {
                        gameController.SpeedPoweredRunners.Remove(CollidedWith);
                    }

                    for (int i = 0; i < gameController.PowerUpBtns.Count; i++)
                    {
                        if (gameController.PowerUpBtns[i].activeInHierarchy)
                        {
                            gameController.PowerUpBtns[i].SetActive(false);
                        }
                    }
                    gameController.PowerUpBtns[1].SetActive(true);

                    pv.RPC("DisablePickup", RpcTarget.AllBuffered, null);
                    break;

                case PowerupTypes.Thrown:

                    break;
            }
        }
    }

    //public void GotHit()
    //{
    //    if (Thrower != null && Thrower != CollidedWith)
    //    {
    //        if (!CollidedWith.GetComponent<PlayerController>().canRace)
    //        {
    //            if (!gameController.VictimRunners.Contains(CollidedWith))
    //            {
    //                gameController.VictimRunners.Add(CollidedWith);

    //                gameController.shakeCamera(0.15f, 0.2f, 2.0f);
    //                gameController.GetComponent<AudioSource>().clip = GameObject.FindWithTag("AudioManager").GetComponent<AudioControl>().Hit;
    //                gameController.GetComponent<AudioSource>().Play();

    //                string t = Thrower.GetComponent<PlayerController>().UserName;
    //                string v = CollidedWith.GetComponent<PlayerController>().UserName;
    //                gameController.GotHit(t, v);
    //            }
    //        }
    //    }
    //    Destroy(gameObject, 0.25f);
    //}

    [PunRPC]
    public void DisablePickup()
    {
        StartCoroutine(PickupControl());
    }

    public IEnumerator PickupControl()
    {
        GetComponent<Collider2D>().enabled = false;
        _sprite.enabled = false;

        yield return new WaitForSeconds(.5f);

        _sprite.enabled = true;
        GetComponent<Collider2D>().enabled = true;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(ThrowerName);
        }
        if (stream.IsReading)
        {
            ThrowerName = (string)stream.ReceiveNext();
        }

    }
}
