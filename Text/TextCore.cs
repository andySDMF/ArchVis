using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Xml.Linq;

namespace Grain.Text
{
    public interface IText
    {
        void Append(string rawData);
    }

    [System.Serializable]
    public class TextSource
    {
        public string id;
        public List<TextTag> tags;

        public TextSource(string id, string tagData)
        {
            this.id = id;

            tags = new List<TextTag>();

            var doc = XDocument.Parse(tagData);

            foreach(XElement e in doc.Elements().Descendants())
            {
                TextTag tTag = new TextTag(e.Name.ToString(), e.Value);
                tags.Add(tTag);
            }
        }

        public string Get(string tagID)
        {
            TextTag tTag = tags.FirstOrDefault(t => t.id.Equals(tagID));

            return (tTag != null) ? tTag.data : "";
        }
    }

    [System.Serializable]
    public class TextTag
    {
        public string id;
        public string data;

        public TextTag(string id, string data)
        {
            this.id = id;
            this.data = data;
        }
    }
}
