// if using text mesh pro, look into why text objects share the same outline settings
#define TEXTMESH_PRO

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if TEXTMESH_PRO
using TMPro;
#else
using UnityEngine.UI;
#endif

public class Dialog : MonoBehaviour
{
    [TextArea(3, 10)]
    public string dialog;
    public GameObject dialogBox;
#if TEXTMESH_PRO
    public TextMeshProUGUI dialogText;
#else
    public Text dialogText;
#endif
    public bool playerInRange = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // check for a space press while the player is in range
        if(!Input.GetKeyDown(KeyCode.Space)
            || !playerInRange)
        {
            return;
        }

        if(dialogBox.activeInHierarchy)
        {
            dialogBox.SetActive(false);
        }
        else
        {
            dialogBox.SetActive(true);
            dialogText.text = dialog;
        }

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // check to see that the player tag is attached to the one colliding
        if (!other.CompareTag("Player"))
        {
            return;
        }

        // set our playerInRange to true
        playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // check to see that the player tag is attached to the one colliding
        if (!other.CompareTag("Player"))
        {
            return;
        }

        // set our playerInRange to false
        playerInRange = false;
        dialogBox.SetActive(false);
    }
}
