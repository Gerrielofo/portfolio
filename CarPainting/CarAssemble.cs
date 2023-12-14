using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarAssemble : MonoBehaviour
{
    GameObject _carPart;
    [SerializeField] GameObject _snapPos;
    [SerializeField] float _snapDistance = 5f;
    public static bool canSnap;
    public bool isPaintable;
    private QuestSystem _questSystem;
    [SerializeField] bool _isSnapped;

    private void Awake()
    {
        _questSystem = GameObject.Find("GameManager").GetComponent<QuestSystem>();
        _carPart = gameObject;
        if (isPaintable)
        {
            _questSystem.quests.goal[0].paintedNeeded++;
        }
        _questSystem.quests.goal[2].AssembleNeeded++;
    }
    void Update()
    {
        //Check if the car part is close to in place
        if (Vector3.Distance(_carPart.transform.position, _snapPos.transform.position) < _snapDistance && canSnap)
        {
            //Snap car part in place 
            if (!_isSnapped)
            {
                Snap();
                _isSnapped = true;
            }
        }
        else
        {
            if (!canSnap)
                _questSystem.quests.goal[2].AssembledAmount--;
            canSnap = true;
            _isSnapped = false;
        }
    }

    void Snap()
    {
        _carPart.transform.position = _snapPos.transform.position;
        _carPart.transform.rotation = _snapPos.transform.rotation;
        _questSystem.quests.goal[2].AssembledAmount++;
        canSnap = false;
    }
}
