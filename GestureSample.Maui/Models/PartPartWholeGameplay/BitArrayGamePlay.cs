using GestureSample.Maui.Data;
using GestureSample.Views.Tests;
using GestureSample.Debugging;

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
        public bool IsPrimaryColorAssignedToLeft { get; private set; } = true;
        private int _currentStagedArrowStepIndex;
        private int _completedStagedArrowCycles;
        private bool _isCurrentStagedArrowMasked;
        private bool _isCurrentStagedArrowRevealed;

        public List<int> triads = new();


        private void GenerateArrowExercise()
        {
            var r = new Random();
            var (fromIndex, lengthIndexes) = PrepareArrowFields(r);
            SetBitArrayForArrow(fromIndex, lengthIndexes);
            Console.WriteLine("above number:{0}", aboveNumber);
        }

        private bool UsesMaskedThirdStagedArrow()
        {
            return Config?.KeyboardConfig?.MaskThirdArrowAfterCycleCount > 0 &&
                   (Config.QuestionOrder == QuestionOrder.FromLeft || Config.QuestionOrder == QuestionOrder.ToLeft);
        }

        private void AdvanceStagedArrowCycleState(bool isNewCycle)
        {
            if (!UsesMaskedThirdStagedArrow())
            {
                _currentStagedArrowStepIndex = 0;
                _completedStagedArrowCycles = 0;
                _isCurrentStagedArrowMasked = false;
                _isCurrentStagedArrowRevealed = false;
                return;
            }

            if (isNewCycle)
            {
                if (_currentStagedArrowStepIndex > 0)
                    _completedStagedArrowCycles++;

                _currentStagedArrowStepIndex = 1;
            }
            else
            {
                _currentStagedArrowStepIndex++;
            }

            _isCurrentStagedArrowMasked =
                _currentStagedArrowStepIndex == 3 &&
                _completedStagedArrowCycles >= Config.KeyboardConfig.MaskThirdArrowAfterCycleCount;
            _isCurrentStagedArrowRevealed = false;
        }

        public string? GetCurrentArrowLabelText()
        {
            if (_isCurrentStagedArrowMasked && !_isCurrentStagedArrowRevealed)
                return "?";

            return length > -1 ? length.ToString() : null;
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
                int maxKey = Math.Min(Config.MaxSum, keys);
                aboveNumber = _nextArrowAboveNumber;
                if (Config.QuestionOrder == QuestionOrder.CyclicalRight) dir = Direction.Right;
                else if (Config.QuestionOrder == QuestionOrder.CyclicalLeft) dir = Direction.Left;
                else if (Config.QuestionOrder == QuestionOrder.CyclicalMixed && Config.OnlyToTen)
                { if(aboveNumber == keys) dir = Direction.Left; else if (aboveNumber == 1) dir = Direction.Right;
                 length = r.Next(1, dir == Direction.Right ? (keys - aboveNumber + 1) : (aboveNumber+1));
                }

                if (Config.OnlyToTen)
                {
                    if (dir == Direction.Right && aboveNumber >= maxKey)
                        aboveNumber = 1;
                    else if (dir == Direction.Left && aboveNumber <= 1)
                        aboveNumber = maxKey;

                    int maxLength = dir == Direction.Right
                        ? Math.Max(1, maxKey - aboveNumber)
                        : Math.Max(1, aboveNumber - 1);

                    length = r.Next(1, maxLength + 1);
                }

                if (dir == Direction.Left && _prevDir == Direction.Right)
                    aboveNumber = (aboveNumber + keys + (isOrdinal ? 0 : -1)) % keys;
                if (dir == Direction.Right && _prevDir == Direction.Left)
                    aboveNumber = (aboveNumber + (isOrdinal ? 0 : 1)) % keys;
                if (aboveNumber == 0) { aboveNumber = keys; }

                _prevDir = dir;
                if (Config.OnlyToTen)
                {
                    _nextArrowAboveNumber = dir == Direction.Right ? aboveNumber + length : aboveNumber - length;
                    if (_nextArrowAboveNumber <= 1)
                        _nextArrowAboveNumber = maxKey;
                    else if (_nextArrowAboveNumber >= maxKey)
                        _nextArrowAboveNumber = 1;
                }
                else
                {
                    _nextArrowAboveNumber = ((dir == Direction.Right ? (aboveNumber + length) : (aboveNumber - length)) + keys) % keys;
                    if (_nextArrowAboveNumber == 0) { _nextArrowAboveNumber = keys; }
                }
            }

            // FromLeft / ToLeft handling (triads sequence)
            if (Config.QuestionOrder == QuestionOrder.FromLeft || Config.QuestionOrder == QuestionOrder.ToLeft)
            {
                bool isFirst = false;
                bool isNewCycle = triads.Count == 0;
                if (isNewCycle)
                {
                    (int firstSegmentLength, int secondSegmentLength) = ResolveStagedArrowSegments(keys);
                    int sum = addend1 + addend2;
                    triads.Add(0);
                    triads.Add(firstSegmentLength);
                    triads.Add(secondSegmentLength);
                    triads.Add(sum);
                    isFirst = true;
                }

                AdvanceStagedArrowCycleState(isNewCycle);

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

        private (int firstSegmentLength, int secondSegmentLength) ResolveStagedArrowSegments(int keys)
        {
            int firstSegmentLength = addend1;
            int secondSegmentLength = addend2;
            int maxFirstSegmentLength = Math.Max(1, keys - 2);
            int totalLength = Math.Max(2, addend1 + addend2);

            if (firstSegmentLength > maxFirstSegmentLength && secondSegmentLength <= maxFirstSegmentLength)
            {
                (firstSegmentLength, secondSegmentLength) = (secondSegmentLength, firstSegmentLength);
            }
            else if (firstSegmentLength > maxFirstSegmentLength && secondSegmentLength > maxFirstSegmentLength)
            {
                firstSegmentLength = Math.Min(maxFirstSegmentLength, totalLength - 1);
                secondSegmentLength = Math.Max(1, totalLength - firstSegmentLength);
            }

            return (firstSegmentLength, secondSegmentLength);
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
        private readonly KeyEventRepository _keyEventRepository;

        public BitArrayGamePlay(GameConfig config) : base(config)
        {
            ArrayQuestionType = config.UIQuestionType;
            BitArrayQuestion = new bool[config.KeyboardConfig.KeysInRow];
            BitArrayQuestion2 = new bool[config.KeyboardConfig.KeysInRow];
            _keyboardQuestionRepository = ServiceHelper.GetService<KeyboardQuestionRepository>();
            _keyEventRepository = ServiceHelper.GetService<KeyEventRepository>();
            _chainSeed = Config?.Plan?.Seed ?? Environment.TickCount;

        }

        public override async Task<ExerciseCheckResult> EvaluateAsync(PianoKeyboard pianoKeyboard)
        {
            if (CurrentOperation == Operation.GroupByColor)
            {
                return await EvaluateGroupByColorAsync(pianoKeyboard);
            }

            bool[] submittedKeyboard = pianoKeyboard.ToBitArray();
            bool result = CheckOnly(submittedKeyboard);
            _status = result ? Statement.True : Statement.False;
            IncrementGuessNumber();
            DateTime submittedTime = DateTime.Now;
            var savedAttempt = await _keyboardQuestionRepository.SaveSubmittedSnapshotAsync(
                GameId.ToString(),
                _questionNumber,
                submittedKeyboard,
                submittedTime,
                result ? 1 : 0);

            if (savedAttempt != null)
            {
                await _keyEventRepository.AssignPendingEventsToAttemptAsync(GameId.ToString(), _questionNumber, savedAttempt.AttemptNumber);
                await _keyEventRepository.SaveCheckEventAsync(GameId.ToString(), _questionNumber, savedAttempt.AttemptNumber, submittedTime);
            }

            if (result)
            {
                _prevBitArrayAnswer = submittedKeyboard.ToArray();
            }

            GameCompletionResult? completion = result
                ? await RegisterSuccessfulAttemptAsync()
                : await RegisterFailedAttemptAsync();

            return CreateCheckResult(result, completion: completion);
        }

        private async Task<ExerciseCheckResult> EvaluateGroupByColorAsync(PianoKeyboard pianoKeyboard)
        {
            bool[] primaryAnswer = pianoKeyboard.GetBitsForColor(Colors.Yellow);
            bool[] secondaryAnswer = pianoKeyboard.GetBitsForColor(Colors.LightGreen);
            bool result = ArraysEqual(primaryAnswer, GetPrimaryColorTargetBits()) &&
                          ArraysEqual(secondaryAnswer, GetSecondaryColorTargetBits());

            _status = result ? Statement.True : Statement.False;
            IncrementGuessNumber();

            DateTime submittedTime = DateTime.Now;
            bool[] submittedKeyboard = pianoKeyboard.ToBitArray();
            var savedAttempt = await _keyboardQuestionRepository.SaveSubmittedSnapshotAsync(
                GameId.ToString(),
                _questionNumber,
                submittedKeyboard,
                submittedTime,
                result ? 1 : 0);

            if (savedAttempt != null)
            {
                await _keyEventRepository.AssignPendingEventsToAttemptAsync(GameId.ToString(), _questionNumber, savedAttempt.AttemptNumber);
                await _keyEventRepository.SaveCheckEventAsync(GameId.ToString(), _questionNumber, savedAttempt.AttemptNumber, submittedTime);
            }

            if (result)
            {
                _prevBitArrayAnswer = submittedKeyboard.ToArray();
            }
            else if (_isCurrentStagedArrowMasked && !_isCurrentStagedArrowRevealed)
            {
                _isCurrentStagedArrowRevealed = true;
            }

            GameCompletionResult? completion = result
                ? await RegisterSuccessfulAttemptAsync()
                : await RegisterFailedAttemptAsync();

            return CreateCheckResult(result, completion: completion);
        }

        public bool CheckOnly(bool[] bitArrayAnswer)
        {
           return CurrentOperation switch
            {

                Operation.Quantity => SumArray(BitArrayQuestion) == SumArray(bitArrayAnswer),
                Operation.SUMM => SumArray(BitArrayQuestion) + SumArray(BitArrayQuestion2) == SumArray(bitArrayAnswer),
                Operation.GroupByColor => ArraysEqual(bitArrayAnswer, BitArrayCorrectAnswer),
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

        public override Task<ExerciseGenerationResult> GenerateExerciseAsync()
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
            BeginExercise();
            SnapshotPrev();
            ExerciseGenerationResult generatedExercise = CreateGeneratedExerciseResult();
            return Task.FromResult(new ExerciseGenerationResult
            {
                ActionText = generatedExercise.ActionText,
                PersistenceTask = PersistGeneratedExerciseAsync()
            });
        }

        protected override async Task PersistGeneratedExerciseAsync()
        {
            await EnsureGameInitializedAsync();
            await SaveQuestionToDbAsync();
            await SaveState(syncAfterSave: false);
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
                bool hasCanonicalCorrectAnswer = BitArrayCorrectAnswer != null;
                quantity = hasCanonicalCorrectAnswer ? SumArray(BitArrayCorrectAnswer) : SumArray(BitArrayQuestion);
                DevLog.Write("Question is\t\t: "+ FormatBits(BitArrayQuestion));
                DevLog.Write("Correct answer is\t: "+ FormatBits(BitArrayCorrectAnswer));
                DevLog.Write("Is it ok?\t\t\t: "+(!(
                    (Config.KeyboardOnly && hasCanonicalCorrectAnswer && AreOverlapingSets(BitArrayQuestion, BitArrayCorrectAnswer) && CurrentOperation != Operation.Copy) ||
                    !IsAllowedQuestion2Combination(BitArrayQuestion, BitArrayQuestion2))).ToString());

            }
            while ( quantity < Config.MinSum ||
                    quantity > Config.MaxSum ||
                    (Config.KeyboardOnly && BitArrayCorrectAnswer != null && AreOverlapingSets(BitArrayQuestion, BitArrayCorrectAnswer)&& CurrentOperation!=Operation.Copy) ||
                    !IsAllowedQuestion2Combination(BitArrayQuestion, BitArrayQuestion2) ||
                    (CurrentOperation == Operation.SUMM &&
                    SumArray(BitArrayQuestion) + SumArray(BitArrayQuestion2) > BitArrayQuestion.Length)
                    );
        }

        private static string FormatBits(bool[]? bits)
        {
            if (bits == null || bits.Length == 0)
                return "-";

            return string.Join("", bits.Select(bit => bit ? "1" : "0"));
        }

        private bool AreOverlapingSets(bool[] arr1, bool[] arr2)
        {
            if (arr1 == null || arr2 == null) return true;
            if (arr1.Length != arr2.Length) return true;
            if (SumArray(arr1) == 0 || SumArray(arr2) == 0) return true;
            
            bool[] arrAns = new bool[arr1.Length];
            for (int i = 0; i < arr1.Length; i++)
                arrAns[i] = arr1[i] && arr2[i];

            return SumArray(arrAns)>0;
        }

        private bool IsAllowedQuestion2Combination(bool[] arr1, bool[] arr2)
        {
            if (Config.AllowedGroupCombinations == GroupCombinationMode.None)
                return true;

            GroupCombinationMode relation = GetGroupCombination(arr1, arr2);
            return Config.AllowedGroupCombinations.HasFlag(relation);
        }

        private GroupCombinationMode GetGroupCombination(bool[] arr1, bool[] arr2)
        {
            if (arr1 == null || arr2 == null || SumArray(arr2) == 0)
                return GroupCombinationMode.Empty;

            if (AreEqualSets(arr1, arr2))
                return GroupCombinationMode.Same;

            if (IsOneInsideAnother(arr1, arr2))
                return GroupCombinationMode.OneInsideAnother;

            if (AreOverlapingSets(arr1, arr2))
                return GroupCombinationMode.Overlapping;

            return GroupCombinationMode.Strange;
        }

        private bool AreEqualSets(bool[] arr1, bool[] arr2)
        {
            if (arr1 == null || arr2 == null || arr1.Length != arr2.Length)
                return false;

            for (int i = 0; i < arr1.Length; i++)
            {
                if (arr1[i] != arr2[i])
                    return false;
            }

            return true;
        }

        private bool IsOneInsideAnother(bool[] arr1, bool[] arr2)
        {
            if (arr1 == null || arr2 == null || arr1.Length != arr2.Length)
                return false;

            return IsProperSubset(arr1, arr2) || IsProperSubset(arr2, arr1);
        }

        private static bool IsProperSubset(bool[] subsetCandidate, bool[] setCandidate)
        {
            bool hasStrictDifference = false;
            for (int i = 0; i < subsetCandidate.Length; i++)
            {
                if (subsetCandidate[i] && !setCandidate[i])
                    return false;

                if (!subsetCandidate[i] && setCandidate[i])
                    hasStrictDifference = true;
            }

            return hasStrictDifference;
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

        protected override ExerciseGenerationResult CreateGeneratedExerciseResult()
        {
            string actionText = CurrentOperation.ToDString();
            if (CurrentOperation == Operation.MoveBy)
            {
                string strDir = moveBydir == Direction.Right ? "RIGHT( -> )" : "LEFT( <- )";
                actionText += " " + strDir + " BY " + moveByLength;
            }
            else if (CurrentOperation == Operation.GroupByColor)
            {
                actionText = "Group By Color";
            }

            return new ExerciseGenerationResult
            {
                ActionText = actionText
            };
        }

        public bool[] GetTutorialQuestionBits()
        {
            return BitArrayQuestion?.ToArray() ?? Array.Empty<bool>();
        }

        public bool[] GetTutorialQuestionBits2()
        {
            return BitArrayQuestion2?.ToArray() ?? Array.Empty<bool>();
        }

        public bool[] GetTutorialAnswerBits()
        {
            if (BitArrayCorrectAnswer == null)
                BuildCorrectAnswer();

            return BitArrayCorrectAnswer?.ToArray() ?? GetTutorialQuestionBits();
        }

        public bool[] GetPrimaryColorTargetBits()
        {
            if (CurrentOperation != Operation.GroupByColor)
                return Array.Empty<bool>();

            return IsPrimaryColorAssignedToLeft
                ? BuildLeftPackedBits(BitArrayQuestion)
                : BuildRightPackedBits(BitArrayQuestion);
        }

        public bool[] GetSecondaryColorTargetBits()
        {
            if (CurrentOperation != Operation.GroupByColor)
                return Array.Empty<bool>();

            return IsPrimaryColorAssignedToLeft
                ? BuildRightPackedBits(BitArrayQuestion2)
                : BuildLeftPackedBits(BitArrayQuestion2);
        }

        private async Task SaveQuestionToDbAsync()
        {
            Data.SQLite.KeyboardQuestion s = new()
            {
                GameId = this.GameId.ToString(),
                QuestionNumber = _questionNumber,
                Time = DateTime.Now,
                keyboard1 = BitArrayQuestion,
                keyboard2 = BitArrayQuestion2,
                Op = CurrentOperation,
                dir = dir,
                KeyboardRows = Config.KeyboardConfig?.Rows ?? 1,
                KeyboardKeysInRow = Config.KeyboardConfig?.KeysInRow ?? BitArrayQuestion.Length
            };

            if (Config.KeyboardConfig != null && Config.KeyboardConfig.IsArrow)
            {
                s.aboveNumber = aboveNumber;
                s.length = length;
            }

            if (CurrentOperation == Operation.MoveBy)
            {
                s.MoveByLength = moveByLength;
                s.MoveByDirection = moveBydir;
            }

            await _keyboardQuestionRepository.SaveAsync(s);
        }

        private void SnapshotPrev()
        {
            _prevBitArrayQuestion = BitArrayQuestion.ToArray();
            _prevBitArrayQuestion2 = BitArrayQuestion2?.ToArray();
        }

        private void GenerateNonArrowExercise(Random r)
        {
            if (CurrentOperation == Operation.GroupByColor)
            {
                GenerateGroupByColorExercise(r);
                return;
            }

            int from, length;

            int start = 0; int end = BitArrayQuestion.Length; int half = BitArrayQuestion.Length / 2;
            whichHand = null;
            if (Config.RestrictsLogicalKeyboardToOneHand)
            {
                whichHand = Config.WhichHand ?? (r.Next(0, 2) == 0 ? Direction.Left : Direction.Right);
                start = whichHand == Direction.Left ? 0 : BitArrayQuestion.Length / 2;
                end = whichHand == Direction.Left ? BitArrayQuestion.Length / 2 : BitArrayQuestion.Length;
            }

                // first pair (preserve original behavior: min length 1)
                (from, length) = ChooseFromAndLength(r, 1, start, end);
            BitArrayQuestion = Config.OnlySequence ? GenerateSequenceArrayQuestion(from, length) : RandomArray(start, end);

            // second pair (original code allowed length to be 0 initially; use minLength 0 to match)
            (from, length) = ChooseFromAndLength(r, 0, start, end);
            BitArrayQuestion2 = Config.OnlySequence ? GenerateSequenceArrayQuestion(from, length) : RandomArray(start, end);

            // move-by configuration
            moveBydir = r.Next(0, 2) == 0 ? Direction.Right : Direction.Left;
            moveByLength = r.Next(1, BitArrayQuestion.Length);
            if (CurrentOperation is Operation.MoveBy && Config.OnlyToTen)
            {
                // ensure edges constraints
                while (BitArrayQuestion[0] && BitArrayQuestion[BitArrayQuestion.Length - 1])
                {
                    (from, length) = ChooseFromAndLength(r, 1, start, end);
                    BitArrayQuestion = Config.OnlySequence ? GenerateSequenceArrayQuestion(from, length) : RandomArray(start, end);
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
            else if(CurrentOperation is Operation.MoveBy && !Config.OnlyToTen)
            {
                moveBydir = r.Next(0, 2) == 0 ? Direction.Right : Direction.Left;
                moveByLength = r.Next(1, BitArrayQuestion.Length);
            }

        }

        private void GenerateGroupByColorExercise(Random r)
        {
            int start = 0;
            int end = BitArrayQuestion.Length;
            whichHand = null;
            if (Config.RestrictsLogicalKeyboardToOneHand)
            {
                whichHand = Config.WhichHand ?? (r.Next(0, 2) == 0 ? Direction.Left : Direction.Right);
                start = whichHand == Direction.Left ? 0 : BitArrayQuestion.Length / 2;
                end = whichHand == Direction.Left ? BitArrayQuestion.Length / 2 : BitArrayQuestion.Length;
            }

            do
            {
                BitArrayQuestion = Config.OnlySequence ? GenerateRandomSequence(r, start, end) : RandomArray(start, end);
                BitArrayQuestion2 = Config.OnlySequence ? GenerateRandomSequence(r, start, end) : RandomArray(start, end);
            }
            while (SumArray(BitArrayQuestion) == 0 ||
                   SumArray(BitArrayQuestion2) == 0 ||
                   AreOverlapingSets(BitArrayQuestion, BitArrayQuestion2));

            IsPrimaryColorAssignedToLeft = r.Next(2) == 0;
        }

        private bool[] GenerateRandomSequence(Random r, int start, int end)
        {
            (int from, int len) = ChooseFromAndLength(r, 1, start, end);
            return GenerateSequenceArrayQuestion(from, len);
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
                case Operation.GroupByColor:
                    {
                        bool[] primaryTarget = GetPrimaryColorTargetBits();
                        bool[] secondaryTarget = GetSecondaryColorTargetBits();
                        for (int i = 0; i < len; i++)
                            BitArrayCorrectAnswer[i] = primaryTarget[i] || secondaryTarget[i];
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

                case Operation.ExclusiveOr:
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

        private bool[] BuildLeftPackedBits(bool[] source)
        {
            bool[] packed = new bool[source.Length];
            int count = SumArray(source);
            for (int i = 0; i < count && i < packed.Length; i++)
                packed[i] = true;
            return packed;
        }

        private bool[] BuildRightPackedBits(bool[] source)
        {
            bool[] packed = new bool[source.Length];
            int count = SumArray(source);
            for (int i = 0; i < count && i < packed.Length; i++)
                packed[packed.Length - 1 - i] = true;
            return packed;
        }
        public static bool Equals(bool[] bitArrayAnswer, bool[] BitArrayQuestion)
        {
            if (bitArrayAnswer == null || BitArrayQuestion == null) return false;
            if (bitArrayAnswer.Length != BitArrayQuestion.Length) return false;
            //TODO? Through Exceptions
            for (int i = 0; i < bitArrayAnswer.Length; i++)
                if (bitArrayAnswer[i] != BitArrayQuestion[i]) return false;
            return true;
        }

        #region NOT NEEDED FUNCTIONS
        
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
