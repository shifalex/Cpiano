using GestureSample.Maui.Data;
using GestureSample.Maui.Data.SQLite;
using Microsoft.Maui.Controls;
using GestureSample.Maui;

namespace GestureSample.Views
{
    internal static class ShowDataRoutingHelper
    {
        public static bool ShouldUseKeyboardData(GameConfig? config)
        {
            return config?.KeyboardConfig != null &&
                   !config.KeyboardConfig.KeyboardOnlyForHelp;
        }

        public static Page CreatePageForGame(Game? game, bool forTeacher = false)
        {
            if (game != null && ShouldUseKeyboardData(game.Config))
                return new ShowDataXamlKeyboard(game.Id);

            return new ShowDataXaml(forTeacher, game?.Id);
        }

        public static async Task<Page> CreatePageForGameAsync(Guid gameId, bool forTeacher = false)
        {
            GameRepository gameRepository = ServiceHelper.GetService<GameRepository>();
            Game? game = await gameRepository.GetByIdAsync(gameId);
            return CreatePageForGame(game, forTeacher);
        }
    }
}
