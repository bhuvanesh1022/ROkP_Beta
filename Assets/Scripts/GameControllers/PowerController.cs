using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PowerController : MonoBehaviourPun, IPunObservable
{
    public enum PowerupTypes { SpeedBoost, Throwable, Thrown };
    public PowerupTypes powerup;

    public string Thrower;

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
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        CollidedWith = collision.gameObject;

        if (CollidedWith.CompareTag("Player"))
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

                    gameController.PowerUpBtns[0].SetActive(true);
                    gameController.PowerUpBtns[1].SetActive(false);

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

                    gameController.PowerUpBtns[0].SetActive(false);
                    gameController.PowerUpBtns[1].SetActive(true);

                    pv.RPC("DisablePickup", RpcTarget.AllBuffered, null);
                    break;

                case PowerupTypes.Thrown:

                    CollidedWith.GetComponent<PlayerController>().GotAttack();

                    gameController.ShowHitText(Thrower, CollidedWith.GetComponent<PlayerController>().UserName);
                    //pv.RPC("HitText", RpcTarget.AllBuffered, Thrower, CollidedWith.GetComponent<PlayerController>().UserName);

                    if (!CollidedWith.GetComponent<PlayerController>().pv.IsMine)
                    {

                    }
                    break;
            }
        }
    }
   
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
            stream.SendNext(Thrower);
        }
        if (stream.IsReading)
        {
            Thrower = (string)stream.ReceiveNext();
        }

    }
}
