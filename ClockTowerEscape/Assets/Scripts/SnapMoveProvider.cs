using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

// snap/step movement - moves in fixed increments instead of continuous
public class SnapMoveProvider : LocomotionProvider
{
    [SerializeField] float m_StepDistance = 1f;
    [SerializeField] float m_Cooldown = 0.3f;
    [SerializeField] float m_InputThreshold = 0.5f;
    [SerializeField] bool m_EnableStrafe = true;
    [SerializeField] Transform m_ForwardSource;

    [SerializeField]
    XRInputValueReader<Vector2> m_LeftHandMoveInput = new XRInputValueReader<Vector2>("Left Hand Move");

    [SerializeField]
    XRInputValueReader<Vector2> m_RightHandMoveInput = new XRInputValueReader<Vector2>("Right Hand Move");

    float m_LastStepTime;

    void OnEnable()
    {
        m_LeftHandMoveInput.EnableDirectActionIfModeUsed();
        m_RightHandMoveInput.EnableDirectActionIfModeUsed();
    }

    void OnDisable()
    {
        m_LeftHandMoveInput.DisableDirectActionIfModeUsed();
        m_RightHandMoveInput.DisableDirectActionIfModeUsed();
    }

    void Update()
    {
        if (Time.time - m_LastStepTime < m_Cooldown)
            return;

        var input = m_LeftHandMoveInput.ReadValue() + m_RightHandMoveInput.ReadValue();
        if (input.sqrMagnitude < m_InputThreshold * m_InputThreshold)
            return;

        var moveDir = GetMoveDirection(input);
        if (TryStartLocomotionImmediately())
        {
            var origin = mediator.xrOrigin;
            if (origin != null)
                origin.transform.position += moveDir * m_StepDistance;

            m_LastStepTime = Time.time;
            TryEndLocomotion();
        }
    }

    Vector3 GetMoveDirection(Vector2 input)
    {
        var source = m_ForwardSource != null ? m_ForwardSource : transform;

        var fwd = source.forward;
        fwd.y = 0f;
        fwd.Normalize();

        var dir = fwd * input.y;

        if (m_EnableStrafe)
        {
            var right = source.right;
            right.y = 0f;
            right.Normalize();
            dir += right * input.x;
        }

        return dir.normalized;
    }
}
