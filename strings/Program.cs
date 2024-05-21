using static System.Console;

class Program
{
    public static void Main(string[] args)
    {
        string dashes = new string('-', 64);
        string title = "C# Learnmode Strings";
        string banner = $"\n{dashes}\n{title}\n{dashes}\n";
        WriteLine(banner);
        // MyString is a custom type declared as class
        MyString m = new MyString("Lorem Ipsum 42 Life");
        m.Show();
        m.GetVowels();
        m.ChangeCase(true);
        m.ChangeCase(false);
        m.GetHexBytes();
    }
}

public class MyString
{
    private string Value = string.Empty;
    private int Length = 0;
    public MyString(string s)
    {
        this.Value = s;
        this.Length = this.Value.Length;
    }
    public void Show()
    {
        // MultiLine string with `$@` prefix
        WriteLine($@"
            Value Length  : {this.Length}
            Value String  : {this.Value}
        ");
    }
    public void ChangeCase(bool asLowerCase)
    {
        if (asLowerCase)
        {
            WriteLine(this.Value.ToLower());
        }
        else
        {
            WriteLine(this.Value.ToUpper());
        }
    }
    public void GetVowels()
    {
        if (this.Value == "" && this.Length == 0)
        {
            WriteLine("Empty String, nothing to check for");
        }
        // Naive String Inspection
        string result = "\n";
        for (var i = 0; i < this.Length; i++)
        {
            char x = this.Value[i];
            if (x == 'a' || x == 'e' || x == 'i' || x == 'o' || x == 'u')
            {
                result += $"\t\tvowel : {x} | index: {i}\n";
            }
        }
        if (result.Length > 0)
        {
            WriteLine($@"
            ---------------------------------------
            String Naive Vowel Matching
            ---------------------------------------
            {result}
            ---------------------------------------
            ");
        }
    }
    public void GetHexBytes()
    {
        string result = "\n";
        for (var i = 0; i < this.Length; i++)
        {
            byte b = Convert.ToByte(this.Value[i]);
            result += $"\tchar: {this.Value[i]} | byte: 0x{b:X} | decimal: {b}\n";
        }
        WriteLine($@"
        ---------------------------------------
        String Chars To Bytes & Decimal Values
        ---------------------------------------
        {result}
        ---------------------------------------
        ");
    }
}