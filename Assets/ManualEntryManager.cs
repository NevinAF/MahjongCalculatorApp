using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MahjongTypes;
using System.Linq;

public class ManualEntryManager : Manager<ManualEntryManager>
{
	public GameObject TilePrefab;

    public GameObject ClickableGroupPrefab;
	public GroupEditorUI GroupEditorUI;
	public ClickableGroup DoraIndicatorsGroup;
	public ClickableGroup ClosedGroup;
	public ClickableGroup DiscardsGroup;
	public ClickableGroup WinningTileGroup;
	public Transform OpenGroupTransform;
	[System.NonSerialized] public List<ClickableGroup> OpenGroups;

    public CustomToggleGroup SeatWindToggleGroup;
    public CustomToggleGroup PrevalentWindToggleGroup;

	public Button AddMeldButton;

	public Toggle TsumoToggle;
    public Toggle RiichiToggle;
    public Toggle DoubleRiichiToggle;
    public Toggle IppatsuToggle;
    public Toggle KanWinToggle;
    public Toggle LastDrawToggle;

    public TMP_Text HandTitleText;
    public TMP_Text HandDescriptionText;

    private void Start()
    {
        UnityEngine.Debug.Log(PlayerBoardManager.Instance.PlayerBoard);

        OpenGroups = new List<ClickableGroup>();
        PlayerBoardManager.Instance.PlayerBoard = new PlayerBoard();

        SeatWindToggleGroup.OnChangedEvent.AddListener(_ => {
            PlayerBoardManager.Instance.PlayerBoard.SeatWind = Rank.EastWind + SeatWindToggleGroup.SelectedIndex;
            UpdateUI();
        });

        PrevalentWindToggleGroup.OnChangedEvent.AddListener(_ => {
            PlayerBoardManager.Instance.PlayerBoard.PrevalentWind = Rank.EastWind + PrevalentWindToggleGroup.SelectedIndex;
            UpdateUI();
        });

        TsumoToggle.onValueChanged.AddListener(value => {
            PlayerBoardManager.Instance.PlayerBoard.Tsumo = value;
            UpdateUI();
        });

        RiichiToggle.onValueChanged.AddListener(value => {
            PlayerBoardManager.Instance.PlayerBoard.Riichi = value;
            UpdateUI();
        });

        DoubleRiichiToggle.onValueChanged.AddListener(value => {
            PlayerBoardManager.Instance.PlayerBoard.DoubleRiichi = value;
            UpdateUI();
        });

        IppatsuToggle.onValueChanged.AddListener(value => {
            PlayerBoardManager.Instance.PlayerBoard.Ippatsu = value;
            UpdateUI();
        });

        KanWinToggle.onValueChanged.AddListener(value => {
            PlayerBoardManager.Instance.PlayerBoard.KanWin = value;
            UpdateUI();
        });

        LastDrawToggle.onValueChanged.AddListener(value => {
            PlayerBoardManager.Instance.PlayerBoard.LastDraw = value;
            UpdateUI();
        });

        DiscardsGroup.Button.onClick.AddListener(() => { OpenGroupEditor(DiscardsGroup.TileGroupUI); });
        ClosedGroup.Button.onClick.AddListener(() => { OpenGroupEditor(ClosedGroup.TileGroupUI); });
        DoraIndicatorsGroup.Button.onClick.AddListener(() => { OpenGroupEditor(DoraIndicatorsGroup.TileGroupUI); });
        WinningTileGroup.Button.onClick.AddListener(() => { OpenGroupEditor(WinningTileGroup.TileGroupUI); });

        AddMeldButton.onClick.AddListener(() => AddOpenGroup());

		MatchPlayerBoard();

		UpdateUI();
    }

