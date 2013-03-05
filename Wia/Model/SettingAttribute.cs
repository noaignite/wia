using System;

namespace Wia.Model {
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class SettingAttribute : Attribute {
        public SettingAttribute(string section, string key) {
            Section = section;
            Key = key;
        }

        public string Section { get; set; }
        public string Key { get; set; }
        public string HelpText { get; set; }
    }
}