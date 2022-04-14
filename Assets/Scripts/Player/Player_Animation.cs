using UnityEngine;
using System.Collections;
using Luminosity.IO;

public class Player_Animation : MonoBehaviour {

    public float idleAnimation;
    public bool ownerIsAI;

    private Transform Camera;
    public Animator anim;
    private References references;
    private Hero_Movement Player;
    private Third_Person_Camera CameraAim;
    private Rigidbody rigidbody;
    private int updateTimer;

    public int updateDelay;
    public float SpeedMultiplierA;  // Aura toggled ON;
    public float SpeedMultiplierB;  // Sword Specific Slash speed;
    private float BaseSpeedMultiplier;
    private float penaltySpeedDivider = 1f;
    public float finalSpeedMultiplier;

    public float cameraY;
    private Vector3 inputDirection;
    public float inputH;
    public float inputV;
    public float finalWeaponType;
    public float meleeWeaponType;
    public int meleeSlot;
    public float gunWeaponType;
    public int gunSlot;
    public float displacementSpeed;
    public float meleeAttack;
    public float attackLevel;
    public float landingStrenght;

    public bool isMoving;

    public bool isJumping;
    public bool isDoubleJumping;
    public bool isBouncing;
    public bool isFalling;
    public float fallingSpeed;
    public bool isGrounded;
    public bool isCrouching;

    public bool isAttackingMelee;
    public bool isAttackingMeleeAir;
    public bool isAiming;
    public bool rise;
    public bool fire;
    public bool isReloading;

    public bool blocking;
    public bool blockingImpact;
    public bool blockingBullet;
    public int blockingBulletAnimation;
    public bool blockingCounter;
    public Vector3 blockingDirection;
    private float blockingDirectionX;
    private float blockingDirectionZ;

    public bool comboMode;
    public bool isDead;
    private bool justDied;
    public bool inPain;
    public bool isStandingUp;
    public float damageIntensity;
    public float finalDamageIntensity;

    public bool lHandFist;
    public bool lHandExtended;
    public bool lHandTrigger;

    public bool isSaving;

    // Camera, look up or down

    private bool alreadyDead;
    private bool slotSelectedZeroGun;
    private bool slotSelectedZeroSword;

    private float freeHandsTimer;

    private bool hasDistortedAura = false;

    // Use this for initialization
    void Start() {

        anim = GetComponent<Animator>();
        ResetValues();

        Player = GetComponent<Hero_Movement>();
        references = GetComponent<References>();
        Camera = GetComponent<References>().Camera.transform;
        CameraAim = Camera.GetComponent<Third_Person_Camera>();
        anim.SetBool("isDead", false);
        updateTimer = 0;
        anim.SetBool("isAI", ownerIsAI);
        BaseSpeedMultiplier = 1f;
        SpeedMultiplierA = 1f;
        SpeedMultiplierB = 1f;
        rigidbody = GetComponent<Rigidbody>();

        if (references._DistortedAuraPS)
            hasDistortedAura = true;
    }

    public void ResetValues() {
        isJumping = false;
        isBouncing = false;
        isMoving = false;
        comboMode = false;
        isAttackingMelee = false;
        isAttackingMeleeAir = false;
        isAiming = false;
        isReloading = false;
        inputH = 0f;
        inputV = 0f;
    }

    // Update is called once per frame
    private void Update() {
       //FinalWeaponRotation();
    }

    public void PenalizeSpeed(bool toggle, float amount) {
        if (toggle && amount > 1f) { 
            penaltySpeedDivider = amount;
        } else
            penaltySpeedDivider = 1f;
    }

