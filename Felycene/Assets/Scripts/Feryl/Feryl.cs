using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.SceneManagement;

public class Feryl : MonoBehaviour
{
    // ~Components
    CharacterController controller;

    //~Controls
    // gets all the predefined InputActions
    InputAction move;
    InputAction look;
    InputAction attack;
    InputAction interact;
    InputAction jump;
    InputAction dash;
    InputAction parry;
    InputAction heal;
    InputAction spell;
    InputAction pause;

    // ~Variables
    // general
    int health = 10; // player's health, (half hearts count as 1, full heart as 2)

    // upgrades


    // camera
    [SerializeField]
    GameObject pivot; // the pivot the camera works around, gameObject so it has position
    [SerializeField]
    GameObject camera; // the main camera for Feryl
    float dragRadius = 1.5f; // how far the camera's pivot can be from Feryl before it starts getting dragged
    float targetHeight = 1.5f; // how high above Feryl's position the pivot should try to be
    float pivotAcceleration = .005f; // the pivots acceleration as it moves towards the player if not dragged
    // the camera will always be -camTarget * camDistance from the pivot (or shorter, if colliding)
    Vector3 camTarget = Vector3.forward; // the target that the camera is looking at, normalized
    float camDistance = 5f; // how far the camera should be from the pivot
    float camCurrentDistance = 5f; // how far the camera currently is from the pivot (if modified to ensure smooth zooming out)
    bool frozen = false; // if true, the camera won't move at all
    bool cameraFollowing = false; // if the camera is following an object
    GameObject followObject = null; // the object the camera is following, reset to null **note change this type to one that has camera position in the parent class** 

    // movement
    Vector3 direction = Vector3.forward; // (normalized) most recent direction the player has pressed
    Vector3 moveVelocity = Vector3.zero; // the current walking velocity of the player
    float acceleration = .2f; // how fast the player speeds up and slows down (linearly, x per frame)
    float walkSpeed = 7; // player walk speed
    // air movement & jumping
    float airAcceleration = .04f; // acceleration used while the player is in the air
    float airWalkSpeed = 5; // player walk speed while in the air
    float gravity = -35; // player gravity (added as gravity*Time.deltaTime every frame while in the air)
    float maxFallSpeed = -20; // caps the player's fall speed at this
    float jumpStrength = 8; // the y velocity added when jumping // total should = about 12
    //float holdJumpStrength = 4; // y velocity added when holding jump (multiplied by the time left) **Depracated -- No longer in game**
    float maxJumpLength = .4f; // how long the player can hold the jump button to stay moving up
    float jumpLengthTimer = 0; // the current timer for the player holding jump to go up
    bool holdingJump = true; // if the player is currently holding jump to higher
    float jumpStopStrength = 2; // y velocity is divided by this number upon stopping holding jump
    bool jumpOverride = false; // if the player preinputs jump while not able to jump

    // dashing
    bool dashing = false;
    float dashSpeed = 15; // speed the player moves during a dash
    float dashCooldown = .7f; // cooldown inbetween dashes
    float dashCooldownTimer = 0; // the timer for cooldowns between dashes
    float dashLength = .25f; // how long a dash lasts
    float dashTimer = 0; // timer that tracks how long a dash lasts
    bool sprinting = false; // if the player is sprinting
    float sprintSpeed = 11; // the speed the player moves while sprinting
    float sprintRecoveryTime = .3f; // cooldown before dashing after ending sprint
    bool dashReleased = false; // if the player released the dash key
    bool dashOverride = false; // if the player preinputs dash while not able to dash


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        //Settings settings = get **make it get settings and use the settings**

        // fixing the framerate
        Application.targetFrameRate = 240; // *update to using the value in settings*

        // getting the character controller component
        controller = GetComponent<CharacterController>();

