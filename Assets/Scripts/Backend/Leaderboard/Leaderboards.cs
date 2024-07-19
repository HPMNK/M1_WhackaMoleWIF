using System.Collections;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.Events;

public class Leaderboards : SingletonDontDestroyGenerated<Leaderboards>
{
    //VAR
    private string _leaderboardScoreId = "score";


    //METHODS
    public override void OnCreate()
    {
        base.OnCreate();
    }

    public static void UpdateScore(int score)
    {
        //if not log in, cant register score, and at this point if the player could not logged in, it means he is just a guest so dont wait for login
        if (!PlayerAccountManager.IsLoggedIn) return;

        UpdatePlayerStatisticsRequest request = new UpdatePlayerStatisticsRequest();
        request.Statistics = new List<StatisticUpdate>();

        //Add the score
        request.Statistics.Add(Instance.GetStatisticUpdate(Instance._leaderboardScoreId, score));

        PlayFabClientAPI.UpdatePlayerStatistics(request, (result) =>
           {
               Debug.Log("Successfully updated leaderboard score ! " + score);
           }, (error) =>
           {
               Debug.Log("Error during the Leaderboard's Update ! " + error);
           });

    }

    //Shortcut to get a StatisticUpdate variable
    private StatisticUpdate GetStatisticUpdate(string val, int data)
    {
        StatisticUpdate statUpdate = new StatisticUpdate();
        statUpdate.StatisticName = val;
        statUpdate.Value = data;
        return statUpdate;
    }

    /// <summary>
    /// Use to get the leaderboard infos
    /// </summary>
    /// <param name="topCount">Amount of players you want to get</param>
    /// <param name="callback">Callback when the results are available</param>
    public void GetTopScores(int topCount, UnityAction<List<PlayerLeaderboardEntry>> callback)
    {
        GetLeaderboardRequest request = new GetLeaderboardRequest();
        request.MaxResultsCount = topCount;
        request.StatisticName = _leaderboardScoreId;
        PlayerProfileViewConstraints constraints = new PlayerProfileViewConstraints();

        PlayFabClientAPI.GetLeaderboard(request, (result) => callback?.Invoke(result.Leaderboard), (error) => Debug.Log("Couldnot retrieve leaderboard datas " + error));

    }

    /*
        Exemple
    
        Leaderboards.GetTopScores((entries) =>
        {
            for (int i = 0; i < entries.Count; i++)
            {
                string playerName = entries[i].DisplayName;
                
                if (entries[i].DisplayName == null)
                {
                    playerName = "Unknown";
                }

                bool isLocal = entries[i].PlayFabId == PlayerAccountManager.Local_PlayfabID;
                int rank = entries[i].Position;
                int score = entries[i].StatValue;
                //Spawn your UI and assign the datas with rank and score variable, use isLocal to set a visual difference
            }
           
        });
    
    */

}
