#define LOGS
#define DEBUG

using ActionCode.ColorPalettes;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public enum PlayerState
{
    WALK,
    ATTACK,
    ITEM,
    INTERACT
}

public enum Tunic
{
    GREEN,
    BLUE,
    RED,
    DAMAGE_1,
    DAMAGE_2,
    DAMAGE_3,
    DAMAGE_4
}

public class PlayerScript : MonoBehaviour
{
    public PlayerState currentState;
    [SerializeField]
    private sbyte walkSpeed = 6;
    private float damageMultiplier = 1;
    [SerializeField]
    private byte damageFlash = 3;
    [SerializeField]
    private float damageFlashTime = 0.05f;

    private Animator animator;
    private ColorPaletteSwapperCycle swapper;

    // movement variables
    private float xAxis;
    private float yAxis;
    private Rigidbody2D rb2d;
    private string currentAnimaton;

    // item variable
    [SerializeField]
    public Tunic currentTunic;
    private bool hasBlueRing = false;
    private bool hasRedRing = false;
    private bool hasBigShield = false;


    // animation states 2.0
    // axis movement
    const string PICKUP = "PICKUP";
    const string UP = "UP";
    const string DOWN = "DOWN";
    const string HORIZONTAL = "X";

    // states
    const string IDLE = "_IDLE";
    const string MOVE = "_MOVE";
    const string ATTACK = "_ATTACK";

    // shield type
    const string SMALL = "_s";
    const string BIG = "_S";

    private bool wasMovingVertical = false;
    private bool wasMovingUP = false;
    private bool wasMovingDOWN = true;

    // Start is called before the first frame update
    void Start()
    {
        currentState = PlayerState.WALK;
        rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        swapper = GetComponent<ColorPaletteSwapperCycle>();

        RingCheck();
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
        if (Input.GetButtonDown("Attack") && currentState == PlayerState.WALK)
        {
            //isAttackPressed = true;
            StartCoroutine(AttackCo());
        }
        // [1] 'B' key pressed
        else if (Input.GetButtonDown("Item") && currentState == PlayerState.WALK)
        {
            //isItemPressed = true;
            StartCoroutine(ItemCo());
        }

#if DEBUG
        if (Input.GetKeyDown(KeyCode.Z)) hasBigShield = !hasBigShield;

        if (Input.GetKeyDown(KeyCode.X)) hasBlueRing = !hasBlueRing;

        if (Input.GetKeyDown(KeyCode.C)) hasRedRing = !hasRedRing;

        if (Input.GetKeyDown(KeyCode.V)) RingCheck();

        if (Input.GetKeyDown(KeyCode.B)) StartCoroutine(DamageTaken());
#endif
    }

    //=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
    // Physics based time step loop [executing physics and movements]
    //=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
    void FixedUpdate()
    {
        if(currentState != PlayerState.WALK)
        {
            return;
        }
        // check for two keys being pressed at once
        CheckDiagonal();
        // check our X movement
        CheckXMovement();
        // check our Y movement
        CheckYMovement();


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
        string nextAnimation = "";
        if (xAxis != 0 || yAxis != 0)
        {
            nextAnimation = XYCheck(MOVE, true);
        }
        else
        {
            nextAnimation = XYCheck(IDLE, true);
        }

#if LOGS
        print(nextAnimation);
#endif

        //AJK: will only have one ChangeAnimationState here once the string is built
        ChangeAnimationState(nextAnimation);
    }

    // Allows other "outside sources" to move the character if needed.
    void MoveCharacter(Vector2 velocity)
    {
        rb2d.velocity = velocity;
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
                wasMovingUP = false;
                wasMovingDOWN = false;
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
            wasMovingUP = false;
            wasMovingDOWN = false;
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
        if (xAxis < 0)
        {
            // flip the animation for the direction we are headed
            transform.localScale = new Vector2(-1, 1);
            xAxis = -walkSpeed;
        }
        else if (0 < xAxis)
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
        if (yAxis < 0)
        {
            // flip the animation for the direction we are headed
            transform.localScale = new Vector2(1, 1);
            yAxis = -walkSpeed;
            wasMovingUP = false;
            wasMovingDOWN = true;
        }
        else if (0 < yAxis)
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

    void ChangeAnimationState(string newAnimation)
    {
        // stop the same animation from interrupting itslef
        if (currentAnimaton == newAnimation) return;

        // play the animation
        animator.Play(newAnimation);

        // reassign the current state
        currentAnimaton = newAnimation;
    }

    private IEnumerator AttackCo()
    {
        string nextAnimation = XYCheck(ATTACK, false);

        // make sure the character is not moving
        MoveCharacter(new Vector2(0, 0));
        currentState = PlayerState.ATTACK;

        //AJK: WILL NEED TO SEE WHAT SWORD IS EQUIPED for which one to draw
        ChangeAnimationState(nextAnimation);
        yield return new WaitForEndOfFrame();

        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        currentState = PlayerState.WALK;
    }
    private IEnumerator ItemCo()
    {
        string nextAnimation = XYCheck(ATTACK, false);

        // make sure the character is not moving
        MoveCharacter(new Vector2(0, 0));
        currentState = PlayerState.ITEM;

        //AJK: WILL NEED TO SEE WHAT ITEM IS EQUIPED for which one to draw
#if DEBUG
        ChangeAnimationState(PICKUP);
#else
        ChangeAnimationState(nextAnimation);
#endif
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        currentState = PlayerState.WALK;
    }
    private IEnumerator DamageTaken()
    {
        for (int i = 0; i < damageFlash; i++)
        {
            swapper.SwapPalette((int)(Tunic.DAMAGE_1));
            yield return new WaitForSeconds(damageFlashTime);

            swapper.SwapPalette((int)(Tunic.DAMAGE_2));
            yield return new WaitForSeconds(damageFlashTime);

            swapper.SwapPalette((int)(Tunic.DAMAGE_3));
            yield return new WaitForSeconds(damageFlashTime);
        }
        
        swapper.SwapPalette((int)(currentTunic));
    }

    private string XYCheck(string action, bool doShieldCheck)
    {
        if (wasMovingUP)
        {
            return UP + action;
        }
        else if (wasMovingDOWN)
        {
            return DOWN + action + ShieldCheck(doShieldCheck);
        }
        else
        {
            return HORIZONTAL + action + ShieldCheck(doShieldCheck);
        }
    }
    private string ShieldCheck(bool doCheck)
    {
        if(doCheck == false)
        {
            return "";
        }
        else if (hasBigShield)
        {
            return BIG;
        }
        else
        {
            return SMALL;
        }
    }
    private void RingCheck()
    {
        // AJK: Maybe check here to see if we have the ring(s) attached to do the palette swap
        // Red ring trumps blue, blue = 1/2 red = 1/4
        if (hasRedRing && hasBlueRing)
        {
            damageMultiplier = .25f;
            currentTunic = Tunic.RED;
        }
        else if (hasRedRing)
        {
            damageMultiplier = .5f;
            currentTunic = Tunic.RED;
        }
        else if (hasBlueRing)
        {
            damageMultiplier = .5f;
            currentTunic = Tunic.BLUE;
        }
        else
        {
            currentTunic = Tunic.GREEN;
        }

        swapper.SwapPalette((int) (currentTunic));
    }
}