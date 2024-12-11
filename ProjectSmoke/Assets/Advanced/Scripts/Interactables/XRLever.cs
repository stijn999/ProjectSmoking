using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityEngine.XR.Content.Interaction
{
    /// <summary>
    /// An interactable lever that snaps into an on or off position by a direct interactor.
    /// </summary>
    public class XRLever : XRBaseInteractable
    {
        const float k_LeverDeadZone = 0.1f; // Dead zone to prevent rapid switching between on and off states when the lever is near the center

        [SerializeField]
        [Tooltip("The object that is visually grabbed and manipulated.")]
        Transform m_Handle = null; // The visual handle of the lever

        [SerializeField]
        [Tooltip("The current value of the lever. True represents 'on', and false represents 'off'.")]
        bool m_Value = false; // The current state of the lever

        [SerializeField]
        [Tooltip("If enabled, the lever will snap to the value position when released.")]
        bool m_LockToValue = false; // Whether the lever should snap to the 'on' or 'off' position when released

        [SerializeField]
        [Tooltip("Angle of the lever in the 'on' position.")]
        [Range(-90.0f, 90.0f)]
        float m_MaxAngle = 90.0f; // The angle representing the 'on' position of the lever

        [SerializeField]
        [Tooltip("Angle of the lever in the 'off' position.")]
        [Range(-90.0f, 90.0f)]
        float m_MinAngle = -90.0f; // The angle representing the 'off' position of the lever

        [SerializeField]
        [Tooltip("Events to trigger when the lever activates (switches to 'on').")]
        UnityEvent m_OnLeverActivate = new UnityEvent(); // Event triggered when the lever is set to 'on'

        [SerializeField]
        [Tooltip("Events to trigger when the lever deactivates (switches to 'off').")]
        UnityEvent m_OnLeverDeactivate = new UnityEvent(); // Event triggered when the lever is set to 'off'

        IXRSelectInteractor m_Interactor; // The interactor interacting with the lever

        /// <summary>
        /// The object that is visually grabbed and manipulated.
        /// </summary>
        public Transform handle
        {
            get => m_Handle;
            set => m_Handle = value;
        }

        /// <summary>
        /// The value of the lever. True represents 'on', and false represents 'off'.
        /// </summary>
        public bool value
        {
            get => m_Value;
            set => SetValue(value, true);
        }

        /// <summary>
        /// If enabled, the lever will snap to the value position when released.
        /// </summary>
        public bool lockToValue
        {
            get => m_LockToValue;
            set => m_LockToValue = value;
        }

        /// <summary>
        /// Angle of the lever in the 'on' position.
        /// </summary>
        public float maxAngle
        {
            get => m_MaxAngle;
            set => m_MaxAngle = value;
        }

        /// <summary>
        /// Angle of the lever in the 'off' position.
        /// </summary>
        public float minAngle
        {
            get => m_MinAngle;
            set => m_MinAngle = value;
        }

        /// <summary>
        /// Events to trigger when the lever activates (switches to 'on').
        /// </summary>
        public UnityEvent onLeverActivate => m_OnLeverActivate;

        /// <summary>
        /// Events to trigger when the lever deactivates (switches to 'off').
        /// </summary>
        public UnityEvent onLeverDeactivate => m_OnLeverDeactivate;

        void Start()
        {
            // Initialize the lever to its current value
            SetValue(m_Value, true);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            // Register event listeners for when the lever is selected or deselected
            selectEntered.AddListener(StartGrab);
            selectExited.AddListener(EndGrab);
        }

        protected override void OnDisable()
        {
            // Unregister event listeners to prevent memory leaks
            selectEntered.RemoveListener(StartGrab);
            selectExited.RemoveListener(EndGrab);
            base.OnDisable();
        }

        void StartGrab(SelectEnterEventArgs args)
        {
            // Store the interactor interacting with the lever
            m_Interactor = args.interactorObject;
        }

        void EndGrab(SelectExitEventArgs args)
        {
            // Set the lever to its current value and clear the interactor reference
            SetValue(m_Value, true);
            m_Interactor = null;
        }

        public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            base.ProcessInteractable(updatePhase);

            if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
            {
                // Update the lever value if it is currently selected
                if (isSelected)
                {
                    UpdateValue();
                }
            }
        }

        Vector3 GetLookDirection()
        {
            // Calculate the direction from the handle to the interactor's attach point
            Vector3 direction = m_Interactor.GetAttachTransform(this).position - m_Handle.position;
            direction = transform.InverseTransformDirection(direction);
            direction.x = 0; // Project onto the Y-Z plane

            return direction.normalized;
        }

        void UpdateValue()
        {
            // Determine the current look direction and angle
            var lookDirection = GetLookDirection();
            var lookAngle = Mathf.Atan2(lookDirection.z, lookDirection.y) * Mathf.Rad2Deg;

            // Clamp the angle within the defined range
            if (m_MinAngle < m_MaxAngle)
                lookAngle = Mathf.Clamp(lookAngle, m_MinAngle, m_MaxAngle);
            else
                lookAngle = Mathf.Clamp(lookAngle, m_MaxAngle, m_MinAngle);

            // Calculate the distance from the angle to the max and min positions
            var maxAngleDistance = Mathf.Abs(m_MaxAngle - lookAngle);
            var minAngleDistance = Mathf.Abs(m_MinAngle - lookAngle);

            // Apply dead zone to the distances
            if (m_Value)
                maxAngleDistance *= (1.0f - k_LeverDeadZone);
            else
                minAngleDistance *= (1.0f - k_LeverDeadZone);

            // Determine the new value based on the closest angle
            var newValue = (maxAngleDistance < minAngleDistance);

            // Update the handle's angle and set the new value
            SetHandleAngle(lookAngle);
            SetValue(newValue);
        }

        void SetValue(bool isOn, bool forceRotation = false)
        {
            // If the value is already set, update the handle angle if forced
            if (m_Value == isOn)
            {
                if (forceRotation)
                    SetHandleAngle(m_Value ? m_MaxAngle : m_MinAngle);

                return;
            }

            // Update the lever's value and trigger appropriate events
            m_Value = isOn;
            if (m_Value)
                m_OnLeverActivate.Invoke();
            else
                m_OnLeverDeactivate.Invoke();

            // Snap the handle to the new value if not selected or if forced
            if (!isSelected && (m_LockToValue || forceRotation))
                SetHandleAngle(m_Value ? m_MaxAngle : m_MinAngle);
        }

        void SetHandleAngle(float angle)
        {
            // Set the handle's rotation based on the specified angle
            if (m_Handle != null)
                m_Handle.localRotation = Quaternion.Euler(angle, 0.0f, 0.0f);
        }

        void OnDrawGizmosSelected()
        {
            // Draw gizmos to visualize the lever's angle positions in the Unity Editor
            var angleStartPoint = transform.position;
            if (m_Handle != null)
                angleStartPoint = m_Handle.position;

            const float k_AngleLength = 0.25f;

            var angleMaxPoint = angleStartPoint + transform.TransformDirection(Quaternion.Euler(m_MaxAngle, 0.0f, 0.0f) * Vector3.up) * k_AngleLength;
            var angleMinPoint = angleStartPoint + transform.TransformDirection(Quaternion.Euler(m_MinAngle, 0.0f, 0.0f) * Vector3.up) * k_AngleLength;

            Gizmos.color = Color.green;
            Gizmos.DrawLine(angleStartPoint, angleMaxPoint);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(angleStartPoint, angleMinPoint);
        }

        void OnValidate()
        {
            // Ensure the handle is set to the correct angle when values are changed in the Inspector
            SetHandleAngle(m_Value ? m_MaxAngle : m_MinAngle);
        }
    }
}
