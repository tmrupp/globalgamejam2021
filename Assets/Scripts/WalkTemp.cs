using UnityEngine;

public class WalkTemp : MonoBehaviour
{
    private int _walkId = Animator.StringToHash("Walk");
    private Animator _animator = null;
    private SpriteRenderer _sr = null;
    private DialogueParser _parser = null;

    void Awake()
    {
        _animator = GetComponent<Animator>();
        _sr = GetComponent<SpriteRenderer>();
        _parser = FindObjectOfType<DialogueParser>();
    }

    void Update()
    {
        _animator.SetBool(_walkId, Input.GetKey(KeyCode.A));
        _sr.flipX = Input.GetKey(KeyCode.D);
        if (Input.GetKeyDown(KeyCode.Z) && _parser.Ready())
        {
            _parser.ShowDialogue("long");
        }
    }
}
