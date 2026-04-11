using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Self-contained game controller that creates ALL UI at runtime.
/// Attach to any GameObject in the Game scene. Bypasses scene Canvas issues.
/// </summary>
public class GameBootstrap : MonoBehaviour
{
    // ========== PLAYER DATA ==========
    const int STAT_CAP = 20;
    const int STARTING_POINTS = 10;
    const int STARTING_MONEY = 5000;
    const int GAME_COST = 2000;

    int graduationYear, currentYear, money;
    int writing = 1, modeling = 1, gameplay = 1, programming = 1, personality = 1, fanbase = 1;
    int statPointsRemaining;
    int gameSeed;
    Dictionary<string, int> genreTrends = new Dictionary<string, int>();
    List<GameRecord> gameHistory = new List<GameRecord>();

    class GameRecord
    {
        public string gameName;
        public int devTime, profit, score;
        public string genre, subGenre;
    }

    static readonly string[] AllGenres = {
        "Shooter", "Action-Adventure", "Role-Playing", "Sports", "Simulation",
        "Racing", "Fighting", "Platformer", "Survival Craft", "Strategy",
        "Survival Horror", "Open World", "Roguelike/lite", "Deck Builder",
        "Party/Casual", "Idle", "Narrative", "Rhythm", "Tower Defense", "Puzzle"
    };

    // ========== UI REFERENCES ==========
    Canvas canvas;
    GameObject currentPanel;

    // Game state
    string currentGameName = "";
    int selectedDevTime = 1;
    string selectedGenre = "";
    string selectedSubGenre = "";
    int pendingProfit, pendingQuality;

    // ========== COLORS ==========
    Color bgColor = new Color(0.08f, 0.08f, 0.14f, 0.95f);
    Color btnColor = new Color(0.2f, 0.25f, 0.35f, 1f);
    Color btnHover = new Color(0.3f, 0.35f, 0.45f, 1f);
    Color accentColor = new Color(0.3f, 0.6f, 1f, 1f);
    Color profitColor = new Color(0.3f, 1f, 0.3f, 1f);

    // ========== LIFECYCLE ==========
    void Start()
    {
        CreateCanvas();
        ShowGraduationPanel();
    }

    void CreateCanvas()
    {
        // Destroy any existing Canvas in scene to avoid conflicts
        Canvas[] existingCanvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (var c in existingCanvases)
        {
            if (c.gameObject != gameObject)
                Destroy(c.gameObject);
        }

        GameObject canvasGo = new GameObject("RuntimeCanvas");
        canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        canvasGo.AddComponent<GraphicRaycaster>();
    }

    // ========== UI HELPERS ==========
    GameObject CreatePanel(string name)
    {
        if (currentPanel != null) Destroy(currentPanel);

        GameObject panel = new GameObject(name);
        panel.transform.SetParent(canvas.transform, false);

        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        Image img = panel.AddComponent<Image>();
        img.color = bgColor;

        currentPanel = panel;
        return panel;
    }

    TextMeshProUGUI AddText(Transform parent, string text, float y, int fontSize = 28, Color? color = null)
    {
        GameObject go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(0, y);
        rt.sizeDelta = new Vector2(800, fontSize + 16);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = color ?? Color.white;
        return tmp;
    }

    Button AddButton(Transform parent, string label, float x, float y, System.Action onClick, float width = 220, float height = 50)
    {
        GameObject go = new GameObject("Btn_" + label);
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(width, height);

        Image img = go.AddComponent<Image>();
        img.color = btnColor;

        Button btn = go.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.highlightedColor = btnHover;
        colors.pressedColor = new Color(0.15f, 0.15f, 0.25f, 1f);
        btn.colors = colors;

        // Label
        GameObject textGo = new GameObject("Label");
        textGo.transform.SetParent(go.transform, false);
        RectTransform textRt = textGo.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;
        TextMeshProUGUI tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 22;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        btn.onClick.AddListener(() => onClick());
        return btn;
    }

