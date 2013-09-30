using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PikiObject
{
    private string _id;
    private string _type;
    private Dictionary<string, string> _properties;

    #region Constructors
    public PikiObject(string id, string type)
    {
        this._id = id;
        this._type = type;
        _properties = new Dictionary<string, string>();
    }
    #endregion

    #region Getter/Setter
    public string Id
    {
        get { return _id; }
        set { _id = value; }
    }
    public string Type
    {
        get { return _type; }
        set { _type = value; }
    }

    public Dictionary<string, string> Properties
    {
        get { return _properties; }
        set { _properties = value; }
    }
    #endregion

    #region Public Functions
    public void SetProperty(string name, string value)
    {
        if (_properties.ContainsKey(name))
        {
            _properties[name] = value;
        }
        else
        {
            _properties.Add(name, value);
        }
    }

    public void SetPropertiesFromState()
    {
        SetProperty("CANNON_DELTA_POWER", (DataContainer.Instance.CannonPower - MyGame.Instance.PrevCannonPower).ToString());
        SetProperty("CANNON_DELTA_ANGLE", (DataContainer.Instance.CannonAngle - MyGame.Instance.PrevCannonAngle).ToString());
        SetProperty("CAMERA_COUNTER", MyGame.Instance.CameraCounter.ToString());
        SetProperty("SAVE_COUNTER", MyGame.Instance.SaveCounter.ToString());

        foreach (KeyValuePair<string, ArrayList> item in MaterialManager.Instance.HorizontalRules)
        {
            int counter = 0;
            foreach(MaterialRule rule in item.Value)
            {
                SetProperty(item.Key.ToUpper() + "_" + counter.ToString() + "_HORIZONTAL", "min:" + rule.MinValue.ToString() + ",max:" + rule.MaxValue.ToString() + ",factor:" + rule.FactorValue);
                counter++;
            }
        }

        foreach (KeyValuePair<string, ArrayList> item in MaterialManager.Instance.VerticalRules)
        {
            int counter = 0;
            foreach (MaterialRule rule in item.Value)
            {
                SetProperty(item.Key.ToUpper() + "_" + counter.ToString() + "_VERTICAL", "min:" + rule.MinValue.ToString() + ",max:" + rule.MaxValue.ToString() + ",factor:" + rule.FactorValue);
                counter++;
            }
        }
    }

    public void SetLandmarkPropertiesFromState()
    {
        SetProperty("CANNON_POWER", DataContainer.Instance.CannonPower.ToString());
        SetProperty("CANNON_ANGLE", DataContainer.Instance.CannonAngle.ToString());
        SetProperty("SCENE_CREATOR_GROUP", DataContainer.Instance.Creator);

        foreach (KeyValuePair<string, ArrayList> item in MaterialManager.Instance.HorizontalRules)
        {
            int counter = 0;
            foreach (MaterialRule rule in item.Value)
            {
                SetProperty(item.Key.ToUpper() + "_" + counter.ToString() + "_HORIZONTAL", "min:" + rule.MinValue.ToString() + ",max:" + rule.MaxValue.ToString() + ",factor:" + rule.FactorValue);
                counter++;
            }
        }

        foreach (KeyValuePair<string, ArrayList> item in MaterialManager.Instance.VerticalRules)
        {
            int counter = 0;
            foreach (MaterialRule rule in item.Value)
            {
                SetProperty(item.Key.ToUpper() + "_" + counter.ToString() + "_VERTICAL", "min:" + rule.MinValue.ToString() + ",max:" + rule.MaxValue.ToString() + ",factor:" + rule.FactorValue);
                counter++;
            }
        }
    }

    public void ClearProperties()
    {
        _properties.Clear();
    }
    #endregion
}
