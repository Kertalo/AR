using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Text.RegularExpressions;

public enum Commands
{
    forward, rotate_left, rotate_right, loop_oper, if_oper, tab
}

public class Command
{
    public Commands command;
    public int line;
    public int number;

    public Command(Commands command, int line, int number)
    {
        this.command = command;
        this.line = line;
        this.number = number;
    }

    public Command(Commands command, int line)
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
        string loop_oper = @"\s*loop (\d\d?)\s*";
        string if_oper = @"\s*if (isEmpty)\s*";
        List<Command> commands = new();
        for (int i = 0; i < lines.Length; i++)
        {
            while (Regex.IsMatch(lines[i], tab, RegexOptions.IgnoreCase))
            {
                commands.Add(new(Commands.tab, i));
                lines[i] = Regex.Replace(lines[i], tab, "");
            }

            if (Regex.IsMatch(lines[i], forward, RegexOptions.IgnoreCase))
                commands.Add(new(Commands.forward, i));
            else if (Regex.IsMatch(lines[i], right, RegexOptions.IgnoreCase))
                commands.Add(new(Commands.rotate_right, i));
            else if (Regex.IsMatch(lines[i], left, RegexOptions.IgnoreCase))
                commands.Add(new(Commands.rotate_left, i));
            else if (Regex.IsMatch(lines[i], loop_oper, RegexOptions.IgnoreCase))
            {
                var match = Regex.Match(lines[i], loop_oper);
                commands.Add(new(Commands.loop_oper, i, Int32.Parse(match.Groups[1].Value)));
            }
            else if (Regex.IsMatch(lines[i], if_oper, RegexOptions.IgnoreCase))
                commands.Add(new(Commands.if_oper, i));
            else
            {
                ShowDescription("Синтаксическая ошибка в строке " + (i + 1));
                return;
            }
        }
        int k = 0;
        Commands[] commandsResult = Block(ref k, commands.ToArray());
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && player.TryGetComponent<Level>(out var level))
            level.RunCode(commandsResult);
    }

    private Commands[] Block(ref int i, Command[] commands, int tab = 0)
    {
        List<Commands> commandsResult = new();
        while (i < commands.Length)
        {
            int currentTab = 0;
            while (currentTab != tab)
            {
                if (commands[i].command != Commands.tab)
                    return commandsResult.ToArray();
                i++;
                currentTab++;
            }
            if (commands[i].command == Commands.forward ||
                commands[i].command == Commands.rotate_right ||
                commands[i].command == Commands.rotate_left)
            {
                commandsResult.Add(commands[i].command);
            }
            else if (commands[i].command == Commands.loop_oper)
            {
                int count = commands[i].number;
                int start = i + 1;
                for (int j = 0; j < count; j++)
                {
                    i = start;
                    Commands[] newCommands = Block(ref i, commands, tab + 1);
                    for (int k = 0; k < newCommands.Length; k++)
                        commandsResult.Add(newCommands[k]);
                    i--;
                }
            }
            else
                return new Commands[] { };
            i++;
        }
        return commandsResult.ToArray();
    }
}