using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Scripting.APIUpdating;

public class Feryl : MonoBehaviour
{
    // ~Components
    CharacterController controller;

    //~Controls
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
    int health = 10;

    // movement
    Vector3 direction = Vector3.forward;
    Vector3 moveVelocity = Vector3.zero;
    float acceleration = 1;
    float walkSpeed = 5;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        // getting the character controller
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
        // if (controller.isGrounded) {
        if (true) { // can't change direction in mid air
            // converting to a vector3
            Vector3 moveInput = new Vector3(move.ReadValue<Vector2>().x, 0, move.ReadValue<Vector2>().y);
            // setting the direction based on the movement being input
            if (moveInput != Vector3.zero){
                direction = moveInput.normalized;
            }
            // using moveTowards and acceleration to modify moveVelocity based on movement being input
            moveVelocity = Vector3.MoveTowards(moveVelocity, moveInput.normalized*walkSpeed, acceleration);
            print(moveVelocity);
        }
        

        // calculating final velocity to be moved
        Vector3 totalVelocity = moveVelocity;
        // actual move function
        controller.Move(totalVelocity*Time.deltaTime);
        print(totalVelocity);
    }
}
