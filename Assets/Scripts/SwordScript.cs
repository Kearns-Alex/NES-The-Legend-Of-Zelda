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
    private bool willShoot = false;
    private bool isBeam = false;

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
        bc2d = GetComponent<BoxCollider2D>();
        rb2d = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (transform.position != targetPosition && !isBeam)
        {
            // move to a sinle place
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, .1f);
            //transform.position = Vector3.MoveTowards(transform.position, targetPosition, 2);
        }
        else if (!isBeam && willShoot)
        {
            GameObject sword = (GameObject)Instantiate(swordRef);
            sword.transform.position = new Vector3(transform.position.x, transform.position.y, 0);
            sword.GetComponent<SwordScript>().shoot(lookingHorizontal, lookingVertical);

            DestroyYourself();
        }

    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Wall"))
        {
            DestroyYourself();

            //AJK: spawn the diagonal animations
            if(isBeam)
            {
                //GameObject sword = (GameObject)Instantiate(swordRef);
                //sword.transform.position = new Vector3(transform.position.x, transform.position.y, 0);
                //sword.GetComponent<SwordScript>().Swing(currentSword, lookingHorizontal, lookingVertical, true);
            }
        }
    }

    public void DestroyYourself()
    {
        Destroy(gameObject);
    }

    public void Swing(Sword type, int x, int y, bool hasFullHealth)
    {
        init();
        willShoot = hasFullHealth;
        lookingHorizontal = x;
        lookingVertical = y;
        targetPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z);
       
        string newAnimation = "";

        if (x != 0)
        {
            newAnimation = "X";
            bc2d.size = new Vector2(bc2d.size.y, bc2d.size.x);
            bc2d.offset = new Vector2(bc2d.offset.y, bc2d.offset.x);

            targetPosition.x = transform.position.x + (.5f * x);
        }
        else if (y != 0)
        {
            newAnimation = "Y";
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

        animator.Play(newAnimation);
    }

    public void shoot(int x, int y)
    {
        init();
        isBeam = true;
        rb2d.velocity = new Vector2((10 * x), (10 * y));

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

    /*private IEnumerator Beam(string direction, byte number)
    {
        animator.Play(direction + "_" + number);

        if(number == 4)
        {
            number = 0;
        }

        number++;

        yield return new WaitForSeconds(.5f);

        Beam(direction, number);
    }*/
}
