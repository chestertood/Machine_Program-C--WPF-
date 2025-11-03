using System;
using System.Collections.Generic;
using System.IO;

namespace service
{
    public class FileConfig
    {
        public Dictionary<string, Dictionary<string, string>> Data { get; private set; }

        public FileConfig(string path)
        {
            Data = new Dictionary<string, Dictionary<string, string>>();
            ParseFile(path);
        }

        private void ParseFile(string path)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine($"❌ File not found: {path}");
                return;
            }

            string currentSection = null;

            foreach (var rawLine in File.ReadLines(path))
            {
                string line = rawLine.Trim();

                if (string.IsNullOrWhiteSpace(line)) continue; // ข้ามบรรทัดว่าง
                if (line.StartsWith(";") || line.StartsWith("#")) continue; // ข้าม comment

                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    currentSection = line.Trim('[', ']');
                    if (!Data.ContainsKey(currentSection))
                    {
                        Data[currentSection] = new Dictionary<string, string>();
                    }
                }
                else if (currentSection != null && line.Contains("="))
                {
                    string[] parts = line.Split('=', 2);
                    string key = parts[0].Trim();
                    string value = parts[1].Trim();
                    Data[currentSection][key] = value;
                }
            }
        }

        public string Get(string section, string key, string defaultValue = "")
        {
            if (Data.ContainsKey(section) && Data[section].ContainsKey(key))
                return Data[section][key];
            return defaultValue;
        }

        public void Set(string section, string key, string value)
        {
            if (!Data.ContainsKey(section))
                Data[section] = new Dictionary<string, string>();

            Data[section][key] = value;
        }

        public void Save(string path)
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                foreach (var section in Data)
                {
                    writer.WriteLine($"[{section.Key}]");
                    foreach (var kvp in section.Value)
                    {
                        writer.WriteLine($"{kvp.Key} = {kvp.Value}");
                    }
                    writer.WriteLine(); // ช่องว่างระหว่าง section
                }
            }
        }

    }
}
