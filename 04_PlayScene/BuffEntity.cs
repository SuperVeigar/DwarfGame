using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffEntity : MonoBehaviour
{
    public Buff buff;
    public float buffTime;
    public float buffElapsedTime;
    public float buffAmount1;   // moveSpd
    public float buffAmount2;   // animSpd
    public bool isHaving;
    public bool isUsing;
    public GameObject buffParticle;
    public GameObject buffOnParticle;

    private bool finishAcknowledgment;
    private float healAmount;
    private AudioSource buffSound;


    // Start is called before the first frame update
    void Start()
    {
        buffSound = GetComponent<AudioSource>();
        Reset();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateBuff()
    {
        if (isUsing)
        {
            buffElapsedTime -= Time.deltaTime;

            if (buff == Buff.heal)
            {
                healAmount = buffAmount1 / buffTime * Time.deltaTime;
            }

            if (buffElapsedTime < 0f)
            {
                buffElapsedTime = 0f;
                isUsing = false;
                finishAcknowledgment = true;
            }
        }
    }

    public bool IsFinished()
    {
        return finishAcknowledgment;
    }

    public void Reset()
    {
        buffTime = 0f;
        buffElapsedTime = 0f;
        buffAmount1 = 0f;
        buffAmount2 = 0f;
        isHaving = false;
        isUsing = false;

        finishAcknowledgment = false;
        healAmount = 0f;

        if (buff != Buff.heal)
        {
            if (buffParticle != null) buffParticle.SetActive(false);
            if (buffOnParticle != null) buffOnParticle.SetActive(false);
        }            
    }

    public void Use()
    {
        isUsing = true;
        buffSound.Play();
        if (buffParticle != null)
        {
            buffParticle.SetActive(false);
            buffParticle.SetActive(true);
        }
        if(buffOnParticle != null)
        {
            buffOnParticle.SetActive(false);
            buffOnParticle.SetActive(true);
        }
    }

    public float GetHealAmount()
    {
        return healAmount;
    }
}
