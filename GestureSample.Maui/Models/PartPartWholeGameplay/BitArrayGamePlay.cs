using GestureSample.Maui.Data;
using GestureSample.Views.Tests;
using GestureSample.Debugging;
using Microsoft.Maui.Graphics;

namespace GestureSample.Maui.Models
{
    internal sealed class GroupByColorStep
    {
        public GroupByColorStep(bool[] bits, Color color, Direction direction, bool[]? targetBits = null)
        {
            Bits = bits;
            Color = color;
            Direction = direction;
            TargetBits = targetBits ?? bits;
        }

        public bool[] Bits { get; }
        public Color Color { get; }
        public Direction Direction { get; }
        public bool[] TargetBits { get; }
    }

    internal class BitArrayGamePlay : PPWGamePlay
    {
        private static readonly Color[] GroupByColorPalette =
        {
            Colors.Yellow,
            Colors.LightGreen,
            Colors.Blue
        };

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
        public bool ForceShowMaskedThirdArrow { get; set; }
        private int _arrowLabelStartValue;
        private int _arrowLabelEndValue;
        private int _arrowLabelDistance;
        private int? _arrowLabelMiddleValue;
        private ArrowLabelExerciseMode _activeArrowLabelExerciseMode = ArrowLabelExerciseMode.None;
        private ArrowLabelExerciseMode _primaryArrowLabelExerciseMode = ArrowLabelExerciseMode.None;
        private ArrowLabelExerciseMode _pendingArrowLabelExerciseMode = ArrowLabelExerciseMode.None;
        private MissingValueTargetFlags _activeArrowLabelMissingTarget = MissingValueTargetFlags.None;
        private MissingValueTargetFlags _primaryArrowLabelMissingTarget = MissingValueTargetFlags.None;
        private MissingValueTargetFlags _pendingArrowLabelMissingTarget = MissingValueTargetFlags.None;
        private bool _isArrowLabelRetryAlternateActive;
        private bool _pendingArrowLabelKeyboardQuestion;
        private bool _usesActiveOnKeyboardArrow;
        private ArrowType _activeArrowType = ArrowType.Straight;
        private bool[]? _stagedArrowFirstBits;
        private bool[]? _stagedArrowSecondBits;
        private readonly List<bool[]> _groupByColorQuestionGroups = new();
        private readonly List<Direction> _groupByColorTargetDirections = new();

        public List<int> triads = new();


        private void GenerateArrowExercise()
        {
            var r = new Random();
            var (fromIndex, lengthIndexes) = PrepareArrowFields(r);
            SetBitArrayForArrow(fromIndex, lengthIndexes);
            CaptureStagedArrowOverlayState();
            Console.WriteLine("above number:{0}", aboveNumber);
        }

        private bool SupportsComposedArrowVariants()
        {
            KeyboardConfig? keyboardConfig = Config?.KeyboardConfig;
            return keyboardConfig != null &&
                   (keyboardConfig.AllowedArrowPromptKinds != ArrowPromptKindFlags.None ||
                    keyboardConfig.AllowedArrowRouteKinds != ArrowRouteKindFlags.None ||
                    keyboardConfig.SpecialArrowMissingTargets != MissingValueTargetFlags.None);
        }

        private ArrowLabelExerciseMode GetCurrentArrowLabelExerciseMode()
        {
            if (SupportsComposedArrowVariants())
                return _activeArrowLabelExerciseMode;

            return Config?.KeyboardConfig?.ArrowLabelExerciseMode ?? ArrowLabelExerciseMode.None;
        }

        private ArrowType GetCurrentArrowType()
        {
            if (SupportsComposedArrowVariants())
                return _activeArrowType;

            return Config?.KeyboardConfig?.ArrowType ?? ArrowType.Straight;
        }

        private static ArrowType GetArrowTypeForPromptMode(ArrowLabelExerciseMode mode)
        {
            return mode == ArrowLabelExerciseMode.OrdinalStartAndLength
                ? ArrowType.Rounded
                : ArrowType.Straight;
        }

        private static ArrowLabelExerciseMode GetPromptModeForMissingTarget(MissingValueTargetFlags target, ArrowRouteKindFlags routeKinds)
        {
            if (routeKinds.HasFlag(ArrowRouteKindFlags.Ordinal) && target.HasFlag(MissingValueTargetFlags.Sum))
                return ArrowLabelExerciseMode.OrdinalStartAndLength;

            if (target.HasFlag(MissingValueTargetFlags.Addend1))
                return ArrowLabelExerciseMode.EndAndLengthWithMissingStart;

            if (target.HasFlag(MissingValueTargetFlags.Addend2))
                return ArrowLabelExerciseMode.StartAndEndWithMissingLength;

            return ArrowLabelExerciseMode.StartAndLength;
        }

        private static MissingValueTargetFlags GetMissingTargetForPromptMode(ArrowLabelExerciseMode mode)
        {
            return mode switch
            {
                ArrowLabelExerciseMode.StartAndEndWithMissingLength => MissingValueTargetFlags.Addend2,
                ArrowLabelExerciseMode.EndAndLengthWithMissingStart => MissingValueTargetFlags.Addend1,
                ArrowLabelExerciseMode.None => MissingValueTargetFlags.None,
                _ => MissingValueTargetFlags.Sum
            };
        }

        private MissingValueTargetFlags GetCurrentArrowLabelMissingTarget()
        {
            if (SupportsComposedArrowVariants())
                return _activeArrowLabelMissingTarget == MissingValueTargetFlags.None
                    ? GetMissingTargetForPromptMode(_activeArrowLabelExerciseMode)
                    : _activeArrowLabelMissingTarget;

            MissingValueTargetFlags configuredTarget = Config?.ArrowLabelMissingValueTarget ?? MissingValueTargetFlags.None;
            return configuredTarget == MissingValueTargetFlags.None
                ? GetMissingTargetForPromptMode(GetCurrentArrowLabelExerciseMode())
                : configuredTarget;
        }

        private void SetActiveArrowLabelMissingTarget(MissingValueTargetFlags target)
        {
            _activeArrowLabelMissingTarget = target;
            if (Config != null)
                Config.ArrowLabelMissingValueTarget = target;
        }

        private ArrowLabelExerciseMode GetArrowLabelRetryAlternateMode()
        {
            return Config?.KeyboardConfig?.ArrowLabelRetryAlternateMode ?? ArrowLabelExerciseMode.None;
        }

        private bool UsesArrowLabelRetry()
        {
            return Config?.KeyboardConfig?.EnableArrowLabelRetry == true ||
                   Config?.KeyboardConfig?.ArrowLabelRetryMode != ArrowLabelRetryMode.None;
        }

        private List<(ArrowLabelExerciseMode Mode, MissingValueTargetFlags Target)> GetArrowLabelRetryAlternatePrompts()
        {
            List<(ArrowLabelExerciseMode Mode, MissingValueTargetFlags Target)> prompts = new();
            KeyboardConfig? keyboardConfig = Config?.KeyboardConfig;
            if (keyboardConfig == null || !UsesArrowLabelRetry())
                return prompts;

            ArrowRouteKindFlags routeKinds = keyboardConfig.AllowedArrowRouteKinds == ArrowRouteKindFlags.None
                ? ArrowRouteKindFlags.Cardinal
                : keyboardConfig.AllowedArrowRouteKinds;

            MissingValueTargetFlags retryTargets = keyboardConfig.SpecialArrowRetryAlternateTargets;
            if (retryTargets != MissingValueTargetFlags.None)
            {
                if (retryTargets.HasFlag(MissingValueTargetFlags.Sum))
                    prompts.Add((GetPromptModeForMissingTarget(MissingValueTargetFlags.Sum, routeKinds), MissingValueTargetFlags.Sum));
                if (retryTargets.HasFlag(MissingValueTargetFlags.Addend2))
                    prompts.Add((GetPromptModeForMissingTarget(MissingValueTargetFlags.Addend2, routeKinds), MissingValueTargetFlags.Addend2));
                if (retryTargets.HasFlag(MissingValueTargetFlags.TotalDistance))
                    prompts.Add((GetPromptModeForMissingTarget(MissingValueTargetFlags.TotalDistance, routeKinds), MissingValueTargetFlags.TotalDistance));
                if (retryTargets.HasFlag(MissingValueTargetFlags.Addend1))
                    prompts.Add((GetPromptModeForMissingTarget(MissingValueTargetFlags.Addend1, routeKinds), MissingValueTargetFlags.Addend1));
            }

            ArrowLabelExerciseMode singleAlternateMode = GetArrowLabelRetryAlternateMode();
            if (singleAlternateMode != ArrowLabelExerciseMode.None)
                prompts.Add((singleAlternateMode, GetMissingTargetForPromptMode(singleAlternateMode)));

            return prompts
                .Where(prompt => prompt.Mode != ArrowLabelExerciseMode.None &&
                                 (prompt.Mode != _primaryArrowLabelExerciseMode ||
                                  prompt.Target != _primaryArrowLabelMissingTarget))
                .Distinct()
                .ToList();
        }

