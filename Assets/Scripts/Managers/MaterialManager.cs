using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MaterialManager {

    public const string BLOCK = "Block";
    public const string TRAMPOLINE = "Trampoline";

    #region Singleton
    public static MaterialManager _instance;

    public static MaterialManager Instance
    {
        get
        {
            return _instance;
        }
    }
    #endregion    
    
    public float defaultVertical = 0.5f;
    public float defaultHorizontal = 0.5f;
    public Dictionary<string, ArrayList> verticalRules;
    public Dictionary<string, ArrayList> horizontalRules;
    public string currentMaterial;

    #region Get/Set Modifiers
    public Dictionary<string, ArrayList> VerticalRules
    {
        get
        {
            return verticalRules;
        }

        set
        {
            verticalRules = value;
        }
    }

    public Dictionary<string, ArrayList> HorizontalRules
    {
        get
        {
            return horizontalRules;
        }

        set
        {
            horizontalRules = value;
        }
    }

    public string CurrentMaterial
    {
        get
        {
            return currentMaterial;
        }

        set
        {
            currentMaterial = value;
        }
    }
    #endregion

    public MaterialManager()
    {
        _instance = this;

        //BLOCK
        ArrayList verticalBlockDefault = new ArrayList();
        verticalBlockDefault.Add(new MaterialRule(0.0f, 0.4f, Mathf.Infinity));
        ArrayList horizontalBlockDefault = new ArrayList();
        horizontalBlockDefault.Add(new MaterialRule(0.0f, 0.4f, Mathf.Infinity));

        //TRAMPOLINE
        ArrayList verticalTrampolineDefault = new ArrayList();
        verticalTrampolineDefault.Add(new MaterialRule());
        ArrayList horizontalTrampolineDefault = new ArrayList();
        horizontalTrampolineDefault.Add(new MaterialRule());

        verticalRules = new Dictionary<string, ArrayList>();
        verticalRules.Add(BLOCK, verticalBlockDefault);
        verticalRules.Add(TRAMPOLINE, verticalTrampolineDefault);

        horizontalRules = new Dictionary<string, ArrayList>();
        horizontalRules.Add(BLOCK, horizontalBlockDefault);
        horizontalRules.Add(TRAMPOLINE, horizontalTrampolineDefault);

        currentMaterial = BLOCK;
    }

    public void AddVerticalRule(string tagName, MaterialRule rule)
    {
        verticalRules[tagName].Add(rule);
    }

    public void AddHorizontalRule(string tagName, MaterialRule rule)
    {
        horizontalRules[tagName].Add(rule);
    }

    public void RemoveVerticalRule(string tagName, MaterialRule rule)
    {
        verticalRules[tagName].Remove(rule);
    }

    public void RemoveHorizontalRule(string tagName, MaterialRule rule)
    {
        horizontalRules[tagName].Remove(rule);
    }

    public float GetVFactor(string tagName, float vIn)
    {
        if (verticalRules.ContainsKey(tagName))
        {
            ArrayList ruleList = verticalRules[tagName];
            for (int i = ruleList.Count - 1; i >= 0; i--)
            {
                MaterialRule rule = ruleList[i] as MaterialRule;
                if (vIn >= rule.MinValue && vIn < rule.MaxValue)
                    return rule.FactorValue;
            }
        }
        return defaultVertical;
    }

    public float GetHFactor(string tagName, float vIn)
    {
        if (verticalRules.ContainsKey(tagName))
        {
            ArrayList ruleList = horizontalRules[tagName];
            for (int i = ruleList.Count - 1; i >= 0; i--)
            {
                MaterialRule rule = ruleList[i] as MaterialRule;
                if (vIn >= rule.MinValue && vIn < rule.MaxValue)
                    return rule.FactorValue;
            }
        }
        return defaultHorizontal;
    }
}
