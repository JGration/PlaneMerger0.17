using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnePlayerMovement : MonoBehaviour {

    private Animator anim;
    private Rigidbody rb;
    private GameObject[] WeaponTrail;
    public static bool playerdeath;
    //public static float health = 100f;
    //public static float max = 100f;
    public static bool facingRight;
    public bool canClick, _leftTrig, _rightTrig, isShadowing = false, isGrounded, isAttacking = false, shadowPower = false, emitDashCooldown = false, isDashing = false, isCrouching = false, canDash = true, doubleJump = true, candoublejump = false;
    private float jumpForce = 9f, dashForce = 850f, superDashForce = 300f, dashCooldown = 1f, shadowCooldown = 4f, nextShadow = 0f, nextDash = 0f, PlayerSpeed = 5f;
    private int takeOnce = 0;
    public int noOfClicks;
    public int weaponSelected;
    public int weaponType;
    public float checkRbVelocityY, RightTriggerAxis, LeftTriggerAxis;
    public AnimatorOverrideController OverridingController;
    public AnimationClip shadowDashClip;
    public AnimationClip normalDashClip;
    public GameObject NormalAfterImage;
    public GameObject ShadowAfterImage;


    private List<Collider> m_collisions = new List<Collider>();

    private void Awake() {
        WeaponTrail = GameObject.FindGameObjectsWithTag("Weapon Trail");
    }

    void Start() {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        facingRight = true;
        noOfClicks = 0;
        canClick = true;
        anim.runtimeAnimatorController = OverridingController;
        NormalAfterImage.SetActive(false);
        ShadowAfterImage.SetActive(false);

        weaponSelected = GameObject.Find("Weapon Holder").GetComponent<WeaponSwitching>().selectedWeapon;
        if (WeaponTrail != null)
            Invoke("DeactivateTrail", 0.5f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        ContactPoint[] contactPoints = collision.contacts;
        for (int i = 0; i < contactPoints.Length; i++)
        {
            if (Vector3.Dot(contactPoints[i].normal, Vector3.up) > 0.5f)
            {
                if (!m_collisions.Contains(collision.collider))
                {
                    m_collisions.Add(collision.collider);
                }
                isGrounded = true;
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        ContactPoint[] contactPoints = collision.contacts;
        bool validSurfaceNormal = false;
        for (int i = 0; i < contactPoints.Length; i++)
        {
            if (Vector3.Dot(contactPoints[i].normal, Vector3.up) > 0.5f)
            {
                validSurfaceNormal = true; break;
            }
        }

        if (validSurfaceNormal)
        {
            isGrounded = true;
            if (!m_collisions.Contains(collision.collider))
            {
                m_collisions.Add(collision.collider);
            }
        }
        else
        {
            if (m_collisions.Contains(collision.collider))
            {
                m_collisions.Remove(collision.collider);
            }
            if (m_collisions.Count == 0) { isGrounded = false; }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (m_collisions.Contains(collision.collider))
        {
            m_collisions.Remove(collision.collider);
        }
        if (m_collisions.Count == 0) { isGrounded = false; }
    }

    void Update() {
        if (!PauseMenu.GameIsPaused)
        {
            anim.SetBool("grounded", isGrounded);
            JumpingAndLanding();
            Dashing();
            RightTriggerAxis = Input.GetAxis("Dash");
            LeftTriggerAxis = Input.GetAxis("ShadowForm");
            RightTrigger();
            LeftTrigger();
            weaponSelected = GameObject.Find("Weapon Holder").GetComponent<WeaponSwitching>().selectedWeapon;

            //Character power moves
            if (Input.GetKeyDown(KeyCode.Q) || _leftTrig)
            {
                if (Time.time > nextShadow)
                    shadowPower = !shadowPower;                      
            }
            else if (!canDash)
                shadowPower = false;
            if(shadowPower)
                OverridingController["Dash"] = shadowDashClip;
            if (!shadowPower)
                OverridingController["Dash"] = normalDashClip;

            //Normal combo starter
            if (isGrounded && Input.GetButtonDown("Fire1") && !isCrouching)
                ComboStarter();

            checkRbVelocityY = rb.velocity.y;
            if (rb.velocity.y > 20)
                rb.velocity = new Vector3(rb.velocity.x, 0, 0);

            float misc = Input.GetAxis("Vertical");
            if (misc < 0 && isGrounded)
            {
                anim.SetBool("crouching", true);
                rb.velocity = new Vector3(0, 0, 0);
                isCrouching = true;
            }
            else if (misc >= 0 && isGrounded)
            {
                anim.SetBool("crouching", false);
                isCrouching = false;
            }
            if (Input.GetButtonDown("Fire1") && isCrouching)
            {
                anim.SetTrigger("crouchattack");
                isAttacking = true;
            }
            else if (!Input.GetButtonDown("Fire1") && isCrouching)
                isAttacking = false;
            //Particle Systems
            if (isDashing || isAttacking)
                NormalAfterImage.SetActive(true);
            else if(isShadowing && isDashing)
                NormalAfterImage.SetActive(false);
            else if(!isDashing || !isAttacking)
                NormalAfterImage.SetActive(false);
            if (isShadowing)
                ShadowAfterImage.SetActive(true);
            else
                ShadowAfterImage.SetActive(false);
        }
    }

    void FixedUpdate() {
        if (!PauseMenu.GameIsPaused)
        {
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("1HAttack 1") || anim.GetCurrentAnimatorStateInfo(0).IsName("1HAttack 2") || anim.GetCurrentAnimatorStateInfo(0).IsName("1HAttack 3") || anim.GetCurrentAnimatorStateInfo(0).IsName("JumpAttack") || anim.GetCurrentAnimatorStateInfo(0).IsName("Crouch Slash") || anim.GetCurrentAnimatorStateInfo(0).IsName("DualAttack1") || anim.GetCurrentAnimatorStateInfo(0).IsName("DualAttack2") || anim.GetCurrentAnimatorStateInfo(0).IsName("DualAttack3") || anim.GetCurrentAnimatorStateInfo(0).IsName("GreatAttack1") || anim.GetCurrentAnimatorStateInfo(0).IsName("GreatAttack2") || anim.GetCurrentAnimatorStateInfo(0).IsName("GreatAttack3"))
                isAttacking = true;
            else
                isAttacking = false;
            if (anim.GetCurrentAnimatorStateInfo(0).IsName("Idle") && noOfClicks > 4 || anim.GetCurrentAnimatorStateInfo(0).IsName("Run") && noOfClicks > 4)
                noOfClicks = 0;
            //movement
            
            if (isAttacking && isGrounded)
            {
                rb.velocity = new Vector3(0, rb.velocity.y, 0);
                if (WeaponTrail != null)
                    ActivateTrail();
                return;
            }
            else if (isAttacking && !isGrounded)
            {
                if (WeaponTrail != null)
                    ActivateTrail();
                return;
            }
            else if (!playerdeath || !isAttacking)
            {
                if (WeaponTrail != null)
                    DeactivateTrail();
                float move = Input.GetAxis("Horizontal");

                if (!isCrouching || !isDashing)
                    rb.velocity = new Vector3(move * PlayerSpeed, rb.velocity.y, 0);
                anim.SetFloat("Speed", Mathf.Abs(move));
                if (!isDashing)
                {
                    if (move > 0 && !facingRight)
                        FlipRight();
                    else if (move < 0 && facingRight)
                        FlipLeft();
                }
            }
        }
    }

    void FlipLeft() {
        facingRight = !facingRight;
        transform.eulerAngles = new Vector3(0, 270, 0);
    }
    void FlipRight() {
        facingRight = !facingRight;
        transform.eulerAngles = new Vector3(0, 90, 0);
    }
    void JumpingAndLanding() {
        //jump and doublejump
        if (Input.GetButtonDown("Jump") && !isAttacking)
        {
            if (isGrounded)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                if (doubleJump == true)
                    candoublejump = true;
            }
            else
            {
                if (candoublejump)
                {
                    anim.Play("Jump");
                    rb.velocity = new Vector3(0, 1, 0);
                    rb.AddForce(Vector3.up * (jumpForce / 1.5f), ForceMode.Impulse);
                    candoublejump = false;
                }
            }
        }
        if (!isGrounded && Input.GetButtonDown("Fire1"))
        {
            isAttacking = true;
            anim.SetBool("AttackAir", isAttacking);
            Invoke("AirAttackCooldown", 0.2f);
        }
    }
    void Dashing() {
        //grounddashing and airdashing
        if (Time.time > nextDash)
            if (isGrounded)
                canDash = true;
        if (isGrounded)
        {
            if (Input.GetButtonDown("Fire3") && canDash && !shadowPower || _rightTrig && canDash && !shadowPower)
                StartCoroutine(DashMove());
            if (Input.GetButtonDown("Fire3") && canDash && shadowPower || _rightTrig && canDash && shadowPower)
                StartCoroutine(SuperDashMove());
        }
        else if (!isGrounded)
        {
            if (Input.GetButtonDown("Fire3") && canDash && !shadowPower || _rightTrig && canDash && !shadowPower)
                StartCoroutine(DashMove());
            if (Input.GetButtonDown("Fire3") && canDash && shadowPower || _rightTrig && canDash && shadowPower)
                StartCoroutine(SuperDashMove());
        }
    }

    IEnumerator DashMove()
    {
        if (facingRight)
        {
            rb.velocity = new Vector3(0, 0, 0);
            isDashing = true;
            rb.AddForce(dashForce, 0, 0);
            anim.Play("Dash");
            canDash = false;
            nextDash = Time.time + dashCooldown;
            NormalAfterImage.SetActive(true);
            yield return 0;
            rb.AddForce(dashForce, 0, 0);
            rb.velocity = new Vector3(0, 0, 0);
            yield return 0;
            rb.AddForce(dashForce, 0, 0);
            rb.velocity = new Vector3(0, 0, 0);
            yield return 0;
            rb.AddForce(dashForce, 0, 0);
            rb.velocity = new Vector3(0, 0, 0);
            yield return 0;
            rb.AddForce(dashForce, 0, 0);
            rb.velocity = new Vector3(0, 0, 0);
            yield return 0;
            rb.AddForce(dashForce, 0, 0);
            rb.velocity = new Vector3(0, 0, 0);
            yield return 0;
            rb.AddForce(dashForce, 0, 0);
            rb.velocity = new Vector3(0, 0, 0);
            yield return 0;
            rb.AddForce(dashForce, 0, 0);
            rb.velocity = new Vector3(0, 0, 0);
            yield return 0;
            NormalAfterImage.SetActive(false);
            isDashing = false;
        }
        else if (!facingRight)
        {
            rb.velocity = new Vector3(0, 0, 0);
            isDashing = true;
            rb.AddForce(-dashForce, 0, 0);
            anim.Play("Dash");
            canDash = false;
            nextDash = Time.time + dashCooldown;
            NormalAfterImage.SetActive(true);
            yield return 0;
            rb.AddForce(-dashForce, 0, 0);
            rb.velocity = new Vector3(0, 0, 0);
            yield return 0;
            rb.AddForce(-dashForce, 0, 0);
            rb.velocity = new Vector3(0, 0, 0);
            yield return 0;
            rb.AddForce(-dashForce, 0, 0);
            rb.velocity = new Vector3(0, 0, 0);
            yield return 0;
            rb.AddForce(-dashForce, 0, 0);
            rb.velocity = new Vector3(0, 0, 0);
            yield return 0;
            rb.AddForce(-dashForce, 0, 0);
            rb.velocity = new Vector3(0, 0, 0);
            yield return 0;
            rb.AddForce(-dashForce, 0, 0);
            rb.velocity = new Vector3(0, 0, 0);
            yield return 0;
            rb.AddForce(-dashForce, 0, 0);
            rb.velocity = new Vector3(0, 0, 0);
            yield return 0;
            NormalAfterImage.SetActive(false);
            isDashing = false;
        }
    }
    IEnumerator SuperDashMove()
    {
        if (facingRight)
        {
            rb.velocity = new Vector3(0, 0, 0);
            isDashing = true;
            isShadowing = true;
            SlowTime2f();
            rb.AddForce(Vector3.right * superDashForce, ForceMode.Impulse);
            anim.Play("Dash");
            canDash = false;
            emitDashCooldown = true;
            nextShadow = Time.time + shadowCooldown;
            SlowTime2f();
            yield return new WaitForSeconds(0.2f);
            SlowTime4f();
            yield return new WaitForSeconds(0.1f);
            SlowTime6f();
            yield return new WaitForSeconds(0.1f);
            SlowTime8f();
            yield return new WaitForSeconds(0.05f);
            TimeNormal();
            isDashing = false;
            isShadowing = false;
        }
        else if (!facingRight)
        {
            rb.velocity = new Vector3(0, 0, 0);
            isDashing = true;
            isShadowing = true;
            SlowTime2f();
            rb.AddForce(Vector3.left * superDashForce, ForceMode.Impulse);
            anim.Play("Dash");
            canDash = false;
            emitDashCooldown = true;
            nextShadow = Time.time + shadowCooldown;
            SlowTime2f();
            yield return new WaitForSeconds(0.2f);
            SlowTime4f();
            yield return new WaitForSeconds(0.1f);
            SlowTime6f();
            yield return new WaitForSeconds(0.1f);
            SlowTime8f();
            yield return new WaitForSeconds(0.05f);
            TimeNormal();
            isDashing = false;
            isShadowing = false;
        }
    }

    void AirAttackCooldown()
    {
        isAttacking = false;
        anim.SetBool("AttackAir", isAttacking);
    }
    void ComboStarter()
    {
        if (canClick)
            noOfClicks++;
        if (noOfClicks == 1)
        {
            anim.SetInteger("AttackGround", noOfClicks);
            anim.SetInteger("WeaponType", weaponType);
        }
    }
    public void ComboCheck()
    {
        canClick = false;

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("1HAttack 1") && noOfClicks == 1)
        {//If the first animation is still playing and only 1 click has happened, return to idle  
            anim.SetInteger("AttackGround", 0);
            canClick = true;
            noOfClicks = 0;
        }
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("1HAttack 1") && noOfClicks >= 2)
        {//If the first animation is still playing and at least 2 clicks have happened, continue the combo          
            anim.SetInteger("AttackGround", 2);
            canClick = true;
        }
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("1HAttack 2") && noOfClicks == 2)
        {  //If the second animation is still playing and only 2 clicks have happened, return to idle          
            anim.SetInteger("AttackGround", 0);
            canClick = true;
            noOfClicks = 0;
        }
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("1HAttack 2") && noOfClicks < 3)
        {  //Failproof return to idle       
            anim.SetInteger("AttackGround", 0);
            canClick = true;
            noOfClicks = 0;
        }
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("1HAttack 2") && noOfClicks >= 3)
        {  //If the second animation is still playing and at least 3 clicks have happened, continue the combo         
            anim.SetInteger("AttackGround", 3);
            canClick = true;
        }
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("1HAttack 3"))
        { //Since this is the third and last animation, return to idle                  
            anim.SetInteger("AttackGround", 0);
            canClick = true;
            noOfClicks = 0;
        }
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("1HAttack 3") && noOfClicks >= 3)
        { //Failproof return to idle               
            anim.SetInteger("AttackGround", 0);
            canClick = true;
            noOfClicks = 0;
        }
        else
        {
            anim.SetInteger("AttackGround", 0);
            canClick = true;
            noOfClicks = 0;
        }
    }
    public void ComboCheckDual()
    {
        canClick = false;

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("DualAttack1") && noOfClicks == 1)
        {//If the first animation is still playing and only 1 click has happened, return to idle  
            anim.SetInteger("AttackGround", 0);
            canClick = true;
            noOfClicks = 0;
        }
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("DualAttack1") && noOfClicks >= 2)
        {//If the first animation is still playing and at least 2 clicks have happened, continue the combo          
            anim.SetInteger("AttackGround", 2);
            canClick = true;
        }
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("DualAttack2") && noOfClicks == 2)
        {  //If the second animation is still playing and only 2 clicks have happened, return to idle          
            anim.SetInteger("AttackGround", 0);
            canClick = true;
            noOfClicks = 0;
        }
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("DualAttack2") && noOfClicks < 3)
        {  //Failproof return to idle       
            anim.SetInteger("AttackGround", 0);
            canClick = true;
            noOfClicks = 0;
        }
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("DualAttack2") && noOfClicks >= 3)
        {  //If the second animation is still playing and at least 3 clicks have happened, continue the combo         
            anim.SetInteger("AttackGround", 3);
            canClick = true;
        }
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("DualAttack3"))
        { //Since this is the third and last animation, return to idle                  
            anim.SetInteger("AttackGround", 0);
            canClick = true;
            noOfClicks = 0;
        }
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("DualAttack3") && noOfClicks >= 3)
        { //Failproof return to idle               
            anim.SetInteger("AttackGround", 0);
            canClick = true;
            noOfClicks = 0;
        }
        else
        {
            anim.SetInteger("AttackGround", 0);
            canClick = true;
            noOfClicks = 0;
        }
    }
    public void ComboCheckGreat()
    {
        canClick = false;

        if (anim.GetCurrentAnimatorStateInfo(0).IsName("GreatAttack1") && noOfClicks == 1)
        {//If the first animation is still playing and only 1 click has happened, return to idle  
            anim.SetInteger("AttackGround", 0);
            canClick = true;
            noOfClicks = 0;
        }
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("GreatAttack1") && noOfClicks >= 2)
        {//If the first animation is still playing and at least 2 clicks have happened, continue the combo          
            anim.SetInteger("AttackGround", 2);
            canClick = true;
        }
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("GreatAttack2") && noOfClicks == 2)
        {  //If the second animation is still playing and only 2 clicks have happened, return to idle          
            anim.SetInteger("AttackGround", 0);
            canClick = true;
            noOfClicks = 0;
        }
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("GreatAttack2") && noOfClicks < 3)
        {  //Failproof return to idle       
            anim.SetInteger("AttackGround", 0);
            canClick = true;
            noOfClicks = 0;
        }
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("GreatAttack2") && noOfClicks >= 3)
        {  //If the second animation is still playing and at least 3 clicks have happened, continue the combo         
            anim.SetInteger("AttackGround", 3);
            canClick = true;
        }
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("GreatAttack3"))
        { //Since this is the third and last animation, return to idle                  
            anim.SetInteger("AttackGround", 0);
            canClick = true;
            noOfClicks = 0;
        }
        else if (anim.GetCurrentAnimatorStateInfo(0).IsName("GreatAttack3") && noOfClicks >= 3)
        { //Failproof return to idle               
            anim.SetInteger("AttackGround", 0);
            canClick = true;
            noOfClicks = 0;
        }
        else
        {
            anim.SetInteger("AttackGround", 0);
            canClick = true;
            noOfClicks = 0;
        }
    }

    public void ActivateTrail() {
        foreach (GameObject weapon in WeaponTrail)
            weapon.GetComponent<XftWeapon.XWeaponTrail>().Activate(); }
    public void DeactivateTrail() {
        foreach (GameObject weapon in WeaponTrail)
            weapon.GetComponent<XftWeapon.XWeaponTrail>().Deactivate(); }
    public void SlowTime2f() { Time.timeScale = 0.2f; }
    public void SlowTime4f() { Time.timeScale = 0.4f; }
    public void SlowTime6f() { Time.timeScale = 0.6f; }
    public void SlowTime8f() { Time.timeScale = 0.8f; }
    public void TimeNormal() { Time.timeScale = 1f; }
    public void SmallMove() {
        StartCoroutine(SmallMoveRoutine());
    }
    public void MediumMove()
    {
        StartCoroutine(MediumMoveRoutine());
    }
    public void BigMove() {
        StartCoroutine(BigMoveRoutine());
    }

    IEnumerator SmallMoveRoutine()
    {
        if (facingRight)
        {
            rb.AddForce(20 * dashForce * Time.deltaTime, 0, 0);
            yield return new WaitForSeconds(0.1f);
            rb.AddForce(20 * dashForce * Time.deltaTime, 0, 0);
            yield return new WaitForSeconds(0.1f);
            rb.AddForce(20 * dashForce * Time.deltaTime, 0, 0);
            yield return new WaitForSeconds(0.1f);
        }
        if (!facingRight)
        {
            rb.AddForce(-20 * dashForce * Time.deltaTime, 0, 0);
            yield return new WaitForSeconds(0.1f);
            rb.AddForce(-20 * dashForce * Time.deltaTime, 0, 0);
            yield return new WaitForSeconds(0.1f);
            rb.AddForce(-20 * dashForce * Time.deltaTime, 0, 0);
            yield return new WaitForSeconds(0.1f);

        }

        yield return 0;
    }
    IEnumerator MediumMoveRoutine()
    {
        if (facingRight)
        {
            rb.AddForce(40 * dashForce * Time.deltaTime, 0, 0);
            yield return new WaitForSeconds(0.1f);
            rb.AddForce(40 * dashForce * Time.deltaTime, 0, 0);
            yield return new WaitForSeconds(0.1f);
            rb.AddForce(40 * dashForce * Time.deltaTime, 0, 0);
            yield return new WaitForSeconds(0.1f);
        }
        if (!facingRight)
        {
            rb.AddForce(-40 * dashForce * Time.deltaTime, 0, 0);
            yield return new WaitForSeconds(0.1f);
            rb.AddForce(-40 * dashForce * Time.deltaTime, 0, 0);
            yield return new WaitForSeconds(0.1f);
            rb.AddForce(-40 * dashForce * Time.deltaTime, 0, 0);
            yield return new WaitForSeconds(0.1f);

        }

        yield return 0;
    }
    IEnumerator BigMoveRoutine()
    {
        if (facingRight)
        {
            rb.AddForce(60 * dashForce * Time.deltaTime, 0, 0);
            yield return new WaitForSeconds(0.1f);
            rb.AddForce(60 * dashForce * Time.deltaTime, 0, 0);
            yield return new WaitForSeconds(0.1f);
            rb.AddForce(60 * dashForce * Time.deltaTime, 0, 0);
            yield return new WaitForSeconds(0.1f);
        }
        if (!facingRight)
        {
            rb.AddForce(-60 * dashForce * Time.deltaTime, 0, 0);
            yield return new WaitForSeconds(0.1f);
            rb.AddForce(-60 * dashForce * Time.deltaTime, 0, 0);
            yield return new WaitForSeconds(0.1f);
            rb.AddForce(-60 * dashForce * Time.deltaTime, 0, 0);
            yield return new WaitForSeconds(0.1f);

        }

        yield return 0;
    }

    private bool isLeftBeingPressed = false;
    private bool isRightBeingPressed = false;
    private void LeftTrigger()
    {
        if (LeftTriggerAxis > 0)
        {
            if (!isLeftBeingPressed)
                _leftTrig = true;
            else if (isLeftBeingPressed)
                _leftTrig = false;
            isLeftBeingPressed = true;

        }
        else if (LeftTriggerAxis == 0)
        {
            isLeftBeingPressed = false;
        }
    }
    private void RightTrigger()
    {
        if (RightTriggerAxis > 0)
        {
            if (!isRightBeingPressed)
                _rightTrig = true;
            else if (isRightBeingPressed)
                _rightTrig = false;
            isRightBeingPressed = true;
        }
        else if (RightTriggerAxis == 0)
        {
            isRightBeingPressed = false;
        }
    }
}
