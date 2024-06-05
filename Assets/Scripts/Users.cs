using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Data;
using System.Xml.Linq;
using System.IO;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using UnityEngine.UI;

public class Users : MonoBehaviour
{
    public TMP_InputField login;
    public TMP_InputField password;
    public TextMeshProUGUI error;
    public GameObject description;
    public GameObject authorization;

    public Button[] levelButtons;

    public static int maxLevel = 0;

    async void Start()
    {
        await UnityServices.InitializeAsync();
    }

    /*public void Continue()
    {
        if (login.text.Length <= 3 || login.text.Length > 14)
        {
            error.text = "Длина логина должна быть больше 3 и меньше 14 символов";
            return;
        }
        if (password.text.Length <= 3 || password.text.Length > 20)
        {
            error.text = "Длина пароля должна быть больше 3 и меньше 20 символов";
            return;
        }    

        connection.Open();

        IDbCommand command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM users WHERE name = '" + login.text + "';";
        IDataReader dataReader = command.ExecuteReader();

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
        }
        connection.Close();
        command.Dispose();

        description.SetActive(true);
        authorization.SetActive(false);
    }*/

    async void SignInWithUsernamePasswordAsync(string username, string password)
    {
        try
        {
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);
            GetLevelData();
            description.SetActive(true);
            authorization.SetActive(false);
        }
        catch (AuthenticationException ex)
        {
            error.text = ex.Message;
        }
        catch (RequestFailedException ex)
        {
            error.text = ex.Message;
        }
    }

    public void SignIn()
    {
        SignInWithUsernamePasswordAsync(login.text, password.text);
    }

    async void SignUpWithUsernamePasswordAsync(string username, string password)
    {
        try
        {
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);
            SetUsernameLevelData(username);
            description.SetActive(true);
            authorization.SetActive(false);
        }
        catch (AuthenticationException ex)
        {
            error.text = ex.Message;
        }
        catch (RequestFailedException ex)
        {
            error.text = ex.Message;
        }
    }

    public void SignUp()
    {
        SignUpWithUsernamePasswordAsync(login.text, password.text);
    }

    public static async void SetLevelData(int level)
    {
        maxLevel = level;
        await CloudSaveService.Instance.Data.Player.SaveAsync(new Dictionary<string, object>{
          { "level", level }
        });
    }

    public void ButtonActive()
    {
        for (int i = 0; i <= maxLevel; i++)
            levelButtons[i].interactable = true;
    }

    public static async void SetUsernameLevelData(string username)
    {
        await CloudSaveService.Instance.Data.Player.SaveAsync(new Dictionary<string, object>{
          { "username", username },
          { "level", 0 }
        });
    }

    private async void GetLevelData()
    {
        var playerData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { "level" });
        if (playerData.TryGetValue("level", out var keyName))
        {
            maxLevel = keyName.Value.GetAs<int>();
            Interpreter.level = maxLevel;
        }
        ButtonActive();
    }

    public void ChangeLevel(int level)
    {
        var Level = GameObject.Find("Level(Clone)");
        if (Level != null)
        {
            Interpreter.level = level;
            Level.GetComponent<Level>().NewLevel();
        }
    }
}