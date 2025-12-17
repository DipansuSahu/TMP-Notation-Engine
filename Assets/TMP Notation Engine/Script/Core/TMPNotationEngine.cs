using System;
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
            private float _superscriptSize = 60f;
            private float _subscriptSize = 60f;
            private float _fractionSize = 70f;

            public float SuperscriptSize
            {
                get => _superscriptSize;
                set => _superscriptSize = value > 0 && value <= 200 ? value : 60f;
            }

            public float SubscriptSize
            {
                get => _subscriptSize;
                set => _subscriptSize = value > 0 && value <= 200 ? value : 60f;
            }

            public float FractionSize
            {
                get => _fractionSize;
                set => _fractionSize = value > 0 && value <= 200 ? value : 70f;
            }

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

        // Thread-safe lazy initialization of reverse mappings
        private static readonly Lazy<Dictionary<string, char>> _reverseSuperscriptMap =
            new Lazy<Dictionary<string, char>>(() =>
            {
                var dict = new Dictionary<string, char>();
                foreach (var kvp in SuperscriptMap)
                {
                    dict[kvp.Value] = kvp.Key;
                }
                return dict;
            });

        private static readonly Lazy<Dictionary<string, char>> _reverseSubscriptMap =
            new Lazy<Dictionary<string, char>>(() =>
            {
                var dict = new Dictionary<string, char>();
                foreach (var kvp in SubscriptMap)
                {
                    dict[kvp.Value] = kvp.Key;
                }
                return dict;
            });

        private static Dictionary<string, char> ReverseSuperscriptMap => _reverseSuperscriptMap.Value;
        private static Dictionary<string, char> ReverseSubscriptMap => _reverseSubscriptMap.Value;

        #endregion

        #region Compiled Regex Patterns

        private static readonly Regex CaretNotationRegex =
            new Regex(@"\^(\{[^}]+\}|\([^)]+\)|[\w\-+]+)", RegexOptions.Compiled);

        private static readonly Regex UnderscoreSubscriptRegex =
            new Regex(@"_(\{[^}]+\}|\([^)]+\)|[\w\-+]+)", RegexOptions.Compiled);

        private static readonly Regex FractionRegex =
            new Regex(@"(\([^)]+\)|\{[^}]+\}|\d+)/(\([^)]+\)|\{[^}]+\}|\d+)", RegexOptions.Compiled);

        private static readonly Regex ChemicalFormulaRegex =
            new Regex(@"(?<!<[^>]*)([A-Z][a-z]?)(\d+)(?![^<]*>)", RegexOptions.Compiled);

        private static readonly Regex SuperscriptTagRegex =
            new Regex(@"<sup><size=\d+%>([^<]+)</size></sup>", RegexOptions.Compiled);

        private static readonly Regex SubscriptTagRegex =
            new Regex(@"<sub><size=\d+%>([^<]+)</size></sub>", RegexOptions.Compiled);

        private static readonly Regex AllTagsRegex =
            new Regex(@"<[^>]+>", RegexOptions.Compiled);

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
            if (string.IsNullOrEmpty(text))
                return text;
            return $"<sup><size={size}%>{text}</size></sup>";
        }

        /// <summary>
        /// Create subscript tag
        /// </summary>
        public static string Subscript(string text, float size = 60f)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            return $"<sub><size={size}%>{text}</size></sub>";
        }

        /// <summary>
        /// Create fraction display
        /// </summary>
        public static string Fraction(string numerator, string denominator, float size = 70f)
        {
            if (string.IsNullOrEmpty(numerator) || string.IsNullOrEmpty(denominator))
                return $"{numerator}/{denominator}";
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
            string input = text.ToString();

            string result = CaretNotationRegex.Replace(input, match =>
            {
                string content = match.Groups[1].Value;

                // Handle empty content (edge case: x^)
                if (string.IsNullOrEmpty(content))
                    return match.Value;

                // Remove braces or parentheses if present
                content = content.Trim('{', '}', '(', ')');

                // Validate content is not empty after trimming
                if (string.IsNullOrEmpty(content))
                    return match.Value;

                return Superscript(content, config.SuperscriptSize);
            });

            text.Clear();
            text.Append(result);
        }

        private static void ConvertUnderscoreSubscript(StringBuilder text, FormatConfig config)
        {
            string input = text.ToString();

            string result = UnderscoreSubscriptRegex.Replace(input, match =>
            {
                string content = match.Groups[1].Value;

                // Handle empty content (edge case: x_)
                if (string.IsNullOrEmpty(content))
                    return match.Value;

                content = content.Trim('{', '}', '(', ')');

                // Validate content is not empty after trimming
                if (string.IsNullOrEmpty(content))
                    return match.Value;

                return Subscript(content, config.SubscriptSize);
            });

            text.Clear();
            text.Append(result);
        }

        private static void ConvertFractions(StringBuilder text, FormatConfig config)
        {
            string input = text.ToString();

            string result = FractionRegex.Replace(input, match =>
            {
                string num = match.Groups[1].Value.Trim('(', ')', '{', '}');
                string den = match.Groups[2].Value.Trim('(', ')', '{', '}');

                // Validate both parts exist
                if (string.IsNullOrEmpty(num) || string.IsNullOrEmpty(den))
                    return match.Value;

                return Fraction(num, den, config.FractionSize);
            });

            text.Clear();
            text.Append(result);
        }

        private static void ConvertChemicalFormulas(StringBuilder text, FormatConfig config)
        {
            string input = text.ToString();

            string result = ChemicalFormulaRegex.Replace(input, match =>
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
        /// Remove all TMP tags from text (converts to plain text)
        /// </summary>
        public static string PlainText(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            return AllTagsRegex.Replace(text, string.Empty);
        }

        /// <summary>
        /// Convert TMP formatted text back to Unicode representation
        /// Example: "A<sub><size=60%>0</size></sub>" → "A₀"
        /// </summary>
        public static string Unicode(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            string result = text;

            // Convert superscript tags back to Unicode
            result = SuperscriptTagRegex.Replace(result, match =>
            {
                string content = match.Groups[1].Value;

                // Try to convert single characters
                if (content.Length == 1 && ReverseSuperscriptMap.TryGetValue(content, out char unicodeChar))
                {
                    return unicodeChar.ToString();
                }

                // Try to convert each character in multi-char content
                StringBuilder sb = new StringBuilder();
                bool allConverted = true;

                foreach (char c in content)
                {
                    string charStr = c.ToString();
                    if (ReverseSuperscriptMap.TryGetValue(charStr, out char unicode))
                    {
                        sb.Append(unicode);
                    }
                    else
                    {
                        allConverted = false;
                        break;
                    }
                }

                // If all characters converted successfully, use Unicode version
                if (allConverted && sb.Length > 0)
                {
                    return sb.ToString();
                }

                // Otherwise, just strip tags and return plain content
                return content;
            });

            // Convert subscript tags back to Unicode
            result = SubscriptTagRegex.Replace(result, match =>
            {
                string content = match.Groups[1].Value;

                // Try to convert single characters
                if (content.Length == 1 && ReverseSubscriptMap.TryGetValue(content, out char unicodeChar))
                {
                    return unicodeChar.ToString();
                }

                // Try to convert each character in multi-char content
                StringBuilder sb = new StringBuilder();
                bool allConverted = true;

                foreach (char c in content)
                {
                    string charStr = c.ToString();
                    if (ReverseSubscriptMap.TryGetValue(charStr, out char unicode))
                    {
                        sb.Append(unicode);
                    }
                    else
                    {
                        allConverted = false;
                        break;
                    }
                }

                // If all characters converted successfully, use Unicode version
                if (allConverted && sb.Length > 0)
                {
                    return sb.ToString();
                }

                // Otherwise, just strip tags and return plain content
                return content;
            });

            return result;
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
        /// Check if text contains Unicode superscript characters
        /// </summary>
        public static bool HasUnicodeSuperscript(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            foreach (char c in text)
            {
                if (SuperscriptMap.ContainsKey(c))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Check if text contains Unicode subscript characters
        /// </summary>
        public static bool HasUnicodeSubscript(string text)
        {
            if (string.IsNullOrEmpty(text))
                return false;

            foreach (char c in text)
            {
                if (SubscriptMap.ContainsKey(c))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Check if text contains any Unicode superscript or subscript characters
        /// </summary>
        public static bool HasUnicodeScripts(string text)
        {
            return HasUnicodeSuperscript(text) || HasUnicodeSubscript(text);
        }

        /// <summary>
        /// Get all Unicode superscript characters found in text
        /// </summary>
        public static List<char> GetUnicodeSuperscripts(string text)
        {
            List<char> found = new List<char>();
            if (string.IsNullOrEmpty(text))
                return found;

            foreach (char c in text)
            {
                if (SuperscriptMap.ContainsKey(c) && !found.Contains(c))
                    found.Add(c);
            }

            return found;
        }

        /// <summary>
        /// Get all Unicode subscript characters found in text
        /// </summary>
        public static List<char> GetUnicodeSubscripts(string text)
        {
            List<char> found = new List<char>();
            if (string.IsNullOrEmpty(text))
                return found;

            foreach (char c in text)
            {
                if (SubscriptMap.ContainsKey(c) && !found.Contains(c))
                    found.Add(c);
            }

            return found;
        }

        #endregion
    }

    #region Extension Methods

    public static class TMPTextExtensions
    {
        /// <summary>
        /// Format string using TMPNotationEngine with default config
        /// </summary>
        public static string ToFormatTMP(this string text)
        {
            return text == null ? null : TMPNotationEngine.Format(text);
        }

        /// <summary>
        /// Format string with custom config
        /// </summary>
        public static string ToFormatTMP(this string text, TMPNotationEngine.FormatConfig config)
        {
            return text == null ? null : TMPNotationEngine.Format(text, config);
        }

        /// <summary>
        /// Convert text to superscript format
        /// </summary>
        public static string ToSuperscriptTMP(this string text, float size = 60f)
        {
            return text == null ? null : TMPNotationEngine.Superscript(text, size);
        }

        /// <summary>
        /// Convert text to subscript format
        /// </summary>
        public static string ToSubscriptTMP(this string text, float size = 60f)
        {
            return text == null ? null : TMPNotationEngine.Subscript(text, size);
        }

        /// <summary>
        /// Create fraction from numerator and denominator
        /// </summary>
        public static string ToFractionTMP(this string numerator, string denominator, float size = 70f)
        {
            return TMPNotationEngine.Fraction(numerator, denominator, size);
        }

        /// <summary>
        /// Convert TMP formatted text back to Unicode representation
        /// Example: "A<sub><size=60%>0</size></sub>" → "A₀"
        /// </summary>
        public static string ToUnicodeTMP(this string text)
        {
            return text == null ? null : TMPNotationEngine.Unicode(text);
        }

        /// <summary>
        /// Convert TMP formatted text to plain text (strips all formatting)
        /// Example: "A<sub><size=60%>0</size></sub>" → "A0"
        /// </summary>
        public static string ToPlainTextTMP(this string text)
        {
            return text == null ? null : TMPNotationEngine.PlainText(text);
        }

        /// <summary>
        /// Check if text contains TMP formatting
        /// </summary>
        public static bool HasFormattingTMP(this string text)
        {
            return TMPNotationEngine.HasFormatting(text);
        }

        /// <summary>
        /// Check if text contains Unicode superscript characters
        /// </summary>
        public static bool HasUnicodeSuperscriptTMP(this string text)
        {
            return TMPNotationEngine.HasUnicodeSuperscript(text);
        }

        /// <summary>
        /// Check if text contains Unicode subscript characters
        /// </summary>
        public static bool HasUnicodeSubscriptTMP(this string text)
        {
            return TMPNotationEngine.HasUnicodeSubscript(text);
        }

        /// <summary>
        /// Check if text contains any Unicode superscript or subscript characters
        /// </summary>
        public static bool HasUnicodeScriptsTMP(this string text)
        {
            return TMPNotationEngine.HasUnicodeScripts(text);
        }

        /// <summary>
        /// Get all Unicode superscript characters found in text
        /// </summary>
        public static List<char> GetUnicodeSuperscriptsTMP(this string text)
        {
            return TMPNotationEngine.GetUnicodeSuperscripts(text);
        }

        /// <summary>
        /// Get all Unicode subscript characters found in text
        /// </summary>
        public static List<char> GetUnicodeSubscriptsTMP(this string text)
        {
            return TMPNotationEngine.GetUnicodeSubscripts(text);
        }
    }

    #endregion
}

#region Usage Examples
/*
===================================================================================
BASIC USAGE
===================================================================================
using AbS;  // Add this namespace to your script

// Simple formatting with defaults
string formatted = TMPNotationEngine.Format("x^2 + H2O → CO2");
tmpText.text = formatted;

// Using extension method
tmpText.text = "E = mc^2".ToFormatTMP();

===================================================================================
CUSTOM CONFIGURATION
===================================================================================
var config = new TMPNotationEngine.FormatConfig
{
    SuperscriptSize = 50f,
    SubscriptSize = 55f,
    EnableChemicalFormulas = true
};
tmpText.text = TMPNotationEngine.Format("H2SO4 + x^2", config);

===================================================================================
MANUAL FORMATTING
===================================================================================
string super = TMPNotationEngine.Superscript("2");
string sub = TMPNotationEngine.Subscript("0");
string fraction = TMPNotationEngine.Fraction("a+b", "c+d");

===================================================================================
SUPPORTED PATTERNS
===================================================================================
Caret Notation:    x^2, x^-3, x^(n+1), e^{2x}
Underscore:        x_1, x_{n+1}, a_(i,j)
Unicode:           x² x₂ (automatic conversion)
Fractions:         1/2, (a+b)/(c+d)
Chemistry:         H2O, CO2, Ca(OH)2

===================================================================================
UTILITY METHODS
===================================================================================
// Check for formatting
bool hasFormat = TMPNotationEngine.HasFormatting(text);

// Convert to plain text
string plain = TMPNotationEngine.PlainText(formattedText);

// Check for Unicode characters
bool hasSuperUnicode = TMPNotationEngine.HasUnicodeSuperscript("x²y³");     // true
bool hasSubUnicode = TMPNotationEngine.HasUnicodeSubscript("H₂O");          // true
bool hasAnyUnicode = TMPNotationEngine.HasUnicodeScripts("x²₁");            // true

// Get specific Unicode characters found
List<char> supers = TMPNotationEngine.GetUnicodeSuperscripts("x²y³z⁴");    // ['²', '³', '⁴']
List<char> subs = TMPNotationEngine.GetUnicodeSubscripts("H₂O + CO₂");     // ['₂']

===================================================================================
CONVERSION METHODS
===================================================================================
// Convert TMP formatted text back to Unicode
string formatted = "A<sub><size=60%>0</size></sub>";
string unicode = TMPNotationEngine.Unicode(formatted);      // "A₀"

string formatted2 = "x<sup><size=60%>2</size></sup>";
string unicode2 = TMPNotationEngine.Unicode(formatted2);    // "x²"

// Convert to plain text (strips formatting)
string plainText = TMPNotationEngine.PlainText(formatted);  // "A0"

// Round-trip example
string original = "H₂O";
string tmpFormatted = TMPNotationEngine.Format(original);     // "H<sub><size=60%>2</size></sub>O"
string backToUnicode = TMPNotationEngine.Unicode(tmpFormatted); // "H₂O"

===================================================================================
EXTENSION METHODS - Math/Scientific formatting
===================================================================================
string superscript = "2".ToSuperscriptTMP();      // <sup><size=60%>2</size></sup>
string subscript = "n".ToSubscriptTMP();          // <sub><size=60%>n</size></sub>
string fraction = "a+b".ToFractionTMP("c+d");     // Fraction format

// Auto-format mathematical expressions
tmpText.text = "x^2 + H2O".ToFormatTMP();

// Check Unicode using extensions
bool hasSuper = "x²y³".HasUnicodeSuperscriptTMP();  // true
bool hasSub = "H₂O".HasUnicodeSubscriptTMP();        // true
bool hasAny = "x²₁".HasUnicodeScriptsTMP();          // true

// Get Unicode characters using extensions
List<char> foundSupers = "x²y³".GetUnicodeSuperscriptsTMP();
List<char> foundSubs = "H₂O".GetUnicodeSubscriptsTMP();

// Convert back to Unicode using extensions
string formattedText = "A<sub><size=60%>0</size></sub>";
string unicodeText = formattedText.ToUnicodeTMP();   // "A₀"
string plainOnly = formattedText.ToPlainTextTMP();   // "A0"

// Strip formatting for plain text
string stripped = formattedText.ToPlainTextTMP();
bool hasFormatting = text.HasFormattingTMP();

// Fluent chaining
string equation = "E=mc^2".ToFormatTMP();

===================================================================================
ADVANCED EXAMPLES
===================================================================================
// Complex mathematical expressions
string math = "∫(x^2 + 2x + 1)dx = x^3/3 + x^2 + x + C".ToFormatTMP();

// Chemical reactions
string chem = "2H2 + O2 → 2H2O".ToFormatTMP();

// Physics formulas
string physics = "F = G(m_1*m_2)/r^2".ToFormatTMP();

// Mixed notation
string mixed = "x₁² + y^2 = r²".ToFormatTMP();

===================================================================================
*/
#endregion