using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBoardManager : Manager<PlayerBoardManager>
{
    public PlayerBoard PlayerBoard;

    protected override void Awake()
    {
        base.Awake();

        PlayerBoard = new PlayerBoard(true);
    }
}
