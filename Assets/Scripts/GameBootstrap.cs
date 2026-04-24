using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

/// <summary>
/// Self-contained game controller that creates ALL UI at runtime.
/// Updated with Danny's scope changes from the To-Do doc (April 2026).
/// </summary>
public class GameBootstrap : MonoBehaviour
{
    // ========== PLAYER DATA ==========
    const int STAT_CAP = 20;
    const int STARTING_POINTS = 10;
    const int STARTING_MONEY = 5000;
    const int GAME_COST = 2000;
    const int BASE_PROFIT = 500; // $500 guaranteed base profit per Danny's doc

    int graduationYear, currentYear, money;
    // 5 allocatable stats (fanbase removed, replaced by reputation)
    int writing = 1, modeling = 1, gameplay = 1, programming = 1, personality = 1;
    int reputation = 0; // hidden stat, no cap, increases by profit on game result
    int statPointsRemaining;
    int gameSeed;
    Dictionary<string, int> genreTrends = new Dictionary<string, int>();
    List<GameRecord> gameHistory = new List<GameRecord>();

    class GameRecord
    {
        public string gameName;
        public int devTime, profit, score;
        public string genre;
    }

    // Genre data: name, primary stat name, primary stat index
    static readonly string[] AllGenres = {
        "Shooter", "Action-Adventure", "Role-Playing", "Sports", "Simulation",
        "Racing", "Fighting", "Platformer", "Survival Craft", "Strategy",
        "Survival Horror", "Open World", "Roguelike/lite", "Deck Builder",
        "Party/Casual", "Idle", "Narrative", "Rhythm", "Tower Defense", "Puzzle"
    };

    // Primary stat index for each genre (0=Writing, 1=Modeling, 2=Gameplay, 3=Programming, 4=Personality)
    // Distributed evenly: 4 genres per stat
    static readonly int[] GenrePrimaryStat = {
        1, // Shooter -> Modeling
        2, // Action-Adventure -> Gameplay
        0, // Role-Playing -> Writing
        2, // Sports -> Gameplay
        3, // Simulation -> Programming
        1, // Racing -> Modeling
        2, // Fighting -> Gameplay
        2, // Platformer -> Gameplay
        3, // Survival Craft -> Programming
        3, // Strategy -> Programming
        0, // Survival Horror -> Writing
        1, // Open World -> Modeling
        3, // Roguelike/lite -> Programming
        0, // Deck Builder -> Writing
        4, // Party/Casual -> Personality
        4, // Idle -> Personality
        0, // Narrative -> Writing
        4, // Rhythm -> Personality
        1, // Tower Defense -> Modeling
        4, // Party/Casual -> Personality (Puzzle)
    };

    static readonly string[] StatNames = { "Writing", "Modeling", "Gameplay", "Programming", "Personality" };

    // ========== UI REFERENCES ==========
    Canvas canvas;
    GameObject currentPanel;
    // Persistent top bar
    GameObject topBar;
    TextMeshProUGUI yearText, moneyText;

    // Game state
    string currentGameName = "";
    int selectedDevTime = 1;
    string selectedGenre = "";
    int selectedGenreIndex = 0;
    int pendingProfit, pendingQuality;

    // ========== COLORS (Danny's spec: deep blue bg, lighter blue buttons) ==========
    Color bgColor = new Color(0.05f, 0.07f, 0.18f, 0.97f);
    Color btnColor = new Color(0.15f, 0.25f, 0.5f, 1f);
    Color btnHover = new Color(0.1f, 0.18f, 0.38f, 1f); // darken on hover per doc
    Color btnPressed = new Color(0.2f, 0.8f, 0.3f, 1f); // flash green on press per doc
    Color accentColor = new Color(0.4f, 0.7f, 1f, 1f);
    Color profitColor = new Color(0.3f, 1f, 0.3f, 1f);
    Color goldColor = new Color(1f, 0.85f, 0.3f, 1f);

    // ========== LIFECYCLE ==========
    void Start()
    {
        CreateCanvas();
        CreateTopBar();
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

    // Persistent top bar: year (top center) and money (below year)
    void CreateTopBar()
    {
        topBar = new GameObject("TopBar");
        topBar.transform.SetParent(canvas.transform, false);
        RectTransform rt = topBar.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.3f, 0.92f);
        rt.anchorMax = new Vector2(0.7f, 1f);
        rt.sizeDelta = Vector2.zero;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image bg = topBar.AddComponent<Image>();
        bg.color = new Color(0.03f, 0.05f, 0.12f, 0.9f);

        yearText = AddText(topBar.transform, "", 12, 26, accentColor);
        yearText.rectTransform.anchoredPosition = new Vector2(0, 12);
        moneyText = AddText(topBar.transform, "", -14, 22, profitColor);
        moneyText.rectTransform.anchoredPosition = new Vector2(0, -14);

        topBar.SetActive(false);
    }

