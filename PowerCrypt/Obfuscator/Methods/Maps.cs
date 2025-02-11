namespace PowerCrypt.Obfuscator.Methods
{
    public class ReplacementMapUniversal
    {
        public required int StartOffset { get; set; }
        public required int Length { get; set; }
        public required string OriginalName { get; set; }
        public required string Text { get; set; }
        public required string Type { get; set; }
        public required bool RequiresKeyword { get; set; }
    }
}
