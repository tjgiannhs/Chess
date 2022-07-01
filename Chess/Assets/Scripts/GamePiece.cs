using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePiece : MonoBehaviour
{
    public enum PieceType {Pawn, Bishop, Knight, Rook, Queen, King};

    [SerializeField] PieceType pieceType;
    [SerializeField] int pieceTeam;
    [SerializeField] int pieceValue;
    [SerializeField] int timesMoved=0;
    [SerializeField] int timesAte=0;
    [SerializeField] int profit=0;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public PieceType GetPieceType()
    {
        return pieceType;
    }

    public int GetPieceTeam()
    {
        return pieceTeam;
    }

    public void SetPieceTeam(int i, Color32 c)
    {
        pieceTeam = i;
        GetComponent<Image>().color = c;
    }

    public int GetTimesMoved()
    {
        return timesMoved;
    }
    public int GetTimesEaten()
    {
        return timesAte;
    }
    public int GetProfit()
    {
        return profit;
    }
    public void SetTimesMoved(int t)
    {
        timesMoved = t;
    }
    public void SetTimesEaten(int t)
    {
        timesAte = t;
    }
    public void SetProfit(int p)
    {
        profit = p;
    }
    public int GetPieceValue()
    {
        return pieceValue;
    }

    public void increaseTimesMoved(int i = 0)
    {
        timesMoved+=i;
    }
    public void increaseTimesAte(int i = 0)
    {
        timesAte+=i;
    }
    public void increasePieceProfit(int i = 0)
    {
        profit+=i;
    }
}
