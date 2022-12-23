using MahjongTypes;
using System.Collections.Generic;
using System.Linq;

public class PlayerBoard
{
	private Tile[] _allTilesInPlay;
	public Tile[] AllTilesInPlay
	{
		get => _allTilesInPlay ?? (_allTilesInPlay =
			AllHandTiles
				.Concat(DoraIndicators)
				.Concat(Discards)
				.ToArray()
		);
		private set {
			if (value == _allTilesInPlay) return;

			_validFlippedTiles = null;
			_validBoard = null;
			_allTilesInPlay = value;
		}
	}

	private Tile[] _allHandTiles;
	public Tile[] AllHandTiles
	{
		get => _allHandTiles ?? (_allHandTiles = AllHandGroups.SelectMany(g => g.Tiles).ToArray());
		private set {
			if (value ==  _allHandTiles) return;

			AllTilesInPlay = null; suitsInHand = null; RedFives = null;
			clearSuitBasedWins();
			_validHandSize = null;
			_allHandTiles = value;
		}
	}

	private TileGroup[] _allHandGroups;
	public TileGroup[] AllHandGroups
	{
		get
		{
			if (_allHandGroups != null) return _allHandGroups;
			if (WinningTile == null) return null;

			_allHandGroups = HandGrouper.CreateClosedGroups(HiddenHandTiles.Concat(new Tile[] { WinningTile }).ToList());

			_allHandGroups = _allHandGroups.Concat(_visibleHandGroups).ToArray();
			return _allHandGroups;
		}
		private set {
			if (value == _allHandGroups) return;
		
			AllHandTiles = null; ClosedGroups = null; OpenGroups = null;
			TripletGroups = null; KanGroups = null; TupletGroups = null;
			SequenceGroups = null; PairGroup = null; SequenceGroups = null;
			WinningTileGroup = null;
			hasAllTripletsWin = null; _validGroups = null;
			_allHandGroups = value;
		}
	}

	private Tile[] _closedTiles;
	public Tile[] ClosedTiles
	{
		get => _closedTiles ?? (_closedTiles = ClosedGroups.SelectMany(g => g.Tiles).ToArray());
		private set { _closedTiles = value; }
	}

	private Tile[] _openTiles;
	public Tile[] openTiles
	{
		get => _openTiles ?? (_openTiles = OpenGroups.SelectMany(g => g.Tiles).ToArray());
		private set { _openTiles = value; }
	}

	private TileGroup[] _closedGroups;
	public TileGroup[] ClosedGroups
	{
		get => _closedGroups ?? (_closedGroups = AllHandGroups.Where(g => g.Closed).ToArray());
		private set
		{
			if (value == _closedGroups) return;

			closedHand = null; ClosedTiles = null;
			hasSevenPairsWin = null; hasThirteenOrphansWin = null;
			_closedGroups = value;
		}
	}

	private TileGroup[] _openGroups;
	public TileGroup[] OpenGroups 
	{
		get => _openGroups ?? (_openGroups = AllHandGroups.Where(g => !g.Closed).ToArray());
		private set
		{
			if (value == _openGroups) return;

			openTiles = null; openHand = null;
			_openGroups = value;
		}
	}

	private TileGroup[] _sequenceGroups;
	public TileGroup[] SequenceGroups 
	{
		get => _sequenceGroups ?? (_sequenceGroups =
			AllHandGroups.Where(g => g.Type == TileGroupType.Sequence).ToArray()
		);
		private set
		{
			if (value == _sequenceGroups) return;
			
			isPinfuHand = null; pureDoubleSequenceCount = null;
			hasMixedTripleSequenceWin = null; hasPureStraightWin = null;
			_sequenceGroups = value;
		}
	}

	private TileGroup _pairGroup;
	public TileGroup PairGroup
	{
		get => _pairGroup ?? (_pairGroup =
			AllHandGroups.FirstOrDefault(g => g.Type == TileGroupType.Pair)
		);
		private set { _pairGroup = value; }
	}

	private TileGroup[] _tripletGroups;
	public TileGroup[] TripletGroups
	{
		get => _tripletGroups ?? (_tripletGroups =
			AllHandGroups.Where(g => g.Type == TileGroupType.Triplet).ToArray()
		);
		private set { _tripletGroups = value; }
	}

	private TileGroup[] _kanGroups;
	public TileGroup[] KanGroups
	{
		get => _kanGroups ?? (_kanGroups =
			AllHandGroups.Where(g => g.Type == TileGroupType.Kan).ToArray()
		);
		private set { 
			if (value == _kanGroups) return;
			
			hasAllTripletsWin = null; hasFourKansWin = null;
			_validHandSize = null;_validScoreParameters = null;
			_kanGroups = value;
		}
	}

	private TileGroup[] _dragonGroups;
	public TileGroup[] DragonGroups
	{
		get => _dragonGroups ?? (_dragonGroups =
			TupletGroups.Where(group =>
				group.Tiles[0].Suit == Suit.Honor &&
				group.Tiles[0].Rank.isDragon()
			).ToArray()
		);
		private set {
			if (value == _dragonGroups) return;

			hasGreenDragonYakuhaiWin = null; hasLittleThreeDragonsWin = null;
			hasBigThreeDragonsWin = null;
			_dragonGroups = value;
		}
	}

	private TileGroup[] _windGroups;
	public TileGroup[] WindGroups
	{
		get => _windGroups ?? (_windGroups =
			TupletGroups.Where(group =>
				group.Tiles[0].Suit == Suit.Honor &&
				group.Tiles[0].Rank.isWind()
			).ToArray()
		);
		private set {
			if (value == _windGroups) return;

			hasSeatWindWin = null; hasPrevalentWindWin = null;
			hasLittleFourWindsWin = null; hasBigFourWindsWin = null;
			_windGroups = value;
		}
	}

	private TileGroup[] _tupletGroups;
	public TileGroup[] TupletGroups
	{
		get => _tupletGroups ?? (_tupletGroups =
			TripletGroups.Concat(KanGroups).ToArray()
		);
		private set { 
			if (value == _tupletGroups) return;

			DragonGroups = null; WindGroups = null;
			ConcealedTuplets = null; hasSequenceTripletsWin = null;
			_tupletGroups = value;
		}
	}

	private Tile[] _redFives;
	public Tile[] RedFives
	{
		get => _redFives ?? (_redFives =
			AllHandTiles.Where(t => t.IsRed).ToArray()
		);
		private set { _redFives = value; }
	}

	private TileGroup[] _concealedTuplets;
	public TileGroup[] ConcealedTuplets
	{
		get
		{
			if (_concealedTuplets != null) return _concealedTuplets;

			var temp_concealedTuplets = TupletGroups.Where(g => g.Closed);

			if (ron)
				temp_concealedTuplets = temp_concealedTuplets.Where(g => g != WinningTileGroup);

			return _concealedTuplets = temp_concealedTuplets.ToArray();
		}
		private set {
			if (value == _concealedTuplets) return;

			hasThreeConcealedTripletsWin = null;
			hasFourConcealedTripletsWin = null;
			hasSingleWaitFourConcealedTripletsWin = null;
			_concealedTuplets = value;
		}
	}

	private Tile[] _hiddenHandTiles = null;
	/// <summary>
    /// Externally set data. Tiles in the player's hand that are not visible to the other players (no melds, closed kans, or winning tile)
    /// </summary>
	public Tile[] HiddenHandTiles
	{
		get => _hiddenHandTiles;
		set
		{
			_hiddenHandTiles = value;
			AllHandGroups = null;

			handChanged();
		}
	}

	private Tile[][] _visibleHandTiles = null;
	/// <summary>
    /// Externally set data. Tile groups in the player's hand that are visible to the other players (melds, closed kans)
    /// </summary>
	public Tile[][] VisibleHandTiles
	{
		get => _visibleHandTiles;
		set
		{
			_visibleHandTiles = value;
			AllHandGroups = null;
			handChanged();

			if (value != null)
			{
				_visibleHandGroups = value.Select(group => new TileGroup(group, false)).ToArray();
			}
			else _visibleHandGroups = null;
		}
	}

	private TileGroup[] _visibleHandGroups = null;
	public TileGroup[] VisibleHandGroups
	{
		get => _visibleHandGroups;
		private set {
			if (value == _visibleHandGroups) return;

			handChanged();
			AllHandGroups = null;
			_visibleHandGroups = value;
		}
	}

	private Tile _winningTile = null;
	/// <summary>
    /// Externally set data. Winning tile of the hand.
    /// </summary>
	public Tile WinningTile
	{
		get => _winningTile;
		set
		{
			if (value == _winningTile) return;
			
			handChanged();
			AllHandGroups = null;
			_winningTile = value;
		}
	}