    void OnAnimatorMove() {

        if (updateTimer >= updateDelay) {
            if (!ownerIsAI) {
                if (!isDead) {
                    if (finalWeaponType > 6 && (isAiming || fire || rise || isMoving || isReloading)) {
                        anim.SetLayerWeight(5, Mathf.Lerp(anim.GetLayerWeight(5), 1f, 20f * Time.deltaTime));
                        //anim.SetLayerWeight (6, Mathf.Lerp (anim.GetLayerWeight (6), 1f, 2.5f * Time.deltaTime));
                    } else if (finalWeaponType <= 6 && finalWeaponType > 5 && (isAiming || fire || rise || isMoving || isReloading)) {
                        anim.SetLayerWeight(5, Mathf.Lerp(anim.GetLayerWeight(5), 1f, 20f * Time.deltaTime));
                        //anim.SetLayerWeight (6, Mathf.Lerp (anim.GetLayerWeight (6), 1f, 2.5f * Time.deltaTime));
                    } else if (/*!isCrouching &&*/ finalWeaponType <= 5 && finalWeaponType > 4) {
                        if (!fire && (isAiming || rise || isMoving || isReloading)) {
                            anim.SetLayerWeight(5, Mathf.Lerp(anim.GetLayerWeight(5), 1f, 20f * Time.deltaTime));
                        } else /*if (fire)*/{
                            anim.SetLayerWeight(5, Mathf.Lerp(anim.GetLayerWeight(5), 0f, 20f * Time.deltaTime));
                        }
                    } else {
                        anim.SetLayerWeight(5, Mathf.Lerp(anim.GetLayerWeight(5), 0f, 20f * Time.deltaTime));
                        //anim.SetLayerWeight (6, Mathf.Lerp (anim.GetLayerWeight (6), 0f, 2.5f * Time.deltaTime));
                    }
                } else {
                    anim.SetLayerWeight(5, Mathf.Lerp(anim.GetLayerWeight(5), 0f, 20f * Time.deltaTime));
                    //anim.SetLayerWeight (6, Mathf.Lerp (anim.GetLayerWeight (6), 0f, 2.5f * Time.deltaTime));
                }

                if (Player.isGroundedAndStable || Player.isGrounded)
                    isGrounded = true;
                else
                    isGrounded = false;


                if (!isGrounded || Player.isFalling)
                    isFalling = true;
                else
                    isFalling = false;


                //if (isJumping || !Player.doNotMove) {

                isJumping = GetComponent<Hero_Movement>().isJumping;
                isBouncing = GetComponent<Hero_Movement>().isBouncing;

                if (!Player.doNotMove) {
                    comboMode = InputManager.GetButton("Combo Mode");

                    inputDirection.x = InputManager.GetAxis("Horizontal");
                    inputDirection.z = InputManager.GetAxis("Vertical");
                    inputV = Mathf.Lerp(inputH, transform.InverseTransformDirection(rigidbody.velocity).z, 10f * Time.deltaTime);
                    inputH = Mathf.Lerp(inputV, transform.InverseTransformDirection(rigidbody.velocity).x, 10f * Time.deltaTime);


                    if (isGrounded) {
                        isAiming = InputManager.GetButton("Aim") || Player.lockedAimMode;
                    }

                    if (InputManager.GetAxis("Vertical") != 0 | InputManager.GetAxis("Horizontal") != 0)
                        isMoving = true;
                    else
                        isMoving = false;

                    if (isMoving && Player.magnitude <= 0.99f) {
                        displacementSpeed = Mathf.Lerp(displacementSpeed, 1f, 0.5f);
                    } else if (!isReloading && isMoving && InputManager.GetButton("Sprint") && Player.sprintTimer > 0.5f && Player.magnitude > 0.99f && !InputManager.GetButton("Aim") && !InputManager.GetButton("Combo Mode") && !Player.lockedAimMode && !Player.lockedComboMode) {
                        displacementSpeed = Mathf.Lerp(displacementSpeed, 3f, 0.5f);

                    } else if (!isReloading && InputManager.GetButton("Sprint") && Player.lockedAimMode && InputManager.GetAxis("Horizontal") == 0f) {
                        displacementSpeed = Mathf.Lerp(displacementSpeed, 3f, 0.5f);
                    } else if (isMoving) {
                        displacementSpeed = Mathf.Lerp(displacementSpeed, 2f, 0.5f);
                    }

                    if (InputManager.GetButton("Aim") || Player.lockedAimMode) {
                        cameraY = CameraAim.camYRotation + 5f;
                    } else if (!Player.doNotMove && InputManager.GetButton("Combo Mode") || Player.lockedComboMode) {
                        cameraY = CameraAim.camYRotation;
                    } else {
                        cameraY = Mathf.Lerp(cameraY, 0f, 0.5f);
                    }
                }
                //}

                fallingSpeed = -Player.fallingSpeed;

                blockingDirection = transform.InverseTransformDirection(blockingDirection);
                blockingDirectionX = Mathf.Lerp(blockingDirectionX, blockingDirection.x, 0.4f);
                blockingDirectionZ = Mathf.Lerp(blockingDirectionZ, blockingDirection.z, 0.4f);

                FinalWeaponType();
            } else {
                inputV = Mathf.Lerp(inputH, transform.InverseTransformDirection(rigidbody.velocity).z, 20f * Time.deltaTime);
                inputH = Mathf.Lerp(inputV, transform.InverseTransformDirection(rigidbody.velocity).x, 20f * Time.deltaTime);
            }

            finalSpeedMultiplier = BaseSpeedMultiplier * SpeedMultiplierA * SpeedMultiplierB;

            if (!isDead && damageIntensity > 0)
                damageIntensity -= Time.deltaTime / 100f;

            finalDamageIntensity = Mathf.Round(damageIntensity);
            PassValues();
            updateTimer = 0;
            fire = false;
        } else
            updateTimer = updateTimer + 1;
        /*
        if (hasDistortedAura) {
            references._DistortedAuraPS.Stop(true);
            FinalWeaponRotation();
            references._DistortedAuraPS.Play(true);
        } else*/
            FinalWeaponRotation();
    }

