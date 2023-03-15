#define TOOLING_MASK

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mask
{
	public readonly int x1;
	public readonly int y1;
	public readonly int x2;
	public readonly int y2;
	private readonly CompressedMask compressedMask;

	public Mask(int[] box, byte[] compressedMask) : this(box[0], box[1], box[2], box[3], compressedMask)
	{
		if (box.Length != 4)
		{
			throw new System.ArgumentException("Box must be an array of length 4! [x1, y1, x2, y2]");
		}
	}


	public Mask(int x1, int y1, int x2, int y2, byte[] compressedMask)
	{
		this.x1 = x1;
		this.y1 = y1;
		this.x2 = x2;
		this.y2 = y2;
		this.compressedMask = new CompressedMask(compressedMask);


		#if DEBUG
		Debug.Assert(Width > 0, "Mask width must be positive! Point x1 is greater than x2!");
		Debug.Assert(Height > 0, "Mask height must be positive! Point y1 is greater than y2!");
		Debug.Assert(Mathf.Abs(this.compressedMask.Decompress().Length * 8 - Width * Height) < 8, "Mask size does not match the size of the compressed mask! This is likely due to a bug in the compression algorithm: Compressed.Length = " + compressedMask.Length + ", DecompressMask.Length = " + this.compressedMask.Decompress().Length * 8 + ", Expected = " + Width * Height + " + [0-7]");
		#endif
	}

	public int Width => x2 - x1;
	public int Height => y2 - y1;


	private Texture2D m_texture = null;
	public Texture2D Texture => CreateTexture();

	public Texture2D CreateTexture(bool forceDraw = false)
	{
		if (m_texture == null || forceDraw)
		{
			byte[] mask = compressedMask.Decompress();

			m_texture = new Texture2D(Width, Height, TextureFormat.Alpha8, false);
			m_texture.filterMode = FilterMode.Point;
			// m_texture.wrapMode = TextureWrapMode.Clamp;

			Color[] colors = new Color[Width * Height];

			for (int i = 0; i < colors.Length; i++)
			{
				colors[i] = IsPixelMasked(mask, i) ? Color.white : Color.clear; 
			}

			m_texture.SetPixels(colors);
			m_texture.Apply();
		}

		return m_texture;
	}

	/// <summary>
	/// Utility function for returning true if the pixel at the given index is masked. The mask is stored as a byte array, where each bit (1 or 0) represents a pixel.
	/// </summary>
	/// <param name="mask">Byte array that contains the mask pixels. Each byte represents 8 pixels, with the pixels read as a continuous line, row first</param>
	/// <param name="pixelIndex">Index of the pixel to check. Same as width * y_pos + x_pos.</param>
	/// <returns>True if the pixel is masked, false otherwise</returns>
	public static bool IsPixelMasked(byte[] mask, int pixelIndex)
	{
		// mask[pixelIndex >> 3] is the byte that contains the pixelIndex bit
		// (1 << (pixelIndex % 8)) is the bit that we want to check
		var index = pixelIndex >> 3;
		return index < mask.Length && (mask[index] & (1 << (pixelIndex % 8))) != 0;
	}

	public struct CompressedMask
	{
		public byte[] data;

		public CompressedMask(byte[] data)
		{
			this.data = data;
		}

		private static List<byte> _workingMask;
		public byte[] Decompress()
		{
			// The byte array is compressed using each byte to represent a signal change, starting with 0.
			// If the byte is 255, then the signal stays the same for 255 pixels, with no change.

	#if TOOLING_MASK
			// Time the decompression
			System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
			stopwatch.Start();
	#endif

			if (_workingMask == null) _workingMask = new List<byte>();
			else _workingMask.Clear();

			byte currentByte = 0;
			byte byteIndex = 8;
			bool currentSignal = false;
			bool nextSignal = false;

			for (int i = 0; i < data.Length; i++)
			{

				currentSignal = nextSignal;
				short spacing = data[i];

				if (spacing != 255) nextSignal = !currentSignal;

				if (byteIndex != 8)
				{
					while (byteIndex != 8 && spacing > 0)
					{
						if(byteIndex >= 8) {
							Debug.Log("ERROR!!");
							return new byte[] {};
						}
						if(spacing <= 0) {
							Debug.Log("ERROR!!");
							return new byte[] {};
						}

						if (currentSignal)
							currentByte |= (byte)((1 << byteIndex));

						byteIndex++;
						spacing--;
					}

					if (byteIndex != 8) continue;

					_workingMask.Add(currentByte);
					currentByte = 0;
				}

				if (spacing == 0) continue;

				while (spacing >= 8)
				{
					_workingMask.Add(currentSignal ? (byte)255 : (byte)0);
					spacing -= 8;
				}

				if(currentByte != 0) {
					Debug.Log("ERROR!!");
					return new byte[] {};
				}
				if(byteIndex != 8) {
					Debug.Log("ERROR!!");
					return new byte[] {};
				}
				if(spacing >= 8) {
					Debug.Log("ERROR!!");
					return new byte[] {};
				}

				if (spacing == 0) continue;
				
				byteIndex = 0;
				while (spacing > 0)
				{
					if(byteIndex >= 8) {
						Debug.Log("ERROR!!");
						return new byte[] {};
					}
					if(spacing >= 8) {
						Debug.Log("ERROR!!");
						return new byte[] {};
					}
					if (currentSignal)
						currentByte |= (byte)((1 << byteIndex));

					byteIndex++;
					spacing--;
				}

				if (byteIndex == 8)
				{
					_workingMask.Add(currentByte);
					currentByte = 0;
				}
			}

			if (byteIndex != 8)
				_workingMask.Add(currentByte);

	#if TOOLING_MASK_MASK
			stopwatch.Stop();
			Debug.Log("Decompressed mask of size " + _workingMask.Count() + " in " + stopwatch.ElapsedMilliseconds + "ms");
	#endif

			return _workingMask.ToArray();
		}
	}
}
