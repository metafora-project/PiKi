using System;
using System.Collections.Generic;

namespace XML
{
	public class XMLUtils
	{
        public static int ParseInt(XMLNode element)
        {
            if (element == null)
            {
                return 0;
            }

            return int.Parse(element.innerText);
        }

        public static float ParseFloat(XMLNode element)
        {
            if (element == null)
            {
                return float.NaN;
            }

            return float.Parse(element.innerText);
        }

        public static float[] ParseFloatArray(XMLNode element)
        {
            if (element == null)
            {
                return null;
            }
            char[] delimeters = { ',' };
            string[] parts = element.innerText.Split(delimeters, StringSplitOptions.RemoveEmptyEntries);

            float[] result = new float[parts.Length];
            for (int i = 0; i < result.Length; i++)
                result[i] = float.Parse(parts[i]);

            return result;
        }
    }
}
