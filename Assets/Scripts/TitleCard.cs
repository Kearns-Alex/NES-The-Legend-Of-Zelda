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


public class TitleCard : MonoBehaviour
{
    public string placeName;
    public GameObject text;
#if TEXTMESH_PRO
    public TextMeshProUGUI placeText;
#else
    public Text placeText;
#endif

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
