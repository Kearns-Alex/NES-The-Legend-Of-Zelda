using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleCard : MonoBehaviour
{
    public string placeName;
    public GameObject text;
    public Text placeText;
    public float displayTime = 4;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        StartCoroutine(PlaceNameCo());
    }

    // method that can run paralel to other processes and allows a specified wait time
    private IEnumerator PlaceNameCo()
    {
        text.SetActive(true);
        placeText.text = placeName;
        yield return new WaitForSeconds(displayTime);
        text.SetActive(false);
    }
}
