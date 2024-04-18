using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static bool GameOver;
    public static float Money;
    public static bool isPainting;
    public static int completedQuests;

    public TMP_Text moneyTxt;

    public List<Quest> activeRequests;

    void Start()
    {
        Money = 1000f;
    }

    void Update()
    {
        if (Money < 100 && !isPainting)
        {
            GameOver = true;
        }
        moneyTxt.text = Money.ToString("f0");
    }
}
