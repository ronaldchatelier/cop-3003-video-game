// Name: Ronald Chatelier
// Course: COP 3003
// file: CharacterController2D.cs

// Quality and Security: https://www.sonarsource.com/csharp/
// Style and Structure: https://google.github.io/styleguide/csharp-style.html

// Style and structure of the program is included with the naming rules,
// organization, and whitespace rules.
// Quality and security is the use of sonarsource, which will sync
// this project's repository on GitHub in order to identify
// the reliability, maintainability, and security of the program.

using UnityEngine;
using UnityEngine.Events;

public class CharacterController2D : MonoBehaviour
{
    // LO1c. Utilize an initialization list
    // Amount of force added when the player jumps.
    // [SerializeField]: Force Unity to serialize a private field.
    [SerializeField] private float m_JumpForce = 400f;

    // How much to smooth out the movement.
    [Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f;

    // Whether or not a player can steer while jumping.
    [SerializeField] private bool m_AirControl = false;

    // A position marking where to check for ceilings.
    [SerializeField] private Transform m_CeilingCheck;

    // A collider that will be disabled when crouching.
    // Collider2D: Parent class for collider types used with 2D gameplay.
    [SerializeField] private Collider2D m_CrouchDisableCollider;

    // A mask determining what is ground to the character.
    // LayerMask: Specifies Layers to use in a Physics.Raycast.
    // Physics.Raycast: Casts a ray, from point origin, in direction direction, of length maxDistance, against all colliders in the Scene.
    [SerializeField] private LayerMask m_WhatIsGround;

    // A position marking where to check if the player is grounded.
    // Transform: Position, rotation and scale of an object.
    [SerializeField] private Transform m_GroundCheck;

    // Radius of the overlap circle to determine if grounded.
    const float k_GroundedRadius = .2f;

    // Whether or not the player is grounded
    //LO6. Use object-oriented encapsulation mechanisms such as interfaces and private members.
    private bool m_Grounded;

    // Radius of the overlap circle to determine if the player can stand up.
    const float k_CeilingRadius = .2f;

    // Rigidbody2D: Rigidbody physics component for 2D sprites.
    private Rigidbody2D m_Rigidbody2D;

    // For determining which way the player is currently facing.
    private bool m_FacingRight = true;

    // Vector3: Representation of 3D vectors and points.
    private Vector3 m_Velocity = Vector3.zero;

    // Uses Events tab through Unity engine that's tied to an object (character, item, etc).
    [Header("Events")]

    // Space: The coordinate space in which to operate.
    [Space]

    // UnityEvent: A zero argument persistent callback that can be saved with the Scene.
    public UnityEvent OnLandEvent;

    // Serializable: Indicates that a class or a struct can be serialized.
    [System.Serializable]
    public class BoolEvent : UnityEvent<bool> { }

    public BoolEvent OnCrouchEvent;
    private bool m_wasCrouching = false;

    private void Awake()
    {
        // GetComponent: Returns the component of Type type if the game object has one attached, null if it doesn't.
        m_Rigidbody2D = GetComponent<Rigidbody2D>();

        if (OnLandEvent == null)
            OnLandEvent = new UnityEvent();

        if (OnCrouchEvent == null)

            // BoolEvent: Does the object exist.
            OnCrouchEvent = new BoolEvent();
    }

    private void FixedUpdate()
    {
        bool wasGrounded = m_Grounded;
        m_Grounded = false;

        // LO4. Include a comment in which you compare and contrast the procedural/functional approach and the object-oriented approach.
        // Functional approach compared with object-oriented approach is having the position and ground operating with Physics2D.
        // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground.
        // This can be done using layers instead but Sample Assets will not overwrite your project settings.
        // Physics2D.OverlapCircleAll: Get a list of all Colliders that fall within a circular area.  
        Collider2D[] colliders = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject != gameObject)
            {
                // If character is on the ground, then enable the ground event.
                m_Grounded = true;
                if (!wasGrounded)
                    OnLandEvent.Invoke();
            }
        }
    }

    private void Flip()
    {
        // Switch the way the player is labelled as facing.
        m_FacingRight = !m_FacingRight;

        // Multiply the player's x local scale by -1.
        // transform.localScale: The scale of the transform relative to the GameObjects parent.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    // LO1b.Overload a constructor
    public void Move(float move, bool crouch, bool jump)
    {
        // If crouching, check to see if the character can stand up.
        if (!crouch)
        {
            // If the character has a ceiling preventing them from standing up, keep them crouching.
            if (Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingRadius, m_WhatIsGround))
            {
                crouch = true;
            }
        }
        // Only control the player if grounded or airControl is turned on.
        if (m_Grounded || m_AirControl)
        {
            // If crouching.
            if (crouch)
            {
                if (!m_wasCrouching)
                {
                    m_wasCrouching = true;
                    OnCrouchEvent.Invoke(true);
                }

                // Disable one of the colliders when crouching.
                if (m_CrouchDisableCollider != null)
                    m_CrouchDisableCollider.enabled = false;
            }
            else
            {
                // Enable the collider when not crouching.
                if (m_CrouchDisableCollider != null)
                    m_CrouchDisableCollider.enabled = true;

                if (m_wasCrouching)
                {
                    m_wasCrouching = false;
                    OnCrouchEvent.Invoke(false);
                }
            }

            // Move the character by finding the target velocity.
            Vector3 targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.velocity.y);
            // And then smoothing it out and applying it to the character.
            m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);

            // If character is moving right or left.
            if (move > 0 && !m_FacingRight)
            {
                // Reverse controls.
                Flip();
            }
            // If character is moving left or right.
            else if (move < 0 && m_FacingRight)
            {
                // Reverse controls.
                Flip();
            }
        }
        // If the jump is inputed.
        if (m_Grounded && jump)
        {
            // Character gets vertical movement.
            // Rigidbody2D.AddForce: Apply a force to the rigidbody.
            m_Grounded = false;
            m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
        }
    }
}