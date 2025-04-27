using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using Grain.CMS;

public class GrainCMSUnitCache : MonoBehaviour, IGrainCache
{
    [SerializeField]
    private bool debug = false;

    [SerializeField]
    private List<UnitCondition> conditions = new List<UnitCondition>();

    [SerializeField]
    private List<Unit> units = new List<Unit>();

    [SerializeField]
    private DatabaseCache cache;

    public List<Unit> Units { get { return units; } }

    public static bool Debugging { get; private set; }

    public List<UnitCondition> Conditions { get { return conditions; } }

    public DatabaseCache CacheObject { get { return cache; } }

    [HideInInspector]
    [SerializeField]
    private string cachedRawData = "";
    private bool init = false;

    private void Awake()
    {
        Debugging = debug;

        GrainCMSUtils.UnitCacheHandler = this;

        cachedRawData = PlayerPrefs.GetString("UNITS");
    }

    public void RecieveCMSData(string rawData)
    {
        bool cacheUsed = false;
        bool noChange = false;

        if (Application.isPlaying)
        {
            if (!string.IsNullOrEmpty(rawData))
            {
                if (PlayerPrefs.GetString("UNITS").Equals(rawData))
                {
                    noChange = true;
                    cachedRawData = PlayerPrefs.GetString("UNITS");
                }
                else
                {
                    cache.SetUnits(rawData);
                    cachedRawData = rawData;
                    PlayerPrefs.SetString("UNITS", cachedRawData);
                }
            }
            else
            {
                cachedRawData = PlayerPrefs.GetString("UNITS");

                if(string.IsNullOrEmpty(cachedRawData))
                {
                    cacheUsed = true;
                }
            }
        }
        else
        {
            cache.SetUnits(rawData);
            cachedRawData = rawData;
        }

        if(!noChange)
        {
            units.Clear();
            List<string> rawUnits;

            if (cacheUsed)
            {
                rawUnits = cache.units;
            }
            else
            {
                rawUnits = GetRawUnits(cachedRawData);
            }

            foreach (string rawUnit in rawUnits)
            {
                Unit unit = new Unit(rawUnit, conditions);
                units.Add(unit);
            }
        }

        if (Application.isPlaying)
        {
            if (init)
            {
                GrainCMSUtils.UnitHandler.Apply();
            }

            init = true;
        }
    }

    public void Clear()
    {
        cachedRawData = "";
        PlayerPrefs.SetString("UNITS", cachedRawData);

        if (cache != null)
        {
            cache.Clear();
        }
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

                if (debug) Debug.Log("Unit raw data [" + rawUnit + "]");

                rawUnits.Add(rawUnit);
            }
        }

        if (debug) Debug.Log("Total raw units [" + rawUnits.Count.ToString() + "]");

        return rawUnits;
    }
}

[System.Serializable]
public class Unit
{
    [SerializeField]
    private string id = "";

    [SerializeField]
    private List<UnitElement> elements = new List<UnitElement>();

    public string ID { get { return id; } }

    public Unit(string rawData, List<UnitCondition> conditions)
    {
        Update(rawData, conditions);
    }

    public void Update(string rawData, List<UnitCondition> conditions)
    {
        if (string.IsNullOrEmpty(rawData))
        {
            if (GrainCMSUnitCache.Debugging) Debug.Log("Raw data is Null!");
            return;
        }

        char[] separators = new char[] { '"' };

        string[] temp = rawData.Split(separators, System.StringSplitOptions.RemoveEmptyEntries);
        string tempStr = string.Join("\n", temp);

        string[] attributes = tempStr.Split(',');

        if (attributes.Length <= 0)
        {
            if (GrainCMSUnitCache.Debugging) Debug.Log("There are no attributes!");
            return;
        }

        for (int i = 0; i < attributes.Length; i++)
        {
            string[] attributeData = attributes[i].Split(':');

            if (i.Equals(0))
            {
                id = attributeData[1].Replace("\n", string.Empty).Replace("\t", string.Empty).Replace("\r", string.Empty);
                continue;
            }

            attributeData[0] = attributeData[0].Replace("\n", string.Empty).Replace("\t", string.Empty).Replace("\r", string.Empty);
            attributeData[1] = attributeData[1].Replace("\n", string.Empty).Replace("\t", string.Empty).Replace("\r", string.Empty);

            UnitElement element = GetElement(attributeData[0]);

            if(element != null)
            {
                element.Update(attributeData[1].ToString(), conditions);
            }
            else
            {
                element = new UnitElement(attributeData[0], attributeData[1].ToString(), conditions);
                elements.Add(element);
            }
        }

        if (GrainCMSUnitCache.Debugging) Debug.Log("Unit " + id.ToString() + ", " + elements.Count.ToString() + " elements added");
    }