    void FinalWeaponType() {

        if (!ownerIsAI) {
            if (GetComponent<CycleGuns>().slotSelected == 0)
                slotSelectedZeroGun = true;
            else
                slotSelectedZeroGun = false;

            if (GetComponent<CycleSwords>().slotSelected == 0)
                slotSelectedZeroSword = true;
            else
                slotSelectedZeroSword = false;

            if (meleeWeaponType > 1f && GetComponent<CycleGuns>().currentGun.activeSelf) {
                finalWeaponType = Mathf.Lerp(finalWeaponType, meleeWeaponType, 10f * Time.deltaTime);
            } else if (gunWeaponType > 1f && GetComponent<CycleGuns>().currentGun.activeSelf) {
                finalWeaponType = Mathf.Lerp(finalWeaponType, gunWeaponType + 3f, 10f * Time.deltaTime); //+3 es el offset de acuerdo a la cantidad de tipos de melee que hayan en animator
            } else if (!isSaving || slotSelectedZeroGun && slotSelectedZeroSword) {
                finalWeaponType = Mathf.Lerp(finalWeaponType, 0f, 2.5f * Time.deltaTime);
            } else {
                finalWeaponType = Mathf.Lerp(finalWeaponType, 0f, 2.5f * Time.deltaTime);
            }
        }
    }

    void FinalWeaponRotation() {
        //Fix Rotations
        if (finalWeaponType >= 5f && (isAiming || rise || fire || isReloading)) {
            references.Spine.transform.rotation = Quaternion.Euler(references.OverShoulderCameraPos.transform.rotation.eulerAngles + Vector3.up * 60f);
        } else if (finalWeaponType >= 4f && finalWeaponType < 5f && (isAiming || rise || fire /*|| isReloading*/)) {
            if (!isMoving && isCrouching) {
                references.Spine.transform.rotation = Quaternion.Euler(references.OverShoulderCameraPos.transform.rotation.eulerAngles + Vector3.up * 60f);
                references.Head.transform.rotation = Quaternion.Euler(references.OverShoulderCameraPos.transform.rotation.eulerAngles + Vector3.up * -7.5f);
            } else if (isMoving && !isCrouching) {
                //references.LeftShoulder.transform.rotation = Quaternion.Euler (references.OverShoulderCameraPos.transform.rotation.eulerAngles + Vector3.up * 60f);
                references.LeftShoulder.transform.rotation = Quaternion.Euler(references.OverShoulderCameraPos.transform.rotation.eulerAngles + new Vector3(-100.622f, 175.064f, -55.33099f));
            }
        }
    }
    void UpdateDistortedAura() {
        if (hasDistortedAura) {
            references._DistortedAuraPS.Stop(true);
            references._DistortedAuraPS.Play(true);
        }
    }



