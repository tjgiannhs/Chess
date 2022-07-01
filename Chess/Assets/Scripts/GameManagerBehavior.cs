using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManagerBehavior : MonoBehaviour
{
    bool holdingPiece = false;
    int heldPieceParentSiblingIndex = -1;
    GameObject board;
    bool playingWhite;
    [SerializeField] GameObject tile;
    [SerializeField] int columns;
    [SerializeField] int rows;
    [SerializeField] Color32 whitePieceColor;
    [SerializeField] Color32 blackPieceColor;
    [SerializeField] Color32 whiteTileColor;
    [SerializeField] Color32 blackTileColor;
    [SerializeField] List<GameObject> allPieces;
    GameObject overlay;
    List<int> availableTiles;

    // Start is called before the first frame update
    void Start()
    {
        playingWhite = GameObject.FindGameObjectWithTag("ConnectionManager").GetComponent<ConnectionManagerScript>().GetPlayingWhite();
        board = transform.parent.GetChild(1).gameObject;
        overlay = transform.parent.GetChild(transform.parent.childCount-1).gameObject;
        availableTiles = new List<int>();
        SetupBoard();
        //confine cursor inside the window
        //Cursor.lockState = CursorLockMode.Confined;
    }

    // Update is called once per frame
    void Update()
    {
        if(holdingPiece)
        {
            overlay.transform.GetChild(0).position = Input.mousePosition;
        }
    }

    public void CancelHoldingPiece()
    {
        holdingPiece = false;
        HideAllTileMarks();
        heldPieceParentSiblingIndex = -1;
        availableTiles.Clear();
    }

    public void CancelPieceMovement()
    {
        overlay.transform.GetChild(0).SetParent(board.transform.GetChild(heldPieceParentSiblingIndex));
        board.transform.GetChild(heldPieceParentSiblingIndex).GetChild(1).transform.localPosition = new Vector3(0,0,0);
        CancelHoldingPiece();
    }
    public void OnPointerUp()
    {
        if(holdingPiece){
            Transform parentTileTransform = board.transform.GetChild(heldPieceParentSiblingIndex).transform;
            overlay.transform.GetChild(0).SetParent(parentTileTransform);
            parentTileTransform.GetChild(1).transform.localPosition = new Vector3(0,0,0);
        }
        CancelHoldingPiece();
    }

    public void SetHeldPiece(int parentSiblingIndex)
    {
        if(parentSiblingIndex < 0) 
        {
            return;
        }
        holdingPiece = true;
        heldPieceParentSiblingIndex = parentSiblingIndex;
        
        availableTiles = GetPossibleTilesIndexes();

        for(int i=0; i<board.transform.childCount; i++)
        {
            if(i!=parentSiblingIndex && availableTiles.Contains(i))
            {
                board.transform.GetChild(i).GetComponent<TileBehavior>().ShowTileMark();
            }
        }
    }

    void HideAllTileMarks()
    {
        for(int i=0; i<board.transform.childCount; i++)
        {

            board.transform.GetChild(i).GetComponent<TileBehavior>().HideTileMark();
        }
    }

    List<int> GetPossibleTilesIndexes()
    {
        List<Vector2> availableTilesGridPositions = CalculatePossibleTileOptionsGridPositions();
        List<int> availableTilesIndexes = new List<int>();

        int c;

        for(int i=0; i<availableTilesGridPositions.Count; i++)
        {
            c = GetSiblingIndexByGridPosition(availableTilesGridPositions[i]);
            availableTilesIndexes.Add(c);
        }
        
        return availableTilesIndexes;
    }

    int GetSiblingIndexByGridPosition(Vector2 gridPosition)
    {
        int s = (int)gridPosition.x + (int)gridPosition.y*columns;
        return s;
    }

    List<Vector2> CalculatePossibleTileOptionsGridPositions()
    {
        Vector2 tileGridPosition = GetGridPositionBySiblingIndex(heldPieceParentSiblingIndex);
        List<Vector2> availableTilePositions = new List<Vector2>();
        int teamIndex = board.transform.GetChild(heldPieceParentSiblingIndex).GetChild(1).GetComponent<GamePiece>().GetPieceTeam();

        switch(board.transform.GetChild(heldPieceParentSiblingIndex).GetChild(1).GetComponent<GamePiece>().GetPieceType())
        {
            case GamePiece.PieceType.Pawn:
            {
                availableTilePositions = GetPawnAvailableMovementTiles(tileGridPosition, teamIndex);
                break;
            }
            case GamePiece.PieceType.Bishop:
            {
                availableTilePositions = GetBishopAvailableMovementTiles(tileGridPosition, teamIndex);
                break;
            }
            case GamePiece.PieceType.Knight:
            {
                availableTilePositions = GetKnightAvailableMovementTiles(tileGridPosition, teamIndex);
                break;
            }case GamePiece.PieceType.Rook:
            {
                availableTilePositions = GetRookAvailableMovementTiles(tileGridPosition, teamIndex);
                break;   
            }case GamePiece.PieceType.Queen:
            {
                //no duplicates in the list because the movement of the rook and the knight are mutually exclusive
                availableTilePositions.AddRange(GetBishopAvailableMovementTiles(tileGridPosition, teamIndex));
                availableTilePositions.AddRange(GetRookAvailableMovementTiles(tileGridPosition, teamIndex));
                break;
            }case GamePiece.PieceType.King:
            {
                availableTilePositions = GetKingAvailableMovementTiles(tileGridPosition,teamIndex);
                break;
            }default: break;
        }
        return availableTilePositions;
    }

    bool CheckIfGridPositionOccupied(Vector2 gridPosition)
    {
        int s = GetSiblingIndexByGridPosition(gridPosition);
        if(board.transform.GetChild(s).childCount>1)
        {
            return true;
        }
        return false;
    }

    bool CheckIfGridPositionOccupied(Vector2 gridPosition, int team)
    {
        int s = GetSiblingIndexByGridPosition(gridPosition);
        if(board.transform.GetChild(s).childCount>1)
        {
            if(board.transform.GetChild(s).GetChild(1).GetComponent<GamePiece>().GetPieceTeam()==team)
            {
                return true;
            }
        }
        return false;
    }
    bool CheckIfGridPositionOccupiedByFriendly(Vector2 gridPosition, int team)
    {
        int s = GetSiblingIndexByGridPosition(gridPosition);
        if(board.transform.GetChild(s).GetChild(1).GetComponent<GamePiece>().GetPieceTeam()==team)
        {
            return true;
        }
        return false;
    }

    List<Vector2> GetPawnAvailableMovementTiles(Vector2 pawnGridPosition, int team)
    {
        List<Vector2> pawnAvailableGridPositions = new List<Vector2>();
        int oppositeTeam = 1-team;
        if(team==0)
        {
            if(board.transform.GetChild(GetSiblingIndexByGridPosition(pawnGridPosition)).GetChild(1).GetComponent<GamePiece>().GetTimesMoved()==0)
                if(pawnGridPosition.y+2<=rows-1 && !CheckIfGridPositionOccupied(new Vector2(pawnGridPosition.x,pawnGridPosition.y+1)))
                    pawnAvailableGridPositions.Add(new Vector2(pawnGridPosition.x,pawnGridPosition.y+2));

            if(pawnGridPosition.y != rows-1)
            {
                Vector2 newGridPosition = new Vector2(pawnGridPosition.x,pawnGridPosition.y+1);
                if(!CheckIfGridPositionOccupied(newGridPosition))
                {
                    pawnAvailableGridPositions.Add(newGridPosition);
                }

                if(pawnGridPosition.x>0)
                {
                    newGridPosition = new Vector2(pawnGridPosition.x-1,pawnGridPosition.y+1);
                    if(CheckIfGridPositionOccupied(newGridPosition,oppositeTeam))
                    {
                        pawnAvailableGridPositions.Add(newGridPosition);
                    }
                }

                if(pawnGridPosition.x<columns-1)
                {
                    newGridPosition = new Vector2(pawnGridPosition.x+1,pawnGridPosition.y+1);
                    if(CheckIfGridPositionOccupied(newGridPosition,oppositeTeam))
                    {
                        pawnAvailableGridPositions.Add(newGridPosition);
                    }
                }
            }
        }else
        {
            if(board.transform.GetChild(GetSiblingIndexByGridPosition(pawnGridPosition)).GetChild(1).GetComponent<GamePiece>().GetTimesMoved()==0)
                if(pawnGridPosition.y-2>=0 && !CheckIfGridPositionOccupied(new Vector2(pawnGridPosition.x,pawnGridPosition.y-1)))
                    pawnAvailableGridPositions.Add(new Vector2(pawnGridPosition.x,pawnGridPosition.y-2));

            if(pawnGridPosition.y != 0)
            {
                Vector2 newGridPosition = new Vector2(pawnGridPosition.x,pawnGridPosition.y-1);
                if(!CheckIfGridPositionOccupied(newGridPosition))
                {
                    pawnAvailableGridPositions.Add(newGridPosition);
                }
                
                if(pawnGridPosition.x<columns-1)
                {
                    newGridPosition = new Vector2(pawnGridPosition.x+1,pawnGridPosition.y-1);
                    if(CheckIfGridPositionOccupied(newGridPosition,oppositeTeam))
                    {
                        pawnAvailableGridPositions.Add(newGridPosition);
                    }
                }

                if(pawnGridPosition.x>0)
                {
                    newGridPosition = new Vector2(pawnGridPosition.x-1,pawnGridPosition.y-1);
                    if(CheckIfGridPositionOccupied(newGridPosition,oppositeTeam))
                    {
                        pawnAvailableGridPositions.Add(newGridPosition);
                    }
                }
            }
        }

        return pawnAvailableGridPositions;
    }

    List<Vector2> GetKingAvailableMovementTiles(Vector2 kingGridPosition, int team)
    {   
        List<Vector2> kingAvailableGridPositions = new List<Vector2>();
        Vector2 newGridPosition;
        if(kingGridPosition.x != 0)
        {   
            if(kingGridPosition.y != 0)
            {
                newGridPosition = new Vector2(kingGridPosition.x-1,kingGridPosition.y-1);
                if(!CheckIfGridPositionOccupied(newGridPosition,team))
                    kingAvailableGridPositions.Add(newGridPosition);
            }
            if(kingGridPosition.y != rows-1)
            {
                newGridPosition = new Vector2(kingGridPosition.x-1,kingGridPosition.y+1);
                if(!CheckIfGridPositionOccupied(newGridPosition,team))
                    kingAvailableGridPositions.Add(newGridPosition);
            }
            newGridPosition = new Vector2(kingGridPosition.x-1,kingGridPosition.y);
            if(!CheckIfGridPositionOccupied(newGridPosition,team))
                kingAvailableGridPositions.Add(newGridPosition);
        }
        if(kingGridPosition.x != columns-1)
        {
            if(kingGridPosition.y != 0)
            {
                newGridPosition = new Vector2(kingGridPosition.x+1,kingGridPosition.y-1);
                if(!CheckIfGridPositionOccupied(newGridPosition,team))
                    kingAvailableGridPositions.Add(newGridPosition);
            }
            if(kingGridPosition.y != rows-1)
            {
                newGridPosition = new Vector2(kingGridPosition.x+1,kingGridPosition.y+1);
                if(!CheckIfGridPositionOccupied(newGridPosition,team))
                    kingAvailableGridPositions.Add(newGridPosition);
            }
            newGridPosition = new Vector2(kingGridPosition.x+1,kingGridPosition.y);
            if(!CheckIfGridPositionOccupied(newGridPosition,team))
                kingAvailableGridPositions.Add(newGridPosition);
        }
        if(kingGridPosition.y != 0)
        {
            newGridPosition = new Vector2(kingGridPosition.x,kingGridPosition.y-1);
            if(!CheckIfGridPositionOccupied(newGridPosition,team))
                kingAvailableGridPositions.Add(newGridPosition);
        }
        if(kingGridPosition.y != rows-1)
        {
            newGridPosition = new Vector2(kingGridPosition.x,kingGridPosition.y+1);
            if(!CheckIfGridPositionOccupied(newGridPosition,team))
                kingAvailableGridPositions.Add(newGridPosition); 
        }
      
        //Roke
        if(board.transform.GetChild(GetSiblingIndexByGridPosition(kingGridPosition)).GetChild(1).GetComponent<GamePiece>().GetTimesMoved()!=0)
        {
            if(kingAvailableGridPositions.Count>0)
                for(int i=kingAvailableGridPositions.Count-1; i>=0; i--)
                {
                    if(CheckIfSquareNextToEnemyKing(kingAvailableGridPositions[i],team))
                    kingAvailableGridPositions.RemoveAt(i);
                }
            return kingAvailableGridPositions;
        }

        if(kingGridPosition.y==rows-1 || kingGridPosition.y == 0)
        {
            if(kingGridPosition.x+3<=columns-1)
            if(!CheckIfGridPositionOccupied(new Vector2 (kingGridPosition.x+1,kingGridPosition.y)) && !CheckIfGridPositionOccupied(new Vector2 (kingGridPosition.x+2,kingGridPosition.y))) 
            {
                if(CheckIfGridPositionOccupied(new Vector2 (kingGridPosition.x+3,kingGridPosition.y)) && board.transform.GetChild(GetSiblingIndexByGridPosition(kingGridPosition)+3).GetChild(1).GetComponent<GamePiece>().GetPieceType() == GamePiece.PieceType.Rook) 
                    kingAvailableGridPositions.Add(new Vector2 (kingGridPosition.x+2,kingGridPosition.y));
                if(kingGridPosition.x+4<=columns-1)
                if(!CheckIfGridPositionOccupied(new Vector2 (kingGridPosition.x+3,kingGridPosition.y)))
                if(CheckIfGridPositionOccupied(new Vector2 (kingGridPosition.x+4,kingGridPosition.y)) && board.transform.GetChild(GetSiblingIndexByGridPosition(kingGridPosition)+4).GetChild(1).GetComponent<GamePiece>().GetPieceType() == GamePiece.PieceType.Rook) 
                    kingAvailableGridPositions.Add(new Vector2 (kingGridPosition.x+2,kingGridPosition.y));
            }
            if(kingGridPosition.x-3>=0)
            if(!CheckIfGridPositionOccupied(new Vector2 (kingGridPosition.x-1,kingGridPosition.y)) && !CheckIfGridPositionOccupied(new Vector2 (kingGridPosition.x-2,kingGridPosition.y))) 
            {
                if(CheckIfGridPositionOccupied(new Vector2 (kingGridPosition.x-3,kingGridPosition.y)) && board.transform.GetChild(GetSiblingIndexByGridPosition(kingGridPosition)-3).GetChild(1).GetComponent<GamePiece>().GetPieceType() == GamePiece.PieceType.Rook) 
                    kingAvailableGridPositions.Add(new Vector2 (kingGridPosition.x-2,kingGridPosition.y));
                if(kingGridPosition.x-4>=0)
                if(!CheckIfGridPositionOccupied(new Vector2 (kingGridPosition.x-3,kingGridPosition.y)))
                if(CheckIfGridPositionOccupied(new Vector2 (kingGridPosition.x-4,kingGridPosition.y)) && board.transform.GetChild(GetSiblingIndexByGridPosition(kingGridPosition)-4).GetChild(1).GetComponent<GamePiece>().GetPieceType() == GamePiece.PieceType.Rook) 
                    kingAvailableGridPositions.Add(new Vector2 (kingGridPosition.x-2,kingGridPosition.y));
            }
        }

        if(kingAvailableGridPositions.Count>0)
            for(int i=kingAvailableGridPositions.Count-1; i>=0; i--)
            {
                if(CheckIfSquareNextToEnemyKing(kingAvailableGridPositions[i],team))
                kingAvailableGridPositions.RemoveAt(i);
            }
            
        return kingAvailableGridPositions;

    }


    bool CheckIfSquareNextToEnemyKing(Vector2 gridPosition, int team){
        Vector2 newGridPosition;
        GamePiece gp;
        if(gridPosition.x != 0)
        {   
            if(gridPosition.y != 0)
            {
                newGridPosition = new Vector2(gridPosition.x-1,gridPosition.y-1);
                if(CheckIfGridPositionOccupied(newGridPosition))
                {
                    gp = board.transform.GetChild(GetSiblingIndexByGridPosition(newGridPosition)).GetChild(1).GetComponent<GamePiece>();
                    if(gp.GetPieceType()==GamePiece.PieceType.King && gp.GetPieceTeam()!=team)
                        return true;
                }
            }
            if(gridPosition.y != rows-1)
            {
                newGridPosition = new Vector2(gridPosition.x-1,gridPosition.y+1);
                if(CheckIfGridPositionOccupied(newGridPosition))
                {
                    gp = board.transform.GetChild(GetSiblingIndexByGridPosition(newGridPosition)).GetChild(1).GetComponent<GamePiece>();
                    if(gp.GetPieceType()==GamePiece.PieceType.King && gp.GetPieceTeam()!=team)
                        return true;
                }
            }
            newGridPosition = new Vector2(gridPosition.x-1,gridPosition.y);
            if(CheckIfGridPositionOccupied(newGridPosition))
            {
                gp = board.transform.GetChild(GetSiblingIndexByGridPosition(newGridPosition)).GetChild(1).GetComponent<GamePiece>();
                if(gp.GetPieceType()==GamePiece.PieceType.King && gp.GetPieceTeam()!=team)
                    return true;
            }       
        }
        
        if(gridPosition.x != columns-1)
        {
            if(gridPosition.y != 0)
            {
                newGridPosition = new Vector2(gridPosition.x+1,gridPosition.y-1);
                if(CheckIfGridPositionOccupied(newGridPosition))
                {
                    gp = board.transform.GetChild(GetSiblingIndexByGridPosition(newGridPosition)).GetChild(1).GetComponent<GamePiece>();
                    if(gp.GetPieceType()==GamePiece.PieceType.King && gp.GetPieceTeam()!=team)
                        return true;
                }
            }
            if(gridPosition.y != rows-1)
            {
                newGridPosition = new Vector2(gridPosition.x+1,gridPosition.y+1);
                if(CheckIfGridPositionOccupied(newGridPosition))
                {
                    gp = board.transform.GetChild(GetSiblingIndexByGridPosition(newGridPosition)).GetChild(1).GetComponent<GamePiece>();
                    if(gp.GetPieceType()==GamePiece.PieceType.King && gp.GetPieceTeam()!=team)
                        return true;
                }
            }
            newGridPosition = new Vector2(gridPosition.x+1,gridPosition.y);
            if(CheckIfGridPositionOccupied(newGridPosition))
            {
                gp = board.transform.GetChild(GetSiblingIndexByGridPosition(newGridPosition)).GetChild(1).GetComponent<GamePiece>();
                if(gp.GetPieceType()==GamePiece.PieceType.King && gp.GetPieceTeam()!=team)
                    return true;
            }
        }

        if(gridPosition.y != 0)
        {
            newGridPosition = new Vector2(gridPosition.x,gridPosition.y-1);
            if(CheckIfGridPositionOccupied(newGridPosition))
            {
                gp = board.transform.GetChild(GetSiblingIndexByGridPosition(newGridPosition)).GetChild(1).GetComponent<GamePiece>();
                if(gp.GetPieceType()==GamePiece.PieceType.King && gp.GetPieceTeam()!=team)
                    return true;
            }
        }
        if(gridPosition.y != rows-1)
        {
            newGridPosition = new Vector2(gridPosition.x,gridPosition.y+1);
            if(CheckIfGridPositionOccupied(newGridPosition))
            {
                gp = board.transform.GetChild(GetSiblingIndexByGridPosition(newGridPosition)).GetChild(1).GetComponent<GamePiece>();
                if(gp.GetPieceType()==GamePiece.PieceType.King && gp.GetPieceTeam()!=team)
                    return true;
            }
        }

        return false;
    }

    List<Vector2> GetKnightAvailableMovementTiles(Vector2 knightGridPosition, int team)
    {   
        Vector2 temp = knightGridPosition;
        List<Vector2> knightAvailableGridPositions = new List<Vector2>();
        if(knightGridPosition.x-2>=0 && knightGridPosition.y-1>=0) {
            if(!CheckIfGridPositionOccupied(knightGridPosition+new Vector2(-2,-1),team))
            knightAvailableGridPositions.Add(knightGridPosition+new Vector2(-2,-1));};
        if(knightGridPosition.x+2<columns && knightGridPosition.y-1>=0) { 
            if(!CheckIfGridPositionOccupied(knightGridPosition+new Vector2(2,-1),team))
            knightAvailableGridPositions.Add(knightGridPosition+new Vector2(2,-1));};
        if(knightGridPosition.x-2>=0 && knightGridPosition.y+1<rows) { 
            if(!CheckIfGridPositionOccupied(knightGridPosition+new Vector2(-2,1),team))
            knightAvailableGridPositions.Add(knightGridPosition+new Vector2(-2,1));};
        if(knightGridPosition.x+2<columns && knightGridPosition.y+1<rows) { 
            if(!CheckIfGridPositionOccupied(knightGridPosition+new Vector2(2,1),team))
            knightAvailableGridPositions.Add(knightGridPosition+new Vector2(2,1));};
        if(knightGridPosition.x+1<columns && knightGridPosition.y+2<rows) { 
            if(!CheckIfGridPositionOccupied(knightGridPosition+new Vector2(1,2),team))
            knightAvailableGridPositions.Add(knightGridPosition+new Vector2(1,2));};
        if(knightGridPosition.x-1>=0 && knightGridPosition.y+2<rows) { 
            if(!CheckIfGridPositionOccupied(knightGridPosition+new Vector2(-1,2),team))
            knightAvailableGridPositions.Add(knightGridPosition+new Vector2(-1,2));};
        if(knightGridPosition.x-1>=0 && knightGridPosition.y-2>=0) { 
            if(!CheckIfGridPositionOccupied(knightGridPosition+new Vector2(-1,-2),team))
            knightAvailableGridPositions.Add(knightGridPosition+new Vector2(-1,-2));};
        if(knightGridPosition.x+1<columns && knightGridPosition.y-2>=0) { 
            if(!CheckIfGridPositionOccupied(knightGridPosition+new Vector2(1,-2),team))
            knightAvailableGridPositions.Add(knightGridPosition+new Vector2(1,-2));};

        return knightAvailableGridPositions;
    }

    List<Vector2> GetBishopAvailableMovementTiles(Vector2 bishopGridPosition, int team)
    {
        Vector2 temp = bishopGridPosition;
        List<Vector2> bishopAvailableGridPositions = new List<Vector2>();
        temp.x--;
        temp.y--;
        while(temp.x >= 0 && temp.y >=0)
        {
            if(CheckIfGridPositionOccupied(temp)) {if(!CheckIfGridPositionOccupiedByFriendly(temp,team)) bishopAvailableGridPositions.Add(temp); break;}
            bishopAvailableGridPositions.Add(temp);
            temp.x--;
            temp.y--;
        }    
        temp = bishopGridPosition;
        temp.x--;
        temp.y++;
        while(temp.x >= 0 && temp.y <rows)
        {
            if(CheckIfGridPositionOccupied(temp)) {if(!CheckIfGridPositionOccupiedByFriendly(temp,team)) bishopAvailableGridPositions.Add(temp); break;}
            bishopAvailableGridPositions.Add(temp);
            temp.x--;
            temp.y++;
        }  
        temp = bishopGridPosition;
        temp.x++;
        temp.y--;
        while(temp.x < columns && temp.y >=0)
        {
            if(CheckIfGridPositionOccupied(temp)) {if(!CheckIfGridPositionOccupiedByFriendly(temp,team)) bishopAvailableGridPositions.Add(temp); break;}
            bishopAvailableGridPositions.Add(temp);
            temp.x++;
            temp.y--;
        }  
        temp = bishopGridPosition;
        temp.x++;
        temp.y++;
        while(temp.x < columns && temp.y < rows)
        {
            if(CheckIfGridPositionOccupied(temp)) {if(!CheckIfGridPositionOccupiedByFriendly(temp,team)) bishopAvailableGridPositions.Add(temp); break;}
            bishopAvailableGridPositions.Add(temp);
            temp.x++;
            temp.y++;
        }  

        return bishopAvailableGridPositions;
    }

    List<Vector2> GetRookAvailableMovementTiles(Vector2 rookGridPosition, int team)
    {   
        Vector2 temp = rookGridPosition;
        List<Vector2> rookAvailableGridPositions = new List<Vector2>();
        temp.x--;
        while(temp.x>=0)
        {
            if(CheckIfGridPositionOccupied(temp)) {if(!CheckIfGridPositionOccupiedByFriendly(temp,team)) rookAvailableGridPositions.Add(temp); break;}
            rookAvailableGridPositions.Add(temp);
            temp.x--;
        }
        temp = rookGridPosition;
        temp.x++;
        while(temp.x < columns)
        {
            if(CheckIfGridPositionOccupied(temp)) {if(!CheckIfGridPositionOccupiedByFriendly(temp,team)) rookAvailableGridPositions.Add(temp); break;}
            rookAvailableGridPositions.Add(temp);
            temp.x++;
        }
        temp = rookGridPosition;
        temp.y--;
        while(temp.y>=0)
        {
            if(CheckIfGridPositionOccupied(temp)) {if(!CheckIfGridPositionOccupiedByFriendly(temp,team)) rookAvailableGridPositions.Add(temp); break;}
            rookAvailableGridPositions.Add(temp);
            temp.y--;
        }
        temp = rookGridPosition;
        temp.y++;
        while(temp.y < rows)
        {
            if(CheckIfGridPositionOccupied(temp)) {if(!CheckIfGridPositionOccupiedByFriendly(temp,team)) rookAvailableGridPositions.Add(temp); break;}
            rookAvailableGridPositions.Add(temp);
            temp.y++;
        }
        return rookAvailableGridPositions;
    }

    Vector2 GetGridPositionBySiblingIndex(int siblingIndex)
    {
        Vector2 gridPosition = new Vector2();
        gridPosition.x = siblingIndex % columns;
        gridPosition.y = siblingIndex / columns;
        
        return gridPosition;

    }

    public bool CheckIfMoveLegal(int targetSiblingIndex)
    {
        if(availableTiles.Contains(targetSiblingIndex))
        {
            return true;
        }
        return false;
    }

    public bool CheckIfMoveEats(int targetSiblingIndex)
    {   

        if(overlay.transform.GetChild(0).GetComponent<GamePiece>().GetPieceTeam() != board.transform.GetChild(targetSiblingIndex).transform.GetChild(1).GetComponent<GamePiece>().GetPieceTeam())
        {
            return true;
        }
        return false;
    }

    public bool GetHoldingPiece()
    {
        return holdingPiece;
    }

    List<Vector2> RemoveDuplicatesFromList(List<Vector2> theList)
    {
        List<Vector2> tempList = new List<Vector2>();
        for(int i=0; i<theList.Count; i++)
        {
            if(!tempList.Contains(theList[i]))
            {
                tempList.Add(theList[i]);
            }
        }
        return tempList;
    }

    void SpawnPieceByNumberCode(int boardPositionIndex, int pieceNumberCode, int timesMoved, int timesEaten, int profit, bool whiteTeam)
    {
        GameObject piece = Instantiate(allPieces[pieceNumberCode-1],new Vector3(0,0,0),Quaternion.identity,board.transform.GetChild(boardPositionIndex));
        piece.GetComponent<GamePiece>().SetPieceTeam(whiteTeam?0:1,whiteTeam?whitePieceColor:blackPieceColor);
        piece.GetComponent<GamePiece>().SetTimesMoved(timesMoved);
        piece.GetComponent<GamePiece>().SetTimesEaten(timesEaten);
        piece.GetComponent<GamePiece>().SetProfit(profit);
        piece.transform.localPosition = new Vector3(0,0,0);
    }
    void DespawnPieceByGridIndex(int index)
    {
        board.transform.GetChild(index).GetChild(1).gameObject.SetActive(false);
        board.transform.GetChild(index).GetChild(1).transform.SetParent(overlay.transform);
    }
    public void UpdateBoard(List<int> gridNumbersCodeList)
    {
        for(int i=0; i<gridNumbersCodeList.Count; i++)
        {
            if(gridNumbersCodeList[i]/1000000%100!=0)//piece exists on tile
            {
                if(board.transform.GetChild(i).childCount==1)
                {
                    int absGridNumberCode = Mathf.Abs(gridNumbersCodeList[i]);
                    SpawnPieceByNumberCode(i, (absGridNumberCode/1000000)%100, absGridNumberCode%100, (absGridNumberCode/100)%100, (absGridNumberCode/10000)%100, gridNumbersCodeList[i]>0);  
                }else if(board.transform.GetChild(i).childCount==2)
                {
                    if(board.transform.GetChild(i).childCount==2)
                    {
                        DespawnPieceByGridIndex(i);
                    }
                    int absGridNumberCode = Mathf.Abs(gridNumbersCodeList[i]);
                    SpawnPieceByNumberCode(i, (absGridNumberCode/1000000)%100, absGridNumberCode%100, (absGridNumberCode/100)%100, (absGridNumberCode/10000)%100, gridNumbersCodeList[i]>0);  
                }  
            }else
            {
                if(board.transform.GetChild(i).childCount==2)
                {
                    DespawnPieceByGridIndex(i);
                }
            }
        }

        for (int i=overlay.transform.childCount-1; i>=0; i--)
        {
            Destroy(overlay.transform.GetChild(i).gameObject);
        }
    }

    void SetupBoard()
    {
        if(!playingWhite)
        {
            board.GetComponent<GridLayoutGroup>().startCorner = GridLayoutGroup.Corner.UpperRight;
        }
        GenerateBoard();
        SetupPieces();        
    }

    void SetupPieces()
    {
        SetupWhitePieces();
        SetupBlackPieces();
    }

    void SetupWhitePieces()
    {
        GameObject g;
        for(int i=8; i<16; i++)
        {
            g = Instantiate(allPieces[0],new Vector3(0,0,0),Quaternion.identity,board.transform.GetChild(i));
            g.GetComponent<GamePiece>().SetPieceTeam(0,whitePieceColor);
        }
        Instantiate(allPieces[3],new Vector3(0,0,0),Quaternion.identity,board.transform.GetChild(0)).GetComponent<GamePiece>().SetPieceTeam(0,whitePieceColor);
        Instantiate(allPieces[3],new Vector3(0,0,0),Quaternion.identity,board.transform.GetChild(7)).GetComponent<GamePiece>().SetPieceTeam(0,whitePieceColor);
        Instantiate(allPieces[1],new Vector3(0,0,0),Quaternion.identity,board.transform.GetChild(1)).GetComponent<GamePiece>().SetPieceTeam(0,whitePieceColor);
        Instantiate(allPieces[1],new Vector3(0,0,0),Quaternion.identity,board.transform.GetChild(6)).GetComponent<GamePiece>().SetPieceTeam(0,whitePieceColor);
        Instantiate(allPieces[2],new Vector3(0,0,0),Quaternion.identity,board.transform.GetChild(2)).GetComponent<GamePiece>().SetPieceTeam(0,whitePieceColor);
        Instantiate(allPieces[2],new Vector3(0,0,0),Quaternion.identity,board.transform.GetChild(5)).GetComponent<GamePiece>().SetPieceTeam(0,whitePieceColor);
        Instantiate(allPieces[4],new Vector3(0,0,0),Quaternion.identity,board.transform.GetChild(3)).GetComponent<GamePiece>().SetPieceTeam(0,whitePieceColor);
        Instantiate(allPieces[5],new Vector3(0,0,0),Quaternion.identity,board.transform.GetChild(4)).GetComponent<GamePiece>().SetPieceTeam(0,whitePieceColor);
    }

    void SetupBlackPieces()
    {
        for(int i=48; i<56; i++)
        {
            Instantiate(allPieces[0],new Vector3(0,0,0),Quaternion.identity,board.transform.GetChild(i)).GetComponent<GamePiece>().SetPieceTeam(1,blackPieceColor);
        }
        Instantiate(allPieces[3],new Vector3(0,0,0),Quaternion.identity,board.transform.GetChild(56)).GetComponent<GamePiece>().SetPieceTeam(1,blackPieceColor);
        Instantiate(allPieces[3],new Vector3(0,0,0),Quaternion.identity,board.transform.GetChild(63)).GetComponent<GamePiece>().SetPieceTeam(1,blackPieceColor);
        Instantiate(allPieces[1],new Vector3(0,0,0),Quaternion.identity,board.transform.GetChild(57)).GetComponent<GamePiece>().SetPieceTeam(1,blackPieceColor);
        Instantiate(allPieces[1],new Vector3(0,0,0),Quaternion.identity,board.transform.GetChild(62)).GetComponent<GamePiece>().SetPieceTeam(1,blackPieceColor);
        Instantiate(allPieces[2],new Vector3(0,0,0),Quaternion.identity,board.transform.GetChild(58)).GetComponent<GamePiece>().SetPieceTeam(1,blackPieceColor);
        Instantiate(allPieces[2],new Vector3(0,0,0),Quaternion.identity,board.transform.GetChild(61)).GetComponent<GamePiece>().SetPieceTeam(1,blackPieceColor);
        Instantiate(allPieces[4],new Vector3(0,0,0),Quaternion.identity,board.transform.GetChild(59)).GetComponent<GamePiece>().SetPieceTeam(1,blackPieceColor);
        Instantiate(allPieces[5],new Vector3(0,0,0),Quaternion.identity,board.transform.GetChild(60)).GetComponent<GamePiece>().SetPieceTeam(1,blackPieceColor);
    }

    void GenerateBoard()
    {   
        DestroyBoardTiles();
        board.GetComponent<GridLayoutGroup>().constraintCount = columns;
        for(int i=0; i<rows*columns; i++)
        {
            Instantiate(tile,new Vector3(0,0,0),Quaternion.identity,board.transform);
        }
        
        for(int i=0;i<board.transform.childCount;i++)
        {
            board.transform.GetChild(i).GetComponent<Image>().color = (i%columns + i/columns)%2==1?blackTileColor:whiteTileColor;
            //print(i + ": "+((i%columns + i/columns)%2==1));
        }
    }

    void DestroyBoardTiles()
    {
        for(int i=board.transform.childCount-1; i>=0; i--)
        {
            board.transform.GetChild(i).gameObject.SetActive(false);
            board.transform.GetChild(i).transform.SetParent(overlay.transform);
        }
        for (int i=overlay.transform.childCount-1; i>=0; i--)
        {
            Destroy(overlay.transform.GetChild(i).gameObject);
        }
    }
}
