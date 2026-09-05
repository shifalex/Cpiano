using GestureSample.Views.Tests;

namespace GestureSample.Maui.Models
{
    internal class DecompositionGamePlay : PPWGamePlay
    {
        private int _level = 2;
        private int _streakCorrect = 0;
        private int _streakWrong = 0;
        private readonly int CORRECT_TO_LEVEL_UP = 20, WRONG_TO_LEVEL_DOWN = 5;

        private Label? _lblStats;
        private Picker? _pkrLevel;


        public string StatsString
        {
            get
            {
                return string.Format("Correct in this Level:{0} (reach {1} to level up)\nWrong in a row: {2} (reach {3} and you level down)",
                _streakCorrect, CORRECT_TO_LEVEL_UP,
                _streakWrong, WRONG_TO_LEVEL_DOWN);
            }
        }

        public DecompositionGamePlay(GameConfig config) : base(config)
        {
        }

        public void AttachDashboard(Label lblStats, Picker pkrLevel)
        {
            _lblStats = lblStats;
            _pkrLevel = pkrLevel;
            _pkrLevel.BindingContext = this;

            if (_pkrLevel.SelectedIndex < 0)
                _pkrLevel.SelectedIndex = _level - 1;

            UpdateLevelStats();
            _lblStats.Text = StatsString;
        }

        public override async Task<ExerciseCheckResult> EvaluateAsync()
        {
            ExerciseCheckResult result = await base.EvaluateAsync();
            if (result.IsCorrect) { _streakCorrect++; } else { _streakWrong++; }


            if (_streakWrong >= WRONG_TO_LEVEL_DOWN)
            {
                _level--;
                UpdateLevelStats();
            }
            else if (_streakCorrect >= CORRECT_TO_LEVEL_UP)
            {
                _level++;
                UpdateLevelStats();
            }

            if (_lblStats != null)
                _lblStats.Text = StatsString;
            return result;
        }

        public override async Task<bool> CheckAsync()
        {
            return (await EvaluateAsync()).IsCorrect;
        }

        protected override int[] Factors
        {
            get
            {
                if (_level == 1) { return base.Factors; }

                if (Sum != addend1 + addend2)
                    _streakWrong++;//you moved next without solving. TODO: what happens if it downs your level?
                return base.Factors;
                //return FactorsThroughTen;
            }
        }
        private bool _levelChangedByUser = false;
        public async Task<ExerciseGenerationResult> OnLevelSelectedAsync(int selectedIndex)
        {
            if (_pkrLevel == null)
                throw new InvalidOperationException("Decomposition dashboard is not attached.");

            _level = selectedIndex + 1;
            _levelChangedByUser = true;
            _streakCorrect = 0; _streakWrong = 0;
            UpdateLevelStats();
            _levelChangedByUser = false;
            return await GenerateExerciseAsync();
        }

        private void UpdateLevelStats()
        {
            _streakCorrect = 0;
            _streakWrong = 0;
            switch (_level)
            {
                case 0:
                    //_status = Statement.Lose;
                    _level = 2;
                    break;
                case 1:
                    Config.MinSum = 0; Config.MaxSum = 10; Config.MinAddend = 0; Config.MaxAddend = 10; Config.OnlyThrougTen = false; Config.MinAddend2 = NAN; Config.MaxAddend2 = NAN;
                    break;
                case 2:
                    Config.MinSum = 0; Config.MaxSum = 20; Config.MinAddend = 0; Config.MaxAddend = 20; Config.OnlyThrougTen = true; Config.MinAddend2 = NAN; Config.MaxAddend2 = NAN;
                    break;
                case 3:
                    Config.MinSum = 0; Config.MaxSum = 100; Config.MinAddend = 0; Config.MaxAddend = 100; Config.OnlyThrougTen = true; Config.MinAddend2 = 1; Config.MaxAddend2 = 9;
                    break;
                case 4:
                    Config.MinSum = 0; Config.MaxSum = 100; Config.MinAddend = 0; Config.MaxAddend = 100; Config.OnlyThrougTen = true; Config.MinAddend2 = NAN; Config.MaxAddend2 = NAN;
                    break;
                case 5:
                    //_status = Statement.Win;
                    _level = 2;
                    break;
                default: _level = 2; break;
            }

            if (_pkrLevel == null)
                return;

            if (!_levelChangedByUser)
                _pkrLevel.SelectedIndex = _level - 1;

            string selectedItem = _pkrLevel.Items[_pkrLevel.SelectedIndex];
            Console.WriteLine("Selected Item", $"You selected: {selectedItem}", "OK");
            base.GeneratePossibleTriadsSet();
        }

    }
}