    public void MatchPlayerBoard()
    {
        SeatWindToggleGroup.SelectedIndex = PlayerBoardManager.Instance.PlayerBoard.SeatWind - Rank.EastWind;
        PrevalentWindToggleGroup.SelectedIndex = PlayerBoardManager.Instance.PlayerBoard.PrevalentWind - Rank.EastWind;
        TsumoToggle.isOn = PlayerBoardManager.Instance.PlayerBoard.Tsumo;
        RiichiToggle.isOn = PlayerBoardManager.Instance.PlayerBoard.Riichi;
        DoubleRiichiToggle.isOn = PlayerBoardManager.Instance.PlayerBoard.DoubleRiichi;
        IppatsuToggle.isOn = PlayerBoardManager.Instance.PlayerBoard.Ippatsu;
        KanWinToggle.isOn = PlayerBoardManager.Instance.PlayerBoard.KanWin;
        LastDrawToggle.isOn = PlayerBoardManager.Instance.PlayerBoard.LastDraw;

		ClearGroups();

		FillGroupWithTiles(DiscardsGroup.TileGroupUI, PlayerBoardManager.Instance.PlayerBoard.Discards);
        FillGroupWithTiles(ClosedGroup.TileGroupUI, PlayerBoardManager.Instance.PlayerBoard.HiddenHandTiles);
        FillGroupWithTiles(DoraIndicatorsGroup.TileGroupUI, PlayerBoardManager.Instance.PlayerBoard.DoraIndicators);
        FillGroupWithTiles(WinningTileGroup.TileGroupUI, new Tile[] { PlayerBoardManager.Instance.PlayerBoard.WinningTile });
        foreach (Tile[] g in PlayerBoardManager.Instance.PlayerBoard.VisibleHandTiles)
        {
            var group = Instantiate(ClickableGroupPrefab, OpenGroupTransform).GetComponent<ClickableGroup>();
            group.Button.onClick.AddListener(() => { OpenGroupEditor(group.TileGroupUI); });
            OpenGroups.Add(group);
            FillGroupWithTiles(group.TileGroupUI, g);
        }
    }

	public void OpenGroupEditor(TileGroupUI tileGroupUI)
    {
		GroupEditorUI.StartEditingGroup(
            tileGroupUI,
            tileGroupUI == ClosedGroup.TileGroupUI || tileGroupUI == DoraIndicatorsGroup.TileGroupUI || tileGroupUI == DiscardsGroup.TileGroupUI,
            tileGroupUI == WinningTileGroup.TileGroupUI);
	}

    public void CloseGroupEditor(bool deleteGroup = false)
    {
		var changingGroup = GroupEditorUI.ChangingGroup;
		GroupEditorUI.StopEditingGroup(deleteGroup);

        if (deleteGroup)
        {
            if (changingGroup == ClosedGroup.TileGroupUI)
            {
                ClosedGroup.TileGroupUI.ClearTiles();
            }
            else if (changingGroup == DoraIndicatorsGroup.TileGroupUI)
            {
                DoraIndicatorsGroup.TileGroupUI.ClearTiles();
            }
            else if (changingGroup == WinningTileGroup.TileGroupUI)
            {
                WinningTileGroup.TileGroupUI.ClearTiles();
            }
            else if (changingGroup == DiscardsGroup.TileGroupUI)
            {
                DiscardsGroup.TileGroupUI.ClearTiles();
            }
            else
            {
				var x = changingGroup.GetComponentInParent<ClickableGroup>();
                OpenGroups.Remove(x);
                Destroy(x.gameObject);
            }
        }

        if (changingGroup == ClosedGroup.TileGroupUI)
        {
            PlayerBoardManager.Instance.PlayerBoard.HiddenHandTiles = ClosedGroup.TileGroupUI.GroupTiles.Select(t => GetTileFromTileUI(t)).ToArray();
        }
        else if (changingGroup == DoraIndicatorsGroup.TileGroupUI)
        {
            PlayerBoardManager.Instance.PlayerBoard.DoraIndicators = DoraIndicatorsGroup.TileGroupUI.GroupTiles.Select(t => GetTileFromTileUI(t)).ToArray();
        }
        else if (changingGroup == WinningTileGroup.TileGroupUI)
        {
            PlayerBoardManager.Instance.PlayerBoard.WinningTile = GetTileFromTileUI(WinningTileGroup.TileGroupUI.GroupTiles[0]);
        } else if (changingGroup == DiscardsGroup.TileGroupUI)
        {
            PlayerBoardManager.Instance.PlayerBoard.Discards = DiscardsGroup.TileGroupUI.GroupTiles.Select(t => GetTileFromTileUI(t)).ToArray();
        }
        else
        {
            PlayerBoardManager.Instance.PlayerBoard.VisibleHandTiles = OpenGroups.Select(g => g.TileGroupUI.GroupTiles.Select(t => GetTileFromTileUI(t)).ToArray()).ToArray();
        }

        UpdateUI();
    }


