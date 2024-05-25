using static System.Console;
using System.Text;

namespace MyModules.Strings;


/// <summary>
/// Class <c>MyString</c> shows usage of C# Strings.
/// </summary>
public class MyString
{
    /// <summary>
    /// String _value used to evaluate different methods
    /// </summary>
    private readonly string _value = string.Empty;

    /// <summary>
    /// _length of String _value
    /// </summary>
    private readonly int _length = 0;

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
        this._value = s.Trim();
        this._length = this._value.Length;
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
    /// Convert internal _value to Bytes
    /// </summary>
    public void GetBytes()
    {
        byte[] bytes = Encoding.ASCII.GetBytes(this._value);
        int chars_count = Encoding.ASCII.GetCharCount(bytes);
        int bytes_count = Encoding.ASCII.GetByteCount(this._value);
        string _values = string.Join(" ", bytes);
        string hex_values = string.Join(" ", bytes.Select(b => $"{b:X}").ToArray());

        this.Display(
            $@"
            String _value    : {this._value}
            String _length   : {this._length}
            Chars Count      : {chars_count}
            Bytes Count      : {bytes_count}
            Bytes (Decimal)  : [ {_values} ]
            Bytes (Hex)      : [ {hex_values} ]
            "
        );
    }

    /// <summary>
    /// Gets Vowels contained in the _value
    /// </summary>
    public void GetVowels()
    {
        char[] vowels = { 'a', 'e', 'i', 'o', 'u' };

        char[] chars = this._value.ToLower()
                           .ToCharArray()
                           .Where(c => vowels.Contains(c))
                           .ToArray();
        this.Display(
            $"Get Vowels > [ {string.Join(", ", chars)} ]"
        );
    }
    /// <summary>
    /// Custom CSV of Character, Decimal, Hex _value
    /// </summary>
    public void ToCsv()
    {

        byte[] bytes = Encoding.ASCII.GetBytes(this._value);
        string csv = "\"Character\",\"Byte_Decimal\",\"Byte_Hex\"\n";
        csv += string.Join("", bytes.Select(
            b => $"\t\t\"{(char)b}\",\"{b}\",\"{b:X}\"\n"
        ).ToArray());

        this.Display(csv);
    }

    /// <summary>
    /// Converts the _value into a base64 string
    /// </summary>
    public void ToBase64()
    {
        byte[] bytes = Encoding.ASCII.GetBytes(this._value);
        string b64 = Convert.ToBase64String(bytes);
        this.Display(
            $"ToBase64 > {b64}"
        );
    }

    /// <summary>
    /// Converts the _value to lowercase
    /// </summary>
    public void ToLowerCase()
    {
        this.Display(
            $"LowerCase > {this._value.ToLower()}"
        );
    }

    /// <summary>
    /// Converts the _value to uppercase
    /// </summary>
    public void ToUpperCase()
    {
        this.Display(
            $"UpperCase > {this._value.ToUpper()}"
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