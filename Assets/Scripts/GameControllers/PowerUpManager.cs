using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
    public string Slinger;
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
            Destroy(gameObject, 20.0f);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (collision.gameObject.GetComponent<PlayerController>().pv.IsMine)
            {
                for (int i = 0; i < gameController.PowerUpBtns.Count; i++)
                {
                    if (gameController.PowerUpBtns[i].name == Powerup.ToString())
                        gameController.PowerUpBtns[i].SetActive(true);
                    else
                        gameController.PowerUpBtns[i].SetActive(false);
                }

                pv.RPC("DisablePickup", RpcTarget.AllBuffered, null);
            }
            else
            {
                Debug.Log(Slinger + " Hit " + collision.gameObject.GetComponent<PlayerController>().UserName);

                collision.gameObject.GetComponent<PlayerController>().GotAttack();
                Destroy(gameObject, 0.1f);
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


}