	private TileGroup _winningTileGroup;
	public TileGroup WinningTileGroup
	{
		get => _winningTileGroup;
		private set
		{
			if (value == _winningTileGroup) return;

			WinningTile = null;
			WinningTileGroup = null;
			_winningTileGroup = value;
		}
	}

	private Tile[] _doraIndicators = null;
	/// <summary>
    /// Externally set data. Dora indicators for the round.
    /// </summary>
	public Tile[] DoraIndicators
	{
		get => _doraIndicators;
		set
		{
			handChanged();
			AllTilesInPlay = null;
			_doraIndicators = value;
		}
	}

	private Tile[] _discards = null;
	/// <summary>
    /// Externally set data. Discards from the player this round. This should only include the discards from the player that is being evaluated.
    /// </summary>
	public Tile[] Discards
	{
		get => _discards;
		set
		{
			if (value == _discards) return;

			handChanged();
			AllTilesInPlay = null;
			_discards = value;
		}
	}

	private Rank _seatWind = Rank.EastWind;
	/// <summary>
    /// Externally set data. Seat wind of the evaluating player for this round.
    /// </summary>
	public Rank SeatWind
	{
		get => _seatWind;
		set
		{
			if (_seatWind == value) return;

			handChanged();
			_validScoreParameters = null;
			hasSeatWindWin = null;
			_seatWind = value;
		}
	}

	private Rank _prevalentWind = Rank.EastWind;
	/// <summary>
    /// Externally set data. Prevalent wind of the round.
    /// </summary>
	public Rank PrevalentWind
	{
		get => _prevalentWind;
		set
		{
			if (_prevalentWind == value) return;

			_prevalentWind = value;

			handChanged();
			_validScoreParameters = null;
			hasPrevalentWindWin = null;
		}
	}

	private bool _tsumo = false;
	/// <summary>
    /// Externally set data. Whether the hand was a tsumo (true, winning tile is a draw) or ron (false, winning tile is a discard).
    /// </summary>
	public bool Tsumo
	{
		get => _tsumo;
		set
		{
			if (_tsumo == value) return;

			_tsumo = value;

			handChanged();
			hasMenzenchinTsumoWin = null;
			hasRinshanKaihouWin = null;
			hasRobbingAKanWin = null;
			hasUnderTheSeaWin = null;
			hasUnderTheRiverWin = null;
		}
	}

	/// <summary>
    /// Externally set data. Whether the hand was a ron (true, winning tile is a discard) or tsumo (false, winning tile is a draw).
    /// </summary>
	public bool ron
	{
		get => !_tsumo;
		set => Tsumo = !value;
	}

	private bool _riichi = false;
	/// <summary>
    /// Externally set data. Whether the hand was a riichi (player have a closed hand and called riichi).
    /// </summary>
	public bool Riichi
	{
		get => _riichi;
		set
		{
			if (_riichi == value) return;

			_validScoreParameters = null;
			hasRiichiWin = null; hasDoubleRiichiWin = null;
			_riichi = value;
		}
	}

	private bool _doubleRiichi = false;
	/// <summary>
	/// Externally set data. Whether the hand was a double riichi (player have a closed hand, called riichi, and won before the next round).
	/// </summary>
	public bool DoubleRiichi
	{
		get => _doubleRiichi;
		set
		{
			if (_doubleRiichi == value) return;

			_doubleRiichi = value;

			handChanged();
			_validScoreParameters = null;
			hasRiichiWin = null; hasDoubleRiichiWin = null;
		}
	}

	private bool _ippatsu = false;
	/// <summary>
    /// Externally set data. Whether the hand was an ippatsu (player called riichi and won on the first discard).
    /// </summary>
	public bool Ippatsu
	{
		get => _ippatsu;
		set
		{
			if (_ippatsu == value) return;

			_ippatsu = value;

			handChanged();
			_validScoreParameters = null;
			hasIppatsuWin = null;
		}
	}

	private bool _kanWin = false;
	/// <summary>
    /// Externally set data. Whether the winning tile was a kan win (player won with a kan).
    /// </summary>
	public bool KanWin
	{
		get => _kanWin;
		set
		{
			if (_kanWin == value) return;

			_kanWin = value;

			handChanged();
			_validScoreParameters = null;
			_hasRinshanKaihouWin = null;
			hasRobbingAKanWin = null;
		}
	}

	private bool _lastDraw = false;
	/// <summary>
    /// Externally set data. Whether the winning tile was a last draw win (player won with the last tile in the wall).
    /// </summary>
	public bool LastDraw
	{
		get => _lastDraw;
		set
		{
			if (_lastDraw == value) return;

			_lastDraw = value;

			handChanged();
			hasUnderTheRiverWin = null;
			hasUnderTheSeaWin = null;
		}
	}

	private bool? _validHandSize = null;
	public bool ValidateHandSize()
	{
		if (_validHandSize.HasValue) return _validHandSize.Value;

		_validHandSize = (AllHandTiles.Length - KanGroups.Length) == 14;
		if (_validHandSize.Value) return true;

		int handCount = AllHandTiles.Length - KanGroups.Length;
		if (handCount > 14)
		{
			throw new InvalidHandAndTilesError("The hand has too many tiles, with " + (handCount - 14) + " extra tiles: " + AllHandTiles.Length + " Total, with " + HiddenHandTiles.Length + " closed tiles, " + (AllHandTiles.Length - HiddenHandTiles.Length) + " open tiles.\nHand size should be " + (14 + KanGroups.Length) + " tiles given there " + (KanGroups.Length == 1 ? "is" : "are") + " " + KanGroups.Length + " kan group" + (KanGroups.Length == 1 ? "" : "s") + ".");
		}
		else
		{
			throw new InvalidHandAndTilesError("The hand has too few tiles, with " + (14 - handCount) + " tiles missing: " + AllHandTiles.Length + " Total, with " + HiddenHandTiles.Length + " closed tiles, " + (AllHandTiles.Length - HiddenHandTiles.Length) + " open tiles.\nHand size should be " + (14 + KanGroups.Length) + " tiles given there " + (KanGroups.Length == 1 ? "is" : "are") + " " + KanGroups.Length + " kan group" + (KanGroups.Length == 1 ? "" : "s") + ".");
		}
	}

	private bool? _validFlippedTiles = null;
	/**
	 * Validates that all face-down tiles resolve to a valid tile. All tiles that are included in the board state must be resolvable to a suit and a rank.
	 * @returns True if the boards flipped down tiles are valid, false if it is not.
	 */
	public bool ValidateFlippedTiles()
	{
		if (_validFlippedTiles.HasValue) return _validFlippedTiles.Value;
	
		_validFlippedTiles = AllTilesInPlay.All(t => t.Rank != Rank.Back && t.Suit != Suit.Back);
		if (_validFlippedTiles.Value) return true;

		throw new InvalidHandAndTilesError("Board cannot consist of unknown tile backs! Tile backs are reserved for closed kans (classified as an open group as the tiles cannot be grouped in any other way as they might be in a closed hand).");
	}

	private bool? _validBoard = null;
	public bool ValidateBoard()
	{
		if (_validBoard.HasValue) return _validBoard.Value;

		ValidateTileQuintuplets();

		if (Tile.DoubleRedFives(RedFives))
		{
			throw new InvalidHandAndTilesError("There are multiple red fives of the same suit! List of Red Fives: " + AllTilesInPlay.Where(t => t.IsRed).Format(", ", "", ""));
		}

		return (_validBoard = true).Value;
	}

	private void ValidateTileQuintuplets()
	{
		if (AllTilesInPlay.Length < 5)
			return;

		for (int i = 0; i < AllTilesInPlay.Length - 4; i++)
		{
			int matches = 1;

			for (int j = i + 1; j < AllTilesInPlay.Length && AllTilesInPlay.Length - j > 5 - matches; j++)
			{
				if (AllTilesInPlay[i].softEquals(AllTilesInPlay[j]))
				{
					matches++;
				}
			}

			if (matches >= 5)
			{
				throw new InvalidHandAndTilesError($"There are {matches} copies of the {AllTilesInPlay[i].LongToString()} tiles showing in the dora indicators / hand / discard!");
			}
		}
	}

	private bool? _validScoreParameters = null;
	public bool ValidateScoreParameters()
	{
		if (_validScoreParameters.HasValue) return _validScoreParameters.Value;

		if (openHand.Value && Riichi)
			throw new InvalidScoreArgumentsError("Cannot riichi on a non-closed hand");

		if (DoubleRiichi && !Riichi)
			throw new InvalidScoreArgumentsError("Cannot have double riichi without riichi");

		if (Ippatsu && !Riichi)
			throw new InvalidScoreArgumentsError("Cannot have ippatsu without riichi");
		
		if (!PrevalentWind.isWind())
			throw new InvalidScoreArgumentsError("Prevalent wind is not a wind tile");
		
		if (!SeatWind.isWind())
			throw new InvalidScoreArgumentsError("Player wind is not a wind tile");
		
		if (KanWin && KanGroups.Length == 0)
			throw new InvalidScoreArgumentsError("Cannot have kan win without a kan");
		
		return (_validScoreParameters = true).Value;
	}

