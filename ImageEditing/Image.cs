namespace ImageEditing
{
	public class Image
	{
		//private int fileSize;
		//private int pixelArrayOffset;
		private int imageWidth;
		private int imageHeight;

		public Image(int width, int height)
		{

		}
		public Image(string path)
		{
			try
			{
				using FileStream file = File.OpenRead(path);

				byte[] bitmapHeader = new byte[14];
				file.Read(bitmapHeader, 0, bitmapHeader.Length);
				if (BitConverter.ToInt16(bitmapHeader, 0) != 0x424d) // check for BMP signature
					throw new NotSupportedException("Specified file isn't a BMP image");

				int fileSize = BitConverter.ToInt32(bitmapHeader, 2);
				int pixelArrayOffset = BitConverter.ToInt32(bitmapHeader, 10);
				byte[] dibHeaderSize = new byte[4];
				file.Read(dibHeaderSize, 14, dibHeaderSize.Length);
				
				byte[] dibHeader = new byte[BitConverter.ToInt32(dibHeaderSize)];
				file.Read(dibHeader, 14, dibHeader.Length);

				imageWidth = BitConverter.ToInt32(dibHeader, 18);
				imageHeight = BitConverter.ToInt32(dibHeader, 22);
				short colorPlanes = BitConverter.ToInt16(dibHeader, 26); // must be 1
				short bitsPerPixel = BitConverter.ToInt16(dibHeader, 28);
				int compression = BitConverter.ToInt32(dibHeader, 30);
				int imageSize = BitConverter.ToInt32(dibHeader, 34); // the size of the raw bitmap data
				int xPixelsPerMetre = BitConverter.ToInt32(dibHeader, 38); // the horizontal resolution of the image
				int yPixelsPerMetre = BitConverter.ToInt32(dibHeader, 42); // the vertical resolution of the image
				int colorsInColorTable = BitConverter.ToInt32(dibHeader, 46); // if 0 then 2^n
				int importantColorsCount = BitConverter.ToInt32(dibHeader, 50);

				
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}
		}
	}
}
