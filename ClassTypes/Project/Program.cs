using Project;
internal class Program
{
    private static void Main(string[] args)
    {

        Veri1 veri = new Veri1("Taha", 27);

        veri.isim = "Ahmet";

        veri = veri with { isim = "Ahmet" };





        //Console.WriteLine
        //    (Taha.Text);
    }
}