    void PassValues() {

        if (Time.timeScale > 0) {
            if (!isDead) {
                anim.SetFloat("SpeedMultiplier", finalSpeedMultiplier);

                anim.SetFloat("cameraY", cameraY);
                anim.SetFloat("inputH", inputH);
                anim.SetFloat("inputV", inputV);

                anim.SetFloat("FinalWeaponType", finalWeaponType);
                anim.SetFloat("MeleeWeaponType", meleeWeaponType);
                anim.SetFloat("meleeSlot", meleeSlot);
                anim.SetFloat("GunWeaponType", gunWeaponType);
                anim.SetFloat("gunSlot", gunSlot);

                anim.SetFloat("IdleAnimation", idleAnimation);
                anim.SetFloat("DisplacementSpeed", displacementSpeed);
                anim.SetFloat("MeleeAttack", meleeAttack);
                anim.SetFloat("AttackLevel", attackLevel);
                anim.SetFloat("FallingSpeed", fallingSpeed);
                anim.SetFloat("LandingStrenght", landingStrenght);

                anim.SetBool("isMoving", isMoving);
                anim.SetBool("isJumping", isJumping);
                anim.SetBool("isDoubleJumping", isDoubleJumping);
                anim.SetBool("isBouncing", isBouncing);
                anim.SetBool("isFalling", isFalling);
                anim.SetBool("isGrounded", isGrounded);
                anim.SetBool("isCrouching", isCrouching);

                anim.SetBool("isAttackingMelee", isAttackingMelee);
                anim.SetBool("isAttackingMeleeAir", isAttackingMeleeAir);
                anim.SetBool("isAiming", isAiming);
                anim.SetBool("rise", rise);
                anim.SetBool("fire", fire);
                anim.SetBool("isReloading", isReloading);

                anim.SetBool("isBlocking", blocking);
                anim.SetBool("isBlockingImpact", blockingImpact);
                anim.SetBool("isBlockingBullet", blockingBullet);
                anim.SetInteger("blockingBulletAnimation", blockingBulletAnimation);
                anim.SetBool("isBlockingCounter", blockingCounter);
                anim.SetFloat("blockingDirectionX", blockingDirectionX);
                anim.SetFloat("blockingDirectionZ", blockingDirectionZ);

                anim.SetBool("comboMode", comboMode);
                anim.SetBool("isDead", isDead);
                anim.SetBool("inPain", inPain);
                anim.SetBool("isStandingUp", isStandingUp);

                if (!ownerIsAI && slotSelectedZeroGun && GetComponent<CycleGuns>().currentGun.activeSelf) {
                    anim.SetFloat("GunWeaponType", 0f);
                    lHandExtended = true;

                    lHandFist = false;
                    lHandTrigger = false;
                } else {
                    lHandExtended = false;
                }

                if (!ownerIsAI && slotSelectedZeroSword && GetComponent<CycleSwords>().currentSword.activeSelf) {
                    anim.SetFloat("MeleeWeaponType", 0f);
                } else {
                }

                anim.SetBool("LHand - Fist", lHandFist);
                anim.SetBool("LHand - Extended", lHandExtended);
                anim.SetBool("LHand - Trigger", lHandTrigger);

            } else {
                if (!justDied) {
                    anim.SetFloat("damageIntensity", Random.Range(0, 6));
                    justDied = true;
                }
                anim.Play("Death");
            }
        }
    }
}