        // setting the InputActions to their inputs
        move = InputSystem.actions.FindAction("Move");
        look = InputSystem.actions.FindAction("Look");
        attack = InputSystem.actions.FindAction("Attack");
        interact = InputSystem.actions.FindAction("Interact");
        jump = InputSystem.actions.FindAction("Jump");
        dash = InputSystem.actions.FindAction("Dash");
        parry = InputSystem.actions.FindAction("Parry");
        heal = InputSystem.actions.FindAction("Heal");
        spell = InputSystem.actions.FindAction("Spell");
        pause = InputSystem.actions.FindAction("Pause"); // *note* make this call a function when pressed instead of running with feryl's code
    }

    // Update is called once per frame
    void Update() { // *note* change this to fixed update and update all the move_towards
        
        //print("fps = " + 1/Time.deltaTime);

        updateCamera();
        
        // handles all movement
        movement(controller.isGrounded);
        

        // calculating final velocity to be moved by adding up all the sources
        Vector3 totalVelocity = moveVelocity;
        // actual move function using total velocity and delta time
        controller.Move(totalVelocity*Time.deltaTime);
        //print(totalVelocity);

        updatePivot(false); // reducing jitteryness
    }

    void updatePivot(bool move=true){
        // updates the pivot, called at the beginning and end of every frame
        if (pivot.transform.position.y-targetHeight>transform.position.y){ // ensuring the pivot can't be dragged down by the player
            pivot.transform.position = new(pivot.transform.position.x, transform.position.y + targetHeight, pivot.transform.position.z);
        }

        Vector3 diff = pivot.transform.position - Vector3.up*targetHeight - transform.position; // getting the diff in position between pivot & Feryl
        if (diff.magnitude > dragRadius){ // if the pivot is too far away
            pivot.transform.position = diff.normalized * dragRadius + Vector3.up*targetHeight + transform.position; // updating position
        } else if (move){ // if it should slowly go back to centered on the player, yes for start of frame, no for end **change to yes for fixed update, no for update**
            pivot.transform.position = Vector3.MoveTowards(pivot.transform.position, Vector3.up*targetHeight + transform.position, pivotAcceleration);
        }
    }

    void updateCamera(){
        if (frozen){
            return;
        }

        // updating the pivot
        updatePivot();

        // apply mouse input to camera rotation



        // check if you are following an object, lerp camera rotation to that object



        // check for collisions and adjust distance
    }

    void movement(bool grounded) {
        if (dashPhysics()){ // if dash physics instead of walking physics
            return;
        }
        // ***add in the button overrides to allow players to preinput buttons while not able to input them***
        if (grounded) {
            groundMovement();
        } else{ // in the air
            airMovement();
        }
        
    }

    bool dashPhysics(){ // returns true if player shouldn't calculate other movement, false if it should
        // ~Dashing
        if (!dashing && !sprinting && dashCooldownTimer <= 0 && ((dash.triggered && dash.IsPressed()) || dashOverride)){ // starting a new dash

            // allowing direction change at the very start of the dash
            Vector3 moveInput = new(move.ReadValue<Vector2>().x, 0, move.ReadValue<Vector2>().y);
            // setting the direction based on the movement being input
            if (moveInput != Vector3.zero){ // making sure direction isn't updated for no reason
                direction = moveInput.normalized; // setting it to the normalized input
            }

            dashing = true; // the dash is started

            // setting the movement which shouldn't be changed (unless in the air)
            moveVelocity = direction*dashSpeed+Vector3.up*moveVelocity.y;

            dashCooldownTimer = dashCooldown;
            dashTimer = dashLength;

            dashOverride = false;
            dashReleased = false;
            holdingJump = false; // so you cant keep jumping
            


            // ****finish after starting the camera, also change direction with camera****
        } else if (dashCooldown > 0){ // if mid-dash and or on cooldown
            dashCooldownTimer -= Time.deltaTime;
            dashTimer -= Time.deltaTime;

            if (!dash.IsPressed()){ // if the player lets go of dash, dashReleased is true
                dashReleased = true;
            }


            // if the dash ends (dashtimer<0)
            if (dashTimer <= 0 && dashing && !sprinting){
                dashing = false;
                if (!dashReleased){ // player is holding dash and hasn't let go
                    sprinting = true;
                } else{
                    // setting movement to walk speed
                    moveVelocity = direction*walkSpeed+Vector3.up*moveVelocity.y;
                }
            }
        }

        if (sprinting && !dashReleased){ // keep sprinting
            // **allow for changing direction during sprint but give it a cooldown and stuff
            // // converting the movement info (vector2) to a vector3 (y velocity = 0)
            // Vector3 moveInput = new(move.ReadValue<Vector2>().x, 0, move.ReadValue<Vector2>().y);

            // // letting the player change direction mid sprint
            // // setting the direction based on the movement being input
            // if (moveInput != Vector3.zero){ // making sure direction isn't updated for no reason
            //     direction = moveInput.normalized; // setting it to the normalized input
            // }
            
            // using moveTowards and acceleration to modify moveVelocity based on movement being input
            // also adds the y velocity so it isn't modified by this function
            moveVelocity = Vector3.MoveTowards(moveVelocity, direction*sprintSpeed+Vector3.up*moveVelocity.y, acceleration);

        } else if (sprinting){ // stop sprinting
            sprinting = false;
            dashCooldown = sprintRecoveryTime; // so you can't dash instantly after
        }
        
        if (dash.triggered && dash.IsPressed() && dashReleased && dashCooldownTimer > 0){ // pressing dash while dashing or on cooldown
            dashOverride = true; // *check if this works*
        }

        if ((dashing || sprinting) && !holdingJump){ // avoiding double using this
            adjustVertical(); // vertical movement stuff -- not affected by dashing
        }

        return dashing || sprinting; // if dashing or sprinting, don't let the player walk
    }

    void groundMovement(){
        // ~Walking/Direction
        // converting the movement info (veector2) to a vector3 (y velocity = 0)
        Vector3 moveInput = new(move.ReadValue<Vector2>().x, 0, move.ReadValue<Vector2>().y);
        // setting the direction based on the movement being input
        if (moveInput != Vector3.zero){ // making sure direction isn't updated for no reason
            direction = moveInput.normalized; // setting it to the normalized input
        }
        
        // using moveTowards and acceleration to modify moveVelocity based on movement being input
        // also adds the y velocity so it isn't modified by this function
        moveVelocity = Vector3.MoveTowards(moveVelocity, moveInput.normalized*walkSpeed+Vector3.up*moveVelocity.y, acceleration);

        adjustVertical();

        // ~Jumping
        if (jump.triggered && jump.IsPressed()){ // checking if jump was pressed while the jump action is triggered
            moveVelocity.y = jumpStrength;
            holdingJump = true;
            jumpLengthTimer = maxJumpLength;
        }
    }

    void airMovement(){
        // // adding gravity // moved to doing this only when not holding jump
        // moveVelocity.y += gravity * Time.deltaTime;


        // walk direction change possible in air, not whole player direction though
        Vector3 moveInput = new(move.ReadValue<Vector2>().x, 0, move.ReadValue<Vector2>().y);
        // using air acceleration
        moveVelocity = Vector3.MoveTowards(moveVelocity, moveInput.normalized*airWalkSpeed+Vector3.up*moveVelocity.y, airAcceleration);


        // ~Jumping in air
        if (holdingJump){ // if last checked as holding jump
            if (jump.IsPressed()){
                print("holding jump");
                jumpLengthTimer -= Time.deltaTime;
                if (jumpLengthTimer <= 0) { // timer ran out
                    // moveVelocity.y /= jumpStopStrength; // initial slowing to stop/limit vertical ascending
                    holdingJump = false;
                } else{
                    // removed for smoother jumping
                    // moveVelocity.y += holdJumpStrength * Time.deltaTime * jumpLengthTimer; // adds holdJumpStrength to the y velocity
                }
            } else{ // letting go of jump
                holdingJump = false;
                // removed for being unnecessary
                // moveVelocity.y /= jumpStopStrength; // initial slowing to stop/limit vertical ascending **depracated)
                // print("let go of jump");
            }
        } else{ // adds gravity only after the jump button was let go
            adjustVertical();
        }
    }

    void adjustVertical(){ // adds gravity to the player's vertical velocity or resets it if they are on the ground

        if(controller.isGrounded){
            // resetting downwards movement if on the ground
            if (moveVelocity.y < -1){
                moveVelocity.y = 0;
            }
            return;
        }

        // adding gravity
        moveVelocity.y += gravity * Time.deltaTime;
        // print("falling");

        if (moveVelocity.y < maxFallSpeed){ // caps the fall speed at the max fall speed
            moveVelocity.y = maxFallSpeed;
        }
    }

}
