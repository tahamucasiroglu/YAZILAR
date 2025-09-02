namespace Project
{
    public record Veri1(string isim, int yas);

    public record Veri2
    {
        public string isim { get; init; } = string.Empty;
        public int yas { get; init; }
    }
}
