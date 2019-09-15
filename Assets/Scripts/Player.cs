using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] float moveSpeed = 10f;
    [SerializeField] float padding = 1f;
    [SerializeField] GameObject laserPrefab;
    [SerializeField] float projectileSpeed = 10f;
    [SerializeField] float projectileFiringPeriod = 1f;

    Coroutine firingCoroutine;

    float xMin;
    float yMin;
    float xMax;
    float yMax;

    // Start is called before the first frame update
    void Start()
    {
        SetUpMoveBoundaries();
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        Fire();
    }

    private void Fire()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            firingCoroutine = StartCoroutine(FireContinuously());
        }
        if (Input.GetButtonUp("Fire1"))
        {
            StopCoroutine(firingCoroutine);
        }
    }

    private IEnumerator FireContinuously()
    {
        while (true)
        {
            GameObject laser = Instantiate(laserPrefab, transform.position, Quaternion.identity) as GameObject;
            laser.GetComponent<Rigidbody2D>().velocity = new Vector2(0, projectileSpeed);
            yield return new WaitForSeconds(projectileFiringPeriod);
        }
    }

    private void Move()
    {
        // Time.deltaTime: Devuelve el tiempo que tarda en ejecutar cada frame
        // Lo usamos para que la nave se mueva a la misma velocidad en computadoras lentas y rápidas
        var deltaX = Input.GetAxis("Horizontal") * Time.deltaTime * moveSpeed;
        var deltaY = Input.GetAxis("Vertical") * Time.deltaTime * moveSpeed;

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