        private void CapturePrimaryArrowLabelPromptMode()
        {
            _primaryArrowLabelExerciseMode = GetCurrentArrowLabelExerciseMode();
            _primaryArrowLabelMissingTarget = GetCurrentArrowLabelMissingTarget();
            _pendingArrowLabelExerciseMode = ArrowLabelExerciseMode.None;
            _pendingArrowLabelMissingTarget = MissingValueTargetFlags.None;
            _isArrowLabelRetryAlternateActive = false;
        }

        private void QueueArrowLabelPrompt(ArrowLabelExerciseMode mode, MissingValueTargetFlags target)
        {
            _pendingArrowLabelExerciseMode = mode;
            _pendingArrowLabelMissingTarget = target;
        }

        public bool ApplyPendingArrowLabelPromptMode()
        {
            if (_pendingArrowLabelExerciseMode == ArrowLabelExerciseMode.None &&
                !_pendingArrowLabelKeyboardQuestion)
            {
                return false;
            }

            if (_pendingArrowLabelKeyboardQuestion)
            {
                _pendingArrowLabelKeyboardQuestion = false;
                _pendingArrowLabelExerciseMode = ArrowLabelExerciseMode.None;
                _pendingArrowLabelMissingTarget = MissingValueTargetFlags.None;
                _activeArrowLabelExerciseMode = ArrowLabelExerciseMode.None;
                SetActiveArrowLabelMissingTarget(MissingValueTargetFlags.None);
                _usesActiveOnKeyboardArrow = true;
                _activeArrowType = ArrowType.Straight;
                _isArrowLabelRetryAlternateActive = true;
                return true;
            }

            _activeArrowLabelExerciseMode = _pendingArrowLabelExerciseMode;
            _pendingArrowLabelExerciseMode = ArrowLabelExerciseMode.None;
            SetActiveArrowLabelMissingTarget(_pendingArrowLabelMissingTarget == MissingValueTargetFlags.None
                ? GetMissingTargetForPromptMode(_activeArrowLabelExerciseMode)
                : _pendingArrowLabelMissingTarget);
            _pendingArrowLabelMissingTarget = MissingValueTargetFlags.None;
            _usesActiveOnKeyboardArrow = false;
            _activeArrowType = GetArrowTypeForPromptMode(_activeArrowLabelExerciseMode);
            _isArrowLabelRetryAlternateActive =
                _activeArrowLabelExerciseMode != _primaryArrowLabelExerciseMode ||
                GetCurrentArrowLabelMissingTarget() != _primaryArrowLabelMissingTarget;
            ApplyArrowLabelPpwState(revealMissingValue: false);
            return true;
        }

        private bool TryQueueArrowLabelRetryAlternatePrompt()
        {
            if (!UsesArrowLabelRetry() ||
                Config?.KeyboardConfig?.ArrowLabelRetryMode != ArrowLabelRetryMode.None)
                return false;

            if (_isArrowLabelRetryAlternateActive)
            {
                return false;
            }

            List<(ArrowLabelExerciseMode Mode, MissingValueTargetFlags Target)> alternatePrompts = GetArrowLabelRetryAlternatePrompts();
            if (alternatePrompts.Count == 0)
                return false;

            (ArrowLabelExerciseMode alternateMode, MissingValueTargetFlags alternateTarget) =
                alternatePrompts[Random.Shared.Next(alternatePrompts.Count)];
            QueueArrowLabelPrompt(alternateMode, alternateTarget);
            return true;
        }

        private bool TryQueuePrimaryArrowLabelPromptAfterAlternateSuccess()
        {
            if (!UsesArrowLabelRetry() ||
                !_isArrowLabelRetryAlternateActive ||
                _primaryArrowLabelExerciseMode == ArrowLabelExerciseMode.None)
            {
                return false;
            }

            QueueArrowLabelPrompt(_primaryArrowLabelExerciseMode, _primaryArrowLabelMissingTarget);
            return true;
        }

        public bool QueueArrowLabelRetryKeyboardQuestion()
        {
            if (!UsesArrowLabelRetry() ||
                _isArrowLabelRetryAlternateActive ||
                _primaryArrowLabelExerciseMode == ArrowLabelExerciseMode.None)
            {
                return false;
            }

            _pendingArrowLabelExerciseMode = ArrowLabelExerciseMode.None;
            _pendingArrowLabelMissingTarget = MissingValueTargetFlags.None;
            _pendingArrowLabelKeyboardQuestion = true;
            return true;
        }

        public bool IsActiveOnKeyboardArrowQuestion => UsesOnKeyboardArrowExercise();

        public void HideCurrentArrowLabelMissingValue()
        {
            if (UsesArrowLabelExercise())
                ApplyArrowLabelPpwState(revealMissingValue: false);
        }

        private bool UsesOnKeyboardArrowExercise()
        {
            if (SupportsComposedArrowVariants())
                return _usesActiveOnKeyboardArrow;

            return Config?.KeyboardConfig?.IsArrow == true;
        }

        private bool UsesArrowLabelExercise()
        {
            return GetCurrentArrowLabelExerciseMode() is
                ArrowLabelExerciseMode.StartAndLength or
                ArrowLabelExerciseMode.StartAndEndWithMissingLength or
                ArrowLabelExerciseMode.EndAndLengthWithMissingStart or
                ArrowLabelExerciseMode.OrdinalStartAndLength or
                ArrowLabelExerciseMode.ComplexBridgeToNextTen or
                ArrowLabelExerciseMode.ComplexLongDistance;
        }

        private void ResolveCurrentArrowVariant(Random r)
        {
            KeyboardConfig? keyboardConfig = Config?.KeyboardConfig;
            _activeArrowLabelExerciseMode = keyboardConfig?.ArrowLabelExerciseMode ?? ArrowLabelExerciseMode.None;
            SetActiveArrowLabelMissingTarget(Config?.ArrowLabelMissingValueTarget == MissingValueTargetFlags.None
                ? GetMissingTargetForPromptMode(_activeArrowLabelExerciseMode)
                : Config?.ArrowLabelMissingValueTarget ?? MissingValueTargetFlags.None);
            _usesActiveOnKeyboardArrow = keyboardConfig?.IsArrow == true;
            _activeArrowType = keyboardConfig?.ArrowType ?? ArrowType.Straight;

            if (keyboardConfig == null || !SupportsComposedArrowVariants())
                return;

            ArrowPromptKindFlags promptKinds = keyboardConfig.AllowedArrowPromptKinds == ArrowPromptKindFlags.None
                ? ArrowPromptKindFlags.OnKeyboard
                : keyboardConfig.AllowedArrowPromptKinds;
            ArrowRouteKindFlags routeKinds = keyboardConfig.AllowedArrowRouteKinds == ArrowRouteKindFlags.None
                ? ArrowRouteKindFlags.Cardinal
                : keyboardConfig.AllowedArrowRouteKinds;
            MissingValueTargetFlags missingTargets = keyboardConfig.SpecialArrowMissingTargets == MissingValueTargetFlags.None
                ? MissingValueTargetFlags.Sum
                : keyboardConfig.SpecialArrowMissingTargets;

            List<(bool UseOnKeyboard, ArrowLabelExerciseMode LabelMode, MissingValueTargetFlags MissingTarget, ArrowType ArrowType)> variants = new();

            if (promptKinds.HasFlag(ArrowPromptKindFlags.OnKeyboard))
            {
                if (routeKinds.HasFlag(ArrowRouteKindFlags.Cardinal))
                    variants.Add((true, ArrowLabelExerciseMode.None, MissingValueTargetFlags.None, ArrowType.Straight));
                if (routeKinds.HasFlag(ArrowRouteKindFlags.Ordinal))
                    variants.Add((true, ArrowLabelExerciseMode.None, MissingValueTargetFlags.None, ArrowType.Rounded));
            }

            if (promptKinds.HasFlag(ArrowPromptKindFlags.SpecialPrompt))
            {
                if (routeKinds.HasFlag(ArrowRouteKindFlags.Cardinal))
                {
                    bool useConfiguredComplexPrompt =
                        keyboardConfig.ArrowLabelExerciseMode is ArrowLabelExerciseMode.ComplexBridgeToNextTen or
                            ArrowLabelExerciseMode.ComplexLongDistance;
                    ArrowLabelExerciseMode distancePromptMode = useConfiguredComplexPrompt
                        ? keyboardConfig.ArrowLabelExerciseMode
                        : ArrowLabelExerciseMode.StartAndLength;
                    ArrowLabelExerciseMode endPromptMode = useConfiguredComplexPrompt
                        ? keyboardConfig.ArrowLabelExerciseMode
                        : ArrowLabelExerciseMode.StartAndLength;

                    if (missingTargets.HasFlag(MissingValueTargetFlags.Sum))
                        variants.Add((false, endPromptMode, MissingValueTargetFlags.Sum, ArrowType.Straight));
                    if (missingTargets.HasFlag(MissingValueTargetFlags.Addend2))
                    {
                        variants.Add((false, useConfiguredComplexPrompt
                            ? keyboardConfig.ArrowLabelExerciseMode
                            : ArrowLabelExerciseMode.StartAndEndWithMissingLength, MissingValueTargetFlags.Addend2, ArrowType.Straight));
                        if (!useConfiguredComplexPrompt)
                            variants.Add((false, ArrowLabelExerciseMode.EndAndLengthWithMissingStart, MissingValueTargetFlags.Addend2, ArrowType.Straight));
                    }
                    if (missingTargets.HasFlag(MissingValueTargetFlags.TotalDistance))
                        variants.Add((false, distancePromptMode, MissingValueTargetFlags.TotalDistance, ArrowType.Straight));
                    if (missingTargets.HasFlag(MissingValueTargetFlags.Addend1))
                        variants.Add((false, ArrowLabelExerciseMode.EndAndLengthWithMissingStart, MissingValueTargetFlags.Addend1, ArrowType.Straight));
                }

                if (routeKinds.HasFlag(ArrowRouteKindFlags.Ordinal) &&
                    missingTargets.HasFlag(MissingValueTargetFlags.Sum))
                {
                    variants.Add((false, ArrowLabelExerciseMode.OrdinalStartAndLength, MissingValueTargetFlags.Sum, ArrowType.Rounded));
                }
            }

            if (variants.Count == 0)
            {
                _activeArrowLabelExerciseMode = ArrowLabelExerciseMode.StartAndLength;
                SetActiveArrowLabelMissingTarget(MissingValueTargetFlags.Sum);
                _usesActiveOnKeyboardArrow = false;
                _activeArrowType = ArrowType.Straight;
                return;
            }

            (bool useOnKeyboard, ArrowLabelExerciseMode labelMode, MissingValueTargetFlags missingTarget, ArrowType arrowType) = variants[r.Next(variants.Count)];
            _usesActiveOnKeyboardArrow = useOnKeyboard;
            _activeArrowLabelExerciseMode = useOnKeyboard ? ArrowLabelExerciseMode.None : labelMode;
            SetActiveArrowLabelMissingTarget(useOnKeyboard ? MissingValueTargetFlags.None : missingTarget);
            _activeArrowType = arrowType;
        }

