using ImageEditing;

namespace lab3console
{
	public class Program
	{
		static void ImageEditor()
		{
			Image image = null!;
			while (image == null)
			{
				try
				{
					Console.Write("Input path to image: ");
					string path = Console.ReadLine()!;
					image = new(path);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
			}

			while (true)
			{
				try
				{
					Console.WriteLine("1 - Rotate image");
					Console.WriteLine("2 - Crop image");
					Console.WriteLine("3 - Change pixel");
					Console.WriteLine("4 - Make collage");
					Console.WriteLine("5 - Add title on top of image");
					Console.WriteLine("6 - Edit image brightness");
					Console.WriteLine("7 - Edit image contrast");
					Console.WriteLine("s - Save");
					Console.WriteLine("q - Exit");

					Console.Write("Choose option: ");
					char option = Console.ReadLine()![0];
					switch (option)
					{
						case '1':
							{
								Console.Write("Input angle: ");
								int angle = Convert.ToInt32(Console.ReadLine());
								Console.Write("Input x coordinate: ");
								int x = Convert.ToInt32(Console.ReadLine());
								Console.Write("Input y coordinate: ");
								int y = Convert.ToInt32(Console.ReadLine());
								image.Rotate(angle, x, y);
								Console.WriteLine("Image rotated\n");
								break;
							}
						case '2':
							{
								Console.Write("Input first point x coordinate: ");
								int x1 = Convert.ToInt32(Console.ReadLine());
								Console.Write("Input first point y coordinate: ");
								int y1 = Convert.ToInt32(Console.ReadLine());
								Console.Write("Input second point x coordinate: ");
								int x2 = Convert.ToInt32(Console.ReadLine());
								Console.Write("Input second point y coordinate: ");
								int y2 = Convert.ToInt32(Console.ReadLine());
								image.Crop(x1, y1, x2, y2);
								Console.WriteLine("Image cropped\n");
								break;
							}
						case '3':
							{
								Console.Write("Input x coordinate: ");
								int x = Convert.ToInt32(Console.ReadLine());
								Console.Write("Input y coordinate: ");
								int y = Convert.ToInt32(Console.ReadLine());
								Console.Write("Input red value: ");
								byte red = Convert.ToByte(Console.ReadLine());
								Console.Write("Input green value: ");
								byte green = Convert.ToByte(Console.ReadLine());
								Console.Write("Input blue value: ");
								byte blue = Convert.ToByte(Console.ReadLine());
								image.SetPixel(x, y, red, green, blue);
								Console.WriteLine("Pixel changed\n");
								break;
							}
						case '4':
							{
								Console.Write("Input path to image: ");
								Image image1 = new(Console.ReadLine()!);
								Console.Write("Input x coordinate: ");
								int x = Convert.ToInt32(Console.ReadLine());
								Console.Write("Input y coordinate: ");
								int y = Convert.ToInt32(Console.ReadLine());
								image.MakeCollage(image, x, y);
								Console.WriteLine("Collage created\n");
								break;
							}
						case '5':
							{
								Console.Write("Input text to put on image: ");
								string title = Console.ReadLine() ?? string.Empty;
								Console.Write("Input x coordinate: ");
								int x = Convert.ToInt32(Console.ReadLine());
								Console.Write("Input y coordinate: ");
								int y = Convert.ToInt32(Console.ReadLine());
								image.AddTitle(title, x, y);
								Console.WriteLine("Title added\n");
								break;
							}
						case '6':
							{
								Console.Write("Input value: ");
								int delta = Convert.ToInt32(Console.ReadLine());
								image.EditBrightness(delta);
								Console.WriteLine("Brightness changed\n");
								break;
							}
						case '7':
							{
								Console.Write("Input value: ");
								double value = Convert.ToDouble(Console.ReadLine());
								image.EditContrast(value);
								Console.WriteLine("Contrast changed\n");
								break;
							}
						case 's':
							image.SaveToFile();
							Console.WriteLine("Image saved\n");
							break;
						case 'q':
							return;
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
			}
		}

		static void Main(string[] args)
		{
			ImageEditor();
		}
	}
}
