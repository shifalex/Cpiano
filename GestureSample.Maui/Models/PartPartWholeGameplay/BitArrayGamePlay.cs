using GestureSample.Maui.Data;
using GestureSample.Views.Tests;
using MongoDB.Driver.Core.Operations;

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
        public int moveByLength =0;

        public List<int> triads = new();


        private void GenerateArrowExercise()
        {
            var r = new Random();
            var (fromIndex, lengthIndexes) = PrepareArrowFields(r);
            SetBitArrayForArrow(fromIndex, lengthIndexes);
            Console.WriteLine("above number:{0}", aboveNumber);
        }

        private (int fromIndex, int lengthIndexes) PrepareArrowFields(Random r)
        {
            int fromIndex = 0, lengthIndexes = 1;
            int keys = BitArrayQuestion.Length;
            bool isOrdinal = Config?.KeyboardConfig?.ArrowType == ArrowType.Rounded;

            // initial direction and fallback factors
            dir = r.Next(0, 2) == 0 ? Direction.Right : Direction.Left;
            int[] specialAboves = { 1, 5, 6, 10 };

            // impose edges handling
            if (Config != null && Config.KeyboardConfig != null && Config.KeyboardConfig.ImposeEdges)
            {
                aboveNumber = specialAboves[r.Next(specialAboves.Length)];
                length = r.Next(1, Math.Min(Config.MaxAddend2+1, keys));
                length = (aboveNumber == 5 || aboveNumber == 6) ? length % 5 : length;
                length++;
                dir = (aboveNumber == 5 || aboveNumber == 10) ? Direction.Left : Direction.Right;
            }
            else
            {
                int[] factors = Factors;
                aboveNumber = factors[0] % keys;
                do length = factors[1] % Math.Min(Config.MaxAddend2+1, keys); while (length+aboveNumber > Config.MaxSum);
                if (length == 0)
                {
                    length = 1;
                    Console.WriteLine("{3}->{4}: {0} {1} {2}", factors[0], factors[1], factors[2], aboveNumber, length);
                }
                aboveNumber = (dir == Direction.Left) ? (aboveNumber + length) % keys : aboveNumber + 1;
                Console.WriteLine("{3}->{4}: {0} {1} {2}", factors[0], factors[1], factors[2], aboveNumber, length);
            }

            // cyclical orders
            if (new QuestionOrder[] { QuestionOrder.CyclicalRight, QuestionOrder.CyclicalLeft, QuestionOrder.CyclicalMixed }
                .Contains(Config.QuestionOrder))
            {
                aboveNumber = _nextArrowAboveNumber;
                if (Config.QuestionOrder == QuestionOrder.CyclicalRight) dir = Direction.Right;
                else if (Config.QuestionOrder == QuestionOrder.CyclicalLeft) dir = Direction.Left;
                else if (Config.QuestionOrder == QuestionOrder.CyclicalMixed && Config.OnlyToTen)
                { if(aboveNumber == keys) dir = Direction.Left; else if (aboveNumber == 1) dir = Direction.Right;
                 length = r.Next(1, dir == Direction.Right ? (keys - aboveNumber + 1) : (aboveNumber+1));
                }

                if (dir == Direction.Left && _prevDir == Direction.Right)
                    aboveNumber = (aboveNumber + keys + (isOrdinal ? 0 : -1)) % keys;
                if (dir == Direction.Right && _prevDir == Direction.Left)
                    aboveNumber = (aboveNumber + (isOrdinal ? 0 : 1)) % keys;
                if (aboveNumber == 0) { aboveNumber = keys; }

                _prevDir = dir;
                _nextArrowAboveNumber = ((dir == Direction.Right ? (aboveNumber + length) : (aboveNumber - length)) + keys) % keys;
                if (_nextArrowAboveNumber == 0) { _nextArrowAboveNumber = keys; }
            }

            // FromLeft / ToLeft handling (triads sequence)
            if (Config.QuestionOrder == QuestionOrder.FromLeft || Config.QuestionOrder == QuestionOrder.ToLeft)
            {
                bool isFirst = false;
                if (triads.Count == 0)
                {
                    addend1 = r.Next(1, keys);
                    addend2 = r.Next(1, keys);
                    if (addend2 > addend1 && Config.QuestionOrder == QuestionOrder.FromLeft)
                    {
                        int t = addend1; addend1 = addend2; addend2 = t;
                    }
                    int sum = (addend1 + addend2) % keys; if (sum == 0) sum = keys;
                    triads.Add(0); triads.Add(addend1); triads.Add(addend2); triads.Add(sum);
                    isFirst = true;
                }

                if (Config.QuestionOrder == QuestionOrder.FromLeft)
                {
                    dir = Direction.Right;
                    fromIndex = triads.Count == 2 ? 0 : triads[0];
                    lengthIndexes = triads[1];
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
                    aboveNumber = isFirst ? 1 : triads[^1];
                    length = isFirst ? triads[^1] : triads[^2];
                    if (!isFirst) triads.RemoveAt(triads.Count - 1);
                    if (triads.Count == 1) triads.RemoveAt(0);
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
            }

            return (fromIndex, lengthIndexes);
        }

        private void SetBitArrayForArrow(int fromIndex, int lengthIndexes)
        {
            int keys = BitArrayQuestion.Length;
            if (Config?.KeyboardConfig?.ArrowType == ArrowType.Rounded)
            {
                int start = ((dir == Direction.Left ? (aboveNumber - lengthIndexes + keys) : (aboveNumber + lengthIndexes)) - 1) % keys;
                BitArrayQuestion = GenerateSequenceArrayQuestion(start, 1);
            }
            else
            {
                BitArrayQuestion = GenerateSequenceArrayQuestion(fromIndex, lengthIndexes);
            }
        }

        public bool[] BitArrayQuestion { get; set; }
        public bool[] BitArrayQuestion2 { get; set; }
        private bool[] BitArrayCorrectAnswer { get; set; }

        private bool[]? _prevBitArrayQuestion;
        private bool[]? _prevBitArrayQuestion2;
        private bool[]? _prevBitArrayAnswer;

        // plan permutation stable seed
        private int _chainSeed;
        public UIQuestionType ArrayQuestionType { get; set; }

        private Direction? whichHand;

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
            _chainSeed = Config?.Plan?.Seed ?? Environment.TickCount;

        }

        public override async Task<bool> CheckAsync(PianoKeyboard pianoKeyboard)
        {
            bool result = CheckOnly(pianoKeyboard.ToBitArray());
            _status = result ? Statement.True : Statement.False;
            if(result)
                _prevBitArrayAnswer = pianoKeyboard.ToBitArray().ToArray();


            await _view.UpdateView();
            await Task.Delay(Config.SecondsTillNextExercise * 1000);
            return result;
        }

        public bool CheckOnly(bool[] bitArrayAnswer)
        {
           return CurrentOperation switch
            {

                Operation.Quantity => SumArray(BitArrayQuestion) == SumArray(bitArrayAnswer),
                Operation.SUMM => SumArray(BitArrayQuestion) + SumArray(BitArrayQuestion2) == SumArray(bitArrayAnswer),
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

        private (int from, int length) ChooseFromAndLength(Random r, int minLength, int start =0, int end = -1)
        {
            if( end == -1) end = BitArrayQuestion.Length;
            int from = r.Next(start, end);
            int length = r.Next(minLength, end - from);

            if ((from + length > BitArrayQuestion.Length && Config.OnlyToTen) ||
                   (from + length <= BitArrayQuestion.Length && Config.OnlyThrougTen))
            {
                Console.WriteLine("Rechoosing from:{0} length: {1}", from, length);
                return ChooseFromAndLength(r, minLength, start, end);
            }
            return (from, length);
        }

        public override void GenerateExercise()
        {
            Random r = new();

            ExercisePlanStep? step = AcquirePlanStep();

            // 1) Resolve operation
            ResolveOperation(r, step);

            // 2) Resolve question source
            ResolveQuestionSource(r, step);

            // 3) Apply step extras (permutation-based operand2, etc.)
            //ApplyBitArrayStepExtrasIfNeeded(step);

            // 4) Persist + snapshot + UI
            SaveQuestionToDb();
            SnapshotPrev();
            _view.UpdateView(true);

            if (CurrentOperation == Operation.MoveBy)
            {
                string strDir = moveBydir == Direction.Right ? "RIGHT( -> )" : "LEFT( <- )";
                _view.AddToLblAction(" " + strDir + " BY " + moveByLength.ToString());
            }
        }

        private void ResolveOperation(Random r, ExercisePlanStep? step)
        {
            // Preserve your rule: Arrow keyboard always uses Copy
            if (Config.KeyboardConfig != null && Config.KeyboardConfig.IsArrow)
            {
                CurrentOperation = Operation.Copy;
                return;
            }

            if (step != null)
            {
                ApplyOpMode(step); // plan decides op (Fixed/Keep/RandomFromConfigList)
                return;
            }

            // legacy behavior
            CurrentOperation = Config.OperationList[r.Next(Config.OperationList.Count)];
        }

        private void ResolveQuestionSource(Random r, ExercisePlanStep? step)
        {
            if (step != null)
            {
                if (step.Kind == PlanStepKind.RepeatQuestion && _prevBitArrayQuestion != null)
                {
                    BitArrayQuestion = _prevBitArrayQuestion.ToArray();
                    BitArrayQuestion2 = _prevBitArrayQuestion2?.ToArray();
                    BuildCorrectAnswer(); // ensure answer matches reused question
                    return;
                }

                if (step.Kind == PlanStepKind.UsePrevAnswer && _prevBitArrayAnswer != null)
                {
                    BitArrayQuestion = _prevBitArrayAnswer.ToArray();
                    BitArrayQuestion2 = _prevBitArrayQuestion2?.ToArray();
                    BuildCorrectAnswer();
                    return;
                }
            }

            // Otherwise: NewQuestion (plan) OR legacy mode
            GenerateNewQuestion(r);
        }

        private void GenerateNewQuestion(Random r)
        {
            if (Config.KeyboardConfig != null && Config.KeyboardConfig.IsArrow)
            {
                GenerateArrowExercise();
                BuildCorrectAnswer();
                return;
            }

            // non-arrow: keep your validity constraints
            int quantity;
            do
            {
                GenerateNonArrowExercise(r);
                BuildCorrectAnswer(); // IMPORTANT: must rebuild every iteration
                quantity = BitArrayCorrectAnswer!=null?SumArray(BitArrayCorrectAnswer):SumArray(BitArrayQuestion);
                 
            }
            while (quantity == 0 ||
                  (quantity == 1 && Config.OnlyResultMoreThanOne) ||
                  (Config.OnlyStrangeGroups && !AreStrangeGroups(BitArrayQuestion, BitArrayQuestion2)) ||
                  (CurrentOperation == Operation.SUMM &&
                   SumArray(BitArrayQuestion) + SumArray(BitArrayQuestion2) > BitArrayQuestion.Length));
        }

        private bool AreStrangeGroups(bool[] arr1, bool[] arr2)
        {
            if (arr1 == null || arr2 == null) return false;
            if (arr1.Length != arr2.Length) return false;
            if (SumArray(arr1) == 0 || SumArray(arr2) == 0) return false;
            if(arr1.Equals(arr2)) return false;

            bool[] arrAns = new bool[arr1.Length];
            for (int i = 0; i < arr1.Length; i++)
                arrAns[i] = arr1[i] && arr2[i];

            return SumArray(arrAns)>0;
        }

        /*private void ApplyBitArrayStepExtrasIfNeeded(ExercisePlanStep? step)
        {
            if (step == null) return;

            // If you want permutation-based operand2 to be enforced by plan,
            // do it here so it applies to repeat steps too.
            if (!step.UseSecondOperandFromPermutation) return;

            // If you have special cases (e.g. arrow keyboard), you can early-return here.
            if (Config.KeyboardConfig != null && Config.KeyboardConfig.IsArrow) return;

            // Build operand2 from permutation policy (you likely already have / will add a helper).
            BitArrayQuestion2 = BuildPermutedOperand(BitArrayQuestion, step.PermutationPolicy);

            BuildCorrectAnswer();
        }*/

        private void SaveQuestionToDb()
        {
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
        }

        private void SnapshotPrev()
        {
            _prevBitArrayQuestion = BitArrayQuestion.ToArray();
            _prevBitArrayQuestion2 = BitArrayQuestion2?.ToArray();
        }

        private void GenerateNonArrowExercise(Random r)
        {
            int from, length;

            int start = 0; int end = BitArrayQuestion.Length; int half = BitArrayQuestion.Length / 2;
            whichHand = null;
            if (Config.IsOnlyOneHand)
            {
                whichHand = Config.WhichHand ?? (r.Next(0, 2) == 0 ? Direction.Left : Direction.Right);
                start = whichHand == Direction.Left ? 0 : BitArrayQuestion.Length / 2;
                end = whichHand == Direction.Left ? BitArrayQuestion.Length / 2 : BitArrayQuestion.Length;
            }

                // first pair (preserve original behavior: min length 1)
                (from, length) = ChooseFromAndLength(r, 1, start, end);
            BitArrayQuestion = (Config.isOnlySequence) ? GenerateSequenceArrayQuestion(from, length) : RandomArray(start, end);

            // second pair (original code allowed length to be 0 initially; use minLength 0 to match)
            (from, length) = ChooseFromAndLength(r, 0, start, end);
            BitArrayQuestion2 = (Config.isOnlySequence) ? GenerateSequenceArrayQuestion(from, length) : RandomArray(start, end);

            // move-by configuration
            moveBydir = r.Next(0, 2) == 0 ? Direction.Right : Direction.Left;
            moveByLength = r.Next(1, BitArrayQuestion.Length);
            if (CurrentOperation is Operation.MoveBy && Config.OnlyToTen)
            {
                // ensure edges constraints
                while (BitArrayQuestion[0] && BitArrayQuestion[BitArrayQuestion.Length - 1])
                {
                    (from, length) = ChooseFromAndLength(r, 1, start, end);
                    BitArrayQuestion = (Config.isOnlySequence) ? GenerateSequenceArrayQuestion(from, length) : RandomArray(start, end);
                }

                if (BitArrayQuestion[0] || BitArrayQuestion[BitArrayQuestion.Length - 1])
                {
                    if (BitArrayQuestion[0] && !BitArrayQuestion[BitArrayQuestion.Length - 1])
                        moveBydir = Direction.Right;
                    else if (!BitArrayQuestion[0] && BitArrayQuestion[BitArrayQuestion.Length - 1])
                        moveBydir = Direction.Left;
                }
                else
                {
                    moveBydir = r.Next(0, 2) == 0 ? Direction.Right : Direction.Left;
                }

                int maxLength = BitArrayQuestion.Length;
                if (moveBydir == Direction.Left)
                {
                    for (int i = 0; i < BitArrayQuestion.Length; i++)
                    {
                        if (BitArrayQuestion[i])
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
                moveByLength = r.Next(1, maxLength + 1);
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
            if (CurrentOperation == Operation.SUMM)
            {
                // allow difference in total count up to allowedDifferences
                int total1 = SumArray(BitArrayQuestion) + SumArray(BitArrayQuestion2);
                int total2 = SumArray(candidate);
                return Math.Abs(total1 - total2) <= allowedDifferences;
            }

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


        protected bool[] RandomArray(int from =0, int to = -1)
        {
            if( to == -1) to = BitArrayQuestion.Length;
            Random r = new();
            bool[] array = new bool[BitArrayQuestion.Length];

            for (int i = 0; i < array.Length; i++)
            {
                if( i>= from && i< to)
                    array[i] = r.Next(2) == 1; // Generates either true or false
                else
                    array[i] = false;
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

        private int SumArray(bool[] bitArray)
        {
            int s1 = 0;
            for (int i = 0; i < bitArray.Length; i++)
             { s1 += bitArray[i] ? 1 : 0; }
            return s1;
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
        private bool IsResultAllZeros()
        {
            int quantity = SumArray(BitArrayCorrectAnswer);
            return quantity == 0 ||
                (Config.OnlyResultMoreThanOne && quantity == 1);
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

        public bool Equals(bool[] bitArrayAnswer)
        {
            //TODO? Through Exceptions
            for (int i = 0; i < bitArrayAnswer.Length; i++)
                if (bitArrayAnswer[i] != BitArrayQuestion[i]) return false;
            return true;
        }

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
