using UnityEngine;
using System.Collections;

public class BBComponent : MonoBehaviour
{
    #region Protected Fields
    protected Vector3 lastDirection = Vector3.right;
    protected Color Red = new Color(1.0f, 0.0f, 0.0f, 0.3f);
    protected Color Green = new Color(0.0f, 1.0f, 0.0f, 0.3f);
    protected Color AlphaZero = new Color(0.0f, 0.0f, 0.0f, 0.0f);

    protected bool isValidPosition = false;
    protected Vector3 prevPosition = Vector3.zero;
    #endregion

    #region Public Fields
    public float bbWidth = 1.0f;
    #endregion

    #region Messages
    public void SetWidth(float width)
    {
        bbWidth = width;
        Vector3 scale = transform.localScale;
        scale.x = width;
        transform.localScale = scale;
    }

    public void ShowGreen()
    {
        renderer.material.color = Green;
    }

    public void ShowRed()
    {
        renderer.material.color = Red;
    }

    public void Hide()
    {
        renderer.material.color = AlphaZero;
    }

    public bool CheckValidity()
    {
        return isValidPosition;
    }
    #endregion

    #region Unity Callback
    void Start()
    {
        Hide();
    }

    void Update()
    {
        if ((transform.position - prevPosition).magnitude > 0)
        {
            isValidPosition = !ObjectManager.Instance.CheckBounds(gameObject);
        }
        prevPosition = transform.position;
    }
    #endregion
}
