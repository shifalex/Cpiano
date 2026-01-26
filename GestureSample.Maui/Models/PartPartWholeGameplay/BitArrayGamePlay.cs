using GestureSample.Maui.Data;
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

        public Direction moveBydir = Direction.Right;
        public int moveByLength;

        public List<int> triads = new();


        private void GenerateArrowExercise()
        {
            int fromIndex =0, lengthIndexes =1;
            int keys = BitArrayQuestion.Length;
            bool isOrdinal = Config.KeyboardConfig.ArrowType == ArrowType.Rounded;
            Random r = new();
            dir = r.Next(0, 2) == 0 ? Direction.Right : Direction.Left;
            int[] specialAboves = { 1, 5, 6, 10 };
            if (Config != null && Config.KeyboardConfig != null && Config.KeyboardConfig.ImposeEdges)
            {
                aboveNumber = specialAboves[r.Next(specialAboves.Length)];
                length = r.Next(1, keys);
                length = (aboveNumber ==5 || aboveNumber == 6)?length% 5 : length % keys;
                length++;
                
                dir = (aboveNumber == 5 || aboveNumber == 10) ? Direction.Left : Direction.Right;
            }
            else
            /*if (Config.MaxSum >= keys)
            {
                aboveNumber = r.Next(1, keys + 1);
                length = r.Next(1, keys);
            }
            else*/
            {
                int[] factors = Factors;
                //aboveNumber = Math.Max( factors[0], factors[1])% keys;
                //length = Math.Min(factors[0], factors[1])% keys;
                //if (aboveNumber == 0) aboveNumber = 1;

                aboveNumber = factors[0] % keys; length = factors[1] % keys;
                if (length == 0) {length = 1; Console.WriteLine("{3}->{4}: {0} {1} {2}", factors[0], factors[1], factors[2], aboveNumber, length);}
                aboveNumber = (dir == Direction.Left) ? (aboveNumber + length) % keys : aboveNumber+1;
                Console.WriteLine("{3}->{4}: {0} {1} {2}", factors[0], factors[1], factors[2], aboveNumber, length);

            }

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
        private bool[] BitArrayCorrectAnswer { get; set; }
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

        

        private readonly KeyboardQuestionRepository _keyboardQuestionRepository;

        public BitArrayGamePlay(SimpleViewCellsPage view, GameConfig config) : base(view, config)
        {
            ArrayQuestionType = config.UIQuestionType;
            BitArrayQuestion = new bool[config.KeyboardConfig.KeysInRow];
            BitArrayQuestion2 = new bool[config.KeyboardConfig.KeysInRow];
            _keyboardQuestionRepository = ServiceHelper.GetService<KeyboardQuestionRepository>();
        }

        public override async Task<bool> CheckAsync(PianoKeyboard pianoKeyboard)
        {
            bool result = CheckOnly(pianoKeyboard.ToBitArray());
            _status = result ? Statement.True : Statement.False;
            await Task.Delay(Config.SecondsTillNextExercise * 1000);
            await _view.UpdateView();
            return result;
        }

        public bool CheckOnly(bool[] bitArrayAnswer)
        {
           return CurrentOperation switch
            {
               
                Operation.Quantity => this.QuantityEquals(bitArrayAnswer),
                Operation.SUMM => this.SumEquals(bitArrayAnswer),
                /*Operation.Copy => this.Equals(bitArrayAnswer),
                Operation.Mirror => this.Mirror(bitArrayAnswer),
                Operation.SequenceLTR => this.Sequence(bitArrayAnswer, Direction.Right),
                Operation.SequenceRTL => this.Sequence(bitArrayAnswer, Direction.Left),
                Operation.Split => this.Split(bitArrayAnswer),
                Operation.MoveBy => this.Move(bitArrayAnswer),
                //BitArrayGameType.SerializeWithArrow => this.Equals(),
                Operation.Not => this.Not(bitArrayAnswer),
                Operation.And => this.And(bitArrayAnswer),
                Operation.Or => this.Or(bitArrayAnswer),
                Operation.Neutralise => this.Xor(bitArrayAnswer),*/

                _ =>ArraysEqual(bitArrayAnswer, BitArrayCorrectAnswer)
            };
        }

        private static bool ArraysEqual(bool[]? a, bool[]? b)
        {
            if (a is null || b is null) return false;
            if (a.Length != b.Length) return false;
            return a.SequenceEqual(b); // or use a.AsSpan().SequenceEqual(b) for slightly better perf
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

        private (int from, int length) ChooseFromAndLength(Random r, int minLength)
        {
            int from = r.Next(0, BitArrayQuestion.Length);
            int length = r.Next(minLength, BitArrayQuestion.Length);
            while ((from + length > BitArrayQuestion.Length && Config.OnlyToTen) ||
                   (from + length <= BitArrayQuestion.Length && Config.OnlyThrougTen))
            {
                from = r.Next(0, BitArrayQuestion.Length);
                length = r.Next(minLength, BitArrayQuestion.Length);
                Console.WriteLine("Rechoosing from:{0} length: {1}", from, length);
            }
            return (from, length);
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

                // first pair (preserve original behavior: min length 1)
                (from, length) = ChooseFromAndLength(r, 1);
                BitArrayQuestion = (Config.isOnlySequence) ? GenerateSequenceArrayQuestion(from, length) : RandomArray();


                // second pair (original code allowed length to be 0 initially; use minLength 0 to match)
                (from, length) = ChooseFromAndLength(r, 0);
                BitArrayQuestion2 = (Config.isOnlySequence) ? GenerateSequenceArrayQuestion(from, length) : RandomArray();


                moveBydir = r.Next(0, 2) == 0 ? Direction.Right : Direction.Left;
                moveByLength = r.Next(1, BitArrayQuestion.Length);
                if (CurrentOperation is Operation.MoveBy && Config.OnlyToTen)
                {
                    while (BitArrayQuestion[0] && BitArrayQuestion[BitArrayQuestion.Length - 1] )
                    {
                        (from, length) = ChooseFromAndLength(r, 1);
                        BitArrayQuestion = (Config.isOnlySequence) ? GenerateSequenceArrayQuestion(from, length) : RandomArray(); ;
                    }
                    if (BitArrayQuestion[0] || BitArrayQuestion[BitArrayQuestion.Length - 1])
                    {
                        if (BitArrayQuestion[0] && !BitArrayQuestion[BitArrayQuestion.Length - 1])
                        {
                            moveBydir = Direction.Right;
                        }
                        else if (!BitArrayQuestion[0] && BitArrayQuestion[BitArrayQuestion.Length - 1])
                        {
                            moveBydir = Direction.Left;
                        }
                    }
                    else
                    {
                        moveBydir = r.Next(0, 2) == 0 ? Direction.Right : Direction.Left;
                        Console.WriteLine("Random direction: {0}", moveBydir);
                    }
                        int maxLength= BitArrayQuestion.Length;
                        if (moveBydir == Direction.Left)
                        {
                            for (int i = 0; i < BitArrayQuestion.Length; i++)
                            {
                                if (BitArrayQuestion[i] )
                                {
                                    maxLength = i;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            for (int i = BitArrayQuestion.Length - 1; i >= 0; i--)
                            {
                                if (BitArrayQuestion[i])
                                {
                                    maxLength = BitArrayQuestion.Length - 1 - i;
                                    break;
                                }
                            }
                        }
                        Console.WriteLine("Max length for move by: {0}", maxLength);
                        moveByLength = r.Next(1, maxLength+1);
                    }
                
                 

                BuildCorrectAnswer();

                while (IsResultAllZeros() ||
                    (CurrentOperation == Operation.SUMM &&  
                    SumArray(BitArrayQuestion) + SumArray(BitArrayQuestion2) > BitArrayQuestion.Length))
                {
                    GenerateExercise();
                    return;
                }


            }

/* Unmerged change from project 'GestureSample.Maui (net7.0-ios)'
Before:
            Data.KeyboardQuestion s = new()
            {
After:
            KeyboardQuestion s = new()
            {
*/
            Data.SQLite.KeyboardQuestion s = new()
            {

                GameId = this.GameId.ToString(),
                QuestionNumber = _questionNumber,
                Time = DateTime.Now,
                keyboard1 = BitArrayQuestion,
                keyboard2 = BitArrayQuestion2
            };
            if (Config.KeyboardConfig != null && Config.KeyboardConfig.IsArrow)
            {
                s.aboveNumber = aboveNumber;
                s.length = length;
            }
            _keyboardQuestionRepository.SaveAsync(s);


            _view.UpdateView(true);
            if(CurrentOperation==Operation.MoveBy)
            {
                _view.AddToLblAction(" "+ moveBydir.ToString()+ " BY " + moveByLength.ToString()  );
            }

        }

        public bool IsCloseEnough(bool[] candidate, int allowedDifferences = 1)
        {
            if (candidate == null) return false;

            // Prefer comparing to the precomputed canonical correct answer
            if (BitArrayCorrectAnswer != null)
            {
                if (candidate.Length != BitArrayCorrectAnswer.Length) return false;
                int diffs = 0;
                for (int i = 0; i < candidate.Length; i++)
                {
                    if (candidate[i] != BitArrayCorrectAnswer[i] && ++diffs > allowedDifferences)
                        return false;
                }
                return true;
            }

            // Operation-specific tolerant checks
            if (CurrentOperation == Operation.Quantity)
            {
                // allow difference in count up to allowedDifferences
                return Math.Abs(SumArray(BitArrayQuestion) - SumArray(candidate)) <= allowedDifferences;
            }
            if(CurrentOperation == Operation.SUMM)
            {
                // allow difference in total count up to allowedDifferences
                int total1 = SumArray(BitArrayQuestion) + SumArray(BitArrayQuestion2);
                int total2 = SumArray(candidate);
                return Math.Abs(total1 - total2) <= allowedDifferences;
            }


            //SHIFT BUT WRONG ONE IS CLOSE ENOUGH..
            /*
            if (CurrentOperation == Operation.MoveBy)
            {
                // allow minimal mismatches across all circular shifts up to allowedDifferences
                int len = BitArrayQuestion.Length;
                if (candidate.Length != len) return false;
                for (int shift = 0; shift < len; shift++)
                {
                    int diffs = 0;
                    for (int i = 0; i < len; i++)
                    {
                        if (candidate[i] != BitArrayQuestion[(i + shift) % len] && ++diffs > allowedDifferences)
                            break;
                    }
                    if (diffs <= allowedDifferences) return true;
                }
                return false;
            }
                        */
            // Generic fallback: compare candidate to the original BitArrayQuestion with tolerance
            if (candidate.Length != BitArrayQuestion.Length) return false;
            int genericDiffs = 0;
            for (int i = 0; i < candidate.Length; i++)
            {
                if (candidate[i] != BitArrayQuestion[i] && ++genericDiffs > allowedDifferences)
                    return false;
            }
            return true;
        }

        // Overload so callers can pass the PianoKeyboard directly
        public override bool IsCloseEnough(PianoKeyboard keyboard, int allowedDifferences = 1)
        {
            if (keyboard == null) return false;
            return IsCloseEnough(keyboard.ToBitArray(), allowedDifferences);
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


        public bool QuantityEquals(bool[] bitArrayAnswer)
        {
            /*int s1 = 0, s2 = 0;
            for (int i = 0; i < bitArrayAnswer.Length; i++)
            { s1 += BitArrayQuestion[i] ? 1 : 0; s2 += bitArrayAnswer[i] ? 1 : 0; }*/
            return SumArray(BitArrayQuestion) == SumArray(bitArrayAnswer);
        }

        public bool SumEquals(bool[] bitArrayAnswer)
        {
            /*int s1 = 0, s2 = 0;
            for (int i = 0; i < bitArrayAnswer.Length; i++)
            { s1 += BitArrayQuestion[i] ? 1 : 0; s2 += bitArrayAnswer[i] ? 1 : 0; }*/
            return SumArray(BitArrayQuestion) + SumArray(BitArrayQuestion2) == SumArray(bitArrayAnswer);
        }

        private int SumArray(bool[] bitArray)
        {
            int s1 = 0;
            for (int i = 0; i < bitArray.Length; i++)
             { s1 += bitArray[i] ? 1 : 0; }
            return s1;
        }

        public bool Equals(bool[] bitArrayAnswer)
        {
            //TODO? Through Exceptions
           for (int i = 0; i < bitArrayAnswer.Length; i++)
                if (bitArrayAnswer[i] != BitArrayQuestion[i]) return false;
            return true;
        }


        private void BuildCorrectAnswer()
        {
            BitArrayCorrectAnswer = null;

            if (BitArrayQuestion == null) return;

            int len = BitArrayQuestion.Length;
            BitArrayCorrectAnswer = new bool[len];

            switch (CurrentOperation)
            {
                case Operation.Copy:
                    BitArrayCorrectAnswer = BitArrayQuestion.ToArray();
                    break;

                case Operation.Mirror:
                    for (int i = 0; i < len; i++)
                        BitArrayCorrectAnswer[i] = BitArrayQuestion[len - 1 - i];
                    break;

                case Operation.SequenceRTL:
                    {
                        int count = Sum;
                        for (int i = 0; i < len; i++)
                            BitArrayCorrectAnswer[i] = i < count;
                    }
                    break;

                case Operation.SequenceLTR:
                    {
                        int count = Sum;
                        for (int i = 0; i < len; i++)
                            BitArrayCorrectAnswer[i] = i >= (len - count);
                    }
                    break;
                case Operation.Split:
                    {
                        int countR = 0; int countL = 0;
                        for (int i = 0; i < len; i++)
                            if (BitArrayQuestion[i])
                            {
                                if (i < len / 2) countL++;
                                else countR++;
                            }
                        for (int i = 0; i < len; i++)
                            BitArrayCorrectAnswer[i] = (i < len / 2) ? (i < countL) : (i >= len - countR);
                    }
                    break;

                case Operation.MoveBy:
                    {
                        int moveIndex = moveBydir == Direction.Right ? moveByLength : len - moveByLength;
                        for (int k = 0; k < len; k++)
                            BitArrayCorrectAnswer[k] = BitArrayQuestion[(k - moveIndex + len) % len];
                    }
                    break;

                case Operation.Not:
                    for (int i = 0; i < len; i++)
                        BitArrayCorrectAnswer[i] = !BitArrayQuestion[i];
                    break;

                case Operation.And:
                    if (BitArrayQuestion2 != null)
                    {
                        for (int i = 0; i < len; i++)
                            BitArrayCorrectAnswer[i] = BitArrayQuestion[i] && BitArrayQuestion2[i];
                    }
                    break;

                case Operation.Or:
                    if (BitArrayQuestion2 != null)
                    {
                        for (int i = 0; i < len; i++)
                            BitArrayCorrectAnswer[i] = BitArrayQuestion[i] || BitArrayQuestion2[i];
                    }
                    break;

                case Operation.Neutralise:
                    if (BitArrayQuestion2 != null)
                    {
                        for (int i = 0; i < len; i++)
                            BitArrayCorrectAnswer[i] = BitArrayQuestion[i] ^ BitArrayQuestion2[i];
                    }
                    break;

                case Operation.Quantity:
                case Operation.SUMM:
                default:
                    // Quantity allows any array with the same count — keep BitArrayCorrectAnswer null so fallback
                    BitArrayCorrectAnswer = null;
                    break;
            }
        }

        #region NOT NEEDED FUNCTIONS

        public bool Mirror(bool[] bitArrayAnswer)
        {
            //TODO? Through Exceptions
            for (int i = 0; i < bitArrayAnswer.Length; i++)
                if (bitArrayAnswer[i] != BitArrayQuestion[bitArrayAnswer.Length-1-i]) return false;
            return true;
        }
        public bool Sequence(bool[] bitArrayAnswer, Direction dir)
        {
            int s1 = 0, s2 = 0;
            for (int i = 0; i < bitArrayAnswer.Length; i++)
            {   s1 += BitArrayQuestion[ dir==Direction.Left ? i : BitArrayQuestion.Length - 1 - i] ? 1 : 0; 
                s2 += bitArrayAnswer[i] ? 1 : 0; 
            }
            if( s1 == s2)
            {

                for (int i = 0; i < s1; i++)

                    if ((!bitArrayAnswer[i] && dir==Direction.Left) ||
                        (!bitArrayAnswer[bitArrayAnswer.Length-1-i] && dir == Direction.Right)) return false;
                return true;
            }
            return false;
        }

        public bool Split(bool[] bitArrayAnswer)
        {
            if (Sequence(bitArrayAnswer[..(bitArrayAnswer.Length/2)], Direction.Left) 
                && Sequence(bitArrayAnswer[(bitArrayAnswer.Length / 2)..], Direction.Right))
                    return true;
            return false;
        }

        public bool Move(bool[] bitArrayAnswer)
        {
            int moveIndex = moveBydir == Direction.Right ? moveByLength : bitArrayAnswer.Length - moveByLength;
            //TODO? Through Exceptions
            for (int i = 0; i < bitArrayAnswer.Length; i++)
                if (BitArrayQuestion[i] != bitArrayAnswer[(i+moveIndex)%bitArrayAnswer.Length] ) return false;
            return true;
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
        #endregion
    }
}
