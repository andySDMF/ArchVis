using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Grain.CMS;

public class GrainCMSUnitPropertiesCache : MonoBehaviour, IGrainCache
{
    [SerializeField]
    private bool debug = false;

    [SerializeField]
    private bool derivedFromUnits = false;

    [SerializeField]
    private string defaultAllValue = "All";

    [SerializeField]
    private List<PropertyField> propertyFields = new List<PropertyField>();

    [SerializeField]
    private DatabaseCache cache;

    public List<PropertyField> PropertyFields { get { return propertyFields; } }

    private string cachedRawData = "";

    private void Awake()
    {
        GrainCMSUtils.UnitPropertiesCacheHandler = this;
    }

    public void RecieveCMSData(string rawData)
    {
        bool update = true;

        if (Application.isPlaying)
        {
            if (!string.IsNullOrEmpty(rawData))
            {
                if (PlayerPrefs.GetString("UNIT-PROPERTIES").Equals(rawData))
                {
                    update = (derivedFromUnits) ? true : false;
                    cachedRawData = PlayerPrefs.GetString("UNIT-PROPERTIES");
                }
                else
                {
                    cache.rawPropertiesData = rawData;

                    cachedRawData = rawData;
                    PlayerPrefs.SetString("UNIT-PROPERTIES", cachedRawData);
                }
            }
            else
            {
                cachedRawData = PlayerPrefs.GetString("UNIT-PROPERTIES");

                if (string.IsNullOrEmpty(cachedRawData))
                {
                    PlayerPrefs.SetString("UNIT-PROPERTIES", cache.rawPropertiesData);
                    cachedRawData = cache.rawPropertiesData;
                }
            }
        }
        else
        {
            cache.rawPropertiesData = rawData;
            cachedRawData = rawData;
        }

        if (update)
        {
            propertyFields.Clear();
            propertyFields = GetProperties(cachedRawData);
        }
    }

    private List<PropertyField> GetProperties(string str)
    {
        List<PropertyField> properties = new List<PropertyField>();

        if (!string.IsNullOrEmpty(str))
        {
            char[] separators = new char[] { '"', '{', '[', ']' };

            string[] temp = str.Split(separators, System.StringSplitOptions.RemoveEmptyEntries);
            str = string.Join("\n", temp);
            str =  str.Remove(str.Length - 1, 1);

            temp = str.Split('}');
            string key = "";
            bool isNewProperty = false;

            Dictionary<string, List<string>> fields = new Dictionary<string, List<string>>();

            foreach (string rawStr in temp)
            {
                string rawPropertry = rawStr.Replace("\n", string.Empty).Replace("\t", string.Empty).Replace("\r", string.Empty);

                if (string.IsNullOrEmpty(rawPropertry)) continue;

                string[] tempFields = rawPropertry.Split(',');

                foreach(string rawValue in tempFields)
                {
                    string[] tempVals = rawValue.Split(':');

                    for(int i = 0; i < tempVals.Length; i++)
                    {
                        if (tempVals[i].Equals("name"))
                        {
                            if(!tempVals[i + 1].Equals(key))
                            {
                                isNewProperty = true;
                                key = tempVals[i + 1];
                            }
                        }

                        if (isNewProperty)
                        {
                            fields.Add(key, new List<string>());
                            isNewProperty = false;
                        }

                        if (!derivedFromUnits)
                        {
                            if (tempVals[i].Equals("value"))
                            {
                                fields[key].Add(tempVals[i + 1]);
                            }
                        }
                    }
                }
            }

            foreach(KeyValuePair<string, List<string>> field in fields)
            {
                if(derivedFromUnits)
                {
                    if (field.Value.Count <= 0)
                    {
                        List<string> values = GrainCMSUtils.GetUnitPropertyValuesFromUnits(field.Key);
                        field.Value.AddRange(values);
                    }
                }

                field.Value.Insert(0, defaultAllValue);

                PropertyField newPropertyField = new PropertyField(field.Key, field.Value);
                properties.Add(newPropertyField);
            }
        }

        return properties;
    }
}

[System.Serializable]
public class PropertyField
{
    [SerializeField]
    private string key = "";

    [SerializeField]
    private List<string> values = new List<string>();

    public string Key { get { return key; } }

    public List<string> Values { get { return values; } }

    public PropertyField(string k, List<string> v)
    {
        key = k;

        values.Clear();
        values.AddRange(v);
    }
}
