using Backend.Data;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services
{
    public class InventoryNumberService
    {
        private readonly ApplicationDbContext _context;

        public InventoryNumberService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Генерирует уникальный инвентарный номер для оборудования
        /// Формат: PREFIX-XXXX, где PREFIX - первые 3 буквы типа оборудования (транслитерированные)
        /// </summary>
        public async Task<string> GenerateInventoryNumber(int typeId, string typeName)
        {
            // Получаем префикс из названия типа (первые 3 символа, транслитерированные в латиницу)
            var prefix = GetPrefix(typeName);

            // Считаем количество оборудования данного типа
            var count = await _context.Equipment.CountAsync(e => e.TypeId == typeId);

            // Генерируем номер
            var number = (count + 1).ToString("D4"); // 4 цифры с ведущими нулями

            return $"{prefix}-{number}";
        }

        private string GetPrefix(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "EQP";

            // Транслитерация кириллицы в латиницу
            var transliterated = Transliterate(name);

            // Берем первые 3 буквы и приводим к верхнему регистру
            var prefix = new string(transliterated
                .Where(char.IsLetter)
                .Take(3)
                .ToArray())
                .ToUpper();

            return prefix.Length >= 3 ? prefix : prefix.PadRight(3, 'X');
        }

        private string Transliterate(string text)
        {
            var translitMap = new Dictionary<char, string>
            {
                {'а', "a"}, {'б', "b"}, {'в', "v"}, {'г', "g"}, {'д', "d"},
                {'е', "e"}, {'ё', "e"}, {'ж', "zh"}, {'з', "z"}, {'и', "i"},
                {'й', "y"}, {'к', "k"}, {'л', "l"}, {'м', "m"}, {'н', "n"},
                {'о', "o"}, {'п', "p"}, {'р', "r"}, {'с', "s"}, {'т', "t"},
                {'у', "u"}, {'ф', "f"}, {'х', "h"}, {'ц', "ts"}, {'ч', "ch"},
                {'ш', "sh"}, {'щ', "sch"}, {'ъ', ""}, {'ы', "y"}, {'ь', ""},
                {'э', "e"}, {'ю', "yu"}, {'я', "ya"},
                {'А', "A"}, {'Б', "B"}, {'В', "V"}, {'Г', "G"}, {'Д', "D"},
                {'Е', "E"}, {'Ё', "E"}, {'Ж', "Zh"}, {'З', "Z"}, {'И', "I"},
                {'Й', "Y"}, {'К', "K"}, {'Л', "L"}, {'М', "M"}, {'Н', "N"},
                {'О', "O"}, {'П', "P"}, {'Р', "R"}, {'С', "S"}, {'Т', "T"},
                {'У', "U"}, {'Ф', "F"}, {'Х', "H"}, {'Ц', "Ts"}, {'Ч', "Ch"},
                {'Ш', "Sh"}, {'Щ', "Sch"}, {'Ъ', ""}, {'Ы', "Y"}, {'Ь', ""},
                {'Э', "E"}, {'Ю', "Yu"}, {'Я', "Ya"}
            };

            var result = new System.Text.StringBuilder();
            foreach (var c in text)
            {
                if (translitMap.TryGetValue(c, out var replacement))
                    result.Append(replacement);
                else
                    result.Append(c);
            }
            return result.ToString();
        }
    }
}
