using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace AbS
{
    /// <summary>
    /// Advanced TextMeshPro formatter with support for superscripts, subscripts, 
    /// fractions, and custom formatting rules.
    /// </summary>
    public static class TMPNotationEngine
    {
        #region Configuration

        public class FormatConfig
        {
            public float SuperscriptSize { get; set; } = 60f;
            public float SubscriptSize { get; set; } = 60f;
            public float FractionSize { get; set; } = 70f;
            public bool EnableCaretNotation { get; set; } = true;
            public bool EnableUnicodeConversion { get; set; } = true;
            public bool EnableChemicalFormulas { get; set; } = true;
            public bool EnableFractions { get; set; } = true;
            public bool EnableUnderscoreSubscript { get; set; } = true;

            public static FormatConfig Default => new FormatConfig();
        }

        #endregion

        #region Unicode Mappings

        private static readonly Dictionary<char, string> SuperscriptMap = new Dictionary<char, string>
        {
            {'⁰', "0"}, {'¹', "1"}, {'²', "2"}, {'³', "3"}, {'⁴', "4"},
            {'⁵', "5"}, {'⁶', "6"}, {'⁷', "7"}, {'⁸', "8"}, {'⁹', "9"},
            {'⁺', "+"}, {'⁻', "-"}, {'⁼', "="}, {'⁽', "("}, {'⁾', ")"},
            {'ⁿ', "n"}, {'ⁱ', "i"}
        };

        private static readonly Dictionary<char, string> SubscriptMap = new Dictionary<char, string>
        {
            {'₀', "0"}, {'₁', "1"}, {'₂', "2"}, {'₃', "3"}, {'₄', "4"},
            {'₅', "5"}, {'₆', "6"}, {'₇', "7"}, {'₈', "8"}, {'₉', "9"},
            {'₊', "+"}, {'₋', "-"}, {'₌', "="}, {'₍', "("}, {'₎', ")"},
            {'ₐ', "a"}, {'ₑ', "e"}, {'ₒ', "o"}, {'ₓ', "x"}, {'ₕ', "h"},
            {'ₖ', "k"}, {'ₗ', "l"}, {'ₘ', "m"}, {'ₙ', "n"}, {'ₚ', "p"},
            {'ₛ', "s"}, {'ₜ', "t"}
        };

        #endregion

        #region Public API

        /// <summary>
        /// Format text with default configuration
        /// </summary>
        public static string Format(string input)
        {
            return Format(input, FormatConfig.Default);
        }

        /// <summary>
        /// Format text with custom configuration
        /// </summary>
        public static string Format(string input, FormatConfig config)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            StringBuilder output = new StringBuilder(input);

            // Apply formatters in order of priority
            if (config.EnableUnicodeConversion)
                ConvertUnicodeCharacters(output, config);

            if (config.EnableCaretNotation)
                ConvertCaretNotation(output, config);

            if (config.EnableUnderscoreSubscript)
                ConvertUnderscoreSubscript(output, config);

            if (config.EnableFractions)
                ConvertFractions(output, config);

            if (config.EnableChemicalFormulas)
                ConvertChemicalFormulas(output, config);

            return output.ToString();
        }

        /// <summary>
        /// Create superscript tag
        /// </summary>
        public static string Superscript(string text, float size = 60f)
        {
            return $"<sup><size={size}%>{text}</size></sup>";
        }

        /// <summary>
        /// Create subscript tag
        /// </summary>
        public static string Subscript(string text, float size = 60f)
        {
            return $"<sub><size={size}%>{text}</size></sub>";
        }

        /// <summary>
        /// Create fraction display
        /// </summary>
        public static string Fraction(string numerator, string denominator, float size = 70f)
        {
            return $"{Superscript(numerator, size)}/{Subscript(denominator, size)}";
        }

        #endregion

        #region Formatters

        private static void ConvertUnicodeCharacters(StringBuilder text, FormatConfig config)
        {
            string input = text.ToString();
            text.Clear();

            foreach (char c in input)
            {
                if (SuperscriptMap.TryGetValue(c, out string supValue))
                {
                    text.Append(Superscript(supValue, config.SuperscriptSize));
                }
                else if (SubscriptMap.TryGetValue(c, out string subValue))
                {
                    text.Append(Subscript(subValue, config.SubscriptSize));
                }
                else
                {
                    text.Append(c);
                }
            }
        }

        private static void ConvertCaretNotation(StringBuilder text, FormatConfig config)
        {
            // Pattern: x^2, x^-2, x^(n+1), e^{2x}
            string pattern = @"\^(\{[^}]+\}|\([^)]+\)|[\w\-+]+)";
            string input = text.ToString();

            string result = Regex.Replace(input, pattern, match =>
            {
                string content = match.Groups[1].Value;
                // Remove braces or parentheses if present
                content = content.Trim('{', '}', '(', ')');
                return Superscript(content, config.SuperscriptSize);
            });

            text.Clear();
            text.Append(result);
        }

        private static void ConvertUnderscoreSubscript(StringBuilder text, FormatConfig config)
        {
            // Pattern: x_1, x_{n+1}, a_(i,j)
            string pattern = @"_(\{[^}]+\}|\([^)]+\)|[\w\-+]+)";
            string input = text.ToString();

            string result = Regex.Replace(input, pattern, match =>
            {
                string content = match.Groups[1].Value;
                content = content.Trim('{', '}', '(', ')');
                return Subscript(content, config.SubscriptSize);
            });

            text.Clear();
            text.Append(result);
        }

        private static void ConvertFractions(StringBuilder text, FormatConfig config)
        {
            // Pattern: 1/2, (a+b)/(c+d), {numerator}/{denominator}
            string pattern = @"(\([^)]+\)|\{[^}]+\}|\d+)/(\([^)]+\)|\{[^}]+\}|\d+)";
            string input = text.ToString();

            string result = Regex.Replace(input, pattern, match =>
            {
                string num = match.Groups[1].Value.Trim('(', ')', '{', '}');
                string den = match.Groups[2].Value.Trim('(', ')', '{', '}');
                return Fraction(num, den, config.FractionSize);
            });

            text.Clear();
            text.Append(result);
        }

        private static void ConvertChemicalFormulas(StringBuilder text, FormatConfig config)
        {
            // Pattern: H2O, CO2, Ca(OH)2 - numbers after letters become subscripts
            // Avoid already formatted text (contains '<')
            string pattern = @"(?<!<[^>]*)([A-Z][a-z]?)(\d+)(?![^<]*>)";
            string input = text.ToString();

            string result = Regex.Replace(input, pattern, match =>
            {
                string element = match.Groups[1].Value;
                string number = match.Groups[2].Value;
                return element + Subscript(number, config.SubscriptSize);
            });

            text.Clear();
            text.Append(result);
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Remove all TMP tags from text
        /// </summary>
        public static string StripTags(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return Regex.Replace(text, @"<[^>]+>", string.Empty);
        }

        /// <summary>
        /// Check if text contains TMP formatting
        /// </summary>
        public static bool HasFormatting(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            return text.Contains("<sup>") || text.Contains("<sub>") ||
                   text.Contains("<size=");
        }

        /// <summary>
        /// Convert plain text numbers to superscript
        /// </summary>
        public static string ToSuperscript(string text, float size = 60f)
        {
            return Superscript(text, size);
        }

        /// <summary>
        /// Convert plain text numbers to subscript
        /// </summary>
        public static string ToSubscript(string text, float size = 60f)
        {
            return Subscript(text, size);
        }

        #endregion
    }

    #region Extension Methods

    public static class TMPTextExtensions
    {
        /// <summary>
        /// Format string using TMPTextFormatter with default config
        /// </summary>
        public static string FormatForTMP(this string text)
        {
            return TMPNotationEngine.Format(text);
        }

        /// <summary>
        /// Format string with custom config
        /// </summary>
        public static string FormatForTMP(this string text, TMPNotationEngine.FormatConfig config)
        {
            return TMPNotationEngine.Format(text, config);
        }

        /// <summary>
        /// Remove all TMP tags from text
        /// </summary>
        public static string StripTMPTags(this string text)
        {
            return TMPNotationEngine.StripTags(text);
        }

        /// <summary>
        /// Check if text contains TMP formatting
        /// </summary>
        public static bool HasTMPFormatting(this string text)
        {
            return TMPNotationEngine.HasFormatting(text);
        }

        /// <summary>
        /// Convert text to superscript format
        /// </summary>
        public static string ToSuperscript(this string text, float size = 60f)
        {
            return TMPNotationEngine.Superscript(text, size);
        }

        /// <summary>
        /// Convert text to subscript format
        /// </summary>
        public static string ToSubscript(this string text, float size = 60f)
        {
            return TMPNotationEngine.Subscript(text, size);
        }

        /// <summary>
        /// Create fraction from numerator and denominator
        /// </summary>
        public static string ToFraction(this string numerator, string denominator, float size = 70f)
        {
            return TMPNotationEngine.Fraction(numerator, denominator, size);
        }
    }

    #endregion
}

