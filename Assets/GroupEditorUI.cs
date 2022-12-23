using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using MahjongTypes;

[RequireComponent(typeof(RectTransform))]
public class GroupEditorUI : Manager<GroupEditorUI>
{
	// public RectTransform RectTransform => (RectTransform)transform;

	public float EditingClosedGroupHeight = 1110f;
	public float ChangingOpenGroupHeight = 100f;

	public TileGroupUI PreviewGroup;
	public TileGroupUI EditingGroup;

	public CustomToggleGroup EditingType_TG;
	public CustomToggleGroup ChangingType_TG;
	public CustomToggleGroup Suit_TG;
	public CustomToggleGroup NumbersRank_TG;
	public CustomToggleGroup HonorsRank_TG;

    public Toggle HasRedFive_Toggle;
    public Toggle Concealed_Toggle;

    public RectTransform PopupPanel;


	private TileGroupUI _changingGroup;
    public TileGroupUI ChangingGroup => _changingGroup;

	public UnityEvent<bool> OnEditingGroupChanged;
	public UnityEvent<bool> OnChangingGroupChanged;
    public UnityEvent<bool> OnWinningTileUI;
    public UnityEvent<bool> OnNotWinningTileUI;

    public bool Initialized { get; private set; } = false;


    private bool _isWinningTile = false;
    public bool IsWinningTile
    {
        get => _isWinningTile;
        private set => _isWinningTile = value;
    }

	protected override void Awake()
	{
		base.Awake();
	}

    private void Start()
    {
        EditingType_TG.OnChangedEvent.AddListener(_ => UpdateUI());
        ChangingType_TG.OnChangedEvent.AddListener(_ => UpdateUI());
        Suit_TG.OnChangedEvent.AddListener(_ => UpdateUI());
        NumbersRank_TG.OnChangedEvent.AddListener(_ => UpdateUI());
        HonorsRank_TG.OnChangedEvent.AddListener(_ => UpdateUI());
        HasRedFive_Toggle.onValueChanged.AddListener(_ => UpdateUI());
        Concealed_Toggle.onValueChanged.AddListener(_ => UpdateUI());
    }

	public void StartEditingGroup(TileGroupUI group, bool isEditing, bool isWinningTile)
	{
        UnityEngine.Debug.Log("StartEditingGroup(group = " + group.gameObject.name + ", isEditing = " + isEditing + ", isWinningTile = " + isWinningTile + ")", group);

        gameObject.SetActive(true);
		_changingGroup = group;
        
        IsEditing = isEditing;
        IsWinningTile = isWinningTile;

        OnEditingGroupChanged.Invoke(IsEditing);
        OnChangingGroupChanged.Invoke(!IsEditing);
        OnWinningTileUI.Invoke(isWinningTile);
        OnNotWinningTileUI.Invoke(!isWinningTile);

		if (!Initialized)
		{
            if (isEditing)
			    InitializeTogglesAndPreview();
            else Initialized = true;
		}

		

		if (IsEditing)
        {
            CopyGroup(group, EditingGroup);
			EditingGroup.GroupTiles.ForEach(t => t.Interactable = true);
        }
		else
		{
			CopyGroup(group, PreviewGroup);
            SetTogglesToMatchPreview();
		}

        if (isWinningTile)
        {
            ChangingType_TG.SelectedIndex = 0; // Make sure to free up the honor suit
            ChangingType_TG.gameObject.SetActive(false);
        }

		UpdateUI();
	}

	public void StopEditingGroup(bool cancelEditing = false)
	{
        if (cancelEditing)
        {
            if (IsEditing)
                EditingGroup.GroupTiles.ForEach(t => {
                    t.Interactable = false;
                    t.Selected = false;
                });
            gameObject.SetActive(false);
		    _changingGroup = null;
            return;
        }

		while (_changingGroup.GroupTiles.Count > 0)
		{
			var tile = _changingGroup.GroupTiles[0];
			_changingGroup.GroupTiles.RemoveAt(0);
			Destroy(tile.gameObject);
		}

		if (IsEditing)
		{
			EditingGroup.GroupTiles.ForEach(t => {
				t.Interactable = false;
				t.Selected = false;
            });
			MoveGroup(EditingGroup, _changingGroup);
		}
		else
		{
			CopyGroup(PreviewGroup, _changingGroup);
		}

		_changingGroup = null;
        gameObject.SetActive(false);
	}

	public void AddTilesToEditingGroup()
	{
		CopyGroup(PreviewGroup, EditingGroup, false);
	}

	public void RemoveSelectedTiles()
	{
		List<TileUI> tilesToRemove = new List<TileUI>();
		foreach (TileUI t in EditingGroup.GroupTiles)
		{
			if (t.Selected)
			{
				tilesToRemove.Add(t);
			}
		}

		foreach (TileUI t in tilesToRemove)
		{
			EditingGroup.GroupTiles.Remove(t);
			Destroy(t.gameObject);
		}
	}