        private int GetKeyboardValueAtIndex(int index)
        {
            bool withoutZero = Config?.KeyboardConfig?.WithoutZero ?? false;
            return withoutZero ? index + 1 : index;
        }

        private int GetKeyboardIndexForValue(int value)
        {
            bool withoutZero = Config?.KeyboardConfig?.WithoutZero ?? false;
            return withoutZero ? value - 1 : value;
        }

        private int GetMaxArrowLabelDistance(int maxPossibleDistance)
        {
            int configuredMaxDistance = Config?.KeyboardConfig?.MaxArrowLabelDistance ?? 0;
            if (configuredMaxDistance <= 0)
                return maxPossibleDistance;

            return Math.Max(1, Math.Min(maxPossibleDistance, configuredMaxDistance));
        }

        private void GenerateArrowLabelExercise(Random r)
        {
            int keyCount = Math.Max(
                BitArrayQuestion?.Length ?? 0,
                Math.Max(1, (Config?.KeyboardConfig?.Rows ?? 1) * (Config?.KeyboardConfig?.KeysInRow ?? 1)));
            int minValue = GetKeyboardValueAtIndex(0);
            int maxValue = GetKeyboardValueAtIndex(keyCount - 1);
            int maxArrowLabelDistance = GetMaxArrowLabelDistance(maxValue - minValue);
            _arrowLabelMiddleValue = null;

            switch (GetCurrentArrowLabelExerciseMode())
            {
                case ArrowLabelExerciseMode.ComplexBridgeToNextTen:
                {
                    List<(int Start, int End, int Middle)> candidates = new();
                    for (int start = minValue; start < maxValue; start++)
                    {
                        int middle = ((start + 10) / 10) * 10;
                        int endMin = middle + 1;
                        int endMax = Math.Min(middle + 9, maxValue);

                        if (middle <= start || endMin > endMax)
                            continue;
                        if (middle != 10)
                            continue;

                        for (int end = endMin; end <= endMax; end++)
                        {
                            int distance = end - start;
                            if (start >= 10 || distance >= 10 || distance > maxArrowLabelDistance)
                                continue;

                            candidates.Add((start, end, middle));
                        }
                    }

                    if (candidates.Count > 0)
                    {
                        (int start, int end, int middle) = candidates[r.Next(candidates.Count)];
                        _arrowLabelStartValue = start;
                        _arrowLabelEndValue = end;
                        _arrowLabelMiddleValue = middle;
                        _arrowLabelDistance = end - start;
                        break;
                    }

                    goto default;
                }

                case ArrowLabelExerciseMode.ComplexLongDistance:
                {
                    List<(int Start, int End)> candidates = new();
                    for (int start = minValue; start <= maxValue - 2; start++)
                    {
                        int endMax = Math.Min(maxValue, start + maxArrowLabelDistance);
                        for (int end = start + 2; end <= endMax; end++)
                            candidates.Add((start, end));
                    }

                    if (candidates.Count > 0)
                    {
                        (int start, int end) = candidates[r.Next(candidates.Count)];
                        _arrowLabelStartValue = start;
                        _arrowLabelEndValue = end;
                        _arrowLabelMiddleValue = Math.Min(maxValue, Math.Max(start + 1, start + ((end - start + 1) / 2)));
                        _arrowLabelDistance = end - start;
                        break;
                    }

                    goto default;
                }

                default:
                    _arrowLabelStartValue = r.Next(minValue, maxValue);
                    int maxDistanceFromStart = GetMaxArrowLabelDistance(maxValue - _arrowLabelStartValue);
                    _arrowLabelDistance = r.Next(1, maxDistanceFromStart + 1);
                    _arrowLabelEndValue = _arrowLabelStartValue + _arrowLabelDistance;
                    break;
            }

            CapturePrimaryArrowLabelPromptMode();
            ApplyArrowLabelPpwState(revealMissingValue: false);

            int answerStartValue = _arrowLabelStartValue + 1;
            int answerFromIndex = GetKeyboardIndexForValue(answerStartValue);
            BitArrayQuestion = GetCurrentArrowLabelExerciseMode() == ArrowLabelExerciseMode.OrdinalStartAndLength
                ? GenerateSequenceArrayQuestion(GetKeyboardIndexForValue(_arrowLabelEndValue), 1)
                : GenerateSequenceArrayQuestion(answerFromIndex, _arrowLabelDistance);
            BitArrayQuestion2 = new bool[keyCount];
        }

        public bool HasArrowLabelPrompt => UsesArrowLabelExercise();

        public int ArrowLabelAddend1Value => _arrowLabelStartValue;

        public int? ArrowLabelAddend2Value =>
            GetCurrentArrowLabelExerciseMode() is ArrowLabelExerciseMode.ComplexBridgeToNextTen or
                ArrowLabelExerciseMode.ComplexLongDistance
                ? _arrowLabelMiddleValue
                : GetCurrentArrowLabelExerciseMode() is
                ArrowLabelExerciseMode.StartAndLength or
                ArrowLabelExerciseMode.StartAndEndWithMissingLength or
                ArrowLabelExerciseMode.EndAndLengthWithMissingStart or
                ArrowLabelExerciseMode.OrdinalStartAndLength
                ? _arrowLabelDistance
                : null;

        public int ArrowLabelSumValue => _arrowLabelEndValue;
        public int ArrowLabelDistanceValue => _arrowLabelDistance;
        public ArrowLabelExerciseMode CurrentArrowLabelExerciseMode => GetCurrentArrowLabelExerciseMode();
        public MissingValueTargetFlags CurrentArrowLabelMissingTarget => GetCurrentArrowLabelMissingTarget();

        protected override int GetPersistedQuestionAnswerAddend1()
        {
            if (UsesArrowLabelExercise())
            {
                return GetCurrentArrowLabelMissingTarget() == MissingValueTargetFlags.Addend1 &&
                       _status != Statement.True
                    ? NAN
                    : _arrowLabelStartValue;
            }

            return base.GetPersistedQuestionAnswerAddend1();
        }

