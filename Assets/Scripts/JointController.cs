using UnityEngine;

public class JointController : MonoBehaviour
{
    public enum JointDegree
    {
        RotateX = 0,
        RotateY = 1,
        RotateZ = 2
    }
    public JointDegree degreeOfFreedom;
    public float maxRotationAngle = 120f;
    private Vector3 _axis;
    private Quaternion _default;
    void Start()
    {
        _axis = new Vector3(0, 0, 0);
        _default = transform.localRotation;
    }

    public void SetRotation(float degrees)
    {
        var transform1 = transform;
        if (degrees > 360f || degrees < -360f) degrees %= 360f;
        _axis[(int)degreeOfFreedom] = Mathf.Clamp(degrees, -maxRotationAngle, maxRotationAngle);
        var q = new Quaternion { eulerAngles = _axis };
        transform1.localRotation = _default * q;
    }

    public float GetRotation()
    {
        return _axis[(int)degreeOfFreedom];
    }

    public void Rotate(float delta)
    {
        SetRotation(GetRotation() + delta);
    }
}
