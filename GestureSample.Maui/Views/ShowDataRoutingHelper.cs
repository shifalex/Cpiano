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

        public static Page CreateChooserPage(Guid? selectedGameId = null, bool forTeacher = false)
        {
            return new ShowDataChooserPage(forTeacher, selectedGameId);
        }

        public static Page CreatePageForGame(Game? game, bool forTeacher = false, bool showSelectors = false, bool sortNewestFirst = true, User? dataUser = null)
        {
            if (game != null && ShouldUseKeyboardData(game.Config))
                return new ShowDataXamlKeyboard(game.Id, showSelectors, sortNewestFirst, forTeacher, dataUser);

            return new ShowDataXaml(forTeacher, game?.Id, showSelectors, sortNewestFirst);
        }

        public static async Task<Page> CreatePageForGameAsync(Guid gameId, bool forTeacher = false, bool showSelectors = false, bool sortNewestFirst = true)
        {
            GameRepository gameRepository = ServiceHelper.GetService<GameRepository>();
            Game? game = await gameRepository.GetByIdAsync(gameId);
            return CreatePageForGame(game, forTeacher, showSelectors, sortNewestFirst);
        }
    }
}