    TMP_InputField AddInputField(Transform parent, string placeholder, float y, float width = 300)
    {
        GameObject go = new GameObject("InputField");
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(0, y);
        rt.sizeDelta = new Vector2(width, 45);

        Image img = go.AddComponent<Image>();
        img.color = new Color(0.15f, 0.15f, 0.2f, 1f);

        TMP_InputField input = go.AddComponent<TMP_InputField>();

        // Text Area
        GameObject textArea = new GameObject("TextArea");
        textArea.transform.SetParent(go.transform, false);
        RectTransform taRt = textArea.AddComponent<RectTransform>();
        taRt.anchorMin = Vector2.zero;
        taRt.anchorMax = Vector2.one;
        taRt.offsetMin = new Vector2(10, 0);
        taRt.offsetMax = new Vector2(-10, 0);
        textArea.AddComponent<RectMask2D>();

        // Input text
        GameObject textGo = new GameObject("Text");
        textGo.transform.SetParent(textArea.transform, false);
        RectTransform itRt = textGo.AddComponent<RectTransform>();
        itRt.anchorMin = Vector2.zero;
        itRt.anchorMax = Vector2.one;
        itRt.sizeDelta = Vector2.zero;
        TextMeshProUGUI itTmp = textGo.AddComponent<TextMeshProUGUI>();
        itTmp.fontSize = 24;
        itTmp.color = Color.white;

        // Placeholder
        GameObject phGo = new GameObject("Placeholder");
        phGo.transform.SetParent(textArea.transform, false);
        RectTransform phRt = phGo.AddComponent<RectTransform>();
        phRt.anchorMin = Vector2.zero;
        phRt.anchorMax = Vector2.one;
        phRt.sizeDelta = Vector2.zero;
        TextMeshProUGUI phTmp = phGo.AddComponent<TextMeshProUGUI>();
        phTmp.text = placeholder;
        phTmp.fontSize = 24;
        phTmp.fontStyle = FontStyles.Italic;
        phTmp.color = new Color(0.5f, 0.5f, 0.5f, 0.7f);

        input.textComponent = itTmp;
        input.textViewport = taRt;
        input.placeholder = phTmp;

        return input;
    }

    // ========== GRADUATION PANEL ==========
    void ShowGraduationPanel()
    {
        GameObject panel = CreatePanel("GraduationPanel");

        AddText(panel.transform, "THE INDIE PROTEGE", 200, 42, accentColor);
        AddText(panel.transform, "What year did you graduate?", 100, 24);

        TMP_InputField yearInput = AddInputField(panel.transform, "Enter 4-digit year...", 30);
        yearInput.characterLimit = 4;
        yearInput.contentType = TMP_InputField.ContentType.IntegerNumber;

        TextMeshProUGUI errorText = AddText(panel.transform, "", -30, 20, Color.red);

        AddButton(panel.transform, "CONFIRM", 0, -100, () =>
        {
            string input = yearInput.text;
            if (input.Length != 4 || !int.TryParse(input, out int year))
            {
                errorText.text = "Enter a valid 4-digit year.";
                return;
            }
            graduationYear = year;
            currentYear = year;
            money = STARTING_MONEY;
            statPointsRemaining = STARTING_POINTS;
            gameSeed = year * 1000 + System.DateTime.Now.Millisecond;
            Random.InitState(gameSeed);

            genreTrends.Clear();
            foreach (string g in AllGenres)
                genreTrends[g] = Random.Range(2, 6);

            ShowStatSheet();
        });

        yearInput.ActivateInputField();
    }

    // ========== STAT SHEET ==========
    TextMeshProUGUI[] statValueTexts;
    TextMeshProUGUI pointsText;

    void ShowStatSheet()
    {
        GameObject panel = CreatePanel("StatSheet");

        AddText(panel.transform, "STAT ALLOCATION", 320, 36, accentColor);
        pointsText = AddText(panel.transform, "Points: " + statPointsRemaining, 270, 24);

        string[] names = { "Writing", "Modeling", "Gameplay", "Programming", "Personality", "Fanbase" };
        statValueTexts = new TextMeshProUGUI[6];

        for (int i = 0; i < 6; i++)
        {
            float y = 190 - i * 70;
            int idx = i;

            AddText(panel.transform, names[i], y, 22).rectTransform.anchoredPosition = new Vector2(-120, y);
            AddButton(panel.transform, "-", -10, y, () => { DecreaseStat(idx); }, 45, 40);
            statValueTexts[i] = AddText(panel.transform, "1", y, 26, accentColor);
            statValueTexts[i].rectTransform.anchoredPosition = new Vector2(50, y);
            statValueTexts[i].rectTransform.sizeDelta = new Vector2(50, 40);
            AddButton(panel.transform, "+", 110, y, () => { IncreaseStat(idx); }, 45, 40);
        }

        AddButton(panel.transform, "CONFIRM STATS", 0, -280, () => { ShowDevBlueprint(); }, 280, 55);
    }

