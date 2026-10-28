using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace GestureSample.Maui.Handlers;

public static class AppLanguage
{
    // English source strings remain stable keys; never translate persisted game identifiers.
    private static readonly Dictionary<string, string[]> Translations = Catalog.Split('\n', StringSplitOptions.RemoveEmptyEntries)
        // Capitalization distinguishes labels (Left) from instruction cues (LEFT).
        .Select(line => line.TrimEnd('\r').Split('|')).ToDictionary(parts => parts[0], parts => parts, StringComparer.Ordinal);

    // Preserve case-insensitive lookup for callers whose capitalization differs from the catalog.
    private static readonly Dictionary<string, string[]> CaseInsensitiveTranslations = Translations.Values
        .GroupBy(parts => parts[0], StringComparer.OrdinalIgnoreCase)
        .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);

    public static string Text(string text, bool gripping = false) => Translate(text, LanguagePreferences.Get(gripping: gripping));

    public static string Translate(string text, InterfaceLanguage language)
    {
        if (string.IsNullOrEmpty(text) || language == InterfaceLanguage.English) return text;
        if (Translations.TryGetValue(text, out var words) || CaseInsensitiveTranslations.TryGetValue(text, out words))
            return words[(int)language];
        if (text.StartsWith("USER SETTINGS (", StringComparison.Ordinal))
            return Translate("USER SETTINGS", language) + text[13..];
        if (text.StartsWith("Gripping - ", StringComparison.Ordinal))
            return Translate("Gripping", language) + " - " + Translate(text[11..], language);
        var quantity = Regex.Match(text, @"^(\d+) (rows|sec)$");
        if (quantity.Success) return quantity.Groups[1].Value + " " + Translate(quantity.Groups[2].Value, language);
        if (text.StartsWith("CONDITION: ", StringComparison.Ordinal))
            return Translate("CONDITION", language) + ": " + Translate(text[11..], language);
        if (text.Contains(" — ", StringComparison.Ordinal))
        {
            int separator = text.LastIndexOf(" — ", StringComparison.Ordinal);
            return Translate(text[..separator], language) + " — " + Translate(text[(separator + 3)..], language);
        }
        if (text.StartsWith("LEFT: ", StringComparison.Ordinal) && text.Contains("     |     RIGHT: ", StringComparison.Ordinal))
        {
            var hands = text[6..].Split("     |     RIGHT: ");
            return Translate("LEFT", language) + ": " + Translate(hands[0], language) + "     |     " +
                Translate("RIGHT", language) + ": " + Translate(hands[1], language);
        }
        var movement = Regex.Match(text, @"^(SHIFT|MOVE LOWER|MOVE UPPER) (UP|DOWN) BY (\d+)$");
        if (movement.Success) return Translate(movement.Groups[1].Value, language) + " " +
            Translate(movement.Groups[2].Value, language) + " " + Translate("BY", language) + " " + movement.Groups[3].Value;
        var boundary = Regex.Match(text, @"^MOVE SHARED BOUNDARY (UP|DOWN)(?: BY (A LITTLE|A LOT|\d+))?$");
        if (boundary.Success)
        {
            string direction = Translate(boundary.Groups[1].Value, language);
            string amount = boundary.Groups[2].Value;
            return Translate("MOVE SHARED BOUNDARY", language) + " " + direction +
                (amount.Length == 0 ? "" : " " + Translate("BY", language) + " " + Translate(amount, language));
        }
        return text;
    }

    // Split after translation: narration language can differ from the screen language.
    // Keep empty positions so an omitted clause never shifts later cues to an earlier state.
    public static string[] GetNarrationParts(string source, InterfaceLanguage language) =>
        Translate(source, language).Split(',', StringSplitOptions.TrimEntries);

    public static string[] GetNarrationParts(string source, bool gripping = true) =>
        GetNarrationParts(source, LanguagePreferences.Get(narration: true, gripping: gripping));

    public static async Task<bool> SpeakAsync(string source, bool gripping = true, CancellationToken cancellationToken = default,
        int? partIndex = null)
    {
        var language = LanguagePreferences.Get(narration: true, gripping: gripping);
        string spoken;
        if (partIndex is int index)
        {
            string[] parts = GetNarrationParts(source, language);
            if (index < 0 || index >= parts.Length) return false;
            spoken = parts[index];
        }
        else
            spoken = Translate(source, language);
        if (string.IsNullOrWhiteSpace(spoken)) return false;
        cancellationToken.ThrowIfCancellationRequested();
        string code = LanguagePreferences.Code(language);
        var locales = await TextToSpeech.Default.GetLocalesAsync();
        var locale = locales.FirstOrDefault(item => item.Language.Split('-', '_')[0].Equals(code, StringComparison.OrdinalIgnoreCase)
            || (code == "he" && item.Language.Split('-', '_')[0].Equals("iw", StringComparison.OrdinalIgnoreCase)));
        if (locale == null) return false;
        if (language == InterfaceLanguage.English)
            spoken = spoken.Replace("Large+small", "Large plus small").Replace("Large-small", "Large minus small");
        cancellationToken.ThrowIfCancellationRequested();
        await TextToSpeech.Default.SpeakAsync(spoken, new SpeechOptions { Locale = locale }, cancellationToken);
        return true;
    }

    private sealed class Original { public string Source = ""; public string Last = ""; }
    private static readonly ConditionalWeakTable<BindableObject, Original> Originals = new();

    /// <summary>Refresh a settings page without losing selections. Gameplay surfaces keep their orientation.</summary>
    public static void LocalizeSettings(Element element, bool gripping)
    {
        BindableProperty? property = element switch
        {
            Label => Label.TextProperty,
            Button => Button.TextProperty,
            ContentPage => Page.TitleProperty,
            _ => null
        };
        if (property != null && element.GetValue(property) is string current)
        {
            var original = Originals.GetValue(element, _ => new Original());
            if (current != original.Last) original.Source = current;
            original.Last = Text(original.Source, gripping);
            element.SetValue(property, original.Last);
        }
        if (element is Microsoft.Maui.IVisualTreeElement tree)
            foreach (var child in tree.GetVisualChildren().OfType<Element>())
                if (child is not GestureSample.Maui.Views.LanguageSettingsView) LocalizeSettings(child, gripping);
    }

    private const string Catalog = """
Keyboard arrow · variable length|חץ על המקלדת · אורך משתנה|Стрелка на клавиатуре · переменная длина
Keyboard arrow · fixed length|חץ על המקלדת · אורך קבוע|Стрелка на клавиатуре · фиксированная длина
Keyboard arrow · ordinal|חץ על המקלדת · Ordinal|Стрелка на клавиатуре · порядковая
Arrow Label · destination missing|Arrow Label · יעד חסר|Подпись стрелки · неизвестен конец
Arrow Label · distance missing|Arrow Label · מרחק חסר|Подпись стрелки · неизвестно расстояние
Arrow Label · start missing|Arrow Label · התחלה חסרה|Подпись стрелки · неизвестно начало
Ordinal arrow|חץ Ordinal|Порядковая стрелка
What is missing?|מה חסר בשאלה?|Что неизвестно?
Destination|יעד|Конечная точка
Starting point|נקודת התחלה|Начальная точка
The keyboard always has ten keys. Numbers wrap around modulo ten.|במקלדת תמיד עשרה קלידים. המספרים נעטפים במודולו עשר.|На клавиатуре всегда десять клавиш. Числа переходят по модулю десять.
For a ten-key keyboard, choose a distance from 1 to 10.|במקלדת של עשרה קלידים בחרו מרחק מ־1 עד 10.|Для клавиатуры из десяти клавиш выберите расстояние от 1 до 10.
Color trail is turned off for ordinal arrows.|שביל הצבע כבוי בחצי Ordinal.|Цветной след отключён для порядковых стрелок.
Crossing ten needs a maximum length of at least two.|למעבר דרך עשר נדרש אורך מרבי של לפחות 2.|Для перехода через десять нужна максимальная длина не менее двух.
Custom arrow - specific question|תרגול חצים — שאלה לחזרה|Стрелки — точное задание
Custom arrow - mixed practice|תרגול חצים מעורב|Смешанная практика стрелок
Starting point and crossing ten are available for keyboard arrows.|בחירת נקודת התחלה ומעבר דרך עשר זמינות לחצים על המקלדת.|Выбор начала и переход через десять доступны для стрелок на клавиатуре.
Custom arrow practice|תרגול חצים מותאם|Своя практика стрелок
Choose a mix or one exact question|בחירת ערבוב או שאלה מסוימת|Выбрать смесь или точное задание
Mix the questions you need, or repeat one exact question.|בחרו אילו שאלות לערבב, או חזרו על שאלה מסוימת.|Выберите типы заданий или повторяйте одно точное задание.
Mix selected question types|ערבוב סוגי שאלות|Смешать выбранные типы
One specific question|שאלה מסוימת אחת|Одно точное задание
Question type|סוג השאלה|Тип задания
Starting key|קליד התחלה|Начальная клавиша
Destination key|קליד היעד|Конечная клавиша
Distance|מרחק|Расстояние
Find the destination|מציאת היעד|Найди конечную точку
Edit practice|עריכת התרגול|Изменить практику
This practice repeats the exact question you chose.|התרגול חוזר על השאלה המסוימת שבחרת.|Практика повторяет выбранное точное задание.
Choose at least one question type.|בחרו לפחות סוג שאלה אחד.|Выберите хотя бы один тип задания.
Choose a valid question type.|בחרו סוג שאלה תקין.|Выберите допустимый тип задания.
Choose a maximum length between 1 and 10.|בחרו אורך מרבי בין 1 ל־10.|Выберите максимальную длину от 1 до 10.
Choose one question type for a specific question.|לשאלה מסוימת יש לבחור סוג שאלה אחד.|Для точного задания выберите один тип.
Choose two different keys from 1 to 20.|בחרו שני קלידים שונים מ־1 עד 20.|Выберите две разные клавиши от 1 до 20.
The destination may be to the left or right of the start.|היעד יכול להיות משמאל או מימין לנקודת ההתחלה.|Конечная точка может быть слева или справа от начальной.
Listening|צליל והקראה|Звук и озвучивание
Keyboard helpers|עזרי מקלדת|Подсказки клавиатуры
Arrow movement|תנועת החץ|Движение стрелки
What would you like to hear?|מה רוצים לשמוע?|Что вы хотите слышать?
Quiet|ללא צליל|Без звука
Key sounds|צלילי הקלידים|Звуки клавиш
Spoken numbers|הקראת מספרים|Озвучивание чисел
Sounds and numbers|צלילים ומספרים|Звуки и числа
Color trail|שביל צבע|Цветной след
Show key numbers|הצגת מספרי הקלידים|Показывать номера клавиш
The color trail is available in ordinal practice.|שביל הצבע זמין בתרגול Ordinal.|Цветной след доступен в порядковой практике.
How far can the arrow move?|עד כמה החץ יכול לנוע?|Как далеко может двигаться стрелка?
Full keyboard|כל המקלדת|Вся клавиатура
Where should the next arrow start?|מאיפה יתחיל החץ הבא?|Откуда начнётся следующая стрелка?
Continue from previous answer|המשך מהתשובה הקודמת|Продолжить с предыдущего ответа
From an edge|מאחד הקצוות|От края
Crossing ten|מעבר דרך עשר|Переход через десять
Any movement|ללא הגבלה|Любое движение
Stay within ten|בתוך העשר|В пределах десятка
Cross ten|מעבר דרך עשר|Через десять
Key sound|צליל הקלידים|Звук клавиш
Off|כבוי|Выключено
Sound and reading|צליל והקראה|Звук и озвучивание
Four consecutive successes advance to the next step.|ארבע הצלחות רצופות מעבירות לשלב הבא.|Четыре правильных ответа подряд переводят на следующий шаг.
Progression or mixed practice|התקדמות או תרגול מעורב|Постепенная или смешанная практика
Move through the steps at your own pace, or jump straight to mixed practice.|מתקדמים בשלבים בקצב שלך, או עוברים ישר לתרגול מעורב.|Проходите шаги в своём темпе или сразу переходите к смешанной практике.
Progression|התקדמות|Постепенная практика
Everything mixed|הכול מעורבב|Всё вперемешку
Continue progression|המשך במסלול|Продолжить по шагам
Mixed practice is always available. It does not change your place in the progression.|התרגול המעורב תמיד זמין ואינו משנה את המקום שלך במסלול.|Смешанная практика всегда доступна и не меняет вашу позицию в последовательности.
Start progression from the beginning|התחלת המסלול מחדש|Начать последовательность сначала
Continue from|ממשיכים מ־|Продолжить с
Next step|השלב הבא|Следующий шаг
Reset to defaults|איפוס לברירות המחדל|Сбросить настройки
Apply settings to restart the current practice.|החלת ההגדרות תתחיל מחדש את התרגול הנוכחי.|Применение настроек перезапустит текущую практику.
Arrow practice options|אפשרויות תרגול חצים|Настройки практики стрелок
Your options are remembered for each stage on this device.|האפשרויות נשמרות לכל שלב במכשיר זה.|Настройки каждого этапа сохраняются на этом устройстве.
Sound|צליל|Звук
Read key numbers|הקראת מספרי הקלידים|Озвучивать номера клавиш
Numbers on keys|מספרים על הקלידים|Номера на клавишах
Maximum arrow length|אורך חץ מרבי|Максимальная длина стрелки
No limit|ללא הגבלה|Без ограничения
Starting position|נקודת התחלה|Начальная позиция
Continue from last spot|המשך מהנקודה הקודמת|Продолжать с предыдущей позиции
Only edges|רק בקצוות|Только от краёв
Random start|התחלה אקראית|Случайное начало
Ten boundary|גבול העשר|Граница десятка
None|ללא|Нет
ToTen|עד עשר|До десяти
ThroughTen|מעבר דרך עשר|С переходом через десять
Accumulating color, starting position and ten boundary apply to arrows on the keyboard.|צבע מצטבר, נקודת התחלה וגבול העשר חלים על חצים על המקלדת.|Накопление цвета, начальная позиция и граница десятка применяются к стрелкам на клавиатуре.
Unable to start practice. Please try again.|לא ניתן להתחיל את התרגול. נסו שוב.|Не удалось начать практику. Попробуйте ещё раз.
1 · Variable length|1 · אורך משתנה|1 · Переменная длина
2 · Fixed length|2 · אורך קבוע|2 · Фиксированная длина
3 · Ordinal|3 · מיקום סודר|3 · Порядковые позиции
Variable length|אורך משתנה|Переменная длина
Fixed length|אורך קבוע|Фиксированная длина
Accumulating color|צבע מצטבר|Накопление цвета
Count to ten with reading|ספירה עד עשר עם הקראה|Счёт до десяти с озвучиванием
Count through ten with reading|ספירה מעבר לעשר עם הקראה|Счёт с переходом через десять с озвучиванием
Ordinal with key numbers|מיקום סודר עם מספרים על הקלידים|Порядковые позиции с номерами клавиш
Arrow Label|תוויות חצים|Подписи стрелок
Start and distance|נקודת התחלה ומרחק|Начало и расстояние
Find the distance|מציאת המרחק|Найди расстояние
Find the starting point|מציאת נקודת ההתחלה|Найди начальную точку
Test activities|פעילויות בדיקה|Тестовые упражнения
More practice|תרגול נוסף|Дополнительная практика
Segment length|אורך קטע|Длина отрезка
Hold the correct grip to answer|להחזיק את האחיזה הנכונה כדי לענות|Удерживать правильный хват для ответа
Grip only: skip watch-then-click|אחיזה בלבד: ללא שלב צפייה והמתנה ללחיצה|Только хват: без ожидания перед нажатием
Compare two grips|משווים שתי אחיזות|Сравниваем два хвата
Add|חבר|Прибавь
Subtract|חסר|Вычти
Part and complementary part|חלק וחלק משלים|Часть и дополняющая часть
Part inside|חלק בפנים|Часть внутри
Part outside|חלק בחוץ|Часть снаружи
Show eye / hand in all stages|הצגת עין / יד בכל השלבים|Показывать глаз / руку на всех этапах
Always show eye / hand in all stages|הצגת עין / יד באופן קבוע בכל השלבים|Всегда показывать глаз / руку на всех этапах
You can press now|אפשר ללחוץ עכשיו|Теперь можно нажимать
First the starting position|קודם המצב הראשון|Сначала исходное положение
Lift your fingers|הרימו את האצבעות|Поднимите пальцы
Watch and wait|הסתכלו והמתינו|Смотрите и ждите
Show eye / hand|הצגת עין / יד|Показать глаз / руку
Hide eye / hand|הסתרת עין / יד|Скрыть глаз / руку
MEMORIZE BOTH|זוכרים את שניהם|ЗАПОМНИ ОБА
Remember the first, then the second|זוכרים את הראשון, ואז את השני|Запоминаем первый, затем второй
Remember the first|זוכרים את הראשון|Запоминаем первый
Then the second|ואז את השני|Затем второй
Recall the first grip, then the second. Tap the title to hear the question.|משחזרים את האחיזה הראשונה ואחריה את השנייה. לחצו על הכותרת לשמיעת השאלה.|Повтори первый хват, затем второй. Нажми на заголовок, чтобы услышать задание.
No narration. Recall the first grip, then the second.|ללא הקראה. משחזרים את האחיזה הראשונה ואחריה את השנייה.|Без озвучивания. Повтори первый хват, затем второй.
Watch now|עכשיו צופים|Сейчас смотрим
Your turn|עכשיו תורך|Твоя очередь
Lift fingers|מרימים אצבעות|Подними пальцы
Language|שפה|Язык
Text language|שפת הטקסט|Язык текста
Narration language|שפת הקריינות|Язык озвучивания
Use user settings|לפי הגדרות המשתמש|Использовать настройки пользователя
Preview narration|השמעת דוגמה|Прослушать пример
Narration preview|זוהי דוגמה לקריינות בעברית|Это пример озвучивания на русском языке
No voice available for this language. Install a voice in your device settings.|אין קול זמין בשפה זו. יש להתקין קול בהגדרות המכשיר.|Нет голоса для этого языка. Установите голос в настройках устройства.
Narration is unavailable on this device.|הקריינות אינה זמינה במכשיר זה.|Озвучивание недоступно на этом устройстве.
USER SETTINGS|הגדרות משתמש|НАСТРОЙКИ ПОЛЬЗОВАТЕЛЯ
Switch User|החלפת משתמש|Сменить пользователя
Numeric keyboard|מקלדת מספרים|Цифровая клавиатура
Numeric keyboard for current user|מקלדת מספרים למשתמש הנוכחי|Цифровая клавиатура текущего пользователя
Stage default|ברירת המחדל של השלב|По умолчанию для этапа
App keypad|מקלדת היישום|Клавиатура приложения
System keyboard|מקלדת המערכת|Системная клавиатура
Add New User|הוספת משתמש חדש|Добавить пользователя
Force Re-sync|סנכרון מחדש|Повторная синхронизация
Sync|סנכרון|Синхронизация
Gripping|אחיזות|Хваты
Gripping settings|הגדרות אחיזות|Настройки хватов
Control Categories|קטגוריות פעילויות|Категории занятий
Choose an activity to begin|בחרו פעילות כדי להתחיל|Выберите занятие
Choose a gripping activity|בחרו פעילות אחיזה|Выберите упражнение на хваты
Grip practice|תרגול אחיזות|Практика хватов
Build control, memory, and coordination|פיתוח שליטה, זיכרון ותיאום|Развивайте контроль, память и координацию
Grip language|שפת האחיזות|Язык хватов
Learn the signs and rules for changing a grip|לימוד הסימנים והכללים לשינוי אחיזה|Изучайте знаки и правила изменения хвата
Copy a grip|העתקת אחיזה|Повторить хват
Remember a grip|זכירת אחיזה|Запомнить хват
Remember two grips|זכירת שתי אחיזות|Запомнить два хвата
Remember grip changes|זכירת שינויי אחיזה|Запомнить изменения хватов
Remember grip changes — Easy|זכירת שינויי אחיזה — קל|Запомнить изменения хватов — легко
Remember grip changes — Hard|זכירת שינויי אחיזה — קשה|Запомнить изменения хватов — сложно
Easy grip changes settings|הגדרות שינויי אחיזה — קל|Настройки изменений хватов — легко
Hard grip changes settings|הגדרות שינויי אחיזה — קשה|Настройки изменений хватов — сложно
Start practice|התחלת התרגול|Начать практику
Apply and restart practice|החלה והתחלת התרגול מחדש|Применить и перезапустить практику
Keep left hand at the bottom|השארת יד שמאל בתחתית|Держать левую руку внизу
Prefer the same left-hand size between questions. Move the right hand when needed.|העדפת גודל קבוע ליד שמאל בין שאלות. הזזת יד ימין לפי הצורך.|По возможности сохранять размер левой руки между вопросами. Перемещать правую при необходимости.
Combination exposure time|משך הצגת כל מצב בשילוב|Время показа каждой позиции
Changing the number-line height requires restarting practice. Other settings can be applied without restarting.|שינוי גובה ציר המספרים דורש התחלת התרגול מחדש. אפשר להחיל הגדרות אחרות ללא התחלה מחדש.|Изменение высоты числовой прямой требует перезапуска практики. Остальные настройки можно применить без перезапуска.
Learn the arrows|לימוד החצים|Изучить стрелки
Arrows from bottom|חצים מלמטה|Стрелки снизу
Arrow combinations|שילובי חצים|Комбинации стрелок
Synchronous process test|בדיקת תהליך מסונכרן|Проверка синхронного процесса
Arrow design lab|מעבדת עיצוב חצים|Настройка вида стрелок
Stage 5.1 settings|הגדרות שלב 5.1|Настройки этапа 5.1
Transformations|שינויים|Преобразования
Commutativity|חוק החילוף|Переместительное свойство
Synchronous exchange|החלפה מסונכרנת|Синхронный обмен
Move shared boundary up/down|הזזת הגבול המשותף למעלה או למטה|Сдвиг общей границы вверх или вниз
Keep the whole; shift where the parts meet|שמרו על השלם; הזיזו את נקודת המפגש|Сохранить целое; сдвинуть границу частей
Resize upper|שינוי גודל האחיזה העליונה|Изменить верхний хват
Change the upper hand|שינוי היד העליונה|Изменить верхнюю руку
Resize attached|שינוי גודל תוך שמירה על חיבור|Изменить размер с сохранением соединения
Keep the other hand connected|שמרו על חיבור היד השנייה|Сохранить соединение с другой рукой
Change lower by one|שינוי התחתון באחד|Изменить нижний на единицу
Change upper by one|שינוי העליון באחד|Изменить верхний на единицу
Large +/− small|גדול ועוד או פחות קטן|Большое плюс или минус малое
Large-small, to Large+small — or the reverse|מגדול פחות קטן לגדול ועוד קטן — או להפך|От большого минус малое к большому плюс малое — или наоборот
Change subtraction by one|שינוי המחסר באחד|Изменить вычитаемое на единицу
Attach small part to other edge|הצמדת החלק הקטן לקצה השני|Присоединить малую часть к другому краю
Move the same part across the whole|העבירו את אותו החלק לצד השני של השלם|Перенести ту же часть на другую сторону целого
Parts and halves|חלקים וחצאים|Части и половины
Complementary parts|חלקים משלימים|Дополняющие части
Keep the whole; change the part|שמרו על השלם; שנו את החלק|Сохранить целое; изменить часть
Part outside, part inside|חלק בחוץ, חלק בפנים|Часть снаружи, часть внутри
A linked pair with complementary parts|זוג מחובר עם חלקים משלימים|Связанная пара с дополняющими частями
Split a jump|פיצול קפיצה|Разделить прыжок
One jump becomes two|קפיצה אחת הופכת לשתיים|Один прыжок становится двумя
One half, other half|חצי אחד, החצי השני|Одна половина, другая половина
Full + one half → full + the other half|שלם ועוד חצי אחד ← שלם ועוד החצי השני|Целое + одна половина → целое + другая половина
Around half|סביב החצי|Около половины
Whole + half → one above or below half|שלם ועוד חצי ← אחד מעל או מתחת לחצי|Целое + половина → на единицу больше или меньше половины
Half of half|חצי של חצי|Половина половины
Continue from half to quarter|המשיכו מחצי לרבע|Перейти от половины к четверти
Relative size|גודל יחסי|Относительный размер
A little smaller|קצת יותר קטן|Немного меньше
Much smaller|הרבה יותר קטן|Намного меньше
A little bigger|קצת יותר גדול|Немного больше
Much bigger|הרבה יותר גדול|Намного больше
One row|שורה אחת|Один ряд
Several rows|כמה שורות|Несколько рядов
Choose at least one exercise.|בחרו לפחות תרגיל אחד.|Выберите хотя бы одно упражнение.
Start Stage 5.1|התחלת שלב 5.1|Начать этап 5.1
Apply and restart Stage 5.1|החלה והתחלה מחדש של שלב 5.1|Применить и перезапустить этап 5.1
Apply without restarting|החלה ללא התחלה מחדש|Применить без перезапуска
⚙  CUSTOM PRACTICE|⚙  תרגול מותאם אישית|⚙  НАСТРОЙКА ПРАКТИКИ
What do you want to work on?|מה תרצו לתרגל?|Что вы хотите потренировать?
Mix any exercises. Reopen this from the smiley row.|שלבו תרגילים כרצונכם. ניתן לפתוח שוב משורת הסמיילים.|Сочетайте любые упражнения. Откройте настройки снова из строки смайликов.
Select all|בחירת הכל|Выбрать всё
Clear|ניקוי|Снять выбор
Anchor hand|יד עוגן|Опорная рука
Left|שמאל|Левая
Right|ימין|Правая
Both|שתיהן|Обе
Raise the whole|העלאת השלם|Поднять целое
Raise both equally|העלאת שניהם במידה שווה|Поднять оба одинаково
Raise the anchor only|העלאת העוגן בלבד|Поднять только опору
7−3 → 9−5: keep the difference|7−3 → 9−5: שמרו על ההפרש|7−3 → 9−5: сохранить разность
7−3 → 9−3: keep the subtracted part's size|7−3 → 9−3: שמרו על גודל החלק המחוסר|7−3 → 9−3: сохранить размер вычитаемой части
Practice feel|אופן התרגול|Параметры практики
Quick movement animations|הנפשות תנועה מהירות|Быстрые анимации движения
Normal question color, no tutorial pause|צבע שאלה רגיל, ללא הפסקת הדרכה|Обычный цвет вопроса, без обучающей паузы
Vary interval sizes|שינוי גדלי המרווחים|Менять размеры интервалов
Keep the bottom anchor while changing proportions|שמרו על העוגן התחתון בעת שינוי היחסים|Сохранять нижнюю опору при изменении пропорций
Read instruction aloud|הקראת ההוראה|Читать инструкцию вслух
Speak the transformation shown above the keyboard|הקראת השינוי המוצג מעל המקלדת|Озвучивать преобразование над клавиатурой
Show first state only|הצגת המצב הראשון בלבד|Показывать только первое состояние
Show the first state, then fade it without showing the second|הצגת המצב הראשון והעלמתו ללא הצגת השני|Показать первое состояние и скрыть его, не показывая второе
Magnitude vocabulary|ניסוח גודל השינוי|Формулировка величины
Magnitude wording|ניסוח גודל השינוי|Формулировка величины
Intuitive|אינטואיטיבי|Интуитивно
By a little / by a lot|בקצת / בהרבה|Немного / намного
By exact number|במספר מדויק|Точное число
Number-line height|גובה ציר המספרים|Высота числовой прямой
Show our grip hand when tapping ?|הדגמה עם היד שלנו בלחיצה על סימן השאלה|Показывать нашу руку при нажатии ?
How long is the left grip compared with the right grip?|מה אורך האחיזה משמאל ביחס לזו שמימין?|Какова длина левого хвата по сравнению с правым?
How long is the right grip compared with the left grip?|מה אורך האחיזה מימין ביחס לזו שמשמאל?|Какова длина правого хвата по сравнению с левым?
Are the lengths close together or far apart?|האורכים קרובים או רחוקים זה מזה?|Длины близки или сильно отличаются?
Where is the smaller grip aligned with the larger grip?|איפה האחיזה הקטנה מוצמדת ביחס לגדולה?|Где меньший хват выровнен относительно большего?
Options for young children|אפשרויות לילדים קטנים|Настройки для малышей
For Copy a grip, Remember a grip and Remember two grips. Changes apply to the next exercise.|להעתקת אחיזה, זכירת אחיזה וזכירת שתי אחיזות. השינויים יחולו מהתרגיל הבא.|Для копирования и запоминания одного или двух хватов. Изменения со следующего задания.
Always start at the bottom|תמיד מתחילים מלמטה|Всегда начинать снизу
Keep the base finger visible during recall|להשאיר את אצבע הבסיס גלויה בזמן הזכירה|Показывать нижний палец при запоминании
Color pressed keys: green for correct, red for retry|צבעי הקלידים: ירוק לנכון, אדום לניסיון נוסף|Цвет нажатых клавиш: зелёный — верно, красный — повторить
Start answering immediately with a faint grip guide|להתחיל מיד עם אחיזה מנחה חלשה ברקע|Отвечать сразу с бледной подсказкой хвата
Memorize each position|זכירת כל מיקום|Время запоминания позиции
No exercises selected|לא נבחרו תרגילים|Упражнения не выбраны
exercises selected|תרגילים נבחרו|упражнений выбрано
rows|שורות|рядов
sec|שניות|сек
Restart required|נדרשת התחלה מחדש|Требуется перезапуск
Changing the number-line height requires ‘Apply and restart Stage 5.1’. Other settings can be applied without restarting.|שינוי גובה ציר המספרים דורש החלה והתחלה מחדש של שלב 5.1. אפשר להחיל הגדרות אחרות ללא התחלה מחדש.|Изменение высоты числовой прямой требует перезапуска этапа 5.1. Остальные настройки можно применить без перезапуска.
OK|אישור|ОК
RESIZE — KEEP ATTACHED|שנו גודל — שמרו על חיבור|ИЗМЕНИТЕ РАЗМЕР — СОХРАНИТЕ СОЕДИНЕНИЕ
Large-small, to Large+small|גדול פחות קטן, לגדול ועוד קטן|Большое минус малое, к большому плюс малое
Large+small, to Large-small|גדול ועוד קטן, לגדול פחות קטן|Большое плюс малое, к большому минус малое
ATTACH SMALL PART TO THE OTHER EDGE|הצמידו את החלק הקטן לקצה השני|ПРИСОЕДИНИТЕ МАЛУЮ ЧАСТЬ К ДРУГОМУ КРАЮ
PART INSIDE, PART OUTSIDE|חלק בפנים, חלק בחוץ|ЧАСТЬ ВНУТРИ, ЧАСТЬ СНАРУЖИ
SPLIT THE JUMP|פצלו את הקפיצה|РАЗДЕЛИТЕ ПРЫЖОК
A LITTLE MORE THAN HALF|קצת יותר מחצי|НЕМНОГО БОЛЬШЕ ПОЛОВИНЫ
A LITTLE LESS THAN HALF|קצת פחות מחצי|НЕМНОГО МЕНЬШЕ ПОЛОВИНЫ
SUBTRACT ONE MORE|חסרו עוד אחד|ВЫЧТИТЕ НА ЕДИНИЦУ БОЛЬШЕ
SUBTRACT ONE LESS|חסרו אחד פחות|ВЫЧТИТЕ НА ЕДИНИЦУ МЕНЬШЕ
INCREASE LOWER BY ONE|הגדילו את התחתון באחד|УВЕЛИЧЬТЕ НИЖНИЙ НА ЕДИНИЦУ
INCREASE UPPER BY ONE|הגדילו את העליון באחד|УВЕЛИЧЬТЕ ВЕРХНИЙ НА ЕДИНИЦУ
DECREASE LOWER BY ONE|הקטינו את התחתון באחד|УМЕНЬШИТЕ НИЖНИЙ НА ЕДИНИЦУ
DECREASE UPPER BY ONE|הקטינו את העליון באחד|УМЕНЬШИТЕ ВЕРХНИЙ НА ЕДИНИЦУ
MOVE SHARED BOUNDARY|הזיזו את הגבול המשותף|СДВИНЬТЕ ОБЩУЮ ГРАНИЦУ
UP|למעלה|ВВЕРХ
DOWN|למטה|ВНИЗ
BY|ב־|НА
A LITTLE|מעט|НЕМНОГО
A LOT|הרבה|МНОГО
Check|בדיקה|Проверить
Next|הבא|Далее
Prev|הקודם|Назад
Help|עזרה|Помощь
Show Prev|הצגת הקודם|Показать предыдущее
Answer time|זמן תשובה|Время ответа
COPY|העתיקו|ПОВТОРИТЕ
COPY TO OTHER HAND|העתיקו ליד השנייה|ПОВТОРИТЕ ДРУГОЙ РУКОЙ
HOLD|החזיקו|УДЕРЖИВАЙТЕ
SHIFT|הזיזו את האחיזה|СДВИНЬТЕ ХВАТ
MOVE LOWER|הזיזו את התחתון|СДВИНЬТЕ НИЖНИЙ
MOVE UPPER|הזיזו את העליון|СДВИНЬТЕ ВЕРХНИЙ
LEFT|שמאל|СЛЕВА
RIGHT|ימין|СПРАВА
CONDITION|תנאי|УСЛОВИЕ
RESIZE — KEEP LOWER KEYS DOWN|שנו גודל — השאירו את הקלידים התחתונים לחוצים|ИЗМЕНИТЕ РАЗМЕР — УДЕРЖИВАЙТЕ НИЖНИЕ КЛАВИШИ
MOVE LEFT HAND ABOVE RIGHT|העבירו את יד שמאל מעל יד ימין|ПЕРЕМЕСТИТЕ ЛЕВУЮ РУКУ ВЫШЕ ПРАВОЙ
MOVE RIGHT HAND ABOVE LEFT|העבירו את יד ימין מעל יד שמאל|ПЕРЕМЕСТИТЕ ПРАВУЮ РУКУ ВЫШЕ ЛЕВОЙ
LEFT HAND|יד שמאל|ЛЕВАЯ РУКА
RIGHT HAND|יד ימין|ПРАВАЯ РУКА
BOTH HANDS|שתי הידיים|ОБЕ РУКИ
Synchronous two-hand process|תהליך מסונכרן בשתי ידיים|Синхронный процесс двумя руками
Sync is already running.|הסנכרון כבר פועל.|Синхронизация уже выполняется.
No active user to sync.|אין משתמש פעיל לסנכרון.|Нет активного пользователя для синхронизации.
Exercise navigation|ניווט בין שאלות|Навигация по заданиям
Stage settings|הגדרות השלב|Настройки этапа
Advance only with <|מעבר לשאלה הבאה רק בלחיצה על <|Переход вперёд только по кнопке <
Advance only with the next button|מעבר לשאלה הבאה רק בלחיצה על כפתור ההמשך|Переход вперёд только по кнопке следующего задания
Wait after solving. Previous exercises are read-only; the double arrow returns to the current exercise.|המתנה אחרי הפתרון. שאלות קודמות מוצגות לצפייה בלבד; החץ הכפול מחזיר לשאלה הנוכחית.|Ожидать после решения. Предыдущие задания доступны только для просмотра; двойная стрелка возвращает к текущему.
Wait after solving. Previous exercises are read-only; << returns to the current exercise.|המתנה אחרי הפתרון. שאלות קודמות מוצגות לצפייה בלבד; << מחזיר לשאלה הנוכחית.|Ожидать после решения. Предыдущие задания доступны только для просмотра; << возвращает к текущему.
Previous solution|פתרון קודם|Предыдущее решение
Next exercise|השאלה הבאה|Следующее задание
Return to current exercise|חזרה לשאלה הנוכחית|Вернуться к текущему заданию
Solved · press < to continue|נפתר · להמשך לחצו <|Решено · нажмите < для продолжения
Current exercise|השאלה הנוכחית|Текущее задание
Previous solution · read only|פתרון קודם · לצפייה בלבד|Предыдущее решение · только просмотр
Second keyboard|המקלדת השנייה|Вторая клавиатура
Solution|פתרון|Решение
One difference|הבדל יחיד|Одно различие
Standard colors|צבעים רגילים|Обычные цвета
""";
}
