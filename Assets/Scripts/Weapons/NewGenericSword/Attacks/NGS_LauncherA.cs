using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Luminosity.IO;

public class NGS_LauncherA : NGS_AttackClass {

    public int attackLevel = 1;

    public GameObject launcherATarget;
    public bool landedLauncherA;

    public GameObject Level1ParticlesFX;

    Coroutine checkForBreakConditions;
    bool paraTrowRunning;
    public bool breakConditionsMet;
    bool paraThrowRunning;

    float normalizedTimeA;
   // public bool isObsolete = true; //bool para throwenemylauncher en customMethods

    private Coroutine coro1;
    private Coroutine coro2;
    private Coroutine coro3;
    private Coroutine coro4;
    private Coroutine coro5;

    // Use this for initialization

    public override void Start() {
        base.Start();
    }

    public override void ForcedReset() {
        boxCollider.size = swordCPU.boxColliderOrigSize;
        boxCollider.center = swordCPU.boxColliderOrigCenter;
        landedLauncherA = false;
        launcherATarget = null;
        movementScript.doNotMove = false;
        movementScript.cantShoot = false;
        movementScript.cantStab = false;
        playerAnimation.isAttackingMelee = false;
        breakConditionsMet = true;
        paraThrowRunning = false;
        //isObsolete = true;

        if (swordCPU.CoroSpecialCororunning != null)
            StopCoroutine(swordCPU.CoroSpecialCororunning);
        if (checkForBreakConditions != null)
            StopCoroutine(checkForBreakConditions);
        //swordCPU.AttackDone();
    }

    // Update is called once per frame
    void Update() {
        if (movementScript.isGrounded && !movementScript.isJumping && cooldown <= 0f) {
            if (InputManager.GetButtonDown("Launcher") &&
                (!InputManager.GetButton("Combo Mode") || InputManager.GetAxis("Vertical") < 0.2f && InputManager.GetAxis("Vertical") > -0.2f) &&
                !movementScript.isJumping) {
                swordCPU.AttackReady(this);
            }
        }
        if (cooldown > 0f)
            cooldown -= Time.deltaTime;
    }

    public void StopAllCoroutinesExceptThrowEnemy() {
        if (coro2 != null) {
            StopCoroutine(coro2);
        }
        if (coro3 != null) {
            StopCoroutine(coro3);
        }
        if (coro4 != null) {
            StopCoroutine(coro4);
        }
        if (coro5 != null) {
            StopCoroutine(coro5);
        }
    }

    public override bool SpecialOntriggerenter(Collider collider) {
        if (!landedLauncherA) {
            landedLauncherA = true;
            launcherATarget = collider.gameObject;
        }
        return true;
    }

