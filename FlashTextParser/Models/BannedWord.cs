namespace FlashTextParser.Models
{
    public class BannedWord
    {
        public int IdKey { get; set; }
        public string Word { get; set; }
        public bool CaseSensitive { get; set; }
        public bool WholeWordOnly { get; set; }
        public bool TrimWord { get; set; }

    }
}
