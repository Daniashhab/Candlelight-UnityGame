using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxTwo : MonoBehaviour
{
    [SerializeField] Rigidbody2D playerRB;

    [SerializeField] Rigidbody2D backgroundA;
    [SerializeField] float pFactorA_x = 0;
    [SerializeField] float pFactorA_y = 0;

    [SerializeField] Rigidbody2D backgroundB;
    [SerializeField] float pFactorB_x = 0;
    [SerializeField] float pFactorB_y = 0;

    [SerializeField] Rigidbody2D backgroundC;
    [SerializeField] float pFactorC_x = 0;
    [SerializeField] float pFactorC_y = 0;

    [SerializeField] Rigidbody2D backgroundD;
    [SerializeField] float pFactorD_x = 0;
    [SerializeField] float pFactorD_y = 0;

    [SerializeField] Rigidbody2D backgroundE;
    [SerializeField] float pFactorE_x = 0;
    [SerializeField] float pFactorE_y = 0;

    void Update()
    {
        backgroundA.velocity = new Vector2(playerRB.velocity.x * -pFactorA_x, playerRB.velocity.y * pFactorA_y);
        backgroundB.velocity = new Vector2(playerRB.velocity.x * -pFactorB_x, playerRB.velocity.y * pFactorB_y);
        backgroundC.velocity = new Vector2(playerRB.velocity.x * -pFactorC_x, playerRB.velocity.y * pFactorC_y);
        backgroundD.velocity = new Vector2(playerRB.velocity.x * -pFactorD_x, playerRB.velocity.y * pFactorD_y);

        if (backgroundE != null) backgroundE.velocity = new Vector2(playerRB.velocity.x * -pFactorE_x, playerRB.velocity.y * pFactorE_y);
    }
}