	private bool? _validGroups = false;
	public bool ValidateGroups()
	{
		if (_validGroups.HasValue) return _validGroups.Value;

		var error_group = AllHandGroups.FirstOrDefault(g => g.Type == TileGroupType.Error);

		if (error_group != null)
		{
			throw new InvalidHandAndTilesError("Invalid Tile Group: " + error_group.Error!);
		}

		return (_validGroups = true).Value;
	}

	private bool? _closedHand;
	public bool? closedHand
	{
		get => _closedHand ?? (_closedHand = OpenGroups.Length == 0);
		private set {
			if (value == _closedHand) return;

			_validScoreParameters = null;
			hasMenzenchinTsumoWin = null; hasPinfuWin = null;
			hasThreeConcealedTripletsWin = null;
			hasFourConcealedTripletsWin = null;
			hasSingleWaitFourConcealedTripletsWin = null;
			_closedHand = value;
		}
	}

	public bool? openHand
	{
		get => !closedHand;
		private set => closedHand = !value;
	}


	private bool? _hasRiiichiWin;
	public bool? hasRiichiWin
	{
		get => _hasRiiichiWin ?? (_hasRiiichiWin = Riichi && !DoubleRiichi);
		private set => _hasRiiichiWin = value;
	}

	private bool? _hasDoubleRiichiWin;
	public bool? hasDoubleRiichiWin
	{
		get => _hasDoubleRiichiWin ?? (_hasDoubleRiichiWin = Riichi && DoubleRiichi);
		private set => _hasDoubleRiichiWin = value;
	}

	private bool? _hasIppatsuWin;
	public bool? hasIppatsuWin
	{
		get => _hasIppatsuWin ?? (_hasIppatsuWin = Ippatsu);
		private set => _hasIppatsuWin = value;
	}

	private bool? _hasMenzenchinTsumoWin;
	public bool? hasMenzenchinTsumoWin
	{
		get => _hasMenzenchinTsumoWin ?? (_hasMenzenchinTsumoWin = closedHand.Value && Tsumo);
		private set => _hasMenzenchinTsumoWin = value;
	}

	private bool? _hasUnderTheSeaWin;
	public bool? hasUnderTheSeaWin
	{
		get => _hasUnderTheSeaWin ?? (_hasUnderTheSeaWin = LastDraw && Tsumo);
		private set => _hasUnderTheSeaWin = value;
	}


	private bool? _hasUnderTheRiverWin;
	public bool? hasUnderTheRiverWin
	{
		get => _hasUnderTheRiverWin ?? (_hasUnderTheRiverWin = LastDraw && ron);
		private set => _hasUnderTheRiverWin = value;
	}

	private bool? _hasRinshanKaihouWin;
	public bool? hasRinshanKaihouWin
	{
		get => _hasRinshanKaihouWin ?? (_hasRinshanKaihouWin = KanWin && Tsumo);
		private set => _hasRinshanKaihouWin = value;
	}

	private bool? _hasRobbingAKanWin;
	public bool? hasRobbingAKanWin
	{
		get => _hasRobbingAKanWin ?? (_hasRobbingAKanWin = KanWin && ron);
		private set => _hasRobbingAKanWin = value;
	}


	private bool? _hasSevenPairsWin;
	public bool? hasSevenPairsWin
	{
		get => _hasSevenPairsWin ?? (_hasSevenPairsWin = closedHand.Value && ClosedGroups.Length == 7);
		private set => _hasSevenPairsWin = value;
	}

	private bool? _hasThirteenOrphansWin;
	public bool? hasThirteenOrphansWin
	{
		get => _hasThirteenOrphansWin ?? (_hasThirteenOrphansWin = 
			closedHand.Value &&
			ClosedGroups.Length == 13 &&
			!hasPureThirteenOrphansWin.Value);
		private set => _hasThirteenOrphansWin = value;
	}

	private bool? _hasPureThirteenOrphansWin;
	public bool? hasPureThirteenOrphansWin
	{
		get => _hasPureThirteenOrphansWin ?? (_hasPureThirteenOrphansWin =
			closedHand.Value &&
			ClosedGroups.Length == 13 &&
			HiddenHandTiles.Some(t => WinningTile.softEquals(t)));
		private set => _hasPureThirteenOrphansWin = value;
	}


	private bool? _isPinfuHand;
	public bool? isPinfuHand
	{
		get
		{
			if (_isPinfuHand.HasValue) return _isPinfuHand;

			if (SequenceGroups.Length != 4 || WinningTileGroup == null || WinningTileGroup.Tiles.Length != 3)
				return _isPinfuHand = false;

			if (PairGroup == null || PairGroup.Tiles[0].Suit == Suit.Honor)
				return _isPinfuHand = false;

			return _isPinfuHand =
				WinningTileGroup.Tiles[1] != WinningTile &&
				(WinningTileGroup.Tiles[0] == WinningTile || WinningTileGroup.Tiles[0].Rank != Rank.One) &&
				(WinningTileGroup.Tiles[2] == WinningTile || WinningTileGroup.Tiles[2].Rank != Rank.Nine);
		}
		private set 
		{
			if (value ==_isPinfuHand) return;

			hasPinfuWin = null;
			_isPinfuHand = value;
		}
	}

	private bool? _hasPinfuWin;
	public bool? hasPinfuWin
	{
		get => _hasPinfuWin ?? (_hasPinfuWin = isPinfuHand.Value && closedHand.Value);
		private set => _hasPinfuWin = value;
	}

	private int? _pureDoubleSequenceCount;
	public int? pureDoubleSequenceCount
	{
		get {
			if (_pureDoubleSequenceCount.HasValue) return _pureDoubleSequenceCount;

			int pure_double_count = 0;

			for (int i = 0; i < SequenceGroups.Length - 1; i++)
			{
				// if this sequence contains the same tiles as another
				for (int j = i + 1; j < SequenceGroups.Length; j++)
				{
					if (SequenceGroups[i].Tiles[0].softEquals(SequenceGroups[j].Tiles[0]))
						pure_double_count++;
				}
			}

			return _pureDoubleSequenceCount = pure_double_count;
		}
		private set
		{
			if (value == _pureDoubleSequenceCount) return;

			hasPureDoubleSequenceWin = null;
			hasTwicePureDoubleSequenceWin = null;
			_pureDoubleSequenceCount = value;
		}
	}

	private bool? _hasPureDoubleSequenceWin;
	public bool? hasPureDoubleSequenceWin
	{
		get => _hasPureDoubleSequenceWin ?? (_hasPureDoubleSequenceWin = pureDoubleSequenceCount == 1);
		private set => _hasPureDoubleSequenceWin = value;
	}

	private bool? _hasTwicePureDoubleSequenceWin;
	public bool? hasTwicePureDoubleSequenceWin
	{
		get => _hasTwicePureDoubleSequenceWin ?? (_hasTwicePureDoubleSequenceWin = pureDoubleSequenceCount == 2);
		private set => _hasTwicePureDoubleSequenceWin = value;
	}

	private bool? _hasGreenDragonYakuhaiWin;
	public bool? hasGreenDragonYakuhaiWin
	{
		get => _hasGreenDragonYakuhaiWin ?? (_hasGreenDragonYakuhaiWin =
			DragonGroups.Some(group => group.Tiles[0].Rank == Rank.GreenDragon)
		);
		private set {
			if (value.HasValue) _hasGreenDragonYakuhaiWin = value;
			else if (_hasGreenDragonYakuhaiWin.HasValue) {
				_hasGreenDragonYakuhaiWin = null;
				_hasRedDragonYakuhaiWin = null;
				_hasWhiteDragonYakuhaiWin = null;
			}
		}
	}

	private bool? _hasRedDragonYakuhaiWin;
	public bool? hasRedDragonYakuhaiWin
	{
		get => _hasRedDragonYakuhaiWin ?? (_hasRedDragonYakuhaiWin =
			DragonGroups.Some(group => group.Tiles[0].Rank == Rank.RedDragon)
		);
		private set {
			if (value.HasValue) _hasRedDragonYakuhaiWin = value;
			else if (_hasRedDragonYakuhaiWin.HasValue) {
				_hasGreenDragonYakuhaiWin = null;
				_hasRedDragonYakuhaiWin = null;
				_hasWhiteDragonYakuhaiWin = null;
			}
		}
	}

