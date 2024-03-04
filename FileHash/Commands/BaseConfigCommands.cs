using CommandLine;

namespace FileHash
{
    internal class BaseConfigCommands
    {
        [Option('i', "input", Required = true, HelpText = "Set input filename for processing.")]
        public string FileName { get; set; }

        [Option('o', "offset", Required = true, HelpText = "Set offset bytes at one batch for processing.")]
        public int Offset { get; set; }

    }
}
