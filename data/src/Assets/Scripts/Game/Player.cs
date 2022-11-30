using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class Player : MonoBehaviour {

    Rigidbody2D body;
    SpriteRenderer sprite;
    Animator anim;

    public HealthBar healthBar;

    [Header("Attributes")]
    public int attackDamage = 30;
    public int maxHealth = 100;
    int currentHealth;

    [Header("Movement variables")]
    public float speed;
    public float jumpForce;
    public Transform groundCheck;
    public LayerMask whatIsGround;
    bool isOnFloor = false;
    bool isJumping = false;

    [Header("Attack variables")]
    public Transform attackPoint;
    public float attackRange;
    public LayerMask enemyLayers;
    float timeNextAttack;

    // ****


    void Start() {
        body = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        healthBar.SetMaxHealth(maxHealth);
        currentHealth = maxHealth;
    }


    void Update() {
        if(Input.GetKeyDown(KeyCode.Escape)) {   // esc = volta para o menu
            SceneManager.LoadScene("Menu");
        }
        else {
            isOnFloor = Physics2D.Linecast(transform.position, groundCheck.position, whatIsGround);
            if(Input.GetButtonDown("Jump") && isOnFloor == true) {
                isJumping = true;
            }

            if(timeNextAttack <= 0f) {
                if(Input.GetButtonDown("Fire1") && body.velocity.x == 0 && body.velocity.y == 0) {
                    anim.SetTrigger("attack");
                    timeNextAttack = 0.2f;
                }
            }
            else {
                timeNextAttack -= Time.deltaTime;
            }
        }
    }

    void FixedUpdate() {
        float move = Input.GetAxis("Horizontal");    
        body.velocity = new Vector2(move * speed, body.velocity.y);

        if((move > 0 && sprite.flipX == true) || (move < 0 && sprite.flipX == false)) {
           Flip();
        }
        
        if(isJumping) {
            body.AddForce(new Vector2(0f, jumpForce));
            isJumping = false;
        }

        SetParameters();
    }


    // methods

    void Flip() {
        sprite.flipX = !sprite.flipX;
        attackPoint.localPosition = new Vector2(-attackPoint.localPosition.x, attackPoint.localPosition.y);
    }

    void SetParameters() {
        anim.SetFloat("velX", Mathf.Abs(body.velocity.x));

        if(body.velocity.y == 0) {
            anim.SetBool("isJumping", false);
            anim.SetBool("isFalling", false);
        }
        else if(body.velocity.y > 0.1) {
            anim.SetBool("isJumping", true);
            anim.SetBool("isFalling", false);
        }
        else if(body.velocity.y < 0.1) {
            anim.SetBool("isJumping", false);
            anim.SetBool("isFalling", true);
        }
    }

    void PlayerAttack() {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            attackPoint.position,
            attackRange,
            enemyLayers
        );
        bool critial = new System.Random().Next(0, 10) == 9;   // 10%
        int enemyDamage = critial ? attackDamage * 2 : attackDamage;

        foreach(Collider2D enemy in hitEnemies) {
            enemy.GetComponent<Enemy>().TakeDamage(enemyDamage);
        }
    }

    void Die() {
        anim.SetBool("isDead", true);
        StartCoroutine(RestartLevel());
        this.enabled = false;
    }

    IEnumerator RestartLevel() {
        yield return new WaitForSeconds(4);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


    // public methods

    public void TakeDamage(int damage) {
        currentHealth -= damage;
        
        if(currentHealth <= 0) {
            Die();
        }

        healthBar.SetHeath(currentHealth);
    }


    void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
