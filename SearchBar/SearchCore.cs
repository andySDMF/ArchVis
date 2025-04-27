using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Grain.SearchBar
{
    public interface ISearch
    {
        void Publish(string rawData);

        bool Display(string criteria);

        void Show();

        void Hide();

        RecordData Data { get; }

        string ID { get; }

        void UpdateDataValue(string k, string v);

        MonoBehaviour Component { get; }
    }

    [System.Serializable]
    public class AllRecords
    {
        public List<RecordData> records;
    }

    [System.Serializable]
    public class RecordData
    {
        public List<RecordSingleData> values;

        public string GetValue(string k)
        {
            return values.FirstOrDefault(s => s.key.Equals(k)).value;
        }
    }

    [System.Serializable]
    public class RecordSingleData
    {
        public string key;
        public string value;

        public RecordSingleData(string k, string v)
        {
            key = k;
            value = v;
        }
    }

    [System.Serializable]
    public class RecordUIElements
    {
        public string key;
        public MaskableGraphic asset;
    }

    [System.Serializable]
    public enum ReadFileType { _CSV, _TXT }
}
