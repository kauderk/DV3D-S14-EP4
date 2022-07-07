using System;
using System.Collections;
using System.Collections.Generic;
using ToolBox.Pools;
using UnityEngine;

public class Pickupable : MonoBehaviour
{
    [SerializeField] private int _score_ = 3;

    [SerializeField] private AudioSource pickupSound;


    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, 1);
        // draw a thick red line pointing upwards
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 2);
    }

    private void Awake()
    {
        ToggleVisivility(true);
        pickupSound = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        ToggleVisivility(true);
        PoolsManager.OnReleaseAll += OnReleaseAll;
    }
    private void OnDisable()
    {
        PoolsManager.OnReleaseAll -= OnReleaseAll;
    }

    private void OnReleaseAll(string WhiteList)
    {
        if (!gameObject.tag.Equals(WhiteList))
            gameObject.Release();
    }

    // an Event? or a method? An interface is a better choice.
    public int Take()
    {
        StartCoroutine(SecondsCoroutine(pickupSound.clip.length + .2f)); // I NEED YOU ASYNC!
        return _score_;
    }

    IEnumerator SecondsCoroutine(float seconds)
    {
        Debug.Log("Crystal Pickuped up!");
        //pickupSound.Play();
        ToggleVisivility(false);
        yield return new WaitForSeconds(seconds);
        gameObject.Release();
    }

    private void ToggleVisivility(bool b)
    {
        // disable mesh renderer
        GetComponent<MeshRenderer>().enabled = b;
        // disable collider
        GetComponent<Collider>().enabled = b;
    }
}
