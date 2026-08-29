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
        private const int ArrowGroupPressMaxGapMs = 220;
        private const int ArrowSequenceMinGapMs = 220;
        private const int ArrowSplitMinGapMs = 260;
        private const int ArrowAttemptResetGapMs = 2000;

        private int _nextArrowAboveNumber = 1;
        private Direction _prevDir = Direction.Right;
        public Direction dir = Direction.Right;
        public int aboveNumber;
        public int length;

        public Direction moveBydir = Direction.Right;
        public int moveByLength =0;
        private int _precisionShiftDelta = 0;
        private int _precisionShiftLeftDelta;
        private int _precisionShiftRightDelta;
        private bool _precisionShiftLeftBaseAtTop;
        private bool _precisionShiftRightBaseAtTop;
        private bool _precisionShiftLeftIsShift;
        private bool _precisionShiftRightIsShift;
        private string _precisionGrammarCondition = string.Empty;
        private bool _isPrecisionShiftAsMinus;
        private bool[]? _sequenceMemorizeFirstPinch;
        private bool[]? _sequenceMemorizeSecondPinch;
        private bool _sequenceMemorizeGenerateSecond;
        private bool _sequenceMemorizeCurrentIsFirst;
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
        private bool _usesRtlComplexPrompt;
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
        private ArrowMovementMode _activeArrowMovementMode = ArrowMovementMode.Legacy;
        private string _lastArrowMovementDebugText = string.Empty;
        private bool[]? _stagedArrowFirstBits;
        private bool[]? _stagedArrowSecondBits;
        private readonly List<bool[]> _groupByColorQuestionGroups = new();
        private readonly List<Direction> _groupByColorTargetDirections = new();

        public List<int> triads = new();


        private void GenerateArrowExercise()
        {
            var r = new Random();
            SelectActiveArrowMovementMode(r);
            var (fromIndex, lengthIndexes) = PrepareArrowFields(r);
            SetBitArrayForArrow(fromIndex, lengthIndexes);
            CaptureStagedArrowOverlayState();
            SetArrowMovementDebug("generated");
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
            if (GetCurrentArrowMovementMode() is ArrowMovementMode.JumpToEnd or ArrowMovementMode.OneByOne or ArrowMovementMode.JumpThroughMiddle)
                return ArrowType.Rounded;

            if (SupportsComposedArrowVariants())
                return _activeArrowType;

            return Config?.KeyboardConfig?.ArrowType ?? ArrowType.Straight;
        }

        private ArrowMovementMode GetCurrentArrowMovementMode()
        {
            return _activeArrowMovementMode != ArrowMovementMode.Legacy
                ? _activeArrowMovementMode
                : Config?.KeyboardConfig?.ArrowMovementMode ?? ArrowMovementMode.Legacy;
        }

        private void SelectActiveArrowMovementMode(Random r)
        {
            KeyboardConfig? keyboardConfig = Config?.KeyboardConfig;
            _activeArrowMovementMode = ArrowMovementMode.Legacy;
            if (keyboardConfig == null)
                return;

            List<ArrowMovementMode> movementModes = GetAllowedArrowMovementModes(keyboardConfig.AllowedArrowMovementModes);
            if (movementModes.Count == 0)
            {
                _activeArrowMovementMode = keyboardConfig.ArrowMovementMode;
                return;
            }

            int index = r.Next(movementModes.Count);
            _activeArrowMovementMode = movementModes[index];
        }

        private static List<ArrowMovementMode> GetAllowedArrowMovementModes(ArrowMovementModeFlags flags)
        {
            List<ArrowMovementMode> modes = new();
            if (flags.HasFlag(ArrowMovementModeFlags.AllTogether))
                modes.Add(ArrowMovementMode.AllTogether);
            if (flags.HasFlag(ArrowMovementModeFlags.Arpeggio))
                modes.Add(ArrowMovementMode.Arpeggio);
            if (flags.HasFlag(ArrowMovementModeFlags.Splited))
                modes.Add(ArrowMovementMode.Splited);
            if (flags.HasFlag(ArrowMovementModeFlags.MiddleSplited))
                modes.Add(ArrowMovementMode.MiddleSplited);
            if (flags.HasFlag(ArrowMovementModeFlags.JumpToEnd))
                modes.Add(ArrowMovementMode.JumpToEnd);
            if (flags.HasFlag(ArrowMovementModeFlags.OneByOne))
                modes.Add(ArrowMovementMode.OneByOne);
            if (flags.HasFlag(ArrowMovementModeFlags.JumpThroughMiddle))
                modes.Add(ArrowMovementMode.JumpThroughMiddle);

            return modes;
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

        private static bool IsComplexArrowLabelPromptMode(ArrowLabelExerciseMode mode)
        {
            return mode is ArrowLabelExerciseMode.ComplexBridgeToNextTen or
                ArrowLabelExerciseMode.ComplexBridgeToAnyNextTen or
                ArrowLabelExerciseMode.ComplexLongDistance;
        }

        private bool UsesRtlComplexThroughTenPrompt()
        {
            return _usesRtlComplexPrompt ||
                   Config?.GameName?.Contains("rtl complex", StringComparison.OrdinalIgnoreCase) == true;
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
                ArrowLabelExerciseMode.ComplexBridgeToAnyNextTen or
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
                        IsComplexArrowLabelPromptMode(keyboardConfig.ArrowLabelExerciseMode);
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
                        variants.Add((false, useConfiguredComplexPrompt
                            ? keyboardConfig.ArrowLabelExerciseMode
                            : ArrowLabelExerciseMode.EndAndLengthWithMissingStart, MissingValueTargetFlags.Addend1, ArrowType.Straight));
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
            _usesRtlComplexPrompt = false;

            switch (GetCurrentArrowLabelExerciseMode())
            {
                case ArrowLabelExerciseMode.ComplexBridgeToNextTen:
                {
                    List<(int Start, int End, int Middle)> candidates = new();
                    _usesRtlComplexPrompt = Config?.GameName?.Contains("rtl complex", StringComparison.OrdinalIgnoreCase) == true;
                    if (_usesRtlComplexPrompt)
                    {
                        const int middle = 10;
                        for (int start = Math.Max(minValue, 1); start < middle; start++)
                        {
                            for (int end = middle + 1; end <= Math.Min(maxValue, 19); end++)
                            {
                                int distance = end - start;
                                if (distance >= 10 || distance > maxArrowLabelDistance)
                                    continue;

                                candidates.Add((start, end, middle));
                            }
                        }
                    }
                    else
                    {
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

                case ArrowLabelExerciseMode.ComplexBridgeToAnyNextTen:
                {
                    List<(int Start, int End, int Middle)> candidates = new();
                    bool learnerChosenMiddle = Config?.KeyboardConfig?.AllowLearnerChosenComplexMiddle == true;
                    _usesRtlComplexPrompt = !learnerChosenMiddle &&
                                            Config?.KeyboardConfig?.AllowRtlComplexPrompts == true &&
                                            Random.Shared.Next(2) == 0;
                    for (int middle = 20; middle < maxValue; middle += 10)
                    {
                        int startMin = Math.Max(minValue, middle - (learnerChosenMiddle ? 89 : 9));
                        int startMax = middle - 1;
                        int endMin = middle + 1;
                        int endMax = Math.Min(maxValue, middle + (learnerChosenMiddle ? 89 : 9));

                        if (startMin > startMax || endMin > endMax)
                            continue;

                        for (int start = startMin; start <= startMax; start++)
                        {
                            for (int end = endMin; end <= endMax; end++)
                            {
                                int distance = end - start;
                                if ((!learnerChosenMiddle && distance > 9) ||
                                    distance > maxArrowLabelDistance ||
                                    (learnerChosenMiddle && (start < 10 || end < 10 || distance < 10)))
                                {
                                    continue;
                                }

                                candidates.Add((start, end, middle));
                            }
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

            if (UsesRtlComplexThroughTenPrompt() &&
                IsComplexArrowLabelPromptMode(GetCurrentArrowLabelExerciseMode()) &&
                GetCurrentArrowLabelMissingTarget() == MissingValueTargetFlags.Sum)
            {
                SetActiveArrowLabelMissingTarget(MissingValueTargetFlags.TotalDistance);
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
            IsComplexArrowLabelPromptMode(GetCurrentArrowLabelExerciseMode())
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
        public bool UsesRtlComplexPrompt => UsesRtlComplexThroughTenPrompt();

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
            EnsureSplitedArrowIsShort(r, keys, ref fromIndex, ref lengthIndexes);
            EnsureMiddleSplitedArrowIsShort(r, keys, ref fromIndex, ref lengthIndexes);
            EnsureJumpThroughMiddleHasMiddle(r, keys, ref fromIndex, ref lengthIndexes);
            return (fromIndex, lengthIndexes);
        }

        private void EnsureSplitedArrowIsShort(Random r, int keys, ref int fromIndex, ref int lengthIndexes)
        {
            if (GetCurrentArrowMovementMode() != ArrowMovementMode.Splited)
                return;

            int maxDistance = GetMaxArrowLabelDistance(keys - 1);
            if (lengthIndexes > 1 &&
                lengthIndexes <= maxDistance &&
                !ArrowWrapsKeyboardBoundary(aboveNumber, lengthIndexes, dir, keys))
                return;

            List<(int Above, int Length, Direction Direction)> candidates = new();
            AddArrowDistanceCandidates(candidates, Direction.Right, keys, maxDistance, mustCrossTen: false);
            AddArrowDistanceCandidates(candidates, Direction.Left, keys, maxDistance, mustCrossTen: false);
            candidates = candidates
                .Where(candidate => candidate.Length > 1 &&
                                    !ArrowWrapsKeyboardBoundary(candidate.Above, candidate.Length, candidate.Direction, keys))
                .ToList();

            if (candidates.Count == 0)
                return;

            (aboveNumber, length, dir) = candidates[r.Next(candidates.Count)];
            lengthIndexes = length;
            fromIndex = dir == Direction.Left
                ? (aboveNumber - length + keys) % keys
                : aboveNumber - 1;
        }

        private void EnsureMiddleSplitedArrowIsShort(Random r, int keys, ref int fromIndex, ref int lengthIndexes)
        {
            if (GetCurrentArrowMovementMode() != ArrowMovementMode.MiddleSplited)
                return;

            int maxDistance = GetMaxArrowLabelDistance(keys - 1);
            if (lengthIndexes > 1 && lengthIndexes <= maxDistance)
                return;

            List<(int Above, int Length, Direction Direction)> candidates = new();
            AddArrowDistanceCandidates(candidates, Direction.Right, keys, maxDistance, mustCrossTen: false);
            AddArrowDistanceCandidates(candidates, Direction.Left, keys, maxDistance, mustCrossTen: false);
            candidates = candidates.Where(candidate => candidate.Length > 1).ToList();

            if (candidates.Count == 0)
                return;

            (aboveNumber, length, dir) = candidates[r.Next(candidates.Count)];
            lengthIndexes = length;
            fromIndex = dir == Direction.Left
                ? (aboveNumber - length + keys) % keys
                : aboveNumber - 1;
        }

        private void EnsureJumpThroughMiddleHasMiddle(Random r, int keys, ref int fromIndex, ref int lengthIndexes)
        {
            if (GetCurrentArrowMovementMode() != ArrowMovementMode.JumpThroughMiddle)
                return;

            List<(int Above, int Length, Direction Direction)> candidates = new();
            AddArrowDistanceCandidates(candidates, Direction.Right, keys, 2, mustCrossTen: false);
            AddArrowDistanceCandidates(candidates, Direction.Left, keys, 2, mustCrossTen: false);
            candidates = candidates.Where(candidate => candidate.Length == 2).ToList();

            if (candidates.Count == 0)
                return;

            (aboveNumber, length, dir) = candidates[r.Next(candidates.Count)];
            lengthIndexes = length;
            fromIndex = dir == Direction.Left
                ? (aboveNumber - length + keys) % keys
                : aboveNumber - 1;
        }

        private void ApplyOnKeyboardArrowDistanceConstraints(Random r, int keys, ref int fromIndex, ref int lengthIndexes)
        {
            int configuredMaxDistance = Config?.KeyboardConfig?.MaxArrowLabelDistance ?? 0;
            bool needsDistanceCap = configuredMaxDistance > 0;
            bool needsThroughTen = Config?.OnlyThrougTen == true;
            bool needsToTen = Config?.OnlyToTen == true &&
                              Config?.KeyboardConfig?.EnableSecondArrowLeftTrace == true;
            if (!needsDistanceCap && !needsThroughTen && !needsToTen)
                return;

            int maxDistance = needsDistanceCap
                ? Math.Max(1, Math.Min(keys - 1, configuredMaxDistance))
                : keys - 1;

            bool wrapsKeyboardBoundary = ArrowWrapsKeyboardBoundary(aboveNumber, lengthIndexes, dir, keys);
            bool currentIsValid =
                lengthIndexes <= maxDistance &&
                (!needsThroughTen || wrapsKeyboardBoundary) &&
                (!needsToTen || !wrapsKeyboardBoundary);
            if (currentIsValid)
                return;

            List<(int Above, int Length, Direction Direction)> candidates = new();
            AddArrowDistanceCandidates(candidates, Direction.Right, keys, maxDistance, needsThroughTen, needsToTen);
            AddArrowDistanceCandidates(candidates, Direction.Left, keys, maxDistance, needsThroughTen, needsToTen);

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
            bool mustCrossTen,
            bool mustStayWithinTen = false)
        {
            for (int start = 1; start <= keys; start++)
            {
                for (int candidateLength = 1; candidateLength <= maxDistance; candidateLength++)
                {
                    bool wrapsKeyboardBoundary = ArrowWrapsKeyboardBoundary(start, candidateLength, candidateDirection, keys);
                    if (mustCrossTen && !wrapsKeyboardBoundary)
                        continue;

                    if (mustStayWithinTen && wrapsKeyboardBoundary)
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
                ArrowLabelExerciseMode.ComplexBridgeToAnyNextTen =>
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
            ArrowMovementMode movementMode = GetCurrentArrowMovementMode();
            bool result = CheckOnly(submittedKeyboard);
            DateTime submittedTime = DateTime.Now;
            if (result)
                result = await CheckArrowMovementTimingAsync();

            _status = result ? Statement.True : Statement.False;
            IncrementGuessNumber();
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

        private async Task<bool> CheckArrowMovementTimingAsync()
        {
            ArrowMovementMode movementMode = GetCurrentArrowMovementMode();
            if (!UsesOnKeyboardArrowExercise() || movementMode == ArrowMovementMode.Legacy)
                return true;

            if (_keyEventRepository == null)
            {
                SetArrowMovementDebug("timing skipped: no repository");
                return true;
            }

            List<Data.SQLite.KeyEvent> pendingEvents = await _keyEventRepository.GetPendingEventsAsync(GameId.ToString(), _questionNumber);
            List<Data.SQLite.KeyEvent> rawDownEvents = pendingEvents
                .Where(item => item.EventType == 1 && item.KeyNumber > 0)
                .OrderBy(item => item.EventTime)
                .ThenBy(item => item.id)
                .ToList();

            List<int> expectedKeys = GetExpectedArrowMovementKeyNumbers(movementMode);

            if (expectedKeys.Count == 0 || rawDownEvents.Count == 0)
            {
                SetArrowMovementDebug("timing failed: missing expected or actual key-downs", expectedKeys, rawDownEvents);
                return false;
            }

            List<Data.SQLite.KeyEvent> attemptEvents = GetLatestArrowAttemptByIdleGap(rawDownEvents);
            List<Data.SQLite.KeyEvent> downEvents = GetLatestArrowAttemptDownEvents(attemptEvents, expectedKeys.Count, movementMode);
            string attemptWindowDebug = rawDownEvents.Count == downEvents.Count
                ? string.Empty
                : $" raw={FormatKeys(rawDownEvents.Select(item => item.KeyNumber))}";

            string reason;
            bool isCorrect;
            switch (movementMode)
            {
                case ArrowMovementMode.AllTogether:
                    isCorrect = MatchesAllTogetherTiming(downEvents, expectedKeys, out reason);
                    break;
                case ArrowMovementMode.Arpeggio:
                    isCorrect = MatchesSequentialTiming(downEvents, expectedKeys, out reason);
                    break;
                case ArrowMovementMode.Splited:
                    isCorrect = MatchesSplitTiming(downEvents, expectedKeys, GetVisibleArrowSplitFirstCount(expectedKeys.Count), out reason);
                    break;
                case ArrowMovementMode.MiddleSplited:
                    isCorrect = MatchesSplitTiming(downEvents, expectedKeys, (int)Math.Ceiling(expectedKeys.Count / 2.0), out reason);
                    break;
                case ArrowMovementMode.JumpToEnd:
                    isCorrect = MatchesJumpToEndCurrentAttemptTiming(downEvents, expectedKeys, out reason);
                    break;
                case ArrowMovementMode.JumpThroughMiddle:
                    isCorrect = MatchesSequentialTiming(downEvents, expectedKeys, out reason);
                    break;
                case ArrowMovementMode.OneByOne:
                    isCorrect = MatchesSequentialTiming(downEvents, expectedKeys, out reason);
                    break;
                default:
                    isCorrect = true;
                    reason = "timing skipped";
                    break;
            }

            SetArrowMovementDebug(isCorrect ? $"timing ok: {reason}{attemptWindowDebug}" : $"timing failed: {reason}{attemptWindowDebug}", expectedKeys, downEvents);
            return isCorrect;
        }

        private static List<Data.SQLite.KeyEvent> GetLatestArrowAttemptDownEvents(
            IReadOnlyList<Data.SQLite.KeyEvent> downEvents,
            int expectedKeyCount,
            ArrowMovementMode movementMode)
        {
            if (movementMode == ArrowMovementMode.JumpToEnd)
                return downEvents.ToList();

            int attemptKeyCount = expectedKeyCount;
            if (attemptKeyCount <= 0 || downEvents.Count <= attemptKeyCount)
                return downEvents.ToList();

            return downEvents.Skip(downEvents.Count - attemptKeyCount).ToList();
        }

        private static List<Data.SQLite.KeyEvent> GetLatestArrowAttemptByIdleGap(IReadOnlyList<Data.SQLite.KeyEvent> downEvents)
        {
            if (downEvents.Count <= 1)
                return downEvents.ToList();

            int startIndex = 0;
            for (int i = 1; i < downEvents.Count; i++)
            {
                int gapMs = ToMilliseconds(downEvents[i].EventTime - downEvents[i - 1].EventTime);
                if (gapMs >= ArrowAttemptResetGapMs)
                    startIndex = i;
            }

            return downEvents.Skip(startIndex).ToList();
        }

        private List<int> GetExpectedArrowMovementKeyNumbers(ArrowMovementMode movementMode)
        {
            List<int> routeKeys = GetArrowTutorialStepIndices()
                .Select(index => index + 1)
                .Where(keyNumber => keyNumber > 0)
                .ToList();

            if (movementMode == ArrowMovementMode.JumpToEnd && routeKeys.Count > 0)
                return new List<int> { routeKeys[^1] };

            if (movementMode != ArrowMovementMode.JumpThroughMiddle || routeKeys.Count <= 1)
                return routeKeys;

            int middleIndex = Math.Max(0, (routeKeys.Count - 1) / 2);
            int endKey = routeKeys[^1];
            int middleKey = routeKeys[middleIndex];
            return middleKey == endKey
                ? new List<int> { endKey }
                : new List<int> { middleKey, endKey };
        }

        private static bool MatchesAllTogetherTiming(IReadOnlyList<Data.SQLite.KeyEvent> downEvents, IReadOnlyList<int> expectedKeys, out string reason)
        {
            if (downEvents.Count != expectedKeys.Count)
            {
                reason = $"count actual {downEvents.Count}, expected {expectedKeys.Count}";
                return false;
            }

            if (!downEvents.Select(item => item.KeyNumber).OrderBy(key => key)
                    .SequenceEqual(expectedKeys.OrderBy(key => key)))
            {
                reason = "keys differ";
                return false;
            }

            int maxGapMs = GetMaxGapMs(downEvents);
            reason = $"max gap {maxGapMs}ms <= {ArrowGroupPressMaxGapMs}ms";
            return maxGapMs <= ArrowGroupPressMaxGapMs;
        }

        private static bool MatchesSequentialTiming(IReadOnlyList<Data.SQLite.KeyEvent> downEvents, IReadOnlyList<int> expectedKeys, out string reason)
        {
            if (downEvents.Count != expectedKeys.Count)
            {
                reason = $"count actual {downEvents.Count}, expected {expectedKeys.Count}";
                return false;
            }

            if (!downEvents.Select(item => item.KeyNumber).SequenceEqual(expectedKeys))
            {
                reason = "order differs";
                return false;
            }

            int minGapMs = GetMinGapMs(downEvents);
            reason = downEvents.Count <= 1
                ? "single key"
                : $"min gap {minGapMs}ms >= {ArrowSequenceMinGapMs}ms";
            return downEvents.Count <= 1 || minGapMs >= ArrowSequenceMinGapMs;
        }

        private static bool MatchesSplitTiming(IReadOnlyList<Data.SQLite.KeyEvent> downEvents, IReadOnlyList<int> expectedKeys, int firstCount, out string reason)
        {
            if (downEvents.Count != expectedKeys.Count || expectedKeys.Count <= 1)
            {
                reason = $"count actual {downEvents.Count}, expected {expectedKeys.Count}";
                return false;
            }

            firstCount = Math.Max(1, Math.Min(expectedKeys.Count - 1, firstCount));
            List<Data.SQLite.KeyEvent> firstEvents = downEvents.Take(firstCount).ToList();
            List<Data.SQLite.KeyEvent> secondEvents = downEvents.Skip(firstCount).ToList();

            if (secondEvents.Count == 0)
            {
                reason = "missing second group";
                return false;
            }

            if (!firstEvents.Select(item => item.KeyNumber).OrderBy(key => key)
                    .SequenceEqual(expectedKeys.Take(firstCount).OrderBy(key => key)))
            {
                reason = "first group keys differ";
                return false;
            }

            if (!secondEvents.Select(item => item.KeyNumber).OrderBy(key => key)
                    .SequenceEqual(expectedKeys.Skip(firstCount).OrderBy(key => key)))
            {
                reason = "second group keys differ";
                return false;
            }

            int splitGapMs = ToMilliseconds(secondEvents[0].EventTime - firstEvents[^1].EventTime);
            if (splitGapMs < ArrowSplitMinGapMs)
            {
                reason = $"split gap {splitGapMs}ms < {ArrowSplitMinGapMs}ms";
                return false;
            }

            int firstMaxGapMs = GetMaxGapMs(firstEvents);
            int secondMaxGapMs = GetMaxGapMs(secondEvents);
            reason = $"split gap {splitGapMs}ms, group gaps {firstMaxGapMs}/{secondMaxGapMs}ms";
            return firstMaxGapMs <= ArrowGroupPressMaxGapMs &&
                   secondMaxGapMs <= ArrowGroupPressMaxGapMs;
        }

        private static bool MatchesJumpToEndCurrentAttemptTiming(IReadOnlyList<Data.SQLite.KeyEvent> downEvents, IReadOnlyList<int> expectedKeys, out string reason)
        {
            if (downEvents.Count != expectedKeys.Count)
            {
                reason = $"current attempt count actual {downEvents.Count}, expected {expectedKeys.Count}";
                return false;
            }

            if (!downEvents.Select(item => item.KeyNumber).SequenceEqual(expectedKeys))
            {
                reason = "end key differs";
                return false;
            }

            reason = $"single end key {expectedKeys[^1]}";
            return true;
        }

        private void SetArrowMovementDebug(string reason)
        {
            List<int> expectedKeys = GetExpectedArrowMovementKeyNumbers(GetCurrentArrowMovementMode());

            SetArrowMovementDebug(reason, expectedKeys, null);
        }

        private void SetArrowMovementDebug(
            string reason,
            IReadOnlyList<int> expectedKeys,
            IReadOnlyList<Data.SQLite.KeyEvent>? downEvents)
        {
            string actualText = downEvents == null
                ? "-"
                : FormatKeys(downEvents.Select(item => item.KeyNumber));
            _lastArrowMovementDebugText =
                $"ArrowDebug mode={GetCurrentArrowMovementMode()} expected={FormatKeys(expectedKeys)} actual={actualText} {reason}";
            Console.WriteLine(_lastArrowMovementDebugText);
            DevLog.Write(_lastArrowMovementDebugText);
        }

        private static string FormatKeys(IEnumerable<int> keys)
        {
            return string.Join(",", keys);
        }

        private int GetVisibleArrowSplitFirstCount(int tutorialStepCount)
        {
            if (tutorialStepCount <= 1)
                return tutorialStepCount;

            int keyCount = BitArrayQuestion?.Length ?? 0;
            if (keyCount <= 0)
                return Math.Max(1, tutorialStepCount / 2);

            int firstCount = dir == Direction.Right
                ? keyCount - aboveNumber + 1
                : aboveNumber;

            if (firstCount <= 0 || firstCount >= tutorialStepCount)
                firstCount = (int)Math.Ceiling(tutorialStepCount / 2.0);

            return Math.Max(1, Math.Min(tutorialStepCount - 1, firstCount));
        }

        private static int GetMaxGapMs(IReadOnlyList<Data.SQLite.KeyEvent> events)
        {
            if (events.Count <= 1)
                return 0;

            int maxGap = 0;
            for (int i = 1; i < events.Count; i++)
                maxGap = Math.Max(maxGap, ToMilliseconds(events[i].EventTime - events[i - 1].EventTime));

            return maxGap;
        }

        private static int GetMinGapMs(IReadOnlyList<Data.SQLite.KeyEvent> events)
        {
            if (events.Count <= 1)
                return int.MaxValue;

            int minGap = int.MaxValue;
            for (int i = 1; i < events.Count; i++)
                minGap = Math.Min(minGap, ToMilliseconds(events[i].EventTime - events[i - 1].EventTime));

            return minGap;
        }

        private static int ToMilliseconds(TimeSpan duration)
        {
            if (duration < TimeSpan.Zero)
                duration = TimeSpan.Zero;

            return (int)Math.Round(duration.TotalMilliseconds);
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
                case ArrowLabelExerciseMode.ComplexBridgeToAnyNextTen:
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

        private bool[] GenerateTwoKeyPinch(Random random, int start, int end)
        {
            bool[] pinch = new bool[BitArrayQuestion.Length];
            if (Config.KeyboardConfig?.IsVerticalPrecisionPinchExercise == true)
            {
                int maxInterval = Math.Clamp(
                    Config.KeyboardConfig.PrecisionPinchMaxInterval,
                    1,
                    Math.Max(1, Config.KeyboardConfig.Rows - 1));
                bool moveUpperOnly = Config.KeyboardConfig.IsPrecisionShiftExercise &&
                                     Config.KeyboardConfig.PrecisionPinchMoveOptions == PrecisionPinchMoveOptions.MoveUpper;
                if (Config.KeyboardConfig.PrecisionShiftBothHands)
                {
                    if (Config.KeyboardConfig.IsPrecisionSynchronousProcessExercise)
                    {
                        // Both hands begin with the same grip. Keep at least one empty
                        // row between the fingers so one-step squeeze commands are legal.
                        int maximumInterval = Math.Min(Config.KeyboardConfig.Rows - 1, maxInterval);
                        int interval = maximumInterval >= 2
                            ? random.Next(2, maximumInterval + 1)
                            : maximumInterval;
                        int lowerRow = random.Next(0, Config.KeyboardConfig.Rows - interval);
                        int upperRow = lowerRow + interval;
                        for (int column = 0; column < Config.KeyboardConfig.KeysInRow; column++)
                        {
                            pinch[(lowerRow * Config.KeyboardConfig.KeysInRow) + column] = true;
                            pinch[(upperRow * Config.KeyboardConfig.KeysInRow) + column] = true;
                        }
                        return pinch;
                    }

                    if (Config.KeyboardConfig.IsPrecisionGrammarExercise)
                    {
                        GeneratePrecisionGrammarStartingPinches(random, pinch, maxInterval);
                        return pinch;
                    }

                    if (moveUpperOnly)
                    {
                        // The lower finger is the permanent base in this stage. Logical
                        // row 0 is the visually bottom row; choose the moving finger above it.
                        for (int column = 0; column < Config.KeyboardConfig.KeysInRow; column++)
                        {
                            int upperRow = random.Next(1, Math.Min(Config.KeyboardConfig.Rows - 1, maxInterval) + 1);
                            pinch[column] = true;
                            pinch[(upperRow * Config.KeyboardConfig.KeysInRow) + column] = true;
                        }
                        return pinch;
                    }

                    if (!Config.KeyboardConfig.IsPrecisionShiftExercise)
                    {
                        // In two-hand vertical COPY, each hand has its own pinch.
                        // The two row pairs may coincide naturally, but are not coupled.
                        for (int column = 0; column < Config.KeyboardConfig.KeysInRow; column++)
                        {
                            int handFirstRow = random.Next(Config.KeyboardConfig.Rows);
                            int handSecondRow;
                            do
                            {
                                handSecondRow = random.Next(Config.KeyboardConfig.Rows);
                            }
                            while (handSecondRow == handFirstRow ||
                                   Math.Abs(handSecondRow - handFirstRow) > maxInterval);

                            pinch[(handFirstRow * Config.KeyboardConfig.KeysInRow) + column] = true;
                            pinch[(handSecondRow * Config.KeyboardConfig.KeysInRow) + column] = true;
                        }
                        return pinch;
                    }

                    if (Config.KeyboardConfig.PrecisionShiftStaggerHandsInitially &&
                        Config.KeyboardConfig.Rows >= 5 &&
                        Config.KeyboardConfig.KeysInRow >= 2)
                    {
                        // Stage 9 starts with the left pinch high and the right pinch low.
                        pinch[Config.KeyboardConfig.KeysInRow] = true;
                        pinch[(2 * Config.KeyboardConfig.KeysInRow)] = true;
                        pinch[1] = true;
                        pinch[Config.KeyboardConfig.KeysInRow + 1] = true;
                        return pinch;
                    }

                    int firstRow = random.Next(Config.KeyboardConfig.Rows);
                    int secondRow;
                    do
                    {
                        secondRow = random.Next(Config.KeyboardConfig.Rows);
                    }
                    while (secondRow == firstRow ||
                           Math.Abs(secondRow - firstRow) > maxInterval ||
                           (Math.Min(firstRow, secondRow) == 0 &&
                            Math.Max(firstRow, secondRow) == Config.KeyboardConfig.Rows - 1) ||
                           IsTerminalUpperOnlyPinch(firstRow, secondRow));

                    for (int column = 0; column < Config.KeyboardConfig.KeysInRow; column++)
                    {
                        pinch[(firstRow * Config.KeyboardConfig.KeysInRow) + column] = true;
                        pinch[(secondRow * Config.KeyboardConfig.KeysInRow) + column] = true;
                    }
                    return pinch;
                }

                int firstColumn = Config.KeyboardConfig.PrecisionShiftBothHands ? 0 : random.Next(0, 2);
                int lastColumn = Config.KeyboardConfig.PrecisionShiftBothHands ? 1 : firstColumn;
                for (int column = firstColumn; column <= lastColumn; column++)
                {
                    if (moveUpperOnly)
                    {
                        int upperRow = random.Next(1, Math.Min(Config.KeyboardConfig.Rows - 1, maxInterval) + 1);
                        pinch[column] = true;
                        pinch[(upperRow * Config.KeyboardConfig.KeysInRow) + column] = true;
                        continue;
                    }

                    int[] columnIndices = Enumerable.Range(0, Config.KeyboardConfig.Rows)
                        .Select(row => (row * Config.KeyboardConfig.KeysInRow) + column)
                        .Where(index => index >= 0 && index < pinch.Length)
                        .ToArray();
                    int firstPosition = random.Next(columnIndices.Length);
                    int secondPosition;
                    do
                    {
                        secondPosition = random.Next(columnIndices.Length);
                    }
                    while (secondPosition == firstPosition ||
                           Math.Abs(secondPosition - firstPosition) > maxInterval ||
                           (Config.KeyboardConfig.IsPrecisionShiftExercise &&
                            Math.Min(firstPosition, secondPosition) == 0 &&
                            Math.Max(firstPosition, secondPosition) == columnIndices.Length - 1) ||
                           IsTerminalUpperOnlyPinch(firstPosition, secondPosition));

                    pinch[columnIndices[firstPosition]] = true;
                    pinch[columnIndices[secondPosition]] = true;
                }
                return pinch;
            }

            if (Config.KeyboardConfig?.PrecisionShiftBothHands == true)
            {
                int half = pinch.Length / 2;
                AddTwoRandomKeys(random, pinch, 0, half);
                AddTwoRandomKeys(random, pinch, half, pinch.Length);
                return pinch;
            }

            int first = random.Next(start, end);
            int second;
            do
            {
                second = random.Next(start, end);
            }
            while (second == first);

            pinch[first] = true;
            pinch[second] = true;
            return pinch;
        }

        private void GeneratePrecisionGrammarStartingPinches(Random random, bool[] pinch, int maxInterval)
        {
            int rows = Math.Max(2, Config.KeyboardConfig.Rows);
            int columns = Math.Max(2, Config.KeyboardConfig.KeysInRow);

            // Equal starts are deliberately uncommon. They belong to the dedicated
            // "start equal" condition and always grow upward from the bottom row.
            if (random.Next(100) < 20)
            {
                int maximumUpperRow = Math.Min(rows - 1, maxInterval);
                int upperRow = maximumUpperRow >= 2
                    ? random.Next(2, maximumUpperRow + 1)
                    : maximumUpperRow;
                for (int column = 0; column < Math.Min(2, columns); column++)
                {
                    pinch[column] = true;
                    pinch[(upperRow * columns) + column] = true;
                }
                return;
            }

            List<(int LowBottom, int LowTop, int HighBottom, int HighTop)> stacked = new();
            for (int lowBottom = 0; lowBottom < rows; lowBottom++)
            for (int lowTop = lowBottom + 2; lowTop < rows; lowTop++)
            {
                int highBottom = lowTop + 1;
                if (highBottom >= rows)
                    continue;
                for (int highTop = highBottom + 2; highTop < rows; highTop++)
                {
                    if (lowTop - lowBottom <= maxInterval && highTop - highBottom <= maxInterval)
                        stacked.Add((lowBottom, lowTop, highBottom, highTop));
                }
            }

            if (stacked.Count == 0)
            {
                // Very short keyboards cannot contain two separated two-key grips.
                // Keep the fallback deterministic and bottom-anchored.
                int upperRow = Math.Min(rows - 1, Math.Max(1, maxInterval));
                for (int column = 0; column < Math.Min(2, columns); column++)
                {
                    pinch[column] = true;
                    pinch[(upperRow * columns) + column] = true;
                }
                return;
            }

            var choice = stacked[random.Next(stacked.Count)];
            bool leftHandIsHigher = random.Next(2) == 0;
            int leftBottom = leftHandIsHigher ? choice.HighBottom : choice.LowBottom;
            int leftTop = leftHandIsHigher ? choice.HighTop : choice.LowTop;
            int rightBottom = leftHandIsHigher ? choice.LowBottom : choice.HighBottom;
            int rightTop = leftHandIsHigher ? choice.LowTop : choice.HighTop;
            pinch[leftBottom * columns] = true;
            pinch[leftTop * columns] = true;
            pinch[(rightBottom * columns) + 1] = true;
            pinch[(rightTop * columns) + 1] = true;
        }

        private bool IsTerminalUpperOnlyPinch(int firstRow, int secondRow)
        {
            KeyboardConfig keyboard = Config.KeyboardConfig;
            return keyboard.IsPrecisionShiftExercise &&
                   keyboard.PrecisionPinchMoveOptions == PrecisionPinchMoveOptions.MoveUpper &&
                   Math.Min(firstRow, secondRow) == keyboard.Rows - 2 &&
                   Math.Max(firstRow, secondRow) == keyboard.Rows - 1;
        }

        private static void AddTwoRandomKeys(Random random, bool[] bits, int start, int end)
        {
            int first = random.Next(start, end);
            int second;
            do
            {
                second = random.Next(start, end);
            }
            while (second == first ||
                   (Math.Min(first, second) == start && Math.Max(first, second) == end - 1));

            bits[first] = true;
            bits[second] = true;
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
                ArrowLabelExerciseMode.ComplexBridgeToAnyNextTen =>
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
            if (Config.KeyboardConfig?.IsPrecisionPinchSequenceMemorize == true)
            {
                ResolveSequenceMemorizeQuestion(r);
                return;
            }

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
                    if (Config.KeyboardConfig?.IsTransformativePrecisionCopyExercise == true)
                    {
                        TransformPrecisionCopyQuestion(r);
                    }
                    else if (Config.KeyboardConfig?.IsPrecisionShiftExercise == true)
                    {
                        RefreshPrecisionPinchesByChance(r);
                        if (!Config.KeyboardConfig.PrecisionShiftBothHands)
                            MaybeTransferOneHandPrecisionPinch(r);
                        ConfigurePrecisionShift(r);
                    }
                    BuildCorrectAnswer();
                    return;
                }
            }

            // Otherwise: NewQuestion (plan) OR legacy mode
            GenerateNewQuestion(r);
        }

        private void ResolveSequenceMemorizeQuestion(Random random)
        {
            CurrentOperation = Operation.Copy;

            if (!_sequenceMemorizeGenerateSecond)
            {
                if (Config.KeyboardConfig.IsTwoHandCombinationMemorize)
                {
                    (_sequenceMemorizeFirstPinch, _sequenceMemorizeSecondPinch) =
                        GenerateTwoHandCombinationPair(random);
                    BitArrayQuestion = _sequenceMemorizeFirstPinch.ToArray();
                    BitArrayQuestion2 = Array.Empty<bool>();
                    BuildCorrectAnswer();
                    _sequenceMemorizeGenerateSecond = true;
                    _sequenceMemorizeCurrentIsFirst = true;
                    whichHand = null;
                    return;
                }

                if (_sequenceMemorizeFirstPinch == null)
                {
                    GenerateNewQuestion(random);
                }
                else
                {
                    BitArrayQuestion = ChooseNextSequenceFirstPinch(
                        random, _sequenceMemorizeFirstPinch);
                    BitArrayQuestion2 = Array.Empty<bool>();
                    BuildCorrectAnswer();
                }

                _sequenceMemorizeFirstPinch = BitArrayQuestion.ToArray();
                int maximumDistance = Math.Clamp(
                    Config.KeyboardConfig.PrecisionPinchSequenceSecondMaxDistance, 1, 3);
                _sequenceMemorizeSecondPinch = ChooseSequenceSecondPinch(
                    random, _sequenceMemorizeFirstPinch, maximumDistance);
                _sequenceMemorizeGenerateSecond = true;
                _sequenceMemorizeCurrentIsFirst = true;
                return;
            }

            BitArrayQuestion = _sequenceMemorizeSecondPinch?.ToArray() ??
                               _sequenceMemorizeFirstPinch!.ToArray();
            BitArrayQuestion2 = Array.Empty<bool>();
            BuildCorrectAnswer();
            _sequenceMemorizeGenerateSecond = false;
            _sequenceMemorizeCurrentIsFirst = false;
        }

        private (bool[] First, bool[] Second) GenerateTwoHandCombinationPair(Random random)
        {
            int rows = Math.Max(7, Config.KeyboardConfig.Rows);
            // Do not generate the two "gap" families (2 and 7). In those
            // combinations one hand starts above the other hand's endpoint with
            // two or more unused rows between them, which is not a valid Stage 5.1 prompt.
            int[] allowedCombinations = { 0, 1, 3, 4, 5, 6, 8 };
            int combination = allowedCombinations[random.Next(allowedCombinations.Length)];
            ((int Lower, int Upper) Left, (int Lower, int Upper) Right) first;
            ((int Lower, int Upper) Left, (int Lower, int Upper) Right) second;

            switch (combination)
            {
                case 0: // Commutativity: exchange the order of two touching intervals.
                    first = ((0, 2), (3, 4));
                    second = ((2, 4), (0, 1));
                    break;
                case 1: // Associativity: move the shared boundary.
                    first = ((0, 2), (3, 5));
                    second = random.Next(2) == 0
                        ? ((0, 3), (4, 5))
                        : ((0, 1), (2, 5));
                    break;
                case 2: // Put one separated hand directly above the other.
                    first = ((0, 1), (4, 5));
                    second = ((0, 1), (2, 3));
                    break;
                case 3: // Touching hands: move the upper endpoint of the upper hand.
                    first = ((0, 1), (2, 4));
                    second = ((0, 1), (2, Math.Min(rows - 1, 5)));
                    break;
                case 4: // Resize lower hand and shift upper hand to remain attached.
                    first = ((0, 2), (3, 4));
                    second = ((0, 3), (4, 5));
                    break;
                case 5: // Addition/subtraction: flip the upper interval across boundary.
                    first = ((0, 2), (3, 5));
                    second = ((0, 2), (0, 2));
                    if (random.Next(2) == 0)
                        (first, second) = (second, first);
                    break;
                case 6: // Subtraction/difference: move smaller from top to bottom contact.
                    first = ((0, 4), (3, 4));
                    second = ((0, 4), (0, 1));
                    if (random.Next(2) == 0)
                        (first, second) = (second, first);
                    break;
                case 7: // Move the lone combination farther by the lower hand's top.
                    first = ((0, 2), (5, 6));
                    second = ((0, 3), (5, 6));
                    break;
                default: // Split: full interval stays; other hand changes part grips.
                    first = ((0, rows - 1), (0, 2));
                    second = ((0, rows - 1), (3, rows - 1));
                    if (random.Next(2) == 0)
                        (first, second) = (second, first);
                    break;
            }

            // Keep this invariant independent of the numbered cases so newly added
            // combinations cannot reintroduce a gap of two empty rows or greater.
            if (HasDisallowedTwoHandCombinationGap(first) ||
                HasDisallowedTwoHandCombinationGap(second))
            {
                return GenerateTwoHandCombinationPair(random);
            }

            // Randomize which physical hand carries each interval, without changing
            // the mathematical relationship between the two displayed combinations.
            if (random.Next(2) == 0)
            {
                first = (first.Right, first.Left);
                second = (second.Right, second.Left);
            }

            return (BuildTwoHandCombinationBits(first.Left, first.Right, rows),
                    BuildTwoHandCombinationBits(second.Left, second.Right, rows));
        }

        private static bool HasDisallowedTwoHandCombinationGap(
            ((int Lower, int Upper) Left, (int Lower, int Upper) Right) combination)
        {
            int emptyRows = combination.Left.Upper < combination.Right.Lower
                ? combination.Right.Lower - combination.Left.Upper - 1
                : combination.Right.Upper < combination.Left.Lower
                    ? combination.Left.Lower - combination.Right.Upper - 1
                    : 0;
            return emptyRows >= 2;
        }

        private static bool[] BuildTwoHandCombinationBits(
            (int Lower, int Upper) left,
            (int Lower, int Upper) right,
            int rows)
        {
            const int columns = 2;
            bool[] bits = new bool[rows * columns];
            bits[Math.Clamp(left.Lower, 0, rows - 1) * columns] = true;
            bits[Math.Clamp(left.Upper, 0, rows - 1) * columns] = true;
            bits[(Math.Clamp(right.Lower, 0, rows - 1) * columns) + 1] = true;
            bits[(Math.Clamp(right.Upper, 0, rows - 1) * columns) + 1] = true;
            return bits;
        }

        public bool IsSequenceMemorizeFirstResponse() =>
            Config.KeyboardConfig?.IsPrecisionPinchSequenceMemorize == true &&
            _sequenceMemorizeCurrentIsFirst;

        public bool AdvanceSequenceMemorizeToLastResponse()
        {
            if (!IsSequenceMemorizeFirstResponse() || _sequenceMemorizeSecondPinch == null)
                return false;

            BitArrayQuestion = _sequenceMemorizeSecondPinch.ToArray();
            BitArrayQuestion2 = Array.Empty<bool>();
            BuildCorrectAnswer();
            _sequenceMemorizeCurrentIsFirst = false;
            _sequenceMemorizeGenerateSecond = false;
            return true;
        }

        public bool[] GetSequenceMemorizeSecondPreview() =>
            _sequenceMemorizeSecondPinch?.ToArray() ?? Array.Empty<bool>();

        public bool[] GetSequenceMemorizeFirstPreview() =>
            _sequenceMemorizeFirstPinch?.ToArray() ?? Array.Empty<bool>();

        private bool[] ChooseNextSequenceFirstPinch(Random random, bool[] previousFirst)
        {
            List<bool[]> candidates = new() { previousFirst.ToArray() }; // COPY
            candidates.Add(TransferPrecisionPinchToOtherHand(previousFirst));
            candidates.AddRange(BuildSequenceTransforms(previousFirst, maximumDistance: 1));
            bool[] selected = candidates
                .GroupBy(bits => string.Concat(bits.Select(bit => bit ? '1' : '0')))
                .Select(group => group.First())
                .OrderBy(_ => random.Next())
                .First();
            UpdateSequenceMemorizeHand(selected);
            return selected;
        }

        private bool[] ChooseSequenceSecondPinch(Random random, bool[] first, int maximumDistance)
        {
            List<bool[]> candidates = BuildSequenceTransforms(first, maximumDistance);
            if (candidates.Count == 0)
                return first.ToArray();

            bool[] selected = candidates[random.Next(candidates.Count)];
            UpdateSequenceMemorizeHand(selected);
            return selected;
        }

        private List<bool[]> BuildSequenceTransforms(bool[] source, int maximumDistance)
        {
            List<bool[]> candidates = new();
            int columns = Math.Max(1, Config.KeyboardConfig.KeysInRow);
            int[] active = source
                .Select((isActive, index) => (isActive, index))
                .Where(item => item.isActive)
                .Select(item => item.index)
                .OrderBy(index => index)
                .ToArray();
            if (active.Length != 2 || active[0] % columns != active[1] % columns)
                return candidates;

            int lower = active[0];
            int upper = active[1];
            for (int distance = 1; distance <= maximumDistance; distance++)
            {
                foreach (int delta in new[] { -distance, distance })
                {
                    int shiftedLower = GetPrecisionShiftTarget(lower, delta);
                    int shiftedUpper = GetPrecisionShiftTarget(upper, delta);
                    if (IsSequenceTargetValid(shiftedLower, lower, columns, source.Length) &&
                        IsSequenceTargetValid(shiftedUpper, upper, columns, source.Length))
                    {
                        bool[] shifted = new bool[source.Length];
                        shifted[shiftedLower] = true;
                        shifted[shiftedUpper] = true;
                        candidates.Add(shifted);
                    }

                    int movedUpper = GetPrecisionShiftTarget(upper, delta);
                    if (movedUpper > lower &&
                        IsSequenceTargetValid(movedUpper, upper, columns, source.Length))
                    {
                        bool[] resized = source.ToArray();
                        resized[upper] = false;
                        resized[movedUpper] = true;
                        candidates.Add(resized);
                    }
                }
            }

            return candidates
                .GroupBy(bits => string.Concat(bits.Select(bit => bit ? '1' : '0')))
                .Select(group => group.First())
                .ToList();
        }

        private static bool IsSequenceTargetValid(
            int target, int source, int columns, int length) =>
            target >= 0 && target < length && target % columns == source % columns;

        private void UpdateSequenceMemorizeHand(bool[] pinch)
        {
            int columns = Math.Max(1, Config.KeyboardConfig.KeysInRow);
            int first = Array.FindIndex(pinch, bit => bit);
            if (first >= 0)
                whichHand = first % columns < columns / 2 ? Direction.Left : Direction.Right;
        }

        private void TransformPrecisionCopyQuestion(Random random)
        {
            if (BitArrayQuestion == null)
                return;

            // Make hand alternation an explicit, even choice. Previously changing hands
            // was only one candidate among several same-hand movements, which allowed
            // long runs on one side (most noticeably the left hand).
            if (random.Next(2) == 0)
            {
                BitArrayQuestion = TransferPrecisionPinchToOtherHand(BitArrayQuestion);
                whichHand = GetPrecisionSideActiveIndices(leftSide: true).Length > 0
                    ? Direction.Left
                    : Direction.Right;
                return;
            }

            List<bool[]> candidates = new();

            bool leftSide = GetPrecisionSideActiveIndices(leftSide: true).Length > 0;
            int[] active = GetPrecisionSideActiveIndices(leftSide);
            if (active.Length == 2)
            {
                foreach (int delta in new[] { -1, 1 })
                {
                    int[] shifted = active.Select(index => GetPrecisionShiftTarget(index, delta)).ToArray();
                    if (shifted.All(target => IsPrecisionTargetOnSide(target, leftSide)) &&
                        shifted.Distinct().Count() == active.Length)
                    {
                        bool[] result = new bool[BitArrayQuestion.Length];
                        foreach (int target in shifted)
                            result[target] = true;
                        candidates.Add(result);
                    }

                    for (int movingPosition = 0; movingPosition < active.Length; movingPosition++)
                    {
                        int target = GetPrecisionShiftTarget(active[movingPosition], delta);
                        int fixedIndex = active[1 - movingPosition];
                        bool preservesOrder = movingPosition == 0
                            ? target < fixedIndex
                            : target > fixedIndex;
                        if (!preservesOrder || !IsPrecisionTargetOnSide(target, leftSide))
                            continue;

                        bool[] result = BitArrayQuestion.ToArray();
                        result[active[movingPosition]] = false;
                        result[target] = true;
                        candidates.Add(result);
                    }
                }
            }

            if (candidates.Count > 0)
                BitArrayQuestion = candidates[random.Next(candidates.Count)];
            whichHand = GetPrecisionSideActiveIndices(leftSide: true).Length > 0
                ? Direction.Left
                : Direction.Right;
        }

        private bool[] TransferPrecisionPinchToOtherHand(bool[] source)
        {
            bool[] transferred = new bool[source.Length];
            if (Config.KeyboardConfig.IsVerticalPrecisionPinchExercise)
            {
                int columns = Math.Max(1, Config.KeyboardConfig.KeysInRow);
                for (int index = 0; index < source.Length; index++)
                {
                    if (!source[index])
                        continue;
                    int row = index / columns;
                    int oppositeColumn = columns - 1 - (index % columns);
                    transferred[(row * columns) + oppositeColumn] = true;
                }
            }
            else
            {
                for (int index = 0; index < source.Length; index++)
                {
                    if (source[index])
                        transferred[source.Length - 1 - index] = true;
                }
            }
            return transferred;
        }

        private void MaybeTransferOneHandPrecisionPinch(Random random)
        {
            KeyboardConfig keyboard = Config.KeyboardConfig;
            if (BitArrayQuestion == null || keyboard.PrecisionShiftBothHands || random.Next(2) == 0)
                return;

            BitArrayQuestion = TransferPrecisionPinchToOtherHand(BitArrayQuestion);
            whichHand = GetPrecisionSideActiveIndices(leftSide: true).Length > 0
                ? Direction.Left
                : Direction.Right;
        }

        private void RefreshPrecisionPinchesByChance(Random random)
        {
            KeyboardConfig keyboard = Config.KeyboardConfig;
            if (BitArrayQuestion == null)
                return;

            int newPinchPercent = Math.Clamp(keyboard.PrecisionShiftNewPinchPercent, 0, 100);
            if (newPinchPercent <= 0)
                return;

            if (!keyboard.PrecisionShiftBothHands)
            {
                // The single active hand makes one independent 25/75 roll.
                if (random.Next(100) >= newPinchPercent)
                    return;

                bool[] previousQuestion = BitArrayQuestion.ToArray();
                bool[] freshPinch = GenerateTwoKeyPinch(random, 0, BitArrayQuestion.Length);
                for (int attempt = 0;
                     attempt < 12 && PrecisionPinchGeometryMatches(previousQuestion, freshPinch);
                     attempt++)
                {
                    freshPinch = GenerateTwoKeyPinch(random, 0, BitArrayQuestion.Length);
                }

                BitArrayQuestion = freshPinch;
                whichHand = GetPrecisionSideActiveIndices(leftSide: true).Length > 0
                    ? Direction.Left
                    : Direction.Right;
                return;
            }

            // With two hands, the 25/75 roll and the generated pinch are independent
            // for each hand. This naturally permits zero, one, or two fresh pinches.
            foreach (bool leftSide in new[] { true, false })
            {
                if (random.Next(100) >= newPinchPercent)
                    continue;

                bool[] previousQuestion = BitArrayQuestion.ToArray();
                bool[] freshPinch = GenerateTwoKeyPinch(random, 0, BitArrayQuestion.Length);
                for (int attempt = 0;
                     attempt < 12 && PrecisionSideMatches(previousQuestion, freshPinch, leftSide);
                     attempt++)
                {
                    freshPinch = GenerateTwoKeyPinch(random, 0, BitArrayQuestion.Length);
                }

                for (int index = 0; index < BitArrayQuestion.Length; index++)
                {
                    if (IsPrecisionIndexOnSide(index, leftSide))
                        BitArrayQuestion[index] = freshPinch[index];
                }
            }

            System.Diagnostics.Debug.Assert(GetPrecisionSideActiveIndices(leftSide: true).Length == 2);
            System.Diagnostics.Debug.Assert(GetPrecisionSideActiveIndices(leftSide: false).Length == 2);
        }

        private bool PrecisionPinchGeometryMatches(bool[] first, bool[] second)
        {
            int columns = Math.Max(1, Config.KeyboardConfig.KeysInRow);
            int half = Math.Max(1, first.Length / 2);
            int[] GetGeometry(bool[] bits) => Enumerable.Range(0, bits.Length)
                .Where(index => bits[index])
                .Select(index => Config.KeyboardConfig.PrecisionShiftAxis == PrecisionShiftAxis.Vertical
                    ? index / columns
                    : index % half)
                .OrderBy(position => position)
                .ToArray();

            return GetGeometry(first).SequenceEqual(GetGeometry(second));
        }

        private bool PrecisionSideMatches(bool[] first, bool[] second, bool leftSide)
        {
            int length = Math.Min(first.Length, second.Length);
            for (int index = 0; index < length; index++)
            {
                if (IsPrecisionIndexOnSide(index, leftSide) && first[index] != second[index])
                    return false;
            }
            return true;
        }

        private bool IsPrecisionIndexOnSide(int index, bool leftSide)
        {
            int columns = Math.Max(1, Config.KeyboardConfig.KeysInRow);
            int keyCount = BitArrayQuestion?.Length ?? 0;
            int half = keyCount / 2;
            return Config.KeyboardConfig.PrecisionShiftAxis == PrecisionShiftAxis.Vertical
                ? (index % columns == 0) == leftSide
                : (index < half) == leftSide;
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
                    (Config.KeyboardOnly &&
                     hasCanonicalCorrectAnswer &&
                     AreOverlapingSets(BitArrayQuestion, BitArrayCorrectAnswer) &&
                     CurrentOperation != Operation.Copy &&
                     Config.KeyboardConfig?.IsPrecisionShiftExercise != true) ||
                    !IsAllowedQuestion2Combination(BitArrayQuestion, BitArrayQuestion2))).ToString());

            }
            while ( quantity < Config.MinSum ||
                    quantity > Config.MaxSum ||
                    (Config.KeyboardOnly &&
                     BitArrayCorrectAnswer != null &&
                     AreOverlapingSets(BitArrayQuestion, BitArrayCorrectAnswer) &&
                     CurrentOperation != Operation.Copy &&
                     Config.KeyboardConfig?.IsPrecisionShiftExercise != true) ||
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
            if (CurrentOperation == Operation.Copy &&
                Config.KeyboardConfig?.IsVerticalPrecisionPinchExercise == true)
            {
                actionText = string.Empty;
            }
            else if (CurrentOperation == Operation.Copy &&
                Config.KeyboardConfig?.CopyPrecisionPinchToOtherHand == true)
            {
                actionText = "COPY TO OTHER HAND";
            }
            if (CurrentOperation == Operation.MoveBy)
            {
                if (Config.KeyboardConfig?.IsPrecisionShiftExercise == true)
                {
                    actionText = BuildPrecisionShiftActionText();
                    return new ExerciseGenerationResult { ActionText = actionText };
                }
                string strDir = moveBydir == Direction.Right ? "RIGHT" : "LEFT";
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

        private string BuildPrecisionShiftActionText()
        {
            KeyboardConfig keyboard = Config.KeyboardConfig;
            if (keyboard.IsPrecisionGrammarExercise && !string.IsNullOrWhiteSpace(_precisionGrammarCondition))
                return $"CONDITION: {_precisionGrammarCondition}";

            if (keyboard.PrecisionShiftAxis == PrecisionShiftAxis.Vertical)
            {
                if (keyboard.PrecisionShiftBothHands)
                {
                    string left = BuildPrecisionSideActionText(leftSide: true);
                    string right = BuildPrecisionSideActionText(leftSide: false);
                    return $"LEFT: {left}     |     RIGHT: {right}";
                }

                return BuildPrecisionSideActionText(IsLeftPrecisionColumnActive());
            }

            int distance = Math.Max(1, Math.Abs(_precisionShiftDelta));
            if (keyboard.PrecisionShiftBothHands)
            {
                string left = FormatHorizontalSideInstruction(_precisionShiftLeftDelta, isLeftSide: true);
                string right = FormatHorizontalSideInstruction(_precisionShiftRightDelta, isLeftSide: false);
                return $"{left} | {right}";
            }

            string direction = _precisionShiftDelta >= 0 ? "RIGHT" : "LEFT";
            string line = _precisionShiftDelta >= 0 ? $"|------{distance}----->" : $"|------<-----{distance}";
            return $"{line}  MOVE PINCH {direction} BY {distance}";
        }

        private string BuildPrecisionSideActionText(bool leftSide)
        {
            int delta = leftSide ? _precisionShiftLeftDelta : _precisionShiftRightDelta;
            if (delta == 0)
                return "HOLD";

            bool isShift = leftSide ? _precisionShiftLeftIsShift : _precisionShiftRightIsShift;
            bool baseAtTop = leftSide ? _precisionShiftLeftBaseAtTop : _precisionShiftRightBaseAtTop;
            string operation = isShift
                ? "SHIFT"
                : baseAtTop
                    ? "MOVE LOWER"
                    : "MOVE UPPER";
            string direction = delta > 0 ? "UP" : "DOWN";
            return $"{operation} {direction} BY {Math.Abs(delta)}";
        }

        private static string BuildVerticalSideInstructions(int leftDelta, int rightDelta)
        {
            string[] left = FormatVerticalSideInstruction(leftDelta);
            string[] right = FormatVerticalSideInstruction(rightDelta);
            return string.Join("\n", Enumerable.Range(0, left.Length)
                .Select(row => $"{left[row],-5}     {right[row],5}"));
        }

        private static string[] FormatVerticalSideInstruction(int delta)
        {
            if (delta > 0)
                return new[] { "↑", "│", Math.Abs(delta).ToString(), "│", "──" };
            if (delta < 0)
                return new[] { "──", "│", Math.Abs(delta).ToString(), "│", "↓" };
            return new[] { string.Empty, string.Empty, string.Empty, string.Empty, string.Empty };
        }

        public string GetVerticalShiftSideInstruction(bool leftSide)
        {
            if (Config?.KeyboardConfig?.IsPrecisionShiftExercise != true ||
                Config.KeyboardConfig.PrecisionShiftAxis != PrecisionShiftAxis.Vertical)
                return string.Empty;

            int delta = Config.KeyboardConfig.PrecisionShiftBothHands
                ? (leftSide ? _precisionShiftLeftDelta : _precisionShiftRightDelta)
                : IsLeftPrecisionColumnActive() == leftSide ? _precisionShiftDelta : 0;
            return string.Join("\n", FormatVerticalSideInstruction(delta));
        }

        public int GetPrecisionShiftSideDelta(bool leftSide)
        {
            if (Config?.KeyboardConfig?.IsPrecisionShiftExercise != true)
                return 0;

            if (Config.KeyboardConfig.PrecisionShiftBothHands)
                return leftSide ? _precisionShiftLeftDelta : _precisionShiftRightDelta;

            return IsLeftPrecisionColumnActive() == leftSide ? _precisionShiftDelta : 0;
        }

        public bool GetPrecisionShiftSideBaseAtTop(bool leftSide) =>
            leftSide ? _precisionShiftLeftBaseAtTop : _precisionShiftRightBaseAtTop;

        public bool IsPrecisionShiftSideGenericShift(bool leftSide)
        {
            if (Config?.KeyboardConfig?.IsPrecisionShiftExercise != true)
                return false;
            return leftSide ? _precisionShiftLeftIsShift : _precisionShiftRightIsShift;
        }

        private bool IsLeftPrecisionColumnActive()
        {
            if (BitArrayQuestion == null)
                return false;
            int columns = Math.Max(1, Config.KeyboardConfig.KeysInRow);
            int half = BitArrayQuestion.Length / 2;
            return BitArrayQuestion
                .Select((active, index) => (active, index))
                .Any(item => item.active &&
                    (Config.KeyboardConfig.PrecisionShiftAxis == PrecisionShiftAxis.Vertical
                        ? item.index % columns == 0
                        : item.index < half));
        }

        private static string FormatHorizontalSideInstruction(int delta, bool isLeftSide)
        {
            if (delta == 0)
                return string.Empty;

            int distance = Math.Abs(delta);
            if (delta > 0)
                return $"--{distance}-->";
            return $"<--{distance}--";
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

        public bool UsesPrecisionShiftTutorial() =>
            Config?.KeyboardConfig?.IsPrecisionShiftExercise == true &&
            CurrentOperation == Operation.MoveBy;

        public int[] GetPrecisionShiftTutorialTargets()
        {
            int length = BitArrayQuestion?.Length ?? 0;
            int[] targets = Enumerable.Range(0, length).ToArray();
            if (!UsesPrecisionShiftTutorial())
                return targets;

            foreach (bool leftSide in new[] { true, false })
            {
                int[] active = GetPrecisionSideActiveIndices(leftSide);
                if (active.Length == 0)
                    continue;
                int delta = leftSide ? _precisionShiftLeftDelta : _precisionShiftRightDelta;
                bool isShift = leftSide ? _precisionShiftLeftIsShift : _precisionShiftRightIsShift;
                bool baseAtTop = leftSide ? _precisionShiftLeftBaseAtTop : _precisionShiftRightBaseAtTop;
                // Vertical key indices increase visually upward (row 0 is drawn at the
                // bottom), so Max is the upper key and Min is the lower key.
                int fixedIndex = baseAtTop ? active.Max() : active.Min();
                foreach (int index in active)
                {
                    if (isShift || index != fixedIndex)
                        targets[index] = GetPrecisionShiftTarget(index, delta);
                }
            }

            return targets;
        }

        public bool UsesShiftAsMinusFlipTutorial() =>
            UsesPrecisionShiftTutorial() && _isPrecisionShiftAsMinus;

        public bool[] GetShiftAsMinusTutorialBits()
        {
            bool[] movingBits = new bool[BitArrayQuestion?.Length ?? 0];
            if (!_isPrecisionShiftAsMinus || BitArrayQuestion == null)
                return movingBits;

            bool movingLeftSide = _precisionShiftLeftDelta != 0;
            foreach (int index in GetPrecisionSideActiveIndices(movingLeftSide))
                movingBits[index] = true;
            return movingBits;
        }

        public int GetShiftAsMinusFlipAxisSourceIndex()
        {
            if (!_isPrecisionShiftAsMinus)
                return -1;

            bool movingLeftSide = _precisionShiftLeftDelta != 0;
            int[] active = GetPrecisionSideActiveIndices(movingLeftSide);
            return active.Length == 2 ? active.Min() : -1;
        }

        public int[] GetShiftAsMinusFlipTutorialTargets()
        {
            int[] targets = GetPrecisionShiftTutorialTargets();
            if (!_isPrecisionShiftAsMinus)
                return targets;

            bool movingLeftSide = _precisionShiftLeftDelta != 0;
            int[] active = GetPrecisionSideActiveIndices(movingLeftSide);
            if (active.Length != 2)
                return targets;

            int delta = movingLeftSide ? _precisionShiftLeftDelta : _precisionShiftRightDelta;
            int nearAxisTarget = GetPrecisionShiftTarget(active.Min(), delta);
            int farTarget = GetPrecisionShiftTarget(active.Max(), delta);
            targets[active.Min()] = Math.Max(nearAxisTarget, farTarget);
            targets[active.Max()] = Math.Min(nearAxisTarget, farTarget);
            return targets;
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

        public ArrowMovementMode CurrentArrowMovementMode => GetCurrentArrowMovementMode();

        public string LastArrowMovementDebugText => _lastArrowMovementDebugText;

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

        public IReadOnlyList<int> GetArrowMovementTutorialStepIndices()
        {
            IReadOnlyList<int> routeIndices = GetArrowTutorialStepIndices();
            if (GetCurrentArrowMovementMode() == ArrowMovementMode.JumpToEnd && routeIndices.Count > 0)
                return new List<int> { routeIndices[^1] };

            if (GetCurrentArrowMovementMode() != ArrowMovementMode.JumpThroughMiddle || routeIndices.Count <= 1)
                return routeIndices;

            int middleIndex = Math.Max(0, (routeIndices.Count - 1) / 2);
            int endIndex = routeIndices[^1];
            int middle = routeIndices[middleIndex];
            return middle == endIndex
                ? new List<int> { endIndex }
                : new List<int> { middle, endIndex };
        }

        public IReadOnlyList<int> GetArrowTutorialArcIndices()
        {
            if (!IsOrdinalArrowTutorial())
                return Array.Empty<int>();

            List<int> routeIndices = GetArrowTutorialStepIndices().ToList();
            if (routeIndices.Count == 0)
                return Array.Empty<int>();

            int keyCount = BitArrayQuestion?.Length ?? 0;
            if (keyCount <= 0)
                return routeIndices;

            int startIndex;
            if (UsesArrowLabelExercise())
            {
                bool leftToRight = Config?.QuestionOrder != QuestionOrder.ToLeft;
                int startValue = leftToRight ? _arrowLabelStartValue : _arrowLabelEndValue;
                startIndex = GetKeyboardIndexForValue(startValue);
            }
            else
            {
                int directionStep = dir == Direction.Right ? 1 : -1;
                startIndex = (routeIndices[0] - directionStep + keyCount) % keyCount;
            }

            if (startIndex >= 0 && startIndex < keyCount && startIndex != routeIndices[0])
                routeIndices.Insert(0, startIndex);

            if (GetCurrentArrowMovementMode() == ArrowMovementMode.JumpToEnd && routeIndices.Count > 1)
                return new List<int> { routeIndices[0], routeIndices[^1] };

            return routeIndices;
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
            if (Config.KeyboardConfig?.IsPrecisionPinchExercise == true)
            {
                BitArrayQuestion = GenerateTwoKeyPinch(r, start, end);
            }
            else
            {
                (from, length) = ChooseFromAndLength(r, 1, start, end);
                BitArrayQuestion = Config.OnlySequence ? GenerateSequenceArrayQuestion(from, length) : RandomArray(start, end);
            }

            // second pair (original code allowed length to be 0 initially; use minLength 0 to match)
            if (Config.KeyboardConfig?.IsPrecisionPinchExercise == true)
            {
                BitArrayQuestion2 = GenerateTwoKeyPinch(r, start, end);
            }
            else
            {
                (from, length) = ChooseFromAndLength(r, 0, start, end);
                BitArrayQuestion2 = Config.OnlySequence ? GenerateSequenceArrayQuestion(from, length) : RandomArray(start, end);
            }

            if (CurrentOperation == Operation.MoveBy && Config.KeyboardConfig?.IsPrecisionShiftExercise == true)
            {
                ConfigurePrecisionShift(r);
                return;
            }

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

        private void ConfigurePrecisionShift(Random random)
        {
            KeyboardConfig keyboard = Config.KeyboardConfig;
            _precisionShiftLeftDelta = 0;
            _precisionShiftRightDelta = 0;
            _precisionShiftLeftIsShift = false;
            _precisionShiftRightIsShift = false;
            _precisionGrammarCondition = string.Empty;
            _isPrecisionShiftAsMinus = false;
            int minimum = Math.Max(1, keyboard.PrecisionShiftMinDistance);
            int maximum = Math.Max(minimum, keyboard.PrecisionShiftMaxDistance);
            bool leftIsActive = keyboard.PrecisionShiftBothHands || IsLeftPrecisionColumnActive();
            bool rightIsActive = keyboard.PrecisionShiftBothHands || !leftIsActive;

            if (keyboard.IsPrecisionSignLearningExercise && _questionNumber < 3)
            {
                ConfigurePrecisionSignLearningIntro(_questionNumber);
                FinalizePrecisionShiftConfiguration(minimum);
                return;
            }

            if (keyboard.IsPrecisionGrammarExercise)
            {
                const int maximumStartAttempts = 24;
                bool configured = false;
                for (int attempt = 0; attempt < maximumStartAttempts && !configured; attempt++)
                {
                    configured = ConfigurePrecisionGrammarMission(
                        random, minimum, maximum, leftIsActive, rightIsActive);
                    if (!configured)
                        BitArrayQuestion = GenerateTwoKeyPinch(random, 0, BitArrayQuestion.Length);
                }
                if (!configured)
                    throw new InvalidOperationException("Could not create a non-HOLD grammar mission.");
                FinalizePrecisionShiftConfiguration(minimum);
                return;
            }

            if (keyboard.IsPrecisionSynchronousProcessExercise)
            {
                ConfigureSynchronousProcessMission(random, minimum, maximum);
                FinalizePrecisionShiftConfiguration(minimum);
                return;
            }

            if (leftIsActive && rightIsActive && keyboard.PrecisionShiftSynchronizeHands)
            {
                List<(int Delta, bool IsShift, bool BaseAtTop)> leftCandidates =
                    GetPrecisionSideTransformCandidates(true, minimum, maximum);
                List<(int Delta, bool IsShift, bool BaseAtTop)> rightCandidates =
                    GetPrecisionSideTransformCandidates(false, minimum, maximum);
                List<(int Delta, bool IsShift, bool BaseAtTop)> sharedCandidates = leftCandidates
                    .Where(left => rightCandidates.Contains(left))
                    .ToList();
                if (sharedCandidates.Count > 0)
                {
                    var shared = sharedCandidates[random.Next(sharedCandidates.Count)];
                    _precisionShiftLeftDelta = _precisionShiftRightDelta = shared.Delta;
                    _precisionShiftLeftIsShift = _precisionShiftRightIsShift = shared.IsShift;
                    _precisionShiftLeftBaseAtTop = _precisionShiftRightBaseAtTop = shared.BaseAtTop;
                }
            }
            else if (leftIsActive)
                ChoosePrecisionSideTransform(random, true, minimum, maximum,
                    out _precisionShiftLeftDelta, out _precisionShiftLeftIsShift, out _precisionShiftLeftBaseAtTop);
            if (rightIsActive && !keyboard.PrecisionShiftSynchronizeHands)
                ChoosePrecisionSideTransform(random, false, minimum, maximum,
                    out _precisionShiftRightDelta, out _precisionShiftRightIsShift, out _precisionShiftRightBaseAtTop);

            FinalizePrecisionShiftConfiguration(minimum);
        }

        private void ConfigureSynchronousProcessMission(Random random, int minimum, int maximum)
        {
            List<(int Delta, bool IsShift, bool BaseAtTop)> leftCandidates =
                GetPrecisionSideTransformCandidates(true, minimum, maximum);
            List<(int Delta, bool IsShift, bool BaseAtTop)> rightCandidates =
                GetPrecisionSideTransformCandidates(false, minimum, maximum);
            List<((int Delta, bool IsShift, bool BaseAtTop) Left,
                  (int Delta, bool IsShift, bool BaseAtTop) Right)> pairs =
                (from left in leftCandidates
                 from right in rightCandidates
                 where left != right
                 select (left, right)).ToList();

            if (pairs.Count == 0)
                throw new InvalidOperationException(
                    "Could not create two different legal one-step hand commands.");

            var mission = pairs[random.Next(pairs.Count)];
            (_precisionShiftLeftDelta, _precisionShiftLeftIsShift, _precisionShiftLeftBaseAtTop) =
                mission.Left;
            (_precisionShiftRightDelta, _precisionShiftRightIsShift, _precisionShiftRightBaseAtTop) =
                mission.Right;
        }

        private void ConfigurePrecisionSignLearningIntro(int exerciseIndex)
        {
            int columns = Math.Max(1, Config.KeyboardConfig.KeysInRow);
            int rows = Math.Max(5, Config.KeyboardConfig.Rows);
            int column = Math.Min(exerciseIndex % Math.Min(2, columns), columns - 1);
            bool[] pinch = new bool[BitArrayQuestion.Length];

            // 1: enlarge upward by two rows.
            // 2: squeeze downward by two rows.
            // 3: shift the complete grip downward by two rows.
            int lowerRow;
            int upperRow;
            int delta;
            bool isShift;
            switch (exerciseIndex)
            {
                case 0:
                    lowerRow = 0;
                    upperRow = Math.Min(2, rows - 3);
                    delta = 2;
                    isShift = false;
                    break;
                case 1:
                    lowerRow = 0;
                    upperRow = Math.Min(4, rows - 1);
                    delta = -2;
                    isShift = false;
                    break;
                default:
                    lowerRow = 2;
                    upperRow = Math.Min(4, rows - 1);
                    delta = -2;
                    isShift = true;
                    break;
            }

            pinch[(lowerRow * columns) + column] = true;
            pinch[(upperRow * columns) + column] = true;
            BitArrayQuestion = pinch;

            if (column == 0)
            {
                _precisionShiftLeftDelta = delta;
                _precisionShiftLeftIsShift = isShift;
                _precisionShiftLeftBaseAtTop = false;
            }
            else
            {
                _precisionShiftRightDelta = delta;
                _precisionShiftRightIsShift = isShift;
                _precisionShiftRightBaseAtTop = false;
            }
        }

        private bool ConfigurePrecisionGrammarMission(
            Random random,
            int minimum,
            int maximum,
            bool leftIsActive,
            bool rightIsActive)
        {
            PrecisionPinchMoveOptions configured = Config.KeyboardConfig.PrecisionPinchMoveOptions;
            bool handsStartTogether = PrecisionGrammarHandsStartTogether();
            List<(string Label, (int Delta, bool IsShift, bool BaseAtTop)? Left,
                (int Delta, bool IsShift, bool BaseAtTop)? Right)> missions = new();

            if (handsStartTogether && configured.HasFlag(PrecisionPinchMoveOptions.ShiftWhole))
            {
                List<(int Delta, bool IsShift, bool BaseAtTop)> leftShiftCandidates = leftIsActive
                    ? GetPrecisionSideTransformCandidates(true, minimum, maximum)
                        .Where(candidate => candidate.IsShift && Math.Abs(candidate.Delta) == 1).ToList()
                    : new();
                List<(int Delta, bool IsShift, bool BaseAtTop)> rightShiftCandidates = rightIsActive
                    ? GetPrecisionSideTransformCandidates(false, minimum, maximum)
                        .Where(candidate => candidate.IsShift && Math.Abs(candidate.Delta) == 1).ToList()
                    : new();
                AddRandomEqualHandMission(
                    missions, random, "START EQUAL — SHIFT BY ONE",
                    leftShiftCandidates, rightShiftCandidates);

                List<(int Delta, bool IsShift, bool BaseAtTop)> leftHigherCandidates = leftIsActive
                    ? GetPrecisionSideTransformCandidates(true, minimum, maximum)
                        .Where(candidate => !candidate.IsShift && !candidate.BaseAtTop).ToList()
                    : new();
                List<(int Delta, bool IsShift, bool BaseAtTop)> rightHigherCandidates = rightIsActive
                    ? GetPrecisionSideTransformCandidates(false, minimum, maximum)
                        .Where(candidate => !candidate.IsShift && !candidate.BaseAtTop).ToList()
                    : new();

                // Moving the higher endpoint is the main equal-start vocabulary.
                // Four entries versus one shift entry produce an 80/20 family weight.
                for (int weight = 0; weight < 4; weight++)
                {
                    AddRandomEqualHandMission(
                        missions, random, "START EQUAL — MOVE HIGHER",
                        leftHigherCandidates, rightHigherCandidates);
                }
            }
            else if (!handsStartTogether)
            {
                int columns = Math.Max(1, Config.KeyboardConfig.KeysInRow);
                int[] leftRows = GetPrecisionSideActiveIndices(leftSide: true)
                    .Select(index => index / columns).OrderBy(row => row).ToArray();
                int[] rightRows = GetPrecisionSideActiveIndices(leftSide: false)
                    .Select(index => index / columns).OrderBy(row => row).ToArray();
                bool lowerIsLeft = leftRows.Length == 2 && rightRows.Length == 2 &&
                                   leftRows.Max() < rightRows.Min();
                bool lowerSide = lowerIsLeft;
                bool upperSide = !lowerIsLeft;

                List<(int Delta, bool IsShift, bool BaseAtTop)> upperResizeCandidates =
                    GetPrecisionSideTransformCandidates(upperSide, minimum, maximum)
                        .Where(candidate => !candidate.IsShift && !candidate.BaseAtTop)
                        .ToList();
                if (upperResizeCandidates.Count > 0)
                {
                    var resize = upperResizeCandidates[random.Next(upperResizeCandidates.Count)];
                    missions.Add(("ENLARGE / REDUCE UPPER HAND",
                        upperSide ? resize : null,
                        upperSide ? null : resize));
                }

                // The lower hand changes at its upper endpoint. Shift the complete
                // upper hand by the same delta so b and b+1 remain adjacent.
                List<(int Delta, bool IsShift, bool BaseAtTop)> lowerResizeCandidates =
                    GetPrecisionSideTransformCandidates(lowerSide, minimum, maximum)
                        .Where(candidate => !candidate.IsShift && !candidate.BaseAtTop)
                        .ToList();
                List<(int Delta, bool IsShift, bool BaseAtTop)> upperShiftCandidates =
                    GetPrecisionSideTransformCandidates(upperSide, minimum, maximum)
                        .Where(candidate => candidate.IsShift)
                        .ToList();
                List<((int Delta, bool IsShift, bool BaseAtTop) Resize,
                    (int Delta, bool IsShift, bool BaseAtTop) Shift)> attachedCandidates = new();
                foreach (var resize in lowerResizeCandidates)
                {
                    var matchingShift = upperShiftCandidates
                        .FirstOrDefault(shift => shift.Delta == resize.Delta);
                    if (matchingShift == default)
                        continue;
                    attachedCandidates.Add((resize, matchingShift));
                }
                if (attachedCandidates.Count > 0)
                {
                    var attached = attachedCandidates[random.Next(attachedCandidates.Count)];
                    missions.Add(("ENLARGE / REDUCE LOWER HAND — KEEP UPPER ATTACHED",
                        lowerIsLeft ? attached.Resize : attached.Shift,
                        lowerIsLeft ? attached.Shift : attached.Resize));
                }

                List<(int Delta, bool IsShift, bool BaseAtTop)> upperLowerEdgeCandidates =
                    GetPrecisionSideTransformCandidates(upperSide, minimum, maximum)
                        .Where(candidate => !candidate.IsShift && candidate.BaseAtTop)
                        .ToList();
                List<((int Delta, bool IsShift, bool BaseAtTop) LowerBoundary,
                    (int Delta, bool IsShift, bool BaseAtTop) UpperBoundary)> associativityCandidates = new();
                foreach (var lowerBoundary in lowerResizeCandidates)
                {
                    var upperBoundary = upperLowerEdgeCandidates
                        .FirstOrDefault(candidate => candidate.Delta == lowerBoundary.Delta);
                    if (upperBoundary != default)
                        associativityCandidates.Add((lowerBoundary, upperBoundary));
                }
                if (associativityCandidates.Count > 0)
                {
                    var associativity = associativityCandidates[random.Next(associativityCandidates.Count)];
                    missions.Add(("ASSOCIATIVITY — MOVE THE SHARED BOUNDARY",
                        lowerIsLeft ? associativity.LowerBoundary : associativity.UpperBoundary,
                        lowerIsLeft ? associativity.UpperBoundary : associativity.LowerBoundary));
                }

                int lowerLength = (lowerIsLeft ? leftRows : rightRows)[1] -
                                  (lowerIsLeft ? leftRows : rightRows)[0] + 1;
                int upperLength = (lowerIsLeft ? rightRows : leftRows)[1] -
                                  (lowerIsLeft ? rightRows : leftRows)[0] + 1;

                // a..b | b+1..b+c  ->  a..b | b-c+1..b
                // The complete upper interval moves down by its own length. As an
                // interval transformation this is a mirror across the b | b+1 line.
                int shiftAsMinusDelta = -upperLength;
                if (CanApplyPrecisionSideTransform(upperSide, shiftAsMinusDelta,
                        isShift: true, baseAtTop: false))
                {
                    var shiftAsMinus =
                        (Delta: shiftAsMinusDelta, IsShift: true, BaseAtTop: false);
                    (string Label,
                        (int Delta, bool IsShift, bool BaseAtTop)? Left,
                        (int Delta, bool IsShift, bool BaseAtTop)? Right) shiftAsMinusMission =
                        ("SHIFT AS MINUS — MIRROR UPPER ACROSS b | b+1",
                            upperSide ? shiftAsMinus : null,
                            upperSide ? null : shiftAsMinus);
                    missions.Add(shiftAsMinusMission);
                    missions.Add(shiftAsMinusMission);
                }

            }

            if (missions.Count == 0)
                return false;

            var mission = missions[random.Next(missions.Count)];
            _precisionGrammarCondition = mission.Label;
            _isPrecisionShiftAsMinus = mission.Label.StartsWith("SHIFT AS MINUS", StringComparison.Ordinal);
            if (mission.Left is { } left)
                (_precisionShiftLeftDelta, _precisionShiftLeftIsShift, _precisionShiftLeftBaseAtTop) = left;
            if (mission.Right is { } right)
                (_precisionShiftRightDelta, _precisionShiftRightIsShift, _precisionShiftRightBaseAtTop) = right;
            return _precisionShiftLeftDelta != 0 || _precisionShiftRightDelta != 0;
        }

        private static void AddRandomEqualHandMission(
            List<(string Label, (int Delta, bool IsShift, bool BaseAtTop)? Left,
                (int Delta, bool IsShift, bool BaseAtTop)? Right)> missions,
            Random random,
            string label,
            List<(int Delta, bool IsShift, bool BaseAtTop)> leftCandidates,
            List<(int Delta, bool IsShift, bool BaseAtTop)> rightCandidates)
        {
            List<int> scopes = new();
            if (leftCandidates.Count > 0)
                scopes.Add(0);
            if (rightCandidates.Count > 0)
                scopes.Add(1);
            if (leftCandidates.Count > 0 && rightCandidates.Count > 0)
                scopes.Add(2);
            if (scopes.Count == 0)
                return;

            int scope = scopes[random.Next(scopes.Count)];
            (int Delta, bool IsShift, bool BaseAtTop)? left = scope is 0 or 2
                ? leftCandidates[random.Next(leftCandidates.Count)]
                : null;
            (int Delta, bool IsShift, bool BaseAtTop)? right = scope is 1 or 2
                ? rightCandidates[random.Next(rightCandidates.Count)]
                : null;
            string scopeLabel = scope == 0 ? "LEFT HAND" : scope == 1 ? "RIGHT HAND" : "BOTH HANDS";
            missions.Add(($"{label} — {scopeLabel}", left, right));
        }

        private bool PrecisionGrammarHandsStartTogether()
        {
            int columns = Math.Max(1, Config.KeyboardConfig.KeysInRow);
            int[] leftRows = GetPrecisionSideActiveIndices(leftSide: true)
                .Select(index => index / columns)
                .OrderBy(row => row)
                .ToArray();
            int[] rightRows = GetPrecisionSideActiveIndices(leftSide: false)
                .Select(index => index / columns)
                .OrderBy(row => row)
                .ToArray();
            return leftRows.Length == 2 && leftRows.SequenceEqual(rightRows);
        }

        private void FinalizePrecisionShiftConfiguration(int minimum)
        {
            bool leftIsActive = _precisionShiftLeftDelta != 0;
            _precisionShiftDelta = leftIsActive ? _precisionShiftLeftDelta : _precisionShiftRightDelta;
            moveByLength = Math.Max(Math.Abs(_precisionShiftLeftDelta), Math.Abs(_precisionShiftRightDelta));
            if (moveByLength == 0)
                moveByLength = minimum;
            moveBydir = _precisionShiftDelta >= 0 ? Direction.Right : Direction.Left;

            // Finalize legality here, before action text, arrows, tutorial targets and
            // the correct answer are exposed. Every consumer now sees the same shift.
            // Grammar missions are assembled from already-legal candidates and may
            // intentionally leave one hand at HOLD, so they must not be expanded into
            // an unintended two-hand mission by the general legality fallback.
            if (!Config.KeyboardConfig.IsPrecisionGrammarExercise)
                EnsurePrecisionShiftTransformsAreLegal();
        }

        private void ChoosePrecisionSideTransform(
            Random random,
            bool leftSide,
            int minimum,
            int maximum,
            out int delta,
            out bool isShift,
            out bool baseAtTop)
        {
            List<(int Delta, bool IsShift, bool BaseAtTop)> candidates =
                GetPrecisionSideTransformCandidates(leftSide, minimum, maximum);

            if (candidates.Count == 0)
            {
                delta = 0;
                isShift = false;
                baseAtTop = false;
                return;
            }

            int moveLowerPercent = Config.KeyboardConfig.PrecisionMoveLowerPercent;
            if (moveLowerPercent >= 0)
            {
                List<(int Delta, bool IsShift, bool BaseAtTop)> moveLowerCandidates =
                    candidates.Where(candidate => !candidate.IsShift && candidate.BaseAtTop).ToList();
                List<(int Delta, bool IsShift, bool BaseAtTop)> otherCandidates =
                    candidates.Where(candidate => candidate.IsShift || !candidate.BaseAtTop).ToList();
                bool chooseMoveLower = moveLowerCandidates.Count > 0 &&
                                       (otherCandidates.Count == 0 ||
                                        random.Next(100) < Math.Clamp(moveLowerPercent, 0, 100));
                if (chooseMoveLower)
                {
                    (delta, isShift, baseAtTop) =
                        moveLowerCandidates[random.Next(moveLowerCandidates.Count)];
                    return;
                }

                if (otherCandidates.Count > 0)
                    candidates = otherCandidates;
            }

            bool preferSingleKeyMovement =
                !Config.KeyboardConfig.PrecisionShiftBothHands &&
                Config.KeyboardConfig.PrecisionPinchMoveOptions == PrecisionPinchMoveOptions.All;
            if (preferSingleKeyMovement)
            {
                List<(int Delta, bool IsShift, bool BaseAtTop)> singleKeyCandidates =
                    candidates.Where(candidate => !candidate.IsShift).ToList();
                List<(int Delta, bool IsShift, bool BaseAtTop)> wholeShiftCandidates =
                    candidates.Where(candidate => candidate.IsShift).ToList();

                // Stage 4 is primarily about transforming one finger around its fixed
                // base. Keep occasional whole-grip shifts so the full vocabulary remains.
                bool chooseSingleKey = singleKeyCandidates.Count > 0 &&
                                       (wholeShiftCandidates.Count == 0 || random.Next(5) != 0);
                List<(int Delta, bool IsShift, bool BaseAtTop)> weightedPool =
                    chooseSingleKey ? singleKeyCandidates : wholeShiftCandidates;
                (delta, isShift, baseAtTop) = weightedPool[random.Next(weightedPool.Count)];
                return;
            }

            (delta, isShift, baseAtTop) = candidates[random.Next(candidates.Count)];
        }

        private List<(int Delta, bool IsShift, bool BaseAtTop)> GetPrecisionSideTransformCandidates(
            bool leftSide,
            int minimum,
            int maximum)
        {
            PrecisionPinchMoveOptions options = Config.KeyboardConfig.PrecisionPinchMoveOptions;
            List<(int Delta, bool IsShift, bool BaseAtTop)> candidates = new();
            for (int distance = minimum; distance <= maximum; distance++)
            {
                foreach (int signedDistance in new[] { -distance, distance })
                {
                    if (options.HasFlag(PrecisionPinchMoveOptions.ShiftWhole) &&
                        CanApplyPrecisionSideTransform(leftSide, signedDistance, isShift: true, baseAtTop: false))
                        candidates.Add((signedDistance, true, false));

                    if (options.HasFlag(PrecisionPinchMoveOptions.MoveUpper) &&
                        CanApplyPrecisionSideTransform(leftSide, signedDistance, isShift: false, baseAtTop: false))
                        candidates.Add((signedDistance, false, false));
                    if (options.HasFlag(PrecisionPinchMoveOptions.MoveLower) &&
                        CanApplyPrecisionSideTransform(leftSide, signedDistance, isShift: false, baseAtTop: true))
                        candidates.Add((signedDistance, false, true));
                }
            }
            return candidates;
        }

        private bool CanApplyPrecisionSideTransform(bool leftSide, int delta, bool isShift, bool baseAtTop)
        {
            int[] active = GetPrecisionSideActiveIndices(leftSide);
            if (active.Length != 2)
                return false;

            int fixedIndex = Config.KeyboardConfig.PrecisionShiftAxis == PrecisionShiftAxis.Vertical
                ? (baseAtTop ? active.Max() : active.Min())
                : (baseAtTop ? active.Min() : active.Max());
            List<int> transformed = new(active.Length);
            foreach (int index in active)
            {
                if (!isShift && index == fixedIndex)
                {
                    transformed.Add(index);
                    continue;
                }

                int target = GetPrecisionShiftTarget(index, delta);
                if (!IsPrecisionTargetOnSide(target, leftSide))
                    return false;
                transformed.Add(target);

                if (!isShift)
                {
                    // The moving finger must remain on its original side of the fixed
                    // finger: equality would merge them and passing it would cross them.
                    bool preservesOrder = Config.KeyboardConfig.PrecisionShiftAxis == PrecisionShiftAxis.Vertical
                        // Vertical indices increase visually upward.
                        ? (baseAtTop ? target < fixedIndex : target > fixedIndex)
                        : (baseAtTop ? target > fixedIndex : target < fixedIndex);
                    if (!preservesOrder)
                        return false;
                }
            }

            if (Config.KeyboardConfig.PrecisionShiftAxis == PrecisionShiftAxis.Vertical)
            {
                int columns = Math.Max(1, Config.KeyboardConfig.KeysInRow);
                int maximumInterval = Math.Clamp(
                    Config.KeyboardConfig.PrecisionPinchMaxInterval,
                    1,
                    Math.Max(1, Config.KeyboardConfig.Rows - 1));
                int[] rows = transformed.Select(index => index / columns).ToArray();
                if (rows.Max() - rows.Min() > maximumInterval)
                    return false;
            }
            return true;
        }

        private int[] GetPrecisionSideActiveIndices(bool leftSide)
        {
            if (BitArrayQuestion == null)
                return Array.Empty<int>();

            int columns = Math.Max(1, Config.KeyboardConfig.KeysInRow);
            int half = BitArrayQuestion.Length / 2;
            return Enumerable.Range(0, BitArrayQuestion.Length)
                .Where(index => BitArrayQuestion[index] &&
                    (Config.KeyboardConfig.PrecisionShiftAxis == PrecisionShiftAxis.Vertical
                        ? (index % columns == 0) == leftSide
                        : (index < half) == leftSide))
                .OrderBy(index => index)
                .ToArray();
        }

        private int GetPrecisionShiftTarget(int index, int delta)
        {
            if (Config.KeyboardConfig.PrecisionShiftAxis == PrecisionShiftAxis.Vertical)
            {
                int columns = Math.Max(1, Config.KeyboardConfig.KeysInRow);
                // The keyboard grid draws logical row 0 at the bottom. Increasing the
                // logical row therefore moves visually upward.
                return index + (delta * columns);
            }
            return index + delta;
        }

        private bool IsPrecisionTargetOnSide(int target, bool leftSide)
        {
            if (target < 0 || BitArrayQuestion == null || target >= BitArrayQuestion.Length)
                return false;

            int columns = Math.Max(1, Config.KeyboardConfig.KeysInRow);
            int half = BitArrayQuestion.Length / 2;
            return Config.KeyboardConfig.PrecisionShiftAxis == PrecisionShiftAxis.Vertical
                ? (target % columns == 0) == leftSide
                : (target < half) == leftSide;
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
                    BitArrayCorrectAnswer = Config.KeyboardConfig?.CopyPrecisionPinchToOtherHand == true
                        ? TransferPrecisionPinchToOtherHand(BitArrayQuestion)
                        : BitArrayQuestion.ToArray();
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
                        if (Config.KeyboardConfig?.IsPrecisionShiftExercise == true)
                        {
                            ApplyPrecisionShiftAnswer();
                            break;
                        }
                        ApplyLegacyMoveAnswer();
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

        private void ApplyPrecisionShiftAnswer()
        {
            foreach (bool leftSide in new[] { true, false })
            {
                int[] active = GetPrecisionSideActiveIndices(leftSide);
                if (active.Length == 0)
                    continue;
                int delta = leftSide ? _precisionShiftLeftDelta : _precisionShiftRightDelta;
                bool isShift = leftSide ? _precisionShiftLeftIsShift : _precisionShiftRightIsShift;
                bool baseAtTop = leftSide ? _precisionShiftLeftBaseAtTop : _precisionShiftRightBaseAtTop;
                int fixedIndex = Config.KeyboardConfig.PrecisionShiftAxis == PrecisionShiftAxis.Vertical
                    ? (baseAtTop ? active.Max() : active.Min())
                    : (baseAtTop ? active.Min() : active.Max());
                foreach (int index in active)
                {
                    int target = isShift || index != fixedIndex
                        ? GetPrecisionShiftTarget(index, delta)
                        : index;
                    if (target >= 0 && target < BitArrayCorrectAnswer.Length)
                        BitArrayCorrectAnswer[target] = true;
                }
            }
        }

        private void EnsurePrecisionShiftTransformsAreLegal()
        {
            KeyboardConfig keyboard = Config.KeyboardConfig;
            int minimum = Math.Max(1, keyboard.PrecisionShiftMinDistance);
            int maximum = Math.Max(minimum, keyboard.PrecisionShiftMaxDistance);
            bool leftIsActive = keyboard.PrecisionShiftBothHands || IsLeftPrecisionColumnActive();
            bool rightIsActive = keyboard.PrecisionShiftBothHands || !leftIsActive;

            if (leftIsActive && rightIsActive && keyboard.PrecisionShiftSynchronizeHands)
            {
                bool currentIsLegal =
                    CanApplyPrecisionSideTransform(true, _precisionShiftLeftDelta,
                        _precisionShiftLeftIsShift, _precisionShiftLeftBaseAtTop) &&
                    CanApplyPrecisionSideTransform(false, _precisionShiftRightDelta,
                        _precisionShiftRightIsShift, _precisionShiftRightBaseAtTop);
                if (!currentIsLegal)
                {
                    List<(int Delta, bool IsShift, bool BaseAtTop)> sharedCandidates =
                        GetPrecisionSideTransformCandidates(true, minimum, maximum)
                            .Where(candidate => GetPrecisionSideTransformCandidates(false, minimum, maximum)
                                .Contains(candidate))
                            .ToList();
                    if (sharedCandidates.Count > 0)
                    {
                        var replacement = ChooseClosestLegalPrecisionTransform(
                            sharedCandidates,
                            _precisionShiftLeftDelta,
                            _precisionShiftLeftIsShift,
                            _precisionShiftLeftBaseAtTop);
                        _precisionShiftLeftDelta = _precisionShiftRightDelta = replacement.Delta;
                        _precisionShiftLeftIsShift = _precisionShiftRightIsShift = replacement.IsShift;
                        _precisionShiftLeftBaseAtTop = _precisionShiftRightBaseAtTop = replacement.BaseAtTop;
                    }
                }
            }
            else
            {
                if (leftIsActive)
                    EnsurePrecisionSideTransformIsLegal(true, minimum, maximum);
                if (rightIsActive)
                    EnsurePrecisionSideTransformIsLegal(false, minimum, maximum);
            }

            _precisionShiftDelta = leftIsActive ? _precisionShiftLeftDelta : _precisionShiftRightDelta;
            moveByLength = Math.Max(Math.Abs(_precisionShiftLeftDelta), Math.Abs(_precisionShiftRightDelta));
            moveBydir = _precisionShiftDelta >= 0 ? Direction.Right : Direction.Left;
        }

        private void EnsurePrecisionSideTransformIsLegal(bool leftSide, int minimum, int maximum)
        {
            int delta = leftSide ? _precisionShiftLeftDelta : _precisionShiftRightDelta;
            bool isShift = leftSide ? _precisionShiftLeftIsShift : _precisionShiftRightIsShift;
            bool baseAtTop = leftSide ? _precisionShiftLeftBaseAtTop : _precisionShiftRightBaseAtTop;
            if (CanApplyPrecisionSideTransform(leftSide, delta, isShift, baseAtTop))
                return;

            List<(int Delta, bool IsShift, bool BaseAtTop)> candidates =
                GetPrecisionSideTransformCandidates(leftSide, minimum, maximum);
            if (candidates.Count == 0)
                return;

            var replacement = ChooseClosestLegalPrecisionTransform(candidates, delta, isShift, baseAtTop);
            if (leftSide)
            {
                _precisionShiftLeftDelta = replacement.Delta;
                _precisionShiftLeftIsShift = replacement.IsShift;
                _precisionShiftLeftBaseAtTop = replacement.BaseAtTop;
            }
            else
            {
                _precisionShiftRightDelta = replacement.Delta;
                _precisionShiftRightIsShift = replacement.IsShift;
                _precisionShiftRightBaseAtTop = replacement.BaseAtTop;
            }
        }

        private static (int Delta, bool IsShift, bool BaseAtTop) ChooseClosestLegalPrecisionTransform(
            IEnumerable<(int Delta, bool IsShift, bool BaseAtTop)> candidates,
            int requestedDelta,
            bool requestedIsShift,
            bool requestedBaseAtTop) =>
            candidates
                .OrderBy(candidate => candidate.IsShift == requestedIsShift ? 0 : 1)
                .ThenBy(candidate => candidate.BaseAtTop == requestedBaseAtTop ? 0 : 1)
                .ThenBy(candidate => Math.Abs(Math.Abs(candidate.Delta) - Math.Abs(requestedDelta)))
                .First();

        private void ApplyLegacyMoveAnswer()
        {
            int length = BitArrayQuestion.Length;
            int signedDistance = moveBydir == Direction.Right ? moveByLength : -moveByLength;
            bool wraps = !Config.OnlyToTen;
            for (int index = 0; index < length; index++)
            {
                if (!BitArrayQuestion[index])
                    continue;

                int target = index + signedDistance;
                if (wraps)
                    target = ((target % length) + length) % length;
                if (target >= 0 && target < length)
                    BitArrayCorrectAnswer[target] = true;
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
