using MahjongTypes;

public static class WinCatalog
{
	public static readonly Win
	Double_Riichi = new(
		name: "Double Riichi",
		desc: "Riichi twice in the same hand",
		closed_han: 1),
	Riichi = new(
		name: "Riichi",
		desc: "Declare riichi before the first discard",
		closed_han: 1),
	Ippatsu = new(
		name: "Ippatsu",
		desc: "Win on the first discard after declaring riichi",
		closed_han: 1),
	Menzenchin_Tsumo = new(
		name: "Menzenchin Tsumo",
		desc: "Win by tsumo with no discards",
		closed_han: 1),
	Under_The_Sea = new(
		name: "Under The Sea",
		desc: "Win with all tiles in your hand being 1-4",
		open_han: 1,
		closed_han: 1),
	Under_The_River = new(
		name: "Under The River",
		desc: "Win with all tiles in your hand being 5-8",
		open_han: 1,
		closed_han: 1),
	Rinshan_Kaihou = new(
		name: "Rinshan Kaihou",
		desc: "Win by tsumo after drawing the winning tile from the wall",
		open_han: 1,
		closed_han: 1),
	Robbing_A_Kan = new(
		name: "Robbing A Kan",
		desc: "Win by tsumo after drawing the winning tile from a kan",
		open_han: 1,
		closed_han: 1),
	Seven_Pairs = new(
		name: "Seven Pairs",
		desc: "Win with seven pairs",
		closed_han: 2),
	Thirteen_Orphans = new(
		name: "Thirteen Orphans",
		desc: "Win with all terminals and honors",
		closed_han: 0,
		yaakuman: Yaakuman.Single),
	Pure_Thirteen_Orphans = new(
		name: "True Thirteen Orphans",
		desc: "Win with all terminals and honors",
		closed_han: 0,
		yaakuman: Yaakuman.Double),
	Pinfu = new(
		name: "Pinfu",
		desc: "Win with a simple hand",
		closed_han: 1),
	Pure_Double_Sequence = new(
		name: "Pure Double Sequence",
		desc: "Win with two sequences of the same suit",
		open_han: 1,
		closed_han: 1),
	Twice_Pure_Double_Sequence = new(
		name: "Twice Pure Double Sequence",
		desc: "Win with two sequences of the same suit and two sequences of the same suit",
		open_han: 3,
		closed_han: 3),
	Red_Dragon_Yakuhai = new(
		name: "Red Dragon Yakuhai",
		desc: "Win with a dragon yakuhai",
		open_han: 1,
		closed_han: 1),
	White_Dragon_Yakuhai = new(
		name: "White Dragon Yakuhai",
		desc: "Win with a dragon yakuhai",
		open_han: 1,
		closed_han: 1),
	Green_Dragon_Yakuhai = new(
		name: "Green Dragon Yakuhai",
		desc: "Win with a dragon yakuhai",
		open_han: 1,
		closed_han: 1),
	Prevalent_Wind = new(
		name: "Prevalent Wind",
		desc: "Win with the prevalent wind in your hand",
		open_han: 1,
		closed_han: 1),
	Seat_Wind = new(
		name: "Seat Wind",
		desc: "Win with the seat wind in your hand",
		open_han: 1,
		closed_han: 1),
	Little_Three_Dragons = new(
		name: "Little Three Dragons",
		desc: "Win with the east, south, and west dragons in your hand",
		open_han: 2,
		closed_han: 2),
	Big_Three_Dragons = new(
		name: "Big Three Dragons",
		desc: "Win with the east, south, and west dragons in your hand",
		open_han: 0,
		closed_han: 0,
		yaakuman: Yaakuman.Single),
	Little_Four_Winds = new(
		name: "Little Four Winds",
		desc: "Win with the east, south, west, and north winds in your hand",
		open_han: 0,
		closed_han: 0,
		yaakuman: Yaakuman.Single),
	Big_Four_Winds = new(
		name: "Big Four Winds",
		desc: "Win with the east, south, west, and north winds in your hand",
		open_han: 0,
		closed_han: 0,
		yaakuman: Yaakuman.Double),
	Mixed_Triple_Sequence = new(
		name: "Mixed Triple Sequence",
		desc: "Win with three sequences of different suits",
		open_han: 1,
		closed_han: 2),
	Pure_Straight = new(
		name: "Pure Straight",
		desc: "Win with a straight of all the same suit",
		open_han: 1,
		closed_han: 2),
	All_Terminals = new(
		name: "All Terminals",
		desc: "Win with all terminals in your hand",
		open_han: 0,
		closed_han: 0,
		yaakuman: Yaakuman.Single),
	All_Honors = new(
		name: "All Honors",
		desc: "Win with all honors in your hand",
		open_han: 0,
		closed_han: 0,
		yaakuman: Yaakuman.Single),
	All_Simples = new(
		name: "All Simples",
		desc: "Win with all simples in your hand",
		open_han: 1,
		closed_han: 1),
	Full_Outside_Hand = new(
		name: "Full Outside Hand",
		desc: "Win with all terminals and honors in your hand",
		open_han: 3,
		closed_han: 2),
	Half_Outside_Hand = new(
		name: "Half Outside Hand",
		desc: "Win with all terminals and honors in your hand",
		open_han: 1,
		closed_han: 2),
	All_Triplets = new(
		name: "All Triplets",
		desc: "Win with all triplets in your hand",
		open_han: 2),
	Three_Concealed_Triplets = new(
		name: "Three Concealed Triplets",
		desc: "Win with three concealed triplets",
		open_han: 2,
		closed_han: 2),
	Four_Concealed_Triplets = new(
		name: "Four Concealed Triplets",
		desc: "Win with four concealed triplets",
		closed_han: 0,
		yaakuman: Yaakuman.Single),
	Single_Wait_Four_Concealed_Triplets = new(
		name: "Single Wait Four Concealed Triplets",
		desc: "Win with four concealed triplets and a single wait",
		closed_han: 0,
		yaakuman: Yaakuman.Double),
	Three_Kans = new(
		name: "Three Kan",
		desc: "Win with three kan",
		open_han: 2,
		closed_han: 2),
	Four_Kans = new(
		name: "Four Kan",
		desc: "Win with four kan",
		open_han: 0,
		closed_han: 0,
		yaakuman: Yaakuman.Single),
	Triple_Triplets = new(
		name: "Triple Triplets",
		desc: "Win with three triplets",
		open_han: 2,
		closed_han: 2),
	All_Green = new(
		name: "All Green",
		desc: "Win with all green tiles in your hand",
		open_han: 0,
		closed_han: 0,
		yaakuman: Yaakuman.Single),
	True_Nine_Gates = new(
		name: "True Nine Gates",
		desc: "Win with all terminals and honors in your hand",
		closed_han: 0,
		yaakuman: Yaakuman.Double),
	Nine_Gates = new(
		name: "Nine Gates",
		desc: "Win with all terminals and honors in your hand",
		closed_han: 0,
		yaakuman: Yaakuman.Single),
	Full_Flush = new(
		name: "Full Flush",
		desc: "Win with a flush of all the same suit",
		open_han: 5,
		closed_han: 6),
	Half_Flush = new(
		name: "Half Flush",
		desc: "Win with a flush of all the same suit",
		open_han: 2,
		closed_han: 3),
	Dora_Indicator = new(
		name: "Dora Indicator",
		desc: "Win with a dora indicator in your hand",
		open_han: 1,
		closed_han: 1),
	Red_Five = new(
		name: "Red Five",
		desc: "Win with a red five in your hand",
		open_han: 1,
		closed_han: 1);

	public static readonly Rank[] NineGatesHand = new Rank[] { Rank.One, Rank.One, Rank.One, Rank.Two, Rank.Three, Rank.Four, Rank.Five, Rank.Six, Rank.Seven, Rank.Eight, Rank.Nine, Rank.Nine, Rank.Nine };
}