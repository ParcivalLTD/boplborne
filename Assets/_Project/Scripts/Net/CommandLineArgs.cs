using System;

namespace GraceRun.Net
{
    /// <summary>
    /// Launch options read from the command line: -server, -port &lt;n&gt;, -connect &lt;ip[:port]&gt;, -statusfile &lt;path&gt; (server only: a status file for monitoring).
    /// </summary>
    public readonly struct LaunchArgs
    {
        public LaunchArgs(bool startServer, ushort? port, string connectAddress, string statusFile = null)
        {
            StatusFile = statusFile;
            StartServer = startServer;
            Port = port;
            ConnectAddress = connectAddress;
        }

        public bool StartServer { get; }
        public ushort? Port { get; }
        public string ConnectAddress { get; }
        public string StatusFile { get; }
        public bool HasConnectAddress => !string.IsNullOrEmpty(ConnectAddress);
    }

    /// <summary>Parses the launch options in <see cref="LaunchArgs"/>. Pure logic, covered by EditMode tests.</summary>
    public static class CommandLineArgs
    {
        private const string ServerFlag = "-server";
        private const string PortFlag = "-port";
        private const string ConnectFlag = "-connect";
        private const string StatusFileFlag = "-statusfile";

        public static LaunchArgs Parse(string[] args)
        {
            var startServer = false;
            ushort? port = null;
            string connectAddress = null;
            string statusFile = null;
            if (args == null) return new LaunchArgs(false, null, null);

            for (var i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case ServerFlag:
                        startServer = true;
                        break;
                    case PortFlag when i + 1 < args.Length:
                        if (ushort.TryParse(args[i + 1], out var parsedPort) && parsedPort > 0) port = parsedPort;
                        i++;
                        break;
                    case ConnectFlag when i + 1 < args.Length:
                        connectAddress = args[i + 1];
                        i++;
                        break;
                    case StatusFileFlag when i + 1 < args.Length:
                        statusFile = args[i + 1];
                        i++;
                        break;
                }
            }

            return new LaunchArgs(startServer, port, connectAddress, statusFile);
        }

        /// <summary>Launch options of the running process.</summary>
        public static LaunchArgs Current() => Parse(Environment.GetCommandLineArgs());
    }
}
