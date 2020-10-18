#define LOGS

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenMove : MonoBehaviour
{
	// the different directions that we can move from screen to screen
	public enum Direction : byte
	{
		 NORTH_SOUTH
		,EAST_WEST
	}
	
	public Direction direction = Direction.NORTH_SOUTH;
	public Vector2 cameraChange = new Vector2(16, 11);
	public Vector3 playerChange = new Vector3(0, 0, 0);

	private CameraMovement cam;

	// Start is called before the first frame update
	void Start()
	{
		// set the camera object
		cam = Camera.main.GetComponent<CameraMovement>();
	}

	// Update is called once per frame
	void Update()
	{

	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		// make a temperary holder for the variables so we do not override the master ones
		Vector2 _cameraChange = cameraChange;
		Vector3 _playerChange = playerChange;

		// check to see that the player tag is attached to the one colliding
		if (!other.CompareTag("Player"))
		{
			return;
		}

		// see what direction we are moving and make changes 
		switch(direction)
		{
			case Direction.NORTH_SOUTH:
				_cameraChange.x = 0;
				_playerChange.x = 0;

				// check if we are coming from the North
				if (transform.GetComponent<Collider2D>().bounds.center.y < other.bounds.center.y)
				{
					_cameraChange.y *= -1;
					_playerChange.y *= -1;
#if (LOGS)
					print("Move South");
				}
				else
				{
					print("Move North");
#endif
				}
				break;

			case Direction.EAST_WEST:
				_cameraChange.y = 0;
				_playerChange.y = 0;

				// check if we are coming from the East
				if (transform.GetComponent<Collider2D>().bounds.center.x < other.bounds.center.x)
				{
					_cameraChange.x *= -1;
					_playerChange.x *= -1;
#if (LOGS)
					print("Move West");
				}
				else
				{
					print("Move East");
#endif
				}
				break;

			default:
				break;
		}
		
		// update the positions
		cam.minPosition += _cameraChange;
		cam.maxPosition += _cameraChange;

		other.transform.position += _playerChange;
	}
}
