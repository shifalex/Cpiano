using GestureSample.Views.Tests;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestureSample.Maui.Models
{
    internal class DecompositionGamePlay : PPWGamePlay
    {
        private int _level=2; 
        private int _streakCorrect = 0; 
        private int _streakWrong = 0;
        private readonly int CORRECT_TO_LEVEL_UP = 20, WRONG_TO_LEVEL_DOWN = 5;
        
        private Label _lblStats;
        private Picker _pkrLevel;


        public string StatsString
        {
            get { return string.Format("Correct in this Level:{0} (reach {1} to level up)\nWrong in a row: {2} (reach {3} and you level down)", 
                _streakCorrect,CORRECT_TO_LEVEL_UP,
                _streakWrong,WRONG_TO_LEVEL_DOWN); 
            }
        }

        public int Level { get { return _level; } set { _level = value; } }

        public DecompositionGamePlay(SimpleViewCellsPage view, Label lblStats, Picker pkrLevel) : base(view, null)
        {
            _lblStats = lblStats;
            _pkrLevel = pkrLevel;
            /*
            _lblStats.BindingContext = this;
            _lblStats.SetBinding(Label.TextProperty, nameof(StatsString));*/
            _pkrLevel.BindingContext = this;
            _pkrLevel.SetBinding(Picker.SelectedItemProperty, nameof(Level));

            _lblStats.Text = StatsString;

            UpdateLevelStats();
        }

        public override bool Check()
        {
            bool check = base.Check();
            if (check) { _streakCorrect++; } else { _streakWrong++; }
            
                       
            if(_streakWrong >= WRONG_TO_LEVEL_DOWN)
            {
                _level--; 
                UpdateLevelStats();
            }
            else if (_streakCorrect >= CORRECT_TO_LEVEL_UP)
            {
                _level++; 
                UpdateLevelStats();
            }

            _lblStats.Text = StatsString;
            return check;
        }

        protected override int[] Factors
        {
            get
            {
                if (_level == 1) { return base.Factors; }

                int[] factors = new int[3];
                Random r = new();
                if (Sum != addend1 + addend2)
                    _streakWrong++;//you moved next without solving. TODO: what happens if it downs your level?
                return FactorsThroughTen;


            }
        }

        private void UpdateLevelStats()
        {
            _streakCorrect = 0;
            _streakWrong = 0;
            switch (_level)
            {
                case 0:
                    _status = Statement.Lose; 
                    _level = 2;
                    break;
                case 1:
                    Config.MinSum = 10; Config.MaxSum = 10; Config.MinAddend = 0; Config.MaxAddend = 10;
                    break;
                case 2:
                    Config.MinSum = 0; Config.MaxSum = 20; Config.MinAddend = 0; Config.MaxAddend = 20;
                    break;
                case 3:
                    Config.MinSum = 0; Config.MaxSum = 100; Config.MinAddend = 0; Config.MaxAddend = 100;
                    break;
                case 4:
                    _status= Statement.Win;
                    _level = 2;
                    break;
                default: _level = 2; break;
            }

            _pkrLevel.SelectedItem = Level;
        }

    }
}
