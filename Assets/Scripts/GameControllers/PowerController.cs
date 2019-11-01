using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PowerController : MonoBehaviourPun
{
    public enum PowerupTypes { SpeedBoost, Throwable, Thrown };
    public PowerupTypes powerup;

    public List<GameObject> Triggered = new List<GameObject>();
    public GameObject Thrower;

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

                pv.RPC("DisablePickup", RpcTarget.AllBuffered, null);
                break;

            case PowerupTypes.Thrown:

                if (collision.CompareTag("Player"))
                {
                    if (!collision.GetComponent<PlayerController>().pv.IsMine)
                    {
                        if (!Triggered.Contains(collision.gameObject))
                        {
                            Triggered.Add(collision.gameObject);
                        }

                        Debug.Log(Triggered[Triggered.Count - 1].GetComponent<PlayerController>().UserName);
                    }
                }
                break;
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