    void UpdateTopBar()
    {
        if (currentYear > 0)
        {
            topBar.SetActive(true);
            yearText.text = "Year: " + currentYear;
            moneyText.text = "$ " + money;
        }
    }

    // ========== UI HELPERS ==========
    // Per doc: UI on left half of screen
    GameObject CreatePanel(string name)
    {
        if (currentPanel != null) Destroy(currentPanel);

        GameObject panel = new GameObject(name);
        panel.transform.SetParent(canvas.transform, false);

        RectTransform rt = panel.AddComponent<RectTransform>();
        // Left half of screen per Danny's doc
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(0.55f, 0.9f); // leave room for top bar
        rt.sizeDelta = Vector2.zero;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image img = panel.AddComponent<Image>();
        img.color = bgColor;

        currentPanel = panel;
        return panel;
    }

    // Fullscreen panel (for graduation + retirement which need full attention)
    GameObject CreateFullPanel(string name)
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
        rt.sizeDelta = new Vector2(900, fontSize + 16);
        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = color ?? Color.white;
        return tmp;
    }

    // Buttons: double-sized per doc, green flash on press, darken on hover
    Button AddButton(Transform parent, string label, float x, float y, System.Action onClick, float width = 320, float height = 65)
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
        colors.normalColor = btnColor;
        colors.highlightedColor = btnHover;
        colors.pressedColor = btnPressed; // green flash
        colors.selectedColor = btnColor;
        btn.colors = colors;
        btn.targetGraphic = img;

        // Label
        GameObject textGo = new GameObject("Label");
        textGo.transform.SetParent(go.transform, false);
        RectTransform textRt = textGo.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;
        TextMeshProUGUI tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 24;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        btn.onClick.AddListener(() => onClick());
        return btn;
    }

    // Smaller button variant for stat +/-, genre list, etc.
    Button AddSmallButton(Transform parent, string label, float x, float y, System.Action onClick, float width = 160, float height = 42)
    {
        Button btn = AddButton(parent, label, x, y, onClick, width, height);
        TextMeshProUGUI tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null) tmp.fontSize = 19;
        return btn;
    }

    TMP_InputField AddInputField(Transform parent, string placeholder, float y, float width = 350)
    {
        GameObject go = new GameObject("InputField");
        go.transform.SetParent(parent, false);
        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(0, y);
        rt.sizeDelta = new Vector2(width, 50);

        Image img = go.AddComponent<Image>();
        img.color = new Color(0.1f, 0.12f, 0.22f, 1f);

        TMP_InputField input = go.AddComponent<TMP_InputField>();

        GameObject textArea = new GameObject("TextArea");
        textArea.transform.SetParent(go.transform, false);
        RectTransform taRt = textArea.AddComponent<RectTransform>();
        taRt.anchorMin = Vector2.zero;
        taRt.anchorMax = Vector2.one;
        taRt.offsetMin = new Vector2(10, 0);
        taRt.offsetMax = new Vector2(-10, 0);
        textArea.AddComponent<RectMask2D>();

        GameObject textGo = new GameObject("Text");
        textGo.transform.SetParent(textArea.transform, false);
        RectTransform itRt = textGo.AddComponent<RectTransform>();
        itRt.anchorMin = Vector2.zero;
        itRt.anchorMax = Vector2.one;
        itRt.sizeDelta = Vector2.zero;
        TextMeshProUGUI itTmp = textGo.AddComponent<TextMeshProUGUI>();
        itTmp.fontSize = 24;
        itTmp.color = Color.white;

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
        phTmp.color = new Color(0.5f, 0.5f, 0.6f, 0.7f);

        input.textComponent = itTmp;
        input.textViewport = taRt;
        input.placeholder = phTmp;

        return input;
    }

    // ========== GRADUATION PANEL (fullscreen) ==========
    void ShowGraduationPanel()
    {
        GameObject panel = CreateFullPanel("GraduationPanel");

        AddText(panel.transform, "THE INDIE PROTEGE", 200, 48, accentColor);
        AddText(panel.transform, "An Indie Developer Simulator", 145, 20, new Color(0.6f, 0.6f, 0.7f));
        AddText(panel.transform, "What year did you graduate?", 60, 26);

        TMP_InputField yearInput = AddInputField(panel.transform, "Enter 4-digit year...", -10);
        yearInput.characterLimit = 4;
        yearInput.contentType = TMP_InputField.ContentType.IntegerNumber;

        TextMeshProUGUI errorText = AddText(panel.transform, "", -60, 20, Color.red);

        AddButton(panel.transform, "CONFIRM", 0, -140, () =>
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
            reputation = 0;
            gameSeed = year * 1000 + System.DateTime.Now.Millisecond;
            Random.InitState(gameSeed);

            genreTrends.Clear();
            foreach (string g in AllGenres)
                genreTrends[g] = Random.Range(2, 6);

            ShowStatSheet();
        });

        yearInput.ActivateInputField();
    }

    // ========== STAT SHEET (5 stats, no fanbase) ==========
    TextMeshProUGUI[] statValueTexts;
    TextMeshProUGUI pointsText;

    void ShowStatSheet()
    {
        GameObject panel = CreateFullPanel("StatSheet");

        AddText(panel.transform, "STAT ALLOCATION", 350, 38, accentColor);
        pointsText = AddText(panel.transform, "Points Remaining: " + statPointsRemaining, 295, 24);

        statValueTexts = new TextMeshProUGUI[5];

        for (int i = 0; i < 5; i++)
        {
            float y = 200 - i * 80;
            int idx = i;

            AddText(panel.transform, StatNames[i], y, 24).rectTransform.anchoredPosition = new Vector2(-160, y);
            AddSmallButton(panel.transform, "-", -30, y, () => { DecreaseStat(idx); }, 55, 50);
            statValueTexts[i] = AddText(panel.transform, "1", y, 30, accentColor);
            statValueTexts[i].rectTransform.anchoredPosition = new Vector2(50, y);
            statValueTexts[i].rectTransform.sizeDelta = new Vector2(60, 45);
            AddSmallButton(panel.transform, "+", 130, y, () => { IncreaseStat(idx); }, 55, 50);
        }

        AddButton(panel.transform, "CONFIRM STATS", 0, -280, () =>
        {
            UpdateTopBar();
            ShowDevBlueprint();
        });
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
        if (pointsText != null) pointsText.text = "Points Remaining: " + statPointsRemaining;
        for (int i = 0; i < 5; i++)
            if (statValueTexts != null && statValueTexts[i] != null)
                statValueTexts[i].text = GetStatByIndex(i).ToString();
    }

    // ========== DEV BLUEPRINT (left panel) ==========
    void ShowDevBlueprint()
    {
        UpdateTopBar();
        GameObject panel = CreatePanel("DevBlueprint");

        AddText(panel.transform, "GAME BLUEPRINT", 280, 34, accentColor);

        AddText(panel.transform, "Game Name:", 180, 22);
        TMP_InputField nameInput = AddInputField(panel.transform, "Enter game name...", 130);

        AddText(panel.transform, "Development Time:", 40, 22);
        selectedDevTime = 1;
        TextMeshProUGUI devTimeText = AddText(panel.transform, "1 Year", -10, 26, accentColor);

        AddButton(panel.transform, "1 Year", -100, -70, () =>
        {
            selectedDevTime = 1;
            devTimeText.text = "1 Year";
        }, 180, 60);

        AddButton(panel.transform, "2 Years", 100, -70, () =>
        {
            selectedDevTime = 2;
            devTimeText.text = "2 Years";
        }, 180, 60);

        AddButton(panel.transform, "START DEV", 0, -200, () =>
        {
            currentGameName = string.IsNullOrEmpty(nameInput.text) ? "Untitled Game" : nameInput.text;
            ShowGenreSelection();
        });
    }

    // ========== GENRE SELECTION (left panel, shows primary stat) ==========
    void ShowGenreSelection()
    {
        GameObject panel = CreatePanel("GenreSelection");

        AddText(panel.transform, "CHOOSE A GENRE", 420, 34, accentColor);
        AddText(panel.transform, "Primary stat shown in parentheses", 385, 16, new Color(0.6f, 0.6f, 0.7f));

        selectedGenre = AllGenres[0];
        selectedGenreIndex = 0;

        // Genre list with trends + primary stat visible
        float startY = 350;
        for (int i = 0; i < AllGenres.Length; i++)
        {
            int idx = i;
            string g = AllGenres[i];
            string trend = new string('!', genreTrends[g]);
            string primary = StatNames[GenrePrimaryStat[i]];
            float y = startY - i * 38;

            AddSmallButton(panel.transform, g + " " + trend + " (" + primary + ")", 0, y, () =>
            {
                selectedGenre = AllGenres[idx];
                selectedGenreIndex = idx;
            }, 480, 34);
        }

        AddButton(panel.transform, "CONFIRM GENRE", 0, -420, () =>
        {
            ShowRandomEvent();
        });
    }

    // ========== RANDOM EVENTS (simplified per doc: +2 or +3, guaranteed money loss) ==========
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
        AddText(panel.transform, "RANDOM EVENT", 280, 34, accentColor);
        AddText(panel.transform, evt, 180, 24);

        TextMeshProUGUI resultText = AddText(panel.transform, "", 40, 22, profitColor);

        AddButton(panel.transform, "YES (Risk / Reward)", 0, -40, () =>
        {
            // Per doc: random stat +2 or +3, guaranteed money loss, +3 costs $250 more
            int skillIdx = Random.Range(0, 5);
            int gain = Random.Range(0, 2) == 0 ? 2 : 3; // +2 or +3
            int baseCost = Random.Range(200, 450);
            int moneyCost = gain == 3 ? baseCost + 250 : baseCost; // +3 costs $250 more

            int current = GetStatByIndex(skillIdx);
            SetStatByIndex(skillIdx, Mathf.Min(current + gain, STAT_CAP));
            money -= moneyCost;

            resultText.text = StatNames[skillIdx] + " +" + gain + "  |  Money -$" + moneyCost;

            foreach (Button b in panel.GetComponentsInChildren<Button>())
                Destroy(b.gameObject);
            AddButton(panel.transform, "CONTINUE", 0, -80, () => { ShowDevelopment(); });
        });

        AddButton(panel.transform, "NO (Safe +1, Free)", 0, -120, () =>
        {
            // Safe: +1 random stat, no cost
            int skillIdx = Random.Range(0, 5);
            int current = GetStatByIndex(skillIdx);
            SetStatByIndex(skillIdx, Mathf.Min(current + 1, STAT_CAP));
            resultText.text = StatNames[skillIdx] + " +1 (safe choice, no cost)";

            foreach (Button b in panel.GetComponentsInChildren<Button>())
                Destroy(b.gameObject);
            AddButton(panel.transform, "CONTINUE", 0, -80, () => { ShowDevelopment(); });
        });
    }

    // ========== DEVELOPMENT ==========
    void ShowDevelopment()
    {
        GameObject panel = CreatePanel("Development");
        AddText(panel.transform, "DEVELOPING:", 200, 28);
        AddText(panel.transform, currentGameName, 150, 34, accentColor);
        AddText(panel.transform, "Genre: " + selectedGenre, 80, 22);
        AddText(panel.transform, "Dev Time: " + selectedDevTime + " year(s)", 40, 22);
        AddText(panel.transform, "Your dev is hard at work...", -40, 20, new Color(0.5f, 0.5f, 0.6f));

        AddButton(panel.transform, "FINISH DEVELOPMENT", 0, -150, () => { ShowResults(); });
    }

    // ========== RESULTS (with reputation, $500 base profit) ==========
    void ShowResults()
    {
        pendingQuality = CalculateQuality();
        float trendMult = GetTrendMultiplier(selectedGenre);
        float personalityMult = personality >= STAT_CAP ? 2f : 1f + personality * 0.02f;
        float prevEarnings = 0f;
        foreach (var r in gameHistory) prevEarnings += r.profit * 0.5f;
        float baseProfit = pendingQuality * 50f;
        float total = baseProfit * trendMult * personalityMult + prevEarnings;
        if (selectedDevTime >= 2) total *= 1.25f;
        // $500 guaranteed minimum per doc
        pendingProfit = Mathf.Max(BASE_PROFIT, Mathf.RoundToInt(total));

        GameObject panel = CreatePanel("Results");
        AddText(panel.transform, "GAME RESULTS", 380, 34, accentColor);
        AddText(panel.transform, currentGameName, 330, 28);

        string trend = genreTrends.ContainsKey(selectedGenre) ? new string('!', genreTrends[selectedGenre]) : "???";
        AddText(panel.transform, "Genre: " + selectedGenre + "  |  Trend: " + trend, 280, 18);
        AddText(panel.transform, "Primary Stat: " + StatNames[GenrePrimaryStat[selectedGenreIndex]], 250, 18, accentColor);
        AddText(panel.transform, "Trend Multiplier: x" + trendMult.ToString("F2"), 210, 18);
        AddText(panel.transform, "Game Quality: " + pendingQuality + " / 98", 170, 26, pendingQuality > 70 ? profitColor : Color.white);
        AddText(panel.transform, "Personality Bonus: x" + personalityMult.ToString("F2"), 130, 18);
        AddText(panel.transform, "Previous Work Bonus: +$" + Mathf.RoundToInt(prevEarnings), 100, 18);

        AddText(panel.transform, "PROFIT: $" + pendingProfit, 30, 44, profitColor);

        AddButton(panel.transform, "CONTINUE", 0, -100, () =>
        {
            currentYear += selectedDevTime;
            AdvanceTrends();
            money += pendingProfit;
            reputation += pendingProfit; // reputation = cumulative profit per doc
            gameHistory.Add(new GameRecord
            {
                gameName = currentGameName,
                devTime = selectedDevTime,
                profit = pendingProfit,
                score = pendingQuality,
                genre = selectedGenre
            });
            UpdateTopBar();
            ShowNextProject();
        });
    }

    // ========== NEXT PROJECT ==========
    void ShowNextProject()
    {
        UpdateTopBar();
        GameObject panel = CreatePanel("NextProject");

        AddText(panel.transform, "WHAT'S NEXT?", 250, 36, accentColor);

        bool canAffordGame = money >= GAME_COST;
        bool canAffordSkills = money >= 1000;

        if (canAffordGame)
        {
            AddButton(panel.transform, "NEW GAME (-$" + GAME_COST + ")", 0, 80, () =>
            {
                money -= GAME_COST;
                UpdateTopBar();
                ShowDevBlueprint();
            });
        }

        if (canAffordSkills)
        {
            AddButton(panel.transform, "INVEST IN SKILLS", 0, -10, () =>
            {
                ShowInvestSkills();
            });
        }

        AddButton(panel.transform, "RETIRE", 0, -100, () => { ShowRetirement(); });

        if (!canAffordGame && !canAffordSkills)
        {
            AddText(panel.transform, "Not enough money for anything else...", -200, 20, Color.red);
        }
    }

    // ========== INVEST IN SKILLS ==========
    void ShowInvestSkills()
    {
        GameObject panel = CreatePanel("InvestSkills");

        AddText(panel.transform, "INVEST IN SKILLS", 350, 34, accentColor);

        int chosenSkill = 0;
        int chosenYears = 1;
        int chosenMoney = 1;

        TextMeshProUGUI skillLabel = AddText(panel.transform, StatNames[0] + " (current: " + GetStatByIndex(0) + ")", 280, 24, accentColor);
        TextMeshProUGUI yearsLabel = AddText(panel.transform, "Time: 1 Year (x1.0)", 140, 22);
        TextMeshProUGUI moneyLabel = AddText(panel.transform, "Cost: $1000", 60, 22);
        TextMeshProUGUI previewLabel = AddText(panel.transform, "Result: ~1 skill points", 0, 24, profitColor);

        // Skill selection
        for (int i = 0; i < 5; i++)
        {
            int idx = i;
            AddSmallButton(panel.transform, StatNames[i], -240 + i * 120, 220, () =>
            {
                chosenSkill = idx;
                skillLabel.text = StatNames[idx] + " (current: " + GetStatByIndex(idx) + ")";
                UpdatePreview();
            }, 110, 40);
        }

        // Years
        for (int y = 1; y <= 3; y++)
        {
            int yr = y;
            AddSmallButton(panel.transform, yr + " yr", -100 + (y - 1) * 100, 90, () =>
            {
                chosenYears = yr;
                float mult = 1f + 0.5f * (yr - 1);
                yearsLabel.text = "Time: " + yr + " Year(s) (x" + mult.ToString("F1") + ")";
                UpdatePreview();
            }, 90, 40);
        }

        // Money
        for (int m = 1; m <= 5; m++)
        {
            int mu = m;
            int cost = CalculateInvestCost(m);
            if (cost > money) break;
            AddSmallButton(panel.transform, "$" + cost, -200 + (m - 1) * 100, 10, () =>
            {
                chosenMoney = mu;
                moneyLabel.text = "Cost: $" + CalculateInvestCost(mu);
                UpdatePreview();
            }, 90, 40);
        }

        void UpdatePreview()
        {
            float timeMult = 1f + 0.5f * (chosenYears - 1);
            int points = Mathf.RoundToInt(chosenMoney * timeMult);
            previewLabel.text = "Result: ~" + points + " skill points";
        }

        AddButton(panel.transform, "INVEST", -100, -120, () =>
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
            UpdateTopBar();
            ShowNextProject();
        }, 200, 65);

        AddButton(panel.transform, "CANCEL", 100, -120, () => { ShowNextProject(); }, 200, 65);
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

    // ========== RETIREMENT (fullscreen, with ranking + reputation) ==========
    void ShowRetirement()
    {
        GameObject panel = CreateFullPanel("Retirement");
        topBar.SetActive(false);

        int careerLength = currentYear - graduationYear;

        AddText(panel.transform, "RETIREMENT", 420, 44, accentColor);
        AddText(panel.transform, "Career Length: " + careerLength + " years", 365, 24);
        AddText(panel.transform, "Total Money: $" + money, 330, 26, profitColor);

        // Games list
        float y = 270;
        AddText(panel.transform, "--- Games Made ---", y, 22, accentColor);
        y -= 32;
        if (gameHistory.Count == 0)
        {
            AddText(panel.transform, "No games made.", y, 18);
            y -= 28;
        }
        else
        {
            foreach (var r in gameHistory)
            {
                AddText(panel.transform, r.gameName + " (" + r.genre + ") - " + r.score + "/98 - $" + r.profit, y, 17);
                y -= 28;
            }
        }

        // Reputation per doc
        y -= 15;
        AddText(panel.transform, "Reputation: " + reputation, y, 26, goldColor);
        y -= 40;

        // Ranking system per doc (F to S tier based on distance from max stats)
        int totalStats = writing + modeling + gameplay + programming + personality;
        int maxTotal = STAT_CAP * 5; // 100
        int distance = maxTotal - totalStats;

        string rank;
        Color rankColor;
        if (distance == 0) { rank = "S"; rankColor = goldColor; }
        else if (distance <= 5) { rank = "A"; rankColor = profitColor; }
        else if (distance <= 10) { rank = "B"; rankColor = accentColor; }
        else if (distance <= 20) { rank = "C"; rankColor = Color.white; }
        else if (distance <= 35) { rank = "D"; rankColor = new Color(1f, 0.6f, 0.3f); }
        else { rank = "F"; rankColor = Color.red; }

        AddText(panel.transform, "RANK", y, 22, accentColor);
        y -= 50;
        // Wax stamp style: big letter
        AddText(panel.transform, rank, y, 72, rankColor);
        y -= 60;
        AddText(panel.transform, "Stats: " + totalStats + " / " + maxTotal + " (" + distance + " from max)", y, 16);

        AddButton(panel.transform, "NEW RUN", -120, -420, () =>
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }, 220, 65);

        AddButton(panel.transform, "MAIN MENU", 120, -420, () =>
        {
            SceneManager.LoadScene(0);
        }, 220, 65);
    }

    // ========== GAME LOGIC ==========
    // Reworked: each genre has a primary stat with much higher weight
    int CalculateQuality()
    {
        float score = 0f;

        // All stats contribute a base amount
        score += writing * 0.5f + modeling * 0.5f + gameplay * 0.5f + programming * 0.5f + personality * 0.3f;

        // Primary stat has MUCH higher weight (x4)
        int primaryIdx = GenrePrimaryStat[selectedGenreIndex];
        score += GetStatByIndex(primaryIdx) * 4f;

        // Programming always matters some
        score += programming * 1f;

        // Normalize to 1-98
        // Max possible: 20*0.5*5 + 20*0.3 + 20*4 + 20*1 = 50+6+80+20 = 156
        // Min possible: 1*0.5*5 + 1*0.3 + 1*4 + 1*1 = 2.5+0.3+4+1 = 7.8
        float normalized = Mathf.Clamp01((score - 7f) / 149f);
        int mc = Mathf.Clamp(Mathf.RoundToInt(normalized * 97f) + 1, 1, 98);
        mc += Random.Range(-8, 9);
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
}