    int GetStatByIndex(int i)
    {
        switch (i)
        {
            case 0: return writing;
            case 1: return modeling;
            case 2: return gameplay;
            case 3: return programming;
            case 4: return personality;
            case 5: return fanbase;
            default: return 0;
        }
    }

    void SetStatByIndex(int i, int val)
    {
        switch (i)
        {
            case 0: writing = val; break;
            case 1: modeling = val; break;
            case 2: gameplay = val; break;
            case 3: programming = val; break;
            case 4: personality = val; break;
            case 5: fanbase = val; break;
        }
    }

    void IncreaseStat(int i)
    {
        if (statPointsRemaining <= 0 || GetStatByIndex(i) >= STAT_CAP) return;
        SetStatByIndex(i, GetStatByIndex(i) + 1);
        statPointsRemaining--;
        RefreshStatUI();
    }

    void DecreaseStat(int i)
    {
        if (GetStatByIndex(i) <= 1) return;
        SetStatByIndex(i, GetStatByIndex(i) - 1);
        statPointsRemaining++;
        RefreshStatUI();
    }

    void RefreshStatUI()
    {
        if (pointsText != null) pointsText.text = "Points: " + statPointsRemaining;
        for (int i = 0; i < 6; i++)
            if (statValueTexts != null && statValueTexts[i] != null)
                statValueTexts[i].text = GetStatByIndex(i).ToString();
    }

    // ========== DEV BLUEPRINT ==========
    void ShowDevBlueprint()
    {
        GameObject panel = CreatePanel("DevBlueprint");

        AddText(panel.transform, "GAME BLUEPRINT", 250, 36, accentColor);
        AddText(panel.transform, "Year: " + currentYear + "  |  Money: $" + money, 200, 20);

        AddText(panel.transform, "Game Name:", 120, 22);
        TMP_InputField nameInput = AddInputField(panel.transform, "Enter game name...", 70);

        AddText(panel.transform, "Development Time:", -10, 22);
        selectedDevTime = 1;
        TextMeshProUGUI devTimeText = AddText(panel.transform, "1 Year", -60, 24, accentColor);

        AddButton(panel.transform, "1 Year", -80, -120, () =>
        {
            selectedDevTime = 1;
            devTimeText.text = "1 Year";
        }, 150, 45);

        AddButton(panel.transform, "2 Years", 80, -120, () =>
        {
            selectedDevTime = 2;
            devTimeText.text = "2 Years";
        }, 150, 45);

        AddButton(panel.transform, "START DEV", 0, -220, () =>
        {
            currentGameName = string.IsNullOrEmpty(nameInput.text) ? "Untitled Game" : nameInput.text;
            ShowGenreSelection();
        }, 250, 55);
    }

    // ========== GENRE SELECTION ==========
    void ShowGenreSelection()
    {
        GameObject panel = CreatePanel("GenreSelection");

        AddText(panel.transform, "CHOOSE A GENRE", 400, 36, accentColor);

        selectedGenre = AllGenres[0];
        selectedSubGenre = "";

        // Genre list with trends
        float startY = 330;
        for (int i = 0; i < AllGenres.Length; i++)
        {
            int idx = i;
            string g = AllGenres[i];
            string trend = new string('!', genreTrends[g]);
            float y = startY - i * 38;

            AddButton(panel.transform, g + " " + trend, 0, y, () =>
            {
                selectedGenre = AllGenres[idx];
            }, 400, 34);
        }

        AddButton(panel.transform, "CONFIRM GENRE", 0, -420, () =>
        {
            ShowRandomEvent();
        }, 250, 55);
    }