    public override IEnumerator ExecuteAttack() {
        CheckCurrentAnimatorState();
        //isObsolete = false;
        breakConditionsMet = false;
        movementScript.cantShoot = true;
        movementScript.cantStab = true;
        movementScript.doNotMove = true;
        playerAnimation.isAttackingMelee = true;
        SpeedMultiplier = playerAnimation.finalSpeedMultiplier;
        cooldown = 0.8f;         

        if (!InputManager.GetButton("Combo Mode")) {
            GameObject FoundTarget = CustomMethods.SearchForTargetInDirection(Player);
            if (FoundTarget)
                StartCoroutine(CustomMethods.SmoothRotateTowards(Player, FoundTarget.transform, 0.125f));
        }

        playerAnimation.meleeAttack = 9;
        playerAnimation.attackLevel = attackLevel;

        if (attackLevel <= 1) {
            playerAnimation.attackLevel = 1f;
            swordCPU.attackDamage = swordCPU.baseDamage * 1.125f;
            meleeWeaponTrail._trailObject.GetComponent<Renderer>().material = meleeWeaponTrail._material;
        } else if (attackLevel == 2) {
            playerAnimation.attackLevel = 2f;
            swordCPU.attackDamage = swordCPU.baseDamage * 1.225f;
            meleeWeaponTrail._trailObject.GetComponent<Renderer>().material = meleeWeaponTrail._materialBlue;
        } else if (attackLevel == 3) {
            playerAnimation.attackLevel = 3f;
            swordCPU.attackDamage = swordCPU.baseDamage * 1.35f;
            meleeWeaponTrail._trailObject.GetComponent<Renderer>().material = meleeWeaponTrail._materialRed;
        }

        if (Level1ParticlesFX) {
            particlesRunning = Instantiate(Level1ParticlesFX, Player.transform.position + Player.transform.forward * 0.5f + Player.transform.right * 1 / 4 + Player.transform.up * 0.25f, Player.transform.rotation * Quaternion.Euler(-40f, -40f, -240f));
            particlesRunning.transform.parent = null;
            ParticleSystem.MainModule partSys = particlesRunning.GetComponent<ParticleSystem>().main;
            partSys.startSpeedMultiplier = partSys.startSpeedMultiplier * SpeedMultiplier;
            Destroy(particlesRunning, 1f);
        }

        boxCollider.size = new Vector3(swordCPU.boxColliderOrigSize.x * 1.5f, swordCPU.boxColliderOrigSize.y * 4f, swordCPU.boxColliderOrigSize.z);
        boxCollider.center = new Vector3(swordCPU.boxColliderOrigCenter.x, swordCPU.boxColliderOrigCenter.y * 20F, swordCPU.boxColliderOrigCenter.z);
        Player.GetComponent<Collider>().material.dynamicFriction = 1.0f;
        rigidbody.velocity = Vector3.zero;
        rigidbody.AddForce(Player.transform.forward * 4f, ForceMode.VelocityChange);
        Player.transform.rotation = Player.transform.rotation * Quaternion.Euler(0f, -40f, 0f);

        while (!swordCPU.damageCol) {
            yield return null;
        }

        rigidbody.AddForce(Player.transform.forward * 2f, ForceMode.VelocityChange);

        while (swordCPU.damageCol && !landedLauncherA) {
            yield return null;
        }

        Player.transform.rotation = Player.transform.rotation * Quaternion.Euler(0f, 40f, 0f);
        rigidbody.velocity = Vector3.zero;

        if (/*InputManager.GetButton("Launcher") &&*/ InputManager.GetButton("Jump") && movementScript.jumpsLeft > 1) {
            if (landedLauncherA) {
                rigidbody.velocity = Vector3.zero;
                Rigidbody LauncherATargetRigidbody = launcherATarget.GetComponent<Rigidbody>();
                coro1 = StartCoroutine(ThrowEnemy(LauncherATargetRigidbody));
                coro2 = StartCoroutine(CustomMethods.MoveRigidbodyTogetherLauncher(this, Player, launcherATarget, new Vector3(0, 0.25f, -Vector3.Distance(Player.transform.position, launcherATarget.transform.position)), 2.0f));
                coro3 = StartCoroutine(MoveTogetherCancel(launcherATarget));
                swordCPU.Player.GetComponent<References>().enemiesDetector.target = launcherATarget;
            } else {
                rigidbody.velocity = Vector3.zero;
                coro5 = StartCoroutine(ThrowEnemy(rigidbody));
            }

            playerAnimation.meleeAttack = 10;
            yield return new WaitForSeconds(0.1f / SpeedMultiplier);
            movementScript.isJumping = true;
            movementScript.jumpsLeft -= 2;
            movementScript.cantShoot = false;

            animationTimer = 3.0f;
            while (TerminateAnimation() && !movementScript.isGrounded && animationTimer > 0f) {
                animationTimer -= Time.deltaTime;
                yield return null;
            }
            /*
            boxCollider.size = swordCPU.boxColliderOrigSize;
            boxCollider.center = swordCPU.boxColliderOrigCenter;
            landedLauncherA = false;
            launcherATarget = null;
            movementScript.cantShoot = false;
            movementScript.doNotMove = false;
            movementScript.cantStab = false;
            playerAnimation.isAttackingMelee = false;
            swordCPU.AttackDone();
            yield break;
            */

        } else {
            if (landedLauncherA) {
                Rigidbody LauncherATargetRigidbody = launcherATarget.GetComponent<Rigidbody>();
                coro1 = StartCoroutine(ThrowEnemy(LauncherATargetRigidbody));
            }
            movementScript.cantShoot = false;
            yield return new WaitForSeconds(0.1f / SpeedMultiplier);
            if (!CustomMethods.CheckDisplacementInput() && landedLauncherA) {
                yield return null;
            } else
                yield return new WaitForSeconds(0.15f / SpeedMultiplier);

            animationTimer = 1.5f;
            while (animationTimer > 0f) {
                if (TerminateAnimation()) {
                    animationTimer -= Time.deltaTime;
                    yield return null;
                } else
                    break;
            }
        }

        boxCollider.size = swordCPU.boxColliderOrigSize;
        boxCollider.center = swordCPU.boxColliderOrigCenter;
        landedLauncherA = false;
        launcherATarget = null;
        movementScript.cantShoot = false;
        movementScript.doNotMove = false;
        movementScript.cantStab = false;
        playerAnimation.isAttackingMelee = false;
        swordCPU.AttackDone();
        yield break;
    }