        protected override int GetPersistedQuestionAnswerAddend2()
        {
            if (UsesArrowLabelExercise())
            {
                return GetCurrentArrowLabelMissingTarget() is (MissingValueTargetFlags.Addend2 or MissingValueTargetFlags.TotalDistance) &&
                       _status != Statement.True
                    ? NAN
                    : _arrowLabelDistance;
            }

            return base.GetPersistedQuestionAnswerAddend2();
        }

        protected override int GetPersistedQuestionAnswerSum()
        {
            if (UsesArrowLabelExercise())
            {
                return GetCurrentArrowLabelMissingTarget() == MissingValueTargetFlags.Sum &&
                       _status != Statement.True
                    ? NAN
                    : _arrowLabelEndValue;
            }

            return base.GetPersistedQuestionAnswerSum();
        }

        protected override Operation GetPersistedQuestionAnswerOperation()
        {
            if (UsesArrowLabelExercise())
                return Operation.Sum;

            return base.GetPersistedQuestionAnswerOperation();
        }

        private bool UsesStagedArrowFlow()
        {
            return Config?.QuestionOrder == QuestionOrder.FromLeft ||
                   Config?.QuestionOrder == QuestionOrder.ToLeft;
        }

        private bool UsesMaskedThirdStagedArrow()
        {
            return Config?.KeyboardConfig?.MaskThirdArrowAfterCycleCount > 0 &&
                   (Config.QuestionOrder == QuestionOrder.FromLeft || Config.QuestionOrder == QuestionOrder.ToLeft);
        }

        private void AdvanceStagedArrowCycleState(bool isNewCycle)
        {
            if (!UsesStagedArrowFlow())
            {
                _currentStagedArrowStepIndex = 0;
                _completedStagedArrowCycles = 0;
                _isCurrentStagedArrowMasked = false;
                _isCurrentStagedArrowRevealed = false;
                _stagedArrowFirstBits = null;
                _stagedArrowSecondBits = null;
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

            if (_currentStagedArrowStepIndex <= 1)
            {
                _stagedArrowFirstBits = null;
                _stagedArrowSecondBits = null;
            }

            _isCurrentStagedArrowMasked =
                UsesMaskedThirdStagedArrow() &&
                _currentStagedArrowStepIndex == 3 &&
                _completedStagedArrowCycles >= Config.KeyboardConfig.MaskThirdArrowAfterCycleCount;
            _isCurrentStagedArrowRevealed = false;
        }

        public string? GetCurrentArrowLabelText()
        {
            if (_isCurrentStagedArrowMasked &&
                !_isCurrentStagedArrowRevealed &&
                !ForceShowMaskedThirdArrow)
                return "?";

            return length > -1 ? length.ToString() : null;
        }

        public bool SupportsThirdArrowVisibilityControl()
        {
            return UsesStagedArrowFlow();
        }

        public bool IsThirdArrowCurrentlyHidden()
        {
            return _isCurrentStagedArrowMasked &&
                   !_isCurrentStagedArrowRevealed &&
                   !ForceShowMaskedThirdArrow;
        }

        private void CaptureStagedArrowOverlayState()
        {
            if (!UsesStagedArrowFlow() || BitArrayQuestion == null)
                return;

            if (_currentStagedArrowStepIndex == 1)
            {
                _stagedArrowFirstBits = BitArrayQuestion.ToArray();
                _stagedArrowSecondBits = null;
            }
            else if (_currentStagedArrowStepIndex == 2)
            {
                _stagedArrowSecondBits = BitArrayQuestion.ToArray();
            }
        }

        public Color?[]? GetStagedArrowTraceOverlayColors()
        {
            if (!UsesStagedArrowFlow() ||
                Config?.KeyboardConfig?.UsePermutationTraceColors != true ||
                BitArrayQuestion == null)
            {
                return null;
            }

            Color?[] overlayColors = new Color?[BitArrayQuestion.Length];

            if (_currentStagedArrowStepIndex >= 2 && _stagedArrowFirstBits != null)
                PaintOverlayBits(overlayColors, _stagedArrowFirstBits, Colors.LightGreen);

            return overlayColors;
        }

        public Color?[]? GetStagedArrowSecondaryTraceOverlayColors()
        {
            if (!UsesStagedArrowFlow() ||
                Config?.KeyboardConfig?.UsePermutationTraceColors != true ||
                BitArrayQuestion == null)
            {
                return null;
            }

            Color?[] overlayColors = new Color?[BitArrayQuestion.Length];

            if (_currentStagedArrowStepIndex >= 3 && _stagedArrowSecondBits != null)
                PaintOverlayBits(overlayColors, _stagedArrowSecondBits, Colors.DeepSkyBlue);

            return overlayColors;
        }

        private static void PaintOverlayBits(Color?[] overlayColors, bool[] bits, Color color)
        {
            int limit = Math.Min(overlayColors.Length, bits.Length);
            for (int i = 0; i < limit; i++)
            {
                if (bits[i])
                    overlayColors[i] = color;
            }
        }

        private (int fromIndex, int lengthIndexes) PrepareArrowFields(Random r)
        {
            int fromIndex = 0, lengthIndexes = 1;
            int keys = BitArrayQuestion.Length;
            bool isOrdinal = GetCurrentArrowType() == ArrowType.Rounded;

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
                addend1 = factors[0];
                addend2 = factors[1];
                Sum = factors[2];
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
                    addend1 = firstSegmentLength;
                    addend2 = secondSegmentLength;
                    Sum = sum;
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

            ApplyOnKeyboardArrowDistanceConstraints(r, keys, ref fromIndex, ref lengthIndexes);
            return (fromIndex, lengthIndexes);
        }

        private void ApplyOnKeyboardArrowDistanceConstraints(Random r, int keys, ref int fromIndex, ref int lengthIndexes)
        {
            int configuredMaxDistance = Config?.KeyboardConfig?.MaxArrowLabelDistance ?? 0;
            bool needsDistanceCap = configuredMaxDistance > 0;
            bool needsThroughTen = Config?.OnlyThrougTen == true;
            if (!needsDistanceCap && !needsThroughTen)
                return;

            int maxDistance = needsDistanceCap
                ? Math.Max(1, Math.Min(keys - 1, configuredMaxDistance))
                : keys - 1;

            bool currentIsValid =
                lengthIndexes <= maxDistance &&
                (!needsThroughTen || ArrowWrapsKeyboardBoundary(aboveNumber, lengthIndexes, dir, keys));
            if (currentIsValid)
                return;

            List<(int Above, int Length, Direction Direction)> candidates = new();
            AddArrowDistanceCandidates(candidates, Direction.Right, keys, maxDistance, needsThroughTen);
            AddArrowDistanceCandidates(candidates, Direction.Left, keys, maxDistance, needsThroughTen);

            if (candidates.Count == 0)
            {
                if (needsThroughTen)
                    AddArrowDistanceCandidates(candidates, Direction.Right, keys, keys - 1, mustCrossTen: true);
                if (needsThroughTen)
                    AddArrowDistanceCandidates(candidates, Direction.Left, keys, keys - 1, mustCrossTen: true);
            }

            if (candidates.Count == 0)
            {
                length = Math.Min(length, maxDistance);
                lengthIndexes = Math.Min(lengthIndexes, maxDistance);
                return;
            }

            (aboveNumber, length, dir) = candidates[r.Next(candidates.Count)];
            lengthIndexes = length;
            fromIndex = dir == Direction.Left
                ? (aboveNumber - length + keys) % keys
                : aboveNumber - 1;
        }

        private static void AddArrowDistanceCandidates(
            List<(int Above, int Length, Direction Direction)> candidates,
            Direction candidateDirection,
            int keys,
            int maxDistance,
            bool mustCrossTen)
        {
            for (int start = 1; start <= keys; start++)
            {
                for (int candidateLength = 1; candidateLength <= maxDistance; candidateLength++)
                {
                    if (mustCrossTen && !ArrowWrapsKeyboardBoundary(start, candidateLength, candidateDirection, keys))
                        continue;

                    candidates.Add((start, candidateLength, candidateDirection));
                }
            }
        }

        private static bool ArrowWrapsKeyboardBoundary(int start, int candidateLength, Direction candidateDirection, int keys)
        {
            return candidateDirection == Direction.Right
                ? start + candidateLength > keys
                : start - candidateLength < 1;
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
            if (GetCurrentArrowType() == ArrowType.Rounded)
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
        public bool[]? BitArrayQuestion3 { get; set; }
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
            int keyCount = Math.Max(1, config.KeyboardConfig.Rows * config.KeyboardConfig.KeysInRow);
            BitArrayQuestion = new bool[keyCount];
            BitArrayQuestion2 = new bool[keyCount];
            _activeArrowLabelExerciseMode = config.KeyboardConfig?.ArrowLabelExerciseMode ?? ArrowLabelExerciseMode.None;
            _activeArrowLabelMissingTarget = config.ArrowLabelMissingValueTarget == MissingValueTargetFlags.None
                ? GetMissingTargetForPromptMode(_activeArrowLabelExerciseMode)
                : config.ArrowLabelMissingValueTarget;
            _keyboardQuestionRepository = ServiceHelper.GetService<KeyboardQuestionRepository>();
            _keyEventRepository = ServiceHelper.GetService<KeyEventRepository>();
            _chainSeed = Config?.Plan?.Seed ?? Environment.TickCount;

        }

        public override async Task<ExerciseCheckResult> EvaluateAsync(int a1, int a2, int s)
        {
            if (!UsesArrowLabelExercise())
                return await base.EvaluateAsync(a1, a2, s);

            MissingValueTargetFlags missingTarget = GetCurrentArrowLabelMissingTarget();
            bool isCorrect = GetCurrentArrowLabelExerciseMode() switch
            {
                ArrowLabelExerciseMode.ComplexBridgeToNextTen =>
                    a1 == _arrowLabelStartValue &&
                    a2 == _arrowLabelDistance &&
                    s == _arrowLabelEndValue,
                ArrowLabelExerciseMode.ComplexLongDistance =>
                    a1 == _arrowLabelStartValue &&
                    (missingTarget is not (MissingValueTargetFlags.Addend2 or MissingValueTargetFlags.TotalDistance) ||
                     a2 == _arrowLabelDistance) &&
                    s == _arrowLabelEndValue,
                _ =>
                    a1 == _arrowLabelStartValue &&
                    a2 == _arrowLabelDistance &&
                    s == _arrowLabelEndValue
            };

            _status = isCorrect ? Statement.True : Statement.False;
            IncrementGuessNumber();

            if (isCorrect)
            {
                ApplyArrowLabelPpwState(revealMissingValue: true);

                if (TryQueuePrimaryArrowLabelPromptAfterAlternateSuccess())
                    return CreateCheckResult(isCorrect: true, refreshCurrentQuestion: true);

                _prevBitArrayAnswer = BitArrayCorrectAnswer?.ToArray() ?? BitArrayQuestion.ToArray();
                GameCompletionResult? completion = await RegisterSuccessfulAttemptAsync();
                return CreateCheckResult(isCorrect: true, completion: completion);
            }

            GameCompletionResult? failedCompletion = await RegisterFailedAttemptAsync();
            bool shouldRefreshCurrentQuestion = failedCompletion == null && TryQueueArrowLabelRetryAlternatePrompt();
            return CreateCheckResult(isCorrect: false, completion: failedCompletion, refreshCurrentQuestion: shouldRefreshCurrentQuestion);
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
                result ? 1 : 0,
                pianoKeyboard.GetCurrentColors());

            if (savedAttempt != null)
                await FinalizeKeyboardAttemptAsync(savedAttempt, submittedTime);

            if (result)
            {
                if (_isArrowLabelRetryAlternateActive &&
                    _primaryArrowLabelExerciseMode != ArrowLabelExerciseMode.None &&
                    UsesOnKeyboardArrowExercise())
                {
                    QueueArrowLabelPrompt(_primaryArrowLabelExerciseMode, _primaryArrowLabelMissingTarget);
                    ApplyPendingArrowLabelPromptMode();
                    ApplyArrowLabelPpwState(revealMissingValue: true);
                    return CreateCheckResult(isCorrect: true, refreshCurrentQuestion: true);
                }

                if (UsesArrowLabelExercise())
                    ApplyArrowLabelPpwState(revealMissingValue: true);

                if (TryQueuePrimaryArrowLabelPromptAfterAlternateSuccess())
                    return CreateCheckResult(isCorrect: true, refreshCurrentQuestion: true);

                _prevBitArrayAnswer = submittedKeyboard.ToArray();
            }
            else if (_isArrowLabelRetryAlternateActive &&
                     _primaryArrowLabelExerciseMode != ArrowLabelExerciseMode.None &&
                     UsesOnKeyboardArrowExercise())
            {
                GameCompletionResult? failedCompletion = await RegisterFailedAttemptAsync();
                if (failedCompletion == null && TryQueuePrimaryArrowLabelPromptAfterAlternateSuccess())
                    return CreateCheckResult(isCorrect: false, refreshCurrentQuestion: true);

                return CreateCheckResult(isCorrect: false, completion: failedCompletion);
            }

            GameCompletionResult? completion = result
                ? await RegisterSuccessfulAttemptAsync()
                : await RegisterFailedAttemptAsync();

            return CreateCheckResult(result, completion: completion);
        }

