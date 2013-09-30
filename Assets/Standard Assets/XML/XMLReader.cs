using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XML
{
	public class XMLReader
	{
        private static char TAG_START = '<';
        private static char TAG_END = '>';
        private static char SPACE = ' ';
        private static char QUOTE = '"';
        private static char SLASH = '/';
        private static String BEGIN_QUOTE = "=\"";

        public XMLReader()
        {
        }

        public XMLNode read(String xml)
        {
            int index = 0;
            int lastIndex = 0;
            int prevLastIndex = 0;
            XMLNode rootNode = new XMLNode();
            XMLNode currentNode = rootNode;

            while (true)
            {
                index = xml.IndexOf(TAG_START, lastIndex);

                if (index < 0 || index >= xml.Length)
                {
                    break;
                }

                index++;

                prevLastIndex = lastIndex + 1;
                lastIndex = xml.IndexOf(TAG_END, index);
                if (lastIndex < 0 || lastIndex >= xml.Length)
                {
                    break;
                }

                int tagLength = lastIndex - index;
                String xmlTag = xml.Substring(index, tagLength);

                // if the tag starts with ? or ! go on
                if ('?' == xmlTag[0])
                    continue;
                if ('!' == xmlTag[0])
                {
                    if (1 == xmlTag.IndexOf("DOCTYPE"))
                        continue;
                    else if (1 == xmlTag.IndexOf("[CDATA["))
                    {
                        lastIndex = xml.IndexOf("]]>", index);
                        if (lastIndex < 0 || lastIndex >= xml.Length)
                            break;
                        else
                        {
                            index += 8;
                            currentNode.cdata += xml.Substring(index, lastIndex - index);

                            index = lastIndex + 4;
                            lastIndex = xml.IndexOf(TAG_END, index);

                            tagLength = lastIndex - index;
                            xmlTag = xml.Substring(index, tagLength);
                        }
                    }
                    else
                    {
                        lastIndex = xml.IndexOf("-->", index);
                        if (lastIndex < 0 || lastIndex >= xml.Length)
                            break;
                        else
                            continue;
                    }
                }

                // if the tag starts with a </ then it is an end tag
                //
                if (xmlTag[0] == SLASH)
                {
                    if (0 == currentNode.children.Count)
                        currentNode.innerText = xml.Substring(prevLastIndex, index - prevLastIndex - 1);
                    currentNode = currentNode.parentNode;
                    continue;
                }

                bool openTag = true;

                // if the tag ends in /> the tag can be considered closed
                if (xmlTag[tagLength - 1] == SLASH)// || xmlTag[tagLength - 1] == ']')
                {
                    // cut away the slash
                    xmlTag = xmlTag.Substring(0, tagLength - 1);
                    openTag = false;
                }


                XMLNode node = parseTag(xmlTag);
                node.parentNode = currentNode;
                currentNode.children.Add(node);

                if (openTag)
                {
                    currentNode = node;
                }
            }

            return rootNode;
        }

        public XMLNode parseTag(String xmlTag)
        {
            XMLNode node = new XMLNode();

            int nameEnd = xmlTag.IndexOf(SPACE, 0);
            if (nameEnd < 0)
            {
                node.tagName = xmlTag.Trim('\r', '\n', ' ');
                return node;
            }

            String tagName = xmlTag.Substring(0, nameEnd);
            node.tagName = tagName.Trim('\r', '\n', ' ');

            String attrString = xmlTag.Substring(nameEnd, xmlTag.Length - nameEnd);
            return parseAttributes(attrString, node);
        }

        public XMLNode parseAttributes(String xmlTag, XMLNode node)
        {
            int index = 0;
            int attrNameIndex = 0;
            int lastIndex = 0;

            while (true)
            {
                index = xmlTag.IndexOf(BEGIN_QUOTE, lastIndex);
                if (index < 0 || index > xmlTag.Length)
                {
                    break;
                }

                attrNameIndex = xmlTag.LastIndexOf(SPACE, index);
                if (attrNameIndex < 0 || attrNameIndex > xmlTag.Length)
                {
                    break;
                }


                attrNameIndex++;
                String attrName = xmlTag.Substring(attrNameIndex, index - attrNameIndex);
                // skip the equal and quote character
                //
                index += 2;

                lastIndex = xmlTag.IndexOf(QUOTE, index);
                if (lastIndex < 0 || lastIndex > xmlTag.Length)
                {
                    break;
                }

                int tagLength = lastIndex - index;
                String attrValue = xmlTag.Substring(index, tagLength);

                node.attributes.Add(attrName, attrValue);//[attrName] = attrValue;			
            }

            return node;
        }

        public void printXML(XMLNode node, int indent)
        {
            indent++;

            foreach (XMLNode n in node.children)
            {
                String attr = " ";
                foreach (KeyValuePair<String, String> p in n.attributes)
                {
                    attr += "[" + p.Key + ": " + p.Value + "] ";
                    //Debug.Log( attr );
                }

                String indentString = "";
                for (int i = 0; i < indent; i++)
                {
                    indentString += "-";
                }

                Debug.Log("" + indentString + " " + n.tagName + attr);
                printXML(n, indent);
            }
        }
    }
}
