using CommandLine;

namespace FileHash
{
    internal class BaseConfigCommands
    {
        [Option('i', "input", Required = true, HelpText = "Set input filename for processing.")]
        public string FileName { get; set; }

        [Option('b', "batch", Required = true, HelpText = "Set bytes count in one batch for processing.")]
        public int BatchSize { get; set; }

        [Option('t', "task_limit", Required = false, HelpText = "Set max count task executing for processing. Default = Processors Count.")]
        public int? TaskLimit { get; set; }

        [Option('c', "channel_capacity", Required = false, HelpText = "Set channel capacity.")]
        public int? ChannelCapacity { get; set; }

    }
}
