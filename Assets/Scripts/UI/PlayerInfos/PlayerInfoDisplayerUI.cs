using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PlayerInfoDisplayerUI : MonoBehaviour
{
    enum PlayerDataKey
    {
        Name,
    }

    [SerializeField]
    private PlayerDataKey _dataKey;



    void OnEnable()
    {
        UpdateData();
    }


    private void UpdateData()
    {
        //if not yet logged in, update the data when it is
        if (!PlayerAccountManager.IsLoggedIn)
        {
            PlayerAccountManager.OnLogIn += () => UpdateData();
            return;
        }


        string dataStr = "";

        if (_dataKey == PlayerDataKey.Name)
        {
            dataStr = PlayerAccountManager.TG_UserName;
        }
        GetComponent<TextMeshProUGUI>().text = dataStr;
    }


}
