using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityEngine.XR.Content.Interaction
{
    /// <summary>
    /// An interactable knob that follows the rotation of the interactor.
    /// </summary>
    public class XRKnob : XRBaseInteractable
    {
        const float k_ModeSwitchDeadZone = 0.1f; // Dead zone to prevent rapid switching between rotation modes

        /// <summary>
        /// Helper class used to track rotations that can go beyond 180 degrees while minimizing accumulation error.
        /// </summary>
        struct TrackedRotation
        {
            /// <summary>
            /// The anchor rotation used to calculate an offset from.
            /// </summary>
            float m_BaseAngle;

            /// <summary>
            /// The target rotation angle we calculate the offset to.
            /// </summary>
            float m_CurrentOffset;

            /// <summary>
            /// Any previous offsets that have been accumulated.
            /// </summary>
            float m_AccumulatedAngle;

            /// <summary>
            /// The total rotation that occurred from when tracking started.
            /// </summary>
            public float totalOffset => m_AccumulatedAngle + m_CurrentOffset;

            /// <summary>
            /// Resets the tracked rotation so that the total offset returns to 0.
            /// </summary>
            public void Reset()
            {
                m_BaseAngle = 0.0f;
                m_CurrentOffset = 0.0f;
                m_AccumulatedAngle = 0.0f;
            }

            /// <summary>
            /// Sets a new anchor rotation while maintaining any previously accumulated offset.
            /// </summary>
            /// <param name="direction">The direction vector used to calculate a rotation angle.</param>
            public void SetBaseFromVector(Vector3 direction)
            {
                // Update accumulated angle
                m_AccumulatedAngle += m_CurrentOffset;

                // Set a new base angle
                m_BaseAngle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
                m_CurrentOffset = 0.0f;
            }

            /// <summary>
            /// Sets the target rotation based on a direction vector and calculates the offset.
            /// </summary>
            /// <param name="direction">The direction vector used to calculate the target rotation.</param>
            public void SetTargetFromVector(Vector3 direction)
            {
                // Calculate the target angle
                var targetAngle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;

                // Calculate and set the offset
                m_CurrentOffset = ShortestAngleDistance(m_BaseAngle, targetAngle, 360.0f);

                // If the offset is greater than 90 degrees, update the base angle
                if (Mathf.Abs(m_CurrentOffset) > 90.0f)
                {
                    m_BaseAngle = targetAngle;
                    m_AccumulatedAngle += m_CurrentOffset;
                    m_CurrentOffset = 0.0f;
                }
            }
        }

        [Serializable]
        public class ValueChangeEvent : UnityEvent<float> { }

        [SerializeField]
        [Tooltip("The object that is visually grabbed and manipulated.")]
        Transform m_Handle = null; // The visual handle of the knob

        [SerializeField]
        [Tooltip("The current value of the knob.")]
        [Range(0.0f, 1.0f)]
        float m_Value = 0.5f; // The value of the knob, ranging from 0 to 1

        [SerializeField]
        [Tooltip("Whether the knob's rotation should be clamped by the angle limits.")]
        bool m_ClampedMotion = true; // Whether to restrict knob rotation within defined limits

        [SerializeField]
        [Tooltip("The maximum rotation angle of the knob at value '1'.")]
        float m_MaxAngle = 90.0f; // The maximum rotation angle when the knob value is 1

        [SerializeField]
        [Tooltip("The minimum rotation angle of the knob at value '0'.")]
        float m_MinAngle = -90.0f; // The minimum rotation angle when the knob value is 0

        [SerializeField]
        [Tooltip("Angle increments to support, if greater than '0'.")]
        float m_AngleIncrement = 0.0f; // Angle increments for snapping the knob to discrete positions

        [SerializeField]
        [Tooltip("The position radius within which the interactor controls rotation.")]
        float m_PositionTrackedRadius = 0.1f; // Radius around the handle where position affects rotation

        [SerializeField]
        [Tooltip("How sensitive the knob is to controller rotation.")]
        float m_TwistSensitivity = 1.5f; // Sensitivity multiplier for rotation

        [SerializeField]
        [Tooltip("Events triggered when the knob is rotated.")]
        ValueChangeEvent m_OnValueChange = new ValueChangeEvent(); // Event for notifying knob value changes

        IXRSelectInteractor m_Interactor; // The interactor interacting with the knob

        bool m_PositionDriven = false; // Whether rotation is driven by position offset
        bool m_UpVectorDriven = false; // Whether rotation is driven by the controller's up vector

        TrackedRotation m_PositionAngles = new TrackedRotation(); // Tracked rotation based on position offset
        TrackedRotation m_UpVectorAngles = new TrackedRotation(); // Tracked rotation based on the up vector
        TrackedRotation m_ForwardVectorAngles = new TrackedRotation(); // Tracked rotation based on the forward vector

        float m_BaseKnobRotation = 0.0f; // The base rotation of the knob

        /// <summary>
        /// The object that is visually grabbed and manipulated.
        /// </summary>
        public Transform handle
        {
            get => m_Handle;
            set => m_Handle = value;
        }

        /// <summary>
        /// The current value of the knob.
        /// </summary>
        public float value
        {
            get => m_Value;
            set
            {
                SetValue(value);
                SetKnobRotation(ValueToRotation());
            }
        }

        /// <summary>
        /// Whether the knob's rotation should be clamped by the angle limits.
        /// </summary>
        public bool clampedMotion
        {
            get => m_ClampedMotion;
            set => m_ClampedMotion = value;
        }

        /// <summary>
        /// The maximum rotation angle of the knob at value '1'.
        /// </summary>
        public float maxAngle
        {
            get => m_MaxAngle;
            set => m_MaxAngle = value;
        }

        /// <summary>
        /// The minimum rotation angle of the knob at value '0'.
        /// </summary>
        public float minAngle
        {
            get => m_MinAngle;
            set => m_MinAngle = value;
        }

        /// <summary>
        /// The position radius within which the interactor controls rotation.
        /// </summary>
        public float positionTrackedRadius
        {
            get => m_PositionTrackedRadius;
            set => m_PositionTrackedRadius = value;
        }

        /// <summary>
        /// Events triggered when the knob is rotated.
        /// </summary>
        public ValueChangeEvent onValueChange => m_OnValueChange;

        private void Start()
        {
            // Initialize the knob value and rotation
            SetValue(m_Value);
            SetKnobRotation(ValueToRotation());
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            // Add event listeners for knob interactions
            selectEntered.AddListener(StartGrab);
            selectExited.AddListener(EndGrab);
        }

        protected override void OnDisable()
        {
            // Remove event listeners for knob interactions
            selectEntered.RemoveListener(StartGrab);
            selectExited.RemoveListener(EndGrab);
            base.OnDisable();
        }

        private void StartGrab(SelectEnterEventArgs args)
        {
            m_Interactor = args.interactorObject;

            // Reset rotation tracking
            m_PositionAngles.Reset();
            m_UpVectorAngles.Reset();
            m_ForwardVectorAngles.Reset();

            // Update the base rotation of the knob and start tracking rotation
            UpdateBaseKnobRotation();
            UpdateRotation(true);
        }

        private void EndGrab(SelectExitEventArgs args)
        {
            m_Interactor = null;
        }

        public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            base.ProcessInteractable(updatePhase);

            if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
            {
                // Update rotation if the knob is selected
                if (isSelected)
                {
                    UpdateRotation();
                }
            }
        }

        private void UpdateRotation(bool freshCheck = false)
        {
            // Get the transform of the interactor
            var interactorTransform = m_Interactor.GetAttachTransform(this);

            // Calculate potential sources of rotation: position offset, forward vector, and up vector
            var localOffset = transform.InverseTransformVector(interactorTransform.position - m_Handle.position);
            localOffset.y = 0.0f;
            var radiusOffset = transform.TransformVector(localOffset).magnitude;
            localOffset.Normalize();

            var localForward = transform.InverseTransformDirection(interactorTransform.forward);
            var localY = Math.Abs(localForward.y);
            localForward.y = 0.0f;
            localForward.Normalize();

            var localUp = transform.InverseTransformDirection(interactorTransform.up);
            localUp.y = 0.0f;
            localUp.Normalize();

            if (m_PositionDriven && !freshCheck)
                radiusOffset *= (1.0f + k_ModeSwitchDeadZone);

            // Determine which rotation mode to use
            if (radiusOffset >= m_PositionTrackedRadius)
            {
                if (!m_PositionDriven || freshCheck)
                {
                    m_PositionAngles.SetBaseFromVector(localOffset);
                    m_PositionDriven = true;
                }
            }
            else
                m_PositionDriven = false;

            if (!freshCheck)
            {
                if (!m_UpVectorDriven)
                    localY *= (1.0f - (k_ModeSwitchDeadZone * 0.5f));
                else
                    localY *= (1.0f + (k_ModeSwitchDeadZone * 0.5f));
            }

            if (localY > 0.707f)
            {
                if (!m_UpVectorDriven || freshCheck)
                {
                    m_UpVectorAngles.SetBaseFromVector(localUp);
                    m_UpVectorDriven = true;
                }
            }
            else
            {
                if (m_UpVectorDriven || freshCheck)
                {
                    m_ForwardVectorAngles.SetBaseFromVector(localForward);
                    m_UpVectorDriven = false;
                }
            }

            // Apply the selected rotation mode
            if (m_PositionDriven)
                m_PositionAngles.SetTargetFromVector(localOffset);

            if (m_UpVectorDriven)
                m_UpVectorAngles.SetTargetFromVector(localUp);
            else
                m_ForwardVectorAngles.SetTargetFromVector(localForward);

            // Calculate and set the knob rotation
            var knobRotation = m_BaseKnobRotation - ((m_UpVectorAngles.totalOffset + m_ForwardVectorAngles.totalOffset) * m_TwistSensitivity) - m_PositionAngles.totalOffset;

            // Clamp the rotation to the defined range
            if (m_ClampedMotion)
                knobRotation = Mathf.Clamp(knobRotation, m_MinAngle, m_MaxAngle);

            SetKnobRotation(knobRotation);

            // Calculate and set the knob value
            var knobValue = (knobRotation - m_MinAngle) / (m_MaxAngle - m_MinAngle);
            SetValue(knobValue);
        }

        private void SetKnobRotation(float angle)
        {
            // Snap to angle increments if specified
            if (m_AngleIncrement > 0)
            {
                var normalizeAngle = angle - m_MinAngle;
                angle = (Mathf.Round(normalizeAngle / m_AngleIncrement) * m_AngleIncrement) + m_MinAngle;
            }

            // Apply the calculated rotation to the knob handle
            if (m_Handle != null)
                m_Handle.localEulerAngles = new Vector3(0.0f, angle, 0.0f);
        }

        private void SetValue(float value)
        {
            // Clamp value to range if motion is clamped
            if (m_ClampedMotion)
                value = Mathf.Clamp01(value);

            // Snap value to angle increments if specified
            if (m_AngleIncrement > 0)
            {
                var angleRange = m_MaxAngle - m_MinAngle;
                var angle = Mathf.Lerp(0.0f, angleRange, value);
                angle = Mathf.Round(angle / m_AngleIncrement) * m_AngleIncrement;
                value = Mathf.InverseLerp(0.0f, angleRange, angle);
            }

            // Set the value and invoke the change event
            m_Value = value;
            m_OnValueChange.Invoke(m_Value);
        }

        private float ValueToRotation()
        {
            // Convert the knob value to rotation angle
            return m_ClampedMotion ? Mathf.Lerp(m_MinAngle, m_MaxAngle, m_Value) : Mathf.LerpUnclamped(m_MinAngle, m_MaxAngle, m_Value);
        }

        private void UpdateBaseKnobRotation()
        {
            // Update the base rotation of the knob based on its value
            m_BaseKnobRotation = Mathf.LerpUnclamped(m_MinAngle, m_MaxAngle, m_Value);
        }

        private static float ShortestAngleDistance(float start, float end, float max)
        {
            var angleDelta = end - start;
            var angleSign = Mathf.Sign(angleDelta);

            angleDelta = Math.Abs(angleDelta) % max;
            if (angleDelta > (max * 0.5f))
                angleDelta = -(max - angleDelta);

            return angleDelta * angleSign;
        }

        private void OnDrawGizmosSelected()
        {
            const int k_CircleSegments = 16;
            const float k_SegmentRatio = 1.0f / k_CircleSegments;

            // Draw a circle to represent the position tracking radius
            if (m_PositionTrackedRadius <= Mathf.Epsilon)
                return;

            var circleCenter = transform.position;

            if (m_Handle != null)
                circleCenter = m_Handle.position;

            var circleX = transform.right;
            var circleY = transform.forward;

            Gizmos.color = Color.green;
            var segmentCounter = 0;
            while (segmentCounter < k_CircleSegments)
            {
                var startAngle = (float)segmentCounter * k_SegmentRatio * 2.0f * Mathf.PI;
                segmentCounter++;
                var endAngle = (float)segmentCounter * k_SegmentRatio * 2.0f * Mathf.PI;

                Gizmos.DrawLine(circleCenter + (Mathf.Cos(startAngle) * circleX + Mathf.Sin(startAngle) * circleY) * m_PositionTrackedRadius,
                    circleCenter + (Mathf.Cos(endAngle) * circleX + Mathf.Sin(endAngle) * circleY) * m_PositionTrackedRadius);
            }
        }

        private void OnValidate()
        {
            // Ensure the value is clamped if motion is clamped
            if (m_ClampedMotion)
                m_Value = Mathf.Clamp01(m_Value);

            // Ensure minimum angle does not exceed maximum angle
            if (m_MinAngle > m_MaxAngle)
                m_MinAngle = m_MaxAngle;

            // Update the knob rotation based on the new values
            SetKnobRotation(ValueToRotation());
        }
    }
}
