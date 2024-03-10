using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Commands
{
    forward, rotate_left, rotate_right, shot
}

public enum Result
{
    solve, notSolve, wall
}

public class Player : MonoBehaviour
{
    public float transformForward;
    public int speedForward;
    public int speedRotate;
    public Level level;

    public IEnumerator GetCommands(Commands[] commands, Result result)
    {
        foreach (Commands command in commands)
        {
            if (command == Commands.forward)
            {
                float speed = transformForward / (float)speedForward;
                for (int i = 0; i < speedForward; i++)
                {
                    transform.Translate(-speed, 0, 0);
                    yield return new WaitForFixedUpdate();
                }
            }
            else if (command == Commands.rotate_left)
            {
                float speed = 90 / (float)speedRotate;
                for (int i = 0; i < speedRotate; i++)
                {
                    transform.Rotate(0, -speed, 0);
                    yield return new WaitForFixedUpdate();
                }
            }
            else if (command == Commands.rotate_right)
            {
                float speed = 90 / (float)speedRotate;
                for (int i = 0; i < speedRotate; i++)
                {
                    transform.Rotate(0, speed, 0);
                    yield return new WaitForFixedUpdate();
                }
            }
        }
        if (result == Result.solve)
            level.NewLevel();
    }
}
