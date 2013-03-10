using System;

namespace Wia {
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class ConfigAttribute : Attribute {
        public ConfigAttribute(string section, string key) {
            Section = section;
            Key = key;
        }

        public string Section { get; set; }
        public string Key { get; set; }
        public string HelpText { get; set; }
    }
}