using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Sword
{
    NONE,
    WOOD,
    WHITE,
    MAGIC
}

public class SwordScript : MonoBehaviour
{

    private Vector3 targetPosition;
    private Vector3 originalPosition;
    private bool willShoot = false;
    private bool isBeam = false;
    private bool isDiagonal = false;

    Object swordRef;
    private int lookingHorizontal = 0;
    private int lookingVertical = -1;

    private Animator animator;
    private Rigidbody2D rb2d;
    private BoxCollider2D bc2d;

    // Start is called before the first frame update
    void Start()
    {

    }

    private void init()
    {
        swordRef = Resources.Load("Sword");
        animator = GetComponent<Animator>();
        bc2d     = GetComponent<BoxCollider2D>();
        rb2d     = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(isBeam || isDiagonal) return;

        if (transform.position != targetPosition)
        {
            // move to a sinle place
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, .2f);
        }
        else if (transform.position == targetPosition && willShoot)
        {
            GameObject sword = (GameObject)Instantiate(swordRef);
            sword.transform.position = new Vector3(transform.position.x, transform.position.y, 0);
            sword.GetComponent<SwordScript>().Shoot(lookingHorizontal, lookingVertical);

            willShoot = false;
        }
        else if (transform.position == targetPosition && targetPosition != originalPosition)
        {
            targetPosition = originalPosition;
        }
        else if (transform.position == targetPosition && targetPosition == originalPosition)
        {
            DestroyYourself();
        }
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isBeam) return;

        if (collision.CompareTag("Wall"))
        {
            SpawnDiagonals();
            DestroyYourself();
        }
    }
    public void OnTriggerExit2D(Collider2D collision)
    {
        if (!isBeam) return;

        if (collision.CompareTag("MainCamera"))
        {
            SpawnDiagonals();
            DestroyYourself();
        }
    }

    public void DestroyYourself()
    {
        Destroy(gameObject);
    }

    public void SpawnDiagonals()
    {
        SpawnDiagonal(1, 1);
        SpawnDiagonal(-1, 1);
        SpawnDiagonal(1, -1);
        SpawnDiagonal(-1, -1);
    }
    public void SpawnDiagonal(short x, short y)
    {
        GameObject sword = (GameObject)Instantiate(swordRef);
        sword.transform.position = new Vector3(transform.position.x, transform.position.y, 0);
        sword.GetComponent<SwordScript>().Diagonal(x, y);
    }

    public void Swing(Sword type, int x, int y, bool hasFullHealth)
    {
        init();
        willShoot = hasFullHealth;
        lookingHorizontal = x;
        lookingVertical = y;
        originalPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        string newAnimation = "";

        if (x != 0)
        {
            newAnimation = "X";
            bc2d.size = new Vector2(bc2d.size.y, bc2d.size.x);
            bc2d.offset = new Vector2(bc2d.offset.y, bc2d.offset.x);
            originalPosition.y -= .1f;
        }
        else if (y != 0)
        {
            newAnimation = "Y";
            originalPosition.x -= .1f;
        }

        // check for the need to do any flipping of the sword
        if (x < 0)
        {
            transform.localScale = new Vector2(-1, 1);
        }
        else if (y < 0)
        {
            transform.localScale = new Vector2(1, -1);
            originalPosition.x += .2f;
        }

        // check for what type of sword was passed in
        switch (type)
        {
            case Sword.WOOD:
                newAnimation += "_1";
                break;

            case Sword.WHITE:
                newAnimation += "_2";
                break;

            case Sword.MAGIC:
                newAnimation += "_3";
                break;
        }

        // set any of the fine tweeks to the position of the the object
        transform.position = originalPosition;
        targetPosition = originalPosition;

        targetPosition.x = transform.position.x + (.9f * x);
        targetPosition.y = transform.position.y + (.9f * y);

        animator.Play(newAnimation);
    }
    public void Shoot(int x, int y)
    {
        init();
        isBeam = true;
        rb2d.velocity = new Vector2((11 * x), (11 * y));

        string newAnimation = "";

        if (x != 0)
        {
            newAnimation = "X_Beam";
            bc2d.size = new Vector2(bc2d.size.y, bc2d.size.x);
            bc2d.offset = new Vector2(bc2d.offset.y, bc2d.offset.x);

            targetPosition.x = transform.position.x + (.5f * x);
        }
        else if (y != 0)
        {
            newAnimation = "Y_Beam";
            targetPosition.y = transform.position.y + (.5f * y);
        }

        if (x < 0)
        {
            transform.localScale = new Vector2(-1, 1);
        }

        if (y < 0)
        {
            transform.localScale = new Vector2(1, -1);
        }

        animator.Play(newAnimation);
    }
    public void Diagonal(short x, short y)
    {
        init();
        isDiagonal = true;
        rb2d.velocity = new Vector2((-11 * x), (11 * y));
        animator.Play("Diagonal_UP_LEFT");
        transform.localScale = new Vector2((1 * x), (1 * y));

        Invoke("DestroyYourself", .17f);
    }
}