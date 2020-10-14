using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public sbyte speed = 4;
    private Rigidbody2D myRigidBody;
    private Vector3 change;

    private Animator animator;

    bool wasMovingVertical = false;

    // Start is called before the first frame update
    void Start()
    {
        myRigidBody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    // FIXEDUPDATE!!!!!!!!!!! This solves issues with movement dealing with the rigidbody
    void FixedUpdate()
    {
        // zero out the change
        change = Vector3.zero;
        // goes directly to 1. No accelleration 
        change.x = Input.GetAxisRaw("Horizontal");
        change.y = Input.GetAxisRaw("Vertical");

        UpdateAnimationAndMove();
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
            } else
            {
                change.x = 0;
            }
        } else if (isMovingHorizontal)
        {
            wasMovingVertical = false;
        } else if (isMovingVertical)
        {
            wasMovingVertical = true;
        }

        // move character if we have speed to do so
        if(change != Vector3.zero)
        {
            MoveCharacter(change);
            animator.SetFloat("moveX", change.x);
            animator.SetFloat("moveY", change.y);
            animator.SetBool("moving", true);
        } else {
            animator.SetBool("moving", false);
        }
    }

    // Allows other "outside sources" to move the character if needed.
    void MoveCharacter(Vector3 delta)
    {
        // this line of code was making weird things happen such as the demo in unity being choppy and slow 
        //  compared to the build application being smooth and quicker. The following line seems to have
        //  fixed these issues...but the second line will cause jittering
        // SOLVED: replace 'Update()' with 'FixedUpdate()' and this line of code works perfectly
        myRigidBody.MovePosition(transform.position + delta * speed * Time.deltaTime);
        //transform.position = transform.position + delta * speed * Time.deltaTime;
    }
}
