using AbS;
using TMPro;
using UnityEngine;

/// <summary>
/// Demo script to showcase MathStringFormatter SDK usage with TextMeshPro
/// </summary>
public class TMPNotationEngineSimpleExample : MonoBehaviour
{
    [SerializeField]
    string dataText = "E = mc^2", dataText2 = "H2O + CO2 = H2CO3", dataText3 = "(a+b)/(c+d) + x_1",
                            dataText4 = "x⁰ + x¹ + x² + x³ + x⁴ + x⁵ + x⁶ + x⁷ + x⁸ + x⁹ + x¹⁰",
                            dataText5 = "A₀, A₁, A₂, A₃, A₄, A₅, A₆, A₇, A₈, A₉, A₁₀";

    [Header("TMP Text Components")]
    [SerializeField] private TMP_Text equationText;
    [SerializeField] TMP_Text equationText2, equationText3, equationText4, equationText5;

    private void Start()
    {
        // Example 1: Auto-format math expression
        equationText.text = dataText.ToFormatTMP();

        // Example 2: Chemistry + math together
        equationText2.text = dataText2.ToFormatTMP();

        // Example 3: Fractions and subscripts
        equationText3.text = dataText3.ToFormatTMP();

        // Example 4 & 5: Customizing superscript and subscript sizes
        TMPNotationEngine.FormatConfig formatConfig = new TMPNotationEngine.FormatConfig();
        formatConfig.SuperscriptSize = 100; // Set superscript size to 100% of normal text size
        formatConfig.SubscriptSize = 100;   // Set subscript size to 100% of normal text size

        // Example 4: Superscripts
        equationText4.text = dataText4.ToFormatTMP(formatConfig);

        // Example 5: Subscripts
        equationText5.text = dataText5.ToFormatTMP(formatConfig);
    }
}