using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UnityEventTyped<T> : UnityEvent<T>
{

}

public class UnityEventTyped<T, A> : UnityEvent<T, A>
{

}