    // ========== RANDOM EVENTS ==========
    void ShowRandomEvent()
    {
        string[] events = {
            "You're invited to a tech convention. Do you go?",
            "A game jam is happening this weekend! Participate?",
            "A streamer wants to interview you. Accept?",
            "You find an online course on sale. Buy it?",
            "Friends invite you to the pub. Do you go?",
            "A local indie dev meetup is tonight. Attend?",
            "A friend asks you to co-author a blog post. Help?",
            "Your computer crashes. Spend money to recover data?"
        };

        string evt = events[Random.Range(0, events.Length)];

        GameObject panel = CreatePanel("RandomEvent");
        AddText(panel.transform, "RANDOM EVENT", 200, 36, accentColor);
        AddText(panel.transform, evt, 100, 24);

        TextMeshProUGUI resultText = AddText(panel.transform, "", -20, 22, profitColor);

        AddButton(panel.transform, "YES", -100, -100, () =>
        {
            // Random positive outcome
            string[] skills = { "Writing", "Modeling", "Gameplay", "Programming", "Personality", "Fanbase" };
            int skillIdx = Random.Range(0, 6);
            int gain = Random.Range(1, 4);
            int current = GetStatByIndex(skillIdx);
            SetStatByIndex(skillIdx, Mathf.Min(current + gain, STAT_CAP));

            int moneyCost = Random.value > 0.5f ? Random.Range(100, 500) : 0;
            money -= moneyCost;

            resultText.text = skills[skillIdx] + " +" + gain +
                (moneyCost > 0 ? "  |  Money -$" + moneyCost : "  |  Free!");

            // Replace buttons with continue
            foreach (Button b in panel.GetComponentsInChildren<Button>())
                Destroy(b.gameObject);
            AddButton(panel.transform, "CONTINUE", 0, -100, () => { ShowDevelopment(); }, 200, 50);
        });

        AddButton(panel.transform, "NO (Safe +1)", 100, -100, () =>
        {
            int skillIdx = Random.Range(0, 6);
            int current = GetStatByIndex(skillIdx);
            SetStatByIndex(skillIdx, Mathf.Min(current + 1, STAT_CAP));
            string[] skills = { "Writing", "Modeling", "Gameplay", "Programming", "Personality", "Fanbase" };
            resultText.text = skills[skillIdx] + " +1 (safe choice)";

            foreach (Button b in panel.GetComponentsInChildren<Button>())
                Destroy(b.gameObject);
            AddButton(panel.transform, "CONTINUE", 0, -100, () => { ShowDevelopment(); }, 200, 50);
        }, 220, 50);
    }

    // ========== DEVELOPMENT ==========
    void ShowDevelopment()
    {
        GameObject panel = CreatePanel("Development");
        AddText(panel.transform, "DEVELOPING: " + currentGameName, 150, 32, accentColor);
        AddText(panel.transform, "Genre: " + selectedGenre, 90, 22);
        AddText(panel.transform, "Dev Time: " + selectedDevTime + " year(s)", 50, 22);
        AddText(panel.transform, "Your dev is hard at work...", -20, 20, new Color(0.7f, 0.7f, 0.7f));

        AddButton(panel.transform, "FINISH DEVELOPMENT", 0, -120, () => { ShowResults(); }, 300, 55);
    }

    // ========== RESULTS ==========
    void ShowResults()
    {
        // Calculate
        pendingQuality = CalculateQuality();
        float trendMult = GetTrendMultiplier(selectedGenre);
        float personalityMult = personality >= STAT_CAP ? 2f : 1f + personality * 0.02f;
        float prevEarnings = 0f;
        foreach (var r in gameHistory) prevEarnings += r.profit * 0.5f;
        float baseProfit = pendingQuality * 50f;
        float total = baseProfit * trendMult * personalityMult + prevEarnings;
        if (selectedDevTime >= 2) total *= 1.25f;
        pendingProfit = Mathf.Max(100, Mathf.RoundToInt(total));

        GameObject panel = CreatePanel("Results");
        AddText(panel.transform, "GAME RESULTS", 300, 36, accentColor);
        AddText(panel.transform, currentGameName, 240, 28);
        AddText(panel.transform, "Genre: " + selectedGenre + "  |  Trend: " + new string('!', genreTrends.ContainsKey(selectedGenre) ? genreTrends[selectedGenre] : 3), 190, 20);
        AddText(panel.transform, "Trend Multiplier: x" + trendMult.ToString("F2"), 150, 20);
        AddText(panel.transform, "Game Quality: " + pendingQuality + " / 98", 110, 24, pendingQuality > 70 ? profitColor : Color.white);
        AddText(panel.transform, "Personality Bonus: x" + personalityMult.ToString("F2"), 70, 20);
        AddText(panel.transform, "Previous Work Bonus: +$" + Mathf.RoundToInt(prevEarnings), 30, 20);

        AddText(panel.transform, "PROFIT: $" + pendingProfit, -40, 42, profitColor);

        // Accolades
        List<string> accolades = GetAccolades();
        if (accolades.Count > 0)
            AddText(panel.transform, string.Join(" | ", accolades), -100, 18, accentColor);

        AddButton(panel.transform, "CONTINUE", 0, -200, () =>
        {
            currentYear += selectedDevTime;
            AdvanceTrends();
            money += pendingProfit;
            gameHistory.Add(new GameRecord
            {
                gameName = currentGameName,
                devTime = selectedDevTime,
                profit = pendingProfit,
                score = pendingQuality,
                genre = selectedGenre,
                subGenre = selectedSubGenre
            });
            ShowNextProject();
        }, 250, 55);
    }