        private void ApplyArrowLabelPpwState(bool revealMissingValue)
        {
            if (!UsesArrowLabelExercise() || Config?.KeyboardConfig == null)
                return;

            MissingValueTargetFlags missingTarget = GetCurrentArrowLabelMissingTarget();
            switch (GetCurrentArrowLabelExerciseMode())
            {
                case ArrowLabelExerciseMode.StartAndLength:
                case ArrowLabelExerciseMode.OrdinalStartAndLength:
                    addend1 = missingTarget == MissingValueTargetFlags.Addend1 && !revealMissingValue ? NAN : _arrowLabelStartValue;
                    addend2 = missingTarget is (MissingValueTargetFlags.Addend2 or MissingValueTargetFlags.TotalDistance) && !revealMissingValue ? NAN : _arrowLabelDistance;
                    Sum = missingTarget == MissingValueTargetFlags.Sum && !revealMissingValue ? NAN : _arrowLabelEndValue;
                    break;

                case ArrowLabelExerciseMode.StartAndEndWithMissingLength:
                    addend1 = missingTarget == MissingValueTargetFlags.Addend1 && !revealMissingValue ? NAN : _arrowLabelStartValue;
                    addend2 = missingTarget is (MissingValueTargetFlags.Addend2 or MissingValueTargetFlags.TotalDistance) && !revealMissingValue ? NAN : _arrowLabelDistance;
                    Sum = missingTarget == MissingValueTargetFlags.Sum && !revealMissingValue ? NAN : _arrowLabelEndValue;
                    break;

                case ArrowLabelExerciseMode.EndAndLengthWithMissingStart:
                    addend1 = missingTarget == MissingValueTargetFlags.Addend1 && !revealMissingValue ? NAN : _arrowLabelStartValue;
                    addend2 = missingTarget is (MissingValueTargetFlags.Addend2 or MissingValueTargetFlags.TotalDistance) && !revealMissingValue ? NAN : _arrowLabelDistance;
                    Sum = missingTarget == MissingValueTargetFlags.Sum && !revealMissingValue ? NAN : _arrowLabelEndValue;
                    break;

                case ArrowLabelExerciseMode.ComplexBridgeToNextTen:
                    addend1 = missingTarget == MissingValueTargetFlags.Addend1 && !revealMissingValue ? NAN : _arrowLabelStartValue;
                    addend2 = missingTarget is (MissingValueTargetFlags.Addend2 or MissingValueTargetFlags.TotalDistance) && !revealMissingValue
                        ? NAN
                        : _arrowLabelDistance;
                    Sum = missingTarget == MissingValueTargetFlags.Sum && !revealMissingValue ? NAN : _arrowLabelEndValue;
                    break;

                case ArrowLabelExerciseMode.ComplexLongDistance:
                    addend1 = missingTarget == MissingValueTargetFlags.Addend1 && !revealMissingValue ? NAN : _arrowLabelStartValue;
                    addend2 = missingTarget is (MissingValueTargetFlags.Addend2 or MissingValueTargetFlags.TotalDistance) && !revealMissingValue
                        ? NAN
                        : _arrowLabelDistance;
                    Sum = missingTarget == MissingValueTargetFlags.Sum && !revealMissingValue ? NAN : _arrowLabelEndValue;
                    break;
            }
        }

