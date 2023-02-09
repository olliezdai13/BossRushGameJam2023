using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class BossDoor : MonoBehaviour
{
    public enum BossDoorType
    {
        RAT,
        CAT,
        OWL
    }
    public Sprite spriteOpen;
    public Sprite spriteClosed;
    public float doorOpenDistance = 1f;
    private GameObject player;
    private SpriteRenderer spriteRenderer;
    private bool activated;
    public BossDoorType type;
    void Start()
    {
        activated = false;
        player = GameObject.FindGameObjectWithTag("Player");
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (Unlocked() && Vector2.Distance(player.transform.position, transform.position) < doorOpenDistance)
        {
            if (!activated && (Input.GetAxis("Fire1") >= Mathf.Epsilon || Input.GetKeyDown(KeyCode.E)))
            {
                activated = true;
                if (type == BossDoorType.RAT)
                {
                    GameManager.instance.EnterBoss1();
                }
                if (type == BossDoorType.CAT)
                {
                    GameManager.instance.EnterBoss2();
                }
                if (type == BossDoorType.OWL)
                {
                    GameManager.instance.EnterBoss3();
                }
            }
            spriteRenderer.sprite = spriteOpen;
        } else
        {
            spriteRenderer.sprite = spriteClosed;
        }
    }

    private bool Unlocked()
    {
        if (type == BossDoorType.RAT)
        {
            // Always unlocked
            return true;
        }
        if (type == BossDoorType.CAT)
        {
            return GameData.beatRat || GameData.beatCat || GameData.beatOwl;
        }
        if (type == BossDoorType.OWL)
        {
            return GameData.beatCat || GameData.beatOwl;
        }
        return false;
    }
}
