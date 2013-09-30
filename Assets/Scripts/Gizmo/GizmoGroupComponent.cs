using UnityEngine;
using System.Collections;

public class GizmoGroupComponent : MonoBehaviour
{
    public GizmoHandleComponent axisX;
    public GizmoHandleComponent axisY;

    public void setParent(Transform parent) {
        transform.parent = parent;
        axisX.setParent(parent);
        axisY.setParent(parent);
    }

    public void setType(GizmoComponent.GizmoType type)
    {
        axisX.setVisibility(false);
        axisY.setVisibility(false);

        switch (type)
        {
            case (GizmoComponent.GizmoType.HORIZONTAL):
                axisX.setVisibility(true);
                break;
            case (GizmoComponent.GizmoType.VERTICAL):
                axisY.setVisibility(true);
                break;
            case (GizmoComponent.GizmoType.BOTH):
                axisX.setVisibility(true);
                axisY.setVisibility(true);
                break;
        }
    }
}
