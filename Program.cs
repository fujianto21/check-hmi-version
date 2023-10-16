using System;
using System.IO;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;

class Program
{
    static void Main()
    {
        Console.SetWindowSize(34, 13);
        Console.SetBufferSize(34, 13);
        Console.Clear();

        Console.Title = "Check HMI Version";
        string jsonFile = File.ReadAllText("HMI_LIST.json");
        JObject jsonObj = JObject.Parse(jsonFile);
        JArray hmiList = (JArray)jsonObj["hmi"];

        Console.WriteLine("{0, 33}", "HMI Name & IP | Version");
        foreach (JObject sectionObj in hmiList)
        {
            JObject section = (JObject)sectionObj["section"];

            string sectionName = (string)section["name"];
            Console.WriteLine(sectionName + " :");

            JArray data = (JArray)section["data"];
            foreach (JObject hmiObj in data)
            {
                string hmiName = (string)hmiObj["hmi_name"];
                string hmiIp = (string)hmiObj["hmi_ip"];
                string iniFilePath = (string)hmiObj["ini_file_path"];
                string iniFileFullPath = "\\\\" + hmiIp + iniFilePath;

                Console.WriteLine("{0, 7} - {1, -13} : {2, 7}", hmiName, hmiIp, GetHMIVersion(iniFileFullPath));
            }
            Console.WriteLine();
        }
        Console.WriteLine("Press any key to exit.");
        Console.ReadKey();
    }

    static string GetHMIVersion(string filePath)
    {
        if (!File.Exists(filePath)) return "ERROR"; // The file path does not exist
        string section = "InTouch";
        string key = "ApplicationVersionNo";
        var iniData = new Dictionary<string, Dictionary<string, string>>();
        string currentSection = null;
        foreach (string line in File.ReadLines(filePath))
        {
            string trimmedLine = line.Trim();

            if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
            {
                currentSection = trimmedLine.Substring(1, trimmedLine.Length - 2);
                iniData[currentSection] = new Dictionary<string, string>();
            }
            else if (!string.IsNullOrEmpty(trimmedLine) && currentSection != null)
            {
                int equalsIndex = trimmedLine.IndexOf('=');

                if (equalsIndex >= 0)
                {
                    string iniKey = trimmedLine.Substring(0, equalsIndex).Trim();
                    string iniValue = trimmedLine.Substring(equalsIndex + 1).Trim();
                    iniData[currentSection][iniKey] = iniValue;
                }
            }
        }

        if (iniData.ContainsKey(section) && iniData[section].ContainsKey(key))
        {
            return iniData[section][key];
        }

        return "N/A";
    }

}