    // ========== NEXT PROJECT ==========
    void ShowNextProject()
    {
        GameObject panel = CreatePanel("NextProject");

        AddText(panel.transform, "WHAT'S NEXT?", 200, 36, accentColor);
        AddText(panel.transform, "Year: " + currentYear + "  |  Money: $" + money, 140, 22);

        bool canAffordGame = money >= GAME_COST;
        bool canAffordSkills = money >= 1000;

        if (canAffordGame)
        {
            AddButton(panel.transform, "NEW GAME (-$" + GAME_COST + ")", 0, 40, () =>
            {
                money -= GAME_COST;
                ShowDevBlueprint();
            }, 320, 55);
        }

        if (canAffordSkills)
        {
            AddButton(panel.transform, "INVEST IN SKILLS", 0, -40, () =>
            {
                ShowInvestSkills();
            }, 320, 55);
        }

        AddButton(panel.transform, "RETIRE", 0, -120, () => { ShowRetirement(); }, 320, 55);

        if (!canAffordGame && !canAffordSkills)
        {
            AddText(panel.transform, "Not enough money for anything else...", -180, 20, Color.red);
        }
    }

    // ========== INVEST IN SKILLS ==========
    void ShowInvestSkills()
    {
        GameObject panel = CreatePanel("InvestSkills");

        AddText(panel.transform, "INVEST IN SKILLS", 300, 36, accentColor);
        AddText(panel.transform, "Money: $" + money, 250, 20);

        string[] skillNames = { "Writing", "Modeling", "Gameplay", "Programming", "Personality", "Fanbase" };
        int chosenSkill = 0;
        int chosenYears = 1;
        int chosenMoney = 1;

        TextMeshProUGUI skillLabel = AddText(panel.transform, "Skill: " + skillNames[0] + " (current: " + GetStatByIndex(0) + ")", 180, 22, accentColor);
        TextMeshProUGUI yearsLabel = AddText(panel.transform, "Time: 1 Year (x1.0)", 80, 22);
        TextMeshProUGUI moneyLabel = AddText(panel.transform, "Money: $1000", 20, 22);
        TextMeshProUGUI previewLabel = AddText(panel.transform, "Result: ~1 skill points", -40, 22, profitColor);

        // Skill selection buttons
        for (int i = 0; i < 6; i++)
        {
            int idx = i;
            AddButton(panel.transform, skillNames[i], -300 + i * 110, 130, () =>
            {
                chosenSkill = idx;
                skillLabel.text = "Skill: " + skillNames[idx] + " (current: " + GetStatByIndex(idx) + ")";
                UpdateInvestPreview();
            }, 100, 35);
        }

        // Year buttons
        for (int y = 1; y <= 3; y++)
        {
            int yr = y;
            AddButton(panel.transform, yr + "yr", -80 + (y - 1) * 80, 40, () =>
            {
                chosenYears = yr;
                float mult = 1f + 0.5f * (yr - 1);
                yearsLabel.text = "Time: " + yr + " Year(s) (x" + mult.ToString("F1") + ")";
                UpdateInvestPreview();
            }, 70, 35);
        }

        // Money buttons
        for (int m = 1; m <= 5; m++)
        {
            int mu = m;
            int cost = CalculateInvestCost(m);
            if (cost > money) break;
            AddButton(panel.transform, "$" + cost, -160 + (m - 1) * 90, -20, () =>
            {
                chosenMoney = mu;
                moneyLabel.text = "Money: $" + CalculateInvestCost(mu);
                UpdateInvestPreview();
            }, 80, 35);
        }

        void UpdateInvestPreview()
        {
            float timeMult = 1f + 0.5f * (chosenYears - 1);
            int points = Mathf.RoundToInt(chosenMoney * timeMult);
            previewLabel.text = "Result: ~" + points + " skill points";
        }

        AddButton(panel.transform, "INVEST", -80, -160, () =>
        {
            int cost = CalculateInvestCost(chosenMoney);
            if (money < cost) return;
            float timeMult = 1f + 0.5f * (chosenYears - 1);
            int points = Mathf.RoundToInt(chosenMoney * timeMult);
            int current = GetStatByIndex(chosenSkill);
            SetStatByIndex(chosenSkill, Mathf.Min(current + points, STAT_CAP));
            money -= cost;
            currentYear += chosenYears;
            AdvanceTrends();
            ShowNextProject();
        }, 150, 50);

        AddButton(panel.transform, "CANCEL", 80, -160, () => { ShowNextProject(); }, 150, 50);
    }

