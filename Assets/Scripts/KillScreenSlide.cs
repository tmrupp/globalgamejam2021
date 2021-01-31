using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillScreenSlide : MonoBehaviour
{
    SpriteRenderer left;
    SpriteRenderer right;

    [SerializeField] Vector2 leftDestination; //set in editor
    [SerializeField] Vector2 rightDestination; //set in editor
    [SerializeField] float globalSizeScaling; //set in editor

    private static KillScreenSlide instance = null;

    [SerializeField] List<Sprite> victim = null;
    [SerializeField] List<Sprite> hunter = null;
    [SerializeField] List<Sprite> monster = null;

    private int semaphore = 0;

    public static Sprite GetKillScreenSprite(AgentType type, int index)
    {
        switch (type)
        {
            case AgentType.victim: return instance.victim[index];
            case AgentType.hunter: return instance.hunter[index];
            case AgentType.monster: return instance.monster[index];
        }
        return null;
    }

    void Start()
    {
        SpriteRenderer[] srs = GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in srs)
        {
            if (sr.name == "Left")
                left = sr;
            else if (sr.name == "Right")
                right = sr;
        }
    }

    private void Awake()
    {
        instance = this;
    }

    public static void PerformKillScreen(Sprite leftSprite, Sprite rightSprite)
    {
        instance.PerformKillScreenInstance(leftSprite, rightSprite);
    }

    public void PerformKillScreenInstance(Sprite leftSprite, Sprite rightSprite)
    {
        left.sprite = leftSprite;
        right.sprite = rightSprite;

        StartCoroutine(TryAgainIfNecessary());
    }

    private IEnumerator TryAgainIfNecessary()
    {
        while (semaphore != 0)
        {
            yield return null;
        }

        semaphore += 2;

        StartCoroutine(SlideIn(left, leftDestination, 0.2f, 1.5f, 0f));
        StartCoroutine(SlideIn(right, rightDestination, 0.2f, 1.5f, 0.15f));

        yield return null;
    }

    private IEnumerator SlideIn(SpriteRenderer thingToMove, Vector2 destination, float slideTime, float fadeTime, float delay)
    {
        thingToMove.transform.localScale = new Vector3(globalSizeScaling, globalSizeScaling, 1);

        if (delay > 0)
        {
            yield return new WaitForSeconds(delay);
        }

        //Slide in
        float progress = 0f;
        Vector3 start = thingToMove.transform.position;
        while (progress < 1f)
        {
            yield return null;
            progress += Time.deltaTime / slideTime;
            thingToMove.transform.position = Vector3.Lerp(start, (Vector3)destination, progress);
        }
        thingToMove.transform.position = destination;

        //Fade out
        progress = 0f;
        while (progress < 1f)
        {
            yield return null;
            progress += Time.deltaTime / fadeTime;
            thingToMove.color = Color.Lerp(Color.white, Color.clear, progress);
        }
        thingToMove.color = Color.clear;

        //Reset opacity and position
        thingToMove.transform.position = start;
        thingToMove.color = Color.white;

        semaphore--;

        yield return null;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            StartCoroutine(SlideIn(left, leftDestination, 0.2f, 0.5f, 0f));
            StartCoroutine(SlideIn(right, rightDestination, 0.2f, 0.5f, 0.15f));
        }
    }
}
