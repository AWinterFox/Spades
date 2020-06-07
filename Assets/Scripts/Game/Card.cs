using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    [SerializeField]
    private Sprite backFace;

    [SerializeField]
    private Sprite frontFace;

    public int Goodness;
    public Player PlayedBy;

    public int Order
    {
        get { return renderer.sortingOrder; }
        set { renderer.sortingOrder = value; }
    }

    public bool IsMoving
    {
        get { return moving; }
    }

    public CardValue Value;
    public CardSuit Suit;

    private new SpriteRenderer renderer;
    private CardFace face = CardFace.Back;

    [SerializeField]
    private float moveSpeed;

    private Vector3 moveFromPosition;
    private Vector3 moveToPosition;

    private Quaternion fromRotation;
    private Quaternion toRotation;
    private bool moving = false;

    // Start is called before the first frame update
    void Awake()
    {
        renderer = GetComponent<SpriteRenderer>();
        renderer.sprite = face == CardFace.Back ? backFace : frontFace;
    }

    private void Update()
    {
        if(moving)
        {
            var move = (moveToPosition - transform.position).normalized * Time.deltaTime * moveSpeed;
            transform.position += move;

            var distanceTotal = Vector3.Distance(moveFromPosition, moveToPosition);
            var distanceCurrent = Vector3.Distance(transform.position, moveFromPosition);
            transform.rotation = Quaternion.Lerp(fromRotation, toRotation, distanceCurrent / distanceTotal);
            if (Vector2.Distance(transform.position, moveToPosition) < moveSpeed * Time.deltaTime)
            {
                moving = false;
                transform.position = moveToPosition;
                transform.rotation = toRotation;
            }
        }
    }

    public void Flip(CardFace face)
    {
        this.face = face;
        switch (face)
        {
            case CardFace.Front:
                renderer.sprite = frontFace;
                break;
            case CardFace.Back:
                renderer.sprite = backFace;
                break;
        }
    }

    public void SetInactive(bool inactive)
    {
        renderer.color = inactive ? Color.gray : Color.white;
    }

    public void SetPosition(int i)
    {
        renderer.sortingOrder = i;
    }

    public void MoveTo(Vector3 position)
    {
        moveFromPosition = transform.position;
        moveToPosition = position;
        moving = true;
    }

    public void MoveTo(Transform target)
    {
        MoveTo(target.position);
        toRotation = target.rotation;
        fromRotation = transform.rotation;
    }

    public void SetDirection(CardDirection direction)
    {
        switch (direction)
        {
            case CardDirection.North:
                transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
                break;
            case CardDirection.West:
                transform.rotation = Quaternion.Euler(new Vector3(0, 0, 90));
                break;
            case CardDirection.East:
                transform.rotation = Quaternion.Euler(new Vector3(0, 0, 270));
                break;
            case CardDirection.South:
                transform.rotation = Quaternion.Euler(new Vector3(0, 0, 180));
                break;
        }
    }
}

public enum CardSuit
{
    Joker = 5,
    Spades = 4,
    Hearts = 3,
    Clubs = 2,
    Diamonds = 1,
}

public enum CardValue
{
    Three = 1,
    Four = 2,
    Five = 3,
    Six = 4,
    Seven = 5,
    Eight = 6,
    Nine = 7,
    Ten = 8,
    Jack = 9,
    Queen = 10,
    King = 11,
    Ace = 12,
    Two = 13,
    HighTwo = 14,
    JokerLower = 15,
    JokerHigh = 16,
}
