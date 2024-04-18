using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Quest 
{
    public bool isActive;
    
    public string[] color;    
    public float reward;
    public Image colorImage;
    public Image colorFill;

    public Color[] colors = new Color[8];

    public QuestGoal[] goal;

    public void Complete()
    {
        isActive = false;
        Debug.Log("Request was completed");
    }
}
