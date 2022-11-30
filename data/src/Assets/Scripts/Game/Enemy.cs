using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class Enemy : MonoBehaviour {

    Rigidbody2D body;
    SpriteRenderer sprite;
    Animator anim;


    
    [Header("Attributes")]
    public int attackDamage;           // dano que causa por ataque
    public float fieldOfView;          // distancia que nota o player
    public int maxHealth;              // hp
    public bool isBoss = false;        // eh um boss?
    int currentHealth;

    [Header("Movement variables")]
    public float speed;                // velocidade que anda | se tiver virado p/ esq: -speed, senão +speed
    bool isMoving = false;

    [Header("Attack variables")]
    public Transform attackPoint;
    public float attackRange;          // range do ataque
    public float attackInterval;       // intervalo entre um ataque e outro
    public float distanceToAttack;     // quando estiver nessa distancia ou mais perto, ataca
    public LayerMask playerLayers;
    float timeNextAttack;
    Transform player;

   // ****


    void Start() {
        currentHealth = maxHealth;
        player = GameObject.FindGameObjectsWithTag("Player")[0].GetComponent<Transform>();

        body = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        if(sprite.flipX) {
            speed *= -1;
            attackPoint.localPosition = new Vector2(-attackPoint.localPosition.x, attackPoint.localPosition.y);
        }
    }

    void Update() {
        float distance = GetPlayerDistance();
        isMoving = distance <= fieldOfView && distance > distanceToAttack;

        if(distance <= fieldOfView) {
            if((player.position.x > body.position.x && sprite.flipX) || (player.position.x < body.position.x && !sprite.flipX)){
                Flip();
            }
        }

        if(!isMoving && distance <= distanceToAttack && timeNextAttack <= 0f) {
            timeNextAttack = attackInterval;
            anim.SetTrigger("attack");
        }
        else {
            timeNextAttack -= Time.deltaTime;
        }
    }

    void FixedUpdate() {
        if(isMoving) {
            body.velocity = new Vector2(speed, body.velocity.y);
            anim.SetBool("moving", true);
        }
        else {
            anim.SetBool("moving", false);
        }
    }


    // methods

    float GetPlayerDistance() {
        return Mathf.Abs(player.position.x - body.position.x);
    }

    void Flip() {
        sprite.flipX = !sprite.flipX;
        attackPoint.localPosition = new Vector2(-attackPoint.localPosition.x, attackPoint.localPosition.y);

        speed *= -1;
    }

    void Attack() {
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRange,
            playerLayers
        );
        foreach(Collider2D player in hitPlayers) {
            player.GetComponent<Player>().TakeDamage(attackDamage);
        }
    }



    void Die() {
        anim.SetBool("isDead", true);
        this.enabled = false;

        if(isBoss) {
            //GetComponentInChildren<HealthBar1>().hpImage.enabled = false;
            //GetComponentInChildren<HealthBar1>().hpEffectImage.enabled = false;
            //GetComponentInChildren<HealthBar1>().bgImage.enabled = false;
            StartCoroutine(EndLevel());
        }
    }

    IEnumerator EndLevel() {
        yield return new WaitForSeconds(4);
        SceneManager.LoadScene("Menu");
    }


    // public methods

    public void TakeDamage(int damage) {
        currentHealth -= damage;

        if(isBoss) {
            GetComponentInChildren<HealthBar1>().hp -= 30;
        }
        anim.SetTrigger("hurt");

        if(currentHealth <= 0) {
            Die();
        }
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
