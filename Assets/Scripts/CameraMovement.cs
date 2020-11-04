using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public GameObject player;
    private Transform target;
    public float smoothing;

    private bool needToUpdatePlayerState = false;

    public Vector2 maxPosition;
    public Vector2 minPosition;

    // Start is called before the first frame update
    void Start()
    {
        target = player.transform;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 targetPosition = new Vector3(target.position.x, target.position.y, transform.position.z);

        targetPosition.x = Mathf.Clamp(targetPosition.x, minPosition.x, maxPosition.x);
        targetPosition.y = Mathf.Clamp(targetPosition.y, minPosition.y, maxPosition.y);

        if (transform.position != targetPosition)
        {
            //AJK: update player state to PAUSE so we do not do anything
            player.GetComponent<PlayerScript>().currentState = PlayerState.PAUSE;
            player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;

            //AJK: set our update variable to true
            needToUpdatePlayerState = true;
            
            transform.position = Vector3.MoveTowards(transform.position,targetPosition, smoothing);
        }
        else if(needToUpdatePlayerState)
        {
            //AJK: update player state to PAUSE so we do not do anything
            player.GetComponent<PlayerScript>().currentState = PlayerState.WALK;
            //AJK: set our update variable to true
            needToUpdatePlayerState = false;
        }
    }

}
