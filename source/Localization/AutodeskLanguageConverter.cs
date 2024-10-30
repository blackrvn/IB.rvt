using System.Globalization;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI;


namespace SelectSimilar.Views.Converters
{
    public class AutodeskLanguageConverter
    {
        public CultureInfo Convert(LanguageType autodeskLanguage)
        {
            switch (autodeskLanguage)
            {
                case LanguageType.German:
                    return CultureInfo.GetCultureInfo("de-DE");

                case LanguageType.English_USA:
                    return CultureInfo.GetCultureInfo("en-US");

                case LanguageType.English_GB:
                    return CultureInfo.GetCultureInfo("en-US");

                case LanguageType.French:
                    return CultureInfo.GetCultureInfo("fr-FR");

                case LanguageType.Italian:
                    return CultureInfo.GetCultureInfo("it-IT");

                default:
                    return CultureInfo.GetCultureInfo("en-US");
            }
        }

        public LanguageType ConvertBack(CultureInfo culture)
        {
            switch (culture.Name)
            {
                case "de-DE":
                    return LanguageType.German;

                case "en-US":
                    return LanguageType.English_USA;

                case "en-GB":
                    return LanguageType.English_USA;

                case "fr-FR":
                    return LanguageType.French;

                case "it-IT":
                    return LanguageType.Italian;

                default:
                    return LanguageType.English_USA;
            }
        }
    }
}
