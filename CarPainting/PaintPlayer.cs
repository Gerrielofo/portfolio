using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PaintPlayer : MonoBehaviour
{
    [SerializeField] ParticleSystem paintParticle;
    [SerializeField] float ammoAddMultiplier;
    [SerializeField] float ammoUseMultiplier;
    public static bool spray;
    [SerializeField] Material paintFill;
    [SerializeField] GameObject glass;

    public Color[] colors;

    float moneyPercent;
    public void Spray()
    {
        spray = !spray;
    }

    private void Update()
    {
        //Calculate how much money you have
        float singlePer = GameManager.Money / 100;
        float total = 1000f;
        float current = GameManager.Money;
        float percent = current / total;
        percent *= 100f;
        moneyPercent = singlePer *= percent;
        moneyPercent /= 500f;
        moneyPercent /= 7f;

        //Play partifcle effect and set shaderFill relative to money amount
        if (spray)
        {
            paintFill.SetFloat("_Fill", moneyPercent);
            paintParticle.Emit(200);
            GameManager.Money -= ammoUseMultiplier * Time.deltaTime;
        }
        else
        {
            paintParticle.Stop();
        }
    }
}