	private void UpdateUI()
	{
		if (IsEditing)
		{
			PopupPanel.sizeDelta = new Vector2(PopupPanel.sizeDelta.x, EditingClosedGroupHeight);
		}
		else
		{
			PopupPanel.sizeDelta = new Vector2(PopupPanel.sizeDelta.x, ChangingOpenGroupHeight);
		}

		PreviewGroup.ClearTiles();

        Suit suit = (Suit)Suit_TG.SelectedIndex;
		bool needsRedFive = HasRedFive_Toggle.isOn;
		bool isNumber = suit != Suit.Honor;
		Rank rank = IndexToRank(isNumber ? NumbersRank_TG.SelectedIndex : HonorsRank_TG.SelectedIndex, isNumber);

		if (IsWinningTile)
        {
            AddTileToPreview(rank, suit, needsRedFive);
        }
        else if (IsEditing)
        {
            if (EditingType_TG.SelectedIndex != 3)
                for (int i = 0; i < EditingType_TG.SelectedIndex + 1; i++)
                    needsRedFive = AddTileToPreview(rank, suit, needsRedFive);
            else {
                if (rank == Rank.Eight || rank == Rank.Nine)
					rank = Rank.Seven;
				needsRedFive = AddTileToPreview(rank++, suit, needsRedFive);
				needsRedFive = AddTileToPreview(rank++, suit, needsRedFive);
                needsRedFive = AddTileToPreview(rank, suit, needsRedFive);
            }
        }
        else
        {
            if (ChangingType_TG.SelectedIndex == 0)
                for (int i = 0; i < 3; i++)
                    needsRedFive = AddTileToPreview(rank, suit, needsRedFive);
            else if (ChangingType_TG.SelectedIndex == 1)
			{
				if (rank == Rank.Eight || rank == Rank.Nine)
					rank = Rank.Seven;
				needsRedFive = AddTileToPreview(rank++, suit, needsRedFive);
				needsRedFive = AddTileToPreview(rank++, suit, needsRedFive);
                needsRedFive = AddTileToPreview(rank, suit, needsRedFive);
			} else {
				needsRedFive = true;
				needsRedFive = AddTileToPreview(rank, suit, needsRedFive);

                if (!Concealed_Toggle.isOn) {
                    AddTileToPreview(rank, suit, false);
                    AddTileToPreview(rank, suit, false);
                } else {
                    AddTileToPreview(Rank.Back, Suit.Back, false);
                    AddTileToPreview(Rank.Back, Suit.Back, false);
				}

                AddTileToPreview(rank, suit, false);
            }
		}

		HasRedFive_Toggle.gameObject.SetActive(PreviewGroup.GroupTiles.Some(t => t.TileData.Rank == Rank.Five));

		Concealed_Toggle.gameObject.SetActive(!IsEditing && ChangingType_TG.SelectedIndex == 2);
	}


    private bool AddTileToPreview(Rank rank, Suit suit, bool needsRedFive)
    {
        GameObject go = Instantiate(ManualEntryManager.Instance.TilePrefab, PreviewGroup.transform);
        TileUI tile = go.GetComponent<TileUI>();
        tile.TileData = TileCatalog.GetEntry(rank, suit, needsRedFive && rank == Rank.Five);
        PreviewGroup.GroupTiles.Add(tile);

		return rank == Rank.Five ? false : needsRedFive;
	}

	private static void CopyGroup(TileGroupUI from, TileGroupUI to, bool clear = true)
	{
        if (clear) to.ClearTiles();

		foreach (TileUI t in from.GroupTiles)
		{
			GameObject go = Instantiate(ManualEntryManager.Instance.TilePrefab, to.transform);
			TileUI tile = go.GetComponent<TileUI>();
            tile.TileData = t.TileData;
			to.GroupTiles.Add(tile);
		}
	}

	private static void MoveGroup(TileGroupUI from, TileGroupUI to)
	{
		while (from.GroupTiles.Count > 0)
		{
			var tile = from.GroupTiles[0];
			from.GroupTiles.RemoveAt(0);

			tile.transform.SetParent(to.transform);
			to.GroupTiles.Add(tile);
		}
	}

	public void InitializeTogglesAndPreview()
	{
        Initialized = true;

		PreviewGroup.ClearTiles();
		EditingGroup.ClearTiles();

		AddTileToPreview(Rank.One, Suit.Bamboo, false);
		AddTileToPreview(Rank.Two, Suit.Bamboo, false);
		AddTileToPreview(Rank.Three, Suit.Bamboo, false);

        EditingType_TG.SelectedIndex = 0;
        ChangingType_TG.SelectedIndex = 0;
        Suit_TG.SelectedIndex = 0;
        NumbersRank_TG.SelectedIndex = 0;
        HonorsRank_TG.SelectedIndex = 0;

        HasRedFive_Toggle.isOn = false;
        Concealed_Toggle.isOn = false;

		SetTogglesToMatchPreview();
	}

