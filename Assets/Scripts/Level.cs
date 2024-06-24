using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.UIElements.UxmlAttributeDescription;
using Mono.Data.Sqlite;
using System.Data;
//using UnityEditor.PackageManager;

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

    public static Data currentLevel;
    
    private Data[] levels = new Data[7];
    private Vector3 playerPosition;

    //private IDbConnection connection;

    private void Start()
    {
        //connection = new SqliteConnection("URI=file:" + Application.streamingAssetsPath + "/levels.db");
        //connection.Open();
        //IDbCommand command = connection.CreateCommand();
        //command.CommandText = "SELECT * FROM users WHERE name = '" + "';";
        /*IDataReader dataReader = command.ExecuteReader();

        if (dataReader.Read())
        {
            if (dataReader.GetString(1) == password.text)
            {
                maxLevel = dataReader.GetInt32(2);
            }
            else
            {
                error.text = "Неверный пароль";
                connection.Close();
                command.Dispose();
                return;
            }
        }
        else
        {
            command.Dispose();
            command.CommandText = "INSERT INTO users (name,password,level) " +
                "VALUES('" + login.text + "', '" + password.text + "', 0);";
            command.ExecuteReader();
        }*/
        //connection.Close();
        //command.Dispose();
        levels[0] = new Data(3, 2, new int[]{ 0, 2, 4, 1, 0, 0 }, 0, 1,
            "<b><color=#7030a0>1 УРОВЕНЬ</color></b>\n" +
            "Перед Вами робот и сундук, в котором ему нужно оказаться.\n" +
            "Для движения робота вперед на одну клетку используем команду:\n" +
            "<b><color=#0070c0>forward()</color></b>\n" +
            "Эту команду нужно использовать 2 раза.\n" +
            "Каждая команда записывается с новой строки.");
        levels[1] = new Data(3, 2, new int[] { 0, 2, 0, 1, 0, 4 }, 0, 1,
            "<b><color=#7030a0>2 УРОВЕНЬ</color></b>\n" +
            "Этот уровень похож на 1 уровень.\n" +
            "Чтобы дойти до финиша, нужно использовать две команды:\n" +
            "<b><color=#0070c0>forward()</color></b> – движение вперед\n" +
            "<b><color=#0070c0>rotate_right()</color></b> – поворот направо");
        levels[2] = new Data(3, 3, new int[] { 4, 2, 2, 0, 0, 0, 1, 1, 0 }, 7, 0,
            "<b><color=#7030a0>3 УРОВЕНЬ</color></b>\n" +
            "Используем команды:\n" +
            "<b><color=#0070c0>forward()</color></b> – движение вперед\n" +
            "<b><color=#0070c0>rotate_right()</color></b> – поворот направо\n" +
            "<b><color=#0070c0>rotate_left()</color></b> – поворот налево");
        levels[3] = new Data(4, 2, new int[] { 0, 3, 2, 0, 0, 0, 0, 4 }, 4, 1,
            "<b><color=#7030a0>4 УРОВЕНЬ</color></b>\n" +
            "Учимся использовать циклы.\n" +
            "<b>Цикл</b> - это конструкция, которая заставляет блок кода выполняться несколько раз.\n" +
            "Используем команду:\n" +
            "<b><color=#0070c0>loop n:</color></b>\n" +
            "Например:\n" +
            "<color=red>loop 2:</color>\n" +
            "<color=red>    forward()</color>\n" +
            "<color=red>    rotate_right()</color>");
        levels[4] = new Data(3, 3, new int[] { 0, 2, 0, 1, 1, 0, 0, 5, 0 }, 8, 0,
            "<b><color=#7030a0>5 УРОВЕНЬ</color></b>\n" +
            "Этот уровень посложнее.\n" +
            "Постарайтесь написать программу из пяти строк!!!");
        levels[5] = new Data(3, 2, new int[] { 0, 6, 4, 1, 0, 0 }, 0, 1,
            "<b><color=#7030a0>6 УРОВЕНЬ</color></b>\n" +
            "Учимся использовать условный оператор!\n" +
            "У нас  2 сундука, но только один из них заполнен золотом.\n" +
            "Условие для дальнейшего движения робота, если сундук пуст, задается командой:\n" +
            "<b><color=#0070c0>if (isEmpty):</color></b>\n" +
            "Не забудьте поставить в следующих строках пробел в начале.");
        levels[6] = new Data(3, 2, new int[] { 0, 6, 4, 1, 0, 4 }, 0, 1,
            "<b><color=#7030a0>7 УРОВЕНЬ</color></b>\n" +
            "Здесь уже 3 сундука.\n" +
            "Нужно использовать условие, вложенное в другое условие.\n" +
            "То есть в последующих условиях пробелов должно быть на один больше, чем у предыдущего.");
        interpreter = GameObject.Find("Canvas").GetComponent<Interpreter>();
        NewLevel();
    }

    private void CreateLevel()
    {
        float wallDepth = cell / 10;
        float wallHeight = cell / 2;

        int w = currentLevel.width;
        int h = currentLevel.height;

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
            if ((currentLevel.labyrinth[i] & 1) == 1)
            {
                var newWall = Instantiate(wall, transform).transform;
                newWall.localPosition = cellPosition + new Vector3(cell / 2, wallHeight / 2, 0);
                newWall.localScale = new Vector3(wallDepth, wallHeight, cell + wallDepth);
            }
            if ((currentLevel.labyrinth[i] & 2) == 2)
            {
                var newWall = Instantiate(wall, transform).transform;
                newWall.localPosition = cellPosition + new Vector3(0, wallHeight / 2, -cell / 2);
                newWall.localScale = new Vector3(cell + wallDepth, wallHeight, wallDepth);
            }
            if ((currentLevel.labyrinth[i] & 4) == 4)
            {
                var newFinish = Instantiate(finish, transform).transform;
                newFinish.localPosition = cellPosition;
                newFinish.localScale = new Vector3(cell / 2, cell / 2, cell / 2);
            }
            if (currentLevel.playerPosition == i)
            {
                player = Instantiate(playerPrefab, transform);
                player.transform.SetLocalPositionAndRotation(
                    cellPosition, Quaternion.Euler(0, currentLevel.playerRotation * 90, 0));
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
                playerPosition, Quaternion.Euler(0, currentLevel.playerRotation * 90, 0));
    }

    public static int CountFinishPositions()
    {
        int count = 0;
        for (int i = 0; i < currentLevel.labyrinth.Length; i++)
            if ((currentLevel.labyrinth[i] & 4) == 4)
                count++;
        return count;
    }

    public static bool DoCommand(Command command, Data data, ref int position, ref int rotation)
    {
        if (command == Command.rotate_left)
            rotation = (rotation + 3) % 4;
        else if (command == Command.rotate_right)
            rotation = (rotation + 1) % 4;
        else if (command == Command.forward)
        {
            int w = data.width;
            int h = data.height;
            switch (rotation)
            {
                case 0:
                    if (position - w >= 0 && (data.labyrinth[position - w] & 2) != 2)
                        position -= w;
                    else
                        return false;
                    break;
                case 1:
                    if ((position + 1) % w != 0 && (data.labyrinth[position] & 1) != 1)
                        position += 1;
                    else
                        return false;
                    break;
                case 2:
                    if (position + w < w * h && (data.labyrinth[position] & 2) != 2)
                        position += w;
                    else
                        return false;
                    break;
                case 3:
                    if (position % w != 0 && (data.labyrinth[position - 1] & 1) != 1)
                        position -= 1;
                    else
                        return false;
                    break;
                default:
                    break;
            }
        }
        return true;
    }

    private int IsLevelSolve(Command[] commands)
    {
        int position = currentLevel.playerPosition;
        int rotation = currentLevel.playerRotation;

        for (int i = 0; i < commands.Length; i++)
            if (!DoCommand(commands[i], currentLevel, ref position, ref rotation))
                return i;

        if ((currentLevel.labyrinth[position] & 4) == 4)
            return commands.Length + 1;
        return commands.Length;
    }

    public void RunCode(Result firstResult, Command[] commands)
    {
        if (currentLevel == null)
            return;
        StopAllCoroutines();
        SetTankTransform();
        int count = IsLevelSolve(commands);
        Result result = count < commands.Length ? Result.wall :
            (count == commands.Length ? Result.notSolve : Result.solve);
        if (count > commands.Length)
            count = commands.Length;
        if (firstResult == Result.wrongIf || firstResult == Result.wrongChest)
            StartCoroutine(Player.GetCommands(commands[0..count], firstResult));
        else
            StartCoroutine(Player.GetCommands(commands[0..count], result));
    }

    private void ClearLevel()
    {
        while (transform.childCount > 0)
            DestroyImmediate(transform.GetChild(0).gameObject);
    }

    public void WallOnWay()
    {
        interpreter.ShowDescription("Стена находится на пути");
    }

    public void WrongIf()
    {
        interpreter.ShowDescription("Проверка на пустоту сундука не может проверять пустую клетку");
    }

    public void WrongChest()
    {
        interpreter.ShowDescription("Этот сундук пуст");
    }

    private void GameIsOver()
    {
        interpreter.ShowDescription("Игра пройдена!");
    }

    public void SolveLevel()
    {
        Interpreter.level++;
        if (Interpreter.level == levels.Length)
            Invoke(nameof(GameIsOver), 1);
        else
        {
            if (Users.maxLevel < Interpreter.level)
            {
                Users.SetLevelData(Interpreter.level);
                GameObject.Find("AR Session").GetComponent<Users>().ButtonActive();
            }
            Invoke(nameof(NewLevel), 1);
        }
    }

    public void NewLevel()
    {
        currentLevel = levels[Interpreter.level];
        if (currentLevel.description != "")
            interpreter.ShowDescription(currentLevel.description);
        ClearLevel();
        CreateLevel();
    }
}