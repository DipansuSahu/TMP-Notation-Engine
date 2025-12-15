using AbS;
using TMPro;
using UnityEngine;

/// <summary>
/// Demo script showcasing all TMPNotationEngine features
/// Attach this to a GameObject with TMP_Text components as children
/// </summary>
public class TMPNotationEngineFullExample : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text headerText;

    [SerializeField] private TMP_Text mathematicsText;
    [SerializeField] private TMP_Text chemistryText;
    [SerializeField] private TMP_Text physicsText;
    [SerializeField] private TMP_Text customNotationText;
    [SerializeField] private TMP_Text unicodeText;
    [SerializeField] private TMP_Text fractionsText;
    [SerializeField] private TMP_Text extensionsText;
    [SerializeField] private TMP_Text configExampleText;
    [SerializeField] private TMP_Text utilityText;

    [Header("Settings")]
    [SerializeField] private bool autoRefresh = true;

    [SerializeField] private float refreshInterval = 2f;

    private float timer;
    private int exampleIndex = 0;

    private void Start()
    {
        DisplayAllExamples();
    }

    private void Update()
    {
        if (autoRefresh)
        {
            timer += Time.deltaTime;
            if (timer >= refreshInterval)
            {
                timer = 0f;
                CycleExamples();
            }
        }
    }

    /// <summary>
    /// Display all formatting examples at once
    /// </summary>
    public void DisplayAllExamples()
    {
        ShowHeader();
        ShowMathematicsExamples();
        ShowChemistryExamples();
        ShowPhysicsExamples();
        ShowCustomNotationExamples();
        ShowUnicodeExamples();
        ShowFractionExamples();
        ShowExtensionMethodsExamples();
        ShowCustomConfigExamples();
        ShowUtilityMethodsExamples();
    }

    #region Example Sections

    private void ShowHeader()
    {
        if (headerText == null) return;
        headerText.text = "TMPNotationEngine - All Features Demo".FormatForTMP();
    }

    private void ShowMathematicsExamples()
    {
        if (mathematicsText == null) return;

        string examples = @"<b>Mathematics Examples:</b>

1. Quadratic: x^2 + 2x + 1 = 0
2. Cubic: y^3 - 5y^2 + 3y - 7 = 0
3. Exponents: 2^8 = 256, 10^-3 = 0.001
4. Variables: a^n + b^n = c^n
5. Complex: (x+1)^2 = x^2 + 2x + 1
6. Indices: x_1, x_2, x_n
7. Matrix: a_(i,j) where i=1,2,3";

        mathematicsText.text = TMPNotationEngine.Format(examples);
    }

    private void ShowChemistryExamples()
    {
        if (chemistryText == null) return;

        string examples = @"<b>Chemistry Examples:</b>

1. Water: H2O
2. Carbon Dioxide: CO2
3. Sulfuric Acid: H2SO4
4. Glucose: C6H12O6
5. Calcium Hydroxide: Ca(OH)2
6. Methane: CH4
7. Ammonia: NH3
8. Reaction: 2H2 + O2 → 2H2O";

        chemistryText.text = TMPNotationEngine.Format(examples);
    }

    private void ShowPhysicsExamples()
    {
        if (physicsText == null) return;

        string examples = @"<b>Physics Examples:</b>

1. Energy: E = mc^2
2. Force: F = ma
3. Gravity: F = G(m1*m2)/r^2
4. Velocity: v = v_0 + at
5. Kinetic Energy: KE = (1/2)mv^2
6. Power: P = E/t
7. Acceleration: a = Δv/Δt
8. Momentum: p_x, p_y, p_z";

        physicsText.text = TMPNotationEngine.Format(examples);
    }

    private void ShowCustomNotationExamples()
    {
        if (customNotationText == null) return;

        string examples = @"<b>Custom Notation Support:</b>

1. Braces: x^{n+1}, e^{2x}
2. Parentheses: x^(a+b), y_(i+j)
3. Nested: (a^2 + b^2)^(1/2)
4. Multiple: x^2_i + y^3_j
5. Complex subscript: a_{n,m}
6. Powers of e: e^{-x^2/2}";

        customNotationText.text = TMPNotationEngine.Format(examples);
    }

    private void ShowUnicodeExamples()
    {
        if (unicodeText == null) return;

        string examples = @"<b>Unicode Conversion:</b>

1. Superscripts: x² + y³ = z⁴
2. Subscripts: H₂O + CO₂
3. Mixed: x₁² + x₂² = r²
4. Operators: x⁺ y⁻ z⁼
5. Letters: xⁿ + yⁱ
6. Auto-convert: ⁰¹²³⁴⁵⁶⁷⁸⁹
7. Sub-convert: ₀₁₂₃₄₅₆₇₈₉";

        unicodeText.text = TMPNotationEngine.Format(examples);
    }

    private void ShowFractionExamples()
    {
        if (fractionsText == null) return;

        string examples = @"<b>Fraction Formatting:</b>

1. Simple: 1/2, 3/4, 5/8
2. Variables: a/b, x/y, m/n
3. Complex: (a+b)/(c+d)
4. Expressions: (x^2+1)/(x-1)
5. Numbers: 22/7 ≈ π
6. Multiple: 1/2 + 1/3 = 5/6";

        fractionsText.text = TMPNotationEngine.Format(examples);
    }

    private void ShowExtensionMethodsExamples()
    {
        if (extensionsText == null) return;

        string line1 = "Manual Super: " + "2".ToSuperscript() + " and " + "3".ToSuperscript();
        string line2 = "Manual Sub: x" + "1".ToSubscript() + " + x" + "2".ToSubscript();
        string line3 = "Fraction: " + "a+b".ToFraction("c+d");
        string line4 = "Auto Format: " + "E = mc^2".FormatForTMP();
        string line5 = "Chemistry: " + "H2O + CO2".FormatForTMP();

        extensionsText.text = $@"<b>Extension Methods:</b>

{line1}
{line2}
{line3}
{line4}
{line5}";
    }

    private void ShowCustomConfigExamples()
    {
        if (configExampleText == null) return;

        // Example 1: Default config
        string default1 = TMPNotationEngine.Format("x^2 + H2O");

        // Example 2: Custom sizes
        var config = new TMPNotationEngine.FormatConfig
        {
            SuperscriptSize = 50f,
            SubscriptSize = 55f,
            EnableFractions = false
        };
        string custom1 = TMPNotationEngine.Format("x^2 + H2O", config);

        // Example 3: Disable certain features
        var config2 = new TMPNotationEngine.FormatConfig
        {
            EnableChemicalFormulas = false,
            EnableCaretNotation = true
        };
        string custom2 = TMPNotationEngine.Format("x^2 + H2O", config2);

        configExampleText.text = $@"<b>Custom Configuration:</b>

Default (60%): {default1}
Small (50%): {custom1}
No Chemistry: {custom2}";
    }

    private void ShowUtilityMethodsExamples()
    {
        if (utilityText == null) return;

        string formatted = "x^2 + H2O".FormatForTMP();
        string stripped = formatted.StripTMPTags();
        bool hasFormat = formatted.HasTMPFormatting();

        utilityText.text = $@"<b>Utility Methods:</b>

Formatted: {formatted}
Stripped: {stripped}
Has Formatting: {hasFormat}

Check: '{"plain"}' = {("plain".HasTMPFormatting())}
Check: '{formatted}' = {formatted.HasTMPFormatting()}";
    }

    #endregion Example Sections

    #region Dynamic Examples

    /// <summary>
    /// Cycle through different example sets
    /// </summary>
    private void CycleExamples()
    {
        exampleIndex = (exampleIndex + 1) % 5;

        switch (exampleIndex)
        {
            case 0:
                ShowDynamicMathExamples();
                break;

            case 1:
                ShowDynamicChemistryExamples();
                break;

            case 2:
                ShowDynamicPhysicsExamples();
                break;

            case 3:
                ShowDynamicComparisonExamples();
                break;

            case 4:
                ShowDynamicComplexExamples();
                break;
        }
    }

    private void ShowDynamicMathExamples()
    {
        if (mathematicsText == null) return;

        string[] equations = new[]
        {
            "Pythagorean: a^2 + b^2 = c^2",
            "Binomial: (a+b)^2 = a^2 + 2ab + b^2",
            "Factorial: n! = n(n-1)(n-2)...1",
            "Logarithm: log_{10}(100) = 2",
            "Sum: Σ_{i=1}^{n} i = n(n+1)/2"
        };

        mathematicsText.text = "<b>Math (Dynamic):</b>\n\n" +
            TMPNotationEngine.Format(equations[Random.Range(0, equations.Length)]);
    }

    private void ShowDynamicChemistryExamples()
    {
        if (chemistryText == null) return;

        string[] reactions = new[]
        {
            "Combustion: CH4 + 2O2 → CO2 + 2H2O",
            "Photosynthesis: 6CO2 + 6H2O → C6H12O6 + 6O2",
            "Neutralization: HCl + NaOH → NaCl + H2O",
            "Rust: 4Fe + 3O2 → 2Fe2O3"
        };

        chemistryText.text = "<b>Chemistry (Dynamic):</b>\n\n" +
            TMPNotationEngine.Format(reactions[Random.Range(0, reactions.Length)]);
    }

    private void ShowDynamicPhysicsExamples()
    {
        if (physicsText == null) return;

        string[] formulas = new[]
        {
            "Einstein: E = mc^2",
            "Newton's 2nd: F = ma",
            "Universal Gravitation: F = G(m_1*m_2)/r^2",
            "Kinematic: s = ut + (1/2)at^2",
            "Wave: v = fλ"
        };

        physicsText.text = "<b>Physics (Dynamic):</b>\n\n" +
            TMPNotationEngine.Format(formulas[Random.Range(0, formulas.Length)]);
    }

    private void ShowDynamicComparisonExamples()
    {
        if (customNotationText == null) return;

        string unformatted = "Before: x^2 + H2O + a_1";
        string formatted = TMPNotationEngine.Format("After: x^2 + H2O + a_1");

        customNotationText.text = $@"<b>Before/After Comparison:</b>

{unformatted}
{formatted}";
    }

    private void ShowDynamicComplexExamples()
    {
        if (unicodeText == null) return;

        string complex = @"Complex Mixed:
E = mc^2
F = G(m_1*m_2)/r^2
PV = nRT
a^2 + b^2 = c^2
H2SO4 + 2NaOH → Na2SO4 + 2H2O";

        unicodeText.text = "<b>Combined:</b>\n\n" + TMPNotationEngine.Format(complex);
    }

    #endregion Dynamic Examples

    #region Public Methods (Call from Inspector/Buttons)

    public void RefreshAllExamples()
    {
        DisplayAllExamples();
        Debug.Log("All examples refreshed!");
    }

    public void ToggleAutoRefresh()
    {
        autoRefresh = !autoRefresh;
        Debug.Log($"Auto refresh: {autoRefresh}");
    }

    public void TestCustomInput(string input)
    {
        if (utilityText != null)
        {
            utilityText.text = $"<b>Your Input:</b>\n\n{TMPNotationEngine.Format(input)}";
        }
    }

    #endregion Public Methods (Call from Inspector/Buttons)

    #region Editor Helper

