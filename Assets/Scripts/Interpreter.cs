using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Text.RegularExpressions;
using UnityEngine.UIElements;

public enum Command
{
    forward, rotate_left, rotate_right, loop_oper, if_oper, tab
}

public class CommandObj
{
    public Command command;
    public int line;
    public int number;

    public CommandObj(Command command, int line, int number)
    {
        this.command = command;
        this.line = line;
        this.number = number;
    }

    public CommandObj(Command command, int line)
    {
        this.command = command;
        this.line = line;
    }
}

public class Interpreter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI code;
    [SerializeField] private GameObject buttons;
    [SerializeField] private GameObject description;
    [SerializeField] private TextMeshProUGUI descriptionText;
    private GameObject surfaces;
    private GameObject spawner;
    private bool isFirstDescription = true;
    public static int level = 0;

    private void Start()
    {
        surfaces = GameObject.Find("Trackables");
        spawner = GameObject.Find("Object Spawner");
        DisableSurfaces();
    }

    public void DisableSurfaces()
    {
        surfaces.SetActive(false);
    }

    public void DeleteLevel()
    {
        GameObject level = GameObject.FindGameObjectWithTag("Player");
        if (level != null)
            Destroy(level);
        buttons.SetActive(false);
        spawner.SetActive(false);
        surfaces.SetActive(true);
        Invoke(nameof(DeleteIsOver), 0.3f);
    }

    private void DeleteIsOver()
    {
        spawner.SetActive(true);
    }

    public void CloseDescription()
    {
        if (isFirstDescription)
        {
            isFirstDescription = false;
            surfaces.SetActive(true);
        }
        else
            buttons.SetActive(true);
    }

    public void ShowDescription(string descriptionString)
    {
        buttons.SetActive(false);
        description.SetActive(true);
        descriptionText.text = descriptionString;
    }

    public void RunCode()
    {
        string[] lines = code.text.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        string tab = @"^\s";
        string forward = @"\s*forward\(\)\s*";
        string right = @"\s*rotate_right\(\)\s*";
        string left = @"\s*rotate_left\(\)\s*";
        string loop_oper = @"\s*loop (\d\d?):\s*";
        string if_oper = @"\s*if \(isEmpty\):\s*";
        List<CommandObj> commands = new();
        for (int i = 0; i < lines.Length; i++)
        {
            while (Regex.IsMatch(lines[i], tab, RegexOptions.IgnoreCase))
            {
                commands.Add(new(Command.tab, i));
                lines[i] = Regex.Replace(lines[i], tab, "");
            }

            if (Regex.IsMatch(lines[i], forward, RegexOptions.IgnoreCase))
                commands.Add(new(Command.forward, i));
            else if (Regex.IsMatch(lines[i], right, RegexOptions.IgnoreCase))
                commands.Add(new(Command.rotate_right, i));
            else if (Regex.IsMatch(lines[i], left, RegexOptions.IgnoreCase))
                commands.Add(new(Command.rotate_left, i));
            else if (Regex.IsMatch(lines[i], loop_oper, RegexOptions.IgnoreCase))
            {
                var match = Regex.Match(lines[i], loop_oper);
                commands.Add(new(Command.loop_oper, i, Int32.Parse(match.Groups[1].Value)));
            }
            else if (Regex.IsMatch(lines[i], if_oper, RegexOptions.IgnoreCase))
                commands.Add(new(Command.if_oper, i));
            else
            {
                if (lines.Length == 1 && lines[i].Length == 1)
                    ShowDescription("Командная строка пуста");
                else
                    ShowDescription("Синтаксическая ошибка в строке " + (i + 1));
                return;
            }
        }

        List<int> finishPositions = new List<int>();
        for (int i = 0; i < Level.currentLevel.labyrinth.Length; i++)
            if ((Level.currentLevel.labyrinth[i] & 4) == 4)
                finishPositions.Add(i);
        System.Random random = new();
        int finish = finishPositions[random.Next(finishPositions.Count)];

        int k = 0;
        int position = Level.currentLevel.playerPosition;
        int rotation = Level.currentLevel.playerRotation;
        var commandsResult = Block(ref k, commands.ToArray(), ref position, ref rotation, finish);
        Debug.Log(commandsResult.Item1);
        //for (int i = 0; i < commandsResult.Item2.Length; i++)
        //    Debug.Log(commandsResult.Item2[i]);
        var item1 = commandsResult.Item1;
        if (item1 == Result.solve && position != finish
            && (Level.currentLevel.labyrinth[position] & 4) == 4)
            item1 = Result.wrongChest;
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && player.TryGetComponent<Level>(out var level))
            level.RunCode(item1, commandsResult.Item2);
    }

    private Tuple<Result, Command[]> Block(ref int i, CommandObj[] commands, ref int position, ref int rotation, int finish, int tab = 0)
    {
        List<Command> commandsResult = new();
        while (i < commands.Length)
        {
            Debug.Log(position);
            int currentTab = 0;
            while (currentTab != tab)
            {
                if (commands[i].command != Command.tab)
                    return new(Result.solve, commandsResult.ToArray());
                i++;
                currentTab++;
            }
            if (commands[i].command == Command.forward ||
                commands[i].command == Command.rotate_right ||
                commands[i].command == Command.rotate_left)
            {
                commandsResult.Add(commands[i].command);
                Level.DoCommand(commands[i].command, Level.currentLevel, ref position, ref rotation);
                //if (!Level.DoCommand(commands[i].command, Level.currentLevel, ref position, ref rotation))
                //    return new(Result.wall, commandsResult.ToArray());
            }
            else if (commands[i].command == Command.loop_oper)
            {
                int count = commands[i].number;
                int start = i + 1;
                for (int j = 0; j < count; j++)
                {
                    i = start;
                    var newCommands = Block(ref i, commands, ref position, ref rotation, finish, tab + 1);
                    for (int k = 0; k < newCommands.Item2.Length; k++)
                        commandsResult.Add(newCommands.Item2[k]);
                    if (newCommands.Item1 != Result.solve)
                        return new(newCommands.Item1, commandsResult.ToArray());
                    i--;
                }
            }
            else if (commands[i].command == Command.if_oper)
            {
                i++;
                if ((Level.currentLevel.labyrinth[position] & 4) != 4)
                    return new(Result.wrongIf, commandsResult.ToArray());
                bool isFinish = true;
                //Debug.Log(position);
                //Debug.Log(finish);
                if (position != finish)
                    isFinish = false;
                int tempPosition = position;
                int tempRotation = rotation;
                var newCommands = Block(ref i, commands, ref tempPosition, ref tempRotation, finish, tab + 1);
                if (!isFinish)
                {
                    position = tempPosition;
                    rotation = tempRotation;
                    for (int k = 0; k < newCommands.Item2.Length; k++)
                        commandsResult.Add(newCommands.Item2[k]);
                    if (newCommands.Item1 != Result.solve)
                        return new(newCommands.Item1, commandsResult.ToArray());
                }
                i--;
            }
            i++;
        }
        return new(Result.solve, commandsResult.ToArray());
    }
}