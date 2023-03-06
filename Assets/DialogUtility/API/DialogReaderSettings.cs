using System;

namespace DialogUtilitySpruce.API
{
    public static class DialogReaderSettings
    {
        public const string DefaultLanguage = "English";
        
        public static void Init(string language, LocalisationResource resource)
        {
            Language = language;
            CharacterLocalisation = resource;
            OnInit?.Invoke(Language);
        }

        public static Action<string> OnInit;
        public static string Language;
        public static LocalisationResource CharacterLocalisation;
    }
}