using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VillanAI : MonoBehaviour
{
    [Header("Health System")]
    float characterHealth = 100f;
    public float prensetHealth;
    public float respawnTime = 5f;
    public GameObject grannyDeathText;
    public Text deathText;
    public GameObject grannyDeadBody;

    [Header("Sound Detection")]
    UnityEngine.AI.NavMeshAgent navMeshAgent;
    public float moveSpeed = 3.5f;
    public Transform startPosition;
    private Vector3 soundLocation;
    public bool isReturning = false;
    private bool isChasing = false;
    private bool isAttacking = false;
    private bool isWaiting = false;
    private bool isDead = false;
    bool soundHeard = false;

    [Header("Granny State")]
    public Transform player;
    public float detectionRadius = 7f;
    public float attackRange = 2f;
    public float attackCooldown = 1f;
    float lastAttackTime = 0f;


    private Animator animator;
    
    [Header("FootStep")]
    public AudioClip[] footstepSounds;
    AudioSource audioSource;
    float footstepInterval = 0.5f;
    float nextFootstepTime = 0f;


    void Start()
    {
        prensetHealth = characterHealth;
        navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        navMeshAgent.speed = moveSpeed;
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        grannyDeathText.SetActive(false);
    }


     void Update()
    {
        if (isDead) return;

    
        if (isReturning)
        {
            ReturnToStart();
        }
        else if (isAttacking)
        {
            AttackPlayer();
        }
         else if (isChasing && soundHeard)
        {
            ChasePlayer();
        }
        else if (!isWaiting)
        {
            LookForPlayer();
        }

        UpdateAnimations();
        PlayFootstepSounds();

        if(Game.instance.isEasy)
        {
            respawnTime = 120f;
            deathText.text = "Granny is gone for 2 minutes";
        }
        else
        {
            respawnTime = 60f;
            deathText.text = "Granny is gone for 1 minutes";
        }
        
    }

    public void OnSoundHeard(Vector3 location)
    {
        if (isDead) return;

        soundLocation = location;
        soundHeard = true;
        isReturning = false;
        isChasing = false;
        isAttacking = false;
        isWaiting = false;
        MoveToSoundLocation();
    }

     void MoveToSoundLocation()
    {
        navMeshAgent.SetDestination(soundLocation);
        if (Vector3.Distance(transform.position, soundLocation) <= navMeshAgent.stoppingDistance)
        {
            StartCoroutine(WaitBeforeReturning());
        }
    }

    IEnumerator WaitBeforeReturning()
    {
        isWaiting = true;
        yield return new WaitForSeconds(10f);
        isWaiting = false;
        isReturning = true;
        soundHeard = false;
    }


     void ReturnToStart()
    {
        navMeshAgent.SetDestination(startPosition.position);
        if (Vector3.Distance(transform.position, startPosition.position) <= navMeshAgent.stoppingDistance)
        {
            isReturning = false;
        }
    }    


    void LookForPlayer()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player"))
            {
                isChasing = true;
                isReturning = false;
                break;
            }
        }
    } 

    void ChasePlayer()
    {
        navMeshAgent.SetDestination(player.position);
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackRange)
        {
            navMeshAgent.isStopped = true;
            isChasing = false;
            isAttacking = true;
        }
        else if (distanceToPlayer > detectionRadius)
        {
            isChasing = false;
            isReturning = true;
        }
    }   

    void AttackPlayer()
    {
        if (Time.time > lastAttackTime + attackCooldown)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                //damage the player
                Debug.Log("Attack the Player");
                playerController.TakeDamage(100f);
            }

            lastAttackTime = Time.time;

            //respawn the graanny at start position
            StartCoroutine(Respawn(2));
            
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);
            if (distanceToPlayer > attackRange)
            {
                navMeshAgent.isStopped = false;
                isChasing = true;
                isAttacking = false;
            }
        }
    }


    void UpdateAnimations()
    {
        bool isMoving = navMeshAgent.velocity.magnitude > 0.1f;
        animator.SetBool("isWalking", isMoving);
        animator.SetBool("isAttacking", isAttacking);
        animator.SetBool("isDead", isDead);

        if (!isMoving && !isAttacking && !isDead)
        {
            animator.SetBool("isIdle", true);
        }
        else
        {
            animator.SetBool("isIdle", false);
        }
    }

    void PlayFootstepSounds()
    {
        if (navMeshAgent.velocity.magnitude > 0.1f && Time.time >= nextFootstepTime)
        {
            if (footstepSounds.Length > 0)
            {
                AudioClip footstepSound = footstepSounds[Random.Range(0, footstepSounds.Length)];
                audioSource.PlayOneShot(footstepSound);
                nextFootstepTime = Time.time + footstepInterval;
            }
        }
    }

    public void characterHitDamage(float takeDamage)
    {
        if (isDead) return;

        prensetHealth -= takeDamage;
        
        if (prensetHealth <= 0)
        {
            characterDie();
        }
    }

     void characterDie()
    {
        isDead = true;
        moveSpeed = 0f;
        navMeshAgent.speed = moveSpeed;
        detectionRadius = 0f;

        animator.SetBool("isDead", true);
        GetComponent<Collider>().enabled = false;
        navMeshAgent.enabled = false;

        //UI
        grannyDeathText.SetActive(true);

        //Respawn
        StartCoroutine(Respawn(respawnTime));
    }

    IEnumerator Respawn(float delay)
    {
        yield return new WaitForSeconds(3f);
        
        grannyDeathText.SetActive(false);
        grannyDeadBody.SetActive(false);  

        yield return new WaitForSeconds(delay - 3);

        prensetHealth = characterHealth;
        isDead = false;
        animator.SetBool("isDead", false);
        grannyDeadBody.SetActive(true); 

        GetComponent<Collider>().enabled = true;
        navMeshAgent.enabled = true;
        this.enabled = true;

        transform.position = startPosition.position;
        navMeshAgent.Warp(startPosition.position);
        
        isReturning = false;
        isChasing = false;
        isAttacking = false;
        isWaiting = false;
        soundHeard = false;
        moveSpeed = 3.5f;
        navMeshAgent.speed = moveSpeed;
        detectionRadius = 15f;
    }
}

