using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class Gameplay : MonoBehaviour
{
    [Header("References")]
    public PlayerInfo playerInfo;

    // =============================================
    // PANELS
    // =============================================
    [Header("Panels")]
    public GameObject graduationPanel;
    public GameObject playerStatScreen;
    public GameObject devBlueprint;
    public GameObject genreSelection;
    public GameObject gameDevelopment;
    public GameObject resultsScreen;
    public GameObject nextProject;
    public GameObject investInSkills;
    public GameObject retirement;
    public GameObject randomEventPanel;

    // =============================================
    // GRADUATION UI
    // =============================================
    [Header("Graduation UI")]
    public TMP_InputField yearInput;
    public TextMeshProUGUI errorText;

    // =============================================
    // STAT SHEET UI (Fallout-style allocation)
    // =============================================
    [Header("Stat Sheet UI")]
    public TextMeshProUGUI pointsRemainingText;
    public TextMeshProUGUI writingValueText;
    public TextMeshProUGUI modelingValueText;
    public TextMeshProUGUI gameplayValueText;
    public TextMeshProUGUI programmingValueText;
    public TextMeshProUGUI personalityValueText;
    public TextMeshProUGUI fanbaseValueText;

    // =============================================
    // DEV BLUEPRINT UI
    // =============================================
    [Header("Dev Blueprint UI")]
    public TMP_InputField gameNameInput;
    public Toggle oneYearToggle;
    public Toggle twoYearToggle;

    // =============================================
    // GENRE SELECTION UI
    // =============================================
    [Header("Genre Selection UI")]
    public TMP_Dropdown genreDropdown;
    public TMP_Dropdown subGenreDropdown;
    public TextMeshProUGUI genreTrendText;
    public TextMeshProUGUI subGenreTrendText;
    public TextMeshProUGUI allTrendsText; // shows all genre trends at a glance

    // =============================================
    // RANDOM EVENT UI
    // =============================================
    [Header("Random Event UI")]
    public TextMeshProUGUI eventDescriptionText;
    public TextMeshProUGUI eventResultText;
    public Button eventYesButton;
    public Button eventNoButton;
    public Button eventContinueButton;

    // =============================================
    // RESULTS SCREEN UI
    // =============================================
    [Header("Results Screen UI")]
    public TextMeshProUGUI resultGameNameText;
    public TextMeshProUGUI resultGenreText;
    public TextMeshProUGUI resultTrendText;
    public TextMeshProUGUI resultQualityText;
    public TextMeshProUGUI resultPersonalityText;
    public TextMeshProUGUI resultPreviousWorkText;
    public TextMeshProUGUI resultProfitText;
    public TextMeshProUGUI resultAccoladesText;

    // =============================================
    // INVEST IN SKILLS UI
    // =============================================
    [Header("Invest in Skills UI")]
    public Slider investTimeSlider;
    public Slider investMoneySlider;
    public TextMeshProUGUI investTimeLabel;
    public TextMeshProUGUI investMoneyLabel;
    public TextMeshProUGUI investPreviewText;
    public TextMeshProUGUI investCostText;
    public TMP_Dropdown investSkillDropdown;

    // =============================================
    // TOP BAR
    // =============================================
    [Header("Top Bar")]
    public TextMeshProUGUI yearDisplay;
    public TextMeshProUGUI moneyDisplay;

    // =============================================
    // RETIREMENT UI
    // =============================================
    [Header("Retirement UI")]
    public TextMeshProUGUI careerLengthText;
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI retirementGamesListText;
    public TextMeshProUGUI retirementAccoladesText;

    // =============================================
    // PRIVATE STATE
    // =============================================
    private int selectedDevTime = 1;
    private string currentGameName = "";
    private string selectedGenre = "";
    private string selectedSubGenre = "";

    // Random event data
    private List<RandomEvent> randomEvents = new List<RandomEvent>();
    private RandomEvent currentEvent;

    [System.Serializable]
    public class RandomEvent
    {
        public string description;
        public string yesResult1Desc;
        public string yesResult2Desc;
        public string noResultDesc;
        public string affectedStat1;
        public int statChange1;
        public string affectedStat2;
        public int statChange2;
        public int moneyChange;
        // "no" path
        public string noStat;
        public int noStatChange;
    }

    // =============================================
    // START
    // =============================================
    void Start()
    {
        Time.timeScale = 0f;

        if (errorText != null) errorText.text = "";

        // Autofocus input field
        if (yearInput != null) yearInput.ActivateInputField();

        BuildRandomEvents();
        SetupDropdowns();

        Debug.Log("Game started, waiting for graduation input.");
    }

    void Update()
    {
        // Top bar updates
        if (playerInfo.currentYear > 0)
        {
            if (yearDisplay != null)
                yearDisplay.text = "Year: " + playerInfo.currentYear;
            if (moneyDisplay != null)
                moneyDisplay.text = "$" + playerInfo.money;
        }
    }

    // =============================================
    // SETUP HELPERS
    // =============================================
    void SetupDropdowns()
    {
        // Genre dropdowns
        if (genreDropdown != null)
        {
            genreDropdown.ClearOptions();
            genreDropdown.AddOptions(new List<string>(PlayerInfo.AllGenres));
            genreDropdown.onValueChanged.AddListener(OnGenreChanged);
        }

        if (subGenreDropdown != null)
        {
            subGenreDropdown.ClearOptions();
            List<string> subOptions = new List<string> { "None" };
            subOptions.AddRange(PlayerInfo.AllGenres);
            subGenreDropdown.AddOptions(subOptions);
            subGenreDropdown.onValueChanged.AddListener(OnSubGenreChanged);
        }

        // Invest skill dropdown
        if (investSkillDropdown != null)
        {
            investSkillDropdown.ClearOptions();
            investSkillDropdown.AddOptions(new List<string>
            {
                "Writing", "Modeling", "Gameplay", "Programming", "Personality", "Fanbase"
            });
        }

        // Invest sliders
        if (investTimeSlider != null)
        {
            investTimeSlider.minValue = 1;
            investTimeSlider.maxValue = 3;
            investTimeSlider.wholeNumbers = true;
            investTimeSlider.value = 1;
            investTimeSlider.onValueChanged.AddListener(UpdateInvestPreview);
        }

        if (investMoneySlider != null)
        {
            investMoneySlider.minValue = 1;
            investMoneySlider.maxValue = 5;
            investMoneySlider.wholeNumbers = true;
            investMoneySlider.value = 1;
            investMoneySlider.onValueChanged.AddListener(UpdateInvestPreview);
        }
    }

    // =============================================
    // GRADUATION PANEL
    // =============================================
    public void ConfirmGraduationYear()
    {
        string input = yearInput.text;

        if (input.Length != 4 || !int.TryParse(input, out int year))
        {
            errorText.text = "Enter a valid 4-digit year.";
            return;
        }

        playerInfo.InitializePlayer(year);

        Debug.Log("Graduation year set: " + year + " | Seed: " + playerInfo.gameSeed);

        graduationPanel.SetActive(false);
        playerStatScreen.SetActive(true);

        RefreshStatDisplay();
    }

    // =============================================
    // PLAYER STAT SCREEN (Fallout-style)
    // =============================================
    void RefreshStatDisplay()
    {
        if (pointsRemainingText != null)
            pointsRemainingText.text = "Points Remaining: " + playerInfo.statPointsRemaining;
        if (writingValueText != null)
            writingValueText.text = playerInfo.writing.ToString();
        if (modelingValueText != null)
            modelingValueText.text = playerInfo.modeling.ToString();
        if (gameplayValueText != null)
            gameplayValueText.text = playerInfo.gameplay.ToString();
        if (programmingValueText != null)
            programmingValueText.text = playerInfo.programming.ToString();
        if (personalityValueText != null)
            personalityValueText.text = playerInfo.personality.ToString();
        if (fanbaseValueText != null)
            fanbaseValueText.text = playerInfo.fanbase.ToString();
    }

    // Button handlers for stat +/- (called from UI buttons)
    public void IncreaseWriting() { playerInfo.TryIncreaseStat("writing"); RefreshStatDisplay(); }
    public void DecreaseWriting() { playerInfo.TryDecreaseStat("writing"); RefreshStatDisplay(); }
    public void IncreaseModeling() { playerInfo.TryIncreaseStat("modeling"); RefreshStatDisplay(); }
    public void DecreaseModeling() { playerInfo.TryDecreaseStat("modeling"); RefreshStatDisplay(); }
    public void IncreaseGameplay() { playerInfo.TryIncreaseStat("gameplay"); RefreshStatDisplay(); }
    public void DecreaseGameplay() { playerInfo.TryDecreaseStat("gameplay"); RefreshStatDisplay(); }
    public void IncreaseProgramming() { playerInfo.TryIncreaseStat("programming"); RefreshStatDisplay(); }
    public void DecreaseProgramming() { playerInfo.TryDecreaseStat("programming"); RefreshStatDisplay(); }
    public void IncreasePersonality() { playerInfo.TryIncreaseStat("personality"); RefreshStatDisplay(); }
    public void DecreasePersonality() { playerInfo.TryDecreaseStat("personality"); RefreshStatDisplay(); }
    public void IncreaseFanbase() { playerInfo.TryIncreaseStat("fanbase"); RefreshStatDisplay(); }
    public void DecreaseFanbase() { playerInfo.TryDecreaseStat("fanbase"); RefreshStatDisplay(); }

    public void ConfirmPlayerStats()
    {
        playerStatScreen.SetActive(false);
        devBlueprint.SetActive(true);
        Debug.Log("Stats confirmed. Moved to Dev Blueprint.");
    }

    // =============================================
    // DEV BLUEPRINT
    // =============================================
    public void ConfirmDevBlueprint()
    {
        currentGameName = gameNameInput.text;
        if (string.IsNullOrEmpty(currentGameName))
        {
            currentGameName = "Untitled Game";
        }

        selectedDevTime = oneYearToggle.isOn ? 1 : 2;

        Debug.Log("Game Name: " + currentGameName + " | Dev Time: " + selectedDevTime);

        devBlueprint.SetActive(false);
        genreSelection.SetActive(true);

        RefreshTrendDisplay();
    }

    // =============================================
    // GENRE SELECTION
    // =============================================
    void OnGenreChanged(int index)
    {
        selectedGenre = PlayerInfo.AllGenres[index];
        RefreshTrendDisplay();
    }

    void OnSubGenreChanged(int index)
    {
        selectedSubGenre = (index == 0) ? "" : PlayerInfo.AllGenres[index - 1];
        RefreshTrendDisplay();
    }

    void RefreshTrendDisplay()
    {
        if (genreDropdown != null)
            selectedGenre = PlayerInfo.AllGenres[genreDropdown.value];

        if (genreTrendText != null && !string.IsNullOrEmpty(selectedGenre))
            genreTrendText.text = selectedGenre + " Trend: " + playerInfo.GetTrendDisplay(selectedGenre);

        if (subGenreTrendText != null && !string.IsNullOrEmpty(selectedSubGenre))
            subGenreTrendText.text = selectedSubGenre + " Trend: " + playerInfo.GetTrendDisplay(selectedSubGenre);
        else if (subGenreTrendText != null)
            subGenreTrendText.text = "";

        // Show all trends overview
        if (allTrendsText != null)
        {
            string trendsOverview = "--- Industry Trends ---\n";
            foreach (string genre in PlayerInfo.AllGenres)
            {
                trendsOverview += genre + ": " + playerInfo.GetTrendDisplay(genre) + "\n";
            }
            allTrendsText.text = trendsOverview;
        }
    }

    public void ConfirmGenre()
    {
        if (string.IsNullOrEmpty(selectedGenre))
            selectedGenre = PlayerInfo.AllGenres[0];

        Debug.Log("Genre: " + selectedGenre + " | Sub: " + selectedSubGenre);

        genreSelection.SetActive(false);

        // Trigger random event before development
        TriggerRandomEvent();
    }

    // =============================================
    // RANDOM EVENTS
    // =============================================
    void BuildRandomEvents()
    {
        randomEvents.Add(new RandomEvent
        {
            description = "You're invited to a new tech convention by some alumni. Do you go?",
            yesResult1Desc = "You gain valuable insight into a niche modeling skill from a new friend! Modeling +3, but tickets cost $500.",
            yesResult2Desc = "Due to a ticketing issue, your ticket was free! You learn about programming techniques. Programming +2.",
            noResultDesc = "You binge some videos during meals and pick up a new trick. Random skill +1.",
            affectedStat1 = "modeling", statChange1 = 3,
            affectedStat2 = "programming", statChange2 = 2,
            moneyChange = -500,
            noStat = "", noStatChange = 1
        });

        randomEvents.Add(new RandomEvent
        {
            description = "You're invited out to the pub with some old friends. Do you go?",
            yesResult1Desc = "A friend rants about modern storytelling. Writing +2, but you forget some skills from the booze. Random skill -1.",
            yesResult2Desc = "Great storytelling discussion! Writing +2, but you covered everyone's bill. Money -$300.",
            noResultDesc = "You had wild shower thoughts about a tool application. Programming +1.",
            affectedStat1 = "writing", statChange1 = 2,
            affectedStat2 = "writing", statChange2 = 2,
            moneyChange = -300,
            noStat = "programming", noStatChange = 1
        });

        randomEvents.Add(new RandomEvent
        {
            description = "A game jam is happening this weekend! Do you participate?",
            yesResult1Desc = "Your team wins! Great experience all around. Gameplay +3, Programming +1.",
            yesResult2Desc = "You didn't win, but you learned a ton about rapid prototyping. Gameplay +2.",
            noResultDesc = "You spend the weekend studying design patterns. Programming +1.",
            affectedStat1 = "gameplay", statChange1 = 3,
            affectedStat2 = "gameplay", statChange2 = 2,
            moneyChange = 0,
            noStat = "programming", noStatChange = 1
        });

        randomEvents.Add(new RandomEvent
        {
            description = "A popular streamer wants to interview you about indie dev life. Do you accept?",
            yesResult1Desc = "The interview goes viral! Your personality shines. Personality +3, Fanbase +2.",
            yesResult2Desc = "Decent interview, but some trolls appeared. Personality +1, Fanbase +1.",
            noResultDesc = "You spend the time working on your craft instead. Modeling +1.",
            affectedStat1 = "personality", statChange1 = 3,
            affectedStat2 = "personality", statChange2 = 1,
            moneyChange = 0,
            noStat = "modeling", noStatChange = 1
        });

        randomEvents.Add(new RandomEvent
        {
            description = "You find an online course on advanced 3D modeling on sale. Do you buy it?",
            yesResult1Desc = "The course is incredible! Modeling +3. Cost: $400.",
            yesResult2Desc = "The course was okay, but you picked up some tricks. Modeling +2. Cost: $400.",
            noResultDesc = "You find a free tutorial instead. Modeling +1.",
            affectedStat1 = "modeling", statChange1 = 3,
            affectedStat2 = "modeling", statChange2 = 2,
            moneyChange = -400,
            noStat = "modeling", noStatChange = 1
        });

        randomEvents.Add(new RandomEvent
        {
            description = "Your computer crashes and you lose some progress. Do you spend money to recover data?",
            yesResult1Desc = "Data recovery successful! You lose $600 but save your work. Programming +1 from the experience.",
            yesResult2Desc = "Partial recovery. You lose $600 and have to redo some work, but learn backup strategies. Programming +2.",
            noResultDesc = "You rebuild from scratch. Painful, but you improve. Programming +1, Gameplay +1.",
            affectedStat1 = "programming", statChange1 = 1,
            affectedStat2 = "programming", statChange2 = 2,
            moneyChange = -600,
            noStat = "programming", noStatChange = 1
        });

        randomEvents.Add(new RandomEvent
        {
            description = "A friend asks you to co-author a game design blog post. Do you help?",
            yesResult1Desc = "The post blows up! Great exposure. Writing +2, Fanbase +2.",
            yesResult2Desc = "Modest readership but good practice. Writing +2.",
            noResultDesc = "You journal your own thoughts instead. Writing +1.",
            affectedStat1 = "writing", statChange1 = 2,
            affectedStat2 = "writing", statChange2 = 2,
            moneyChange = 0,
            noStat = "writing", noStatChange = 1
        });

        randomEvents.Add(new RandomEvent
        {
            description = "You discover a local indie dev meetup. Do you attend?",
            yesResult1Desc = "You meet amazing people and form connections! Personality +2, Fanbase +2. Dinner cost $200.",
            yesResult2Desc = "Quiet night, but you learned about industry trends. Gameplay +1, Personality +1.",
            noResultDesc = "You practice your skills solo. Random skill +1.",
            affectedStat1 = "personality", statChange1 = 2,
            affectedStat2 = "gameplay", statChange2 = 1,
            moneyChange = -200,
            noStat = "", noStatChange = 1
        });
    }

    void TriggerRandomEvent()
    {
        if (randomEvents.Count == 0 || randomEventPanel == null)
        {
            // Skip events if no panel is set up yet
            gameDevelopment.SetActive(true);
            Time.timeScale = 1f;
            return;
        }

        currentEvent = randomEvents[Random.Range(0, randomEvents.Count)];

        randomEventPanel.SetActive(true);

        if (eventDescriptionText != null)
            eventDescriptionText.text = currentEvent.description;
        if (eventResultText != null)
            eventResultText.text = "";

        // Show choice buttons, hide continue
        if (eventYesButton != null) eventYesButton.gameObject.SetActive(true);
        if (eventNoButton != null) eventNoButton.gameObject.SetActive(true);
        if (eventContinueButton != null) eventContinueButton.gameObject.SetActive(false);
    }

    public void EventChoiceYes()
    {
        if (currentEvent == null) return;

        // Random pick between two outcomes
        bool outcome1 = Random.value > 0.5f;
        string resultDesc;

        if (outcome1)
        {
            resultDesc = currentEvent.yesResult1Desc;
            ApplyStatChange(currentEvent.affectedStat1, currentEvent.statChange1);
            if (currentEvent.moneyChange != 0)
                playerInfo.money += currentEvent.moneyChange;
        }
        else
        {
            resultDesc = currentEvent.yesResult2Desc;
            ApplyStatChange(currentEvent.affectedStat2, currentEvent.statChange2);
            // Second outcome might also cost money (for events like the pub)
            if (currentEvent.moneyChange != 0 && currentEvent.affectedStat2 == currentEvent.affectedStat1)
                playerInfo.money += currentEvent.moneyChange;
        }

        if (eventResultText != null) eventResultText.text = resultDesc;

        // Hide choice buttons, show continue
        if (eventYesButton != null) eventYesButton.gameObject.SetActive(false);
        if (eventNoButton != null) eventNoButton.gameObject.SetActive(false);
        if (eventContinueButton != null) eventContinueButton.gameObject.SetActive(true);
    }

    public void EventChoiceNo()
    {
        if (currentEvent == null) return;

        string stat = currentEvent.noStat;
        if (string.IsNullOrEmpty(stat))
        {
            // Random skill +1
            string[] stats = { "writing", "modeling", "gameplay", "programming", "personality", "fanbase" };
            stat = stats[Random.Range(0, stats.Length)];
        }

        ApplyStatChange(stat, currentEvent.noStatChange);

        if (eventResultText != null)
            eventResultText.text = currentEvent.noResultDesc;

        if (eventYesButton != null) eventYesButton.gameObject.SetActive(false);
        if (eventNoButton != null) eventNoButton.gameObject.SetActive(false);
        if (eventContinueButton != null) eventContinueButton.gameObject.SetActive(true);
    }

    public void EventContinue()
    {
        randomEventPanel.SetActive(false);
        gameDevelopment.SetActive(true);
        Time.timeScale = 1f;
        Debug.Log("Event resolved. Gameplay resumed.");
    }

    void ApplyStatChange(string stat, int change)
    {
        int current = playerInfo.GetStat(stat);
        int newVal = Mathf.Clamp(current + change, 1, PlayerInfo.STAT_CAP);
        playerInfo.SetStat(stat, newVal);
    }

    // =============================================
    // GAME DEVELOPMENT
    // =============================================
    public void SkipToEndCycle()
    {
        gameDevelopment.SetActive(false);
        ShowResults();
    }

    // =============================================
    // RESULTS SCREEN
    // =============================================
    void ShowResults()
    {
        resultsScreen.SetActive(true);

        // Calculate everything
        int quality = playerInfo.CalculateGameQuality(selectedGenre, selectedSubGenre);
        float trendMult = playerInfo.GetTrendMultiplier(selectedGenre);
        float personalityMult = (playerInfo.personality >= PlayerInfo.STAT_CAP) ? 2.0f : 1.0f + playerInfo.personality * 0.02f;
        float previousEarnings = 0f;
        foreach (var record in playerInfo.gameHistory)
            previousEarnings += record.profit * 0.5f;

        float baseProfit = quality * 50f;
        float totalProfit = (baseProfit * trendMult * personalityMult) + previousEarnings;
        if (selectedDevTime >= 2) totalProfit *= 1.25f;
        int finalProfit = Mathf.Max(100, Mathf.RoundToInt(totalProfit));

        // Display breakdown
        if (resultGameNameText != null)
            resultGameNameText.text = currentGameName;
        if (resultGenreText != null)
            resultGenreText.text = selectedGenre + (!string.IsNullOrEmpty(selectedSubGenre) ? " / " + selectedSubGenre : "");
        if (resultTrendText != null)
            resultTrendText.text = "Trend: " + playerInfo.GetTrendDisplay(selectedGenre) + " (x" + trendMult.ToString("F2") + ")";
        if (resultQualityText != null)
            resultQualityText.text = "Game Quality: " + quality + "/98";
        if (resultPersonalityText != null)
            resultPersonalityText.text = "Personality Bonus: x" + personalityMult.ToString("F2");
        if (resultPreviousWorkText != null)
            resultPreviousWorkText.text = "Previous Work Bonus: +$" + Mathf.RoundToInt(previousEarnings);
        if (resultProfitText != null)
            resultProfitText.text = "PROFIT: $" + finalProfit;

        // Check accolades
        if (resultAccoladesText != null)
        {
            List<string> accolades = playerInfo.GetAccolades();
            resultAccoladesText.text = accolades.Count > 0 ? string.Join(", ", accolades) : "";
        }

        // Store for ProceedResults
        _pendingProfit = finalProfit;
        _pendingQuality = quality;

        Debug.Log("Results displayed. Quality: " + quality + " Profit: $" + finalProfit);
    }

    private int _pendingProfit;
    private int _pendingQuality;

    public void ProceedResults()
    {
        // Advance year + trends
        playerInfo.currentYear += selectedDevTime;
        playerInfo.AdvanceTrends();

        // Add profit
        playerInfo.money += _pendingProfit;

        // Save game record
        playerInfo.AddGame(currentGameName, selectedDevTime, _pendingProfit, _pendingQuality, selectedGenre, selectedSubGenre);

        Debug.Log("Results applied. Year: " + playerInfo.currentYear + " Money: $" + playerInfo.money);

        resultsScreen.SetActive(false);
        nextProject.SetActive(true);
    }

    // =============================================
    // NEXT PROJECT
    // =============================================
    public void NewGame()
    {
        // Check if player can afford it
        int gameCost = 2000;
        if (playerInfo.money < gameCost)
        {
            // Force retirement
            Retire();
            return;
        }

        playerInfo.money -= gameCost;
        nextProject.SetActive(false);
        devBlueprint.SetActive(true);
    }

    public void InvestSkills()
    {
        if (playerInfo.money < 1000)
        {
            // Can't afford minimum investment
            Retire();
            return;
        }

        nextProject.SetActive(false);
        investInSkills.SetActive(true);

        // Update slider max based on available money
        if (investMoneySlider != null)
        {
            int maxThousands = Mathf.Min(playerInfo.money / 1000, 5);
            investMoneySlider.maxValue = Mathf.Max(1, maxThousands);
            investMoneySlider.value = 1;
        }

        UpdateInvestPreview(0); // trigger initial preview
    }

    public void Retire()
    {
        nextProject.SetActive(false);
        retirement.SetActive(true);

        int careerLength = playerInfo.currentYear - playerInfo.graduationYear;

        if (careerLengthText != null)
            careerLengthText.text = "Career Length: " + careerLength + " years";
        if (moneyText != null)
            moneyText.text = "Money Earned: $" + playerInfo.money;

        // Games list
        if (retirementGamesListText != null)
        {
            string gamesList = "--- Games Made ---\n";
            foreach (var record in playerInfo.gameHistory)
            {
                gamesList += record.gameName + " (" + record.genre + ") - " + record.metacriticScore + "/98 - $" + record.profit + "\n";
            }
            if (playerInfo.gameHistory.Count == 0)
                gamesList += "No games made.\n";
            retirementGamesListText.text = gamesList;
        }

        // Accolades
        if (retirementAccoladesText != null)
        {
            List<string> accolades = playerInfo.GetAccolades();
            if (accolades.Count > 0)
                retirementAccoladesText.text = "--- Accolades ---\n" + string.Join("\n", accolades);
            else
                retirementAccoladesText.text = "No accolades earned.";
        }

        Debug.Log("Player retired.");
    }

    // =============================================
    // INVEST IN SKILLS
    // =============================================
    void UpdateInvestPreview(float _)
    {
        if (investTimeSlider == null || investMoneySlider == null) return;

        int years = Mathf.RoundToInt(investTimeSlider.value);
        int moneyUnits = Mathf.RoundToInt(investMoneySlider.value);

        // Time multiplier: M(y) = 1 + 0.5*(y-1)
        float timeMult = 1f + 0.5f * (years - 1);

        // Money to skill points with depreciation:
        // 1st point = $1000, 2nd = $1500, 3rd+ = $2000 each
        int totalCost = 0;
        int basePoints = 0;
        for (int i = 1; i <= moneyUnits; i++)
        {
            if (i == 1) totalCost += 1000;
            else if (i == 2) totalCost += 1500;
            else totalCost += 2000;
            basePoints++;
        }

        int finalPoints = Mathf.RoundToInt(basePoints * timeMult);

        if (investTimeLabel != null)
            investTimeLabel.text = years + " Year" + (years > 1 ? "s" : "") + " (x" + timeMult.ToString("F1") + ")";
        if (investMoneyLabel != null)
            investMoneyLabel.text = "$" + totalCost;
        if (investPreviewText != null)
            investPreviewText.text = "~" + finalPoints + " skill points";
        if (investCostText != null)
            investCostText.text = "Cost: $" + totalCost + " + " + years + " year" + (years > 1 ? "s" : "");
    }

    public void ConfirmInvestment()
    {
        if (investTimeSlider == null || investMoneySlider == null || investSkillDropdown == null) return;

        int years = Mathf.RoundToInt(investTimeSlider.value);
        int moneyUnits = Mathf.RoundToInt(investMoneySlider.value);

        float timeMult = 1f + 0.5f * (years - 1);

        int totalCost = 0;
        int basePoints = 0;
        for (int i = 1; i <= moneyUnits; i++)
        {
            if (i == 1) totalCost += 1000;
            else if (i == 2) totalCost += 1500;
            else totalCost += 2000;
            basePoints++;
        }

        if (playerInfo.money < totalCost) return; // can't afford

        int finalPoints = Mathf.RoundToInt(basePoints * timeMult);

        // Apply
        string[] skillNames = { "writing", "modeling", "gameplay", "programming", "personality", "fanbase" };
        string chosenSkill = skillNames[investSkillDropdown.value];

        int current = playerInfo.GetStat(chosenSkill);
        int newVal = Mathf.Clamp(current + finalPoints, 1, PlayerInfo.STAT_CAP);
        playerInfo.SetStat(chosenSkill, newVal);

        playerInfo.money -= totalCost;
        playerInfo.currentYear += years;
        playerInfo.AdvanceTrends();

        Debug.Log("Invested in " + chosenSkill + ": +" + finalPoints + " points. Cost: $" + totalCost + ", " + years + " years.");

        investInSkills.SetActive(false);
        nextProject.SetActive(true);
    }

    public void FinishInvesting()
    {
        investInSkills.SetActive(false);
        nextProject.SetActive(true);
    }

    // =============================================
    // RETIREMENT PANEL
    // =============================================
    public void ExitGame()
    {
        Debug.Log("Game Closed.");
        Application.Quit();
    }

    public void StartNewRun()
    {
        Debug.Log("Restarting Game.");
        SceneManager.LoadScene(0);
    }
}
