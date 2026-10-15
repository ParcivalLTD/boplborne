namespace GraceRun.Net
{
    /// <summary>
    /// Reads what the player types in the Join field: "25.1.2.3" or "25.1.2.3:7778".
    /// </summary>
    public static class AddressParser
    {
        public static bool TryParse(string input, ushort defaultPort, out string address, out ushort port)
        {
            address = null;
            port = defaultPort;
            if (string.IsNullOrWhiteSpace(input)) return false;

            var text = input.Trim();
            var separator = text.LastIndexOf(':');
            if (separator >= 0)
            {
                var portText = text.Substring(separator + 1);
                if (!ushort.TryParse(portText, out var parsedPort) || parsedPort == 0) return false;
                port = parsedPort;
                text = text.Substring(0, separator).Trim();
            }

            if (text.Length == 0) return false;
            address = text;
            return true;
        }
    }
}
