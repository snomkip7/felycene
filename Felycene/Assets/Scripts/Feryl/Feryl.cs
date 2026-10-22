using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Scripting.APIUpdating;

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
    // dashing
    bool dashing = false;
    float dashSpeed = 15; // speed the player moves during a dash
    float dashCooldown = .7f; // cooldown inbetween dashes
    float dashCooldownTimer = 0; // the timer for cooldowns between dashes
    float dashLength = .5f; // how long a dash lasts
    float dashTimer = 0; // timer that tracks how long a dash lasts
    bool sprinting = false; // if the player is sprinting
    float sprintSpeed = 11; // the speed the player moves while sprinting


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
    void Update() {
        
        //print("fps = " + 1/Time.deltaTime);
        
        // handles all movement
        movement(controller.isGrounded);
        

        // calculating final velocity to be moved by adding up all the sources
        Vector3 totalVelocity = moveVelocity;
        // actual move function using total velocity and delta time
        controller.Move(totalVelocity*Time.deltaTime);
        //print(totalVelocity);


    }

    void movement(bool grounded) {
        // ~Dashing
        if (dashCooldownTimer == 0 && !dashing && dash.triggered && dash.IsPressed()){ // starting the dash
            Vector3 moveInput = new(move.ReadValue<Vector2>().x, 0, move.ReadValue<Vector2>().y);
            // setting the direction based on the movement being input
            if (moveInput != Vector3.zero){ // making sure direction isn't updated for no reason
                direction = moveInput.normalized; // setting it to the normalized input
            }

            dashing = true;
            // ****finish after starting the camera, also change direction with camera****
        }

        // ~Walking/Direction
        if (grounded) { // can't change direction in mid air

            // converting the movement info (veector2) to a vector3 (y velocity = 0)
            Vector3 moveInput = new(move.ReadValue<Vector2>().x, 0, move.ReadValue<Vector2>().y);
            // setting the direction based on the movement being input
            if (moveInput != Vector3.zero){ // making sure direction isn't updated for no reason
                direction = moveInput.normalized; // setting it to the normalized input
            }
            
            // using moveTowards and acceleration to modify moveVelocity based on movement being input
            // also adds the y velocity so it isn't modified by this function
            moveVelocity = Vector3.MoveTowards(moveVelocity, moveInput.normalized*walkSpeed+Vector3.up*moveVelocity.y, acceleration);

            // resetting downwards movement if on the ground
            if (moveVelocity.y < -1){
                moveVelocity.y = 0;
            }

            // ~Jumping
            if (jump.triggered && jump.IsPressed()){ // checking if jump was pressed while the jump action is triggered
                moveVelocity.y = jumpStrength;
                holdingJump = true;
                jumpLengthTimer = maxJumpLength;
            }
        }

        else{ // in the air

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
            } else{ // adds gravity only after the jump button was pressed
                // adding gravity
                moveVelocity.y += gravity * Time.deltaTime;
                // print("falling");

                if (moveVelocity.y < maxFallSpeed){ // caps the fall speed at the max fall speed
                    moveVelocity.y = maxFallSpeed;
                }
            }
        }
    }

}