    public void UpdateUI()
    {
        if (PlayerBoardManager.Instance.PlayerBoard.IsValid)
        {
            HandTitleText.text = PlayerBoardManager.Instance.PlayerBoard.WinTitle;
            HandDescriptionText.text = PlayerBoardManager.Instance.PlayerBoard.Wins.Select(win => win.name).Aggregate((a, b) => a + "\n" + b) +
                "\n" + PlayerBoardManager.Instance.PlayerBoard.PayOrder;
        }
        else
        {
            HandTitleText.text = "Invalid Hand";
            HandDescriptionText.text = PlayerBoardManager.Instance.PlayerBoard.handError;
        }

		UnityEngine.Debug.Log(PlayerBoardManager.Instance.PlayerBoard.DumpToString());


		AddMeldButton.interactable = OpenGroups.Count < 4;
    }

	// public void StartWinningTileSelection()
	// {
    //     for (int i = 0; i < OpenGroups.Count; i++)
    //     {
    //         OpenGroups[i].TileGroupUI.Tiles.ForEach(tileUI => tileUI.focusable = true);
	// 	}
    //     ClosedGroup.TileGroupUI.Tiles.ForEach(tileUI => tileUI.focusable = true);
	// }

    // public void StopWinningTileSelection()
    // {
    //     for (int i = 0; i < OpenGroups.Count; i++)
    //     {
    //         OpenGroups[i].TileGroupUI.Tiles.ForEach(tileUI => tileUI.focusable = false);
    //     }
    //     ClosedGroup.TileGroupUI.Tiles.ForEach(tileUI => tileUI.focusable = false);
    // }


    private void ClearGroups()
    {
        for (int i = 0; i < OpenGroups.Count; i++)
        {
            Destroy(OpenGroups[i].gameObject);
        }
        OpenGroups.Clear();
        DoraIndicatorsGroup.TileGroupUI.ClearTiles();
        ClosedGroup.TileGroupUI.ClearTiles();
        DiscardsGroup.TileGroupUI.ClearTiles();
        WinningTileGroup.TileGroupUI.ClearTiles();
    }

    public void AddOpenGroup()
    {
        GameObject newGroup = Instantiate(ClickableGroupPrefab, OpenGroupTransform);
        ClickableGroup newGroupUI = newGroup.GetComponent<ClickableGroup>();
        OpenGroups.Add(newGroupUI);
		FillGroupWithTiles(newGroupUI.TileGroupUI, new Tile[3] {
            new Tile(Rank.One, Suit.Bamboo, false),
            new Tile(Rank.Two, Suit.Bamboo, false),
            new Tile(Rank.Three, Suit.Bamboo, false)
        });
		newGroupUI.Button.onClick.AddListener(() => OpenGroupEditor(newGroupUI.TileGroupUI));

        PlayerBoardManager.Instance.PlayerBoard.VisibleHandTiles = OpenGroups.Select(g => g.TileGroupUI.GroupTiles.Select(t => GetTileFromTileUI(t)).ToArray()).ToArray();

		UpdateUI();
	}


    public static TileUI GetTileUIFromTile(Tile tile, Transform parent = null)
    {
        if (parent == null)
        {
            parent = Instance.transform;
        }

        GameObject go = Instantiate(Instance.TilePrefab, parent);
        TileUI tileUI = go.GetComponent<TileUI>();
        if (tile.IsFaceDown)
            tileUI.TileData = TileCatalog.GetEntry(Rank.Back, Suit.Back, false);
        else tileUI.TileData = TileCatalog.GetEntry(tile.Rank, tile.Suit, tile.IsRed && tile.Rank == Rank.Five);

		return tileUI;
    }

    public static void FillGroupWithTiles(TileGroupUI group, Tile[] tiles)
    {
        foreach (var tile in tiles)
        {
            var ui = GetTileUIFromTile(tile, group.transform);
            group.GroupTiles.Add(ui);
        }
    }

    public static Tile GetTileFromTileUI(TileUI tileUI)
    {
        return new Tile(tileUI.TileData.Suit, tileUI.TileData.Rank, tileUI.TileData.IsRed);
    }

}
