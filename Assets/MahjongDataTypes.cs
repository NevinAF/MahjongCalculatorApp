using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MahjongTypes
{
	public enum Suit
	{
		Bamboo = 0,
		Dot = 1,
		Character = 2,
		Honor = 3,
		Back = 4,
		NaN = 5,
	}
	
	public  enum Rank
	{
		NaN = 0,
		One = 1,
		Two = 2,
		Three = 3,
		Four = 4,
		Five = 5,
		Six = 6,
		Seven = 7,
		Eight = 8,
		Nine = 9,

		GreenDragon = 10,
		RedDragon = 11,
		WhiteDragon = 12,

		EastWind = 13,
		SouthWind = 14,
		WestWind = 15,
		NorthWind = 16,

		Back = 17
	}

	public static class RankExtensions
	{
		public static bool isValid(this Rank rank)
		{
			return rank == Rank.NaN;
		}

		public static bool isHonor(this Rank rank)
		{
			return rank >= Rank.GreenDragon && rank <= Rank.NorthWind;
		}

		public static bool isWind(this Rank rank)
		{
			return rank >= Rank.EastWind && rank <= Rank.NorthWind;
		}

		public static bool isDragon(this Rank rank)
		{
			return rank >= Rank.GreenDragon && rank <= Rank.WhiteDragon;
		}

		public static bool isNumber(this Rank rank)
		{
			return rank >= Rank.One && rank <= Rank.Nine;
		}

		public static bool isTerminal(this Rank rank)
		{
			return rank == Rank.One || rank == Rank.Nine;
		}

		public static bool isSimple(this Rank rank)
		{
			return rank >= Rank.Two && rank <= Rank.Eight;
		}

		public static Rank getCyclicNext(this Rank rank)
		{
			if (rank >= Rank.One && rank <= Rank.Nine)
			{
				if (rank == Rank.Nine)
					return Rank.One;
				else
					return rank + 1;
			}
			else if (rank >= Rank.EastWind && rank <= Rank.NorthWind)
			{
				if (rank == Rank.NorthWind)
					return Rank.EastWind;
				else
					return rank + 1;
			}
			else if (rank >= Rank.GreenDragon && rank <= Rank.WhiteDragon)
			{
				if (rank == Rank.WhiteDragon)
					return Rank.GreenDragon;
				else
					return rank + 1;
			}
			else
			{
				return Rank.NaN;
			}
		}
	}

	public class InvalidTileError: Exception
	{
		public InvalidTileError(string message): base(message){ }
	}

	public class InvalidHandAndTilesError: Exception
	{
		public InvalidHandAndTilesError(string message): base(message){ }
	}

	public class InvalidScoreArgumentsError: Exception
	{
		public InvalidScoreArgumentsError(string message): base(message){ }
	}

	public struct TileBuilder
	{
		public Suit? suit;
		public Rank? rank;
		public bool? isRed;
	}

	public class Tile
	{
		/// <summary> The suit of the tile. </summary>
		public Suit Suit { get; private set; }

		/// <summary> The rank of the tile. </summary>
		public Rank Rank { get; private set; }

		/// <summary> If the tile is face down (back of tile). The tile can still have a suit and rank if they can be inferred by context (like kans) </summary>
		public bool IsFaceDown { get; private set; }

		/// <summary> Iff the tile is a 5, isRed is if the tile is a red 5 </summary>
		public readonly bool IsRed;

		/// <summary> If the tile was called from another player (thus part of an open set) </summary>
		public readonly bool IsCalled;

		public Tile(Rank rank, Suit suit, bool red = false, bool called = false): this(suit, rank, red, called) { }

		public Tile(Suit suit, Rank rank, bool red = false, bool called = false)
		{
			this.Suit = suit;
			this.Rank = rank;
			this.IsRed = red;
			this.IsCalled = called;

			if (this.Suit == Suit.NaN || this.Rank == Rank.NaN)
				throw new InvalidTileError("Invalid tile: Suit or rank is NaN: " + this);
			
			if (this.Suit == Suit.Back || this.Rank == Rank.Back)
				if (this.Suit != Suit.Back || this.Rank != Rank.Back)
					throw new InvalidTileError("Invalid tile: Suit or rank is Back, but not both: " + this);
				else
					this.IsFaceDown = true;
			else this.IsFaceDown = false;
			
			if (this.Suit == Suit.Honor && this.Rank < Rank.GreenDragon)
				throw new InvalidTileError("Invalid tile: Honor suit but rank is not a dragon or wind: " + this);
			
			if (this.Suit != Suit.Honor && this.Rank >= Rank.GreenDragon && this.Rank != Rank.Back)
				throw new InvalidTileError("Invalid tile: Non-honor suit but rank is a dragon or wind: " + this);

			if (this.Rank != Rank.Five && this.IsRed)
				throw new InvalidTileError("Invalid tile: Rank is not 5 but isRed is true: " + this);
		}

		public void OverrideData(Suit suit, Rank rank, bool isRed)
		{
			if (this.Suit != Suit.Back || this.Rank != Rank.Back || !this.IsFaceDown)
				throw new InvalidTileError("Can only override data on a back tile: " + this);
			if (this.IsCalled)
				throw new InvalidTileError("Cannot override data of a called tile: " + this);

			this.Suit = suit;
			this.Rank = rank;
		}

		private bool _isGreen; // cache
		public bool IsGreen
		{
			get => this._isGreen ? true : (this._isGreen =
				(this.Suit == Suit.Bamboo && (
					this.Rank == Rank.Two ||
					this.Rank == Rank.Three ||
					this.Rank == Rank.Four ||
					this.Rank == Rank.Six ||
					this.Rank == Rank.Eight
				)) || (this.Suit == Suit.Honor && this.Rank == Rank.GreenDragon)
			);
		}

		public override string ToString()
		{
			return Tile.ShortStringifyData(this.Rank, this.Suit, this.IsRed);
		}

		public static string ShortStringifyData(Rank rank, Suit suit, bool isRed = false)
		{
			if (rank.isNumber() && (suit != Suit.Bamboo && suit != Suit.Dot && suit != Suit.Character))
			{
				return "NaN";
			}

			if (rank.isHonor() && suit != Suit.Honor)
			{
				return "NaN";
			}

			if (rank == Rank.NaN || suit == Suit.NaN)
			{
				return "NaN";
			}

			char suitChar = ' ';
			switch (suit)
			{
				case Suit.Bamboo: suitChar = 'b'; break;
				case Suit.Character: suitChar = 'n'; break;
				case Suit.Dot: suitChar = 'd'; break;
			}

			switch (rank)
			{
				case Rank.One: return "1" + suitChar;
				case Rank.Two: return "2" + suitChar;
				case Rank.Three: return "3" + suitChar;
				case Rank.Four: return "4" + suitChar;
				case Rank.Five: return ((isRed) ? "$" : "5") + suitChar;
				case Rank.Six: return "6" + suitChar;
				case Rank.Seven: return "7" + suitChar;
				case Rank.Eight: return "8" + suitChar;
				case Rank.Nine: return "9" + suitChar;
				case Rank.GreenDragon: return "gd";
				case Rank.RedDragon: return "rd";
				case Rank.WhiteDragon: return "wd";
				case Rank.EastWind: return "ew";
				case Rank.SouthWind: return "sw";
				case Rank.WestWind: return "ww";
				case Rank.NorthWind: return "nw";
				case Rank.Back: return "xx";
			}

			return "??";
		}

		public static Tile FromBuilder(TileBuilder builder)
		{
			if (!builder.suit.HasValue)
				throw new InvalidTileError("Invalid tile: Suit is undefined. Make sure that all tiles have been set!");
			if (!builder.rank.HasValue)
				throw new InvalidTileError("Invalid tile: Rank is undefined. Make sure that all tiles have been set!");
			if (!builder.isRed.HasValue)
				builder.isRed = false;

			return new Tile(builder.suit.Value, builder.rank.Value, builder.isRed.Value && builder.rank == Rank.Five);
		}

		public static Tile FromString(string tileString, bool called = false)
		{
			Suit suit = Suit.NaN;
			Rank rank = Rank.NaN;
			bool isRed = false;

			switch (tileString[0])
			{
				case '1': rank = Rank.One; break;
				case '2': rank = Rank.Two; break;
				case '3': rank = Rank.Three; break;
				case '4': rank = Rank.Four; break;
				case '5': rank = Rank.Five; break;
				case '$': rank = Rank.Five; isRed = true; break;
				case '6': rank = Rank.Six; break;
				case '7': rank = Rank.Seven; break;
				case '8': rank = Rank.Eight; break;
				case '9': rank = Rank.Nine; break;
				case 'g': rank = Rank.GreenDragon; if (tileString[1] == 'd') suit = Suit.Honor; break;
				case 'r': rank = Rank.RedDragon; if (tileString[1] == 'd') suit = Suit.Honor; break;
				case 'e': rank = Rank.EastWind; if (tileString[1] == 'w') suit = Suit.Honor; break;
				case 's': rank = Rank.SouthWind; if (tileString[1] == 'w') suit = Suit.Honor; break;
				case 'w':
					if (tileString[1] == 'w') { rank = Rank.WestWind; suit = Suit.Honor; }
					if (tileString[1] == 'd') { rank = Rank.WhiteDragon; suit = Suit.Honor; break; }
					break;
				case 'n': rank = Rank.NorthWind; if (tileString[1] == 'w') suit = Suit.Honor; break;
				case 'b': rank = Rank.Back; if (tileString[1] == 'b') suit = Suit.Back; break;
			}

			switch (tileString[1])
			{
				case 'b': suit = Suit.Bamboo; break;
				case 'd': suit = Suit.Dot; break;
				case 'n': suit = Suit.Character; break;
				case 'x': suit = Suit.Back; break;
			}

			if (rank == Rank.NaN || suit == Suit.NaN)
				throw new InvalidTileError("Invalid tile string (FromString): " + tileString);

			return new Tile(suit, rank, isRed, called);
		}

		public static bool isPair(Tile[] tiles)
		{
			if (tiles.Length != 2)
				return false;
			return tiles[0].softEquals(tiles[1]);
		}

		/**
		* Predicate for whether a set of tiles is a triplet (three of the same tile)
		* @param tiles The tiles to check
		* @returns True if the tiles are a triplet, false otherwise
		*/
		public static bool isTriplet(Tile[] tiles)
		{
			if (tiles.Length != 3)
				return false;
			
			return tiles[0].softEquals(tiles[1]) && tiles[1].softEquals(tiles[2]);
		}

		/**
		* Predicate for whether a set of tiles is a sequence (three tiles in a row)
		* @param tiles The tiles to check
		* @returns Whether the tiles are a sequence
		*/
		public static bool isSequence(Tile[] tiles)
		{
			if (tiles.Length != 3)
				return false;
			
			// If suits are the same
			if (tiles[0].Suit != tiles[1].Suit || tiles[1].Suit != tiles[2].Suit)
				return false;
			
			// If suits are numbers
			if (tiles[0].Suit == Suit.Honor || tiles[0].Suit == Suit.NaN || tiles[0].Suit == Suit.Back)
				return false;
			
			// If ranks are in order (note that the order is not necessarily 1, 2, 3)
			return (
				// 0 => 1 => 2
				(tiles[0].Rank + 1 == tiles[1].Rank && tiles[1].Rank + 1 == tiles[2].Rank) ||
				// 0 => 2 => 1
				(tiles[0].Rank + 1 == tiles[2].Rank && tiles[2].Rank + 1 == tiles[1].Rank) ||
				// 1 => 0 => 2
				(tiles[1].Rank + 1 == tiles[0].Rank && tiles[0].Rank + 1 == tiles[2].Rank) ||
				// 1 => 2 => 0
				(tiles[1].Rank + 1 == tiles[2].Rank && tiles[2].Rank + 1 == tiles[0].Rank) ||
				// 2 => 0 => 1
				(tiles[2].Rank + 1 == tiles[0].Rank && tiles[0].Rank + 1 == tiles[1].Rank) ||
				// 2 => 1 => 0
				(tiles[2].Rank + 1 == tiles[1].Rank && tiles[1].Rank + 1 == tiles[0].Rank)
			);
		}

		public static bool orderedIsSequence(Tile[] tiles)
		{
			if (tiles.Length != 3)
				return false;
			
			// If suits are the same
			if (tiles[0].Suit != tiles[1].Suit || tiles[1].Suit != tiles[2].Suit)
				return false;
			
			// If suits are numbers
			if (tiles[0].Suit == Suit.Honor || tiles[0].Suit == Suit.NaN || tiles[0].Suit == Suit.Back)
				return false;
			
			// If ranks are in order, and the order is 1, 2, 3
			return (
				// 0 => 1 => 2
				(tiles[0].Rank + 1 == tiles[1].Rank && tiles[1].Rank + 1 == tiles[2].Rank)
			);
		}

		public static bool isKan(Tile[] tiles)
		{
			if (tiles.Length != 4)
				return false;

			if (tiles[0].softEquals(tiles[1]) && tiles[1].softEquals(tiles[2]) && tiles[2].softEquals(tiles[3]))
				return true;

			return false;
		}

		public static bool isMeld(Tile[] tiles)
		{
			if (tiles.Length != 3 && tiles.Length != 4)
			{
				return false;
			}

			if (tiles.Length == 3)
			{
				return Tile.isTriplet(tiles) || Tile.isSequence(tiles);
			}
			else return Tile.isKan(tiles);
		}

		public static bool DoubleRedFives(Tile[] redFives)
		{
			if (redFives.Length > 3) return true;
			
			if (redFives.Length > 1)
			{
				bool bamboo = false, circle = false, character = false;
				foreach (Tile tile in redFives)
				{
					if (tile.Suit == Suit.Bamboo && !bamboo)
						bamboo = true;
					else if (tile.Suit == Suit.Dot && !circle)
						circle = true;
					else if (tile.Suit == Suit.Character && !character)
						character = true;
					else
						return true;
				}
			}

			return false;
		}

		public bool softEquals(Tile other)
		{
			return this.Suit == other.Suit && this.Rank == other.Rank && this.Rank != Rank.Back;
		}

		public string LongToString()
		{
			return this.Rank + ((this.Suit != Suit.Honor && this.Suit != Suit.Back) ? " of " + this.Suit : "");
		}
	}

	public enum TileGroupType
	{
		Pair = 0,
		Triplet = 1,
		Sequence = 2,
		Kan = 3,
		Orphan = 4,
		Error = 5,
	}

	public class TileGroup
	{
		public Tile[] Tiles { get; private set;}
		public bool Closed { get; private set; }
		public TileGroupType Type { get; private set; }
		public string Error { get; private set; }

		public TileGroup(Tile[] tiles, bool closed, bool canBeOrphan = false, string customError = null)
		{
			this.Tiles = tiles;
			this.Closed = closed;

			if (customError != null)
			{
				this.Type = TileGroupType.Error;
				this.Error = customError;
				return;
			}

			if (tiles.Length != 2 && tiles.Length != 3 && tiles.Length != 4)
			{
				if (tiles.Length == 1 && canBeOrphan)
				{
					this.Type = TileGroupType.Orphan;
					return;
				}

				this.Error = "Invalid number of tiles in a tile group: " + tiles.Length + this;
				this.Type = TileGroupType.Error;
				return;
			}

			if (Tile.isPair(tiles))
			{
				this.Type = TileGroupType.Pair;
				if (!closed)
				{
					this.Error = "Pairs must be closed: " + this;
					this.Type = TileGroupType.Error;
					return;
				}
			}
			else if (Tile.isTriplet(tiles))
			{
				this.Type = TileGroupType.Triplet;
			}
			else if (tiles.Length == 3)
			{
				this.Tiles.OrderBy(a => a.Rank);
				if (Tile.orderedIsSequence(tiles))
				{
					this.Type = TileGroupType.Sequence;
				}
				else
				{
					this.Error = "Invalid tiles for creating a group: " + this;
					this.Type = TileGroupType.Error;
					return;
				}
			}
			else if (Tile.isKan(tiles))
			{
				this.Type = TileGroupType.Kan;

				var backs = tiles.Where(t => t.IsFaceDown);
				if (backs.Count() == 2)
				{
					this.Closed = true;
				}
			}
			else if (tiles.Length == 4)
			{
				// Kans are also possible if exactly two tiles are backs
				Tile[] nonBacks = tiles.Where(t => t.Suit != Suit.Back && t.Rank != Rank.Back).ToArray();

				if (Tile.isPair(nonBacks))
				{
					bool needsRedFive = nonBacks[0].Rank == Rank.Five && nonBacks.FirstOrDefault(t => t.IsRed) == null;
					foreach (Tile t in tiles)
					{
						if (t.Suit == Suit.Back && t.Rank == Rank.Back)
						{
							if (needsRedFive)
							{
								t.OverrideData(nonBacks[0].Suit, nonBacks[0].Rank, true);
								needsRedFive = false;
							}
							else t.OverrideData(nonBacks[0].Suit, nonBacks[0].Rank, false);
						}
					};
					this.Type = TileGroupType.Kan;
					this.Closed = true;
				}
				else
				{
					this.Error = "Invalid tiles for creating a group: " + this;
					this.Type = TileGroupType.Error;
					return;
				}
			}
			else
			{
				this.Error = "Invalid tiles for creating a group: " + this;
				this.Type = TileGroupType.Error;
				return;
			}
		}

		public override string ToString() { return this.Tiles.Format(); }

		internal object DumpToString()
		{
			return $"{{ type: {this.Type}, closed: {this.Closed}, tiles: {this.Tiles.Format()}, error: {this.Error} }}";
		}
	}

	public enum Yaakuman { None = 0, Single = 1, Double = 2 }

	public class Win
	{
		public readonly string name;
		public readonly string desc;
		public readonly int open_han;
		public readonly int closed_han;
		public readonly Yaakuman yaakuman;

		public Win(string name, string desc, int open_han = -1, int closed_han = -1, Yaakuman yaakuman = Yaakuman.None)
		{
			this.name = name;
			this.desc = desc;
			this.open_han = open_han;
			this.closed_han = closed_han;
			this.yaakuman = yaakuman;
		}

		override public string ToString()
		{
			return this.name;
		}
	}
}