	public void SetTogglesToMatchPreview()
	{
		Rank rank = PreviewGroup.GroupTiles[0].TileData.Rank;
		Suit suit = PreviewGroup.GroupTiles[0].TileData.Suit;
		int groupType = -1;

		if (IsEditing)
		{
            groupType = PreviewGroup.GroupTiles.Count - 1;

            if (groupType == 2 && (
                PreviewGroup.GroupTiles[0].TileData.Rank != PreviewGroup.GroupTiles[1].TileData.Rank ||
                PreviewGroup.GroupTiles[1].TileData.Rank != PreviewGroup.GroupTiles[2].TileData.Rank)
            )
            {
                groupType = 3;
            }

			EditingType_TG.SelectedIndex = groupType;
		}
        else if (!IsWinningTile) {
            if (PreviewGroup.GroupTiles.Count == 4) groupType = 2;
            else
            {
				groupType = PreviewGroup.GroupTiles[0].TileData.Rank != PreviewGroup.GroupTiles[1].TileData.Rank ||
				PreviewGroup.GroupTiles[1].TileData.Rank != PreviewGroup.GroupTiles[2].TileData.Rank ? 1 : 0;
			}
            ChangingType_TG.SelectedIndex = groupType;
		}

        // Activate the correct toggle for the suit
        int suitIndex = (int)suit;
		Suit_TG.SelectedIndex = suitIndex;

		// Activate the correct toggle for the rank
		int rankIndex = RankToIndex(rank);

		if (rank.isNumber())
		{
			NumbersRank_TG.SelectedIndex = rankIndex;
		}
		else
		{
			 HonorsRank_TG.SelectedIndex = rankIndex;
		}

        UnityEngine.Debug.Log("IsEditing: " + IsEditing + " groupType: " + groupType + " Concealed_Toggle.isOn: " + Concealed_Toggle.isOn + " PreviewGroup.GroupTiles.Where(t => t.TileData.Rank == Rank.Back).Count(): " + PreviewGroup.GroupTiles.Where(t => t.TileData.Rank == Rank.Back).Count() + ".");
        if (!IsEditing)
            Concealed_Toggle.isOn = groupType == 2 && PreviewGroup.GroupTiles.Where(t => t.TileData.Rank == Rank.Back).Count() == 2;
	}

    private int RankToIndex(Rank rank)
    {
        switch (rank)
		{
			case Rank.One: return 0;
			case Rank.Two: return 1;
			case Rank.Three: return 2;
			case Rank.Four: return 3;
			case Rank.Five: return 4;
			case Rank.Six: return 5;
			case Rank.Seven: return 6;
			case Rank.Eight: return 7;
			case Rank.Nine: return 8;
			case Rank.EastWind: return 0;
			case Rank.SouthWind: return 1;
			case Rank.WestWind: return 2;
			case Rank.NorthWind: return 3;
			case Rank.GreenDragon: return 4;
			case Rank.RedDragon: return 5;
			case Rank.WhiteDragon: return 6;
			default: return -1;
		}
    }

	private Rank IndexToRank(int index, bool isNumber)
	{
		if (isNumber)
		{
            switch (index)
            {
                case 0: return Rank.One;
                case 1: return Rank.Two;
                case 2: return Rank.Three;
                case 3: return Rank.Four;
                case 4: return Rank.Five;
                case 5: return Rank.Six;
                case 6: return Rank.Seven;
                case 7: return Rank.Eight;
                case 8: return Rank.Nine;
                default: return Rank.NaN;
            }
        }
        else
        {
            switch (index)
            {
                case 0: return Rank.EastWind;
                case 1: return Rank.SouthWind;
                case 2: return Rank.WestWind;
                case 3: return Rank.NorthWind;
                case 4: return Rank.GreenDragon;
                case 5: return Rank.RedDragon;
                case 6: return Rank.WhiteDragon;
                default: return Rank.NaN;
            }
		}
	}

    [SerializeField] private bool _isEditing = false;
    public bool IsEditing { get { return _isEditing; } private set { _isEditing = value; } }


#if UNITY_EDITOR
	private bool previous_isEditing = false;

	private void OnValidate()
	{
		if (!Application.isPlaying && previous_isEditing != _isEditing)
		{
			previous_isEditing = _isEditing;

			UnityEditor.EditorApplication.delayCall += () =>
			{
                OnEditingGroupChanged.Invoke(_isEditing);
                OnChangingGroupChanged.Invoke(!_isEditing);
                if (_isEditing)
                {
                    PopupPanel.sizeDelta = new Vector2(PopupPanel.sizeDelta.x, ChangingOpenGroupHeight);
                }
                else
                {
                    PopupPanel.sizeDelta = new Vector2(PopupPanel.sizeDelta.x, EditingClosedGroupHeight);
                }

			};
		}
	}
#endif
}
