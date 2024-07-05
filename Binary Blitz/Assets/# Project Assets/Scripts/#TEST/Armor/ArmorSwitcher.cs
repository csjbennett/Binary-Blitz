using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmorSwitcher : MonoBehaviour
{
    [Header("Helmet")]
    public SpriteRenderer helmetMain;
    public Sprite[] helmetMainSprites;
    public SpriteRenderer helmetVisor;
    public Sprite[] helmetVisorSprites;
    private int helmetNum = 0;

    [Header("Torso")]
    public SpriteRenderer torsoMain;
    public Sprite[] torsoMainSprites;
    public SpriteRenderer torsoAccent;
    public Sprite[] torsoAccentSprites;
    private int torsoNum = 0;

    [Header("Front Arm")]
    public SpriteRenderer armUFMain;
    public Sprite[] armUFMainSprites;
    public SpriteRenderer armUFAccent;
    public Sprite[] armUFAccentSprites;
    public SpriteRenderer armLFMain;
    public Sprite[] armLFMainSprites;
    public SpriteRenderer armLFAccent;
    public Sprite[] armLFAccentSprites;

    [Header("Rear Arm")]
    public SpriteRenderer armURMain;
    public Sprite[] armURMainSprites;
    public SpriteRenderer armURAccent;
    public Sprite[] armURAccentSprites;
    public SpriteRenderer armLRMain;
    public Sprite[] armLRMainSprites;
    public SpriteRenderer armLRAccent;
    public Sprite[] armLRAccentSprites;

    private int armNum = 0;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            helmetNum = (helmetNum + 1) % helmetMainSprites.Length;

            helmetMain.sprite = helmetMainSprites[helmetNum];
            helmetVisor.sprite = helmetVisorSprites[helmetNum];
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            torsoNum = (torsoNum + 1) % torsoMainSprites.Length;

            torsoMain.sprite = torsoMainSprites[torsoNum];
            torsoAccent.sprite = torsoAccentSprites[torsoNum];
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            armNum = (armNum + 1) % armUFMainSprites.Length;

            armUFMain.sprite = armUFMainSprites[armNum];
            armUFAccent.sprite = armUFAccentSprites[armNum];
            armLFMain.sprite = armLFMainSprites[armNum];
            armLFAccent.sprite = armLFAccentSprites[armNum];

            armURMain.sprite = armURMainSprites[armNum];
            armURAccent.sprite = armURAccentSprites[armNum];
            armLRMain.sprite = armLRMainSprites[armNum];
            armLRAccent.sprite = armLRAccentSprites[armNum];
        }
    }
}
