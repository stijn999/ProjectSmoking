using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

namespace UnityEngine.XR.Content.Interaction
{
    /// <summary>
    /// An interactable button that can be pressed by a direct interactor in VR.
    /// </summary>
    public class XRGripButton : XRBaseInteractable
    {
        [SerializeField]
        [Tooltip("The object that visually represents the button being pressed.")]
        private Transform m_Button = null; // The visual representation of the button

        [SerializeField]
        [Tooltip("The distance the button can be pressed.")]
        private float m_PressDistance = 0.1f; // Maximum distance the button can be pressed down

        [SerializeField]
        [Tooltip("Treat this button like an on/off toggle.")]
        private bool m_ToggleButton = false; // Whether the button should act as a toggle (on/off)

        [SerializeField]
        [Tooltip("Events to trigger when the button is pressed.")]
        private UnityEvent m_OnPress; // Event triggered when the button is pressed

        [SerializeField]
        [Tooltip("Events to trigger when the button is released.")]
        private UnityEvent m_OnRelease; // Event triggered when the button is released

        private bool m_Hovered = false; // Whether the button is currently hovered over
        private bool m_Selected = false; // Whether the button is currently selected (pressed)
        private bool m_Toggled = false; // Whether the button is currently toggled on

        /// <summary>
        /// The object that visually represents the button being pressed.
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
        /// Events to trigger when the button is pressed.
        /// </summary>
        public UnityEvent onPress => m_OnPress;

        /// <summary>
        /// Events to trigger when the button is released.
        /// </summary>
        public UnityEvent onRelease => m_OnRelease;

        private void Start()
        {
            // Initialize the button's position to its default state
            SetButtonHeight(0.0f);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            // Add listeners based on whether the button is a toggle or not
            if (m_ToggleButton)
                selectEntered.AddListener(StartTogglePress);
            else
            {
                selectEntered.AddListener(StartPress);
                selectExited.AddListener(EndPress);
                hoverEntered.AddListener(StartHover);
                hoverExited.AddListener(EndHover);
            }
        }

        protected override void OnDisable()
        {
            // Remove listeners when the button is disabled
            if (m_ToggleButton)
                selectEntered.RemoveListener(StartTogglePress);
            else
            {
                selectEntered.RemoveListener(StartPress);
                selectExited.RemoveListener(EndPress);
                hoverEntered.RemoveListener(StartHover);
                hoverExited.RemoveListener(EndHover);
                base.OnDisable();
            }
        }

        private void StartTogglePress(SelectEnterEventArgs args)
        {
            // Toggle the button state and invoke the appropriate event
            m_Toggled = !m_Toggled;

            if (m_Toggled)
            {
                SetButtonHeight(-m_PressDistance);
                m_OnPress.Invoke();
            }
            else
            {
                SetButtonHeight(0.0f);
                m_OnRelease.Invoke();
            }
        }

        private void StartPress(SelectEnterEventArgs args)
        {
            // Handle the start of a button press
            SetButtonHeight(-m_PressDistance);
            m_OnPress.Invoke();
            m_Selected = true;
        }

        private void EndPress(SelectExitEventArgs args)
        {
            // Handle the end of a button press
            if (m_Hovered)
                m_OnRelease.Invoke();

            SetButtonHeight(0.0f);
            m_Selected = false;
        }

        private void StartHover(HoverEnterEventArgs args)
        {
            // Handle the start of a button hover
            m_Hovered = true;
            if (m_Selected)
                SetButtonHeight(-m_PressDistance);
        }

        private void EndHover(HoverExitEventArgs args)
        {
            // Handle the end of a button hover
            m_Hovered = false;
            SetButtonHeight(0.0f);
        }

        private void SetButtonHeight(float height)
        {
            // Update the vertical position of the button to simulate pressing
            if (m_Button == null)
                return;

            Vector3 newPosition = m_Button.localPosition;
            newPosition.y = height;
            m_Button.localPosition = newPosition;
        }

        private void OnDrawGizmosSelected()
        {
            // Visualize the button press distance in the Scene view
            var pressStartPoint = transform.position;
            var pressDownDirection = -transform.up;

            if (m_Button != null)
            {
                pressStartPoint = m_Button.position;
                pressDownDirection = -m_Button.up;
            }

            Gizmos.color = Color.green;
            Gizmos.DrawLine(pressStartPoint, pressStartPoint + (pressDownDirection * m_PressDistance));
        }

        private void OnValidate()
        {
            // Ensure button height is reset when values are validated in the editor
            SetButtonHeight(0.0f);
        }
    }
}
