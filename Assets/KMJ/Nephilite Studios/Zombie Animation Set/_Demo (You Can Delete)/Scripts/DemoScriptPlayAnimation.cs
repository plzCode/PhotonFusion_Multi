using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DemoScriptPlayAnimation : MonoBehaviour
{
    private Animator animator;
    private Vector3 startingPosition;
    private Quaternion startingRotation;

    [Header("Current Animation")]
    public bool isPerformingAnimation = false;
    [SerializeField] string currentAnimation;

    [Header("Animations")]
    [SerializeField] List<string> animations = new List<string>();
    [SerializeField] Text currentAnimationName;
    private int animationListIndex = -1;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        startingPosition = transform.position;
        startingRotation = transform.rotation;

        foreach (AnimationClip ac in animator.runtimeAnimatorController.animationClips)
        {
            animations.Add(ac.name);
        }
    }

    private void Start()
    {
        PlayNextAnimation();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            PlayPeviousAimation();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            PlayNextAnimation();
        }
    }

    public void PlayNextAnimation()
    {
        transform.rotation = startingRotation;
        transform.position = startingPosition;

        animationListIndex++;

        if (animationListIndex >= animations.Count)
        {
            animationListIndex = 0;
        }

        isPerformingAnimation = true;
        animator.Play(animations[animationListIndex]);
        currentAnimationName.text = animations[animationListIndex];
    }

    public void PlayPeviousAimation()
    {
        transform.rotation = startingRotation;
        transform.position = startingPosition;

        animationListIndex--;

        if (animationListIndex < 0)
        {
            animationListIndex = animations.Count -1;
        }

        isPerformingAnimation = true;
        animator.Play(animations[animationListIndex]);
        currentAnimationName.text = animations[animationListIndex];
    }
}
