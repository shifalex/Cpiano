using GestureSample.Views.Tests;

namespace GestureSample.Maui.Models
{
    internal class BitArrayGamePlay : PPWGamePlay
    {
        private int _nextArrowAboveNumber = 1;
        private Direction _prevDir = Direction.Right;
        public Direction dir = Direction.Right;
        public int aboveNumber;
        public int length;
        public List<int> triads = new();


        private void GenerateArrowExercise()
        {
            int fromIndex =0, lengthIndexes =1;
            int keys = BitArrayQuestion.Length;
            bool isOrdinal = Config.KeyboardConfig.ArrowType == ArrowType.Rounded;
            Random r = new();
            //int[] specialAboves = { 1, 5, 6, 10, 3 };
            //if(true) aboveNumber = specialAboves[r.Next(specialAboves.Length)]; else
            aboveNumber = r.Next(1, keys + 1);
            length = r.Next(1, keys);
            dir = r.Next(0, 2) == 0 ? Direction.Right : Direction.Left;

            if (new QuestionOrder[] { QuestionOrder.CyclicalRight, QuestionOrder.CyclicalLeft, QuestionOrder.CyclicalMixed }.Contains(Config.QuestionOrder))
            {
                aboveNumber = _nextArrowAboveNumber;
                if (Config.QuestionOrder == QuestionOrder.CyclicalRight) dir = Direction.Right;
                if (Config.QuestionOrder == QuestionOrder.CyclicalLeft) dir = Direction.Left;

                if (dir == Direction.Left && _prevDir == Direction.Right)
                    aboveNumber = (aboveNumber + keys +(isOrdinal ? 0 :-1)) % keys;
                if (dir == Direction.Right && _prevDir == Direction.Left)
                    aboveNumber = (aboveNumber + (isOrdinal ? 0 : 1)) % keys;
                if (aboveNumber == 0) { aboveNumber = keys; }

                _prevDir = dir;
                _nextArrowAboveNumber = ((dir == Direction.Right ? (aboveNumber + length) : (aboveNumber - length)) + keys) % keys;
                if (_nextArrowAboveNumber == 0) { _nextArrowAboveNumber = keys; }
            }

            if (Config.QuestionOrder == QuestionOrder.FromLeft || Config.QuestionOrder == QuestionOrder.ToLeft)
            {
                bool isFirst = false;
                if (triads.Count == 0)
                {
                    addend1 = r.Next(1, keys);
                    addend2 = r.Next(1, keys);
                    if(addend2 > addend1 && Config.QuestionOrder == QuestionOrder.FromLeft) { int t = addend1; addend1 = addend2; addend2 = t;  }
                    int sum = (addend1 + addend2)%keys; if(sum == 0) sum = keys;
                    triads.Add(0); triads.Add(addend1); triads.Add(addend2); triads.Add(sum );
                    isFirst = true;
                }
                if (Config.QuestionOrder == QuestionOrder.FromLeft)
                {
                    dir = Direction.Right;
                    fromIndex = triads.Count == 2 ? 0 : triads[0]; 
                    lengthIndexes = triads[1];
                    //BitArrayQuestion = triads.Count == 2 ? GenerateSequenceArrayQuestion(0, triads[1]) : GenerateSequenceArrayQuestion(triads[0], triads[1]);
                    aboveNumber = triads.Count == 2 ? 1 : triads[0] + 1;
                    length = triads[1];
                    triads.RemoveAt(0); if (triads.Count == 1) { triads.RemoveAt(0); }
                }
                if (Config.QuestionOrder == QuestionOrder.ToLeft)
                {
                    if (addend1 + addend2 == keys) isFirst = false;
                    dir = isFirst ? Direction.Right : Direction.Left;
                    fromIndex = isFirst ? 0 : ((triads[^1] - triads[^2] + keys) % keys);
                    lengthIndexes = isFirst ? triads[^1] : triads[^2];
                    //BitArrayQuestion = isFirst ? GenerateSequenceArrayQuestion(0, triads[^1]) : GenerateSequenceArrayQuestion((triads[^1] - triads[^2] + keys) % keys, triads[^2]);
                    aboveNumber = isFirst ? 1 : triads[^1];
                    length = isFirst ? triads[^1] : triads[^2];
                    if (!isFirst) triads.RemoveAt(triads.Count - 1);
                    if (triads.Count == 1)
                    {
                        triads.RemoveAt(0);
                    }
                    if (triads.Count == 3)
                    {
                        triads.RemoveAt(2); triads.Add(addend1);
                        triads.RemoveAt(0);
                    }
                }
                if (length == 0) length = keys;
                if (aboveNumber == 0) aboveNumber = keys;

            }
            else
            {
                fromIndex = (dir == Direction.Left ? (aboveNumber - length + keys) : (aboveNumber) - 1) % keys;
                lengthIndexes = length;
                //BitArrayQuestion = GenerateSequenceArrayQuestion((dir == Direction.Left ? (aboveNumber - length + keys) : (aboveNumber) - 1) % keys, length);
            }
                

            if(Config.KeyboardConfig.ArrowType==ArrowType.Rounded)
                BitArrayQuestion = GenerateSequenceArrayQuestion(((dir == Direction.Left ? (aboveNumber - lengthIndexes + keys) : (aboveNumber+ lengthIndexes)) - 1) % keys, 1);
            else
                BitArrayQuestion = GenerateSequenceArrayQuestion(fromIndex, lengthIndexes);
            Console.WriteLine("above number:{0}", aboveNumber);
        }

