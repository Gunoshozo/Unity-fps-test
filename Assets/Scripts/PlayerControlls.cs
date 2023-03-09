using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControlls : MonoBehaviour
{
    public void OnMove(InputValue value) => Debug.Log(value.Get<Vector2>().ToString());

    public void OnAction(InputValue value) => Debug.Log(value);
}
