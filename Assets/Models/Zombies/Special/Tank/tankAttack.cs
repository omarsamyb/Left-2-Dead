using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class tankAttack : StateMachineBehaviour
{
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        var random = new System.Random();
        int animationChoose = random.Next(1, 4);
        switch (animationChoose)
        {
            case 1:
                animator.SetFloat("Blend", 0);
                animator.SetBool("midAttack", true);
                break;
            case 2:
                animator.SetFloat("Blend", 0.5f);
                animator.SetBool("midAttack", false);
                break;
            case 3:
                animator.SetFloat("Blend", 1);
                animator.SetBool("midAttack", false);
                break;
        }


    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
