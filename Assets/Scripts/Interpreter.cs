using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Interpreter : MonoBehaviour
{
    public TextMeshProUGUI code;
    private GameObject surfaces;

    private void Start()
    {
        surfaces = GameObject.Find("Trackables");
    }

    public void ChangeVisibleSurfaces()
    {
        surfaces.SetActive(!surfaces.activeSelf);
    }

    public void RunCode()
    {
        string[] operators = code.text.Split(';');
        List<Commands> commands = new();
        for (int i = 0; i < operators.Length; i++)
        {
            if (operators[i].Trim() == "forward")
                commands.Add(Commands.forward);
            else if (operators[i].Trim() == "rotate_left")
                commands.Add(Commands.rotate_left);
            else if (operators[i].Trim() == "rotate_right")
                commands.Add(Commands.rotate_right);
        }    

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && player.TryGetComponent<Level>(out var level))
            level.RunCode(commands.ToArray());
    }
}
