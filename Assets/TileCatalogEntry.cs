using UnityEngine;
using MahjongTypes;

[CreateAssetMenu(fileName = "TileCatalogEntry", menuName = "Mahjong/TileCatalogEntry", order = 0)]
public class TileCatalogEntry : ScriptableObject
{
	public Rank Rank;
	public Suit Suit;
	public bool IsRed;
	public Sprite Sprite;

	public string ShortName { get => Tile.ShortStringifyData(Rank, Suit, IsRed); }
	public string LongName { get => $"{System.Enum.GetName(typeof(Rank), Rank)} of {System.Enum.GetName(typeof(Suit), Suit)}"; }


	public override string ToString()
	{
		return ShortName;
	}
}