	private bool? _hasWhiteDragonYakuhaiWin;
	public bool? hasWhiteDragonYakuhaiWin
	{
		get => _hasWhiteDragonYakuhaiWin ?? (_hasWhiteDragonYakuhaiWin =
			DragonGroups.Some(group => group.Tiles[0].Rank == Rank.WhiteDragon)
		);
		private set {
			if (value.HasValue) _hasWhiteDragonYakuhaiWin = value;
			else if (_hasWhiteDragonYakuhaiWin.HasValue) {
				_hasGreenDragonYakuhaiWin = null;
				_hasRedDragonYakuhaiWin = null;
				_hasWhiteDragonYakuhaiWin = null;
			}
		}
	}

	private bool? _hasPrevalentWindWin;
	public bool? hasPrevalentWindWin
	{
		get => _hasPrevalentWindWin ?? (_hasPrevalentWindWin =
			WindGroups.Some(group => group.Tiles[0].Rank == PrevalentWind)
		);
		private set => _hasPrevalentWindWin = value;
	}

	private bool? _hasSeatWindWin;
	public bool? hasSeatWindWin
	{
		get => _hasSeatWindWin ?? (_hasSeatWindWin =
			WindGroups.Some(group => group.Tiles[0].Rank == SeatWind)
		);
		private set => _hasSeatWindWin = value;
	}

	private bool? _hasLittleThreeDragonsWin;
	public bool? hasLittleThreeDragonsWin
	{
		get => _hasLittleThreeDragonsWin ?? (_hasLittleThreeDragonsWin = DragonGroups.Length == 2);
		private set => _hasLittleThreeDragonsWin = value;
	}

	private bool? _hasBigThreeDragonsWin;
	public bool? hasBigThreeDragonsWin
	{
		get => _hasBigThreeDragonsWin ?? (_hasBigThreeDragonsWin = DragonGroups.Length == 3);
		private set => _hasBigThreeDragonsWin = value;
	}

	private bool? _hasLittleFourWindsWin;
	public bool? hasLittleFourWindsWin
	{
		get => _hasLittleFourWindsWin ?? (_hasLittleFourWindsWin = WindGroups.Length == 3);
		private set => _hasLittleFourWindsWin = value;
	}

	private bool? _hasBigFourWindsWin;
	public bool? hasBigFourWindsWin
	{
		get => _hasBigFourWindsWin ?? (_hasBigFourWindsWin = WindGroups.Length == 4);
		private set => _hasBigFourWindsWin = value;
	}

	private bool? _hasMixedTripleSequenceWin;
	public bool? hasMixedTripleSequenceWin
	{
		get {
			if (_hasMixedTripleSequenceWin.HasValue) return _hasMixedTripleSequenceWin;

			for (int i = 0; i < SequenceGroups.Length - 2; i++)
			{
				for (int j = i + 1; j < SequenceGroups.Length - 1; j++)
				{
					for (int k = j + 1; k < SequenceGroups.Length; k++)
					{
						if (SequenceGroups[i] == SequenceGroups[j] &&
							SequenceGroups[i] == SequenceGroups[k])
							return _hasMixedTripleSequenceWin = true;
					}
				}
			}

			return _hasMixedTripleSequenceWin = false;
		}
		private set => _hasMixedTripleSequenceWin = value;
	}

	private bool? _hasPureStraightWin;
	public bool? hasPureStraightWin
	{
		get
		{
			if (_hasPureStraightWin.HasValue) return _hasPureStraightWin;

			for (int i = 0; i < SequenceGroups.Length - 2; i++)
			{
				for (int j = i + 1; j < SequenceGroups.Length - 1; j++)
				{
					for (int k = j + 1; k < SequenceGroups.Length; k++)
					{
						if (SequenceGroups[i].Tiles[0].Suit == SequenceGroups[j].Tiles[0].Suit &&
							SequenceGroups[j].Tiles[0].Suit == SequenceGroups[k].Tiles[0].Suit && (
							(SequenceGroups[i].Tiles[0].Rank == Rank.One && SequenceGroups[j].Tiles[0].Rank == Rank.Four && SequenceGroups[k].Tiles[0].Rank == Rank.Seven) ||
							(SequenceGroups[i].Tiles[0].Rank == Rank.Four && SequenceGroups[j].Tiles[0].Rank == Rank.Seven && SequenceGroups[k].Tiles[0].Rank == Rank.One) ||
							(SequenceGroups[i].Tiles[0].Rank == Rank.Seven && SequenceGroups[j].Tiles[0].Rank == Rank.One && SequenceGroups[k].Tiles[0].Rank == Rank.Four)
						)) return _hasPureStraightWin = true;
					}
				}
			}

			return _hasPureStraightWin = false;
		}
		private set => _hasPureStraightWin = value;
	}

	private bool? _hasAllSimplesWin;
	public bool? hasAllSimplesWin
	{
		get
		{
			if (_hasAllSimplesWin.HasValue) return _hasAllSimplesWin;

			setSuitBasedWins();
			return _hasAllSimplesWin ?? false;
		}
		private set => _hasAllSimplesWin = value;
	}

	private bool? _hasAllTerminalsWin;
	public bool? hasAllTerminalsWin
	{
		get
		{
			if (_hasAllTerminalsWin.HasValue) return _hasAllTerminalsWin;

			setSuitBasedWins();
			return _hasAllTerminalsWin ?? false;
		}
		private set => _hasAllTerminalsWin = value;
	}

	private bool? _hasAllHonorsWin;
	public bool? hasAllHonorsWin
	{
		get
		{
			if (_hasAllHonorsWin.HasValue) return _hasAllHonorsWin;

			setSuitBasedWins();
			return _hasAllHonorsWin ?? false;
		}
		private set => _hasAllHonorsWin = value;
	}

	private bool? _hasHalfOutsideWin;
	public bool? hasHalfOutsideWin
	{
		get
		{
			if (_hasHalfOutsideWin.HasValue) return _hasHalfOutsideWin;

			setSuitBasedWins();
			return _hasHalfOutsideWin ?? false;
		}
		private set => _hasHalfOutsideWin = value;
	}

	private bool? _hasFullOutsideWin;
	public bool? hasFullOutsideWin
	{
		get
		{
			if (_hasFullOutsideWin.HasValue) return _hasFullOutsideWin;

			setSuitBasedWins();
			return _hasFullOutsideWin ?? false;
		}
		private set => _hasFullOutsideWin = value;
	}

	private void setSuitBasedWins()
	{
		if (AllHandGroups.Length != 5)
		{
			_hasAllSimplesWin = _hasAllTerminalsWin = _hasAllHonorsWin = _hasHalfOutsideWin = _hasFullOutsideWin = false;
		}
		else if (AllHandTiles.All(tile => tile.Rank.isSimple()))
		{
			_hasAllSimplesWin = true;
			_hasAllTerminalsWin = _hasAllHonorsWin = _hasHalfOutsideWin = _hasFullOutsideWin = false;
		}
		else if (AllHandTiles.All(tile => tile.Rank.isTerminal()))
		{
			_hasAllTerminalsWin = true;
			_hasAllSimplesWin = _hasAllHonorsWin = _hasHalfOutsideWin = _hasFullOutsideWin = false;
		}
		else if (AllHandTiles.All(tile => tile.Rank.isHonor()))
		{
			_hasAllHonorsWin = true;
			_hasAllSimplesWin = _hasAllTerminalsWin = _hasHalfOutsideWin = _hasFullOutsideWin = false;
		}
		else if (AllHandTiles.All(tile => tile.Rank.isHonor() || tile.Rank.isTerminal()))
		{
			_hasFullOutsideWin = true;
			_hasAllSimplesWin = _hasAllTerminalsWin = _hasAllHonorsWin = _hasHalfOutsideWin = false;
		}
		else if (AllHandGroups.All(g => g.Tiles.Some(t => t.Rank.isTerminal() || t.Rank.isHonor())))
		{
			_hasHalfOutsideWin = true;
			_hasAllSimplesWin = _hasAllTerminalsWin = _hasAllHonorsWin = _hasFullOutsideWin = false;
		}
	}

	private void clearSuitBasedWins()
	{
		_hasAllSimplesWin = _hasAllTerminalsWin = _hasAllHonorsWin =
		_hasHalfOutsideWin = _hasFullOutsideWin = _hasAllGreensWin = null;
	}
	
	private bool? _hasAllGreensWin;
	public bool? hasAllGreensWin
	{
		get => _hasAllGreensWin ?? (_hasAllGreensWin =
			AllHandTiles.All(tile => tile.IsGreen)
		);
		private set => _hasAllGreensWin = value;
	}

	private bool? _hasAllTripletsWin;
	public bool? hasAllTripletsWin
	{
		get => _hasAllTripletsWin ?? (_hasAllTripletsWin =
			openHand.Value &&
			TripletGroups.Length + KanGroups.Length == 4
		);
		private set => _hasAllTripletsWin = value;
	}

