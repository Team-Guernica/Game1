using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleManager : MonoBehaviour
{
    public GameObject pressText;   // "Press anywhere to start"

    private bool started = false;

    void Update()
    {
        if (started) return;

        if (Input.GetMouseButtonDown(0))
        {
            started = true;

            pressText.SetActive(false);
        }
    }
}
