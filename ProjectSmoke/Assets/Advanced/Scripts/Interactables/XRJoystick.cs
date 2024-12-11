using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityEngine.XR.Content.Interaction
{
    /// <summary>
    /// An interactable joystick that can move side-to-side and forward-and-back by a direct interactor.
    /// </summary>
    public class XRJoystick : XRBaseInteractable
    {
        const float k_MaxDeadZonePercent = 0.9f; // Maximum percentage of the joystick's angle that can be a dead zone

        /// <summary>
        /// Enum to define the types of joystick motion.
        /// </summary>
        public enum JoystickType
        {
            BothCircle,  // Joystick moves in a circular motion
            BothSquare,  // Joystick moves in a square motion
            FrontBack,   // Joystick moves forward and backward
            LeftRight,   // Joystick moves left and right
        }

        [Serializable]
        public class ValueChangeEvent : UnityEvent<float> { }

        [Tooltip("Controls how the joystick moves.")]
        [SerializeField]
        private JoystickType m_JoystickMotion = JoystickType.BothCircle; // Defines the joystick's motion type

        [SerializeField]
        [Tooltip("The object that is visually grabbed and manipulated.")]
        private Transform m_Handle = null; // The visual handle of the joystick

        [SerializeField]
        [Tooltip("The value of the joystick.")]
        private Vector2 m_Value = Vector2.zero; // Current value of the joystick in 2D space

        [SerializeField]
        [Tooltip("If true, the joystick will return to center on release.")]
        private bool m_RecenterOnRelease = true; // Whether the joystick returns to the center position when released

        [SerializeField]
        [Tooltip("Maximum angle the joystick can move.")]
        [Range(1.0f, 90.0f)]
        private float m_MaxAngle = 60.0f; // Maximum angle the joystick can be pushed

        [SerializeField]
        [Tooltip("Minimum amount the joystick must move off the center to register changes.")]
        [Range(1.0f, 90.0f)]
        private float m_DeadZoneAngle = 10.0f; // Minimum angle to trigger joystick movement

        [SerializeField]
        [Tooltip("Events to trigger when the joystick's x value changes.")]
        private ValueChangeEvent m_OnValueChangeX = new ValueChangeEvent(); // Event triggered when the x value changes

        [SerializeField]
        [Tooltip("Events to trigger when the joystick's y value changes.")]
        private ValueChangeEvent m_OnValueChangeY = new ValueChangeEvent(); // Event triggered when the y value changes

        private IXRSelectInteractor m_Interactor; // The interactor interacting with this joystick

        /// <summary>
        /// Controls how the joystick moves.
        /// </summary>
        public JoystickType joystickMotion
        {
            get => m_JoystickMotion;
            set => m_JoystickMotion = value;
        }

        /// <summary>
        /// The object that is visually grabbed and manipulated.
        /// </summary>
        public Transform handle
        {
            get => m_Handle;
            set => m_Handle = value;
        }

        /// <summary>
        /// The value of the joystick.
        /// </summary>
        public Vector2 value
        {
            get => m_Value;
            set
            {
                if (!m_RecenterOnRelease)
                {
                    SetValue(value);
                    SetHandleAngle(value * m_MaxAngle);
                }
            }
        }

        /// <summary>
        /// If true, the joystick will return to center on release.
        /// </summary>
        public bool recenterOnRelease
        {
            get => m_RecenterOnRelease;
            set => m_RecenterOnRelease = value;
        }

        /// <summary>
        /// Maximum angle the joystick can move.
        /// </summary>
        public float maxAngle
        {
            get => m_MaxAngle;
            set => m_MaxAngle = value;
        }

        /// <summary>
        /// Minimum amount the joystick must move off the center to register changes.
        /// </summary>
        public float deadZoneAngle
        {
            get => m_DeadZoneAngle;
            set => m_DeadZoneAngle = value;
        }

        /// <summary>
        /// Events to trigger when the joystick's x value changes.
        /// </summary>
        public ValueChangeEvent onValueChangeX => m_OnValueChangeX;

        /// <summary>
        /// Events to trigger when the joystick's y value changes.
        /// </summary>
        public ValueChangeEvent onValueChangeY => m_OnValueChangeY;

        private void Start()
        {
            // Initialize the joystick handle to the center position if recentering on release is enabled
            if (m_RecenterOnRelease)
                SetHandleAngle(Vector2.zero);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            // Add event listeners for joystick interactions
            selectEntered.AddListener(StartGrab);
            selectExited.AddListener(EndGrab);
        }

        protected override void OnDisable()
        {
            // Remove event listeners when the joystick is disabled
            selectEntered.RemoveListener(StartGrab);
            selectExited.RemoveListener(EndGrab);
            base.OnDisable();
        }

        private void StartGrab(SelectEnterEventArgs args)
        {
            // Store the interactor interacting with the joystick
            m_Interactor = args.interactorObject;
        }

        private void EndGrab(SelectExitEventArgs args)
        {
            // Update joystick value and reset handle position and value if recentering on release
            UpdateValue();

            if (m_RecenterOnRelease)
            {
                SetHandleAngle(Vector2.zero);
                SetValue(Vector2.zero);
            }

            m_Interactor = null;
        }

        public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            base.ProcessInteractable(updatePhase);

            if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
            {
                // Update joystick value if currently selected
                if (isSelected)
                {
                    UpdateValue();
                }
            }
        }

        private Vector3 GetLookDirection()
        {
            // Calculate direction of joystick movement based on interactor's attach transform
            Vector3 direction = m_Interactor.GetAttachTransform(this).position - m_Handle.position;
            direction = transform.InverseTransformDirection(direction);
            switch (m_JoystickMotion)
            {
                case JoystickType.FrontBack:
                    direction.x = 0;
                    break;
                case JoystickType.LeftRight:
                    direction.z = 0;
                    break;
            }

            direction.y = Mathf.Clamp(direction.y, 0.01f, 1.0f);
            return direction.normalized;
        }

        private void UpdateValue()
        {
            // Update joystick value based on its current direction
            var lookDirection = GetLookDirection();

            // Get up/down and left/right angles
            var upDownAngle = Mathf.Atan2(lookDirection.z, lookDirection.y) * Mathf.Rad2Deg;
            var leftRightAngle = Mathf.Atan2(lookDirection.x, lookDirection.y) * Mathf.Rad2Deg;

            // Determine sign for angles
            var signX = Mathf.Sign(leftRightAngle);
            var signY = Mathf.Sign(upDownAngle);

            upDownAngle = Mathf.Abs(upDownAngle);
            leftRightAngle = Mathf.Abs(leftRightAngle);

            // Calculate stick value
            var stickValue = new Vector2(leftRightAngle, upDownAngle) * (1.0f / m_MaxAngle);

            // Clamp values based on joystick motion type
            if (m_JoystickMotion != JoystickType.BothCircle)
            {
                stickValue.x = Mathf.Clamp01(stickValue.x);
                stickValue.y = Mathf.Clamp01(stickValue.y);
            }
            else
            {
                if (stickValue.magnitude > 1.0f)
                {
                    stickValue.Normalize();
                }
            }

            // Rebuild angle values for visuals
            leftRightAngle = stickValue.x * signX * m_MaxAngle;
            upDownAngle = stickValue.y * signY * m_MaxAngle;

            // Apply deadzone
            var deadZone = m_DeadZoneAngle / m_MaxAngle;
            var aliveZone = (1.0f - deadZone);
            stickValue.x = Mathf.Clamp01((stickValue.x - deadZone)) / aliveZone;
            stickValue.y = Mathf.Clamp01((stickValue.y - deadZone)) / aliveZone;

            // Re-apply signs
            stickValue.x *= signX;
            stickValue.y *= signY;

            // Update handle angle and joystick value
            SetHandleAngle(new Vector2(leftRightAngle, upDownAngle));
            SetValue(stickValue);
        }

        private void SetValue(Vector2 value)
        {
            // Set joystick value and trigger corresponding events
            m_Value = value;
            m_OnValueChangeX.Invoke(m_Value.x);
            m_OnValueChangeY.Invoke(m_Value.y);
        }

        private void SetHandleAngle(Vector2 angles)
        {
            // Adjust the handle's angle based on joystick angles
            if (m_Handle == null)
                return;

            var xComp = Mathf.Tan(angles.x * Mathf.Deg2Rad);
            var zComp = Mathf.Tan(angles.y * Mathf.Deg2Rad);
            var largerComp = Mathf.Max(Mathf.Abs(xComp), Mathf.Abs(zComp));
            var yComp = Mathf.Sqrt(1.0f - largerComp * largerComp);

            m_Handle.up = (transform.up * yComp) + (transform.right * xComp) + (transform.forward * zComp);
        }

        private void OnDrawGizmosSelected()
        {
            // Draw gizmos to visualize the joystick's movement constraints
            var angleStartPoint = transform.position;

            if (m_Handle != null)
                angleStartPoint = m_Handle.position;

            const float k_AngleLength = 0.25f;

            if (m_JoystickMotion != JoystickType.LeftRight)
            {
                Gizmos.color = Color.green;
                var axisPoint1 = angleStartPoint + transform.TransformDirection(Quaternion.Euler(m_MaxAngle, 0.0f, 0.0f) * Vector3.up) * k_AngleLength;
                var axisPoint2 = angleStartPoint + transform.TransformDirection(Quaternion.Euler(-m_MaxAngle, 0.0f, 0.0f) * Vector3.up) * k_AngleLength;
                Gizmos.DrawLine(angleStartPoint, axisPoint1);
                Gizmos.DrawLine(angleStartPoint, axisPoint2);

                if (m_DeadZoneAngle > 0.0f)
                {
                    Gizmos.color = Color.red;
                    axisPoint1 = angleStartPoint + transform.TransformDirection(Quaternion.Euler(m_DeadZoneAngle, 0.0f, 0.0f) * Vector3.up) * k_AngleLength;
                    axisPoint2 = angleStartPoint + transform.TransformDirection(Quaternion.Euler(-m_DeadZoneAngle, 0.0f, 0.0f) * Vector3.up) * k_AngleLength;
                    Gizmos.DrawLine(angleStartPoint, axisPoint1);
                    Gizmos.DrawLine(angleStartPoint, axisPoint2);
                }
            }

            if (m_JoystickMotion != JoystickType.FrontBack)
            {
                Gizmos.color = Color.green;
                var axisPoint1 = angleStartPoint + transform.TransformDirection(Quaternion.Euler(0.0f, 0.0f, m_MaxAngle) * Vector3.up) * k_AngleLength;
                var axisPoint2 = angleStartPoint + transform.TransformDirection(Quaternion.Euler(0.0f, 0.0f, -m_MaxAngle) * Vector3.up) * k_AngleLength;
                Gizmos.DrawLine(angleStartPoint, axisPoint1);
                Gizmos.DrawLine(angleStartPoint, axisPoint2);

                if (m_DeadZoneAngle > 0.0f)
                {
                    Gizmos.color = Color.red;
                    axisPoint1 = angleStartPoint + transform.TransformDirection(Quaternion.Euler(0.0f, 0.0f, m_DeadZoneAngle) * Vector3.up) * k_AngleLength;
                    axisPoint2 = angleStartPoint + transform.TransformDirection(Quaternion.Euler(0.0f, 0.0f, -m_DeadZoneAngle) * Vector3.up) * k_AngleLength;
                    Gizmos.DrawLine(angleStartPoint, axisPoint1);
                    Gizmos.DrawLine(angleStartPoint, axisPoint2);
                }
            }
        }

        private void OnValidate()
        {
            // Ensure the dead zone angle does not exceed a certain percentage of the maximum angle
            m_DeadZoneAngle = Mathf.Min(m_DeadZoneAngle, m_MaxAngle * k_MaxDeadZonePercent);
        }
    }
}
