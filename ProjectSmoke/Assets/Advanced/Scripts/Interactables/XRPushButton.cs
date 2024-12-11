using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityEngine.XR.Content.Interaction
{
    /// <summary>
    /// An interactable button that can be pressed by a direct interactor's movement.
    /// Supports toggle functionality and can trigger events on press and release.
    /// </summary>
    public class XRPushButton : XRBaseInteractable
    {
        /// <summary>
        /// Stores information about each interactor interacting with the button.
        /// </summary>
        class PressInfo
        {
            internal IXRHoverInteractor m_Interactor; // Reference to the interactor
            internal bool m_InPressRegion = false; // Indicates if the interactor is within the press region
            internal bool m_WrongSide = false; // Indicates if the interactor is on the wrong side of the button
        }

        /// <summary>
        /// Event triggered when the button's press value changes.
        /// </summary>
        [Serializable]
        public class ValueChangeEvent : UnityEvent<float> { }

        [SerializeField]
        [Tooltip("The object that is visually pressed down")]
        private Transform m_Button = null; // The visual representation of the button being pressed

        [SerializeField]
        [Tooltip("The distance the button can be pressed")]
        private float m_PressDistance = 0.1f; // Maximum distance the button can be pressed down

        [SerializeField]
        [Tooltip("Extra distance for clicking the button down")]
        private float m_PressBuffer = 0.01f; // Additional buffer distance for button press

        [SerializeField]
        [Tooltip("Offset from the button base to start testing for push")]
        private float m_ButtonOffset = 0.0f; // Offset to account for the initial button position

        [SerializeField]
        [Tooltip("How big of a surface area is available for pressing the button")]
        private float m_ButtonSize = 0.1f; // Size of the area that can be pressed on the button

        [SerializeField]
        [Tooltip("Treat this button like an on/off toggle")]
        private bool m_ToggleButton = false; // Whether the button behaves like a toggle switch

        [SerializeField]
        [Tooltip("Events to trigger when the button is pressed")]
        private UnityEvent m_OnPress; // Event triggered when the button is pressed

        [SerializeField]
        [Tooltip("Events to trigger when the button is released")]
        private UnityEvent m_OnRelease; // Event triggered when the button is released

        [SerializeField]
        [Tooltip("Events to trigger when the button pressed value is updated. Only called when the button is pressed")]
        private ValueChangeEvent m_OnValueChange; // Event triggered when the button's value changes

        private bool m_Pressed = false; // Indicates if the button is currently pressed
        private bool m_Toggled = false; // Indicates if the button is currently in the toggle state
        private float m_Value = 0f; // Current value of the button press, from 0 to 1
        private Vector3 m_BaseButtonPosition = Vector3.zero; // Initial position of the button

        private Dictionary<IXRHoverInteractor, PressInfo> m_HoveringInteractors = new Dictionary<IXRHoverInteractor, PressInfo>(); // Tracks interactors hovering over the button

        /// <summary>
        /// The object that is visually pressed down.
        /// </summary>
        public Transform button
        {
            get => m_Button;
            set => m_Button = value;
        }

        /// <summary>
        /// The distance the button can be pressed.
        /// </summary>
        public float pressDistance
        {
            get => m_PressDistance;
            set => m_PressDistance = value;
        }

        /// <summary>
        /// The distance (in percentage from 0 to 1) the button is currently being held down.
        /// </summary>
        public float value => m_Value;

        /// <summary>
        /// Events to trigger when the button is pressed.
        /// </summary>
        public UnityEvent onPress => m_OnPress;

        /// <summary>
        /// Events to trigger when the button is released.
        /// </summary>
        public UnityEvent onRelease => m_OnRelease;

        /// <summary>
        /// Events to trigger when the button distance value is changed. Only called when the button is pressed.
        /// </summary>
        public ValueChangeEvent onValueChange => m_OnValueChange;

        /// <summary>
        /// Whether or not a toggle button is in the locked down position.
        /// </summary>
        public bool toggleValue
        {
            get => m_ToggleButton && m_Toggled;
            set
            {
                if (!m_ToggleButton)
                    return;

                m_Toggled = value;
                if (m_Toggled)
                    SetButtonHeight(-m_PressDistance);
                else
                    SetButtonHeight(0.0f);
            }
        }

        /// <summary>
        /// Determines if the button can be hovered over by the given interactor.
        /// </summary>
        /// <param name="interactor">The interactor to check.</param>
        /// <returns>True if the interactor can hover over the button; otherwise, false.</returns>
        public override bool IsHoverableBy(IXRHoverInteractor interactor)
        {
            // Disallow hovering by XRRayInteractor (e.g., laser pointers)
            if (interactor is XRRayInteractor)
                return false;

            return base.IsHoverableBy(interactor);
        }

        void Start()
        {
            // Store the initial position of the button
            if (m_Button != null)
                m_BaseButtonPosition = m_Button.position;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            // Set the button height based on the toggle state
            if (m_Toggled)
                SetButtonHeight(-m_PressDistance);
            else
                SetButtonHeight(0.0f);

            // Add listeners for hover events
            hoverEntered.AddListener(StartHover);
            hoverExited.AddListener(EndHover);
        }

        protected override void OnDisable()
        {
            // Remove listeners for hover events
            hoverEntered.RemoveListener(StartHover);
            hoverExited.RemoveListener(EndHover);
            base.OnDisable();
        }

        /// <summary>
        /// Handles the start of hover interaction by adding the interactor to the list.
        /// </summary>
        /// <param name="args">Event arguments containing the interactor.</param>
        void StartHover(HoverEnterEventArgs args)
        {
            m_HoveringInteractors.Add(args.interactorObject, new PressInfo { m_Interactor = args.interactorObject });
        }

        /// <summary>
        /// Handles the end of hover interaction by removing the interactor from the list.
        /// </summary>
        /// <param name="args">Event arguments containing the interactor.</param>
        void EndHover(HoverExitEventArgs args)
        {
            m_HoveringInteractors.Remove(args.interactorObject);

            // Reset button height if no interactors are hovering
            if (m_HoveringInteractors.Count == 0)
            {
                if (m_ToggleButton && m_Toggled)
                    SetButtonHeight(-m_PressDistance);
                else
                    SetButtonHeight(0.0f);
            }
        }

        public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            base.ProcessInteractable(updatePhase);

            // Update the button press state during the dynamic update phase
            if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
            {
                if (m_HoveringInteractors.Count > 0)
                {
                    UpdatePress();
                }
            }
        }

        /// <summary>
        /// Updates the button press state based on the interactors' positions.
        /// </summary>
        void UpdatePress()
        {
            float minimumHeight = 0.0f;

            // Set minimum height based on toggle state
            if (m_ToggleButton && m_Toggled)
                minimumHeight = -m_PressDistance;

            // Process each interactor to determine press height
            foreach (var pressInfo in m_HoveringInteractors.Values)
            {
                var interactorTransform = pressInfo.m_Interactor.GetAttachTransform(this);
                var localOffset = transform.InverseTransformVector(interactorTransform.position - m_BaseButtonPosition);

                // Check if the interactor is within the button region
                bool withinButtonRegion = (Mathf.Abs(localOffset.x) < m_ButtonSize && Mathf.Abs(localOffset.z) < m_ButtonSize);
                if (withinButtonRegion)
                {
                    if (!pressInfo.m_InPressRegion)
                    {
                        pressInfo.m_WrongSide = (localOffset.y < m_ButtonOffset);
                    }

                    if (!pressInfo.m_WrongSide)
                        minimumHeight = Mathf.Min(minimumHeight, localOffset.y - m_ButtonOffset);
                }

                pressInfo.m_InPressRegion = withinButtonRegion;
            }

            // Apply the press buffer to the minimum height
            minimumHeight = Mathf.Max(minimumHeight, -(m_PressDistance + m_PressBuffer));

            // Determine if the button is pressed based on height
            bool pressed = m_ToggleButton ? (minimumHeight <= -(m_PressDistance + m_PressBuffer)) : (minimumHeight < -m_PressDistance);

            float currentDistance = Mathf.Max(0f, -minimumHeight - m_PressBuffer);
            m_Value = currentDistance / m_PressDistance;

            if (m_ToggleButton)
            {
                if (pressed)
                {
                    if (!m_Pressed)
                    {
                        m_Toggled = !m_Toggled;

                        if (m_Toggled)
                            m_OnPress.Invoke();
                        else
                            m_OnRelease.Invoke();
                    }
                }
            }
            else
            {
                if (pressed)
                {
                    if (!m_Pressed)
                        m_OnPress.Invoke();
                }
                else
                {
                    if (m_Pressed)
                        m_OnRelease.Invoke();
                }
            }
            m_Pressed = pressed;

            // Call the value change event if the button is pressed
            if (m_Pressed)
                m_OnValueChange.Invoke(m_Value);

            // Set the button height based on the calculated minimum height
            SetButtonHeight(minimumHeight);
        }

        /// <summary>
        /// Sets the height of the button.
        /// </summary>
        /// <param name="height">The height to set.</param>
        void SetButtonHeight(float height)
        {
            if (m_Button == null)
                return; // Return early if the button is not set

            Vector3 newPosition = m_Button.localPosition;
            newPosition.y = height;
            m_Button.localPosition = newPosition;
        }

        void OnDrawGizmosSelected()
        {
            // Draw a wireframe cube representing the button press area in the Scene view
            Vector3 pressStartPoint = Vector3.zero;

            if (m_Button != null)
            {
                pressStartPoint = m_Button.localPosition;
            }

            pressStartPoint.y += m_ButtonOffset - (m_PressDistance * 0.5f);

            Gizmos.color = Color.green;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(pressStartPoint, new Vector3(m_ButtonSize, m_PressDistance, m_ButtonSize));
        }

        void OnValidate()
        {
            // Update the button height when values are changed in the Inspector
            SetButtonHeight(0.0f);
        }
    }
}