    IEnumerator ThrowEnemy(Rigidbody Target) {
        HealthBar health = Target.GetComponent<HealthBar>();
        Coroutine coro4 = StartCoroutine(MoveRigidBody(Target));

        float duration = 9f;
        normalizedTimeA = 0f;
        while (!breakConditionsMet) {
            if (normalizedTimeA > 0f) {
                if (health.justGotHurt) {
                    breakConditionsMet = true;
                    StopCoroutine(coro4);
                    Target.velocity = Vector3.zero;
                    break;
                }
            }
            normalizedTimeA += Time.deltaTime + (Time.deltaTime - Time.deltaTime * Time.deltaTime) / duration;
            yield return null;
        }

        breakConditionsMet = true;
        paraThrowRunning = false;
        StopCoroutine(coro4);        
        yield return null;
    }
    IEnumerator MoveRigidBody(Rigidbody Target) {
        HealthBar health = Target.GetComponent<HealthBar>();
        Target.velocity = Vector3.zero;

        Vector3 startPos = Target.transform.position;
        Vector3 tempPos = startPos;
        Vector3 prevPos = startPos;
        Vector3 destination = swordCPU.Player.transform.position + swordCPU.Player.transform.forward * 2f;

        CapsuleCollider capsuleCollider = Target.gameObject.GetComponent<CapsuleCollider>();

        float height = swordCPU.movementScript.jumpBaseForce * 3 / 4f;

        while (!breakConditionsMet || health.justGotHurt) {
            prevPos = tempPos;
            float yOffset = height * 4.0f * (normalizedTimeA - normalizedTimeA * normalizedTimeA); // formula para la altura
            tempPos = CustomMethods.Vector3LerpUnclamped(startPos, destination, normalizedTimeA) + yOffset * Vector3.up;
            RaycastHit hitPoint;
            Debug.DrawRay(prevPos + (destination - startPos).normalized * capsuleCollider.radius / 2f,
               (tempPos - prevPos).normalized * 2f, Color.red, 0.01f, false);
            if (Physics.Raycast(prevPos + (destination - startPos).normalized * capsuleCollider.radius / 2f, tempPos - prevPos, out hitPoint, capsuleCollider.radius * 2, LayerMask.GetMask("Scenario", "Default"), QueryTriggerInteraction.Ignore)) { //detectar colision y mover al punto de impacto
                Target.MovePosition(hitPoint.point + ((prevPos - hitPoint.point).normalized * capsuleCollider.radius) + Target.transform.forward * (-0.5f) + Vector3.up);
                breakConditionsMet = true;
                Target.velocity = Vector3.zero;
                Target.gameObject.GetComponent<References>().Landing();
                Target.gameObject.GetComponent<References>().LandingSound();
                yield return new WaitForFixedUpdate();
                Target.velocity = Vector3.zero;
                break;
            } else {
                Target.MovePosition(Vector3.Lerp(Target.transform.position, tempPos, 0.25f));
                yield return new WaitForFixedUpdate();
            }
        }
        Target.velocity = Vector3.zero;
    }

    IEnumerator MoveTogetherCancel(GameObject Target) {
        HealthBar health = Target.GetComponent<HealthBar>();
        float durationC = 9f;
        float normalizedTimeC = 0.0f;
        breakConditionsMet = false;
        while (!breakConditionsMet) {
            if (normalizedTimeC > 0f && health.justGotHurt) {
                    breakConditionsMet = true;
                break;
            }
            normalizedTimeC += Time.deltaTime + (Time.deltaTime - Time.deltaTime * Time.deltaTime) / durationC;
            yield return null;
        }
        yield return null;
    }
}

