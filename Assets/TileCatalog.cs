using System.Linq;
using MahjongTypes;
using UnityEngine;


public class TileCatalog : Manager<TileCatalog>
{
	[SerializeField] TileCatalogEntry[] _catalog;

	public static TileCatalogEntry[] Catalog { get => Instance._catalog; }

	public static TileCatalogEntry GetEntry(Rank rank, Suit suit, bool isRed = false)
	{
		var result = Catalog.FirstOrDefault(x => x.Rank == rank && x.Suit == suit && x.IsRed == isRed);

		if (result == null)
		{
			Debug.LogWarning($"No entry found for {(isRed ? "red " : "")}{rank} of {suit}!");
		}

		return result;
	}
}