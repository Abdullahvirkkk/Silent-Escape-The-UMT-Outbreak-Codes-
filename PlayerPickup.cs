using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPickup : MonoBehaviour
{
    public static PlayerPickup instance;
    public float pickupRadius = 5f;
    public KeyCode pickupKey = KeyCode.P;
    public KeyCode dropKey = KeyCode.O;
    public Transform itemHoldPosition;
    public Transform itemHoldPosition2;
    public Transform itemHoldPosition3;
    public Transform playerBody;
    public Transform keyHolder;

    private GameObject heldItem;
    bool isKey;
    public bool isRifle;
  
    [Header("Audio")]
    public AudioClip pickupSound;
    public AudioClip dropSound;
    public AudioClip pickupSoundGun;
    public AudioClip dropSoundGun;
    public AudioClip pickupSoundAxe;
    public AudioClip dropSoundAxe;
    public AudioSource audioSource;

    void Start()
    {
        instance = this;
        audioSource = GetComponent<AudioSource>();
    }


    void Update()
    {
        if (Input.GetKeyDown(pickupKey))
        {
            TryPickupItem();
        }

        if (Input.GetKeyDown(dropKey))
        {
            DropItem();
        }
    }

    void TryPickupItem()
    {
        if (heldItem != null)
        {
            Debug.Log("Already holding an item. Drop it first to pick up another.");
            return;
        }

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, pickupRadius);
        GameObject closestItem = null;
        float closestDistance = pickupRadius;
        //bool foundKey = false;
        Debug.Log("Scanning for items...");
        Debug.Log($"Found {hitColliders.Length} items nearby.");


        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("PickupItem"))
            {
                float distance = Vector3.Distance(transform.position, hitCollider.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestItem = hitCollider.gameObject;
                }
                isKey = true;
            }

             else if (hitCollider.CompareTag("Rifle"))
            {
                float distance = Vector3.Distance(transform.position, hitCollider.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestItem = hitCollider.gameObject;
                }
                isKey = false;
                isRifle = true;
            }

            else if (hitCollider.CompareTag("OtherObjects"))
            {
                float distance = Vector3.Distance(transform.position, hitCollider.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestItem = hitCollider.gameObject;
                }
                isKey = false;
                isRifle = false;
            }
        }

        if (closestItem != null)
        {
            //isKey = foundKey;
            Pickup(closestItem);
        }
    }

    void Pickup(GameObject item)
    {
            heldItem = item;
            heldItem.GetComponent<Collider>().enabled = false;
            if(heldItem.GetComponent<Rigidbody>())
            {
                heldItem.GetComponent<Rigidbody>().isKinematic = true;
            }

            if(isKey)
            {
                heldItem.transform.SetParent(playerBody);
                heldItem.transform.position = itemHoldPosition.position;
                heldItem.transform.rotation = itemHoldPosition.rotation;
                PlaySound(pickupSound);
            }

            else if(isRifle)
            {
                heldItem.transform.SetParent(playerBody);
                heldItem.transform.position = itemHoldPosition2.position;
                heldItem.transform.rotation = itemHoldPosition2.rotation;
                PlaySound(pickupSoundGun);
            }

            else
            {
                heldItem.transform.SetParent(playerBody);
                heldItem.transform.position = itemHoldPosition3.position;
                heldItem.transform.rotation = itemHoldPosition3.rotation;
                PlaySound(pickupSoundAxe);
            }
            Debug.Log("Picked up: " + item.name);
            //sound
            //PlaySound(pickupSound);
    }

    void DropItem()
    {
        if (heldItem != null)
        {

        heldItem.GetComponent<Collider>().enabled = true;
        heldItem.transform.position = transform.position + transform.forward;
         if(heldItem.GetComponent<Rigidbody>())
            {
                heldItem.GetComponent<Rigidbody>().isKinematic = false;
            }
             heldItem.transform.SetParent(keyHolder);
            Debug.Log("Dropped: " + heldItem.name);
            heldItem = null;

            //sound
            if (isKey)
            {
                PlaySound(dropSound);
            }
            else if (isRifle)
            {
                PlaySound(dropSoundGun);
            }
            else 
            {
                PlaySound(dropSoundAxe);
            }
            //PlaySound(dropSound);
            //granny to know

            VillanAI villanAI = FindObjectOfType<VillanAI>();
            if(villanAI != null)
            {
                villanAI.OnSoundHeard(transform.position);
            }
            
            //heldItem = null;
            isKey = false;
            isRifle = false; 

        }

        else
        {
            Debug.Log("No item to drop.");
        }
    }

    void PlaySound(AudioClip clip)
    {
        if(clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}

