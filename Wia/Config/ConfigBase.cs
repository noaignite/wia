using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Wia.Model;
using Wia.Utility;

namespace Wia {
    public abstract class ConfigBase {
        private readonly string _configPath;

        protected ConfigBase() {
            var appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _configPath = Path.Combine(appPath, ".wiaconfig");
            Load(); 
        }
        
        public void Load() {
            var iniFileManager = new IniFileManager(_configPath);
            var properties = GetType().GetProperties().Where(prop => prop.IsDefined(typeof(SettingAttribute), false));

            foreach (var prop in properties) {
                var settingAttribute = (SettingAttribute)prop.GetCustomAttributes(typeof(SettingAttribute), false).FirstOrDefault();
                var valueFromConfig = iniFileManager.GetValue(settingAttribute.Section, settingAttribute.Key);

                prop.SetValue(this, valueFromConfig, null);
            }
            
        }

        public void Save() {
            var iniFileManager = new IniFileManager(_configPath);
            var properties = GetType().GetProperties().Where(prop => prop.IsDefined(typeof(SettingAttribute), false));
            
            foreach (var prop in properties) {
                var settingAttribute = (SettingAttribute)prop.GetCustomAttributes(typeof (SettingAttribute), false).FirstOrDefault();
                var valueFromMemory = prop.GetValue(this, null) as string;

                iniFileManager.SetValue(settingAttribute.Section, settingAttribute.Key, valueFromMemory);
            }
            
            iniFileManager.Save(_configPath);
        }

        public string GetValue(string section, string key) {
            var properties = GetType().GetProperties().Where(prop => prop.IsDefined(typeof(SettingAttribute), false));

            foreach (var prop in properties) {
                var settingAttribute = (SettingAttribute)prop.GetCustomAttributes(typeof(SettingAttribute), false).FirstOrDefault();

                if (settingAttribute.Section.Equals(section, StringComparison.OrdinalIgnoreCase) &&
                    settingAttribute.Key.Equals(key, StringComparison.OrdinalIgnoreCase)) {
                    return prop.GetValue(this, null) as string;
                }
            }
         
            throw new KeyNotFoundException("Key could not be found in configuration.");
        }

        public IEnumerable<ConfigProperty> GetValues() {
            var properties = GetType().GetProperties().Where(prop => prop.IsDefined(typeof(SettingAttribute), false));

            foreach (var prop in properties) {
                var settingAttribute = (SettingAttribute)prop.GetCustomAttributes(typeof(SettingAttribute), false).FirstOrDefault();
                var key = settingAttribute.Section + "." + settingAttribute.Key;
                var value = prop.GetValue(this, null) as string;

                yield return new ConfigProperty(key, value, settingAttribute.HelpText);
            }
        }

        public void SaveValue(string section, string key, string value) {
            var properties = GetType().GetProperties().Where(prop => prop.IsDefined(typeof(SettingAttribute), false));

            foreach (var prop in properties) {
                var settingAttribute = (SettingAttribute)prop.GetCustomAttributes(typeof(SettingAttribute), false).FirstOrDefault();
                
                if (settingAttribute.Section.Equals(section, StringComparison.OrdinalIgnoreCase) &&
                    settingAttribute.Key.Equals(key, StringComparison.OrdinalIgnoreCase)) {
                    prop.SetValue(this, value, null);
                }
            }
            Save();
        }
    }
}
