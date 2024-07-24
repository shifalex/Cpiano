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
        public bool[] bitArrayQuestion = new bool[10];
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
            bitArrayQuestion = RandomArray();
            while (IsAllZeros())
                bitArrayQuestion = RandomArray();
            _view.UpdateView(true);
            
        }

         private bool IsAllZeros()
        {
            bool isAllZeros = true;
            for (int i = 0; i < bitArrayQuestion.Length; i++)
            {
                if (bitArrayQuestion[i] == true) { isAllZeros = false; break; }
            }
            return isAllZeros;

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
    }
}
