using System;
using System.Collections.Generic;
using ToolBox.Pools;
using UnityEngine;

public class Pickupable : MonoBehaviour
{
    [SerializeField] private int _score = 3;

    [SerializeField] private AudioSource pickupSound;

    private void Awake()
    {
        pickupSound = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
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
        StartCoroutine("PlayPickupSound"); // I NEED YOU ASYNC!
        return _score;
    }

    IEnumerator<WaitForSeconds> WaitForSeconds(float seconds)
    {
        pickupSound.Play();
        // disable mesh renderer
        GetComponent<MeshRenderer>().enabled = false;
        // disable collider
        GetComponent<Collider>().enabled = false;
        // disable rigidbody
        GetComponent<Rigidbody>().isKinematic = true;
        yield return new WaitForSeconds(seconds);
        gameObject.Release();

    }
}
