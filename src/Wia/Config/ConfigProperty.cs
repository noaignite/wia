namespace Wia {
    public class ConfigProperty {
        public ConfigProperty() {}

        public ConfigProperty(string key, string value, string helpText) {
            Key = key;
            Value = value;
            HelpText = helpText;
        }

        public string Key { get; set; }
        public string Value { get; set; }
        public string HelpText { get; set; }
    }
}