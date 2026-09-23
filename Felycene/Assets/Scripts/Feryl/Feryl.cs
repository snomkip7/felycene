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
    float airAcceleration = .03f; // acceleration used while the player is in the air
    float walkSpeed = 7; // player walk speed
    float gravity = -35; // player gravity (added as gravity*Time.deltaTime every frame while in the air)
    float jumpStrength = 12; // the y velocity added when jumping // total should = about 12
    float holdJumpStrength = .2f; // y velocity added when holding jump
    float maxJumpLength = .5f; // how long the player can hold the jump button to stay moving up
    float jumpLengthTimer = .5f; // the current timer for the player holding jump to go up
    bool holdingJump = true; // if the player is currently holding jump to higher
    float jumpStopStrength = 2; // y velocity is divided by this number upon stopping holding jump


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
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
        
        // ~Walking/Direction
        if (controller.isGrounded) { // can't change direction in mid air
            // converting the movement info (veector2) to a vector3 (y velocity = 0)
            Vector3 moveInput = new Vector3(move.ReadValue<Vector2>().x, 0, move.ReadValue<Vector2>().y);
            // setting the direction based on the movement being input
            if (moveInput != Vector3.zero){ // making sure direction isn't updated for no reason
                direction = moveInput.normalized; // setting it to the normalized input
            }
            // using moveTowards and acceleration to modify moveVelocity based on movement being input
            // also adds the y velocity so it isn't modified by this function
            moveVelocity = Vector3.MoveTowards(moveVelocity, moveInput.normalized*walkSpeed+Vector3.up*moveVelocity.y, acceleration);
            print(moveVelocity);
            // ~Jumping
            if (controller.isGrounded && jump.triggered){
                moveVelocity.y = jumpStrength;
                holdingJump = true;
                jumpLengthTimer = maxJumpLength;
            }
        }
        else{ // in the air
            // adding gravity
            moveVelocity.y += gravity * Time.deltaTime;

            // walk direction change possible in air, not whole player direction though
            Vector3 moveInput = new Vector3(move.ReadValue<Vector2>().x, 0, move.ReadValue<Vector2>().y);
            if (moveInput != Vector3.zero){ // making sure direction isn't updated for no reason
                direction = moveInput.normalized; // setting it to the normalized input
            } // using air acceleration
            moveVelocity = Vector3.MoveTowards(moveVelocity, moveInput.normalized*walkSpeed+Vector3.up*moveVelocity.y, airAcceleration);
            print(moveVelocity);
            // ~Jumping in air
            if (holdingJump){ // if last checked as holding jump
                if (jump.triggered){ // ***HOLDING DOWN BUTTON NOT WORKING FULLY ****
                    print("holding jump");
                    jumpLengthTimer -= Time.deltaTime;
                    if (jumpLengthTimer <= 0) { // timer ran out
                       // moveVelocity.y /= jumpStopStrength; // initial slowing to stop/limit vertical ascending
                        holdingJump = false;
                    } else{
                        moveVelocity.y += holdJumpStrength; // adds holdJumpStrength to the y velocity
                        print("extending jump");
                    }
                } else{ // letting go of jump
                    holdingJump = false;
                    // moveVelocity.y /= jumpStopStrength; // initial slowing to stop/limit vertical ascending
                    print("let go of jump");
                }
            } 
            
        }
        

        // calculating final velocity to be moved by adding up all the sources
        Vector3 totalVelocity = moveVelocity;
        // actual move function using total velocity and delta time
        controller.Move(totalVelocity*Time.deltaTime);
        print(totalVelocity);


    }

}
