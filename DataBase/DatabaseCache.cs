using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "Database/Cache", order = 1)]
public class DatabaseCache : ScriptableObject
{
    public List<string> units = new List<string>();
    public string rawPropertiesData = "";
    public string rawCategoriesData = "";

    public void SetUnits(string rawData)
    {
        units = GetRawUnits(rawData);
        rawPropertiesData = rawData;
    }

    public void Clear()
    {
        units.Clear();
        rawPropertiesData = "";
    }

    private List<string> GetRawUnits(string str)
    {
        List<string> rawUnits = new List<string>();

        if (!string.IsNullOrEmpty(str))
        {
            char[] separators = new char[] { '[', ']' };

            string[] temp = str.Split(separators, System.StringSplitOptions.RemoveEmptyEntries);
            string tempStr = string.Join("\n", temp);

            string[] units = tempStr.Split('}');

            foreach (string s in units)
            {
                if (string.IsNullOrEmpty(s)) continue;

                string preUnit = "";
                string rawUnit = "";

                if (s[0].Equals(','))
                {
                    preUnit = s.Substring(1, s.Length - 1);
                }
                else
                {
                    preUnit = s;
                }

                rawUnit = preUnit.Replace("{", "");

                rawUnits.Add(rawUnit);
            }
        }

        return rawUnits;
    }
}