#region Usage Examples
/*
// BASIC USAGE:
using TextFormatting;

// Simple formatting with defaults
string formatted = TMPTextFormatter.Format("x^2 + H2O → CO2");
tmpText.text = formatted;

// Using extension method
tmpText.text = "E = mc^2".FormatForTMP();

// CUSTOM CONFIGURATION:
var config = new TMPTextFormatter.FormatConfig
{
    SuperscriptSize = 50f,
    SubscriptSize = 55f,
    EnableChemicalFormulas = true
};
tmpText.text = TMPTextFormatter.Format("H2SO4 + x^2", config);

// MANUAL FORMATTING:
string super = TMPTextFormatter.Superscript("2");
string sub = TMPTextFormatter.Subscript("0");
string fraction = TMPTextFormatter.Fraction("a+b", "c+d");

// SUPPORTED PATTERNS:
// Caret: x^2, x^-3, x^(n+1), e^{2x}
// Underscore: x_1, x_{n+1}, a_(i,j)
// Unicode: x² x₂ (automatic conversion)
// Fractions: 1/2, (a+b)/(c+d)
// Chemistry: H2O, CO2, Ca(OH)2

// UTILITY METHODS:
bool hasFormat = TMPTextFormatter.HasFormatting(text);
string plain = TMPTextFormatter.StripTags(formattedText);

// EXTENSION METHODS - Math/Scientific formatting:
string superscript = "2".ToSuperscript();  // <sup><size=60%>2</size></sup>
string subscript = "n".ToSubscript();      // <sub><size=60%>n</size></sub>
string fraction = "a+b".ToFraction("c+d"); // Fraction format

// Auto-format mathematical expressions
tmpText.text = "x^2 + H2O".FormatForTMP();

// Strip formatting for plain text
string plainText = formattedText.StripTMPTags();
bool hasFormatting = text.HasTMPFormatting();

// Fluent chaining
string equation = "E=mc^2".FormatForTMP();
*/
#endregion