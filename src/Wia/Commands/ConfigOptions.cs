using CommandLine;

namespace Wia.Commands {
    public class ConfigOptions {
        [Option('r', "reset", HelpText = "Resets the value to null")]
        public bool Reset { get; set; }

        [ValueOption(0)]
        public string ConfigKey { get; set; }
        
        [ValueOption(1)]
        public string ConfigValue { get; set; }
    }
}