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
			this.path = path;
			using FileStream file = File.OpenRead(path);
			byte[] fileData = new byte[file.Length];
			file.Read(fileData, 0, (int)file.Length);

			if (BitConverter.ToInt16(fileData, 0) is not (0x424d or 0x4d42))
				throw new NotSupportedException("Incorrect signature");
			if (BitConverter.ToInt32(fileData, 22) < 0)
				throw new NotSupportedException("Unsupported pixel array layout (negative image height)");
			if (BitConverter.ToInt16(fileData, 28) is not 24)
				throw new NotSupportedException("Unsupported color depth");
			if (BitConverter.ToInt32(fileData, 30) != 0)
				throw new NotSupportedException("Unsupported compression method");
			if (BitConverter.ToInt32(fileData, 46) != 0)
				throw new NotSupportedException("Unsupported color table");

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
					byte blue = fileData[pixelArrayOffset + (imageHeight - i - 1) * RowSize + j * bitsPerPixel / 8];
					byte green = fileData[pixelArrayOffset + (imageHeight - i - 1) * RowSize + j * bitsPerPixel / 8 + 1];
					byte red = fileData[pixelArrayOffset + (imageHeight - i - 1) * RowSize + j * bitsPerPixel / 8 + 2];

					pixelArray[i, j] = new(red, green, blue);
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
			header.bmpSize = RowSize * newImageHeight;
			fileSize = pixelArrayOffset + header.bmpSize;
		}

		// Counterclockwise rotation around (x, y) pixel, angle in degrees
		public void Rotate(double angle, int x, int y)
		{
			if (x < 0 || y < 0)
				throw new ArgumentOutOfRangeException(x < 0 ? nameof(x) : nameof(y), "Less than 0");

			int imageHeight = header.bmpHeight, imageWidth = header.bmpWidth;
			Pixel[,] rotatedPixelArray = new Pixel[imageHeight, imageWidth];
			angle = angle * Math.PI / 180;

			for (int i = 0; i < imageHeight; i++)
				for (int j = 0; j < imageWidth; j++)
				{
					int xActual = j - x;
					int yActual = i - y;
					int xRotated = (int)(xActual * Math.Cos(angle) - yActual * Math.Sin(angle));
					int yRotated = (int)(xActual * Math.Sin(angle) + yActual * Math.Cos(angle));
					xRotated += x;
					yRotated += y;

					if (xRotated >= 0 && xRotated < imageWidth && yRotated >= 0 && yRotated < imageHeight)
						rotatedPixelArray[i, j] = pixelArray[yRotated, xRotated];
				}

			pixelArray = rotatedPixelArray;
		}

		public void MakeCollage(Image image, int xPos, int yPos)
		{
			int rightEnd = Math.Max(image.header.bmpWidth + xPos, header.bmpWidth);
			int leftEnd = Math.Min(xPos, 0);
			int lowEnd = Math.Max(image.header.bmpHeight + yPos, header.bmpHeight);
			int highEnd = Math.Min(yPos, 0);
			int width = rightEnd - leftEnd;
			int height = lowEnd - highEnd;

			Pixel[,] collage = new Pixel[height, width];

			for (int y = 0; y < header.bmpHeight; y++)
				for (int x = 0; x < header.bmpWidth; x++)
					collage[y - highEnd, x - leftEnd] = pixelArray[y, x];
			for (int y = 0; y < image.header.bmpHeight; y++)
				for (int x = 0; x < image.header.bmpWidth; x++)
					collage[y + yPos - highEnd, x + xPos - leftEnd] = image.pixelArray[y, x];

			pixelArray = collage;
			header.bmpWidth = width;
			header.bmpHeight = height;
			header.bmpSize = height * RowSize;
			fileSize = pixelArrayOffset + header.bmpSize;
		}

		public void SetPixel(int x, int y, byte red, byte green, byte blue)
		{
			SetPixel(x, y, new(red, green, blue));
		}
		public void SetPixel(int x, int y, Pixel pixel)
		{
			if (x < 0 || y < 0)
				throw new ArgumentOutOfRangeException(x < 0 ? nameof(x) : nameof(y), "Less than 0");

			pixelArray[y, x] = pixel;
		}

		public void SaveToFile()
		{
			SaveToFile(path);
		}
		public void SaveToFile(string path)
		{
			try
			{
				using FileStream file = File.OpenWrite(path);

				List<byte> data = new();
				data.AddRange(BitConverter.GetBytes((short)0x4d42));
				data.AddRange(BitConverter.GetBytes(fileSize));
				data.AddRange(BitConverter.GetBytes(0));
				data.AddRange(BitConverter.GetBytes(pixelArrayOffset));
				data.AddRange(BitConverter.GetBytes(header.headerSize));
				data.AddRange(BitConverter.GetBytes(header.bmpWidth));
				data.AddRange(BitConverter.GetBytes(header.bmpHeight));
				data.AddRange(BitConverter.GetBytes(header.colorPlanes));
				data.AddRange(BitConverter.GetBytes(header.bitsPerPixel));
				data.AddRange(BitConverter.GetBytes(header.compression));
				data.AddRange(BitConverter.GetBytes(header.bmpSize));
				data.AddRange(BitConverter.GetBytes(header.horizontalResolution));
				data.AddRange(BitConverter.GetBytes(header.verticalResolution));
				data.AddRange(BitConverter.GetBytes(header.colorTableSize));
				data.AddRange(BitConverter.GetBytes(header.importantColors));
				byte[] gap = new byte[pixelArrayOffset - 14 - header.headerSize];
				data.AddRange(gap);

				byte[] padding = new byte[RowSize - header.bmpWidth * header.bitsPerPixel / 8];
				for (int i = header.bmpHeight - 1; i >= 0; i--)
				{
					for (int j = 0; j < header.bmpWidth; j++)
					{
						data.Add(pixelArray[i, j].Blue);
						data.Add(pixelArray[i, j].Green);
						data.Add(pixelArray[i, j].Red);
					}
					data.AddRange(padding);
				}

				if (data.Count % 4 != 0)
					data.AddRange(new byte[4 - data.Count % 4]);

				file.Write(data.ToArray(), 0, fileSize);
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
