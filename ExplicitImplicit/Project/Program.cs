internal class Program
{
    private static void Main(string[] args)
    {
        TurkLirasi turkLirasi = 1000.0m;
        AmerikanDolari amerikanDolari = 1000.0m;

        /*1*/Console.WriteLine(amerikanDolari.Deger);
        /*2*/Console.WriteLine(((TurkLirasi)amerikanDolari).Deger);

        /*3*/Console.WriteLine(turkLirasi.Deger);
        /*4*/Console.WriteLine(((AmerikanDolari)turkLirasi).Deger);

        // Çıktı

        /*
            1-) 1000,0
            2-) 41100,00
            3-) 1000,0
            4-) 24,330900243309002433090024331
         */
    }
}


public class TurkLirasi
{
    public decimal Deger { get; set; }
    public static explicit operator AmerikanDolari
        (TurkLirasi turkLirasi)
    {
        return new AmerikanDolari() { Deger = turkLirasi.Deger / 41.1m };
    }
    public static implicit operator TurkLirasi(decimal para)
    {
        return new TurkLirasi() { Deger = para };
    }
}
public class AmerikanDolari
{
    public decimal Deger { get; set; }
    public static explicit operator TurkLirasi
        (AmerikanDolari amerikanDolari)
    {
        return new TurkLirasi() { Deger = amerikanDolari.Deger * 41.1m };
    }
    public static implicit operator AmerikanDolari(decimal para)
    {
        return new AmerikanDolari() { Deger = para };
    }
}