	private bool? _hasThreeConcealedTripletsWin;
	public bool? hasThreeConcealedTripletsWin
	{
		get => _hasThreeConcealedTripletsWin ?? (_hasThreeConcealedTripletsWin =
			KanGroups.Length + TripletGroups.Length >= 3 &&
			ClosedGroups.Length >= 3 &&
			ConcealedTuplets.Length == 3
		);
		private set => _hasThreeConcealedTripletsWin = value;
	}

	private bool? _hasFourConcealedTripletsWin;
	public bool? hasFourConcealedTripletsWin
	{
		get => _hasFourConcealedTripletsWin ?? (_hasFourConcealedTripletsWin =
			KanGroups.Length + TripletGroups.Length == 4 &&
			ConcealedTuplets.Length == 4 &&
			WinningTileGroup != null &&
			WinningTileGroup.Tiles.Length > 2
		);
		private set => _hasFourConcealedTripletsWin = value;
	}

	private bool? _hasSingleWaitFourConcealedTripletsWin;
	public bool? hasSingleWaitFourConcealedTripletsWin
	{
		get => _hasSingleWaitFourConcealedTripletsWin ?? (_hasSingleWaitFourConcealedTripletsWin =
			closedHand.Value &&
			KanGroups.Length + TripletGroups.Length == 4 &&
			ConcealedTuplets.Length == 4 &&
			WinningTileGroup?.Tiles.Length == 2
		);
		private set => _hasSingleWaitFourConcealedTripletsWin = value;
	}

	private bool? _hasThreeKansWin;
	public bool? hasThreeKansWin
	{
		get => _hasThreeKansWin ?? (_hasThreeKansWin =
			KanGroups.Length == 3
		);
		private set => _hasThreeKansWin = value;
	}

	private bool? _hasFourKansWin;
	public bool? hasFourKansWin
	{
		get => _hasFourKansWin ?? (_hasFourKansWin =
			KanGroups.Length == 4
		);
		private set => _hasFourKansWin = value;
	}

	private bool? _hasSequenceTripletsWin;
	public bool? hasSequenceTripletsWin
	{
		get 
		{
			if (_hasSequenceTripletsWin.HasValue) return _hasSequenceTripletsWin;

			for (int i = 0; i < TupletGroups.Length - 2; i++)
			{
				for (int j = i + 1; j < TupletGroups.Length - 1; j++)
				{
					for (int k = j + 1; k < TupletGroups.Length; k++)
					{
						if (TupletGroups[i].Tiles[0].Rank == TupletGroups[j].Tiles[0].Rank &&
							TupletGroups[j].Tiles[0].Rank == TupletGroups[k].Tiles[0].Rank)
						{
							return _hasSequenceTripletsWin = true;
						}
					}
				}
			}

			return _hasSequenceTripletsWin = false;
		}
		private set => _hasSequenceTripletsWin = value;
	}

	private Suit[] _suitsInHand;
	public Suit[] suitsInHand
	{
		get => _suitsInHand ?? (_suitsInHand =
			AllHandTiles.Select(tile => tile.Suit).Distinct().ToArray()
		);
		private set
		{
			if (value == _suitsInHand) return;

			hasHalfFlushWin = hasFullFlushWin = hasNineGatesWin = hasTrueNineGatesWin = null;
			_suitsInHand = value;
		}
	}

	private bool? _hasHalfFlushWin;
	public bool? hasHalfFlushWin
	{
		get => _hasHalfFlushWin ?? (_hasHalfFlushWin =
			suitsInHand.Length == 2 &&
			suitsInHand.Some(suit => suit == Suit.Honor)
		);
		private set => _hasHalfFlushWin = value;
	}

	private bool? _hasFullFlushWin;
	public bool? hasFullFlushWin
	{
		get => _hasFullFlushWin ?? (_hasFullFlushWin =
			suitsInHand.Length == 1 &&
			suitsInHand[0] != Suit.Honor &&
			hasNineGatesWin == false
		);
		private set => _hasFullFlushWin = value;
	}

	private bool? _hasNineGatesWin;
	public bool? hasNineGatesWin
	{
		get {
			if (_hasNineGatesWin.HasValue) return _hasNineGatesWin;

			setNineGatesWins();
			return _hasNineGatesWin;
		}
		private set => _hasNineGatesWin = value;
	}

	private bool? _hasTrueNineGatesWin;
	public bool? hasTrueNineGatesWin
	{
		get {
			if (_hasTrueNineGatesWin.HasValue) return _hasTrueNineGatesWin;

			setNineGatesWins();
			return _hasTrueNineGatesWin;
		}
		private set => _hasTrueNineGatesWin = value;
	}

	private void setNineGatesWins()
	{
		if (openHand.Value || suitsInHand.Length != 1 || suitsInHand[0] == Suit.Honor)
		{
			_hasNineGatesWin = _hasTrueNineGatesWin = false;
			return;
		}

		var nineGatesTrueHand = WinCatalog.NineGatesHand.ToList();

		foreach (Tile tile in HiddenHandTiles)
		{
			nineGatesTrueHand.Remove(tile.Rank); // Possibly won't remove anything, list Counts are used below to see what didn't get removed
		}

		if (nineGatesTrueHand.Count == 0)
		{
			_hasTrueNineGatesWin = true;
			_hasNineGatesWin = false;
		}
		else if (nineGatesTrueHand.Count == 1 && WinningTile?.Rank == nineGatesTrueHand[0])
		{
			_hasTrueNineGatesWin = false;
			_hasNineGatesWin = true;
		}
		else
		{
			_hasTrueNineGatesWin = _hasNineGatesWin = false;
		}
	}

	public PlayerBoard(bool initializeWithDefaultTiles = true)
	{
		if (!initializeWithDefaultTiles) return;

		SetAllBoardData(
			hiddenHandTiles: new Tile[] {
				new Tile(Suit.Bamboo, Rank.One),
				new Tile(Suit.Bamboo, Rank.Two),
				new Tile(Suit.Honor, Rank.WhiteDragon),
				new Tile(Suit.Honor, Rank.WhiteDragon),
			},
			visibleHandTiles: new Tile[][] {
				new Tile[] {
					new Tile(Suit.Honor, Rank.EastWind),
					new Tile(Suit.Back, Rank.Back),
					new Tile(Suit.Back, Rank.Back),
					new Tile(Suit.Honor, Rank.EastWind)
				},
				new Tile[] {
					new Tile(Suit.Dot, Rank.Five),
					new Tile(Suit.Dot, Rank.Six),
					new Tile(Suit.Dot, Rank.Seven)
				},
				new Tile[] {
					new Tile(Suit.Bamboo, Rank.Seven),
					new Tile(Suit.Bamboo, Rank.Eight),
					new Tile(Suit.Bamboo, Rank.Nine)
				}
			},
			winningTile: new Tile(Suit.Bamboo, Rank.Three),
			doraIndicators: new Tile[] { new Tile(Suit.Honor, Rank.RedDragon) },
			discards: new Tile[] {
				new Tile(Suit.Bamboo, Rank.Four),
				new Tile(Suit.Honor, Rank.RedDragon),
				new Tile(Suit.Character, Rank.Six),
				new Tile(Suit.Character, Rank.One),
				new Tile(Suit.Honor, Rank.WestWind),
				new Tile(Suit.Honor, Rank.NorthWind),
				new Tile(Suit.Bamboo, Rank.Six),
				new Tile(Suit.Character, Rank.Eight),
				new Tile(Suit.Dot, Rank.Two),
				new Tile(Suit.Dot, Rank.Three),
				new Tile(Suit.Honor, Rank.GreenDragon),
				new Tile(Suit.Honor, Rank.GreenDragon),
				new Tile(Suit.Bamboo, Rank.Three),
				new Tile(Suit.Honor, Rank.NorthWind),
				new Tile(Suit.Honor, Rank.SouthWind),
				new Tile(Suit.Dot, Rank.One),
			}
		);
	}

	public void SetAllBoardData(
		Tile[] hiddenHandTiles,
		Tile[][] visibleHandTiles,
		Tile[] doraIndicators,
		Tile[] discards,
		Tile winningTile,
		Rank seatWind = Rank.EastWind,
		Rank prevalentWind = Rank.EastWind,
		bool tsumo = false,
		bool riichi = false,
		bool doubleRiichi = false,
		bool ippatsu = false,
		bool kanWin = false,
		bool lastDraw = false
	) {
		HiddenHandTiles = hiddenHandTiles;
		VisibleHandTiles = visibleHandTiles;
		DoraIndicators = doraIndicators;
		Discards = discards;
		WinningTile = winningTile;
		SeatWind = seatWind;
		PrevalentWind = prevalentWind;
		Tsumo = tsumo;
		Riichi = riichi;
		DoubleRiichi = doubleRiichi;
		Ippatsu = ippatsu;
		KanWin = kanWin;
		LastDraw = lastDraw;
	}

