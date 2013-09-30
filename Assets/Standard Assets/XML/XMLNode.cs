using System;
using System.Collections;
using System.Collections.Generic;

namespace XML
{
	public class XMLNode
	{
        public String tagName;
        public XMLNode parentNode;
        public ArrayList children;
        public Dictionary<String, String> attributes;
        public String innerText;
        public String cdata;

        public XMLNode this[string childName]
        {
            get
            {
                foreach (XMLNode node in children)
                {
                    if (node.tagName == childName)
                        return node;
                }

                return null;
            }
        }

        public bool HasAttribute(string name)
        {
            return attributes.ContainsKey(name);
        }

        public string GetAttributeAsString(string name, string defaultValue)
        {
            string ret;
            if (attributes.TryGetValue(name, out ret))
                return ret;
            else
                return defaultValue;
        }

        public string GetAttributeAsString(string name)
        {
            return this.GetAttributeAsString(name, null);
        }

        public float GetAttributeAsFloat(string name, float defaultValue)
        {
            string ret;
            if (attributes.TryGetValue(name, out ret))
                return float.Parse(ret);
            else
                return defaultValue;
        }

        public float GetAttributeAsFloat(string name)
        {
            return this.GetAttributeAsFloat(name, float.NaN);
        }

        public int GetAttributeAsInt(string name, int defaultValue)
        {
            string ret;
            if (attributes.TryGetValue(name, out ret))
                return int.Parse(ret);
            else
                return defaultValue;
        }

        public int GetAttributeAsInt(string name)
        {
            return this.GetAttributeAsInt(name, 0);
        }

        public XMLNode()
        {
            tagName = "NONE";
            parentNode = null;
            children = new ArrayList();
            attributes = new Dictionary<String, String>();
            innerText = "";
            cdata = "";
        }
    }
}
