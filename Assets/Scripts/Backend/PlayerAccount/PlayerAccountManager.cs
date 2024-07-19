using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.Events;




public class PlayerAccountManager : SingletonDontDestroy<PlayerAccountManager>
{
    /// VARIABLES

    [SerializeField]
    private bool _isTelegramBuild = true;

    private bool _isTryingToLogin = false;
    private bool _isLoggedIn = false;
    public static bool IsLoggedIn
    {
        get { return PlayerAccountManager.Instance._isLoggedIn; }
    }


    //default url for the game to be playable in editor or other build env
    private string DefaultURL
    {
        get
        {
            return "https://dvv06vczfssco.cloudfront.net/index.html?userId=6407237237&?userName=DeytoohKr&";
        }
    }

    public static UnityAction OnLogIn;

    //Infos got from the URL (telegram userId and userName + additional things)
    private TelegramURLInfos _telegramUserInfos;
    public static string TG_UserName { get { return Instance._telegramUserInfos.GetTelegramUserName(); } }
    public static string TG_UserId { get { return Instance._telegramUserInfos.GetTelegramUserID(); } }

    private PlayerProfileModel _localPlayer;
    public static string Local_PlayfabID { get { return Instance._localPlayer.PlayerId; } }


    /// UNITY METHODS


    //Custom Awake()
    public override void OnCreate()
    {
        base.OnCreate();
        AutoLogin();

    }

    /// LOG IN METHODS
    private void AutoLogin()
    {
        if (_isTryingToLogin || _isLoggedIn) return;

        _isTryingToLogin = true;
        LoginTelegram();
    }

    private void ConfirmLogin()
    {
        _isLoggedIn = true;
        _isTryingToLogin = false;
        OnLogIn?.Invoke();
    }


    private void LoginTelegram()
    {

        string url = Application.absoluteURL;


        //use my url in editor
#if UNITY_EDITOR
        url = DefaultURL;

#endif
        if (!_isTelegramBuild)
        {
            url = DefaultURL;
        }


        //Get the URL parameters (telegram id, username, others...)
        _telegramUserInfos = new TelegramURLInfos(url);

        Debug.Log(url);

        LoginWithCustomIDRequest loginRequest = new LoginWithCustomIDRequest();
        loginRequest.CreateAccount = true;
        loginRequest.CustomId = _telegramUserInfos.GetTelegramUserID();
        if (loginRequest.CustomId == "null")
        {
            Debug.LogError("Could not log in, tg id is null (from URL)");
            return;
        }


        PlayFabClientAPI.LoginWithCustomID(loginRequest, (result) =>
        {
            Debug.Log("Log in Success");
            Debug.Log("Account ID : " + result.PlayFabId);
            Debug.Log(result.ToJson());
            bool hasBeenCreated = result.NewlyCreated;

            //should avoid to UpdateDisplayName every log in but as on TG we can change it easily, better to update it
            //If the account was just made, update the display name of the account to the tg username
            UpdateDisplayName(_telegramUserInfos.GetTelegramUserName(), () =>
            {
                //get the local playfab profile after setting the username to finalize the Log In Process
                GetLocalProfile(result.PlayFabId);
            });

        }, (error) =>
        {
            Debug.LogError("Could not log in : " + error.ErrorMessage);
        });
    }

    public void UpdateDisplayName(string newName, UnityEngine.Events.UnityAction callback = null)
    {
        string newDisplayName = newName;

        var request = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = newDisplayName
        };

        PlayFabClientAPI.UpdateUserTitleDisplayName(request, (result) =>
        {
            Debug.Log("Updated the user's name " + request.DisplayName);
            callback?.Invoke();
        }, (error) =>
        {
            Debug.LogError("Could not update the user's name ! " + error);
            callback?.Invoke();
        });
    }

    private void GetLocalProfile(string playFabId = "")
    {
        Debug.Log("Get Local Profile");
        GetPlayerProfileRequest request = new GetPlayerProfileRequest();
        request.PlayFabId = playFabId;
        request.ProfileConstraints = new PlayerProfileViewConstraints()
        {
            ShowDisplayName = true
        };

        PlayFabClientAPI.GetPlayerProfile(request, (result) =>
        {
            Debug.Log("Local Profile Loaded " + result.PlayerProfile.DisplayName);
            Debug.Log(result.ToJson());
            _localPlayer = result.PlayerProfile;
            ConfirmLogin();
        }, (error) =>
        {
            Debug.Log("Could not retrieve local profile ! " + error);
        });

    }

}

class TelegramURLInfos
{
    private Dictionary<string, string> _urlInfos;

    public TelegramURLInfos(string url)
    {
        _urlInfos = new Dictionary<string, string>();
        //Search for the URL Parameters we need
        string[] urlParams = url.Split('?');
        for (int i = 0; i < urlParams.Length; i++)
        {
            string currentParam = urlParams[i];
            if (currentParam.Contains("="))
            {
                string[] currentParamDetails = currentParam.Split('=');
                if (currentParamDetails.Length > 1)
                {
                    string key = currentParamDetails[0];
                    string[] value = currentParamDetails[1].Split('&');
                    if (value.Length > 0)
                    {
                        Add(key, value[0]);
                    }
                }
            }
        }
    }

    private string _userIdKey = "userId";
    private string _userNameKey = "userName";

    private string _emptyValue = "null";

    public void Add(string key, string value)
    {
        if (_urlInfos.ContainsKey(key)) return;
        _urlInfos.Add(key, value);
    }

    public string GetTelegramUserID()
    {
        return _urlInfos.ContainsKey(_userIdKey) ? _urlInfos[_userIdKey] : _emptyValue;
    }
    public string GetTelegramUserName()
    {
        return _urlInfos.ContainsKey(_userNameKey) ? _urlInfos[_userNameKey] : _emptyValue;
    }

}