#if UNITY_EDITOR

    [ContextMenu("Auto-Assign TMP Components")]
    private void AutoAssignComponents()
    {
        TMP_Text[] allTexts = GetComponentsInChildren<TMP_Text>(true);

        if (allTexts.Length >= 10)
        {
            headerText = allTexts[0];
            mathematicsText = allTexts[1];
            chemistryText = allTexts[2];
            physicsText = allTexts[3];
            customNotationText = allTexts[4];
            unicodeText = allTexts[5];
            fractionsText = allTexts[6];
            extensionsText = allTexts[7];
            configExampleText = allTexts[8];
            utilityText = allTexts[9];

            Debug.Log("Auto-assigned " + allTexts.Length + " TMP_Text components!");
        }
        else
        {
            Debug.LogWarning($"Found only {allTexts.Length} TMP_Text components. Need at least 10.");
        }
    }

#endif

    #endregion Editor Helper
}

/*
===========================================
SETUP INSTRUCTIONS:
===========================================

1. CREATE UI HIERARCHY:
   - Canvas
     └─ TMPFormatterDemo (attach this script)
        ├─ Header (TMP_Text)
        ├─ Mathematics (TMP_Text)
        ├─ Chemistry (TMP_Text)
        ├─ Physics (TMP_Text)
        ├─ CustomNotation (TMP_Text)
        ├─ Unicode (TMP_Text)
        ├─ Fractions (TMP_Text)
        ├─ Extensions (TMP_Text)
        ├─ ConfigExample (TMP_Text)
        └─ Utility (TMP_Text)

2. AUTO-ASSIGN (EASY WAY):
   - Right-click script in Inspector
   - Select "Auto-Assign TMP Components"

3. MANUAL SETUP:
   - Drag each TMP_Text to corresponding field

4. OPTIONAL - ADD BUTTONS:
   - Refresh All Examples → RefreshAllExamples()
   - Toggle Auto Refresh → ToggleAutoRefresh()
   - Test Input Field → TestCustomInput(string)

5. RECOMMENDED SETTINGS:
   - Use ScrollView for long content
   - Set TMP_Text to AutoSize or set appropriate size
   - Use monospace font for better alignment
   - Enable Rich Text on all TMP_Text components

===========================================
FEATURES:
===========================================
✓ Displays all formatting examples
✓ Auto-refresh with cycling examples
✓ Before/After comparisons
✓ Extension method demonstrations
✓ Custom configuration examples
✓ Utility method showcases
✓ Dynamic random examples
✓ Test your own input

===========================================
*/