        private async Task<ExerciseCheckResult> EvaluateGroupByColorAsync(PianoKeyboard pianoKeyboard)
        {
            bool[] submittedKeyboard = pianoKeyboard.ToBitArray();
            bool result = CheckGroupByColorAnswer(pianoKeyboard, submittedKeyboard);

            _status = result ? Statement.True : Statement.False;
            IncrementGuessNumber();

            DateTime submittedTime = DateTime.Now;
            var savedAttempt = await _keyboardQuestionRepository.SaveSubmittedSnapshotAsync(
                GameId.ToString(),
                _questionNumber,
                submittedKeyboard,
                submittedTime,
                result ? 1 : 0,
                pianoKeyboard.GetCurrentColors());

            if (savedAttempt != null)
                await FinalizeKeyboardAttemptAsync(savedAttempt, submittedTime);

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

        private bool CheckGroupByColorAnswer(PianoKeyboard pianoKeyboard, bool[] submittedKeyboard)
        {
            List<bool[]> targets = BuildGroupByColorTargetGroups();
            if (targets.Count == 0)
                return false;

            bool[] expectedOccupied = new bool[BitArrayQuestion.Length];
            for (int groupIndex = 0; groupIndex < targets.Count; groupIndex++)
            {
                bool[] normalizedTarget = NormalizeToKeyboardLength(targets[groupIndex], BitArrayQuestion.Length);
                Color expectedColor = GroupByColorPalette[Math.Min(groupIndex, GroupByColorPalette.Length - 1)];
                bool[] submittedColorBits = pianoKeyboard.GetBitsForColor(expectedColor);

                if (!ArraysEqual(submittedColorBits, normalizedTarget))
                    return false;

                for (int keyIndex = 0; keyIndex < expectedOccupied.Length; keyIndex++)
                    expectedOccupied[keyIndex] = expectedOccupied[keyIndex] || normalizedTarget[keyIndex];
            }

            return ArraysEqual(submittedKeyboard, expectedOccupied) &&
                   pianoKeyboard.GetNonFreeColorCount() == SumArray(expectedOccupied);
        }

        private static bool ArraysEqual(bool[]? a, bool[]? b)
        {
            if (a is null || b is null) return false;
            if (a.Length != b.Length) return false;
            return a.SequenceEqual(b); // or use a.AsSpan().SequenceEqual(b) for slightly better perf
        }

        private static bool[] NormalizeToKeyboardLength(bool[] bits, int length)
        {
            bool[] normalized = new bool[length];
            int limit = Math.Min(length, bits.Length);
            Array.Copy(bits, normalized, limit);
            return normalized;
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

        public override string? GetKeyboardQuestionPromptText()
        {
            if (!UsesArrowLabelExercise())
                return base.GetKeyboardQuestionPromptText();

            const string arrowLine = "|--->";
            MissingValueTargetFlags missingTarget = GetCurrentArrowLabelMissingTarget();
            string startText = missingTarget == MissingValueTargetFlags.Addend1 ? "?" : _arrowLabelStartValue.ToString();
            string distanceText = missingTarget is (MissingValueTargetFlags.Addend2 or MissingValueTargetFlags.TotalDistance) ? "?" : _arrowLabelDistance.ToString();
            string endText = missingTarget == MissingValueTargetFlags.Sum ? "?" : _arrowLabelEndValue.ToString();

            return GetCurrentArrowLabelExerciseMode() switch
            {
                ArrowLabelExerciseMode.StartAndLength =>
                    $"   {distanceText}\n{arrowLine}\n{startText}",
                ArrowLabelExerciseMode.StartAndEndWithMissingLength =>
                    $"   {distanceText}\n{arrowLine}\n{startText}      {endText}",
                ArrowLabelExerciseMode.EndAndLengthWithMissingStart =>
                    $"   {distanceText}\n{arrowLine}\n{startText}      {endText}",
                ArrowLabelExerciseMode.OrdinalStartAndLength =>
                    $"   {distanceText}\n(ordinal)\n{startText}",
                ArrowLabelExerciseMode.ComplexBridgeToNextTen =>
                    $"   {distanceText}\n{arrowLine}\n{startText}      {endText}",
                ArrowLabelExerciseMode.ComplexLongDistance =>
                    $"   {distanceText}\n{arrowLine}\n{startText}      {endText}",
                _ => base.GetKeyboardQuestionPromptText()
            };
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
            if (Config.KeyboardConfig != null && (UsesOnKeyboardArrowExercise() || UsesArrowLabelExercise()))
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
            ResolveCurrentArrowVariant(r);

            if (UsesArrowLabelExercise())
            {
                GenerateArrowLabelExercise(r);
                BuildCorrectAnswer();
                return;
            }

            if (UsesOnKeyboardArrowExercise())
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
            if (UsesOnKeyboardArrowExercise()) return;

            // Build operand2 from permutation policy (you likely already have / will add a helper).
            BitArrayQuestion2 = BuildPermutedOperand(BitArrayQuestion, step.PermutationPolicy);

            BuildCorrectAnswer();
        }*/

        protected override ExerciseGenerationResult CreateGeneratedExerciseResult()
        {
            string actionText = CurrentOperation.ToDString();
            if (CurrentOperation == Operation.MoveBy)
            {
                string strDir = moveBydir == Direction.Right ? "RIGHT" : "LEFT";
                actionText += " " + strDir + " BY " + moveByLength;
                strDir = moveBydir == Direction.Right ? "->" : "<-";
                actionText += "\n" + strDir + moveByLength;
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

        public bool UsesArrowDirectionTutorial()
        {
            return Config?.KeyboardConfig != null &&
                   (UsesOnKeyboardArrowExercise() || UsesArrowLabelExercise());
        }

        public bool IsOrdinalArrowTutorial()
        {
            return UsesArrowDirectionTutorial() &&
                   GetCurrentArrowType() == ArrowType.Rounded;
        }

        public bool IsSpecialOrdinalArrowTutorial()
        {
            return UsesArrowLabelExercise() &&
                   GetCurrentArrowLabelExerciseMode() == ArrowLabelExerciseMode.OrdinalStartAndLength;
        }

        public IReadOnlyList<int> GetArrowTutorialStepIndices()
        {
            if (!UsesArrowDirectionTutorial())
                return Array.Empty<int>();

            int keyCount = BitArrayQuestion?.Length ?? 0;
            if (keyCount <= 0)
                return Array.Empty<int>();

            List<int> indices = new();

            if (UsesArrowLabelExercise())
            {
                ArrowLabelExerciseMode mode = GetCurrentArrowLabelExerciseMode();

                if (mode == ArrowLabelExerciseMode.OrdinalStartAndLength)
                {
                    bool leftToRight = Config?.QuestionOrder != QuestionOrder.ToLeft;
                    if (leftToRight)
                    {
                        for (int value = _arrowLabelStartValue + 1; value <= _arrowLabelEndValue; value++)
                        {
                            int index = GetKeyboardIndexForValue(value);
                            if (index >= 0 && index < keyCount)
                                indices.Add(index);
                        }
                    }
                    else
                    {
                        for (int value = _arrowLabelEndValue - 1; value >= _arrowLabelStartValue; value--)
                        {
                            int index = GetKeyboardIndexForValue(value);
                            if (index >= 0 && index < keyCount)
                                indices.Add(index);
                        }
                    }
                }
                else if (mode == ArrowLabelExerciseMode.EndAndLengthWithMissingStart)
                {
                    int startIndex = GetKeyboardIndexForValue(_arrowLabelEndValue);
                    for (int i = 0; i < _arrowLabelDistance; i++)
                    {
                        int index = startIndex - i;
                        if (index >= 0 && index < keyCount)
                            indices.Add(index);
                    }
                }
                else
                {
                    int startIndex = GetKeyboardIndexForValue(_arrowLabelStartValue + 1);
                    for (int i = 0; i < _arrowLabelDistance; i++)
                    {
                        int index = startIndex + i;
                        if (index >= 0 && index < keyCount)
                            indices.Add(index);
                    }
                }

                return indices;
            }

            int currentValue = aboveNumber;
            int stepCount = Math.Max(1, length);
            bool withoutZero = Config?.KeyboardConfig?.WithoutZero ?? false;
            int minValue = withoutZero ? 1 : 0;
            int maxValue = withoutZero ? keyCount : keyCount - 1;

            if (IsOrdinalArrowTutorial() && dir == Direction.Left)
            {
                currentValue -= 1;
                if (currentValue < minValue)
                    currentValue = maxValue;
            }
            else if (IsOrdinalArrowTutorial() && dir == Direction.Right)
            {
                currentValue += 1;
                if (currentValue > maxValue)
                    currentValue = minValue;
            }

            for (int i = 0; i < stepCount; i++)
            {
                int index = GetKeyboardIndexForValue(currentValue);
                if (index >= 0 && index < keyCount)
                    indices.Add(index);

                currentValue += dir == Direction.Right ? 1 : -1;

                if (currentValue < minValue)
                    currentValue = maxValue;
                else if (currentValue > maxValue)
                    currentValue = minValue;
            }

            return indices;
        }

        public bool[] GetPrimaryColorTargetBits()
        {
            return GetGroupByColorTargetBits(0);
        }

        public bool[] GetSecondaryColorTargetBits()
        {
            return GetGroupByColorTargetBits(1);
        }

        public bool[] GetTertiaryColorTargetBits()
        {
            return GetGroupByColorTargetBits(2);
        }

        public Color[] GetGroupByColorQuestionColors()
        {
            if (CurrentOperation != Operation.GroupByColor || _groupByColorQuestionGroups.Count == 0)
                return Array.Empty<Color>();

            Color[] colors = Enumerable.Repeat(Colors.White, BitArrayQuestion.Length).ToArray();
            for (int i = 0; i < _groupByColorQuestionGroups.Count; i++)
            {
                bool[] bits = _groupByColorQuestionGroups[i];
                Color color = GroupByColorPalette[Math.Min(i, GroupByColorPalette.Length - 1)];
                int limit = Math.Min(colors.Length, bits.Length);
                for (int keyIndex = 0; keyIndex < limit; keyIndex++)
                {
                    if (bits[keyIndex])
                        colors[keyIndex] = color;
                }
            }

            return colors;
        }

        public override Color[]? GetQuestionKeyboardColors()
        {
            if (CurrentOperation == Operation.GroupByColor)
                return GetGroupByColorQuestionColors();

            return base.GetQuestionKeyboardColors();
        }

        public IReadOnlyList<GroupByColorStep> GetGroupByColorTutorialSteps()
        {
            List<GroupByColorStep> steps = new();
            List<bool[]> targets = BuildGroupByColorTargetGroups();
            for (int i = 0; i < _groupByColorQuestionGroups.Count; i++)
            {
                steps.Add(new GroupByColorStep(
                    _groupByColorQuestionGroups[i],
                    GroupByColorPalette[Math.Min(i, GroupByColorPalette.Length - 1)],
                    _groupByColorTargetDirections.Count > i ? _groupByColorTargetDirections[i] : Direction.Left,
                    i < targets.Count ? targets[i] : Array.Empty<bool>()));
            }

            return OrderGroupByColorStepsByTargetPosition(steps);
        }

        public IReadOnlyList<GroupByColorStep> GetGroupByColorMissionArrows()
        {
            List<GroupByColorStep> steps = new();
            List<bool[]> targets = BuildGroupByColorTargetGroups();
            for (int i = 0; i < _groupByColorQuestionGroups.Count; i++)
            {
                steps.Add(new GroupByColorStep(
                    i < targets.Count ? targets[i] : Array.Empty<bool>(),
                    GroupByColorPalette[Math.Min(i, GroupByColorPalette.Length - 1)],
                    _groupByColorTargetDirections.Count > i ? _groupByColorTargetDirections[i] : Direction.Left,
                    i < targets.Count ? targets[i] : Array.Empty<bool>()));
            }

            return OrderGroupByColorStepsByTargetPosition(steps);
        }

        private static IReadOnlyList<GroupByColorStep> OrderGroupByColorStepsByTargetPosition(IEnumerable<GroupByColorStep> steps)
        {
            return steps
                .OrderBy(step =>
                {
                    int index = Array.FindIndex(step.TargetBits, bit => bit);
                    return index < 0 ? int.MaxValue : index;
                })
                .ToList();
        }

        private bool[] GetGroupByColorTargetBits(int groupIndex)
        {
            if (CurrentOperation != Operation.GroupByColor)
                return Array.Empty<bool>();

            List<bool[]> targets = BuildGroupByColorTargetGroups();
            return groupIndex >= 0 && groupIndex < targets.Count
                ? targets[groupIndex]
                : Array.Empty<bool>();
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
                KeyboardKeysInRow = Config.KeyboardConfig?.KeysInRow ?? BitArrayQuestion.Length,
                ShowNumbersOnKeys = Config.KeyboardConfig?.ShowNumbersOnKeys == true,
                KeyboardWeights = Config.KeyboardConfig?.WeightsArray?.ToArray(),
                InitialKeyboardState = GetInitialKeyboardState(),
                InitialKeyboardColors = GetInitialKeyboardColors(),
                QuestionKeyboardColors = GetQuestionKeyboardColors(),
                QuestionPromptText = GetKeyboardQuestionPromptText()
            };

            if (UsesOnKeyboardArrowExercise())
            {
                s.aboveNumber = aboveNumber;
                s.length = length;
            }
            else if (UsesArrowLabelExercise())
            {
                s.aboveNumber = _arrowLabelStartValue;
                s.length = _arrowLabelDistance;
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

            _groupByColorQuestionGroups.Clear();
            _groupByColorTargetDirections.Clear();
            BitArrayQuestion3 = null;

            int groupCount = Math.Clamp(Config.KeyboardConfig?.GroupByColorColorCount ?? 2, 2, 3);
            if (groupCount == 3)
            {
                if ((Config.KeyboardConfig?.GroupByColorLayoutMode ?? GroupByColorLayoutMode.Free) == GroupByColorLayoutMode.AssociativityEdges)
                    GenerateAssociativityThreeColorGroupByColorExercise(r, start, end);
                else
                    GenerateThreeColorGroupByColorExercise(r, start, end);
            }
            else
            {
                if ((Config.KeyboardConfig?.GroupByColorLayoutMode ?? GroupByColorLayoutMode.Free) == GroupByColorLayoutMode.CommutativityEdges)
                    GenerateCommutativityTwoColorGroupByColorExercise(r, start, end);
                else
                    GenerateTwoColorGroupByColorExercise(r, start, end);
            }

            BitArrayQuestion = _groupByColorQuestionGroups[0];
            BitArrayQuestion2 = _groupByColorQuestionGroups[1];
            BitArrayQuestion3 = _groupByColorQuestionGroups.Count > 2 ? _groupByColorQuestionGroups[2] : null;
            IsPrimaryColorAssignedToLeft = _groupByColorTargetDirections.Count > 0 &&
                                           _groupByColorTargetDirections[0] == Direction.Left;
        }

        private void GenerateTwoColorGroupByColorExercise(Random r, int start, int end)
        {
            bool[] primary;
            bool[] secondary;
            do
            {
                primary = Config.OnlySequence ? GenerateRandomSequence(r, start, end) : RandomArray(start, end);
                secondary = Config.OnlySequence ? GenerateRandomSequence(r, start, end) : RandomArray(start, end);
            }
            while (SumArray(primary) == 0 ||
                   SumArray(secondary) == 0 ||
                   AreOverlapingSets(primary, secondary));

            _groupByColorQuestionGroups.Add(primary);
            _groupByColorQuestionGroups.Add(secondary);
            AssignGroupByColorDirections(r, _groupByColorQuestionGroups.Count);
        }

        private void GenerateCommutativityTwoColorGroupByColorExercise(Random r, int start, int end)
        {
            int available = Math.Max(2, end - start);
            int[] counts = Config.KeyboardConfig?.GroupByColorCounts is { Length: >= 2 }
                ? ResolveGroupByColorCounts(2, available, Config.KeyboardConfig.GroupByColorCounts)
                : CreateRandomTwoColorEdgeCounts(r, available);
            bool yellowOnLeft = r.Next(2) == 0;

            bool[] yellow = yellowOnLeft
                ? BuildRangeBits(BitArrayQuestion.Length, start, counts[0])
                : BuildRangeBits(BitArrayQuestion.Length, end - counts[0], counts[0]);
            bool[] green = yellowOnLeft
                ? BuildRangeBits(BitArrayQuestion.Length, end - counts[1], counts[1])
                : BuildRangeBits(BitArrayQuestion.Length, start, counts[1]);

            _groupByColorQuestionGroups.Add(yellow);
            _groupByColorQuestionGroups.Add(green);
            _groupByColorTargetDirections.Add(yellowOnLeft ? Direction.Right : Direction.Left);
            _groupByColorTargetDirections.Add(yellowOnLeft ? Direction.Left : Direction.Right);
        }

        private static int[] CreateRandomTwoColorEdgeCounts(Random r, int availableSlots)
        {
            int maxPerSide = Math.Max(1, availableSlots - 1);
            int yellowCount = r.Next(1, maxPerSide + 1);
            int greenMax = Math.Max(1, availableSlots - yellowCount);
            int greenCount = r.Next(1, greenMax + 1);
            return new[] { yellowCount, greenCount };
        }

        private void GenerateThreeColorGroupByColorExercise(Random r, int start, int end)
        {
            int[] counts = Config.KeyboardConfig?.GroupByColorCounts is { Length: >= 3 } configuredCounts
                ? configuredCounts.Take(3).ToArray()
                : new[] { 2, 1, 1 };

            if (Config.KeyboardConfig?.GroupByColorKeepOuterColorsOnSides == true &&
                Config.KeyboardConfig.GroupByColorKeepBlueInMiddle)
            {
                _groupByColorQuestionGroups.Add(BuildRangeBits(BitArrayQuestion.Length, start, counts[0]));
                _groupByColorQuestionGroups.Add(BuildMiddleBlueBits(r, start, end, counts[0], counts[1], counts[2]));
                _groupByColorQuestionGroups.Add(BuildRangeBits(BitArrayQuestion.Length, end - counts[2], counts[2]));
            }
            else
            {
                List<bool[]> groups = new();
                while (groups.Count < 3)
                {
                    bool[] candidate = Config.OnlySequence ? GenerateRandomSequence(r, start, end) : RandomArray(start, end);
                    if (SumArray(candidate) == 0 || groups.Any(existing => AreOverlapingSets(existing, candidate)))
                        continue;

                    groups.Add(candidate);
                }

                _groupByColorQuestionGroups.AddRange(groups);
            }

            AssignGroupByColorDirections(r, _groupByColorQuestionGroups.Count);
        }

        private void GenerateAssociativityThreeColorGroupByColorExercise(Random r, int start, int end)
        {
            int available = Math.Max(3, end - start);
            int[] counts = ResolveGroupByColorCounts(3, available, new[] { 3, 2, 2 });

            bool yellowOnLeft = r.Next(2) == 0;
            bool blueNearYellow = r.Next(2) == 0;

            bool[] yellow;
            bool[] green;
            bool[] blue;

            if (yellowOnLeft)
            {
                yellow = BuildRangeBits(BitArrayQuestion.Length, start, counts[0]);
                green = BuildRangeBits(BitArrayQuestion.Length, end - counts[1], counts[1]);
                blue = blueNearYellow
                    ? BuildRangeBits(BitArrayQuestion.Length, start + counts[0], counts[2])
                    : BuildRangeBits(BitArrayQuestion.Length, end - counts[1] - counts[2], counts[2]);

                _groupByColorTargetDirections.Add(Direction.Left);
                _groupByColorTargetDirections.Add(Direction.Right);
                _groupByColorTargetDirections.Add(blueNearYellow ? Direction.Right : Direction.Left);
            }
            else
            {
                yellow = BuildRangeBits(BitArrayQuestion.Length, end - counts[0], counts[0]);
                green = BuildRangeBits(BitArrayQuestion.Length, start, counts[1]);
                blue = blueNearYellow
                    ? BuildRangeBits(BitArrayQuestion.Length, end - counts[0] - counts[2], counts[2])
                    : BuildRangeBits(BitArrayQuestion.Length, start + counts[1], counts[2]);

                _groupByColorTargetDirections.Add(Direction.Right);
                _groupByColorTargetDirections.Add(Direction.Left);
                _groupByColorTargetDirections.Add(blueNearYellow ? Direction.Left : Direction.Right);
            }

            _groupByColorQuestionGroups.Add(yellow);
            _groupByColorQuestionGroups.Add(green);
            _groupByColorQuestionGroups.Add(blue);
        }

        private void AssignGroupByColorDirections(Random r, int groupCount)
        {
            bool allowSameSideTargets = Config.KeyboardConfig?.GroupByColorAllowSameSideTargets == true;

            if (groupCount <= 0)
                return;

            if (!allowSameSideTargets && groupCount == 2)
            {
                bool firstLeft = r.Next(2) == 0;
                _groupByColorTargetDirections.Add(firstLeft ? Direction.Left : Direction.Right);
                _groupByColorTargetDirections.Add(firstLeft ? Direction.Right : Direction.Left);
                return;
            }

            for (int i = 0; i < groupCount; i++)
                _groupByColorTargetDirections.Add(r.Next(2) == 0 ? Direction.Left : Direction.Right);
        }

        private bool[] BuildMiddleBlueBits(Random r, int start, int end, int leftCount, int blueCount, int rightCount)
        {
            int middleStart = start + leftCount;
            int middleEndExclusive = end - rightCount;
            int maxStart = Math.Max(middleStart, middleEndExclusive - blueCount);
            int from = r.Next(middleStart, maxStart + 1);
            return BuildRangeBits(BitArrayQuestion.Length, from, blueCount);
        }

        private int[] ResolveGroupByColorCounts(int groupCount, int availableSlots, int[] fallbackCounts)
        {
            int[] counts = Config.KeyboardConfig?.GroupByColorCounts is { Length: >= 1 } configuredCounts
                ? configuredCounts.Take(groupCount).Concat(Enumerable.Repeat(1, Math.Max(0, groupCount - configuredCounts.Length))).Take(groupCount).ToArray()
                : fallbackCounts.Take(groupCount).ToArray();

            if (counts.Length < groupCount)
                counts = counts.Concat(Enumerable.Repeat(1, groupCount - counts.Length)).ToArray();

            for (int i = 0; i < counts.Length; i++)
                counts[i] = Math.Max(1, counts[i]);

            int total = counts.Sum();
            while (total > availableSlots)
            {
                int index = Array.IndexOf(counts, counts.Max());
                if (index < 0 || counts[index] <= 1)
                    break;

                counts[index]--;
                total--;
            }

            return counts;
        }

        private static bool[] BuildRangeBits(int totalLength, int from, int count)
        {
            bool[] bits = new bool[totalLength];
            for (int i = 0; i < count && from + i < totalLength; i++)
                bits[from + i] = true;

            return bits;
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
                        foreach (bool[] target in BuildGroupByColorTargetGroups())
                        {
                            for (int i = 0; i < len && i < target.Length; i++)
                                BitArrayCorrectAnswer[i] = BitArrayCorrectAnswer[i] || target[i];
                        }
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

        private List<bool[]> BuildGroupByColorTargetGroups()
        {
            List<bool[]> targets = new();
            if (CurrentOperation != Operation.GroupByColor || _groupByColorQuestionGroups.Count == 0)
                return targets;

            if ((Config.KeyboardConfig?.GroupByColorLayoutMode ?? GroupByColorLayoutMode.Free) == GroupByColorLayoutMode.AssociativityEdges &&
                _groupByColorQuestionGroups.Count >= 3)
            {
                return BuildAssociativityTargetGroups();
            }

            int leftOffset = 0;
            int rightOffset = 0;
            int keyboardLength = BitArrayQuestion.Length;

            for (int i = 0; i < _groupByColorQuestionGroups.Count; i++)
            {
                bool[] source = _groupByColorQuestionGroups[i];
                int count = SumArray(source);
                bool[] target = new bool[keyboardLength];
                Direction direction = _groupByColorTargetDirections.Count > i
                    ? _groupByColorTargetDirections[i]
                    : Direction.Left;

                if (direction == Direction.Left)
                {
                    for (int keyIndex = 0; keyIndex < count && leftOffset + keyIndex < keyboardLength; keyIndex++)
                        target[leftOffset + keyIndex] = true;

                    leftOffset += count;
                }
                else
                {
                    for (int keyIndex = 0; keyIndex < count && rightOffset + keyIndex < keyboardLength; keyIndex++)
                        target[keyboardLength - 1 - rightOffset - keyIndex] = true;

                    rightOffset += count;
                }

                targets.Add(target);
            }

            return targets;
        }

        private List<bool[]> BuildAssociativityTargetGroups()
        {
            List<bool[]> targets = new();
            int keyboardLength = BitArrayQuestion.Length;

            bool yellowTargetsLeft = _groupByColorTargetDirections.Count > 0 && _groupByColorTargetDirections[0] == Direction.Left;
            bool greenTargetsLeft = _groupByColorTargetDirections.Count > 1 && _groupByColorTargetDirections[1] == Direction.Left;
            bool blueTargetsLeft = _groupByColorTargetDirections.Count > 2 && _groupByColorTargetDirections[2] == Direction.Left;

            int yellowCount = SumArray(_groupByColorQuestionGroups[0]);
            int greenCount = SumArray(_groupByColorQuestionGroups[1]);
            int blueCount = SumArray(_groupByColorQuestionGroups[2]);

            bool[] yellow = new bool[keyboardLength];
            bool[] green = new bool[keyboardLength];
            bool[] blue = new bool[keyboardLength];

            int leftOffset = 0;
            int rightOffset = 0;

            if (yellowTargetsLeft)
            {
                FillRange(yellow, leftOffset, yellowCount, true);
                leftOffset += yellowCount;
                if (blueTargetsLeft)
                    FillRange(blue, leftOffset, blueCount, true);
            }
            else
            {
                FillRange(yellow, rightOffset, yellowCount, false);
                rightOffset += yellowCount;
                if (!blueTargetsLeft)
                    FillRange(blue, rightOffset, blueCount, false);
            }

            if (greenTargetsLeft)
            {
                FillRange(green, leftOffset, greenCount, true);
                leftOffset += greenCount;
                if (blueTargetsLeft && !yellowTargetsLeft)
                    FillRange(blue, leftOffset, blueCount, true);
            }
            else
            {
                FillRange(green, rightOffset, greenCount, false);
                rightOffset += greenCount;
                if (!blueTargetsLeft && yellowTargetsLeft)
                    FillRange(blue, rightOffset, blueCount, false);
            }

            targets.Add(yellow);
            targets.Add(green);
            targets.Add(blue);
            return targets;
        }

        private static void FillRange(bool[] target, int offset, int count, bool fromLeft)
        {
            for (int keyIndex = 0; keyIndex < count && keyIndex < target.Length; keyIndex++)
            {
                int absoluteIndex = fromLeft
                    ? offset + keyIndex
                    : target.Length - 1 - offset - keyIndex;

                if (absoluteIndex >= 0 && absoluteIndex < target.Length)
                    target[absoluteIndex] = true;
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
