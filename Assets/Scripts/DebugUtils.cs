using System;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;

public class DebugUtils
{
    public struct Watch
    {
        public Rect rect;
        public string str;
    }

    public static bool isDebugActive = true;

    private static List<Watch> watches = new List<Watch>();
    private static int frameCounter = -1;

    #region Assert functions
    public static void Assert(bool value)
    {
        if (!isDebugActive)
            return;

        if (!value) {
            StackTrace st = new StackTrace(true);

            string filename = "empty";
            int    line     = -1;

            if (st.FrameCount > 1) {
                filename = st.GetFrame(1).GetFileName();
                line     = st.GetFrame(1).GetFileLineNumber();
            }

            UnityEngine.Debug.LogError("assertion faled @ " + filename + " (" + line.ToString() + ")");
            UnityEngine.Debug.Break();
        }
    }

    public static void Assert(bool value, string msg, params object[] args)
    {
        if (!isDebugActive)
            return;

        if (!value)
        {
            StackTrace st = new StackTrace(true);

            string filename = "empty";
            int    line     = -1;

            if (st.FrameCount > 1)
            {
                filename = st.GetFrame(1).GetFileName();
                line     = st.GetFrame(1).GetFileLineNumber();
            }

            UnityEngine.Debug.LogError("assertion faled @ " + filename + " (" + line.ToString() + ")\n" + "message: " + String.Format(msg, args));
            UnityEngine.Debug.Break();
        }
    }
    #endregion

    public static void AddWatch(Rect rect, string msg, params object[] args)
    {
        if (!isDebugActive)
            return;

        if (frameCounter != Time.frameCount)
        {
            frameCounter = Time.frameCount;
            watches.Clear();
        }

        Watch watch = new Watch();
        watch.rect = rect;
        watch.str = String.Format(msg, args);
        watches.Add(watch);
    }

    public static void DrawWatches()
    {
        if (!isDebugActive)
            return;

        Rect bgSize = new Rect(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);
        foreach (Watch watch in watches)
        {
            bgSize.xMin = Math.Min(bgSize.xMin, watch.rect.xMin);
            bgSize.yMin = Math.Min(bgSize.yMin, watch.rect.yMin);
            bgSize.xMax = Math.Max(bgSize.xMax, watch.rect.xMax);
            bgSize.yMax = Math.Max(bgSize.yMax, watch.rect.yMax);
        }

        GUI.Box(bgSize, "");

        foreach(Watch watch in watches)
        {
            GUI.Label(watch.rect, watch.str);
        }
    }
};
