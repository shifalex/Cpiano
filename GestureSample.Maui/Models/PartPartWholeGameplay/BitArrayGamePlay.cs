using GestureSample.Views.Tests;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestureSample.Maui.Models
{
    internal class BitArrayGamePlay : PPWGamePlay
    {
        public bool[] bitArrayQuestion;
        public ArrayQuestionTypes arrayQuestionType;
        public BitArrayGameType bitArrayGameType;
        public BitArrayGamePlay(GameType gameType, SimpleViewCellsPage view, GameConfig config) : base(gameType, view, config)
        {
            arrayQuestionType = config.ArrayQuestionTypes;
            bitArrayGameType = config.BitArrayGameType;

        }

        public override bool Check(PianoKeyboard pianoKeyboard)
        {
            bool result = Check(pianoKeyboard.ToBitArray());
            _status = result?Statement.True:Statement.False;
            _view.UpdateView();
            return result;
        }

        public bool Check(bool[] bitArrayAnswer)
        {
            return bitArrayGameType switch
            {
                BitArrayGameType.Copy => this.Equals(bitArrayAnswer),
                //BitArrayGameType.Reorder => this.Equals(),
                BitArrayGameType.Quantity => this.SumEquals(bitArrayAnswer),
                //BitArrayGameType.SerializeWithArrow => this.Equals(),
                _ => false
            };
        }

        public override void GenerateExercise()
        {
           _view.UpdateView(true);
        }

        public bool Equals(bool[] bitArrayAnswer)
        {
            //TODO? Through Exceptions
            //if(bitArrayAnswer==null || bitArrayQuestion==null|| bitArrayAnswer.Length!= bitArrayQuestion.Length) return false;
            for(int i = 0; i < bitArrayAnswer.Length ; i++)
                if(bitArrayAnswer[i]!= bitArrayQuestion[i]) return false;
            return true;
        }
        public bool SumEquals(bool[] bitArrayAnswer)
        {
            int s1 = 0, s2 = 0;
            for (int i = 0; i < bitArrayAnswer.Length; i++)
               { s1 += bitArrayQuestion[i] ? 1 : 0; s2 += bitArrayAnswer[i] ? 1 : 0; }
            return s1==s2;
        }
    }
}
