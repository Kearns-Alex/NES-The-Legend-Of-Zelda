#define LOGS
#define DEBUG

using ActionCode.ColorPalettes;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

#if DEBUG
using UnityEngine.Assertions;
#endif

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
    private int currentHealth = 3;
    private int maxHealth = 13;
    private bool hasMaxHealth = false;

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

    Object swordRef;
    public Sword currentSword;

    //public Item currentItem;


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

    private int lookingHorizontal = 0;
    private int lookingVertical = -1;

    // Start is called before the first frame update
    void Start()
    {
        swordRef = Resources.Load("Sword");
        currentSword = Sword.NONE;
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

        // action key is pressed
        if ((Input.GetButtonDown("Attack") || Input.GetButtonDown("Item")) 
            && currentState == PlayerState.WALK) 
            StartCoroutine(ActionCo());
        
#if DEBUG
        if (Input.GetKeyDown(KeyCode.Z)) hasBigShield = !hasBigShield;
        if (Input.GetKeyDown(KeyCode.X)) hasBlueRing = !hasBlueRing;
        if (Input.GetKeyDown(KeyCode.C)) hasRedRing = !hasRedRing;
        if (Input.GetKeyDown(KeyCode.V)) RingCheck();
        if (Input.GetKeyDown(KeyCode.B)) StartCoroutine(DamageTaken());
        if (Input.GetKeyDown(KeyCode.N)) if ((System.Enum.GetNames(typeof(Sword)).Length - 1) < (int)++currentSword) currentSword = Sword.NONE;
#endif
    }

    //=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
    // Physics based time step loop [executing physics and movements]
    //=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=
    void FixedUpdate()
    {
        if(currentState != PlayerState.WALK) return;

        // check for two keys being pressed at once
        CheckDiagonal();


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
            // X MOVEMENT
            if (wasMovingVertical) UpdateXMovement();
            // Y MOVEMENT
            else UpdateYMovement();
        }
        else if (isMovingHorizontal)
        {
            // X MOVEMENT
            wasMovingVertical = false;
            UpdateXMovement();
        }
        else if (isMovingVertical)
        {
            // Y MOVEMENT
            wasMovingVertical = true;
            UpdateYMovement();
        }
    }
    void UpdateXMovement()
    {
        lookingVertical = 0;
        lookingHorizontal = Mathf.Clamp((int)xAxis, -1, 1);
        yAxis = 0;
#if DEBUG
        Assert.AreNotEqual(0, (int)lookingHorizontal);
#endif
        transform.localScale = new Vector2((1 * lookingHorizontal), 1);
        xAxis = (walkSpeed * lookingHorizontal);
    }
    void UpdateYMovement()
    {
        lookingHorizontal = 0;
        lookingVertical = Mathf.Clamp((int)yAxis, -1, 1);
        xAxis = 0;
        transform.localScale = new Vector2(1, 1);
        yAxis = (walkSpeed * lookingVertical);
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

    private IEnumerator ActionCo()
    {
        string nextAnimation = XYCheck(ATTACK, false);

        // make sure the character is not moving
        MoveCharacter(new Vector2(0, 0));

        // [2] 'A' key pressed
        if (Input.GetButtonDown("Attack"))
        {
            currentState = PlayerState.ATTACK;
            if (currentSword != Sword.NONE) SwordLogic();
        }
        // [1] 'B' key pressed
        else
        {
            currentState = PlayerState.ITEM;
            /*if (currentItem != Item.NONE)*/ ItemLogic();
#if DEBUG
            nextAnimation = PICKUP;
#endif
        }

        ChangeAnimationState(nextAnimation);

        yield return new WaitForEndOfFrame();

        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        currentState = PlayerState.WALK;
    }

    private void SwordLogic()
    {
        GameObject sword = (GameObject)Instantiate(swordRef);
        sword.transform.position = new Vector3(transform.position.x, transform.position.y, 0);
        sword.GetComponent<SwordScript>().Swing(currentSword, lookingHorizontal, lookingVertical, hasMaxHealth);
    }
    private void ItemLogic()
    {

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
        //if (wasMovingUP)
        if (0 < lookingVertical)
        {
            return UP + action;
        }
        //else if (wasMovingDOWN)
        else if (lookingVertical < 0)
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