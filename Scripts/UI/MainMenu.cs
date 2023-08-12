using Godot;
using System;

namespace KayEth.Mancala.UI
{
    public class MainMenu : Control
    {
        HBoxContainer _mainContent;
        PlayGame _playGame;

        Godot.Collections.Array _languages;
        
        int _currentLanguage = 0;

        public override void _Ready()
        {
            _mainContent = GetNode<HBoxContainer>("MainContent");
            _playGame = GetNode<PlayGame>("PlayGame");

            _languages = TranslationServer.GetLoadedLocales();
            var currentlocale = TranslationServer.GetLocale();

            int ind = 0;
            foreach (string locale in _languages)
            {
                if (currentlocale == locale)
                {
                    _currentLanguage = ind;
                    break;
                }
                ind++;
            }
        }

        public async void OnPlayButtonPressed()
        {
            _mainContent.Visible = false;
            _playGame.Visible = true;
            await ToSignal(_playGame, nameof(PlayGame.Finished));
            _playGame.Visible = false;
            _mainContent.Visible = true;
        }

        public void OnPlayMultiButtonPressed()
        {
            // GetTree().ChangeScene("res://Resources/Scenes/Game.scn");
        }

        public void OnExitButtonPressed()
        {
            GetTree().Quit();
        }

        public void OnButtonDown()
        {
            GetNode<AudioStreamPlayer>("ButtonClick").Play();
        }

        public void OnChangeLanguageButtonPressed()
        {
            _currentLanguage++;
            if (_currentLanguage == _languages.Count)
                _currentLanguage = 0;
            
            TranslationServer.SetLocale(_languages[_currentLanguage] as string);
        }
    }
}
