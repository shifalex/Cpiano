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

        public SoundService(IAudioManager audioManager)
        {
            _audioManager = audioManager;
        }

        /// <summary>
        /// Plays the spoken number 1–10 from an embedded audio file.
        /// Files should be named "1.m4a", "2.m4a", etc. in Resources/Raw.
        /// </summary>
        public async Task PlayNumberAsync(int number)
        {
            if (number < 1 || number > 10)
                return;

            if (!_players.TryGetValue(number, out var player))
            {
                var fileName = $"EN/{number}.m4a"; // or .wav/.mp3 if you prefer

                // File must be in Resources/Raw with Build Action: MauiAsset
                var stream = await FileSystem.OpenAppPackageFileAsync(fileName);
                player = _audioManager.CreatePlayer(stream);
                Console.WriteLine("playing file...");
                _players[number] = player;
            }

            // restart from beginning each time
            player.Stop();

            Console.WriteLine("playing Stopped...");
            player.Play();

            Console.WriteLine("playing Started...");
        }
    }
}
