using System;

namespace Telegram.Altayskaya97.Core.Model
{
    public class Command
    {
        public string Name { get; }
        public string Template { get; }
        public string Description { get; }
        public string Text { get; set; }
        public bool IsShown { get; }
        public bool IsAdmin { get; }
        public Command(string name, string template, string description, bool isAdmin = false, bool isShown = true)
        {
            Name = name.Trim();
            Template = template.Trim();
            Description = description;
            IsAdmin = isAdmin;
            IsShown = isShown;
        }

        public bool IsValid
        {
            get
            {
                if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Template) || string.IsNullOrEmpty(Text) 
                    || !Name.StartsWith("/") || !Template.StartsWith("/"))
                    return false;

                var templateChars = Template.ToCharArray();
                
                int countPartsCommand = 1;
                int bracketStatus = 0;
                for (int i = 1; i < templateChars.Length; i++)
                {
                    if (templateChars[i] == '[')
                        bracketStatus++;
                    else if (templateChars[i] == ']')
                        bracketStatus--;
                    else if (templateChars[i] == ' ' && bracketStatus == 0)
                        countPartsCommand++;
                }

                var textParts = CountParts(Text);
                return textParts >= countPartsCommand;
            }
        }

        public long? GetFirstNumber()
        {
            var content = GetFirstWord();

            if (string.IsNullOrEmpty(content) || !long.TryParse(content, out long result))
                return null;

            return result;
        }

        public string GetFirstWord()
        {
            var content = this.Text.Replace(this.Name, "").Trim();

            int firstSpaceIndex = content.IndexOf(' ');
            if (firstSpaceIndex > -1)
            {
                content = content.Substring(0, firstSpaceIndex);
            }

            return content;
        }

        private int CountParts(string text)
        {
            return text.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Length;
        }

        public override string ToString()
        {
            return Name;
        }

        public static bool operator ==(Command operand1, Command operand2)
        {
            if (operand1 is null)
            {
                return operand2 is null;
            }

            if (operand2 is null)
            {
                return operand1 is null;
            }

            return operand1.Name.ToLower() == operand2.Name.ToLower();
        }

        public static bool operator !=(Command operand1, Command operand2)
        {
            return !(operand1 == operand2);
        }

        public override bool Equals(object obj)
        {
            var command = obj as Command;
            if (command == null)
                return false;

            return Name.Equals(command.Name);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name);
        }
    }
}
