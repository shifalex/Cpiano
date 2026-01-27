using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Plugin.Maui.Audio;

namespace GestureSample.Maui.Models
{
    public class SoundService
    {
        private readonly IAudioManager _audioManager;
        private readonly Dictionary<int, IAudioPlayer> _players = new();
        private readonly Dictionary<int, string> _playerFileNames = new();
        private IAudioPlayer Player = null;
        private int _currentNumber = -1;
        public int Mode { get; set; } = 1; //1-play each, 2-play till stop, 3-change what plays

        public SoundService(IAudioManager audioManager)
        {
            _audioManager = audioManager;
            _players.Clear();
        }

        /// <summary>
        /// Plays the spoken number 1–10 from an embedded audio file.
        /// Files should be named "1.m4a", "2.m4a", etc. in Resources/Raw/EN.
        /// </summary>
        public async Task PlayNumberAsync(int number)
        {

            await PlayCustomVoiceAsync(number, number, "EN", "m4a");
        }

        /// <summary>
        /// Plays the spoken sound from an embedded audio file.
        /// Files should be named "1.wav", "2.wav", etc. in Resources/Raw/Voice.
        /// </summary>
        public async Task PlayVoiceAsync(int number)
        {
          await PlayCustomVoiceAsync(number, number);
        }

       
        public void StopVoiceAsync(int number)
        {
            if(_players.ContainsKey(number))
                _players[number].Stop();
           }

        public void StopAllVoices()
        {

            //Console.WriteLine("playing Stopped1");
            foreach (var player in _players.Values)
            {
                if (player != null)
                    player.Stop();
            }

        } 
        
        public async Task PlayCustomVoiceAsync(int keyNumber, int voiceNumber, string folderName="Voice", string fileType = "wav")
        {
            var fileName = string.Format("{0}/{1}.{2}", folderName, voiceNumber, fileType); // or .wav/.mp3 if you prefer

            if (!_players.TryGetValue(keyNumber, out var Player) || _playerFileNames[keyNumber] != fileName)
            {
                _currentNumber = keyNumber;
                // File must be in Resources/Raw with Build Action: MauiAsset
                var stream = await FileSystem.OpenAppPackageFileAsync(fileName);
                Player = _audioManager.CreatePlayer(stream);
               // Console.WriteLine("playing file...");
                _players[keyNumber] = Player;
                _playerFileNames[keyNumber] = fileName;
            }

            // restart from beginning each time
            Player.Stop();

            //Console.WriteLine("playing Stopped...");
            Player.Play();

            //Console.WriteLine("playing Started...");
        }
    }
}
