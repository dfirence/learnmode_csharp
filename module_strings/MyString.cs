using static System.Console;
using System.Text;

namespace MyProgram
{
    class MyString
    {
        public string Value = string.Empty;
        public int Length = 0;
        public MyString(string s)
        {
            this.Value = s;
            this.Length = this.Value.Length;
        }
        public void Display()
        {
            WriteLine($@"
            Class Name         : {this.GetType().Name}
            Class Value Length : {this.Length}
            Class Value        : {this.Value}
            Class Value Base64 : 
            ----------------------------------------------
            Value Bytes
            ----------------------------------------------
            ");
            this.ToBase64();
            this.ShowBytes();
        }
        private void ShowBytes()
        {
            string result = "\n";
            byte counter = 0;
            for (var i = 0; i < this.Length; i++)
            {
                byte b = Convert.ToByte(this.Value[i]);
                result += $" 0x{b:X} ";
                counter += 1;
                if (counter == 16)
                {
                    counter = 0;
                    result += "\n";
                }
            }
            WriteLine($"{result}\n");
        }
        private void ToBase64()
        {
            byte[] bytes = Encoding.ASCII.GetBytes(this.Value);
            var b64 = Convert.ToBase64String(bytes);
            WriteLine($"{"ToBase64: >",30} {b64}");
        }
    }
}