using UnityEngine;

public class Data
{
    public int width;
    public int height;
    public int[] labyrinth;
    public int playerPosition;
    public int playerRotation;
    public string description;

    public Data(int width, int height, int[] labyrinth, int playerPosition, int playerRotation, string description)
    {
        this.width = width;
        this.height = height;
        this.labyrinth = labyrinth;
        this.playerPosition = playerPosition;
        this.playerRotation = playerRotation;
        this.description = description;
    }
}

public class Level : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject wall;
    [SerializeField] private GameObject finish;
    [SerializeField] private float cell;

    private Interpreter interpreter;
    
    private GameObject player;
    private Player Player;

    public static Data currLevel;
    public static int finishPosition = 0;
    
    private Data[] levels = new Data[5];
    private Vector3 playerPosition;

    private int w;
    private int h;

    private void Start()
    {
        levels[0] = new Data(3, 2, new int[]{ 0, 2, 4, 1, 0, 0 }, 0, 1,
            "1 уровень. «десь вы можете увидеть робота и сундук, в котором ему " +
            "нужно оказатьс€. ƒл€ того, чтобы двигать робота вперед на одну клетку, нужно " +
            "использовать команду \"forward()\". ¬ данном уровне используйте эту команду дважды. " +
            " оманды нужно раздел€ть переносом строки.");
        levels[1] = new Data(3, 2, new int[] { 0, 2, 0, 1, 0, 4 }, 0, 1, 
            "2 уровень. Ётот уровень похож на предыдущий, но здесь нужно использовать поворот " +
            "направо \"rotate_right()\", чтобы дойти до финиша.");
        levels[2] = new Data(3, 3, new int[] { 4, 2, 2, 0, 0, 0, 1, 1, 0 }, 7, 0,
            "3 уровень. “ут вам нужно использовать повороты направо и налево: " +
            "\"rotate_right\", \"rotate_left\".");
        levels[3] = new Data(4, 2, new int[] { 0, 3, 2, 0, 0, 0, 0, 4 }, 4, 1,
            "4 уровень. “еперь вы научитесь использовать циклы. " +
            "÷иклы - это конструкци€, котора€ заставл€ет блок кода выполн€тьс€ несколько раз. " +
            "„тобы их использовать, напишите \"loop n:\", где \"n\" - количество повторений. " +
            "ƒалее, на каждой последующей строке, котора€ входит в этот цикл, нужно поставить пробел в начале.");
        levels[4] = new Data(3, 3, new int[] { 0, 2, 0, 1, 1, 0, 0, 5, 0 }, 8, 0,
            "5 уровень.\nЁтот уровень посложнее. ѕостарайтесь написать программу из п€ти строк.");
        interpreter = GameObject.Find("Canvas").GetComponent<Interpreter>();
        if (interpreter == null)
            return;
        interpreter.DisableSurfaces();
        NewLevel();
    }

    private void CreateLevel()
    {
        float wallDepth = cell / 10;
        float wallHeight = cell / 2;

        w = currLevel.width;
        h = currLevel.height;

        for (int i = 0; i < 4; i++)
        {
            Transform side = Instantiate(wall, transform).transform;
            side.localPosition = i % 2 == 0 ? new Vector3((i - 1) * w * cell / 2.0f, wallHeight / 2, 0) :
                new Vector3(0, wallHeight / 2, (i - 2) * h * cell / 2.0f);
            side.localScale = i % 2 == 0 ? new Vector3(wallDepth, wallHeight, h * cell + wallDepth) :
                new Vector3(w * cell + wallDepth, wallHeight, wallDepth);
        }

        Vector3 cellPosition = new(-w / 2 * cell + (w % 2 == 0 ? cell / 2 : 0),
            0, h / 2 * cell - (h % 2 == 0 ? cell / 2 : 0));

        for (int i = 0; i < w * h; i++)
        {
            if ((currLevel.labyrinth[i] & 1) == 1)
            {
                var newWall = Instantiate(wall, transform).transform;
                newWall.localPosition = cellPosition + new Vector3(cell / 2, wallHeight / 2, 0);
                newWall.localScale = new Vector3(wallDepth, wallHeight, cell + wallDepth);
            }
            if ((currLevel.labyrinth[i] & 2) == 2)
            {
                var newWall = Instantiate(wall, transform).transform;
                newWall.localPosition = cellPosition + new Vector3(0, wallHeight / 2, -cell / 2);
                newWall.localScale = new Vector3(cell + wallDepth, wallHeight, wallDepth);
            }
            if ((currLevel.labyrinth[i] & 4) == 4)
            {
                var newFinish = Instantiate(finish, transform).transform;
                newFinish.localPosition = cellPosition;
                newFinish.localScale = new Vector3(cell / 2, cell / 2, cell / 2);
            }
            if (currLevel.playerPosition == i)
            {
                player = Instantiate(playerPrefab, transform);
                player.transform.SetLocalPositionAndRotation(
                    cellPosition, Quaternion.Euler(0, currLevel.playerRotation * 90, 0));
                player.transform.localScale = new Vector3(cell, cell, cell);
                Player = player.GetComponent<Player>();
                Player.level = GetComponent<Level>();
                Player.transformForward = cell;
                playerPosition = player.transform.localPosition;
            }

            if ((i + 1) % w == 0)
                cellPosition -= new Vector3(cell * (w - 1), 0, cell);
            else
                cellPosition += new Vector3(cell, 0, 0);
        }
    }

    private void SetTankTransform()
    {
        if (playerPosition != null)
            player.transform.SetLocalPositionAndRotation(
                playerPosition, Quaternion.Euler(0, currLevel.playerRotation * 90, 0));
    }

    public static int CountFinishPositions()
    {
        int count = 0;
        for (int i = 0; i < currLevel.labyrinth.Length; i++)
            if ((currLevel.labyrinth[i] & 4) == 4)
                count++;
        return count;
    }

    public static bool IsFinishPosition(int position)
    {
        int count = 0;
        for (int i = 0; i <= position; i++)
            if ((currLevel.labyrinth[i] & 4) == 4)
            {
                if (finishPosition == count && position == i)
                    return true;
                count++;
            }
        return false;
    }

    private int IsLevelSolve(Commands[] commands)
    {
        int position = currLevel.playerPosition;
        int rotation = currLevel.playerRotation;

        for (int i = 0; i < commands.Length; i++)
        {
            if (commands[i] == Commands.rotate_left)
                rotation = (rotation + 3) % 4;
            else if (commands[i] == Commands.rotate_right)
                rotation = (rotation + 1) % 4;
            else if (commands[i] == Commands.forward)
            {
                switch (rotation)
                {
                    case 0:
                        if (position - w >= 0 && (currLevel.labyrinth[position - w] & 2) != 2)
                            position -= w;
                        else
                            return i;
                        break;
                    case 1:
                        if ((position + 1) % w != 0 && (currLevel.labyrinth[position] & 1) != 1)
                            position += 1;
                        else
                            return i;
                        break;
                    case 2:
                        if (position + w < w * h && (currLevel.labyrinth[position] & 2) != 2)
                            position += w;
                        else
                            return i;
                        break;
                    case 3:
                        if (position % w != 0 && (currLevel.labyrinth[position - 1] & 1) != 1)
                            position -= 1;
                        else
                            return i;
                        break;
                    default:
                        break;
                }
            }
        }

        if ((currLevel.labyrinth[position] & 4) == 4)
            return commands.Length + 1;
        return commands.Length;
    }

    public void RunCode(Commands[] commands)
    {
        if (currLevel == null)
            return;
        StopAllCoroutines();
        SetTankTransform();
        int count = IsLevelSolve(commands);
        Result result = count < commands.Length ? Result.wall :
            (count == commands.Length ? Result.notSolve : Result.solve);
        if (count > commands.Length)
            count = commands.Length;
        StartCoroutine(Player.GetCommands(commands[0..count], result));
    }

    private void ClearLevel()
    {
        while (transform.childCount > 0)
            DestroyImmediate(transform.GetChild(0).gameObject);
    }

    public void SolveLevel()
    {
        Interpreter.level++;
        if (Interpreter.level == levels.Length)
            interpreter.ShowDescription("»гра пройдена!");
        else
        {
            Invoke(nameof(NewLevel), 1);
        }
    }

    private void NewLevel()
    {
        currLevel = levels[Interpreter.level];
        if (currLevel.description != "")
            interpreter.ShowDescription(currLevel.description);
        ClearLevel();
        CreateLevel();
    }
}