using ImageEditing;
namespace lab3console
{
    public class Program
    {
        static void ImageEditor()
        {
            Image image = new("C:\\Users\\haide\\Desktop\\480-360-sample - Copy.bmp");
            int option = Convert.ToInt32(Console.ReadLine());
            switch (option)
            {
                case 1:
                    {
                        int angle = Convert.ToInt32(Console.ReadLine());
                        int x = Convert.ToInt32(Console.ReadLine());
                        int y = Convert.ToInt32(Console.ReadLine());
                        image.Rotate(angle, x, y);
                        break;
                    }
                case 2:
                    {
                        int x1 = Convert.ToInt32(Console.ReadLine());
                        int y1 = Convert.ToInt32(Console.ReadLine());
                        int x2 = Convert.ToInt32(Console.ReadLine());
                        int y2 = Convert.ToInt32(Console.ReadLine());
                        image.Crop(x1, y1, x2, y2);
                        break;
                    }
                case 3:
                    {
                        int x = Convert.ToInt32(Console.ReadLine());
                        int y = Convert.ToInt32(Console.ReadLine());
                        byte red = Convert.ToByte(Console.ReadLine());
                        byte green = Convert.ToByte(Console.ReadLine());
                        byte blue = Convert.ToByte(Console.ReadLine());
                        image.SetPixel(x, y, red, green, blue);
                        break;
                    }
                case 4:
                    {
                        int x = Convert.ToInt32(Console.ReadLine());
                        int y = Convert.ToInt32(Console.ReadLine());
                        image.MakeCollage(image, x, y);
                        break;
                    }
            }
            image.SaveToFile("C:\\Users\\haide\\Desktop\\480-360-sample - Copy1.bmp");
        }

        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += (sender, args) => { Console.WriteLine("ss"); };
            ImageEditor();
        }
    }
}
