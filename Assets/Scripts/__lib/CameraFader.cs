using UnityEngine;

public class CameraFader : MonoBehaviour
{
    #region Private members
    private bool fading = false;
    private int guiDepth = -1000;

    private Texture2D dummyTex;
    private GUIStyle bgStyle;
    private TimeSource timeSource;

    private float startTime;
    private float endTime;
    private float startAlpha;
    private float endAlpha;

    private Color currentColor = Color.white;
    #endregion

    #region Public properties
    public float Percent {
        get {
            if (fading)
                return Mathf.Clamp01((timeSource.TotalTime - startTime) / (endTime - startTime));
            else
                return 0.0f;
        }
    }

    public bool IsFading {
        get {
            return fading;
        }
    }

    public Texture2D FadeTexture {
        get {
            return bgStyle.normal.background;
        }
        set {
            bgStyle.normal.background = value;
        }
    }

    public int GuiDepth {
        get {
            return guiDepth;
        }
        set {
            guiDepth = value;
        }
    }
    #endregion

    #region Public methods
    public void FadeIn(float duration)
    {
        float realDuration = duration * currentColor.a;
        this.FadeTo(0.0f, realDuration);
    }

    public void FadeOut(float duration)
    {
        float realDuration = duration * (1.0f - currentColor.a);
        this.FadeTo(1.0f, realDuration);
    }

    public void FadeTo(float alpha, float duration)
    {
        startTime = timeSource.TotalTime;
        endTime = startTime + duration;
        startAlpha = currentColor.a;
        endAlpha = alpha;
        fading = true;
    }

    public void ForceAlpha(float alpha)
    {
        fading = false;
        currentColor.a = alpha;
    }

    public void Reset()
    {
        currentColor.a = 0.0f;
        fading = false;
    }
    #endregion

    #region Unity callbacks
    void Awake()
    {
        dummyTex = new Texture2D(1, 1);
        dummyTex.SetPixel(0, 0, Color.clear);
        dummyTex.Apply();

        bgStyle = new GUIStyle();
        bgStyle.normal.background = null;

        timeSource = GameCamera.Instance.FSM.TimeSource;
    }

    void Update()
    {
        if (fading) {
            float t = Mathf.Clamp01((timeSource.TotalTime - startTime) / (endTime - startTime)),
                  s = 1.0f - t;

            currentColor.a = (startAlpha * s) + (endAlpha * t);

            if (MathUtils.Equals(t, 1.0f)) {
                currentColor.a = endAlpha;
                fading = false;
            }
        }
    }

    void OnGUI()
    {
        Color prevColor = GUI.color;
        GUI.color = currentColor;
        GUI.depth = guiDepth;
        GUI.Label(new Rect(0, 0, Screen.width, Screen.height), dummyTex, bgStyle);
        GUI.color = prevColor;
    }
    #endregion
};
