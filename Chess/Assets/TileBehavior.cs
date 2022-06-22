﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileBehavior : MonoBehaviour
{

    GameObject overlay;
    GameManagerBehavior gameManagerBehavior;

    // Start is called before the first frame update
    void Start()
    {
        overlay = transform.parent.parent.GetChild(transform.parent.parent.childCount-1).gameObject;
        gameManagerBehavior = transform.parent.parent.GetChild(0).GetComponent<GameManagerBehavior>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerDown()
    {
        if(transform.childCount > 1)
        {
            gameManagerBehavior.SetHeldPiece(transform.GetSiblingIndex());
            transform.GetChild(1).SetParent(overlay.transform);
        }
    }

    public void OnPointerUp()
    {            
        if(gameManagerBehavior.GetHoldingPiece()) 
        {
            if(!gameManagerBehavior.CheckIfMoveLegal(transform.GetSiblingIndex())) 
            {
                gameManagerBehavior.CancelPieceMovement();
                return;
            }

            if(transform.childCount>1 && gameManagerBehavior.CheckIfMoveEats(transform.GetSiblingIndex()))
            {
                transform.GetChild(1).gameObject.SetActive(false);
                transform.GetChild(1).SetParent(overlay.transform);
                
                overlay.transform.GetChild(0).GetComponent<GamePiece>().increaseTimesAte(1);
                overlay.transform.GetChild(0).GetComponent<GamePiece>().increasePieceProfit(overlay.transform.GetChild(1).GetComponent<GamePiece>().GetPieceValue());
                
                Destroy(overlay.transform.GetChild(1).gameObject);
            }
            
            overlay.transform.GetChild(0).GetComponent<GamePiece>().increaseTimesMoved(1);
            overlay.transform.GetChild(0).SetParent(transform);
            transform.GetChild(1).transform.localPosition = new Vector3(0,0,0);
        }
        gameManagerBehavior.CancelHoldingPiece();
    }

    public void ShowTileMark()
    {
        transform.GetChild(0).gameObject.SetActive(true);
    }

    public void HideTileMark()
    {
        transform.GetChild(0).gameObject.SetActive(false);
    }
}