    int CalculateInvestCost(int units)
    {
        int total = 0;
        for (int i = 1; i <= units; i++)
        {
            if (i == 1) total += 1000;
            else if (i == 2) total += 1500;
            else total += 2000;
        }
        return total;
    }

    // ========== RETIREMENT ==========
    void ShowRetirement()
    {
        GameObject panel = CreatePanel("Retirement");

        int careerLength = currentYear - graduationYear;

        AddText(panel.transform, "RETIREMENT", 350, 42, accentColor);
        AddText(panel.transform, "Career Length: " + careerLength + " years", 280, 24);
        AddText(panel.transform, "Total Money: $" + money, 240, 24, profitColor);

        // Games list
        float y = 180;
        AddText(panel.transform, "--- Games Made ---", y, 22, accentColor);
        y -= 35;
        if (gameHistory.Count == 0)
        {
            AddText(panel.transform, "No games made.", y, 18);
        }
        else
        {
            foreach (var r in gameHistory)
            {
                AddText(panel.transform, r.gameName + " (" + r.genre + ") - " + r.score + "/98 - $" + r.profit, y, 18);
                y -= 30;
            }
        }

        // Accolades
        y -= 20;
        List<string> accolades = GetAccolades();
        AddText(panel.transform, "--- Accolades ---", y, 22, accentColor);
        y -= 35;
        if (accolades.Count > 0)
        {
            foreach (string a in accolades)
            {
                AddText(panel.transform, a, y, 20, a == "LEGEND" ? profitColor : Color.white);
                y -= 28;
            }
        }
        else
        {
            AddText(panel.transform, "None earned.", y, 18);
        }

        AddButton(panel.transform, "NEW RUN", -100, -350, () =>
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }, 180, 55);

        AddButton(panel.transform, "MAIN MENU", 100, -350, () =>
        {
            SceneManager.LoadScene(0);
        }, 180, 55);
    }

    // ========== GAME LOGIC ==========
    int CalculateQuality()
    {
        float score = programming * 2f;

        switch (selectedGenre)
        {
            case "Role-Playing": case "Narrative":
                score += writing * 3f + gameplay * 1f + modeling * 1f; break;
            case "Shooter": case "Action-Adventure": case "Fighting":
                score += modeling * 3f + gameplay * 2f + writing * 0.5f; break;
            case "Platformer": case "Roguelike/lite": case "Tower Defense":
                score += gameplay * 3f + modeling * 1f + writing * 0.5f; break;
            case "Puzzle": case "Strategy": case "Deck Builder":
                score += programming * 2f + gameplay * 2f + writing * 1f; break;
            default:
                score += gameplay * 2f + modeling * 1.5f + writing * 1f; break;
        }

        float normalized = Mathf.Clamp01((score - 5f) / 155f);
        int mc = Mathf.Clamp(Mathf.RoundToInt(normalized * 97f) + 1, 1, 98);
        mc += Random.Range(-10, 11);
        return Mathf.Clamp(mc, 1, 98);
    }

    float GetTrendMultiplier(string genre)
    {
        if (!genreTrends.ContainsKey(genre)) return 1f;
        return 0.25f + genreTrends[genre] * 0.25f;
    }

    void AdvanceTrends()
    {
        List<string> keys = new List<string>(genreTrends.Keys);
        foreach (string g in keys)
        {
            int shift = Random.Range(-1, 2);
            genreTrends[g] = Mathf.Clamp(genreTrends[g] + shift, 1, 6);
        }
    }

    List<string> GetAccolades()
    {
        List<string> a = new List<string>();
        if (programming >= STAT_CAP) a.Add("Bug-Free!");
        if (writing >= STAT_CAP) a.Add("Master Storyteller");
        if (modeling >= STAT_CAP) a.Add("Visual Virtuoso");
        if (gameplay >= STAT_CAP) a.Add("Fun Factory");
        if (personality >= STAT_CAP) a.Add("Fan Favorite");
        if (fanbase >= STAT_CAP) a.Add("Household Name");
        if (a.Count >= 6) a.Add("LEGEND");
        return a;
    }
}
