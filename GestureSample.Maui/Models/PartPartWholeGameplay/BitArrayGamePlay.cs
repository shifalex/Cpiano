using GestureSample.Views.Tests;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GestureSample.Maui.Models
{
    internal class BitArrayGamePlay : PPWGamePlay
    {
        public bool[] bitArrayQuestion;
        public bool[] bitArrayQuestion2 ;
        public UIQuestionType arrayQuestionType;

        

        public BitArrayGamePlay(SimpleViewCellsPage view, GameConfig config) : base(view, config)
        {
            arrayQuestionType = config.UIQuestionType;
            bitArrayQuestion = new bool[config.KeyboardConfig.KeysInRow];
            bitArrayQuestion2 = new bool[config.KeyboardConfig.KeysInRow];

        }

        public override bool Check(PianoKeyboard pianoKeyboard)
        {
            bool result = CheckOnly(pianoKeyboard.ToBitArray());
            _status = result?Statement.True:Statement.False;
            _view.UpdateView();
            return result;
        }

        public bool CheckOnly(bool[] bitArrayAnswer)
        {
            return CurrentOperation switch
            {
                Operation.Copy => this.Equals(bitArrayAnswer),
                //BitArrayGameType.Reorder => this.Equals(),
                Operation.Quantity => this.SumEquals(bitArrayAnswer),
                //BitArrayGameType.SerializeWithArrow => this.Equals(),
                Operation.Not => this.Not(bitArrayAnswer),
                Operation.And => this.And(bitArrayAnswer),
                Operation.Or => this.Or(bitArrayAnswer),
                Operation.Neutralize => this.Neutralize(bitArrayAnswer),
                _ => false
            };
        }

        public override void GenerateExercise()
        {
            Random r = new();
            CurrentOperation = Config.OperationList[r.Next(Config.OperationList.Count)];
            

            bitArrayQuestion = RandomArray();
            bitArrayQuestion2 = RandomArray();
            while (IsResultAllZeros())
            {
                bitArrayQuestion = RandomArray();
                bitArrayQuestion2 = RandomArray();
            }
            _view.UpdateView(true);
            
        }

         private bool IsResultAllZeros()
        {
            bool[] wrongArray = new bool[bitArrayQuestion.Length];
            for (int i = 0; i < wrongArray.Length; i++)
            {
                wrongArray[i] = false;
            }
            return !CheckOnly(wrongArray);

        }

        public bool[] RandomArray()
        {
            Random random = new Random();
            bool[] array = new bool[bitArrayQuestion.Length];

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = random.Next(2)==1; // Generates either true or false
            }
            return array; 
        }

         public void BitArrayforHands(int[] leftHandBits, int[] rightHandBits)
        {
            for (int i = 0; i < rightHandBits.Length; i++)
            {
                leftHandBits[i] = bitArrayQuestion[rightHandBits.Length -1 - i]?1:0; // Generates either 0 or 1
                rightHandBits[i] = bitArrayQuestion[rightHandBits.Length + i]?1:0; // Generates either 0 or 1
            }
            
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

        public bool Not(bool[] bitArrayAnswer)
        {
            for (int i = 0; i < bitArrayAnswer.Length; i++)
                if (bitArrayAnswer[i] != bitArrayQuestion[i]) return true;
            return false;
        }

        public bool And(bool[] bitArrayAnswer)
        {
            for (int i = 0; i < bitArrayAnswer.Length; i++)
                if (bitArrayAnswer[i] != (bitArrayQuestion[i]&& bitArrayQuestion2[i])) return false;
            return true;
        }

        public bool Or(bool[] bitArrayAnswer)
        {
            for (int i = 0; i < bitArrayAnswer.Length; i++)
                if (bitArrayAnswer[i] != (bitArrayQuestion[i] || bitArrayQuestion2[i])) return false;
            return true;
        }

        public bool Neutralize(bool[] bitArrayAnswer)
        {
            for (int i = 0; i < bitArrayAnswer.Length; i++)
                if ((bitArrayAnswer[i] && bitArrayQuestion[i] && bitArrayQuestion2[i])||
                    bitArrayAnswer[i] != (bitArrayQuestion[i] || bitArrayQuestion2[i])) 
                    return false;
            return true;
        }
    }
}
