using GestureSample.Views.Tests;

namespace GestureSample.Maui.Models
{
    internal class BitArrayGamePlay : PPWGamePlay
    {
        public bool[] BitArrayQuestion { get; set; }
        public bool[] BitArrayQuestion2 { get; set; }
        public UIQuestionType ArrayQuestionType { get; set; }



        public BitArrayGamePlay(SimpleViewCellsPage view, GameConfig config) : base(view, config)
        {
            ArrayQuestionType = config.UIQuestionType;
            BitArrayQuestion = new bool[config.KeyboardConfig.KeysInRow];
            BitArrayQuestion2 = new bool[config.KeyboardConfig.KeysInRow];
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
                Operation.Neutralize => this.Xor(bitArrayAnswer),
                _ => false
            };
        }

        public override void GenerateExercise()
        {
            Random r = new();
            CurrentOperation = Config.OperationList[r.Next(Config.OperationList.Count)];            

            BitArrayQuestion = RandomArray();
            BitArrayQuestion2 = RandomArray();
            while (IsResultAllZeros())
            {
                BitArrayQuestion = RandomArray();
                BitArrayQuestion2 = RandomArray();
            }
            _view.UpdateView(true);
            
        }

         private bool IsResultAllZeros()
        {
            bool[] wrongArray = new bool[BitArrayQuestion.Length];
            for (int i = 0; i < wrongArray.Length; i++)
            {
                wrongArray[i] = false;
            }
            return CheckOnly(wrongArray);

        }

        protected bool[] RandomArray()
        {
            Random r = new();
            bool[] array = new bool[BitArrayQuestion.Length];

            for (int i = 0; i < array.Length; i++)
            {
                array[i] = r.Next(2)==1; // Generates either true or false
            }
            return array; 
        }

         public void BitArrayforHands(int[] leftHandBits, int[] rightHandBits)
        {
            for (int i = 0; i < rightHandBits.Length; i++)
            {
                leftHandBits[i] = BitArrayQuestion[rightHandBits.Length -1 - i]?1:0; // Generates either 0 or 1
                rightHandBits[i] = BitArrayQuestion[rightHandBits.Length + i]?1:0; // Generates either 0 or 1
            }
            
        }
        
        public bool Equals(bool[] bitArrayAnswer)
        {
            //TODO? Through Exceptions
            //if(bitArrayAnswer==null || bitArrayQuestion==null|| bitArrayAnswer.Length!= bitArrayQuestion.Length) return false;
            for(int i = 0; i < bitArrayAnswer.Length ; i++)
                if(bitArrayAnswer[i]!= BitArrayQuestion[i]) return false;
            return true;
        }
        public bool SumEquals(bool[] bitArrayAnswer)
        {
            int s1 = 0, s2 = 0;
            for (int i = 0; i < bitArrayAnswer.Length; i++)
               { s1 += BitArrayQuestion[i] ? 1 : 0; s2 += bitArrayAnswer[i] ? 1 : 0; }
            return s1==s2;
        }

        public bool Not(bool[] bitArrayAnswer)
        {
            for (int i = 0; i < bitArrayAnswer.Length; i++)
                if (bitArrayAnswer[i] == BitArrayQuestion[i]) return false;
            return true;
        }

        public bool And(bool[] bitArrayAnswer)
        {
            for (int i = 0; i < bitArrayAnswer.Length; i++)
                if (bitArrayAnswer[i] != (BitArrayQuestion[i]&& BitArrayQuestion2[i])) return false;
            return true;
        }

        public bool Or(bool[] bitArrayAnswer)
        {
            for (int i = 0; i < bitArrayAnswer.Length; i++)
                if (bitArrayAnswer[i] != (BitArrayQuestion[i] || BitArrayQuestion2[i])) return false;
            return true;
        }

        public bool Xor(bool[] bitArrayAnswer)
        {
            for (int i = 0; i < bitArrayAnswer.Length; i++)
                if (/*(bitArrayAnswer[i] && BitArrayQuestion[i] && BitArrayQuestion2[i])||
                    (!bitArrayAnswer[i] && BitArrayQuestion[i] && !BitArrayQuestion2[i])||
                    (!bitArrayAnswer[i] && !BitArrayQuestion[i] && BitArrayQuestion2[i])||
                    (bitArrayAnswer[i] && !BitArrayQuestion[i] && !BitArrayQuestion2[i]))*/
                    bitArrayAnswer[i] != (BitArrayQuestion[i] ^ BitArrayQuestion2[i])) 
                    return false;
            return true;
        }
    }
}
