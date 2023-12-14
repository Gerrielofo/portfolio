using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuestSystem : MonoBehaviour
{
    public Quest quest;
    public GameManager gameManager;

    [SerializeField] TMP_Text _money;

    [Header("Request UI")]
    [SerializeField] GameObject _requestUI;
    [SerializeField] TMP_Text _colorTxt;
    [SerializeField] TMP_Text _rewardTxt;

    [Header("New Request UI")]
    [SerializeField] GameObject _newRequestUI;
    [SerializeField] int _reRolls;
    [SerializeField] TMP_Text _rollsTxt;
    [SerializeField] GameObject _noReRollsUI;

    [Header("Active Request UI")]
    [SerializeField] GameObject _activeRequestUI;
    [SerializeField] TMP_Text _activeColorTxt;
    [SerializeField] Image _colorImage;
    [SerializeField] Image _colorFill;
    [SerializeField] GameObject _glassFill;
    [SerializeField] GameObject _sprayParticle;
    Renderer _sprayRend;
    Renderer _rend;

    public static int colorIndex;

    [Header("Completed Goals")]
    [SerializeField] bool _PaintDone;
    [SerializeField] bool _CleanDone;
    [SerializeField] bool _AssembleDone;
    [SerializeField] Image _paintUX;
    [SerializeField] Image _cleanUX;
    [SerializeField] Image _assembleUX;
    bool _goalsReached;
    [SerializeField] WaypointSystem _waypointSystem;
    [SerializeField] CarSpawner _carSpawner;

    private void Start()
    {
        gameManager = GetComponent<GameManager>();
        _rollsTxt.text = "Re-rolls left:\n" + _reRolls;
        for (int i = 0; i < quest.goal.Length; i++)
        {
            quest.goal[i].Start();
        }
    }

    private void Update()
    {
        _money.text = "Money: " + GameManager.Money.ToString("f0");
        if (_PaintDone && _CleanDone && _AssembleDone && !_goalsReached)
        {
            _goalsReached = true;
            quest.Complete();
            _reRolls = 3;
        }
        else
        {
            #region UX Color Changing
            if (_PaintDone)
            {
                _paintUX.color = Color.green;
            }
            else
            {
                _paintUX.color = Color.red;
            }
            if (_CleanDone)
            {
                _cleanUX.color = Color.green;
            }
            else
            {
                _cleanUX.color = Color.red;
            }
            if (_AssembleDone)
            {
                _assembleUX.color = Color.green;
            }
            else
            {
                _assembleUX.color = Color.red;
            }
            #endregion

            if (quest.goal[0].PaintIsReached())
            {
                _PaintDone = true;
            }
            if (quest.goal[1].CleanedIsReached())
            {
                _CleanDone = true;
            }
            if (quest.goal[2].AssembleIsReached())
            {
                _AssembleDone = true;
            }
        }
    }
    public void OpenRequest()
    {
        quest.goal[0].paintedNeeded = 0;
        quest.goal[1].CleanedNeeded = 0;
        quest.goal[2].AssembleNeeded = 0;
        if (_reRolls <= 0)
        {
            _noReRollsUI.SetActive(true);
            return;
        }
        colorIndex = Random.Range(0, quest.colors.Length);
        _colorTxt.text = quest.color[colorIndex];
        _rewardTxt.text = quest.reward.ToString();
        quest.colorImage.color = quest.colors[colorIndex];
        quest.colorFill.color = quest.colors[colorIndex];
        _reRolls--;
        _rollsTxt.text = "Re-rolls left:" + "\n" + _reRolls;
        _requestUI.SetActive(true);
    }

    public void AcceptRequest()
    {
        gameManager.activeRequests.Add(quest);

        _requestUI.SetActive(false);
        quest.isActive = true;

        _newRequestUI.SetActive(false);

        _activeColorTxt.text = _colorTxt.text;
        _colorImage.color = quest.colors[colorIndex];
        _colorFill.color = quest.colors[colorIndex];
        _activeRequestUI.SetActive(true);

        _carSpawner.SpawnCar();
        _waypointSystem = CarSpawner.currentCar.GetComponent<WaypointSystem>();
        _rend = _glassFill.GetComponent<Renderer>();
        _rend.material.SetColor("_LiquidColor", quest.colors[QuestSystem.colorIndex]);
        _rend.material.SetColor("_SurfaceColor", quest.colors[QuestSystem.colorIndex]);
        _sprayRend = _sprayParticle.GetComponent<Renderer>();
        _sprayRend.material.color = quest.colors[QuestSystem.colorIndex];
    }

    public void PaintCarPart()
    {
        if (quest.isActive)
        {
            quest.goal[0].CarPartPainted();
        }
    }

    public void CleanCarPart()
    {
        if (quest.isActive)
        {
            quest.goal[1].CarPartCleaned();
        }
    }

    public void CarPartAssembled()
    {
        if (quest.isActive)
        {
            quest.goal[2].CarPartAssembled();
        }
    }

    public void CompleteRequest()
    {
        float newReward = quest.reward / 100;
        float total = quest.goal[0].paintedNeeded + quest.goal[1].CleanedNeeded + quest.goal[2].AssembleNeeded;
        float completed = quest.goal[0].paintedAmount + quest.goal[1].CleanedAmount + quest.goal[2].AssembledAmount;
        float percent = completed / total;
        percent *= 100f;
        Debug.Log("Percent of total gained: " + percent);
        newReward *= percent;
        GameManager.Money += newReward;
        Debug.Log("Money gained: " + newReward);

        _waypointSystem.end = false;


        _activeRequestUI.SetActive(false);
        _newRequestUI.SetActive(true);
        _reRolls = 3;
    }
}