    public UnitElement GetElement(string key)
    {
        return elements.FirstOrDefault(e => e.Key.Equals(key));
    }

    public List<string> GetElementValues(string key)
    {
        return elements.FirstOrDefault(e => e.Key.Equals(key)).Values;
    }

    public string GetElementValueByIndex(string key, int index = 0)
    {
        UnitElement element = GetElement(key);

        if (element == null) return "";

        if (index < 0 || index > element.Values.Count - 1) return "";

        return elements.FirstOrDefault(e => e.Key.Equals(key)).Values[index];
    }

    public List<string> GetElementValueByArray(string key, int[] indexes)
    {
        UnitElement element = GetElement(key);

        List<string> values = new List<string>();

        if (element == null) return values;

        for (int i = 0; i < element.Values.Count; i++)
        {
            if (indexes.Contains(i)) values.Add(element.Values[i]);
        }

        return values;
    }
}

[System.Serializable]
public class UnitElement
{
    [SerializeField]
    private string key = "";

    [SerializeField]
    private List<string> values = new List<string>();

    public string Key { get { return key; } }

    public List<string> Values {  get { return values; } }

    public UnitElement(string k, string v, List<UnitCondition> conditions = null)
    {
        key = k;

        Update(v, conditions);
    }

    public void Update(string v, List<UnitCondition> conditions)
    {
        string[] attributes = v.Split(',');

        if (attributes.Length <= 0)
        {
            if (GrainCMSUnitCache.Debugging) Debug.Log("There are no attributes for KEY [" + key + "]");
            return;
        }

        for (int i = 0; i < attributes.Length; i++)
        {
            if (string.IsNullOrEmpty(attributes[i])) continue;

            UnitCondition condition = conditions.FirstOrDefault(c => c.IsConditon(key));

            if (condition != null)
            {
                attributes[i] = condition.PerformCondition(key, attributes[i]);
            }

            if(!values.Contains(attributes[i])) values.Add(attributes[i]);
        }
    }
}

[System.Serializable]
public class UnitCondition
{
    [SerializeField]
    private string id = "";

    [SerializeField]
    private bool isCurrency = false;

    [SerializeField]
    private Currency currency = new Currency();

    [SerializeField]
    private List<Add> addChars = new List<Add>();

    [SerializeField]
    private List<Replace> replaceChars = new List<Replace>();

    [SerializeField]
    private List<Remove> removeChars = new List<Remove>();

    public string ID {  get { return id; } }

    public bool IsConditon(string id)
    {
        if (string.IsNullOrEmpty(id)) return false;

        return this.id.Equals(id);
    }

    public string PerformCondition(string id, string val)
    {
        if (string.IsNullOrEmpty(id) || !this.id.Equals(id)) return val;

        if(isCurrency)
        {
            val = currency.Amend(val);
        }
        else
        {
            addChars.ForEach(c => val = c.Amend(val));
            removeChars.ForEach(c => val = c.Amend(val));
            replaceChars.ForEach(c => val = c.Amend(val));
        }

        return val;
    }

    [System.Serializable]
    public class Add : CharRules
    {
        public enum AddTo { _Start, _End, _Value }

        public AddTo addTo = AddTo._Value;
        public int charValue = 0;
        public string addString = "";

        public string Amend(string s)
        {
            switch(addTo)
            {
                case AddTo._Start:
                    s = s.Insert(0, addString);
                    break;

                case AddTo._End:
                    s = s.Insert(s.Length, addString);
                    break;

                default:
                    s = s.Insert(charValue, addString);
                    break;
            }

            return s;
        }
    }

    [System.Serializable]
    public class Currency : CharRules
    {
        public string symbol = "£";
        public char seperator;
        public string undiclosedFee = "Undisclosed";

        public string Amend(string s)
        {
            if (s.Length <= 1 && s[0].Equals('0')) s = undiclosedFee;
            else
            {
                string temp = s;

                if (temp.Length > 6) s = temp.Insert(temp.Length - 6, seperator.ToString());

                temp = s;

                if (temp.Length > 3) s = temp.Insert(temp.Length - 3, seperator.ToString());

                s = s.Insert(0, symbol);
            }

            return s;
        }
    }

    [System.Serializable]
    public class Replace : CharRules
    {
        public int charValue = 0;
        public string find;
        public string replace;

        public string Amend(string s)
        {
            s = s.Replace(find, replace);

            return s;
        }
    }

    [System.Serializable]
    public class Remove : CharRules
    {
        public int charValue = 0;
        public int charCount = 0;

        public string Amend(string s)
        {
            s = s.Remove(charValue, charCount);

            return s;
        }
    }

    public interface CharRules
    {
        string Amend(string s);
    }
}
