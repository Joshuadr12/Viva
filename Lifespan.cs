using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lifespan : MonoBehaviour
{
    [SerializeField] int life = 5;
    [SerializeField] float spread = 0.25f;

    float time = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        while (time >= 1)
        {
            transform.Translate(Random.Range(-1f, 1f) * spread, Random.Range(-1f, 1f) * spread, 0);
            time--;
            life--;
        }
        if (life <= 0) { Destroy(gameObject); }
    }
}