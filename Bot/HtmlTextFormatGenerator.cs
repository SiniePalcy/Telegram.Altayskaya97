using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Telegram.Altayskaya97.Bot
{
    public class HtmlTextFormatGenerator
    {
        private int _shift = 0;
        private string[] _tags = new string[] { "<b>", "<i>", "<u>", "<s>", "<code>"};

        public string GenerateHtmlText(Message message)
        {
            if (message.Type == MessageType.Text)
                return GenerateTextByEntities(message.Text, message.Entities, message.EntityValues);
            else if (message.Type == MessageType.Photo)
                return GenerateTextByEntities(message.Caption, message.CaptionEntities, message.CaptionEntityValues);
            return string.Empty;
        }

        private string GenerateTextByEntities(string text, MessageEntity[] entities, IEnumerable<string> values)
        {
            var updatedText = UpdateSpecialLetters(text, entities);

            StringBuilder textBuilder = new StringBuilder(updatedText);

            if (entities == null)
                return textBuilder.ToString();

            for (int i = 0; i < entities.Length; i++)
            {
                var entity = entities[i];
                string val = updatedText.Substring(entity.Offset, entity.Length);
                switch (entity.Type)
                {
                    case MessageEntityType.Bold:
                        InsertTag(textBuilder, "<b>", val, entity.Offset, entity.Length);
                        break;
                    case MessageEntityType.Italic:
                        InsertTag(textBuilder, "<i>", val, entity.Offset, entity.Length);
                        break;
                    case MessageEntityType.Underline:
                        InsertTag(textBuilder, "<u>", val, entity.Offset, entity.Length);
                        break;
                    case MessageEntityType.Strikethrough:
                        InsertTag(textBuilder, "<s>", val, entity.Offset, entity.Length);
                        break;
                    case MessageEntityType.Code:
                        InsertTag(textBuilder, "<code>", val, entity.Offset, entity.Length);
                        break;
                    case MessageEntityType.TextLink:
                        InsertLinkTag(textBuilder, entity.Url, val, entity.Offset, entity.Length);
                        break;
                }
            }

            return textBuilder.ToString();
        }

        private string UpdateSpecialLetters(string sourceText, MessageEntity[] entities)
        {
            var sourceChars = sourceText.ToCharArray();
            List<char> destChars = new List<char>();
            for (int i = 0; i < sourceChars.Length; i++)
            {
                char ch = sourceChars[i];
                switch (ch)
                {
                    case '&':
                        var ampArray = "&amp;".ToCharArray();
                        destChars.AddRange(ampArray);
                        entities.Where(e => e.Offset > i).ToList().ForEach(e => e.Offset += ampArray.Length - 1);
                        break;
                    case '<':
                        var ltArray = "&lt;".ToCharArray();
                        destChars.AddRange(ltArray);
                        entities.Where(e => e.Offset > i).ToList().ForEach(e => e.Offset += ltArray.Length - 1);
                        break;
                    case '>':
                        var gtArray = "&gt;".ToCharArray();
                        destChars.AddRange(gtArray);
                        entities.Where(e => e.Offset > i).ToList().ForEach(e => e.Offset += gtArray.Length - 1);
                        break;
                    case '\"':
                        var quotArray = "&quot;".ToCharArray();
                        destChars.AddRange(quotArray);
                        entities.Where(e => e.Offset > i).ToList().ForEach(e => e.Offset += quotArray.Length - 1);
                        break;
                    default:
                        destChars.Add(ch);
                        break;
                }
            }

            return new string(destChars.ToArray());
        }

        private int InsertTag(StringBuilder buffer, string tag, string text, int offset, int length)
        {
            var endTag = tag.Insert(1, "/");
            
            string formatText = tag + text + endTag;
            
            int position = offset + _shift;
            buffer.Remove(position, length);
            buffer.Insert(position, formatText);
            
            _shift += tag.Length + endTag.Length;

            return position;
        }

        private int InsertLinkTag(StringBuilder buffer, string link, string text, int offset, int length)
        {
            var tag = $"<a href=\"{link}\">";
            var endTag = "</a>";

            string formatText = tag + text + endTag;

            int position = offset + _shift;
            buffer.Remove(position, length);
            buffer.Insert(position, formatText);

            _shift += tag.Length + endTag.Length;

            return position;
        }
    }
}
