using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Interpreter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI code;
    [SerializeField] private GameObject buttons;
    [SerializeField] private GameObject description;
    [SerializeField] private TextMeshProUGUI descriptionText;
    private GameObject surfaces;
    public static int level = 0;

    private void Start()
    {
        surfaces = GameObject.Find("Trackables");
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
        surfaces.SetActive(true);
    }

    public void ShowDescription(string descriptionString)
    {
        buttons.SetActive(false);
        description.SetActive(true);
        descriptionText.text = descriptionString;
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
