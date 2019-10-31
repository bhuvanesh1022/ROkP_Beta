using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PowerUpManager : MonoBehaviourPun, IPunObservable
{
    public string Slinger;
    public string Victim;
    public GameObject VictimObj;
    public enum TypeOfPowerups { SpeedBoost, Throwable, Thrown };
    public TypeOfPowerups Powerup;

    private SpriteRenderer _sprite;
    private GameController gameController;
    private PhotonView pv;

    public void Awake()
    {
        _sprite = GetComponentInChildren<SpriteRenderer>();
        gameController = GameObject.FindWithTag("GameController").GetComponent<GameController>();
        pv = GetComponent<PhotonView>();
    }

    public void Start()
    {
        if (Powerup == TypeOfPowerups.Thrown)
        {
            pv.RPC("DestroyThrownObj", RpcTarget.AllBuffered, null);
            GetComponent<Rigidbody2D>().velocity = new Vector2(50f, 0.0f);
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (collision.gameObject.GetComponent<PlayerController>().pv.IsMine)
            {
                EnablePowerUp();
            }

            switch (Powerup)
            {
                case TypeOfPowerups.SpeedBoost:

                    pv.RPC("DisablePickup", RpcTarget.AllBuffered, null);
                    break;

                case TypeOfPowerups.Throwable:

                    pv.RPC("DisablePickup", RpcTarget.AllBuffered, null);
                    break;

                case TypeOfPowerups.Thrown:

                    if (!collision.gameObject.GetComponent<PlayerController>().pv.IsMine)
                    {
                        Victim = collision.gameObject.GetComponent<PlayerController>().UserName;
                        Debug.Log(Victim);
                        VictimObj = collision.gameObject;
                        pv.RPC("BeenAttack", RpcTarget.AllBuffered, null);
                    }
                    break;

                default:
                    break;
            }


        }
    }

    public void EnablePowerUp()
    {
        for (int i = 0; i < gameController.PowerUpBtns.Count; i++)
        {
            if (gameController.PowerUpBtns[i].name == Powerup.ToString())
                gameController.PowerUpBtns[i].SetActive(true);
            else
                gameController.PowerUpBtns[i].SetActive(false);
        }
    }

    [PunRPC]
    public void BeenAttack()
    {
        VictimObj.GetComponent<PlayerController>().GotAttack();
        Destroy(gameObject, 0.1f);
        Debug.Log(Slinger + " Hit " + Victim);
    }

    [PunRPC]
    public void DestroyThrownObj()
    {
        Destroy(gameObject, 20.0f);
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
            stream.SendNext(Slinger);
            stream.SendNext(Victim);
        }
        else if (stream.IsReading)
        {
           Slinger = (string) stream.ReceiveNext();
           Victim = (string)stream.ReceiveNext();
        }
    }
}