        public bool[] BitArrayQuestion { get; set; }
        public bool[] BitArrayQuestion2 { get; set; }
        public UIQuestionType ArrayQuestionType { get; set; }

        public override int Sum
        {
            get
            {
                int s1 = 0;
                for (int i = 0; i < BitArrayQuestion.Length; i++)
                { s1 += BitArrayQuestion[i] ? 1 : 0; }
                if (s1 == 0) s1 = 1;
                return s1;
            }
        }



        public BitArrayGamePlay(SimpleViewCellsPage view, GameConfig config) : base(view, config)
        {
            ArrayQuestionType = config.UIQuestionType;
            BitArrayQuestion = new bool[config.KeyboardConfig.KeysInRow];
            BitArrayQuestion2 = new bool[config.KeyboardConfig.KeysInRow];
        }

        public override async Task<bool> CheckAsync(PianoKeyboard pianoKeyboard)
        {
            bool result = CheckOnly(pianoKeyboard.ToBitArray());
            _status = result ? Statement.True : Statement.False;
            _view.UpdateView();
            await Task.Delay(Config.SecondsTillNextExercise * 1000);
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

        public bool[] GenerateSequenceArrayQuestion(int from, int length)
        {
            bool[] bitArrayQuestion = new bool[BitArrayQuestion.Length];
            Console.WriteLine("from:{0} length: {1}", from, length);
            //CurrentOperation = Operation.Copy;
            for (int i = 0; i < bitArrayQuestion.Length; i++)
                bitArrayQuestion[i] = false;

            for (int i = 0; i < length; i++)
                bitArrayQuestion[(from + i) % bitArrayQuestion.Length] = true;

            //addend1 = from; addend2 = length; Sum= addend1+ addend2;
            return bitArrayQuestion;

        }

        public override void GenerateExercise()
        {
            Random r = new();
            CurrentOperation = Config.OperationList[r.Next(Config.OperationList.Count)];
            if (Config.KeyboardConfig != null && Config.KeyboardConfig.IsArrow)
            {
                CurrentOperation = Operation.Copy;
                GenerateArrowExercise();
            }
            else
            {
                int from, length;
                from = r.Next(0, BitArrayQuestion.Length); length = r.Next(1, BitArrayQuestion.Length);
                while((from+length >= BitArrayQuestion.Length && Config.OnlyToTen) ||
                    (from + length < BitArrayQuestion.Length && Config.OnlyThrougTen))
                { from = r.Next(0, BitArrayQuestion.Length); length = r.Next(1, BitArrayQuestion.Length); }
                BitArrayQuestion = (Config.isOnlySequence) ? GenerateSequenceArrayQuestion(from,length) : RandomArray();
                from = r.Next(0, BitArrayQuestion.Length); length = r.Next(0, BitArrayQuestion.Length);
                while ((from + length >= BitArrayQuestion.Length && Config.OnlyToTen) ||
                    (from + length < BitArrayQuestion.Length && Config.OnlyThrougTen))
                { from = r.Next(0, BitArrayQuestion.Length); length = r.Next(1, BitArrayQuestion.Length); }
                BitArrayQuestion2 = (Config.isOnlySequence) ? GenerateSequenceArrayQuestion(from, length) : RandomArray();
                while (IsResultAllZeros())
                {
                    BitArrayQuestion = RandomArray();
                    BitArrayQuestion2 = RandomArray();
                }

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
                array[i] = r.Next(2) == 1; // Generates either true or false
            }
            return array;
        }

        public void BitArrayforHands(int[] leftHandBits, int[] rightHandBits)
        {
            for (int i = 0; i < rightHandBits.Length; i++)
            {
                leftHandBits[i] = BitArrayQuestion[rightHandBits.Length - 1 - i] ? 1 : 0; // Generates either 0 or 1
                rightHandBits[i] = BitArrayQuestion[rightHandBits.Length + i] ? 1 : 0; // Generates either 0 or 1
            }

        }

        public bool Equals(bool[] bitArrayAnswer)
        {
            //TODO? Through Exceptions
           for (int i = 0; i < bitArrayAnswer.Length; i++)
                if (bitArrayAnswer[i] != BitArrayQuestion[i]) return false;
            return true;
        }
        public bool SumEquals(bool[] bitArrayAnswer)
        {
            int s1 = 0, s2 = 0;
            for (int i = 0; i < bitArrayAnswer.Length; i++)
            { s1 += BitArrayQuestion[i] ? 1 : 0; s2 += bitArrayAnswer[i] ? 1 : 0; }
            return s1 == s2;
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
                if (bitArrayAnswer[i] != (BitArrayQuestion[i] && BitArrayQuestion2[i])) return false;
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
                if (bitArrayAnswer[i] != (BitArrayQuestion[i] ^ BitArrayQuestion2[i]))
                    return false;
            return true;
        }
    }
}
