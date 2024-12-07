namespace PackageTransferSystem.Extensions
{
    public static class TextWriterExtensions
    {
        public static void WriteLineWithUniqueColor(this TextWriter writer, string id, string message)
        {
            int colorIndex = GetColorIndexFromId(id);

            // Use ANSI 256-color mode
            string ansiColor = $"\u001b[38;5;{colorIndex}m";
            string reset = "\u001b[0m";

            writer.WriteLine($"{ansiColor}{message}{reset}");
        }

        // Derive a color index (0-255) from the hash of the ID.
        private static int GetColorIndexFromId(string id)
        {
            int hash = id.GetHashCode();
            return Math.Abs(hash) % 256;
        }
    }
}
