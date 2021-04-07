using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Build;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using Random = UnityEngine.Random;

public class Combined_Controller : MonoBehaviour
{
    //Camera Position
    public Camera fpCam;
    public Camera tpCam;
    public bool camswicher;
    public GameObject standartcamPos;

    // Mouse Movement
    public float mouseSensitivity = 100f;
    float horizontalRotation = 0f;
    float verticalRotation = 0f;
    public float turnSmoothTime = 0.1f;
    public float turnSmoothVelocity;

    // Movement Speed
    public float movementSpeed = 5f;
    public float runningSpeed = 8f;
    private CharacterController controller;
    private ControllerColliderHit _contact;
    public Vector3 earthPull;
    public bool groundCheck;
    private float oldSpeed;
    public Vector3 movement;

    //Physics
    public float gravity;
    public float jumpheight;

    //Animations
    public Animator animator;

    //Steps
    public int randomstepSound;

    // Audio
    public AudioClip[] m_FootstepSounds;
    private AudioClip m_JumpSound;
    private AudioClip m_LandSound;
    public AudioSource audioSource;

    //Bobing
    public float bobSpeed = 14f;
    public float bobAmount = 0.05f;

    //float defaultPosY = 0;
    float timer = 0;

    //Heath & Falling
    public playerHealth playerHealth;
    public int maxHealth = 100;
    public int leftHealth;
    public int maxfallPos;
    public int fallDmg;
    private float fallPos;
    private float landPos;
    public bool fallMoment;

    //Screens
    public GameObject playscreen;
    public GameObject deathscreen;

