using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class EasterEgg : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] ParticleSystem burst = null;


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            EasterBurst();
        }
    }

    public void EasterBurst()
    {
        burst.Play();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        EasterBurst();
    }
}
