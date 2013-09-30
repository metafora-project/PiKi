using UnityEngine;
using System.Collections;

[System.Serializable]
public class MaterialRule
{
    public float _min;
    public float _max;
    public float _factor;

    public float MinValue { get { return _min; } set { _min = value; } }
    public float MaxValue { get { return _max; } set { _max = value; } }
    public float FactorValue { get { return _factor; } set { _factor = Mathf.Clamp(value, 0.0f, 0.99f); } }

    public MaterialRule()
    {
        _min = 0.0f;
        _factor = 0.85f;
        _max = Mathf.Infinity;
    }

    public MaterialRule(float min, float factor, float max)
    {
        _min = min;
        _factor = factor;
        _max = max;
    }

    #region Support String Fields for GUI
    public string _minStr = "";
    public string _maxStr = "";
    public string _factorStr = "";

    public string MinString { get { return _minStr; } set { _minStr = value; } }
    public string MaxString { get { return _maxStr; } set { _maxStr = value; } }
    public string FactorString
    {
        get { return _factorStr; }
        set
        {
            //float val = float.Parse(value);
            _factorStr = value; // Mathf.Clamp(val, 0.0f, 0.99f).ToString(); 
        }
    }
    #endregion
}
