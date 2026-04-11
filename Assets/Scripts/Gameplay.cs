using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class Gameplay : MonoBehaviour
{
    [Header("References")]
    public PlayerInfo playerInfo;

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

    [Header("Graduation UI")]
    public TMP_InputField yearInput;
    public TextMeshProUGUI errorText;

    [Header("Dev Blueprint UI")]
    public TMP_InputField gameNameInput;
    public Toggle oneYearToggle;
    public Toggle twoYearToggle;

    [Header("Top Bar")]
    public TextMeshProUGUI yearDisplay;

    [Header("Retirement UI")]
    public TextMeshProUGUI careerLengthText;
    public TextMeshProUGUI moneyText;

    private int selectedDevTime = 1;
    private string currentGameName = "";

    void Start()
    {
        Time.timeScale = 0f;

        errorText.text = "";

        // Autofocus input field
        yearInput.ActivateInputField();

        Debug.Log("Game started, waiting for graduation input.");
    }

    void Update()
    {
        // Always show current year
        if (playerInfo.currentYear > 0)
        {
            yearDisplay.text = "Year: " + playerInfo.currentYear;
        }
    }

    // -----------------------------
    // GRADUATION PANEL
    // -----------------------------
    public void ConfirmGraduationYear()
    {
        string input = yearInput.text;

        // Validate: must be 4 digits
        if (input.Length != 4 || !int.TryParse(input, out int year))
        {
            errorText.text = "Enter a valid 4-digit year.";
            Debug.Log("Invalid graduation input.");
            return;
        }

        playerInfo.graduationYear = year;
        playerInfo.currentYear = year;

        Debug.Log("Graduation year set: " + year);

        graduationPanel.SetActive(false);
        playerStatScreen.SetActive(true);
    }

    // -----------------------------
    // PLAYER STAT SCREEN
    // -----------------------------
    public void ConfirmPlayerStats()
    {
        playerStatScreen.SetActive(false);
        devBlueprint.SetActive(true);

        Debug.Log("Moved to Dev Blueprint.");
    }

    // -----------------------------
    // DEV BLUEPRINT
    // -----------------------------
    public void ConfirmDevBlueprint()
    {
        currentGameName = gameNameInput.text;

        selectedDevTime = oneYearToggle.isOn ? 1 : 2;

        Debug.Log("Game Name: " + currentGameName);
        Debug.Log("Dev Time: " + selectedDevTime);

        devBlueprint.SetActive(false);
        genreSelection.SetActive(true);
    }

    // -----------------------------
    // GENRE SELECTION
    // -----------------------------
    public void ConfirmGenre()
    {
        genreSelection.SetActive(false);
        gameDevelopment.SetActive(true);

        Time.timeScale = 1f; // Resume gameplay here

        Debug.Log("Genre selected. Gameplay resumed.");
    }

    // -----------------------------
    // GAME DEVELOPMENT
    // -----------------------------
    public void SkipToEndCycle()
    {
        gameDevelopment.SetActive(false);
        resultsScreen.SetActive(true);

        Debug.Log("Skipped to results.");
    }

    // -----------------------------
    // RESULTS SCREEN
    // -----------------------------
    public void ProceedResults()
    {
        // Advance year
        playerInfo.currentYear += selectedDevTime;

        // Add profit
        int profit = 1000;
        playerInfo.money += profit;

        // Save game record
        playerInfo.AddGame(currentGameName, selectedDevTime, profit);

        Debug.Log("Results applied. Year: " + playerInfo.currentYear + " Money: " + playerInfo.money);

        resultsScreen.SetActive(false);
        nextProject.SetActive(true);
    }

    // -----------------------------
    // NEXT PROJECT
    // -----------------------------
    public void InvestSkills()
    {
        nextProject.SetActive(false);
        investInSkills.SetActive(true);
    }

    public void NewGame()
    {
        nextProject.SetActive(false);
        devBlueprint.SetActive(true);
    }

    public void Retire()
    {
        nextProject.SetActive(false);
        retirement.SetActive(true);

        int careerLength = playerInfo.currentYear - playerInfo.graduationYear;

        careerLengthText.text = "Career Length: " + careerLength + " years";
        moneyText.text = "Money Earned: $" + playerInfo.money;

        Debug.Log("Player retired.");
    }

    // -----------------------------
    // INVEST IN SKILLS
    // -----------------------------
    public void FinishInvesting()
    {
        investInSkills.SetActive(false);
        nextProject.SetActive(true);
    }

    // -----------------------------
    // RETIREMENT PANEL
    // -----------------------------
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