	private void handChanged()
	{
		_wins = null;
		_handError = null;
		WinTitle = null;
		PayOrder = null;
	}

	private Win[] _wins;

	public static List<Win> workingWins = new List<Win>();
	public Win[] Wins
	{
		get
		{
			if (_wins != null) return _wins;

			workingWins.Clear();
			if (hasDoubleRiichiWin.IsTrue()) workingWins.Add(WinCatalog.Double_Riichi);
			if (hasRiichiWin.IsTrue()) workingWins.Add(WinCatalog.Riichi);
			if (hasIppatsuWin.IsTrue()) workingWins.Add(WinCatalog.Ippatsu);
			if (hasMenzenchinTsumoWin.IsTrue()) workingWins.Add(WinCatalog.Menzenchin_Tsumo);
			if (hasUnderTheSeaWin.IsTrue()) workingWins.Add(WinCatalog.Under_The_Sea);
			if (hasUnderTheRiverWin.IsTrue()) workingWins.Add(WinCatalog.Under_The_River);
			if (hasRinshanKaihouWin.IsTrue()) workingWins.Add(WinCatalog.Rinshan_Kaihou);
			if (hasRobbingAKanWin.IsTrue()) workingWins.Add(WinCatalog.Robbing_A_Kan);
			if (hasSevenPairsWin.IsTrue()) workingWins.Add(WinCatalog.Seven_Pairs);
			if (hasThirteenOrphansWin.IsTrue()) workingWins.Add(WinCatalog.Thirteen_Orphans);
			if (hasPureThirteenOrphansWin.IsTrue()) workingWins.Add(WinCatalog.Pure_Thirteen_Orphans);
			if (hasPinfuWin.IsTrue()) workingWins.Add(WinCatalog.Pinfu);
			if (hasPureDoubleSequenceWin.IsTrue()) workingWins.Add(WinCatalog.Pure_Double_Sequence);
			if (hasTwicePureDoubleSequenceWin.IsTrue()) workingWins.Add(WinCatalog.Twice_Pure_Double_Sequence);
			if (hasGreenDragonYakuhaiWin.IsTrue()) workingWins.Add(WinCatalog.Green_Dragon_Yakuhai);
			if (hasWhiteDragonYakuhaiWin.IsTrue()) workingWins.Add(WinCatalog.White_Dragon_Yakuhai);
			if (hasRedDragonYakuhaiWin.IsTrue()) workingWins.Add(WinCatalog.Red_Dragon_Yakuhai);
			if (hasPrevalentWindWin.IsTrue()) workingWins.Add(WinCatalog.Prevalent_Wind);
			if (hasSeatWindWin.IsTrue()) workingWins.Add(WinCatalog.Seat_Wind);
			if (hasLittleThreeDragonsWin.IsTrue()) workingWins.Add(WinCatalog.Little_Three_Dragons);
			if (hasBigThreeDragonsWin.IsTrue()) workingWins.Add(WinCatalog.Big_Three_Dragons);
			if (hasLittleFourWindsWin.IsTrue()) workingWins.Add(WinCatalog.Little_Four_Winds);
			if (hasBigFourWindsWin.IsTrue()) workingWins.Add(WinCatalog.Big_Four_Winds);
			if (hasMixedTripleSequenceWin.IsTrue()) workingWins.Add(WinCatalog.Mixed_Triple_Sequence);
			if (hasPureStraightWin.IsTrue()) workingWins.Add(WinCatalog.Pure_Straight);
			if (hasAllTerminalsWin.IsTrue()) workingWins.Add(WinCatalog.All_Terminals);
			if (hasAllHonorsWin.IsTrue()) workingWins.Add(WinCatalog.All_Honors);
			if (hasAllGreensWin.IsTrue()) workingWins.Add(WinCatalog.All_Green);
			if (hasAllSimplesWin.IsTrue()) workingWins.Add(WinCatalog.All_Simples);
			if (hasFullOutsideWin.IsTrue()) workingWins.Add(WinCatalog.Full_Outside_Hand);
			if (hasHalfOutsideWin.IsTrue()) workingWins.Add(WinCatalog.Half_Outside_Hand);
			if (hasAllTripletsWin.IsTrue()) workingWins.Add(WinCatalog.All_Triplets);
			if (hasThreeConcealedTripletsWin.IsTrue()) workingWins.Add(WinCatalog.Three_Concealed_Triplets);
			if (hasFourConcealedTripletsWin.IsTrue()) workingWins.Add(WinCatalog.Four_Concealed_Triplets);
			if (hasSingleWaitFourConcealedTripletsWin.IsTrue()) workingWins.Add(WinCatalog.Single_Wait_Four_Concealed_Triplets);
			if (hasThreeKansWin.IsTrue()) workingWins.Add(WinCatalog.Three_Kans);
			if (hasFourKansWin.IsTrue()) workingWins.Add(WinCatalog.Four_Kans);
			if (hasSequenceTripletsWin.IsTrue()) workingWins.Add(WinCatalog.Triple_Triplets);
			if (hasTrueNineGatesWin.IsTrue()) workingWins.Add(WinCatalog.True_Nine_Gates);
			if (hasNineGatesWin.IsTrue()) workingWins.Add(WinCatalog.Nine_Gates);
			if (hasFullFlushWin.IsTrue()) workingWins.Add(WinCatalog.Full_Flush);
			if (hasHalfFlushWin.IsTrue()) workingWins.Add(WinCatalog.Half_Flush);

			if (workingWins.Count == 0) return _wins = new Win[] { };

			for (int i = 0; i < RedFives.Length; i++)
				workingWins.Add(WinCatalog.Red_Five);

			foreach (var dora in DoraIndicators)
			{
				Rank next = dora.Rank.getCyclicNext();
				foreach (var t in AllHandTiles)
				{
					UnityEngine.Debug.Log("Checking " + t + " against " + dora + " using " + next + "");

					if (t.Rank == next && t.Suit == dora.Suit)
						workingWins.Add(WinCatalog.Dora_Indicator);
				}
			}

			return _wins = workingWins.ToArray();
		}
	}

	private int? _baseWinAmount;
	public int baseWinAmount
	{
		get
		{
			if (_baseWinAmount != null) return _baseWinAmount.Value;

			SetWinAmount();
			return _baseWinAmount.Value;
		}
		set => _baseWinAmount = value;
	}

	private string _winTitle;
	public string WinTitle
	{
		get
		{
			if (_winTitle != null) return _winTitle;

			SetWinAmount();
			return _winTitle;
		}
		set => _winTitle = value;
	}

	private string _payOrder;
	public string PayOrder
	{
		get
		{
			if (_payOrder != null) return _payOrder;

			SetWinAmount();
			return _payOrder;
		}
		set => _payOrder = value;
	}

