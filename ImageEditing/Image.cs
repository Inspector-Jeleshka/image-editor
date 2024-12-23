namespace ImageEditing
{
	public class Image
	{
		private string path = null!;
		private int fileSize;
		private int pixelArrayOffset;
		private DIBHeader header;
		private Pixel[,] pixelArray = null!;


		public Image(string path)
		{
			try
			{
				this.path = path;
				using FileStream file = File.OpenRead(path);
				byte[] fileData = new byte[file.Length];
				file.Read(fileData, 0, (int)file.Length);

				if (BitConverter.ToInt16(fileData, 0) != 0x424d)
					throw new NotSupportedException("Incorrect signature");
				if (BitConverter.ToInt32(fileData, 22) < 0)
					throw new NotSupportedException("Unsupported pixel array layout (negative image height)");
				if (BitConverter.ToInt16(fileData, 28) is not 24)
					throw new NotSupportedException("Unsupported color depth");
				if (BitConverter.ToInt32(fileData, 30) != 0)
					throw new NotSupportedException("Unsupported compression method");

				fileSize = BitConverter.ToInt32(fileData, 2);
				pixelArrayOffset = BitConverter.ToInt32(fileData, 10);
				int imageWidth = BitConverter.ToInt32(fileData, 18);
				int imageHeight = BitConverter.ToInt32(fileData, 22);
				int bitsPerPixel = BitConverter.ToInt16(fileData, 28);
				int imageSize = BitConverter.ToInt32(fileData, 34);
				int horizontalResolution = BitConverter.ToInt32(fileData, 38);
				int verticalResolution = BitConverter.ToInt32(fileData, 42);
				int colorTableSize = BitConverter.ToInt32(fileData, 46);

				header = new(imageWidth, imageHeight, imageSize, horizontalResolution, verticalResolution, colorTableSize);
				pixelArray = new Pixel[imageHeight, imageWidth];

				for (int i = 0; i < imageHeight; i++)
					for (int j = 0; j < imageWidth; j++)
					{
						byte blue = fileData[pixelArrayOffset + (imageHeight - i) * RowSize + j * bitsPerPixel / 8];
						byte green = fileData[pixelArrayOffset + (imageHeight - i) * RowSize + j * bitsPerPixel / 8 + 1];
						byte red = fileData[pixelArrayOffset + (imageHeight - i) * RowSize + j * bitsPerPixel / 8 + 2];

						pixelArray[i, j] = new(red, green, blue);
					}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}


		public int RowSize => (int)Math.Ceiling(header.bitsPerPixel * header.bmpWidth / 32d) * 4;

		
		public void Crop(int x1, int y1, int x2, int y2)
		{
			if (x1 < 0 || x2 < 0 || y1 < 0 || y2 < 0)
				throw new ArgumentOutOfRangeException(nameof(x1), "Less than 0");

			int newImageWidth = Math.Abs(x2 - x1);
			int newImageHeight = Math.Abs(y2 - y1);
			if (newImageWidth == 0 || newImageHeight == 0)
				throw new ArgumentException("Image can't have side with a 0 length");

			Pixel[,] croppedImage = new Pixel[newImageHeight, newImageWidth];
			x1 = x1 < x2 ? x1 :	x2;
			y1 = y1 < y2 ? y1 : y2;

			for (int i = 0; i < newImageHeight; i++)
				for (int j = 0; j < newImageWidth; j++)
					croppedImage[i, j] = pixelArray[y1 + i, x1 + j];

			pixelArray = croppedImage;

			header.bmpHeight = newImageHeight;
			header.bmpWidth = newImageWidth;
		}

		// Counterclockwise rotation around (x, y) pixel
		public void Rotate(float angle, int x, int y)
		{
			if (x < 0 || y < 0)
				throw new ArgumentOutOfRangeException(x < 0 ? nameof(x) : nameof(y), "Less than 0");

			int imageHeight = header.bmpHeight, imageWidth = header.bmpWidth;
			Pixel[,] rotatedPixelArray = new Pixel[imageHeight, imageWidth];

			for (int i = 0; i < imageHeight; i++)
				for (int j = 0; j < imageWidth; j++)
				{
					int xActual = j - x;
					int yActual = i - y;
					int xRotated = (int)(xActual * Math.Cos(angle) - yActual * Math.Sin(angle));
					int yRotated = (int)(xActual * Math.Sin(angle) + yActual * Math.Cos(angle));

					rotatedPixelArray[xRotated, yRotated] = pixelArray[i, j];
				}

			pixelArray = rotatedPixelArray;
		}
		public void Save()
		{
			try
			{
				using FileStream file = File.OpenWrite(path);

				byte[] data = new byte[fileSize];

				file.Write(, )
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}


		public struct Pixel
		{
			public byte Red;
			public byte Green;
			public byte Blue;

			public Pixel(byte red, byte green, byte blue)
			{
				Red = red;
				Green = green;
				Blue = blue;
			}
		}
		private struct DIBHeader
		{
			public readonly int headerSize = 40;
			public int bmpWidth;
			public int bmpHeight;
			public readonly short colorPlanes = 1;
			public readonly short bitsPerPixel = 24;
			public readonly int compression = 0;
			public int bmpSize;
			public int horizontalResolution;
			public int verticalResolution;
			public int colorTableSize;
			public readonly int importantColors = 0;

			public DIBHeader(int bmpWidth, int bmpHeight, int bmpSize, int horizontalResolution, int verticalResolution, int colorTableSize)
			{
				this.bmpWidth = bmpWidth;
				this.bmpHeight = bmpHeight;
				this.bmpSize = bmpSize;
				this.horizontalResolution = horizontalResolution;
				this.verticalResolution = verticalResolution;
				this.colorTableSize = colorTableSize;
			}
		}
	}
}
