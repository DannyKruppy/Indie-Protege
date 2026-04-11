using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class UIBuilder : EditorWindow
{
    [MenuItem("Tools/Build All Game UI")]
    public static void BuildAllUI()
    {
        // Find the Canvas
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("No Canvas found in scene!");
            return;
        }

        // Find Gameplay Manager
        Gameplay gameplay = Object.FindFirstObjectByType<Gameplay>();
        if (gameplay == null)
        {
            Debug.LogError("No Gameplay component found!");
            return;
        }

        PlayerInfo playerInfo = Object.FindFirstObjectByType<PlayerInfo>();

        // Build each panel's UI
        BuildStatSheetUI(gameplay);
        BuildDevBlueprintUI(gameplay);
        BuildGenreSelectionUI(gameplay);
        BuildRandomEventUI(gameplay, canvas.transform);
        BuildResultsScreenUI(gameplay);
        BuildInvestInSkillsUI(gameplay);
        BuildTopBarUI(gameplay);
        BuildRetirementUI(gameplay);
        BuildAudioManager();

        // Wire graduation UI (existing elements)
        WireGraduationUI(gameplay);

        // Mark scene dirty so it saves
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("=== ALL UI BUILT AND WIRED SUCCESSFULLY ===");
    }

    // =========================================
    // HELPERS
    // =========================================
    static TextMeshProUGUI CreateTMPText(Transform parent, string name, string defaultText, Vector2 anchoredPos, Vector2 size)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        TextMeshProUGUI tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = defaultText;
        tmp.fontSize = 24;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        return tmp;
    }

    static Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPos, Vector2 size)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        Image img = go.AddComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        Button btn = go.AddComponent<Button>();
        ColorBlock colors = btn.colors;
        colors.highlightedColor = new Color(0.35f, 0.35f, 0.35f, 1f);
        colors.pressedColor = new Color(0.15f, 0.15f, 0.15f, 1f);
        btn.colors = colors;

        // Button label
        GameObject textGo = new GameObject("Text");
        textGo.transform.SetParent(go.transform, false);

        RectTransform textRt = textGo.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;

        TextMeshProUGUI tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 20;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        return btn;
    }

    static Slider CreateSlider(Transform parent, string name, Vector2 anchoredPos, Vector2 size, float min, float max)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        Slider slider = go.AddComponent<Slider>();
        slider.minValue = min;
        slider.maxValue = max;
        slider.wholeNumbers = true;
        slider.value = min;

        // Background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(go.transform, false);
        RectTransform bgRt = bg.AddComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.sizeDelta = Vector2.zero;
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.15f, 0.15f, 0.15f, 1f);

        // Fill area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(go.transform, false);
        RectTransform fillAreaRt = fillArea.AddComponent<RectTransform>();
        fillAreaRt.anchorMin = Vector2.zero;
        fillAreaRt.anchorMax = Vector2.one;
        fillAreaRt.sizeDelta = new Vector2(-20, 0);

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        RectTransform fillRt = fill.AddComponent<RectTransform>();
        fillRt.sizeDelta = new Vector2(10, 0);
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0.4f, 0.7f, 1f, 1f);

        slider.fillRect = fillRt;

        // Handle area
        GameObject handleArea = new GameObject("Handle Slide Area");
        handleArea.transform.SetParent(go.transform, false);
        RectTransform handleAreaRt = handleArea.AddComponent<RectTransform>();
        handleAreaRt.anchorMin = Vector2.zero;
        handleAreaRt.anchorMax = Vector2.one;
        handleAreaRt.sizeDelta = new Vector2(-20, 0);

        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(handleArea.transform, false);
        RectTransform handleRt = handle.AddComponent<RectTransform>();
        handleRt.sizeDelta = new Vector2(20, 0);
        Image handleImg = handle.AddComponent<Image>();
        handleImg.color = Color.white;

        slider.handleRect = handleRt;

        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        return slider;
    }

    static TMP_Dropdown CreateDropdown(Transform parent, string name, Vector2 anchoredPos, Vector2 size)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);

        RectTransform rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        Image img = go.AddComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.2f, 1f);

        TMP_Dropdown dropdown = go.AddComponent<TMP_Dropdown>();

        // Label
        GameObject label = new GameObject("Label");
        label.transform.SetParent(go.transform, false);
        RectTransform labelRt = label.AddComponent<RectTransform>();
        labelRt.anchorMin = Vector2.zero;
        labelRt.anchorMax = Vector2.one;
        labelRt.sizeDelta = new Vector2(-20, 0);
        TextMeshProUGUI labelTmp = label.AddComponent<TextMeshProUGUI>();
        labelTmp.text = "Select...";
        labelTmp.fontSize = 18;
        labelTmp.alignment = TextAlignmentOptions.Center;
        labelTmp.color = Color.white;
        dropdown.captionText = labelTmp;

        // Template (required for dropdown to work)
        GameObject template = new GameObject("Template");
        template.transform.SetParent(go.transform, false);
        RectTransform templateRt = template.AddComponent<RectTransform>();
        templateRt.anchoredPosition = new Vector2(0, -size.y);
        templateRt.sizeDelta = new Vector2(0, 150);
        templateRt.pivot = new Vector2(0.5f, 1f);
        Image templateImg = template.AddComponent<Image>();
        templateImg.color = new Color(0.15f, 0.15f, 0.15f, 1f);
        ScrollRect scroll = template.AddComponent<ScrollRect>();

        // Viewport
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(template.transform, false);
        RectTransform viewportRt = viewport.AddComponent<RectTransform>();
        viewportRt.anchorMin = Vector2.zero;
        viewportRt.anchorMax = Vector2.one;
        viewportRt.sizeDelta = Vector2.zero;
        viewport.AddComponent<Image>().color = Color.clear;
        viewport.AddComponent<Mask>().showMaskGraphic = false;

        // Content
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        RectTransform contentRt = content.AddComponent<RectTransform>();
        contentRt.anchorMin = new Vector2(0, 1);
        contentRt.anchorMax = Vector2.one;
        contentRt.pivot = new Vector2(0.5f, 1f);
        contentRt.sizeDelta = new Vector2(0, 28);

        scroll.viewport = viewportRt;
        scroll.content = contentRt;

        // Item
        GameObject item = new GameObject("Item");
        item.transform.SetParent(content.transform, false);
        RectTransform itemRt = item.AddComponent<RectTransform>();
        itemRt.anchorMin = new Vector2(0, 0.5f);
        itemRt.anchorMax = new Vector2(1, 0.5f);
        itemRt.sizeDelta = new Vector2(0, 28);
        Toggle toggle = item.AddComponent<Toggle>();

        // Item label
        GameObject itemLabel = new GameObject("Item Label");
        itemLabel.transform.SetParent(item.transform, false);
        RectTransform itemLabelRt = itemLabel.AddComponent<RectTransform>();
        itemLabelRt.anchorMin = Vector2.zero;
        itemLabelRt.anchorMax = Vector2.one;
        itemLabelRt.sizeDelta = Vector2.zero;
        TextMeshProUGUI itemTmp = itemLabel.AddComponent<TextMeshProUGUI>();
        itemTmp.text = "Option";
        itemTmp.fontSize = 18;
        itemTmp.alignment = TextAlignmentOptions.Center;
        itemTmp.color = Color.white;

        dropdown.itemText = itemTmp;
        dropdown.template = templateRt;
        toggle.isOn = true;

        template.SetActive(false);

        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        return dropdown;
    }

    // =========================================
    // WIRE GRADUATION (existing elements)
    // =========================================
    static void WireGraduationUI(Gameplay gameplay)
    {
        // These might already be wired, but let's ensure
        Transform gradPanel = gameplay.graduationPanel.transform;

        if (gameplay.yearInput == null)
        {
            TMP_InputField yearInput = gradPanel.GetComponentInChildren<TMP_InputField>();
            if (yearInput != null)
            {
                gameplay.yearInput = yearInput;
                Debug.Log("Wired: Year Input");
            }
        }

        if (gameplay.errorText == null)
        {
            Transform errorT = gradPanel.Find("ErrorText");
            if (errorT != null)
            {
                gameplay.errorText = errorT.GetComponent<TextMeshProUGUI>();
                Debug.Log("Wired: Error Text");
            }
        }

        // Wire ConfirmButton OnClick -> ConfirmGraduationYear
        Transform confirmBtn = gradPanel.Find("ConfirmButton");
        if (confirmBtn != null)
        {
            Button btn = confirmBtn.GetComponent<Button>();
            if (btn != null)
            {
                UnityEditor.Events.UnityEventTools.AddPersistentListener(
                    btn.onClick,
                    new UnityEngine.Events.UnityAction(gameplay.ConfirmGraduationYear));
                Debug.Log("Wired: ConfirmButton -> ConfirmGraduationYear");
            }
        }
    }

    // =========================================
    // STAT SHEET
    // =========================================
    static void BuildStatSheetUI(Gameplay gameplay)
    {
        Transform parent = gameplay.playerStatScreen.transform;

        // Clear existing children
        for (int i = parent.childCount - 1; i >= 0; i--)
            Undo.DestroyObjectImmediate(parent.GetChild(i).gameObject);

        // Title
        CreateTMPText(parent, "Title", "STAT ALLOCATION", new Vector2(0, 250), new Vector2(500, 50));

        // Points remaining
        TextMeshProUGUI pointsText = CreateTMPText(parent, "PointsRemainingText", "Points Remaining: 10", new Vector2(0, 200), new Vector2(400, 40));
        gameplay.pointsRemainingText = pointsText;

        // Stat rows
        string[] statNames = { "Writing", "Modeling", "Gameplay", "Programming", "Personality", "Fanbase" };
        TextMeshProUGUI[] valueTexts = new TextMeshProUGUI[6];
        string[] increaseMethodNames = { "IncreaseWriting", "IncreaseModeling", "IncreaseGameplay", "IncreaseProgramming", "IncreasePersonality", "IncreaseFanbase" };
        string[] decreaseMethodNames = { "DecreaseWriting", "DecreaseModeling", "DecreaseGameplay", "DecreaseProgramming", "DecreasePersonality", "DecreaseFanbase" };

        for (int i = 0; i < statNames.Length; i++)
        {
            float y = 140 - (i * 60);

            // Stat label
            CreateTMPText(parent, statNames[i] + "Label", statNames[i], new Vector2(-150, y), new Vector2(200, 40));

            // Decrease button
            Button decBtn = CreateButton(parent, "Decrease" + statNames[i], "-", new Vector2(-10, y), new Vector2(40, 40));
            UnityEditor.Events.UnityEventTools.AddPersistentListener(
                decBtn.onClick,
                System.Delegate.CreateDelegate(typeof(UnityEngine.Events.UnityAction), gameplay, decreaseMethodNames[i]) as UnityEngine.Events.UnityAction);

            // Value text
            valueTexts[i] = CreateTMPText(parent, statNames[i] + "ValueText", "1", new Vector2(50, y), new Vector2(60, 40));

            // Increase button
            Button incBtn = CreateButton(parent, "Increase" + statNames[i], "+", new Vector2(110, y), new Vector2(40, 40));
            UnityEditor.Events.UnityEventTools.AddPersistentListener(
                incBtn.onClick,
                System.Delegate.CreateDelegate(typeof(UnityEngine.Events.UnityAction), gameplay, increaseMethodNames[i]) as UnityEngine.Events.UnityAction);
        }

        // Wire value texts
        gameplay.writingValueText = valueTexts[0];
        gameplay.modelingValueText = valueTexts[1];
        gameplay.gameplayValueText = valueTexts[2];
        gameplay.programmingValueText = valueTexts[3];
        gameplay.personalityValueText = valueTexts[4];
        gameplay.fanbaseValueText = valueTexts[5];

        // Confirm button
        Button confirmBtn = CreateButton(parent, "ConfirmStatsButton", "CONFIRM STATS", new Vector2(0, -250), new Vector2(250, 50));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            confirmBtn.onClick,
            new UnityEngine.Events.UnityAction(gameplay.ConfirmPlayerStats));

        Debug.Log("Built: Stat Sheet UI");
    }

    // =========================================
    // DEV BLUEPRINT
    // =========================================
    static void BuildDevBlueprintUI(Gameplay gameplay)
    {
        Transform parent = gameplay.devBlueprint.transform;

        for (int i = parent.childCount - 1; i >= 0; i--)
            Undo.DestroyObjectImmediate(parent.GetChild(i).gameObject);

        CreateTMPText(parent, "Title", "GAME BLUEPRINT", new Vector2(0, 200), new Vector2(500, 50));

        // Game name label + input
        CreateTMPText(parent, "NameLabel", "Game Name:", new Vector2(-100, 100), new Vector2(200, 40));

        GameObject inputGo = new GameObject("GameNameInput");
        inputGo.transform.SetParent(parent, false);
        RectTransform inputRt = inputGo.AddComponent<RectTransform>();
        inputRt.anchoredPosition = new Vector2(100, 100);
        inputRt.sizeDelta = new Vector2(250, 40);
        Image inputImg = inputGo.AddComponent<Image>();
        inputImg.color = new Color(0.15f, 0.15f, 0.15f, 1f);
        TMP_InputField inputField = inputGo.AddComponent<TMP_InputField>();

        // Text area for input
        GameObject textArea = new GameObject("Text Area");
        textArea.transform.SetParent(inputGo.transform, false);
        RectTransform taRt = textArea.AddComponent<RectTransform>();
        taRt.anchorMin = Vector2.zero;
        taRt.anchorMax = Vector2.one;
        taRt.sizeDelta = new Vector2(-10, 0);

        GameObject inputText = new GameObject("Text");
        inputText.transform.SetParent(textArea.transform, false);
        RectTransform itRt = inputText.AddComponent<RectTransform>();
        itRt.anchorMin = Vector2.zero;
        itRt.anchorMax = Vector2.one;
        itRt.sizeDelta = Vector2.zero;
        TextMeshProUGUI itTmp = inputText.AddComponent<TextMeshProUGUI>();
        itTmp.fontSize = 20;
        itTmp.color = Color.white;
        inputField.textComponent = itTmp;
        inputField.textViewport = taRt;

        Undo.RegisterCreatedObjectUndo(inputGo, "Create GameNameInput");
        gameplay.gameNameInput = inputField;

        // Dev time toggles
        CreateTMPText(parent, "DevTimeLabel", "Development Time:", new Vector2(0, 30), new Vector2(300, 40));

        // Toggle group
        GameObject toggleGroup = new GameObject("DevTimeToggleGroup");
        toggleGroup.transform.SetParent(parent, false);
        ToggleGroup tg = toggleGroup.AddComponent<ToggleGroup>();
        Undo.RegisterCreatedObjectUndo(toggleGroup, "Create ToggleGroup");

        // 1 Year Toggle
        GameObject oneYearGo = new GameObject("OneYearToggle");
        oneYearGo.transform.SetParent(parent, false);
        RectTransform oyRt = oneYearGo.AddComponent<RectTransform>();
        oyRt.anchoredPosition = new Vector2(-80, -30);
        oyRt.sizeDelta = new Vector2(150, 40);
        Image oyImg = oneYearGo.AddComponent<Image>();
        oyImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        Toggle oneYearToggle = oneYearGo.AddComponent<Toggle>();
        oneYearToggle.isOn = true;
        oneYearToggle.group = tg;

        GameObject oyLabel = new GameObject("Label");
        oyLabel.transform.SetParent(oneYearGo.transform, false);
        RectTransform oyLabelRt = oyLabel.AddComponent<RectTransform>();
        oyLabelRt.anchorMin = Vector2.zero;
        oyLabelRt.anchorMax = Vector2.one;
        oyLabelRt.sizeDelta = Vector2.zero;
        TextMeshProUGUI oyTmp = oyLabel.AddComponent<TextMeshProUGUI>();
        oyTmp.text = "1 Year";
        oyTmp.fontSize = 20;
        oyTmp.alignment = TextAlignmentOptions.Center;
        oyTmp.color = Color.white;

        // Checkmark
        GameObject oyCheck = new GameObject("Checkmark");
        oyCheck.transform.SetParent(oneYearGo.transform, false);
        RectTransform oyCheckRt = oyCheck.AddComponent<RectTransform>();
        oyCheckRt.anchoredPosition = new Vector2(-55, 0);
        oyCheckRt.sizeDelta = new Vector2(20, 20);
        Image oyCheckImg = oyCheck.AddComponent<Image>();
        oyCheckImg.color = new Color(0.4f, 0.7f, 1f, 1f);
        oneYearToggle.graphic = oyCheckImg;

        Undo.RegisterCreatedObjectUndo(oneYearGo, "Create OneYearToggle");
        gameplay.oneYearToggle = oneYearToggle;

        // 2 Year Toggle
        GameObject twoYearGo = new GameObject("TwoYearToggle");
        twoYearGo.transform.SetParent(parent, false);
        RectTransform tyRt = twoYearGo.AddComponent<RectTransform>();
        tyRt.anchoredPosition = new Vector2(80, -30);
        tyRt.sizeDelta = new Vector2(150, 40);
        Image tyImg = twoYearGo.AddComponent<Image>();
        tyImg.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        Toggle twoYearToggle = twoYearGo.AddComponent<Toggle>();
        twoYearToggle.isOn = false;
        twoYearToggle.group = tg;

        GameObject tyLabel = new GameObject("Label");
        tyLabel.transform.SetParent(twoYearGo.transform, false);
        RectTransform tyLabelRt = tyLabel.AddComponent<RectTransform>();
        tyLabelRt.anchorMin = Vector2.zero;
        tyLabelRt.anchorMax = Vector2.one;
        tyLabelRt.sizeDelta = Vector2.zero;
        TextMeshProUGUI tyTmp = tyLabel.AddComponent<TextMeshProUGUI>();
        tyTmp.text = "2 Years";
        tyTmp.fontSize = 20;
        tyTmp.alignment = TextAlignmentOptions.Center;
        tyTmp.color = Color.white;

        GameObject tyCheck = new GameObject("Checkmark");
        tyCheck.transform.SetParent(twoYearGo.transform, false);
        RectTransform tyCheckRt = tyCheck.AddComponent<RectTransform>();
        tyCheckRt.anchoredPosition = new Vector2(-55, 0);
        tyCheckRt.sizeDelta = new Vector2(20, 20);
        Image tyCheckImg = tyCheck.AddComponent<Image>();
        tyCheckImg.color = new Color(0.4f, 0.7f, 1f, 1f);
        twoYearToggle.graphic = tyCheckImg;

        Undo.RegisterCreatedObjectUndo(twoYearGo, "Create TwoYearToggle");
        gameplay.twoYearToggle = twoYearToggle;

        // Confirm button
        Button confirmBtn = CreateButton(parent, "ConfirmBlueprintButton", "START DEVELOPMENT", new Vector2(0, -150), new Vector2(280, 50));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            confirmBtn.onClick,
            new UnityEngine.Events.UnityAction(gameplay.ConfirmDevBlueprint));

        Debug.Log("Built: Dev Blueprint UI");
    }

    // =========================================
    // GENRE SELECTION
    // =========================================
    static void BuildGenreSelectionUI(Gameplay gameplay)
    {
        Transform parent = gameplay.genreSelection.transform;

        for (int i = parent.childCount - 1; i >= 0; i--)
            Undo.DestroyObjectImmediate(parent.GetChild(i).gameObject);

        CreateTMPText(parent, "Title", "GENRE SELECTION", new Vector2(0, 250), new Vector2(500, 50));

        // Main genre dropdown
        CreateTMPText(parent, "GenreLabel", "Main Genre:", new Vector2(-150, 180), new Vector2(200, 35));
        TMP_Dropdown genreDd = CreateDropdown(parent, "GenreDropdown", new Vector2(80, 180), new Vector2(250, 35));
        gameplay.genreDropdown = genreDd;

        // Sub genre dropdown
        CreateTMPText(parent, "SubGenreLabel", "Sub-Genre:", new Vector2(-150, 120), new Vector2(200, 35));
        TMP_Dropdown subGenreDd = CreateDropdown(parent, "SubGenreDropdown", new Vector2(80, 120), new Vector2(250, 35));
        gameplay.subGenreDropdown = subGenreDd;

        // Trend texts
        gameplay.genreTrendText = CreateTMPText(parent, "GenreTrendText", "", new Vector2(0, 60), new Vector2(400, 35));
        gameplay.subGenreTrendText = CreateTMPText(parent, "SubGenreTrendText", "", new Vector2(0, 20), new Vector2(400, 35));

        // All trends overview (scrollable area would be ideal but text works for prototype)
        TextMeshProUGUI allTrends = CreateTMPText(parent, "AllTrendsText", "--- Industry Trends ---", new Vector2(0, -100), new Vector2(400, 200));
        allTrends.fontSize = 14;
        allTrends.alignment = TextAlignmentOptions.TopLeft;
        gameplay.allTrendsText = allTrends;

        // Confirm button
        Button confirmBtn = CreateButton(parent, "ConfirmGenreButton", "CONFIRM GENRE", new Vector2(0, -250), new Vector2(250, 50));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            confirmBtn.onClick,
            new UnityEngine.Events.UnityAction(gameplay.ConfirmGenre));

        Debug.Log("Built: Genre Selection UI");
    }

    // =========================================
    // RANDOM EVENT
    // =========================================
    static void BuildRandomEventUI(Gameplay gameplay, Transform canvasTransform)
    {
        // Create new panel under Canvas
        GameObject panelGo = new GameObject("randomEventPanel");
        panelGo.transform.SetParent(canvasTransform, false);

        RectTransform panelRt = panelGo.AddComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.one;
        panelRt.sizeDelta = Vector2.zero;

        Image panelImg = panelGo.AddComponent<Image>();
        panelImg.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);

        Undo.RegisterCreatedObjectUndo(panelGo, "Create randomEventPanel");
        gameplay.randomEventPanel = panelGo;

        Transform parent = panelGo.transform;

        CreateTMPText(parent, "EventTitle", "RANDOM EVENT", new Vector2(0, 200), new Vector2(400, 50));

        // Description
        TextMeshProUGUI descText = CreateTMPText(parent, "EventDescriptionText", "Event description here...", new Vector2(0, 80), new Vector2(500, 100));
        descText.fontSize = 22;
        gameplay.eventDescriptionText = descText;

        // Result text
        TextMeshProUGUI resultText = CreateTMPText(parent, "EventResultText", "", new Vector2(0, -30), new Vector2(500, 80));
        resultText.fontSize = 20;
        resultText.color = new Color(0.4f, 1f, 0.4f, 1f);
        gameplay.eventResultText = resultText;

        // Yes button
        Button yesBtn = CreateButton(parent, "EventYesButton", "YES", new Vector2(-100, -130), new Vector2(150, 50));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            yesBtn.onClick,
            new UnityEngine.Events.UnityAction(gameplay.EventChoiceYes));
        gameplay.eventYesButton = yesBtn;

        // No button
        Button noBtn = CreateButton(parent, "EventNoButton", "NO", new Vector2(100, -130), new Vector2(150, 50));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            noBtn.onClick,
            new UnityEngine.Events.UnityAction(gameplay.EventChoiceNo));
        gameplay.eventNoButton = noBtn;

        // Continue button (hidden initially)
        Button contBtn = CreateButton(parent, "EventContinueButton", "CONTINUE", new Vector2(0, -130), new Vector2(200, 50));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            contBtn.onClick,
            new UnityEngine.Events.UnityAction(gameplay.EventContinue));
        gameplay.eventContinueButton = contBtn;
        contBtn.gameObject.SetActive(false);

        // Start inactive
        panelGo.SetActive(false);

        Debug.Log("Built: Random Event UI");
    }

    // =========================================
    // RESULTS SCREEN
    // =========================================
    static void BuildResultsScreenUI(Gameplay gameplay)
    {
        Transform parent = gameplay.resultsScreen.transform;

        for (int i = parent.childCount - 1; i >= 0; i--)
            Undo.DestroyObjectImmediate(parent.GetChild(i).gameObject);

        CreateTMPText(parent, "Title", "GAME RESULTS", new Vector2(0, 270), new Vector2(500, 50));

        gameplay.resultGameNameText = CreateTMPText(parent, "ResultGameNameText", "", new Vector2(0, 210), new Vector2(500, 40));
        gameplay.resultGenreText = CreateTMPText(parent, "ResultGenreText", "", new Vector2(0, 165), new Vector2(500, 35));
        gameplay.resultTrendText = CreateTMPText(parent, "ResultTrendText", "", new Vector2(0, 125), new Vector2(500, 35));
        gameplay.resultQualityText = CreateTMPText(parent, "ResultQualityText", "", new Vector2(0, 85), new Vector2(500, 35));
        gameplay.resultPersonalityText = CreateTMPText(parent, "ResultPersonalityText", "", new Vector2(0, 45), new Vector2(500, 35));
        gameplay.resultPreviousWorkText = CreateTMPText(parent, "ResultPreviousWorkText", "", new Vector2(0, 5), new Vector2(500, 35));

        TextMeshProUGUI profitText = CreateTMPText(parent, "ResultProfitText", "", new Vector2(0, -55), new Vector2(500, 50));
        profitText.fontSize = 36;
        gameplay.resultProfitText = profitText;

        gameplay.resultAccoladesText = CreateTMPText(parent, "ResultAccoladesText", "", new Vector2(0, -110), new Vector2(500, 35));

        // Continue button
        Button continueBtn = CreateButton(parent, "ProceedResultsButton", "CONTINUE", new Vector2(0, -200), new Vector2(250, 50));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            continueBtn.onClick,
            new UnityEngine.Events.UnityAction(gameplay.ProceedResults));

        Debug.Log("Built: Results Screen UI");
    }

    // =========================================
    // INVEST IN SKILLS
    // =========================================
    static void BuildInvestInSkillsUI(Gameplay gameplay)
    {
        Transform parent = gameplay.investInSkills.transform;

        for (int i = parent.childCount - 1; i >= 0; i--)
            Undo.DestroyObjectImmediate(parent.GetChild(i).gameObject);

        CreateTMPText(parent, "Title", "INVEST IN SKILLS", new Vector2(0, 230), new Vector2(500, 50));

        // Skill dropdown
        CreateTMPText(parent, "SkillLabel", "Skill to train:", new Vector2(-120, 160), new Vector2(200, 35));
        TMP_Dropdown skillDd = CreateDropdown(parent, "InvestSkillDropdown", new Vector2(100, 160), new Vector2(220, 35));
        gameplay.investSkillDropdown = skillDd;

        // Time slider
        CreateTMPText(parent, "TimeSliderLabel", "Time:", new Vector2(-180, 90), new Vector2(100, 35));
        Slider timeSlider = CreateSlider(parent, "InvestTimeSlider", new Vector2(30, 90), new Vector2(250, 25), 1, 3);
        gameplay.investTimeSlider = timeSlider;
        gameplay.investTimeLabel = CreateTMPText(parent, "InvestTimeLabel", "1 Year (x1.0)", new Vector2(230, 90), new Vector2(200, 35));

        // Money slider
        CreateTMPText(parent, "MoneySliderLabel", "Money:", new Vector2(-180, 30), new Vector2(100, 35));
        Slider moneySlider = CreateSlider(parent, "InvestMoneySlider", new Vector2(30, 30), new Vector2(250, 25), 1, 5);
        gameplay.investMoneySlider = moneySlider;
        gameplay.investMoneyLabel = CreateTMPText(parent, "InvestMoneyLabel", "$1000", new Vector2(230, 30), new Vector2(200, 35));

        // Preview
        gameplay.investPreviewText = CreateTMPText(parent, "InvestPreviewText", "~1 skill points", new Vector2(0, -40), new Vector2(400, 35));
        gameplay.investCostText = CreateTMPText(parent, "InvestCostText", "Cost: $1000 + 1 year", new Vector2(0, -80), new Vector2(400, 35));

        // Confirm + Cancel buttons
        Button confirmBtn = CreateButton(parent, "ConfirmInvestButton", "INVEST", new Vector2(-80, -160), new Vector2(150, 50));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            confirmBtn.onClick,
            new UnityEngine.Events.UnityAction(gameplay.ConfirmInvestment));

        Button cancelBtn = CreateButton(parent, "CancelInvestButton", "CANCEL", new Vector2(80, -160), new Vector2(150, 50));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            cancelBtn.onClick,
            new UnityEngine.Events.UnityAction(gameplay.FinishInvesting));

        Debug.Log("Built: Invest in Skills UI");
    }

    // =========================================
    // TOP BAR
    // =========================================
    static void BuildTopBarUI(Gameplay gameplay)
    {
        // Find topBar in hierarchy
        Transform canvasTransform = gameplay.graduationPanel.transform.parent;
        Transform topBar = canvasTransform.Find("topBar");

        if (topBar == null)
        {
            // Create topBar
            GameObject topBarGo = new GameObject("topBar");
            topBarGo.transform.SetParent(canvasTransform, false);
            RectTransform rt = topBarGo.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.sizeDelta = new Vector2(0, 40);
            rt.anchoredPosition = Vector2.zero;

            Image img = topBarGo.AddComponent<Image>();
            img.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);

            Undo.RegisterCreatedObjectUndo(topBarGo, "Create topBar");
            topBar = topBarGo.transform;
        }

        // Year display might already exist
        TextMeshProUGUI yearDisplay = null;
        Transform yearT = topBar.Find("Year Display Text");
        if (yearT == null)
            yearT = topBar.Find("YearDisplayText");
        if (yearT != null)
            yearDisplay = yearT.GetComponent<TextMeshProUGUI>();

        if (yearDisplay == null)
        {
            yearDisplay = CreateTMPText(topBar, "YearDisplayText", "Year: ---", new Vector2(-200, 0), new Vector2(200, 35));
        }
        gameplay.yearDisplay = yearDisplay;

        // Money display
        TextMeshProUGUI moneyDisplay = CreateTMPText(topBar, "MoneyDisplayText", "$0", new Vector2(200, 0), new Vector2(200, 35));
        gameplay.moneyDisplay = moneyDisplay;

        Debug.Log("Built: Top Bar UI");
    }

    // =========================================
    // RETIREMENT
    // =========================================
    static void BuildRetirementUI(Gameplay gameplay)
    {
        Transform parent = gameplay.retirement.transform;

        for (int i = parent.childCount - 1; i >= 0; i--)
            Undo.DestroyObjectImmediate(parent.GetChild(i).gameObject);

        CreateTMPText(parent, "Title", "RETIREMENT", new Vector2(0, 270), new Vector2(500, 50));

        gameplay.careerLengthText = CreateTMPText(parent, "CareerLengthText", "Career Length: 0 years", new Vector2(0, 200), new Vector2(500, 40));
        gameplay.moneyText = CreateTMPText(parent, "MoneyText", "Money Earned: $0", new Vector2(0, 155), new Vector2(500, 40));

        TextMeshProUGUI gamesList = CreateTMPText(parent, "RetirementGamesListText", "--- Games Made ---", new Vector2(0, 30), new Vector2(500, 200));
        gamesList.fontSize = 18;
        gamesList.alignment = TextAlignmentOptions.TopLeft;
        gameplay.retirementGamesListText = gamesList;

        TextMeshProUGUI accolades = CreateTMPText(parent, "RetirementAccoladesText", "", new Vector2(0, -120), new Vector2(500, 80));
        accolades.fontSize = 20;
        gameplay.retirementAccoladesText = accolades;

        // Buttons
        Button newRunBtn = CreateButton(parent, "StartNewRunButton", "NEW RUN", new Vector2(-100, -230), new Vector2(170, 50));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            newRunBtn.onClick,
            new UnityEngine.Events.UnityAction(gameplay.StartNewRun));

        Button exitBtn = CreateButton(parent, "ExitGameButton", "QUIT", new Vector2(100, -230), new Vector2(170, 50));
        UnityEditor.Events.UnityEventTools.AddPersistentListener(
            exitBtn.onClick,
            new UnityEngine.Events.UnityAction(gameplay.ExitGame));

        Debug.Log("Built: Retirement UI");
    }

    // =========================================
    // AUDIO MANAGER
    // =========================================
    static void BuildAudioManager()
    {
        // Check if one already exists
        AudioManager existing = Object.FindFirstObjectByType<AudioManager>();
        if (existing != null)
        {
            Debug.Log("AudioManager already exists, skipping.");
            return;
        }

        GameObject go = new GameObject("AudioManager");
        go.AddComponent<AudioManager>();
        Undo.RegisterCreatedObjectUndo(go, "Create AudioManager");
        Debug.Log("Built: AudioManager GameObject");
    }
}
