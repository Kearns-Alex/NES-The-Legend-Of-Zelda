using ActionCode.ColorPalettes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private sbyte walkSpeed = 4;

    private Animator animator;
    private ColorPaletteSwapperCycle swapper;

    // movement variables
    private float xAxis;
    private float yAxis;
    private Rigidbody2D rb2d;
    private bool isItemPressed;
    private bool isUsingItem;
    private bool isAttackPressed;
    private bool isAttacking;
    private string currentAnimaton;

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
        swapper = GetComponent<ColorPaletteSwapperCycle>();
        
        // AJK: Maybe check here to see if we have the ring(s) attached to do the palette swap
        // Red ring trumps blue, blue = 1/2 red = 1/4
    }

    //=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
    // Used for Input Checks [called once per frame]
    //=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
    private void Update()
    {
        // goes directly to 1. No accelleration 
        xAxis = Input.GetAxisRaw("Horizontal");
        yAxis = Input.GetAxisRaw("Vertical");

        // [2] 'A' key pressed
        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            isAttackPressed = true;
        }
        else
        // [1] 'B' key pressed
        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            isItemPressed = true;
        }

        if (Input.GetKeyDown(KeyCode.C)) swapper.SwapPalette();
    }

    //=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
    // Physics based time step loop [executing physics and movements]
    //=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
    void FixedUpdate()
    {
        // check for two keys being pressed at once
        CheckDiagonal();

        // check our X movement
        CheckXMovement();

        // check our Y movement
        CheckYMovement();

        // check for attack
        CheckAttackPress();

        // check for item use
        CheckItemPress();


        //=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
        // By now, our xAxis and yAxis should be finilized 
        // and able to move into any final animations/player movement.
        //=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=


        // change animation
        UpdateAnimations();

        // finally move our character
        MoveCharacter(new Vector2(xAxis, yAxis));
    }

    void UpdateAnimations()
    {
        if (xAxis != 0)
        {
            ChangeAnimationState(PLAYER_X_MOVE);
            wasMovingUP = false;
            wasMovingDOWN = false;
        }
        else if (yAxis != 0)
        {
            if (yAxis < 0)
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

    void CheckDiagonal()
    {
        // check to see what keys are currently pressed
        bool isMovingHorizontal = (Mathf.Abs(xAxis) > 0);
        bool isMovingVertical = (Mathf.Abs(yAxis) > 0);

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
    }

    void CheckXMovement()
    {
        if (xAxis < 0 && !isAttacking && !isUsingItem)
        {
            // flip the animation for the direction we are headed
            transform.localScale = new Vector2(-1, 1);
            xAxis = -walkSpeed;
        }
        else if (0 < xAxis && !isAttacking && !isUsingItem)
        {
            // flip the animation for the direction we are headed
            transform.localScale = new Vector2(1, 1);
            xAxis = walkSpeed;
        }
        else
        {
            xAxis = 0;
        }
    }

    void CheckYMovement()
    {
        if (yAxis < 0 && !isAttacking && !isUsingItem)
        {
            // flip the animation for the direction we are headed
            transform.localScale = new Vector2(1, 1);
            yAxis = -walkSpeed;
            wasMovingUP = false;
            wasMovingDOWN = true;
        }
        else if (0 < yAxis && !isAttacking && !isUsingItem)
        {
            // flip the animation for the direction we are headed
            transform.localScale = new Vector2(1, 1);
            yAxis = walkSpeed;
            wasMovingUP = true;
            wasMovingDOWN = false;
        }
        else
        {
            yAxis = 0;
        }
    }

    void CheckAttackPress()
    {
        if (isAttackPressed && !isUsingItem)
        {
            isAttackPressed = false;

            // set both movements to 0. We do not allow attacking and moving
            xAxis = 0;
            yAxis = 0;

            if (!isAttacking)
            {
                isAttacking = true;
                //AJK: WILL NEED TO SEE WHAT SHIELD/SWORD IS EQUIPED

                //ChangeAnimationState(PLAYER_ATTACK);

                Invoke("AttackComplete", animator.GetCurrentAnimatorStateInfo(0).length);
            }
        }
    }

    void CheckItemPress()
    {
        // AJK: TO BE ADDED AT A LATER DATE
        if (isItemPressed && !isAttacking)
        {
            isItemPressed = false;

            // set both movements to 0. We do not allow attacking and moving
            xAxis = 0;
            yAxis = 0;

            if (!isUsingItem)
            {
                isUsingItem = true;

                //AJK: FIND OUT WHAT ITEM IS BEING USED IN B
                //ChangeAnimationState(PLAYER_ATTACK);

                Invoke("AttackComplete", animator.GetCurrentAnimatorStateInfo(0).length);
            }
        }
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