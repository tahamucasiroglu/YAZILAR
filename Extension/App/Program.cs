internal class Program
{
    private static void Main(string[] args)
    {
        int sayi = 5;
        Console.WriteLine(sayi.Pow(1));  //Çıktı = 5
        Console.WriteLine(sayi.Pow(2));  //Çıktı = 25
        Console.WriteLine(sayi.Pow(3));  //Çıktı = 625
        Console.WriteLine(sayi.Pow(4));  //Çıktı = 390625
    }

}

static public class Extension
{
   static public int Pow(this int value, int powIndex = 2)
    {
        for (int i = 0; i < powIndex - 1; i++)
        {
            value *= value;
        }
        return value;
    }
}