	private static List<string> workingTitles = new List<string>();
	private void SetWinAmount()
	{
		if (Wins.Some(w => w.yaakuman == Yaakuman.Double))
		{
			_baseWinAmount = 16_000;
			_winTitle = "Double Yaakuman";
		}
		else if (Wins.Some(w => w.yaakuman == Yaakuman.Single))
		{
			_baseWinAmount = 8_000;
			_winTitle = "Yaakuman";
		}
		else
		{
			int han = 0;

			if (closedHand.IsTrue()) foreach (var w in Wins)
			{
				if (w.closed_han != -1) han += w.closed_han;
				else throw new System.Exception("There is a closed hand win without closed_han");
			}
			else foreach (var w in Wins)
			{
				if (w.open_han != -1) han += w.open_han;
				else throw new System.Exception("There is an open hand win without open_han");
			}
			
			if (han >= 5) switch (han)
			{
				case 5: _baseWinAmount = 2_000; _winTitle = "Mangan"; break;
				case 6:
				case 7: _baseWinAmount = 3_000; _winTitle = "Haneman"; break;
				case 8:
				case 9:
				case 10: _baseWinAmount = 4_000; _winTitle = "Baiman"; break;
				case 11:
				case 12: _baseWinAmount = 6_000; _winTitle = "Sanbaiman"; break;
				default: _baseWinAmount = 8_000; _winTitle = "Kazoe Yaakuman"; break;
			}
			// Calculate fu
			else
			{
				if (hasSevenPairsWin.IsTrue())
				{
					_baseWinAmount = 25;
					_winTitle = "7Pairs (25)";
				}
				else
				{
					_baseWinAmount = 20;
					workingTitles.Clear();
					if (closedHand.IsTrue() && !Tsumo)
					{
						_baseWinAmount += 10;
						workingTitles.Add("Menzen-Kafu (10)");
					}

					// tsumo can only be given on pinfu open (no pinfu for open tho )
					if (Tsumo && isPinfuHand.IsTrue())
					{
						_baseWinAmount += 2;
						workingTitles.Add("Tsumo (2)");
					}
					else
					{
						foreach (TileGroup g in AllHandGroups)
						{
							int points = 0;
							string title = "";
							if (g.Type == TileGroupType.Triplet)
							{
								points = 2;
								title = "Triplet";
							}
							else if (g.Type == TileGroupType.Kan)
							{
								points = 8;
								title = "Kan";
							}
							else continue;

							if (g.Tiles[0].Rank.isTerminal())
							{
								points *= 2;
								title = "Terminal " + title;
							}
							else if (g.Tiles[0].Rank.isHonor())
							{
								points *= 2;
								title = "Honor " + title;
							}

							if (g.Closed)
							{
								points *= 2;
								title = "Closed " + title;
							}

							_baseWinAmount += points;
							workingTitles.Add(title + " (" + points + ")");
						}
					
						bool singleWait =
							PairGroup != null &&
							PairGroup == WinningTileGroup &&
							WinningTileGroup.Type == TileGroupType.Sequence &&
							WinningTileGroup.Tiles[0] == WinningTile;

						if (singleWait)
						{
							_baseWinAmount += 2;
							workingTitles.Add("Single Wait (2)");
						}

						if (PairGroup != null && (
							PairGroup.Tiles[0].Rank.isDragon() ||
							PairGroup.Tiles[0].Rank == SeatWind ||
							PairGroup.Tiles[0].Rank == PrevalentWind
						)) {
							_baseWinAmount += 2;
							workingTitles.Add("Yakuhai Pair (2)");
						}

						if (_baseWinAmount == 20 && openHand.IsTrue())
						{
							_baseWinAmount += 2;
							workingTitles.Add("Open Pinfu (2)");
						}

						_baseWinAmount = (int)System.Math.Ceiling(_baseWinAmount.Value / 10d) * 10;
						_winTitle = string.Join(", ", workingTitles.ToArray());
					}
				}

				_baseWinAmount = _baseWinAmount * (int)System.Math.Pow(2, 2 + han);

				if (_baseWinAmount > 2_000)
				{
					_baseWinAmount = 2_000;
					_winTitle = "Mangan!: " + _winTitle;
				}
			}
		}

		UnityEngine.Debug.Log("Winning: " + _winTitle + " (" + _baseWinAmount + ")");

		if (Tsumo)
		{
			if (SeatWind == PrevalentWind)
			{
				_payOrder = $"Each player pays ${(_baseWinAmount * 2).Value.Round100()} for ${(_baseWinAmount * 2 * 3).Value.Round100()} in total.";
			}
			else
			{
				_payOrder = $"The dealer (${System.Enum.GetName(typeof(Rank), PrevalentWind)}) pays ${(_baseWinAmount * 2).Value.Round100()} and other players pay ${(_baseWinAmount).Value.Round100()} for ${(_baseWinAmount * 2 * 2).Value.Round100()} in total.";
			}
		}
		else if (SeatWind == PrevalentWind)
		{
			_payOrder = $"Losing player pays ${(_baseWinAmount * 6).Value.Round100()}.";
		} else
		{
			_payOrder = $"Losing player pays ${(_baseWinAmount * 4).Value.Round100()}.";
		}
	}

	private string _handError;
	public string handError
	{
		get
		{
			if (!string.IsNullOrEmpty(_handError)) return _handError;

			try
			{
				ValidateHandSize();
				ValidateScoreParameters();
				ValidateFlippedTiles();
				ValidateBoard();
				ValidateGroups();

				if (Wins.Length == 0)
					throw new System.Exception("No wins! Hand is valid, but cannot be a winning hand");

				if (pureDoubleSequenceCount > 2)
					throw new InvalidScoreArgumentsError("There are more than two pure double sequences in the hand! This means that there were three triplets that were grouped as runs and not triplets!");

				return _handError = null;
			}
			catch (System.Exception e)
			{
				return _handError = e.Message;
			}
		}
	}

	/**
	 * Helper function to evaluate the scores and winning of the hand, catching any errors.
	 */
	public string ValidateHand()
	{
		return handError;
	}

	public bool IsValid { get => handError == null; }


	override public string ToString()
	{
		return $"{{ hiddenTiles: {HiddenHandTiles.Format()}, visibleTiles: {VisibleHandTiles.Format()}, winningTile: {WinningTile}, doraIndicators: {DoraIndicators.Format()}, Discards: {Discards.Format()}, seatWind: {System.Enum.GetName(typeof(Rank), SeatWind)}, prevalentWind: {System.Enum.GetName(typeof(Rank), PrevalentWind)}, tsumo: {Tsumo}, riichi: {Riichi}, doubleRiichi: {DoubleRiichi}, ippatsu: {Ippatsu}, kanWin: {KanWin}, lastDraw: {LastDraw} }}";
	}

	public string DumpToString()
	{
		return $"{{\n\thiddenTiles: {HiddenHandTiles.Format()},\n\tvisibleTiles: {VisibleHandTiles.Format()},\n\twinningTile: {WinningTile},\n\tdoraIndicators: {DoraIndicators.Format()},\n\tDiscards: {Discards.Format()},\n\tseatWind: {System.Enum.GetName(typeof(Rank), SeatWind)},\n\tprevalentWind: {System.Enum.GetName(typeof(Rank), PrevalentWind)},\n\ttsumo: {Tsumo},\n\triichi: {Riichi},\n\tdoubleRiichi: {DoubleRiichi},\n\tippatsu: {Ippatsu},\n\tkanWin: {KanWin},\n\tlastDraw: {LastDraw},\n\twins: {Wins.Format()},\n\tgroups: {AllHandGroups.Format()},\n\tpureDoubleSequenceCount: {pureDoubleSequenceCount},\n\thandError: {handError}\n}}";
	}
}


public static class HandGrouper
{
	public static TileGroup[] GetSevenPairs(List<Tile> tiles)
	{
		if (tiles.Count != 14)
			return null;
		
		var pairs = new List<(int, int)>();

		for (int i = 0; i < tiles.Count; i++)
		{
			for (int j = i + 1; j < tiles.Count; j++)
			{
				if (pairs.FirstOrDefault(p => p.Item2 == i || p.Item2 == j) != default((int, int)))
					continue;

				if (tiles[i].softEquals(tiles[j]))
				{
					pairs.Add((i, j));
					break;
				}
			}
		}

		if (pairs.Count != 7)
			return null;

		return pairs.Select(p => new TileGroup(new Tile[] { tiles[p.Item1], tiles[p.Item2] }, true)).ToArray();
	}

	public static TileGroup[] GetThirteenOrphans(List<Tile> tiles)
	{
		if (tiles.Count != 14)
			return null;
		
		// Check if all tiles are in the thirteen orphans set
		for (int i = 0; i < tiles.Count; i++)
		{
			if (tiles[i].Suit == Suit.Honor) continue;
			else if (tiles[i].Rank == Rank.One || tiles[i].Rank == Rank.Nine) continue;
			else return null;
		}
		
		var orphans = new List<List<Tile>>();

		for (int i = 0; i < tiles.Count; i++)
		{
			var list = orphans.FirstOrDefault(g => g[0].softEquals(tiles[i]));
			if (list != null)
			{
				list.Add(tiles[i]);
			}
			else
			{
				orphans.Add(new List<Tile>() { tiles[i] });
			}
		}

		if (orphans.Count == 14)
			throw new System.Exception("Tile.isThirteenOrphans(): 14 orphans found! This should not be possible.");

		if (orphans.Count == 13)
		{
			return orphans.Select(g => new TileGroup(g.ToArray(), true, true)).ToArray();
		}
		else return null;
	}