    //Enemy Damage
    public EnemyAIMercy enemyAI;
    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null)
            throw new UnityException("No Character Controller");
    }

    void Start()
    {
        //defaultPosY = fpCam.transform.position.y;
        Cursor.lockState = CursorLockMode.Locked;
        camswicher = false;
        leftHealth = maxHealth;
        playerHealth.MaxHealth(maxHealth);
        fallMoment = true;
    }
    void Update()
    {
        StepSequence();
        //Changes the cameras by disabling and enabling them in the scene
        if (Input.GetKeyDown(KeyCode.V))
        {
            camswicher = !camswicher;
        }
        if (camswicher)
        {
            fpCam.gameObject.SetActive(false);
            tpCam.gameObject.SetActive(true);
            TPMovement();
        }
        else
        {
            fpCam.gameObject.SetActive(true);
            tpCam.gameObject.SetActive(false);
            FPMovement();
        }
        Movement();
        FallDamage();
        Death();
        EnemyDamage();
    }
    private void HealthBar(int damage)
    {
        //indication of damage via left helth decriment
        leftHealth -= damage;
        playerHealth.Health(leftHealth);
    }
    private void EnemyDamage()
    {
        if (enemyAI.isRunning)
        {
            HealthBar(enemyAI.damage);
        }
    }
    private void FallDamage()
    {
        //We check whether we are on the ground 
        if (!controller.isGrounded)
        {
            //if we check a bool that is active only for a frame and is initially set to TRUE
            if (fallMoment)
            {
                //store the y pos of the transform
                fallPos = transform.position.y;
                //stop checking it not to store any values after we’re not touching the ground
                fallMoment = false;
            }
        }
        // if not on ground
        else
        {
            //store the height of collision 
            landPos = transform.position.y;
            // if it is larger than publicly exposed maimum fall position without taking damage or is not a fall moment indicate damage
            //convert the ablolute of the difference into an integer as we might be dealing with negative positions
            if (Convert.ToInt32(Math.Abs(fallPos - landPos)) > maxfallPos && !fallMoment)
            {
                HealthBar(Convert.ToInt32(Math.Abs(fallPos - landPos)) * fallDmg);
            }
            // reset fall moment to repeat the process
            fallMoment = true;
        }
    }
    private void Death()
    {
        //death sequence
        if(leftHealth<=0)
        {
            Cursor.lockState = CursorLockMode.None;
            playscreen.SetActive(false);
            deathscreen.SetActive(true);
        }
    }
    void StepSequence()
    {
        //step sequence in a random range of int  
        if (controller.isGrounded && controller.velocity.magnitude > 1f)
        {
            randomstepSound = Random.Range(0, 4);
            audioSource.clip = m_FootstepSounds[randomstepSound];
            audioSource.Play();
        }
    }
    private void FPMovement()
    {
        //Standard unity imput system to define movement of the mouse. We multiply the input rotation by preset publicly exposed mouseSensitivity. 
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;//multiply by Time.deltaTime to make it framerate independent
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        //define rotation and the inverse of the input or otherwise we'll have a "plane mechanics" aka down is up and up is down.
        horizontalRotation -= mouseY;
        //direct input of rotation from the given position
        verticalRotation += mouseX;
        //clamp defines the rotation of the camera->head in the game, most of time head in reality does not spin in all 6 degrees of freedom so se have to clamp its rotation. 
        horizontalRotation = Mathf.Clamp(horizontalRotation, -90f, 90f);
        //we define horizontaland vertical rotations separately as if we defince them together we'll have a floating point errors or inprecisions and the camera would tilt each time we rotate it.
        fpCam.transform.localRotation = Quaternion.Euler(horizontalRotation, 0f, 0f);
        transform.localRotation = Quaternion.Euler(0f, verticalRotation, 0f);
        //standart inputs for movement 
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");

        ////On this step we define the ground check a bool that tells us whether we are on the ground or not, this is essential for falling and jumping

        //// First approach using the distance till the surface and the normal of the surface we're colliding with and check if the distance till the object is equal to the normal of the surface
        //// then we are colliding with it. 

        //void OnControllerColliderHit(ControllerColliderHit hit)
        //{

        //    if (controller.velocity.magnitude == hit.transform.forward.normalized.magnitude)
        //    {
        //        groundCheck = true;
        //    }
        //    else
        //    {
        //        groundCheck = false;
        //    }
        //}

        ////For instance an approach of direct calculation like the one below wouldn't work for a number of reasons.
        //// 1. The earthPull in defined through the froundcheck so that we fall only when we are NOT on the ground.
        //// 2. Even if the earthPull and the groundCheck were uncorrelated we would still face the problem of teleorting to the ground as the earthPull would grow continually. 
        //if (controller.velocity.y== 0)
        //{
        //    groundCheck = true;
        //}
        //else
        //{
        //    groundCheck = false;
        //}

        //// The approach of integrating the given CollisionFlags of the controller was the "easy way" problem is, that this approach is not working if the controller is not directly colliding the ground.
        //// Direct collision in space are inprecise and create floating point inprecisions destroing the unibility of this method
        //if (controller.collisionFlags == CollisionFlags.Below)
        //{
        //    groundCheck = true;
        //}
        //else
        //{
        //    groundCheck = false;
        //}

        //// The by fat most reasonable method is spherecast desctibed in details in the next sections.   
        //RaycastHit hit;
        //float distance = 0;
        //Vector3 capsuleBottom = transform.position-Vector3.up*controller.height/2;
        //// Cast a sphere wrapping character controller 10 meters forward
        //// to see if it is about to hit anything.
        //if (Physics.SphereCast(capsuleBottom, controller.radius, Vector3.down, out hit, 2))
        //{
        //    print(hit.distance);
        //    if(hit.distance<=0.01)
        //    {
        //        groundCheck = true;      
        //    }
        //    else
        //    {
        //        groundCheck = false;
        //    }
        //}
        //Debug.DrawRay(capsuleBottom, Vector3.down, Color.green);

        // Integrated Unity groundChecker
        groundCheck = controller.isGrounded;

        if (groundCheck && earthPull.y < 0)
        {
            earthPull.y = 0f;
        }
        //the movement is defined as x/2 as walking to sides is at least twice as complicated as walking forward.
        movement = transform.right * x / 2 + transform.forward * y;
        //calling an incapsulated function
        HeadBobbing();
        //movement of the controller defined by a publicly exposed variable of movementSpeed and multiplied by Time.deltaTime to framerate unrelated
        controller.Move(movement * movementSpeed * Time.deltaTime);

        //Basic jumping that checks as one of its conditions whether the player is on the ground or not.
        if (Input.GetKeyDown(KeyCode.Space) && groundCheck)
        {
            earthPull.y = Mathf.Sqrt(jumpheight * -2f * gravity);
        }
        //using the standard formula for gracity m/s^2
        earthPull.y += gravity * Time.deltaTime; //analog to m/s
        controller.Move(earthPull * Time.deltaTime);//analog to m/s^2, multiply by deltatime to make it framerate disconnected
    }
    //Bob the head to create the illusion of walking
    private void HeadBobbing()
    {
        Vector3 defultPos = fpCam.transform.position;
        //HeadBobCurve
        //Checks whether we are walking or not
        if (movement.magnitude >= 0.1f)
        {
            //Moving
            //sets the timer that counts the delta time * bobSpeed to modify the position of the camera based on time and the value of sin
            timer += Time.deltaTime * bobSpeed;
            //set the position of x and z to the original position of the camera and the defult Pos to the sin of timer that gradually moves along the sinusoid and multilpier bobAmount to define the shaking 
            fpCam.transform.position = new Vector3(fpCam.transform.position.x, defultPos.y + Mathf.Sin(timer) * bobAmount, fpCam.transform.position.z);
        }
        else
        {
            //if we're not moving than the value of timer is returned to it's original value of the standard cam position. 
            //Idle
            timer = 0;
            fpCam.transform.position = standartcamPos.transform.position;
        }
    }
    private void TPMovement()
    {
        //Standard Unity user input for movement
        //Vector 3 defining the movement and normalizing it so that we don't go faster when we press two keys at the same time
        Vector3 moveVec = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical")).normalized;
        //checks if we're pressing any of the keys
        if (moveVec.magnitude >= 0.1f)
        {//Atan2  returns the angle between the x - axis and the vector starting at zero and terminating at x,y.
            float perspective = Mathf.Atan2(moveVec.x, moveVec.z) * Mathf.Rad2Deg+tpCam.transform.eulerAngles.y;
            //rotation smoothing of the turning position based on the look postition to not shift momentarely
            float rotationSmooth = Mathf.SmoothDampAngle(transform.eulerAngles.y, perspective, ref turnSmoothVelocity, turnSmoothTime);
            //definition of rotation along y axis
            transform.rotation = Quaternion.Euler(0f, rotationSmooth, 0f);
            //movement along lookforward
            Vector3 moveDir = Quaternion.Euler(0f, perspective, 0f)*Vector3.forward;
            //normalized movement not to go faster when moving to sides * the predefined movement speed * deltatime to make framerate independent
            controller.Move(moveDir.normalized * movementSpeed * Time.deltaTime);
        }
        // the solution is implemented using notes from Unity in Action Multiplatform game development in C by Joe Hocking
        //Advanced groundcheck using a combination of raycasting and integrated normal check 
        groundCheck = false;
        //casting a ray and calculating the goundcheck on
        RaycastHit hit;
        //casting a ray only when our vertical acceletration is negative
        if (earthPull.y<0 && Physics.Raycast(transform.position, Vector3.down, out hit))
        {   //dividing the sum of the controller height and radius by 1.9 instead of 2 for the position of the check to be slightly below the capsule.
            float check = (controller.height + controller.radius)/ 1.9f;
            //ground ground check is based on the distance between the set point and the ground
            groundCheck = hit.distance <= check;
        }
        //if the ray indicares that we are on the ground than we can jump
        //this would universally work as the indication of the surface below our capsule is the prime condition for a ground check
        if(groundCheck)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                earthPull.y = Mathf.Sqrt(jumpheight * -2f * gravity);
            }
        }
        //if the ray in not hitting an object within check
        //btw using check as the condition is a bit of an overkill as we could have just dinamically chaned the length of the ray
        else
        {
            earthPull.y += gravity * Time.deltaTime;

            // movement vector’s facing the point of collision determined using the dot product
            if (controller.isGrounded)
            {
                //nullifiing the earhpull if we are not on the ground yet the collider is hitting an object
                if (earthPull.y < 0)
                {
                    earthPull.y = 0;
                }
                //if our movement vectors and contact vectors are facing different directions then
                if (Vector3.Dot(moveVec, _contact.normal) < 0)
                {
                    //using _contact.normal  finding the normal of contact surface and multiply it by movement speed to 
                    moveVec = _contact.normal * movementSpeed;
                }
                else
                {
                    //if thevectors are facing the same direction we continually slide of the normal of the surface below
                    moveVec += _contact.normal * movementSpeed;
                }
            }
        }
        //horizontal movement of a capsule defined by the eathbull
        moveVec.y = earthPull.y;
        //standard formula of free-falling 
        controller.Move(earthPull * Time.deltaTime);
    }
    //definig hit as the collision of the capsule
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        _contact = hit;
    }
    
    private void Movement()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            animator.SetFloat("Forward", 0.55f);
        }
        else if (Input.GetKeyUp(KeyCode.W))
        {
            animator.SetFloat("Forward", 0f);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            animator.SetFloat("Forward", -1f);
        }
        else if (Input.GetKeyUp(KeyCode.S))
        {
            animator.SetFloat("Forward", 0f);
        }
        if (Input.GetKeyDown(KeyCode.A))
        {
            animator.SetFloat("Sides", -1f);
        }
        else if (Input.GetKeyUp(KeyCode.A))
        {
            animator.SetFloat("Sides", 0f);
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            animator.SetFloat("Sides", 1f);
        }
        else if (Input.GetKeyUp(KeyCode.D))
        {
            animator.SetFloat("Sides", 0f);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            animator.SetBool("Jump", true);
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            animator.SetBool("Jump", false);
        }
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            oldSpeed = movementSpeed;
            movementSpeed = runningSpeed;
            animator.SetFloat("Forward", 0.78f);
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            movementSpeed = oldSpeed;
            animator.SetFloat("Forward", 0.55f);
        }
    }
}
