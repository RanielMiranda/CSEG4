using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public LayerMask blockingLayer;
    private bool isMoving = false;
    private bool isWin = false;
    private Vector3 lastDirection = Vector3.zero; // Store last movement direction
    private Animator animator;
    private List<Vector3> metalBoxFuturePositions = new List<Vector3>();
    private Vector3 futureEBoxPosition;


    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        if (GameManager.Instance != null) {
        GameManager.Instance.SaveState();
        }

        //Level Editor
        if (GameObject.FindFirstObjectByType<LevelManager>() != null)
        {
            enabled = false;
        }        
        
        if (GameManager.Instance != null) 
        foreach (var metalBox in GameManager.Instance.GetMetalBoxes())
        {
            metalBoxFuturePositions.Add(metalBox.GetFuturePosition());
        }
    }

    void Update()
    {
        if (isWin) return;
        if (isMoving) return;

        GetNonMovementInput();

        var movement = Vector3.zero;
        GetMovementInput(ref movement);

        //Animations
        CheckForBoxesAround();

        if (movement != Vector3.zero)
        {   
            TryToMove(movement);
        }   
    }

    private void CheckForBoxesAround()
    {
        if (lastDirection == Vector3.zero) return;

        animator.SetBool("isBoxInFront", false); // Reset in front detection

        // Check in front
        if (Physics.Raycast(transform.position, lastDirection, out RaycastHit hitFront, 1f, blockingLayer))
        {
            if (hitFront.collider.CompareTag("Box"))
                {animator.SetBool("isBoxInFront", true);}
            if (hitFront.collider.CompareTag("Ember Box") ||
                hitFront.collider.CompareTag("Volt Box") || hitFront.collider.CompareTag("Frost Box") || 
                hitFront.collider.CompareTag("Magnet Box"))
            {
                animator.SetBool("isBoxInFront", true);
                futureEBoxPosition = hitFront.collider.GetComponent<ElementalBoxController>().GetFuturePosition(lastDirection);
            }
        }
    }

    private void GetNonMovementInput()
    {
        if (Input.GetKeyDown(KeyCode.Z)) GameManager.Instance.Undo();
        if (Input.GetKeyDown(KeyCode.X)) GameManager.Instance.Redo();
        if (Input.GetKeyDown(KeyCode.R)) GameManager.Instance.Reset();
        if (Input.GetKeyDown(KeyCode.C)) GameManager.Instance.Cheat();
        if (Input.GetKeyDown(KeyCode.Escape)) GameManager.Instance.toggleWinScreenUI();
    }

    private void GetMovementInput(ref Vector3 movement)
    {
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) movement = Vector3.forward;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) movement = Vector3.back;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) movement = Vector3.left;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) movement = Vector3.right;

        if (movement != Vector3.zero)
        {
            lastDirection = movement; // Store the last movement direction
        }
    }

    private void TryToMove(Vector3 direction)
    {        
        var targetPosition = transform.position + direction;
        transform.rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(0, -90, 0);
        GameManager.Instance.UpdateMetalBoxes(); 

        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, 1f, blockingLayer))
        {
            // If the object hit is a wall or an unmovable object, do not proceed
            if (!hit.collider.CompareTag("Box") && 
                !hit.collider.CompareTag("Ember Box") &&
                !hit.collider.CompareTag("Volt Box") &&
                !hit.collider.CompareTag("Frost Box") &&
                !hit.collider.CompareTag("Magnet Box") &&
                !hit.collider.CompareTag("Goal"))
            {
                return; // Block movement
            }

            // Moving normal boxes
            if (hit.collider.CompareTag("Box"))
            {
                var box = hit.collider.GetComponent<NeutralBoxController>();
                if (box != null && box.TryToPushBox(direction, moveSpeed))
                {
                    animator.SetBool("isPushing", true);
                    StartCoroutine(MoveToPosition(targetPosition));
                }
                return;
            }

            // Moving elemental boxes
            if (hit.collider.CompareTag("Ember Box") || hit.collider.CompareTag("Volt Box") ||
                hit.collider.CompareTag("Frost Box") || hit.collider.CompareTag("Magnet Box"))
            {
                var box = hit.collider.GetComponent<ElementalBoxController>();

                if (box != null)
                {
                    animator.SetBool("isPushing", true);

                    // Empty space
                    if (box.TryToPushBox(direction, moveSpeed) && !box.GetIsReacting())
                    {

                        if (targetPosition.x == futureEBoxPosition.x && targetPosition.z == futureEBoxPosition.z)
                        {
                            return;
                        }
                        StartCoroutine(MoveToPosition(targetPosition));
                    }
                    // Reaction occurred
                    else
                    {
                        box.PerformReaction(box.CheckForReaction(hit), hit, targetPosition, direction, moveSpeed);
                    }
                }
                return;
            }

            // Moving to goal
            if (hit.collider.CompareTag("Goal") && GameManager.Instance.CheckWinCondition())
            {
                StartCoroutine(MoveToPosition(targetPosition));
                GameManager.Instance.isWin = true;                
                GameManager.Instance.toggleWinScreenUI();
                isWin = true;
                return;
            }
        }
        else
        {
        
        // No obstacles detected, move freely

        animator.SetBool("isPushing", false);
        StartCoroutine(MoveToPosition(targetPosition));          
        }
    }

    private IEnumerator MoveToPosition(Vector3 target)
    {
        isMoving = true;
        AudioManager.Instance.PlayMovePlayer();
        animator.SetBool("isMoving", true); // Set animation for movement

        while (Vector3.Distance(transform.position, target) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = target;
        GameManager.Instance.SaveState();
        ResetMovePosition();
    }

    private void ResetMovePosition()
    {
        isMoving = false;
        animator.SetBool("isMoving", false); // Reset animation for movement
        animator.SetBool("isPushing", false);

        metalBoxFuturePositions.Clear();
        foreach (var metalBox in GameManager.Instance.GetMetalBoxes())
        {
            metalBoxFuturePositions.Add(metalBox.GetFuturePosition());
        }        
    }

    public bool MetalTryToMove(Vector3 direction)
    {
        Vector3 targetPosition = transform.position + direction;

        // Check if space is empty
        if (!Physics.CheckSphere(targetPosition, 0.1f, blockingLayer)) 
        {
            StartCoroutine(MoveToPosition(targetPosition));
            return true; // Player moved successfully
        }

        return false; // Player couldn't move
    }
}