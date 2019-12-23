using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    // Configuration parameters
    [Header("General")]
    [SerializeField] int health = 200;

    [Header("Movement")]
    [SerializeField] float moveSpeed = 10f;
    [SerializeField] float padding = 1f;

    [Header("Shooting")]
    [SerializeField] AudioClip shootSound;
    [SerializeField] [Range(0, 1)] float shootSoundVolume = 0.12f;

    [Header("Death")]
    [SerializeField] GameObject deathVFX;
    [SerializeField] float durationOfExplosion = 1f;
    [SerializeField] AudioClip deathSound;
    [SerializeField] [Range(0, 1)] float deathSoundVolume = 0.75f;

    [Header("Projectile")]
    [SerializeField] GameObject laserPrefab;
    [SerializeField] float projectileSpeed = 10f;
    [SerializeField] float projectileFiringPeriod = 0.2f;

    // Variables
    Coroutine firingCoroutine;
    float xMin;
    float yMin;
    float xMax;
    float yMax;
    bool isAndroid = false;

    // Start is called before the first frame update
    void Start()
    {
        CheckOS();
        SetUpMoveBoundaries();
    }

    private void CheckOS()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            isAndroid = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        Fire();
    }

    public int GetHealth()
    {
        return health;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        DamageDealer damageDealer = collision.gameObject.GetComponent<DamageDealer>();
        if (!damageDealer) {
            return;
        }
        ProcessHit(damageDealer);
    }

    private void ProcessHit(DamageDealer damageDealer)
    {
        health -= damageDealer.GetDamage();
        damageDealer.Hit();
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        FindObjectOfType<Level>().LoadGameOver();
        Destroy(gameObject);
        GameObject explosion = Instantiate(deathVFX, transform.position, transform.rotation) as GameObject;
        Destroy(explosion, durationOfExplosion);
        AudioSource.PlayClipAtPoint(deathSound, Camera.main.transform.position, deathSoundVolume);
    }

    private void Fire()
    {
        if (isAndroid)
        {
            // Smartphones
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                firingCoroutine = StartCoroutine(FireContinuously());
            }
            else if (touch.phase == TouchPhase.Ended)
            {
                StopCoroutine(firingCoroutine);
            }
        }
        else
        {
            // Windows
            if (Input.GetButtonDown("Fire1"))
            {
                firingCoroutine = StartCoroutine(FireContinuously());
            }
            if (Input.GetButtonUp("Fire1"))
            {
                StopCoroutine(firingCoroutine);
            }
        }
    }

    private IEnumerator FireContinuously()
    {
        while (true)
        {
            GameObject laser = Instantiate(laserPrefab, transform.position, Quaternion.identity) as GameObject;
            laser.GetComponent<Rigidbody2D>().velocity = new Vector2(0, projectileSpeed);
            AudioSource.PlayClipAtPoint(shootSound, Camera.main.transform.position, shootSoundVolume);
            yield return new WaitForSeconds(projectileFiringPeriod);
        }
    }

    private void Move()
    {
        // Time.deltaTime: Devuelve el tiempo que tarda en ejecutar cada frame
        // Lo usamos para que la nave se mueva a la misma velocidad en computadoras lentas y rápidas
        float deltaX = 0;
        float deltaY = 0;
        if (isAndroid)
        {
            deltaX = Input.acceleration.x * Time.deltaTime * moveSpeed;
            deltaY = Input.acceleration.y * Time.deltaTime * moveSpeed;
        }
        else
        {
            deltaX = Input.GetAxis("Horizontal") * Time.deltaTime * moveSpeed;
            deltaY = Input.GetAxis("Vertical") * Time.deltaTime * moveSpeed;
        }

        var newXPos = Mathf.Clamp(transform.position.x + deltaX, xMin, xMax);
        var newYPos = Mathf.Clamp(transform.position.y + deltaY, yMin, yMax);
        transform.position = new Vector2(newXPos, newYPos);
    }

    private void SetUpMoveBoundaries()
    {
        Camera gameCamera = Camera.main;
        // ViewportToWorldPoint: Coge la posicion relativa de la camara y lo transforma a una posicion global
        // Por ej: la posicion "0, 0" de la camara podria ser la posicion global "-5.61, 12.23"

        Vector3 bottomLeft = gameCamera.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3 topRight = gameCamera.ViewportToWorldPoint(new Vector3(1, 1, 0));

        xMin = bottomLeft.x + padding;
        yMin = bottomLeft.y + padding;
        xMax = topRight.x - padding;
        yMax = topRight.y - padding;
    }
}