	private static List<TileGroup> workingClosedGroups = new List<TileGroup>();
	public static TileGroup[] CreateClosedGroups(List<Tile> closed_tiles)
	{
		TileGroup[] specialGroup = null;

		if ((specialGroup = GetSevenPairs(closed_tiles)) != null)
			return specialGroup;

		if ((specialGroup = GetThirteenOrphans(closed_tiles)) != null)
			return specialGroup;


		workingClosedGroups.Clear();

		System.Func<string, TileGroup[]> throwError = str =>
		{
			string error = (str + ": Found => " + workingClosedGroups.Format() + ", invalid => " + closed_tiles.Format());
			workingClosedGroups.Add(new TileGroup(closed_tiles.ToArray(), true, false, error));
			return workingClosedGroups.ToArray();
		}; 

		bool created_pair = false;
		bool sorted = false;
		int loop_count = 0;
		Tile[] array;
		while (closed_tiles.Count > 0)
		{
			loop_count++;
			if (loop_count > 10) return throwError("Loop count broke!");

			switch (closed_tiles.Count)
			{
				case 1:
					return throwError("Cannot have a single tile left over when creating closed groups");

				case 2:
					array = closed_tiles.ToArray();
					if (!Tile.isPair(array))
						return throwError("Cannot have a single tile left over when creating closed groups");

					workingClosedGroups.Add(new TileGroup(array, true));
					closed_tiles.Clear();
					break;

				case 3:
					array = closed_tiles.ToArray();
					if (!Tile.isMeld(array))
						return throwError("Bad meld while creating closed groups");

					workingClosedGroups.Add(new TileGroup(array, true));
					closed_tiles.Clear();
					break;

				case 4:
				case 7:
				case 10:
				case 13:
					return throwError($"Cannot (ever) create groups from ${closed_tiles.Count} tiles when creating closed groups (needs to be multiple of 3 with pair)");

				case 5:
					// There must be one meld and one pair. Remove the meld.
				case 6:
				case 9:
				case 12:

					if (!sorted)
					{
						closed_tiles.Sort((a, b) => a.Rank - b.Rank);
						sorted = true;
					}
					// Must be all melds. These melds are assumed to only have one valid combination in a valid hand, so FirstOrDefault one of the melds here, and let the while loop FirstOrDefault the other (case 6/9 and case 3).
					var foundMeld = FirstOrDefaultAnyMeld(closed_tiles);

					if (!foundMeld.HasValue)
						return throwError($"Could not FirstOrDefault ANY melds while creating closed groups from the last ${closed_tiles.Count} tiles (which must have at least one meld)");
					else
					{
						workingClosedGroups.Add(new TileGroup(
							new Tile[] { closed_tiles[foundMeld.Value.Item1], closed_tiles[foundMeld.Value.Item2], closed_tiles[foundMeld.Value.Item3] }
						, true));
						closed_tiles.RemoveAt(foundMeld.Value.Item3);
						closed_tiles.RemoveAt(foundMeld.Value.Item2);
						closed_tiles.RemoveAt(foundMeld.Value.Item1);
					}
					break;

				case 8:
				case 11:
				case 14:
					if (created_pair)
						return throwError($"Cannot create groups from ${closed_tiles.Count} tiles because pair was already made when creating closed groups");
					
					// First, FirstOrDefault any triplets that cannot be a sequence
					var triplet = FirstOrDefaultNonSequenceTriplet(closed_tiles);
					if (triplet.HasValue)
					{
						workingClosedGroups.Add(new TileGroup(
							new Tile[] { closed_tiles[triplet.Value.Item1], closed_tiles[triplet.Value.Item2], closed_tiles[triplet.Value.Item3] }
						, true));
						closed_tiles.RemoveAt(triplet.Value.Item3);
						closed_tiles.RemoveAt(triplet.Value.Item2);
						closed_tiles.RemoveAt(triplet.Value.Item1);
						break;
					}

					// FirstOrDefault a pair that keeps the hand valid (only one pair if there is a pair with no meld options)
					(int, int)[] pairs = FirstOrDefaultBestValidPairs(closed_tiles);

					if (pairs.Length <= 0)
						return throwError($"Could not FirstOrDefault a pair while creating closed groups from the last ${closed_tiles.Count} tiles (which must have a pair)");
					else if (pairs.Length == 1)
					{
						workingClosedGroups.Add(new TileGroup(
							new Tile[] { closed_tiles[pairs[0].Item1], closed_tiles[pairs[0].Item2] }
							, true));
						closed_tiles.RemoveAt(pairs[0].Item2);
						closed_tiles.RemoveAt(pairs[0].Item1);
						created_pair = true;
					}
					else
					{
						for (int i = 0; i < pairs.Length; i++)
						{
							try
							{
								List<Tile> copy = new List<Tile>(closed_tiles);
								copy.RemoveAt(pairs[0].Item2);
								copy.RemoveAt(pairs[0].Item1);
								var groups = CreateClosedGroups(copy);
								workingClosedGroups.Add(new TileGroup(
									new Tile[] { closed_tiles[pairs[0].Item1], closed_tiles[pairs[0].Item2] }
									, true));
								workingClosedGroups.AddRange(groups);
								return workingClosedGroups.ToArray();
							}
							catch { }
						}

						return throwError($"Could FirstOrDefault pairs, but all resulted in bad hands while creating closed groups from the last ${closed_tiles.Count} tiles (which must have a pair). Pairs: " + pairs.Select(p => $"[${closed_tiles[p.Item1].Rank}, ${closed_tiles[p.Item2].Rank}]").Format());
					}

					break;

				default:
					return throwError("Closed tiles is higher than 14!");
			}
		}

		return workingClosedGroups.ToArray();
	}

	private static Tile[] _testingArray = new Tile[3];
	private static (int, int, int)? FirstOrDefaultNonSequenceTriplet(List<Tile> closed_tiles)
	{
		for (int i = 0; i < closed_tiles.Count - 2; i++)
		{
			for (int j = i + 1; j < closed_tiles.Count - 1; j++)
			{
				for (int k = j + 1; k < closed_tiles.Count; k++)
				{
					_testingArray[0] = closed_tiles[i];
					_testingArray[1] = closed_tiles[j];
					_testingArray[2] = closed_tiles[k];
					if (Tile.isTriplet(_testingArray))
					{
						bool sequ = false;
						if (closed_tiles[i].Suit != Suit.Honor)
						{
							for (int x = 0; x < closed_tiles.Count - 1 && !sequ; x++)
							{
								if (x != i && x != j && x != k)
									for (int y = x + 1; y < closed_tiles.Count && !sequ; y++)
									{
										if (y != i && y != j && y != k)
										{
											_testingArray[0] = closed_tiles[i];
											_testingArray[1] = closed_tiles[x];
											_testingArray[2] = closed_tiles[y];
											if (Tile.isSequence(_testingArray))
												sequ = true;
										}
									}
							}
						}

						if (!sequ)
							return (i, j, k);
					}
				}
			}
		}

		return null;
	}

	private static (int, int, int)? FirstOrDefaultAnyMeld(List<Tile> closed_tiles)
	{
		// FirstOrDefault triplets first
		Tile[] _testingArray = new Tile[3];
		for (int i = 0; i < closed_tiles.Count - 2; i++)
		{
			for (int j = i + 1; j < closed_tiles.Count - 1; j++)
			{
				for (int k = j + 1; k < closed_tiles.Count; k++)
				{
					_testingArray[0] = closed_tiles[i];
					_testingArray[1] = closed_tiles[j];
					_testingArray[2] = closed_tiles[k];
					if (Tile.isTriplet(_testingArray))
					{
						return (i, j, k);
					}
				}
			}
		}

		// FirstOrDefault sequences second
		for (int i = 0; i < closed_tiles.Count - 2; i++)
		{
			for (int j = i + 1; j < closed_tiles.Count - 1; j++)
			{
				for (int k = j + 1; k < closed_tiles.Count; k++)
				{
					_testingArray[0] = closed_tiles[i];
					_testingArray[1] = closed_tiles[j];
					_testingArray[2] = closed_tiles[k];
					if (Tile.isSequence(_testingArray))
					{
						return (i, j, k);
					}
				}
			}
		}

		return null;
	}

	private static (int, int)[] FirstOrDefaultBestValidPairs(List<Tile> closed_tiles)
	{
		List<(int, int)> pairs = new List<(int, int)>();
		for (int i = 0; i < closed_tiles.Count - 1; i++)
		{
			for (int j = i + 1; j < closed_tiles.Count; j++)
			{
				if (pairs.Some((y) => closed_tiles[y.Item2].softEquals(closed_tiles[i]) || closed_tiles[y.Item2].softEquals(closed_tiles[j])))
					continue; // If already used in a pair, skip

				if (closed_tiles[i].softEquals(closed_tiles[j]))
				{
					bool canMeldI = false;
					bool canMeldJ = false;
					for (int k = 0; k < closed_tiles.Count - 1; k++)
					{
						for (int l = k + 1; l < closed_tiles.Count; l++)
						{
							_testingArray[0] = closed_tiles[i];
							_testingArray[1] = closed_tiles[k];
							_testingArray[2] = closed_tiles[l];
							if (!canMeldI && k != i && l != i && Tile.isMeld(_testingArray))
							{
								canMeldI = true;
							}

							_testingArray[0] = closed_tiles[j];
							if (!canMeldJ && k != j && l != j && Tile.isMeld(_testingArray))
							{
								canMeldJ = true;
							}
						}
					}

					// If either of the tiles in a pair cannot be melded, than it must be the pair in a valid hand
					if (canMeldI == false || canMeldJ == false)
					{
						return new (int, int)[] { (i, j) };
					}
					else pairs.Add((i, j));
				}
			}
		}

		return pairs.ToArray();
	}
}

public static class EvaluatorUtils
{
	public static bool IsTrue(this bool? value)
	{
		return value.HasValue && value.Value;
	}

	public static int Round100(this int value)
	{
		return (int)System.Math.Round(value / 100f) * 100;
	}
}