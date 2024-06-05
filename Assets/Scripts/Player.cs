using System.Collections;
using UnityEngine;

public enum Result
{
    solve, notSolve, wall, wrongIf, wrongChest
}

public class Player : MonoBehaviour
{
    public float transformForward;
    public int speedForward;
    public int speedRotate;
    public Level level;

    public IEnumerator GetCommands(Command[] commands, Result result)
    {
        foreach (Command command in commands)
        {
            if (command == Command.forward)
            {
                float speed = transformForward / (float)speedForward;
                for (int i = 0; i < speedForward; i++)
                {
                    transform.Translate(0, 0, speed);
                    yield return new WaitForFixedUpdate();
                }
            }
            else if (command == Command.rotate_left)
            {
                float speed = 90 / (float)speedRotate;
                for (int i = 0; i < speedRotate; i++)
                {
                    transform.Rotate(0, -speed, 0);
                    yield return new WaitForFixedUpdate();
                }
            }
            else if (command == Command.rotate_right)
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
            level.SolveLevel();
        else if (result == Result.wall)
            level.WallOnWay();
        else if (result == Result.wrongIf)
            level.WrongIf();
        else if (result == Result.wrongChest)
            level.WrongChest();
    }
}
