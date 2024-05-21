using static System.Console;
using System.Text;
using System.Runtime.InteropServices;

namespace MyModules.Strings;


/// <summary>
/// Class <c>MyString</c> shows usage of C# Strings.
/// </summary>
public class MyString
{
    /// <summary>
    /// String Value used to evaluate different methods
    /// </summary>
    private readonly string Value = string.Empty;

    /// <summary>
    /// Length of String Value
    /// </summary>
    private readonly int Length = 0;

    /// <summary>
    /// Name of Class used in this module
    /// </summary>
    private readonly string className = string.Empty;

    /// <summary>
    /// Helper String To Create Pretty Dashes In Stdout
    /// </summary>
    private readonly string _dashes = new string('-', 64);

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="s">String To Initialize</param>
    public MyString(string s)
    {
        this.Value = s.Trim();
        this.Length = this.Value.Length;
        this.className = this.GetType().Name;
    }

    /// <summary>
    /// Run Method, when instantiated class then this method
    /// is used to execute this module.
    /// </summary>
    public void Run()
    {
        this.Banner();
        this.GetBytes();
        this.GetVowels();
        this.ToBase64();
        this.ToCsv();
        this.ToLowerCase();
        this.ToUpperCase();
    }

    /// <summary>
    /// Convert internal Value to Bytes
    /// </summary>
    public void GetBytes()
    {
        byte[] bytes = Encoding.ASCII.GetBytes(this.Value);
        int chars_count = Encoding.ASCII.GetCharCount(bytes);
        int bytes_count = Encoding.ASCII.GetByteCount(this.Value);
        string values = string.Join(" ", bytes);
        string hex_values = string.Join(" ", bytes.Select(b => $"{b:X}").ToArray());

        this.Display(
            $@"
            String Value    : {this.Value}
            String Length   : {this.Length}
            Chars Count     : {chars_count}
            Bytes Count     : {bytes_count}
            Bytes (Decimal) : [ {values} ]
            Bytes (Hex)     : [ {hex_values} ]
            "
        );
    }

    /// <summary>
    /// Gets Vowels contained in the Value
    /// </summary>
    public void GetVowels()
    {
        char[] vowels = { 'a', 'e', 'i', 'o', 'u' };

        char[] chars = this.Value.ToLower()
                           .ToCharArray()
                           .Where(c => vowels.Contains(c))
                           .ToArray();
        this.Display(
            $"Get Vowels > [ {string.Join(", ", chars)} ]"
        );
    }
    /// <summary>
    /// Custom CSV of Character, Decimal, Hex Value
    /// </summary>
    public void ToCsv()
    {

        byte[] bytes = Encoding.ASCII.GetBytes(this.Value);
        string csv = "\"Character\",\"Byte_Decimal\",\"Byte_Hex\"\n";
        csv += string.Join("", bytes.Select(
            b => $"\t\t\"{(char)b}\",\"{b}\",\"{b:X}\"\n"
        ).ToArray());

        this.Display(csv);
    }

    /// <summary>
    /// Converts the value into a base64 string
    /// </summary>
    public void ToBase64()
    {
        byte[] bytes = Encoding.ASCII.GetBytes(this.Value);
        string b64 = Convert.ToBase64String(bytes);
        this.Display(
            $"ToBase64 > {b64}"
        );
    }

    /// <summary>
    /// Converts the value to lowercase
    /// </summary>
    public void ToLowerCase()
    {
        this.Display(
            $"LowerCase > {this.Value.ToLower()}"
        );
    }

    /// <summary>
    /// Converts the value to uppercase
    /// </summary>
    public void ToUpperCase()
    {
        this.Display(
            $"UpperCase > {this.Value.ToUpper()}"
        );
    }

    /// <summary>
    /// Convenient Banner String
    /// </summary>
    private void Banner()
    {
        Write(
            $@"
            {this._dashes}
            {"Strings Module",36} - {this.className}
            {this._dashes}
            "
        );
    }

    /// <summary>
    /// Displays the resultant operation used in this class
    /// </summary>
    /// <param name="s">custom string</param>
    private void Display(string s)
    {
        Write(
            $@"
            {s}
            "
        );
    }
}