using System.Linq;
using System.Collections.Generic;
using System.Text;
using Telegram.BotAPI.AvailableTypes;
using Telegram.BotAPI;
using Telegram.Altayskaya97.Service.Interface;

namespace Telegram.Altayskaya97.Service
{
    public class HtmlTextFormatGenerator : IHtmlTextFormatGenerator
    {
        private int _shift = 0;

        public string GenerateHtmlText(Message message)
        {
            if (!string.IsNullOrEmpty(message.Text))
                return GenerateTextByEntities(message.Text, message.Entities);
            else if (!string.IsNullOrEmpty(message.Caption))
                return GenerateTextByEntities(message.Caption, message.CaptionEntities);
            return string.Empty;
        }

        private string GenerateTextByEntities(string text, IEnumerable<MessageEntity> entities)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            var updatedText = UpdateSpecialLetters(text, entities);

            StringBuilder textBuilder = new StringBuilder(updatedText);

            if (entities == null)
                return textBuilder.ToString();

            for (int i = 0; i < entities.Count(); i++)
            {
                var entity = entities.ElementAt(i);
                string val = updatedText.Substring(entity.Offset, entity.Length);
                switch (entity.GetEntityType())
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

        private string UpdateSpecialLetters(string sourceText, IEnumerable<MessageEntity> entities)
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
                        UpdateEntities(entities, i, ampArray.Length - 1);
                        break;
                    case '<':
                        var ltArray = "&lt;".ToCharArray();
                        destChars.AddRange(ltArray);
                        UpdateEntities(entities, i, ltArray.Length - 1);
                        break;
                    case '>':
                        var gtArray = "&gt;".ToCharArray();
                        destChars.AddRange(gtArray);
                        UpdateEntities(entities, i, gtArray.Length - 1);
                        break;
                    case '\"':
                        var quotArray = "&quot;".ToCharArray();
                        destChars.AddRange(quotArray);
                        UpdateEntities(entities, i, quotArray.Length - 1);
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

        private void UpdateEntities(IEnumerable<MessageEntity> entities, int position, int shift)
        {
            if (entities == null || !entities.Any())
                return;

            var entitiesToUpdate = entities.Where(e => e.Offset > position);
            foreach (var entity in entitiesToUpdate)
                entity.Offset += shift;
        }
    }
}
