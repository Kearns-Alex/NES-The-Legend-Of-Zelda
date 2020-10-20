using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private sbyte walkSpeed = 4;

    private Animator animator;

    // movement variables
    private Vector3 change;
    private float xAxis;
    private float yAxis;
    private Rigidbody2D rb2d;
    private bool isItemPressed;
    private bool isUsingItem;
    private bool isAttackPressed;
    private bool isAttacking;
    private string currentAnimaton;
    [SerializeField]
    private float attackDelay;

    // animation states
    const string PLAYER_X_IDLE = "Player_X_idle";
    const string PLAYER_X_MOVE = "Player_X_move";
    const string PLAYER_X_ATTACK = "Player_X_attack";

    const string PLAYER_UP_IDLE = "Player_UP_idle";
    const string PLAYER_UP_MOVE = "Player_UP_move";
    const string PLAYER_UP_ATTACK = "Player_UP_attack";

    const string PLAYER_DOWN_IDLE = "Player_DOWN_idle";
    const string PLAYER_DOWN_MOVE = "Player_DOWN_move";
    const string PLAYER_DOWN_ATTACK = "Player_DOWN_attack";

    private bool wasMovingVertical = false;
    private bool wasMovingUP = false;
    private bool wasMovingDOWN = true;

    // Start is called before the first frame update
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    //=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
    // Used for Input Checks [called once per frame]
    //=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
    private void Update()
    {
        // goes directly to 1. No accelleration 
        xAxis = Input.GetAxisRaw("Horizontal");
        yAxis = Input.GetAxisRaw("Vertical");

        // [1] 'B' key pressed
        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            isItemPressed = true;
        }

        // [2] 'A' key pressed
        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            isAttackPressed = true;
        }
    }

    //=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
    // Physics based time step loop [executing physics and movements]
    //=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
    void FixedUpdate()
    {
        // check first for a double key press (diagonals)
        bool isMovingHorizontal = Mathf.Abs(xAxis) > 0;
        bool isMovingVertical = Mathf.Abs(yAxis) > 0;

        // check for diagonal movement
        if (isMovingVertical && isMovingHorizontal)
        {
            //moving in both directions, prioritize the first
            if (wasMovingVertical)
            {
                yAxis = 0;
            }
            else
            {
                xAxis = 0;
            }
        }
        else if (isMovingHorizontal)
        {
            wasMovingVertical = false;
            yAxis = 0;
        }
        else if (isMovingVertical)
        {
            wasMovingVertical = true;
            xAxis = 0;
        }

        // by now, our xAxis and yAxis have been finilized in regards to movement input
        Vector2 vel = new Vector2(0, 0);

        // check our X movement
        if (xAxis < 0 && !isAttacking)
        {
            // flip the animation for the direction we are headed
            transform.localScale = new Vector2(-1, 1);
            vel.x = -walkSpeed;
        }
        else if (0 < xAxis && !isAttacking)
        {
            // flip the animation for the direction we are headed
            transform.localScale = new Vector2(1, 1);
            vel.x = walkSpeed;
        }
        else
        {
            vel.x = 0;
        }

        // check our Y movement
        if (yAxis < 0 && !isAttacking)
        {
            // flip the animation for the direction we are headed
            transform.localScale = new Vector2(1, 1);
            vel.y = -walkSpeed;
            wasMovingUP = false;
            wasMovingDOWN = true;
        }
        else if (0 < yAxis && !isAttacking)
        {
            // flip the animation for the direction we are headed
            transform.localScale = new Vector2(1, 1);
            vel.y = walkSpeed;
            wasMovingUP = true;
            wasMovingDOWN = false;
        }
        else
        {
            vel.y = 0;
        }

        // check for attack
        if (isAttackPressed)
        {
            isAttackPressed = false;

            // set both movements to 0. We do not allow attacking and moving
            vel.x = 0;
            vel.y = 0;

            if (!isAttacking)
            {
                isAttacking = true;
                //AJK: WILL NEED TO SEE WHAT SHIELD/SWORD IS EQUIPED

                //ChangeAnimationState(PLAYER_ATTACK);

                Invoke("AttackComplete", animator.GetCurrentAnimatorStateInfo(0).length);
            }
        }

        // check for item use
        // AJK: TO BE ADDED AT A LATER DATE
        // check for attack
        if (isItemPressed)
        {
            isItemPressed = false;

            // set both movements to 0. We do not allow attacking and moving
            vel.x = 0;
            vel.y = 0;

            if (!isUsingItem)
            {
                isUsingItem = true;

                //AJK: FIND OUT WHAT ITEM IS BEING USED IN B
                //ChangeAnimationState(PLAYER_ATTACK);

                Invoke("AttackComplete", animator.GetCurrentAnimatorStateInfo(0).length);
            }
        }

        //change animation
        if (xAxis != 0)
        {
            ChangeAnimationState(PLAYER_X_MOVE);
            wasMovingUP = false;
            wasMovingDOWN = false;
        }
        else if(yAxis != 0)
        {
            if(yAxis < 0)
            {
                ChangeAnimationState(PLAYER_DOWN_MOVE);
            }
            else
            {
                ChangeAnimationState(PLAYER_UP_MOVE);
            }
        }
        else if (wasMovingUP == true)
        {
            ChangeAnimationState(PLAYER_UP_IDLE);
        }
        else if (wasMovingDOWN == true)
        {
            ChangeAnimationState(PLAYER_DOWN_IDLE);
        }
        else if (!isAttacking)
        {
            ChangeAnimationState(PLAYER_X_IDLE);
        }

        // finally move our character
        MoveCharacter(vel);
    }

    void UpdateAnimationAndMove()
    {
        bool isMovingHorizontal = Mathf.Abs(change.x) > 0;
        bool isMovingVertical = Mathf.Abs(change.y) > 0;

        // check for diagonal movement
        if (isMovingVertical && isMovingHorizontal)
        {
            //moving in both directions, prioritize later
            if (wasMovingVertical)
            {
                change.y = 0;
            }
            else
            {
                change.x = 0;
            }
        }
        else if (isMovingHorizontal)
        {
            wasMovingVertical = false;
        }
        else if (isMovingVertical)
        {
            wasMovingVertical = true;
        }

        // move character if we have speed to do so
        if(change != Vector3.zero)
        {
            MoveCharacter(change);
            //ChangeAnimationState(PLAYER_MOVE);
        }
        else
        {
            //ChangeAnimationState(PLAYER_MOVE);
        }
    }

    // Allows other "outside sources" to move the character if needed.
    void MoveCharacter(Vector2 velocity)
    {
        // this line of code was making weird things happen such as the demo in unity being choppy and slow 
        //  compared to the build application being smooth and quicker. The following line seems to have
        //  fixed these issues...but the second line will cause jittering
        // SOLVED: replace 'Update()' with 'FixedUpdate()' and this line of code works perfectly
        rb2d.velocity = velocity;
        //transform.position = transform.position + delta * speed * Time.deltaTime;
    }

    void AttackComplete()
    {
        isAttacking = false;
        isUsingItem = false;
    }

    void ChangeAnimationState(string newAnimation)
    {
        // stop the same animation from interrupting itslef
        if (currentAnimaton == newAnimation) return;

        // play the animation
        animator.Play(newAnimation);

        // reassign the current state
        currentAnimaton = newAnimation;
    }
}
