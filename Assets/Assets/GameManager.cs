
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.MLAgents;
using System.Collections.Generic;
using NUnit.Framework.Interfaces;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager instance { get; private set; }

    [Header("match")]
    [SerializeField] private float matchDuration = 120f;
    [SerializeField] private float scoreUpdateInterval = 0.2f;

    [Header("boxes")]
    [SerializeField] private BoxSpawner boxSpawner;

    [Header("UI")]
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text redScoreText;
    [SerializeField] private TMP_Text blueScoreText;
    [SerializeField] private TMP_Text episodeText;
    [SerializeField] private TMP_Text leaderText;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private Image timerFill;

    [Header("wins")]
    [SerializeField] private TMP_Text redWinsText;
    [SerializeField] private TMP_Text blueWinsText;
    [SerializeField] private TMP_Text drawsText;

    [Header("stats" )]
    [SerializeField] private TMP_Text redWinRateText;
    [SerializeField] private TMP_Text blueWinRateText;
    [SerializeField] private TMP_Text completedMatchesText;
    [SerializeField] private TMP_Text redAverageRewardText;
    [SerializeField] private TMP_Text blueAverageRewardText;
    [SerializeField] private TMP_Text lastResultText;

    private SimpleMultiAgentGroup blueGroup;
    private SimpleMultiAgentGroup redGroup;

    private readonly List<MoveToTargetAgent> agents = new();

    private float timer;
    private float scoreUpdateTimer;
    private bool matchOver;
    private int episodeNumber = 1;
    private int completedMatches;
    private int redWins;
    private int blueWins;
    private int draws;
    private float totalRedReward;
    private float totalBlueReward;
    private float matchTimerRealTime;

    private float oldTimescale = 1f;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        redGroup = new SimpleMultiAgentGroup();
        blueGroup = new SimpleMultiAgentGroup();
    }

    private void Start()
    {
        StartNewMatch();
        UpdateUI();
    }

    public void RegisterAgent(MoveToTargetAgent agent)
    {
        if (agents.Contains(agent))
            return;

        agents.Add(agent);
        
        if (agent.team == TeamColour.Red)
            redGroup.RegisterAgent(agent);
        else if (agent.team == TeamColour.Blue)
            blueGroup.RegisterAgent(agent);
    }

    public void UnregisterAgent(MoveToTargetAgent agent)
    {
        if (!agents.Contains(agent))
            return;

        if (agent.team == TeamColour.Red)
            redGroup.UnregisterAgent(agent);
        else if (agent.team == TeamColour.Blue)
            blueGroup.UnregisterAgent(agent);
        agents.Remove(agent);
    }

    void NewGame()
    {
        timer = matchDuration;

        matchTimerRealTime = Time.realtimeSinceStartup + matchDuration;

        scoreUpdateTimer = 0f;
        matchOver = false;

        if (resultText != null)
            resultText.text = "";

        UpdateUI();
    }

    private void Update()
    {
        if (matchOver)
            return;

        timer = Mathf.Max(0f, matchTimerRealTime - Time.realtimeSinceStartup);

        scoreUpdateTimer += Time.unscaledDeltaTime;

        if (scoreUpdateTimer >= scoreUpdateInterval)
        {
            scoreUpdateTimer = 0f;
            UpdateUI();
        }

        if (timer <= 0f)
        {
            EndMatch("TIME OUT");
        }
    }

    private void StartNewMatch()
    {
        timer = matchDuration;
        matchOver = false;
        scoreUpdateTimer = 0f;

        UpdateUI();
    }

    private int GetTeamScore(TeamColour team)
    {
        Box[] boxes = FindObjectsByType<Box>(
            FindObjectsSortMode.None
        );

        int score = 0;

        foreach (Box box in boxes)
        {
            if (box.Owner == team)
                score++;
        }

        return score;
    }

    private float GetTeamReward(TeamColour team)
    {
        float reward = 0f;

        foreach (MoveToTargetAgent agent in agents)
        {
            if (agent != null && agent.team == team)
            {
                reward += agent.GetCumulativeReward();
            }
        }

        return reward;
    }

    private int GetTeamAgentCount(TeamColour team)
    {
        int count = 0;

        foreach (MoveToTargetAgent agent in agents)
        {
            if (agent != null && agent.team == team)
                count++;
        }

        return count;
    }

    private void UpdateUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timer / 60f);
            int seconds = Mathf.FloorToInt(timer % 60f);

            timerText.text = $"{minutes:00}:{seconds:00}";
        }

        if (timerFill != null)
        {
            if (matchDuration > 0f)
            {
                timerFill.fillAmount = (float)(timer / matchDuration);
            }
            else
            {
                timerFill.fillAmount = (float)0f;
            }
        }

        int redScore = GetTeamScore(TeamColour.Red);
        int blueScore = GetTeamScore(TeamColour.Blue);

        if (redScoreText != null)
            redScoreText.text = redScore.ToString();

        if (blueScoreText != null)
            blueScoreText.text = blueScore.ToString();

        if (episodeText != null)
            episodeText.text = "EPISODE " + episodeNumber;

        if (redWinsText != null)
            redWinsText.text = redWins.ToString();

        if (blueWinsText != null)
            blueWinsText.text = blueWins.ToString();

        if (drawsText != null)
            drawsText.text = draws.ToString();

        if (completedMatchesText != null)
            completedMatchesText.text = completedMatches.ToString();

        if (leaderText != null)
        {
            if (redScore > blueScore)
                leaderText.text = "RED TEAM WINNING";
            else if (blueScore > redScore)
                leaderText.text = "BLUE TEAM WINNING";
            else
                leaderText.text = "DRAW";
        }

        if (completedMatches > 0)
        {
            float redRate =
                (float)redWins / completedMatches * 100f;

            float blueRate =
                (float)blueWins / completedMatches * 100f;

            if (redWinRateText != null)
                redWinRateText.text = redRate.ToString("F1") + "%";

            if (blueWinRateText != null)
                blueWinRateText.text = blueRate.ToString("F1") + "%";
        }
        else
        {
            if (redWinRateText != null)
                redWinRateText.text = "0%";

            if (blueWinRateText != null)
                blueWinRateText.text = "0%";
        }

        if (completedMatches > 0)
        {
            if (redAverageRewardText != null)
                redAverageRewardText.text =
                    (totalRedReward / completedMatches).ToString("F2");

            if (blueAverageRewardText != null)
                blueAverageRewardText.text =
                    (totalBlueReward / completedMatches).ToString("F2");
        }
    }

    public void EndMatch(string reason)
    {
        if (matchOver)
            return;

        matchOver = true;

        int redScore = GetTeamScore(TeamColour.Red);
        int blueScore = GetTeamScore(TeamColour.Blue);
        float redReward = GetTeamReward(TeamColour.Red);
        float blueReward = GetTeamReward(TeamColour.Blue);

        totalRedReward += redReward;
        totalBlueReward += blueReward;
        completedMatches++;
        string result;

        if (redScore > blueScore)
        {
            redWins++;
            result = "RED WINS!";
        }
        else if (blueScore > redScore)
        {
            blueWins++;
            result = "BLUE WINS!";
        }
        else
        {
            draws++;
            result = "DRAW";
        }

        if (resultText != null)
            resultText.text = result;

        if (lastResultText != null)
        {
            lastResultText.text =
                $"EPISODE {episodeNumber}: {result} " +
                $"(RED {redScore} - BLUE {blueScore})";
        }

        Debug.Log(
            $"Episode {episodeNumber} end " +
            $"Red: {redScore}, blue: {blueScore} " +
            $"winner: {result}"
        );

        UpdateUI();
        if (boxSpawner != null)
            boxSpawner.ResetBoxes();

        redGroup.EndGroupEpisode();
        blueGroup.EndGroupEpisode();

        episodeNumber++;

        StartCoroutine(StartTheNextMatch());
    }

    private IEnumerator StartTheNextMatch()
    {
        oldTimescale = Time.timeScale;
        Time.timeScale = 0f;

        yield return new WaitForSecondsRealtime(1f);

        episodeNumber++;

        Time.timeScale = oldTimescale;

        StartNewMatch();
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            Time.timeScale = oldTimescale;
            instance = null;
        }
    }
}
